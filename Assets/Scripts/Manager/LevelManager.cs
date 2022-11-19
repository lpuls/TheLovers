using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class LevelManager : MonoBehaviour, IServerTicker {

        public bool EnableSpawn = true;
        private List<BaseEnemy> _aliveEnemys = new List<BaseEnemy>();

        private float _progress = 0.0f;
        private int _waveIndex = 0;
        private LevelConfigScriptObject _levelConfig = null;

        public void Initilze(string configPath) {
            _levelConfig = Asset.Load<LevelConfigScriptObject>(configPath);
            _progress = 0;
            _waveIndex = -1;
            CheckNextWave();
        }

        private void CheckNextWave() {
            LevelWaveScriptObject currentWave = null;
            LevelWaveScriptObject nextWave = null;
            if (_waveIndex >= 0 && _waveIndex < _levelConfig.LevelWaves.Count)
                currentWave = _levelConfig.LevelWaves[_waveIndex];

            int nextWaveIndex = _waveIndex + 1;
            if (nextWaveIndex >= 0 && nextWaveIndex < _levelConfig.LevelWaves.Count)
                nextWave = _levelConfig.LevelWaves[nextWaveIndex];

            // 到达触发条件了，判断是否
            if (null != currentWave) {
                if (LevelWaveScriptObject.ELevelWaveCompleteType.WaitAllDie == currentWave.CompleteType && _aliveEnemys.Count > 0) {
                    if (null != nextWave) {
                        _progress = nextWave.TriggerTime;
                    }
                    return;
                }
            }

            // 判断是否到达下一波的条件，如果是则生成下一波的敌人
            float t = _progress / _levelConfig.LevelTime;
            if (null != nextWave) {
                if (t >= nextWave.TriggerTime) {
                    SpawnUnits(nextWave);
                    _waveIndex = nextWaveIndex;
                }
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

        public int GetPriority() {
            return (int)EServerTickLayers.PreTick;
        }

        public void Tick(float dt) {
            if (!EnableSpawn)
                return;

            _progress += dt;
            CheckNextWave();
        }

        public bool IsEnable() {
            return true;
        }
    }
}