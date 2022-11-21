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

    [SerializeField]
    public class MoveToFixLocation : BaseBehaviour {
        public int FixLocationIndex = 0;
        private Vector3 _fixLocation = Vector3.zero;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);

            LevelManager levelManager = World.GetWorld().GetManager<LevelManager>();
            if (null != levelManager) {
                if (!levelManager.TryGetFixLocation(FixLocationIndex, out _fixLocation)) {
                    Debug.LogError("FixLocation is invalid " + FixLocationIndex);
                }
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                if (Vector3.Distance(gameObject.transform.position, _fixLocation) <= movementComponent.Speed * dt) {
                    gameObject.transform.position = _fixLocation;
                    GameLogicUtility.SetPositionDirty(gameObject);
                    return EBehavourExecuteResult.Done;
                }
                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt, 
                    (_fixLocation - gameObject.transform.position).normalized, false);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }

    [SerializeField]
    public class RandomMove : BaseBehaviour {

        public string BBKey = string.Empty;
        public bool Loop = false;

        public override void Initialize(IAIBehaviour behaviour) {
            base.Initialize(behaviour);
            if (behaviour.GetOwner().TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                behaviour.GetBlackboard().SetValue<Vector3>(BBKey, baseEnemy.GetRandomLocation());
            }
        }

        public override EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            if (!behaviour.GetBlackboard().TryGetValue<Vector3>(BBKey, out Vector3 target)) {
                return EBehavourExecuteResult.Error;
            }

            GameObject gameObject = behaviour.GetOwner();
            if (gameObject.TryGetComponent<MovementComponent>(out MovementComponent movementComponent)) {
                Debug.DrawLine(gameObject.transform.position, target);
                if (Vector3.Distance(gameObject.transform.position, target) <= movementComponent.Speed * dt) {
                    gameObject.transform.position = target;
                    GameLogicUtility.SetPositionDirty(gameObject);
                    if (Loop) {
                        if (gameObject.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                            target = baseEnemy.GetRandomLocation();
                            behaviour.GetBlackboard().SetValue<Vector3>(BBKey, baseEnemy.GetRandomLocation());
                        }
                    }
                    else {
                        return EBehavourExecuteResult.Done;
                    }
                }
                gameObject.transform.position = movementComponent.MoveTick(gameObject.transform.position, dt, 
                    (target - gameObject.transform.position).normalized, true);
                GameLogicUtility.SetPositionDirty(gameObject);
                return EBehavourExecuteResult.Wait;
            }
            return EBehavourExecuteResult.Error;
        }
    }
}
