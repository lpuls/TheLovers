using UnityEngine;

namespace Hamster.SpaceWar {


    [SerializeField]
    public class RandomMove : BaseBehaviour {

        public string MoveTimeBBKey = string.Empty;
        public string RandomLocationBBKey = string.Empty;
        public bool Loop = false;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            if (behaviour.GetOwner().TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                behaviour.GetBlackboard().SetValue<Vector3>(RandomLocationBBKey, baseEnemy.GetRandomLocation());
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<Vector3>(RandomLocationBBKey, out Vector3 target)) {
                return EBehavourExecuteResult.Error;
            }
            if (!behaviour.GetBlackboard().TryGetValue<float>(MoveTimeBBKey, out float moveTime)) {
                moveTime = 0;
            }

            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                if (Vector3.Distance(gameObject.transform.position, target) <= movementComponent.Speed * dt || moveTime >= 3.0f) {
                    gameObject.transform.position = target;
                    GameLogicUtility.SetPositionDirty(gameObject);
                    if (Loop) {
                        if (gameObject.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                            target = baseEnemy.GetRandomLocation();
                            behaviour.GetBlackboard().SetValue<Vector3>(RandomLocationBBKey, baseEnemy.GetRandomLocation());
                        }
                    }
                    else {
                        return EBehavourExecuteResult.Done;
                    }
                    moveTime = 0;
                }
                moveTime += dt;
                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt,
                    (target - gameObject.transform.position).normalized, true);
                behaviour.GetBlackboard().SetValue<float>(MoveTimeBBKey, moveTime);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }
}
