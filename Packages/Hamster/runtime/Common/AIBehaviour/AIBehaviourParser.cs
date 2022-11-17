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


            script.Initialize = ScriptableObject.CreateInstance<SequenceBehaviour>();
            script.Executor = ScriptableObject.CreateInstance<SequenceBehaviour>();
            script.Finish = ScriptableObject.CreateInstance<SequenceBehaviour>();

#if UNITY_EDITOR
            script.Initialize.name = "Initialize";
            script.Executor.name = "Executor";
            script.Finish.name = "Finish";
            UnityEditor.AssetDatabase.AddObjectToAsset(script.Initialize, script);
            UnityEditor.AssetDatabase.AddObjectToAsset(script.Executor, script);
            UnityEditor.AssetDatabase.AddObjectToAsset(script.Finish, script);
            UnityEditor.EditorUtility.SetDirty(script);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            int initBegin = input.IndexOf("Initialize");
            int initEnd = input.IndexOf("InitEnd\n") + "InitEnd\n".Length;
            Parse(input.Substring(initBegin, initEnd - initBegin), script.Initialize, ref index);

            int executorBegin = input.IndexOf("Executor");
            int executorEnd = input.IndexOf("ExecutorEnd") + "ExecutorEnd".Length;
            index = 0;
            Parse(input.Substring(executorBegin, executorEnd - executorBegin), script.Executor, ref index);

            int finishBegin = input.IndexOf("Finish");
            int finishEnd = input.IndexOf("FinishEnd") + "FinishEnd".Length;
            index = 0;
            Parse(input.Substring(finishBegin, finishEnd - finishBegin), script.Finish, ref index);


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
                            BaseBehaviour newRoot = ScriptableObject.CreateInstance<SequenceBehaviour>();
                            // BaseBehaviour newRoot = new SequenceBehaviour();
                            newRoot.name = command;
                            AddToRoot(root, newRoot);
                            Parse(input, newRoot, ref index);
                        }
                        break;
                    case "SelectBehaviour": {
                            SelectBehaviour newRoot = ScriptableObject.CreateInstance<SelectBehaviour>();
                            newRoot.name = command;
                            // SelectBehaviour newRoot = new SelectBehaviour();
                            SelectBehaviour.ISelectCondition condition = CreateConditionByCommand(commands);
                            newRoot.SetSelectCondition(condition);
                            AddToRoot(root, newRoot);
                            Parse(input, newRoot, ref index);
                        }
                        break;
                    case "ParallelBehaviour": {
                            BaseBehaviour newRoot = ScriptableObject.CreateInstance<ParallelBehaviour>();
                            newRoot.name = command;
                            // BaseBehaviour newRoot = new ParallelBehaviour();
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
                            if (null == baseBehaviour) {
                                Debug.LogError("Create Behaviour Failed " + commands);
                            }
                            else {
                                baseBehaviour.name = command;
                                AddToRoot(root, baseBehaviour);
                            }
                        }
                        break;
                }
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

        public static SelectBehaviour.ISelectCondition CreateConditionByCommand(string[] commands) {
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
                    return null;
            }
        }
    }
}
