using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public interface IDamage {
        void OnHit(GameObject hitObject, GameObject hitTrajectory);
    }

    public struct ServerOperate {
        public int Index;
        public int Operate;
    }

    public class ServerPlayerController : PlayerController, IDamage {

        private bool _readByInputDevice = true;
        protected int _operate = 0;
        protected int _operatorIndex = 0;
        protected List<ServerOperate> _operates = new List<ServerOperate>(8);

        protected MovementComponent _movementComponent = null;
        protected LocalAbilityComponent _localAbilityComponent = null;

        protected override void ProcessorInput(int operate) {
            GameLogicUtility.GetOperateFromInput(transform, operate, out Vector3 moveDirection, out bool cast1);

            // 玩家发送射子弹
            if (null != _localAbilityComponent && cast1)
                _localAbilityComponent.Cast((int)EAbilityIndex.Fire, 1.0f);

            // 玩家进行移动
            if (!moveDirection.Equals(Vector3.zero))
                _movementComponent.Move(moveDirection);
            else
                _movementComponent.Stop();
        }

        public override void Init() {
            base.Init();
            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
            _netSyncComponent = GetComponent<NetSyncComponent>();
        }

        public void SetIsReadByInputDevice(bool readByInputDevice) {
            _readByInputDevice = readByInputDevice;
        }

        public void SetOperator(int input, int index) {
            //if (_readByInputDevice) {
            //    _operate = input;
            //    _operatorIndex = index;
            //}
            //else {
                _operates.Add(new ServerOperate {
                    Operate = input,
                    Index = index
                });
            //}
        }

        protected override int GetOperator(InputKeyMapValue inputKeyMapValue) {
            //if (_readByInputDevice) {
            //    _operate = GameLogicUtility.ReadKeyboardInput(inputKeyMapValue);
            //    _netSyncComponent.PredictionIndex = -1;
            //}
            //else 
            if (_operates.Count > 0) {
                ServerOperate serverOperate = _operates[0];
                _operates.RemoveAt(0);
                _operate = serverOperate.Operate;
                _operatorIndex = serverOperate.Index;

                _netSyncComponent.PredictionIndex = _operatorIndex;
            }
            else {
                _operate = 0;
                _operatorIndex = -1;
            }
            return _operate;
        }

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
        }

        public override void Tick(float dt) {
            base.Tick(dt);
            _operate = 0;

            // 对武器进行更新
            _localAbilityComponent.Tick(dt);

            // 逻辑执行移动操作
            if (_movementComponent.NeedMove) {
                // Vector3 preLocation = _simulateComponent.CurrentLocation;
                // Vector3 currentLocation = _movementComponent.MoveTick(_simulateComponent.CurrentLocation, dt, _operatorIndex);
                // _simulateComponent.UpdatePosition(preLocation, currentLocation);
                transform.position = _movementComponent.MoveTick(transform.position, dt, _operatorIndex);
                GameLogicUtility.SetPositionDirty(gameObject);
            }
        }

    }
}
