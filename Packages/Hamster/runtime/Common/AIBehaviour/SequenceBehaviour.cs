using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    [SerializeField]
    public class SequenceBehaviour : BaseBehaviour {
        private int _executeIndex = 0;
        private List<BaseBehaviour> _behaviours = new();

        public override void Initialize(GameObject gameObject, IAIBehaviour behaviour, Blackboard blackboard) {
            base.Initialize(gameObject, behaviour, blackboard);
            for (int i = 0; i < _behaviours.Count; i++) {
                _behaviours[i].Initialize(gameObject, behaviour, blackboard);
            }
            _executeIndex = 0;
        }

        public override EBehavourExecuteResult Execute(float dt) {
            for (int i = _executeIndex; i < _behaviours.Count; i++) {
                EBehavourExecuteResult result = _behaviours[i].Execute(dt);
                if (EBehavourExecuteResult.Wait == result) {
                    return result;
                }
                else if (EBehavourExecuteResult.Stop == result) {
                    _executeIndex = 0;
                    return result;
                }
                else if (EBehavourExecuteResult.Error == result) {
                    throw new System.Exception("AI Behaviour Execute Error");
                }
                _executeIndex = i;
            }
            base.Execute(dt);
            return EBehavourExecuteResult.Done;
        }

        public SequenceBehaviour AddBehaviour(BaseBehaviour baseBehaviour) {
            _behaviours.Add(baseBehaviour);
            return this;
        }

        public override void ResetBehaviour() {
            for (int i = _executeIndex; i < _behaviours.Count; i++) {
                _behaviours[i].ResetBehaviour();
            }
        }
    }
}
