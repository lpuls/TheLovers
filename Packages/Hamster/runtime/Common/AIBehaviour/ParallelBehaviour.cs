using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class ParallelBehaviour : BaseBehaviour {
        public List<BaseBehaviour> Behaviours = new();
        public string BBKey = string.Empty;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);

            List<EBehavourExecuteResult> results = new();
            behaviour.GetBlackboard().SetValue<List<EBehavourExecuteResult>>(BBKey, results);

            for (int i = 0; i < Behaviours.Count; i++) {
                results.Add(EBehavourExecuteResult.Wait);
                Behaviours[i].Initialize(behaviour);
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<List<EBehavourExecuteResult>>(BBKey, out List<EBehavourExecuteResult> array))
                Debug.LogError("Can't Find Execute Result " + BBKey);

            bool isDone = false;
            for (int i = 0; i < Behaviours.Count; i++) {
                if (EBehavourExecuteResult.Done != array[i]) {
                    EBehavourExecuteResult result = Behaviours[i].Execute(behaviour, dt);
                    array[i] = result;
                    isDone = isDone && EBehavourExecuteResult.Done == result;
                }
            }
            return isDone ? EBehavourExecuteResult.Done : EBehavourExecuteResult.Wait;
        }

        public override void ResetBehaviour(IAIBehaviour behaviour) {
            if (!behaviour.GetBlackboard().TryGetValue<List<EBehavourExecuteResult>>(BBKey, out List<EBehavourExecuteResult> array))
                Debug.LogError("Can't Find Execute Result " + BBKey);
            for (int i = 0; i < Behaviours.Count; i++) {
                array[i] = EBehavourExecuteResult.Wait;
                Behaviours[i].ResetBehaviour(behaviour);
            }
        }

        public ParallelBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            Behaviours.Add(baseBehaviour);
            return this;
        }
    }
}
