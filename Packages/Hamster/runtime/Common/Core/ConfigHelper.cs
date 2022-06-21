using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Hamster {
    public class ConfigHelper {
        private Dictionary<Type, Dictionary<int, Google.Protobuf.IMessage>> _configs = new Dictionary<Type, Dictionary<int, Google.Protobuf.IMessage>>();

        public void Initialize(string path) {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                BinaryReader binaryReader = new BinaryReader(fs);
                int configCount = binaryReader.ReadInt32();
                for (int i = 0; i < configCount; i++) {
                    int byteLength = binaryReader.ReadInt32();
                    byte[] configBytes = binaryReader.ReadBytes(byteLength);

                    BinaryReader configReader = new BinaryReader(new MemoryStream(configBytes));
                    int byteCount = configReader.ReadInt32();

                    // 通过名称或着parser
                    int nameLength = configReader.ReadInt32();
                    string configName = new string(configReader.ReadChars(nameLength));
                    Type configType = Type.GetType("Config." + configName);
                    BindingFlags flag = BindingFlags.Static | BindingFlags.NonPublic;
                    FieldInfo parserFiledInfo = configType.GetField("_parser", flag);
                    var parser = parserFiledInfo.GetValue(null) as MessageParser;

                    // 读取每一个配置实体
                    Dictionary<int, Google.Protobuf.IMessage> config = new Dictionary<int, Google.Protobuf.IMessage>();
                    for (int j = 0; j < byteCount; j++) {
                        int configInstanceLength = configReader.ReadInt32();
                        byte[] configInstanceBytes = configReader.ReadBytes(configInstanceLength);

                        var instance = parser.ParseFrom(configInstanceBytes);
                        config.Add(GetID(configType, instance), instance);
                    }
                    _configs.Add(configType, config);
                }
            }
        }

        public void Initialize(byte[] binarys, Assembly assembly) {
            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(binarys))) {
                int configCount = binaryReader.ReadInt32();
                for (int i = 0; i < configCount; i++) {
                    int byteLength = binaryReader.ReadInt32();
                    byte[] configBytes = binaryReader.ReadBytes(byteLength);

                    BinaryReader configReader = new BinaryReader(new MemoryStream(configBytes));
                    int byteCount = configReader.ReadInt32();

                    // 通过名称或着parser
                    int nameLength = configReader.ReadInt32();
                    string configName = new string(configReader.ReadChars(nameLength));
                    //Type configType = Type.GetType("Config." + configName);
                    Type configType = assembly.GetType("Config." + configName);
                    BindingFlags flag = BindingFlags.Static | BindingFlags.NonPublic;
                    FieldInfo parserFiledInfo = configType.GetField("_parser", flag);
                    var parser = parserFiledInfo.GetValue(null) as MessageParser;

                    // 读取每一个配置实体
                    Dictionary<int, Google.Protobuf.IMessage> config = new Dictionary<int, Google.Protobuf.IMessage>();
                    for (int j = 0; j < byteCount; j++) {
                        int configInstanceLength = configReader.ReadInt32();
                        byte[] configInstanceBytes = configReader.ReadBytes(configInstanceLength);

                        var instance = parser.ParseFrom(configInstanceBytes);
                        config.Add(GetID(configType, instance), instance);
                    }
                    _configs.Add(configType, config);
                }
            }
        }

        private int GetID(Type type, object data) {
            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo idFiledInfo = type.GetField("iD_", flag);
            return (int)idFiledInfo.GetValue(data);
        }

        public T GetConfig<T>(int id) where T : Google.Protobuf.IMessage {
            if (_configs.TryGetValue(typeof(T), out Dictionary<int, Google.Protobuf.IMessage> configs)) {
                configs.TryGetValue(id, out Google.Protobuf.IMessage config);
                return (T)config;
            }
            return default(T);
        }

        public bool TryGetConfig<T>(int id, out T config) where T : Google.Protobuf.IMessage {
            config = default(T);
            if (_configs.TryGetValue(typeof(T), out Dictionary<int, Google.Protobuf.IMessage> configs)) {
                if (configs.TryGetValue(id, out Google.Protobuf.IMessage data)) {
                    config = (T)data;
                    return true;  
                }
            }
            return false;
        }

        public Dictionary<int, Google.Protobuf.IMessage> GetConfigs<T>() where T : Google.Protobuf.IMessage {
            if (_configs.TryGetValue(typeof(T), out Dictionary<int, Google.Protobuf.IMessage> configs)) {
                return configs;
            }
            return null;
        }
    }
}
