using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

class Portal2D : Portal {

    public void OnTriggerEnter2D(Collider2D collision) {
        Execute(collision.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Portal2D))]
public class Portal2DInspector : PortalInspector {
}
#endif