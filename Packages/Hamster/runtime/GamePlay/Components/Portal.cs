using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Portal : MonoBehaviour {

    public int MapID = 0;
    public int PortalID = 0;

    public int TargetMapID = 0;
    public int TargetPortalID = 0;
    public Vector3 LocationOffset = Vector3.zero;

    public void Execute(GameObject triggerGameObject) {
        // MapProcessor.TeleportTo(Hamster.ECS.ECSContext.ActiveContext, entity, TargetMapID, TargetPortalID);
    }

    public Vector3 GetTargetLocation() {
        return transform.position + LocationOffset;
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected() {
        string context = string.Format("{0}-{1} To {2}-{3}", MapID, PortalID, TargetMapID, TargetPortalID);
        Handles.Label(transform.position, context);
        Gizmos.DrawSphere(transform.position + LocationOffset, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + LocationOffset);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(Portal))]
public class PortalInspector : Editor {

    public override void OnInspectorGUI() {
        Portal portal = target as Portal;
        EditorGUILayout.IntField("ID:", portal.PortalID);
        EditorGUILayout.IntField("场景ID:", portal.MapID);

        // 生成地图列表
        MapAndPortalInfos mapAndPortalInfos = AssetDatabase.LoadAssetAtPath<MapAndPortalInfos>("Assets/Editor/MapAndPortalInfos.asset");

        if (null == mapAndPortalInfos)
            return;

        int[] mapIDs = mapAndPortalInfos.MapKeys.ToArray();
        string[] mapNames = mapAndPortalInfos.MapNames.ToArray();

        if (null == mapIDs || mapIDs.Length <= 0)
            return;

        // 选择传送地图
        portal.TargetMapID = EditorGUILayout.IntPopup(portal.TargetMapID, mapNames, mapIDs);


        // 选择对应的传送点
        int[] portalIDs = null;
        string[] portalNames = null;
        if (mapAndPortalInfos.PortalMapKeys.Contains(portal.TargetMapID)) {
            int index = mapAndPortalInfos.PortalMapKeys.IndexOf(portal.TargetMapID);
            PortalInfos infos = mapAndPortalInfos.PortalMapValues[index];

            portalIDs = infos.Keys.ToArray();
            portalNames = infos.Names.ToArray();
        }
        if (null == portalNames || null == portalIDs || portalIDs.Length <= 0 || portalNames.Length <= 0)
            return;
        portal.TargetPortalID = EditorGUILayout.IntPopup(portal.TargetPortalID, portalNames, portalIDs);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

}
#endif