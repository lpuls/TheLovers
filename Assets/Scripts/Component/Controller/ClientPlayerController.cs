using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ClientPlayerController : PlayerController, IMover {

        private GameLogicSyncModule _gameLogicSyncModule = null;
        private MovementComponent _movementComponent = null;
        private SimulateComponent _simulateComponent = null;
        private PropertyComponent _propertyComponent = null;
        private PlayerEffectComponent _playerEffectComponent = null;

        private InputCommand _inputCommand = new InputCommand();

        public override void Init() {
            base.Init();

            if (null == _movementComponent) {
                _movementComponent = gameObject.TryGetOrAdd<MovementComponent>();
                _movementComponent.Mover = this;
            }
            if (null == _simulateComponent)
                _simulateComponent = gameObject.TryGetOrAdd<SimulateComponent>();
            if (null == _propertyComponent)
                _propertyComponent = gameObject.TryGetOrAdd<PropertyComponent>();
            if (null == _playerEffectComponent)
                _playerEffectComponent = gameObject.TryGetOrAdd<PlayerEffectComponent>();

            _propertyComponent.InitProperty(_netSyncComponent.ConfigID);
            _movementComponent.Speed = _propertyComponent.GetSpeed();

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

        protected override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return GameLogicUtility.ReadKeyboardInput(inputKeyMapValue);
        }

        private int GetFrameIndex() {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            return frameDataManager.GameLogicFrame;
        }

        protected override void ProcessorInput(int input) {
            // 客户端预表现
            // GameLogicUtility.GetOperateFromInput(transform, input, out Vector3 moveDirection, out bool cast1);
            GameLogicUtility.GetOperateFromInput(transform, input, _inputCommand);
            if (!_inputCommand.Direction.Equals(Vector3.zero))
                _movementComponent.Move(_inputCommand.Direction);
            else
                _movementComponent.Stop();

            // 将记录帧及操作发给服务端
            if (null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(input, GetFrameIndex());
            }
            else {
                GameLogicUtility.SetPlayerOperator(_netSyncComponent.NetID, input, GetFrameIndex());
            }
        }

        public virtual RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            return Physics2D.BoxCast(transform.position, GetSize(), 0, direction, distance,
                (1 << (int)ESpaceWarLayers.ENEMY) | (1 << (int)ESpaceWarLayers.BULLET) | (1 << (int)ESpaceWarLayers.PICKER));
        }

        public virtual Vector3 GetSize() {
            // todo 这边先偷懒吧，毕竟只有玩家飞机会预测
            return new Vector3(3.612638f, 1.211238f);
        }

        public bool OnHitSomething(RaycastHit2D raycastHit) {
            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, (ESpaceWarLayers)gameObject.layer);
            return true;
        }

        public override void Tick(float dt) {
            int input = GetOperator(InputKeyToValue);
            ProcessorInput(input);

            // 逻辑执行移动操作
            int frameIndex = GetFrameIndex();
            if (_movementComponent.NeedMove) {
                Vector3 preLocation = _simulateComponent.CurrentLocation;
                Vector3 currentLocation = _movementComponent.MoveTick(_simulateComponent.CurrentLocation, dt, frameIndex);
                _simulateComponent.UpdatePosition(preLocation, currentLocation);
                // Debug.Log(string.Format("ClientPlayerController {0} {1} {2} {3} {4} {5}", gameObject.name, preLocation, currentLocation, transform.position, input, frameIndex));

                // 将每一帧的預測的结果及当时的客户端帧号都记录下来
                _simulateComponent.AddPredictionCommand(frameIndex, currentLocation, input);
            }

            // 提前播放闪避动画
            if (_inputCommand.IsDodge && !_playerEffectComponent.IsDodging) {
                _playerEffectComponent.PlayDodge();
            }

            _inputCommand.Reset();
        }

    }
}
