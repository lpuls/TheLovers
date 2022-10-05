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

    public class NetPlayerController : BasePlayerController {

        private GameLogicSyncModule _gameLogicSyncModule = null;
        private LocalMovementComponent _localMovementComponent = null;
        private NetSyncComponent _netSyncComponent = null;

        private int _predictionIndex = 0;
        private Dictionary<int, NetPlayerCommand> _predicationCommands = new Dictionary<int, NetPlayerCommand>(new Int32Comparer());

        public override void Init() {
            base.Init();

            PreLocation = transform.position;
            CurrentLocation = transform.position;

            _netSyncComponent = gameObject.GetComponent<NetSyncComponent>();
            _localMovementComponent = gameObject.TryGetOrAdd<LocalMovementComponent>();

            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>() as ClientFrameDataManager;
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate += OnFrameUpdate;
            }

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

        public override void OnDestroy() {
            base.OnDestroy();
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>() as ClientFrameDataManager;
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate -= OnFrameUpdate;
            }
        }

        private void OnFrameUpdate(FrameData pre, FrameData current) {
            int netID = _netSyncComponent.NetID;
            UpdateInfo preUpdateInfo = null;
            UpdateInfo currentUpdateInfo = null;
            if (null != pre && pre.TryGetUpdateInfo(netID, EUpdateActorType.Position, out preUpdateInfo)) {
                PreLocation = preUpdateInfo.Data1.Vec3;
            }
            if (null != current && current.TryGetUpdateInfo(netID, EUpdateActorType.Position, out currentUpdateInfo)) {
                CurrentLocation = currentUpdateInfo.Data1.Vec3;
                _predictionIndex = currentUpdateInfo.Data2.Int32;
                _simulateTime = 0;
            }
        }

        public override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return GameLogicUtility.ReadKeyboardInput(inputKeyMapValue);
        }

        public override void ProcessorInput(int input) {
            // 将每一帧的預測的结果及当时的客户端帧号都记录下来
            ClientSpaceWarWorld world = World.GetWorld<ClientSpaceWarWorld>();
            int frameIndex = world.GetFrameIndex();
            NetPlayerCommand command = ObjectPool<NetPlayerCommand>.Malloc();
            command.FrameIndex = frameIndex;
            command.Location = transform.position;
            command.operate = input;
            _predicationCommands.Add(frameIndex, command);

            // 将记录帧及操作发给服务端
            if (null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(input, frameIndex);
            }
        }

        public override void Tick(float dt) {
            base.Tick(dt);

            // 逻辑执行移动操作
            if (_localMovementComponent.NeedMove) {
                PreLocation = CurrentLocation;
                CurrentLocation = _localMovementComponent.MoveTick(CurrentLocation, dt);
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

        public void RemoveTopPredictionCommand(int frameIndex) {
            if (_predicationCommands.TryGetValue(frameIndex, out NetPlayerCommand command)) {
                _predicationCommands.Remove(frameIndex);
                ObjectPool<NetPlayerCommand>.Free(command);
            }
        }

        public void CleanPredicationLocations() {
            _predicationCommands.Clear();
        }

        public void SimulateAfter() {
            foreach (var item in _predicationCommands) {
                NetPlayerCommand command = item.Value;
                GameLogicUtility.GetOperateFromInput(transform, command.operate, out Vector3 moveDirection, out bool _);
                command.Location = transform.position;
            }
        }

        public override void Simulate() {
            if (null == _netSyncComponent)
                return;

            if (_netSyncComponent.IsAuthority()) {
                Debug.LogError("=========> Can't run Net Movement In Server");
                return;
            }

            ClientSpaceWarWorld spaceWarWorld = World.GetWorld<ClientSpaceWarWorld>();
            if (null == spaceWarWorld) {
                Debug.LogError("=========> clientSpaceWarWorld is null");
                return;
            }

            base.Simulate();
        }

    }
}
