using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hamster {
    public class GMConsole : MonoBehaviour {

        [SerializeField]
        private bool _showCommands = false;
        private static Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();
        private Dictionary<string, string> _prepareCommand = new Dictionary<string, string>();

        private GUIStyle _fontStyle = new GUIStyle();

        [SerializeField]
        private string _currentCommand = string.Empty;
        [SerializeField]
        private bool _commit = false;

        public void Awake() {
            _fontStyle.fontSize = 40;
            _fontStyle.normal.background = null;
            _fontStyle.normal.textColor = Color.white;
        }

        public static void AddCommand(string command, Action<string[]> action) {
            _commands.Add(command.ToUpper(), action);
        }

        public void AddPrepareCommand(string commandName, string commandContext) {
            _prepareCommand.Add(commandName, commandContext);
        }

        public void Trigger(string command) {
            string[] commands = command.Split(' ');
            if (commands.Length <= 0)
                return;

            if (_commands.TryGetValue(commands[0].ToUpper(), out Action<string[]> action))
                action?.Invoke(commands);
        }

        public void Update() {
            //if (Input.GetKeyUp(KeyCode.F1))
            //    _showCommands = !_showCommands;
            if (_commit) {
                Trigger(_currentCommand);
                _currentCommand = string.Empty;
                _commit = false;
            }
        }

        public void OnGUI() {
            if (_showCommands) {
                GUILayout.BeginHorizontal();
                GUI.skin.textField.fontSize = 32;
                _currentCommand = GUILayout.TextField(_currentCommand, GUILayout.Width(500), GUILayout.Height(50));
                if (GUILayout.Button("Commit", GUILayout.Width(200), GUILayout.Height(50)) || Input.GetKeyDown(KeyCode.Return)) {
                    Trigger(_currentCommand);
                    _currentCommand = string.Empty;
                }

                // 显示预备指令
                var it = _prepareCommand.GetEnumerator();
                while (it.MoveNext()) {
                    if (GUILayout.Button(it.Current.Key)) {
                        Trigger(it.Current.Value);  
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

    }
}