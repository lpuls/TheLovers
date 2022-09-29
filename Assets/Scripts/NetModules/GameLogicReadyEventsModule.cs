using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Hamster.SpaceWar {

    public enum EGameLogicEventResult {
        ReadyEventResult_Success = 0,
        ReadyEventResult_SpawnFailed = 1
    };

    public enum EGameLogicEventID {
        None = 0,
        C2S_SpawnShip = 1,
        S2C_SpawnShip = 2,
        S2C_EventFaield = 3,
        
        C2S_AllReady = 4,
        S2C_StartGame = 5
    }

    public class C2SSpawnShipMessage : MutileMessage, IPool {

        public int SpawnShipID = 0;

        public override int MessageType {
            get {
                return (int)EGameLogicEventID.C2S_SpawnShip;
            }
        }

        public override int NetModuleID {
            get {
                return ServerGameLogicEventModule.SERVER_NET_GAME_LOGIC_READY_EVENT_ID;
            }
        }

        protected override int PacketSize => base.PacketSize + sizeof(int);

        public override Packet ToPacket(IPacketMallocer mallocer) {
            Packet packet = base.ToPacket(mallocer);
            packet.WriteInt32(SpawnShipID);
            return packet;
        }

        public void Reset() {
            SpawnShipID = 0;
        }
    }

    public class S2CEventFaieldMessage : MutileMessage, IPool {
        public EGameLogicEventResult Result = EGameLogicEventResult.ReadyEventResult_Success;
        public EGameLogicEventID EventID = EGameLogicEventID.None;

        public void Reset() {
            Result = EGameLogicEventResult.ReadyEventResult_Success;
            EventID = EGameLogicEventID.None;
        }
    }

    public class S2CSpawnShipMessage : MutileMessage, IPool {

        public struct SpawnShipInfo {
            public int ConfigID;
            public int NetID;
            public float X;
            public float Y;
            public bool UserShip;
        }

        private List<SpawnShipInfo> _shipInfos = new List<SpawnShipInfo>();

        public override int MessageType {
            get {
                return (int)EGameLogicEventID.S2C_SpawnShip;
            }
        }

        public override int NetModuleID {
            get {
                return ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID;
            }
        }

        protected override int PacketSize => base.PacketSize + sizeof(int) + (sizeof(int) * 2 + sizeof(float) * 2 + sizeof(bool)) * _shipInfos.Count;

        public void AddShipInfo(int configID, int netID, float x, float y, bool userShip) {
            _shipInfos.Add(new SpawnShipInfo { 
                ConfigID = configID,
                NetID = netID,
                X = x,
                Y = y,
                UserShip = userShip
            });
        }

        public override Packet ToPacket(IPacketMallocer mallocer) {
            Packet packet = base.ToPacket(mallocer);
            packet.WriteInt32(_shipInfos.Count);
            var it = _shipInfos.GetEnumerator();
            while (it.MoveNext()) {
                SpawnShipInfo info = it.Current;
                packet.WriteInt32(info.ConfigID);
                packet.WriteInt32(info.NetID);
                packet.WriteFloat(info.X);
                packet.WriteFloat(info.Y);
                packet.WriteBool(info.UserShip);
            }
            return packet;
        }

        public void Reset() {
            _shipInfos.Clear();
        }
    }

    public class C2SAllReadyMessage : MutileMessage, IPool {

        public override int MessageType {
            get {
                return (int)EGameLogicEventID.C2S_AllReady;
            }
        }

        public override int NetModuleID {
            get {
                return ServerGameLogicEventModule.SERVER_NET_GAME_LOGIC_READY_EVENT_ID;
            }
        }

        protected override int PacketSize => base.PacketSize + sizeof(int); 
        public override Packet ToPacket(IPacketMallocer mallocer) {
            Packet packet = base.ToPacket(mallocer);
            packet.WriteInt32(0);
            return packet;
        }

        public void Reset() {
        }
    }

    public class S2CStartGameMessage : MutileMessage, IPool {

        public override int MessageType {
            get {
                return (int)EGameLogicEventID.S2C_StartGame;
            }
        }

        public override int NetModuleID {
            get {
                return ClientGameLogicEventModule.CLIENT_NET_GAME_LOGIC_READY_EVENT_ID;
            }
        }

        public void Reset() {
        }
    }

    public class ClientGameLogicEventModule : ClientMutileMessageModule {
        public const int CLIENT_NET_GAME_LOGIC_READY_EVENT_ID = 3;

        public ClientGameLogicEventModule() {
            Register((int)EGameLogicEventID.S2C_SpawnShip, OnReceiveSpawnShipResponMessage);
            Register((int)EGameLogicEventID.S2C_StartGame, OnReceiveGameStartMessage);
        }

        public override int GetModuleID() {
            return CLIENT_NET_GAME_LOGIC_READY_EVENT_ID;
        }

        // 收到服务端回应的创建战机事件
        private void OnReceiveSpawnShipResponMessage(Packet packet) {
            int shipCount = packet.ReadInt32();
            for (int i = 0; i < shipCount; i++) {
                int configID = packet.ReadInt32();  // todo 该值是用于创建对应的角色的，现在先写死
                int netID = packet.ReadInt32();
                float x = packet.ReadFloat();
                float y = packet.ReadFloat();
                bool userShip = packet.ReadBool();

                // 创建角色
                GameObject ship = GameLogicUtility.ClientCreateShip(configID, netID, new Vector3(x, y, 0));
                if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                    if (userShip)
                        netSyncComponent.SetAutonomousProxy();
                    else
                        netSyncComponent.SetSimulatedProxy();
                }
                ship.AddComponent<NetMovementComponent>();
                if (userShip) {
                    ship.AddComponent<NetPlayerController>();
                }
            }

            RequestReadyToServer();
        }

        private void OnReceiveGameStartMessage(Packet packet) {
            Debug.Log("========>GameStart");
        }

        public void RequestReadyToServer() {
            C2SAllReadyMessage c2SAllReadyMessage = ObjectPool<C2SAllReadyMessage>.Malloc();
            _device.SendMessage(c2SAllReadyMessage);
            ObjectPool<C2SAllReadyMessage>.Free(c2SAllReadyMessage);
        }

        public void RequestSpawnShipToServer(int configID) {
            C2SSpawnShipMessage message = ObjectPool<C2SSpawnShipMessage>.Malloc();
            message.SpawnShipID = configID;
            _device.SendMessage(message);
            ObjectPool<C2SSpawnShipMessage>.Free(message);
        }
    }

    public class ServerGameLogicEventModule : ServerMutileMessageModule {
        public const int SERVER_NET_GAME_LOGIC_READY_EVENT_ID = 4;

        public override int GetModuleID() {
            return SERVER_NET_GAME_LOGIC_READY_EVENT_ID;
        }

        public ServerGameLogicEventModule() {
            Register((int)EGameLogicEventID.C2S_SpawnShip, OnReceiveSpawnShipRequestMessage);
            Register((int)EGameLogicEventID.C2S_AllReady, OnReceiveAllReadyRequestMessage);
        }

        // 收到客户端准备完成的消息
        private void OnReceiveAllReadyRequestMessage(Packet packet, ClientInstance clientInstance) {
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            UnityEngine.Debug.Assert(!frameDataManager.IsGameStart, "Game Is Started");

            frameDataManager.CurrentPlayerCount++;
            if (frameDataManager.CurrentPlayerCount >= frameDataManager.MaxPlayerCount) {
                frameDataManager.IsGameStart = true;
                BroadcastGameStart();
            }
        }

        // 收到客户端请求的创建战机事件
        private void OnReceiveSpawnShipRequestMessage(Packet packet, ClientInstance clientInstance) {
            int shipID = packet.ReadInt32();

            GameObject ship = GameLogicUtility.ServerInitShip(shipID, false);
            if (null != ship && ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                clientInstance.UserData = netSyncComponent.NetID;
                ResponSpawnShipToClients(shipID, netSyncComponent.NetID, ship.transform.position);
            }
            else {
                ResponSpawnShipFialedToClient(EGameLogicEventResult.ReadyEventResult_SpawnFailed, clientInstance);
            }
        }

        public void ResponSpawnShipFialedToClient(EGameLogicEventResult result, ClientInstance client) {
            S2CEventFaieldMessage message = ObjectPool<S2CEventFaieldMessage>.Malloc();
            message.Result = result;
            message.EventID = EGameLogicEventID.C2S_SpawnShip;
            client.SendMessage(message);
            ObjectPool<S2CEventFaieldMessage>.Free(message);
        }

        public void ResponSpawnShipToClients(int configID, int netID, Vector3 location) {
            S2CSpawnShipMessage message = ObjectPool<S2CSpawnShipMessage>.Malloc();

            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            var netActors = frameDataManager.GetAllNetActor();
            var it = netActors.GetEnumerator();
            while (it.MoveNext()) {
                NetSyncComponent netSyncComponent = it.Current.Value;
                GameObject ship = netSyncComponent.gameObject;
                message.AddShipInfo(netSyncComponent.ConfigID, netSyncComponent.NetID,
                    ship.transform.position.x,
                    ship.transform.position.y,
                    netSyncComponent.NetID == netID);
            }
            _device.SendMessage(message);
            ObjectPool<S2CSpawnShipMessage>.Free(message);
        }

        public void BroadcastGameStart() {
            S2CStartGameMessage message = ObjectPool<S2CStartGameMessage>.Malloc();
            _device.SendMessage(message);
            ObjectPool<S2CStartGameMessage>.Free(message);
        }
    }

}
