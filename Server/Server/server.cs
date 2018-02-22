using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class server
    {

        static bool quit = false;
        static LinkedList<String> incommingMessages = new LinkedList<string>();

        static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>();
        static int clientID = 1;

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
                    Thread.Sleep(500);
                    var player = new Player();
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

            var dungeon = new Dungeon(); // Initialise Dungeon
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
            var dungeonResult = (dungeon.SendInfo());
            byte[] sendBuffer = encoder.GetBytes(dungeonResult); // Send Dungeon back to client
            byte[] buffer = new byte[4096];


            //if (newConnection != null)
            //{            
            //    while (true)
            //    {

            //        byte[] buffer = new byte[4096];

            //        try
            //        {
            //            int result = newConnection.Receive(buffer);

            //            if (result > 0)
            //            {

            //                foreach (KeyValuePair<String, Socket> whatisthis in clientDictionary)    //new
            //                {
            //                    Console.WriteLine(whatisthis);
            //                }

            //                String recdMsg = encoder.GetString(buffer, 0, result);
            //                Console.WriteLine("Received: " + recdMsg);

            //                dungeonResult = dungeon.Process(recdMsg);


            //                sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client

            //                bytesSent = newConnection.Send(sendBuffer);

            //            }
            //        }
            //        catch (System.Exception ex)
            //        {
            //            Console.WriteLine(ex);    	
            //        }    

            //    }
            //}

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

                if (labelToPrint != "")
                {
                    Console.WriteLine(labelToPrint);

                    Char delimiter = ':';
                    String[] substrings = labelToPrint.Split(delimiter);

                    dungeonResult = dungeon.Process(substrings[1]);
                    Console.WriteLine(dungeonResult);

                    sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client

                    int bytesSent = GetSocketFromName("client" + substrings[0]).Send(sendBuffer);
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

        //static void Main(string[] args)
        //{

        //    var dungeon = new Dungeon();
        //    dungeon.Init();

        //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

        //    s.Bind(ipLocal);
        //    s.Listen(4);            

        //    Console.WriteLine("Waiting for client ...");

        //    Socket newConnection = s.Accept();
        //    if (newConnection != null)
        //    {


        //        while (true)
        //        {
        //            byte[] buffer = new byte[4096];

        //            //dungeon.Process(" ");

        //            try
        //            {
        //                int result = newConnection.Receive(buffer);

        //                if (result > 0)
        //                {
        //                    // ASCIIEncoding encoder = new ASCIIEncoding();
        //                    //String recdMsg = encoder.GetString(buffer, 0, result);
        //                    ASCIIEncoding encoder = new ASCIIEncoding();

        //                    String userCmd = encoder.GetString(buffer, 0, result);
        //                    Console.WriteLine("Received: " + userCmd);

        //                    var dungeonResult = dungeon.Process(userCmd);
        //                    Console.WriteLine(dungeonResult);

        //                    //NetworkStream writer = new NetworkStream(newConnection.BeginSend)

        //                    byte[] sendBuffer = encoder.GetBytes(dungeonResult);

        //                    int bytesSent = newConnection.Send(sendBuffer);
        //                    Console.WriteLine("\nWriting to Client ");



        //                }
        //            }
        //            catch (System.Exception ex)
        //            {
        //                Console.WriteLine(ex);    	
        //            }

        //        }
        //    }
        //}
    }
}
