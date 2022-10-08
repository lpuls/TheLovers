using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class BaseEnemy : BaseController {

        protected MovementComponent _movementComponent = null;
        protected LocalAbilityComponent _localAbilityComponent = null;

        public override void Init() {
            base.Init();
            _movementComponent = GetComponent<MovementComponent>();
            _localAbilityComponent = GetComponent<LocalAbilityComponent>();
        }

        public override void Tick(float dt) {
            base.Tick(dt);

        }

    }
}