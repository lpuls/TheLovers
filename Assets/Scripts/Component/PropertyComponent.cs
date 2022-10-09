using System;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EPlayerState {
        Alive,
        Deading,
        Dead
    }

    public class PropertyComponent : MonoBehaviour {
        protected ModifyProperty<int> _health = new ModifyProperty<int>();
        protected ModifyProperty<float> _speed = new ModifyProperty<float>();

        public bool IsDeading {
            get { return State == EPlayerState.Deading; }
        }

        public bool IsDead {
            get { return State == EPlayerState.Dead; }
        }

        public bool IsAlive {
            get { return State == EPlayerState.Alive; }
        }

        public EPlayerState State {
            get;
            private set;
        }

        public void InitProperty(int configID) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig config)) {
                _health.Init(config.Health, config.Health);
                _speed.Init(config.Speed, config.Speed);
            }
            else {
                _health.Init(1, 1);
                _speed.Init(1, 1);
            }
            State = EPlayerState.Alive;
        }

        public void ModifyHealth(int delta) {
            int health = _health.GetValue() + delta;
            health = Mathf.Clamp(health, 0, _health.GetMaxValue());
            _health.SetValue(health);

            // 死亡回调
            if (health <= 0) {
                State = EPlayerState.Deading;
            }
        }

        public int GetHealth() {
            return (int)(_health.GetValue() * _health.GetMagnification());
        }

        public void SetDead() {
            State = EPlayerState.Dead;
        }

        public float GetSpeed() {
            return _speed.GetValue();
        }

    }
}
