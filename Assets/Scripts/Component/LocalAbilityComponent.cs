using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EAbilityIndex {
        Fire = 0,
        Ultimate = 1
    }

    public class LocalAbilityComponent : MonoBehaviour, ITrajectorySpanwer {
        // todo 之后改成配置表
        public string[] AbilityPrefabs = null;
        public Vector3 SpawnOffset = Vector3.zero;

        public float[] _abilityCDs = null;

        public void Awake() {
            _abilityCDs = new float[AbilityPrefabs.Length];
            for (int i = 0; i < AbilityPrefabs.Length; i++) {
                _abilityCDs[i] = 0;
            }
        }

        public void CastAbility(int index) {
            if (null == AbilityPrefabs || index < 0 || index >= AbilityPrefabs.Length) {
                Debug.LogError("Cast Ability Filed " + index);
                return;
            }

            if (_abilityCDs[index] > 0)
                return;

            GameObject ability = Asset.Load(AbilityPrefabs[index]);
            TrajectoryComponent trajectoryComponent = ability.TryGetOrAdd<TrajectoryComponent>();
            trajectoryComponent.Init(this);

            // todo 之后读配置表
            _abilityCDs[index] = 0.1f;
        }

        private void Update() {
            for (int i = 0; i < _abilityCDs.Length; i++) {
                _abilityCDs[i] -= Time.deltaTime;
                if (_abilityCDs[i] <= 0)
                    _abilityCDs[i] = 0;
            }
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public int GetLayer() {
            return gameObject.layer;
        }

        public Vector3 GetPosition() {
            return transform.position + SpawnOffset;
        }

        public void OnHitDestroy(GameObject trajectory) {
        }

        public void OnHitObject(GameObject hitObject, GameObject trajectory) {
        }

        public void OnOutOfWold(GameObject trajectory) {
        }


#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + SpawnOffset, 0.3f);
        }
#endif
    }
}
