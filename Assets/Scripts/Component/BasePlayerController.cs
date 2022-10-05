using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public class BasePlayerController : MonoBehaviour, IServerTicker {
        
        public InputKeyMapValue InputKeyToValue = null;
        protected SimulateComponent _simulateComponent = null;

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
            World.GetWorld<BaseSpaceWarWorld>().AddTicker(this);
            _simulateComponent = GetComponent<SimulateComponent>();
        }

        public virtual void OnDestroy() {
            World.GetWorld<BaseSpaceWarWorld>().RemoveTicker(this);
        }

        public virtual int GetOperator(InputKeyMapValue inputKeyMapValue) {
            return 0;
        }

        public virtual void ProcessorInput(int input) {
        }

        public virtual void Tick(float dt) {
            int input = GetOperator(InputKeyToValue);
            ProcessorInput(input);
        }
    }
}