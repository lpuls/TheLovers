using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class NetMovementComponent : MonoBehaviour {

        public float MaxDistanceWithServer = 0.3f;

        private Vector3 _prePosition = Vector3.zero;
        private Vector3 _currentPosition = Vector3.zero;
        private NetSyncComponent _netSyncComponent;
        private NetPlayerController _playerController;

        private void Awake() {
            _netSyncComponent = GetComponent<NetSyncComponent>();
            _playerController = GetComponent<NetPlayerController>();
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

                _prePosition = preUpdateInfo.Data1.Vec3;
                _currentPosition = currentUpdateInfo.Data1.Vec3;

                float t = spaceWarWorld.GetLogicFramepercentage();
                if (_netSyncComponent.IsAutonomousProxy()) {
                    // 根据服务端下发的帧号获取当时預測的结果
                    int currentFrameIndex = currentUpdateInfo.Data1.Int32;
                    if (_playerController.TryGetPredictionLocation(currentFrameIndex, out Vector3 predicationLocation)) {
                        // 如果逻辑结果与实际结果相关过大，则需要清理所有預測结果，然后拉到逻辑位置上
                        if (Vector3.Distance(predicationLocation, _currentPosition) > MaxDistanceWithServer) {
                            _playerController.CleanPredicationLocations();
                            transform.position = Vector3.Lerp(preUpdateInfo.Data1.Vec3, currentUpdateInfo.Data1.Vec3, t);
                        }
                        else {
                            _playerController.RemovePredictionLocation(currentFrameIndex);
                        }
                    }
                }
                else {
                    transform.position = Vector3.Lerp(preUpdateInfo.Data1.Vec3, currentUpdateInfo.Data1.Vec3, t);
                }
            }

            // 偷偷修正位置
            //if (_netSyncComponent.IsAutonomousProxy()) {
            //    transform.position = Vector3.Lerp(_prePosition, _currentPosition, Time.deltaTime);
            //}
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_prePosition, 1.0f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_currentPosition, 1.0f);
        }
#endif

    }
}
