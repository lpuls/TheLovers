﻿using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    [SerializeField]
    public class UnitSpawnScriptObject : ScriptableObject {
        public int ID = 0;
        public int LocationIndex = 0;

#if UNITY_EDITOR
        public void Save(ScriptableObject parent) {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, parent);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif 
    }

    [SerializeField]
    public class LevelWaveScriptObject : ScriptableObject {
        public enum ELevelWaveCompleteType {
            Continue,
            WaitAllDie,
            WaitBossDie
        }

        public float TriggerTime = 0;
        public ELevelWaveCompleteType CompleteType = ELevelWaveCompleteType.Continue;
        public List<UnitSpawnScriptObject> UnitSpawns = new();  // 敌人生成数据

#if UNITY_EDITOR
        public void Save(ScriptableObject parent) {
            UnityEditor.AssetDatabase.AddObjectToAsset(this, parent);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            foreach (var item in UnitSpawns) {
                item.Save(this);
            }

        }
#endif 
    }

    [SerializeField]
    public class LevelConfigScriptObject : ScriptableObject {
        public float LevelTime = 10.0f;                     // 关卡时长
        public string ClientAsset = string.Empty;           // 客户端表现资源
        public List<string> LocationNames = new();          // 特殊点的名称
        public List<Vector3> FixLocations = new();          // 卡关中的特殊点 

        public List<LevelWaveScriptObject> LevelWaves = new();  // 敌人波数生成数据

#if UNITY_EDITOR
        public void Save(string path) {
            if (System.IO.File.Exists(path)) {
                UnityEditor.AssetDatabase.DeleteAsset(path);
            }
            UnityEditor.AssetDatabase.CreateAsset(this, path);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            foreach (var item in LevelWaves) {
                item.Save(this);
            }
        }
#endif
    }
}
