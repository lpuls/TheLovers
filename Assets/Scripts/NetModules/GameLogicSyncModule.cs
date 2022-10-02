using System;
using System.IO;
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

        public override Packet ToPacket(IPacketMallocer mallocer)
        {
            Packet packet = mallocer.Malloc(24);
            packet.WriteInt32(GameLogicSyncMessage.NET_GAME_LOGIC_SYNC_ID);
            packet.WriteInt32(Operator);
            return packet;
        }
    }

    public class S2CGameFrameDataSyncMessage : NetMessage {

        public FrameData SendFrameData = null;
        private Packet _sendPacket = new Packet(2048);

        private bool _needUpdateDatas = false;

        public void UpdateData() {
            _needUpdateDatas = true;
        }

        public override Packet ToPacket(IPacketMallocer mallocer) {
            if (_needUpdateDatas) {
                _sendPacket.Clean();
                _sendPacket.WriteInt32(GameLogicSyncMessage.NET_GAME_LOGIC_SYNC_ID);
                _sendPacket.WriteInt32(0);
                _sendPacket.WriteInt32(SendFrameData.FrameIndex);

                _sendPacket.WriteInt32(SendFrameData.SpawnInfos.Count);
                foreach (var it in SendFrameData.SpawnInfos) {
                    it.Write(_sendPacket);
                }
                _sendPacket.WriteInt32(SendFrameData.DestroyInfos.Count);
                foreach (var it in SendFrameData.DestroyInfos) {
                    it.Write(_sendPacket);
                }
                _sendPacket.WriteInt32(SendFrameData.UpdateInfos.Count);
                foreach (var it in SendFrameData.UpdateInfos) {
                    _sendPacket.WriteInt32(it.Key);
                    _sendPacket.WriteInt32(it.Value.Count);
                    for (int i = 0; i < it.Value.Count; i++) {
                        it.Value[i].Write(_sendPacket);
                    }
                }

                //_sendPacket.WriteInt32(SendFrameData.PlayerInfos.Count);
                //foreach (var it in SendFrameData.PlayerInfos) {
                //    it.Write(_sendPacket);
                //}
                //_sendPacket.WriteInt32(SendFrameData.SpawnActorInfos.Count);
                //foreach (var it in SendFrameData.SpawnActorInfos) {
                //    it.Write(_sendPacket);
                //}

                // 往头部写入长度
                int size = _sendPacket.Size;
                _sendPacket.Peek(sizeof(int));
                _sendPacket.WriteInt32(size);
            }

            return _sendPacket;
        }
    }

    public class GameLogicSyncModule : NetModule {

        public const int NET_GAME_LOGIC_SYNC_ID = 2;

        private GameLogicSyncMessage _gameLogicSyncMessage = new GameLogicSyncMessage();

        public override int GetModuleID() {
            return NET_GAME_LOGIC_SYNC_ID;
        }

        public override void OnReceiveServerMessage(Packet p)
        {
            UnityEngine.Debug.Log(string.Format("Receive Logic Mirror Data: {0}", p.GetLength()));
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>() as ClientFrameDataManager;
            int dataSize = p.ReadInt32();
            byte[] byteArray = p.ReadBytes(dataSize);
            frameDataManager.AnalyzeBinary(byteArray);
        }

        public override void OnReceiveClientMessage(Packet p, ClientInstance inst) {
            int playerInput = p.ReadInt32();
            UnityEngine.Debug.Log("Receive Player Input " + inst.UserData + ", " + playerInput);
            GameLogicUtility.SetPlayerOperator(inst.UserData, playerInput);
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
