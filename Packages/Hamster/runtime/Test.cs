//using Hamster;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net.Sockets;
//using UnityEngine;

//public class ConnectSuccessMessage : NetMessage {

//    public override int NetMessageID {
//        get {
//            return 1;
//        }
//    }

//    public int Value1 = 0;
//    public int Value2 = 0;

//    public override Packet ToPacket(INetDevice netDevice) {
//        Packet packet = netDevice.Malloc(16);
//        packet.WriteInt32(1);  // 写入模块ID
//        packet.WriteInt32(NetMessageID);
//        packet.WriteInt32(Value1);
//        packet.WriteInt32(Value2);
//        return packet;
//    }
//}

//public class NetMessageModule : NetModule {
//    public override int GetModuleID() {
//        return 1;
//    }

//    public override void OnReceiveMessage(Packet p) {
//        int messageID = p.ReadInt32();
//        int messageValue1 = p.ReadInt32();
//        int messageValue2 = p.ReadInt32();
//        UnityEngine.Debug.Log("Type " + messageID + " Value1 " + messageValue1 + " Value2 " + messageValue2);
//    }

//    public override void OnSendMessageFaile(Packet p, SocketError error) {
//    }
//}

//public class Command {
//    public string CommandName = string.Empty;
//}

//public class BPRuntime {
//    public delegate Command LoadCodeByte(BinaryReader binaryReader);
//    public delegate int ExecuteCall(BPRuntime runtime, BPScript script, Command command);

//    private Dictionary<string, LoadCodeByte> _loaders = new Dictionary<string, LoadCodeByte>();
//    private Dictionary<string, ExecuteCall> _executor = new Dictionary<string, ExecuteCall>();

//    public Command Load(string name, BinaryReader binaryReader) {
//        if (_loaders.TryGetValue(name, out LoadCodeByte loader)) {
//            return loader.Invoke(binaryReader);
//        }
//        return null;
//    }

//    public void RegisterLoader(string name, LoadCodeByte loader) {
//        _loaders.Add(name, loader);
//    }

//    public int Execute(string name, BPScript script, Command command) {
//        if (_executor.TryGetValue(name, out ExecuteCall executor)) {
//            return executor.Invoke(this, script, command);
//        }
//        return 1;
//    }

//    public void RegisterExecutor(string name, ExecuteCall loader) {
//        _executor.Add(name, loader);
//    }
//}

//public class BPScript {
//    public GameObject Owner {
//        get; protected set;
//    }
//    public Blackboard Local {
//        get; protected set;
//    }

//    protected Blackboard _global = new Blackboard();
//    protected int _location = 0;
//    protected List<Command> _commands = new List<Command>();

//    public void Init(BPRuntime runtime, byte[] bytes) {
//        BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
//        int count = binaryReader.ReadInt32();
//        for (int i = 0; i < count; i++) {
//            string callName = binaryReader.ReadString();
//            Command callInfo = runtime.Load(callName, binaryReader);
//            if (null == callInfo)
//                throw new Exception("Invalid CallInfo");
//            _commands.Add(callInfo);
//        }
//    }

//    public void Execute(BPRuntime runtime) {
//        while (_location < _commands.Count) {
//            Command command = _commands[_location];
//            int offset = runtime.Execute(command.CommandName, this, command);
//            _location += offset;
//        }
//        _location = 0;
//    }

//    public void Active(GameObject owner, Blackboard localBlackboard) {
//        Owner = owner;
//        Local = localBlackboard;
//        _location = 0;
//    }

//    public bool TryGetGlobalVar<T>(string name, out T value) {
//        return _global.TryGetValue<T>(name, out value);
//    }

//    public void Deactive() {
//        Owner = null;

//        Local.Clean();
//        Local = null;

//        _location = 0;
//    }

//}

//public class DebugCommand : Command {
//    public enum EDebugLevel {
//        Debug,
//        Warning,
//        Error
//    }

//    public string DebugInfo = string.Empty;
//    public EDebugLevel DebugLevel = EDebugLevel.Debug;

