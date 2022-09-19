using UnityEngine;

namespace Hamster.SpaceWar {
    public class NetSyncComponent : MonoBehaviour {
        public int NetID = 0;
        public int OwnerID = 0;
        public int ConfigID = 0;
        public bool PendingKill = false;

        public int GetUniqueID() {
            return OwnerID << 16 | NetID;
        }

        public void Update() {
            if (PendingKill) {
                PendingKill = false;

                AssetPool.Destroy(gameObject);
            }
        }
    }
}
