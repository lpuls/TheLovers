using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class FrameDataManager {
        public const float LOGIC_FRAME = 1 / 15.0f;


        private int _netIDCreateIndex = 1;
        private List<FrameData> _frameDatas = new List<FrameData>();
        private Dictionary<int, NetSyncComponent> _netActors = new Dictionary<int, NetSyncComponent>(new Int32Comparer());
        private HashSet<int> _newActorIDs = new HashSet<int>(new Int32Comparer());

        private bool _simulate = false;
        private FrameData _currentFrameData = null;
        private FrameData _preFrameData = null;

        private byte[] _analyzeBytes = new byte[1024];
        private BinaryReader _binaryReader = null;

        public int MaxPlayerCount {
            get;
            set;
        }

        public int CurrentPlayerCount {
            get;
            set;
        }

        public bool IsGameStart {
            get;
            set;
        }

        public int GameLogicFrame {
            get;
            private set;
        }

        public float LogicTime {
            get;
            private set;
        }

        public FrameDataManager() {
            _binaryReader = new BinaryReader(new MemoryStream(_analyzeBytes));

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

        public GameObject SpawnNetObject(int id, int ownerID, string path, int configID, Vector3 pos) {
            GameObject newNetActor = Asset.Load(path);

            NetSyncComponent netSyncComponent = newNetActor.TryGetOrAdd<NetSyncComponent>();
            netSyncComponent.NetID = id;
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;

            newNetActor.transform.position = pos;

            _netActors.Add(ownerID << 16 | id, netSyncComponent);

            return newNetActor;
        }

        public GameObject SpawnServerNetObject(int ownerID, string path, int configID, Vector3 pos) {
            GameObject newNetActor = Asset.Load(path);

            NetSyncComponent netSyncComponent = newNetActor.TryGetOrAdd<NetSyncComponent>();
            netSyncComponent.NetID = _netIDCreateIndex++;
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;

            newNetActor.transform.position = pos;

            _netActors.Add(ownerID << 16 | netSyncComponent.NetID, netSyncComponent);

            return newNetActor;
        }

        public bool HasNetObject(int id, int ownerID) {
            return _netActors.ContainsKey(ownerID << 16 | id);
        }

        public void ReceiveNewFrame(FrameData frameData) {
            if (null != _preFrameData)
                _preFrameData.Free();
            _preFrameData = _currentFrameData;
            _currentFrameData = frameData;

            // 设置逻辑帧
            GameLogicFrame = frameData.FrameIndex;

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
                    SpawnNetObject(playerInfo.ID, 0, "Res/Ships/GreyShip", 0, new Vector3(playerInfo.X, 0, playerInfo.Y));
                }
            }

            for (int i = 0; i < spawnActors.Count; i++) {
                SpawnActorInfo spawnActorInfo = spawnActors[i];
                if (!_netActors.ContainsKey(spawnActorInfo.GetUniqueID())) {
                    SpawnNetObject(spawnActorInfo.ID, spawnActorInfo.OwnerID, "Res/Bullet/Bullet", 0, new Vector3(spawnActorInfo.X, 0, spawnActorInfo.Y));
                }
            }
        }

        public void AnalyzeBinary(byte[] binary) {
            binary.CopyTo(_analyzeBytes, 0);
            _binaryReader.BaseStream.Position = 0;

            FrameData frameData = FrameData.Malloc(_binaryReader.ReadInt32());

            int playerSize = _binaryReader.ReadInt32();
            for (int i = 0; i < playerSize; i++) {
                PlayerInfo playerInfo = PlayerInfo.Malloc();
                playerInfo.Read(_binaryReader);
                frameData.PlayerInfos.Add(playerInfo);
                frameData.NetInfoDict.Add(playerInfo.ID, playerInfo);
            }

            int spawnActorSize = _binaryReader.ReadInt32();
            for (int i = 0; i < spawnActorSize; i++) {
                SpawnActorInfo spawnActorInfo = SpawnActorInfo.Malloc();
                spawnActorInfo.Read(_binaryReader);
                frameData.SpawnActorInfos.Add(spawnActorInfo);
                frameData.NetInfoDict.Add(spawnActorInfo.GetUniqueID(), spawnActorInfo);
            }

            _frameDatas.Add(frameData);

            // 还未开始模拟便已有三个逻辑帧了，开始进行模拟
            if (!_simulate && _frameDatas.Count > 3) {
                _simulate = true;
            }
        }

        public void NextFrame() {
            if (_frameDatas.Count <= 0) {
                Debug.LogWarning("Local Frame Out of Logic Frame Range");
                return;
            }

            FrameData frameData = _frameDatas[0];
            ReceiveNewFrame(frameData);
            _frameDatas.RemoveAt(0);
            // frameData.Free();

        }

        public FrameData GetCurrentFrameData() {
            return _currentFrameData;
        }

        public FrameData GetPreFrameData() {
            return _preFrameData;
        }

        public float GetLogicFramepercentage() {
            return LogicTime / LOGIC_FRAME;
        }

        public Dictionary<int, NetSyncComponent> GetAllNetActor() {
            return _netActors;
        }

        public bool TryGetNetActor(int id, out NetSyncComponent netSyncComponent) {
            return _netActors.TryGetValue(id, out netSyncComponent);
        }

        public void Update() {
            if (!_simulate) {
                return;
            }

            // 更新逻辑时间
            // todo 这样更新可能会有问题，如果客户端卡顿可能会导致逻辑帧数据不能被有效处理
            LogicTime += Time.deltaTime;
            if (LogicTime >= LOGIC_FRAME) {
                LogicTime -= LOGIC_FRAME;
                NextFrame();
            }
        }
    }
}
