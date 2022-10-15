using System;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EPlayerState {
        Spawning,
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

        public bool IsSpawning {
            get {
                return State == EPlayerState.Spawning;
            }
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
            _health.OnValueChange = OnHealthChange;
            SetSpawning();
        }

        public void ModifyHealth(int delta) {
            int health = _health.GetValue() + delta;
            health = Mathf.Clamp(health, 0, _health.GetMaxValue());
            _health.SetValue(health);
        }

        public int GetHealth() {
            return (int)(_health.GetValue() * _health.GetMagnification());
        }

        public void SetAlive() {
            State = EPlayerState.Alive;
            GameLogicUtility.SetRoleState(gameObject);
        }

        public void SetDead() {
            State = EPlayerState.Dead;
            GameLogicUtility.SetRoleState(gameObject);
        }

        public void SetDeading() {
            State = EPlayerState.Deading;
            GameLogicUtility.SetRoleState(gameObject);
        }

        public void SetSpawning() {
            State = EPlayerState.Spawning;
            GameLogicUtility.SetRoleState(gameObject);
        }

        public float GetSpeed() {
            return _speed.GetValue();
        }

        private void OnHealthChange(int oldValue, int newValue) {
            if (oldValue > 0 && newValue <= 0) {
                // SetDeading();
                SetDead();
            }
        }

    }
}
