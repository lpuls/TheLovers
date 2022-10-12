using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public struct ServerOperate {
        public int Index;
        public int Operate;
    }

    public class ServerPlayerController : ServerBaseController {

        // 输入
        protected int _operate = 0;
        protected int _operatorIndex = 0;
        protected bool _isFired = false;
        protected List<ServerOperate> _operates = new List<ServerOperate>(8);


        protected override void ProcessorInput(int operate) {
            GameLogicUtility.GetOperateFromInput(transform, operate, out Vector3 moveDirection, out bool cast1);

            // 玩家发送射子弹
            if (null != _localAbilityComponent && cast1)
                _isFired = true;
                //_localAbilityComponent.Cast((int)EAbilityIndex.Fire, 1.0f);

            // 玩家进行移动
            if (!moveDirection.Equals(Vector3.zero))
                _movementComponent.Move(moveDirection);
            else
                _movementComponent.Stop();
        }

        public void SetOperator(int input, int index) {
            _operates.Add(new ServerOperate {
                Operate = input,
                Index = index
            });
        }

        protected override int GetOperator(InputKeyMapValue inputKeyMapValue) {
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

        public override void OnAlive(float dt) {
            while (_operates.Count > 0) {
                int input = GetOperator(InputKeyToValue);
                ProcessorInput(input);
                

                // 对武器进行更新
                _localAbilityComponent.Tick(dt);

                // 逻辑执行移动操作
                if (_movementComponent.NeedMove) {
                    transform.position = _movementComponent.MoveTick(transform.position, dt, _operatorIndex);
                    GameLogicUtility.SetPositionDirty(gameObject);
                }

                if (_isFired) {
                    _isFired = false;
                    _localAbilityComponent.Cast((int)EAbilityIndex.Fire, 1.0f);
                }

                _operate = 0;
                _operatorIndex = -1;
            }
        }

    }
}
