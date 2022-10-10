using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    
    [CreateAssetMenu(menuName = "ScriptObject/BulletSpanwer")]
    public class BulletSpawner : ScriptableObject {
        public float DelayTime = 0;
        public float CD = 0;
        public AudioClip LauncherAudio = null;

        public List<int> SpawnIDs = new List<int>(8);
        public List<Vector3> SpawnOffsets = new List<Vector3>(8);
        public List<Vector3> SpawnDirections = new List<Vector3>(8);
    }
}
