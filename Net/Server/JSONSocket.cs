using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SimpleJSON.Net.Server {
    public class JSONSocket {
        int ClientId;
        public int clientId { get => ClientId; }
        static int ClientIdCounter = 0;
        Socket Socket;
        public Socket socket { get => Socket; }
        JSONNode data;
        public JSONNode this[string index] {
            get => data[index];
            set => data[index] = value;
        }
        int StrikeCounter;
        JSONServer Parent;
        byte[] Buffer;

        public JSONSocket(Socket socket, JSONServer parent) {
            Socket = socket;
            ClientId = ClientIdCounter++;
            data = new JSONObject();
            StrikeCounter = 3;
            Parent = parent;
            Buffer = new byte[parent.bufferSize];
            SocketEvents = new Dictionary<string, EventHandler>();
            
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnRecieve, socket);
        }

        // Socket Stuff
        void OnRecieve(IAsyncResult AR) {
            int _ammtOfBytesRecieved;
            try { _ammtOfBytesRecieved = socket.EndReceive(AR); }
            catch (SocketException) { Parent.Close(this); return; }
            if (_ammtOfBytesRecieved == 0) { Parent.Close(this); return; }
            
            byte[] _buffer = new byte[_ammtOfBytesRecieved];
            Array.Copy(Buffer, _buffer, _ammtOfBytesRecieved);
            string _compiledPacket = Encoding.UTF8.GetString(_buffer);

            // Try to make a packet
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
            Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnRecieve, Socket);
        }

        Dictionary<string, EventHandler> SocketEvents;
        public delegate void EventHandler(JSONNode packet);
        public void on(string eventName, EventHandler eventHandler) {
            if (SocketEvents == null) throw new InvalidOperationException("This instance of JSONSocket is not initilized!");
            if (SocketEvents.ContainsKey(eventName))
                SocketEvents[eventName] = eventHandler;
            else SocketEvents.Add(eventName, eventHandler);
        }

        public void emit(string eventName) {
            if (eventName == null || eventName.Length == 0) throw new ArgumentNullException();
            Socket.Send(Encoding.UTF8.GetBytes(eventName));
        }

        public void emit(string eventName, JSONNode packet) {
            if (eventName == null || eventName.Length == 0) throw new ArgumentNullException();
            if (packet == null || packet == new JSONObject()) {
                Socket.Send(Encoding.UTF8.GetBytes(eventName));
            }
            else {
                // Create the packet
                JSONNode _netPacket = new JSONObject();
                _netPacket[eventName] = packet;
                Socket.Send(Encoding.UTF8.GetBytes(_netPacket.ToString()));
            }
        }

        public bool eventExists(string eventName) { return SocketEvents.ContainsKey(eventName); }

        public void removeEvent(string eventName) {
            if (eventExists(eventName)) SocketEvents.Remove(eventName);
        }

        public void Strike() {
            if (StrikeCounter-- == 0) Disconnect();
        }

        public void Disconnect()
        {
            Parent.Close(this);
        }
    }
}