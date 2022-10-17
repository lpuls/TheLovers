﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class EnemyManager : MonoBehaviour {

        public int SpawnMinCount = 3;
        public int SpawnMaxCount = 6;

        public float SpawnDelay = 2.0f;

        private float _delayTime = 0;
        private List<BaseEnemy> _aliveEnemys = new List<BaseEnemy>();

        public void Update() {
            if (_aliveEnemys.Count <= 0) {
                _delayTime += Time.deltaTime;
                if (_delayTime >= SpawnDelay) {
                    SpawnEnemys();
                    _delayTime = 0;
                }
            }
        }

        private void SpawnEnemys() {
            int[] enemyIDs = { 10, 11 };
            int spawnCount = Random.Range(SpawnMinCount, SpawnMaxCount);
            for (int i = 0; i < spawnCount; i++) {
                int id = enemyIDs[Random.Range(0, enemyIDs.Length)];
                GameObject ship = GameLogicUtility.ServerCreateEnemy(id, Vector3.zero, 180);
                if (ship.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                    baseEnemy.OnDie += OnEnemyDie;
                    _aliveEnemys.Add(baseEnemy);
                    ship.transform.position = baseEnemy.GetRandomLocation();
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

    }
}