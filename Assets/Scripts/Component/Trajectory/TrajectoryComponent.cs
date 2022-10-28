using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum ESpaceWarLayers {
        BEGIN = 6,
        BULLET = 7,
        PICKER = 8,
        PLAYER = 9,
        ENEMY = 10,
        NATURE = 11,
        END
    }


    public interface ITrajectorySpanwer {
        void OnHitObject(GameObject hitObject, GameObject trajectory);
        void OnHitDestroy(GameObject trajectory);
        void OnOutOfWold(GameObject trajectory);
        int GetLayer();
        ESpaceWarUnitType GetUnitType();
        GameObject GetGameObject();
        GameObject GetOwner();
    }

    public class TrajectoryComponent : BaseController, IMover {
        protected ITrajectorySpanwer _parent = null;
        protected bool _isPlayer = false;
        protected bool _pendingKill = false;
        protected Vector3 _moveDirection = Vector3.zero;
        protected float _moveSpeed = 100.0f;

        [SerializeField] protected bool _hitOnDestroy = true;

        protected MovementComponent _movementComponent = null;

        public bool IsPlayer => _isPlayer;

        public virtual void InitProperty(ITrajectorySpanwer parent, Vector3 moveDirection, float moveSpeed) {
            _parent = parent;
            _moveDirection = moveDirection;
            _moveSpeed = moveSpeed;

            // 初始化移动组件
            if (null == _movementComponent) {
                _movementComponent = GetComponent<MovementComponent>();
                _movementComponent.Mover = this;
            }
            _movementComponent.Speed = _moveSpeed;
            _movementComponent.Move(moveDirection);

            // 修改朝向
            transform.rotation = Quaternion.Euler(moveDirection);
           
            // 确定是玩家还是敌人的子弹
            _isPlayer = CheckLayerValue(parent.GetLayer(), ESpaceWarLayers.PLAYER);
        }

        public override void Tick(float dt) {
            Move(dt);
            if (!World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position) && null != _parent) {
                _parent.OnOutOfWold(gameObject);
            }
        }

        public virtual void Move(float dt) {
            transform.position = _movementComponent.MoveTick(transform.position, dt, -1, false);
            GameLogicUtility.SetPositionDirty(gameObject);
            //MoveBulletByDelta(_moveDirection * dt * _moveSpeed);
        }

        protected void MoveBulletByDelta(Vector3 delta) {
            transform.position += delta;
            GameLogicUtility.SetPositionDirty(gameObject);
        }

        //protected virtual void OnHitSomething(GameObject collider) {
        //    bool isPlayer = CheckLayerValue(collider.layer, ESpaceWarLayers.PLAYER);

        //    // 阵营不同，创成伤害
        //    if (isPlayer != _isPlayer) {
        //        IDamage damage = collider.GetComponent<IDamage>();
        //        if (null != damage) {
        //            damage.OnHit(_parent.GetGameObject(), gameObject);
        //            if (null != _parent)
        //                _parent.OnHitObject(collider, gameObject);
        //        }
        //    }
        //}

        public void OnHitObject(GameObject collider) {
            if (null != _parent)
                _parent.OnHitObject(collider, gameObject);
        }

        protected bool CheckLayerValue(int layer, ESpaceWarLayers value) {
            return layer == (int)value;
        }

        public GameObject GetOwner() {
            return _parent.GetOwner();
        }

        public virtual RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            throw new System.NotImplementedException();
        }

        public void OnHitSomething(RaycastHit2D raycastHit) {
            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, ESpaceWarLayers.BULLET);

            /*
            GameObject hitObject = raycastHit.collider.gameObject;
            bool isPlayer = CheckLayerValue(hitObject.layer, ESpaceWarLayers.PLAYER);

            // 阵营不同，创成伤害
            if (isPlayer != _isPlayer) {
                IDamage damage = hitObject.GetComponent<IDamage>();
                if (null != damage) {
                    damage.OnHit(_parent.GetGameObject(), gameObject);
                    if (null != _parent)
                        _parent.OnHitObject(hitObject, gameObject);
                }
            }
            */
        }

        public virtual Vector3 GetSize() {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawLine(transform.position, transform.position + _moveDirection * _moveSpeed);
        }

#endif
    }
}
