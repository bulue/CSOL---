using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noesis.Javascript;
using CommonQ;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace PwcTool
{
    class SeWorker
    {

        const string url_qrcode = "https://passport.tiancity.com/handler/getqrcodekey.ashx?";
        const string url_capcheck = "http://captcha.tiancity.com/CheckSwitch.ashx?jsoncallback=j";
        const string url_capimage = "http://captcha.tiancity.com/client.ashx?fid=104&code=1";
        const string url_matcheck = "https://passport.tiancity.com/handler/GetMatrix.ashx?jsoncallback=?";
        const string url_matsms = "https://passport.tiancity.com/handler/GetSmsMatrix.ashx?jsoncallback=?";
        const string url_dpwd = "https://passport.tiancity.com/handler/GetSmsDynamic.ashx?jsoncallback=?";
        const string url_log = "https://passport.tiancity.com/Login.ashx?jsoncallback=?";
        const string url_pmsg = "https://passport.tiancity.com/handler/PushLoginMsg.ashx?jsoncallback=?";
        const string url_qrlog = "https://passport.tiancity.com/handler/QrCodeKeyLogin.ashx?jsoncallback=?";
        const string url_aqjf = "http://aq.tiancity.com/Protect/ReopenedAccount";

        static Random m_rgen = new Random(System.Environment.TickCount);
        static JavascriptContext m_JsContext = new JavascriptContext();
        static bool initJavascript = false;
        CLogger m_logger;

        public static string Uid = "";
        public static string Matchinfo = "";
        public static string DBpwd = "";
        public static int KeepRunTime = 0;
        public static int StartRunTime = 0;

        public Dictionary<string, string> m_lgcaptcha;
        public Dictionary<string, string> m_pwcaptcha;

        public event Action<SeWorker,string, string, string, int, int, int> FinishTask;

        const string md5js = "cstc.js";
        string m_safekey = "";

        public bool IsWorking = false;
        public object workArgument;
        public int IpToken = 0;
        public bool QuerySafePoint = false;

        public SeWorker(Dictionary<string, string> lg, Dictionary<string, string> pw, string safekey)
        {
            m_lgcaptcha = new Dictionary<string, string>(lg);
            m_pwcaptcha = new Dictionary<string, string>(pw);

            m_safekey = safekey;
            m_logger = CLogger.FromFolder("pwclog");
            if (!initJavascript)
            {
                m_JsContext.Run(File.ReadAllText(md5js));
                initJavascript = true;
            }
        }

        public void BeginTask(string uid, string pwd, object obj, int iptoken)
        {
            IsWorking = true;
            workArgument = obj;
            IpToken = iptoken;
            try
            {
                m_logger.Debug("====Begin uid:" + uid + "this:" + this.GetHashCode());
                string url = "http://passport.tiancity.com/login/login.aspx";

                HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                request.ServicePoint.Expect100Continue = false;
                request.Timeout = 1000 * 60;
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = new CookieContainer();
                request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_GetCookies), new Tuple<HttpWebRequest, string, string, string>(request, uid, pwd,""));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_GetCookies(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                CookieCollection cookies = response.Cookies;
                response.Close();

                HttpWebRequest next_request = System.Net.WebRequest.Create(url_capcheck + "&fid=104&uid=" + uid) as HttpWebRequest;
                next_request.ServicePoint.Expect100Continue = false;
                next_request.Timeout = 1000 * 60;
                next_request.ContentType = "application/x-www-form-urlencoded";
                next_request.CookieContainer = new CookieContainer(); ;
                next_request.CookieContainer.Add(cookies);
                next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_CheckCaptcha), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_CheckCaptcha(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                AsyncRead(response.GetResponseStream(), new Action<byte[]>((buffer) =>
                {
                    string s = System.Text.Encoding.UTF8.GetString(buffer);
                    response.Close();

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match mt = Regex.Match(s, pattern);
                    if (mt.Groups.Count == 2)
                    {
                        string recvJsonStr = mt.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        string r = jsObj["R"].ToString();

                        if (r == "1")
                        {
                            string captoken = Guid.NewGuid().ToString().Replace("-", "");
                            string image_url = url_capimage + "&uid=" + uid + "&tid=" + captoken + "&rnd=" + m_rgen.NextDouble() * 99999;
                            HttpWebRequest next_request = System.Net.WebRequest.Create(image_url) as HttpWebRequest;
                            next_request.ServicePoint.Expect100Continue = false;
                            next_request.Timeout = 1000 * 60;
                            next_request.ContentType = "application/x-www-form-urlencoded";
                            next_request.CookieContainer = request.CookieContainer;
                            next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_GetCaptcha), new Tuple<HttpWebRequest, string, string, string, string>(next_request, uid, pwd, newpwd, captoken));
                        }
                    }
                }));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void AsyncRead(Stream stream, Action<byte[]> callback)
        {
            byte[] totalbuffer = null;
            byte[] readbuffer = new byte[1024];
            stream.BeginRead(readbuffer, 0, readbuffer.Length, new AsyncCallback(OnAsyncReadCallBack), new Tuple<Stream, byte[], byte[], Action<byte[]>>(
                stream, readbuffer, totalbuffer, callback));
        }

        void OnAsyncReadCallBack(IAsyncResult ar)
        {
            var tuple = ar.AsyncState as Tuple<Stream, byte[], byte[], Action<byte[]>>;
            Stream s = tuple.Item1;
            byte[] readbuffer = tuple.Item2;
            byte[] totalbuffer = tuple.Item3;
            Action<byte[]> callback = tuple.Item4;
            

            int recv = s.EndRead(ar);
            if (recv > 0)
            {
                if (totalbuffer == null)
                {
                    totalbuffer = new byte[recv];
                    Array.Copy(readbuffer, totalbuffer, recv);
                }
                else
                {
                    byte[] tmpbuffer = new byte[recv + totalbuffer.Length];
                    Array.Copy(totalbuffer, 0, tmpbuffer, 0, totalbuffer.Length);
                    Array.Copy(readbuffer, 0, tmpbuffer, totalbuffer.Length, recv);
                    totalbuffer = tmpbuffer;
                }

                Array.Clear(readbuffer, 0, readbuffer.Length);
                s.BeginRead(readbuffer, 0, readbuffer.Length, new AsyncCallback(OnAsyncReadCallBack), new Tuple<Stream, byte[], byte[], Action<byte[]>>(
                    s,readbuffer,totalbuffer,callback));
            }
            else
            {
                s.Close();
                callback.Invoke(totalbuffer);
            }
        }

        void OnTryLoginCallBack_GetCaptcha(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                string captoken = tuple.Item5;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                Stream s = response.GetResponseStream();
                byte[] img_buf = new byte[1024 * 10];

                s.BeginRead(img_buf, 0, img_buf.Length, new AsyncCallback(OnReadLoginCaptcha), new Tuple<Tuple<HttpWebRequest, string, string, string, string>, byte[],int,Stream >(
                    tuple, img_buf, 0, s));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnReadLoginCaptcha(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<Tuple<HttpWebRequest, string, string, string, string>, byte[], int, Stream>;

                byte[] img_buf = tuple.Item2;
                int readbytes = tuple.Item3;
                Stream s = tuple.Item4;

                int nrecv = s.EndRead(ar);
                if (nrecv > 0)
                {
                    int offset = nrecv + readbytes;
                    s.BeginRead(img_buf, offset, img_buf.Length - offset, new AsyncCallback(OnReadLoginCaptcha), new Tuple<Tuple<HttpWebRequest, string, string, string, string>, byte[], int, Stream>(
                        tuple.Item1,img_buf,offset,s));
                }
                else
                {
                    s.Close();

                    HttpWebRequest request = tuple.Item1.Item1;
                    string uid = tuple.Item1.Item2;
                    string pwd = tuple.Item1.Item3;
                    string newpwd = tuple.Item1.Item4;
                    string captoken = tuple.Item1.Item5;

                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] md5_bytes = md5.ComputeHash(img_buf, 0, readbytes);
                    string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
                    string old_md5_str = md5_str;
                    md5_str = cs_md5(md5_str + Uid);
                    md5_str = cs_md5(md5_str + Matchinfo + old_md5_str);
                    md5_str = cs_md5(md5_str + DBpwd + old_md5_str);
                    if (m_lgcaptcha.ContainsKey(md5_str))
                    {
                        string cap = m_lgcaptcha[md5_str];

                        HttpWebRequest next_request = System.Net.WebRequest.Create(url_matcheck + "&id=" + uid) as HttpWebRequest;
                        next_request.ServicePoint.Expect100Continue = false;
                        next_request.Timeout = 1000 * 60;
                        next_request.ContentType = "application/x-www-form-urlencoded";
                        next_request.CookieContainer = request.CookieContainer;
                        next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_RsaKey), new Tuple<HttpWebRequest, string, string, string, string, string>(next_request, uid, pwd, newpwd, captoken, cap));
                    }
                    else
                    {
                        string newcaptoken = Guid.NewGuid().ToString().Replace("-", "");
                        string image_url = url_capimage + "&uid=" + uid + "&tid=" + newcaptoken + "&rnd=" + m_rgen.NextDouble() * 99999;
                        HttpWebRequest next_request = System.Net.WebRequest.Create(image_url) as HttpWebRequest;
                        next_request.ServicePoint.Expect100Continue = false;
                        next_request.Timeout = 1000 * 60;
                        next_request.ContentType = "application/x-www-form-urlencoded";
                        next_request.CookieContainer = request.CookieContainer;
                        next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_GetCaptcha), new Tuple<HttpWebRequest, string, string, string, string>(next_request, uid, pwd, newpwd, newcaptoken));
                    }
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_RsaKey(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                string captoken = tuple.Item5;
                string cap = tuple.Item6;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                AsyncRead(response.GetResponseStream(), new Action<byte[]>((buffer) =>{
                    string s = System.Text.Encoding.UTF8.GetString(buffer);
                    response.Close();

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match r = Regex.Match(s, pattern);
                    if (r.Groups.Count == 2)
                    {
                        string st = "281";
                        string fl = "";
                        string cp = cap + "#" + captoken;
                        string FCQ = "";

                        string recvJsonStr = r.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        string code = jsObj["code"].ToString();
                        string mt = jsObj["mt"].ToString();
                        string pm = jsObj["pm"].ToString();
                        string pk = jsObj["pk"].ToString();
                        string rsa = jsObj["rsa"].ToString();

                        if (code == "199" && rsa == "True")
                        {
                            string ei = "id=" + uid + "&pw=" + pwd + "&mt=" + mt + "&lt=" + 0;
                            lock (m_JsContext)
                            {
                                m_JsContext.SetParameter("ei", ei);
                                m_JsContext.SetParameter("pk", pk);
                                m_JsContext.SetParameter("pm", pm);
                                FCQ = (string)m_JsContext.Run("JiaMi(ei,pk,pm)");

                                m_JsContext.SetParameter("cp", cp);
                                cp = (string)m_JsContext.Run("encodeURIComponent(cp)");
                            }
                            string url = url_log + "&FCQ=" + FCQ + "&cp=" + cp + "&fl=" + fl + "&st=" + st;

                            HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                            next_request.ServicePoint.Expect100Continue = false;
                            next_request.Timeout = 1000 * 60;
                            next_request.ContentType = "application/x-www-form-urlencoded";
                            next_request.CookieContainer = request.CookieContainer;
                            next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_LoginRet), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                        }
                        else
                        {
                            TaskFinishInvoke(this, uid, pwd, "RsaKeyError:" + code);
                        }
                    }
                }));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_LoginRet(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                AsyncRead(response.GetResponseStream(), new Action<byte[]>((buffer) =>
                {
                    string s = System.Text.Encoding.UTF8.GetString(buffer);
                    response.Close();

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match r = Regex.Match(s, pattern);
                    if (r.Groups.Count == 2)
                    {
                        string recvJsonStr = r.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        string logRet = jsObj["code"].ToString();

                        if (logRet == "1")
                        {
                            m_logger.Debug("登录OK");

                            string url = "http://member.tiancity.com/User/IdInfoModify.aspx";
                            HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                            next_request.ServicePoint.Expect100Continue = false;
                            next_request.Timeout = 1000 * 60;
                            next_request.ContentType = "application/x-www-form-urlencoded";
                            next_request.CookieContainer = new CookieContainer();
                            next_request.CookieContainer.Add(response.Cookies);
                            next_request.BeginGetResponse(new AsyncCallback(OnTryGetIsIdCardSafe), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                        }
                        else if (logRet == "7")
                        {
                            m_logger.Debug("ip被封");
                            TaskFinishInvoke(this, uid, pwd, "IP被封");
                        }
                        else if (logRet == "4")
                        {
                            m_logger.Info("uid:" + uid + "密码错误!");
                            TaskFinishInvoke(this, uid, pwd, "密码错误");
                        }
                        else
                        {
                            m_logger.Info("uid:" + uid + " 登录错误:" + logRet);
                            TaskFinishInvoke(this, uid, pwd, "登录错误:" + logRet);
                        }
                    }
                }));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryGetIsIdCardSafe(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(),Encoding.GetEncoding("GB2312"));
                string s = reader.ReadToEnd();
                response.Close();
                reader.Close();
                int has_idcard = 0;

                int bt = System.Environment.TickCount;

                string pattern = "[\\s\\S]*?真实姓名[\\s\\S]*?value=\"([^\"]*)\"";
                Match mt = Regex.Match(s, pattern);
                if (mt.Groups.Count == 2)
                {
                    if (string.IsNullOrEmpty(mt.Groups[1].ToString()))
                    {
                        has_idcard = 1;
                    }
                    else
                    {
                        has_idcard = 2;
                    }
                }
                
                int et = System.Environment.TickCount;
                if (et - bt > 200){
                    m_logger.Debug("OnTryGetIsIdCardSafe 超时{0}", (et - bt));
                }

                //TaskFinishInvoke(this, uid, pwd, "查询成功", has_idcard, 0, 0);

                string url = "http://pay.tiancity.com/Wallet/UserService.aspx";
                HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                next_request.ServicePoint.Expect100Continue = false;
                next_request.Timeout = 1000 * 60;
                next_request.ContentType = "application/x-www-form-urlencoded";
                next_request.CookieContainer = request.CookieContainer;
                next_request.BeginGetResponse(new AsyncCallback(OnTryGetYuEScore), new Tuple<HttpWebRequest, string, string, int>(next_request, uid, pwd, has_idcard));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryGetYuEScore(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, int>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                int has_idcard = tuple.Item4;
                int YuE = -1;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                string s = reader.ReadToEnd();
                response.Close();
                reader.Close();

                int bt = System.Environment.TickCount;

                if (!String.IsNullOrEmpty(s))
                {
                    string pattern = "可用余额[^0-9]*?([0-9]+)点";
                    Match mt = Regex.Match(s, pattern);
                    if (mt.Groups.Count == 2)
                    {
                        YuE = int.Parse(mt.Groups[1].ToString());
                    }
                }

                int et = System.Environment.TickCount;
                if (et - bt > 200)
                {
                    m_logger.Debug("OnTryGetYuEScore 超时{0}", (et - bt));
                }

                if (!QuerySafePoint)
                {
                    TaskFinishInvoke(this, uid, pwd, "查询成功", has_idcard, YuE, -1);
                }
                else
                {
                    string url = "http://aq.tiancity.com/";
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = request.CookieContainer;
                    next_request.CookieContainer.Add(new Cookie("lx_user_guide", "1918551305", "/", "aq.tiancity.com"));
                    next_request.BeginGetResponse(new AsyncCallback(OnTryGetUserSafePoint), new Tuple<HttpWebRequest, string, string, int, int>(next_request, uid, pwd, has_idcard, YuE));
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryGetUserSafePoint(IAsyncResult ar)         //安全评分
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, int, int>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                int has_idcard = tuple.Item4;
                int YuE = tuple.Item5;
                int usersafepoint = -1;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string s = reader.ReadToEnd();
                response.Close();
                reader.Close();

                int bt = System.Environment.TickCount;

                if (!String.IsNullOrEmpty(s))
                {
                    string pattern = "账号安全评分：</strong><strong class=\"fred2 f18 fyah mr1\">[^0-9]*?([0-9]+)";
                    Match mt = Regex.Match(s, pattern);
                    if (mt.Groups.Count == 2)
                    {
                        usersafepoint = int.Parse(mt.Groups[1].ToString());
                    }
                }

                int et = System.Environment.TickCount;
                if (et - bt > 200)
                {
                    m_logger.Debug("OnTryGetUserSafePoint 超时{0}", (et - bt));
                }

                TaskFinishInvoke(this, uid, pwd, "查询成功", has_idcard, YuE, usersafepoint);
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }


        void TaskFinishInvoke(SeWorker worker, string uid, string pwd, string ret,int has_idcard = 2,int yue = 0, int safepoint = 0)
        {
            if (FinishTask != null)
            {
                FinishTask.Invoke(worker, uid, pwd, ret, has_idcard, yue, safepoint);
            }
        }

        public string Encrypt(string strPwd)
        {
            lock (m_JsContext)
            {
                m_JsContext.SetParameter("src", strPwd);
                return (string)m_JsContext.Run("encry(src)");
            }
        }

        public string cs_md5(string src)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] strbytes = Encoding.Default.GetBytes(src);
            byte[] md5_bytes = md5.ComputeHash(strbytes, 0, strbytes.Length);
            string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
            return md5_str;
        }
    }
}
