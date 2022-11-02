using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EDamageReason {
        None,
        BulletDamage,
        ImpactDamage,
        SystemKill
    }

    public class DamageInfo : IPool {
        public int Damage = 0;
        public EDamageReason DamageReason = EDamageReason.None;
        public GameObject Caster = null;
        public GameObject Murderer = null;

        public void Reset() {
            Damage = 0;
            DamageReason = 0;
            Caster = null;
            Murderer = null;
        }
    }


    public interface IDamage {
        void TakeDamage(DamageInfo damageInfo);
    }

    public class ServerBaseController : PlayerController, IDamage, IMover {
        // 功能
        protected MovementComponent _movementComponent = null;
        protected LocalAbilityComponent _localAbilityComponent = null;
        protected PropertyComponent _propertyComponent = null;
        protected Collider2D _collider2D = null;
        protected List<Collider> _colliders = new List<Collider>(4);

        // 其它
        protected float _deadTime = 0;
        protected const float MAX_DEAD_TIME = 0.0f;  // 0.5f;

        protected float _spawnTime = 0;
        protected const float MAX_SPAWNING_TIME = 0.5f;

        public virtual void Awake() {
            Collider[] colliders = GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++) {
                _colliders.Add(colliders[i]);
            }

            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
            _netSyncComponent = GetComponent<NetSyncComponent>();
            _propertyComponent = GetComponent<PropertyComponent>();

            _movementComponent.Mover = this;
        }

        public override void Init() {
            base.Init();

            // 初始化
            _propertyComponent.InitProperty(_netSyncComponent.ConfigID);
            _movementComponent.Speed = _propertyComponent.GetSpeed();
            EnableColliders(true);

            _deadTime = 0;
            _spawnTime = 0;
        }


        protected void EnableColliders(bool enable) {
            for (int i = 0; i < _colliders.Count; i++) {
                _colliders[i].enabled = enable;
            }
        }

        public void TakeDamage(DamageInfo damageInfo) {
            // 进行伤害
            _propertyComponent.ModifyHealth(-damageInfo.Damage);

            // 判断是否死亡
            if (_propertyComponent.IsDeading) {
                EnableColliders(false);
                OnDie?.Invoke(gameObject, damageInfo.Murderer);
            }
            if (_propertyComponent.IsDead) {
                EnableColliders(false);
                _netSyncComponent.Kill(EDestroyActorReason.BeHit);
                OnDie?.Invoke(gameObject, damageInfo.Murderer);
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

        protected virtual int GetMoveRayCastLayerMask() {
            // todo 返回的layermask可以提前算好
            if ((int)ESpaceWarLayers.PLAYER == gameObject.layer)
                return (1 << (int)ESpaceWarLayers.ENEMY) | (1 << (int)ESpaceWarLayers.BULLET) | (1 << (int)ESpaceWarLayers.PICKER);
            else if ((int)ESpaceWarLayers.ENEMY == gameObject.layer)
                return (1 << (int)ESpaceWarLayers.PLAYER) | (1 << (int)ESpaceWarLayers.BULLET) | (1 << (int)ESpaceWarLayers.PICKER);
            else
                return 0;
        }

        public virtual RaycastHit2D MoveRayCast(float distance, Vector3 direction) {
            return Physics2D.BoxCast(transform.position, GetSize(), 0, direction, distance, GetMoveRayCastLayerMask());
        }

        public virtual Vector3 GetSize() {
            if (null == _collider2D) {
                if (gameObject.TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCollider2D)) {
                    _collider2D = boxCollider2D;
                    return boxCollider2D.size;
                }
            }
            return (_collider2D as BoxCollider2D).size;
        }

        public void OnHitSomething(RaycastHit2D raycastHit) {
            CollisionProcessManager collisionProcessManager = World.GetWorld().GetManager<CollisionProcessManager>();
            Debug.Assert(null != collisionProcessManager, "Collision Process Manager is invalid");
            collisionProcessManager.AddCollisionResult(raycastHit, gameObject, (ESpaceWarLayers)gameObject.layer);
        }

#if UNITY_EDITOR

        public bool EnableDebugDraw = true;

        public void OnDrawGizmos() {
            if (EnableDebugDraw) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, GetSize());
            }
        }

#endif

    }
}
