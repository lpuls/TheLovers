using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IDamage {
        void OnHit(GameObject hitObject, GameObject hitTrajectory);
    }

    public class LocalPlayerController : BasePlayerController, IDamage {

        private bool _readByInputDevice = true;
        protected int _operator = 0;

        protected LocalAbilityComponent _localAbilityComponent = null;
        protected LocalMovementComponent _localMovementComponent = null;

        public override void ProcessorInput(int operate) {
            GameLogicUtility.GetOperateFromInput(transform, operate, out Vector3 moveDirection, out bool cast1);

            if (!moveDirection.Equals(Vector3.zero))
                _localMovementComponent.Move(moveDirection);
            else
                _localMovementComponent.Stop();
        }

        public override void Init() {
            base.Init();
            _localMovementComponent = GetComponent<LocalMovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        }

        public void SetIsReadByInputDevice(bool readByInputDevice) {
            _readByInputDevice = readByInputDevice;
        }

        public void SetOperator(int input) {
            _operator = input;
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

            // 逻辑执行移动操作
            if (_localMovementComponent.NeedMove) {
                PreLocation = CurrentLocation;
                CurrentLocation = _localMovementComponent.MoveTick(CurrentLocation, dt);
                _simulateTime = 0;
                GameLogicUtility.SetPositionDirty(gameObject);
            }
        }

    }
}
