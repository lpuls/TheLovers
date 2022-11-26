using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class MoveIntoScreen : BaseBehaviour {
        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                if (World.GetWorld<BaseSpaceWarWorld>().InWorld(gameObject.transform.position)) {
                    return EBehavourExecuteResult.Done;
                }
                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt,
                    (Vector3.zero.SetY(gameObject.transform.position.y) - gameObject.transform.position).normalized, false);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }
}
