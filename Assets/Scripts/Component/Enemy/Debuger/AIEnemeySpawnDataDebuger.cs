using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

#if UNITY_EDITOR
    public class AIEnemeySpawnDataDebuger : BaseEnemeySpawnDataDebuger {
        public List<int> SpawnIDs = new List<int>();

        public int AIEnemeyMinSpawnCount = 1;
        public int AIEnemeyMaxSpawnCount = 5;

        public override EnemySpawnData GetEnemySpawnData() {
            AIEnemySpawnData enemySpawnData = new AIEnemySpawnData();
            enemySpawnData.RandomMinCount = AIEnemeyMinSpawnCount;
            enemySpawnData.RandomMaxCount = AIEnemeyMaxSpawnCount;
            enemySpawnData.SpawnIDs = SpawnIDs;
            enemySpawnData.TriggerZ = transform.position.z;
            return enemySpawnData;
        }
    }
#endif
}