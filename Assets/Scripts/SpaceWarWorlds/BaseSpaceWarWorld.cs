using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IServerTicker {
        void Tick(float dt);
        bool IsEnable();
    }

    public class BaseSpaceWarWorld : World {
        public Vector3 WorldSize = Vector3.one;

        protected WaitForEndOfFrame _waiForEendOfFrame = new WaitForEndOfFrame();
        
        public int PlayerNetID {
            get;
            set;
        }

        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            base.InitWorld(typeof(Config.GameSetting).Assembly, null, GetType().Assembly);

            // 根据视口大小计算可行动区域
            CalWorldSize();

            // 预加载
            StartCoroutine(PreloadAssets());
        }

        public void CalWorldSize() {
            Camera mainCamera = Camera.main;
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));
            WorldSize.x = max.x - min.x;
            WorldSize.y = max.y - min.y;
        }

        protected virtual IEnumerator PreloadAssets() {
            yield break;
        }

        public bool InWorld(Vector3 position) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            return bounds.Contains(position);
        }

        public bool InWorld(Vector3 origin, Vector3 direction, out float distance) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            Ray ray = new Ray(origin, direction);
            return bounds.IntersectRay(ray, out distance);
        }

        public Vector3 ClampInWorld(Vector3 position, float size) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            if (position.x + size >= bounds.max.x) {
                position.x = bounds.max.x - size;
            }
            if (position.x - size <= bounds.min.x) {
                position.x = bounds.min.x + size;
            }
            if (position.y + size >= bounds.max.z) {
                position.y = bounds.max.y - size;
            }
            if (position.y - size <= bounds.min.y) {
                position.y = bounds.min.y + size;
            }
            return position;
        }

        public Vector3 GetRandomEnemtyMoveTarget(float size) {
            float minY = size;
            float maxY = WorldSize.z / 2 - size;
            float minX = -WorldSize.x / 2 + size;
            float maxX = WorldSize.x / 2 - size;
            return new Vector3(
                UnityEngine.Random.Range(minX, maxX),
                0,
                UnityEngine.Random.Range(minY, maxY)
                );
            ;

        }

        public virtual void AddTicker(IServerTicker serverTicker) {
        }

        public virtual void RemoveTicker(IServerTicker serverTicker) {
        }


#if UNITY_EDITOR
        public virtual void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, WorldSize);
        }
#endif
    }
}
