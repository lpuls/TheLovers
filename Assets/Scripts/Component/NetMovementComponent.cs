using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar                                                                                                                                                                                                                                                                                                   {
    public class NetMovementComponent : MonoBehaviour {

        public float MaxDistanceWithServer = 0.1f;

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
                preData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo preUpdateInfo);
                currentData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo currentUpdateInfo);

                int currentFrameIndex = -1;
                if (null != preUpdateInfo)
                    _prePosition = preUpdateInfo.Data1.Vec3;
                if (null != currentUpdateInfo) {
                    _currentPosition = currentUpdateInfo.Data1.Vec3;
                    currentFrameIndex = currentUpdateInfo.Data1.Int32;
                }

                float t = spaceWarWorld.GetLogicFramepercentage();
                if (_netSyncComponent.IsAutonomousProxy()) {
                    // 根据服务端下发的帧号获取当时預測的结果
                    // int currentFrameIndex = currentUpdateInfo.Data1.Int32;
                    if (_playerController.TryGetPredictionLocation(currentFrameIndex, out Vector3 predicationLocation)) {
                        // 如果逻辑结果与实际结果相关过大，则需要清理所有預測结果，然后拉到逻辑位置上
                        if (Vector3.Distance(predicationLocation, _currentPosition) > MaxDistanceWithServer) {
                            _playerController.CleanPredicationLocations();
                            transform.position = Vector3.Lerp(_prePosition, _currentPosition, t);
                        }
                        else {
                            _playerController.RemovePredictionLocation(currentFrameIndex);
                        }
                    }
                }
                else if (!_prePosition.Equals(Vector3.zero) && !_currentPosition.Equals(Vector3.zero)) {
                    transform.position = Vector3.Lerp(_prePosition, _currentPosition, t);
                    Debug.Log(string.Format("===>Pre: {0}, Cur: {1}, Position: {2}, T: {3}, Frame: {4}", _prePosition, _currentPosition, transform.position, t, currentData.FrameIndex));
                }
            }
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
