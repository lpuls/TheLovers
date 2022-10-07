using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class INetInfo : IPool {
        private int _id = 0;

        public int ID {
            get {
                return _id;
            }
            set {
                _id = value;
            }
        }
        public int OwnerID;
        public float X;
        public float Y;
        public float Angle;

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
            ID = (temp >> 29) & 0x7;
            X = binaryReader.ReadSingle();
            Y = binaryReader.ReadSingle();
            Angle = binaryReader.ReadSingle();
        }

        public override void Write(Packet packet) {
            int temp = 0;
            temp |= ID;
            temp = (temp << 7) | Health;
            temp = (temp << 22) | Tags;
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

    public interface IFrameInfo {
        void Read(BinaryReader binaryReader);

        void Write(Packet packet);
    }

    public enum EDestroyActorReason {
        None,
        OutOfWorld,
        BeHit,
        HitOther,
    }

    public enum EUpdateActorType {
        None,
        Position,
        Angle
    }

    public class SpawnInfo : IFrameInfo, IPool {
        public int NetID = 0;
        public int OwnerID = 0;
        public int ConfigID = 0;
        public ENetType NetType = ENetType.None;
        public Vector3 Position = Vector3.zero;

        public void Read(BinaryReader binaryReader) {
            NetID = binaryReader.ReadInt32();
            OwnerID = binaryReader.ReadInt32();
            ConfigID = binaryReader.ReadInt32();
            NetType = (ENetType)binaryReader.ReadInt16();
            Position.x = binaryReader.ReadSingle();
            Position.z = binaryReader.ReadSingle();
        }

        public void Write(Packet packet) {
            packet.WriteInt32(NetID);
            packet.WriteInt32(OwnerID);
            packet.WriteInt32(ConfigID);
            packet.WriteInt16((short)NetType);
            packet.WriteFloat(Position.x);
            packet.WriteFloat(Position.z);
        }

        public void Reset() {
            NetID = 0;
            OwnerID = 0;
            ConfigID = 0;
            NetType = ENetType.None;
            Position = Vector3.zero;
        }
    }

    public class DestroyInfo : IFrameInfo, IPool {
        public int NetID = 0;
        public EDestroyActorReason Reason = EDestroyActorReason.None;

        public void Read(BinaryReader binaryReader) {
            NetID = binaryReader.ReadInt32();
            Reason = (EDestroyActorReason)binaryReader.ReadInt16();
        }

        public void Write(Packet packet) {
            packet.WriteInt32(NetID);
            packet.WriteInt16((short)Reason);
        }

        public void Reset() {
            NetID = 0;
            Reason = EDestroyActorReason.None;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct UpdateData {
        [FieldOffset(0)] public byte Int8;
        [FieldOffset(0)] public short Int16;
        [FieldOffset(0)] public int Int32;
        [FieldOffset(0)] public bool Boolean;
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public Vector3 Vec3;

        public void Clean() {
            Int8 = 0;
            Int16 = 0;
            Int32 = 0;
            Boolean = false;
            Float = 0;
            Vec3 = Vector3.zero;
        }

    }

    public class UpdateInfo : IFrameInfo, IPool {
        public EUpdateActorType UpdateType = EUpdateActorType.None;
        public UpdateData Data1 = new UpdateData();
        public UpdateData Data2 = new UpdateData();

        public void SetFloatForData1(float value) {
            Data1.Float = value;
        }

        public void SetVec3ForData2(float x, float z) {
            Data1.Vec3.x = x;
            Data1.Vec3.z = z;
        }

        public void SetInt32ForData2(int value) {
            Data2.Int32 = value;
        }

        public virtual void Read(BinaryReader binaryReader) {
            UpdateType = (EUpdateActorType)binaryReader.ReadInt16();
            switch (UpdateType) {
                case EUpdateActorType.Position:
                    Data1.Vec3.x = binaryReader.ReadSingle();
                    Data1.Vec3.z = binaryReader.ReadSingle();
                    Data2.Int32 = binaryReader.ReadInt32();
                    break;
                case EUpdateActorType.Angle:
                    Data1.Float = binaryReader.ReadSingle();
                    break;
            }
        }

        public virtual void Write(Packet packet) {
            packet.WriteInt16((short)UpdateType);
            switch (UpdateType) {
                case EUpdateActorType.Position:
                    packet.WriteFloat(Data1.Vec3.x);
                    packet.WriteFloat(Data1.Vec3.z);
                    packet.WriteInt32(Data2.Int32);
                    break;
                case EUpdateActorType.Angle:
                    packet.WriteFloat(Data1.Float);
                    break;
            }
        }

        public void Reset() {
            UpdateType = EUpdateActorType.None;
            Data1.Clean();
            Data2.Clean();
        }
    }

    public class FrameData : IPool {
        public int FrameIndex;
        public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
        public List<SpawnActorInfo> SpawnActorInfos = new List<SpawnActorInfo>();
        public Dictionary<int, INetInfo> NetInfoDict = new Dictionary<int, INetInfo>(new Int32Comparer());

        public List<SpawnInfo> SpawnInfos = new List<SpawnInfo>(32); 
        public List<DestroyInfo> DestroyInfos = new List<DestroyInfo>(32);
        public Dictionary<int, List<UpdateInfo>> UpdateInfos = new Dictionary<int, List<UpdateInfo>>(new Int32Comparer());

        public void Reset() {
            FrameIndex = 0;
            {
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

            // 新的格式
            foreach (var it in SpawnInfos) {
                ObjectPool<SpawnInfo>.Free(it);
            }
            foreach (var it in DestroyInfos) {
                ObjectPool<DestroyInfo>.Free(it);
            }
            foreach (var it in UpdateInfos) {
                for (int i = 0; i < it.Value.Count; i++) {
                    UpdateInfo info = it.Value[i];
                    ObjectPool<UpdateInfo>.Free(info);
                }
                ListPool<UpdateInfo>.Free(it.Value);
            }
            SpawnInfos.Clear();
            DestroyInfos.Clear();
            UpdateInfos.Clear();
        }

        public void AddUpdateInfo(int id, UpdateInfo info) {
            if (UpdateInfos.TryGetValue(id, out List<UpdateInfo> infos)) {
                infos.Add(info);
            }
            else
            {
                infos = ListPool<UpdateInfo>.Malloc();
                UpdateInfos[id] = infos;
                infos.Add(info);
            }
        }

        public bool TryGetUpdateInfo(int id, EUpdateActorType updateType, out UpdateInfo info) {
            info = null;
            if (UpdateInfos.TryGetValue(id, out List<UpdateInfo> infos)) {
                foreach (var item in infos) {
                    if (item.UpdateType == updateType) {
                        info = item;
                        return true;
                    }
                }
            }
            return false;
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
                log += "\n";
            }
            foreach (var it in SpawnActorInfos) {
                log += it.ToString();
                log += "\n";
            }
            return log;
        }
    }


    public class BaseFrameDataManager {
        public const float LOGIC_FRAME_TIME = 1 / 15.0f;

        protected Dictionary<int, NetSyncComponent> _netActors = new Dictionary<int, NetSyncComponent>(new Int32Comparer());
        protected HashSet<IServerTicker> _tickers = new HashSet<IServerTicker>();
        protected List<IServerTicker> _pendingAddTickers = new List<IServerTicker>(128);

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
            netSyncComponent.IsNewObject = true;

            newNetActor.transform.position = pos;

            _netActors.Add(ownerID << 16 | id, netSyncComponent);

            return newNetActor;
        }

        public bool HasNetObject(int id, int ownerID) {
            return _netActors.ContainsKey(ownerID << 16 | id);
        }

        public virtual void Update() {
            List<int> pendingKillActors = ListPool<int>.Malloc();

            var it = _netActors.GetEnumerator();
            while (it.MoveNext()) {
                NetSyncComponent netSyncComponent = it.Current.Value;

                // 如果已死亡，则从列表中移除去
                if (netSyncComponent.IsPendingKill()) {
                    pendingKillActors.Add(it.Current.Key);
                    continue;
                }
            }

            // 将死亡的actor从列表中移除
            foreach (var deadActor in pendingKillActors) {
                GameObject deadGO = _netActors[deadActor].gameObject;
                _netActors.Remove(deadActor);
                AssetPool.Free(deadGO);
            }

            ListPool<int>.Free(pendingKillActors);
        }

        public void UpdateTickers() {
            var pendingIt = _pendingAddTickers.GetEnumerator();
            while (pendingIt.MoveNext()) {
                _tickers.Add(pendingIt.Current);
            }
            _pendingAddTickers.Clear();

            var it = _tickers.GetEnumerator();
            while (it.MoveNext()) {
                it.Current.Tick(LOGIC_FRAME_TIME);
            }
        }

        public void AddTicker(IServerTicker serverTicker) {
            //_tickers.Add(serverTicker);
            _pendingAddTickers.Add(serverTicker);
        }

        public void RemoveTicker(IServerTicker serverTicker) {
            _tickers.Remove(serverTicker);
        }
    }
}
