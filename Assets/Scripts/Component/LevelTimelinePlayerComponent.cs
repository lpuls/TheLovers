using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hamster.SpaceWar {

    [System.Serializable]
    public class LevelTimeline {
        public int ID = 0;
        public PlayableDirector Timeline = null;
        public bool StopOnNext = false;
    }

    public class LevelTimelinePlayerComponent : MonoBehaviour {
        private int _currentID = -1;
        [SerializeField] private List<LevelTimeline> _timelines = new();

        public void AddTimeline(int id, PlayableDirector timleine) {
            foreach (var item in _timelines) {
                if (item.ID == id) {
                    Debug.LogError("Key repeate");
                    return;
                }
            }
            _timelines.Add(new LevelTimeline { 
                ID = id,
                Timeline = timleine
            });
        }

        public void PlayTimeline(int id) {
            foreach (var item in _timelines) {
                if (item.ID == _currentID) {
                    if (item.StopOnNext) {
                        item.Timeline.Stop();
                        item.Timeline.stopped -= OnStop;
                        item.Timeline.gameObject.SetActive(false);
                    }
                    _currentID = -1;
                    continue;
                }
                if (item.ID == id) {
                    _currentID = id;
                    item.Timeline.gameObject.SetActive(true);
                    item.Timeline.Play();
                    item.Timeline.stopped += OnStop;
                    continue;
                }
            }
        }

        public void StopTimeline() {
            foreach (var item in _timelines) {
                if (item.ID == _currentID) {
                    item.Timeline.Stop();
                    item.Timeline.gameObject.SetActive(false);
                    item.Timeline.stopped -= OnStop;
                    break;
                }
            }
            _currentID = -1;
        }

        private void OnStop(PlayableDirector timeline) {
            timeline.gameObject.SetActive(false);
            timeline.stopped -= OnStop;
            _currentID = -1;
        }

    }

}
