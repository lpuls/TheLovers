using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class GameOutsideWorld : World {
        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            base.InitWorld(typeof(Config.GameSetting).Assembly, null, GetType().Assembly);
        }

        private void OnGUI() {
            if (GUI.Button(new Rect(0, 0, 200, 100), "Create Room")) {
                SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Multiple, out Config.GameSetting gameSetting)) {
                    worldSwapData.Setting = gameSetting;
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ServerScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
            if (GUI.Button(new Rect(0, 125, 200, 100), "Join Room")) {
                SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Multiple, out Config.GameSetting gameSetting)) {
                    worldSwapData.Setting = gameSetting;
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ClientScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
            if (GUI.Button(new Rect(0, 250, 200, 100), "Single Play")) {
                SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Single, out Config.GameSetting gameSetting)) {
                    worldSwapData.Setting = gameSetting;
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ServerScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
        }
    }
}
