using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EInputValue {
        None,
        MoveUp = 1,
        MoveDown = 2,
        MoveRight = 3,
        MoveLeft = 4,
        Ability1 = 5,  // 普通开火
        Ability2 = 6,  // 技能
        Dodge = 8,     // 闪避
        Max
    }

    [CreateAssetMenu(menuName = "ScriptObject/InputKeyMapValue")]
    public class InputKeyMapValue : ScriptableObject {
        public List<KeyCode> InputKeys = new List<KeyCode>();
        public List<EInputValue> InputValues = new List<EInputValue>();
    }
}
