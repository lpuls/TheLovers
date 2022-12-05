using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class MoveDirection : BaseBehaviour {

        public Vector3 Direction = Vector3.left;
        public string BBKey = string.Empty;

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<bool>(BBKey, out bool notInWorld)) {
                notInWorld = true;
                behaviour.GetBlackboard().SetValue<bool>(BBKey, true);
            }

            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                // todo 这边其实应该设置左右死线的，偷个懒
                bool inWorld = World.GetWorld<BaseSpaceWarWorld>().InWorld(gameObject.transform.position - Direction * movementComponent.Speed);
                if (!inWorld) {
                    if (!notInWorld) {
                        if (gameObject.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                            baseEnemy.ForceKill();
                        }
                        return EBehavourExecuteResult.Done;
                    }
                }
                else {
                    notInWorld = false;
                    behaviour.GetBlackboard().SetValue<bool>(BBKey, notInWorld);
                }

                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt,
                    Direction, false);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }
}
