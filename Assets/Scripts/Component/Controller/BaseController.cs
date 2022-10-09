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
            _netSyncComponent = gameObject.TryGetOrAdd<NetSyncComponent>();
        }

        public virtual void OnDisable() {
            World.GetWorld<BaseSpaceWarWorld>().RemoveTicker(this);
        }

        public virtual void OnDestroy() {
            OnDisable();
        }

        public virtual void Tick(float dt) {
        }

        protected NetSyncComponent GetNetSyncComponent() {
            if (null != _netSyncComponent)
                return _netSyncComponent;
            _netSyncComponent = GetComponent<NetSyncComponent>();
            return _netSyncComponent;
        }

        public virtual bool IsEnable() {
            return gameObject.activeSelf;
        }
    }
}