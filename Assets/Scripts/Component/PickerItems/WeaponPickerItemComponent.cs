using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WeaponPickerItemComponent : PickerItemComponent {
        public EAbilityIndex AbilityIndex = EAbilityIndex.Fire;
        public int WeaponID = 0;

        public override void OnPicker(PlayerController playerController) {
            base.OnPicker(playerController);
            ServerPlayerController serverPlayerController = playerController as ServerPlayerController;
            if (null != serverPlayerController) {
                Debug.Log("<color=red>Change Weapon !!! </color>" + WeaponID);
                serverPlayerController.ChangeWeapon(AbilityIndex, WeaponID);
            }
        }
    }
}