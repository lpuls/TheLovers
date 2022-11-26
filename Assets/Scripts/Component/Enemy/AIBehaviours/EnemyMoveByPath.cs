using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class EnemyMoveByPath : BaseBehaviour {
        public string BBKey = string.Empty;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            if (behaviour.GetOwner().TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                behaviour.GetBlackboard().SetValue<int>(BBKey, 0);
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<int>(BBKey, out int index)) {
                return EBehavourExecuteResult.Error;
            }

            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                if (!gameObject.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                    return EBehavourExecuteResult.Error;
                }
                if (null == baseEnemy.MovePath || index >= baseEnemy.MovePath.Count) {
                    return EBehavourExecuteResult.Done;
                }
                Vector3 target = baseEnemy.MovePath[index];
                if (Vector3.Distance(gameObject.transform.position, target) <= movementComponent.Speed * dt) {
                    index++;
                    gameObject.transform.position = target;
                    GameLogicUtility.SetPositionDirty(gameObject);
                    if (index >= baseEnemy.MovePath.Count) {
                        baseEnemy.ForceKill();
                        return EBehavourExecuteResult.Done;
                    }
                    else {
                        behaviour.GetBlackboard().SetValue<int>(BBKey, index);
                    }
                }
                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt,
                    (target - gameObject.transform.position).normalized, false);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }
}
