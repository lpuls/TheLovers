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

        public override Packet ToPacket(INetDevice netDevice) {
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

        private float _interval = 0;
        private bool _sendEnable = false;


        private int _frame = 0;
        private float _sendTime = 0;
        private NetPingMessage _netPingMessage = new NetPingMessage();

        public float Ping {
            get;
            private set;
        }

        public override int GetModuleID() {
            return NET_PING_MODULE_ID;
        }

        public override void Initialize(INetDevice device) {
            base.Initialize(device);
            _sendTime = Time.realtimeSinceStartup;
        }

        public override void OnReceiveMessage(Packet p) {
            float time = Time.realtimeSinceStartup;
            Ping = time - _sendTime;
            _sendEnable = true;
            Debug.Log("Ping " + Ping + " RTT " + (Ping / 2) + " Frame " + p.ReadInt32());

            // 重发ping消息包
            // SendPingMessage();
        }

        public void SendPingMessage() {
            _sendTime = Time.realtimeSinceStartup;
            _netPingMessage.Time = _sendTime;
            _netPingMessage.Frame = _frame;
            UnityEngine.Debug.Log(string.Format("=========> SendPingMessage {0}, {1}", _netPingMessage.Time, _netPingMessage.Frame));
            _frame++;
            _device.SendMessage(_netPingMessage);
        }

        public override void OnSendMessageFaile(Packet p, SocketError error) {
            Debug.Log("error " + error);
        }

        public override void Update()
        {
            if (!_sendEnable)
                return;

            _interval += Time.deltaTime;
            if (_interval >= Max_INTERVAL) { 
                _interval -= Max_INTERVAL;
                SendPingMessage();
                _sendEnable = false;
            }
        }
    }
}
