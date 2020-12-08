using System;
using System.Threading.Tasks;
using SimpleJSON;
using SimpleJSON.Net.Server;
using SimpleJSON.Net.Client;

namespace ChatApp
{
    class Program
    {
        const int port = 5000;
        const int bufferSize = 512;
        static JSONServer server;
        static JSONClient client;
        static void Main(string[] args)
        {
            Console.Title = "Simple Chat";
            System.Console.WriteLine("Press 'S' to be server, and any other key for client: ");
            char _type = Console.ReadKey().KeyChar;
            if (_type == 'S' || _type == 's') {
                System.Console.WriteLine("\nRunning as server");
                server = new JSONServer(port, bufferSize, (JSONSocket user) => {
                        System.Console.WriteLine("New Connection");
                        user.on("setName", (JSONNode name) => {
                        user["name"] = name;
                        System.Console.WriteLine("User '" + name + "' joined");
                        server.broadcast("join", name);
                        if (!user.eventExists("message")) {
                            user.on("message", (JSONNode message) => {
                                Console.WriteLine("[" + user["name"] + "]\t" + message);
                                JSONNode _msg = new JSONObject();
                                _msg["sender"] = user["name"];
                                _msg["contents"] = message;
                                server.broadcast("message", _msg);
                            });
                        }
                    });
                },
                (JSONSocket user) => {
                    System.Console.WriteLine("User '" + user["name"] + "' disconnected");
                    server.broadcast("leave", user["name"]);
                });
                while (true) {
                    JSONNode _msg = new JSONObject();
                    _msg["sender"] = "SERVER";
                    _msg["contents"] = Console.ReadLine();
                    server.broadcast("message", _msg);
                    Console.WriteLine("[SERVER]\t" + _msg["contents"]);
                }
            }
            else {
                System.Console.WriteLine("\nRunning as client");
                client = new JSONClient("localhost", port, bufferSize, 5000, () => {
                    Console.Write("Connected to server!\n Enter username: ");
                    client.emit("setName", Console.ReadLine());
                    client.on("message", (JSONNode packet) => 
                        Console.WriteLine("[" + packet["sender"] + "]\t" + packet["contents"]));
                    client.on("join", (JSONNode user) => Console.WriteLine("'" + user + "' joined"));
                    client.on("leave", (JSONNode user) => Console.WriteLine("'" + user + "' left"));
                    while (client.Connected)
                        client.emit("message", Console.ReadLine());
                }, () => Console.WriteLine("Disconnected from server"));
                while (true) Task.Delay(100).Wait();
            }
        }
    }
}
