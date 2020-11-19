using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

using Logger;

namespace SimpleJSON.Net.Client {
    public class JSONClient {
        TcpClient Socket;
        NetworkStream NetworkStream;
        public bool Connected { get { return (Socket != null ? Socket.Connected : false); } }
        byte[] Buffer;

        // For callbacks
        public delegate void EventTrigger();
        public delegate void EventHandler(JSONNode packet);
        EventTrigger OnConnectEvent;
        EventTrigger OnDisconnectEvent;

        public JSONClient(string server, int port, int bufferSize, int timeout, EventTrigger onConnect, EventTrigger onDisconnect) {
            Socket = new TcpClient{
                ReceiveBufferSize = bufferSize,
                SendBufferSize = bufferSize,
                ReceiveTimeout = timeout,
                SendTimeout = timeout
            };
            OnConnectEvent = onConnect;
            OnDisconnectEvent = onDisconnect;
            Buffer = new byte[bufferSize];
            SocketEvents = new Dictionary<string, EventHandler>();

            Socket.BeginConnect(server, port, OnSocketConnect, null);
        }
        
        void OnSocketConnect(IAsyncResult AR) {
            Socket.EndConnect(AR);
            NetworkStream = Socket.GetStream();
            NetworkStream.BeginRead(Buffer, 0, Buffer.Length, OnSocketReceive, null);
            OnConnectEvent();
        }

        void OnSocketReceive(IAsyncResult AR) {
            byte[] _recivedData = new byte[NetworkStream.EndRead(AR)];
            if (_recivedData.Length == 0) { Disconnect(); return; }
            Array.Copy(Buffer, _recivedData, _recivedData.Length);
            string _compiledPacket = Encoding.UTF8.GetString(_recivedData);
            try {
                JSONNode _serializedPacket = JSONObject.Parse(_compiledPacket);
                JSONNode.KeyEnumerator _eventQueue = _serializedPacket.Keys;
                while (_eventQueue.MoveNext())
                    if (SocketEvents.ContainsKey(_eventQueue.Current))
                        SocketEvents[_eventQueue.Current](_serializedPacket[(string)(_eventQueue.Current)]);
            }
            catch {
                if (SocketEvents.ContainsKey(_compiledPacket))
                    SocketEvents[_compiledPacket](null);
            }
            NetworkStream.BeginRead(Buffer, 0, Buffer.Length, OnSocketReceive, null);
        }

        Dictionary<string, EventHandler> SocketEvents;
        public void on(string eventName, EventHandler eventHandler) {
            if (SocketEvents == null) throw new InvalidOperationException("This instance of JSONSocket is not initilized!");
            if (SocketEvents.ContainsKey(eventName))
                SocketEvents[eventName] = eventHandler;
            else SocketEvents.Add(eventName, eventHandler);
        }

        public void emit(string eventName) {
            if (eventName == null || eventName.Length == 0) throw new ArgumentNullException();
            byte[] _packet = Encoding.UTF8.GetBytes(eventName);
            NetworkStream.Write(_packet, 0, _packet.Length);
        }

        public void emit(string eventName, JSONNode packet) {
            if (eventName == null || eventName.Length == 0) throw new ArgumentNullException();
            if (packet == null || packet == new JSONObject()) {
                byte[] _packet = Encoding.UTF8.GetBytes(eventName);
                NetworkStream.Write(_packet, 0, _packet.Length);
            }
            else {
                // Create the packet
                JSONNode _netPacket = new JSONObject();
                _netPacket[eventName] = packet;
                byte[] _packet = Encoding.UTF8.GetBytes(_netPacket.ToString());
                NetworkStream.Write(_packet, 0, _packet.Length);
            }
        }

        public bool eventExists(string eventName) { return SocketEvents.ContainsKey(eventName); }

        public void removeEvent(string eventName) {
            if (eventExists(eventName)) SocketEvents.Remove(eventName);
        }

        public void Disconnect() {
            if (Connected) {
                NetworkStream.Close();
                Socket.Close();
                OnDisconnectEvent();
            }
        }
    }
}