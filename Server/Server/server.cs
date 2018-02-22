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

        static bool quit = false;
        static LinkedList<String> incommingMessages = new LinkedList<string>();
        static LinkedList<String> outgoingMessages = new LinkedList<string>();

        static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>();
        static int clientID = 1;

        static List<Player> PlayerList = new List<Player>(); // Player List
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

            while (quit == false)
            {
                var newClientSocket = s.Accept();

                var myThread = new Thread(clientReceiveThread);
                myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));

                ID++;

                lock (clientDictionary)
                {

                    String clientName = "client" + clientID;
                    clientDictionary.Add(clientName, newClientSocket);
                    Console.WriteLine(clientName + ": Connected");
                    var player = new Player
                    {
                        dungeonRef = dungeon
                    };
                    player.Init();
                    PlayerList.Add(player);
                    Thread.Sleep(500);
                    clientID++;
                }
            }
        }

        static Socket GetSocketFromName(String name)
        {
            lock (clientDictionary)
            {
                return clientDictionary[name];
            }
        } // probbably need this one

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

            while ((quit == false) && (socketLost == false))
            {
                byte[] buffer = new byte[4094];

                try
                {
                    int result = receiveInfo.socket.Receive(buffer);

                    if (result > 0)
                    {
                        ASCIIEncoding encoder = new ASCIIEncoding();

                        lock (incommingMessages)
                        {
                            incommingMessages.AddLast(receiveInfo.ID + ":" + encoder.GetString(buffer, 0, result));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    socketLost = true;
                }
            }
        }


        static void Main(string[] args)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            dungeon.Init();

            string ipAdress = "127.0.0.1";
            int port = 8221;

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(ipAdress), port);

            serverSocket.Bind(ipLocal);
            serverSocket.Listen(4);

            Console.WriteLine("Waiting for client ...");

            var myThread = new Thread(acceptClientThread);
            myThread.Start(serverSocket);



            //Socket newConnection = serverSocket.Accept();
            //var dungeonResult = (dungeon.SendInfo());
            //byte[] sendBuffer = encoder.GetBytes(dungeonResult); // Send Dungeon back to client
            byte[] buffer = new byte[4096];

            while (true)
            {
                String labelToPrint = "";
                lock (incommingMessages)
                {
                    if (incommingMessages.First != null)
                    {
                        labelToPrint = incommingMessages.First.Value;

                        incommingMessages.RemoveFirst();

                    }
                }

                String messageToSend = "";
                lock (outgoingMessages)
                {
                    if (outgoingMessages.First != null)
                    {
                        messageToSend = outgoingMessages.First.Value;

                        outgoingMessages.RemoveFirst();

                    }
                }

                if (messageToSend != "")
                {
                    Console.WriteLine("sending message");
                    String[] substrings = messageToSend.Split(':');
                    string theClient = substrings[0];
                    string dungeonResult = substrings[1];

                    byte[] sendBuffer = encoder.GetBytes(dungeonResult); // Send result back to client

                    String dung = dungeonResult.Substring(0, 6);
                    if (dung == "Player")
                    {
                        Console.WriteLine("\nChat sent to all clients!\n");
                        foreach (KeyValuePair<String, Socket> test in clientDictionary)
                        {
                            int bytesSent = test.Value.Send(sendBuffer);
                        }
                    }
                    else
                    {
                        int bytesSent = GetSocketFromName(theClient).Send(sendBuffer);
                    }
                }

                if (labelToPrint != "")
                {
                    Console.WriteLine(labelToPrint);
                    String[] substrings = labelToPrint.Split(':');

                    int PlayerID = Int32.Parse(substrings[0]) - 1;
                    var dungeonResult = dungeon.Process(substrings[1], PlayerList[PlayerID], PlayerID);
                    String theClient = "client" + substrings[0];
                    Console.WriteLine(dungeonResult);
                    lock (outgoingMessages)
                    {
                        outgoingMessages.AddLast(theClient + ":" + dungeonResult);
                    }
                }

                Thread.Sleep(1);

                lock (clientDictionary)
                {
                    foreach (KeyValuePair<String, Socket> test in clientDictionary)    //new
                    {
                        //Console.WriteLine(test);
                    }
                }
            }
        }
    }
}
