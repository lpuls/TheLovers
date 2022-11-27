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

        private bool _spawning = false;
        private int _spawnIndex = -1;
        private float _spawningTime = 0;


        public void Tick(float dt) {
            _cd -= dt;
            if (_cd <= 0)
                _cd = 0;

            // 检查是否有进行中的子弹生成
            if (_spawning) {
                _spawningTime += dt;
                CheckPendingSpawnBullet();
            }
        }

        public void Spawn(float cdGain) {
            if (_cd > 0)
                return;

            if (_spawning) {
                Debug.LogWarning("Bullet Spaing, Check CD And Spawn Delay");
                return;
            }

            // 检查子弹生成是否没有延时
            _spawnIndex = 0;
            _spawningTime = 0.0f;
            _spawning = false;
            CheckPendingSpawnBullet();

            _cd = Spawner.CD;
        }

        private void CheckPendingSpawnBullet() {
            bool complete = true;
            // 检查生成时间是否大于延迟时间，如果是则生成子弹
            for (int i = _spawnIndex; i < Spawner.SpawnInfos.Count; i++) {
                BulletSpawnInfo info = Spawner.SpawnInfos[i];
                _spawnIndex = i;
                if (info.Delay <= _spawningTime) {
                    SpawnBullet(info);
                }
                else {
                    complete = false;
                    break;
                }
            }

            // 检查是否生成完成
            _spawning = !complete;
            if (!_spawning) {
                _spawnIndex = 0;
                _spawningTime = 0;
            }
        }

        private void SpawnBullet(BulletSpawnInfo info) {
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

            Vector3 offset = info.Offset;
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.AngleAxis(info.Rotation.z, Vector3.forward);
            GameObject bullet = GameLogicUtility.CreateServerBullet(spawnID, _ownerID, transform.position + offset, rotation, this);
            bullet.transform.rotation = rotation;
        }

        public void Equip(int ownerID) {
            _ownerID = ownerID;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public GameObject GetOwner() {
            return Parent != null ? Parent : gameObject;
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
