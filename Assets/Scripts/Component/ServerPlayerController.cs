using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IDamage {
        void OnHit(GameObject hitObject, GameObject hitTrajectory);
    }

    public class ServerPlayerController : BasePlayerController, IDamage {

        private bool _readByInputDevice = true;
        protected int _operator = 0;
        protected int _operatorIndex = 0;

        protected LocalAbilityComponent _localAbilityComponent = null;
        protected MovementComponent _movementComponent = null;

        public override void ProcessorInput(int operate) {
            GameLogicUtility.GetOperateFromInput(transform, operate, out Vector3 moveDirection, out bool cast1);

            if (!moveDirection.Equals(Vector3.zero))
                _movementComponent.Move(moveDirection);
            else
                _movementComponent.Stop();
        }

        public override void Init() {
            base.Init();
            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        }

        public void SetIsReadByInputDevice(bool readByInputDevice) {
            _readByInputDevice = readByInputDevice;
        }

        public void SetOperator(int input, int index) {
            _operator = input;
            _operatorIndex = index;
            if (0 != input)
                Debug.Log(string.Format("=====>SetOperator {0} {1} ", input, index));
        }

        public override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            if (_readByInputDevice) {
                _operator = GameLogicUtility.ReadKeyboardInput(inputKeyMapValue);
            }
            return _operator;
        }

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
        }

        public override void Tick(float dt) {
            base.Tick(dt);
            _operator = 0;

            // 逻辑执行移动操作
            if (_movementComponent.NeedMove) {
                Vector3 preLocation = _simulateComponent.CurrentLocation;
                Vector3 currentLocation = _movementComponent.MoveTick(_simulateComponent.CurrentLocation, dt, _operatorIndex);
                _simulateComponent.UpdateSimulateInfo(preLocation, currentLocation, -1);
                GameLogicUtility.SetPositionDirty(gameObject);
            }
        }

    }
}
