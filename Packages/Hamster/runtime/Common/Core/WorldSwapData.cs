using System.Collections;
using UnityEngine;

namespace Hamster {
    public class WorldSwapData : MonoBehaviour {

        public virtual void Awake() {
            GameObject.DontDestroyOnLoad(gameObject);
            SingleMonobehaviour<WorldSwapData>.GetInstance(gameObject.name);
        }

        public virtual void Clean() {
        }

    }
}