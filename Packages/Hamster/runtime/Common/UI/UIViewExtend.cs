using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hamster {
    public static class UIViewExtend {
        public static Button GetButtonAndAddOnclick(this UIView view, string path, UnityAction onClick) {
            Button button = view.GetComponentFromChild<Button>(path);
            if (null == button) {
                UnityEngine.Debug.LogError("Get Button Failed By " + path);
                return null;
            }

            button.onClick.AddListener(onClick);
            return button;
        }

        public static void CleanClickEvent(this Button button) {
            button.onClick.RemoveAllListeners();
        }

        public static T GetComponentInChild<T>(this Transform transform, string path) where T : MonoBehaviour {
            Transform child = transform.Find(path);
            if (null == child) {
                Debug.LogError("Can't Find transform by " + path);
                return null; 
            }

            T t = child.GetComponent<T>();
            if (null == t)
                Debug.LogError("Can't Find " + typeof(T) + " by " + path);

            return t;
        }
    }
}
