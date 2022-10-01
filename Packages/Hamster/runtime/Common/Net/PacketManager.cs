using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hamster {
    public class PacketManager : IPacketMallocer {
        private const int PACKET_MANAGER_PACKET_SIZE = 10;

        private List<Packet>[] _packetPool = null;

        private object _lock = new object();
        private Queue<Packet> _packetQueue = null;  // new Queue<Packet>();
        private Queue<Packet> _packetQueueFront = new Queue<Packet>();
        private Queue<Packet> _packetQueueBack = new Queue<Packet>();
        
        private Packet _readingPacket = null;
        private byte[] _data = new byte[4096];
        private BinaryReader _binaryReader = null;

        public PacketManager() {
            _packetPool = new List<Packet>[PACKET_MANAGER_PACKET_SIZE];
            for (int i = 0; i < _packetPool.Length; i++) {
                _packetPool[i] = new List<Packet>();
            }
            _packetQueue = _packetQueueFront;
        }


        public Packet Malloc(int size) {
            int realSize = 1;
            for (int i = 0; i < _packetPool.Length; i++) {
                if (realSize >= size) {
                    return MallocImpl(i);
                }
                realSize <<= 1;
            }
            return null;
        }

        public void Free(Packet packet) {
            int size = packet.MaxSize;
            int realSize = 1;
            for (int i = 0; i < _packetPool.Length; i++) {
                if (realSize >= size) {
                    List<Packet> pool = _packetPool[i];
                    pool.Add(packet);
                    packet.Clean();
                    break;
                }
                realSize <<= 1;
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
            Queue<Packet> temp = _packetQueue;
            lock (_lock) {
                if (_packetQueue == _packetQueueFront) {
                    _packetQueue = _packetQueueBack;
                }
                else {
                    _packetQueue = _packetQueueFront;
                }
            }
            return temp; 
        }

        public void CleanPackets(Queue<Packet> queue) {
            // todo 感觉这么写不太合理
            lock (_lock) {
                var it = queue.GetEnumerator();
                while (it.MoveNext()) {
                    Free(it.Current);
                }
                queue.Clear();
            }
        }

        public int Analyze(byte[] bytes) {
            if (null == _binaryReader) {
                _binaryReader = new BinaryReader(new MemoryStream(_data));
            }
            Array.Copy(bytes, _data, bytes.Length);
            _binaryReader.BaseStream.Position = 0;

            lock (_lock) {
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
                        int needRemain = _readingPacket.MaxSize - (int)_readingPacket.GetLength() + Packet.HEAD_LENGTH;
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
}
