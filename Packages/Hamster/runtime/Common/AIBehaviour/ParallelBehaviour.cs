using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class ParallelBehaviour : BaseBehaviour {
        public List<BaseBehaviour> Behaviours = new();
        private Dictionary<GameObject, List<EBehavourExecuteResult>> _resuls = new();
        public string BBKey = string.Empty;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);

            if (!_resuls.TryGetValue(behaviour.GetOwner(), out List<EBehavourExecuteResult> results)) {
                results = new List<EBehavourExecuteResult>();
                _resuls[behaviour.GetOwner()] = results;
            }

            results.Clear();
            for (int i = 0; i < Behaviours.Count; i++) {
                results.Add(EBehavourExecuteResult.Wait);
                Behaviours[i].Initialize(behaviour);
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!_resuls.TryGetValue(behaviour.GetOwner(), out List<EBehavourExecuteResult> results)) {
                return EBehavourExecuteResult.Error;
            }

            bool isDone = false;
            for (int i = 0; i < Behaviours.Count; i++) {
                if (EBehavourExecuteResult.Done != results[i]) {
                    EBehavourExecuteResult result = Behaviours[i].Execute(behaviour, dt);
                    results[i] = result;
                    isDone = isDone && EBehavourExecuteResult.Done == result;
                }
            }
            return isDone ? EBehavourExecuteResult.Done : EBehavourExecuteResult.Wait;
        }

        public override void ResetBehaviour(IAIBehaviour behaviour) {
            if (!_resuls.TryGetValue(behaviour.GetOwner(), out List<EBehavourExecuteResult> results)) {
                return;
            }
            for (int i = 0; i < Behaviours.Count; i++) {
                results[i] = EBehavourExecuteResult.Wait;
                Behaviours[i].ResetBehaviour(behaviour);
            }
        }

        public ParallelBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            Behaviours.Add(baseBehaviour);
            return this;
        }
    }
}
