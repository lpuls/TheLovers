using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class SelectBehaviour : BaseBehaviour {
        public interface ISelectCondition {
            int Select(GameObject gameObject, Blackboard blackboard);
        }

        public ISelectCondition Conditions = null;
        public List<BaseBehaviour> Behaviours = new();

        public override void Initialize(GameObject gameObject, IAIBehaviour behaviour, Blackboard blackboard) {
            base.Initialize(gameObject, behaviour, blackboard);
            for (int i = 0; i < Behaviours.Count; i++) {
                Behaviours[i].Initialize(gameObject, behaviour, blackboard);
            }
        }

        public override EBehavourExecuteResult Execute(float dt) {
            base.Execute(dt);
            int index = Conditions.Select(Owner, Blackboard);
            if (index < 0 || index > Behaviours.Count)
                return EBehavourExecuteResult.Error;
            return Behaviours[index].Execute(dt);
        }

        public void SetSelectCondition(ISelectCondition selectCondition) {
            Conditions = selectCondition;
        }

        public SelectBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            Behaviours.Add(baseBehaviour);
            return this;
        }

        public override void ResetBehaviour() {
            for (int i = 0; i < Behaviours.Count; i++) {
                Behaviours[i].ResetBehaviour();
            }
        }
    }
}
