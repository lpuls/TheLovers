using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class ParallelBehaviour : BaseBehaviour {
        private List<BaseBehaviour> _behaviours = new();
        private List<EBehavourExecuteResult> _behaviourState = new();

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            for (int i = 0; i < _behaviours.Count; i++) {
                _behaviourState.Add(EBehavourExecuteResult.Wait);
                _behaviours[i].Initialize(behaviour);
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            bool isDone = false;
            for (int i = 0; i < _behaviours.Count; i++) {
                if (EBehavourExecuteResult.Done != _behaviourState[i]) {
                    EBehavourExecuteResult result = _behaviours[i].Execute(behaviour, dt);
                    _behaviourState[i] = result;
                    isDone = isDone && EBehavourExecuteResult.Done == result;
                }
            }
            return isDone ? EBehavourExecuteResult.Done : EBehavourExecuteResult.Wait;
        }

        public override void ResetBehaviour(IAIBehaviour behaviour) {
            for (int i = 0; i < _behaviours.Count; i++) {
                _behaviourState[i] = EBehavourExecuteResult.Wait;
                _behaviours[i].ResetBehaviour(behaviour);
            }
        }

        public ParallelBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            _behaviours.Add(baseBehaviour);
            return this;
        }
    }
}
