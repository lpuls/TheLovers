using UnityEngine;

namespace Hamster.SpaceWar {
    public class AIEnemy : BaseEnemy {

        [SerializeField] private float _fireInterval = 0.5f;
        private float _fireTime = 0;

        [SerializeField] private float _closeDistance = 0.5f;
        private Vector3 _moveTarget = Vector3.zero;

        public override void Init() {
            base.Init();
            RandomMoveTarget();
        }

        public override void OnEnable() {
            base.OnEnable();
            RandomMoveTarget();
        }

        public override void OnAlive(float dt) {
            base.OnAlive(dt);

            // 检查是否到达指定位置
            if (Vector3.Distance(transform.position, _moveTarget) < _closeDistance) {
                RandomMoveTarget();
            }
            else if (null != _movementComponent) {
                _movementComponent.Move((_moveTarget - transform.position).normalized);
            }

            // 进行移动
            if (null != _movementComponent && _movementComponent.NeedMove) {
                transform.position = _movementComponent.MoveTick(transform.position, dt, 0);
                // _movementComponent.MoveTick(dt);
            }

            // 尝试进行攻击
            _fireTime += dt;
            if (_fireTime >= _fireInterval) {
                _fireTime -= _fireInterval;
                _localAbilityComponent.Cast((int)EAbilityIndex.Fire, 1.0f);
            }
        }

        private void RandomMoveTarget() {
            _moveTarget = GetRandomLocation();
        }
    }
}
