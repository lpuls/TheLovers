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

        protected ENetRole _role = ENetRole.ROLE_None;
        protected int _childCreateIndex = 0;

        public int GetUniqueID() {
            return OwnerID << 16 | NetID;
        }

        public void SetSimulatedProxy() {
            _role = ENetRole.ROLE_SimulatedProxy;
        }

        public bool IsSimulatedProxy() {
            return ENetRole.ROLE_SimulatedProxy == _role;
        }

        public void SetAutonomousProxy() {
            _role = ENetRole.ROLE_AutonomousProxy;
        }

        public bool IsAutonomousProxy() {
            return ENetRole.ROLE_AutonomousProxy == _role;
        }

        public void SetAuthority() {
            _role = ENetRole.ROLE_Authority;
        }

        public bool IsAuthority() {
            return ENetRole.ROLE_Authority == _role;
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

    }
}
