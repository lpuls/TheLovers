using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Hamster.SpaceWar {
    public interface IFrameInfo {
        void Read(BinaryReader binaryReader);

        void Write(Packet packet);
    }

    public enum EDestroyActorReason {
        None,
        OutOfWorld,
        BeHit,
        HitOther,
        TimeOut,
    }

    public enum EUpdateActorType {
        None,
        Position,
        Angle,
        RoleState,
        Health
    }

    public class SpawnInfo : IFrameInfo, IPool {
        public int NetID = 0;
        public int OwnerID = 0;
        public int ConfigID = 0;
        public float Angle = 0;
        public ENetType NetType = ENetType.None;
        public Vector3 Position = Vector3.zero;

        public void Read(BinaryReader binaryReader) {
            NetID = binaryReader.ReadInt32();
            OwnerID = binaryReader.ReadInt32();
            ConfigID = binaryReader.ReadInt32();
            NetType = (ENetType)binaryReader.ReadInt16();
            Position.x = binaryReader.ReadSingle();
            Position.z = binaryReader.ReadSingle();
            Angle = binaryReader.ReadSingle();
        }

        public void Write(Packet packet) {
            packet.WriteInt32(NetID);
            packet.WriteInt32(OwnerID);
            packet.WriteInt32(ConfigID);
            packet.WriteInt16((short)NetType);
            packet.WriteFloat(Position.x);
            packet.WriteFloat(Position.z);
            packet.WriteFloat(Angle);
        }

        public void Reset() {
            NetID = 0;
            OwnerID = 0;
            ConfigID = 0;
            Angle = 0;
            NetType = ENetType.None;
            Position = Vector3.zero;
        }

        public SpawnInfo CopyToNew() {
            SpawnInfo info = ObjectPool<SpawnInfo>.Malloc();
            info.NetID = NetID;
            info.OwnerID = OwnerID;
            info.ConfigID = ConfigID;
            info.NetType = NetType;
            info.Position = Position;
            info.Angle = Angle;
            return info;
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

        public DestroyInfo CopyToNew() {
            DestroyInfo info = ObjectPool<DestroyInfo>.Malloc();
            info.NetID = NetID;
            info.Reason = Reason;
            return info;
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

        public void SetInt8ForData1(byte value) {
            Data1.Int8 = value;
        }

        public void SetInt32ForData1(int value) {
            Data1.Int32 = value;
        }

        public void SetInt16ForData1(short value) {
            Data1.Int16 = value;
        }

        public void SetFloatForData1(float value) {
            Data1.Float = value;
        }

        public void SetVec3ForData2(float x, float y) {
            Data1.Vec3.x = x;
            Data1.Vec3.y = y;
        }

        public void SetInt32ForData2(int value) {
            Data2.Int32 = value;
        }

        public virtual void Read(BinaryReader binaryReader) {
            UpdateType = (EUpdateActorType)binaryReader.ReadByte();
            switch (UpdateType) {
                case EUpdateActorType.Position:
                    Data1.Vec3.x = binaryReader.ReadSingle();
                    Data1.Vec3.y = binaryReader.ReadSingle();
                    Data2.Int32 = binaryReader.ReadInt32();
                    break;
                case EUpdateActorType.Angle:
                    Data1.Float = binaryReader.ReadSingle();
                    break;
                case EUpdateActorType.RoleState:
                    Data1.Int8 = binaryReader.ReadByte();
                    break;
                case EUpdateActorType.Health:
                    Data1.Int16 = binaryReader.ReadInt16();
                    break;
            }
        }

        public virtual void Write(Packet packet) {
            packet.WriteByte((byte)UpdateType);
            switch (UpdateType) {
                case EUpdateActorType.Position:
                    packet.WriteFloat(Data1.Vec3.x);
                    packet.WriteFloat(Data1.Vec3.y);
                    packet.WriteInt32(Data2.Int32);
                    break;
                case EUpdateActorType.Angle:
                    packet.WriteFloat(Data1.Float);
                    break;
                case EUpdateActorType.RoleState:
                    packet.WriteByte(Data1.Int8);
                    break;
                case EUpdateActorType.Health:
                    packet.WriteInt16(Data1.Int16);
                    break;
            }
        }

        public void Reset() {
            UpdateType = EUpdateActorType.None;
            Data1.Clean();
            Data2.Clean();
        }

        public UpdateInfo CopyToNew() {
            UpdateInfo updateInfo = ObjectPool<UpdateInfo>.Malloc();
            updateInfo.Data1 = Data1;
            updateInfo.Data2 = Data2;
            updateInfo.UpdateType = UpdateType;
            return updateInfo;
        }
    }

    public class FrameData : IPool {
        public int FrameIndex;
        public List<SpawnInfo> SpawnInfos = new List<SpawnInfo>(32); 
        public List<DestroyInfo> DestroyInfos = new List<DestroyInfo>(32);
        public Dictionary<int, List<UpdateInfo>> UpdateInfos = new Dictionary<int, List<UpdateInfo>>(new Int32Comparer());

        public void Reset() {
            FrameIndex = 0;

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
            return log;
        }

        public FrameData CopyToNew() {
            FrameData frameData = Malloc(FrameIndex);
            foreach (var it in SpawnInfos) {
                frameData.SpawnInfos.Add(it.CopyToNew());
            }
            foreach (var it in DestroyInfos) {
                frameData.DestroyInfos.Add(it.CopyToNew());
            }
            foreach (var it in UpdateInfos) {
                foreach (var item in it.Value) {
                    frameData.AddUpdateInfo(it.Key, item.CopyToNew());
                }
            }
            return frameData;
        }
    }

    public enum EServerTickLayers {
        PreTick = 0,
        Tick = 1,
        LateTick = 2,
        Max
    }


    public class BaseFrameDataManager {
        public const float LOGIC_FRAME_TIME = 1 / 15.0f;

        protected Dictionary<int, NetSyncComponent> _netActors = new Dictionary<int, NetSyncComponent>(new Int32Comparer());
        //protected HashSet<IServerTicker> _tickers = new HashSet<IServerTicker>();
        protected List<IServerTicker> _pendingAddTickers = new List<IServerTicker>(128);
        protected HashSet<IServerTicker>[] _tickers = new HashSet<IServerTicker>[(int)EServerTickLayers.Max];

        public bool IsGameStart {
            get;
            set;
        }

        public int CurrentPlayerCount {
            get;
            set;
        }

        public BaseFrameDataManager() {
            CurrentPlayerCount = 0;
            IsGameStart = false;

            for (int i = 0; i < _tickers.Length; i++) {
                _tickers[i] = new HashSet<IServerTicker>();
            }
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
                int index = pendingIt.Current.GetPriority();
                Debug.Assert(index >= 0 && index < (int)EServerTickLayers.Max, "Server tick priority out of range");
                _tickers[index].Add(pendingIt.Current);
            }
            _pendingAddTickers.Clear();

            for (int i = 0; i < _tickers.Length; i++) {
                var it = _tickers[i].GetEnumerator();
                while (it.MoveNext()) {
                    IServerTicker ticker = it.Current;
                    if (ticker.IsEnable())
                        ticker.Tick(LOGIC_FRAME_TIME);
                }
            }
            
        }

        public void AddTicker(IServerTicker serverTicker) {
            _pendingAddTickers.Add(serverTicker);
        }

        public void RemoveTicker(IServerTicker serverTicker) {
            int index = serverTicker.GetPriority();
            Debug.Assert(index >= 0 && index < (int)EServerTickLayers.Max, "Server tick priority out of range");
            _tickers[index].Remove(serverTicker);
        }
    }
}
