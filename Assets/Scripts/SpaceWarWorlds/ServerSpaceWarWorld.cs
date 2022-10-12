using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ServerSpaceWarWorld : BaseSpaceWarWorld {

        private ServerNetDevice _netDevice = null;  // new ServerNetDevice();
        private ServerFrameDataManager _serveFrameDataManager = new ServerFrameDataManager();
        private ClientFrameDataManager _clientFrameDataManager = new ClientFrameDataManager();
        private EnemyManager _enemyManager = new EnemyManager();

        public ServerNetDevice NetDevice {
            get {
                return _netDevice;
            }
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
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
            RegisterManager<ServerFrameDataManager>(_serveFrameDataManager);
            RegisterManager<ClientFrameDataManager>(_clientFrameDataManager);
            _serveFrameDataManager.OnGameStart += OnGameStart;
            _serveFrameDataManager.OnNewFrameData += _clientFrameDataManager.AddNewFrameData;

            // 服务端一起就创建服务器自己的飞机
            GameLogicUtility.ServerInitShip(1, true);
        }


        protected override void PreloadAssets() {
            // 预先加载
            Asset.Cache("Res/Ships/Player/GreyPlayerShip", 2);
            Asset.Cache("Res/Ships/Player/RedPlayerShip", 2);
            Asset.Cache("Res/Ships/Enemy/PurpleShip", 2);
            Asset.Cache("Res/Ships/Enemy/PurpleShipLogic", 2);
            Asset.Cache("Res/Ships/Enemy/RedShip", 2);
            Asset.Cache("Res/Ships/Enemy/RedShipLogic", 2);
            Asset.Cache("Res/Ships/Player/GreyPlayerShipLogic", 2);
            Asset.Cache("Res/Bullet/OriginBullet", 100);
            Asset.Cache("Res/Bullet/OriginBulletLogic", 100);
        }

        public override void AddTicker(IServerTicker serverTicker) {
            _serveFrameDataManager.AddTicker(serverTicker);
        }

        public override void RemoveTicker(IServerTicker serverTicker) {
            _serveFrameDataManager.RemoveTicker(serverTicker);
        }

        private void OnGameStart() {
            GameLogicUtility.ServerCreateEnemy(10, new Vector3(0, 0, 10), 180);
            GameLogicUtility.ServerCreateEnemy(11, new Vector3(0, 0, 10), 180);
        }

        protected override void Update() {
            ActiveWorld();

            // 网络模块更新
            base.Update();
            if (null != _netDevice)
                _netDevice.Update();
            
            _serveFrameDataManager.Update();
            _clientFrameDataManager.Update();
            _enemyManager.Update();
        }

        public void OnDestroy() {
            Debug.Log("=======>Close Net Device");
            if (null != _netDevice)
                _netDevice.Close();
        }

        public void OnGUI() {
            GUILayout.Label("Frame " + _serveFrameDataManager.ServerLogicFrame);
        }



        #region GM
        [GM]
        public static void GM_SpawnEnemy(string[] gmParams) {
            GameLogicUtility.ServerCreateEnemy(10, new Vector3(0, 0, 10), 180);
        }
        #endregion

    }

}