using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hamster {
    public class World : MonoBehaviour {

        private static World _instance = null;

        protected LoadingUI _loadingUI = null;
        protected Animator _transitionUI = null;
        private Dictionary<Type, object> _managers = new Dictionary<Type, object>();

        public static T GetWorld<T>() where T : World {
            return _instance as T; 
        }

        public static bool TryGetWorld<T>(out T world) where T : World {
            world = _instance as T;
            return null != world;
        }

        public static World GetWorld() {
            return _instance;
        }

        protected void ActiveWorld() {
            _instance = this;
        }


        public ConfigHelper ConfigHelper {
            get;
            protected set;
        }

        public UIManager UIManager {
            get;
            protected set;
        }

        public bool TryGetWorldSwapData<T>(out T swapData) where T : WorldSwapData {
            swapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as T;
            return null != swapData;
        } 

        protected void RegisterManager<T>(T manager) {
            _managers.Add(typeof(T), manager);
        }

        public bool TryGetManager<T>(out T manager) {
            manager = default;
            if (_managers.TryGetValue(typeof(T), out object value)) {
                manager = (T)value;
                return null != manager;
            }
            return false;
        }

        public T GetManager<T>() {
            if (_managers.TryGetValue(typeof(T), out object value))
                return (T)value;
            return default;
        }

        protected void InitLoading() {
            // 初始化加载界面
            GameObject canvasGameObject = GameObject.Find("Canvas");
            if (null != canvasGameObject) {
                GameObject.DontDestroyOnLoad(canvasGameObject);
                Transform loadingInstance = canvasGameObject.transform.Find("Loading");
                if (null != loadingInstance)
                    _loadingUI = loadingInstance.gameObject.TryGetOrAdd<LoadingUI>();
            }
        }

        protected void InitTransitionUI() {
            // 初始化转场界面
            GameObject canvasGameObject = GameObject.Find("Canvas");
            if (null != canvasGameObject) {
                GameObject.DontDestroyOnLoad(canvasGameObject);
                Transform transitionsInstance = canvasGameObject.transform.Find("Transitions");
                if (null != transitionsInstance)
                    _transitionUI = transitionsInstance.gameObject.TryGetOrAdd<Animator>();
            }
        }

        protected virtual void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            // 初始化GM组件
            if (null != gmAssemlby)
                GMAttributeProcessor.Processor(gmAssemlby);

            // 初始化资源
#if UNITY_EDITOR
            Asset.UseAssetBundle = false;
#else
            Asset.UseAssetBundle = true;
#endif
            Asset.AssetBundleBasePath = Application.dataPath + "/../AssetBundle/Win";
            Asset.Initialize("AssetBundleConfig", new string[] { "Win" });

            // 初始化配置文件
            if (null != configAssembly) {
                TextAsset textAsset = Asset.Load<TextAsset>("Res/Config");
                ConfigHelper.Initialize(textAsset.bytes, configAssembly);
            }

            // 初始化UI组件
            if (null != uiAssembly) {
                UIManager.Initialize(uiAssembly);
            }
        }

        public void SetProgress(int value) {
            _loadingUI.SetProgress(value / 100.0f);
        }

        public void ShowLoading() {
            _loadingUI.gameObject.SetActive(true);
        }

        public void HideLoading() {
            _loadingUI.gameObject.SetActive(false);
        }

        public void ShowTransition() {
            _transitionUI.Play("Transition", 0, 0);
            Debug.Log("Play Transition");
            // _transitionUI.SetTrigger("Play");
        }

        protected virtual void Update() {
            Asset.Update();
        }
    }
}