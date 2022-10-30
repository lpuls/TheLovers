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
        public int Player1ID = 0;
        public int Player2ID = 0;
        public int EnemeyID = 0;

        public void Update() {
            if (Reset) {
                Bullets.Clear();
                TrajectoryEffectComponent[] transforms = GetComponentsInChildren<TrajectoryEffectComponent>();
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
                Spawner.Player1ID = Player1ID;
                Spawner.Player2ID = Player2ID;
                Spawner.EnemeyID = EnemeyID;
                Spawner.SpawnCount = Bullets.Count;
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
