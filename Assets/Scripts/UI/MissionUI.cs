using UnityEngine.UI;

namespace Hamster.SpaceWar {
    public class MissionUI : UIView {
        private Text _title = null;
        private Text _context = null;
    }

    public class MissionUIModule : UIModule {
    }

    [UIInfo("Res/UI/MissionUI", typeof(MissionUI), typeof(MissionUIModule))]
    public class MissonUIController : UIController<MissionUI, MissionUIModule> {
        protected override void OnInitialize() {
            base.OnInitialize();
        }

        protected override void OnFinish() {
            base.OnFinish();
        }

    }
}
