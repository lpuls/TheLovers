using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;

        public override void Move() {
            float MoveDistance = Time.deltaTime * _moveSpeed;

            Quaternion quaternion = Quaternion.Euler(_moveDirection);
            if (Physics.BoxCast(transform.position, BoxHalfSize, _moveDirection, out RaycastHit hitResult,
                quaternion, _moveSpeed, _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER)) {
                OnHitSomething(hitResult.collider.gameObject);
                MoveDistance = hitResult.distance;
            }

            transform.position += _moveDirection * MoveDistance;
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
