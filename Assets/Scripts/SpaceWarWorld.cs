using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class SpaceWarWorld : World {



        private NetDevice _netDevice = new NetDevice();
        private FrameDataManager _frameDataManager = new FrameDataManager();

        private NetPingModule _netPingModule = new NetPingModule();
        private GameLogicSyncModule _gameLogicSyncModule = new GameLogicSyncModule();
        private GameLogicReadyEventsModule _gameLogicReadyEventsModule = new GameLogicReadyEventsModule();

        public float LogicTime {
            get;
            private set;
        }

        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld(null, null, GetType().Assembly);

            // 注册管理器
            RegisterManager<NetDevice>(_netDevice);
            RegisterManager<FrameDataManager>(_frameDataManager);

            // 启用网络
            _netDevice.RegistModule(_netPingModule);
            _netDevice.RegistModule(_gameLogicSyncModule);
            _netDevice.RegistModule(_gameLogicReadyEventsModule);
            _netDevice.Connect("127.0.0.1", 8888);

            // 加载测试用logic数据
            // TextAsset textAsset = Asset.Load<TextAsset>("Res/Test/Data");
            // _frameDataManager.AnalyzeBinary(textAsset.bytes);
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

        // private float _sendPingTime = 0;

        protected override void Update() {
            base.Update();
            _netDevice.Update();
            _frameDataManager.Update();

            //_sendPingTime += Time.deltaTime;
            //if (_sendPingTime >= 1.0f) {
            //    _netPingModule.SendPingMessage();
            //    _sendPingTime -= 1.0f;
            //}

            // 每一个逻辑辑时长更新一次逻辑数据
            //if (!SpaceWarWorld.Simulate)
            //    return;

            //LogicTime += Time.deltaTime;
            //if (LogicTime >= LOGIC_FRAME) {
            //    LogicTime -= LOGIC_FRAME;
            //    _frameDataManager.NextFrame();
            //}

        }

        public void OnGUI() {
            GUILayout.Label(string.Format("Ping: {0}", _netPingModule.Ping));
        }

        #region GM
        [GM]
        public static void GM_Ping(string[] gmParams) {
            GetWorld<SpaceWarWorld>()._netPingModule.SendPingMessage();
        }
        [GM]
        public static void GM_SpawnShip(string[] gamPrams) {
            NetDevice netDevice = GetWorld().GetManager<NetDevice>();
            GameLogicReadyEventsModule gameLogicReadyEvents = netDevice.GetModule(GameLogicReadyEventsModule.NET_GAME_LOGIC_READY_EVENT_ID) as GameLogicReadyEventsModule;
            gameLogicReadyEvents.SendSpawnShipID(1);
        }
        #endregion

    }

}