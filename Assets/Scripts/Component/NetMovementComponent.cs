using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar                                                                                                                                                                                                                                                                                                   {
    public class NetMovementComponent : MonoBehaviour {

        public float MaxDistanceWithServer = 1.0f;

        private int _lastFrameIndex = -1;
        private Vector3 _prePosition = Vector3.zero;
        private Vector3 _currentPosition = Vector3.zero;
        private NetSyncComponent _netSyncComponent;
        private NetPlayerController _playerController;

        private void Awake() {
            _netSyncComponent = GetComponent<NetSyncComponent>();
            _playerController = GetComponent<NetPlayerController>();
        }

        public void Update() {
            //if (null == _netSyncComponent)
            //    return;

            //if (_netSyncComponent.IsAuthority()) {
            //    Debug.LogError("=========> Can't run Net Movement In Server");
            //    return;
            //}

            //ClientSpaceWarWorld spaceWarWorld = World.GetWorld<ClientSpaceWarWorld>();
            //if (null == spaceWarWorld) {
            //    Debug.LogError("=========> clientSpaceWarWorld is null");
            //    return;
            //}

            //FrameData preData = spaceWarWorld.GetPreFrameData();
            //FrameData currentData = spaceWarWorld.GetCurrentFrameData();
            //if (null != preData && null != currentData) {

            //    // 判断是否需要更新
            //    bool isNewFrame = false;
            //    if (_lastFrameIndex != preData.FrameIndex) {
            //        _lastFrameIndex = preData.FrameIndex;
            //        isNewFrame = true;
            //    }

            //    int netID = _netSyncComponent.NetID;
            //    preData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo preUpdateInfo);
            //    currentData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo currentUpdateInfo);

            //    if (null != preUpdateInfo)
            //        _prePosition = preUpdateInfo.Data1.Vec3;
            //    if (null != currentUpdateInfo) {
            //        _currentPosition = currentUpdateInfo.Data1.Vec3;
            //    }

            //    float t = spaceWarWorld.GetLogicFramepercentage();
            //    if (_netSyncComponent.IsAutonomousProxy()) {
            //        // 根据服务端下发的帧号获取当时預測的结果
            //        if (isNewFrame && _playerController.TryGetTopPredictionCommand(out NetPlayerCommand command)) {

            //            Vector3 oldPosition = transform.position;

            //            // 如果逻辑结果与实际结果相关过大，则需要清理所有預測结果，然后拉到逻辑位置上
            //            transform.position = _currentPosition;
            //            // _playerController.RemoveTopPredictionCommand();
            //            _playerController.SimulateAfter();

            //            // Debug.Log(string.Format("===>Server: {0}, Prediction: {1}, Transform: [{2}, {5}], ServerFrame: {3} ClientFrame: {4}",
            //            //        _currentPosition, command.Location, transform.position, currentData.FrameIndex, command.FrameIndex, oldPosition));
            //        }
            //    }
            //    else if (!_prePosition.Equals(Vector3.zero) && !_currentPosition.Equals(Vector3.zero)) {
            //        transform.position = Vector3.Lerp(_prePosition, _currentPosition, t);
            //    }
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
