using System;
using System.Collections.Generic;

namespace Hamster {

    public interface IUIController {
        void SetOwner(object Owner);

        void Initialize(UIView view, UIModule module);

        void Finish();

        void PushFrom(Type parentType);

        Type PopFrom();

        void CleanFrom();

        void Close();

        void GoBack();
    }

    public class UIController<View, Module> : IUIController where View: UIView where Module : UIModule {
        protected UIManager _manager = null;
        protected View _view = null;
        protected Module _module = null;

        private readonly Stack<Type> _from = new Stack<Type>();

        public void SetOwner(object Owner) {
            _manager = Owner as UIManager;
        }

        public void Initialize(UIView view, UIModule module) {
            _view = view as View;
            _view.UIController = this;
            _view.Initialize();

            _module = module as Module;

            OnInitialize();
        }

        public void Finish() {
            _view.Finish();
            _module.Finish();
            OnFinish();

            _view = null;
            _module = null;
        }

        protected virtual void OnInitialize() {
        }

        protected virtual void OnFinish() {
        }

        public void Close() {
            _manager.Close(GetType()); 
        }

        public void GoBack() {
            _manager.GoBack(); 
        }

        public void PushFrom(Type parentType) {
            _from.Push(parentType);
        }

        public Type PopFrom() {
            if (_from.Count > 0)
                return _from.Pop();
            return null;
        }

        public void CleanFrom() {
            _from.Clear(); 
        }
    }
}