﻿using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EInputValue {
        None,
        MoveUp = 1,
        MoveDown = 2,
        MoveRight = 3,
        MoveLeft = 4,
        Max
    }

    [CreateAssetMenu(menuName = "ScriptObject/InputKeyMapValue")]
    public class InputKeyMapValue : ScriptableObject {
        public List<KeyCode> InputKeys = new List<KeyCode>();
        public List<EInputValue> InputValues = new List<EInputValue>();
    }
}
