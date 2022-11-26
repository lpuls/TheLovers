using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ClientLevelManager : MonoBehaviour, ILevelManager {
        private float _time = 0.0f;
        private int _eventIndex = 0;
        private LevelConfigScriptObject _levelConfig = null;

        public void DestroyAllUnit() {
        }

        public int GetEnemeyCount() {
            return 0;
        }

        public float GetTime() {
            return _time;
        }

        public void SpawnUnit(UnitSpawnScriptObject data) {
        }

        public int GetPendingSpawnUnitCount() {
            return 0;
        }

        public bool IsClient() {
            return true;
        }

        public void Initilze(string configPath) {
            _levelConfig = Asset.Load<LevelConfigScriptObject>(configPath);
            _time = 0;
            _eventIndex = -1;
        }

        public void SetLevelEventIndex(int index) {
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

        public void OnFrameDataUpdate(FrameData pre, FrameData current) {
            if (null != current 
                && current.TryGetUpdateInfo(BaseFrameDataManager.SYSTEM_NET_ACTOR_ID, EUpdateActorType.LevelEventIndex, out UpdateInfo info)) {
                SetLevelEventIndex(info.Data1.Int32);
            }
        }

        
    }
}
