using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hamster {
    public class AssetBundleInfo {
        public string Path;
        public string Name;
        public string DataBase;

        public override string ToString() {
            return string.Format("Path: " + Path + " Name " + Name + " DataBase " + DataBase);
        }
    }

    public class AssetBundleExtend : IPool {
        public int Ref = 0;
        public AssetBundle AssetBundle = null;

        public void Reset() {
            Ref = 0;
            AssetBundle = null;
        }
    }

    public class Asset {
        public static bool UseAssetBundle {
            get;
            set;
        }

        public static string AssetBundleBasePath {
            get;
            set;
        }

        private static Dictionary<string, AssetBundleInfo> _infos = null;
        private static Dictionary<string, UnityEngine.Object> _assets = new Dictionary<string, UnityEngine.Object>();  // 以资源json的key做为key
        private static Dictionary<string, AssetBundleExtend> _bundles = new Dictionary<string, AssetBundleExtend>();               // 以assetBundle的路径为key
        private static Dictionary<string, List<string>> _dependencies = new Dictionary<string, List<string>>();
        private static Dictionary<string, UnityObjectPool> _pools = new Dictionary<string, UnityObjectPool>();
        private static Dictionary<string, SyncLoadOperation> _syncOperation = new Dictionary<string, SyncLoadOperation>();

        public static void Initialize(string path, string[] manifast) {
            // 加载名称: 资源配置表
            TextAsset textAsset = null;
            if (UseAssetBundle) {
                AssetBundle configBundle = AssetBundle.LoadFromFile(string.Format("{0}/res/{1}", AssetBundleBasePath, path));
                textAsset = configBundle.LoadAsset<TextAsset>(path);
            }
            else {
#if UNITY_EDITOR
                textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("Assets/Res/{0}.json", path));
#endif
            }
            _infos = JsonConvert.DeserializeObject<Dictionary<string, AssetBundleInfo>>(textAsset.text);

            // 加载manifest以获取依赖
            for (int i = 0; i < manifast.Length; i++) {
                if (!LoadAssetBundleManifest(manifast[i])) {
                    UnityEngine.Debug.LogError("Can't Load Manifest By " + manifast[i]);
                    return;
                }
            }

#if !UNITY_EDITOR
        UseAssetBundle = true;
#endif
        }

        public static T Load<T>(string path) where T : UnityEngine.Object {
            if (_assets.TryGetValue(path, out Object prefab)) {
                return prefab as T;
            }
            else {
                prefab = GetPrefabFromAssetBundle<UnityEngine.Object>(path);
                return prefab as T;
            }
        }

        public static AsyncOperation LoadScene(string path, string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode) {
            if (UseAssetBundle) {
                if (!TryGetAssetAndName(path, out string assetPath, out string assetName)) {
                    UnityEngine.Debug.LogError("Can't Get Asset Path And Asset Name By " + path);
                    return null;
                }

                if (null != LoadAssetBundle(assetPath)) {
                    return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
                }
                return null;
            }
            else {
                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            }
        }

        public static AsyncOperation UnLoadScene(string path, string sceneName, UnityEngine.SceneManagement.UnloadSceneOptions mode) {
            AsyncOperation asyncOperator;
            if (UseAssetBundle) {
                asyncOperator = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName, mode);
                Unload(path);
            }
            else {
                asyncOperator = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName, mode);
            }
            return asyncOperator;
        }

        public static SyncLoadOperation LoadSync(string path, System.Action<UnityEngine.Object> callBack, uint cache = 0) {
            if (!TryGetAssetAndName(path, out string assetPath, out string assetName)) {
                UnityEngine.Debug.LogError("Can't Get Asset Path And Asset Name By " + path);
                return null;
            }

            // 先判断是否已经在加载过了
            if (_assets.TryGetValue(path, out Object value)) {
                if (_pools.TryGetValue(path, out UnityObjectPool pool)) {
                    callBack?.Invoke(pool.Malloc());
                }
                return null;
            }

            // 先看看是否已经在加载中了
            if (_syncOperation.TryGetValue(path, out SyncLoadOperation operation)) {
                operation.BindCustomCallback(callBack); 
                return operation;
            }

            // 尝试去加载
            if (UseAssetBundle) {
                AssetBundleExtend assetBundle = LoadAssetBundle(assetPath, false);
                if (null == assetBundle) {
                    // 加载依赖
                    var dependencies = GetDependencies(assetPath.ToLower());
                    for (int i = 0; i < dependencies.Count; i++) {
                        LoadAssetBundleSync(dependencies[i]);
                    }

                    // 加载正主
                    SyncLoadAssetAndBundleOperation op = ObjectPool<SyncLoadAssetAndBundleOperation>.Malloc();
                    op.Cache = cache;
                    string loadPath = System.IO.Path.Combine(AssetBundleBasePath, assetPath);
                    AssetBundleCreateRequest bundleCreateRequest = AssetBundle.LoadFromFileAsync(loadPath);
                    op.Initialize(path, assetName, assetPath, bundleCreateRequest, OnLoadBundleAndAssetComplete, callBack);
                    operation = op;
                }
                else {
                    SyncLoadAssetOperation op = ObjectPool<SyncLoadAssetOperation>.Malloc();
                    op.Cache = cache;
                    op.Initialize(path, assetName, assetBundle, OnSyncLoadAssetsComplete, callBack);
                    operation = op;
                }
            }
            else {
#if UNITY_EDITOR
                AssetDataBaseLoadOperation op = ObjectPool<AssetDataBaseLoadOperation>.Malloc();
                op.Cache = cache;
                op.Initialize(path, assetPath, OnAssetDataBaseLoadComplete, callBack);
                operation = op;
#endif
            }
            _syncOperation.Add(path, operation);
            return operation;
        }

        public static GameObject Load(string path, uint cache = 1) {
            if (_pools.TryGetValue(path, out UnityObjectPool pool)) {
                return pool.Malloc();
            }
            else if (_assets.TryGetValue(path, out Object prefab)) {
                pool = InitObjectPool(path, prefab, cache);
                return pool.Malloc();
            }
            else {
                prefab = GetPrefabFromAssetBundle<UnityEngine.Object>(path);
                pool = InitObjectPool(path, prefab, cache);
                return pool.Malloc();
            }
        }

        public static UnityObjectPool Cache(string path, uint cache) {
            if (!_pools.TryGetValue(path, out UnityObjectPool pool)) {
                if (!_assets.TryGetValue(path, out Object prefab))
                    prefab = GetPrefabFromAssetBundle<UnityEngine.Object>(path);
                return InitObjectPool(path, prefab, cache);
            }
            return null;
        }

        public static void Unload(string path) {
            if (_assets.ContainsKey(path)) {
                if (!TryGetAssetAndName(path, out string assetPath, out string assetName)) {
                    UnityEngine.Debug.LogError("Can't Get Asset Path And Asset Name By " + path);
                    return;
                }

                // 清理池
                if (_pools.TryGetValue(path, out UnityObjectPool pool)) {
                    pool.Clean();
                    _pools.Remove(path);
                }

                // 卸载掉AB
                if (UseAssetBundle) {
                    AssetBundleExtend extend = _bundles[assetPath];
                    if (--extend.Ref <= 0) {
                        UnloadAssetBundle(assetPath);
                    }
                }

                _assets.Remove(path);
            }
        }

        private static void UnloadAssetBundle(string assetPath) {
            var dependencies = GetDependencies(assetPath.ToLower());
            for (int i = 0; i < dependencies.Count; i++) {
                if (_bundles.TryGetValue(dependencies[i], out AssetBundleExtend extend)) {
                    if (--extend.Ref <= 0) {
                        UnloadAssetBundle(dependencies[i]);
                    }
                }
            }

            _bundles[assetPath].AssetBundle.Unload(true);
            ObjectPool<AssetBundleExtend>.Free(_bundles[assetPath]);
            _bundles.Remove(assetPath);
        }

        public static void UnloadAll() {
            // 清掉所有的池
            var poolIt = _pools.GetEnumerator();
            while (poolIt.MoveNext()) {
                var pool = poolIt.Current.Value;
                pool.Clean();
            }
            _pools.Clear();

            // 清掉所有的AB
            var it = _bundles.GetEnumerator();
            while (it.MoveNext()) {
                var assetBundle = it.Current.Value;
                assetBundle.AssetBundle.Unload(true);
            }
            _assets.Clear();
            _bundles.Clear();
        }

        private static void OnLoadAssetBundleComplete(SyncLoadAssetBundleOperation operation) {
            AssetBundleExtend bundle = ObjectPool<AssetBundleExtend>.Malloc();
            bundle.AssetBundle = operation.GetAssetBundle();
            bundle.Ref++;
            _bundles.Add(operation.AssetBundleName, bundle);

            operation.ProcessComplete();
        }

        private static void OnLoadBundleAndAssetComplete(SyncLoadAssetAndBundleOperation operation) {
            AssetBundleExtend bundle = ObjectPool<AssetBundleExtend>.Malloc();
            bundle.AssetBundle = operation.GetAssetBundle();
            bundle.Ref++;

            _bundles.Add(operation.AssetBundleName, bundle);
            _assets.Add(operation.ResPath, operation.GetAsset());

            if (0 < operation.Cache) {
                UnityObjectPool pool = InitObjectPool(operation.ResPath, operation.GetAsset(), operation.Cache);
                // operation.CustomCallBack (pool.Malloc());
                operation.TriggerCustomCallback(pool.Malloc());
            }
            else {
                operation.TriggerCustomCallback(operation.GetAsset());
                // operation.CustomCallBack.Invoke(operation.GetAsset());
            }

            operation.ProcessComplete();
        }

        private static void OnSyncLoadAssetsComplete(SyncLoadAssetOperation operation) {
            _assets.Add(operation.ResPath, operation.GetAsset());

            if (0 < operation.Cache) {
                UnityObjectPool pool = InitObjectPool(operation.ResPath, operation.GetAsset(), operation.Cache);
                // operation.CustomCallBack?.Invoke(pool.Malloc());
                operation.TriggerCustomCallback(pool.Malloc());
            }
            else {
                operation.TriggerCustomCallback(operation.GetAsset());
                // operation.CustomCallBack?.Invoke(operation.GetAsset());
            }

            operation.ProcessComplete();
        }

