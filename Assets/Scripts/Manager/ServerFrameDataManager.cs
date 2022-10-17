using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ServerFrameDataManager : BaseFrameDataManager {

        private int _shipCreateIndex = 0;
        private ServerNetDevice _netDevice = null;
        private S2CGameFrameDataSyncMessage _syncMessage = new S2CGameFrameDataSyncMessage();

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

            newNetActor.transform.position = pos;

            _netActors.Add(netSyncComponent.NetID, netSyncComponent);

            return newNetActor;
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
                            updateInfo.SetVec3ForData2(netSyncComponent.transform.position.x, netSyncComponent.transform.position.y);
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
    }
}
