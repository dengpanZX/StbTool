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
        private Socket client;
        private string mTcpHead;  //TCP请求的头部
        private static ManualResetEvent TimeoutObject;
        private bool IsConnectionSuccessful;
        private MainForm mainForm;

        public SocketHandler(MainForm mainForm){
            this.mainForm = mainForm;
        }
        public void stopConnect()
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(true);
                IsConnectionSuccessful = false;
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        public string startConnectStb(string name, string password, string ipAddress)
        {
            byte[] data = new byte[1024];
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse("114.1.3.71"), port);
            TimeoutObject = new ManualResetEvent(false);
            try
            {
               // client.Connect(ie);    
                client.BeginConnect(ie, ConnectCallBack, client);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            if (!TimeoutObject.WaitOne(5000, false)) 
            {
                if (!IsConnectionSuccessful)
                {
                    stopConnect();
                    return "100initialize^connection"; //超时连接
                }
            }  
            int recv;  
            string msg = createMd5("huawei28780808");
            string sessionID = msg.Substring(0,16).ToLower();
            string indefycode = createMd5(sessionID + "huawei").Substring(0,8);
            client.Send(Encoding.ASCII.GetBytes(indefycode + sessionID + "initialize^connection^null"));
            recv = client.Receive(data);
            string getdata = Encoding.UTF8.GetString(data, 0, recv);
            if (getdata == String.Empty)
            {
                stopConnect();
                return "400initialize^connection"; //连接被拒绝
            }
            Console.WriteLine(getdata);         
            string key = getdata.Substring(0, 16);
            indefycode = createMd5(key + "huawei").Substring(0, 8);
            mTcpHead = indefycode + key;
            if (getdata.Contains("501"))
            {
                stopConnect();
                return "501initialize^connection"; //用户名和密码错误
            }
            if (getdata.Contains("200"))
            {
                Thread t = new Thread(createHeartBit);
                t.Start();
                Thread msgt = new Thread(getMessage);
                msgt.Start();
            }
            return getdata.Substring(16);
        }

        private string createMd5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] b = Encoding.Default.GetBytes(str);
            string result = BitConverter.ToString(md5.ComputeHash(b));
            return result.Replace("-", "");
        }

        //socket连接的回调方法
        private void ConnectCallBack(IAsyncResult asyncresult)
        {
            IsConnectionSuccessful = false;
            try
            {
                if (client.Connected)
                    IsConnectionSuccessful = true;
            }
            catch (SocketException e)
            {
                IsConnectionSuccessful = false;
                Console.WriteLine(e.NativeErrorCode);
            }
            finally
            {
                TimeoutObject.Set();
            }
        }

        public void sendRebootCmd()
        {
            byte[] data = new byte[1024];
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + "read^SoftwareVersion^null"));
        }

        private void createHeartBit()
        {
            bool isRunning = true;
            while (isRunning && IsConnectionSuccessful)
            {
                try
                {
                    client.Send(Encoding.ASCII.GetBytes(mTcpHead + "heartbit^null"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    isRunning = false;
                    mainForm.disconnect();
                }
                Thread.Sleep(1000);
            }
        }

        //获取数据
        private void getMessage()
        {
            while (IsConnectionSuccessful)
            {
                byte[] data = new byte[1024];
                int recv = 0;
                if (client.Connected)
                {
                    try
                    {
                        recv = client.Receive(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);;
                    }
                }                    
                string getdata = Encoding.UTF8.GetString(data, 0, recv);
                if (getdata.Contains("200"))
                    Console.WriteLine(getdata);
            }
        }
    }
}
