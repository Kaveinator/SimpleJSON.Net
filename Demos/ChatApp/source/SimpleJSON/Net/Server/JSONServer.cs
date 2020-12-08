using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleJSON.Net.Server {
    public class JSONServer {
        public static int GetRandomUnusedPort()
        {
            TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _listener.Stop();
            return port;
        }

        Socket ServerSocket;
        int BufferSize;
        public int bufferSize { get => BufferSize; }
        public List<JSONSocket> users;
        public delegate void EventTrigger(JSONSocket client);
        EventTrigger OnConnectionEvent;
        EventTrigger OnDisconnectEvent;

        public JSONServer(int port, int bufferSize, EventTrigger onConnection, EventTrigger onDisconnect) {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            BufferSize = bufferSize;
            users = new List<JSONSocket>();
            BufferSize = bufferSize;

            OnConnectionEvent = onConnection;
            OnDisconnectEvent = onDisconnect;
            try {
                ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch { throw new SocketException((int)SocketError.AddressAlreadyInUse); }
            ServerSocket.Listen(5);
            ServerSocket.BeginAccept(OnNewConnection, null);
        }

        public void broadcast(string eventName)
        {
            foreach (JSONSocket user in users) user.emit(eventName);
        }

        public void broadcast(string eventName, JSONNode packet)
        {
            foreach (JSONSocket user in users) user.emit(eventName, packet);
        }

        void OnNewConnection(IAsyncResult AR) {
            Socket _client;
            try { _client = ServerSocket.EndAccept(AR); }
            catch { ServerSocket.BeginAccept(OnNewConnection, null); return; }

            JSONSocket _jsonSocket = new JSONSocket(_client, this);
            users.Add(_jsonSocket);
            OnConnectionEvent(_jsonSocket);
            ServerSocket.BeginAccept(OnNewConnection, null);
        }

        public void Close(JSONSocket user) {
            if (users.Contains(user)) {
                OnDisconnectEvent(user);
                users.Remove(user);
            }
            user.socket.Shutdown(SocketShutdown.Both);
            user.socket.Close();
        }

        public void Close(Socket socket) {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}