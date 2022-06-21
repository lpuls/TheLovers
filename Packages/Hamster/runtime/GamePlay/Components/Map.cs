using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Map : MonoBehaviour {
    public int ID = 0;
    public List<Portal> Portals = new List<Portal>(32);
    public AudioClip BGM = null;

    public void Awake() {
        if (null != BGM) {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource.clip != BGM) {
                audioSource.clip = BGM;
                audioSource.Play();
            }
        }
    }

#if UNITY_EDITOR
    public string MapName = string.Empty;
    public void UpdateMapAndPortalInfos() {
        MapAndPortalInfos mapAndPortalInfos = AssetDatabase.LoadAssetAtPath<MapAndPortalInfos>("Assets/Editor/MapAndPortalInfos.asset");
        if (null == mapAndPortalInfos) {
            mapAndPortalInfos = ScriptableObject.CreateInstance<MapAndPortalInfos>();
            AssetDatabase.CreateAsset(mapAndPortalInfos, "Assets/Editor/MapAndPortalInfos.asset");
        }

        // 添加地图信息
        if (!mapAndPortalInfos.MapKeys.Contains(ID)) {
            mapAndPortalInfos.MapKeys.Add(ID);
            mapAndPortalInfos.MapNames.Add(MapName);
        }

        PortalInfos infos = null;
        if (!mapAndPortalInfos.PortalMapKeys.Contains(ID)) {
            mapAndPortalInfos.PortalMapKeys.Add(ID);
            infos = new PortalInfos();
            mapAndPortalInfos.PortalMapValues.Add(infos);
        }
        else {
            int index = mapAndPortalInfos.PortalMapKeys.IndexOf(ID);
            infos = mapAndPortalInfos.PortalMapValues[index];
        }
        infos.Keys.Clear();
        infos.Names.Clear();
        for (int i = 0; i < Portals.Count; i++) {
            Portals[i].PortalID = i + 1;
            infos.Keys.Add(Portals[i].PortalID);
            infos.Names.Add(Portals[i].name);
        }

        EditorUtility.SetDirty(mapAndPortalInfos);
        AssetDatabase.SaveAssets();
    }

#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(Map))]
public class MapInspector : Editor {
    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();

        Map map = target as Map;
        EditorGUILayout.BeginVertical();

        map.ID = EditorGUILayout.IntField("场景ID:", map.ID);
        map.MapName = EditorGUILayout.TextField("场景名称:", map.MapName);
        map.BGM = EditorGUILayout.ObjectField("背景音乐:", map.BGM, typeof(AudioClip)) as AudioClip;
        for (int i = 0; i < map.Portals.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("传送" + i);
            EditorGUILayout.ObjectField(map.Portals[i], typeof(Portal));
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("更新")) {
            map.Portals.Clear();
            Portal[] portals = map.GetComponentsInChildren<Portal>();
            for (int i = 0; i < portals.Length; i++) {
                portals[i].MapID = map.ID;
                portals[i].PortalID = (i + 1);
                map.Portals.Add(portals[i]);
            }
            map.UpdateMapAndPortalInfos();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorGUILayout.EndVertical();

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif