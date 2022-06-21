using UnityEngine;

namespace Hamster {
    public class Adapter : MonoBehaviour {

        public float Height = 9.6f;
        public float Width = 6.4f;

        // Use this for initialization
        void Start() {

            float orthographicSize = this.GetComponent<Camera>().orthographicSize;
            float aspectRatio = Screen.width * 1.0f / Screen.height;
            float cameraWidth = orthographicSize * 2 * aspectRatio;

            if (cameraWidth < Width) {
                orthographicSize = Width / (2 * aspectRatio);
                this.GetComponent<Camera>().orthographicSize = orthographicSize;
            }

        }
    }
}