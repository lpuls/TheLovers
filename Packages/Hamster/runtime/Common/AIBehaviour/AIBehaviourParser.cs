using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster {
    public static class AIBehaviourParser {

        public delegate BaseBehaviour CreateBehaviour(string[] args);
        public delegate SelectBehaviour.ISelectCondition CreateSelectCondition(string[] args);

        public const string Text =
            "Initialize\n" +
            "InitEnd\n" +
            "Executor\n" +
            "   SequenceBehaviour\n" +
            "       WaitBehaviour;3\n" +
            "       DebugBehaviour;HelloWorld\n" +
            "       ResetBehaviour\n" +
            "   End\n" +
            "ExecutorEnd\n" + 
            "Finish\n" +
            "FinishEnd\n" +
            "";

        public static Dictionary<string, CreateBehaviour> BehaviourDict = new();
        public static Dictionary<string, CreateSelectCondition> ConditionDict = new();

        public static void Parse(string input, string savePath) {
            AIBehaviourScript script = ScriptableObject.CreateInstance<AIBehaviourScript>();

#if UNITY_EDITOR
            if (System.IO.File.Exists(savePath)) {
                System.IO.File.Delete(savePath);
            }

            UnityEditor.AssetDatabase.CreateAsset(script, savePath);
            UnityEditor.EditorUtility.SetDirty(script);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            int index = 0;

            script.Executor = ScriptableObject.CreateInstance<SequenceBehaviour>();

#if UNITY_EDITOR
            script.Executor.name = "Executor";
            (script.Executor as SequenceBehaviour).BBKey = "Base_Execute_Index";
            UnityEditor.AssetDatabase.AddObjectToAsset(script.Executor, script);
            UnityEditor.EditorUtility.SetDirty(script);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            input = input.Replace("\r\n", "\n");
            Parse(input, script.Executor, ref index);


        }

        public static void Parse(string input, BaseBehaviour root, ref int index) {
            string[] datas = input.Split("\n");
            while (index < datas.Length) {
                string command = datas[index++];
                command = command.Replace(" ", "");
                string[] commands = command.Split(";");
                
                string funcName = commands[0];
                switch (funcName) {
                    case "SequenceBehaviour": {
                            SequenceBehaviour newRoot = ScriptableObject.CreateInstance<SequenceBehaviour>();
                            newRoot.name = command;
                            newRoot.BBKey = string.Format("{0}_Execute_Index", newRoot.name);
                            AddToRoot(root, newRoot);
                            Parse(input, newRoot, ref index);
                        }
                        break;
                    case "SelectBehaviour": {
                            SelectBehaviour newRoot = ScriptableObject.CreateInstance<SelectBehaviour>();
                            newRoot.name = command;
                            AddToRoot(root, newRoot);
                            Parse(input, newRoot, ref index);
                        }
                        break;
                    case "ParallelBehaviour": {
                            ParallelBehaviour newRoot = ScriptableObject.CreateInstance<ParallelBehaviour>();
                            newRoot.BBKey = string.Format("{0}_Execute_List", newRoot.name);
                            newRoot.name = command;
                            AddToRoot(root, newRoot);
                            Parse(input, newRoot, ref index);
                        }
                        break;
                    case "End":
                    case "InitEnd":
                    case "ExecutorEnd":
                    case "FinishEnd":
                        return;
                    case "Initialize":
                    case "Executor":
                    case "Finish":
                        break;
                    default: {
                            BaseBehaviour baseBehaviour = CreateBehaviourByCommand(commands);
                            if (null != baseBehaviour) {
                                baseBehaviour.name = command;
                                AddToRoot(root, baseBehaviour);
                            }
                            else {
                                ScriptableObject selectCondition = CreateConditionByCommand(commands);
                                if (null != selectCondition) {
                                    selectCondition.name = command;
                                    AddConditionToSelect(root, selectCondition as SelectBehaviour.ISelectCondition);
                                }
                            }
                        }
                        break;
                }
            }
        }

        public static void AddConditionToSelect(BaseBehaviour root, SelectBehaviour.ISelectCondition condition) {
            SelectBehaviour selectBehaviour = root as SelectBehaviour;
            if (null != selectBehaviour) {
                (root as SelectBehaviour).AddSelectCondition(condition);
            }
        }

        public static void AddToRoot(BaseBehaviour root, BaseBehaviour node) {
            if (null == root)
                return;

            if (root is SequenceBehaviour) {
                (root as SequenceBehaviour).AddBehaviour(node);
            }
            else if (root is SelectBehaviour) {
                (root as SelectBehaviour).AddBehaviour(node);
            }
            else if (root is ParallelBehaviour) {
                (root as ParallelBehaviour).AddBehaviour(node);
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(node, root);
            UnityEditor.EditorUtility.SetDirty(root);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public static ScriptableObject CreateConditionByCommand(string[] commands) {
            switch (commands[1]) {
                case "Int32CompareCondition": {
                        Int32CompareCondition int32CompareCondition = new Int32CompareCondition();
                        if (!Enum.TryParse<ECompare>(commands[2], out int32CompareCondition.CompareType))
                            Debug.LogError("Error Int32CompareCondition Compare Type " + commands[2]);
                        int32CompareCondition.BlackboardKey = commands[3];
                        if (!int.TryParse(commands[4], out int32CompareCondition.CompareValue))
                            Debug.LogError("Error Int32CompareCondition Compare Value " + commands[4]);
                        return int32CompareCondition;
                    }
                default:
                    if (ConditionDict.TryGetValue(commands[0], out CreateSelectCondition value))
                        return value.Invoke(commands) as ScriptableObject;
                    return null;
            }
        }

        public static BaseBehaviour CreateBehaviourByCommand(string[] command) {
            switch (command[0]) {
                case "WaitBehaviour": {
                        //WaitBehaviour behaviour = new WaitBehaviour();
                        WaitBehaviour behaviour = ScriptableObject.CreateInstance<WaitBehaviour>();
                        if (float.TryParse(command[1], out float time))
                            behaviour.WaitTime = time;
                        else
                            Debug.LogError("Error Wait Behaviour Arg " + command[1]);
                        return behaviour;
                    }
                case "DebugBehaviour": {
                        //DebugBehaviour behaviour = new DebugBehaviour();
                        DebugBehaviour behaviour = ScriptableObject.CreateInstance<DebugBehaviour>();
                        behaviour.DebugInfo = command[1];
                        return behaviour;
                    }
                case "StopAndResetBehaviour": {
                        //StopAndResetBehaviour behaviour = new StopAndResetBehaviour();
                        StopAndResetBehaviour behaviour = ScriptableObject.CreateInstance<StopAndResetBehaviour>();
                        return behaviour;
                    }
                case "ResetBehaviour": {
                        //ResetBehaviour behaviour = new ResetBehaviour();
                        ResetBehaviour behaviour = ScriptableObject.CreateInstance<ResetBehaviour>();
                        return behaviour;
                    }
                default:
                    if (BehaviourDict.TryGetValue(command[0], out CreateBehaviour value))
                        return value.Invoke(command);
                    return null;
            }
        }
    }
}
