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

        protected virtual void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            // 初始化GM组件
            if (null != gmAssemlby)
                GMAttributeProcessor.Processor(gmAssemlby);

            // 初始化资源
            TextAsset gameConfigText = Resources.Load<TextAsset>("GameConfig");
            GameConfig gameConfig = JsonUtility.FromJson<GameConfig>(gameConfigText.text);

            if (gameConfig.FindPlatformConfig(Application.platform.ToString(), out PlatformConfig value)) {
                Asset.UseAssetBundle = value.UseAssetBundle;
                Asset.AssetBundleBasePath = string.Format("{0}{1}", Application.dataPath, value.AssetBundlePath);
                Asset.Initialize("AssetBundleConfig", new string[] { value.Manifast });
            }

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

        protected virtual void Update() {
            Asset.Update();
        }

        #region 通用功能
        public void ShowLoading() {
            this.UIManager.ShowLoading();
        }

        public void HideLoading() {
            this.UIManager.HideLoading();
        }

        public void SetProgress(int value) {
            this.UIManager.SetLoadingProgress(value);
        }

        public void ShowTransition() {
            this.UIManager.ShowTransition();
        }

        public void HideTransition() {
            this.UIManager.HideTransition();

        }

        #endregion
    }
}