using System.Collections.Generic;

namespace Hamster {
    public static class HashSetPool<T> {
        private static List<HashSet<T>> _pools = new List<HashSet<T>>();

        public static HashSet<T> Malloc() {
            if (_pools.Count > 0) {
                HashSet<T> temp = _pools[0];
                _pools.RemoveAt(0);
                return temp;
            }
            else {
                return new HashSet<T>();
            }
        }

        public static void Free(HashSet<T> t) {
            t.Clear();
            _pools.Add(t);
        }
    }
}
