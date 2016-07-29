using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace StbTool
{
    public partial class MainForm : Form
    {
        private SocketHandler mSocket;
        private bool mConnectStatus = false; //获取连接状态
        public static string netType = null; //获取网络的连接状态
        private List<DataModel> mModifyList = new List<DataModel>(); //提交时获取的修改队列
        private Thread resultThread; //通过线程打印执行结果
        public bool isOperationSuccessful = false; //通过修改状态打印执行结果
        public string resultMsg; //打印的结果
        private bool isResultRunning; //控制打印结果的线程执行
        public string stb_softVersion; //软件版本信息，提供给升级模块
        private UpgradeSocket upgradesocket; //发送升级数据的socket
        private int timezone_index; //记录时区的位置
        private bool isInUpgrade = false; //记录是否处于升级状态
        private string ipAdrees;
        //声明API函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "EnableWindow")]
        static extern long EnableWindow(IntPtr hwnd, bool fEnable);
        public MainForm()
        {
            InitializeComponent();
            initUiList();
            mSocket = new SocketHandler(this);
            DataModel.initTimeZone();
            initConbobox();
            isResultRunning = true;
            resultThread = new Thread(printResult);
            resultThread.Start();
        }

        //设置默认的焦点
        private void MainForm_Activated(object sender, EventArgs e)
        {
            text_ip1.Focus();
        }

        //初始化用户名和时区的初始值
        private void initConbobox()
        {
            this.comboBox_name.SelectedIndex = 0;
            foreach (string timezone in DataModel.timezoneList)
            {
                this.comboBox_timezone.Items.Add(timezone);
                this.edt_timezone.Items.Add(timezone);
            }
        }

        // 判断输入IP  start
        private int ipJustfy(TextBox textbox)
        {
            string ip1 = textbox.Text.ToString();
            if (ip1 == String.Empty)
                return 0;
            if (Convert.ToInt32(ip1) > 255)
            {
                textbox.Text = "255";
                return 1;
            }

            if (Convert.ToInt32(ip1) > 100)
            {
                return 1;
            }
            return -1;
        }

        private void text_ip_KeyPress(object sender, KeyPressEventArgs e, int index)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            switch (index)
            {
                case 1:
                    if (e.KeyChar == '.' && text_ip1.Text.ToString() != String.Empty)
                        text_ip2.Focus();
                    break;
                case 2:
                    if (e.KeyChar == 8 && text_ip2.Text.ToString() == String.Empty)
                        text_ip1.Focus();
                    if (e.KeyChar == '.' && text_ip2.Text.ToString() != String.Empty)
                        text_ip3.Focus();
                    break;
                case 3:
                    if (e.KeyChar == 8 && text_ip3.Text.ToString() == String.Empty)
                        text_ip2.Focus();
                    if (e.KeyChar == '.' && text_ip3.Text.ToString() != String.Empty)
                        text_ip4.Focus();
                    break;
                case 4:
                    if (e.KeyChar == 8 && text_ip4.Text.ToString() == String.Empty)
                        text_ip3.Focus();
                    break;
            }
        }

        private void text_ip1_KeyPress(object sender, KeyPressEventArgs e)
        {
            text_ip_KeyPress(sender, e, 1);
        }

        private void text_ip2_KeyPress(object sender, KeyPressEventArgs e)
        {
            text_ip_KeyPress(sender, e, 2);
        }

        private void text_ip3_KeyPress(object sender, KeyPressEventArgs e)
        {
            text_ip_KeyPress(sender, e, 3);
        }

        private void text_ip4_KeyPress(object sender, KeyPressEventArgs e)
        {
            text_ip_KeyPress(sender, e, 4);
        }

        private void text_ip1_TextChanged(object sender, EventArgs e)
        {
            if (1 == ipJustfy(text_ip1))
                text_ip2.Focus();
        }

        private void text_ip2_TextChanged(object sender, EventArgs e)
        {
            if (1 == ipJustfy(text_ip2))
                text_ip3.Focus();
        }

        private void text_ip3_TextChanged(object sender, EventArgs e)
        {
            if (1 == ipJustfy(text_ip3))
                text_ip4.Focus();
        }

        private void text_ip4_TextChanged(object sender, EventArgs e)
        {
            if (1 == ipJustfy(text_ip4))
                comboBox_name.Focus();
        }
        // 判断输入IP  end

        //第二个面板按键的跳转  start
        private void panel_scroll_to(int height)
        {
            Point p = new Point(0, height);
            panel_setting.AutoScrollPosition = p;
        }

        private void btn_iptvsetting_Click(object sender, EventArgs e)
        {
            panel_scroll_to(0);
        }

        private void btn_netsetting_Click(object sender, EventArgs e)
        {
            panel_scroll_to(135);
        }

        private void btn_serversetting_Click(object sender, EventArgs e)
        {
            panel_scroll_to(276);
        }

        private void btn_performlog_Click(object sender, EventArgs e)
        {
            panel_scroll_to(466);
        }

        private void btn_networkmanager_Click(object sender, EventArgs e)
        {
            panel_scroll_to(604);
        }
        //第二个面板按键的跳转  end

        private void btn_connect_Click(object sender, EventArgs e)
        {
            stbConnect();
        }

        private void stbConnect()
        {
            if (mSocket == null)
                return;
            if (text_ip1.Text.ToString() == string.Empty ||
                text_ip2.Text.ToString() == string.Empty ||
                text_ip3.Text.ToString() == string.Empty ||
                text_ip4.Text.ToString() == string.Empty)
            {
                text_status.Text = "错误，IP为空!";
                    return;
            }

            if (comboBox_name.Text.ToString() == string.Empty)
            {
                text_status.Text = "错误，用户名为空!";
                    return;
            }

            if (text_password.Text.ToString() == string.Empty)
            {
                text_status.Text = "错误，密码为空!";
                  return;
            }
            ipAdrees = text_ip1.Text.ToString() + "." + text_ip2.Text.ToString() + "." + text_ip3.Text.ToString() + "." + text_ip4.Text.ToString();
            if (!mConnectStatus)
            {
                initParamListData();
                text_status.Text = "正在连接...";
                connect_name = comboBox_name.Text.ToString();
                connect_password = text_password.Text.ToString();
                Thread connect = new Thread(connectThread);
                connect.Start();
            }
            else
            {
                disconnect();
            }
        }

        private Object thisLock = new Object();
        private string connect_name;
        private string connect_password;
        //锁住线程，防止多次开启socket
        private void connectThread()
        {
            lock (thisLock)
            {
                string msg = mSocket.startConnectStb(connect_name, connect_password, ipAdrees);
                if(msg == string.Empty)
                    updateResultMeg("建立网络连接失败，请确保网络通达或远程连接是否已打开！");
                else
                    messageHandler(msg);
            }
        }

        //关闭程序后停止socket和心跳线程
        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            isResultRunning = false;
            disconnect();
        }

        //对连接后返回的结果反馈在UI上
        private void messageHandler(string msg)
        {
            if (msg.Contains("200"))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lock (this.btn_connect)
                    {
                        btn_connect.Text = "断开";
                    }
                });
                mConnectStatus = true;
                resultMsg = "连接成功！";
            }
            else if (msg.Contains("100"))
            {
                resultMsg = "建立网络连接失败，请确保网络通达！";
            }
            else if (msg.Contains("400"))
            {
                resultMsg = "连接被拒绝，请确认远程连接是否已打开！";
            }
            else if (msg.Contains("501"))
            {
                resultMsg = "用户名或密码错误！";
            }
            else if (msg.Contains("503"))
            {
                DialogResult dr;
                dr = MessageBox.Show("连续5次连接失败，机顶盒将锁定三分钟！", "连接失败", MessageBoxButtons.OK,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.OK)
                {
                    mSocket.stopConnect();
                    mConnectStatus = false;
                }
                resultMsg = "";
            }
            isOperationSuccessful = true;
        }

        //对enter键的处理
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control ctl = this.ActiveControl;
            if (keyData == Keys.Enter)
            {
                stbConnect();
            }
            bool ret = base.ProcessCmdKey(ref msg, keyData);
            return ret;
        }

        //重启按键处理
        private void btn_reboot_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            DialogResult dr;
            dr = MessageBox.Show("重启机顶盒可能会影响客户当前使用，请确定您已经征得客户同意！", "重启机顶盒", MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.OK)
            {
                mSocket.sendIoctlMessage("ioctl^reboot^null");
            }
            Thread disconnectThread = new Thread(afterUpgradeDisconnect);
            disconnectThread.Start(); //在重启后断开连接
        }

        //恢复出厂按键处理
        private void btn_reset_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            DialogResult dr;
            dr = MessageBox.Show("恢复出厂可能会影响客户当前使用，请确定您已经征得客户同意！", "恢复出厂", MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.OK)
            {
                mSocket.sendIoctlMessage("ioctl^restore_setting^null");
            }
            Thread disconnectThread = new Thread(afterUpgradeDisconnect);
            disconnectThread.Start(); //在恢复出厂后断开连接
        }

        //断开连接处理
        public void disconnect()
        {
            clearData();
            mConnectStatus = false;
            isImportData2 = false;
            isImportData1 = false;
            isInUpgrade = false;
            updateUpgradeButton(true);
            resuntlTimeSpan = 500;
            updateButtonEnable("disconnect", true);
            if (upgradesocket != null)
                upgradesocket.stopThread(); //停止下载的线程
            // 非UI线程通知主线程修改UI状态
            this.Invoke((MethodInvoker)delegate
                  {
                      lock (this.btn_connect)
                      {
                          btn_connect.Text = "连接";
                      }
                      lock (this.text_status)
                      {
                          text_status.Text = "连接已断开！";
                      }
                  });
            mSocket.stopConnect();
        }

        //请求获取值后更新页面UI显示
        public void updateUI(List<DataModel> list, int listIndex)
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (DataModel model in list)
                {
                    switch (listIndex)
                    {
                        case 1:
                            if (model.getName().Equals("timeZone"))
                            {
                                lock (comboBox_timezone)
                                {
                                    // comboBox_timezone.Text = model.getValue();
                                    //兼容stb获取的UTC和GMT问题
                                    for (int index = 0; index < DataModel.timezoneList.Count; index++)
                                    {
                                        if (model.getValue().Equals(DataModel.timezoneUTCList[index]))
                                        {
                                            comboBox_timezone.SelectedIndex = index;
                                            timezone_index = index;
                                            if (comboBox_timezone.Text.ToString() == string.Empty)
                                                comboBox_timezone.SelectedText = DataModel.timezoneList[index];
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (model.getName().Equals("connecttype"))
                            {
                                string nettype = null;
                                if ("3".Equals(model.getValue()))
                                    nettype = "STATIC IP";
                                else if ("1".Equals(model.getValue()))
                                    nettype = "PPPoE";
                                else if ("2".Equals(model.getValue()))
                                    nettype = "DHCP";
                                lock (text_nettype)
                                {
                                    text_nettype.Text = nettype;
                                }
                            }
                            else if (model.getName().Equals("localTime"))
                            {
                                string time = time_format(model.getValue());
                                lock ((TextBox)model.getObject())
                                {

                                    ((TextBox)model.getObject()).Text = time;
                                }
                            }
                            else
                            {
                                if (model.getName().Equals("SoftwareVersion"))
                                {
                                    stb_softVersion = model.getValue();
                                }
                                lock ((TextBox)model.getObject())
                                {
                                    ((TextBox)model.getObject()).Text = model.getValue();
                                }
                            }
                            break;

                        case 2:
                            if (model.getObject() != null)
                            {
                                lock ((TextBox)model.getObject())
                                {
                                    ((TextBox)model.getObject()).Text = model.getValue();
                                }


                            }
                            else
                            {
                                RadioButton tmpBtn = null;
                                if (model.getName().Equals("connecttype"))
                                {
                                    if ("3".Equals(model.getValue()))
                                        tmpBtn = rbt_static;
                                    else if ("1".Equals(model.getValue()))
                                        tmpBtn = rbt_pppoe;
                                    else if ("2".Equals(model.getValue()))
                                        tmpBtn = rbt_dhcp;
                                }

                                if (model.getName().Equals("QoSLogSwitch"))
                                {
                                    if ("1".Equals(model.getValue()))
                                        tmpBtn = qos_on;
                                    else
                                        tmpBtn = qos_off;
                                }


                                if (model.getName().Equals("browser_log_switch"))
                                {
                                    if ("1".Equals(model.getValue()))
                                        tmpBtn = browserlog_on;
                                    else
                                        tmpBtn = browserlog_off;
                                }


                                if (model.getName().Equals("TMSEnable"))
                                {
                                    if ("1".Equals(model.getValue()))
                                        tmpBtn = management_on;
                                    else
                                        tmpBtn = management_off;
                                }


                                if (model.getName().Equals("TMSHeartBit"))
                                {
                                    if ("1".Equals(model.getValue()))
                                        tmpBtn = heartbit_on;
                                    else
                                        tmpBtn = heartbit_off;
                                }

                                if (model.getName().Equals("timeZone"))
                                {
                                    lock (edt_timezone)
                                    {
                                        for (int index = 0; index < DataModel.timezoneList.Count; index++)
                                        {
                                            if (model.getValue().Equals(DataModel.timezoneUTCList[index]))
                                            {
                                                edt_timezone.SelectedIndex = index;
                                                timezone_index = index;
                                                if (edt_timezone.Text.ToString() == string.Empty)
                                                    edt_timezone.SelectedText = DataModel.timezoneList[index];
                                                break;
                                            }

                                        }
                                    }
                                }
                                else if (tmpBtn != null)
                                {
                                    lock (tmpBtn)
                                    {
                                        tmpBtn.Checked = true;
                                    }
                                }
                            }
                            break;
                    }
                }
            });
        }

        //第一页的刷新按钮处理
        private void btn_fresh_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            clearTable1();
            updateStatus("正在更新数据...");
            mSocket.initSendData(DataModel.table1List, 1, "read");
            mSocket.sendMessage();
            isImportData1 = false;
        }

        //第二页的刷新按钮处理
        private void tb2_btn_refresh_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            clearTable2();
            updateStatus("正在更新数据...");
            mSocket.initSendData(DataModel.table2List, 2, "read");
            mSocket.sendMessage();
            isImportData2 = false;
        }

        //第一页的提交按钮处理
        private void btn_commit_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            updateStatus("正在提交数据...");
            mModifyList.Clear();
            readTable1UIData();
            if (mModifyList.Count == 0)
            {
                text_status.Text = "没有可提交的数据！";
                return;
            }
            mSocket.initSendData(mModifyList, 1, "write");
            mSocket.sendMessage();
            isImportData1 = false;
        }

        //第二页的提交按钮处理
        private void tb2_btn_commit_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            updateStatus("正在提交数据...");
            mModifyList.Clear();
            readTable2UIData();
            if (mModifyList.Count == 0)
            {
                text_status.Text = "没有可提交的数据！";
                return;
            }
            mSocket.initSendData(mModifyList, 2, "write");
            mSocket.sendMessage();
            isImportData2 = false;
        }

        //更新状态栏打印信息
        public void updateResultMeg(string msg)
        {
            resultMsg = msg;
            isOperationSuccessful = true;
        }

        private int resuntlTimeSpan = 500;
        //线程打印操作的处理结果
        private void printResult()
        {
            while (isResultRunning)
            {
                if (isOperationSuccessful)
                {
                    Thread.Sleep(resuntlTimeSpan);
                    updateStatus(resultMsg);
                    isOperationSuccessful = false;
                    resultMsg = "";
                }
            }
        }

        //第二个面板中网络按钮下panel的显示处理
        private void rbtnetwork_CheckedChanged(object sender, EventArgs e)
        {
            if (rbt_static.Checked == true)
                setNetworkPanel(panel_static);
            else if (rbt_dhcp.Checked == true)
                setNetworkPanel(panel_dhcp);
            else if (rbt_pppoe.Checked == true)
                setNetworkPanel(panel_pppoe);
        }

        //网络选择下panel的展示结果
        private void setNetworkPanel(Panel panel)
        {
            panel_static.Visible = false;
            panel_dhcp.Visible = false;
            panel_pppoe.Visible = false;

            panel.Visible = true;
        }

        //通过Invoke处理非UI线程调用的执行结果
        public void updateStatus(string msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.text_status)
                {
                    text_status.Text = msg;
                }
            });
        }

        //更新按钮状态
        public void updateButtonEnable(string str, Boolean enable)
        {
            Button btn = null;
            if (str.Equals("404")) //上传文件不存在
            {
                resultMsg = "上传文件不存在";
            }
            else if (str.Equals("500")) //无法连接sftp服务器
            {
                resultMsg = "无法连接sftp服务器";
            }
            else if (str.Equals("info")) //一键信息收集启动键
            {
                btn = info_start; ;
                resultMsg = "操作成功";
            }
            else if (str.Equals("start")) //一键信息收集启动键
            {
                btn = boot_start; ;
                resultMsg = "操作成功";
            }
            else if (str.Equals("disconnect")) //断开连接后设置激活开机信息收集启动键
            {
                btn = boot_start; ;
            }
            else if (str.Equals("info_upload") || str.Equals("start_upload") || str.Equals("picture_upload")) //开机信息收集上传键
            {
                if (enable)
                {
                    resultMsg = "上传成功"; ;
                }
                else
                {
                    resultMsg = "";
                }
            }
            else if (str.Equals("info_started"))
            {
                resultMsg = "收集信息已经开启";
            }
            if (resultMsg != string.Empty)
                isOperationSuccessful = true;
            if (btn == null)
                return;
            this.Invoke((MethodInvoker)delegate
            {
                lock (btn)
                {
                    btn.Enabled = enable;
                }
                if (str.Equals("info")) //一键信息收集启动键
                {
                    lock (info_upload)
                    {
                        info_upload.Enabled = enable;
                    }
                }
                else if (str.Equals("start") || str.Equals("disconnect"))
                {
                    lock (boot_upload)
                    {
                        boot_upload.Enabled = enable;
                    }
                }
            });
        }

        // 获取面板一中修改数据的队列
        private void readTable1UIData()
        {
            List<DataModel> tempList = new List<DataModel>();
            foreach (DataModel model in DataModel.table1List)
            {
                string value = "";
                if (model.getName().Equals("timeZone"))
                {
                    value = comboBox_timezone.Text.ToString();
                    //兼容时区中UTC和GMT
                    if (value.Equals(DataModel.timezoneList[timezone_index]) &&
                        model.getValue().Equals(DataModel.timezoneUTCList[timezone_index]))
                    {
                        value = model.getValue();
                    }
                }
                else
                {
                    value = ((TextBox)model.getObject()).Text.ToString();
                }
                if (!value.Equals(model.getValue()) && !model.getName().Equals("connecttype") && !model.getName().Equals("localTime"))  //网络状态已经转换,避免每次都可以提交
                {
                    model.setValue(value);
                    mModifyList.Add(model);
                }
                tempList.Add(model);
            }
            DataModel.table1List = tempList;
        }

        // 获取面板二中修改数据的队列
        private void readTable2UIData()
        {
            List<DataModel> tempList = new List<DataModel>();
            netType = null;
            foreach (DataModel model in DataModel.table2List)
            {
                string tempValue = null;
                if (model.getObject() != null)
                {
                    tempValue = ((TextBox)model.getObject()).Text.ToString();
                }
                else
                {
                    if (model.getName().Equals("connecttype"))
                    {
                        if (rbt_static.Checked == true)
                            tempValue = "3";
                        else if (rbt_dhcp.Checked == true)
                            tempValue = "2";
                        else if (rbt_pppoe.Checked == true)
                            tempValue = "1";
                        if (!model.getValue().Equals(tempValue))
                        {
                            netType = tempValue;
                        }
                    }

                    if (model.getName().Equals("QoSLogSwitch"))
                    {
                        if (qos_on.Checked == true)
                            tempValue = "1";
                        else
                            tempValue = "0";
                    }


                    if (model.getName().Equals("browser_log_switch"))
                    {
                        if (browserlog_on.Checked == true)
                            tempValue = "1";
                        else
                            tempValue = "0";
                    }


                    if (model.getName().Equals("TMSEnable"))
                    {
                        if (management_on.Checked == true)
                            tempValue = "1";
                        else
                            tempValue = "0";
                    }


                    if (model.getName().Equals("TMSHeartBit"))
                    {
                        if (heartbit_on.Checked == true)
                            tempValue = "1";
                        else
                            tempValue = "0";
                    }

                    if (model.getName().Equals("timeZone"))
                    {
                        tempValue = edt_timezone.Text.ToString();
                        if (tempValue.Equals(DataModel.timezoneList[timezone_index]) &&
                        model.getValue().Equals(DataModel.timezoneUTCList[timezone_index]))
                        {
                            tempValue = model.getValue();
                        }
                    }
                }
                if (!tempValue.Equals(model.getValue()))
                {
                    model.setValue(tempValue);
                    mModifyList.Add(model);
                }
                tempList.Add(model);
            }
            DataModel.table2List = tempList;
        }

        //升级过程中不让页面切换
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (isInUpgrade)
            {
                e.Cancel = true;
            }
        }

        //tableConrol页面切换
        private bool isImportData1 = false;
        private bool isImportData2 = false; //在导入参数执行后不刷新页面数据
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (SocketHandler.client == null)
                return;
            if (!SocketHandler.client.Connected)
            {
                updateStatus("连接已断开");
                return;
            }
            if (e.TabPage == tabPage3)
            {
                if (mConnectStatus && !isImportData1)
                {
                    updateStatus("正在获取数据...");
                    clearTable1();
                    mSocket.initSendData(DataModel.table1List, 1, "read");
                    mSocket.sendMessage();
                }
            }
            else if (e.TabPage == tabPage1)
            {
                if (mConnectStatus && !isImportData2)
                {
                    updateStatus("正在获取数据...");
                    clearTable2();
                    mSocket.initSendData(DataModel.table2List, 2, "read");
                    mSocket.sendMessage();
                }
            }
            else if (e.TabPage == tabPage4)
            {
                tabControl2.SelectedIndex = 0;
            }
            else if (e.TabPage == tabPage2)
            {
                software_version.Text = stb_softVersion;
            }
        }

        private void clearTable1()
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (DataModel model in DataModel.table1List)
                {
                    if (model.getObject() == null)
                        continue;
                    lock ((TextBox)model.getObject())
                    {
                        ((TextBox)model.getObject()).Text = "";
                    }
                }
                lock (this.comboBox_timezone)
                {
                    comboBox_timezone.Text = "";
                }
            });
        }

        private void clearTable2()
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.edt_timezone)
                {
                    edt_timezone.Text = "";
                }
                foreach (DataModel model in DataModel.table2List)
                {
                    if (model.getObject() == null)
                        continue;
                    lock ((TextBox)model.getObject())
                    {
                        ((TextBox)model.getObject()).Text = "";
                    }
                }
                foreach (RadioButton rbt in DataModel.rbtlist)
                {
                    lock (rbt)
                    {
                        rbt.Checked = false;
                    }
                }
            });
        }

        //断开连接清除数据
        private void clearData()
        {
            clearTable1();
            clearTable2();
            this.Invoke((MethodInvoker)delegate
            {              
                foreach (TextBox textbox in DataModel.info_textList)
                {
                    lock (textbox)
                    {
                        textbox.Text = "";
                    }
                }
                lock (info_allpick)
                {
                    info_allpick.Checked = false;
                }
                lock (info_tcpdump)
                {
                    info_tcpdump.Checked = false;
                }
                lock (this.upgrade_progress)
                {
                    upgrade_progress.Visible = false;
                }
                lock (this.force_upgrade)
                {
                    force_upgrade.Checked = false;
                }
            });
        }

        //格式化获取的时间
        private string time_format(string time)
        {
            if (time == string.Empty)
                return "";
            string year = time.Substring(0, 4);
            string month = time.Substring(4, 2);
            string day = time.Substring(6, 2);
            string hour = time.Substring(8, 2);
            string minute = time.Substring(10, 2);
            string second = time.Substring(12, 2);
            time = year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;
            return time;
        }

        //导入参数
        private void btn_import_params_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string fName = StbToolUtils.GetOpenFileName();
            if (fName == string.Empty)
                return;
            StbToolUtils.ReadTextFileToList(fName);
            updateResultMeg("导入参数成功");
            updateUI(DataModel.paramsList1, 1);
            updateUI(DataModel.paramsList2, 2);
            isImportData1 = true;
            isImportData2 = true;  //判断导入数据后不刷新页面数据
        }

        //导出参数
        private void btn_export_params_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string fName = StbToolUtils.GetSaveFileName();
            if (fName == string.Empty)
                return;
            StbToolUtils.WriteListToTextFile(fName);
            updateResultMeg("导出参数成功");
        }

        //一键收集按全选后抓包选项也选上
        private void info_allpick_CheckedChanged(object sender, EventArgs e)
        {
            if (info_allpick.Checked == true)
            {
                info_tcpdump.Checked = true;
            }
        }

        //一键收集启动按钮
        private void info_start_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string sendMsg = "";
            updateStatus("收集信息");
            if (info_tcpdump.Checked == true)
            {
                sendMsg = "ioctl^startDebugInfo^null^tcpdump -i eth0 -s 0";
            }
            else
            {
                sendMsg = "ioctl^startDebugInfo^null";
            }
            mSocket.sendIoctlMessage(sendMsg);
        }

        //一键收集的停止按钮
        private void info_stop_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            if (info_start.Enabled)
            {
                updateStatus("");
                resultMsg = "已经停止";
                isOperationSuccessful = true;
                return;
            }
            updateStatus("停止收集");
            mSocket.sendIoctlMessage("ioctl^stopDebugInfo^null");
        }

        //一键信息收集上传
        private void info_upload_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string errorMsg = "";
            if (info_stfp_host.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP地址为空");
                errorMsg = "SFTP地址为空";
            }
            else if (!StbToolUtils.IsCorrectIP(info_stfp_host.Text.ToString()))
            {
                updateStatus("非法的SFTP地址");
                errorMsg = "非法的SFTP地址";
            }
            else if (info_sftp_name.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP用户名为空");
                errorMsg = "SFTP用户名为空";
            }
            else if (info_sftp_pwd.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP密码为空");
                errorMsg = "SFTP密码为空";
            }
            DialogResult dr;
            if (errorMsg != string.Empty)
            {
                dr = MessageBox.Show(errorMsg, "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            else
            {
                dr = MessageBox.Show("请确认SFTP服务已经开启", "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            if (dr == DialogResult.OK)
            {
                string sendMsg = "ioctl^UploadDebugInfo^null^" + info_stfp_host.Text.ToString() + "^" + info_sftp_name.Text.ToString() + "^" + info_sftp_pwd.Text.ToString();
                updateStatus("开始上传");
                mSocket.sendIoctlMessage(sendMsg);
            }
        }

        //开机信息收集启动
        private void boot_start_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string sendMsg = "";
            if (start_size.Text.ToString() == string.Empty)
            {
                MessageBox.Show("抓包大小为空", "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            else if (Convert.ToInt32(start_size.Text.ToString()) > 10 || Convert.ToInt32(start_size.Text.ToString()) < 0)
            {
                MessageBox.Show("抓包大小不在数值范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            updateStatus("开机信息收集");
            sendMsg = "ioctl^starStartupInfo^null^" + start_size.Text.ToString();
            mSocket.sendIoctlMessage(sendMsg);
        }

        //开机信息收集上传
        private void start_upload_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;

            string errorMsg = "";
            if (start_sftp_host.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP地址为空");
                errorMsg = "SFTP地址为空";
            }
            else if (!StbToolUtils.IsCorrectIP(start_sftp_host.Text.ToString()))
            {
                updateStatus("非法的SFTP地址");
                errorMsg = "非法的SFTP地址";
            }
            else if (start_sftp_name.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP用户名为空");
                errorMsg = "SFTP用户名为空";
            }
            else if (start_sftp_pwd.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP密码为空");
                errorMsg = "SFTP密码为空";
            }
            DialogResult dr;
            if (errorMsg != string.Empty)
            {
                dr = MessageBox.Show(errorMsg, "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            else
            {
                dr = MessageBox.Show("请确认SFTP服务已经开启", "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            if (dr == DialogResult.OK)
            {
                string sendMsg = "ioctl^UploadStartupInfo^null^" + start_sftp_host.Text.ToString() + "^" + start_sftp_name.Text.ToString() + "^" + start_sftp_pwd.Text.ToString();
                updateStatus("开始上传");
                mSocket.sendIoctlMessage(sendMsg);
            }
        }

        //开机信息收集停止
        private void boot_stop_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            if (boot_start.Enabled)
            {
                updateStatus("");
                resultMsg = "已经停止";
                isOperationSuccessful = true;
                return;
            }
            updateStatus("停止收集");
            mSocket.sendIoctlMessage("ioctl^stopStartupInfo^null");
        }

        //图片信息收集上传
        private void picture_upload_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;

            string errorMsg = "";
            if (picture_sftp_host.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP地址为空");
                errorMsg = "SFTP地址为空";
            }
            else if (!StbToolUtils.IsCorrectIP(picture_sftp_host.Text.ToString()))
            {
                updateStatus("非法的SFTP地址");
                errorMsg = "非法的SFTP地址";
            }
            else if (picture_sftp_name.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP用户名为空");
                errorMsg = "SFTP用户名为空";
            }
            else if (picture_sftp_pwd.Text.ToString() == string.Empty)
            {
                updateStatus("SFTP密码为空");
                errorMsg = "SFTP密码为空";
            }
            DialogResult dr;
            if (errorMsg != string.Empty)
            {
                dr = MessageBox.Show(errorMsg, "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            else
            {
                dr = MessageBox.Show("请确认SFTP服务已经开启", "", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            if (dr == DialogResult.OK)
            {
                string sendMsg = "ioctl^UploadScreencap^null^" + picture_sftp_host.Text.ToString() + "^" + picture_sftp_name.Text.ToString() + "^" + picture_sftp_pwd.Text.ToString();
                updateStatus("开始上传");
                mSocket.sendIoctlMessage(sendMsg);
            }
        }

        private bool isPlayInfoStop = false;
        //可视化定位刷新按钮
        private void playInfo_fresh_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            if (!isPlayInfoStop)
            {
                isPlayInfoStop = true;
                updatePlayInfoButton(true);
                mSocket.sendPlayInfoMsg();
            }
            else
            {
                isPlayInfoStop = false;
                mSocket.stopPlayInfoMsg();
                updatePlayInfoButton(false);
                updateStatus("可视化信息收集已停止");
            }
        }

        //更新可视化定位信息收集的刷新按钮状态
        public void updatePlayInfoButton(bool status)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.playInfo_fresh)
                {
                    if (status)
                        playInfo_fresh.Text = "停止";
                    else
                        playInfo_fresh.Text = "刷新";
                }
            });
        }

        //更新可视化定位UI的数据
        public void updatePlayInfoUI()
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (DataModel model in DataModel.playInfo1List)
                {
                    lock ((Label)model.getObject())
                    {
                        ((Label)model.getObject()).Text = model.getValue();
                    }
                }
            });
        }

        //可视化定位信息收集导出参数
        private void playinfo_export_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            string fName = StbToolUtils.GetSaveFileName();
            if (fName == string.Empty)
                return;
            StbToolUtils.WritePlayInfoToTextFile(fName);
            updateResultMeg("导出参数成功");
        }

        string upgradePath = "";
        //升级获取zip包路径
        private void btn_select_updatezip_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            upgradePath = StbToolUtils.GetUpgradeZipFileName();
            text_upgrade_path.Text = upgradePath;
        }

        //升级按钮的处理
        private void btn_upgrade_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus || upgradePath == string.Empty)
                return;
            int port = mSocket.sendUpgradeMsg(upgradePath, force_upgrade.Checked);
            if (port != 0)
            {
                upgradesocket = new UpgradeSocket(this);
                upgradesocket.startUpgrade(ipAdrees, port, upgradePath);
                upgrade_progress.Visible = true;
                resuntlTimeSpan = 0;
                isInUpgrade = true;
                updateUpgradeButton(false);
            }
        }

        //更新升级按钮状态
        private void updateUpgradeButton(bool status)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.btn_select_updatezip)
                {
                    btn_select_updatezip.Enabled = status;
                }
                lock (this.btn_upgrade)
                {
                    btn_upgrade.Enabled = status;
                }
                lock (this.force_upgrade)
                {
                    force_upgrade.Enabled = status;
                }
            });
        }

        //升级时更新页面状态
        public void upgrade_result(int progress)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.upgrade_status)
                {
                    upgrade_status.AppendText("正在发送文件..." + progress + "%\n");
                    updateStatus("正在发送文件..." + progress + "%");
                }
                lock (this.upgrade_progress)
                {
                    upgrade_progress.Value = progress;
                }
                if (progress == 100)
                {
                    updateResultMeg("升级成功");
                    Thread disconnectThread = new Thread(afterUpgradeDisconnect);
                    disconnectThread.Start(); //在升级后断开连接
                }
            });
        }

        private void afterUpgradeDisconnect()
        {
            Thread.Sleep(5000);
            disconnect();
        }
    }
}
