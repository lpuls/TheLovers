using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class LevelPathScriptObject : ScriptableObject {
        public List<Vector3> Paths = new();

#if UNITY_EDITOR
        public void Save(ScriptableObject parent) {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, parent);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif 
    }
}
