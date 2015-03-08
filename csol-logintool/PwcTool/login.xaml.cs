﻿using System;
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

        const string safe_key = "0x77ffbb";
        const string loginurl = "http://121.42.148.243/captcha.php?";
        //const string loginurl = "http://172.16.3.155/captcha.php?";
        const string captchadb = "captcha";

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            string machineinfo = UrlFunction.UrlEncode(MyDes.Encode(Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";", safe_key));
            string computername = UrlFunction.UrlEncode(Computer.Instance().ComputerName);
            string isneedcaptcha = File.Exists(captchadb) ? "0" : "1";
            string url = loginurl + "username=" + tbxUid.Text + "&machineinfo=" + machineinfo + "&signature=" + m_safekey + "&cpname=" + computername + "&needcaptcha=" + isneedcaptcha;
          
            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 30;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.BeginGetResponse(new AsyncCallback((ar) =>
            {
                StreamReader reader = new StreamReader(request.EndGetResponse(ar).GetResponseStream());
                JObject jsobj = JObject.Parse(reader.ReadToEnd());
                if (jsobj["R"].ToString() == "登录成功")
                {
                    m_lg = jsobj["lg"].ToString();
                    m_pw = jsobj["pw"].ToString();
                    m_dbpwd = jsobj["dbpwd"].ToString();

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CpWorker.Uid = tbxUid.Text;
                        CpWorker.Matchinfo = Computer.Instance().DiskID + ";" + Computer.Instance().CpuID + ";";
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
            }), request);
        }
    }
}