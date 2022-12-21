﻿using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class UnitSpawnScriptObject : ScriptableObject {
        public int ID = 0;
        public float Delay = 0;
        public int LocationIndex = 0;
        public string AIAssetPath = string.Empty;
        public List<Vector3> Path = new List<Vector3>();



#if UNITY_EDITOR
        public void Save(ScriptableObject parent) {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, parent);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif 
    }
}