using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    [ExecuteInEditMode]
    public class BulletSpawnerDebuger : MonoBehaviour {
#if UNITY_EDITOR
        public BulletSpawner Spawner = null;
        public float CD = 0;
        public float DelayDelta = 0.0f;

        public bool Reset = false;
        public bool WriteToSpawner = false;
        public List<float> Delays = new List<float>();
        public List<Transform> Bullets = new List<Transform>();
        public List<int> BulletIDs = new List<int>();
        public int Player1ID = 0;
        public int Player2ID = 0;
        public int EnemeyID = 0;

        public void Update() {
            if (Reset) {
                Bullets.Clear();
                TrajectoryEffectComponent[] transforms = GetComponentsInChildren<TrajectoryEffectComponent>();
                int index = 0;
                foreach (var item in transforms) {
                    Bullets.Add(item.transform);
                    item.name = "Bullet" + index;
                    if (index >= BulletIDs.Count) {
                        BulletIDs.Add(1);
                    }
                    if (index >= Delays.Count) {
                        Delays.Add(DelayDelta * index);
                    }
                    index++;
                }
                Reset = false;
            }
            if (WriteToSpawner) {
                Spawner.CD = CD;
                Spawner.Player1ID = Player1ID;
                Spawner.Player2ID = Player2ID;
                Spawner.EnemeyID = EnemeyID;
                Spawner.SpawnInfos.Clear();
                for (int i = 0; i < Bullets.Count; i++) {
                    var item = Bullets[i];
                    BulletSpawnInfo info = new BulletSpawnInfo {
                        ID = BulletIDs.Count > i ? BulletIDs[i] : 0,
                        Delay = Delays.Count > i ? Delays[i] : 0.0f,
                        Offset = item.position - transform.position,
                        Rotation = item.rotation.eulerAngles
                    };
                    Spawner.SpawnInfos.Add(info);
                }
                Spawner.SpawnInfos.Sort(CompareBulletSpawnInfo);
                WriteToSpawner = false;
                UnityEditor.EditorUtility.SetDirty(Spawner);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

        private static int CompareBulletSpawnInfo(BulletSpawnInfo x, BulletSpawnInfo y) {
            if (x.Delay < y.Delay)
                return -1;
            else if (x.Delay > y.Delay)
                return 1;
            else
                return 0;
        }

#endif
    }
}
