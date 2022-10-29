using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class PickerItem : BaseController, IMover {

        [SerializeField] private float _moveSpeed = 10;
        [SerializeField] private float _itemSize = 1.0f;
        private Vector3 _moveDirection = Vector3.zero;
        protected MovementComponent _movementComponent = null;

        public override void Init() {
            base.Init();

            _movementComponent = GetComponent<MovementComponent>();
            _movementComponent.Speed = _moveSpeed;
            _movementComponent.Mover = this;
        }

        public override void OnEnable() {
            base.OnEnable();
            _moveDirection = GetRandomDirection();
            _movementComponent.Move(_moveDirection);
        }

        public override void Tick(float dt) {
            base.Tick(dt);

            _movementComponent.MoveTick(transform.position, dt, 0, false);
            if (!World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position)) {
                _moveDirection = GetRandomDirection();
                _movementComponent.Move(_moveDirection);
            }
        }

        protected static Vector3 GetRandomDirection() {
            float x = Random.Range(0, 1.0f);
            float y = Random.Range(0, 1.0f);
            return (new Vector3(x, y)).normalized;
        } 

        protected virtual void OnPicker(PlayerController playerController) {
            throw new System.NotImplementedException();
        }

        public RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            return Physics2D.BoxCast(transform.position, GetSize(), 0, direction, distance, 1 << (int)ESpaceWarLayers.PLAYER);
        }

        public void OnHitSomething(RaycastHit2D raycastHit) {
            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, ESpaceWarLayers.BULLET);
        }

        public Vector3 GetSize() {
            return new Vector2(_itemSize, _itemSize);
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _itemSize);
        }

#endif

    }
}