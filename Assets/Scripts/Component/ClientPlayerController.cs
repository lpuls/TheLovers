using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ClientPlayerController : BasePlayerController {

        private GameLogicSyncModule _gameLogicSyncModule = null;
        private MovementComponent _movementComponent = null;
        private NetSyncComponent _netSyncComponent = null;

        public override void Init() {
            base.Init();

            _netSyncComponent = gameObject.GetComponent<NetSyncComponent>();
            _movementComponent = gameObject.TryGetOrAdd<MovementComponent>();

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
            UpdateInfo preUpdateInfo;
            UpdateInfo currentUpdateInfo;
            Vector3 preLocation = _simulateComponent.PreLocation;
            Vector3 currentLocation = _simulateComponent.CurrentLocation;
            if (null != pre && pre.TryGetUpdateInfo(netID, EUpdateActorType.Position, out preUpdateInfo)) {
                preLocation = preUpdateInfo.Data1.Vec3;
            }
            if (null != current && current.TryGetUpdateInfo(netID, EUpdateActorType.Position, out currentUpdateInfo)) {
                currentLocation = currentUpdateInfo.Data1.Vec3;
                _simulateComponent.UpdateSimulateInfo(preLocation, currentLocation, currentUpdateInfo.Data2.Int32);
            }
        }

        public override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return GameLogicUtility.ReadKeyboardInput(inputKeyMapValue);
        }

        public override void ProcessorInput(int input) {
            // 客户端预表现
            GameLogicUtility.GetOperateFromInput(transform, input, out Vector3 moveDirection, out bool cast1);
            if (!moveDirection.Equals(Vector3.zero))
                _movementComponent.Move(moveDirection);
            else
                _movementComponent.Stop();

            ClientSpaceWarWorld world = World.GetWorld<ClientSpaceWarWorld>();
            int frameIndex = world.GetFrameIndex();

            // 将记录帧及操作发给服务端
            if (null != _gameLogicSyncModule) {
                
                _gameLogicSyncModule.SendOperator(input, frameIndex);
            }
        }

        public override void Tick(float dt) {
            int input = GetOperator(InputKeyToValue);
            ProcessorInput(input);
            

            ClientSpaceWarWorld world = World.GetWorld<ClientSpaceWarWorld>();
            int frameIndex = world.GetFrameIndex();

            if (0 != input)
                Debug.Log(string.Format("=====>SetOperator {0} {1} ", input, frameIndex));

            // 逻辑执行移动操作
            if (_movementComponent.NeedMove) {
                Vector3 preLocation = _simulateComponent.CurrentLocation;
                Vector3 currentLocation = _movementComponent.MoveTick(_simulateComponent.CurrentLocation, dt, frameIndex);
                _simulateComponent.UpdateSimulateInfo(preLocation, currentLocation, -1);

                // 将每一帧的預測的结果及当时的客户端帧号都记录下来
                // Debug.Log(string.Format("=====>Index: {0}, Location: {1}, NeedMove: {2}, Input: {3}", frameIndex, currentLocation, _movementComponent.NeedMove, input));
                _simulateComponent.AddPredictionCommand(frameIndex, currentLocation, input);
            }

            
        }

    }
}
