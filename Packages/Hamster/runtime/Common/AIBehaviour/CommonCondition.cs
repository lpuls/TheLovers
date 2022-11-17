using System;
using UnityEngine;

namespace Hamster {
    public enum ECompare {
        Big,
        Less,
        BigEqu,
        LessEqu,
        Equ
    }

    [SerializeField]
    public class Int32CompareCondition : SelectBehaviour.ISelectCondition {

        public ECompare CompareType = ECompare.Equ;
        public string BlackboardKey = string.Empty;
        public int CompareValue = 0;

        public int Select(GameObject gameObject, Blackboard blackboard) {
            if (blackboard.TryGetValue<int>(BlackboardKey, out int value)) {
                switch (CompareType) {
                    case ECompare.Big:
                        return value > CompareValue ? 0 : 1;
                    case ECompare.Less:
                        return value < CompareValue ? 0 : 1;
                    case ECompare.BigEqu:
                        return value >= CompareValue ? 0 : 1;
                    case ECompare.LessEqu:
                        return value <= CompareValue ? 0 : 1;
                    case ECompare.Equ:
                        return value == CompareValue ? 0 : 1;
                }
            }
            return -1;
        }
    }

}
