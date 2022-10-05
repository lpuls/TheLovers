using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {
    public class ClientSpaceWarWorld : BaseSpaceWarWorld {

        private ClientNetDevice _netDevice = new ClientNetDevice();
        private ClientFrameDataManager _frameDataManager = new ClientFrameDataManager();

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld();

            _netDevice = new ClientNetDevice();

            _netDevice.RegistModule(new NetPingModule());
            _netDevice.RegistModule(new GameLogicSyncModule());
            _netDevice.RegistModule(new ClientGameLogicEventModule());

            _netDevice.Connect("127.0.0.1", 8888);
            RegisterManager<ClientNetDevice>(_netDevice);
            RegisterManager<BaseFrameDataManager>(_frameDataManager);

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

        private void OnGUI() {
            GUILayout.Label("Frame " + _frameDataManager.GameLogicFrame);
            if (GUILayout.Button("Spawn Ship")) {
                ClientGameLogicEventModule module = _netDevice.GetModule(ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID) as ClientGameLogicEventModule;
                module.RequestSpawnShipToServer(2);
            }
        }

        public override void AddTicker(IServerTicker serverTicker) {
            _frameDataManager.AddTicker(serverTicker);
        }

        public override void RemoveTicker(IServerTicker serverTicker) {
            _frameDataManager.RemoveTicker(serverTicker);
        }

    }
}
