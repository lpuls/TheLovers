using System.Net.Sockets;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EReadEventID {
        None = 0,
        SpawnShip = 1,
        AllPlayerReady = 2,
        OtherPlayerSpawnShip = 3,
    }

    enum EReadyEventResult {
        ReadyEventResult_Success = 0,
        ReadyEventResult_SpawnFailed = 1
    };

    public class GameLogicReadyEventMessage : NetMessage {
        public const int NET_GAME_LOGIC_READY_EVENT_ID = 3;

        public EReadEventID EventID = EReadEventID.None;
        public int SpawnShipID = 0;

        public override int NetMessageID {
            get {
                return NET_GAME_LOGIC_READY_EVENT_ID;
            }
        }

        public override Packet ToPacket(INetDevice netDevice) {
            Packet packet = null;

            switch (EventID) {
                case EReadEventID.None:
                    break;
                case EReadEventID.SpawnShip: {
                        packet = netDevice.Malloc(sizeof(int) * 3);
                        packet.WriteInt32(GameLogicReadyEventMessage.NET_GAME_LOGIC_READY_EVENT_ID);
                        packet.WriteInt32((int)EventID);
                        packet.WriteInt32(SpawnShipID);
                    }
                    break;
                default:
                    break;
            }

            if (null == packet) {
                UnityEngine.Debug.LogError(string.Format("GameLogicReadyEventMessage Malloc Packet Failed %d", EventID));
            }

            return packet;
        }
    }


    public class GameLogicReadyEventsModule : NetModule {
        public const int NET_GAME_LOGIC_READY_EVENT_ID = 3;

        private GameLogicReadyEventMessage _readEventMessage = new GameLogicReadyEventMessage();

        public override int GetModuleID() {
            return NET_GAME_LOGIC_READY_EVENT_ID;
        }

        public override void OnReceiveMessage(Packet p) {
            EReadEventID eventID = (EReadEventID)p.ReadInt32();
            EReadyEventResult result = (EReadyEventResult)p.ReadInt32();
            UnityEngine.Debug.Log(string.Format("GameLogicReadyEvents OnReceiveMessage {0}, {1}", eventID, result));

            // 不成功就不走后续操作了
            if (EReadyEventResult.ReadyEventResult_Success != result)
                return;

            switch (eventID) {
                case EReadEventID.None:
                    break;
                case EReadEventID.SpawnShip:
                case EReadEventID.OtherPlayerSpawnShip: {
                        int configID = p.ReadInt32();  // todo 该值是用于创建对应的角色的，现在先写死
                        int netID = p.ReadInt32(); 
                        float x = p.ReadFloat();
                        float y = p.ReadFloat();
                        float z = p.ReadFloat();

                        // 创建角色
                        GameObject ship = CreateShip(configID, netID, new Vector3(x, y, z));
                        if (null != ship && eventID == EReadEventID.SpawnShip)
                            ship.AddComponent<LocalPlayerController>();
                    }
                    break;
                case EReadEventID.AllPlayerReady:
                    break;
                default:
                    break;
            }
        }

        private GameObject CreateShip(int configID, int netID, Vector3 position) {
            FrameDataManager frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
            if (!frameDataManager.HasNetObject(netID, 0))
                return frameDataManager.SpawnNetObject(netID, 0, "Res/Ships/GreyShip", position);

            return null;
        }

        public override void OnSendMessageFaile(Packet p, SocketError error) {
            UnityEngine.Debug.LogError("GameLogicReadyEvents error " + error);
        }

        public void SendSpawnShipID(int id) {
            _readEventMessage.SpawnShipID = id;
            _readEventMessage.EventID = EReadEventID.SpawnShip;
            _device.SendMessage(_readEventMessage);
        }
    }
}
