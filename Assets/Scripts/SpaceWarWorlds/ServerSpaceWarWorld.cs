using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ServerSpaceWarWorld : BaseSpaceWarWorld {

        private ServerNetDevice _netDevice = null;  // new ServerNetDevice();
        private ServerFrameDataManager _frameDataManager = new ServerFrameDataManager();

        public ServerNetDevice NetDevice {
            get {
                return _netDevice;
            }
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            // ConfigHelper = Single<ConfigHelper>.GetInstance();
            // base.InitWorld(typeof(Config.GameSetting).Assembly, null, GetType().Assembly);
            base.InitWorld();

            // 启用网络
            if (TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData) && !string.IsNullOrEmpty(swapData.Setting.ServerIP)) {
                _netDevice = new ServerNetDevice();
                _netDevice.RegistModule(new NetPingModule());
                _netDevice.RegistModule(new GameLogicSyncModule());
                _netDevice.RegistModule(new ServerGameLogicEventModule());
                _netDevice.Listen("127.0.0.1", 8888);
                RegisterManager<ServerNetDevice>(_netDevice);
            }

            // 注册管理器
            RegisterManager<BaseFrameDataManager>(_frameDataManager);
            _frameDataManager.OnGameStart += OnGameStart;

            // 服务端一起就创建服务器自己的飞机
            GameLogicUtility.ServerInitShip(1, true);
        }

        public override void AddTicker(IServerTicker serverTicker) {
            _frameDataManager.AddTicker(serverTicker);
        }

        public override void RemoveTicker(IServerTicker serverTicker) {
            _frameDataManager.RemoveTicker(serverTicker);
        }

        private void OnGameStart() {
             GameLogicUtility.ServerCreateEnemy(3);
        }

        protected override void Update() {
            ActiveWorld();

            // 网络模块更新
            base.Update();
            if (null != _netDevice)
                _netDevice.Update();
            
            _frameDataManager.Update();
        }

        public void OnDestroy() {
            Debug.Log("=======>Close Net Device");
            _netDevice.Close();
        }

        public void OnGUI() {
            GUILayout.Label("Frame " + _frameDataManager.ServerLogicFrame);
        }


        

        #region GM
        //[GM]
        //public static void GM_Ping(string[] gmParams) {
        //    GetWorld<NetSpaceWarWorld>()._netPingModule.SendClientPingMessage();
        //}
        #endregion

    }

}