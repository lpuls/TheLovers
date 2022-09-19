using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;

        public override void Move() {
            float MoveDistance = Time.deltaTime * MoveSpeed;

            Quaternion quaternion = Quaternion.Euler(MoveDirection);
            if (Physics.BoxCast(transform.position, BoxHalfSize, MoveDirection, out RaycastHit hitResult,
                quaternion, MoveSpeed, _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER)) {
                OnHitSomething(hitResult.collider.gameObject);
                MoveDistance = hitResult.distance;
            }

            transform.position += MoveDirection * MoveDistance;
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
