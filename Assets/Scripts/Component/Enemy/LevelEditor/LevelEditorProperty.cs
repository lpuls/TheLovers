using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hamster.SpaceWar {

    public class LevelEditorProperty : MonoBehaviour {
#if UNITY_EDITOR
        public enum ELevelProperty {
            None,
            Level,
            Wave,
            Location,
            Spawn,
            MissionUI,
            Path,
            UI,
        }



        // public static List<string> LocationNames = new();
        public static LevelEditorProperty LevelParent = null;

        public ELevelProperty LevelProperty = ELevelProperty.None;

        // 场景数据
        public string ClientAsset = string.Empty;           // 客户端表现资源
        public Dictionary<string, Vector3> FixLocations = new();          // 卡关中的特殊点 

        public List<LevelEditorProperty> LevelWaves = new();  // 敌人波数生成数据
        public List<LevelEditorProperty> UnitPaths = new();  // 敌人移动路径数据

        // 关卡波数
        public float Time = 0;
        public ELevelWaveCompleteType CompleteType = ELevelWaveCompleteType.WaitTime;
        public List<LevelEditorProperty> UnitSpawns = new();  // 敌人生成数据


        // 类型为对象生成
        public float DelaySpawn = 0;
        public int SpawnID = 0;
        public int LocationIndex = 0;
        public string AIAssetPath = string.Empty;
        public LevelEditorProperty PathProperty = null;

        // 路径
        public int Step = 3;
        public bool IsSoomth = true;
        public List<Vector3> Paths = new();

        // MissionUI显示
        public int MissionID = 0;

        // UI显示
        public ELevelEventUI EventUI = ELevelEventUI.Warning;

        // debug
        public bool EnableDebugDraw = false;


        public void UpdatePath() {
            Paths.Clear();
            if (transform.childCount > 0) {
                Transform child = transform.GetChild(0);
                child.name = "PathNode 0";

                Paths.Add(child.position);
                Vector3 last = child.position;
                for (int i = 1; i < transform.childCount; i++) {
                    child = transform.GetChild(i);

                    Vector3 center = Vector3.Lerp(last, child.position, 0.5f);
                    Vector3 centerProject = Vector3.Project(center, last - child.position);
                    center = Vector3.MoveTowards(center, centerProject, 1.0f);

                    Vector3 rightCenter = last - center;
                    Vector3 lastCenter = child.position - center;

                    if (IsSoomth) {
                        for (int j = 1; j < Step; j++) {
                            //Vector3 temp = Vector3.Slerp(last, child.position, j * 1.0f / Step);
                            Vector3 temp = Vector3.Slerp(rightCenter, lastCenter, j * 1.0f / Step) + center;
                            Paths.Add(temp);
                        }
                    }
                    child.name = "PathNode " + i;
                    Paths.Add(child.position);
                    last = child.position;
                }

            }
        }

        public void UpdateUnitSpawn() {
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty levelEditorProperty)) {
                    if (ELevelProperty.Path == levelEditorProperty.LevelProperty) {
                        PathProperty = levelEditorProperty;
                        levelEditorProperty.UpdatePath();
                    }
                }
            }
        }

        public void UpdateWaveConfig() {
            UnitSpawns.Clear();
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty temp)) {
                    if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Spawn) {
                        temp.name = string.Format("Spawn {0} int location {1}", temp.SpawnID, temp.LocationIndex);
                        temp.UpdateUnitSpawn();
                        UnitSpawns.Add(temp);
                    }
                }
            }
        }

        public void UpdateLevelConfig() {
            {
                FixLocations.Clear();
                // LocationNames.Clear();
                Transform waveTransform = transform.Find("FixLocations");
                if (null == waveTransform) {
                    GameObject fixLocationsGameObject = new GameObject("FixLocations");
                    fixLocationsGameObject.transform.SetParent(transform, false);
                    waveTransform = fixLocationsGameObject.transform;
                }
                for (int i = 0; i < waveTransform.childCount; i++) {
                    Transform childTransform = waveTransform.GetChild(i);
                    if (childTransform.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty temp)) {
                        if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Location) {
                            FixLocations.Add(temp.name, temp.transform.position);
                            // LocationNames.Add(temp.name);
                        }
                    }
                }
            }

            {
                LevelWaves.Clear();
                Transform waveTransform = transform.Find("Waves");
                if (null == waveTransform) {
                    GameObject fixLocationsGameObject = new GameObject("Waves");
                    fixLocationsGameObject.transform.SetParent(transform, false);
                    waveTransform = fixLocationsGameObject.transform;
                }
                for (int i = 0; i < waveTransform.childCount; i++) {
                    Transform childTransform = waveTransform.GetChild(i);
                    if (childTransform.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty temp)) {
                        if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Wave 
                            || temp.LevelProperty == ELevelProperty.MissionUI
                            || temp.LevelProperty == ELevelProperty.UI) {
                            temp.UpdateWaveConfig();
                            LevelWaves.Add(temp);
                        }
                    }
                }
            }
        }

        public void UpdateLevelLocations(string name, List<Vector3> array) {
            array.Clear();
            Transform locations = transform.Find(name);
            if (null == locations) {
                GameObject fixLocationsGameObject = new GameObject(name);
                fixLocationsGameObject.transform.SetParent(transform, false);
                locations = fixLocationsGameObject.transform;
            }
            for (int i = 0; i < locations.childCount; i++) {
                Transform fixLocation = locations.GetChild(i);
                if (fixLocation.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty temp)) {
                    if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Location) {
                        array.Add(temp.transform.position);
                    }
                }
            }
        }

        public ScriptableObject CreateScriptableObject() {
            switch (LevelProperty) {
                case ELevelProperty.Level: {
                        UpdateLevelConfig();
                        // LevelEditorProperty.LocationNames.Clear();
                        // LevelEditorProperty.LocationNames.AddRange(FixLocations.Keys);

                        LevelConfigScriptObject levelConfigScriptObject = ScriptableObject.CreateInstance<LevelConfigScriptObject>();
                        levelConfigScriptObject.name = gameObject.name;
                        levelConfigScriptObject.ClientAsset = ClientAsset;
                        levelConfigScriptObject.LocationNames.AddRange(FixLocations.Keys);
                        levelConfigScriptObject.FixLocations.AddRange(FixLocations.Values);
                        foreach (var item in LevelWaves) {
                            levelConfigScriptObject.LevelWaves.Add(item.CreateScriptableObject() as LevelEventScriptObject);
                        }
                        string path = "Assets/Res/ScriptObjects/Levels/" + name + ".asset";
                        levelConfigScriptObject.Save(path);
                        return levelConfigScriptObject;
                    }
                case ELevelProperty.Wave: {
                        LevelWaveScriptObject levelWaveScriptObject = ScriptableObject.CreateInstance<LevelWaveScriptObject>();
                        levelWaveScriptObject.name = transform.parent.name + "_" + gameObject.name;
                        levelWaveScriptObject.Time = Time;
                        levelWaveScriptObject.CompleteType = CompleteType;
                        foreach (var item in UnitSpawns) {
                            levelWaveScriptObject.UnitSpawns.Add(item.CreateScriptableObject() as UnitSpawnScriptObject);
                        }
                        return levelWaveScriptObject;
                    }
                case ELevelProperty.Location:
                    break;
                case ELevelProperty.Spawn: {
                        UpdateUnitSpawn();
                        UnitSpawnScriptObject unitSpawnScriptObject = ScriptableObject.CreateInstance<UnitSpawnScriptObject>();
                        unitSpawnScriptObject.name = transform.parent.name + "_" + gameObject.name;
                        unitSpawnScriptObject.ID = SpawnID;
                        unitSpawnScriptObject.Delay = DelaySpawn;
                        unitSpawnScriptObject.LocationIndex = LocationIndex;
                        unitSpawnScriptObject.AIAssetPath = AIAssetPath;
                        if (null != PathProperty)
                            unitSpawnScriptObject.Path.AddRange(PathProperty.Paths);
                        return unitSpawnScriptObject;
                    }
                case ELevelProperty.MissionUI: {
                        LevelMissionUIScriptObject levelMissionUIScriptObject = ScriptableObject.CreateInstance<LevelMissionUIScriptObject>();
                        levelMissionUIScriptObject.name = transform.parent.name + "_" + gameObject.name;
                        levelMissionUIScriptObject.Time = Time;
                        levelMissionUIScriptObject.MissionID = MissionID;
                        return levelMissionUIScriptObject;
                    }
                case ELevelProperty.UI: {
                        LevelUIScriptObject levelUIScriptObject = ScriptableObject.CreateInstance<LevelUIScriptObject>();
                        levelUIScriptObject.Time = Time;
                        levelUIScriptObject.UIType = EventUI;
                        return levelUIScriptObject;
                    }
            }
            return null;
        }

        public void OnDrawGizmos() {
            if (!EnableDebugDraw)
                return;

            switch (LevelProperty) {
                case ELevelProperty.Level:
                    break;
                case ELevelProperty.Wave:
                    break;
                case ELevelProperty.Location: {
                        int index = 0;
                        var it = LevelEditorProperty.LevelParent.FixLocations.Keys.GetEnumerator();
                        while (it.MoveNext()) {
                            if (it.Current == gameObject.name) {
                                break;
                            }
                            index++;
                        }
                        Handles.Label(transform.position, gameObject.name + "-" + index);
                    }
                    break;
                case ELevelProperty.Spawn:
                    break;
                case ELevelProperty.Path: {
                        if (Paths.Count > 0) {
                            Gizmos.DrawWireSphere(Paths[0], 0.5f);
                            for (int i = 1; i < Paths.Count; i++) {
                                Gizmos.DrawLine(Paths[i - 1], Paths[i]);
                                Gizmos.DrawWireSphere(Paths[i], 0.5f);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Hamster.SpaceWar.LevelEditorProperty))]
    public class LevelEditorPropertyInspector : UnityEditor.Editor {

        private string[] FixLocationNames = new string[128];

        public void OnEnable() {
            LevelEditorProperty levelEditorProperty = (LevelEditorProperty)target;
            if (LevelEditorProperty.ELevelProperty.Level == levelEditorProperty.LevelProperty) {
                LevelEditorProperty.LevelParent = levelEditorProperty;
                levelEditorProperty.UpdateLevelConfig();
                System.Array.Clear(FixLocationNames, 0, 128);
                LevelEditorProperty.LevelParent.FixLocations.Keys.CopyTo(FixLocationNames, 0);
            }
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();
            LevelEditorProperty levelEditorProperty = (LevelEditorProperty)target;

            levelEditorProperty.LevelProperty = (LevelEditorProperty.ELevelProperty)EditorGUILayout.EnumPopup("节点类型", levelEditorProperty.LevelProperty);
            levelEditorProperty.EnableDebugDraw = EditorGUILayout.Toggle("启用调试", levelEditorProperty.EnableDebugDraw);

            switch (levelEditorProperty.LevelProperty) {
                case LevelEditorProperty.ELevelProperty.Spawn: {
                        levelEditorProperty.SpawnID = EditorGUILayout.IntField("生成ID", levelEditorProperty.SpawnID);
                        levelEditorProperty.DelaySpawn = EditorGUILayout.FloatField("延迟生成时间", levelEditorProperty.DelaySpawn);
                        levelEditorProperty.AIAssetPath = EditorGUILayout.TextField("行为树路径", levelEditorProperty.AIAssetPath);
                        //levelEditorProperty.LocationIndex = EditorGUILayout.IntField("生成位置下标", levelEditorProperty.LocationIndex);
                        if (LevelEditorProperty.LevelParent.FixLocations.Count > 0)
                            levelEditorProperty.LocationIndex = EditorGUILayout.Popup(levelEditorProperty.LocationIndex, FixLocationNames);
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Wave: {
                        // 结束类型
                        levelEditorProperty.CompleteType = (ELevelWaveCompleteType)EditorGUILayout.EnumPopup("结束类型", levelEditorProperty.CompleteType);

                        // 触发时间
                        if (ELevelWaveCompleteType.WaitTime == levelEditorProperty.CompleteType)
                            levelEditorProperty.Time = EditorGUILayout.FloatField("持续时长", levelEditorProperty.Time);

                        // 敌人生成数据
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("敌人生成数据 " + levelEditorProperty.UnitSpawns.Count);
                        for (int i = 0; i < levelEditorProperty.UnitSpawns.Count; i++) {
                            var it = levelEditorProperty.UnitSpawns[i];
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(it.SpawnID + " " + it.LocationIndex);
                            EditorGUILayout.EndHorizontal();
                        }

                        if (GUILayout.Button("更新")) {
                            levelEditorProperty.UpdateWaveConfig();
                        }
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Level: {
                        levelEditorProperty.ClientAsset = EditorGUILayout.TextField("客户端表现资源", levelEditorProperty.ClientAsset);
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("特殊点 " + levelEditorProperty.FixLocations.Count);
                        foreach (var item in levelEditorProperty.FixLocations) {
                            EditorGUILayout.LabelField(string.Format("{0}: {1}", item.Key, item.Value));

                        }
                        
                        if (GUILayout.Button("更新")) {
                            levelEditorProperty.UpdateLevelConfig();
                        }
                        if (GUILayout.Button("保存")) {
                            levelEditorProperty.CreateScriptableObject();
                        }
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Location: {
                        EditorGUILayout.LabelField("特殊点 " + levelEditorProperty.name);
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.MissionUI: {
                        levelEditorProperty.Time = EditorGUILayout.FloatField("持续时长", levelEditorProperty.Time);
                        levelEditorProperty.MissionID = EditorGUILayout.IntField("任务ID", levelEditorProperty.MissionID);
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Path: {
                        levelEditorProperty.IsSoomth = EditorGUILayout.Toggle("平滑", levelEditorProperty.IsSoomth);
                        if (levelEditorProperty.IsSoomth)
                            levelEditorProperty.Step = EditorGUILayout.IntField("步长", levelEditorProperty.Step);

                        int index = 0;
                        foreach (var item in levelEditorProperty.Paths) {
                            EditorGUILayout.LabelField(string.Format("Path{0}: {1}", index++, item));
                        }
                        if (GUILayout.Button("更新")) {
                            levelEditorProperty.UpdatePath();
                        }
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.UI: {
                        levelEditorProperty.Time = EditorGUILayout.FloatField("持续时长", levelEditorProperty.Time);
                        levelEditorProperty.EventUI = (ELevelEventUI)EditorGUILayout.EnumPopup("节点类型", levelEditorProperty.EventUI);
                    }
                    break;
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(levelEditorProperty);
            }
            serializedObject.ApplyModifiedProperties();
        }




    }
#endif

}