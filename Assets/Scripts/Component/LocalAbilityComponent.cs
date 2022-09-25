using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EAbilityIndex {
        Fire = 0,
        Ultimate = 1
    }

    public class LocalAbilityComponent : MonoBehaviour, ITrajectorySpanwer {
        // todo 之后改成配置表
        public Vector3 SpawnOffset = Vector3.zero;

        private int _ownerID = 0;
        private List<int> _abilitys = new List<int>(4);
        private List<float> _abilityCD = new List<float>(4);

        public void Init(int configID) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig)) {
                foreach (var it in shipConfig.AbilityID) {
                    _abilitys.Add(it);
                    _abilityCD.Add(0);
                }
            }

            NetSyncComponent netSyncComponent = GetComponent<NetSyncComponent>();
            _ownerID = netSyncComponent.NetID;
        }

        public void CastAbility(int index) {
            if (null == _abilitys || index < 0 || index >= _abilitys.Count) {
                Debug.LogError("Cast Ability Filed " + index);
                return;
            }

            // 检查CD情况
            if (_abilityCD[index] > 0)
                return;


            GameLogicUtility.CreateServerBullet(_abilitys[index], _ownerID, transform.position + SpawnOffset, this, out float cd);
            _abilityCD[index] = cd;
        }

        private void Update() {
            for (int i = 0; i < _abilityCD.Count; i++) {
                _abilityCD[i] -= Time.deltaTime;
                if (_abilityCD[i] <= 0)
                    _abilityCD[i] = 0;
            }
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public int GetLayer() {
            return gameObject.layer;
        }

        public Vector3 GetPosition() {
            return transform.position + SpawnOffset;
        }

        private void OnDestroyTrajectory(GameObject gameObject) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.Kill();
            }
        }

        public void OnHitDestroy(GameObject trajectory) {
            OnDestroyTrajectory(trajectory);
        }

        public void OnHitObject(GameObject hitObject, GameObject trajectory) {
            OnDestroyTrajectory(trajectory);
        }

        public void OnOutOfWold(GameObject trajectory) {
            OnDestroyTrajectory(trajectory);
        }


#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + SpawnOffset, 0.3f);
        }
#endif
    }
}
