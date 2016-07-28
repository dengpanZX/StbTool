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
    class UpgradeSocket
    {
        private Socket upgradeClient;
        private MainForm mainFrom;
        private string ipAddress;
        private string upgradePath;
        private int port;
        private bool stopUpgrade = false;
        public UpgradeSocket(MainForm mainFrom)
        {
            this.mainFrom = mainFrom;
        }

        public void startUpgrade(string ipAddress, int port, string upgradePath)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.upgradePath = upgradePath;
            Thread upgradeThread = new Thread(startUpgradeSocket);
            upgradeThread.Start();
        }

        public void stopThread()
        {
            stopUpgrade = true;
        }

        public void startUpgradeSocket()
        {
            FileInfo EzoneFile = new FileInfo(upgradePath);
            FileStream EzoneStream = EzoneFile.OpenRead();
            int PacketCount = 100;
            int PacketSize = (int)(EzoneStream.Length / PacketCount);
            int LastDataPacket = (int)(EzoneStream.Length - ((long)(PacketSize * PacketCount)));
            upgradeClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            try
            {
                upgradeClient.Connect(ie);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            byte[] data = new byte[PacketSize];
            for (int i = 0; i < PacketCount; i++)
            {
                if (stopUpgrade)
                    break;
                EzoneStream.Read(data, 0, data.Length);
                SendVarData(upgradeClient, data);
                mainFrom.upgrade_result(i + 1);
            }
            if (LastDataPacket != 0)
            {
                data = new byte[LastDataPacket];
                EzoneStream.Read(data, 0, data.Length);
                SendVarData(upgradeClient, data);
            }
            upgradeClient.Close();
            EzoneStream.Close();
        }

        public int SendVarData(Socket s, byte[] data) // return integer indicate how many data sent.
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            while (total < size)
            {
                if (stopUpgrade)
                    break;
                try
                {
                    sent = s.Send(data, total, dataleft, SocketFlags.None);
                    total += sent;
                    dataleft -= sent;
                }
                catch(Exception e) //避免盒子主动断开连接后出现异常
                {
                    Console.WriteLine(e.ToString());
                    stopUpgrade = true;
                    mainFrom.disconnect();
                    break;
                }
            }
            return total;
        }
    }
}
