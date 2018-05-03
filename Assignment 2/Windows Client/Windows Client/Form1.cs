using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Part_03_FormClient
{
    public partial class Form1 : Form
    {
        Socket client;
        private Thread myThread;
        bool bQuit = false;
        bool bConnected = false;

        public Form1()
        {
            InitializeComponent();

            OnStartup();
        }
        private delegate void AddTextDelegate(String s);

        private void AddOutputText(String s)
        {
            if(IsDisposed == true)
            {
                return;
            }

            if (outputText.InvokeRequired)
            {
                try
                {
                    Invoke(new AddTextDelegate(AddOutputText), new object[] { s });
                }
                catch (Exception) { }
            }
            else
            {
                if (outputText.IsDisposed == false)
                {
                    String outString = "";
                    foreach (var ch in s)
                    {
                        if (ch != '\n')
                        {
                            outString += ch;
                        }
                        else
                        {
                            outputText.AppendText(outString);
                            outputText.AppendText(Environment.NewLine);
                            outString = "";
                        }
                    }
                    if (outString.Length > 0)
                    {
                        outputText.AppendText(outString);
                    }

                    outputText.AppendText(Environment.NewLine);
                    outputText.ScrollToCaret();
                }
            }
        }

        static void clientProcess(Object o)
        {
            Form1 form = (Form1)o;

            while ((form.bConnected == false) && (form.bQuit == false))
            {
                try
                {
                    form.client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    form.client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8222));
                    form.bConnected = true;
                    form.AddOutputText("Connected to server");

                    Thread receiveThread;

                    receiveThread = new Thread(clientReceive);
                    receiveThread.Start(o);

                    while ((form.bQuit == false) && (form.bConnected == true))
                    {
                        if (form.IsDisposed == true)
                        {
                            form.bQuit = true;
                            form.client.Close();
                        }
                    }

                    receiveThread.Abort();
                }
                catch (System.Exception)
                {
                    form.AddOutputText("No server!");
                    Thread.Sleep(1000);
                }
            }
        }

        static void clientReceive(Object o)
        {
            Form1 form = (Form1)o;
            byte[] buffer = new byte[4096];
            int result;
            ASCIIEncoding encoder = new ASCIIEncoding();            

            while (form.bConnected == true)
            {
                try
                {                    
                    result = form.client.Receive(buffer);

                    if (result > 0)
                    {
                        form.AddOutputText(encoder.GetString(buffer, 0, result));
                    }
                }
                catch (Exception)
                {
                    form.bConnected = false;
                    Console.WriteLine("Lost server!");
                }

            }
        }

        void OnStartup()
        {            
            FormBorderStyle = FormBorderStyle.FixedDialog;            
            MaximizeBox = false;            
            //MinimizeBox = false;            
            StartPosition = FormStartPosition.CenterScreen;            
            
            myThread = new Thread(clientProcess);
            myThread.Start(this);

            Application.ApplicationExit += delegate { OnExit(); };

            inputText.KeyPress += new KeyPressEventHandler(textBox_Input_KeyPress);
            
            outputText.ScrollBars = ScrollBars.Vertical;
            outputText.AcceptsTab = false;
            outputText.GotFocus += OutputText_GotFocus;
        }

        private void OutputText_GotFocus(object sender, EventArgs e)
        {
            inputText.Focus();
        }

        private void OnExit()
        {
            bQuit = true;
            Thread.Sleep(500);
            if (myThread != null)
            {
                myThread.Abort();
            }
        }


        private void textBox_Input_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //do return ....

                try
                {
                    if (inputText.Text.Length > 0)
                    {
                        ASCIIEncoding encoder = new ASCIIEncoding();
                        int bytesSent = client.Send(encoder.GetBytes(inputText.Text));

                        outputText.AppendText(">>" + inputText.Text);
                        outputText.AppendText(Environment.NewLine);
                        outputText.ScrollToCaret();

                        inputText.Text = "";
                    }
                }
                catch (System.Exception)
                {

                }

                e.Handled = true;
            }
        }

        private void outputText_TextChanged(object sender, EventArgs e)
        {

        }
    }
}