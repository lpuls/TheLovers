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
        private LevelManager _levelManager = null;
        private ClientLevelManager _clientLevelManager = null;

        public ServerNetDevice NetDevice {
            get {
                return _netDevice;
            }
        }

        public bool GameResult {
            get;
            set;
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld();

            // 敌人管理器
            _levelManager = gameObject.TryGetOrAdd<LevelManager>();
            _clientLevelManager = gameObject.TryGetOrAdd<ClientLevelManager>();
            // _enemyManager.EnableSpawn = false;

            // 启用网络
            string levelPath = string.Empty;
            if (TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Mission>(swapData.LevelID, out Config.Mission mission)) {
                    levelPath = mission.Path;
                }

                _netDevice = new ServerNetDevice();
                _netDevice.RegistModule(new NetPingModule());
                _netDevice.RegistModule(new GameLogicSyncModule());
                _netDevice.RegistModule(new ServerGameLogicEventModule());
                //_netDevice.Listen("127.0.0.1", 8888);
                _netDevice.Listen(swapData.IP, swapData.Port);
                RegisterManager<ServerNetDevice>(_netDevice);
            }

            // 注册管理器
            RegisterManager<ServerFrameDataManager>(_serveFrameDataManager);
            RegisterManager<ClientFrameDataManager>(_clientFrameDataManager);
            RegisterManager<CollisionProcessManager>(_collisionResultManager);
            RegisterManager<LevelManager>(_levelManager);
            RegisterManager<ClientLevelManager>(_clientLevelManager);

            if (!string.IsNullOrEmpty(levelPath)) {
                _levelManager.Initilze(levelPath);
                _clientLevelManager.Initilze(levelPath);
            }

            // 初始化或注册事件
            _serveFrameDataManager.OnGameStart += OnGameStart;
            _serveFrameDataManager.OnNewFrameData += _clientFrameDataManager.AddNewFrameData;
            AddTicker(_collisionResultManager);
            AddTicker(_levelManager);
            _clientFrameDataManager.OnBeginSimulate += OnBeginSimulate;
            _clientFrameDataManager.OnFrameUpdate += _clientLevelManager.OnFrameDataUpdate;


            // 服务端一起就创建服务器自己的飞机
            GameLogicUtility.ServerInitShip(1, true, ESpaceWarUnitType.Player1);
        }


        protected override IEnumerator PreloadAssets() {
            // 预先加载
            Asset.Cache("Res/Unit/Player/GreyPlayerShip", 2);
            SetProgress(20);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Player/RedPlayerShip", 2);
            SetProgress(30);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Enemy/PurpleShip", 2);
            SetProgress(40);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Enemy/PurpleShipLogic", 2);
            SetProgress(50);
            yield return _waiForEendOfFrame;

            // 加载图集
            Single<AtlasManager>.GetInstance().LoadAtlas("Res/SpriteAtlas/MainUI");
            SetProgress(55);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Enemy/RedShip", 2);
            SetProgress(60);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Enemy/RedShipLogic", 2);
            SetProgress(70);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Unit/Player/PlayerShipLogic", 2);
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
        private void OnGameStart() {
        }

        public void OnBeginSimulate() {
            Single<UIManager>.GetInstance().Open<MainUIController>();
        }

        public List<NetSyncComponent> GetPlayers() {
            return _serveFrameDataManager.GetPlayers();
        }

        public void SetSystemPropertyDirty(EUpdateActorType updateActorType) {
            _serveFrameDataManager.AddSystemUpdateInfo(updateActorType);
        }

        protected override void Update() {
            ActiveWorld();

            // 网络模块更新
            base.Update();
            if (null != _netDevice)
                _netDevice.Update();

            Tick();
            // _enemyManager.Update();
        }

        protected override void FixTick() {
            _serveFrameDataManager.Update();
            _clientFrameDataManager.Update();
        }

        public void OnDestroy() {
            Debug.Log("=======>Close Net Device");
            if (null != _netDevice)
                _netDevice.Close();
        }

        public void OnGUI() {
            // GUILayout.Label("Frame " + _serveFrameDataManager.ServerLogicFrame);
        }



        #region GM
        [GM]
        public static void GM_SpawnEnemy(string[] gmParams) {
            GameLogicUtility.ServerCreateEnemy(10, new Vector3(0, 0, 10), 180);
        }

        [GM]
        public static void GM_EnableEnemySpawn(string[] gmParams) {
            LevelManager enemyManager = World.GetWorld().GetManager<LevelManager>();
            enemyManager.EnableSpawn = !enemyManager.EnableSpawn;
        }

        [GM]
        public static void GM_KillAllEnemy(string[] gmParams) {
            LevelManager enemyManager = World.GetWorld().GetManager<LevelManager>();
            enemyManager.DestroyAllUnit();
        }

        #endregion

    }

}