using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    [SerializeField]
    public class LevelWaveScriptObject : LevelEventScriptObject {

        // public ELevelEventCompleteType CompleteType = ELevelEventCompleteType.WaitTime;
        public List<UnitSpawnScriptObject> UnitSpawns = new();  // 敌人生成数据

        //public override bool IsComplete(ILevelManager levelManager) {
        //    if (ELevelEventCompleteType.WaitTime == CompleteType) {
        //        return levelManager.GetTime() >= Time;
        //    }
        //    else if (ELevelEventCompleteType.WaitAllDie == CompleteType) {
        //        return levelManager.GetEnemeyCount() <= 0 && levelManager.GetPendingSpawnUnitCount() <= 0;
        //    }
        //    return false;
        //}

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
        private int CompareUnitSpawnByDelay(UnitSpawnScriptObject x, UnitSpawnScriptObject y) {
            if (x.Delay > y.Delay)
                return 1;
            else if (x.Delay < y.Delay)
                return -1;
            else
                return 0;
        }

        public override void Save(ScriptableObject parent) {
            base.Save(parent);

            UnitSpawns.Sort(CompareUnitSpawnByDelay);
            foreach (var item in UnitSpawns) {
                item.Save(this);
            }

        }
#endif 
    }
}
