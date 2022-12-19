using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class GameEnterWorld : World {
        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            ConfigHelper = Single<ConfigHelper>.GetInstance();
            UIManager = Single<UIManager>.GetInstance();
            UIManager.ResetUI();
            base.InitWorld(typeof(Config.GameSetting).Assembly, typeof(MainUIController).Assembly, GetType().Assembly);

            StartCoroutine(EnterGame());
        }

        protected IEnumerator EnterGame() {
            ShowLoading();
            SetProgress(0);
            yield return new WaitForSeconds(0.1f);

            Single<UIManager>.GetInstance().CloseAll();
            Asset.UnloadAll();
            yield return new WaitForSeconds(1.0f);

            Asset.LoadScene("Res/Scene/GameOutsideScene", "GameOutsideScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            SetProgress(100);
        }
    }
}
