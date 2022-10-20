using System.Collections.Generic;
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

        public Color HitAdditionColor = Color.red;
        public AnimationCurve HitColorUpdateCurve = null;
        public float UpdateColorMaxTime = 0.1f;
        private bool _neeUpdateHitColor = false;
        private float _hitColorUpdateTime = 0;
        private List<Color> _originAdditionColor = new List<Color>();
        private List<Material> _shipMaterials = new List<Material>();

        private float _velocityX = 0;
        private float _tailFlameSize = 2;

        private bool _initHealth = false;
        private int _health = 1;
        private int _maxHealth = 1;
        [SerializeField] private OverheadHealthUI _headHealthUI = null;

        private void Awake() {
            _simulateComponent = GetComponent<SimulateComponent>();
            _animator = GetComponentInChildren<Animator>();
            _netSyncComponent = GetComponent<NetSyncComponent>();

            _velocityX = _normalTailFlame;
            _tailFlameSize = _normalTailFlame;

            // 获取所有的材质
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++) {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.materials;
                for (int j = 0; j < materials.Length; j++) {
                    _shipMaterials.Add(materials[j]);
                    _originAdditionColor.Add(materials[j].GetColor("_BaseColor"));
                }
            }
        }

        protected virtual void OnFrameUpdate(FrameData pre, FrameData current) {
            int netID = _netSyncComponent.NetID;
            UpdateInfo updateInfo;
            if (null != current) {
                // 检查角色状态变化
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.RoleState, out updateInfo)) {
                    switch ((EPlayerState)updateInfo.Data1.Int8) {
                        case EPlayerState.Spawning: {
                                GameObject spawnEffect = Asset.Load("Res/VFX/ShipSpawn");
                                spawnEffect.transform.position = transform.position;
                                spawnEffect.transform.forward = transform.forward;
                            }
                            break;
                        case EPlayerState.Alive:
                            break;
                        case EPlayerState.Deading: {
                                GameObject deadEffect = Asset.Load("Res/VFX/DeadBoom");
                                deadEffect.transform.position = transform.position;
                            }
                            break;
                        case EPlayerState.Dead: {
                                _animator.SetTrigger("Dead");
                                GameObject deadEffect = Asset.Load("Res/VFX/DeadBoom");
                                deadEffect.transform.position = transform.position;
                            }
                            break;
                    }
                }

                // 检查角色生命值变化
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.Health, out updateInfo)) {
                    if (!_initHealth) {
                        if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(_netSyncComponent.ConfigID, out Config.ShipConfig config)) {
                            _health = config.Health;
                            _maxHealth = config.Health;
                        }

                        if (null != _headHealthUI) {
                            _headHealthUI.SetHealth(_health, _maxHealth);
                        }
                        _initHealth = true;
                    }

                    int newHealth = updateInfo.Data1.Int16;
                    if (_health > updateInfo.Data1.Int16) {

                        // todo update health ui
                        if (null != _headHealthUI) {
                            _headHealthUI.SetHealth(newHealth, _maxHealth);
                        }

                        // 还活着，进行闪白
                        if (newHealth > 0) {
                            _neeUpdateHitColor = true;
                            _hitColorUpdateTime = 0;
                        }
                    }
                    _health = newHealth;
                }
            }
        }

        public void Update() {
            // 根据前后帧的位置来计算动画表现
            SimulateComponent simulateComponent = GetSimulate();
            if (null != simulateComponent && null != _animator) {
                Vector3 delta = _simulateComponent.CurrentLocation - transform.position;

                // 左右移动时对机体进行左右旋转
                float rightDotValue = Vector3.Dot(delta.normalized, transform.right);
                if (0 == delta.x)
                    _velocityX = Mathf.MoveTowards(_velocityX, _normalVelocity, 0.1f);
                else if (rightDotValue > 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moverightVelocity, 0.1f);
                else if (rightDotValue < 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moveLeftVelocity, 0.1f);
                _animator.SetFloat("VelocityX", _velocityX);

                // 前后加速时对尾焰大小进行修改
                float dotValue = Vector3.Dot(delta.normalized, transform.forward);
                if (0 == delta.z)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _normalTailFlame, 0.1f);
                else if (dotValue > 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _accelerateTailFlame, 0.1f);
                else if (dotValue < 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _slowDownTailFlame, 0.1f);
                SetTailFlameSize(_tailFlameSize);
            }

            // 闪白
            if (_neeUpdateHitColor) {
                _hitColorUpdateTime += Time.deltaTime;
                
                // 更新材质颜色
                if (null != HitColorUpdateCurve) {
                    float t = _hitColorUpdateTime / UpdateColorMaxTime;
                    float value = HitColorUpdateCurve.Evaluate(t);
                    for (int i = 0; i < _shipMaterials.Count; i++) {
                        Material material = _shipMaterials[i];
                        Color originColor = _originAdditionColor[i];
                        material.SetColor("_BaseColor", Color.Lerp(originColor, HitAdditionColor, value));
                    }
                }

                // 完成闪白
                if (_hitColorUpdateTime > UpdateColorMaxTime) {
                    _neeUpdateHitColor = false;
                }
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

            if (null == _netSyncComponent)
                _netSyncComponent = GetComponent<NetSyncComponent>();

            _hitColorUpdateTime = 0;
            _velocityX = _normalTailFlame;
            _tailFlameSize = _normalTailFlame;
        }

        private void OnDisable() {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate -= OnFrameUpdate;
            }
            _initHealth = false;
        }
    }
}
