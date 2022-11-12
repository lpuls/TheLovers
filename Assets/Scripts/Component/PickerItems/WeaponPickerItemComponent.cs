using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WeaponPickerItemComponent : PickerItemComponent {
        public EAbilityIndex AbilityIndex = EAbilityIndex.Fire;
        public Config.WeaponType WeaponID = Config.WeaponType.None;

        public override void OnPicker(PlayerController playerController) {
            base.OnPicker(playerController);
            ServerPlayerController serverPlayerController = playerController as ServerPlayerController;
            if (null != serverPlayerController) {
                Debug.Log("<color=red>Change Weapon !!! </color>" + WeaponID);
                serverPlayerController.ChangeWeapon(AbilityIndex, (int)WeaponID);
            }
        }
    }
}