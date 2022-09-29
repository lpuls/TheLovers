using System.Collections.Generic;
using System.Net.Sockets;

namespace Hamster {
    public class ServerNetDevice : NetDevice {

        private ServerSocket _socket = null;
        private Queue<Packet> _pendingPackets = new Queue<Packet>();
        private int _clientIndex = 1;
        private Dictionary<int, ClientInstance> _clients = new Dictionary<int, ClientInstance>(new Int32Comparer());
        private HashSet<ClientInstance> _pendingKillClient = new HashSet<ClientInstance>();

        public bool IsValid {
            get {
                return _socket.IsListen();
            }
        }

        public override bool IsServer() {
            return true;
        }

        public ServerNetDevice() {
            _socket = new ServerSocket(this);
            _socket.OnAcceptClient += OnAcceptClient;
            _socket.OnCloseClient += OnCloseClient;
        }

        public void Listen(string ip, int port, int listenCount=10) {
            _socket.Listen(ip, port, listenCount);
        }

        public override void SendMessage(NetMessage message) {
            _socket.Broakcast(message);
        }

        private void OnAcceptClient(ClientInstance inst) {
            inst.ServerNetDevice = this;
            inst.CreateIndex = _clientIndex++;
            inst.OnReceiveMessage += OnReceiveMessage;
            inst.OnSendComplete += OnSendMessage;
            _clients.Add(inst.CreateIndex, inst);
        }

        private void OnReceiveMessage(ClientInstance inst) {
            //Queue<Packet> packets = inst.GetReceivePackets();
            //var it = packets.GetEnumerator();
            //while (it.MoveNext()) {
            //    _pendingPackets.Enqueue(it.Current);
            //}
        }

        private void OnSendMessage(ClientInstance inst, SocketError socketError, Packet sendPacket) {
            _packetManager.Free(sendPacket);
        }

        private void OnCloseClient(ClientInstance inst) {
            if (!_clients.ContainsKey(inst.CreateIndex))
                UnityEngine.Debug.LogError("IP " + inst.IP + " never be manager");
            _clients.Remove(inst.CreateIndex);
        }

        protected void OnReceiveMessageCompleted(Packet p, ClientInstance inst) {
            int netType = p.ReadInt32();
            if (_modules.TryGetValue(netType, out NetModule module)) {
                module.OnReceiveClientMessage(p, inst);
            }
            else {
                UnityEngine.Debug.LogError("Can't Find Module By " + netType);
            }
        }

        public override void Update() {
            // 更新收到数据包的客户端
            {
                var it = _clients.GetEnumerator();
                while (it.MoveNext()) {
                    ClientInstance inst = it.Current.Value;
                    Queue<Packet> packets = inst.GetReceivePackets();

                    if (packets.Count > 0) {
                        var packetIT = packets.GetEnumerator();
                        while (packetIT.MoveNext()) {
                            Packet p = packetIT.Current;
                            OnReceiveMessageCompleted(p, inst);
                        }
                        inst.CleanReceivePackets(packets);
                    }
                }
            }

            // 清理离线客户端
            {
                var it = _pendingKillClient.GetEnumerator();
                while (it.MoveNext()) {
                    ClientInstance clientInstance = it.Current;
                    _socket.Disconnect(clientInstance);
                    _clients.Remove(clientInstance.CreateIndex);
                }
            }

            base.Update();
        }

        public void DisconnectClient(int id) {
            if (_clients.TryGetValue(id, out ClientInstance instance)) {
                _pendingKillClient.Add(instance);

            }
        }

        public Dictionary<int, ClientInstance> GetAllClients() {
            return _clients;
        }

        public override void Close() {
            if (null != _socket)
                _socket.Close();
        }

    }
}
