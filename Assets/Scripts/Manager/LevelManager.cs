using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class LevelManager : MonoBehaviour, IServerTicker, ILevelManager {

        public bool EnableSpawn = true;
        public bool IsServerManager = true;
        private List<BaseEnemy> _aliveEnemys = new();

        private float _time = 0.0f;
        private int _eventIndex = 0;
        private LevelConfigScriptObject _levelConfig = null;

        public void Initilze(string configPath) {
            _levelConfig = Asset.Load<LevelConfigScriptObject>(configPath);
            _time = 0;
            _eventIndex = -1;
            EnterNextWave();
        }

        private void CheckNextWave() {
            if (!IsServer())
                return;

            LevelEventScriptObject currentEvent = null;
            if (_eventIndex >= 0 && _eventIndex < _levelConfig.LevelWaves.Count)
                currentEvent = _levelConfig.LevelWaves[_eventIndex];

            // 检查是否有进入下一波的条件
            bool enterNext = false;
            if (currentEvent.IsComplete(this)) {
                enterNext = true;
            }

            // 进入下一波敌人
            if (enterNext) {
                EnterNextWave();
            }
        }

        private void EnterNextWave() {
            if (!IsServer())
                return;

            int nextWaveIndex = _eventIndex + 1;
            LevelEventScriptObject nextWave = null;
            if (nextWaveIndex >= 0 && nextWaveIndex < _levelConfig.LevelWaves.Count)
                nextWave = _levelConfig.LevelWaves[nextWaveIndex];

            Debug.Assert(null != nextWave, "Next wave is null");
            if (null != nextWave) {
                // 判断是否到达下一波的条件，如果是则生成下一波的敌人
                nextWave.OnEnter(this);

                // 更新数据
                _time = 0;
                _eventIndex = nextWaveIndex;
                World.GetWorld<ServerSpaceWarWorld>().SetSystemPropertyDirty(EUpdateActorType.LevelEventIndex);
            }
            else if (nextWaveIndex >= _levelConfig.LevelWaves.Count) {
                // TODO 通过关卡结束
            }
        }

        public void SetLevelEventIndex(int index) {
            if (IsServer())
                return;

            // 退出上一个事件
            if (_eventIndex >= 0 && _eventIndex < _levelConfig.LevelWaves.Count) {
                LevelEventScriptObject currentWave = _levelConfig.LevelWaves[_eventIndex];
                if (null != currentWave) {
                    currentWave.OnLevel(this);
                }
            }

            // 进入新的事件
            if (index >= 0 && index < _levelConfig.LevelWaves.Count) {
                LevelEventScriptObject nextWave = _levelConfig.LevelWaves[index];
                if (null != nextWave) {
                    _time = 0;
                    _eventIndex = index;
                    nextWave.OnEnter(this);
                }
            }
        }

        private void OnEnemyDie(GameObject deceased, GameObject killer) {
            if (deceased.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                baseEnemy.OnDie -= OnEnemyDie;
                _aliveEnemys.Remove(baseEnemy);
                if (_aliveEnemys.Count <= 0) {
                    CheckNextWave();
                }
            }
        }

        public bool TryGetFixLocation(int index, out Vector3 location) {
            location = Vector3.zero;
            if (null == _levelConfig)
                return false;
            Debug.Assert(index >= 0 && index < _levelConfig.FixLocations.Count, "Get Fix Location Index out of range");
            location = _levelConfig.FixLocations[index];
            return true;
        }

        public int GetPriority() {
            return (int)EServerTickLayers.PreTick;
        }

        public void Tick(float dt) {
            if (!EnableSpawn)
                return;

            _time += dt;
            if (IsServer()) {
                CheckNextWave();
            }
            else {
                
            }
        }

        public bool IsEnable() {
            return true;
        }

        public bool IsServer() {
            return IsServerManager;
        }

        public float GetTime() {
            return _time;
        }

        public int GetEnemeyCount() {
            return _aliveEnemys.Count;
        }

        public void SpawnUnit(int id, int locationIndex) {
            GameObject ship = GameLogicUtility.ServerCreateEnemy(id, _levelConfig.FixLocations[locationIndex], 180);
            if (ship.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                baseEnemy.UnitType = ESpaceWarUnitType.Enemy;
                baseEnemy.OnDie += OnEnemyDie;
                _aliveEnemys.Add(baseEnemy);
                GameLogicUtility.SetPositionDirty(ship);
            }
            else {
                AssetPool.Free(ship);
            }
        }

        public void DestroyAllUnit() {
            DamageInfo damageInfo = ObjectPool<DamageInfo>.Malloc();
            damageInfo.Caster = null;
            damageInfo.Murderer = null;
            damageInfo.Damage = 1000;
            damageInfo.DamageReason = EDamageReason.SystemKill;

            List<BaseEnemy> baseEnemies = ListPool<BaseEnemy>.Malloc();
            baseEnemies.AddRange(_aliveEnemys);
            foreach (var item in baseEnemies) {
                item.TakeDamage(damageInfo);
            }
            ListPool<BaseEnemy>.Free(baseEnemies);

            ObjectPool<DamageInfo>.Free(damageInfo);
        }

        public int GetCurrentLevelEventIndex() {
            return _eventIndex;
        }
    }
}