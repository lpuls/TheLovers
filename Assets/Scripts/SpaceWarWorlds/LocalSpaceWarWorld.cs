﻿using System.Reflection;
using UnityEngine;


namespace Hamster.SpaceWar {
    public class LocalSpaceWarWorld : World {
        public Vector3 WorldSize = Vector3.one;


        public void Awake() {
            ActiveWorld();
            InitWorld();
        }

        protected override void InitWorld(Assembly configAssembly = null, Assembly uiAssembly = null, Assembly gmAssemlby = null) {
            base.InitWorld(null, null, GetType().Assembly);

            GameObject mainPlayer = Asset.Load("Res/Ships/GreyShipLocal");
            mainPlayer.transform.position = Vector3.zero;
        }

        public bool InWorld(Vector3 position) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            return bounds.Contains(position);
        }

        public bool InWorld(Vector3 origin, Vector3 direction, out float distance) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            Ray ray = new Ray(origin, direction);
            return bounds.IntersectRay(ray, out distance);
        }

        public Vector3 ClampInWorld(Vector3 position, float size) {
            Bounds bounds = new Bounds(Vector3.zero, WorldSize);
            if (position.x + size >= bounds.max.x) {
                position.x = bounds.max.x - size;
            }
            if (position.x - size <= bounds.min.x) {
                position.x = bounds.min.x + size;
            }
            if (position.z + size >= bounds.max.z) {
                position.z = bounds.max.z - size;
            }
            if (position.z - size <= bounds.min.z) {
                position.z = bounds.min.z + size;
            }
            return position;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, WorldSize);
        }
#endif
    }
}
