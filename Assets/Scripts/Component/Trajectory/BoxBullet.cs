using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;

        public override void Move(float dt) {
            float MoveDistance = dt * _moveSpeed;

            Vector3 currentLocation = GetSimulateComponent().CurrentLocation;

            Quaternion quaternion = Quaternion.Euler(_moveDirection);
            if (Physics.BoxCast(currentLocation, BoxHalfSize, _moveDirection, out RaycastHit hitResult,
                quaternion, _moveSpeed, _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER)) {
                OnHitSomething(hitResult.collider.gameObject);
                MoveDistance = hitResult.distance;
            }

            // transform.position += _moveDirection * MoveDistance;
            MoveBulletByDelta(_moveDirection * MoveDistance);
        }

#if UNITY_EDITOR
        public override void OnDrawGizmosSelected() {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, BoxHalfSize);
        }
#endif
    }
}
