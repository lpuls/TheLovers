using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class WeaponPickerItem : PickerItem {
        public EAbilityIndex AbilityIndex = EAbilityIndex.Fire;
        public int WeaponID = 0;

        protected override void OnPicker(PlayerController playerController) {
            base.OnPicker(playerController);

            ServerPlayerController serverPlayerController = playerController as ServerPlayerController;
            if (null != serverPlayerController) {
                serverPlayerController.ChangeWeapon(AbilityIndex, WeaponID);
            }
        }
    }
}