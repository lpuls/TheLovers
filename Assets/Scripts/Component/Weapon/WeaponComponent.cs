using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WeaponComponent : MonoBehaviour, ITrajectorySpanwer {

        public EAbilityIndex Type = EAbilityIndex.Fire;
        public BulletSpawner Spawner = null;
        public GameObject Parent = null;

        private int _ownerID = 0;
        private float _cd = 0;
        private WaitForSeconds _waitSecond = null;

        public void Awake() {
            if (null != _waitSecond) {
                _waitSecond = new WaitForSeconds(Spawner.DelayTime);
            }
        }

        public void Tick(float dt) {
            _cd -= dt;
            if (_cd <= 0)
                _cd = 0;
        }

        public void Spawn(float cdGain) {
            if (_cd > 0)
                return;

            if (Spawner.DelayTime > 0) {
                StartCoroutine(SpawnBullets(cdGain));
            }
            else {
                SpawnBulletImpl(cdGain);
            }
        }

        private IEnumerator SpawnBullets(float cdGain) {
            yield return _waitSecond;
            SpawnBulletImpl(cdGain);
        }

        private void SpawnBulletImpl(float cdGain) {
            for (int i = 0; i < Spawner.SpawnIDs.Count; i++) {
                int id = Spawner.SpawnIDs[i];
                Vector3 offset = Spawner.SpawnOffsets[i];
                Vector3 direction = Spawner.SpawnDirections[i];
                GameLogicUtility.CreateServerBullet(id, _ownerID, transform.position + offset, direction, this);
            }
            _cd = Spawner.CD;
        }

        public void Equip(int ownerID) {
            _ownerID = ownerID;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public int GetLayer() {
            return null != Parent ? Parent.layer : gameObject.layer;
        }

        private void OnDestroyTrajectory(GameObject gameObject, EDestroyActorReason reason) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.Kill(reason);
            }
        }

        public void OnHitDestroy(GameObject trajectory) {
            OnDestroyTrajectory(trajectory, EDestroyActorReason.BeHit);
        }

        public void OnHitObject(GameObject hitObject, GameObject trajectory) {
            OnDestroyTrajectory(trajectory, EDestroyActorReason.HitOther);
        }

        public void OnOutOfWold(GameObject trajectory) {
            OnDestroyTrajectory(trajectory, EDestroyActorReason.OutOfWorld);
        }
    }
}
