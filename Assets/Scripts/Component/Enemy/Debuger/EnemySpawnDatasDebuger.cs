using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

#if UNITY_EDITOR
    public class BaseEnemeySpawnDataDebuger : MonoBehaviour {
        public virtual EnemySpawnData GetEnemySpawnData() {
            return null;
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + transform.right * 10, transform.position - transform.right * 10);
            UnityEditor.Handles.Label(transform.position, "Spawn " + gameObject.name);
        }
    }


    public class EnemySpawnDatasDebuger : MonoBehaviour {
    }


    [UnityEditor.CustomEditor(typeof(EnemySpawnDatasDebuger))]
    public class EnemySpawnDebugerInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("To Asset")) {
                ToAsset();
            }
        }

        public void ToAsset() {
            EnemySpawnDatasDebuger enemySpawnDebuger = (EnemySpawnDatasDebuger)target;
            string path = string.Format("Assets/Res/EnemySpawner/{0}.asset", enemySpawnDebuger.gameObject.name);

            EnemySpawnDatas info = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemySpawnDatas>(path);
            if (null == info) {
                info = ScriptableObject.CreateInstance<EnemySpawnDatas>();
                UnityEditor.AssetDatabase.CreateAsset(info, path);
            }

            info.Datas.Clear();
            BaseEnemeySpawnDataDebuger[] debugers = enemySpawnDebuger.gameObject.GetComponentsInChildren<BaseEnemeySpawnDataDebuger>();
            for (int i = 0; i < debugers.Length; i++) {
                BaseEnemeySpawnDataDebuger debuger = debugers[i];
                info.Datas.Add(debuger.GetEnemySpawnData());
            }

            UnityEditor.EditorUtility.SetDirty(info);
            UnityEditor.AssetDatabase.SaveAssets();
        }

    }
#endif
}