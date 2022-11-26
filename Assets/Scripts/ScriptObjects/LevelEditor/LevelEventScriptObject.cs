using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelWaveCompleteType {
        WaitTime,
        WaitAllDie,
    }

    public interface ILevelManager {
        float GetTime();
        int GetEnemeyCount();
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

    [SerializeField]
    public class LevelWaveScriptObject : LevelEventScriptObject {

        public ELevelWaveCompleteType CompleteType = ELevelWaveCompleteType.WaitTime;
        public List<UnitSpawnScriptObject> UnitSpawns = new();  // 敌人生成数据

        public override bool IsComplete(ILevelManager levelManager) {
            if (ELevelWaveCompleteType.WaitTime == CompleteType) {
                return levelManager.GetTime() >= Time;
            }
            else if (ELevelWaveCompleteType.WaitAllDie == CompleteType) {
                return levelManager.GetEnemeyCount() <= 0;
            }
            return false;
        }

        public override void OnLevel(ILevelManager levelManager) {
            base.OnLevel(levelManager);
        }

        public override void OnEnter(ILevelManager levelManager) {
            if (levelManager.IsClient())
                return;

            foreach (var item in UnitSpawns) {
                levelManager.SpawnUnit(item);
            }
        }

#if UNITY_EDITOR
        public override void Save(ScriptableObject parent) {
            base.Save(parent);

            foreach (var item in UnitSpawns) {
                item.Save(this);
            }

        }
#endif 
    }

    [SerializeField]
    public class LevelMissionUIScriptObject : LevelEventScriptObject {
        public int MissionID = 0;

        public override void OnEnter(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            Single<UIManager>.GetInstance().Open<MissonUIController>();
            MissionUIModule missionUIModule = Single<UIManager>.GetInstance().GetModule<MissonUIController>() as MissionUIModule;
            missionUIModule.MissionID.SetValue(MissionID);
        }

        public override void OnLevel(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            Single<UIManager>.GetInstance().Close<MissonUIController>();
        }
    }

    public enum ELevelEventUI {
        Warning
    }

    [SerializeField]
    public class LevelUIScriptObject : LevelEventScriptObject {
        public ELevelEventUI UIType = 0;

        public override void OnEnter(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (UIType) {
                case ELevelEventUI.Warning: {
                        Single<UIManager>.GetInstance().Open<WarningUIController>();
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnLevel(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (UIType) {
                case ELevelEventUI.Warning: {
                        Single<UIManager>.GetInstance().Close<WarningUIController>();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
