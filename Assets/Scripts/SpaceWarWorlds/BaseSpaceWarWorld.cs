using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IServerTicker {
        void Tick(float dt);
    }

    public class BaseSpaceWarWorld : World {
        public Vector3 WorldSize = Vector3.one;

        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            base.InitWorld(typeof(Config.GameSetting).Assembly, null, GetType().Assembly);

            // 根据视口大小计算可行动区域
            Camera mainCamera = Camera.main;
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));
            WorldSize.x = max.x - min.x;
            WorldSize.z = max.z - min.z;

            // 预加载
            PreloadAssets();
        }

        protected virtual void PreloadAssets() {
            
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
            if (position.z + size >= bounds.max.z) {
                position.z = bounds.max.z - size;
            }
            if (position.z - size <= bounds.min.z) {
                position.z = bounds.min.z + size;
            }
            return position;
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
