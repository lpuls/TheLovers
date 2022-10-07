using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class ClientFrameDataManager : BaseFrameDataManager {
        public const int MAX_SERVER_FRAME_COUNT = 3;

        private List<FrameData> _frameDatas = new List<FrameData>();

        private bool _simulate = false;
        private FrameData _currentFrameData = null;
        private FrameData _preFrameData = null;

        private byte[] _analyzeBytes = new byte[1024];
        private BinaryReader _binaryReader = null;
        private HashSet<NetSyncComponent> _predictionActors = new HashSet<NetSyncComponent>(32);

        public System.Action<FrameData, FrameData> OnFrameUpdate;

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

            List<SpawnInfo> spawnInfos = frameData.SpawnInfos;
            List<DestroyInfo> destroyInfos = frameData.DestroyInfos;

            foreach (var item in spawnInfos) {
                if (!_netActors.ContainsKey(item.NetID)) {
                    switch (item.NetType) {
                        case ENetType.Player:
                            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(item.ConfigID, out Config.ShipConfig shipConfig)) {
                                SpawnNetObject(item.NetID, item.OwnerID, shipConfig.Path, 0, item.Position);
                            }
                            break;
                        case ENetType.Bullet:
                            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(item.ConfigID, out Config.Abilitys abilityConfig)) {
                                GameObject bullet = SpawnNetObject(item.NetID, item.OwnerID, abilityConfig.Path, 0, item.Position);
                                bullet.TryGetOrAdd<SimulateComponent>();
                                TailManager tailManager = bullet.TryGetOrAdd<TailManager>();
                                tailManager.ShowEffect();
                            }
                            break;
                    }
                }
            }

            foreach (var item in destroyInfos) {
                if (_netActors.TryGetValue(item.NetID, out NetSyncComponent netSyncComponent)) {
                    netSyncComponent.Kill(item.Reason);
                }
            }

            OnFrameUpdate?.Invoke(_preFrameData, _currentFrameData);
        }

        public void AnalyzeBinary(byte[] binary) {
            binary.CopyTo(_analyzeBytes, 0);
            _binaryReader.BaseStream.Position = 0;

            FrameData frameData = FrameData.Malloc(_binaryReader.ReadInt32());

            int spawnCount = _binaryReader.ReadInt32();
            for (int i = 0; i < spawnCount; i++) {
                SpawnInfo playerInfo = ObjectPool<SpawnInfo>.Malloc();
                playerInfo.Read(_binaryReader);
                frameData.SpawnInfos.Add(playerInfo);
            }
            int destroyCount = _binaryReader.ReadInt32();
            for (int i = 0; i < destroyCount; i++) {
                DestroyInfo destroyInfo = ObjectPool<DestroyInfo>.Malloc();
                destroyInfo.Read(_binaryReader);
                frameData.DestroyInfos.Add(destroyInfo);
            }
            int updateCount = _binaryReader.ReadInt32();
            for (int i = 0; i < updateCount; i++) {
                int netID = _binaryReader.ReadInt32();
                int updateTypeCount = _binaryReader.ReadInt32();
                for (int j = 0; j < updateTypeCount; j++) {
                    UpdateInfo updateInfo = ObjectPool<UpdateInfo>.Malloc();
                    updateInfo.Read(_binaryReader);
                    frameData.AddUpdateInfo(netID, updateInfo);
                }
            }

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
            return LogicTime / LOGIC_FRAME_TIME;
        }

        public override void Update() {
            base.Update();

            if (!_simulate) {
                return;
            }
            if (_frameDatas.Count <= 0) {
                return;
            }

            // 如果服务端存放的帧数较多了，直接一路追上去
            if (_frameDatas.Count >= MAX_SERVER_FRAME_COUNT) {
                LogicTime = LOGIC_FRAME_TIME;
                while (_frameDatas.Count >= MAX_SERVER_FRAME_COUNT) {
                    NextFrame();
                }
                LogicTime = 0;
            }

            // 更新逻辑时间
            LogicTime += Time.deltaTime;
            while (LogicTime >= LOGIC_FRAME_TIME) {
                UpdateTickers();
                NextFrame();
                LogicTime -= LOGIC_FRAME_TIME;
            }
        }

        public void AddPredictionActor(NetSyncComponent netSyncComponent) {
            _predictionActors.Add(netSyncComponent);
        }
    }
}
