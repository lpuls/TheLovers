using System;
using System.Net.Sockets;

namespace Hamster.SpaceWar
{
    public class GameLogicSyncMessage : NetMessage
    {
        public const int NET_GAME_LOGIC_SYNC_ID = 2;

        public int Operator = 0;

        public override int NetMessageID
        {
            get
            {
                return NET_GAME_LOGIC_SYNC_ID;
            }
        }

        public override Packet ToPacket(INetDevice netDevice)
        {
            Packet packet = netDevice.Malloc(24);
            packet.WriteInt32(GameLogicSyncMessage.NET_GAME_LOGIC_SYNC_ID);
            packet.WriteInt32(Operator);
            return packet;
        }
    }

    public class GameLogicSyncModule : NetModule
    {

        public const int NET_GAME_LOGIC_SYNC_ID = 2;

        private GameLogicSyncMessage _gameLogicSyncMessage = new GameLogicSyncMessage();

        public override int GetModuleID() {
            return NET_GAME_LOGIC_SYNC_ID;
        }

        public override void OnReceiveMessage(Packet p)
        {
            UnityEngine.Debug.Log(string.Format("Receive Logic Mirror Data: {0}", p.GetLength()));
            FrameDataManager frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
            int dataSize = p.ReadInt32();
            byte[] byteArray = p.ReadBytes(dataSize);
            frameDataManager.AnalyzeBinary(byteArray);
        }

        public override void OnSendMessageFaile(Packet p, SocketError error)
        {
            UnityEngine.Debug.Log("error " + error);
        }

        public void SendOperator(int op)
        {
            _gameLogicSyncMessage.Operator = op;

            _device.SendMessage(_gameLogicSyncMessage);
        }

    }
}
