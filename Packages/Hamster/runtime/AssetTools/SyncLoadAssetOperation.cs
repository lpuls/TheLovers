using UnityEngine;

namespace Hamster {
    public class SyncLoadOperation : System.Collections.IEnumerator {
        protected bool _isDone = false;

        public uint Cache {
            get;
            set;
        }

        public object Current {
            get {
                return null;
            }
        }

        public bool MoveNext() {
            return IsDone();
        }

        public void Reset() {
        }

        protected virtual bool IsDone() {
            return true;
            ;
        }

        public virtual bool Update() {
            return true;
        }

        public virtual void Free() {
        }

        public void ProcessComplete() {
            _isDone = true;
        }

        public virtual void BindCustomCallback(System.Action<UnityEngine.Object> callbcak) {
        }

    }

    public class SyncLoadAssetBundleOperation : SyncLoadOperation, IPool {
        public delegate void OnLoadAssetBundleComplete(SyncLoadAssetBundleOperation operation);

        public string AssetBundleName = string.Empty;
        public AssetBundleCreateRequest _request = null;

        private float _timeOut = 1;
        private OnLoadAssetBundleComplete _assetCallBack = null;

        public void Initialize(string assetBundlaName, AssetBundleCreateRequest request, OnLoadAssetBundleComplete assetCallBack) {
            AssetBundleName = assetBundlaName;

            _request = request;
            _assetCallBack = assetCallBack;
        }

        public AssetBundle GetAssetBundle() {
            return _request.assetBundle;
        }

        protected override bool IsDone() {
            if (null == _request)
                return true;
            return _request.isDone;
        }

        public override bool Update() {
            if (_timeOut > 0)
                _timeOut -= Time.deltaTime;
            else
                Debug.LogError("Time out " + AssetBundleName);

            bool isDone = IsDone();
            if (isDone) {
                _assetCallBack?.Invoke(this);
            }
            return isDone && _isDone;
        }

        void IPool.Reset() {
            _timeOut = 1;
            _request = null;
            AssetBundleName = string.Empty;
            AssetBundleName = string.Empty;
            _assetCallBack = null;
        }

        public override void Free() {
            ObjectPool<SyncLoadAssetBundleOperation>.Free(this);
        }

    }

    public class SyncLoadAssetAndBundleOperation : SyncLoadOperation, IPool {
        public delegate void OnLoadComplete(SyncLoadAssetAndBundleOperation operation);

        private AssetBundleRequest _assetRequest = null;
        private AssetBundleCreateRequest _createRequest = null;

        public string ResPath = string.Empty;
        public string AssetName = string.Empty;
        public string AssetBundleName = string.Empty;

        public OnLoadComplete AssetCallBack = null;
        public event System.Action<UnityEngine.Object> CustomCallBack;

        private float _timeOut = 1;

        public void Initialize(string resKey, string assetName, string assetBundleName, AssetBundleCreateRequest request, OnLoadComplete assetCallBack, System.Action<UnityEngine.Object> callBack) {
            ResPath = resKey;
            AssetName = assetName;
            AssetBundleName = assetBundleName;

            AssetCallBack = assetCallBack;
            CustomCallBack += callBack;

            _createRequest = request;
            _createRequest.completed += OnLoadAssetBundleComplete;
        }

        public void TriggerCustomCallback(UnityEngine.Object asset) {
            CustomCallBack?.Invoke(asset);
        }

        private void OnLoadAssetBundleComplete(AsyncOperation op) {
            _assetRequest = _createRequest.assetBundle.LoadAssetAsync(AssetName);
        }

        protected override bool IsDone() {
            if (null == _createRequest)
                return true;
            return null != _assetRequest && _assetRequest.isDone;
        }

        public AssetBundle GetAssetBundle() {
            return _createRequest.assetBundle;
        }

        public UnityEngine.Object GetAsset() {
            return _assetRequest.asset;
        }

