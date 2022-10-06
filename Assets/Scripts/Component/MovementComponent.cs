using UnityEngine;

namespace Hamster.SpaceWar {
    public class MovementComponent : MonoBehaviour {

        public float Speed = 30.0f;  // 之后读配置表
        public float HalfSize = 1.0f;

        [SerializeField]
        private float _moveSpeed = 0;
        private Vector3 _moveDirection = Vector3.zero;

        public bool NeedMove {
            get;
            private set;
        }

        public void Move(Vector3 direction) {
            _moveDirection = direction;
            NeedMove = true;
        }

        public void Stop() {
            NeedMove = false;
            _moveDirection = Vector3.zero;
            _moveSpeed = 0;
        }
        public Vector3 MoveTick(Vector3 location, float dt, int index) {
            _moveSpeed = Speed * dt;

            Vector3 oldLocation = location;
            location += _moveDirection * _moveSpeed;
            location = World.GetWorld<BaseSpaceWarWorld>().ClampInWorld(location, HalfSize);
            return location;
        }

        public void MoveTick(float dt) {
            _moveSpeed = Speed * dt;

            // 更新角色位置
            transform.position += _moveDirection * _moveSpeed;
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
