﻿using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public struct ServerOperate {
        public int Index;
        public int Operate;
    }


    public class ServerPlayerController : ServerBaseController {

        // 输入
        protected bool _enableInput = true;
        protected int _operate = 0;
        protected int _operatorIndex = 0;
        protected InputCommand _inputCommand = new InputCommand();
        protected List<ServerOperate> _operates = new List<ServerOperate>(8);

        // 闪避处理
        public bool IsDodge { get; protected set; }
        protected float _dodgeTime = 0;
        [SerializeField] protected float _maxDodgeTime = 0.62f;

        public override void Init() {
            base.Init();

            OnDie += OnPlayerDie;
        }

        protected override void ProcessorInput(int operate) {
            // 未启用输入时，不处理
            if (!_enableInput)
                return;

            // 分析输入
            GameLogicUtility.GetOperateFromInput(transform, operate, _inputCommand);

            // 玩家进行移动
            if (!_inputCommand.Direction.Equals(Vector3.zero))
                _movementComponent.Move(_inputCommand.Direction);
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

        public void ChangeWeapon(EAbilityIndex abilityIndex, int id) {
            if (null != _localAbilityComponent)
                _localAbilityComponent.ChangeWeapon(abilityIndex, id);
        }

        public void EnableInput(bool enable) {
            _enableInput = enable;
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
                    // Debug.Log(string.Format("ServerPlayerController {0} {1} {2} {3} {4}", gameObject.name, oldPosition, transform.position, input, _operatorIndex));
                    GameLogicUtility.SetPositionDirty(gameObject);
                }

                //if (_isFired) {
                //    _isFired = false;
                //    _localAbilityComponent.Cast((int)EAbilityIndex.Fire, 1.0f);
                //}
                if (_inputCommand.IsCastAbility1) {
                    _localAbilityComponent.Cast((int)EAbilityIndex.MainWeapon, 1.0f);
                }
                if (_inputCommand.IsDodge) {
                    IsDodge = true;
                    _propertyComponent.SetInvincible(true);
                    GameLogicUtility.SetDodgeDirty(gameObject);
                }

                _inputCommand.Reset();

                _operate = 0;
                _operatorIndex = -1;
            }

            // 闪避处理
            if (IsDodge) {
                _dodgeTime += dt;
                if (_dodgeTime >= _maxDodgeTime) {
                    _dodgeTime = 0;
                    IsDodge = false;
                    _propertyComponent.SetInvincible(false);
                    GameLogicUtility.SetDodgeDirty(gameObject);
                }
            }
        }

        private void OnPlayerDie(GameObject self, GameObject murderer) {
            if (World.TryGetWorld<ServerSpaceWarWorld>(out ServerSpaceWarWorld world)) {
                if (world.TryGetManager<ServerFrameDataManager>(out ServerFrameDataManager frameDataManager)) {
                    // 这个方法会先调用，然后才更新玩家数组，所以如果只剩下一个玩家且当前玩家死亡了，则说明玩家死光了
                    if (frameDataManager.GetPlayers().Count <= 1) {
                        world.GameResult = false;
                        world.SetSystemPropertyDirty(EUpdateActorType.MissionResult);
                    }
                }
            }
        }

    }
}
