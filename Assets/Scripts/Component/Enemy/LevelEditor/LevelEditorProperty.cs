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

        public ELevelProperty LevelProperty = ELevelProperty.None;

        // 场景数据
        public string ClientAsset = string.Empty;           // 客户端表现资源
        public List<Vector3> FixLocations = new();          // 卡关中的特殊点 
        public List<Vector3> PlayerSpawnLocations = new();  // 玩家生成点
        public List<Vector3> EnemtySpawnLocations = new();  // 敌人生成点

        public List<LevelEditorProperty> LevelWaves = new();  // 敌人波数生成数据

        // 关卡波数
        public float TriggerTime = 0;
        public LevelWaveScriptObject.ELevelWaveCompleteType CompleteType = LevelWaveScriptObject.ELevelWaveCompleteType.Continue;
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
            UpdateLevelLocations("FixLocations", FixLocations);
            UpdateLevelLocations("PlayerLocations", PlayerSpawnLocations);
            UpdateLevelLocations("EnemeyLocations", EnemtySpawnLocations);

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
                        LevelConfigScriptObject levelConfigScriptObject = ScriptableObject.CreateInstance<LevelConfigScriptObject>();
                        levelConfigScriptObject.name = gameObject.name;
                        levelConfigScriptObject.ClientAsset = ClientAsset;
                        levelConfigScriptObject.FixLocations.AddRange(FixLocations);
                        levelConfigScriptObject.PlayerSpawnLocations.AddRange(PlayerSpawnLocations);
                        levelConfigScriptObject.EnemtySpawnLocations.AddRange(EnemtySpawnLocations);
                        foreach (var item in LevelWaves) {
                            levelConfigScriptObject.LevelWaves.Add(item.CreateScriptableObject() as LevelWaveScriptObject);
                        }
                        string path = "Assets/Res/Levels/" + name + ".asset";
                        levelConfigScriptObject.Save(path);
                        return levelConfigScriptObject;
                    }
                case ELevelProperty.Wave: {
                        LevelWaveScriptObject levelWaveScriptObject = ScriptableObject.CreateInstance<LevelWaveScriptObject>();
                        levelWaveScriptObject.name = gameObject.name;
                        levelWaveScriptObject.TriggerTime = TriggerTime;
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
                        unitSpawnScriptObject.ID = SpawnID;
                        unitSpawnScriptObject.LocationIndex = LocationIndex;
                        return unitSpawnScriptObject;
                    }
            }
            return null;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Hamster.SpaceWar.LevelEditorProperty))]
    public class LevelEditorPropertyInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            LevelEditorProperty levelEditorProperty = (LevelEditorProperty)target;

            levelEditorProperty.LevelProperty = (LevelEditorProperty.ELevelProperty)EditorGUILayout.EnumPopup("节点类型", levelEditorProperty.LevelProperty);

            switch (levelEditorProperty.LevelProperty) {
                case LevelEditorProperty.ELevelProperty.Spawn: {
                        levelEditorProperty.SpawnID = EditorGUILayout.IntField("生成ID", levelEditorProperty.SpawnID);
                        levelEditorProperty.LocationIndex = EditorGUILayout.IntField("生成位置下标", levelEditorProperty.LocationIndex);
                    }
                    break;
                case LevelEditorProperty.ELevelProperty.Wave: {
                        // 结束类型
                        levelEditorProperty.CompleteType = (LevelWaveScriptObject.ELevelWaveCompleteType)EditorGUILayout.EnumPopup("结束类型", levelEditorProperty.CompleteType);

                        // 触发时间
                        levelEditorProperty.TriggerTime = EditorGUILayout.Slider("触发时间", levelEditorProperty.TriggerTime, 0, 1);

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
                        for (int i = 0; i < levelEditorProperty.FixLocations.Count; i++) {
                            levelEditorProperty.FixLocations[i] = EditorGUILayout.Vector3Field("Location" + i, levelEditorProperty.FixLocations[i]);
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("玩家生成位置 " + levelEditorProperty.PlayerSpawnLocations.Count);
                        for (int i = 0; i < levelEditorProperty.PlayerSpawnLocations.Count; i++) {
                            levelEditorProperty.PlayerSpawnLocations[i] = EditorGUILayout.Vector3Field("Location" + i, levelEditorProperty.PlayerSpawnLocations[i]);
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("敌人生成位置 " + levelEditorProperty.EnemtySpawnLocations.Count);
                        for (int i = 0; i < levelEditorProperty.EnemtySpawnLocations.Count; i++) {
                            levelEditorProperty.EnemtySpawnLocations[i] = EditorGUILayout.Vector3Field("Location" + i, levelEditorProperty.EnemtySpawnLocations[i]);
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

        }




    }
#endif

}