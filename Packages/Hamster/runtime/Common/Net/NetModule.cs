using System;
using System.Net.Sockets;

namespace Hamster {
    public abstract class NetModule {

        protected INetDevice _device = null;

        public virtual void Initialize(INetDevice device) {
            _device = device;
        }

        public virtual int GetModuleID() {
            throw new NotImplementedException();
        }

        // 客户端接收服务器的数据处理
        public abstract void OnReceiveServerMessage(Packet p);

        // 服务端接收客户端数据处理
        public abstract void OnReceiveClientMessage(Packet p, ClientInstance inst);

        public abstract void OnSendMessageFaile(Packet p, SocketError error);

        public virtual void Update() { }
    }

}
