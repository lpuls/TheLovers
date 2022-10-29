using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ServerSpaceWarWorld : BaseSpaceWarWorld {

        private ServerNetDevice _netDevice = null;  // new ServerNetDevice();
        private ServerFrameDataManager _serveFrameDataManager = new ServerFrameDataManager();
        private ClientFrameDataManager _clientFrameDataManager = new ClientFrameDataManager();
        private CollisionProcessManager _collisionResultManager = new CollisionProcessManager();
        private EnemyManager _enemyManager = null;

        public ServerNetDevice NetDevice {
            get {
                return _netDevice;
            }
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld();

            // 敌人管理器
            _enemyManager = gameObject.TryGetOrAdd<EnemyManager>();

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
            RegisterManager<CollisionProcessManager>(_collisionResultManager);
            RegisterManager<EnemyManager>(_enemyManager);

            // 初始化或注册事件
            _serveFrameDataManager.OnGameStart += OnGameStart;
            _serveFrameDataManager.OnNewFrameData += _clientFrameDataManager.AddNewFrameData;
            _serveFrameDataManager.AddTicker(_collisionResultManager);
            _serveFrameDataManager.AddTicker(_enemyManager);
            _clientFrameDataManager.OnBeginSimulate += OnBeginSimulate;


            // 服务端一起就创建服务器自己的飞机
            GameLogicUtility.ServerInitShip(1, true, ESpaceWarUnitType.Player1);
        }


        protected override IEnumerator PreloadAssets() {
            // 预先加载
            Asset.Cache("Res/Ships/Player/GreyPlayerShip", 2);
            SetProgress(20);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Player/RedPlayerShip", 2);
            SetProgress(30);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Enemy/PurpleShip", 2);
            SetProgress(40);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Enemy/PurpleShipLogic", 2);
            SetProgress(50);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Enemy/RedShip", 2);
            SetProgress(60);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Enemy/RedShipLogic", 2);
            SetProgress(70);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Player/GreyPlayerShipLogic", 2);
            SetProgress(75);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Bullet/OriginBullet", 100);
            SetProgress(80);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Bullet/OriginBulletLogic", 100);
            SetProgress(90);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/VFX/DeadBoom", 4);
            SetProgress(95);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/VFX/ShipSpawn", 8);
            SetProgress(100);
            yield return _waiForEendOfFrame;

            HideLoading();
        }

        public override void AddTicker(IServerTicker serverTicker) {
            _serveFrameDataManager.AddTicker(serverTicker);
        }

        public override void RemoveTicker(IServerTicker serverTicker) {
            _serveFrameDataManager.RemoveTicker(serverTicker);
        }

        private void OnGameStart() {
        }

        public void OnBeginSimulate() {
            Single<UIManager>.GetInstance().Open<MainUIController>();
        }

        protected override void Update() {
            ActiveWorld();

            // 网络模块更新
            base.Update();
            if (null != _netDevice)
                _netDevice.Update();
            
            _serveFrameDataManager.Update();
            _clientFrameDataManager.Update();
            // _enemyManager.Update();
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