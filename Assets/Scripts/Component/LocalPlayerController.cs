﻿using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {
    public class LocalPlayerController : BasePlayerController, IPlayerInputReceiver {

        private LocalMovementComponent _localMovementComponent = null;

        public override void Awake() {
            base.Awake();

            _localMovementComponent = GetComponent<LocalMovementComponent>();
        }

        public void SendOperator(int operate) {
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

        protected override void InitPlayerInputReceiver() {
            _playerInputReceiver = this;
        }

        public override void Update() {
            int operate = 0;
            for (int i = 0; i < InputKeyToValue.InputKeys.Count; i++) {
                KeyCode keyCode = InputKeyToValue.InputKeys[i];
                if (Input.GetKey(keyCode)) {
                    operate |= (1 << (int)InputKeyToValue.InputValues[i]);
                }
            }

            if (null != _playerInputReceiver) {
                _playerInputReceiver.SendOperator(operate);
            }
        }

    }
}
