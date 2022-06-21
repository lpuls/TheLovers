using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster {
    public class PacketManager {
        private static PacketManager _instance = null;
        public static PacketManager GetInstance() {
            if (null == _instance)
                _instance = new PacketManager();
            return _instance;
        }

        private List<Packet>[] _packetPool = new List<Packet>[10];
        private Queue<Packet> _packetQueue = new Queue<Packet>();
        private Packet _readingPacket = null;
        private byte[] _data = new byte[4096];
        private BinaryReader _binaryReader = null;

        public PacketManager() {
            for (int i = 0; i < _packetPool.Length; i++) {
                _packetPool[i] = new List<Packet>();
            } 
        }

        public Packet Malloc(int size) {
            size |= (size >> 1);
            size |= (size >> 2);
            size |= (size >> 4);
            size |= (size >> 8);
            size += 1;
            for (int i = 0; i < 32; i++) {
                if (1 == (size >> i & 1)) {
                    return MallocImpl(i - 1);
                }
            }
            return null;
        }

        public void Free(Packet packet) {
            int size = packet.Size;
            size |= (size >> 1);
            size |= (size >> 2);
            size |= (size >> 4);
            size |= (size >> 8);
            size += 1;
            for (int i = 0; i < 32; i++) {
                if (1 == (size >> i & 1)) {
                    if (i >= 0 && i < _packetPool.Length) {
                        List<Packet> pool = _packetPool[i];
                        pool.Add(packet);
                        packet.Peek(0);
                    }
                }
            }
        }

        private Packet MallocImpl(int index) {
            if (index >= 0 && index < _packetPool.Length) {
                List<Packet> pool = _packetPool[index];
                if (pool.Count > 0) {
                    Packet packet = pool[0];
                    pool.RemoveAt(0);
                    return packet;
                }
                else {
                    return new Packet((int)Math.Pow(2, index));
                }
            }
            return null;
        }

        public Queue<Packet> GetPackets() {
            return _packetQueue; 
        }

        public void CleanPackets() {
            var it = _packetQueue.GetEnumerator();
            while (it.MoveNext()) {
                Free(it.Current);
            }
            _packetQueue.Clear();
        }

        public int Analyze(byte[] bytes) {
            if (null == _binaryReader) {
                _binaryReader = new BinaryReader(new MemoryStream(_data));
            }
            Array.Copy(bytes, _data, bytes.Length);
            _binaryReader.BaseStream.Position = 0;

            while (_binaryReader.BaseStream.Position < bytes.Length) {
                if (null == _readingPacket) {
                    // 读取标识符，非该标识符的包说明包体无法解析，直接退出
                    int packetMark = _binaryReader.ReadInt32();
                    if (packetMark != Packet.PACKET_MARK)
                        break;

                    // 读取包长
                    int length = _binaryReader.ReadInt32();
                    if (0 >= length)
                        continue;

                    _readingPacket = Malloc(length);
                    if (null == _readingPacket) {
                        UnityEngine.Debug.LogError("Malloc Packet Failed By Length " + length);   
                        break;  
                    }
                        
                    int remainLength = (int)(bytes.Length - _binaryReader.BaseStream.Position);
                    if (remainLength < length) {
                        _readingPacket.WriteBytes(_binaryReader.ReadBytes(remainLength));
                    }
                    else {
                        _readingPacket.WriteBytes(_binaryReader.ReadBytes(length));
                        _packetQueue.Enqueue(_readingPacket);
                        _readingPacket.Peek(0);
                        _readingPacket = null;
                    }
                }
                else {
                    int needRemain = _readingPacket.Size - (int)_readingPacket.GetReadIndex();
                    int remainLength = (int)(bytes.Length - _binaryReader.BaseStream.Position);
                    if (needRemain < remainLength) {
                        _readingPacket.WriteBytes(_binaryReader.ReadBytes(needRemain));
                        _packetQueue.Enqueue(_readingPacket);
                        _readingPacket.Peek(0);
                        _readingPacket = null;
                    }
                    else {
                        _readingPacket.WriteBytes(_binaryReader.ReadBytes(remainLength));
                    }
                }
            }
            return _packetQueue.Count;
        }
    }
}
