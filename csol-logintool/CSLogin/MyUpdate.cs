using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace CommonQ
{
    class MyUpdate
    {
        const string ver_url = "http://localhost/download/";

        public void CheckVersion(string nowVersion)
        {
            Process.Start("update.exe", "-autoclose -autostart");
            //string downloadurl = ver_url + "/update-check.php";

            //HttpWebRequest request = System.Net.WebRequest.Create(downloadurl) as HttpWebRequest;
            //request.ServicePoint.Expect100Continue = false;
            //request.Timeout = 1000 * 60;
            //request.ContentType = "application/x-www-form-urlencoded";
            ////request.CookieContainer = new CookieContainer();
            //request.BeginGetResponse(new AsyncCallback(OnCheckAppVersion), request);
            //WebResponse response = request.GetResponse();
            //StreamReader stm = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));
            //string s = stm.ReadToEnd();
            //JObject jsObj = JObject.Parse(s);
            //int allcount = (int)jsObj.Count;
            //int count = 0;
            //foreach (JProperty jp in jsObj.Properties())
            //{
            //    count++;
            //    string fname = jp.Name;
            //    string md5value = (string)jp.Value;
            //    string fpath = fname.Substring(fname.IndexOfAny(new char[] { '\\', '/' }) + 1);
            //    if (!File.Exists(fpath)
            //        || cs_md5file(fpath).ToLower() != md5value.ToLower())
            //    {
            //        DownloadFile(fname);
            //    }
            //    else
            //    {
            //        this.Dispatcher.BeginInvoke(new Action(() =>
            //        {
            //            FileInfo fileInfo = new FileInfo(fpath);
            //            tbProgress.Text = string.Format("文件:{0}, {1}/{2}"
            //                                       , fname.Substring(fname.LastIndexOfAny(new char[] { '\\', '/' }) + 1)
            //                                       , fileInfo.Length
            //                                       , fileInfo.Length);
            //            pbProgress.Value = 100.0;
            //            m_logger.Debug("文件:{0}校验完毕，不用下载", fpath);
            //        }));
            //    }

            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        tbTotalProgress.Text = string.Format("进度:{0}/{1}"
            //            , count, allcount);
            //        pbTotalProgress.Value = ((double)count) / allcount * 100.0;
            //    }));
            //}
        }

        void OnCheckAppVersion(IAsyncResult ar)
        {
            
        }
    }
}
