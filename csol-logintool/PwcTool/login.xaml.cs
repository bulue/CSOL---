using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using CommonQ;

namespace PwcTool
{
    /// <summary>
    /// login.xaml 的交互逻辑
    /// </summary>
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
        }

        public string m_safekey = "";
        public string m_lg = "";
        public string m_pw = "";

        const string safe_key = "0x77ffbb";
        const string loginurl = "http://www.xiaozhuhaoa.com/captcha.php?";
        //const string loginurl = "http://172.16.3.155/captcha.php?";

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string password = MyDes.Encode(Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";", safe_key);
            string computername = Computer.Instance().ComputerName;
            string url = loginurl + "username=" + textBox1.Text + "&password=" + password + "&signature=" + m_safekey + "&cpname=" + computername;

            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 600;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());
            JObject jsobj = JObject.Parse(reader.ReadToEnd());
            if (jsobj["R"].ToString() == "登录成功")
            {
                m_lg = jsobj["lg"].ToString();
                m_pw = jsobj["pw"].ToString();
                this.Close();
            }
            else
            {
                MessageBox.Show(jsobj["R"].ToString(), "");
            }
        }
    }
}
