using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    [System.Serializable]
    public class BulletSpawnInfo {
        public int ID = 0;
        public float Delay = 0;
        public Vector3 Offset = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
    }

    [CreateAssetMenu(menuName = "ScriptObject/BulletSpanwer")]
    public class BulletSpawner : ScriptableObject {
        // public float DelayTime = 0;
        public float CD = 0;
        public AudioClip LauncherAudio = null;

        // public List<int> SpawnIDs = new List<int>(8);
        //public int SpawnCount = 1;
        public int Player1ID = 0;
        public int Player2ID = 0;
        public int EnemeyID = 0;
        // public List<float> Delays = new List<float>();
        // public List<Vector3> SpawnOffsets = new List<Vector3>(8);
        // public List<Vector3> SpawnDirections = new List<Vector3>(8);
        public List<BulletSpawnInfo> SpawnInfos = new List<BulletSpawnInfo>(8);
    }
}
