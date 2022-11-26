using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EEnemyType {
        Normal,
        Boss
    }

    public class BaseEnemy : ServerBaseController {

        private BoxCollider2D _collider = null;
        private AIBehaviourRunner AIBehaviour = new();
        
        public EEnemyType EnemyType = EEnemyType.Normal;
        public AIBehaviourScript AIBehaviourScript = null;
        public List<Vector3> MovePath = null;

        public override void Awake() {
            base.Awake();

            _collider = GetComponent<BoxCollider2D>();

            OnDie += OnDieSpawnItem;

            //AIBehaviourScript = Asset.Load<AIBehaviourScript>("Res/ScriptObjects/AI/PathAI");
            
        }

        public void SetAIBehaviourScriptAndRun(string path) {
            AIBehaviourScript = Asset.Load<AIBehaviourScript>(path);
            if (null != AIBehaviourScript) {
                AIBehaviour.Initialize(AIBehaviourScript, gameObject);
                AIBehaviour.Run();
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();

            OnDie -= OnDieSpawnItem;
        }

        protected virtual void OnDieSpawnItem(GameObject deceased, GameObject killer) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.UnitConfig>(_netSyncComponent.ConfigID, out Config.UnitConfig config)) {
                for (int i = 0; i < config.Drops.Count; i++) {
                    if (Random.Range(0, 100.0f) <= config.DropProbability[i]) {
                        GameLogicUtility.ServerCreatePickerItem(config.Drops[i], transform.position);
                        break;
                    }
                }
            }
        }

        public void ForceKill() {
            DamageInfo damageInfo = ObjectPool<DamageInfo>.Malloc();
            damageInfo.Caster = null;
            damageInfo.Murderer = null;
            damageInfo.Damage = 1000;
            damageInfo.DamageReason = EDamageReason.SystemKill;

            TakeDamage(damageInfo);
        }

        public override void Tick(float dt) {
            base.Tick(dt);

            // 更新武器
            _localAbilityComponent.Tick(dt);

            // 更新AI行为树
            if (AIBehaviour.IsRun)
                AIBehaviour.Execute(dt);
        }

        public Vector3 GetRandomLocation() {
            BaseSpaceWarWorld world = World.GetWorld<BaseSpaceWarWorld>();
            Debug.Assert(null != world, "AIEnemey World is invalid");
            if (null != _movementComponent)
                return world.GetRandomEnemtyMoveTarget(_collider.size / 2);
            return Vector3.zero;
        }
    }
}