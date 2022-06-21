using System;
using System.Reflection;

namespace Hamster {
    public class GMAttribute : System.Attribute {
        public string GMName = string.Empty;

        public GMAttribute() {
        }

        public GMAttribute(string name) {
            GMName = name; 
        }
    }

    public static class GMAttributeProcessor {
        public static void Processor(Assembly assembly) {
#if UNITY_EDITOR
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type classType = types[i];
                MethodInfo[] methodInfos = classType.GetMethods();
                for (int j = 0; j < methodInfos.Length; j++) {
                    MethodInfo info = methodInfos[j];
                    GMAttribute attribute = info.GetCustomAttribute<GMAttribute>();
                    if (null != attribute) {
                        string name = attribute.GMName;
                        if (string.IsNullOrEmpty(name))
                            name = info.Name.Replace("GM_", "");
                        GMConsole.AddCommand(name, (string[] command)=> {
                            info.Invoke(null, new object[] { command } );
                        });
                    }
                }
            }
#endif
        }
    }
}
