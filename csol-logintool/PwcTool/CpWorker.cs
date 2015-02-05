﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Noesis.Javascript;
using CommonQ;

namespace PwcTool
{
    class CpWorker
    {
        string url_qrcode = "https://passport.tiancity.com/handler/getqrcodekey.ashx?";
        string url_capcheck = "http://captcha.tiancity.com/CheckSwitch.ashx?jsoncallback=j";
        string url_capimage = "http://captcha.tiancity.com/client.ashx?fid=104&code=1";
        string url_matcheck = "https://passport.tiancity.com/handler/GetMatrix.ashx?jsoncallback=?";
        string url_matsms = "https://passport.tiancity.com/handler/GetSmsMatrix.ashx?jsoncallback=?";
        string url_dpwd = "https://passport.tiancity.com/handler/GetSmsDynamic.ashx?jsoncallback=?";
        string url_log = "https://passport.tiancity.com/Login.ashx?jsoncallback=?";
        string url_pmsg = "https://passport.tiancity.com/handler/PushLoginMsg.ashx?jsoncallback=?";
        string url_qrlog = "https://passport.tiancity.com/handler/QrCodeKeyLogin.ashx?jsoncallback=?";
        string url_aqjf = "http://aq.tiancity.com/Protect/ReopenedAccount";

        Random m_rgen = new Random(System.Environment.TickCount);
        JavascriptContext m_JsContext = new JavascriptContext();
        CLogger m_logger;

        public Dictionary<string, string> m_lgcaptcha;
        public Dictionary<string, string> m_pwcaptcha;
        public Queue<Tuple<string, string>> m_uidqueue;

        public event Action<CpWorker,string, string, string, string> FinishTask;

        const string md5js = "md5.js";

        public CpWorker(Dictionary<string, string> lg,Dictionary<string, string> pw)
        {
            m_lgcaptcha = new Dictionary<string, string>(lg);
            m_pwcaptcha = new Dictionary<string, string>(pw);

            m_logger = CLogger.FromFolder("pwclog");
            m_JsContext.Run(File.ReadAllText(md5js));
        }

