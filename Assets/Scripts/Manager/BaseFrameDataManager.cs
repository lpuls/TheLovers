using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class INetInfo : IPool {
        public int ID;
        public int OwnerID;
        public float X;
        public float Y;
        public float Angle;

        public int GetUniqueID() {
            return OwnerID << 16 | ID;
        }

        public virtual void Read(BinaryReader binaryReader) {
        }

        public virtual void Write(Packet packet) {
        }

        public virtual void Reset() {
            ID = 0;
            OwnerID = 0;
            X = 0;
            Y = 0;
            Angle = 0;
        }

        public virtual void Free() {
        }
    }

    public class PlayerInfo : INetInfo {
        public int Health;
        public int Tags;

        public override void Read(BinaryReader binaryReader) {
            int temp = binaryReader.ReadInt32();
            Tags = temp & 0x3FFFFF;
            Health = (temp >> 22) & 0x7F;
            ID = temp >> 29;
            X = binaryReader.ReadSingle();
            Y = binaryReader.ReadSingle();
            Angle = binaryReader.ReadSingle();
        }

        public override void Write(Packet packet) {
            int temp = 0;
            temp |= ID;
            temp = (temp << 7) | Health;
            temp |= (temp << 22) | Tags;
            packet.WriteInt32(temp);
            packet.WriteFloat(X);
            packet.WriteFloat(Y);
            packet.WriteFloat(Angle);
        }


        public override string ToString() {
            return string.Format("ID: {0}, Position: ({1}, {2}), Angle: {3}", ID, X, Y, Angle);
        }

        public override void Reset() {
            base.Reset();
            Health = 0;
            Tags = 0;
        }

        public static PlayerInfo Malloc() {
            return ObjectPool<PlayerInfo>.Malloc();
        }

        public override void Free() {
            ObjectPool<PlayerInfo>.Free(this);
        }
    }

    public class SpawnActorInfo : INetInfo {
        public override void Read(BinaryReader binaryReader) {
            int temp = binaryReader.ReadInt32();
            ID = temp & 0xFFFF;
            OwnerID = temp >> 16;
            X = binaryReader.ReadSingle();
            Y = binaryReader.ReadSingle();
            Angle = binaryReader.ReadSingle();
        }

        public override void Write(Packet packet) {
            int temp = 0;
            temp |= OwnerID;
            temp = (temp << 16) | ID;
            packet.WriteInt32(temp);
            packet.WriteFloat(X);
            packet.WriteFloat(Y);
            packet.WriteFloat(Angle);
        }

        public override string ToString() {
            return string.Format("ID: {0}: {4}, Position: ({1}, {2}), Angle: {3}", ID, X, Y, Angle, OwnerID);
        }

        public override void Reset() {
            base.Reset();
        }

        public static SpawnActorInfo Malloc() {
            return ObjectPool<SpawnActorInfo>.Malloc();
        }

        public override void Free() {
            ObjectPool<SpawnActorInfo>.Free(this);
        }
    }

    public class FrameData : IPool {
        public int FrameIndex;
        public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
        public List<SpawnActorInfo> SpawnActorInfos = new List<SpawnActorInfo>();
        public Dictionary<int, INetInfo> NetInfoDict = new Dictionary<int, INetInfo>(new Int32Comparer());

        public void Reset() {
            FrameIndex = 0;
            {
                foreach (var it in PlayerInfos) {
                    ObjectPool<PlayerInfo>.Free(it);
                }
                foreach (var it in SpawnActorInfos) {
                    ObjectPool<SpawnActorInfo>.Free(it);
                }
                PlayerInfos.Clear();
                SpawnActorInfos.Clear();
            }

            {
                var it = NetInfoDict.GetEnumerator();
                while (it.MoveNext()) {
                    INetInfo netInfo = it.Current.Value;
                    netInfo.Free();
                }
                NetInfoDict.Clear();
            }
        }

        public static FrameData Malloc(int frameIndex) {
            FrameData frameData = ObjectPool<FrameData>.Malloc();
            frameData.FrameIndex = frameIndex;
            return frameData;
        }

        public void Free() {
            
            ObjectPool<FrameData>.Free(this);
        }

        public override string ToString() {
            string log = "";
            foreach (var it in PlayerInfos) {
                log += it.ToString();
            }
            foreach (var it in SpawnActorInfos) {
                log += it.ToString();
            }
            return log;
        }
    }


    public class BaseFrameDataManager {
        public const float LOGIC_FRAME = 1 / 15.0f;

        protected Dictionary<int, NetSyncComponent> _netActors = new Dictionary<int, NetSyncComponent>(new Int32Comparer());

        public bool IsGameStart {
            get;
            set;
        }

        public int MaxPlayerCount {
            get;
            set;
        }

        public int CurrentPlayerCount {
            get;
            set;
        }

        public BaseFrameDataManager() {
            // todo 之后这个值需要读表
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>(0, out Config.GameSetting gameSetting)) {
                MaxPlayerCount = gameSetting.MaxPlayer;
            }
            else {
                MaxPlayerCount = 1;
            }

            CurrentPlayerCount = 0;
            IsGameStart = false;
        }

        public Dictionary<int, NetSyncComponent> GetAllNetActor() {
            return _netActors;
        }

        public bool TryGetNetActor(int id, out NetSyncComponent netSyncComponent) {
            return _netActors.TryGetValue(id, out netSyncComponent);
        }

        public virtual GameObject SpawnNetObject(int id, int ownerID, string path, int configID, Vector3 pos, ENetType type) {
            GameObject newNetActor = Asset.Load(path);

            NetSyncComponent netSyncComponent = newNetActor.TryGetOrAdd<NetSyncComponent>();
            netSyncComponent.NetID = id;
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;
            netSyncComponent.NetType = type;

            newNetActor.transform.position = pos;

            _netActors.Add(ownerID << 16 | id, netSyncComponent);

            return newNetActor;
        }

        public bool HasNetObject(int id, int ownerID) {
            return _netActors.ContainsKey(ownerID << 16 | id);
        }
    }
}
