using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Hamster {
    using ServerMessageProcessor = Action<Packet, ClientInstance>;
    using CilentMessageProcessor = Action<Packet>;

    public class MutileMessage : NetMessage {

        public virtual int MessageType {
            get;
        } 

        public virtual int NetModuleID {
            get;
        }

        protected virtual int PacketSize {
            get {
                return sizeof(int) * 2;
            }
        }

        public override Packet ToPacket(IPacketMallocer mallocer) {
            UnityEngine.Debug.Assert(PacketSize >= sizeof(int) * 2, "Mutile Message Packet Too Small");
            Packet packet = mallocer.Malloc(PacketSize);
            packet.WriteInt32(NetModuleID);
            packet.WriteInt32(MessageType);
            return packet;
        }
    }

    public class ServerMutileMessageModule : NetModule {
        protected Dictionary<int, ServerMessageProcessor> _processor = 
            new Dictionary<int, ServerMessageProcessor>(new Int32Comparer());

        public void Register(int messageType, ServerMessageProcessor processor) {
            UnityEngine.Debug.Assert(!_processor.ContainsKey(messageType), "Reregister message processor");
            _processor.Add(messageType, processor);
        }

        public override void OnReceiveClientMessage(Packet p, ClientInstance inst) {
            int messageType = p.ReadInt32();
            if (_processor.TryGetValue(messageType, out ServerMessageProcessor processor)) {
                processor?.Invoke(p, inst);
            }
        }

        public override void OnReceiveServerMessage(Packet p) {
            UnityEngine.Debug.LogError("Server Net Module Receive Client Message error ");
        }

        public override void OnSendMessageFaile(Packet p, SocketError error) {
            UnityEngine.Debug.LogError("Send Message error " + error);
        }
    }

    public class ClientMutileMessageModule : NetModule {
        protected Dictionary<int, CilentMessageProcessor> _processor =
            new Dictionary<int, CilentMessageProcessor>(new Int32Comparer());

        public void Register(int messageType, CilentMessageProcessor processor) {
            UnityEngine.Debug.Assert(!_processor.ContainsKey(messageType), "Reregister message processor");
            _processor.Add(messageType, processor);
        }

        public override void OnReceiveClientMessage(Packet p, ClientInstance inst) {
            UnityEngine.Debug.LogError("Client Net Module Receive Server Message error ");
        }

        public override void OnReceiveServerMessage(Packet p) {
            int messageType = p.ReadInt32();
            if (_processor.TryGetValue(messageType, out CilentMessageProcessor processor)) {
                processor?.Invoke(p);
            }
        }

        public override void OnSendMessageFaile(Packet p, SocketError error) {
            UnityEngine.Debug.LogError("Send Message error " + error);
        }
    }
}