        public void TryTaskChangePwd(string uid, string pwd,string newpwd)
        {
            
            m_logger.Debug("修改 uid:" + uid);
            string url = "http://passport.tiancity.com/login/login.aspx";

            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 60;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_1), new Tuple<HttpWebRequest, string, string, string>(request, uid, pwd, newpwd));
        }

        void OnTryLoginCallBack_1(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                HttpWebRequest next_request = System.Net.WebRequest.Create(url_capcheck + "&fid=104&uid=" + uid) as HttpWebRequest;
                next_request.ServicePoint.Expect100Continue = false;
                next_request.Timeout = 1000 * 60;
                next_request.ContentType = "application/x-www-form-urlencoded";
                next_request.CookieContainer = request.CookieContainer;
                next_request.CookieContainer.Add(response.Cookies);
                next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_2), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_2(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;

                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string s = reader.ReadToEnd();

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
                        next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_3), new Tuple<HttpWebRequest, string, string, string, string>(next_request, uid, pwd, newpwd, captoken));
                    }
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_3(IAsyncResult ar)
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
                int bytecount = 0;
                int readcount = 0;
                byte[] img_buf = new byte[1024 * 10];
                while ((readcount = s.Read(img_buf, bytecount, img_buf.Length - bytecount)) > 0)
                {
                    bytecount += readcount;
                }

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5_bytes = md5.ComputeHash(img_buf, 0, bytecount);
                string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");

                if (m_lgcaptcha.ContainsKey(md5_str))
                {
                    string cap = m_lgcaptcha[md5_str];
                    m_logger.Debug("发现 name:" + md5_str + " value:" + cap);

                    HttpWebRequest next_request = System.Net.WebRequest.Create(url_matcheck + "&id=" + uid) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = request.CookieContainer;
                    next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_4), new Tuple<HttpWebRequest, string, string, string, string, string>(next_request, uid, pwd, newpwd, captoken, cap));
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
                    next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_3), new Tuple<HttpWebRequest, string, string, string, string>(next_request, uid, pwd, newpwd, newcaptoken));
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_4(IAsyncResult ar)
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

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string s = reader.ReadToEnd();

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

                    string ei = "id=" + uid + "&pw=" + pwd + "&mt=" + mt + "&lt=" + 0;
                    m_JsContext.SetParameter("ei", ei);
                    m_JsContext.SetParameter("pk", pk);
                    m_JsContext.SetParameter("pm", pm);
                    FCQ = (string)m_JsContext.Run("JiaMi(ei,pk,pm)");

                    m_JsContext.SetParameter("cp", cp);
                    cp = (string)m_JsContext.Run("encodeURIComponent(cp)");
                    string url = url_log + "&FCQ=" + FCQ + "&cp=" + cp + "&fl=" + fl + "&st=" + st;

                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = request.CookieContainer;
                    next_request.BeginGetResponse(new AsyncCallback(OnTryLoginCallBack_5), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryLoginCallBack_5(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string s = reader.ReadToEnd();

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

                        string url = "http://aq.tiancity.com/Home/GetCaptchaUrl";
                        HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                        next_request.ServicePoint.Expect100Continue = false;
                        next_request.Timeout = 1000 * 60;
                        next_request.Method = "POST";
                        next_request.ContentLength = 0;
                        next_request.ContentType = "application/x-www-form-urlencoded";
                        next_request.CookieContainer = new CookieContainer();
                        next_request.CookieContainer.Add(response.Cookies);
                        next_request.BeginGetResponse(new AsyncCallback(OnTryChangePwd_1), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                    }
                    else if (logRet == "7")
                    {
                        m_logger.Debug("ip被封");
                    }
                    else
                    {
                        m_logger.Info("遇到错误码:" + logRet);
                    }
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryChangePwd_1(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                JObject jsObj = JObject.Parse(reader.ReadToEnd());
                string url = jsObj["Url"].ToString() + "&r=" + m_rgen.NextDouble() * 99999; ;

                HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                next_request.ServicePoint.Expect100Continue = false;
                next_request.Timeout = 1000 * 60;
                next_request.ContentType = "application/x-www-form-urlencoded";
                next_request.CookieContainer = request.CookieContainer;
                next_request.CookieContainer.Add(response.Cookies);
                next_request.BeginGetResponse(new AsyncCallback(OnTryChangePwd_2), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryChangePwd_2(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                Stream s = response.GetResponseStream();
                int bytecount = 0;
                int readcount = 0;
                byte[] img_buf = new byte[1024 * 10];
                while ((readcount = s.Read(img_buf, bytecount, img_buf.Length - bytecount)) > 0)
                {
                    bytecount += readcount;
                }

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5_bytes = md5.ComputeHash(img_buf, 0, bytecount);
                string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");

                if (m_pwcaptcha.ContainsKey(md5_str))
                {
                    string cap = m_pwcaptcha[md5_str];
                    m_logger.Debug("pwcaptcha 发现 name:" + md5_str + " value:" + cap);

                    string post = "old_pwd=" + Encrypt(pwd) + "&new_pwd1=" + Encrypt(newpwd) + "&new_pwd2=" + Encrypt(newpwd) + "&captcha=" + cap;
                    byte[] post_buffer = System.Text.Encoding.UTF8.GetBytes(post);

                    string url = "http://aq.tiancity.com/Pwd/PostPwdModify";
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.Method = "POST";
                    next_request.ContentLength = post_buffer.Length;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = request.CookieContainer;
                    next_request.CookieContainer.Add(response.Cookies);

                    Stream postStream = next_request.GetRequestStream();
                    postStream.Write(post_buffer, 0, post_buffer.Length);
                    postStream.Close();
                    next_request.BeginGetResponse(new AsyncCallback(OnTryChangePwd_3), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                }
                else
                {
                    string url = "http://aq.tiancity.com/Home/GetCaptchaUrl";
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.Method = "POST";
                    next_request.ContentLength = 0;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = request.CookieContainer;
                    next_request.CookieContainer.Add(response.Cookies);
                    next_request.BeginGetResponse(new AsyncCallback(OnTryChangePwd_1), new Tuple<HttpWebRequest, string, string, string>(next_request, uid, pwd, newpwd));
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        void OnTryChangePwd_3(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<HttpWebRequest, string, string, string>;
                HttpWebRequest request = tuple.Item1;
                string uid = tuple.Item2;
                string pwd = tuple.Item3;
                string newpwd = tuple.Item4;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string s = reader.ReadToEnd();
                m_logger.Debug(s);

                if (FinishTask != null)
                {
                    FinishTask.Invoke(this, uid, pwd, newpwd, s);
                }
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }

        public string Encrypt(string strPwd)
        {
            m_JsContext.SetParameter("src", strPwd);
            return (string)m_JsContext.Run("encry(src)");
        }

        private void AutoLogin(string uid, string pwd)
        {
            try
            {
                byte[] img_buf = new byte[1024 * 10];
                int bytecount = 0;
                int readcount = 0;
                string md5_string = "";

                CookieContainer SessionCookies = new CookieContainer();
                do
                {
                    string url = "http://passport.tiancity.com/login/login.aspx";

                    HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 1000 * 60;
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.CookieContainer = SessionCookies;
                    HttpWebResponse myResponse = (HttpWebResponse)webRequest.GetResponse();
                    CookieCollection cookies = myResponse.Cookies;
                    SessionCookies.Add(cookies);
                    //StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                    //MessageBox.Show(reader.ReadToEnd());
                    //reader.Close();

                } while (false);

                string R = "";
                do
                {
                    HttpWebRequest webRequest = System.Net.WebRequest.Create(url_capcheck + "&fid=104&uid=" + uid) as HttpWebRequest;
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 1000 * 60;
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.CookieContainer = SessionCookies;
                    HttpWebResponse myResponse = (HttpWebResponse)webRequest.GetResponse();
                    SessionCookies.Add(myResponse.Cookies);
                    StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                    string s = reader.ReadToEnd();

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match r = Regex.Match(s, pattern);
                    if (r.Groups.Count == 2)
                    {
                        string recvJsonStr = r.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        R = jsObj["R"].ToString();
                    }
                } while (false);

                string CapToken = "";
                string cap = "";

                do
                {
                    if (R == "1")
                    {
                        while (true)
                        {
                            bytecount = 0;
                            readcount = 0;

                            //CapToken = (string)m_JsContext.Run("Guid()");
                            CapToken = Guid.NewGuid().ToString().Replace("-", "");
                            string image_url = url_capimage + "&uid=" + uid + "&tid=" + CapToken + "&rnd=" + m_rgen.NextDouble() * 99999;

                            HttpWebRequest webRequest = System.Net.WebRequest.Create(image_url) as HttpWebRequest;
                            webRequest.ServicePoint.Expect100Continue = false;
                            webRequest.Timeout = 1000 * 60;
                            webRequest.ContentType = "application/x-www-form-urlencoded";
                            webRequest.CookieContainer = SessionCookies;
                            Stream s = webRequest.GetResponse().GetResponseStream();
                            //byte[] img_buf = new byte[1024 * 160];
                            while ((readcount = s.Read(img_buf, bytecount, img_buf.Length - bytecount)) > 0)
                            {
                                bytecount += readcount;
                            }

                            MD5 md5 = new MD5CryptoServiceProvider();
                            byte[] md5_ret = md5.ComputeHash(img_buf, 0, bytecount);
                            md5_string = BitConverter.ToString(md5_ret).Replace("-", "");

                            if (m_lgcaptcha.ContainsKey(md5_string))
                            {
                                cap = m_lgcaptcha[md5_string];
                                m_logger.Debug("发现key:" + md5_string + "value:" + cap);
                                break;
                            }
                        }
                    }
                } while (false);

                string FCQ, cp, st, fl;
                st = "281";
                fl = "";
                cp = cap + "#" + CapToken;
                FCQ = "";
                do
                {
                    HttpWebRequest webRequest = System.Net.WebRequest.Create(url_matcheck + "&id=" + uid) as HttpWebRequest;
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 1000 * 60;
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.CookieContainer = SessionCookies;
                    StreamReader reader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                    string data = reader.ReadToEnd();

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match r = Regex.Match(data, pattern);
                    if (r.Groups.Count == 2)
                    {
                        string recvJsonStr = r.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        string code = jsObj["code"].ToString();
                        string mt = jsObj["mt"].ToString();
                        string pm = jsObj["pm"].ToString();
                        string pk = jsObj["pk"].ToString();
                        string rsa = jsObj["rsa"].ToString();

                        string ei = "id=" + uid + "&pw=" + pwd + "&mt=" + mt + "&lt=" + 0;
                        m_JsContext.SetParameter("ei", ei);
                        m_JsContext.SetParameter("pk", pk);
                        m_JsContext.SetParameter("pm", pm);
                        FCQ = (string)m_JsContext.Run("JiaMi(ei,pk,pm)");
                    }
                } while (false);

                do
                {
                    m_JsContext.SetParameter("cp", cp);
                    cp = (string)m_JsContext.Run("encodeURIComponent(cp)");
                    string url = url_log + "&FCQ=" + FCQ + "&cp=" + cp + "&fl=" + fl + "&st=" + st;

                    HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 1000 * 60;
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.CookieContainer = SessionCookies;

                    StreamReader reader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                    string data = reader.ReadToEnd();

                    //MessageBox.Show(data);

                    string pattern = @"[^\(\)]+\(([^\(\)]+)\)";
                    Match r = Regex.Match(data, pattern);
                    if (r.Groups.Count == 2)
                    {
                        string recvJsonStr = r.Groups[1].ToString();
                        JObject jsObj = JObject.Parse(recvJsonStr);
                        string logRet = jsObj["code"].ToString();

                        if (logRet == "1")
                        {
                            m_logger.Debug("登录OK");

                        }
                        else if (logRet == "7")
                        {
                            m_logger.Debug("E7状态,成功捕获! ");
                        }
                        else
                        {
                            m_logger.Info("遇到错误码:" + logRet);
                        }
                    }
                } while (false);
            }
            catch (System.Exception ex)
            {
                m_logger.Error(ex.ToString());
            }
        }
    }
}
