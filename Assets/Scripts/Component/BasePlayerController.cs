using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IPlayerInputReceiver {
        void SendOperator(int operate);
    }

    public class BasePlayerController : MonoBehaviour {
        public InputKeyMapValue InputKeyToValue = null;

        protected IPlayerInputReceiver _playerInputReceiver = null;

        public virtual void Awake() {
            CheckInputKeyToValue();
            InitPlayerInputReceiver();
        }

        protected void CheckInputKeyToValue() {
            if (null == InputKeyToValue) {
                Debug.LogError("=====>Local LocalPlayerController Input Key To Value ");

                InputKeyToValue = Asset.Load<InputKeyMapValue>("Res/ScriptObject/LocalInputKeyMapValue");
                if (null == InputKeyToValue) {
                    Debug.LogError("=====>Local LocalPlayerController Input Key To Value is null");
                    return;
                }
            }
        }

        protected virtual void InitPlayerInputReceiver() {

        }

        public virtual void Update() {
            int operat = 0;
            for (int i = 0; i < InputKeyToValue.InputKeys.Count; i++) {
                KeyCode keyCode = InputKeyToValue.InputKeys[i];
                if (Input.GetKey(keyCode)) {
                    operat |= (int)InputKeyToValue.InputValues[i];
                }
            }

            // 有操作的情况发送操作
            if (0 != operat && null != _playerInputReceiver) {
                _playerInputReceiver.SendOperator(operat);
            }
        }
    }
}