using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class WaitBehaviour : BaseBehaviour {

        public const string WAIT_TICK_TIME_ID = "WaitTick";

        public float WaitTime {
            private get; set;
        }

        public WaitBehaviour() {
            WaitTime = 0;
        }

        public WaitBehaviour(float waitTime) {
            WaitTime = waitTime;
        }

        public override void Initialize(GameObject gameObject, IAIBehaviour behaviour, Blackboard blackboard) {
            base.Initialize(gameObject, behaviour, blackboard);
            blackboard.SetValue<float>(WAIT_TICK_TIME_ID, 0);
        }

        public override EBehavourExecuteResult Execute(float dt) {
            if (Blackboard.TryGetValue<float>(WAIT_TICK_TIME_ID, out float value)) {
                value += dt;
                Blackboard.SetValue<float>(WAIT_TICK_TIME_ID, value);
                if (value >= WaitTime) {
                    return EBehavourExecuteResult.Done;
                }
                return EBehavourExecuteResult.Wait;
            }
            else {
                return EBehavourExecuteResult.Error;
            }
        }

        public override void ResetBehaviour() {
            Blackboard.SetValue<float>(WAIT_TICK_TIME_ID, 0);
        }
    }

    [SerializeField]
    public class DebugBehaviour : BaseBehaviour {
        public string DebugInfo {
            private get; set;
        }

        public DebugBehaviour() {
            DebugInfo = string.Empty;
        }

        public DebugBehaviour(string debugInfo) {
            DebugInfo = debugInfo;
        }

        public override EBehavourExecuteResult Execute(float dt) {
            Debug.Log(string.Format("{0}: {1}", Owner.name, DebugInfo));
            return EBehavourExecuteResult.Done;
        }
    }

    [SerializeField]
    public class StopAndResetBehaviour : BaseBehaviour {
        public override EBehavourExecuteResult Execute(float dt) {
            AIBehaviour.Stop();
            AIBehaviour.Reset();
            return EBehavourExecuteResult.Done;
        }
    }

    public class ResetBehaviour : BaseBehaviour {
        public override EBehavourExecuteResult Execute(float dt) {
            AIBehaviour.Reset();
            return EBehavourExecuteResult.Done;
        }
    }
}
