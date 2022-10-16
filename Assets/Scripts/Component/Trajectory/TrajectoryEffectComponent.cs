using UnityEngine;

namespace Hamster.SpaceWar {
    public class TrajectoryEffectComponent : MonoBehaviour {
        public AudioSource AudioPlayer = null;
        public TrailRenderer Trail = null;

        private NetSyncComponent _netSyncComponent = null;


        public void OnEnable() {
            if (null != AudioPlayer)
                AudioPlayer.Play();
            EnableTrail(false);

            if (null == _netSyncComponent)
                _netSyncComponent = GetComponent<NetSyncComponent>();
            if (null != _netSyncComponent)
                _netSyncComponent.OnKill += OnKill;
        }

        public void OnDisable() {
            EnableTrail(false);

            if (null != _netSyncComponent)
                _netSyncComponent.OnKill -= OnKill;
        }

        public void OnKill(EDestroyActorReason reason) {
            if (EDestroyActorReason.HitOther == reason) {
                GameObject vfx = Asset.Load("Res/VFX/HitSparkExplosion");
                vfx.transform.position = transform.position;
            }
        }

        public void EnableTrail(bool enable) {
            if (null != Trail) {
                Trail.Clear();
                Trail.enabled = enable;
            }
        }

    }
}
