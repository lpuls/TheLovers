using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class TailManager : MonoBehaviour {
        public GameObject Tail = null;
        public GameObject Particle = null;
        public float WaitTime = 0.1f;

        private WaitForSeconds _waitSecond = null;

        //public void Awake() {
        //    _waitSecond = new WaitForSeconds(WaitTime);
        //}

        //public void OnEnable() {
        //    StartCoroutine(EnableTail());
        //}

        public void OnDisable() {
            if (null != Tail)
                Tail.SetActive(false);
            if (null != Particle)
                Particle.SetActive(false);
        }

        //public IEnumerator EnableTail() {
        //    yield return _waitSecond;
        //    if (null != Tail)
        //        Tail.SetActive(true);
        //    if (null != Particle)
        //        Particle.SetActive(true);
        //}

        public void ShowEffect() {
            if (null != Tail)
                Tail.SetActive(true);
            if (null != Particle)
                Particle.SetActive(true);
        }
    }
}
