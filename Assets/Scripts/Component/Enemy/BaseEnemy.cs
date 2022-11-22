﻿using System.Collections;
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

        public override void Awake() {
            base.Awake();

            _collider = GetComponent<BoxCollider2D>();

            OnDie += OnDieSpawnItem;

            AIBehaviourScript = Asset.Load<AIBehaviourScript>("Res/ScriptObjects/AI/BaseAI");
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