using UnityEngine;

namespace Hamster.SpaceWar {
    public class GameOutsideWorld : World {
        private void OnGUI() {
            if (GUI.Button(new Rect(0, 0, 200, 100), "Create Room")) {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ServerScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            if (GUI.Button(new Rect(0, 125, 200, 100), "Join Room")) {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ClientScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

            }
        }
    }
}
