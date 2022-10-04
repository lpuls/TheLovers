using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetPlayerCommand : IPool {
        public int FrameIndex = 0;
        public Vector3 Location = Vector3.zero;
        public int operate = 0;

        public void Reset() {
            FrameIndex = 0;
            Location = Vector3.zero;
            operate = 0;
        }
    }

    public class NetPlayerController : LocalPlayerController {

        private GameLogicSyncModule _gameLogicSyncModule = null;
        private List<NetPlayerCommand> _predicationCommands = new List<NetPlayerCommand>();

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
            NetPlayerCommand command = ObjectPool<NetPlayerCommand>.Malloc();
            command.FrameIndex = frameIndex;
            command.Location = transform.position;
            command.operate = input;
            _predicationCommands.Add(command);

            // 将记录帧及操作发给服务端
            if (null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(input, frameIndex);
            }
        }

        public bool TryGetTopPredictionCommand(out NetPlayerCommand command) {
            command = null;
            if (_predicationCommands.Count > 0) {
                command = _predicationCommands[0];
                return true;
            }
            return false;
        }

        public void RemoveTopPredictionCommand() {
            NetPlayerCommand command = _predicationCommands[0];
            _predicationCommands.RemoveAt(0);
            ObjectPool<NetPlayerCommand>.Free(command);
        }

        public void CleanPredicationLocations() {
            _predicationCommands.Clear();
        }

        public void SimulateAfter() {
            foreach (var item in _predicationCommands) {
                GetOperateFromInput(item.operate, out Vector3 moveDirection, out bool _);
                _localMovementComponent.MoveTick(moveDirection);
                item.Location = transform.position;
            }
        }

    }
}
