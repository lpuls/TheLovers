using System;
using System.IO;
using System.Text;

namespace Hamster {

    public class Packet {
        public const int PACKET_MARK = 1068;
        public const int HEAD_LENGTH = 8;  // 4位标记，4位长度

        private byte[] _data = null;

        private BinaryWriter _binaryWriter = null;
        private BinaryReader _binaryReader = null;

        public byte[] Data {
            get {
                return _data; 
            } 
        }

        public int Size {
            get;
            set;
        }

        public int MaxSize {
            get;
            private set;
        }

        public Packet(int size) {
            Size = 0;
            MaxSize = size;
            
            _data = new byte[size + HEAD_LENGTH];
            _binaryWriter = new BinaryWriter(new MemoryStream(_data));
            _binaryReader = new BinaryReader(new MemoryStream(_data));

            _binaryWriter.Write(PACKET_MARK);
            _binaryWriter.Write(HEAD_LENGTH);
        }

        public long GetLength() {
            return _binaryWriter.BaseStream.Position; 
        }

        public long GetReadIndex() {
            return _binaryReader.BaseStream.Position; 
        }

        public void Peek(int index) {
            _binaryWriter.BaseStream.Position = index + HEAD_LENGTH;
            _binaryReader.BaseStream.Position = index + HEAD_LENGTH;
        }

        public void Clean() {
            Peek(0);
            Size = 0;
        }

        public byte[] TryToBytes() {
            long location = _binaryWriter.BaseStream.Position;
            _binaryWriter.BaseStream.Position = 0;
            _binaryWriter.Write(PACKET_MARK);
            _binaryWriter.Write(Size);
            _binaryWriter.BaseStream.Position = location;
            return _data;
        }

        public byte ReadByte() {
            return _binaryReader.ReadByte();
        }

        public char ReadChar() {
            return _binaryReader.ReadChar();
        }

        public short ReadInt16() {
            return _binaryReader.ReadInt16();
        }

        public ushort ReadUInt16() {
            return _binaryReader.ReadUInt16();
        }

        public int ReadInt32() {
            return _binaryReader.ReadInt32();
        }

        public uint ReadUInt32() {
            return _binaryReader.ReadUInt32();
        }

        public long ReadInt64() {
            return _binaryReader.ReadInt64();
        }

        public ulong ReadUInt64() {
            return _binaryReader.ReadUInt64();
        }

        public float ReadFloat() {
            return _binaryReader.ReadSingle();
        }

        public string ReadString() {
            return _binaryReader.ReadString();
        }

        public byte[] ReadBytes(int length) {
            return _binaryReader.ReadBytes(length);
        }

        public void WriteByte(byte value) {
            _binaryWriter.Write(value);
            Size += 1;
        }

        public void WriteChar(char value) {
            _binaryWriter.Write(value);
            Size += 1;
        }

        public void WriteInt16(short value) {
            _binaryWriter.Write(value);
            Size += 2;
        }

        public void WriteUInt16(ushort value) {
            _binaryWriter.Write(value);
            Size += 2;
        }

        public void WriteInt32(int value) {
            _binaryWriter.Write(value);
            Size += 4;
        }

        public void WriteUInt32(int value) {
            _binaryWriter.Write(value);
            Size += 4;
        }

        public void WriteInt64(long value) {
            _binaryWriter.Write(value);
            Size += 8;
        }

        public void WriteUInt64(ulong value) {
            _binaryWriter.Write(value);
            Size += 8;
        }

        public void WriteFloat(float value) {
            _binaryWriter.Write(value);
            Size += 4;
        }

        public void WriteString(string value) {
            _binaryWriter.Write(value);
            Size += value.Length;
        }

        public void WriteBytes(byte[] bytes) {
            _binaryWriter.Write(bytes, 0, bytes.Length);
            Size += bytes.Length;
        }
    }
}
