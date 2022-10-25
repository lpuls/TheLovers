﻿using System;
using UnityEngine;

namespace Hamster.SpaceWar {

    public interface IMover {
        RaycastHit2D MoveRayCast(float distance, Vector3 direction);
        void OnHitSomething(RaycastHit2D raycastHit);
        Vector3 GetSize();
    }

    public class MovementComponent : MonoBehaviour {

        public float Speed = 30.0f;  // 之后读配置表
        public float HalfSize = 1.0f;
        public IMover Mover = null;

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

            //Vector3 oldLocation = location;
            if (null != Mover) {
                RaycastHit2D raycastHit = Mover.MoveRayCast(_moveSpeed, _moveDirection);
                if (null != raycastHit.collider) {
                    Mover.OnHitSomething(raycastHit);
                    _moveSpeed = raycastHit.distance;
                }
                location += _moveSpeed * _moveDirection;
            }
            else {
                location += _moveDirection * _moveSpeed;
            }
            location = World.GetWorld<BaseSpaceWarWorld>().ClampInWorld(location, _collider.size);
            // Debug.Log(string.Format("MoveTick {0} {1} {2} {3} {4} {5}", gameObject.name, oldLocation, location, _moveDirection, _moveSpeed, index));
            return location;
        }

    }
}
