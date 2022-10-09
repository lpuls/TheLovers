using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;

        public override void Move(float dt) {
            float MoveDistance = dt * _moveSpeed;

            Vector3 currentLocation = transform.position;

            Quaternion quaternion = Quaternion.Euler(_moveDirection);
            if (Physics.BoxCast(currentLocation, BoxHalfSize, _moveDirection, out RaycastHit hitResult,
                quaternion, _moveSpeed, _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER)) {
                OnHitSomething(hitResult.collider.gameObject);
                MoveDistance = hitResult.distance;
            }

            MoveBulletByDelta(_moveDirection * MoveDistance);
        }

    }
}
