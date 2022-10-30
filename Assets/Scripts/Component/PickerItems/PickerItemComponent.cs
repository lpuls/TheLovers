using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class PickerItemComponent : BaseController, IMover {

        [SerializeField] private float _moveSpeed = 10;
        [SerializeField] private float _itemSize = 1.0f;
        protected MovementComponent _movementComponent = null;

        public override void Init() {
            base.Init();

            _movementComponent = GetComponent<MovementComponent>();
            _movementComponent.Speed = _moveSpeed;
            _movementComponent.Mover = this;
        }

        public override void OnEnable() {
            base.OnEnable();
            _movementComponent.Move(Vector3.left);
        }

        public override void Tick(float dt) {
            base.Tick(dt);

            transform.position = _movementComponent.MoveTick(transform.position, dt, 0, false);
            GameLogicUtility.SetPositionDirty(gameObject);
            if (!World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position)) {
                _netSyncComponent.Kill(EDestroyActorReason.OutOfWorld);
            }
        }

        public virtual void OnPicker(PlayerController playerController) {
            _netSyncComponent.Kill(EDestroyActorReason.BePick);
            _movementComponent.Stop();
        }

        public RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            return Physics2D.BoxCast(transform.position, GetSize(), 0, direction, distance, 1 << (int)ESpaceWarLayers.PLAYER);
        }

        public void OnHitSomething(RaycastHit2D raycastHit) {
            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, ESpaceWarLayers.PICKER);
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