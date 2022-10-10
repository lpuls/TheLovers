using UnityEngine;

namespace Hamster.SpaceWar {
    public class TrajectoryEffectComponent : MonoBehaviour {
        public AudioSource AudioPlayer = null;
        public TrailRenderer Trail = null;

        public void OnEnable() {
            if (null != AudioPlayer)
                AudioPlayer.Play();
            EnableTrail(false);
        }

        public void OnDisable() {
            EnableTrail(false);
        }

        public void EnableTrail(bool enable) {
            if (null != Trail) {
                Trail.Clear();
                Trail.enabled = enable;
            }
        }

    }
}
