using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelWaveCompleteType {
        WaitTime,
        WaitAllDie,
        Continue
    }

    public interface ILevelManager {
        float GetTime();
        int GetEnemeyCount();
        int GetPendingSpawnUnitCount();
        void SpawnUnit(UnitSpawnScriptObject data);
        void DestroyAllUnit();
        bool IsClient();

    }

    [SerializeField]
    public class LevelEventScriptObject : ScriptableObject {
        public float Time = 0;

        public virtual bool IsComplete(ILevelManager levelManager) {
            return levelManager.GetTime() >= Time;
        }

        public virtual void OnLevel(ILevelManager levelManager) {
        }

        public virtual void OnEnter(ILevelManager levelManager) {
        }

#if UNITY_EDITOR
        public virtual void Save(ScriptableObject parent) {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, parent);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }

}
