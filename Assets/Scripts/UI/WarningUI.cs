using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WarningUI : UIView {
        public Color TintColor = Color.white;
        
        private Image _warning = null;
        private Animator _animator = null;

        public override void Initialize() {
            base.Initialize();

            _warning = GetComponentFromChild<Image>("Warning");
            _animator = GetComponent<Animator>();
        }

        protected override void OnShow() {
            base.OnShow();
            _animator.Play("Show");
        }

        public void Update() {
            if (null != _warning) {
                _warning.material.SetColor("_Color", TintColor);
                _warning.SetMaterialDirty();
            }
        }


    }

    public class WarningUIModule : UIModule {
        public ModityValue<int> MissionID = new ModityValue<int>();
    }

    [UIInfo("Res/UI/WarningUI", typeof(WarningUI), typeof(WarningUIModule))]
    public class WarningUIController : UIController<WarningUI, WarningUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

        }

        protected override void OnFinish() {
            base.OnFinish();

        }

    }
}
