using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class LevelManager : MonoBehaviour, IServerTicker {

        public bool EnableSpawn = true;
        private List<BaseEnemy> _aliveEnemys = new List<BaseEnemy>();

        private float _time = 0.0f;
        private int _waveIndex = 0;
        private LevelConfigScriptObject _levelConfig = null;

        public void Initilze(string configPath) {
            _levelConfig = Asset.Load<LevelConfigScriptObject>(configPath);
            _time = 0;
            _waveIndex = -1;
            EnterNextWave();
        }

        private void CheckNextWave() {
            LevelWaveScriptObject currentWave = null;
            if (_waveIndex >= 0 && _waveIndex < _levelConfig.LevelWaves.Count)
                currentWave = _levelConfig.LevelWaves[_waveIndex];

            // 检查是否有进入下一波的条件
            bool enterNext = false;
            if ((LevelWaveScriptObject.ELevelWaveCompleteType.WaitTime == currentWave.CompleteType && _time >= currentWave.Time)
                || (LevelWaveScriptObject.ELevelWaveCompleteType.WaitAllDie == currentWave.CompleteType && _aliveEnemys.Count <= 0)) {
                enterNext = true;
            }

            // 进入下一波敌人
            if (enterNext) {
                EnterNextWave();
            }
        }

        private void EnterNextWave() {
            int nextWaveIndex = _waveIndex + 1;
            LevelWaveScriptObject nextWave = null;
            if (nextWaveIndex >= 0 && nextWaveIndex < _levelConfig.LevelWaves.Count)
                nextWave = _levelConfig.LevelWaves[nextWaveIndex];

            Debug.Assert(null != nextWave, "Next wave is null");
            if (null != nextWave) {
                // 判断是否到达下一波的条件，如果是则生成下一波的敌人
                SpawnUnits(nextWave);

                // 更新数据
                _time = 0;
                _waveIndex = nextWaveIndex;
            }
            else if (nextWaveIndex >= _levelConfig.LevelWaves.Count) {
                // TODO 通过关卡结束
            }
        }

        private void SpawnUnits(LevelWaveScriptObject waveConfig) {
            // todo 之后需要考虑敌人延迟生成的问题
            foreach (var item in waveConfig.UnitSpawns) {
                GameObject ship = GameLogicUtility.ServerCreateEnemy(item.ID, _levelConfig.FixLocations[item.LocationIndex], 180);
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

        public void KillAllEnemys() {
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
            CheckNextWave();
        }

        public bool IsEnable() {
            return true;
        }
    }
}