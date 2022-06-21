using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Hamster;

#if UNITY_EDITOR
namespace Hamster.Editor {
    /*
     * 若A -> B -> C
     * 则B是C的直接依赖，A是C的根依赖 
     */
    public class AssetBundleNameGraphNode {
        public string Guid;
        public string Path;
        public string Name;
        public HashSet<string> BeDirectDepend = new HashSet<string>();  // 直接被依赖
        public HashSet<string> BeRootDepend = new HashSet<string>();    // 根被依赖，通过多层后被依赖
    }

    // 用于导出各个资源依赖的json
    public class AssetBundleGraphExportNode {
        public string Path;
        public HashSet<string> BeDirectDepend = new HashSet<string>();
        public HashSet<string> BeRootDepend = new HashSet<string>();
    }

    public class AssetBundleNameDirectGraph {
        private string _assetPackageName = string.Empty;
        private Dictionary<string, AssetBundleNameGraphNode> _nodes = new Dictionary<string, AssetBundleNameGraphNode>();

        public AssetBundleNameDirectGraph(string assetPackageName) {
            _assetPackageName = assetPackageName;
        }

        public void Build(string path) {
            List<string> fileList = new List<string>();
            GetDirs(path, fileList);

            // 遍历每个资源，并找到依赖
            for (int i = 0; i < fileList.Count; i++) {
                var prefabPath = fileList[i];
                string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                var prefabNode = GetNode(prefabGuid, prefabPath);

                // 获取依赖的节点并检查依赖及根依赖
                var dependenciesPath = AssetDatabase.GetDependencies(prefabPath);
                for (int j = 0; j < dependenciesPath.Length; j++) {
                    // var dependObj = AssetDatabase.LoadAssetAtPath(dependenciesPath[i], typeof(Object));
                    string dependPath = dependenciesPath[j];

                    // 是文件则跳过
                    if (Directory.Exists(dependPath))
                        continue;

                    // 指定后缀名跳过
                    if (dependPath.EndsWith(".cs") || dependPath.EndsWith(".js") || dependPath.EndsWith(".shader"))
                        continue;

                    string guid = AssetDatabase.AssetPathToGUID(dependPath);

                    if (guid == prefabGuid)
                        continue;

                    // 将当前文件的路径加入该节点的直接依赖
                    var node = GetNode(guid, dependPath);
                    node.BeDirectDepend.Add(prefabGuid);
                }
            }


            // 分析所有节点的根依赖
            Dictionary<string, AssetBundleInfo> config = new Dictionary<string, AssetBundleInfo>();
            var it = _nodes.GetEnumerator();
            while (it.MoveNext()) {
                var node = it.Current.Value;
                AnalyzeRootDepend(node.Guid);

                var importer = AssetImporter.GetAtPath(node.Path);
                // 如果没有根依赖，说明他没有被任何人依赖，使用自己的路径做为AB名
                // 如果根依赖有1个，说明只被一个资源依赖，使用依赖者的路径
                // 如果根依赖有多个，说明同时被多个资源依赖，要求单独打所，使用自己的路径做为AB名
                string assetBundleName = string.Empty;
                if (node.BeRootDepend.Count == 1) {
                    var rootDependIt = node.BeRootDepend.GetEnumerator();
                    rootDependIt.MoveNext();
                    assetBundleName = RemoveAssetPrefixAndSuffix(GetNode(rootDependIt.Current).Path);
                }
                else {
                    assetBundleName = RemoveAssetPrefixAndSuffix(node.Path);
                }

                importer.assetBundleName = assetBundleName;

                // 创建配置文件
                string key = RemoveAssetPrefixAndSuffix(node.Path.Replace('\\', '/'));
                config.Add(key, new AssetBundleInfo {
                    Path = assetBundleName.ToLower(),
                    Name = node.Name,
                    DataBase = node.Path
                });
            }

            // 将资源的AB路径及名称写入文件
            string context = JsonConvert.SerializeObject(config);
            Debug.Log(context);
            System.IO.File.WriteAllText(Application.dataPath + "/Res/AssetBundleConfig.json", context);
        }

        private AssetBundleNameGraphNode GetNode(string guid, string path = "") {
            AssetBundleNameGraphNode node = null;
            if (!_nodes.TryGetValue(guid, out node) && !string.IsNullOrEmpty(path)) {
                node = new AssetBundleNameGraphNode {
                    Path = path,
                    Guid = guid,
                    Name = System.IO.Path.GetFileName(path).Replace(System.IO.Path.GetExtension(path), "")
                };
                _nodes[guid] = node;
            }
            return node;
        }

        private void AnalyzeRootDepend(string guid, HashSet<string> beRootDepend = null) {
            var node = GetNode(guid);
            if (null == node)
                return;

            if (node.BeDirectDepend.Count <= 0) {
                if (null != beRootDepend) {
                    beRootDepend.Add(guid);
                }
            }
            else {
                if (null == beRootDepend)
                    beRootDepend = node.BeRootDepend;

                var it = node.BeDirectDepend.GetEnumerator();
                while (it.MoveNext()) {
                    AnalyzeRootDepend(it.Current, beRootDepend);
                }
            }
        }

        private static string RemoveAssetPrefixAndSuffix(string path) {
            string name = path.Substring(path.IndexOf("Assets") + 7);
            int index = name.LastIndexOf(".");
            if (index >= 0)
                return name.Replace(name.Substring(index), "");
            return name;
        }

        private static string RemoveAssetPrefix(string path) {
            return path.Substring(path.IndexOf("Assets") + 7);
        }

        private static void GetDirs(string dirPath, List<string> dirs) {
            foreach (string path in Directory.GetFiles(dirPath)) {
                //获取所有文件夹中包含指定后缀的路径
                string extension = System.IO.Path.GetExtension(path);
                if (".prefab" == extension || ".json" == extension || ".bytes" == extension || ".asset" == extension || ".spriteatlas" == extension) {
                    string temp = path.Substring(path.IndexOf("Assets")).Replace('\\', '/');
                    dirs.Add(temp);
                }
            }

            if (Directory.GetDirectories(dirPath).Length > 0) {
                foreach (string path in Directory.GetDirectories(dirPath)) {
                    GetDirs(path, dirs);
                }
            }
        }

        public void ToGraph() {
            Dictionary<string, AssetBundleGraphExportNode> exports = new Dictionary<string, AssetBundleGraphExportNode>();

            var it = _nodes.GetEnumerator();
            while (it.MoveNext()) {
                var value = it.Current.Value;
                AssetBundleGraphExportNode export = new AssetBundleGraphExportNode();
                export.Path = value.Path;
                exports[export.Path] = export;
            }

            it = _nodes.GetEnumerator();
            while (it.MoveNext()) {
                var value = it.Current.Value;

                var export = exports[value.Path];

                // 将直属数据加入
                var deDirectDependIt = value.BeDirectDepend.GetEnumerator();
                while (deDirectDependIt.MoveNext()) {
                    string path = AssetDatabase.GUIDToAssetPath(deDirectDependIt.Current);
                    export.BeDirectDepend.Add(path);
                }

                // 将根依赖数据加入
                var beRootDependIt = value.BeRootDepend.GetEnumerator();
                while (beRootDependIt.MoveNext()) {
                    string path = AssetDatabase.GUIDToAssetPath(beRootDependIt.Current);
                    export.BeRootDepend.Add(path);
                }
            }

            System.IO.File.WriteAllText(Application.dataPath + "/Res/AssetBundleDepend.json", JsonConvert.SerializeObject(exports));
        }
    }
}
#endif