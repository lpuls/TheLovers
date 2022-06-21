using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
namespace Hamster.Editor {
    public class BuildResources : EditorWindow {
        [MenuItem("Tools/Build AssetBundle")]
        static void ExportResource() {
            CleanAllAssetBundleName();

            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");
        
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundle/Win", BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
        }

        [MenuItem("Tools/Build Android AssetBundle")]
        static void ExportAndroidResource() {
            CleanAllAssetBundleName();

            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");

            BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundle/Android", BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
        }

        [MenuItem("Tools/Update AssetBundle")]
        static void UpdateResourceGraph() {
            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");
        }

        [MenuItem("Tools/Clean All AssetBundleName")]
        static void CleanAllAssetBundleName() {
            List<AssetImporter> importers = new List<AssetImporter>();
            CleanAllAssetBundleName(Application.dataPath, importers);
            for (int i = 0; i < importers.Count; i++) {
                EditorUtility.DisplayProgressBar("Clean AssetNameName", "Cleaning", i / importers.Count);
                importers[i].assetBundleName = string.Empty;
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Tools/Show Depedens Graph")]
        static void ShowDepedensGraph() {
            AssetBundleNameDirectGraph directGraph = new AssetBundleNameDirectGraph("assetbundlemanifest");
            directGraph.Build(Application.dataPath + "/Res");
            directGraph.ToGraph();
        }

        [MenuItem("Assets/To Atlas")]
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
    }

}
#endif