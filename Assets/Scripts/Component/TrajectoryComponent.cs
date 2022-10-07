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

        int GetLayer();

        GameObject GetGameObject();
    }

    public class TrajectoryComponent : MonoBehaviour {
        protected ITrajectorySpanwer _parent = null;
        protected bool _isPlayer = false;
        protected bool _pendingKill = false;
        protected Vector3 _moveDirection = Vector3.zero;
        protected float _moveSpeed = 100.0f;

        [SerializeField] protected bool _hitOnDestroy = true;

        public virtual void Init(ITrajectorySpanwer parent, Vector3 moveDirection, float moveSpeed) {
            _parent = parent;
            _moveDirection = moveDirection;
            _moveSpeed = moveSpeed;

            // 修改朝向
            transform.rotation = Quaternion.Euler(moveDirection);

            // 确定是玩家还是敌人的子弹
            _isPlayer = CheckLayerValue(parent.GetLayer(), ESpaceWarLayers.PLAYER);
        }

        public virtual void Update() {
            Move();

            if (!World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position) && null != _parent) {
                _parent.OnOutOfWold(gameObject);
            }
        }

        public virtual void Move() {
            transform.position += _moveDirection * Time.deltaTime * _moveSpeed;
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
                    damage.OnHit(_parent.GetGameObject(), gameObject);
                    if (null != _parent)
                        _parent.OnHitObject(collider, gameObject);
                }
            }

            // 命中后是否销毁
            if (_hitOnDestroy && null != _parent) {
                _parent.OnHitDestroy(gameObject);
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
            Gizmos.DrawLine(transform.position, transform.position + _moveDirection * _moveSpeed);
        }
#endif
    }
}
