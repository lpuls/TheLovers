namespace Hamster {

    public interface IPacketMallocer {
        Packet Malloc(int size);

        void Free(Packet packet);
    };

    public abstract class NetMessage {

        public virtual int NetMessageID {
            get {
                return 0;
            } 
        }

        public abstract Packet ToPacket(IPacketMallocer netDevice);
    }
}
