using System.Collections.Generic;

namespace Hamster {
    public static class DictionaryPool<K, V> {
        private static List<Dictionary<K, V>> _pools = new List<Dictionary<K, V>> ();

        public static Dictionary<K, V> Malloc() {
            if (_pools.Count > 0) {
                Dictionary<K, V> temp = _pools[0];
                _pools.RemoveAt(0);
                return temp;
            }
            else {
                return new Dictionary<K, V>(); 
            }
        }

        public static void Free(Dictionary<K, V> t) {
            t.Clear();
            _pools.Add(t);
        }
    }
}
