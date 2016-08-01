using System;
using System.Collections.Generic;
using System.Text; 
using System.Net; 
using System.Net.Sockets; 
using System.Threading;
using System.Security;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace StbTool
{
    class SocketHandler
    {
        private const int port = 9003;
        public static Socket client;
        private string mTcpHead;  //TCP请求的头部
        private static ManualResetEvent TimeoutObject; //在连接后线程阻塞等待连接结果
        private bool IsConnectionSuccessful; //socket连接状态
        private MainForm mainForm;
        private List<DataModel> mList; //发送数据的队列
        private string mOperate; //操作状态  “read"or"write"
        private int mListIndex; //判断是tabPage1 or tablePage2的数据请求

        public SocketHandler(MainForm mainForm){
            this.mainForm = mainForm;
        }
        public void stopConnect()
        {
            try
            {
                playInfoRunning = false;
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
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            //IPEndPoint ie = new IPEndPoint(IPAddress.Parse("114.1.3.238"), port);
            TimeoutObject = new ManualResetEvent(false);
            try
            {
               // client.Connect(ie);    
                client.BeginConnect(ie, ConnectCallBack, client);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return "";
            }
            //线程阻塞10秒判断是否可以连接的地址
            TimeoutObject.WaitOne(10000, false);
            if (!IsConnectionSuccessful)
            {
                stopConnect();
                return "100initialize^connection"; //超时连接
            }          
            int recv;  
            string msg = createMd5(name + password);
            //string msg = createMd5("root.Yx684");
            string sessionID = msg.Substring(0,16).ToLower();
            string indefycode = createMd5(sessionID + "huawei").Substring(0,8);
            client.Send(Encoding.ASCII.GetBytes(indefycode + sessionID + "initialize^connection^null"));
            try
            {
                recv = client.Receive(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return "";
            }
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
            if (getdata.Contains("503"))
            {
                stopConnect();
                return "503initialize^connection"; //连续5次连接失败
            }
            if (getdata.Contains("200"))
            {
                initSendData(DataModel.table1List, 1, "read");
                sendMessage();
            }
            return getdata.Substring(16);
        }

        //MD5的操作
        private string createMd5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] b = Encoding.Default.GetBytes(str);
            string result = BitConverter.ToString(md5.ComputeHash(b));
            return result.Replace("-", "");
        }

        //初始化发送的数据
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

        public void sendMessage()
        {
            Thread sendNetMsg = new Thread(sendMessageThread);
            sendNetMsg.Start();
        }
        //第一个tab和第二个tab的读写操作，发送数据请求
        public void sendMessageThread()
        {
            byte[] data = new byte[1024];
            int recv = 0;
            List<DataModel> tmpList = new List<DataModel>();
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
                        if (model.getName().Equals("connecttype"))
                            continue;
                        Console.WriteLine(mTcpHead + mOperate + "^" + model.getName() + "^null^" + model.getValue());
                        client.Send(Encoding.ASCII.GetBytes(mTcpHead + mOperate + "^" + model.getName() + "^null^" + model.getValue()));
                    }
                    recv = client.Receive(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    mainForm.disconnect();
                    break;
                }
                string getdata = Encoding.UTF8.GetString(data, 0, recv);
                Console.WriteLine(getdata + "<<<" + model.getName());
                if (getdata.Contains("200read^" + model.getName()))
                {
                    string value = getdata.Substring(9 + model.getName().Length);
                    if (value.Equals("null"))
                        model.setValue("");
                    else
                        model.setValue(value);
                    tmpList.Add(model); //将接收的数据保存在队列
                }
                else if (getdata.Contains("200write"))
                {
                    mainForm.updateResultMeg("操作成功");
                }
            }
            if (mOperate.Equals("write") && mListIndex == 2 && MainForm.netType != null)
            {
                //网络修改后单独在最后发送，避免修改网络后又数据未修改成功
               // sendIoctlMessage(mOperate + "^" + "connecttype" + "^null^" + MainForm.netType);
                ioctlMessage = mOperate + "^" + "connecttype" + "^null^" + MainForm.netType;
                Thread sendNetMsg = new Thread(sendNetworkMessage);
                sendNetMsg.Start();
                return;
            }
            if (mOperate.Equals("read"))
            {
                if (mListIndex == 1)
                {
                    if (tmpList.Count > 0 && tmpList.Count == DataModel.table1List.Count)
                    {
                        mainForm.updateResultMeg("操作成功");
                        //DataModel.table1List = tmpList;
                        mainForm.updateUI(DataModel.table1List, mListIndex);
                    }
                }
                else if (mListIndex == 2)
                {
                    if (tmpList.Count > 0 && tmpList.Count == DataModel.table2List.Count)
                    {
                        mainForm.updateResultMeg("操作成功");
                        //DataModel.table2List = tmpList;
                        mainForm.updateUI(DataModel.table2List, mListIndex);
                    }
                }
            }
        }

        string ioctlMessage = "";
        public void sendIoctlMessage(string msg)
        {
            if (msg == string.Empty)
                return;
            ioctlMessage = msg;
            Thread sendIoctl = new Thread(sendIoctlMessage);
            sendIoctl.Start();
        }

        //发送操作信息
        private void sendIoctlMessage()
        {
            int recv = 0;
            byte[] data = new byte[1024];
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + ioctlMessage));
            recv = client.Receive(data);
            string getdata = Encoding.UTF8.GetString(data, 0, recv);
            Console.WriteLine(getdata + "<<<");
            ioctlResult(getdata);
        }

        //单独处理网络状态修改
        private void sendNetworkMessage()
        {
            byte[] data = new byte[1024];
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + ioctlMessage));
            Thread.Sleep(1000);
            mainForm.disconnect();
        }
        //处理ioctl消息的结果
        private void ioctlResult(string getdata)
        {
            if (getdata.Contains("404"))
            {
                mainForm.updateButtonEnable("404", true); //文件不存在
                return;
            }
            else if (getdata.Contains("500"))
            {
                mainForm.updateButtonEnable("500", true); //无法连接sftp服务器
                return;
            }
            if (ioctlMessage.Contains("DebugInfo"))
            {
                if (getdata.Equals("200ioctl^startDebugInfo"))
                {
                    mainForm.updateButtonEnable("info", false); //将启动按钮设置不可用
                }
                else if (getdata.Equals("200ioctl^stopDebugInfo"))
                {
                    mainForm.updateButtonEnable("info", true); //将启动按钮设置可用200ioctl^UploadDebugInfo
                }
                else if (getdata.Equals("200ioctl^UploadDebugInfo"))
                {
                    mainForm.updateButtonEnable("info_upload", true); //将启动按钮设置可用
                }
                else
                {
                    mainForm.updateButtonEnable("info_started", true); //已经开启了收集信息
                }
            }
            else if (ioctlMessage.Contains("StartupInfo"))
            {
                if (getdata.Equals("200ioctl^starStartupInfo"))
                {
                    mainForm.updateButtonEnable("start", false); //将启动按钮设置不可用
                }
                else if (getdata.Equals("200ioctl^stopStartupInfo"))
                {
                    mainForm.updateButtonEnable("start", true); //将启动按钮设置可用200ioctl^UploadDebugInfo
                }
                else if (getdata.Equals("200ioctl^UploadStartupInfo"))
                {
                    mainForm.updateButtonEnable("start_upload", true); //将启动按钮设置可用
                }
            }
            else if (ioctlMessage.Contains("Screencap"))
            {
                if (getdata.Equals("200ioctl^UploadScreencap"))
                {
                    mainForm.updateButtonEnable("picture_upload", true); //将启动按钮设置可用
                }
            }
        }

        private bool playInfoRunning = false;
        //收集可视化定位信息
        public void sendPlayInfoMsg()
        {
            playInfoRunning = true;
            Thread sendPlayInfo = new Thread(sendPlayInfoThread);
            sendPlayInfo.Start();
        }

        public void stopPlayInfoMsg()
        {
            playInfoRunning = false;
        }

        //收集可视化信息的线程
        private void sendPlayInfoThread()
        {
            while (playInfoRunning)
            {
                int recv = 0;
                if (mainForm == null)
                    break;
                mainForm.updateStatus("正在进行可视化信息收集");
                byte[] data = new byte[1024];
                try
                {
                    client.Send(Encoding.ASCII.GetBytes(mTcpHead + "read^ParasListMain^null"));
                    recv = client.Receive(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    mainForm.disconnect();
                }
                string getdata = Encoding.UTF8.GetString(data, 0, recv);
                Console.WriteLine(getdata);
                resultPlayInfo(getdata);
                Thread.Sleep(3000);
            }
        }

        private void resultPlayInfo(string getdata)
        {
            StringReader stringRead = new StringReader(getdata);
            string str = stringRead.ReadLine();
            int listindex = 0;
            while (str != string.Empty && str != null)
            {
                if (str.Contains("read^ParasListMain"))
                {
                    if (!str.Contains("200"))
                    {
                        mainForm.updateResultMeg("可视化信息收集失败");
                        break;
                    }
                    string CpuUsedRate = str.Substring(22);
                    if (CpuUsedRate.Equals("unknown"))
                    {
                        mainForm.updateResultMeg("可视化信息收集失败");
                        break;
                    }
                }
                string name = DataModel.playInfo1List[listindex].getName();
                int strindex = str.IndexOf(name); //获取name在一行数据中的位置
                if (strindex < 0)
                {
                    listindex++;
                    str = stringRead.ReadLine();
                    continue;
                }
                else
                {
                    string result = str.Substring(strindex + name.Length + 1);
                    DataModel.playInfo1List[listindex].setValue(result);
                    listindex++;
                    str = stringRead.ReadLine();
                }
            }
            if (listindex == 31)
            {
                mainForm.updateResultMeg("可视化信息收集成功");
                mainForm.updatePlayInfoUI();
            }
            else
            {
                mainForm.updateResultMeg("可视化信息收集失败");
            }
        }

        //发送升级消息
        public int sendUpgradeMsg(string upgradePath, bool forceUpgrade)
        {
            int recv = 0;
            byte[] data = new byte[1024];
            FileInfo fileInfo = new FileInfo(upgradePath);
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + "inform^set_upgradelength^null^" + fileInfo.Length));
            recv = client.Receive(data);
            string result = Encoding.UTF8.GetString(data, 0, recv);
            if (forceUpgrade)
                client.Send(Encoding.ASCII.GetBytes(mTcpHead + "ioctl^upgrade^/f"));
            else
                client.Send(Encoding.ASCII.GetBytes(mTcpHead + "ioctl^upgrade^null"));
            try
            {
                recv = client.Receive(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainForm.disconnect();
            }
            string getdata = Encoding.UTF8.GetString(data, 0, recv);
            if (getdata.Contains("200ioctl^upgrade^"))
            {
                string port = getdata.Substring(17);
                Console.WriteLine(port);
                return Convert.ToInt32(port);

            }
            return 0;
        }
    }
}
