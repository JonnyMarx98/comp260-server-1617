using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class client
    {
        static LinkedList<String> incommingMessages = new LinkedList<string>();

        static void serverReceiveThread(Object obj)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] receiveBuffer = new byte[8192];

            Socket s = obj as Socket;

            while (true)
            {
                try
                {
                    int reciever = s.Receive(receiveBuffer);
                    s.Receive(receiveBuffer);
                    if (reciever > 0)
                    {
                        String clientMsg = encoder.GetString(receiveBuffer, 0, reciever);
                        Console.WriteLine(clientMsg);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        static void Main(string[] args)
        {

            string ipAdress = "138.68.173.44";
            int port = 8221;
            //
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(ipAdress), port);

            bool connected = false;

			while (connected == false) 
			{
                try 
				{
					s.Connect (ipLocal);
                    Console.WriteLine("Connected To Server \nWelcome To my MUD! \nType help to see commands list");

					connected = true;
				} 
				catch (Exception) 
				{
					Thread.Sleep (1000);
				}
			}

            int ID = 0;


            var myThread = new Thread(serverReceiveThread);
            myThread.Start(s);

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = new byte[4096];

            while (true)
            {
                String ClientText = Console.ReadLine();
                //String Msg = ID.ToString() + " : " +ClientText;//  " testing, testing, 1,2,3";
                ID++;
                buffer = encoder.GetBytes(ClientText);
                
                try
                {
                    Console.WriteLine("Writing to server: " + ClientText);
                    int bytesSent = s.Send(buffer);


                    //buffer = new byte[4096];
                    //int reciever = s.Receive(buffer);
                    ////s.Receive(buffer);
                    //if (reciever > 0)
                    //{
                    //    String userCmd = encoder.GetString(buffer, 0, reciever);
                    //    Console.WriteLine(userCmd);
                    //}



                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);	
                }
                

                //Thread.Sleep(1);
            }
        }
    }
}
