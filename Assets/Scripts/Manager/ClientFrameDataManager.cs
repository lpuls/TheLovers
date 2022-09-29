using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ClientFrameDataManager : BaseFrameDataManager {
        public const int MAX_SERVER_FRAME_COUNT = 3;

        private List<FrameData> _frameDatas = new List<FrameData>();
        private HashSet<int> _newActorIDs = new HashSet<int>(new Int32Comparer());

        private bool _simulate = false;
        private FrameData _currentFrameData = null;
        private FrameData _preFrameData = null;

        private byte[] _analyzeBytes = new byte[1024];
        private BinaryReader _binaryReader = null;

        public int GameLogicFrame {
            get;
            private set;
        }

        public float LogicTime {
            get;
            private set;
        }

        public ClientFrameDataManager() {
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

            _netActors.Add(id, netSyncComponent);

            return newNetActor;
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
                _newActorIDs.Add(players[i].ID);
            }
            for (int i = 0; i < spawnActors.Count; i++) {
                _newActorIDs.Add(spawnActors[i].ID);
            }

            // 找出本地有但网络没有的物体，设置为移除
            var it = _netActors.GetEnumerator();
            while (it.MoveNext()) {
                NetSyncComponent netSyncComponent = it.Current.Value;
                if (!_newActorIDs.Contains(netSyncComponent.NetID)) {
                    netSyncComponent.Kill();
                }
            }

            // 遍历所有的网络生成物，创建不存在的物体
            for (int i = 0; i < players.Count; i++) {
                PlayerInfo playerInfo = players[i];
                if (!_netActors.ContainsKey(playerInfo.ID)) {
                    SpawnNetObject(playerInfo.ID, 0, "Res/Ships/GreyShip", 0, new Vector3(playerInfo.X, 0, playerInfo.Y));
                }
            }

            for (int i = 0; i < spawnActors.Count; i++) {
                SpawnActorInfo spawnActorInfo = spawnActors[i];
                if (!_netActors.ContainsKey(spawnActorInfo.ID)) {
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
                frameData.NetInfoDict.Add(spawnActorInfo.ID, spawnActorInfo);
            }
            Debug.Log("=====>Analyze Binary: \n" + frameData.ToString());

            _frameDatas.Add(frameData);

            // 还未开始模拟便已有三个逻辑帧了，开始进行模拟
            if (!_simulate && _frameDatas.Count > MAX_SERVER_FRAME_COUNT) {
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

        public override void Update() {
            base.Update();

            if (!_simulate) {
                return;
            }

            // 如果服务端存放的帧数较多了，直接一路追上去
            if (_frameDatas.Count >= MAX_SERVER_FRAME_COUNT) {
                LogicTime = LOGIC_FRAME;
                while (_frameDatas.Count >= MAX_SERVER_FRAME_COUNT) {
                    NextFrame();
                }
                LogicTime = 0;
            }

            // 更新逻辑时间
            LogicTime += Time.deltaTime;
            while (LogicTime >= LOGIC_FRAME) {
                NextFrame();
                LogicTime -= LOGIC_FRAME;
            }
        }
    }
}
