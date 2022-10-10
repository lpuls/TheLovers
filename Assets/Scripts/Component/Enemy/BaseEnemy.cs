using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class BaseEnemy : ServerBaseController, IDamage {


        public float CurrentTime = 0;
        public MovePath MovePath = null;

        private bool _beginMove = false;

        //public override void Init() {
        //    base.Init();
        //    _movementComponent = GetComponent<MovementComponent>();
        //    _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        //}

        public void BeginMove(MovePath movePath) {
            MovePath = movePath;
            CurrentTime = 0;
            _beginMove = false;

            // GetSimulateComponent().UpdatePosition(transform.position, transform.position);
        }

        public override void Tick(float dt) {
            // 不是服务端的话，敌人不需要跑逻辑
            if (!_netSyncComponent.IsAuthority())
                return;

            base.Tick(dt);

            if (null != MovePath && _beginMove) {
                CurrentTime += dt;
                Vector3 delta = MovePath.Evaluate(CurrentTime);
                MoveByDelta(delta);

                if (CurrentTime >= MovePath.Time) {
                    _netSyncComponent.Kill(EDestroyActorReason.TimeOut);
                }
            }
        }

        protected void MoveByDelta(Vector3 delta) {
            transform.position += delta;
            GameLogicUtility.SetPositionDirty(gameObject);
        }

        protected void RotationByDelta(float angle) {
            transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + angle, new Vector3(0, 1, 0));
            GameLogicUtility.SetAngleDirty(gameObject);
        }

    }
}