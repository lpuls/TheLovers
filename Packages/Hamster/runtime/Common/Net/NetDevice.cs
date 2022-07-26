using System.Collections.Generic;
using System.Net.Sockets;

namespace Hamster {

    public interface INetDevice {
        Packet Malloc(int size);
        void SendMessage(NetMessage message);
    }

    public class NetDevice : INetDevice {
        private ClientSocket _socket = null;
        private PacketManager _packetManager = new PacketManager();
        private Dictionary<int, NetModule> _modules = new Dictionary<int, NetModule>(new Int32Comparer());

        public bool IsValid {
            get {
                return _socket.IsConnect(); 
            } 
        }

        public NetDevice() {
            _socket = new ClientSocket(_packetManager);
            _socket.OnConnectSuccess += OnConnectSuccess;
            _socket.OnSendMessageFailed += OnSendMessageFailed;
            // _socket.OnReceiveMessageCompleted += OnReceiveMessageCompleted;
        }

        public void Connect(string ip, int port) {
            _socket.Connect(ip, port);
        }

        private void OnConnectSuccess() {
            UnityEngine.Debug.Log("Connect To Server Success");
        }

        private void OnReceiveMessageCompleted(Packet p) {
            int netType = p.ReadInt32();
            if (_modules.TryGetValue(netType, out NetModule module)) {
                module.OnReceiveMessage(p);
            }
            else {
                UnityEngine.Debug.LogError("Can't Find Module By " + netType);
            }
        }

        private void OnSendMessageFailed(Packet p, SocketError error) {
            int netType = p.ReadInt32();
            if (_modules.TryGetValue(netType, out NetModule module)) {
                module.OnSendMessageFaile(p, error);
            }
            else {
                UnityEngine.Debug.LogError("Can't Find Module By " + netType);
            }
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
            if (!_modules.TryGetValue(moduleID, out NetModule netModule))
            {
                UnityEngine.Debug.LogError("Get Module Failed " + netModule.GetModuleID());
            }
            return netModule;
        }

        public Packet Malloc(int size) {
            return _packetManager.Malloc(size);
        }

        public void SendMessage(NetMessage message) {
            if (IsValid) {
                _socket.SendMessage(message.ToPacket(this));
                UnityEngine.Debug.Log("========>Send Message By Socket");
            }
        }

        public void Update() {
            Queue<Packet> packets = _packetManager.GetPackets();
            { 
                var it = packets.GetEnumerator();
                while (it.MoveNext()) {
                    Packet p = it.Current;
                    OnReceiveMessageCompleted(p);
                }
                _packetManager.CleanPackets(packets);
            }
            
            // 更新网络模块
            {
                var it = _modules.GetEnumerator();
                while (it.MoveNext())
                {
                    it.Current.Value.Update();
                }
            }
            
        }

        public void Close() {
            if (IsValid)
                _socket.Close();
        }
    }
}
