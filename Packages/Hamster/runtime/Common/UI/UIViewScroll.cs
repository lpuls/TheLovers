using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster {

    public class UIViewScrollData : IPool {
        public int ID = 0;

        public virtual void Reset() {
            ID = 0;
        }
    }

    public class UIViewScrollItem : MonoBehaviour {
        public int ID = 0;

        public virtual void Init(UIViewScrollData data) {
            ID = data.ID;
        }

        public virtual void Finish() {
            ID = 0;
        }
    }

    public class UIViewScroll : MonoBehaviour {
        private Transform _context = null;
        private string _prefabPath = string.Empty;
        private HashSet<GameObject> _items = new HashSet<GameObject>();

        public void Init(string prefabPath) {
            _prefabPath = prefabPath;
            _context = transform.Find("Scroll View/Viewport/Content");
        }

        public void AddItem<T>(UIViewScrollData data) where T: UIViewScrollItem {
            GameObject item = Asset.Load(_prefabPath);
            _items.Add(item);
            item.transform.SetParent(_context);
            T t = item.TryGetOrAdd<T>();
            t.Init(data);
        }

        public void RemoveItem(UIViewScrollItem item) {
            if (_items.Contains(item.gameObject)) {
                _items.Remove(item.gameObject);
                item.Finish();
                AssetPool.Free(item.gameObject);
                item.transform.SetParent(null);
            }
        }

        public void Clear() {
            var it = _items.GetEnumerator();
            while (it.MoveNext()) {
                GameObject inst = it.Current;
                inst.transform.SetParent(null);
                AssetPool.Free(inst);
            }
            _items.Clear();
        }

    }
}
