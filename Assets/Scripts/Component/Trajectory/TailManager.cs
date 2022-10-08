using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class TailManager : MonoBehaviour {
        public TrailRenderer Tail = null;
        public GameObject Particle = null;
        public float WaitTime = 0.1f;

        private WaitForSeconds _waitSecond = null;

        public void OnDisable() {
            if (null != Tail)
                Tail.Clear();
        }
    }
}
