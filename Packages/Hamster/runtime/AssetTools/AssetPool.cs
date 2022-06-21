using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    public class UnityObjectPool {
        private GameObject _prefab = null;
        private List<GameObject> _objectPool = new List<GameObject>(32);

        public UnityObjectPool(GameObject prefab, uint cacheCount) {
            if (null == prefab)
                Debug.LogError("The Prefab Of Unity Object Pool Invalid");

            _prefab = prefab;
            for (int i = 0; i < cacheCount; i++) {
                GameObject inst = GameObject.Instantiate<GameObject>(prefab);
                inst.SetActive(false);

                AssetPool pool = inst.AddComponent<AssetPool>();
                pool.Initialize(this);

                _objectPool.Add(inst);
            }
        }

        public GameObject Malloc() {
            GameObject inst;
            if (_objectPool.Count > 0) {
                inst = _objectPool[0];
                _objectPool.RemoveAt(0);
            }
            else {
                inst = GameObject.Instantiate<GameObject>(_prefab);
                AssetPool pool = inst.AddComponent<AssetPool>();
                pool.Initialize(this);
            }

            inst.SetActive(true);
            return inst;
        }

        public void Free(GameObject inst) {
            inst.SetActive(false);
            inst.transform.SetParent(null);
            _objectPool.Add(inst);
        }

        public void Clean() {
            _prefab = null;
            for (int i = 0; i < _objectPool.Count; i++) {
                GameObject.Destroy(_objectPool[i]);
            }
            _objectPool.Clear();
        }
    }

    public class AssetPool : MonoBehaviour {
        private UnityObjectPool _pool = null;
        private float _freeDelay = 0;

        public void Initialize(UnityObjectPool pool) {
            _pool = pool;
        }

        public GameObject Malloc() {
            return _pool.Malloc();
        }

        public void Free(float delay = -1) {
            if (delay > 0) {
                _freeDelay = delay;
            }
            else if (null != _pool) {
                _pool.Free(gameObject);
            }
            else {
                GameObject.Destroy(gameObject);
            }
        }

        private void Update() {
            if (_freeDelay > 0) {
                _freeDelay -= Time.deltaTime;
                if (_freeDelay <= 0) {
                    Free();
                }
            }
        }

        private void Free() {
            if (null != _pool) {
                _pool.Free(gameObject);
            }
            else {
                GameObject.Destroy(gameObject);
            }
        }

        public static void Free(GameObject res, float delay = -1) {
            AssetPool assetPool = res.GetComponent<AssetPool>();
            if (null == assetPool) {
                Debug.Log("Res " + res.name + " Can't Find AssetPool, Will Destory Right Now " + delay);
                GameObject.Destroy(res);
            }
            else {
                assetPool.Free(delay);
            }
        }

    }
}

