using System.Collections;
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

        protected IEnumerator LoadScene(string path, string sceneName, Config.GameModel gameModel) {
            ShowLoading();
            SetProgress(0);
            yield return new WaitForSeconds(0.1f);

            SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
            if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)gameModel, out Config.GameSetting gameSetting)) {
                worldSwapData.Setting = gameSetting;
                worldSwapData.GameModel = gameModel;
                Asset.LoadScene(path, sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            SetProgress(10);
            yield break;
        }

        private void OnGUI() {
            if (GUI.Button(new Rect(0, 0, 200, 100), "Create Room")) {
                //SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                //if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Multiple, out Config.GameSetting gameSetting)) {
                //    worldSwapData.Setting = gameSetting;
                //    worldSwapData.GameModel = Config.GameModel.Multiple;
                //    Asset.LoadScene("Res/Scene/ServerScene", "ServerScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                //}
                StartCoroutine(LoadScene("Res/Scene/ServerScene", "ServerScene", Config.GameModel.Multiple));
            }
            if (GUI.Button(new Rect(0, 125, 200, 100), "Join Room")) {
                //SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                //if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Multiple, out Config.GameSetting gameSetting)) {
                //    worldSwapData.Setting = gameSetting;
                //    worldSwapData.GameModel = Config.GameModel.Multiple;
                //    Asset.LoadScene("Res/Scene/ClientScene", "ClientScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                //}
                StartCoroutine(LoadScene("Res/Scene/ClientScene", "ClientScene", Config.GameModel.Multiple));
            }
            if (GUI.Button(new Rect(0, 250, 200, 100), "Single Play")) {
                //SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
                //if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)Config.GameModel.Single, out Config.GameSetting gameSetting)) {
                //    worldSwapData.Setting = gameSetting;
                //    worldSwapData.GameModel = Config.GameModel.Single;
                //    Asset.LoadScene("Res/Scene/ServerScene", "ServerScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
                //}
                StartCoroutine(LoadScene("Res/Scene/ServerScene", "ServerScene", Config.GameModel.Single));
            }
        }
    }
}
