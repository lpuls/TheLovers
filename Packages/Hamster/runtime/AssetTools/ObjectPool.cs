using System.Collections.Generic;

namespace Hamster {
    public interface IPool {
        void Reset();
    }

    public static class ObjectPool<T> where T : IPool, new() {
        private static List<T> _pools = new List<T>(64);

        public static T Malloc() {
            if (_pools.Count <= 0) {
                return new T();
            }
            else {
                T t = _pools[0];
                _pools.RemoveAt(0);
                return t;
            }
        }

        public static void Free(T t) {
            t.Reset();
            _pools.Add(t);
        }

        public static void Clean() {
            _pools.Clear();
        }
    }
}

