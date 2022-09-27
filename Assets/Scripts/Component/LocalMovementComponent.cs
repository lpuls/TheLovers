using UnityEngine;

namespace Hamster.SpaceWar {
    public class LocalMovementComponent : MonoBehaviour {

        public float Speed = 30.0f;  // 之后读配置表
        public float HalfSize = 1.0f;

        private bool _needMove = false;
        private Vector3 _moveDirection = Vector3.zero;

        public void Move(Vector3 direction) {
            _moveDirection = direction;
            _needMove = true;
        }

        public void Stop() {
            _moveDirection = Vector3.zero;
            _needMove = false;
        }

        public void Update() {
            if (!_needMove)
                return;

            transform.position += _moveDirection * Speed * Time.deltaTime;
            transform.position = World.GetWorld<ServerSpaceWarWorld>().ClampInWorld(transform.position, HalfSize);
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(HalfSize, HalfSize, HalfSize));
            ;
        }
#endif
    }
}
