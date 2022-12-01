#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hamster.Editor {
    public class CommonEditorTools : EditorWindow {

        [MenuItem("Tools/Common/Gen String File")]
        public static void GenStringFile() {
            int min = 0;
            int max = 999;
            string context = "namespace Hamster {{\n\tpublic static class CommonString {{\n\t\t public static string[] CommonIntString = {{{0}}};\n\t\t\n}}\n}}";
            string stringContext = "";
            for (int i = min; i < max; i++) {
                stringContext += string.Format("\"{0}\", ", i);
            }

            File.WriteAllText(Application.dataPath + "/scripts/CommonString.cs", string.Format(context, stringContext));
        }
    }
}
#endif
