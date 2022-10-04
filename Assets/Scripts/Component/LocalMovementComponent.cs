using UnityEngine;

namespace Hamster.SpaceWar {
    public class LocalMovementComponent : MonoBehaviour {

        public float Speed = 30.0f;  // 之后读配置表
        public float HalfSize = 1.0f;

        [SerializeField]
        private float _moveSpeed = 0;
        private Vector3 _moveDirection = Vector3.zero;

        private bool _needMove = false;
        private bool _beginStop = false;

        public void Move(Vector3 direction) {
            _moveDirection = direction;
            _needMove = true;
            _beginStop = false;
        }

        public void Stop() {
            _beginStop = true;

            _beginStop = false;
            _needMove = false;
            _moveDirection = Vector3.zero;
            _moveSpeed = 0;
        }

        public void Update() {
            if (!_needMove)
                return;

            MoveTick(_moveDirection);
        }

        public void MoveTick(Vector3 direction) {
            _moveSpeed = Speed * Time.deltaTime;

            // 更新角色位置
            transform.position += direction * _moveSpeed;
            transform.position = World.GetWorld<BaseSpaceWarWorld>().ClampInWorld(transform.position, HalfSize);
            GameLogicUtility.SetPositionDirty(gameObject);
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(HalfSize, HalfSize, HalfSize));
        }
#endif
    }
}
