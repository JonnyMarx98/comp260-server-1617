using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace MUDServer
{
    class Program
    {
        static bool quit = false;
        
        static ConcurrentQueue<ClientMessageBase> clientCommand = new ConcurrentQueue<ClientMessageBase>();

        class ReceiveThreadLaunchInfo
        {
            public ReceiveThreadLaunchInfo(int ID, Socket socket)
            {
                this.ID = ID;
                this.socket = socket;
            }

            public int ID;
            public Socket socket;
        }

        static void acceptClientThread(Object obj)
        {
            Socket s = obj as Socket;

            int ID = 0;

            while (quit == false)
            {
                var newClientSocket = s.Accept();

                var myThread = new Thread(clientReceiveThread);
                myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));

                ID++;

                clientCommand.Enqueue(new ClientJoined(newClientSocket));

                Console.WriteLine("Client added");
            }
        }

        static void clientReceiveThread(Object obj)
        {
            ReceiveThreadLaunchInfo receiveInfo = obj as ReceiveThreadLaunchInfo;

            ASCIIEncoding encoder = new ASCIIEncoding();
            bool socketLost = false;

            while ((quit == false) && (socketLost == false))
            {
                byte[] buffer = new byte[4096];

                try
                {
                    int result = receiveInfo.socket.Receive(buffer);

                    if (result > 0)
                    {
                        clientCommand.Enqueue(new ClientMessage(receiveInfo.socket, encoder.GetString(buffer, 0, result)) );
                    }
                }
                catch (System.Exception)
                {
                    clientCommand.Enqueue(new ClientLost(receiveInfo.socket));
                    socketLost = true;                    
                }
            }
        }


        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Console.WriteLine ("Running from: " + "'" + args [0] +"'");
			IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(args[0]), 8222);

            s.Bind(ipLocal);
            s.Listen(4);

            SUD.Dungeon dungeon = new SUD.Dungeon();
            dungeon.Init();


            Console.WriteLine("Waiting for clients ...");

            var myThread = new Thread(acceptClientThread);
            myThread.Start(s);

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] outputBuffer;

            while (true)
            {                
                ClientMessageBase command;

                if (clientCommand.TryDequeue(out command) == true)
                {                        
                    if(command is ClientJoined)
                    {
                        try
                        {
                            dungeon.SetClientInRoom(command.client, "Room 0");
                            String dungeonOutput = dungeon.RoomDescription(command.client);

                            dungeonOutput += "type 'help' for help\n";

                            try
                            {
                                command.client.Send(encoder.GetBytes(dungeonOutput));
                            }
                            catch (Exception) { }

                            Console.WriteLine("Send client welcome: " + dungeonOutput);

                            //tell all the clients that this client has entered

                            foreach(var player in dungeon.socketToRoomLookup)
                            {
                                if(player.Key != command.client)
                                {
                                    try
                                    {
                                        player.Key.Send(encoder.GetBytes("A new dungeoneer has entered\n"));
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                        catch (System.Exception)
                        {                                
                            clientCommand.Enqueue(new ClientLost(command.client));                                
                        }
                    }

                    if(command is ClientMessage)
                    {
                        var clientMessage = command as ClientMessage;                            
                            
                        String outputToUser = dungeon.RoomDescription(clientMessage.client);

                        String[] input = clientMessage.message.Split(' ');

                        switch (input[0].ToLower())
                        {
                            case "help":
                                outputToUser += "\nCommands are ....\n";
                                outputToUser += "help - for this screen\n";
                                outputToUser += "look - to look around\n";
                                outputToUser += "go [north | south | east | west]  - to travel between locations\n";
                                outputToUser += "say - talk to all the dungeoneers in the room\n";
                                outputToUser += "\n";

                                break;

                            case "look":
                                outputToUser = dungeon.RoomDescription(clientMessage.client);
                                break;

                            case "say":
                                outputToUser += "\nYou say: ";
                                for (var i = 1; i < input.Length; i++)
                                {
                                    outputToUser += input[i] + " ";
                                }

                                outputToUser += "\n";

                                {
                                    String messageToSend = "Someone says: ";

                                    for (var i = 1; i < input.Length; i++)
                                    {
                                        messageToSend += input[i] + " ";
                                    }

                                    outputBuffer = encoder.GetBytes(messageToSend);

                                    foreach (var kvp in dungeon.socketToRoomLookup)
                                    {
                                        if ((kvp.Key != clientMessage.client)
                                            && (kvp.Value == dungeon.socketToRoomLookup[clientMessage.client])
                                            )
                                        {
                                            try
                                            {
                                                kvp.Key.Send(outputBuffer);
                                            }
                                            catch (Exception)
                                            { }
                                        }
                                    }
                                }

                                break;

                            case "go":
                                // is arg[1] sensible?

                                bool newDestination = false;
                                var oldRoom = dungeon.socketToRoomLookup[clientMessage.client];                                        

                                if ((input[1].ToLower() == "north") && (dungeon.socketToRoomLookup[clientMessage.client].north != null))
                                {
                                    dungeon.socketToRoomLookup[clientMessage.client] = dungeon.roomMap[dungeon.socketToRoomLookup[clientMessage.client].north];
                                    newDestination = true;
                                }
                                else
                                {
                                    if ((input[1].ToLower() == "south") && (dungeon.socketToRoomLookup[clientMessage.client].south != null))
                                    {
                                        dungeon.socketToRoomLookup[clientMessage.client] = dungeon.roomMap[dungeon.socketToRoomLookup[clientMessage.client].south];
                                        newDestination = true;
                                    }
                                    else
                                    {
                                        if ((input[1].ToLower() == "east") && (dungeon.socketToRoomLookup[clientMessage.client].east != null))
                                        {
                                            dungeon.socketToRoomLookup[clientMessage.client] = dungeon.roomMap[dungeon.socketToRoomLookup[clientMessage.client].east];
                                            newDestination = true;
                                        }
                                        else
                                        {
                                            if ((input[1].ToLower() == "west") && (dungeon.socketToRoomLookup[clientMessage.client].west != null))
                                            {
                                                dungeon.socketToRoomLookup[clientMessage.client] = dungeon.roomMap[dungeon.socketToRoomLookup[clientMessage.client].west];
                                                newDestination = true;
                                            }
                                        }
                                    }
                                }

                                if (newDestination == false)
                                {
                                    //handle error
                                    outputToUser += "\nERROR";
                                    outputToUser += "\nCan not go " + input[1] + " from here";
                                    outputToUser += "\n";
                                }
                                else
                                {
                                    var newRoom = dungeon.socketToRoomLookup[clientMessage.client];

                                    outputToUser = dungeon.RoomDescription(clientMessage.client);

                                    foreach (var kvp in dungeon.socketToRoomLookup)
                                    {
                                        if ((kvp.Key != clientMessage.client)
                                            && (kvp.Value == oldRoom)
                                            )
                                        {
                                            try
                                            {
                                                kvp.Key.Send(encoder.GetBytes("A dungeoneer has left this room\n"));
                                            }
                                            catch (Exception)
                                            { }
                                        }

                                        if ((kvp.Key != clientMessage.client)
                                            && (kvp.Value == newRoom)
                                            )
                                        {
                                            try
                                            {
                                                kvp.Key.Send(encoder.GetBytes("A dungeoneer has entered this room\n"));
                                            }
                                            catch (Exception)
                                            { }
                                        }
                                    }
                                }

                                break;

                            default:
                                //handle error
                                outputToUser += "\nERROR";
                                outputToUser += "\nCan not " + clientMessage.message;
                                outputToUser += "\n";
                                break;
                        }

                        try
                        {
                            clientMessage.client.Send(encoder.GetBytes(outputToUser));
                            Console.WriteLine("Send client message: " + outputToUser);
                        }
                        catch (Exception) { }                                                    
                    }

                    if(command is ClientLost)
                    {
                        var clientMessage = command as ClientLost;

                        Console.WriteLine("Client Lost");

                        foreach (var player in dungeon.socketToRoomLookup)
                        {
                            if (player.Key != command.client)
                            {
                                try
                                {
                                    player.Key.Send(encoder.GetBytes("A dungeoneer has left the dungeon\n"));
                                }
                                catch(Exception)
                                {

                                }
                            }
                        }

                        dungeon.RemoveClient(clientMessage.client);
                    }
                }                       
            }
        }
    }
}