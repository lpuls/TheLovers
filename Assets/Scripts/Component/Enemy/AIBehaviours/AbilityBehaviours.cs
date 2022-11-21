using UnityEngine;

namespace Hamster.SpaceWar {
    [SerializeField]
    public class CastAbility : BaseBehaviour {
        public bool Loop = false;
        public EAbilityIndex AbilityIndex = EAbilityIndex.Fire;

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<LocalAbilityComponent>(out LocalAbilityComponent abilityComponent)) {
                abilityComponent.Cast(AbilityIndex, 1.0f);
            }
            return Loop ? EBehavourExecuteResult.Wait : EBehavourExecuteResult.Done;
        }
    }
}
