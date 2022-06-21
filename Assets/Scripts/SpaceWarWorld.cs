using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class INetInfo {
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

        public override string ToString() {
            return string.Format("ID: {0}, Position: ({1}, {2}), Angle: {3}", ID, X, Y, Angle);
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

        public override string ToString() {
            return string.Format("ID: {0}: {4}, Position: ({1}, {2}), Angle: {3}", ID, X, Y, Angle, OwnerID);
        }
    }

    public class FrameData {
        public int FrameIndex;
        public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
        public List<SpawnActorInfo> SpawnActorInfos = new List<SpawnActorInfo>();
        public Dictionary<int, INetInfo> NetInfoDict = new Dictionary<int, INetInfo>(new Int32Comparer());
    }

    public class SpaceWarWorld : World {

        public static bool Simulate = false;
        public const float LOGIC_FRAME = 1 / 15.0f;

        private List<FrameData> _frameDatas = new List<FrameData>();
        private Dictionary<int, NetSyncComponent> _netActors = new Dictionary<int, NetSyncComponent>(new Int32Comparer());


        public int FrameDataIndex {
            get;
            private set;
        }
        public float LogicTime {
            get;
            private set;
        }

        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld(null, null, GetType().Assembly);

            // 加载测试用logic数据
            TextAsset textAsset = Asset.Load<TextAsset>("Res/Test/Data");
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(textAsset.bytes));

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length) {
                FrameData frameData = new FrameData {
                    FrameIndex = binaryReader.ReadInt32()
                };

                int playerSize = binaryReader.ReadInt32();
                for (int i = 0; i < playerSize; i++) {
                    PlayerInfo playerInfo = new PlayerInfo();
                    playerInfo.Read(binaryReader);
                    frameData.PlayerInfos.Add(playerInfo);
                    frameData.NetInfoDict.Add(playerInfo.ID, playerInfo);
                }

                int spawnActorSize = binaryReader.ReadInt32();
                for (int i = 0; i < spawnActorSize; i++) {
                    SpawnActorInfo spawnActorInfo = new SpawnActorInfo();
                    spawnActorInfo.Read(binaryReader);
                    frameData.SpawnActorInfos.Add(spawnActorInfo);
                    frameData.NetInfoDict.Add(spawnActorInfo.GetUniqueID(), spawnActorInfo);
                }

                _frameDatas.Add(frameData);
            }

            ReceiveNewFrame(_frameDatas[0]);
            FrameDataIndex = 0;
        }

        public FrameData GetCurrentFrameData() {
            if (FrameDataIndex >= 0 && FrameDataIndex < _frameDatas.Count)
                return _frameDatas[FrameDataIndex];
            return null;
        }

        public FrameData GetPreFrameData() {
            int index = FrameDataIndex - 1;
            if (index >= 0 && index < _frameDatas.Count)
                return _frameDatas[index];
            return null;
        }

        protected override void Update() {
            base.Update();

            // 每一个逻辑辑时长更新一次逻辑数据
            if (FrameDataIndex < 0 || !SpaceWarWorld.Simulate)
                return;

            LogicTime += Time.deltaTime;
            if (LogicTime >= LOGIC_FRAME) {
                LogicTime -= LOGIC_FRAME;

                if (FrameDataIndex + 1 >= _frameDatas.Count) {
                    FrameDataIndex = -1;
                }
                else {
                    ReceiveNewFrame(_frameDatas[FrameDataIndex + 1]);
                    FrameDataIndex++;
                }
            }
        }

        private HashSet<int> _newActorIDs = new HashSet<int>(new Int32Comparer());

        private void SpawnNetObject(int id, int ownerID, string path, Vector3 pos) {
            GameObject newNetActor = Asset.Load(path);

            NetSyncComponent netSyncComponent = newNetActor.TryGetOrAdd<NetSyncComponent>();
            netSyncComponent.NetID = id;
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.PendingKill = false;

            newNetActor.transform.position = pos;

            _netActors.Add(ownerID << 16 | id, netSyncComponent);
        }

        public void ReceiveNewFrame(FrameData frameData) {
            List<PlayerInfo> players = frameData.PlayerInfos;
            List<SpawnActorInfo> spawnActors = frameData.SpawnActorInfos;

            // 记录下本次所有的网络同步物的ID
            _newActorIDs.Clear();
            for (int i = 0; i < players.Count; i++) {
                _newActorIDs.Add(players[i].GetUniqueID());
            }
            for (int i = 0; i < spawnActors.Count; i++) {
                _newActorIDs.Add(spawnActors[i].GetUniqueID());
            }

            // 找出本地有但网络没有的物体，设置为移除
            var it = _netActors.GetEnumerator();
            while (it.MoveNext()) {
                NetSyncComponent netSyncComponent = it.Current.Value;
                if (!_newActorIDs.Contains(netSyncComponent.GetUniqueID())) {
                    netSyncComponent.PendingKill = true;
                }
            }

            // 遍历所有的网络生成物，创建不存在的物体
            for (int i = 0; i < players.Count; i++) {
                PlayerInfo playerInfo = players[i];
                if (!_netActors.ContainsKey(playerInfo.GetUniqueID())) {
                    SpawnNetObject(playerInfo.ID, 0, "Res/Ships/GreyShip", new Vector3(playerInfo.X, 0, playerInfo.Y));
                }
            }

            for (int i = 0; i < spawnActors.Count; i++) {
                SpawnActorInfo spawnActorInfo = spawnActors[i];
                if (!_netActors.ContainsKey(spawnActorInfo.GetUniqueID())) {
                    SpawnNetObject(spawnActorInfo.ID, spawnActorInfo.OwnerID, "Res/Bullet/Bullet", new Vector3(spawnActorInfo.X, 0, spawnActorInfo.Y));
                }
            }
        }

        #region GM
        [GM]
        public static void GM_BeginSimulate(string[] gmParams) {
            SpaceWarWorld.Simulate = true;
        }
        [GM]
        public static void GM_StopSimulate(string[] gmParams) {
            SpaceWarWorld.Simulate = true;
        }
        #endregion

    }

}