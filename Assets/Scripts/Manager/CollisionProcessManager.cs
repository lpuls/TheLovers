using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class CollisionResult : IPool {

        public RaycastHit2D RaycastHit;
        public GameObject Caster = null;
        public ESpaceWarLayers CasterLayer = ESpaceWarLayers.BEGIN;

        public void Reset() {
            RaycastHit = default;
            Caster = null;
        }
    }

    public class CollisionProcessManager : IServerTicker {
        private List<CollisionResult> _collisionResults = new List<CollisionResult>(64);

        public void AddCollisionResult(RaycastHit2D raycastHit, GameObject caster, ESpaceWarLayers casterLayer) {
            CollisionResult collisionResult = ObjectPool<CollisionResult>.Malloc();
            collisionResult.RaycastHit = raycastHit;
            collisionResult.Caster = caster;
            collisionResult.CasterLayer = casterLayer;
            _collisionResults.Add(collisionResult);
        }

        public int GetPriority() {
            return (int)EServerTickLayers.LateTick;
        }

        public bool IsEnable() {
            return true;
        }

        public void Tick(float dt) {
            foreach (var item in _collisionResults) {
                GameObject colliderObject = item.RaycastHit.collider.gameObject;
                ESpaceWarLayers casterLayer = item.CasterLayer;
                ESpaceWarLayers colliderLayer = (ESpaceWarLayers)colliderObject.layer;
                
                // 任一一方为子弹，则使用子弹的处理方式
                if (ESpaceWarLayers.BULLET == casterLayer) {
                    OnBulletHitSomething(item.Caster, colliderObject);
                }
                else if (ESpaceWarLayers.BULLET == colliderLayer) {
                    OnBulletHitSomething(colliderObject, item.Caster);
                }
                // 任一一方为拾取物且另一方为玩家，按拾取物的方式处理
                else if (ESpaceWarLayers.PICKER == casterLayer && ESpaceWarLayers.PLAYER == colliderLayer) {
                    OnPickerHitSomething(item.Caster, colliderObject);
                }
                else if (ESpaceWarLayers.PICKER == colliderLayer && ESpaceWarLayers.PLAYER == casterLayer) {
                    OnPickerHitSomething(colliderObject, item.Caster);
                }
                // 任一一方为玩家且另一方为敌人，各自扣除一定血量
                else if (ESpaceWarLayers.PLAYER == casterLayer && ESpaceWarLayers.ENEMY == colliderLayer) {
                    OnPlayerHitEnemey(item.Caster, colliderObject);
                }
                else if (ESpaceWarLayers.PLAYER == colliderLayer && ESpaceWarLayers.ENEMY == casterLayer) {
                    OnPlayerHitEnemey(colliderObject, item.Caster);
                }

                ObjectPool<CollisionResult>.Free(item);
            }
            _collisionResults.Clear();
        }

        private void OnBulletHitSomething(GameObject bullet, GameObject collider) {
            if (bullet.TryGetComponent<TrajectoryComponent>(out TrajectoryComponent trajectoryComponent)) {
                GameObject attacker = trajectoryComponent.GetOwner();

                int damage = 1;
                if (bullet.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                    if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(netSyncComponent.ConfigID, out Config.Abilitys abilitys)) {
                        damage = abilitys.Damage;
                    }
                }

                DamageInfo damageInfo = ObjectPool<DamageInfo>.Malloc();
                damageInfo.Damage = damage;
                damageInfo.Caster = bullet;
                damageInfo.Murderer = attacker;
                damageInfo.DamageReason = EDamageReason.BulletDamage;

                ESpaceWarLayers layer = (ESpaceWarLayers)collider.layer;
                if (trajectoryComponent.IsPlayer && ESpaceWarLayers.ENEMY == layer) {
                    if (collider.TryGetComponent<ServerBaseController>(out ServerBaseController playerController)) {
                        // playerController.OnHit(attacker, bullet);
                        playerController.TakeDamage(damageInfo);
                    }
                } 
                else if (!trajectoryComponent.IsPlayer || ESpaceWarLayers.PLAYER == layer) {
                    if (collider.TryGetComponent<ServerBaseController>(out ServerBaseController playerController)) {
                        // playerController.OnHit(attacker, bullet);
                        playerController.TakeDamage(damageInfo);
                    }
                }
                ObjectPool<DamageInfo>.Free(damageInfo);

                trajectoryComponent.OnHitObject(collider);
            }
        }

        private void OnPickerHitSomething(GameObject pickerItem, GameObject collider) {
            if (pickerItem.TryGetComponent<PickerItemComponent>(out PickerItemComponent pickerItemComponent)) {
                if (collider.TryGetComponent<PlayerController>(out PlayerController playerController)) {
                    pickerItemComponent.OnPicker(playerController);
                }
            }
        }

        private void OnPlayerHitEnemey(GameObject player, GameObject enemy) {
            
        }

    }

}
