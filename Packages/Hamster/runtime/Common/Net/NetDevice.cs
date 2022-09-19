using System.Collections.Generic;
using System.Net.Sockets;

namespace Hamster {

    public interface INetDevice {
        Packet Malloc(int size);

        void SendMessage(NetMessage message);

        void Close();

        bool IsServer();
    }

    public class NetDevice : INetDevice {
        protected PacketManager _packetManager = new PacketManager();
        protected Dictionary<int, NetModule> _modules = new Dictionary<int, NetModule>(new Int32Comparer());

        public virtual Packet Malloc(int size) {
            return _packetManager.Malloc(size);
        }

        public virtual void SendMessage(NetMessage message) {
            throw new System.NotImplementedException();
        }

        public virtual void Close() {
        }

        public virtual void Update() {
            // 更新网络模块
            var it = _modules.GetEnumerator();
            while (it.MoveNext()) {
                it.Current.Value.Update();
            }
        }

        public virtual bool IsServer() {
            return false;
        }

        public void RegistModule(NetModule netModule) {
            if (!_modules.ContainsKey(netModule.GetModuleID())) {
                _modules[netModule.GetModuleID()] = netModule;
                netModule.Initialize(this);
            }
            else {
                UnityEngine.Debug.LogError("Register Failed " + netModule.GetModuleID());
            }
        }

        public NetModule GetModule(int moduleID) {
            if (!_modules.TryGetValue(moduleID, out NetModule netModule)) {
                UnityEngine.Debug.LogError("Get Module Failed " + netModule.GetModuleID());
            }
            return netModule;
        }
    }

}
