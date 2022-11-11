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
        // protected Vector3 _moveDirection = Vector3.zero;
        protected float _moveSpeed = 100.0f;

        [SerializeField] protected bool _hitOnDestroy = true;

        protected MovementComponent _movementComponent = null;

        public bool IsPlayer => _isPlayer;

        public virtual void InitProperty(ITrajectorySpanwer parent, float moveSpeed) {
            Init();

            _parent = parent;
            // _moveDirection = transform.right;  // moveDirection;
            _moveSpeed = moveSpeed;

            // 初始化移动组件
            if (null == _movementComponent) {
                _movementComponent = GetComponent<MovementComponent>();
                _movementComponent.Mover = this;
            }
            _movementComponent.Speed = _moveSpeed;
            _movementComponent.Move(transform.right);

            // 修改朝向
            transform.rotation = Quaternion.Euler(transform.right);
           
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

        public bool OnHitSomething(RaycastHit2D raycastHit) {
            if (CollisionProcessManager.UnitIsInvincible(raycastHit.collider.gameObject))
                return false;

            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, ESpaceWarLayers.BULLET);
            return true;
        }

        public virtual Vector3 GetSize() {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR
        public bool EnableDebugDraw = true;

        public void OnDrawGizmos() {
            if (EnableDebugDraw) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, GetSize());
            }
        }
#endif
    }
}
