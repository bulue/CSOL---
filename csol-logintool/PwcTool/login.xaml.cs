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
using System.Net.Sockets;
using System.Text.RegularExpressions;

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
        public string m_dbpwd = "";
        public string deadline_date = "";

        const string safe_key = "0x77ffbb";
        const string loginurl = "http://121.42.148.243/captcha.php?";
        //const string loginurl = "http://172.16.3.155/captcha.php?";

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            string machineinfo = UrlFunction.UrlEncode(MyDes.Encode(Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";", safe_key));
            string computername = UrlFunction.UrlEncode(Computer.Instance().ComputerName);
            string isneedcaptcha = File.Exists(tbxUid.Text) ? "0" : "1";
            string url = loginurl + "username=" + tbxUid.Text + "&machineinfo=" + machineinfo + "&signature="
                + m_safekey + "&cpname=" + computername + "&needcaptcha=" + isneedcaptcha +"&appversion=" + App.version;
          
            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 30;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.BeginGetResponse(new AsyncCallback((ar) =>
            {
                try
                {
                    StreamReader reader = new StreamReader(request.EndGetResponse(ar).GetResponseStream());
                    JObject jsobj = JObject.Parse(reader.ReadToEnd());
                    if (jsobj["R"].ToString() == "登录成功")
                    {
                        m_lg = jsobj["lg"].ToString();
                        m_pw = jsobj["pw"].ToString();
                        m_dbpwd = jsobj["dbpwd"].ToString();
                        deadline_date = jsobj["deadline_date"].ToString();
                        CpWorker.KeepRunTime = int.Parse(jsobj["deadline"].ToString()) * 1000;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CpWorker.Uid = tbxUid.Text;
                            CpWorker.Matchinfo = Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";";
                            SeWorker.Uid = tbxUid.Text;
                            SeWorker.Matchinfo = CpWorker.Matchinfo;
                            GuessWorker.Uid = tbxUid.Text;
                            GuessWorker.Matchinfo = CpWorker.Matchinfo;
                            MainWindow.captchadb = CpWorker.Uid;
                            this.Cursor = Cursors.Arrow;
                            this.Close();
                        }));
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.Cursor = Cursors.Arrow;
                            MessageBox.Show(jsobj["R"].ToString(), "");
                        }));
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }), request);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser1.Navigate("http://121.42.148.243/softwareboard/board_pwctool.html?appverion="+App.version);
        }
    }
}
