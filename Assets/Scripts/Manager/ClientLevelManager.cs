using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ClientLevelManager : MonoBehaviour, ILevelManager {
        private float _time = 0.0f;
        private int _eventIndex = 0;
        private LevelConfigScriptObject _levelConfig = null;
        private LevelTimelinePlayerComponent _levelTimelinePlayerComponent = null;

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
            _levelTimelinePlayerComponent = GameObjectExtend.LoadAndGetComponent<LevelTimelinePlayerComponent>(_levelConfig.ClientAsset, out GameObject _);
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

                    // 执行事件内容
                    nextWave.OnEnter(this);

                    // 播放现表现
                    _levelTimelinePlayerComponent.PlayTimeline(index);
                }
            }
        }

        public void OnFrameDataUpdate(FrameData pre, FrameData current) {
            if (null != current) {
                if (current.TryGetUpdateInfo(BaseFrameDataManager.SYSTEM_NET_ACTOR_ID, EUpdateActorType.LevelEventIndex, out UpdateInfo info)) {
                    SetLevelEventIndex(info.Data1.Int32);
                }
                if (current.TryGetUpdateInfo(BaseFrameDataManager.SYSTEM_NET_ACTOR_ID, EUpdateActorType.MissionResult, out info)) {
                    // 关闭主界面UI
                    Single<UIManager>.GetInstance().Close<MainUIController>();

                    // 开打任务完成界面
                    Single<UIManager>.GetInstance().Open<MissionOverUIController>();
                    MissionOverUIModule module = Single<UIManager>.GetInstance().GetModule<MissionOverUIController>() as MissionOverUIModule;
                    module.IsComplete.SetValue(info.Data1.Boolean);

                    // 等待并切场景
                    World.GetWorld<BaseSpaceWarWorld>().GoBackToOutside();
                }
            }
        }

        
    }
}
