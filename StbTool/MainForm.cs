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
        public MainForm()
        {
            InitializeComponent();
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
    }
}
