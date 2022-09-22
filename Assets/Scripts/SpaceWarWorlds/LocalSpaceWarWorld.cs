using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {

    public class BaseSpaceWarWorld : World {
        public Vector3 WorldSize = Vector3.one;

        public void Awake() {
            ActiveWorld();
            InitWorld();
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

#if UNITY_EDITOR
        public virtual void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, WorldSize);
        }
#endif
    }

    public class LocalSpaceWarWorld : BaseSpaceWarWorld {

        private ClientNetDevice _netDevice = new ClientNetDevice();
        private FrameDataManager _frameDataManager = new FrameDataManager();

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            base.InitWorld(null, null, GetType().Assembly);

            _netDevice = new ClientNetDevice();

            _netDevice.RegistModule(new NetPingModule());
            _netDevice.RegistModule(new ClientGameLogicEventModule());

            _netDevice.Connect("127.0.0.1", 8888);
            RegisterManager<ClientNetDevice>(_netDevice);
        }

        public FrameData GetCurrentFrameData() {
            return _frameDataManager.GetCurrentFrameData();
        }

        public FrameData GetPreFrameData() {
            return _frameDataManager.GetPreFrameData();
        }


        public float GetLogicFramepercentage() {
            return _frameDataManager.GetLogicFramepercentage();
        }

        private void OnGUI() {
            if (GUILayout.Button("Spawn Ship")) {
                ClientGameLogicEventModule module = _netDevice.GetModule(ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID) as ClientGameLogicEventModule;
                module.RequestSpawnShipToServer(2);
            }
        }

    }
}
