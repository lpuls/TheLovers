using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelEventUI {
        Warning
    }

    [SerializeField]
    public class LevelUIScriptObject : LevelEventScriptObject {
        public ELevelEventUI UIType = 0;

        public override void OnEnter(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (UIType) {
                case ELevelEventUI.Warning: {
                        Single<UIManager>.GetInstance().Open<WarningUIController>();
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
