using System.Collections.Generic;

namespace Hamster {
    public static class ListPool<T> {
        private static List<List<T>> _pools = new List<List<T>>();

        public static List<T> Malloc() {
            if (_pools.Count > 0) {
                List<T> temp = _pools[0];
                _pools.RemoveAt(0);
                return temp;
            }
            else {
                return new List<T>(); 
            }
        }

        public static void Free(List<T> t) {
            t.Clear();
            _pools.Add(t);
        }
    }
}
