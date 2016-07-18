using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace StbTool
{
    public partial class MainForm : Form
    {
        private SocketHandler mSocket;
        public MainForm()
        {
            InitializeComponent();
            mSocket = new SocketHandler();
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
                    if (e.KeyChar == '.')
                        text_ip2.Focus();
                    break;
                case 2:
                    if (e.KeyChar == 8 && text_ip2.Text.ToString() == String.Empty)
                        text_ip1.Focus();
                    if (e.KeyChar == '.')
                        text_ip3.Focus();
                    break;
                case 3:
                    if (e.KeyChar == 8 && text_ip3.Text.ToString() == String.Empty)
                        text_ip2.Focus();
                    if (e.KeyChar == '.')
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
            string ip = text_ip1.Text.ToString() + "." + text_ip2.Text.ToString() + "." + text_ip3.Text.ToString() + "." + text_ip4.Text.ToString();
            Console.WriteLine(ip);
            mSocket.startConnectThread(ip);
        }

        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            mSocket.stopThread();
        }
    }
}
