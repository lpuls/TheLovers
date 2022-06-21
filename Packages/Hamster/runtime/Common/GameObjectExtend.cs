using System;
using UnityEngine;

namespace Hamster {
    public static class GameObjectExtend {
        public static T TryGetOrAdd<T>(this GameObject gameObject) where T : Component {
            T c = gameObject.GetComponent<T>();
            if (null == c)
                c = gameObject.AddComponent<T>();
            return c;
        }

        public static Component TryGetOrAddByType(this GameObject gameObject, Type componentType) {
            Component c = gameObject.GetComponent(componentType);
            if (null == c)
                c = gameObject.AddComponent(componentType);
            return c;
        }

        public static T TryGetOrAddFromChild<T>(this GameObject gameObject, string path) where T : Component {
            Transform child = gameObject.transform.Find(path);
            T c = child.gameObject.GetComponent<T>();
            if (null == c)
                c = child.gameObject.AddComponent<T>();
            return c;
        }
    }
}
