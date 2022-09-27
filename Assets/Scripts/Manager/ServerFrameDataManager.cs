using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ServerFrameDataManager : BaseFrameDataManager {

        private int _shipCreateIndex = 0;
        private int _spawnCreateIndex = 0;
        private ServerNetDevice _netDevice = null;
        private S2CGameFrameDataSyncMessage _syncMessage = new S2CGameFrameDataSyncMessage();

        public int ServerLogicFrame {
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
                netSyncComponent.NetID = --_spawnCreateIndex;
            }
            else {
                netSyncComponent.NetID = ++_shipCreateIndex;
            }
            netSyncComponent.OwnerID = ownerID;
            netSyncComponent.ConfigID = configID;
            netSyncComponent.PendingKill = false;
            netSyncComponent.NetType = type;

            newNetActor.transform.position = pos;

            _netActors.Add(netSyncComponent.NetID, netSyncComponent);

            return newNetActor;
        }

        public override void Update() {
            if (!IsGameStart)
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
                    continue;
                }

                // 记录数据
                switch (netSyncComponent.NetType) {
                    case ENetType.Player:
                        PlayerInfo playerInfo = ObjectPool<PlayerInfo>.Malloc();
                        playerInfo.ID = netSyncComponent.NetID;
                        playerInfo.Angle = 0;
                        playerInfo.OwnerID = 0;
                        playerInfo.Tags = 0;
                        playerInfo.Health = 100;
                        playerInfo.X = netSyncComponent.transform.position.x;
                        playerInfo.Y = netSyncComponent.transform.position.y;
                        frameData.PlayerInfos.Add(playerInfo);
                        break;
                    case ENetType.Bullet:
                        SpawnActorInfo spawnActorInfo = ObjectPool<SpawnActorInfo>.Malloc();
                        spawnActorInfo.ID = netSyncComponent.NetID;
                        spawnActorInfo.OwnerID = netSyncComponent.OwnerID;
                        spawnActorInfo.Angle = 0;
                        spawnActorInfo.X = netSyncComponent.transform.position.x;
                        spawnActorInfo.Y = netSyncComponent.transform.position.y;
                        frameData.SpawnActorInfos.Add(spawnActorInfo);
                        break;
                }

            }
            Debug.Log("======>Send\n " + frameData.ToString());

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
            GetNetDevice().SendMessage(_syncMessage);

            ObjectPool<FrameData>.Free(frameData);
        }
    }
}
