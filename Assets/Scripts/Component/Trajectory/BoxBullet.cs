using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;
        private BoxCollider2D _collider = null;

        private void Awake() {
            _collider = GetComponent<BoxCollider2D>();
        }

        public override RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            return Physics2D.BoxCast(transform.position, _collider.size / 2, 0, direction, distance, 
                _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER);
        }

        public override Vector3 GetSize() {
            return _collider.size;
        }

    }
}
