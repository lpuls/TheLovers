using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar
{
    public class NetPlayerController : LocalPlayerController
    {
        protected override void InitPlayerInputReceiver() {
            NetDevice netDeivce = World.GetWorld().GetManager<NetDevice>();
            if (null == netDeivce || !netDeivce.IsValid) {
                Debug.LogError("=====>Local LocalPlayerController Has not NetDevice ");
                return;
            }

            _playerInputReceiver = netDeivce.GetModule(GameLogicSyncModule.NET_GAME_LOGIC_SYNC_ID) as GameLogicSyncModule;
            if (null == _playerInputReceiver) {
                Debug.LogError("=====>Local LocalPlayerController Has not GameLogicSyncModule ");
                return;
            }
        }
    }
}
