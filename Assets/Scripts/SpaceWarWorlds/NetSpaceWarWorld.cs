using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetSpaceWarWorld : World {

        private NetDevice _netDevice = null;
        private FrameDataManager _frameDataManager = new FrameDataManager();

        private NetPingModule _netPingModule = new NetPingModule();
        private GameLogicSyncModule _gameLogicSyncModule = new GameLogicSyncModule();
        // private GameLogicReadyEventsModule _gameLogicReadyEventsModule = new GameLogicReadyEventsModule();

        public float LogicTime {
            get;
            private set;
        }

        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            base.InitWorld(typeof(Config.GameSetting).Assembly, null, GetType().Assembly);

            // 注册管理器
            // RegisterManager<ClientNetDevice>(_netDevice);
            RegisterManager<FrameDataManager>(_frameDataManager);

            // 启用网络
            //_netDevice.RegistModule(_netPingModule);
            //_netDevice.RegistModule(_gameLogicSyncModule);
            //_netDevice.RegistModule(_gameLogicReadyEventsModule);
            //_netDevice.Connect("127.0.0.1", 8888);

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

        protected override void Update() {
            base.Update();
            if (null != _netDevice)
                _netDevice.Update();
            _frameDataManager.Update();
        }

        private bool _requestedSpawnShip = false;

        public void OnGUI() {
            GUILayout.Label(string.Format("Ping: {0}", _netPingModule.Ping));

            if (null == _netDevice) {
                if (GUILayout.Button("Build Room")) {
                    ServerNetDevice serverNetDevice = new ServerNetDevice();

                    ServerGameLogicEventModule serverGameLogicEventModule = new ServerGameLogicEventModule();
                    serverNetDevice.RegistModule(_netPingModule);
                    serverNetDevice.RegistModule(_gameLogicSyncModule);
                    serverNetDevice.RegistModule(serverGameLogicEventModule);

                    serverNetDevice.Listen("127.0.0.1", 8888);
                    RegisterManager<ServerNetDevice>(serverNetDevice);

                    _netDevice = serverNetDevice;

                    // 服务端一起就创建服务器自己的飞机
                    PlayerSpawnUtility.ServerInitShip(1);
                }
                if (GUILayout.Button("Join Room")) {
                    ClientNetDevice clientNetDevice = new ClientNetDevice();

                    clientNetDevice.RegistModule(_netPingModule);
                    clientNetDevice.RegistModule(_gameLogicSyncModule);
                    clientNetDevice.RegistModule(new ClientGameLogicEventModule());

                    clientNetDevice.Connect("127.0.0.1", 8888);
                    RegisterManager<ClientNetDevice>(clientNetDevice);

                    _netDevice = clientNetDevice;
                }
            }
            else if (!_netDevice.IsServer() && !_requestedSpawnShip) {
                if (GUILayout.Button("Spawn Ship")) {
                    _requestedSpawnShip = true;

                    ClientGameLogicEventModule module = _netDevice.GetModule(ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID) as ClientGameLogicEventModule;
                    module.RequestSpawnShipToServer(2);
                }
            }
        }

        #region GM
        [GM]
        public static void GM_Ping(string[] gmParams) {
            GetWorld<NetSpaceWarWorld>()._netPingModule.SendClientPingMessage();
        }
        #endregion

    }

}