        public override bool Update() {
            if (_timeOut > 0)
                _timeOut -= Time.deltaTime;
            else
                Debug.LogError("Time out " + ResPath);

            bool isDone = IsDone();
            if (isDone) {
                AssetCallBack?.Invoke(this);
            }
            return isDone && _isDone;
        }

        public override void Free() {
            ObjectPool<SyncLoadAssetAndBundleOperation>.Free(this);
        }

        public override void BindCustomCallback(System.Action<UnityEngine.Object> callbcak) {
            CustomCallBack += callbcak;
        }
    }

    public class SyncLoadAssetOperation : SyncLoadOperation, IPool {
        public delegate void OnLoadAssetOperationComplete(SyncLoadAssetOperation op);

        private AssetBundleExtend _assetBundle = null;
        private AssetBundleRequest _assetRequest = null;

        public string ResPath = string.Empty;
        public string AssetName = string.Empty;
        public OnLoadAssetOperationComplete AssetCallBack = null;
        public event System.Action<UnityEngine.Object> CustomCallBack;

        private float _timeOut = 1;

        public void Initialize(string resKey, string assetName, AssetBundleExtend assetBundle, OnLoadAssetOperationComplete assetCallBack, System.Action<UnityEngine.Object> callBack) {
            ResPath = resKey;
            AssetName = assetName;

            CustomCallBack += callBack;
            AssetCallBack = assetCallBack;

            _assetBundle = assetBundle;
            _assetRequest = _assetBundle.AssetBundle.LoadAssetAsync(AssetName);
        }

        public void TriggerCustomCallback(UnityEngine.Object asset) {
            CustomCallBack?.Invoke(asset);
        }

        protected override bool IsDone() {
            if (null == _assetBundle || null == _assetRequest)
                return true;
            return _assetRequest.isDone;
        }

        public UnityEngine.Object GetAsset() {
            return _assetRequest.asset;
        }

        public UnityEngine.Object[] GetAssets() {
            return _assetRequest.allAssets;
        }

        public AssetBundleExtend GetAssetBundleExtend() {
            return _assetBundle;
        }

        public override bool Update() {
            if (_timeOut > 0)
                _timeOut -= Time.deltaTime;
            else
                Debug.LogError("Time out " + ResPath);

            bool isDone = IsDone();
            if (isDone) {
                AssetCallBack?.Invoke(this);
            }
            return isDone && _isDone;
        }

        void IPool.Reset() {
            _timeOut = 1;
            _assetBundle = null;
            _assetRequest = null;
            AssetName = string.Empty;
            CustomCallBack = null;
            AssetCallBack = null;
        }

        public override void Free() {
            ObjectPool<SyncLoadAssetOperation>.Free(this);
        }

        public override void BindCustomCallback(System.Action<UnityEngine.Object> callbcak) {
            CustomCallBack += callbcak;
        }
    }

#if UNITY_EDITOR
    public class AssetDataBaseLoadOperation : SyncLoadOperation, IPool {
        public delegate void OnLoadAssetByAssetDataBaseComplete(AssetDataBaseLoadOperation op);

        public string ResPath = string.Empty;
        public string AssetPath = string.Empty;
        public System.Action<UnityEngine.Object> CustomCallBack = null;

        private UnityEngine.Object _asset = null;

        public void Initialize(string resPath, string assetPath, OnLoadAssetByAssetDataBaseComplete assetCallback, System.Action<UnityEngine.Object> callBack) {
            ResPath = resPath;
            AssetPath = assetPath;
            CustomCallBack = callBack;
            _asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            assetCallback?.Invoke(this);
        }

        public UnityEngine.Object GetAsset() {
            return _asset;
        }

        protected override bool IsDone() {
            return true;
        }

        public override bool Update() {
            return true;
        }

        void IPool.Reset() {
            ResPath = string.Empty;
            AssetPath = string.Empty;
            CustomCallBack = null;
        }

        public override void Free() {
            ObjectPool<AssetDataBaseLoadOperation>.Free(this);
        }
    }
#endif
}
