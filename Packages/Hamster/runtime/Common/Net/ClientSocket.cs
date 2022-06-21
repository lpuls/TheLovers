using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Hamster {

    public class ClientSocket {
        private const int BUFFER_LENGTH = 4096;

        private Socket _socket = null;
        private byte[] _receivebuffer = new byte[BUFFER_LENGTH];
        private PacketManager _packetManager = null;

        public Action OnConnectSuccess;
        public Action<Packet> OnReceiveMessageCompleted;
        public Action<Packet, SocketError> OnSendMessageFailed;

        public ClientSocket(PacketManager packetManager) {
            _packetManager = packetManager;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port) {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            _socket.BeginConnect(endPoint, OnConnectedCompleted, this);

        }

        private void OnConnectedCompleted(IAsyncResult asyncResult) {
            if (!_socket.Connected)
                return;

            _socket.EndConnect(asyncResult);
            OnConnectSuccess?.Invoke();
            StartReceive();
        }

        private void StartReceive() {
            if (!_socket.Connected)
                return;
            _socket.BeginReceive(_receivebuffer, 0, BUFFER_LENGTH, SocketFlags.None, OnReceiveFrameLengthComplete, _receivebuffer);
        }

        private void OnReceiveFrameLengthComplete(IAsyncResult asyncResult) {
            if (null != _socket && !_socket.Connected)
                return;

            _socket.EndReceive(asyncResult);
            byte[] data = (byte[])asyncResult.AsyncState;
            if (0 < _packetManager.Analyze(data)) {
                Queue<Packet> packets = _packetManager.GetPackets();
                var it = packets.GetEnumerator();
                while (it.MoveNext()) {
                    Packet p = it.Current;
                    OnReceiveMessageCompleted?.Invoke(p);
                }
                _packetManager.CleanPackets();
            }
            Array.Clear(data, 0, data.Length);
            StartReceive();
        }

        public void SendMessage(Packet packet) {
            byte[] message = packet.TryToBytes();
            _socket.BeginSend(message, 0, packet.Size + Packet.HEAD_LENGTH, SocketFlags.None, OnSendMessageComplete, packet);
        }

        private void OnSendMessageComplete(IAsyncResult asyncResult) {
            _socket.EndSend(asyncResult, out SocketError socketError);
            
            Packet packet = asyncResult.AsyncState as Packet;
            if (socketError != SocketError.Success) {
                OnSendMessageFailed?.Invoke(packet, socketError);
                _socket.Disconnect(false);
                throw new SocketException((int)socketError);
            }
            _packetManager.Free(packet);
        }

        public void Close() {
            _socket.Close();  
        }

        public void Diconnect(bool reuseSocket) {
            _socket.Disconnect(reuseSocket);
        }

        public bool IsConnect() {
            return _socket.Connected; 
        }
    }
}
