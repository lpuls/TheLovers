using System.Collections;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class ServerEnemy : BaseEnemy {
        protected void MoveByDelta(Vector3 delta) {
            Vector3 preLocation = _simulateComponent.CurrentLocation;
            Vector3 newLocation = _simulateComponent.CurrentLocation + delta;
            _simulateComponent.UpdatePosition(preLocation, newLocation);
            GameLogicUtility.SetPositionDirty(gameObject);
        }

        protected void RotationByDelta(float angle) {
            Vector3 rotation = transform.rotation.eulerAngles;
            Vector3 newRotation = new Vector3(0, rotation.y + Mathf.Clamp(angle, -360, 360), 0);
            _simulateComponent.UpdateAngle(rotation.y, newRotation.y);
            GameLogicUtility.SetAngleDirty(gameObject);

        }
    }
}