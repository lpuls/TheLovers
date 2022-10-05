// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Temp/Config.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Config {

  /// <summary>Holder for reflection information generated from Temp/Config.proto</summary>
  public static partial class ConfigReflection {

    #region Descriptor
    /// <summary>File descriptor for Temp/Config.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ConfigReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFUZW1wL0NvbmZpZy5wcm90byIwCghBYmlsaXR5cxIKCgJJRBgBIAEoBRIM",
            "CgRQYXRoGAIgASgJEgoKAkNEGAMgASgFIjkKClNoaXBDb25maWcSCgoCSUQY",
            "ASABKAUSDAoEUGF0aBgCIAEoCRIRCglBYmlsaXR5SUQYAyADKAUiKwoIVmVj",
            "dG9yM0QSCQoBWBgBIAEoAhIJCgFZGAIgASgCEgkKAVoYAyABKAIieAoLR2Ft",
            "ZVNldHRpbmcSCgoCSUQYASABKAUSEQoJTWF4UGxheWVyGAIgASgFEhAKCFNl",
            "cnZlcklQGAMgASgJEhIKClNlcnZlclBvcnQYBCABKAUSEAoIQ2xpZW50SVAY",
            "BSABKAkSEgoKQ2xpZW50UG9ydBgGIAEoBSobCgdFeGFtcGxlEhAKDEV4YW1w",
            "bGVfTk9ORRAAQgmqAgZDb25maWdiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Config.Example), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Config.Abilitys), global::Config.Abilitys.Parser, new[]{ "ID", "Path", "CD" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Config.ShipConfig), global::Config.ShipConfig.Parser, new[]{ "ID", "Path", "AbilityID" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Config.Vector3D), global::Config.Vector3D.Parser, new[]{ "X", "Y", "Z" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Config.GameSetting), global::Config.GameSetting.Parser, new[]{ "ID", "MaxPlayer", "ServerIP", "ServerPort", "ClientIP", "ClientPort" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum Example {
    [pbr::OriginalName("Example_NONE")] None = 0,
  }

  #endregion

  #region Messages
  public sealed partial class Abilitys : pb::IMessage<Abilitys> {
    private static readonly pb::MessageParser<Abilitys> _parser = new pb::MessageParser<Abilitys>(() => new Abilitys());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Abilitys> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Config.ConfigReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Abilitys() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Abilitys(Abilitys other) : this() {
      iD_ = other.iD_;
      path_ = other.path_;
      cD_ = other.cD_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Abilitys Clone() {
      return new Abilitys(this);
    }

    /// <summary>Field number for the "ID" field.</summary>
    public const int IDFieldNumber = 1;
    private int iD_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ID {
      get { return iD_; }
      set {
        iD_ = value;
      }
    }

    /// <summary>Field number for the "Path" field.</summary>
    public const int PathFieldNumber = 2;
    private string path_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Path {
      get { return path_; }
      set {
        path_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "CD" field.</summary>
    public const int CDFieldNumber = 3;
    private int cD_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CD {
      get { return cD_; }
      set {
        cD_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Abilitys);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Abilitys other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ID != other.ID) return false;
      if (Path != other.Path) return false;
      if (CD != other.CD) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ID != 0) hash ^= ID.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      if (CD != 0) hash ^= CD.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ID != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ID);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      if (CD != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(CD);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ID != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ID);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      if (CD != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(CD);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Abilitys other) {
      if (other == null) {
        return;
      }
      if (other.ID != 0) {
        ID = other.ID;
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      if (other.CD != 0) {
        CD = other.CD;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ID = input.ReadInt32();
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
          case 24: {
            CD = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class ShipConfig : pb::IMessage<ShipConfig> {
    private static readonly pb::MessageParser<ShipConfig> _parser = new pb::MessageParser<ShipConfig>(() => new ShipConfig());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ShipConfig> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Config.ConfigReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ShipConfig() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ShipConfig(ShipConfig other) : this() {
      iD_ = other.iD_;
      path_ = other.path_;
      abilityID_ = other.abilityID_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ShipConfig Clone() {
      return new ShipConfig(this);
    }

    /// <summary>Field number for the "ID" field.</summary>
    public const int IDFieldNumber = 1;
    private int iD_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ID {
      get { return iD_; }
      set {
        iD_ = value;
      }
    }

    /// <summary>Field number for the "Path" field.</summary>
    public const int PathFieldNumber = 2;
    private string path_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Path {
      get { return path_; }
      set {
        path_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "AbilityID" field.</summary>
    public const int AbilityIDFieldNumber = 3;
    private static readonly pb::FieldCodec<int> _repeated_abilityID_codec
        = pb::FieldCodec.ForInt32(26);
    private readonly pbc::RepeatedField<int> abilityID_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> AbilityID {
      get { return abilityID_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ShipConfig);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ShipConfig other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ID != other.ID) return false;
      if (Path != other.Path) return false;
      if(!abilityID_.Equals(other.abilityID_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ID != 0) hash ^= ID.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      hash ^= abilityID_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ID != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ID);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      abilityID_.WriteTo(output, _repeated_abilityID_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ID != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ID);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      size += abilityID_.CalculateSize(_repeated_abilityID_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ShipConfig other) {
      if (other == null) {
        return;
      }
      if (other.ID != 0) {
        ID = other.ID;
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      abilityID_.Add(other.abilityID_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ID = input.ReadInt32();
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
          case 26:
          case 24: {
            abilityID_.AddEntriesFrom(input, _repeated_abilityID_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class Vector3D : pb::IMessage<Vector3D> {
    private static readonly pb::MessageParser<Vector3D> _parser = new pb::MessageParser<Vector3D>(() => new Vector3D());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Vector3D> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Config.ConfigReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Vector3D() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Vector3D(Vector3D other) : this() {
      x_ = other.x_;
      y_ = other.y_;
      z_ = other.z_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Vector3D Clone() {
      return new Vector3D(this);
    }

    /// <summary>Field number for the "X" field.</summary>
    public const int XFieldNumber = 1;
    private float x_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float X {
      get { return x_; }
      set {
        x_ = value;
      }
    }

    /// <summary>Field number for the "Y" field.</summary>
    public const int YFieldNumber = 2;
    private float y_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Y {
      get { return y_; }
      set {
        y_ = value;
      }
    }

    /// <summary>Field number for the "Z" field.</summary>
    public const int ZFieldNumber = 3;
    private float z_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Z {
      get { return z_; }
      set {
        z_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Vector3D);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Vector3D other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Z, other.Z)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (X != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
      if (Y != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
      if (Z != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Z);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (X != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (Y != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (Z != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (X != 0F) {
        size += 1 + 4;
      }
      if (Y != 0F) {
        size += 1 + 4;
      }
      if (Z != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Vector3D other) {
      if (other == null) {
        return;
      }
      if (other.X != 0F) {
        X = other.X;
      }
      if (other.Y != 0F) {
        Y = other.Y;
      }
      if (other.Z != 0F) {
        Z = other.Z;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  public sealed partial class GameSetting : pb::IMessage<GameSetting> {
    private static readonly pb::MessageParser<GameSetting> _parser = new pb::MessageParser<GameSetting>(() => new GameSetting());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GameSetting> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Config.ConfigReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GameSetting() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GameSetting(GameSetting other) : this() {
      iD_ = other.iD_;
      maxPlayer_ = other.maxPlayer_;
      serverIP_ = other.serverIP_;
      serverPort_ = other.serverPort_;
      clientIP_ = other.clientIP_;
      clientPort_ = other.clientPort_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GameSetting Clone() {
      return new GameSetting(this);
    }

    /// <summary>Field number for the "ID" field.</summary>
    public const int IDFieldNumber = 1;
    private int iD_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ID {
      get { return iD_; }
      set {
        iD_ = value;
      }
    }

    /// <summary>Field number for the "MaxPlayer" field.</summary>
    public const int MaxPlayerFieldNumber = 2;
    private int maxPlayer_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int MaxPlayer {
      get { return maxPlayer_; }
      set {
        maxPlayer_ = value;
      }
    }

    /// <summary>Field number for the "ServerIP" field.</summary>
    public const int ServerIPFieldNumber = 3;
    private string serverIP_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServerIP {
      get { return serverIP_; }
      set {
        serverIP_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ServerPort" field.</summary>
    public const int ServerPortFieldNumber = 4;
    private int serverPort_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ServerPort {
      get { return serverPort_; }
      set {
        serverPort_ = value;
      }
    }

    /// <summary>Field number for the "ClientIP" field.</summary>
    public const int ClientIPFieldNumber = 5;
    private string clientIP_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientIP {
      get { return clientIP_; }
      set {
        clientIP_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ClientPort" field.</summary>
    public const int ClientPortFieldNumber = 6;
    private int clientPort_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ClientPort {
      get { return clientPort_; }
      set {
        clientPort_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GameSetting);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GameSetting other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ID != other.ID) return false;
      if (MaxPlayer != other.MaxPlayer) return false;
      if (ServerIP != other.ServerIP) return false;
      if (ServerPort != other.ServerPort) return false;
      if (ClientIP != other.ClientIP) return false;
      if (ClientPort != other.ClientPort) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ID != 0) hash ^= ID.GetHashCode();
      if (MaxPlayer != 0) hash ^= MaxPlayer.GetHashCode();
      if (ServerIP.Length != 0) hash ^= ServerIP.GetHashCode();
      if (ServerPort != 0) hash ^= ServerPort.GetHashCode();
      if (ClientIP.Length != 0) hash ^= ClientIP.GetHashCode();
      if (ClientPort != 0) hash ^= ClientPort.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ID != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ID);
      }
      if (MaxPlayer != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(MaxPlayer);
      }
      if (ServerIP.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(ServerIP);
      }
      if (ServerPort != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(ServerPort);
      }
      if (ClientIP.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(ClientIP);
      }
      if (ClientPort != 0) {
        output.WriteRawTag(48);
        output.WriteInt32(ClientPort);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ID != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ID);
      }
      if (MaxPlayer != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(MaxPlayer);
      }
      if (ServerIP.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServerIP);
      }
      if (ServerPort != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ServerPort);
      }
      if (ClientIP.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientIP);
      }
      if (ClientPort != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ClientPort);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GameSetting other) {
      if (other == null) {
        return;
      }
      if (other.ID != 0) {
        ID = other.ID;
      }
      if (other.MaxPlayer != 0) {
        MaxPlayer = other.MaxPlayer;
      }
      if (other.ServerIP.Length != 0) {
        ServerIP = other.ServerIP;
      }
      if (other.ServerPort != 0) {
        ServerPort = other.ServerPort;
      }
      if (other.ClientIP.Length != 0) {
        ClientIP = other.ClientIP;
      }
      if (other.ClientPort != 0) {
        ClientPort = other.ClientPort;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ID = input.ReadInt32();
            break;
          }
          case 16: {
            MaxPlayer = input.ReadInt32();
            break;
          }
          case 26: {
            ServerIP = input.ReadString();
            break;
          }
          case 32: {
            ServerPort = input.ReadInt32();
            break;
          }
          case 42: {
            ClientIP = input.ReadString();
            break;
          }
          case 48: {
            ClientPort = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code