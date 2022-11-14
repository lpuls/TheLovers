using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {
    public class ClientSpaceWarWorld : BaseSpaceWarWorld {

        private ClientNetDevice _netDevice = new ClientNetDevice();
        private ClientFrameDataManager _frameDataManager = new ClientFrameDataManager();

        private GameLogicSyncModule _logicSyncModule = null;

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld();

            _netDevice = new ClientNetDevice();

            _logicSyncModule = new GameLogicSyncModule();

            _netDevice.RegistModule(new NetPingModule());
            _netDevice.RegistModule(_logicSyncModule);
            _netDevice.RegistModule(new ClientGameLogicEventModule());

            _netDevice.Connect("127.0.0.1", 8888);
            RegisterManager<ClientNetDevice>(_netDevice);
            RegisterManager<ClientFrameDataManager>(_frameDataManager);

            _frameDataManager.OnBeginSimulate += OnBeginSimulate;

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
            SetProgress(30);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Ships/Enemy/RedShip", 2);
            SetProgress(30);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/Bullet/OriginBullet", 100);
            SetProgress(50);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/VFX/DeadBoom", 4);
            SetProgress(60);
            yield return _waiForEendOfFrame;

            Asset.Cache("Res/VFX/ShipSpawn", 8);
            SetProgress(100);
            yield return _waiForEendOfFrame;

            HideLoading();
        }

        public int GetFrameIndex() {
            return _frameDataManager.GameLogicFrame;
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
            ActiveWorld();

            base.Update();

            _netDevice.Update();
            _frameDataManager.Update();
        }

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();

        private void OnGUI() {
            style.fontSize = 24;
            GUILayout.Label("Frame " + _frameDataManager.GameLogicFrame);
            GUILayout.Label("Pack Ave " + _logicSyncModule.AveSize, style);
            GUILayout.Label("Max Pack " + _logicSyncModule.MaxSize, style);
            if (GUILayout.Button("Spawn Ship")) {
                ClientGameLogicEventModule module = _netDevice.GetModule(ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID) as ClientGameLogicEventModule;
                module.RequestSpawnShipToServer(2);
            }
        }
#endif

        public void OnBeginSimulate() {
            Single<UIManager>.GetInstance().Open<MainUIController>();
        }

        public override void AddTicker(IServerTicker serverTicker) {
            _frameDataManager.AddTicker(serverTicker);
        }

        public override void RemoveTicker(IServerTicker serverTicker) {
            _frameDataManager.RemoveTicker(serverTicker);
        }

    }
}
