using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Hamster {

    public class ClientInstance : IPool {
        public const int BUFFER_LENGTH = 1024;

        public string IP;
        public int CreateIndex;
        public int UserData;
        public Socket Handle;
        protected byte[] _receiveBuff = new byte[BUFFER_LENGTH];

        public INetDevice ServerNetDevice = null;
        public Action<ClientInstance> OnReceiveMessage;
        public Action<ClientInstance, SocketError, Packet> OnSendComplete;

        private PacketManager _packetManager = new PacketManager();

        private void BeginReceiveBuffComplete(IAsyncResult asyncResult) {
            byte[] data = (byte[])asyncResult.AsyncState;
            int readSize = Handle.EndReceive(asyncResult);
            int count = _packetManager.Analyze(data);
            if (readSize > 0 && count > 0) {
                Array.Clear(_receiveBuff, 0, BUFFER_LENGTH);
                OnReceiveMessage?.Invoke(this); 
            }
            BeginReceiveBuff();
        }

        public Queue<Packet> GetReceivePackets() {
            return _packetManager.GetPackets();
        }

        public void CleanReceivePackets(Queue<Packet> queue) {
            _packetManager.CleanPackets(queue);
        }

        public void BeginReceiveBuff() {
            if (!Handle.Connected)
                return;

            Handle.BeginReceive(_receiveBuff, 0, _receiveBuff.Length,
                    SocketFlags.None, BeginReceiveBuffComplete, _receiveBuff);
        }

        public void SendMessage(NetMessage netMessage) {
            Packet packet = netMessage.ToPacket(_packetManager);
            byte[] message = packet.TryToBytes();
            Handle.BeginSend(message, 0, packet.Size + Packet.HEAD_LENGTH, SocketFlags.None, OnSendMessageComplete, packet);
        }


        private void OnSendMessageComplete(IAsyncResult asyncResult) {
            Handle.EndSend(asyncResult, out SocketError socketError);

            Packet packet = asyncResult.AsyncState as Packet;
            if (socketError != SocketError.Success) {
                // OnSendMessageFailed?.Invoke(packet, socketError);
                Handle.Disconnect(false);
                throw new SocketException((int)socketError);
            }
            OnSendComplete?.Invoke(this, socketError, packet);
            _packetManager.Free(packet);
        }

        public void Reset() {
            CreateIndex = 0;
            IP = String.Empty;
            Handle = null;
            Array.Clear(_receiveBuff, 0, BUFFER_LENGTH);
        }
    }

    public class ServerSocket {

        
        private bool _running = false;
        protected Socket _socket = null;
        protected Thread _listenThread = null;
        protected INetDevice _serverNetDevice = null;

        protected ManualResetEvent _allDone = new ManualResetEvent(false);

        protected Dictionary<string, ClientInstance> _clients = new Dictionary<string, ClientInstance>();
            
        public Action<ClientInstance> OnAcceptClient;
        public Action<ClientInstance> OnCloseClient;

        public ServerSocket(INetDevice serverNetDevice) {
            _serverNetDevice = serverNetDevice;
        }

        public void Listen(string ip, int port, int listenCount) {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _socket.Bind(endPoint);
            _socket.Listen(listenCount);

            _running = true;
            _listenThread = new Thread(ListenConnect);
            _listenThread.Start();
        }

        protected void ListenConnect() {
            while (_running) {
                _allDone.Reset();
                _socket.BeginAccept(OnAcceptComplete, _socket);
                _allDone.WaitOne();

                Thread.Sleep(1);
            }
        }

        protected void OnAcceptComplete(IAsyncResult asyncResult) {
            if (!_running)
                return;

            _allDone.Set();
            Socket server = (Socket)asyncResult.AsyncState;
            Socket handler = server.EndAccept(asyncResult);
            IPEndPoint iep = (IPEndPoint)handler.RemoteEndPoint;
            string ip = iep.Address.ToString();

            ClientInstance clientInstance = ObjectPool<ClientInstance>.Malloc();
            if (!_clients.ContainsKey(ip)) {
                clientInstance.IP = ip;
                clientInstance.Handle = handler;
                OnAcceptClient?.Invoke(clientInstance);
                _clients.Add(ip, clientInstance);
                clientInstance.BeginReceiveBuff();
            }
            else {
                UnityEngine.Debug.LogError("IP " + ip + " Reconnect");
            }
        }

        public void Disconnect(ClientInstance inst) {
            if (!_clients.ContainsKey(inst.IP))
                UnityEngine.Debug.LogError("IP " + inst.IP + " never connect");
            _clients.Remove(inst.IP);
            OnCloseClient?.Invoke(inst);

            inst.Handle.Close();
            ObjectPool<ClientInstance>.Free(inst);
        }

        public void Broakcast(NetMessage netMessage) {
            var it = _clients.GetEnumerator();
            while (it.MoveNext()) {
                it.Current.Value.SendMessage(netMessage);
            }
        }

        public bool IsListen() {
            return _running;
        }

        public void Close() {
            _listenThread.Abort();
            _socket.Close();
            _running = false;
        }

    }
}
