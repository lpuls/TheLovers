using UnityEngine;

namespace Hamster.SpaceWar {
    public static class PlayerSpawnUtility {

        public static GameObject CreateShip(int configID) {
            FrameDataManager frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            Vector3 spawnLocation = 0 == frameDataManager.CurrentPlayerCount ? new Vector3(-5, 0, 0) : new Vector3(5, 0, 0);

            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig))
                return frameDataManager.SpawnServerNetObject(0, shipConfig.Path, configID, spawnLocation);

            return frameDataManager.SpawnServerNetObject(0, "Res/Ships/GreyShip", configID, spawnLocation);
        }

        public static GameObject ClientCreateShip(int configID, int netID, Vector3 position) {
            FrameDataManager frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
            if (!frameDataManager.HasNetObject(netID, 0)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig))
                    return frameDataManager.SpawnServerNetObject(0, shipConfig.Path, configID, position);
            }
            return null;
        }

        public static GameObject ServerInitShip(int configID) {
            GameObject ship = CreateShip(configID);
            UnityEngine.Debug.Assert(null != ship, "ServerInitShip Ship Is Null");

            // 服务端即是客户端也是服务端
            
            // 需要直接添加控制器
            ship.AddComponent<NetPlayerController>();

            // 需要直接接收准备完成数据
            FrameDataManager frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");
            frameDataManager.CurrentPlayerCount++;

            return ship;
        }

    }
}
