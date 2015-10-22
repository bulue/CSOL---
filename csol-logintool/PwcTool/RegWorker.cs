using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CommonQ;
using Newtonsoft.Json.Linq;
using Noesis.Javascript;

namespace PwcTool
{
    class RegWorker
    {
        const string reg_url = "http://member.tiancity.com/Registration/Accountreg.aspx?";
        const string captcha_url = "http://captcha.tiancity.com/getimage.ashx?fid=400&tid=";
        const string regcheck_url = "http://member.tiancity.com/Handler/Reg/NewCommonRegChkHandler.ashx?";

        CLogger m_logger;

        string _account;
        string _pwd;
        string _idcard;
        string _idname;
        bool _isWorking;
        string _exp;
        string _key;
        string _captcha_tid;
        string _captcha_code;
        public int _iptoken = 0;
        static Random _random = new Random(System.Environment.TickCount % 0x99a);
        public Dictionary<string, string> m_lgcaptcha;
        public Dictionary<string, string> m_pwcaptcha;
        static JavascriptContext m_JsContext = new JavascriptContext();
        static bool initJavascript = false;
        public event Action<RegWorker, Tuple<string,string,string,string,string>> FinishTask;

        const string md5js = "cstc.js";
        const string MSG_USERID_CHECK = "请检查您输入的账号是否正确";
        const string MSG_USERID_NULL = "请输入您的账号";
        const string MSG_USERID_NORMAL = "账号以字母开头,由小写英文字母和数字组成的4-16位字符";
        const string MSG_USERID_EMAIL_NORMAL = "请输入正确的邮箱";
        const string MSG_USERID_PHONE_NORMAL = "请输入正确的手机";
        const string MSG_RIGHT = "输入正确";
        const string MSG_USERID_OK = "您可以使用当前账号";
        const string MSG_USERID_EXIST = "该账号已经存在，请重新输入";
        const string MSG_USERID_BAN = "该账号已被禁止注册，请重新输入";
        const string MSG_CHECK_USER_ID_ERROR = "您短时间内尝试次数过多";
        const string MSG_PHONE_USER_ERROR = "该账号已被作为安全手机绑定，请尝试其他账号注册";
        const string MSG_EMAIL_USER_ERROR = "该账号已被作为安全邮箱绑定，请尝试其他账号注册";
        const string MSG_PASSWORD_EMPTY = "请先输入密码";
        const string MSG_PASSWORD_NORMAL = "密码为数字字母组合，6-20位，常用符号如：!@#$%^&*()_+=-";
        const string MSG_PASSWORD_NOT_MATCH = "两次密码输入不一致，请重新输入";
        const string MSG_PASSWORD_MATCH = "两次密码输入一致";
        const string MSG_PASSWORD_REGHT = "密码格式正确";
        const string MSG_REAL_NAME_NORMAL = "请填写您的真实姓名，以保障您的用户权益";

        public RegWorker(Dictionary<string, string> lg, Dictionary<string, string> pw)
        {
            m_lgcaptcha = new Dictionary<string, string>(lg);
            m_pwcaptcha = new Dictionary<string, string>(pw);
            m_logger = CLogger.FromFolder("reg/log");
            if (!initJavascript)
            {
                m_JsContext.Run(File.ReadAllText(md5js));
                initJavascript = true;
            }
            _isWorking = false;
        }

        public bool IsWorking
        {
            get { return _isWorking; }
            set { _isWorking = value; }
        }

        public int IpToken
        {
            get { return _iptoken; }
            set { _iptoken = value; }
        }

        public void Start(string account, string pwd, string idcard, string idname)
        {
            if (_isWorking)
            {
                return;
            }
            _isWorking = true;
            _account = account;
            _pwd = pwd;
            _idcard = idcard;
            _idname = idname;

            try
            {
                string url = regcheck_url + "userid=" + account + "&rnd=" + _random.NextDouble();
                HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                request.ServicePoint.Expect100Continue = false;
                request.Timeout = 1000 * 60;
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = new CookieContainer();
                request.BeginGetResponse(new AsyncCallback(OnCheckUidCallBack), request);
            }
            catch(Exception ex)
            {
                m_logger.Error(ex.ToString());
                TaskFinishInvoke("未知异常");
            }
        }


        void OnCheckUidCallBack(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                Stream stm = response.GetResponseStream();
                StreamReader reader = new StreamReader(stm, Encoding.GetEncoding("GB2312"));
                string retStr = reader.ReadToEnd();
                stm.Close();
                response.Close();
                if (retStr.Length > 0)
                {
                    string[] res = retStr.Split(',');
                    string error_msg = "ok";
                    switch (res[0])
                    {
                        case "1000":
                            {
                                if (res[1] == "1")
                                {
                                    _exp = res[2];
                                    _key = res[3];
                                }
                            } break;
                        case "10101":
                            {
                                error_msg = MSG_USERID_NORMAL;
                            }break;
                        case "10102":
                            {
                                error_msg = MSG_USERID_PHONE_NORMAL;
                            }break;
                        case "10103":
                            {
                                error_msg = MSG_USERID_EMAIL_NORMAL;
                            }break;
                        case "1020":
                            {
                                error_msg = MSG_USERID_EXIST;
                            }break;
                        case "1030":
                            {
                                error_msg = MSG_USERID_BAN;
                            }break;
                        case "1060":
                            {
                                error_msg = MSG_PHONE_USER_ERROR;
                            }break;
                        case "1070":
                            {
                                error_msg = MSG_EMAIL_USER_ERROR;
                            }break;
                        default:
                            {
                                error_msg = "Error:" + res[0];
                            }break;
                    }

                    if (error_msg == "ok")
                    {
                        _captcha_tid = Guid.NewGuid().ToString().Replace("-", "");
                        string url = captcha_url + _captcha_tid;
                        HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                        next_request.ServicePoint.Expect100Continue = false;
                        next_request.Timeout = 1000 * 60;
                        next_request.ContentType = "application/x-www-form-urlencoded";
                        next_request.CookieContainer = new CookieContainer();
                        next_request.BeginGetResponse(new AsyncCallback(OnGetCaptchaCallBack), next_request);
                    }
                    else
                    {
                        TaskFinishInvoke(error_msg);
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex.ToString());
                TaskFinishInvoke("未知异常");
            }
        }

