using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IServerTicker {

        int GetPriority();

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
            UIManager = Single<UIManager>.GetInstance();
            base.InitWorld(typeof(Config.GameSetting).Assembly, typeof(MainUIController).Assembly, GetType().Assembly);
            UIManager.ResetUICamera();

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

        public Vector3 ClampInWorld(Vector3 position, Vector3 size) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            if (position.x + size.x >= bounds.max.x) {
                position.x = bounds.max.x - size.x;
            }
            if (position.x - size.x <= bounds.min.x) {
                position.x = bounds.min.x + size.x;
            }
            if (position.y + size.y >= bounds.max.y) {
                position.y = bounds.max.y - size.y;
            }
            if (position.y - size.y <= bounds.min.y) {
                position.y = bounds.min.y + size.y;
            }
            return position;
        }

        public Vector3 GetRandomEnemtyMoveTarget(Vector3 size) {
            float minY = -WorldSize.y / 2 + size.y;
            float maxY = WorldSize.y / 2 - size.y;
            float minX = size.x;
            float maxX = WorldSize.x / 2 - size.x;
            return new Vector3(
                UnityEngine.Random.Range(minX, maxX),
                UnityEngine.Random.Range(minY, maxY),
                0
                );
            ;

        }

        public int CompressionVectorToInt(Vector3 position) {
            ushort x = (ushort)((position.x + WorldSize.x) * 100);
            ushort y = (ushort)((position.y + WorldSize.y) * 100);
            int result = x;
            return (result << 16) | y;
        }

        public Vector3 UncompressionIntToVector(int position) {
            ushort y = (ushort)(position & 0xFFFF);
            ushort x = (ushort)((position >> 16) & 0xFFFF);
            return new Vector3(
                    ((float)x) / 100 - WorldSize.x,
                    ((float)y) / 100 - WorldSize.y
                );
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
