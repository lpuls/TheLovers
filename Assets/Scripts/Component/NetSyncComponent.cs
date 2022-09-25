using UnityEngine;

namespace Hamster.SpaceWar {

    public enum ENetRole {
        ROLE_None = 0,
        ROLE_SimulatedProxy = 1,    // 该角色的本地模拟代理
        ROLE_AutonomousProxy = 2,   // 该角色的本地自治代理
        ROLE_Authority = 4,         // 对角色的权威控制。
    }

    public enum ENetType {
        None = 0,
        Player = 1,
        Bullet = 2
    }

    public class NetSyncComponent : MonoBehaviour {
        public int NetID = 0;
        public int OwnerID = 0;
        public int ConfigID = 0;
        public bool PendingKill = false;
        public ENetType NetType = ENetType.None;

        protected int _role = 0;
        protected int _childCreateIndex = 0;

        public int GetUniqueID() {
            return OwnerID << 16 | NetID;
        }

        public void SetSimulatedProxy(bool set) {
            if (set) {
                _role |= 0x01;
            }
            else {
                _role ^= 0x01;
            }
        }

        public bool IsSimulatedProxy() {
            return (_role & 0x01) != 0x01;
        }

        public void SetAutonomousProxy(bool set) {
            if (set) {
                _role |= 0x02;
            }
            else {
                _role ^= 0x02;
            }
        }

        public bool IsAutonomousProxy() {
            return (_role & 0x02) != 0x02;
        }

        public void SetAuthority(bool set) {
            if (set) {
                _role |= 0x04;
            }
            else {
                _role ^= 0x04;
            }
        }

        public bool IsAuthority() {
            return (_role & 0x04) != 0x04;
        }

        public void Kill() {
            PendingKill = true;
        }

        public bool IsPendingKill() {
            return PendingKill;
        }

        public int GetSpawnIndex() {
            return OwnerID << 16 | ++_childCreateIndex;
        }

        private void OnDestroy() {
            Debug.Log("=====?");
        }
    }
}
