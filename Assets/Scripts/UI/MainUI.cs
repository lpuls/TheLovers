using UnityEngine;
using UnityEngine.UI;

namespace Hamster.SpaceWar {
    public class MainUIView : UIView {
        private Image _health = null;
        private Image _slowHealth = null;
        private Text _healthValue = null;
        private Image _weaponIcon = null;
        private Animator _systemTalkDialogue = null;
        private Text _systemTalkDialogueText = null;

        public override void Initialize() {
            base.Initialize();

            _health = GetComponentFromChild<Image>("PlayerInfo/Health/Value");
            _slowHealth = GetComponentFromChild<Image>("PlayerInfo/Health/ValueSlow");
            _healthValue = GetComponentFromChild<Text>("PlayerInfo/Health/HealthValue");
            _weaponIcon = GetComponentFromChild<Image>("PlayerInfo/Weapons/Weapon");

            _systemTalkDialogue = gameObject.GetComponentFromeChild<Animator>("SystemTalkDialogue");
            _systemTalkDialogueText = GetComponentFromChild<Text>("SystemTalkDialogue/Text");
        }

        public void UpdateHealth(int value, int max) {
            _health.fillAmount = value * 1.0f / max;
            _healthValue.text = CommonString.CommonIntString[value];  // string.Format("{0}", value);
        }

        public void UpdateWeapon(string spriteName) {
            Sprite sprite = Single<AtlasManager>.GetInstance().GetSprite("Res/SpriteAtlas/MainUI", spriteName);
            _weaponIcon.sprite = sprite;
        }

        public void ShowSystemTalkDialogue(string text) {
            _systemTalkDialogue.Play("ShoSystemTalkDialogue", 0, 0);
            _systemTalkDialogueText.text = text;
        }

        private void Update() {
            if (_slowHealth.fillAmount != _health.fillAmount)
                _slowHealth.fillAmount = Mathf.MoveTowards(_slowHealth.fillAmount, _health.fillAmount, 0.1f);
        }
    }

    public class MainUIModule : UIModule {
        public int MaxHealth = 1;
        public ModityValue<int> Health = new ModityValue<int>();
        public ModityValue<int> WeaponID = new ModityValue<int>();
        public ModityValue<int> SystemTalkID = new ModityValue<int>();
    }

    [UIInfo("Res/UI/MainUI", typeof(MainUIView), typeof(MainUIModule))]
    public class MainUIController : UIController<MainUIView, MainUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

            _module.MaxHealth = 100;
            _module.Health.SetValue(100);

            // 绑定属性修改事件
            _module.Health.OnValueChange += OnHealthChange;
            _module.WeaponID.OnValueChange += OnWeaponChange;
            _module.SystemTalkID.OnValueChange += OnSystemTalkIDChange;
        }

        protected override void OnFinish() {
            base.OnFinish();

            _module.Health.OnValueChange -= OnHealthChange;
            _module.WeaponID.OnValueChange -= OnWeaponChange;
            _module.SystemTalkID.OnValueChange -= OnSystemTalkIDChange;
        }

        private void OnHealthChange(int oldValue, int newValue) {
            _view.UpdateHealth(newValue, _module.MaxHealth);
        }

        private void OnWeaponChange(int oldValue, int newValue) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Weapon>(newValue, out Config.Weapon weapon)) {
                _view.UpdateWeapon(weapon.Icon);
            }
        }

        private void OnSystemTalkIDChange(int oldValue, int newValue) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.MainUITalkContext>(newValue, out Config.MainUITalkContext context)) {
                _view.ShowSystemTalkDialogue(context.Context);
            }
        }
    }

}
