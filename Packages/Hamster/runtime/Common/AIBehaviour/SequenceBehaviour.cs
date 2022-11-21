using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class SequenceBehaviour : BaseBehaviour {
        public List<BaseBehaviour> Behaviours = new();
        public string BBKey = string.Empty;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            for (int i = 0; i < Behaviours.Count; i++) {
                Behaviours[i].Initialize(behaviour);
            }
            behaviour.GetBlackboard().SetValue<int>(BBKey, 0);
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<int>(BBKey, out int ExecuteIndex))
                return EBehavourExecuteResult.Error;

            for (int i = ExecuteIndex; i < Behaviours.Count; i++) {
                EBehavourExecuteResult result = Behaviours[i].Execute(behaviour, dt);
                if (EBehavourExecuteResult.Wait == result) {
                    behaviour.GetBlackboard().SetValue<int>(BBKey, i);
                    return result;
                }
                else if (EBehavourExecuteResult.Stop == result) {
                    behaviour.GetBlackboard().SetValue<int>(BBKey, i);
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
            Behaviours.Add(baseBehaviour);
            return this;
        }

        public override void ResetBehaviour(IAIBehaviour behaviour) {
            for (int i = 0; i < Behaviours.Count; i++) {
                Behaviours[i].ResetBehaviour(behaviour);
            }
        }
    }
}
