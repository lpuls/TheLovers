using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    [ExecuteInEditMode]
    public class BulletSpawnerDebuger : MonoBehaviour {
#if UNITY_EDITOR
        public BulletSpawner Spawner = null;
        public float CD = 0;
        public float Delay = 0;

        public bool Reset = false;
        public bool WriteToSpawner = false;
        public List<Transform> Bullets = new List<Transform>();
        public List<int> BulletIDs = new List<int>();

        public void Update() {
            if (Reset) {
                Bullets.Clear();
                TrajectoryComponent[] transforms = GetComponentsInChildren<TrajectoryComponent>();
                foreach (var item in transforms) {
                    Bullets.Add(item.transform);
                }
                Reset = false;
            }
            if (WriteToSpawner) {
                Spawner.CD = CD;
                Spawner.DelayTime = Delay;
                Spawner.SpawnIDs.Clear();
                Spawner.SpawnDirections.Clear();
                Spawner.SpawnOffsets.Clear();
                for (int i = 0; i < Bullets.Count; i++) {
                    var item = Bullets[i];
                    if (BulletIDs.Count > i) {
                        Spawner.SpawnIDs.Add(BulletIDs[i]);
                    }
                    else {
                        Spawner.SpawnIDs.Add(0);
                    }
                    Spawner.SpawnDirections.Add(item.forward);
                    Spawner.SpawnOffsets.Add(item.position - transform.position);
                }
                WriteToSpawner = false;
                UnityEditor.EditorUtility.SetDirty(Spawner);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

#endif
    }
}
