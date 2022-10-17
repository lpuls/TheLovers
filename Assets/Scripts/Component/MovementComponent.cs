﻿using UnityEngine;

namespace Hamster.SpaceWar {
    public class MovementComponent : MonoBehaviour {

        public float Speed = 30.0f;  // 之后读配置表
        public float HalfSize = 1.0f;

        [SerializeField]
        private float _moveSpeed = 0;
        private Vector3 _moveDirection = Vector3.zero;
        private BoxCollider2D _collider = null;

        private void Awake() {
            _collider = GetComponent<BoxCollider2D>();
        }

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
            location = World.GetWorld<BaseSpaceWarWorld>().ClampInWorld(location, _collider.size / 2);
            // Debug.Log(string.Format("MoveTick {0} {1} {2} {3} {4} {5}", gameObject.name, oldLocation, location, _moveDirection, _moveSpeed, index));
            return location;
        }

        public void MoveTick(float dt) {
            _moveSpeed = Speed * dt;

            // 更新角色位置
            transform.position += _moveDirection * _moveSpeed;
            transform.position = World.GetWorld<BaseSpaceWarWorld>().ClampInWorld(transform.position, _collider.size / 2);
            GameLogicUtility.SetPositionDirty(gameObject);
        }

    }
}
