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

        public static GameObject ClientCreateShip(int configID, int netID, Vector3 position, bool userShip) {
            GameObject ship = null;
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            if (!frameDataManager.HasNetObject(netID, 0)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig)) {
                    ship = frameDataManager.SpawnNetObject(netID, 0, shipConfig.Path, configID, position, ENetType.Player);
                    ship.AddComponent<SimulateComponent>();
                    if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                        if (userShip)
                            netSyncComponent.SetAutonomousProxy();
                        else
                            netSyncComponent.SetSimulatedProxy();
                    }
                    if (userShip) {
                        ship.AddComponent<MovementComponent>();
                        ship.AddComponent<ClientPlayerController>();
                    }
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
            ship.AddComponent<SimulateComponent>();
            ship.AddComponent<MovementComponent>();
            LocalAbilityComponent localAbilityComponent = ship.AddComponent<LocalAbilityComponent>();
            localAbilityComponent.Init(configID);
            ServerPlayerController localPlayerController = ship.AddComponent<ServerPlayerController>();
            localPlayerController.SetIsReadByInputDevice(isCreateForSelf);


            // 需要直接接收准备完成数据
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");
            frameDataManager.CurrentPlayerCount++;

            // 单人测试，直接开服
            if (frameDataManager.CurrentPlayerCount >= 1) {
                frameDataManager.IsGameStart = true;
            }

            return ship;
        }

        public static void SetPlayerOperator(int userData, int playerOperator, int index) {
            BaseFrameDataManager frameDataManager = World.GetWorld().GetManager<BaseFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            if (frameDataManager.TryGetNetActor(userData, out NetSyncComponent netSyncComponent)) {
                if (netSyncComponent.gameObject.TryGetComponent<ServerPlayerController>(out ServerPlayerController netPlayerController)) {
                    netPlayerController.SetOperator(playerOperator, index);
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

        public static GameObject CreateClientBullet(int config, int ownerID, Vector3 position) {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            GameObject bullet = null;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(config, out Config.Abilitys abilityConfig)) {
                bullet = frameDataManager.SpawnNetObject(0, ownerID, abilityConfig.Path, config, position, ENetType.Bullet);
                if (bullet.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                    frameDataManager.AddPredictionActor(netSyncComponent);
                }
            }
            return bullet;
        }

        public static void SetPositionDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.Position);
        }

        public static void SetAngleDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.Angle);
        }

        private static void SetPropertyDirty(GameObject gameObject, EUpdateActorType updateType) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent) && netSyncComponent.IsAuthority()) {
                netSyncComponent.AddNewUpdate(updateType);
            }
        }

        public static void GetOperateFromInput(Transform transform, int operate, out Vector3 direction, out bool castAbility1) {
            direction = Vector3.zero;
            castAbility1 = false;
            for (int i = 0; i < (int)EInputValue.Max; i++) {
                EInputValue value = (EInputValue)i;
                if (1 == ((operate >> i) & 1)) {
                    switch (value) {
                        case EInputValue.MoveUp:
                            direction += transform.forward;
                            break;
                        case EInputValue.MoveDown:
                            direction -= transform.forward;
                            break;
                        case EInputValue.MoveLeft:
                            direction -= transform.right;
                            break;
                        case EInputValue.MoveRight:
                            direction += transform.right;
                            break;
                        case EInputValue.Ability1:
                            castAbility1 = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static int ReadKeyboardInput(InputKeyMapValue inputKeyMapValue) {
            int operate = 0;
            for (int i = 0; i < inputKeyMapValue.InputKeys.Count; i++) {
                KeyCode keyCode = inputKeyMapValue.InputKeys[i];
                if (Input.GetKey(keyCode)) {
                    operate |= (1 << (int)inputKeyMapValue.InputValues[i]);
                }
            }
            return operate;
        }

    }
}
