using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class BaseEnemy : ServerBaseController {
        public override void Tick(float dt) {
            base.Tick(dt);

            // 更新武器
            _localAbilityComponent.Tick(dt);
        }
    }
}