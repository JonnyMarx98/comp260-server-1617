using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class server
    {
        static LinkedList<String> inMessages = new LinkedList<string>(); // List of the incomming Messages 
        static LinkedList<String> outMessages = new LinkedList<string>(); // List of the outgoing Messages 

        static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>(); // client dictionary which stores client name and socket 

        public static List<Player> PlayerList = new List<Player>(); // List of players
        static Dungeon dungeon = new Dungeon(); // Dungeon

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
            int ID = 1;

            while (true)
            {
                var newClientSocket = s.Accept();

                var myThread = new Thread(clientReceiveThread);
                myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));

                lock (clientDictionary)
                {

                    String clientName = "client" + ID;
                    clientDictionary.Add(clientName, newClientSocket);
                    Console.WriteLine(clientName + ": Connected");
                    var player = new Player
                    {
                        dungeonRef = dungeon,
                        playerName = "Player" + ID,
                        clientID = clientName
                    };
                    player.Init();
                    PlayerList.Add(player);

                    var dungeonResult = dungeon.DungeonInfo(player, true);

                    lock (outMessages)
                    {
                        outMessages.AddLast(clientName + ":" + dungeonResult);
                    }

                    //lock (outMessages)
                    //{
                    //    foreach (KeyValuePair<String, Socket> client in clientDictionary)
                    //    {
                    //        outMessages.AddLast(client.Key + ":" + dungeonResult);
                    //    }
                    //}
                    //Thread.Sleep(500);
                    ID++;
                }
            }
        }

        static Socket GetSocketFromName(String name)
        {
            lock (clientDictionary)
            {
                return clientDictionary[name];
            }
        }

        static String GetNameFromSocket(Socket s)
        {
            lock (clientDictionary)
            {
                foreach (KeyValuePair<String, Socket> o in clientDictionary)
                {
                    if (o.Value == s)
                    {
                        return o.Key;
                    }
                }
            }
            return null;
        }

        static void clientReceiveThread(Object obj)
        {
            ReceiveThreadLaunchInfo receiveInfo = obj as ReceiveThreadLaunchInfo;
            bool socketLost = false;

            while (socketLost == false)
            {
                byte[] buffer = new byte[4094];

                try
                {
                    int result = receiveInfo.socket.Receive(buffer);

                    if (result > 0)
                    {
                        ASCIIEncoding encoder = new ASCIIEncoding();

                        lock (inMessages)
                        {
                            inMessages.AddLast(receiveInfo.ID + ":" + encoder.GetString(buffer, 0, result));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    socketLost = true;
                }
            }
        }

        static void globalChatMsg(string message)
        {
            lock (outMessages)
            {
                foreach (KeyValuePair<String, Socket> client in clientDictionary)
                {
                    outMessages.AddLast(client.Key + ":" + message);
                }
            }
            Console.WriteLine(message);
        }

        static void localChatMsg(Player currentPlayer, string message)
        {
            foreach (Player player in PlayerList)
            {
                if (currentPlayer.currentRoom == player.currentRoom)
                {
                    lock (outMessages)
                    {
                        outMessages.AddLast(player.clientID + ":" + message);
                    }
                }
            }
            Console.WriteLine(message);
        }

        public static void roomUpdate(Player player, Player newPlayer, bool enteredRoom)
        {
            lock (outMessages)
            {
                if(enteredRoom)
                {
                    outMessages.AddLast(player.clientID + ":" +
                    "[" + newPlayer.playerName + "]" + " entered the room");
                }
                else
                {
                    outMessages.AddLast(player.clientID + ":" +
                    "[" + newPlayer.playerName + "]" + " left the room");
                }
                
            }
        }


        static void Main(string[] args)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            dungeon.Init(); // Initialise Dungeon

            string ipAdress = "127.0.0.1";
            int port = 8221;

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(ipAdress), port);

            serverSocket.Bind(ipLocal);
            serverSocket.Listen(4);

            Console.WriteLine("Waiting for client ..");

            var myThread = new Thread(acceptClientThread);
            myThread.Start(serverSocket);

            byte[] buffer = new byte[4096];

            while (true)
            {
                String currentMsg = "";
                lock (inMessages)
                {
                    if (inMessages.First != null)
                    {
                        currentMsg = inMessages.First.Value;

                        inMessages.RemoveFirst();

                    }
                }

                String msgToSend = "";
                lock (outMessages)
                {
                    if (outMessages.First != null)
                    {
                        msgToSend = outMessages.First.Value;

                        outMessages.RemoveFirst();

                    }
                }

                if (msgToSend != "")
                {
                    String[] substrings = msgToSend.Split(':');
                    string theClient = substrings[0];
                    string clientMsg = substrings[1];

                    byte[] sendBuffer = encoder.GetBytes(clientMsg); // Send result back to client

                    int bytesSent = GetSocketFromName(theClient).Send(sendBuffer);

                    bytesSent = GetSocketFromName(theClient).Send(sendBuffer);
                }

                if (currentMsg != "")
                {
                    Console.WriteLine(currentMsg);
                    String[] substrings = currentMsg.Split(':');

                    int PlayerID = Int32.Parse(substrings[0]) - 1;
                    String clientMsg = substrings[1];
                    String theClient = "client" + substrings[0];
                    Player player = PlayerList[PlayerID];

                    var dungeonResult = dungeon.Process(clientMsg, player, PlayerID);

                    if (dungeonResult.Length > 7)
                    {
                        if (dungeonResult.Substring(0, 7) == "[local]")
                        {
                            localChatMsg(player, dungeonResult);
                        }
                        else if (dungeonResult.Substring(0, 8) == "[global]")
                        {
                            globalChatMsg(dungeonResult);
                        }
                        else
                        {
                            lock (outMessages)
                            {
                                outMessages.AddLast(theClient + ":" + dungeonResult);
                            }
                        }
                    }

                    else
                    {
                        Console.WriteLine(dungeonResult);
                        lock (outMessages)
                        {
                            outMessages.AddLast(theClient + ":" + dungeonResult);
                        }
                    }
                }

                Thread.Sleep(1);

                lock (clientDictionary)
                {
                    foreach (KeyValuePair<String, Socket> test in clientDictionary)
                    {
                        // not sure what the point of this is hmmm 
                    }
                }
            }
        }
    }
}
