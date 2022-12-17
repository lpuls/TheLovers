using UnityEngine;

namespace Hamster.SpaceWar {
    public class PlayerController : BaseController {
        public InputKeyMapValue InputKeyToValue = null;
        
        public override void Init() {
            base.Init();
            CheckInputKeyToValue();
        }

        protected void CheckInputKeyToValue() {
            if (null == InputKeyToValue && !_netSyncComponent.IsSimulatedProxy()) {
                // Debug.LogError("=====>Local LocalPlayerController Input Key To Value ");

                InputKeyToValue = Asset.Load<InputKeyMapValue>("Res/ScriptObjects/KeyMap/LocalInputKeyMapValue");
            }
        }

        protected virtual void ProcessorInput(int input) {
        }

        protected virtual int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return 0;
        }


        public override void Tick(float dt) {
            int input = GetOperator(InputKeyToValue);
            ProcessorInput(input);
        }

    }
}
