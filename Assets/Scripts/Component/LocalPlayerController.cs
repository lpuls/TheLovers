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
            Vector3 moveDirection = Vector3.zero;
            for (int i = 0; i < (int)EInputValue.Max; i++) {
                EInputValue value = (EInputValue)i;
                if (1 == ((operate >> i) & 1)) {
                    switch (value) {
                        case EInputValue.MoveUp:
                            moveDirection += transform.forward;
                            break;
                        case EInputValue.MoveDown:
                            moveDirection -= transform.forward;
                            break;
                        case EInputValue.MoveLeft:
                            moveDirection -= transform.right;
                            break;
                        case EInputValue.MoveRight:
                            moveDirection += transform.right;
                            break;
                        case EInputValue.Ability1:
                            _localAbilityComponent.CastAbility((int)EAbilityIndex.Fire);
                            break;
                        case EInputValue.Ability2:
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!moveDirection.Equals(Vector3.zero))
                _localMovementComponent.Move(moveDirection);
            else
                _localMovementComponent.Stop();
        }

        public override void Init() {
            _localMovementComponent = GetComponent<LocalMovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        }

        public void SetIsReadByInputDevice(bool readByInputDevice) {
            _readByInputDevice = readByInputDevice;
        }

        public void SetOperator(int input) {
            if (!_readByInputDevice)
                _operator = input;
        }
        
        public override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            if (_readByInputDevice) {
                _operator = 0;
                for (int i = 0; i < InputKeyToValue.InputKeys.Count; i++) {
                    KeyCode keyCode = InputKeyToValue.InputKeys[i];
                    if (Input.GetKey(keyCode)) {
                        _operator |= (1 << (int)InputKeyToValue.InputValues[i]);
                    }
                }
            }
            return _operator;
        }

        public override void Update() {
            base.Update();
            _operator = 0;
        }

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
        }
    }
}
