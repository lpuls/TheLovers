﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
namespace Hamster.Editor {
    public class BuildResources : EditorWindow {

        private static void BuildByPlatform(string platform, BuildTarget buildTarget, bool deleteFolder) {
            CleanAllAssetBundleName();

            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/GameConfig.json");
            GameConfig gameConfig = JsonUtility.FromJson<GameConfig>(textAsset.text);
            if (gameConfig.FindPlatformConfig(platform, out PlatformConfig value)) {
                string path = string.Format("{0}{1}", Application.dataPath, value.BuildAssetBundlePath);
                bool exitstFile = Directory.Exists(path);
                if (deleteFolder) {
                    if (exitstFile) {
                        Directory.Delete(path, true);
                    }
                }
                if (!exitstFile) {
                    Directory.CreateDirectory(path);
                }
                BuildPipeline.BuildAssetBundles(path,
                    BuildAssetBundleOptions.StrictMode,
                    buildTarget);
            }
        }

        [MenuItem("Tools/Build/Build Win Editor AssetBundle", false, 100)]
        static void ExportWinEditorResource() {
            BuildByPlatform("WindowsEditor", BuildTarget.StandaloneWindows, false);
        }

        [MenuItem("Tools/Build/Build Win Player AssetBundle", false, 100)]
        static void ExportWinPlayerResource() {
            BuildByPlatform("WindowsPlayer", BuildTarget.StandaloneWindows, true);
        }

        private static void BuildPlayer(string platformName) {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/GameConfig.json");
            GameConfig gameConfig = JsonUtility.FromJson<GameConfig>(textAsset.text);
            if (gameConfig.FindPlatformConfig(platformName, out PlatformConfig value)) {
                string path = string.Format("{0}{1}", Application.dataPath, value.BuildPlayerPath);

                // 清理整个文件夹
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);

                // 开始构建
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = gameConfig.BuildScenes.ToArray();
                buildPlayerOptions.locationPathName = string.Format("{0}/{1}.exe", path, gameConfig.GameName);  // path + "/Win.exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                if (value.IsRelease)
                    buildPlayerOptions.options = BuildOptions.None;
                else
                    buildPlayerOptions.options = BuildOptions.AllowDebugging | BuildOptions.Development;

                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }
        }

        [MenuItem("Tools/Build/Build Win Player", false, 100)]
        static void BuildWinPlayer() {
            BuildPlayer(RuntimePlatform.WindowsPlayer.ToString());
        }

        [MenuItem("Tools/Build/Build Android AssetBundle", false, 111)]
        static void ExportAndroidResource() {
            BuildByPlatform("Android", BuildTarget.Android, true);
        }

        [MenuItem("Tools/Build/Build Android Player", false, 111)]
        static void BuildAndroidPlayer() {
            BuildPlayer(RuntimePlatform.Android.ToString());
        }

        [MenuItem("Tools/Build/Build Web AssetBundle", false, 122)]
        static void ExportWebResource() {
            BuildByPlatform("WebGLPlayer", BuildTarget.Android, true);
        }

        [MenuItem("Tools/Build/Build Web Player", false, 122)]
        static void BuildWebPlayer() {
            BuildPlayer(RuntimePlatform.WebGLPlayer.ToString());
        }

        [MenuItem("Tools/Build/Update AssetBundle", false, 133)]
        static void UpdateResourceGraph() {
            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");
        }

        [MenuItem("Tools/Build/Clean All AssetBundleName", false, 133)]
        static void CleanAllAssetBundleName() {
            List<AssetImporter> importers = new List<AssetImporter>();
            CleanAllAssetBundleName(Application.dataPath, importers);
            for (int i = 0; i < importers.Count; i++) {
                EditorUtility.DisplayProgressBar("Clean AssetNameName", "Cleaning", i / importers.Count);
                importers[i].assetBundleName = string.Empty;
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Tools/Build/Show Depedens Graph", false, 133)]
        static void ShowDepedensGraph() {
            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");
            directGraph.ToGraph();
        }

        [MenuItem("Assets/Tools/To Atlas")]
        static void PackageAtlas() {
            Debug.Log("Start Create Sprite Atlas");
            if (null == Selection.objects ||
                0 >= Selection.objects.Length ||
                0 >= Selection.assetGUIDs.Length) {
                Debug.Log("请选中图片所在的文件夹");
                return;
            }

            for (int index = 0; index < Selection.objects.Length; index++) {

                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[index]);

                if (string.IsNullOrEmpty(path)) {
                    Debug.Log("请选中图片所在的文件夹");
                    return;
                }

                string fileName = System.IO.Path.GetFileName(path);

                List<Object> spriteList = new List<Object>();
                string[] files = AssetDatabase.FindAssets("t:Texture", new string[] { path });
                for (int i = 0; i < files.Length; i++) {
                    string filePath = AssetDatabase.GUIDToAssetPath(files[i]);
                    string assetPath = filePath.Substring(filePath.IndexOf("Assets"));
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if (texture) {
                        spriteList.Add(texture);
                    }
                }
                SpriteAtlas spriteAtlas = new SpriteAtlas();

                SpriteAtlasExtensions.Add(spriteAtlas, spriteList.ToArray());
                SpriteAtlasPackingSettings spriteAtlasPackingSettings = new SpriteAtlasPackingSettings {
                    enableRotation = false,
                    enableTightPacking = false,
                    padding = 4,
                    blockOffset = 1
                };
                SpriteAtlasExtensions.SetPackingSettings(spriteAtlas, spriteAtlasPackingSettings);
                string spritePath = "Assets/Res/SpriteAtlas/" + fileName + ".spriteatlas";
                spriteAtlas.SetIncludeInBuild(false);
                AssetDatabase.CreateAsset(spriteAtlas, spritePath);
            }
            AssetDatabase.Refresh();

            Debug.Log("End Create Sprite Atlas");
        }

        public static void CleanAllAssetBundleName(string path, List<AssetImporter> importers) {
            DirectoryInfo folder = new DirectoryInfo(path);
            if (!folder.Exists)
                return;

            // 清理当前文件夹下的文件
            foreach (FileInfo file in folder.GetFiles()) {
                if (file.Name.EndsWith(".meta") || file.Name.EndsWith(".cs") || file.Name.EndsWith(".js"))
                    continue;

                string assetPath = file.FullName.Substring(file.FullName.IndexOf("Assets"));
                var importer = AssetImporter.GetAtPath(assetPath);
                if (null != importer) {
                    importers.Add(importer);
                    // importer.assetBundleName = string.Empty;
                }
            }

            // 清理当前文件夹下的文件夹
            foreach (DirectoryInfo direction in folder.GetDirectories()) {
                CleanAllAssetBundleName(direction.FullName, importers);
            }
        }

        [MenuItem("CONTEXT/Transform/SavePrefab")]
        static public void SavePrefab() {
            GameObject source = PrefabUtility.GetPrefabParent(Selection.activeGameObject) as GameObject;
            if (source == null)
                return;
            string prefabPath = AssetDatabase.GetAssetPath(source).ToLower();
            if (prefabPath.EndsWith(".prefab") == false)
                return;
            PrefabUtility.ReplacePrefab(Selection.activeGameObject, source, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
        }
    }

}
#endif