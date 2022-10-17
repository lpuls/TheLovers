using UnityEngine;

namespace Hamster.SpaceWar {
    public class BoxBullet : TrajectoryComponent {
        public Vector3 BoxHalfSize = Vector3.one;
        private BoxCollider2D _collider = null;

        private void Awake() {
            _collider = GetComponent<BoxCollider2D>();
        }

        public override void Move(float dt) {
            float moveDistance = dt * _moveSpeed;

            // bool hasHit = false;
            // float originDistance = moveDistance;
            Vector3 currentLocation = transform.position;

            Quaternion quaternion = Quaternion.Euler(_moveDirection);
            //if (Physics.BoxCast(currentLocation, BoxHalfSize, _moveDirection, out RaycastHit hitResult,
            //    quaternion, moveDistance, _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER)) {
            //    OnHitSomething(hitResult.collider.gameObject);
            //    // hasHit = true;
            //    moveDistance = hitResult.distance;
            //}
            RaycastHit2D hitResult = Physics2D.BoxCast(currentLocation, _collider.size / 2, 0, _moveDirection, moveDistance, 
                _isPlayer ? 1 << (int)ESpaceWarLayers.ENEMY : 1 << (int)ESpaceWarLayers.PLAYER);
            if (null != hitResult.collider) {
                OnHitSomething(hitResult.collider.gameObject);
                moveDistance = hitResult.distance;
            }

            MoveBulletByDelta(_moveDirection * moveDistance);
            // Debug.Log(string.Format("UpdateMove Not Hit {0}, {1}, {2}, {3}, {4}, {5}", gameObject.name, moveDistance, originDistance, hasHit, currentLocation, transform.position));
        }

    }
}
