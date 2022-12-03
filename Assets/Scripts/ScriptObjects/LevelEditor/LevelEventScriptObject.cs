using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelEventCompleteType {
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
        public ELevelEventCompleteType CompleteType = ELevelEventCompleteType.Continue;

        public virtual bool IsComplete(ILevelManager levelManager) {
            if (ELevelEventCompleteType.WaitTime == CompleteType) {
                return levelManager.GetTime() >= Time;
            }
            else if (ELevelEventCompleteType.WaitAllDie == CompleteType) {
                return levelManager.GetEnemeyCount() <= 0 && levelManager.GetPendingSpawnUnitCount() <= 0;
            }
            else if (ELevelEventCompleteType.Continue == CompleteType) {
                return true;
            }
            return false;
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