//    public static Command Load(BinaryReader binaryReader) {
//        DebugCommand command = new DebugCommand {
//            CommandName = "DebugCommand",
//            DebugLevel = (EDebugLevel)binaryReader.ReadInt32(),
//            DebugInfo = binaryReader.ReadString()
//        };

//        return command;
//    }

//    public static int Execute(BPRuntime runtime, BPScript script, Command command) {
//        DebugCommand debugCommand = command as DebugCommand;

//        switch (debugCommand.DebugLevel) {
//            case EDebugLevel.Debug:
//                Debug.Log(debugCommand.DebugInfo);
//                break;
//            case EDebugLevel.Warning:
//                Debug.LogWarning(debugCommand.DebugInfo);
//                break;
//            case EDebugLevel.Error:
//                Debug.LogError(debugCommand.DebugInfo);
//                break;
//        }

//        return 1;
//    }
//}

//public class MoveToTargetByTimeCommand : Command {
//    private const string ORIGIN_LOCATION_BB_KEY = "MoveToTargetByTimeCommand_ORIGIN_LOCATION";

//    public Vector3 Target = Vector3.zero;
//    public float Time = 1.0f;

//    public static Command Load(BinaryReader binaryReader) {
//        MoveToTargetByTimeCommand command = new MoveToTargetByTimeCommand {
//            CommandName = "MoveToTargetByTimeCommand",
//            Target = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()),
//            Time = binaryReader.ReadSingle()
//        };

//        return command;
//    }

//    public static int Execute(BPRuntime _, BPScript script, Command command) {
//        MoveToTargetByTimeCommand moveToTargetByTimeCommand = command as MoveToTargetByTimeCommand;

//        Vector4 Origin = script.Owner.transform.position;
//        if (!script.Local.HasValue(ORIGIN_LOCATION_BB_KEY)) {
//            script.Local.SetValue(ORIGIN_LOCATION_BB_KEY, Origin);
//        }
//        else if (!script.Local.TryGetValue(ORIGIN_LOCATION_BB_KEY, out Origin)) {
//            throw new Exception("Invalid Execute");
//        }

//        Origin.w += UnityEngine.Time.deltaTime;
//        float t = Mathf.Clamp01(Origin.w / moveToTargetByTimeCommand.Time);
//        script.Owner.transform.position = Vector3.Lerp(new Vector3(Origin.x, Origin.y, Origin.z), moveToTargetByTimeCommand.Target, t);
//        Debug.Log(string.Format("Position[{0}], Origin[{1}], Time[{2}], T[{3}]", script.Owner.transform.position, new Vector3(Origin.x, Origin.y, Origin.z).ToString(), Origin.w, t));

//        script.Local.SetValue(ORIGIN_LOCATION_BB_KEY, Origin);

//        return 1;
//    }
//}


//public class Test : MonoBehaviour {

//    private BPScript _script = new BPScript();
//    private BPRuntime _runtime = new BPRuntime();
//    private Blackboard _local = new Blackboard();

//    public void Start() {

//        byte[] datas = new byte[1024];
//        BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(datas));
//        binaryWriter.Write(1);

//        //binaryWriter.Write("DebugCommand");
//        //binaryWriter.Write(0);
//        //binaryWriter.Write("===========> This");

//        binaryWriter.Write("MoveToTargetByTimeCommand");
//        binaryWriter.Write(10f);
//        binaryWriter.Write(0.0f);
//        binaryWriter.Write(0.0f);
//        binaryWriter.Write(1.0f);

//        _runtime.RegisterLoader("DebugCommand", DebugCommand.Load);
//        _runtime.RegisterExecutor("DebugCommand", DebugCommand.Execute);
//        _runtime.RegisterLoader("MoveToTargetByTimeCommand", MoveToTargetByTimeCommand.Load);
//        _runtime.RegisterExecutor("MoveToTargetByTimeCommand", MoveToTargetByTimeCommand.Execute);

//        _script.Init(_runtime, datas);
//        _script.Active(gameObject, _local);
//    }

//    public void Update() {
//        _script.Execute(_runtime);
//    }

//}

