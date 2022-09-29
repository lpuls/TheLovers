using System.Collections.Generic;
using System.Net.Sockets;

namespace Hamster {
    public class ClientNetDevice : NetDevice {
        private ClientSocket _socket = null;

        public bool IsValid {
            get {
                return _socket.IsConnect();
            }
        }

        public override bool IsServer() {
            return false;
        }

        public ClientNetDevice() {
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

        private void OnSendMessageFailed(Packet p, SocketError error) {
            int netType = p.ReadInt32();
            if (_modules.TryGetValue(netType, out NetModule module)) {
                module.OnSendMessageFaile(p, error);
            }
            else {
                UnityEngine.Debug.LogError("Can't Find Module By " + netType);
            }
        }

        public override void SendMessage(NetMessage message) {
            if (IsValid) {
                _socket.SendMessage(message.ToPacket(_packetManager));
            }
        }

        protected void OnReceiveMessageCompleted(Packet p) {
            int netType = p.ReadInt32();
            if (_modules.TryGetValue(netType, out NetModule module)) {
                module.OnReceiveServerMessage(p);
            }
            else {
                UnityEngine.Debug.LogError("Can't Find Module By " + netType);
            }
        }

        public override void Update() {
            Queue<Packet> packets = _packetManager.GetPackets();
            {
                var it = packets.GetEnumerator();
                while (it.MoveNext()) {
                    Packet p = it.Current;
                    OnReceiveMessageCompleted(p);
                }
                _packetManager.CleanPackets(packets);
            }

            base.Update();
        }

        public override void Close() {
            if (IsValid)
                _socket.Close();
        }
    }
}
