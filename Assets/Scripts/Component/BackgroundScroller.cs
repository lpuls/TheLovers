using UnityEngine;

namespace Hamster.SpaceWar {
    public class BackgroundScroller : MonoBehaviour {

        private Material _material = null;
        [SerializeField] private Vector2 _velocity = Vector2.zero;

        public void Awake() {
            _material = GetComponent<Renderer>().material;

        }

        public void Update() {
            _material.mainTextureOffset += _velocity * Time.deltaTime;
        }

    }
}
