using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class TailManager : MonoBehaviour {
        public TrailRenderer Tail = null;

        public void OnEnable() {
            OnDisable();
        }

        public void OnDisable() {
            if (null != Tail) {
                Tail.Clear();
                Tail.enabled = false;
            }
        }

        public void ShowTail() {
            if (null != Tail) {
                Tail.Clear();
                Tail.enabled = true;
            }
        }

    }
}
