using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class MissionUI : UIView {
        private Text _title = null;
        private Text _context = null;
        private RectTransform _background = null;

        private float _right = 1620.0f;

        public override void Initialize() {
            base.Initialize();
            _title = GetComponentFromChild<Text>("MissionTitle");
            _context = GetComponentFromChild<Text>("MissionContext");
            _background = GetComponent<RectTransform>();
        }

        protected override void OnShow() {
            base.OnShow();
            _title.DOFade(0, 1.0f);
            Tween tween = DOTween.To(()=> _right, x => _right = x, 960, 1.0f);
            tween.OnUpdate(() => UpdateRectTransformRight(_right));
        }

        public void SetMissionInfo(string title, string context) {
            _title.text = title;
            _context.text = context;
        }

        private void UpdateRectTransformRight(float value) {
            _background.offsetMin.SetX(value);
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

        private void OnMissionChange(int newValue, int oldValue) {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Mission>(newValue, out Config.Mission mission)) {
                _view.SetMissionInfo(mission.Title, mission.Context);
            }
        }

    }
}