        void OnGetCaptchaCallBack(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                Stream stm = response.GetResponseStream();
                byte[] img_buf = new byte[1024 * 4];
                int offset = 0;
                int readbytes = 0;
                while ((readbytes = stm.Read(img_buf, offset, img_buf.Length - offset)) > 0)
                {
                    offset += readbytes;
                }
                stm.Close();
                response.Close();

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5_bytes = md5.ComputeHash(img_buf, 0, offset);
                string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
                string old_md5_str = md5_str;
                md5_str = cs_md5(md5_str + GuessWorker.Uid);
                md5_str = cs_md5(md5_str + GuessWorker.Matchinfo + old_md5_str);
                md5_str = cs_md5(md5_str + GuessWorker.DBpwd + old_md5_str + "c");
                if (m_pwcaptcha.ContainsKey(md5_str))
                {
                    _captcha_code = m_pwcaptcha[md5_str];

                    string rsapwd;
                    string rsapwd2;
                    lock (m_JsContext)
                    {
                        m_JsContext.SetParameter("pwd", _pwd);
                        m_JsContext.SetParameter("pwd2", _pwd);
                        m_JsContext.SetParameter("exp", _exp);
                        m_JsContext.SetParameter("keya", _key);
                        m_JsContext.Run("MyEncry(pwd,pwd2,exp,keya)");
                        rsapwd = (string)m_JsContext.GetParameter("rsapwd");
                        rsapwd2 = (string)m_JsContext.GetParameter("rsapwd2");
                    }

                    Dictionary<string, string> opt = new Dictionary<string, string>();
                    opt.Add("op", "1");
                    opt.Add("ud", _account);
                    opt.Add("pw", rsapwd);
                    opt.Add("pw2", rsapwd2);
                    opt.Add("sp", "");
                    opt.Add("sp2", "");
                    opt.Add("um", UrlFunction.Escape(_idname));
                    opt.Add("cn", _idcard);
                    opt.Add("ex", "");
                    opt.Add("et", "1");
                    opt.Add("fd", "");
                    opt.Add("fm", "");
                    opt.Add("ia", "true");
                    opt.Add("rc", _captcha_code);
                    opt.Add("td", _captcha_tid);
                    opt.Add("fu", "");
                    opt.Add("if", "0");
                    StringBuilder sb = new StringBuilder();
                    foreach (string key in opt.Keys)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("&");
                        }
                        sb.Append(key);
                        sb.Append("=");
                        sb.Append(opt[key]);
                    }

                    string url = "http://member.tiancity.com/Handler/RegisterHandler.aspx";
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    Byte[] post_buf = encoding.GetBytes(sb.ToString());
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.Method = "POST";
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = new CookieContainer();
                    next_request.ContentLength = sb.Length;
                    Stream post_stm = next_request.GetRequestStream();
                    post_stm.Write(post_buf, 0, post_buf.Length);
                    post_stm.Close();
                    next_request.BeginGetResponse(new AsyncCallback(OnGetRegResult), next_request);
                }
                else
                {
                    _captcha_tid = Guid.NewGuid().ToString().Replace("-", "");
                    string url = captcha_url + _captcha_tid;
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = new CookieContainer();
                    next_request.BeginGetResponse(new AsyncCallback(OnGetCaptchaCallBack), next_request);
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex.ToString());
                TaskFinishInvoke("未知异常");
            }
        }

        void OnGetRegResult(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                Stream stm = response.GetResponseStream();
                StreamReader reader = new StreamReader(stm, Encoding.GetEncoding("GB2312"));
                string retStr = reader.ReadToEnd();
                reader.Close();
                stm.Close();
                response.Close();
                JObject jsObj = JObject.Parse(retStr);
                if (jsObj["IsSuccess"].ToString() == "1")
                {
                    TaskFinishInvoke("OK");
                }
                else if (jsObj["ReturnString"].ToString() == "验证码输入错误")
                {
                    _captcha_tid = Guid.NewGuid().ToString().Replace("-", "");
                    string url = captcha_url + _captcha_tid;
                    HttpWebRequest next_request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                    next_request.ServicePoint.Expect100Continue = false;
                    next_request.Timeout = 1000 * 60;
                    next_request.ContentType = "application/x-www-form-urlencoded";
                    next_request.CookieContainer = new CookieContainer();
                    next_request.BeginGetResponse(new AsyncCallback(OnGetCaptchaCallBack), next_request);
                }
                else
                {
                    TaskFinishInvoke(jsObj["ReturnString"].ToString());
                }
            }
            catch (Exception ex)
            {
                m_logger.Error(ex.ToString());
                TaskFinishInvoke("未知异常");
            }
        }

        public void TaskFinishInvoke(string ret)
        {
            if (FinishTask != null)
            {
                FinishTask.Invoke(this, new Tuple<string, string, string, string, string>(_account, _pwd, ret, _idcard, _idname));
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
