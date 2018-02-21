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
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

			bool connected = false;

			while (connected == false) 
			{
                //byte[] receiveBuffer = new byte[4096];
                try 
				{
					s.Connect (ipLocal);
					connected = true;
				} 
				catch (Exception) 
				{
					Thread.Sleep (1000);
				}
			}

            int ID = 0;


            //Socket ServerResult = s.Accept();
            while (true)
            {
                String ClientText = Console.ReadLine();
                String Msg = ID.ToString() + ClientText;//  " testing, testing, 1,2,3";
                ID++;
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(ClientText);


                
                try
                {
                    Console.WriteLine("Writing to server: " + ClientText);
                    int bytesSent = s.Send(buffer);


                    buffer = new byte[4096];
                    int reciever = s.Receive(buffer);
                    //s.Receive(buffer);
                    if (reciever > 0)
                    {
                        String userCmd = encoder.GetString(buffer, 0, reciever);
                        Console.WriteLine(userCmd);
                    }



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
