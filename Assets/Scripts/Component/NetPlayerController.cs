using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetPlayerController : LocalPlayerController {

        private NetSyncComponent _netSyncComponent = null;
        private GameLogicSyncModule _gameLogicSyncModule = null;

        public override void Init() {
            _netSyncComponent = gameObject.TryGetOrAdd<NetSyncComponent>();

            // 主控端需要通过该网络模块转发数据
            // if (_netSyncComponent.IsAutonomousProxy()) {
            ClientNetDevice netDeivce = World.GetWorld().GetManager<ClientNetDevice>();
            if (null == netDeivce || !netDeivce.IsValid) {
                Debug.LogError("=====>Local LocalPlayerController Has not NetDevice ");
                return;
            }

            _gameLogicSyncModule = netDeivce.GetModule(GameLogicSyncModule.NET_GAME_LOGIC_SYNC_ID) as GameLogicSyncModule;
            if (null == _gameLogicSyncModule) {
                Debug.LogError("=====>Local LocalPlayerController Has not GameLogicSyncModule ");
                return;
            }
            //}
        }

        public override void ProcessorInput(int input) {
            if (null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(input);
            }
        }

    }
}
