using UnityEngine;

namespace Hamster.SpaceWar {
    public class PlayerEffectComponent : MonoBehaviour {
        [SerializeField]
        private Transform[] _tailFlame = null;
        [SerializeField]
        private float _accelerateTailFlame = 4;
        [SerializeField]
        private float _normalTailFlame = 2;
        [SerializeField]
        private float _slowDownTailFlame = 1;

        [SerializeField]
        private float _moveLeftVelocity = -1;
        [SerializeField]
        private float _normalVelocity = 0;
        [SerializeField]
        private float _moverightVelocity = 1;

        private Animator _animator = null;
        private SimulateComponent _simulateComponent = null;
        private NetSyncComponent _netSyncComponent = null;

        private float _velocityX = 0;
        private float _tailFlameSize = 2;

        private void Awake() {
            _simulateComponent = GetComponent<SimulateComponent>();
            _animator = GetComponentInChildren<Animator>();
            _netSyncComponent = GetComponent<NetSyncComponent>();

            _velocityX = _normalTailFlame;
            _tailFlameSize = _normalTailFlame;
        }

        protected virtual void OnFrameUpdate(FrameData pre, FrameData current) {
            int netID = _netSyncComponent.NetID;
            UpdateInfo updateInfo;
            if (null != pre && pre.TryGetUpdateInfo(netID, EUpdateActorType.RoleState, out updateInfo)) {
                switch ((EPlayerState)updateInfo.Data1.Int32) {
                    case EPlayerState.Alive:
                        break;
                    case EPlayerState.Deading:
                        // _animator.SetTrigger("Dead");
                        break;
                    case EPlayerState.Dead:
                        GameObject deadEffect = Asset.Load("Res/VFX/DeadBoom");
                        deadEffect.transform.position = transform.position;
                        break;
                }
            }
        }

        public void Update() {
            // 根据前后帧的位置来计算动画表现
            SimulateComponent simulateComponent = GetSimulate();
            if (null != simulateComponent && null != _animator) {
                Vector3 delta = _simulateComponent.CurrentLocation - transform.position;

                // 左右移动时对机体进行左右旋转
                if (delta.x > 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moverightVelocity, 0.1f);
                else if (delta.x < 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moveLeftVelocity, 0.1f);
                else
                    _velocityX = Mathf.MoveTowards(_velocityX, _normalVelocity, 0.1f);
                _animator.SetFloat("VelocityX", _velocityX);

                // 前后加速时对尾焰大小进行修改
                if (delta.z > 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _accelerateTailFlame, 0.1f);
                else if (delta.z < 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _slowDownTailFlame, 0.1f);
                else
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _normalTailFlame, 0.1f);
                SetTailFlameSize(_tailFlameSize);
            }
        }

        private void SetTailFlameSize(float size) {
            if (null == _tailFlame)
                return;

            for (int i = 0; i < _tailFlame.Length; i++) {
                _tailFlame[i].localScale = new Vector3(size, size, size);
            }
        }

        private SimulateComponent GetSimulate() {
            if (null == _simulateComponent) {
                _simulateComponent = GetComponent<SimulateComponent>();
            }
            return _simulateComponent;
        }

        private void OnEnable() {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate += OnFrameUpdate;
            }
        }

        private void OnDisable() {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate -= OnFrameUpdate;
            }
        }
    }
}
