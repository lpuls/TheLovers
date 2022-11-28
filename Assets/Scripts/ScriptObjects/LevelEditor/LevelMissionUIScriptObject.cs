using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    [SerializeField]
    public class LevelMissionUIScriptObject : LevelEventScriptObject {
        public int MissionID = 0;

        public override void OnEnter(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            Single<UIManager>.GetInstance().Open<MissonUIController>();
            MissionUIModule missionUIModule = Single<UIManager>.GetInstance().GetModule<MissonUIController>() as MissionUIModule;
            missionUIModule.MissionID.SetValue(MissionID);
        }

        public override void OnLevel(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            Single<UIManager>.GetInstance().Close<MissonUIController>();
        }
    }
}
