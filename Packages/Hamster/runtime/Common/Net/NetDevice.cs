using System.Collections.Generic;
using System.Net.Sockets;

namespace Hamster {

    public interface INetDevice {
        Packet Malloc(int size);
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
            _socket.OnReceiveMessageCompleted += OnReceiveMessageCompleted;
        }

        public void Connect(string ip, int port) {
            _socket.Connect(ip, port);
        }

        private void OnConnectSuccess() {
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

        public Packet Malloc(int size) {
            return _packetManager.Malloc(size);
        }

        public void SendMessage(NetMessage message) {
            if (IsValid)
                _socket.SendMessage(message.ToPacket(this));
        }

        public void Close() {
            if (IsValid)
                _socket.Close();
        }
    }
}
