using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace 查号管理
{
    public partial class LoginForm : Form
    {
        public GridManage m_manage;

        public LoginForm()
        {
            InitializeComponent();
        }

        public static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] ips = new string[] { GetLocalIp(), "121.42.148.243" };
            string authentication_ip = "";
            for (int i = 0; i < ips.Length; ++i)
            {
                try
                {
                    Socket tmpSocket;
                    tmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tmpSocket.Connect(ips[i], 7626);
                    authentication_ip = ips[i];
                    tmpSocket.Close();
                    break;
                }
                catch
                {
                }
            }

            if (authentication_ip == "")
            {
                MessageBox.Show("检测服务器失败!!服务器维护中...","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            if (m_manage.m_AuthenticationSession != null)
            {
                
            }
        }
    }
}
