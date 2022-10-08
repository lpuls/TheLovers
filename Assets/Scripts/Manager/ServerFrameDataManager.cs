using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ServerFrameDataManager : BaseFrameDataManager {

        private int _shipCreateIndex = 0;
        private ServerNetDevice _netDevice = null;
        private S2CGameFrameDataSyncMessage _syncMessage = new S2CGameFrameDataSyncMessage();

        public Action OnGameStart;

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
                netSyncComponent.NetID = parentNetSync.GetSpawnIndex();  // --_spawnCreateIndex;
            }
            else {
                netSyncComponent.NetID = ++_shipCreateIndex;
            }
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;
            netSyncComponent.NetType = type;
            netSyncComponent.IsNewObject = true;

            newNetActor.transform.position = pos;

            _netActors.Add(netSyncComponent.NetID, netSyncComponent);

            return newNetActor;
        }

        public override void Update() {
            LogicTime += Time.deltaTime;
            while (LogicTime >= LOGIC_FRAME_TIME) {
                UpdateTickers();
                Tick();
                LogicTime -= LOGIC_FRAME_TIME;
            }
        }

        public void Tick() {
            ServerNetDevice device = GetNetDevice();
            if (!IsGameStart || null == device)
                return;

            FrameData frameData = ObjectPool<FrameData>.Malloc();
            frameData.FrameIndex = ServerLogicFrame++;

            List<int> pendingKillActors = ListPool<int>.Malloc();
            // todo 这里也许可以改成有变更再添加
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

                    continue;
                }
                else if (netSyncComponent.IsNewObject) {
                    netSyncComponent.IsNewObject = false;

                    SpawnInfo spawnInfo = ObjectPool<SpawnInfo>.Malloc();
                    spawnInfo.NetID = netSyncComponent.NetID;
                    spawnInfo.ConfigID = netSyncComponent.ConfigID;
                    spawnInfo.OwnerID = netSyncComponent.OwnerID;
                    spawnInfo.Position = netSyncComponent.transform.position;
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
                            if (netSyncComponent.TryGetComponent<SimulateComponent>(out SimulateComponent playerController)) {
                                updateInfo.SetVec3ForData2(playerController.CurrentLocation.x, playerController.CurrentLocation.z);
                                updateInfo.SetInt32ForData2(netSyncComponent.PredictionIndex);
                            }
                            break;
                        case EUpdateActorType.Angle:
                            updateInfo.SetFloatForData1(netSyncComponent.transform.rotation.eulerAngles.y);
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
            _syncMessage.SendFrameData = frameData;
            _syncMessage.UpdateData();
            device.SendMessage(_syncMessage);

            ObjectPool<FrameData>.Free(frameData);
        }
    }
}
