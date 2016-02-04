using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net;
using System.Xml;
using System.Security.Cryptography;
using CommonQ;
using System.Net.Sockets;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.OCR;
using System.Reflection;

namespace CSLogin
{
    public partial class csLoginTool : Form
    {

#region Member
        static private csLoginTool instance = null;
        private Thread _logicThread = null;
        public IniFile _iniFile = new IniFile(@".\config.ini");
        private LoginManage _loginManage = null;
        bool _bAutoStart = false;
        bool _bUpdate = true;
#endregion

#region API
        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        int Space = 32; //热键ID
        private const int WM_HOTKEY = 0x312; //窗口消息-热键
        private const int WM_CREATE = 0x1; //窗口消息-创建
        private const int WM_DESTROY = 0x2; //窗口消息-销毁
        private const int MOD_ALT = 0x1; //ALT
        private const int MOD_CONTROL = 0x2; //CTRL
        private const int MOD_SHIFT = 0x4; //SHIFT
        private const int VK_SPACE = 0x20; //SPACE
        private const int VK_F12 = 123;


        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="vk">热键</param>
        private void RegKey(IntPtr hwnd, int hotKey_id, int fsModifiers, int vk)
        {
            bool result;
            if (RegisterHotKey(hwnd, hotKey_id, fsModifiers, vk) == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            if (!result)
            {
                MessageBox.Show("注册热键失败！");
            }
        }
        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        private void UnRegKey(IntPtr hwnd, int hotKey_id)
        {
            UnregisterHotKey(hwnd, hotKey_id);
        }
#endregion
        public static string GamePath = "";
        static public Dictionary<string, string> Lg = new Dictionary<string, string>();
        Thread m_RoutieThread = null;

        void CheckVersion()
        {
            string url = "http://localhost/download/ver.php";
            HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 1000 * 60;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());
            string newver = reader.ReadToEnd();
            newver = newver.Replace(".","");
            int newverid = int.Parse(newver);
            int nowverid = Assembly.GetExecutingAssembly().GetName().Version.Major*100
                 + Assembly.GetExecutingAssembly().GetName().Version.Minor*10
                 + Assembly.GetExecutingAssembly().GetName().Version.Build;
            if ( newverid > nowverid)
            {
                if (_bUpdate)
                {
                    if (File.Exists(update_app))
                    {
                        Process.Start(update_app, "-autostart -autoclose");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Global.logger.Debug("update 不存在,不执行更新");
                    }
                }
            }
        }

        private void Init()
        {
            Text += string.Format(" {0:yy-MM-dd HH:mm:ss} Version {1}.{2}.{3}"
                , System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor
                , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);

            _loginManage = new LoginManage(this);

            this.gamePath.Text = _iniFile.IniReadValue("UI", "gamePath");

            textBox_Code.Text = _iniFile.IniReadValue("UI", "code");
            textBox_IP.Text = _iniFile.IniReadValue("UI", "manageIp");
            cbModeHangup.Checked = _iniFile.IniReadValue("UI", "hangup") == "1" ? true : false;


            if (textBox_Code.Text == "")
            {
                textBox_Code.Text = "<默认>" + "1号机";
                _loginManage.m_Code = textBox_Code.Text;
            }
            else
            {
                _loginManage.m_Code = textBox_Code.Text;
            }

            if (textBox_IP.Text == "")
            {
                string hostname = Dns.GetHostName();
                IPHostEntry localhost = Dns.GetHostByName(hostname);
                IPAddress localaddr = localhost.AddressList[0];
                string IP = localaddr.ToString();
                _loginManage.m_ManageIp = IP;
                textBox_IP.Text = IP;
            }
            else
            {
                _loginManage.m_ManageIp = textBox_IP.Text;
            }

            autostartTimer.Interval = 2000;
            autostartTimer.Start();

            rebootTimer.Interval = 1000;

            string autoStart = _iniFile.IniReadValue("Other", "autoStart");
            if (autoStart == "1")
            {
                this.autoStartCkbox.Checked = true;
            }
            else
            {

            }

            Global.logger = CLogger.FromFolder("log/cslog");
            Global.logger.SetShowLogFunction(ShowLogFunc);
        }

