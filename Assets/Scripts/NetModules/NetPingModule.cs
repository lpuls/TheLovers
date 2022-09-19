using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetPingMessage : NetMessage {
        public const int NET_PING_MESSAGE_ID = 1;

        public int Frame = 0;
        public float Time = 0;

        public override int NetMessageID {
            get {
                return NET_PING_MESSAGE_ID;
            }
        }

        public override Packet ToPacket(IPacketMallocer netDevice) {
            Packet packet = netDevice.Malloc(24);
            packet.WriteInt32(NetPingMessage.NET_PING_MESSAGE_ID);
            packet.WriteFloat(Time);
            packet.WriteInt32(Frame);
            return packet;
        }
    }

    public class NetPingModule : NetModule {

        public const int NET_PING_MODULE_ID = 1;
        private const float Max_INTERVAL = .3f;
        private const float CONNECT_TIME_OUT = 5.0f;

        private float _interval = 0;
        private bool _sendEnable = false;


        // 通用数据
        private int _frame = 0;
        private NetPingMessage _netPingMessage = new NetPingMessage();

        // 服务端数据
        private Dictionary<int, float> _clientLastPingTime = new Dictionary<int, float>(new Int32Comparer());


        public NetPingModule() {
        }

        public float Ping {
            get;
            private set;
        }

        public override int GetModuleID() {
            return NET_PING_MODULE_ID;
        }

        public override void Initialize(INetDevice device) {
            base.Initialize(device);
            _sendEnable = !device.IsServer();
        }

        public override void OnReceiveServerMessage(Packet p) {
            float sendTime = p.ReadFloat();
            int frame = p.ReadInt32();

            // 客户端收到数据之后，当前时间减去服务端转发回来的发送时间除2就是ping
            float time = Time.realtimeSinceStartup;
            Ping = time - sendTime;
            _sendEnable = true;
            Debug.Log("[Client] Ping " + Ping + " RTT " + (Ping / 2) + " Frame " + frame);
        }

        public override void OnReceiveClientMessage(Packet p, ClientInstance inst) {
            float sendTime = p.ReadFloat();
            int frame = p.ReadInt32();

            // 服务端收到客户端信息之后，只要将当前时间减去发送时间就能得到ping
            float time = Time.realtimeSinceStartup;
            Ping = time - sendTime;
            Debug.Log("[Server] Ping " + Ping + " RTT " + Ping + " Frame " + frame + " Client " + inst.UserData);

            // 更新该客户端收到消息的时间
            _clientLastPingTime[inst.UserData] = time;

            SendServerPingMessage(inst, sendTime);
        }

        public void SendClientPingMessage() {
            _netPingMessage.Time = Time.realtimeSinceStartup;
            _netPingMessage.Frame = _frame;
            UnityEngine.Debug.Log(string.Format("[Client] =========> SendPingMessage {0}, {1}", _netPingMessage.Time, _netPingMessage.Frame));
            _frame++;
            _device.SendMessage(_netPingMessage);
        }

        public void SendServerPingMessage(ClientInstance inst, float receiveTime) {
            _netPingMessage.Time = receiveTime;
            _netPingMessage.Frame = _frame;
            UnityEngine.Debug.Log(string.Format("[Server] =========> SendPingMessage {0}, {1}", _netPingMessage.Time, _netPingMessage.Frame));

            inst.SendMessage(_netPingMessage);
        }

        public override void OnSendMessageFaile(Packet p, SocketError error) {
            Debug.Log("error " + error);
        }


        public override void Update()
        {
            // 服务端检查太久没有发送消息过来的客户端，超过5S视为超时
            if (_device.IsServer()) {
                ServerNetDevice serverNetDevice = _device as ServerNetDevice;
                var clients = serverNetDevice.GetAllClients();
                var it = clients.GetEnumerator();
                float now = Time.realtimeSinceStartup;
                while (it.MoveNext()) {
                    int key = it.Current.Key;
                    if (_clientLastPingTime.TryGetValue(key, out float value)) {
                        if (value - now > CONNECT_TIME_OUT) {
                            serverNetDevice.DisconnectClient(key);
                            _clientLastPingTime.Remove(key);
                        }
                    }
                    else {
                        _clientLastPingTime[key] = now;
                    }
                }

            }
            else {
                if (!_sendEnable)
                    return;

                _interval += Time.deltaTime;
                if (_interval >= Max_INTERVAL) { 
                    _interval -= Max_INTERVAL;
                    SendClientPingMessage();
                    _sendEnable = false;
                }
            }
        }
    }
}
