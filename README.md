# SimpleJSON.Net
This is an extension of Bunny83's SimpleJSON repository (Link: https://github.com/Bunny83/SimpleJSON), which makes TCP networking with JSON simple

# 'SimpleJSON.Net.Client' namespace
# 'JSONClient' class description: Creates a new TCP Socket made for JSON
Static methods: -none-
Constructor:
JSONClient(string server, int port, int bufferSize, int timeout, Func<void> onConnect, Func<void> onDisconnect)
  - Attempts to connect to a JSONServer
  - bufferSize defines the ammount of memory to allocate for the constuction of incomming packets (1024 recommended)
Public methods:
void on(string eventName, Func<JSONNode, void>)
  - This class will add a event listener and will wait until a packet with an name of the eventName parameter is given, then it will callback to the function passed
  - Using this function with the same eventName parameter will overwrite the callback

void emit(string eventName)
  - Will send a packet with the eventName, and will cause a callback in the server (assuming the event handler is added on the server)

void emit(string eventName, JSONNode contents)
  - Does the same thing as emit(string eventName) but has a contents parameter that will let the reciver use some JSON goodies
  - JSONNode accepts generic types such as int, bool, string, etc but it can also send more types, go to Bunny83's repository to get more supported types
    (Ex: Getting 'SimpleJSONUnity.cs' will unlock Vector2, Vector3's and etc)

bool eventExists(string eventName)
  - Does a callback with the eventName exist?

void removeEvent(string eventName)
  - Will remove a callback with the eventName
  - If no callback with the eventName exists the function will not do anything

void Disconnect()
  - Disconnects from the server, if didn't already

# 'SimpleJSON.Net.Server' namespace
# 'JSONServer' class description: Creates a new TCP Socket server made for JSON
Static methods:
int GetRandomUnusedPort()
  - Will return a random free port number (only use it when you know what your doing)
Constructor:
JSONServer(int port, int bufferSize, Func<JSONSocket, void> onConnection, Func<JSONSocket, void> onDisconnect)
  - Opens a server made for JSON clients
  - bufferSize defines the ammount of memory to allocate for the constuction of incomming packets (1024 recommended)
Public methods:
void broadcase(string eventName)
  - Sends a event to all clients connected to the server

void broadcase(string eventName, JSONNode contents)
  - Sends a event to all clients connected to the server with contents

void Close(JSONSocket user)
  - Used to close a connection of a user
  - If the 'user' instance is part of its active connections, the server will call OnDisconnect callback

void Close(Socket socket)
  - Used to close a TCP socket
  - Use only if you know what this is doing, it is used by the JSONSocket class

# 'JSONSocket' class decription: Defines a JSONClient to the server
Static methods: -none-
Constructor:
JSONSocket(Socket socket, JSONServer parent)
  - Used by the JSONServer to initilize the JSONClient instance
Public methods:
void on(string eventName, Func<JSONNode, void> callback)
  - Will add a event with eventName and a callback when the event is called

void emit(string eventName)
  - Will call an event of eventName on the client

void emit(string eventName, JSONNode contents)
  - Will call an event of eventName on the client and send the contents of it

bool eventExists(string eventName)
  - Will check if a event with the eventName exists

void removeEvent(string EventName)
  - Will remove a event with the eventName

void Strike()
  - Striked the client, three strikes and the Disconnect() will be called

void Disconnect()
  - Disconnects the client
  
Important:
  - The JSONSocket itself is a indexer, it is used as client data
    Ex: Passing `user["username"] = "Kaveman";` where 'user' is an initilized instance of JSONSocket, will set the username of the socket to "Kaveman",
     this will be available anywhere the same instance of JSONSocket is refrenced, unless the key is deleted in another line.
