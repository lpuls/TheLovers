using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ServerFrameDataManager : BaseFrameDataManager {

        private int _shipCreateIndex = 0;
        private ServerNetDevice _netDevice = null;
        private S2CGameFrameDataSyncMessage _syncMessage = new S2CGameFrameDataSyncMessage();

        private List<NetSyncComponent> _players = new List<NetSyncComponent>(4);
        private HashSet<EUpdateActorType> _systemUpdateTypes = new HashSet<EUpdateActorType>(new EUpdateActorTypeComparer());

        public Action OnGameStart;
        public Action<FrameData> OnNewFrameData;

        public int ServerLogicFrame {
            get;
            private set;
        }

        public float LogicTime {
            get;
            private set;
        }

        private ServerNetDevice GetNetDevice() {
            if (null == _netDevice) {
                ServerSpaceWarWorld netSpaceWarWorld = World.GetWorld<ServerSpaceWarWorld>();
                _netDevice = netSpaceWarWorld.NetDevice;
            }
            return _netDevice;
        }

        public override GameObject SpawnNetObject(int id, int ownerID, string path, int configID, Vector3 pos, ENetType type) {
            GameObject newNetActor = Asset.Load(path);

            NetSyncComponent netSyncComponent = newNetActor.TryGetOrAdd<NetSyncComponent>();
            if (TryGetNetActor(ownerID, out NetSyncComponent parentNetSync)) {
                netSyncComponent.NetID = parentNetSync.GetSpawnIndex();
            }
            else {
                netSyncComponent.NetID = ++_shipCreateIndex;
            }
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;
            netSyncComponent.NetType = type;
            netSyncComponent.IsNewObject = true;

            if (ENetType.Player == type) {
                _players.Add(netSyncComponent);
            }

            newNetActor.transform.position = pos;

            _netActors.Add(netSyncComponent.NetID, netSyncComponent);

            return newNetActor;
        }

        public List<NetSyncComponent> GetPlayers() {
            return _players;
        }

        public override void Update() {
            base.Update();

            if (!IsGameStart)
                return;

            LogicTime += Time.deltaTime;
            while (LogicTime >= LOGIC_FRAME_TIME) {
                UpdateTickers();
                Tick();
                LogicTime -= LOGIC_FRAME_TIME;
            }
        }

        public void Tick() {
            BaseSpaceWarWorld world = World.GetWorld<BaseSpaceWarWorld>();

            FrameData frameData = ObjectPool<FrameData>.Malloc();
            frameData.FrameIndex = ServerLogicFrame++;


            List<int> pendingKillActors = ListPool<int>.Malloc();

            // 优化添加全局系统事件
            foreach (var item in _systemUpdateTypes) {
                UpdateInfo updateInfo = ObjectPool<UpdateInfo>.Malloc();
                updateInfo.UpdateType = item;
                switch (item) {
                    case EUpdateActorType.LevelEventIndex: {
                            LevelManager levelManager = world.GetManager<LevelManager>();
                            updateInfo.Data1.Int32 = levelManager.GetCurrentLevelEventIndex();
                        }
                        break;
                    case EUpdateActorType.MissionResult: {
                            //LevelManager levelManager = world.GetManager<LevelManager>();
                            updateInfo.Data1.Boolean = (world as ServerSpaceWarWorld).GameResult;
                        }
                        break;
                    default:
                        UnityEngine.Debug.LogError("Can't Add System Update Info by " + item);
                        break;
                }
                frameData.AddUpdateInfo(SYSTEM_NET_ACTOR_ID, updateInfo);
            }
            _systemUpdateTypes.Clear();

            // 处理每个网络单位的数据
            var it = _netActors.GetEnumerator();
            while (it.MoveNext()) {
                NetSyncComponent netSyncComponent = it.Current.Value;

                // 如果已死亡，则从列表中移除去
                if (netSyncComponent.IsPendingKill()) {
                    pendingKillActors.Add(it.Current.Key);

                    DestroyInfo destroyInfo = ObjectPool<DestroyInfo>.Malloc();
                    destroyInfo.NetID = netSyncComponent.NetID;
                    destroyInfo.Reason = netSyncComponent.DestroyReason;
                    frameData.DestroyInfos.Add(destroyInfo);

                    // 判断是否为玩家角色
                    if (ENetType.Player == netSyncComponent.NetType) {
                        _players.Remove(netSyncComponent);
                    }
                }
                else if (netSyncComponent.IsNewObject) {
                    netSyncComponent.IsNewObject = false;

                    SpawnInfo spawnInfo = ObjectPool<SpawnInfo>.Malloc();
                    spawnInfo.NetID = netSyncComponent.NetID;
                    spawnInfo.ConfigID = netSyncComponent.ConfigID;
                    spawnInfo.OwnerID = netSyncComponent.OwnerID;
                    spawnInfo.Position = netSyncComponent.transform.position;
                    spawnInfo.Angle = netSyncComponent.transform.rotation.eulerAngles.y;
                    spawnInfo.NetType = netSyncComponent.NetType;
                    frameData.SpawnInfos.Add(spawnInfo);
                }

                HashSet<EUpdateActorType> updateTypes = netSyncComponent.UpdateTypes;
                var updateIt = updateTypes.GetEnumerator();
                while (updateIt.MoveNext()) {
                    var type = updateIt.Current;
                    UpdateInfo updateInfo = ObjectPool<UpdateInfo>.Malloc();
                    updateInfo.UpdateType = type;
                    switch (type) {
                        case EUpdateActorType.Position:
                            // updateInfo.SetVec3ForData1(netSyncComponent.transform.position.x, netSyncComponent.transform.position.y);
                            updateInfo.SetInt32ForData1(world.CompressionVectorToInt(netSyncComponent.transform.position));
                            updateInfo.SetInt32ForData2(netSyncComponent.PredictionIndex);
                            break;
                        case EUpdateActorType.Angle:
                            updateInfo.SetFloatForData1(netSyncComponent.transform.rotation.eulerAngles.y);
                            break;
                        case EUpdateActorType.RoleState: {
                                if (netSyncComponent.gameObject.TryGetComponent<PropertyComponent>(out PropertyComponent propertyComponent)) {
                                    updateInfo.SetInt8ForData1((byte)propertyComponent.State);
                                }
                            }
                            break;
                        case EUpdateActorType.Health: {
                                if (netSyncComponent.gameObject.TryGetComponent<PropertyComponent>(out PropertyComponent propertyComponent)) {
                                    updateInfo.SetInt32ForData1((short)propertyComponent.GetHealth());
                                }
                            }
                            break;
                        case EUpdateActorType.Dodge: {
                                if (netSyncComponent.gameObject.TryGetComponent<ServerPlayerController>(out ServerPlayerController serverPlayerController)) {
                                    updateInfo.SetBoolForData1(serverPlayerController.IsDodge);
                                }
                            }
                            break;
                        case EUpdateActorType.MainWeapon: {
                                if (netSyncComponent.gameObject.TryGetComponent<LocalAbilityComponent>(out LocalAbilityComponent localAbilityComponent)) {
                                    if (localAbilityComponent.TryGetWeaponID((int)EAbilityIndex.MainWeapon, out int id))
                                        updateInfo.SetInt16ForData1((short)id);
                                }
                            }
                            break;
                        default:
                            UnityEngine.Debug.LogError("Can't Add Update Info by " + type);
                            break;
                    }
                    frameData.AddUpdateInfo(netSyncComponent.NetID, updateInfo);
                }
                netSyncComponent.CleanUpdate();
            }
            // Debug.Log("======>Send\n " + frameData.ToString());

            // 将死亡的actor从列表中移除
            foreach (var deadActor in pendingKillActors) {
                GameObject deadGO = _netActors[deadActor].gameObject;
                _netActors.Remove(deadActor);
                AssetPool.Free(deadGO);
            }
            ListPool<int>.Free(pendingKillActors);

            // 更新消息包之后发送
            ServerNetDevice device = GetNetDevice();
            if (null != device) {
                _syncMessage.SendFrameData = frameData;
                _syncMessage.UpdateData();
                device.SendMessage(_syncMessage);
            }

            if (null != OnNewFrameData)
                OnNewFrameData?.Invoke(frameData);
            ObjectPool<FrameData>.Free(frameData);
        }

        public void AddSystemUpdateInfo(EUpdateActorType updateActorType) {
            _systemUpdateTypes.Add(updateActorType);
        }
    }
}
