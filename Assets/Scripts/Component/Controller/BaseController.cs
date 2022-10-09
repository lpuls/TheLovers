using System.Collections.Generic;
using UnityEngine;


namespace Hamster.SpaceWar {

    public class BaseController : MonoBehaviour, IServerTicker {
        
        // protected SimulateComponent _simulateComponent = null;
        protected NetSyncComponent _netSyncComponent = null;

        public virtual void OnEnable() {
            Init();
        }

        public virtual void Init() {
            World.GetWorld<BaseSpaceWarWorld>().AddTicker(this);
            // _simulateComponent = gameObject.TryGetOrAdd<SimulateComponent>();
            _netSyncComponent = gameObject.TryGetOrAdd<NetSyncComponent>();

            //ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>() as ClientFrameDataManager;
            //if (null != frameDataManager) {
            //    frameDataManager.OnFrameUpdate += OnFrameUpdate;
            //}
        }

        public virtual void OnDisable() {
            World.GetWorld<BaseSpaceWarWorld>().RemoveTicker(this);

            //ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>() as ClientFrameDataManager;
            //if (null != frameDataManager) {
            //    frameDataManager.OnFrameUpdate -= OnFrameUpdate;
            //}
        }

        public virtual void OnDestroy() {
            OnDisable();
        }

        public virtual void Tick(float dt) {
        }

        //protected SimulateComponent GetSimulateComponent() {
        //    if (null != _simulateComponent)
        //        return _simulateComponent;
        //    _simulateComponent = GetComponent<SimulateComponent>();
        //    return _simulateComponent;
        //}

        protected NetSyncComponent GetNetSyncComponent() {
            if (null != _netSyncComponent)
                return _netSyncComponent;
            _netSyncComponent = GetComponent<NetSyncComponent>();
            return _netSyncComponent;
        }

        //protected virtual void OnFrameUpdate(FrameData pre, FrameData current) {
        //    int netID = _netSyncComponent.NetID;
        //    UpdateInfo preUpdateInfo;
        //    UpdateInfo currentUpdateInfo;
        //    Vector3 preLocation = _simulateComponent.PreLocation;
        //    Vector3 currentLocation = _simulateComponent.CurrentLocation;
        //    if (null != pre && pre.TryGetUpdateInfo(netID, EUpdateActorType.Position, out preUpdateInfo)) {
        //        preLocation = preUpdateInfo.Data1.Vec3;
        //    }
        //    if (null != current && current.TryGetUpdateInfo(netID, EUpdateActorType.Position, out currentUpdateInfo)) {
        //        currentLocation = currentUpdateInfo.Data1.Vec3;
        //        _simulateComponent.UpdateServerToPredictPosition(preLocation, currentLocation, currentUpdateInfo.Data2.Int32);
        //    }
        //}
    }
}