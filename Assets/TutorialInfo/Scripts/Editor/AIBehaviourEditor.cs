using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hamster.SpaceWar {
    public class AIBehaviourEditor {

        [MenuItem("Tools/AI Parse")]
        public static void ParserScript() {

            AIBehaviourParser.BehaviourDict.Clear();
            AIBehaviourParser.BehaviourDict.Add("MoveToFixLocation", CreateMoveToFixLocation);
            AIBehaviourParser.BehaviourDict.Add("RandomMove", CreateRandomMove);
            AIBehaviourParser.BehaviourDict.Add("CastAbility", CreateCastAbility);

            List<string> paths = new List<string>();
            GetDirs(Application.dataPath + "/OriginRes/AI", paths);
            foreach (var item in paths) {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
                if (null != textAsset)
                    AIBehaviourParser.Parse(textAsset.text, "Assets/Res/AI/" + textAsset.name + ".asset");
            }
        }

        private static void GetDirs(string dirPath, List<string> dirs) {
            foreach (string path in Directory.GetFiles(dirPath)) {
                //获取所有文件夹中包含指定后缀的路径
                string extension = System.IO.Path.GetExtension(path);
                if (".txt" == extension) {
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

        private static BaseBehaviour CreateMoveToFixLocation(string[] commands) {
            MoveToFixLocation behaviour = ScriptableObject.CreateInstance<MoveToFixLocation>();
            if (int.TryParse(commands[1], out int index))
                behaviour.FixLocationIndex = index;
            else
                Debug.LogError("Error MoveToFixLocation Behaviour Arg " + commands[1]);
            return behaviour;
        }

        private static BaseBehaviour CreateRandomMove(string[] commands) {
            RandomMove behaviour = ScriptableObject.CreateInstance<RandomMove>();
            behaviour.Loop = "true" == commands[1].ToLower();
            behaviour.BBKey = string.Format("{0}_Target", string.Join('_', commands));
            return behaviour;
        }

        private static BaseBehaviour CreateCastAbility(string[] commands) {
            CastAbility behaviour = ScriptableObject.CreateInstance<CastAbility>();
            behaviour.BBKey = string.Join("_", commands) + "Interval";
            behaviour.Loop = "true" == commands[1].ToLower();
            if (int.TryParse(commands[2], out int index))
                behaviour.AbilityIndex = (EAbilityIndex)index;
            else
                Debug.LogError("Error CastAbility Behaviour Arg " + commands[2]);
            if (float.TryParse(commands[3], out float interval))
                behaviour.Interval = interval;
            else
                Debug.LogError("Error CastAbility Behaviour Arg " + commands[2]);
            return behaviour;
        }
    }
}
