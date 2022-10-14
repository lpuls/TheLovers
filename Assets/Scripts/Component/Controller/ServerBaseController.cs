using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public interface IDamage {
        void OnHit(GameObject hitObject, GameObject hitTrajectory);
    }

    public class ServerBaseController : PlayerController, IDamage {
        // 功能
        protected MovementComponent _movementComponent = null;
        protected LocalAbilityComponent _localAbilityComponent = null;
        protected PropertyComponent _propertyComponent = null;
        protected List<Collider> _colliders = new List<Collider>(4);

        // 其它
        protected float _deadTime = 0;
        protected const float MAX_DEAD_TIME = 0.0f;  // 0.5f;

        protected float _spawnTime = 0;
        protected const float MAX_SPAWNING_TIME = 0.5f;

        public void Awake() {
            Collider[] colliders = GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++) {
                _colliders.Add(colliders[i]);
            }

            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
            _netSyncComponent = GetComponent<NetSyncComponent>();
            _propertyComponent = GetComponent<PropertyComponent>();
        }

        public override void Init() {
            base.Init();

            // 初始化
            _propertyComponent.InitProperty(_netSyncComponent.ConfigID);
            _movementComponent.Speed = _propertyComponent.GetSpeed();
            EnableColliders(true);
        }


        protected void EnableColliders(bool enable) {
            for (int i = 0; i < _colliders.Count; i++) {
                _colliders[i].enabled = enable;
            }
        }

        public void OnHit(GameObject hitObject, GameObject hitTrajectory) {
            int damage = 1;
            if (hitTrajectory.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(netSyncComponent.ConfigID, out Config.Abilitys abilitys)) {
                    damage = abilitys.Damage;
                }
            }

            // 进行伤害
            _propertyComponent.ModifyHealth(-damage);
            // Debug.Log("Damage " + gameObject.name + " HP " + _propertyComponent.GetHealth());

            // 判断是否死亡
            if (_propertyComponent.IsDeading) {
                EnableColliders(false);
                OnDie?.Invoke(gameObject, hitObject);
            }
        }


        public virtual void OnDeading(float dt) {
            _deadTime += dt;
            if (_deadTime >= MAX_DEAD_TIME) {
                _netSyncComponent.Kill(EDestroyActorReason.BeHit);
                _propertyComponent.SetDead();
            }
        }

        public virtual void OnSpawning(float dt) {
            _spawnTime += dt;
            if (_spawnTime >= MAX_SPAWNING_TIME) {
                _propertyComponent.SetAlive();
            }
        } 

        public virtual void OnAlive(float dt) {
        }

        public override void Tick(float dt) {
            // 角色死亡
            if (_propertyComponent.IsDeading) {
                OnDeading(dt);
            }
            else if (_propertyComponent.IsSpawning) {
                OnSpawning(dt);
            }
            else if (_propertyComponent.IsAlive) {
                // base.Tick(dt);
                OnAlive(dt);
            }
        }

    }
}