#if UNITY_EDITOR
        private static void OnAssetDataBaseLoadComplete(AssetDataBaseLoadOperation operation) {
            _assets.Add(operation.ResPath, operation.GetAsset());
            if (0 < operation.Cache) {
                UnityObjectPool pool = InitObjectPool(operation.ResPath, operation.GetAsset(), operation.Cache);
                operation.CustomCallBack?.Invoke(pool.Malloc());
            }
            else {
                operation.CustomCallBack?.Invoke(operation.GetAsset());
            }
        }
#endif

        private static UnityObjectPool InitObjectPool(string path, UnityEngine.Object prefab, uint cache) {
            GameObject gameObject = prefab as GameObject;
            if (null != gameObject) {
                UnityObjectPool pool = new UnityObjectPool(gameObject, cache);
                _pools.Add(path, pool);
                return pool;
            }
            return null;
        }

        private static bool TryGetAssetAndName(string path, out string assetPath, out string assetName) {
            assetPath = string.Empty;
            assetName = string.Empty;
            if (_infos.TryGetValue(path, out AssetBundleInfo info)) {
                if (UseAssetBundle) {
                    assetPath = info.Path;
                    assetName = info.Name;
                }
                else {
#if UNITY_EDITOR
                    assetPath = info.DataBase;
                    assetName = info.Name;
#endif
                }
                return true;
            }
            return false;
        }

        public static bool IsLoadingBySync(string assetPath, out SyncLoadOperation syncLoadOperation) {
            return _syncOperation.TryGetValue(assetPath, out syncLoadOperation);
        }

        private static AssetBundleExtend LoadAssetBundle(string assetPath, bool forceLoad = true) {
            AssetBundleExtend bundle = null;
            // 找不到AB且不要求强制加载时，直接返回空
            if (!_bundles.TryGetValue(assetPath, out bundle)) {
                if (!forceLoad) {
                    return null;
                }
                else {
                    string loadPath = System.IO.Path.Combine(AssetBundleBasePath, assetPath);
                    AssetBundle assetBundle = AssetBundle.LoadFromFile(loadPath);
                    // Debug.Log("====> LoadAssetBundle " + assetPath + ", " + loadPath + ", " + (null != assetBundle));

                    bundle = ObjectPool<AssetBundleExtend>.Malloc();
                    bundle.AssetBundle = assetBundle;

                    _bundles[assetPath] = bundle;
                }
            }
            else {
                // Debug.Log("====> LoadAssetBundle " + assetPath + ", " + (null != bundle));
                return bundle;
            }

            if (null == bundle) {
                UnityEngine.Debug.LogError("Can't find Asset Bundle By " + assetPath);
                return null;
            }

            var dependencies = GetDependencies(assetPath.ToLower());
            if (null != dependencies) {
                for (int i = 0; i < dependencies.Count; i++) {
                    AssetBundleExtend extend = LoadAssetBundle(dependencies[i]);
                    extend.Ref++;
                }
            }

            return bundle;
        }

        private static void LoadAssetBundleSync(string assetPath) {
            AssetBundleExtend assetBundle = LoadAssetBundle(assetPath, false);
            if (null == assetBundle) {
                var dependencies = GetDependencies(assetPath.ToLower());
                for (int i = 0; i < dependencies.Count; i++) {
                    if (!_syncOperation.ContainsKey(dependencies[i])) {
                        LoadAssetBundleSync(dependencies[i]);
                    }
                }

                if (!_syncOperation.ContainsKey(assetPath)) {
                    SyncLoadAssetBundleOperation op = ObjectPool<SyncLoadAssetBundleOperation>.Malloc();
                    AssetBundleCreateRequest depBundleCreateRequest = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(AssetBundleBasePath, assetPath));
                    op.Initialize(assetPath, depBundleCreateRequest, OnLoadAssetBundleComplete);
                    _syncOperation.Add(assetPath, op);
                }
            }
            else {
                assetBundle.Ref++;
            }
        }

        private static T GetPrefabFromAssetBundle<T>(string path) where T : UnityEngine.Object {
            if (!TryGetAssetAndName(path, out string assetPath, out string assetName)) {
                UnityEngine.Debug.LogError("Can't Get Asset Path And Asset Name By " + path);
                return null;
            }

            UnityEngine.Object prefab = null;
            if (UseAssetBundle) {
                var bundle = LoadAssetBundle(assetPath);
                prefab = bundle.AssetBundle.LoadAsset<T>(assetName);
                bundle.Ref += 1;
            }
            else {
#if UNITY_EDITOR
                prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
#endif
            }
            _assets[path] = prefab;

            return prefab as T;
        }

        private static bool LoadAssetBundleManifest(string bundleName) {
            var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(AssetBundleBasePath, bundleName));
            if (null == bundle) {
                UnityEngine.Debug.LogError("Can't get asset bundle by " + System.IO.Path.Combine(AssetBundleBasePath, bundleName));
                return false;
            }

            var manifset = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var bundles = manifset.GetAllAssetBundles();
            for (int i = 0; i < bundles.Length; i++) {
                string[] dependencies = manifset.GetDirectDependencies(bundles[i]);
                List<string> dependList = new List<string>(dependencies.Length);
                for (int j = 0; j < dependencies.Length; j++) {
                    dependList.Add(dependencies[j]);
                }
                _dependencies.Add(bundles[i], dependList);
            }
            return true;
        }

        public static List<string> GetDependencies(string path) {
            if (_dependencies.TryGetValue(path, out List<string> assetDependencies)) {
                return assetDependencies;
            }

            return null;
        }

        private static List<string> _delOperation = new List<string>(64);
        public static void Update() {
            var it = _syncOperation.GetEnumerator();
            while (it.MoveNext()) {
                if (it.Current.Value.Update()) {
                    _delOperation.Add(it.Current.Key);
                    it.Current.Value.Free();
                }
            }

            for (int i = 0; i < _delOperation.Count; i++) {
                _syncOperation.Remove(_delOperation[i]);
            }
            _delOperation.Clear();
        }
    }
}
