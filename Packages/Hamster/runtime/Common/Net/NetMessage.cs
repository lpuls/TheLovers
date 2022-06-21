namespace Hamster {
    public abstract class NetMessage {
        public virtual int NetMessageID {
            get {
                return 0;
            } 
        }

        public abstract Packet ToPacket(INetDevice netDevice);
    }
}
