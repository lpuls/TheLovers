using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hamster {
    public class World : MonoBehaviour {

        private static World _instance = null;

        private Dictionary<Type, object> _managers = new Dictionary<Type, object>();

        public static T GetWorld<T>() where T : World {
            return _instance as T; 
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

        protected void RegisterManager<T>(T manager) {
            _managers.Add(typeof(T), manager);
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

        protected virtual void Update() {
            Asset.Update();
        }
    }
}