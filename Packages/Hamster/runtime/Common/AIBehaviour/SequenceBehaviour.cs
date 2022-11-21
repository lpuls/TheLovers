using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class SequenceBehaviour : BaseBehaviour {
        private int _executeIndex = 0;
        private List<BaseBehaviour> _behaviours = new();
        public string BBKey = string.Empty;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            for (int i = 0; i < _behaviours.Count; i++) {
                _behaviours[i].Initialize(behaviour);
            }
            behaviour.GetBlackboard().SetValue<int>(BBKey, 0);
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<int>(BBKey, out int ExecuteIndex))
                return EBehavourExecuteResult.Error;

            for (int i = ExecuteIndex; i < _behaviours.Count; i++) {
                EBehavourExecuteResult result = _behaviours[i].Execute(behaviour, dt);
                if (EBehavourExecuteResult.Wait == result) {
                    behaviour.GetBlackboard().SetValue<int>(BBKey, ExecuteIndex);
                    return result;
                }
                else if (EBehavourExecuteResult.Stop == result) {
                    behaviour.GetBlackboard().SetValue<int>(BBKey, ExecuteIndex);
                    return result;
                }
                else if (EBehavourExecuteResult.Error == result) {
                    throw new System.Exception("AI Behaviour Execute Error");
                }
                ExecuteIndex = i;
            }
            base.Execute(behaviour, dt);
            return EBehavourExecuteResult.Done;
        }

        public SequenceBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            _behaviours.Add(baseBehaviour);
            return this;
        }

        public override void ResetBehaviour(IAIBehaviour behaviour) {
            for (int i = 0; i < _behaviours.Count; i++) {
                _behaviours[i].ResetBehaviour(behaviour);
            }
        }
    }
}
