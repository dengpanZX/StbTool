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
        private bool mConnectStatus = false;
        private bool isModifyList1 = false;
        private bool isModifyList2 = false;
        public MainForm()
        {
            InitializeComponent();
            initListData();
            mSocket = new SocketHandler(this);
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
        // 判断输入IP  start

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

        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            mSocket.stopConnect();
        }

        private void messageHandler(string msg)
        {
            if (msg.Contains("200"))
            {
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

        private void btn_reboot_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            mSocket.sendRebootCmd();
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            mSocket.resetFactory();
        }

        public void disconnect()
        {
            mSocket.stopConnect();
            mConnectStatus = false;
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
                                lock ((ComboBox)model.getObject())
                                {
                                    ((ComboBox)model.getObject()).Text = model.getValue();
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
                                        edt_timezone.Text = model.getValue();
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
            isModifyList1 = false;
            isModifyList2 = false;
        }

        private void btn_fresh_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            mSocket.initSendData(DataModel.table1List, 1, "read");
            mSocket.sendMessage();
            updateStatus("数据更新成功");
        }

        private void btn_commit_Click(object sender, EventArgs e)
        {
            if (!mConnectStatus)
                return;
            if (!isModifyList1)
            {
                text_status.Text = "没有可提交的数据！";
                return;
            }
            readTable1UIData();
            mSocket.initSendData(DataModel.table1List, 1, "write");
            mSocket.sendMessage();
            isModifyList1 = false;
            updateStatus("数据提交成功");
        }

        private void table1List_TextChanged(object sender, EventArgs e)
        {
            isModifyList1 = true;
        }

        private void table2List_TextChanged(object sender, EventArgs e)
        {
            isModifyList2 = true;
        }

        private void rbt_CheckedChanged(object sender, EventArgs e)
        {
            isModifyList2 = true;
        }

        private void updateStatus(string msg)
        {
            lock (this.text_status)
            {
                text_status.Text = msg;
            }
        }

        private void readTable1UIData()
        {
            List<DataModel> tempList = new List<DataModel>();
            foreach (DataModel model in DataModel.table1List)
            {
                if (model.getName().Equals("timeZone"))
                {
                    model.setValue(((ComboBox)model.getObject()).Text.ToString());
                }
                else
                {
                    model.setValue(((TextBox)model.getObject()).Text.ToString());
                }
                tempList.Add(model);
            }
            DataModel.table1List = tempList;
        }

        private void readTable2UIData()
        {
            List<DataModel> tempList = new List<DataModel>();
            foreach (DataModel model in DataModel.table2List)
            {
                if (model.getObject() != null)
                {
                    model.setValue(((TextBox)model.getObject()).Text.ToString());
                }
                else
                {
                    string tempValue = null;
                    if (model.getName().Equals("connecttype"))
                    {
                        if (rbt_static.Checked == true)
                            tempValue = "3";
                        else if (rbt_dhcp.Checked == true)
                            tempValue = "1";
                        else if (rbt_pppoe.Checked == true)
                            tempValue = "2";
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
                        model.setValue(edt_timezone.Text.ToString());
                    }
                    else if (tempValue != null)
                    {
                        model.setValue(tempValue);
                    }
                }
                tempList.Add(model);
            }
            DataModel.table2List = tempList;
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            Console.WriteLine(">>>>>>>>>" + isFirstTable);
            if (!mConnectStatus && isFirstTable)
            {
                e.Cancel = true;
            }
        }

        private static bool isFirstTable = true;
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if(e.TabPage == tabPage3)
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

        private void tb2_btn_refresh_Click(object sender, EventArgs e)
        {
            mSocket.initSendData(DataModel.table2List, 2, "read");
            mSocket.sendMessage();
            updateStatus("数据更新成功");
        }

        private void tb2_btn_commit_Click(object sender, EventArgs e)
        {
            if (!isModifyList2)
            {
                text_status.Text = "没有可提交的数据！";
                return;
            }
            readTable2UIData();
            mSocket.initSendData(DataModel.table2List, 2, "write");
            mSocket.sendMessage();
            isModifyList2 = false;
            updateStatus("数据提交成功");
        }
    }
}
