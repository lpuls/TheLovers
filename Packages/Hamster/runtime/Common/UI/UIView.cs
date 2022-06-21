using UnityEngine;
using UnityEngine.EventSystems;

namespace Hamster {

    public class UIView : MonoBehaviour {
        public IUIController UIController {
            get;
            set;
        }

        public T GetComponentFromChild<T>(string path) where T : UIBehaviour {
            Transform child = transform.Find(path);
            if (null == child)
                return null;
            return child.GetComponent<T>();
        }

        public T AddComponentToChild<T>(string path) where T : MonoBehaviour {
            Transform child = transform.Find(path);
            if (null == child)
                return null;
            return child.gameObject.AddComponent<T>();
        }

        public T GetOrAddSubView<T>(string path) where T : UIView {
            Transform child = transform.Find(path);
            if (null == child)
                return null;
            return child.gameObject.TryGetOrAdd<T>();
        }

        public void Hide() {
            gameObject.SetActive(false);
            OnHide();
        }

        public void Show() {
            gameObject.SetActive(true);
            OnShow();
            Debug.Log("======>Show View " + name);
        }

        protected virtual void OnHide() {
        }

        protected virtual void OnShow() {
        }

        public virtual void Initialize() {
        }

        public virtual void Finish() {
        }
    }
}