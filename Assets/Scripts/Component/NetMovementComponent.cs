﻿using UnityEngine;

namespace Hamster.SpaceWar {
    public class NetMovementComponent : MonoBehaviour {

        private NetSyncComponent _netSyncComponent;

        private void Awake() {
            _netSyncComponent = GetComponent<NetSyncComponent>();
        }

        public void Update() {
            if (null == _netSyncComponent)
                return;

            if (_netSyncComponent.IsAuthority()) {
                Debug.LogError("=========> Can't run Net Movement In Server");
                return;
            }

            ClientSpaceWarWorld spaceWarWorld = World.GetWorld<ClientSpaceWarWorld>();
            if (null == spaceWarWorld) {
                Debug.LogError("=========> clientSpaceWarWorld is null");
                return;
            }

            FrameData preData = spaceWarWorld.GetPreFrameData();
            FrameData currentData = spaceWarWorld.GetCurrentFrameData();
            if (null != preData && null != currentData) {

                int netID = _netSyncComponent.NetID;
                if (!preData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo preUpdateInfo))
                    return;
                if (!currentData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo currentUpdateInfo))
                    return;

                float t = spaceWarWorld.GetLogicFramepercentage();
                transform.position = Vector3.Lerp(preUpdateInfo.Data.Vec3, currentUpdateInfo.Data.Vec3, t);

                //int netID = _netSyncComponent.OwnerID << 16 | _netSyncComponent.NetID;
                //if (!preData.NetInfoDict.TryGetValue(netID, out INetInfo preNetInfo))
                //    return;
                //if (!currentData.NetInfoDict.TryGetValue(netID, out INetInfo currentNetInfo))
                //    return;

                //// float t = spaceWarWorld.LogicTime / SpaceWarWorld.LOGIC_FRAME;
                //float t = spaceWarWorld.GetLogicFramepercentage();

                //Vector3 lastLocation = new Vector3(preNetInfo.X, 0, preNetInfo.Y);
                //Vector3 currentLocation = new Vector3(currentNetInfo.X, 0, currentNetInfo.Y);
                //transform.position = Vector3.Lerp(lastLocation, currentLocation, t);
            }
        }

    }
}
