using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelEventUI {
        Warning,
        MainUISystemTalkDialogue
    }

    [SerializeField]
    public class LevelUIScriptObject : LevelEventScriptObject {
        public ELevelEventUI UIType = 0;
        public int ArgInt = 0;

        public override void OnEnter(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (UIType) {
                case ELevelEventUI.Warning: {
                        Single<UIManager>.GetInstance().Open<WarningUIController>();
                    }
                    break;
                case ELevelEventUI.MainUISystemTalkDialogue: {
                        MainUIModule mainUIModule = Single<UIManager>.GetInstance().GetModule<MainUIController>() as MainUIModule;
                        mainUIModule.SystemTalkID.SetValue(ArgInt);
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnLevel(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (UIType) {
                case ELevelEventUI.Warning: {
                        Single<UIManager>.GetInstance().Close<WarningUIController>();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
