using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class BaseEnemy : ServerBaseController {
        public override void Tick(float dt) {
            base.Tick(dt);

            // 更新武器
            _localAbilityComponent.Tick(dt);
        }

        public Vector3 GetRandomLocation() {
            BaseSpaceWarWorld world = World.GetWorld<BaseSpaceWarWorld>();
            Debug.Assert(null != world, "AIEnemey World is invalid");
            if (null != _movementComponent)
                return world.GetRandomEnemtyMoveTarget(_movementComponent.HalfSize);
            return Vector3.zero;
        }
    }
}