        public void ShowLogFunc(eLoggerLevel l,string s)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<eLoggerLevel,string>(ShowLogFunc),l,s);
            }
            else
            {
                textBox.Text += s;
                textBox.Text += "\r\n";
                if (textBox.Text.Length > 2000)
                {
                    textBox.Text = "";
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public csLoginTool(bool bAutoStart, bool boUpdate)
        {
            InitializeComponent();
            Init();
            
            _bAutoStart = bAutoStart;
            if (_bAutoStart)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            _bUpdate = boUpdate;

            instance = this;
        }

        static public csLoginTool Instance
        {
            get 
            {
                return instance;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_HOTKEY: //窗口消息-热键
                    switch (m.WParam.ToInt32())
                    {
                        case 32: //热键ID
                            PauseBtn_Click(null, null);
                            break;
                        default:
                            break;
                    }
                    break;
                case WM_CREATE: //窗口消息-创建
                    RegKey(Handle, Space,MOD_CONTROL, VK_F12); //注册热键
                    break;
                case WM_DESTROY: //窗口消息-销毁
                    UnRegKey(Handle, Space); //销毁热键
                    break;
                default:
                    break;
            }
        }


        private static Emgu.CV.OCR.Tesseract _ocr;//创建识别对象
        //传入图片进行识别
        public string RecognizeNumber(Bitmap img)
        {
            if (_ocr == null)
            {
                //方法第一个参数可为""表示通过环境变量调用字库，第二个参数表示字库的文件，第三个表示识别方式，可看文档与资料查找。
                _ocr = new Emgu.CV.OCR.Tesseract(@"./tessdata", "eng", OcrEngineMode.CubeOnly);
                _ocr.SetVariable("tessedit_char_whitelist", "0123456789");//此方法表示只识别1234567890与x字母
            }

            //""标示OCR识别调用失败
            string re = "";
            if (img == null)
                return re;
            else
            {
                Bgr drawColor = new Bgr(Color.White);
                try
                {
                    Image<Bgr, Byte> image = new Image<Bgr, byte>(img);
                    using (Image<Gray, byte> gray = image.Convert<Gray, Byte>())
                    {
                        _ocr.Recognize(gray);
                        Emgu.CV.OCR.Tesseract.Character[] charactors = _ocr.GetCharacters();
                        foreach (Emgu.CV.OCR.Tesseract.Character c in charactors)
                        {
                            image.Draw(c.Region, drawColor, 1);
                        }
                        re = _ocr.GetText();
                    }
                    img.Dispose();
                    return re;
                }
                catch (Exception ex)
                {
                    return re;
                }
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            _loginManage.m_modeHangup = cbModeHangup.Checked;
            StartLogin();
        }

        private void StartLogin()
        {
            GamePath = this.gamePath.Text;
            if (_logicThread != null)
            {
                MessageBox.Show("线程已经启动了!!");
                return;
            }

            if (_logicThread == null)
            {
                _logicThread = new Thread(new ThreadStart(_loginManage.Run));
            }

            if (!_logicThread.IsAlive)
            {
                _logicThread.Start();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_logicThread != null && _logicThread.IsAlive)
            {
                _logicThread.Abort();
                _logicThread = null;
            }
            base.OnClosed(e);
        }

        private void gamePathPickBtn_Click(object sender, EventArgs e)
        {
            //Bitmap i = (Bitmap)Image.FromFile("验证码.bmp");
            //NumberAnalysis s_numberanalysis = new NumberAnalysis();
            //s_numberanalysis.Analysis(i);
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "选择CSOLauncher.exe";
            dialog.Filter = "游戏(*.exe)|*.exe";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.gamePath.Text = dialog.FileName;
                _iniFile.IniWriteValue("UI", "gamePath", this.gamePath.Text);
            }
        }

        public string getGamePath()
        {
            return this.gamePath.Text;
        }

        private void PauseBtn_Click(object sender, EventArgs e)
        {
            if (_logicThread == null)
            {
                MessageBox.Show("登陆器还没有启动");
                return;
            }

            if (LoginManage.isStop)
            {
                LoginManage.isStop = false;
                PauseBtn.Text = "暂停 Ctrl+f12";
            }
            else
            {
                LoginManage.isStop = true;
                PauseBtn.Text = "恢复 Ctrl+f12";
            }
        }

        public static void RegAutoStart(bool reg,bool showDialg = false)
        {
            if (reg)
            {
                string starupPath = Application.ExecutablePath;
                try
                {
                    RegistryKey loca = Registry.LocalMachine;
                    RegistryKey run = loca.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");

                    if (run.GetValue("WinForm") as string != starupPath + @" -autostart")
                    {
                        run.SetValue("WinForm", starupPath + @" -autostart");
                        //MessageBox.Show("成功设置开机启动!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    loca.Close();
                    run.Close();

                }
                catch (Exception ee)
                {
                    if (showDialg)
                    {
                        MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                try
                {
                    RegistryKey loca = Registry.LocalMachine;
                    RegistryKey run = loca.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");
                    run.DeleteValue("WinForm");

                    loca.Close();
                    run.Close();

                    MessageBox.Show("已经关闭开机启动!!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (System.Exception ex)
                {
                    if (showDialg)
                    {
                        MessageBox.Show(ex.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void autoStartCkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoStartCkbox.Checked)
            {
                RegAutoStart(true,true);       //注册开机自动重启
                _iniFile.IniWriteValue("Other", "autoStart", "1");
            }
            else
            {
                RegAutoStart(false,true);
                _iniFile.IniWriteValue("Other", "autoStart", "0");
            }
        }

        private void autostartTime(object sender, EventArgs e)
        {
            if (_bAutoStart)
            {
                Global.logger.Info("========开始启动自动登陆========");
                _loginManage.m_modeHangup = cbModeHangup.Checked;
                StartLogin();
            }
            this.autostartTimer.Stop();
        }

        private void SetCodeBtn_Click(object sender, EventArgs e)
        {
            string s = textBox_Code.Text;
            _loginManage.m_Code = s;
            _iniFile.IniWriteValue("UI", "code", s);

            MessageBox.Show("修改成功");
        }

        private void SetManageIpBtn_Click(object sender, EventArgs e)
        {
            string s = textBox_IP.Text;
            _loginManage.m_ManageIp = s;
            _iniFile.IniWriteValue("UI", "manageIp", s);

            MessageBox.Show("修改成功");
        }

        public void OnMsg(string s)
        {
            _loginManage.OnMsg(s);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (_loginManage.thread != null)
                    _loginManage.thread.Abort();
                if (m_RoutieThread != null)
                    m_RoutieThread.Abort();
                CLogger.StopAllLoggers();
            }
            catch (System.Exception ex)
            {
            	
            }
            base.OnClosing(e);
        }

        const string update_app = "update_v1.exe";

        private void csLoginTool_Load(object sender, EventArgs e)
        {
            CheckVersion();

            tbxMac.Text = CommonApi.GetMacAddress();
            if (File.Exists("conf"))
            {
               string s = ByteDes.Decode(File.ReadAllBytes("conf"));
               XmlDocument doc = new XmlDocument();
               doc.LoadXml(s);

               XmlNode root = doc.SelectSingleNode("Data");
               for (XmlNode item = root.FirstChild; item != null; item = item.NextSibling)
               {
                   string name = item.Attributes["name"].Value;
                   string value = item.Attributes["value"].Value;
                   Lg.Add(name, value);
               }
            }
        }

        private void cbForceReboot_CheckedChanged(object sender, EventArgs e)
        {
            _iniFile.IniWriteValue("UI", "forcereboot", "1");
        }

        private void cbModeHangup_CheckedChanged(object sender, EventArgs e)
        {
            _iniFile.IniWriteValue("UI", "hangup", cbModeHangup.Checked ? "1" : "0");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.logger.Info("测试断网功能!");
            Process.Start("rasdial", "/DISCONNECT");
        }
    }

}
