using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WeaponComponent : MonoBehaviour, ITrajectorySpanwer {

        public EAbilityIndex Type = EAbilityIndex.MainWeapon;
        public BulletSpawner Spawner = null;
        public GameObject Parent = null;
        public int WeaponID = 0;

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
            ESpaceWarUnitType unitType = GetUnitType();
            int spawnID = Spawner.EnemeyID;
            switch (unitType) {
                case ESpaceWarUnitType.Player1:
                    spawnID = Spawner.Player1ID;
                    break;
                case ESpaceWarUnitType.Player2:
                    spawnID = Spawner.Player2ID;
                    break;
                case ESpaceWarUnitType.Enemy:
                    spawnID = Spawner.EnemeyID;
                    break;
            }
            for (int i = 0; i < Spawner.SpawnCount; i++) {
                Vector3 offset = Spawner.SpawnOffsets[i];
                // Vector3 direction = transform.rotation * Spawner.SpawnDirections[i];
                Quaternion rotation = transform.rotation;
                rotation *= Quaternion.AngleAxis(Spawner.SpawnDirections[i].z, Vector3.forward);
                GameObject bullet = GameLogicUtility.CreateServerBullet(spawnID, _ownerID, transform.position + offset, rotation, this);
                bullet.transform.rotation = rotation;
            }
            _cd = Spawner.CD;
        }

        public void Equip(int ownerID) {
            _ownerID = ownerID;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public GameObject GetOwner() {
            return null != Parent ? Parent : gameObject;
        }

        public int GetLayer() {
            return null != Parent ? Parent.layer : gameObject.layer;
        }

        public ESpaceWarUnitType GetUnitType() {
            if (Parent.TryGetComponent<BaseController>(out BaseController baseController)) {
                return baseController.UnitType;
            }
            return ESpaceWarUnitType.None;
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
