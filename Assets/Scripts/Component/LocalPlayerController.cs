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

        protected void GetOperateFromInput(int operate, out Vector3 direction, out bool castAbility1) {
            direction = Vector3.zero;
            castAbility1 = false;
            for (int i = 0; i < (int)EInputValue.Max; i++) {
                EInputValue value = (EInputValue)i;
                if (1 == ((operate >> i) & 1)) {
                    switch (value) {
                        case EInputValue.MoveUp:
                            direction += transform.forward;
                            break;
                        case EInputValue.MoveDown:
                            direction -= transform.forward;
                            break;
                        case EInputValue.MoveLeft:
                            direction -= transform.right;
                            break;
                        case EInputValue.MoveRight:
                            direction += transform.right;
                            break;
                        case EInputValue.Ability1:
                            castAbility1 = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void ProcessorInput(int operate) {
            GetOperateFromInput(operate, out Vector3 moveDirection, out bool cast1);

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

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
        }
    }
}
