﻿using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class BaseEnemy : BaseController, IDamage {

        protected MovementComponent _movementComponent = null;
        protected LocalAbilityComponent _localAbilityComponent = null;

        public float CurrentTime = 0;
        public MovePath MovePath = null;

        private bool _beginMove = false;

        public override void Init() {
            base.Init();
            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        }

        public void BeginMove(MovePath movePath) {
            MovePath = movePath;
            CurrentTime = 0;
            _beginMove = false;

            GetSimulateComponent().UpdatePosition(transform.position, transform.position);
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
            Vector3 preLocation = _simulateComponent.CurrentLocation;
            Vector3 newLocation = _simulateComponent.CurrentLocation + delta;
            _simulateComponent.UpdatePosition(preLocation, newLocation);
            GameLogicUtility.SetPositionDirty(gameObject);
        }

        protected void RotationByDelta(float angle) {
            Vector3 rotation = transform.rotation.eulerAngles;
            Vector3 newRotation = new Vector3(0, rotation.y + Mathf.Clamp(angle, -360, 360), 0);
            _simulateComponent.UpdateAngle(rotation.y, newRotation.y);
            GameLogicUtility.SetAngleDirty(gameObject);
        }

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
            //_netSyncComponent.Kill(EDestroyActorReason.BeHit);
            Debug.Log("I Be Kill!!");
        }
    }
}