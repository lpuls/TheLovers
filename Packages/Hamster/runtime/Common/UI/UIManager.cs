﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hamster {

    public class UIInfoAttribute : System.Attribute {
        public string AssetPath = string.Empty;
        public Type ViewType = null;
        public Type ModuleType = null;

        public UIInfoAttribute(string assetPath, Type viewType, Type moduleType) {
            AssetPath = assetPath;
            ViewType = viewType;
            ModuleType = moduleType;
        }
    }

    public class UIManager {
        private class UIInfo {
            public string AssetPath = string.Empty;
            public UIView View = null;
            public UIModule Module = null;
            public IUIController Controller = null;

            public Type ViewType = null;
        }

        private bool _isInit = false;
        private RectTransform _canvas = null;
        private IUIController _current = null;
        private Dictionary<Type, UIInfo> _uiInfos = new Dictionary<Type, UIInfo>();

        // 固定功能
        private LoadingUI _loadingUI = null;
        private TransitionUI _transitionUI = null;

        public void Initialize(Assembly assembly) {
            if (_isInit)
                return;
            _isInit = true;

            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type classType = types[i];
                UIInfoAttribute attribute = classType.GetCustomAttribute<UIInfoAttribute>();
                if (null == attribute)
                    continue;

                UIInfo uiInfo = new UIInfo() {
                    Controller = Activator.CreateInstance(classType) as IUIController,
                    AssetPath = attribute.AssetPath,
                    ViewType = attribute.ViewType,
                    View = null,
                    Module = Activator.CreateInstance(attribute.ModuleType) as UIModule
                };
                uiInfo.Controller.SetOwner(this);

                _uiInfos[classType] = uiInfo;
            }
        }

        public void ResetUI() {
            GameObject canvas = GameObject.Find("Canvas");
            _canvas = canvas.GetComponent<RectTransform>();

            // 初始化常驻UI
            if (null != canvas) {
                // GameObject.DontDestroyOnLoad(canvas);
                Transform loadingInstance = canvas.transform.Find("Loading");
                if (null != loadingInstance) {
                    _loadingUI = loadingInstance.gameObject.TryGetOrAdd<LoadingUI>();
                    _loadingUI.gameObject.SetActive(false);
                }

                Transform transitionsInstance = canvas.transform.Find("Transitions");
                if (null != transitionsInstance) {
                    _transitionUI = transitionsInstance.gameObject.TryGetOrAdd<TransitionUI>();
                    _transitionUI.gameObject.SetActive(false);
                }

            }
        }

        public void SetLoadingProgress(int value) {
            _loadingUI.SetProgress(value / 100.0f);
        }

        public void ShowLoading() {
            UnityEngine.Debug.Assert(null != _loadingUI, "Loading Is Null");
            _loadingUI.gameObject.SetActive(true);
        }

        public void HideLoading() {
            UnityEngine.Debug.Assert(null != _loadingUI, "Loading Is Null");
            _loadingUI.gameObject.SetActive(false);
        }

        public void ShowTransition() {
            UnityEngine.Debug.Assert(null != _transitionUI, "Transition Is Null");
            _transitionUI.ShowTransition();
        }

        public void HideTransition() {
            UnityEngine.Debug.Assert(null != _transitionUI, "Transition Is Null");
            _transitionUI.HideTransition();
        }

        public UIModule GetModule<T>() where T : IUIController {
            if (_uiInfos.TryGetValue(typeof(T), out UIInfo info)) {
                return info.Module;
            }
            return null;
        }

        public IUIController Open<T>() where T: IUIController {
            return Open(typeof(T));
        }

        public IUIController Open(Type controllerType) {
            if (_uiInfos.TryGetValue(controllerType, out UIInfo info)) {
                if (null == info.View) {
                    GameObject uiObject = Asset.Load(info.AssetPath);
                    uiObject.transform.SetParent(_canvas, false);
                    RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.offsetMin = Vector2.zero;
                    info.View = uiObject.TryGetOrAddByType(info.ViewType) as UIView;
                }
                info.Controller.Initialize(info.View, info.Module);
                return info.Controller;
            }
            return null;
        }

        public void Show(Type controllerType) {
            if (_uiInfos.TryGetValue(controllerType, out UIInfo info)) {
                if (null != info.View) {
                    info.View.Show();
                }
            }
        }

        public void Hide(Type controllerType) {
            if (_uiInfos.TryGetValue(controllerType, out UIInfo info)) {
                info.View.Hide();
            }
        }

        public void Close<T>(float delay = -1) {
            Close(typeof(T), true, delay);
        }

        public void Close(Type controllerType, bool finish = true, float delay = -1) {
            if (_uiInfos.TryGetValue(controllerType, out UIInfo info)) {
                if (finish) {
                    info.Controller.Finish();
                    AssetPool.Free(info.View.gameObject, delay);
                    info.View = null;
                }
                else {
                    info.View.Hide();  
                }
            }
        }

        public void CloseAll() {
            foreach (var item in _uiInfos) {
                if (null != item.Value.View) {
                    if (null != item.Value.Controller)
                        item.Value.Controller.Finish();
                    AssetPool.Free(item.Value.View.gameObject);
                }
                item.Value.View = null;
            }
        }

        public IUIController GoTo<T>() where T : IUIController {
            Type controllerType = typeof(T);

            // 不要重复打开界面
            if (null != _current && controllerType == _current.GetType())
                return _current;

            if (_uiInfos.TryGetValue(controllerType, out UIInfo info)) {
                IUIController next = Open<T>();
                if (null != next) {
                    if (null != _current) {
                        next.PushFrom(_current.GetType());
                        Close(_current.GetType(), false);
                    }
                    _current = next;
                    return _current;
                }
            }
            return null;
        }

        public IUIController GoBack() {
            if (null == _current)
                return null;

            Type fromType = _current.PopFrom();
            IUIController from = Open(fromType);
            Close(_current.GetType());
            _current = from;

            return _current;
        }

        public void DestroyAll() {
            var it = _uiInfos.GetEnumerator();
            while (it.MoveNext()) {
                UIInfo info = it.Current.Value;
                if (null != info.View) {
                    info.Controller.CleanFrom();
                    Close(info.Controller.GetType());
                }
            }
            _current = null;
        }

        public Vector3 ToUILocation(Vector3 location) {
            return Camera.main.WorldToScreenPoint(location);
        }

        public void AttachTo(Transform transform) {
            transform.SetParent(_canvas);
        }
    }
}
