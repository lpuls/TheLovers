﻿using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class PlayerEffectComponent : MonoBehaviour {
        // 尾焰大小变化
        [SerializeField]
        private Transform[] _tailFlame = null;
        [SerializeField]
        private float _accelerateTailFlame = 4;
        [SerializeField]
        private float _normalTailFlame = 2;
        [SerializeField]
        private float _slowDownTailFlame = 1;

        // 左右移动时战机动画值
        [SerializeField]
        private float _moveLeftVelocity = -1;
        [SerializeField]
        private float _normalVelocity = 0;
        [SerializeField]
        private float _moverightVelocity = 1;

        // 组件
        private Animator _animator = null;
        private SimulateComponent _simulateComponent = null;
        private NetSyncComponent _netSyncComponent = null;

        // 受击闪烁
        public Color HitAdditionColor = Color.red;
        public AnimationCurve HitColorUpdateCurve = null;
        public float UpdateColorMaxTime = 0.1f;
        private bool _neeUpdateHitColor = false;
        private float _hitColorUpdateTime = 0;
        private List<Color> _originAdditionColor = new List<Color>();
        private List<Material> _shipMaterials = new List<Material>();

        // 移动时的尾焰及动画
        private float _velocityX = 0;
        private float _tailFlameSize = 2;

        // 生命值UI
        private int _health = 1;
        private int _maxHealth = 1;
        private MainUIModule _mainUIModule = null;
        [SerializeField] private OverheadHealthUI _headHealthUI = null;

        // 闪避
        public bool IsDodging {
            get;
            private set;
        }

        // 特效资源
        public string SpawnEffectPath = "Res/VFX/ShipSpawn";
        public string DeadEffectPath = "Res/VFX/DeadBoom";

        private void Awake() {
            _simulateComponent = GetComponent<SimulateComponent>();
            _animator = GetComponentInChildren<Animator>();
            _netSyncComponent = GetComponent<NetSyncComponent>();

            _velocityX = _normalTailFlame;
            _tailFlameSize = _normalTailFlame;

            // 获取所有的材质
            Transform modleTransofmr = transform.Find("Model");
            if (null != modleTransofmr) {
                Renderer[] renderers = modleTransofmr.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++) {
                    Renderer renderer = renderers[i];
                    Material[] materials = renderer.materials;
                    for (int j = 0; j < materials.Length; j++) {
                        _shipMaterials.Add(materials[j]);
                        _originAdditionColor.Add(materials[j].GetColor("_BaseColor"));
                    }
                }
            }
        }

        public void Init() {
            // 读取配置
            int weaponID = (int)Config.WeaponType.Galting;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.UnitConfig>(_netSyncComponent.ConfigID, out Config.UnitConfig config)) {
                _health = config.Health;
                _maxHealth = config.Health;
                weaponID = config.WeaponID;
            }

            // 确定UI
            if (_netSyncComponent.IsAutonomousProxy()) {
                _mainUIModule = Single<UIManager>.GetInstance().GetModule<MainUIController>() as MainUIModule;
                _headHealthUI.gameObject.SetActive(false);

                _mainUIModule.MaxHealth = _maxHealth;
                _mainUIModule.Health.SetValue(_health);
                _mainUIModule.WeaponID.SetValue(weaponID);
            }
            else if (null != _headHealthUI) {
                _headHealthUI.gameObject.SetActive(true);

                _headHealthUI.SetHealth(_health, _maxHealth);
            }

            // 播放出生动画
            if (null != _animator)
                _animator.Play("Spawn", 0, 0.0f);

        }

        protected virtual void OnFrameUpdate(FrameData pre, FrameData current) {
            int netID = _netSyncComponent.NetID;

            UpdateInfo currentUpdateInfo;
            if (null != current) {
                // 检查角色状态变化
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.RoleState, out currentUpdateInfo)) {
                    switch ((EPlayerState)currentUpdateInfo.Data1.Int8) {
                        case EPlayerState.Spawning: {
                                if (!string.IsNullOrEmpty(SpawnEffectPath)) {
                                    GameObject spawnEffect = Asset.Load(SpawnEffectPath);
                                    spawnEffect.transform.position = transform.position;
                                    spawnEffect.transform.forward = transform.forward;
                                }
                            }
                            break;
                        case EPlayerState.Alive:
                            break;
                        case EPlayerState.Deading: {
                                if (!string.IsNullOrEmpty(DeadEffectPath)) {
                                    GameObject deadEffect = Asset.Load(DeadEffectPath);
                                    deadEffect.transform.position = transform.position;
                                }
                            }
                            break;
                        case EPlayerState.Dead: {
                                if (null != _animator)
                                    _animator.SetTrigger("Dead");
                                if (!string.IsNullOrEmpty(DeadEffectPath)) {
                                    GameObject deadEffect = Asset.Load(DeadEffectPath);
                                    deadEffect.transform.position = transform.position;
                                }
                            }
                            break;
                    }
                }

                // 检查角色生命值变化
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.Health, out currentUpdateInfo)) {
                    // 根据是否为主控角色
                    int newHealth = currentUpdateInfo.Data1.Int16;
                    if (null != _mainUIModule) {
                        _mainUIModule.MaxHealth = _maxHealth;
                        _mainUIModule.Health.SetValue(newHealth);
                    }
                    else if (null != _headHealthUI) {
                        _headHealthUI.SetHealth(_health, _maxHealth);
                    }

                    if (_health > currentUpdateInfo.Data1.Int16) {

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

                // 检查是否闪避
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.Dodge, out currentUpdateInfo)) {
                    if (currentUpdateInfo.Data1.Boolean && !IsDodging) {
                        if (null != _animator)
                            _animator.SetTrigger("Dodge");
                        IsDodging = true;
                    }
                    else if (!currentUpdateInfo.Data1.Boolean && IsDodging) {
                        IsDodging = false;
                    }
                }

                // 检查武器变化
                if (current.TryGetUpdateInfo(netID, EUpdateActorType.MainWeapon, out currentUpdateInfo)) {
                    MainUIModule mainUIModule = Single<UIManager>.GetInstance().GetModule<MainUIController>() as MainUIModule;
                    mainUIModule.WeaponID.SetValue(currentUpdateInfo.Data1.Int16);
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
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _normalTailFlame, 0.1f);
                else if (rightDotValue > 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _accelerateTailFlame, 0.1f);
                else if (rightDotValue < 0)
                    _tailFlameSize = Mathf.MoveTowards(_tailFlameSize, _slowDownTailFlame, 0.1f);
                SetTailFlameSize(_tailFlameSize);


                // 前后加速时对尾焰大小进行修改
                float dotValue = Vector3.Dot(delta.normalized, transform.up);
                if (0 == delta.y)
                    _velocityX = Mathf.MoveTowards(_velocityX, _normalVelocity, 0.1f);
                else if (dotValue > 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moverightVelocity, 0.1f);
                else if (dotValue < 0)
                    _velocityX = Mathf.MoveTowards(_velocityX, _moveLeftVelocity, 0.1f);
                if (null != _animator)
                    _animator.SetFloat("VelocityX", _velocityX);
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

        public void PlayDodge() {
            _animator.SetTrigger("Dodge");
            IsDodging = true;
        }

        private void OnDisable() {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            if (null != frameDataManager) {
                frameDataManager.OnFrameUpdate -= OnFrameUpdate;
            }

            _mainUIModule = null;
        }
    }
}
