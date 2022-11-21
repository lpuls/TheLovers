using UnityEngine;

namespace Hamster.SpaceWar {
    [SerializeField]
    public class CastAbility : BaseBehaviour {
        public bool Loop = false;
        public float Interval = 1.0f;
        public EAbilityIndex AbilityIndex = EAbilityIndex.Fire;
        public string BBKey = string.Empty;


        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<float>(BBKey, out float value)) {
                value = 0.0f;
            }

            value += dt;
            if (value >= Interval) {
                value -= Interval;
                GameObject gameObject = behaviour.GetOwner();
                if (gameObject.TryGetComponent<LocalAbilityComponent>(out LocalAbilityComponent abilityComponent)) {
                    abilityComponent.Cast(AbilityIndex, 1.0f);
                }
            }
            behaviour.GetBlackboard().SetValue<float>(BBKey, value);

            return Loop ? EBehavourExecuteResult.Wait : EBehavourExecuteResult.Done;
        }
    }
}
