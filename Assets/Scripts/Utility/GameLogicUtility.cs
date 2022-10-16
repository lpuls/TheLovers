using UnityEngine;

namespace Hamster.SpaceWar {

    public static class GameLogicUtility {

        #region Server
        public static GameObject CreateShip(int configID, ENetType netType) {
            ServerFrameDataManager frameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            Vector3 spawnLocation = 0 == frameDataManager.CurrentPlayerCount ? new Vector3(-5, 0, 0) : new Vector3(5, 0, 0);

            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig))
                return frameDataManager.SpawnNetObject(0, 0, shipConfig.LogicPath, configID, spawnLocation, netType);

            return frameDataManager.SpawnNetObject(0, 0, "Res/Ships/Player/GreyPlayerShipLogic", configID, spawnLocation, ENetType.None);
        }


        public static GameObject ServerInitShip(int configID, bool isCreateForSelf) {
            // 为逻辑层生成
            GameObject ship = CreateShip(configID, ENetType.Player);
            UnityEngine.Debug.Assert(null != ship, "ServerInitShip Ship Is Null");

            ship.layer = (int)ESpaceWarLayers.PLAYER;

            // 设置为服务端飞机，如果是为自己创建的飞机则需要记录下netid，方便之后创建表现用的飞机
            if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.SetAuthority();

                if (isCreateForSelf) {
                    BaseSpaceWarWorld world = World.GetWorld<BaseSpaceWarWorld>();
                    Debug.Assert(null != world, "Client World Is Invalid");

                    world.PlayerNetID = netSyncComponent.NetID;
                }
            }

            // 需要直接添加控制器
            ship.TryGetOrAdd<MovementComponent>();
            ship.TryGetOrAdd<LocalAbilityComponent>();
            ship.TryGetOrAdd<ServerPlayerController>();


            // 需要直接接收准备完成数据
            ServerFrameDataManager frameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");
            frameDataManager.CurrentPlayerCount++;

            // 单人测试，直接开服
            if (World.GetWorld().TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                if (Config.GameModel.Single == swapData.GameModel) {
                    frameDataManager.IsGameStart = true;
                    frameDataManager.OnGameStart?.Invoke();
                }
            }


            // 为表现层生成
            // ClientCreateShip(configID, netSyncComponent.NetID, ship.transform.position, isCreateForSelf);

            return ship;
        }

        public static GameObject ServerCreateEnemy(int configID, Vector3 position, float angle) {
            GameObject ship = CreateShip(configID, ENetType.Enemy);
            UnityEngine.Debug.Assert(null != ship, "ServerInitShip Ship Is Null");

            ship.transform.forward = Vector3.back;
            ship.layer = (int)ESpaceWarLayers.ENEMY;
            ship.transform.position = position;
            ship.transform.rotation = Quaternion.Euler(0, angle, 0);

            // 服务端即是客户端也是服务端
            if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                netSyncComponent.SetAuthority();
            }

            ship.TryGetOrAdd<PathEnemy>();

            return ship;
        }

        public static void SetPlayerOperator(int userData, int playerOperator, int index) {
            ServerFrameDataManager frameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            if (frameDataManager.TryGetNetActor(userData, out NetSyncComponent netSyncComponent)) {
                if (netSyncComponent.gameObject.TryGetComponent<ServerPlayerController>(out ServerPlayerController netPlayerController)) {
                    netPlayerController.SetOperator(playerOperator, index);
                }
            }
        }

        public static GameObject CreateServerBullet(int config, int ownerID, Vector3 position, ITrajectorySpanwer spanwer, out float CD) {
            ServerFrameDataManager frameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            CD = 0;
            GameObject bullet = null;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(config, out Config.Abilitys abilityConfig)) {
                bullet = frameDataManager.SpawnNetObject(0, ownerID, abilityConfig.LogicPath, config, position, ENetType.Bullet);

                TrajectoryComponent trajectoryComponent = bullet.TryGetOrAdd<TrajectoryComponent>();
                trajectoryComponent.InitProperty(spanwer, Vector3.zero, 0);
                
                // CD = abilityConfig.CD / 1000.0f;
            }
            return bullet;
        }

        public static GameObject CreateServerBullet(int config, int ownerID, Vector3 position, Vector3 direction, ITrajectorySpanwer spanwer) {
            ServerFrameDataManager frameDataManager = World.GetWorld().GetManager<ServerFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            GameObject bullet = null;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(config, out Config.Abilitys abilityConfig)) {
                bullet = frameDataManager.SpawnNetObject(0, ownerID, abilityConfig.LogicPath, config, position, ENetType.Bullet);

                if (bullet.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                    netSyncComponent.SetAuthority();
                }

                //bullet.TryGetOrAdd<SimulateComponent>();

                TrajectoryComponent trajectoryComponent = bullet.TryGetOrAdd<TrajectoryComponent>();
                trajectoryComponent.InitProperty(spanwer, direction, abilityConfig.Speed);
            }
            return bullet;
        }


        public static void SetPositionDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.Position);
        }

        public static void SetAngleDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.Angle);
        }

        public static void SetRoleStateDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.RoleState);
        }

        public static void SetHealthDirty(GameObject gameObject) {
            SetPropertyDirty(gameObject, EUpdateActorType.Health);
        }

        private static void SetPropertyDirty(GameObject gameObject, EUpdateActorType updateType) {
            if (gameObject.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent) && netSyncComponent.IsAuthority()) {
                netSyncComponent.AddNewUpdate(updateType);
            }
        }

        #endregion

        #region Client
        public static GameObject CreateClientBullet(int config, int netID, int ownerID, Vector3 position, float angle) {
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            UnityEngine.Debug.Assert(null != frameDataManager, "Frame Data Manager Is Null");

            GameObject bullet = null;
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Abilitys>(config, out Config.Abilitys abilityConfig)) {
                bullet = frameDataManager.SpawnNetObject(netID, ownerID, abilityConfig.Path, config, position, ENetType.Bullet);
                bullet.transform.rotation = Quaternion.Euler(0, angle, 0);
                if (bullet.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                    //frameDataManager.AddPredictionActor(netSyncComponent);
                    netSyncComponent.SetSimulatedProxy();
                }
                if (bullet.TryGetComponent<TrajectoryEffectComponent>(out TrajectoryEffectComponent trajectoryEffectComponent)) {
                    trajectoryEffectComponent.EnableTrail(true);
                }
            }
            return bullet;
        }

        public static GameObject ClientCreateShip(int configID, int netID, Vector3 position, float angle, ENetType netType /*, bool userShip*/) {
            BaseSpaceWarWorld world = World.GetWorld<BaseSpaceWarWorld>();
            Debug.Assert(null != world, "Client World Is Invalid");

            GameObject ship = null;
            bool userShip = netID == world.PlayerNetID;
            ClientFrameDataManager frameDataManager = World.GetWorld().GetManager<ClientFrameDataManager>();
            if (!frameDataManager.HasNetObject(netID, 0)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.ShipConfig>(configID, out Config.ShipConfig shipConfig)) {
                    ship = frameDataManager.SpawnNetObject(netID, 0, shipConfig.Path, configID, position, netType);
                    ship.transform.rotation = Quaternion.Euler(0, angle, 0);

                    if (ship.TryGetComponent<NetSyncComponent>(out NetSyncComponent netSyncComponent)) {
                        if (userShip)
                            netSyncComponent.SetAutonomousProxy();
                        else
                            netSyncComponent.SetSimulatedProxy();
                    }
                    if (userShip) {
                        ship.TryGetOrAdd<MovementComponent>();
                        ship.AddComponent<ClientPlayerController>();
                    }
                }
            }
            return ship;
        }

        #endregion

        #region Common
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

        #endregion

    }
}
