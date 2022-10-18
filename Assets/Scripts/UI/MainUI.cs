using UnityEngine;
using UnityEngine.UI;

namespace Hamster.SpaceWar {
    public class MainUIView : UIView {
        private Slider _health = null;
        private Text _healthValue = null;

        public override void Initialize() {
            base.Initialize();

            _health = GetComponentFromChild<Slider>("PlayerInfo/Health");
            _healthValue = GetComponentFromChild<Text>("PlayerInfo/HealthValue");
        }

        public void UpdateHealth(int value, int max) {
            _health.value = value * 1.0f / max;
            _healthValue.text = string.Format("{0}", value);
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

            // 初始化玩家生命值
            //ClientSpaceWarWorld world = World.GetWorld<ClientSpaceWarWorld>();
            //Debug.Assert(null != world, "World Is Invalid");

            //ClientFrameDataManager frameDataManager = world.GetManager<ClientFrameDataManager>();
            //Debug.Assert(null != frameDataManager, "Frame Data Manager Is Invalid");

            //int playerID = world.PlayerNetID;
            //if (frameDataManager.TryGetNetActor(playerID, out NetSyncComponent netSyncComponent)) {
            //    if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(netSyncComponent.ConfigID, out Config.ShipConfig config)) {
            //        _module.MaxHealth = config.Health;
            //        _module.Health.SetValue(config.Health);
            //    }
            //}
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
