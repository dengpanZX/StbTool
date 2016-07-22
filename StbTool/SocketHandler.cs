using System;
using System.Collections.Generic;
using System.Text; 
using System.Net; 
using System.Net.Sockets; 
using System.Threading;
using System.Security;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace StbTool
{
    class SocketHandler
    {
        private const int port = 9003;
        private Socket client;
        private string mTcpHead;  //TCP请求的头部
        private static ManualResetEvent TimeoutObject;
        private static ManualResetEvent GetMessageObject;
        private bool IsConnectionSuccessful;
        private MainForm mainForm;
        private List<DataModel> mList;
        private string mOperate;
        private int mListIndex;

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
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse("114.1.3.238"), port);
            TimeoutObject = new ManualResetEvent(false);
            GetMessageObject = new ManualResetEvent(false);
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
            string msg = createMd5("huawei.287aW");
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
                initSendData(DataModel.table1List, 1, "read");
                sendMessage();
                Thread t = new Thread(createHeartBit);
                t.Start();
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

        public void initSendData(List<DataModel> list, int listIndex, string operate)
        {
            this.mListIndex = listIndex;
            this.mList = list;
            this.mOperate = operate;
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
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + "ioctl^reboot^null"));
            client.Receive(data);
            //mainForm.disconnect();
        }

        public void resetFactory()
        {
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + "ioctl^restore_setting^null"));
        }

        private void createHeartBit()
        {
            byte[] data = new byte[1024];
            int recv = 0;
            while (IsConnectionSuccessful)
            {
                GetMessageObject.WaitOne();
                try
                {
                    client.Send(Encoding.ASCII.GetBytes(mTcpHead + "heartbit^null"));
                    recv = client.Receive(data);
                    string getdata = Encoding.UTF8.GetString(data, 0, recv);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    mainForm.disconnect();
                }
                Thread.Sleep(1000);
            }

        }

        public void sendMessage()
        {
            byte[] data = new byte[1024];
            int recv = 0;
            List<DataModel> tmpList = new List<DataModel>();
            GetMessageObject.Reset();
            foreach (DataModel model in mList)
            {
                try
                {
                    if (mOperate.Equals("read"))
                    {
                        Console.WriteLine(mTcpHead + mOperate + "^" + model.getName() + "^null");
                        client.Send(Encoding.ASCII.GetBytes(mTcpHead + mOperate + "^" + model.getName() + "^null"));
                    }
                    else if (mOperate.Equals("write"))
                    {

                        Console.WriteLine(mTcpHead + mOperate + "^" + model.getName() + "^null^" + model.getValue());
                        client.Send(Encoding.ASCII.GetBytes(mTcpHead + mOperate + "^" + model.getName() + "^null^" + model.getValue()));
                    }
                    recv = client.Receive(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    GetMessageObject.Set();
                    break;
                }
                string getdata = Encoding.UTF8.GetString(data, 0, recv);
                Console.WriteLine(getdata + "<<<" + model.getName());
                if (getdata.Contains("200read"))
                {
                    string value = getdata.Substring(9 + model.getName().Length);
                    if (value.Equals("null"))
                        model.setValue("");
                    else
                        model.setValue(value);
                    Console.WriteLine(value);
                    tmpList.Add(model);
                }
            }
            if (mOperate.Equals("read"))
            {
                if (mListIndex == 1)
                {
                    DataModel.table1List = tmpList;
                }
                else if (mListIndex == 2)
                {
                    DataModel.table2List = tmpList;
                }
                mainForm.updateUI(tmpList, mListIndex);
            }
            GetMessageObject.Set();
        }
    }
}
