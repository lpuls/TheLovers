using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class MissionOverUI : UIView {
        private Animator _successAnimator = null;
        private Animator _failedAnimator = null;

        public override void Initialize() {
            base.Initialize();

            _successAnimator = GetMonoComponentFromChild<Animator>("MissionComplete");
            _successAnimator.gameObject.SetActive(false);
            _failedAnimator = GetMonoComponentFromChild<Animator>("MissionFailed");
            _failedAnimator.gameObject.SetActive(false);
        }

        public void ShowMissionComplete() {
            _successAnimator.gameObject.SetActive(true);
            _successAnimator.Play("Show");
        }

        public void ShowMissionFailed() {
            _failedAnimator.gameObject.SetActive(true);
            _failedAnimator.Play("Show");
        }
    }

    public class MissionOverUIModule : UIModule {
        public ModityValue<bool> IsComplete = new ModityValue<bool>();
    }

    [UIInfo("Res/UI/MissionOverUI", typeof(MissionOverUI), typeof(MissionOverUIModule))]
    public class MissionOverUIController : UIController<MissionOverUI, MissionOverUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

            _module.IsComplete.OnValueChange += OnResultChange;
        }

        protected override void OnFinish() {
            base.OnFinish();

            _module.IsComplete.OnValueChange -= OnResultChange;
        }

        private void OnResultChange(bool oldValue, bool newValue) {
            if (newValue)
                _view.ShowMissionComplete();
            else
                _view.ShowMissionFailed();
            _view.StartCoroutine(CloseUI());
        }

        private IEnumerator CloseUI() {
            yield return new WaitForSeconds(5.0f);
            Single<UIManager>.GetInstance().Close<MissionOverUIController>();
        }

    }
}
