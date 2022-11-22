using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class MissionUI : UIView {
        private Text _title = null;
        private Text _context = null;
        private Animator _animator = null;

        public override void Initialize() {
            base.Initialize();
            _title = GetComponentFromChild<Text>("MissionTitle");
            _context = GetComponentFromChild<Text>("MissionContext");
            _animator = GetComponent<Animator>();
        }

        protected override void OnShow() {
            base.OnShow();
            _animator.Play("Show");
        }

        public void SetMissionInfo(string title, string context) {
            _title.text = title;
            _context.text = context;
        }

    }

    public class MissionUIModule : UIModule {
        public ModityValue<int> MissionID = new ModityValue<int>();
    }

    [UIInfo("Res/UI/MissionUI", typeof(MissionUI), typeof(MissionUIModule))]
    public class MissonUIController : UIController<MissionUI, MissionUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

            _module.MissionID.OnValueChange += OnMissionChange;
        }

        protected override void OnFinish() {
            base.OnFinish();

            _module.MissionID.OnValueChange -= OnMissionChange;
        }

        private void OnMissionChange(int oldValue, int newValue) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Mission>(newValue, out Config.Mission mission)) {
                _view.SetMissionInfo(mission.Title, mission.Context.Replace("\\n", "\n"));
            }
        }

    }
}
