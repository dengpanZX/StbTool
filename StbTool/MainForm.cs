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
        private bool isOperationSuccessful = false; //通过修改状态打印执行结果
        public MainForm()
        {
            InitializeComponent();
            initListData();
            mSocket = new SocketHandler(this);
            DataModel.initTimeZone();
            initConbobox();
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
            panel_scroll_to(431);
        }

        private void btn_performlog_Click(object sender, EventArgs e)
        {
            panel_scroll_to(621);
        }

        private void btn_networkmanager_Click(object sender, EventArgs e)
        {
            panel_scroll_to(759);
        }
        //第二个面板按键的跳转  end

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (mSocket == null)
                return;
            if (text_ip1.Text.ToString() == string.Empty ||
                text_ip2.Text.ToString() == string.Empty ||
                text_ip3.Text.ToString() == string.Empty ||
                text_ip4.Text.ToString() == string.Empty)
            {
                text_status.Text = "错误，IP为空!";
            }

            if (comboBox_name.Text.ToString() == string.Empty)
                text_status.Text = "错误，用户名为空!";

            if (text_password.Text.ToString() == string.Empty)
                text_status.Text = "错误，密码为空!";
            string ip = text_ip1.Text.ToString() + "." + text_ip2.Text.ToString() + "." + text_ip3.Text.ToString() + "." + text_ip4.Text.ToString();
            if (!mConnectStatus)
            {
                text_status.Text = "正在连接...";
                string msg = mSocket.startConnectStb(comboBox_name.Text.ToString(), text_password.Text.ToString(), ip);
                messageHandler(msg);
            }
            else
            {
                disconnect();
            }
        }

        //关闭程序后停止socket和心跳线程
        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            disconnect();
        }

        //对连接后返回的结果反馈在UI上
        private void messageHandler(string msg)
        {
            if (msg.Contains("200"))
            {
                resultThread = new Thread(printResult);
                resultThread.Start();
                btn_connect.Text = "断开";
                mConnectStatus = true;
                text_status.Text = "连接成功！";
            }
            else if (msg.Contains("100"))
            {
                text_status.Text = "没有数据连接超时！";
            }
            else if (msg.Contains("400"))
            {
                text_status.Text = "连接被拒绝，请确认远程连接是否已打开！";
            }
            else if (msg.Contains("501"))
            {
                text_status.Text = "用户名或密码错误！";
            }
        }

        //对textbox的TAB键的处理
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control ctl = this.ActiveControl;
            if (keyData == Keys.Tab)
            {
                if (comboBox_name.Focused)
                {
                    text_password.Focus();
                    return true;
                }

                if (text_ip4.Focused)
                {
                    comboBox_name.Focus();
                    return true;
                }

                if (text_ntp_backup.Focused)
                {
                    comboBox_timezone.Focus();
                    return true;
                }

                if (comboBox_timezone.Focused)
                {
                    text_time.Focus();
                    return true;
                }

                if (text_time.Focused)
                {
                    text_managerdomain.Focus();
                    return true;
                }

                if (text_tvmsaddress.Focused)
                {
                    text_sqmaddress.Focus();
                    return true;
                }

                if (text_sqmaddress.Focused)
                {
                    btn_commit.Focus();
                    return true;
                }

                if (btn_fresh.Focused)
                {
                    text_ip1.Focus();
                    return true;
                }
            }
            bool ret = base.ProcessCmdKey(ref msg, keyData);
            return ret;
        }

        //重启按键处理
        private void btn_reboot_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            mSocket.sendRebootCmd();
        }

        //恢复出厂按键处理
        private void btn_reset_Click(object sender, EventArgs e)
        {
            mSocket.resetFactory();
        }

        //断开连接处理
        public void disconnect()
        {
            clearData();
            mSocket.stopConnect();
            mConnectStatus = false;
            // 非UI线程通知主线程修改UI状态
            this.Invoke((MethodInvoker)delegate
                  {
                      lock (this.btn_connect)
                      {
                          btn_connect.Text = "连接"; ;
                      }
                      lock (this.text_status)
                      {
                          text_status.Text = "连接已断开！";
                      }
                  });
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
                                lock ((TextBox)model.getObject())
                                {

                                    ((TextBox)model.getObject()).Text = nettype;
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
            updateStatus("正在更新数据...");
            mSocket.initSendData(DataModel.table1List, 1, "read");
            mSocket.sendMessage();
            isOperationSuccessful = true;
        }

        //第二页的刷新按钮处理
        private void tb2_btn_refresh_Click(object sender, EventArgs e)
        {
            updateStatus("正在更新数据...");
            mSocket.initSendData(DataModel.table2List, 2, "read");
            mSocket.sendMessage();
            isOperationSuccessful = true;
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
            isOperationSuccessful = true;
        }

        //第二页的提交按钮处理
        private void tb2_btn_commit_Click(object sender, EventArgs e)
        {
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
            isOperationSuccessful = true;
        }

        //线程打印操作的处理结果
        private void printResult()
        {
            while (mConnectStatus)
            {
                if (isOperationSuccessful)
                {
                    Thread.Sleep(500);
                    updateStatus("操作成功！");
                    isOperationSuccessful = false;
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
        private void updateStatus(string msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lock (this.text_status)
                {
                    text_status.Text = msg;
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
                    if (value.Equals(DataModel.timezoneList[comboBox_timezone.SelectedIndex]) &&
                        model.getValue().Equals(DataModel.timezoneUTCList[comboBox_timezone.SelectedIndex]))
                    {
                        value = model.getValue();
                    }
                }
                else
                {
                    value = ((TextBox)model.getObject()).Text.ToString();
                }
                if (!value.Equals(model.getValue()) && !model.getName().Equals("connecttype"))  //网络状态已经转换,避免每次都可以提交
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
                        if (tempValue.Equals(DataModel.timezoneList[edt_timezone.SelectedIndex]) &&
                        model.getValue().Equals(DataModel.timezoneUTCList[edt_timezone.SelectedIndex]))
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

        //未连接成功不让页面切换
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (!mConnectStatus && isFirstTable)
            {
                e.Cancel = true;
            }
        }

        //tableConrol页面切换
        private static bool isFirstTable = true;
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            updateStatus("");
            if (e.TabPage == tabPage3)
            {
                mSocket.initSendData(DataModel.table1List, 1, "read");
                isFirstTable = true;
            }
            else if (e.TabPage == tabPage1)
            {
                mSocket.initSendData(DataModel.table2List, 2, "read");
                isFirstTable = false;
            }
            else
            {
                isFirstTable = false;
            }
            mSocket.sendMessage();
        }

        //断开连接清楚数据
        private void clearData()
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

        //格式化获取的时间
        private string time_format(string time)
        {
            string year = time.Substring(0, 4);
            string month = time.Substring(4, 2);
            string day = time.Substring(6, 2);
            string hour = time.Substring(8, 2);
            string minute = time.Substring(10, 2);
            string second = time.Substring(12, 2);
            time = year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;
            return time;
        }
    }
}
