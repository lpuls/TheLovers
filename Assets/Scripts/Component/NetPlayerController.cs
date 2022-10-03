using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetPlayerController : LocalPlayerController {

        private GameLogicSyncModule _gameLogicSyncModule = null;
        private Dictionary<int, Vector3> _predictionLocations = new Dictionary<int, Vector3>(new Int32Comparer());

        public override void Init() {
            _localMovementComponent = gameObject.TryGetOrAdd<LocalMovementComponent>();
            // _localAbilityComponent = gameObject.TryGetOrAdd<LocalAbilityComponent>();

            // 主控端需要通过该网络模块转发数据
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
        }

        public override void ProcessorInput(int input) {
            base.ProcessorInput(input);

            // 将每一帧的預測的结果及当时的客户端帧号都记录下来
            ClientSpaceWarWorld world = World.GetWorld<ClientSpaceWarWorld>();
            int frameIndex = world.GetFrameIndex();
            _predictionLocations.TryAdd(frameIndex, transform.position);

            // 将记录帧及操作发给服务端
            if (null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(input, frameIndex);
            }
        }

        public bool TryGetPredictionLocation(int frameIndex, out Vector3 location) {
            return _predictionLocations.TryGetValue(frameIndex, out location);
        }

        public void RemovePredictionLocation(int frameIndex) {
            _predictionLocations.Remove(frameIndex);
        }

        public void CleanPredicationLocations() {
            _predictionLocations.Clear();
        }

    }
}
