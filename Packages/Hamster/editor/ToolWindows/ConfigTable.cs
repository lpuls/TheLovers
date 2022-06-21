using System.IO;
using UnityEditor;
using UnityEngine;

public class ConfigTableExtend : EditorWindow {
    [MenuItem("Tools/Gen Config Table")]
    static void GenConfigTable() {
        //ConfigTable table = AssetDatabase.LoadAssetAtPath<ConfigTable>("Assets/Res/Configs/ConfigTable.asset");
        //if (null == table) {
        //    table = ScriptableObject.CreateInstance<ConfigTable>();
        //    AssetDatabase.CreateAsset(table, "Assets/Res/Configs/ConfigTable.asset");
        //}

        //table.Configs.Clear();

        //DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/Res/Configs/");
        //FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        //for (int i = 0; i < fileInfos.Length; i++) {
        //    FileInfo info = fileInfos[i];
        //    string path = info.FullName.Substring(info.FullName.IndexOf("Assets"));
        //    if (fileInfos[i].Name.EndsWith(".asset")) {
        //        HamsterConfig config = AssetDatabase.LoadAssetAtPath<HamsterConfig>(path);
        //        if (null != config && config != table) {
        //            Debug.Log(path + config);
        //            table.Configs.Add(config);
        //        }
        //    }
        //}

        //EditorUtility.SetDirty(table);
        //AssetDatabase.SaveAssets();
    }
}