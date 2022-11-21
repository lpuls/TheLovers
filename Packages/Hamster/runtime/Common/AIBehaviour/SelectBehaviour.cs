using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class SelectBehaviour : BaseBehaviour {
        public interface ISelectCondition {
            bool Select(GameObject gameObject, Blackboard blackboard);
        }

        public List<ISelectCondition> Conditions = new();
        public List<BaseBehaviour> Behaviours = new();

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            for (int i = 0; i < Behaviours.Count; i++) {
                Behaviours[i].Initialize(behaviour);
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            base.Execute(behaviour, dt);

            for (int i = 0; i < Conditions.Count; i++) {
                ISelectCondition condition = Conditions[i];
                if (condition.Select(behaviour.GetOwner(), behaviour.GetBlackboard())) {
                    return Behaviours[i].Execute(behaviour, dt);
                }
            }
            return EBehavourExecuteResult.Done;
        }

        public void AddSelectCondition(ISelectCondition selectCondition) {
            Conditions.Add(selectCondition);
        }

        public SelectBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
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
