using System;
using System.Net.Sockets;

namespace Hamster {
    public abstract class NetModule {

        protected INetDevice _device = null;

        public virtual void Initialize(INetDevice device) {
            _device = device;
        }

        public virtual int GetModuleID() {
            return 0;
        }

        public abstract void OnReceiveMessage(Packet p);

        public abstract void OnSendMessageFaile(Packet p, SocketError error);

    }
}
