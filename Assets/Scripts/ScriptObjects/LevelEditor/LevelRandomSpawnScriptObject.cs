using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    [SerializeField]
    public class LevelRandomSpawnScriptObject : LevelEventScriptObject {
        public ELevelEventCompleteType CompleteType = ELevelEventCompleteType.WaitAllDie; 
        public int RandomSpawnCountMin = 3;
        public int RandomSpawnCountMax = 10;
        public string AIAssetPath = string.Empty;
        public Vector2 RandomSpawnDelay = new Vector2(0.00f, 0.03f);
        public List<Vector3> SpawnLocations = new List<Vector3>();
        public List<int> SpawnIDs = new List<int>();
        public List<UnitSpawnScriptObject> UnitSpawns = new List<UnitSpawnScriptObject>();

        public override bool IsComplete(ILevelManager levelManager) {
            if (ELevelEventCompleteType.WaitTime == CompleteType) {
                return levelManager.GetTime() >= Time;
            }
            else if (ELevelEventCompleteType.WaitAllDie == CompleteType) {
                return levelManager.GetEnemeyCount() <= 0 && levelManager.GetPendingSpawnUnitCount() <= 0;
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

            float delayTime = 0;
            int spawnCount = Random.Range(RandomSpawnCountMin, RandomSpawnCountMax);
            for (int i = 0; i < spawnCount; i++) {
                UnitSpawnScriptObject scriptObject = UnitSpawnScriptObject.CreateInstance<UnitSpawnScriptObject>();
                scriptObject.ID = SpawnIDs[Random.Range(0, SpawnIDs.Count)];
                scriptObject.Delay = delayTime;
                scriptObject.AIAssetPath = AIAssetPath;
                scriptObject.SpawnLocation = SpawnLocations[Random.Range(0, SpawnLocations.Count)];
                scriptObject.name = string.Format("Random_{0}_{1}_{2}", scriptObject.ID, scriptObject.SpawnLocation, scriptObject.Delay);
                scriptObject.Save(this);
                UnitSpawns.Add(scriptObject);
                delayTime += Random.Range(RandomSpawnDelay.x, RandomSpawnDelay.y);
            }
        }
#endif
    }
}
