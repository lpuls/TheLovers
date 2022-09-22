using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public class BasePlayerController : MonoBehaviour {
        public InputKeyMapValue InputKeyToValue = null;


        public virtual void Awake() {
            CheckInputKeyToValue();
            Init();
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

        public virtual void Init() {
        }

        public virtual int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return 0;
        }

        public virtual void ProcessorInput(int input) {
        }

        public virtual void Update() {
            int input = GetOperator(InputKeyToValue);
            ProcessorInput(input);
        }
    }
}