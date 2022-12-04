using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum ELevelEventType {
        OpenWarning,
        OpenMainUISystemTalkDialogue,
        EnablePlayerInput,
        DisablePlayerInput,
        JustWait,
        ShowTransition,
        HideMainUI
    }

    [SerializeField]
    public class LevelGameEventScriptObject : LevelEventScriptObject {
        public ELevelEventType EventType = 0;
        public int ArgInt = 0;

        public override void OnEnter(ILevelManager levelManager) {

            switch (EventType) {
                case ELevelEventType.OpenWarning: {
                        if (levelManager.IsClient())
                            Single<UIManager>.GetInstance().Open<WarningUIController>();
                    }
                    break;
                case ELevelEventType.OpenMainUISystemTalkDialogue: {
                        if (levelManager.IsClient()) {
                            MainUIModule mainUIModule = Single<UIManager>.GetInstance().GetModule<MainUIController>() as MainUIModule;
                            mainUIModule.SystemTalkID.SetValue(ArgInt);
                        }
                    }
                    break;
                case ELevelEventType.EnablePlayerInput: {
                        if (!levelManager.IsClient()) {
                            EnableOrDisablePlayerInput(true);
                        }
                    }
                    break;
                case ELevelEventType.DisablePlayerInput: {
                        if (!levelManager.IsClient()) {
                            EnableOrDisablePlayerInput(false);
                        }
                    }
                    break;
                case ELevelEventType.JustWait: {
                        Debug.Log("JustWait");
                    }
                    break;
                case ELevelEventType.ShowTransition: {
                        if (levelManager.IsClient()) {
                            World.GetWorld().ShowTransition();
                            Debug.Log("Begin Transition");
                        }
                    }
                    break;
                case ELevelEventType.HideMainUI: {
                        if (levelManager.IsClient()) {
                            if (0 != ArgInt)
                                Single<UIManager>.GetInstance().Show(typeof(MainUIController));
                            else
                                Single<UIManager>.GetInstance().Hide(typeof(MainUIController));
                            Debug.Log("Show Or Hide Main UI " + ArgInt);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void EnableOrDisablePlayerInput(bool enable) {
            ServerFrameDataManager serverFrameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            var players = serverFrameDataManager.GetPlayers();
            foreach (var item in players) {
                if (item.TryGetComponent<ServerPlayerController>(out ServerPlayerController serverPlayerController)) {
                    serverPlayerController.EnableInput(enable);
                }
            }
        }

        public override void OnLevel(ILevelManager levelManager) {
            if (!levelManager.IsClient())
                return;

            switch (EventType) {
                case ELevelEventType.OpenWarning: {
                        Single<UIManager>.GetInstance().Close<WarningUIController>();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
