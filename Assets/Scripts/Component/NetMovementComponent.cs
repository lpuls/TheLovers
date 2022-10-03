using UnityEngine;

namespace Hamster.SpaceWar {
    public class NetMovementComponent : MonoBehaviour {

        public float MaxDistanceWithServer = 1.0f;

        private Vector3 _prePosition = Vector3.zero;
        private Vector3 _currentPosition = Vector3.zero;
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

            bool isAutonomous = _netSyncComponent.IsAutonomousProxy();
            FrameData preData = spaceWarWorld.GetPreFrameData();
            FrameData currentData = spaceWarWorld.GetCurrentFrameData();
            if (null != preData && null != currentData) {

                int netID = _netSyncComponent.NetID;
                if (!preData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo preUpdateInfo)) {
                    return;
                }
                if (!currentData.TryGetUpdateInfo(netID, EUpdateActorType.Position, out UpdateInfo currentUpdateInfo))
                    return;

                _prePosition = preUpdateInfo.Data.Vec3;
                _currentPosition = currentUpdateInfo.Data.Vec3;

                float t = spaceWarWorld.GetLogicFramepercentage();
                if (_netSyncComponent.IsAutonomousProxy()) {
                    if (Vector3.Distance(transform.position, _currentPosition) >= MaxDistanceWithServer)
                        transform.position = Vector3.Lerp(transform.position, _currentPosition, Time.deltaTime);
                }
                else {
                    transform.position = Vector3.Lerp(preUpdateInfo.Data.Vec3, currentUpdateInfo.Data.Vec3, t);
                }
            }

            // 主端修正
            if (_netSyncComponent.IsAutonomousProxy() && !_prePosition.Equals(Vector3.zero) && !_currentPosition.Equals(Vector3.zero)) {
                transform.position = Vector3.Lerp(transform.position, _currentPosition, Time.deltaTime);
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
