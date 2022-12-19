using System;
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
            UIManager = Single<UIManager>.GetInstance();
            // base.InitWorld(typeof(Config.GameSetting).Assembly, typeof(MainUIController).Assembly, GetType().Assembly);
            UIManager.ResetUI();

            StartCoroutine(PreloadAssets());
        }

        private IEnumerator PreloadAssets() {
            Single<AtlasManager>.GetInstance().LoadAtlas("Res/SpriteAtlas/LevelSelectIcons");
            yield return new WaitForSeconds(1.0f);
            HideLoading();

            Single<UIManager>.GetInstance().Open<LevelSelectController>();
        }

        protected IEnumerator LoadScene(string path, string sceneName, Config.GameModel gameModel) {
            ShowLoading();
            SetProgress(0);
            yield return new WaitForSeconds(0.1f);

            Single<AtlasManager>.GetInstance().UnloadAll();
            Single<UIManager>.GetInstance().CloseAll();
            Asset.UnloadAll();
            yield return new WaitForSeconds(1.0f);

            SpaceWarSwapData worldSwapData = SingleMonobehaviour<WorldSwapData>.GetInstance() as SpaceWarSwapData;
            if (null != worldSwapData && Single<ConfigHelper>.GetInstance().TryGetConfig<Config.GameSetting>((int)gameModel, out Config.GameSetting gameSetting)) {
                worldSwapData.Setting = gameSetting;
                worldSwapData.GameModel = gameModel;
                Asset.LoadScene(path, sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            SetProgress(10);
            yield break;
        }

        public void GoToSinglePlay() {
            StartCoroutine(LoadScene("Res/Scene/ServerScene", "ServerScene", Config.GameModel.Single));
        }

        public void GoToMultipleAsHoster() {
            StartCoroutine(LoadScene("Res/Scene/ServerScene", "ServerScene", Config.GameModel.Multiple));
        }

        public void GoToMultipleAsGuess() {
            StartCoroutine(LoadScene("Res/Scene/ClientScene", "ClientScene", Config.GameModel.Multiple));
        }
    }
}
