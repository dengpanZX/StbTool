﻿using System;
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
        private static ManualResetEvent TimeoutObject; //在连接后线程阻塞等待连接结果
        private static ManualResetEvent GetMessageObject; //阻塞心跳线程
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
            if (getdata.Contains("503"))
            {
                stopConnect();
                return "503initialize^connection"; //连续5次连接失败
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

        //创建心跳线程，处理盒子端主动断开的情景
        private void createHeartBit()
        {
            byte[] data = new byte[1024];
            int recv = 0;
            while (IsConnectionSuccessful)
            {
                GetMessageObject.WaitOne(); //发送数据请求过程阻塞心跳线程
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

        //发送数据请求
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
                    GetMessageObject.Set();
                    break;
                }
                string getdata = Encoding.UTF8.GetString(data, 0, recv);
                //Console.WriteLine(getdata + "<<<" + model.getName());
                if (getdata.Contains("200read"))
                {
                    string value = getdata.Substring(9 + model.getName().Length);
                    if (value.Equals("null"))
                        model.setValue("");
                    else
                        model.setValue(value);
                    Console.WriteLine(value);
                    tmpList.Add(model); //将接收的数据保存在队列
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
                    DataModel.table1List = tmpList;
                }
                else if (mListIndex == 2)
                {
                    DataModel.table2List = tmpList;
                }
                mainForm.updateUI(tmpList, mListIndex);
            }
            GetMessageObject.Set(); //放开心跳线程的阻塞
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
            GetMessageObject.Reset();
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + ioctlMessage));
            recv = client.Receive(data);
            string getdata = Encoding.UTF8.GetString(data, 0, recv);
            Console.WriteLine(getdata + "<<<" + getdata);
            ioctlResult(getdata);
            GetMessageObject.Set();
        }

        //单独处理网络状态修改
        private void sendNetworkMessage()
        {
            byte[] data = new byte[1024];
            GetMessageObject.Reset();
            client.Send(Encoding.ASCII.GetBytes(mTcpHead + ioctlMessage));
            Thread.Sleep(1000);
            mainForm.disconnect();
        }
        //处理ioctl消息的结果
        private void ioctlResult(string getdata)
        {
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
            else
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
        }
    }
}
