using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum ESpaceWarLayers {
        BEGIN = 6,
        PLANE = 7,
        PROPS = 8,
        PLAYER = 9,
        ENEMY = 10,
        NATURE = 11,
        END
    }

    public interface ITrajectorySpanwer {
        void OnHitObject(GameObject hitObject, GameObject trajectory);
        void OnHitDestroy(GameObject trajectory);
        void OnOutOfWold(GameObject trajectory);

        Vector3 GetPosition();
        int GetLayer();

        GameObject GetGameObject();
    }

    public class TrajectoryComponent : MonoBehaviour {
        public ITrajectorySpanwer Parent = null;
        public Vector3 SpawnOffset = Vector3.zero;
        public Vector3 MoveDirection = Vector3.zero;
        public float MoveSpeed = 100.0f;
        public float MoveDelay = 0.0f;
        public bool HitOnDestroy = true;

        protected bool _isPlayer = false;
        protected float _moveDelay = 0.0f;
        protected bool _pendingKill = false;

        public virtual void Init(ITrajectorySpanwer parent) {
            Parent = parent;
            transform.position = parent.GetPosition() + SpawnOffset;
            _moveDelay = MoveDelay;

            _isPlayer = CheckLayerValue(parent.GetLayer(), ESpaceWarLayers.PLAYER);
        }

        public virtual void Update() {
            _moveDelay -= Time.deltaTime;
            if (_moveDelay >= 0)
                return;
            _moveDelay = 0;
            Move();

            if (!World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position)) {
                Parent.OnOutOfWold(gameObject);
            }
        }

        public virtual void Move() {
            transform.position += MoveDirection * Time.deltaTime * MoveSpeed;
        }

        private void OnTriggerEnter(Collider collider) {
            GameObject colliderObject = collider.gameObject;
            OnHitSomething(colliderObject);
        }

        protected virtual void OnHitSomething(GameObject collider) {
            bool isPlane = CheckLayerValue(collider.layer, ESpaceWarLayers.PLANE);
            bool isPlayer = CheckLayerValue(collider.layer, ESpaceWarLayers.PLAYER);

            // 命中时，命中物为飞机且阵营不同
            if (isPlane && isPlayer != _isPlayer) {
                IDamage damage = collider.GetComponent<IDamage>();
                if (null != damage) {
                    damage.OnHit(Parent.GetGameObject(), gameObject);
                    Parent.OnHitObject(collider, gameObject);
                }
            }

            // 命中后是否销毁
            if (HitOnDestroy) {
                Parent.OnHitDestroy(gameObject);
            }
        }

        protected bool CheckLayerValue(int layer, ESpaceWarLayers value) {
            return 1 == (int)((layer >> (int)value) & 1);
        }

        protected int GetLayerValue(int layer, ESpaceWarLayers value) {
            return (int)((layer >> (int)value) & 1);
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + MoveDirection * MoveSpeed);
        }
#endif
    }
}
