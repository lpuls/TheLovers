using UnityEngine;

namespace Hamster {
    public static class VectorExtend {
        public static Vector3 SetX(this Vector3 vec, float x) {
            return new Vector3(x, vec.y, vec.z);
        }

        public static Vector3 SetY(this Vector3 vec, float y) {
            return new Vector3(vec.x, y, vec.z);
        }

        public static Vector3 SetZ(this Vector3 vec, float z) {
            return new Vector3(vec.x, vec.y, z);
        }

        public static Vector2 SetX(this Vector2 vec, float x) {
            return new Vector2(x, vec.y);
        }

        public static Vector2 SetY(this Vector2 vec, float y) {
            return new Vector2(vec.x, y);
        }

    }
}
