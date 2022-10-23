using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class PickerItem : BaseController {

        [SerializeField] private float _moveSpeed = 10;
        [SerializeField] private float _itemSize = 1.0f;
        private Vector3 _moveDirection = Vector3.zero;

        public override void OnEnable() {
            base.OnEnable();
            _moveDirection = GetRandomDirection();
        }

        public override void Tick(float dt) {
            base.Tick(dt);

            float moveDistance = dt * _moveSpeed;
            if (World.GetWorld<BaseSpaceWarWorld>().InWorld(transform.position, _moveDirection * moveDistance, out moveDistance)) {

                RaycastHit2D hitResult = Physics2D.BoxCast(transform.position, new Vector2(_itemSize, _itemSize), 0, _moveDirection, moveDistance, 1 << (int)ESpaceWarLayers.PLAYER);
                if (null != hitResult.collider && hitResult.collider.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController)) {
                    OnPicker(playerController);
                    AssetPool.Free(gameObject);
                }

                if (moveDistance <= _itemSize) {
                    _moveDirection = GetRandomDirection();
                }
            }

        }

        protected static Vector3 GetRandomDirection() {
            float x = Random.Range(0, 1.0f);
            float y = Random.Range(0, 1.0f);
            return (new Vector3(x, y)).normalized;
        } 

        protected virtual void OnPicker(PlayerController playerController) {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _itemSize);
        }
#endif

    }
}