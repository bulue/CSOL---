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
        public int userlvl = 0;
        public int guessworkernum = 16;

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
                + m_safekey + "&cpname=" + computername + "&needcaptcha=" + isneedcaptcha + "&appversion=" + App.version
                + "&apptime=" + UrlFunction.UrlEncode(string.Format("{0:yy-MM-dd HH:mm:ss}", System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)));
          
            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 30;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.BeginGetResponse(new AsyncCallback((ar) =>
            {
                try
                {
                    WebResponse response = request.EndGetResponse(ar);
                    string con = response.Headers.Get("ContentSize");
                    string html_str = "";
                    if (!String.IsNullOrEmpty(response.Headers.Get("ContentSize")))
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => {
                            this.pbInit.Visibility = Visibility.Visible;
                        }));
                        int content_size = Convert.ToInt32(response.Headers.Get("ContentSize"));
                        if (content_size > 0)
                        {
                            Stream stm = response.GetResponseStream();
                            byte[] buf = new byte[content_size];
                            int offset = 0;
                            do
                            {
                                int bytes = stm.Read(buf, offset, Math.Min(buf.Length - offset,1024));
                                offset += bytes;
                                this.Dispatcher.BeginInvoke(new Action<double>((o) =>
                                {
                                    pbInit.Value = ((double)o) / content_size * 100.0;
                                }), offset);

                                if (bytes == 0) {
                                    html_str = Encoding.GetEncoding("GB2312").GetString(buf);
                                    break;
                                }
                            }while(true);
                        }
                    }
                    else
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        html_str = reader.ReadToEnd();
                    }
                    JObject jsobj = JObject.Parse(html_str);
                    if (jsobj["R"].ToString() == "登录成功")
                    {
                        m_lg = jsobj["lg"].ToString();
                        m_pw = jsobj["pw"].ToString();
                        m_dbpwd = jsobj["dbpwd"].ToString();
                        deadline_date = jsobj["deadline_date"].ToString();
                        userlvl = Convert.ToInt32(jsobj["lvl"].ToString());
                        guessworkernum = Convert.ToInt32(jsobj["gswokernum"].ToString());
                        CpWorker.KeepRunTime = int.Parse(jsobj["deadline"].ToString()) * 1000;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CpWorker.Uid = tbxUid.Text;
                            CpWorker.Matchinfo = Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";";
                            SeWorker.Uid = tbxUid.Text;
                            SeWorker.Matchinfo = CpWorker.Matchinfo;
                            GuessWorker.Uid = tbxUid.Text;
                            GuessWorker.Matchinfo = CpWorker.Matchinfo;
                            CardWorker.Uid = tbxUid.Text;
                            CardWorker.Matchinfo = CpWorker.Matchinfo;
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
            this.Title += " " + App.version + "." + App.subversion;
            this.pbInit.Visibility = Visibility.Collapsed;
        }
    }
}
