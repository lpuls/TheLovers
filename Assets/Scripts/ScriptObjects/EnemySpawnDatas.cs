using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    [System.Serializable]
    public class EnemySpawnData {
        public float TriggerZ = 0;
        public List<int> SpawnIDs = new List<int>(16);
    }

    [System.Serializable]
    public class AIEnemySpawnData : EnemySpawnData {
        public bool IsRandom = false;

        public int RandomMinCount = 1;
        public int RandomMaxCount = 5;

        public List<Vector3> SpawnLocations = new List<Vector3>(16);
    }

    [CreateAssetMenu(menuName = "ScriptObject/Enemy/Enemy Spawn")]
    public class EnemySpawnDatas : ScriptableObject {
        public List<EnemySpawnData> Datas = new List<EnemySpawnData>(); 
    }
}
