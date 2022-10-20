using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class OverheadHealthUI : MonoBehaviour {
        public float Size = 10.0f;
        public Color HealthColor = Color.red;
        public SpriteRenderer HealthProgress = null;

        public void SetHealth(int value, int max) {
            float t = value * 1.0f / max;
            float width = Size * t;

            HealthProgress.size = new Vector2(width, HealthProgress.size.y);
        }

        private void OnDisable() {
            HealthProgress.size = new Vector2(Size, HealthProgress.size.y);
        }
    }
}