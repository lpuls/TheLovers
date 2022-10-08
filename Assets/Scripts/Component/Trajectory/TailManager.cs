using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class TailManager : MonoBehaviour {
        public TrailRenderer Tail = null;

        public void OnEnable() {
            if (null != Tail)
                Tail.Clear();
        }

        public void OnDisable() {
            if (null != Tail)
                Tail.Clear();
        }

    }
}
