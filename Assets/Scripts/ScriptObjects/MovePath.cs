using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    
    [CreateAssetMenu(menuName = "ScriptObject/MovePath")]
    public class MovePath : ScriptableObject {
        public float Time = 1;
        public AnimationCurve X = null;
        public AnimationCurve Y = null;
        public AnimationCurve Z = null;

        public Vector3 Evaluate(float t) {
            return new Vector3(
                    X.Evaluate(t),
                    Y.Evaluate(t),
                    Z.Evaluate(t)
                );
        }
    }
}
