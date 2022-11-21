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
        }

        public static List<string> LocationNames = new();

        public ELevelProperty LevelProperty = ELevelProperty.None;

        // 场景数据
        public string ClientAsset = string.Empty;           // 客户端表现资源
        public Dictionary<string, Vector3> FixLocations = new();          // 卡关中的特殊点 

        public List<LevelEditorProperty> LevelWaves = new();  // 敌人波数生成数据

        // 关卡波数
        public float Time = 0;
        public LevelWaveScriptObject.ELevelWaveCompleteType CompleteType = LevelWaveScriptObject.ELevelWaveCompleteType.WaitTime;
        public List<LevelEditorProperty> UnitSpawns = new();  // 敌人生成数据

        // 类型为对象生成
        public int SpawnID = 0;
        public int LocationIndex = 0;

        public void UpdateWaveConfig() {
            UnitSpawns.Clear();
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<LevelEditorProperty>(out LevelEditorProperty temp)) {
                    if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Spawn) {
                        temp.name = string.Format("Spawn {0} int location {1}", temp.SpawnID, temp.LocationIndex);
                        UnitSpawns.Add(temp);
                    }
                }
            }
        }

        public void UpdateLevelConfig() {
            {
                FixLocations.Clear();
                LocationNames.Clear();
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
                            LocationNames.Add(temp.name);
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
                        if (temp.LevelProperty == LevelEditorProperty.ELevelProperty.Wave) {
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
                        LevelEditorProperty.LocationNames.Clear();
                        LevelEditorProperty.LocationNames.AddRange(FixLocations.Keys);

                        LevelConfigScriptObject levelConfigScriptObject = ScriptableObject.CreateInstance<LevelConfigScriptObject>();
                        levelConfigScriptObject.name = gameObject.name;
                        levelConfigScriptObject.ClientAsset = ClientAsset;
                        levelConfigScriptObject.LocationNames.AddRange(FixLocations.Keys);
                        levelConfigScriptObject.FixLocations.AddRange(FixLocations.Values);
                        foreach (var item in LevelWaves) {
                            levelConfigScriptObject.LevelWaves.Add(item.CreateScriptableObject() as LevelWaveScriptObject);
                        }
                        string path = "Assets/Res/Levels/" + name + ".asset";
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
                        UnitSpawnScriptObject unitSpawnScriptObject = ScriptableObject.CreateInstance<UnitSpawnScriptObject>();
                        unitSpawnScriptObject.name = transform.parent.name + "_" + gameObject.name;
                        unitSpawnScriptObject.ID = SpawnID;
                        unitSpawnScriptObject.LocationIndex = LocationIndex;
                        return unitSpawnScriptObject;
                    }
            }
            return null;
        }

        public void OnDrawGizmos() {
            switch (LevelProperty) {
                case ELevelProperty.Level:
                    break;
                case ELevelProperty.Wave:
                    break;
                case ELevelProperty.Location:
                    Handles.Label(transform.position, gameObject.name + "-" + LocationNames.IndexOf(gameObject.name));
                    break;
                case ELevelProperty.Spawn:
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

        public override void OnInspectorGUI() {
            serializedObject.Update();
            LevelEditorProperty levelEditorProperty = (LevelEditorProperty)target;

            levelEditorProperty.LevelProperty = (LevelEditorProperty.ELevelProperty)EditorGUILayout.EnumPopup("节点类型", levelEditorProperty.LevelProperty);

            switch (levelEditorProperty.LevelProperty) {
                case LevelEditorProperty.ELevelProperty.Spawn: {
                        levelEditorProperty.SpawnID = EditorGUILayout.IntField("生成ID", levelEditorProperty.SpawnID);
                        //levelEditorProperty.LocationIndex = EditorGUILayout.IntField("生成位置下标", levelEditorProperty.LocationIndex);
                        if (LevelEditorProperty.LocationNames.Count > 0)
                            levelEditorProperty.LocationIndex = EditorGUILayout.Popup(levelEditorProperty.LocationIndex, LevelEditorProperty.LocationNames.ToArray());
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Wave: {
                        // 结束类型
                        levelEditorProperty.CompleteType = (LevelWaveScriptObject.ELevelWaveCompleteType)EditorGUILayout.EnumPopup("结束类型", levelEditorProperty.CompleteType);

                        // 触发时间
                        if (LevelWaveScriptObject.ELevelWaveCompleteType.WaitTime == levelEditorProperty.CompleteType)
                            levelEditorProperty.Time = EditorGUILayout.Slider("持续时长", levelEditorProperty.Time, 0, 1);

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
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(levelEditorProperty);
            }
            serializedObject.ApplyModifiedProperties();
        }




    }
#endif

}