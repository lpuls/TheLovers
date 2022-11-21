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
    public class Int32CompareCondition : ScriptableObject, SelectBehaviour.ISelectCondition {

        public ECompare CompareType = ECompare.Equ;
        public string BlackboardKey = string.Empty;
        public int CompareValue = 0;

        public bool Select(GameObject gameObject, Blackboard blackboard) {
            if (blackboard.TryGetValue<int>(BlackboardKey, out int value)) {
                switch (CompareType) {
                    case ECompare.Big:
                        return value > CompareValue;
                    case ECompare.Less:
                        return value < CompareValue;
                    case ECompare.BigEqu:
                        return value >= CompareValue;
                    case ECompare.LessEqu:
                        return value <= CompareValue;
                    case ECompare.Equ:
                        return value == CompareValue;
                }
            }
            return false;
        }
    }

}
