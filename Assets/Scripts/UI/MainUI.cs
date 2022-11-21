using UnityEngine;
using UnityEngine.UI;

namespace Hamster.SpaceWar {
    public class MainUIView : UIView {
        private Image _health = null;
        private Image _slowHealth = null;
        private Text _healthValue = null;

        public override void Initialize() {
            base.Initialize();

            _health = GetComponentFromChild<Image>("PlayerInfo/Health/Value");
            _slowHealth = GetComponentFromChild<Image>("PlayerInfo/Health/ValueSlow");
            _healthValue = GetComponentFromChild<Text>("PlayerInfo/Health/HealthValue");
        }

        public void UpdateHealth(int value, int max) {
            _health.fillAmount = value * 1.0f / max;
            _healthValue.text = string.Format("{0}", value);
        }

        private void Update() {
            if (_slowHealth.fillAmount != _health.fillAmount)
                _slowHealth.fillAmount = Mathf.MoveTowards(_slowHealth.fillAmount, _health.fillAmount, 0.1f);
        }
    }

    public class MainUIModule : UIModule {
        public int MaxHealth = 1;
        public ModityValue<int> Health = new ModityValue<int>();
    }

    [UIInfo("Res/UI/MainUI", typeof(MainUIView), typeof(MainUIModule))]
    public class MainUIController : UIController<MainUIView, MainUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

            _module.MaxHealth = 100;
            _module.Health.SetValue(100);

            // 绑定属性修改事件
            _module.Health.OnValueChange += OnHealthChange;
        }

        protected override void OnFinish() {
            base.OnFinish();

            _module.Health.OnValueChange -= OnHealthChange;
        }

        private void OnHealthChange(int oldValue, int newValue) {
            _view.UpdateHealth(newValue, _module.MaxHealth);
        }
    }

}
