using UnityEngine;

namespace Hamster.SpaceWar {

    public static class GameLogicUtility {

        public static GameObject CreateShip(int configID) {
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            Vector3 spawnLocation = 0 == frameDataManager.CurrentPlayerCount ? new Vector3(-5, 0, 0) : new Vector3(5, 0, 0);

            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig))
                return frameDataManager.SpawnNetObject(0, 0, shipConfig.Path, configID, spawnLocation, ENetType.Player);

            return frameDataManager.SpawnNetObject(0, 0, "Res/Ships/GreyShip", configID, spawnLocation, ENetType.Player);
        }

        public static GameObject ClientCreateShip(int configID, int netID, Vector3 position) {
            GameObject ship = null;
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            if (!frameDataManager.HasNetObject(netID, 0)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig)) {
                    ship = frameDataManager.SpawnNetObject(netID, 0, shipConfig.Path, configID, position, ENetType.Player);
                }
            }
            return ship;
        }

        public static GameObject ServerInitShip(int configID, bool isCreateForSelf) {
            GameObject ship = CreateShip(configID);
            UnityEngine.Debug.Assert(null != ship, "ServerInitShip Ship Is Null");

            // 服务端即是客户端也是服务端
            if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.SetAuthority();
            }

            // 需要直接添加控制器
            ship.AddComponent<LocalMovementComponent>();
            LocalAbilityComponent localAbilityComponent = ship.AddComponent<LocalAbilityComponent>();
            localAbilityComponent.Init(configID);
            LocalPlayerController localPlayerController = ship.AddComponent<LocalPlayerController>();
            localPlayerController.SetIsReadByInputDevice(isCreateForSelf);


            // 需要直接接收准备完成数据
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");
            frameDataManager.CurrentPlayerCount++;

            return ship;
        }

        public static void SetPlayerOperator(int userData, int playerOperator) {
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            if (frameDataManager.TryGetNetActor(userData, out NetSyncComponent netSyncComponent)) {
                if (netSyncComponent.gameObject.TryGetComponent<LocalPlayerController>(out LocalPlayerController netPlayerController)) {
                    netPlayerController.SetOperator(playerOperator);
                }
            }
        }

        public static GameObject CreateServerBullet(int config, int ownerID, Vector3 position, ITrajectorySpanwer spanwer, out float CD) {
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            CD = 0;
            GameObject bullet = null;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(config, out Config.Abilitys abilityConfig)) {
                bullet = frameDataManager.SpawnNetObject(0, ownerID, abilityConfig.Path, config, position, ENetType.Bullet);

                TrajectoryComponent trajectoryComponent = bullet.TryGetOrAdd<TrajectoryComponent>();
                trajectoryComponent.Init(spanwer);
                
                CD = abilityConfig.CD / 1000.0f;
            }
            return bullet;
        }

        public static void SetPositionUpdate(GameObject gameObject) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.AddNewUpdate(EUpdateActorType.Position);
            }
        }

        public static void SetAngleUpdate(GameObject gameObject) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.AddNewUpdate(EUpdateActorType.Angle);
            }
        }

    }
}
