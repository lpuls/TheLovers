using UnityEngine;

namespace Hamster {
    public class Single<T> where T : new() {
        private static T _instance = default;
        public static T GetInstance() {
            if (null == _instance)
                _instance = new T();
            return _instance;
        }
    }

    public class SingleMonobehaviour<T> where T : MonoBehaviour {
        private static T _instance = default;

        public static T GetInstance(string name) {
            if (null == _instance) {
                GameObject instances = GameObject.Find(name);
                if (null == instances)
                    instances = new GameObject(name);
                _instance = instances.AddComponent<T>();
            }
            return _instance;
        }
    }
}
