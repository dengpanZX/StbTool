using System;
using System.Collections.Generic;
using System.Text; 
using System.Net; 
using System.Net.Sockets; 
using System.Threading;
using System.Security;
using System.Security.Cryptography;

namespace StbTool
{
    class SocketHandler
    {
        private const int port = 9003;
        private string serverIP;
        private Socket client;
        private Thread connectThread;
        private bool isRunning = false;

        public SocketHandler()
        {
            connectThread = new Thread(startConnectStb);
        }

        public void startConnectThread(string addressIp)
        {
            serverIP = addressIp;
            isRunning = true;
            connectThread.Start();
        }

        public void stopThread()
        {
            try
            {
                isRunning = false;
                connectThread.Abort();
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        public void startConnectStb()
        {
            byte[] data = new byte[1024];
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse("10.75.240.2"), port);
            try
            {
                client.Connect(ie);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            int recv;
    
            string msg = "huawei.287aW";
            msg = createMd5(msg);
            string sessionID = msg.Substring(0,16).ToLower();
            string indefycode = createMd5(sessionID + "huawei").Substring(0,8);
            Console.WriteLine(sessionID + " >>>>" + indefycode);
            client.Send(Encoding.ASCII.GetBytes(indefycode + sessionID + "initialize^connection^null"));
            recv = client.Receive(data);
            string getdata = Encoding.UTF8.GetString(data, 0, recv);
           // string key = getdata.Substring(0, 128);
           // string str = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(getdata));
            Console.WriteLine(getdata + "   >>>>  ");
            while (isRunning)
            {
                data = new byte[1024];
                string msg1 = indefycode + sessionID + "ioctl^reboot^null";
                //Console.WriteLine(AESHelper.AESEncrypt(msg1, private_key));
                //client.Send(Encoding.ASCII.GetBytes(AESHelper.AESEncrypt(msg1, key)));
                recv = client.Receive(data);
                getdata = Encoding.ASCII.GetString(data, 0, recv);
               // Console.WriteLine("<<<<<<<<<<<<" + key);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close(); 
        }

        private string createMd5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] b = Encoding.Default.GetBytes(str);
            string result = BitConverter.ToString(md5.ComputeHash(b));
            return result.Replace("-", "");
        }
    }
}
