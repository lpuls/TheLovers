using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class CollisionResult : IPool {

        public RaycastHit2D RaycastHit;
        public GameObject Caster = null;

        public void Reset() {
            RaycastHit = default;
            Caster = null;
        }
    }

    public class CollisionProcessManager : IServerTicker {
        private List<CollisionResult> _collisionResults = new List<CollisionResult>(64);

        public void AddCollisionResult(RaycastHit2D raycastHit, GameObject caster) {
            CollisionResult collisionResult = ObjectPool<CollisionResult>.Malloc();
            collisionResult.RaycastHit = raycastHit;
            collisionResult.Caster = caster;
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
                ESpaceWarLayers casterLayer = (ESpaceWarLayers)item.Caster.layer;
                ESpaceWarLayers colliderLayer = (ESpaceWarLayers)colliderObject.layer;
                
                // 任一一方为子弹，则使用子弹的处理方式
                if (ESpaceWarLayers.BULLET == casterLayer) {
                    OnBulletHitSomething(item.Caster, colliderObject);
                }
                else if (ESpaceWarLayers.BULLET == colliderLayer) {
                    OnBulletHitSomething(colliderObject, item.Caster);
                }

                ObjectPool<CollisionResult>.Free(item);
            }
            _collisionResults.Clear();
        }

        private void OnBulletHitSomething(GameObject bullet, GameObject collider) {
            if (bullet.TryGetComponent<TrajectoryComponent>(out TrajectoryComponent trajectoryComponent)) {
                GameObject attacker = trajectoryComponent.GetOwner();

                ESpaceWarLayers layer = (ESpaceWarLayers)collider.layer;
                if (trajectoryComponent.IsPlayer && ESpaceWarLayers.ENEMY == layer) {
                    if (collider.TryGetComponent<ServerBaseController>(out ServerBaseController playerController)) {
                        playerController.OnHit(attacker, bullet);
                    }
                } 
                else if (!trajectoryComponent.IsPlayer || ESpaceWarLayers.PLAYER == layer) {
                    if (collider.TryGetComponent<ServerBaseController>(out ServerBaseController playerController)) {
                        playerController.OnHit(attacker, bullet);
                    }
                }
            }
            
            
        }

    }

}
