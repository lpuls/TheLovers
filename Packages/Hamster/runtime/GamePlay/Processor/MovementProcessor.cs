using UnityEngine;

namespace Hamster {

    public enum MoveLayer {
        NONE = 8,
        PLAYER,
        BLOCK,
        FLOOR,
        ENEMY,
        NATURE,
    }

    public interface IUnityInst {
        GameObject GetGameObject();
        Vector3 GetPosition();
        void SetPosition(Vector3 position);
    }

    public interface IMovment {
        bool IsMoveByTangent();
        float GetMoveSpeed();
        float GetRadius();
        float GetHeight();
        RaycastHit2D[] GetCastsForMove(Vector3 direction, float distance);
        RaycastHit2D GetCastForMove(Vector3 direction, float distance);
    }

    public interface IGravity {
        float GetJumpForce();
        void SetJumpForece(float force);
        float GetGravity();
        float GetValidFallDistance();
        float GetGravitySpeed();
        void SetGravitySpeed(float speed);
        RaycastHit2D GetCastForGravity(Vector3 direction, float distance);
        void OnLand();

    }

    public static class MovementProcessor {

        public static bool InAir(IGravity inst, IUnityInst unityInst) {
            //RaycastHit2D hitResult = inst.GetCastForGravity(Vector3.down, 0.02f);
            //return null == hitResult.collider;
            return false;
        }

        public static void DoGravity(IGravity inst, IUnityInst unityInst) {
            float g = inst.GetJumpForce() - inst.GetGravity();
            float newSpeed = inst.GetGravitySpeed() + g * Time.deltaTime;
            float distance = newSpeed * Time.deltaTime;

            bool isLand = false;
            bool isUp = Mathf.Sign(distance) > 0;
            RaycastHit2D hitResult = inst.GetCastForGravity(isUp ? Vector3.up : Vector3.down, Mathf.Abs(distance));
            if (null != hitResult.collider && unityInst.GetGameObject() != hitResult.collider && hitResult.distance <= 0.2f) {
                if (!isUp && hitResult.collider.transform.position.y < unityInst.GetPosition().y) {
                    isLand = true;
                    inst.SetJumpForece(0);
                    distance = 0;
                }
                else {
                    distance = hitResult.distance;
                    inst.SetJumpForece(0);
                }
                inst.SetGravitySpeed(0);
            }
            else {
                inst.SetGravitySpeed(newSpeed);
            }

            if (isLand) {
                inst.SetGravitySpeed(0);
                inst.OnLand();
            }
            else 
            {
                unityInst.SetPosition(unityInst.GetPosition() + Vector3.up * distance);
            }
        }

        public static void DoMove(IMovment inst, IUnityInst unityInst, Vector3 moveDirection, float moveDistance) {
            Vector3 realDirection = moveDirection;
            Vector3 beginLocation = unityInst.GetPosition();
            Debug.DrawRay(beginLocation, realDirection, Color.red);
            RaycastHit2D[] hits = inst.GetCastsForMove(realDirection, moveDistance);
            for (int i = 0; i < hits.Length; i++) {
                RaycastHit2D hit2D = hits[i];

                if (null != hit2D.collider && hit2D.collider.gameObject != unityInst.GetGameObject()) {
                    Vector2 normal = hit2D.normal;
                    Vector2 direction = (hit2D.point - (Vector2)beginLocation).normalized;

                    if (Vector2.Dot(moveDirection, direction) >= 0 && inst.IsMoveByTangent()) {
                        Vector2 orthogonal = new Vector2(normal.y / normal.x, 1);
                        Vector2 orthogonal1 = new Vector2(normal.y / normal.x, -1);
                        if (Vector2.Dot(moveDirection, orthogonal) < 0)
                            orthogonal = orthogonal1;
                        realDirection = Vector2.Dot(realDirection, orthogonal) * moveDistance * orthogonal;
                        realDirection = realDirection.normalized;
                        Debug.DrawRay(beginLocation, normal, Color.blue);
                        Debug.DrawRay(beginLocation, realDirection, Color.green);
                        Debug.DrawRay(beginLocation, orthogonal, Color.yellow);
                        Debug.DrawRay(beginLocation, direction, Color.cyan);
                    }
                }
            }

            RaycastHit2D hitResult = inst.GetCastForMove(realDirection, moveDistance);
            if (null != hitResult.collider) {
                Vector2 direction = (hitResult.point - (Vector2)beginLocation).normalized;
                if (Vector2.Dot(moveDirection, direction) > 0) {
                    moveDistance = hitResult.distance - inst.GetRadius();
                    moveDistance = Mathf.Clamp(moveDistance, 0, moveDistance);
                }
                Debug.DrawRay(unityInst.GetGameObject().transform.position, moveDistance * realDirection, Color.yellow);
            }

            unityInst.SetPosition(unityInst.GetPosition() + moveDistance * realDirection);
        }

        public static void DoMove(IMovment inst, IUnityInst unityInst, Vector3 moveDirection) {
            float moveDistance = inst.GetMoveSpeed() * Time.deltaTime;
            DoMove(inst, unityInst, moveDirection, moveDistance);
        }

    }

}
