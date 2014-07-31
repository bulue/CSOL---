﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using Dama2Lib;
using System.Net;
using System.Net.NetworkInformation;

namespace CSLogin
{
    class accountInfo{
        public string account;
        public string pwd;
        public string rawInfo;

        public accountInfo(string acc_, string pwd_,string rawInfo_)
        {
            account = acc_;
            pwd = pwd_;
            rawInfo = rawInfo_;
        }
    }

    class MainLogic
    {
        static public accountInfo m_account;

        public string m_Code = "";
        public string m_ManageIp = "";

        //public Semaphore m_Sem = new Semaphore(0,1);
        //public Mutex m_lock;

        Session m_session;
        string MacId;       //物理地址

        public MainLogic(csLoginTool loginTool)
        {
            MacId = GetMacAddressByNetworkInformation();
        }

        public void OnMsg(string s)
        {
            try
            {
                CommonApi.TraceInfo("Thread:" + Thread.CurrentThread.ManagedThreadId + " Recv:" + s);
                string[] split = s.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 0)
                {
                    return;
                }

                switch (split[0])
                {
                    case "2":
                        {
                            string accName = split[1];
                            string passWord = split[2];

                            lock (this)
                            {
                                m_account = new accountInfo(accName, passWord, "");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                CommonApi.TraceInfo(ex.ToString());
            }
        }

        public string GetMacAddressByNetworkInformation()
        {
            string macAddress = "";
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (!adapter.GetPhysicalAddress().ToString().Equals(""))
                    {
                        macAddress = adapter.GetPhysicalAddress().ToString();
                        for (int i = 1; i < 6; i++)
                        {
                            macAddress = macAddress.Insert(3 * i - 1, ":");
                        }
                        break;
                    }
                }

            }
            catch
            {
            }
            return macAddress;
        }

        public void Run()
        {
            try
            {
                Random r = new Random();
 
                long nLastQueryTime = 0;
                do
                {
                    Thread.Sleep(1000);
                    if (m_session == null)
                    {
                        Session.IP = m_ManageIp;
                        m_session = new Session();
                        m_session.SetMsgHandle(csLoginTool.Instance.OnMsg);
                    }

                    lock (this)
                    {
                        if (m_account == null)
                        {
                            if (DateTime.Now.Ticks - nLastQueryTime > 30)
                            {
                                m_session.SendMsg("1$" + MacId + "$" + m_Code);
                                nLastQueryTime = DateTime.Now.Ticks;
                            }
                        }
                        else
                        {
                            m_session.SendMsg("4$" + MacId);
                            nLastQueryTime = 0;
                            LoginState stateMachine = new LoginState();
                            stateMachine.Run(m_account, m_session);

                            //do
                            //{
                            //    int a = r.Next(1, 4);
                            //    if (a == 1)
                            //    {
                            //        m_session.SendMsg("3$" + m_account.account + "$" + "OK");
                            //    }
                            //    else if (a == 2)
                            //    {
                            //        m_session.SendMsg("3$" + m_account.account + "$" + "Failed");
                            //    }
                            //    else
                            //    {
                            //        m_session.SendMsg("3$" + m_account.account + "$" + "PasswordError");
                            //    }
                            //} while (false);

                             m_account = null;
                        }
                    }
                } while (true);

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thread Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


    class LoginState
    {
        enum State{
            Kaishi,
            Launncher,
            Wait_Counter_Strike,
            Counter_Strike,
            JieShu,
        };

        public void Run(accountInfo info,Session s)
        {
            _curAccInfo = info;
            _currentState = State.Kaishi;
            m_client = s;

            CommonApi.TraceInfo("");
            CommonApi.TraceInfo("=====账号:" + _curAccInfo.account + "开始=====");
            do 
            {
                Sleep(1);
                ClearOtherWnd();
                RunNextStation();
                _currentState = _NextState;

                if (_currentState == State.JieShu)
                {
                    WaitEnd();
                    Sleep(1000);
                    CommonApi.TraceInfo("=====账号:" + _curAccInfo.account + "结束=====");
                    CommonApi.TraceInfo("");
                    break;
                }
            } while (true);
        }

        void WaitEnd()
        {
            long nowTick = System.Environment.TickCount;
            do
            {
                IntPtr hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                if (hwnd == IntPtr.Zero)
                {
                    break;
                }
                else
                {
                    int x, y, w, h;
                    CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);
                    if (w < 600)
                    {
                        CommonApi.CloseWindow(hwnd);
                    }
                }

                Sleep(300);
            } while (System.Environment.TickCount - nowTick < 150000);
        }

        void ClearOtherWnd()
        {
            IntPtr hwnd ;

            do 
            {
                hwnd = CommonApi.FindWindow(null, "CSOLauncher");
                if (hwnd != IntPtr.Zero)
                {
                    int x, y, w, h;
                    CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);
                    if (w < 500)
                    {
                        CommonApi.CloseWindow(hwnd);
                    }
                }
            } while (false);

            do 
            {
                bool loopBreak = true;
                hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                if (hwnd != IntPtr.Zero)
                {
                    int x, y, w, h;
                    CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);
                    if (w < 600)
                    {
                        CommonApi.CloseWindow(hwnd);
                        loopBreak = false;
                        Sleep(1000);
                    }
                }

                if (loopBreak)
                    break;
            } while (true);

            while (CommonApi.FindWindow(null, "非法程序检测") != IntPtr.Zero)
            {
                CommonApi.CloseWindow(CommonApi.FindWindow(null, "非法程序检测"));
            }

            while (CommonApi.FindWindow(null, "Error") != IntPtr.Zero)
            {
                CommonApi.CloseWindow(CommonApi.FindWindow(null, "Error"));
            }

            while (CommonApi.FindWindow(null, "脚本错误") != IntPtr.Zero)
            {
                CommonApi.CloseWindow(CommonApi.FindWindow(null, "脚本错误"));
            }

        }

        void RunNextStation()
        {
            switch (_currentState)
            {
                case State.Kaishi:
                    {
                        Sleep(100);
                        IntPtr hwnd = CommonApi.FindWindow(null, "CSOLauncher");
                        if (hwnd != IntPtr.Zero)
                        {
                            _NextState = State.Launncher;
                            return;
                        }

                        hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                        if (hwnd != IntPtr.Zero)
                        {
                            _NextState = State.Counter_Strike;
                            return;
                        }

                        string accountPath = _loginTool.Invoke(new Delegate0<string>(_loginTool.getGamePath)) as string;
                        CommonApi.StartProcess(accountPath);
                        Sleep(500);

                        long ticks = System.Environment.TickCount;
                        do 
                        {
                            if (CommonApi.FindWindow(null, "CSOLauncher") != IntPtr.Zero)
                            {
                                _NextState = State.Launncher;
                                break;
                            }
                        } while (System.Environment.TickCount - ticks < 10 * 1000);

                        RunNextStation();
                    }break;
                case State.Launncher:
                    {
                        IntPtr hwnd;

                        bool bCheck = false;
                        long ticks = System.Environment.TickCount;
                        do 
                        {
                            hwnd = CommonApi.FindWindow(null, "CSOLauncher");
                            if (hwnd != IntPtr.Zero)
                            {
                                bCheck = true;
                                break;
                            }
                        } while (System.Environment.TickCount - ticks < 5 * 1000);

                        if (bCheck)
                        {
                            int x, y, w, h;
                            int dx, dy;

                            CommonApi.ShowWindow(hwnd);
                            Sleep(200);
                            CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);

                            if (x > 0 && y > 0 && w > 0 && h > 0)
                            {
                                if (CommonApi.FindPic(x, y, w, h, @".\BMP\界面登陆.bmp", 0.99, out dx, out dy))
                                {
                                    Sleep(2000);
                                    CommonApi.Left_Click(dx + 5, dy + 5);
                                    _NextState = State.Wait_Counter_Strike;
                                }
                            }
                        }
                        else
                        {
                            _NextState = State.Kaishi;
                            return;
                        }
                        
                    }break;
                case State.Wait_Counter_Strike:
                    {
                        IntPtr hwnd;

                        long ticks = System.Environment.TickCount;
                        do 
                        {
                            hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                            if (hwnd != IntPtr.Zero)
                            {
                                _NextState = State.Counter_Strike;
                                return;
                            }
                        } while (System.Environment.TickCount - ticks < 15 * 1000);

                        _NextState = State.Kaishi;
                    }break;
                case State.Counter_Strike:
                    {
                        IntPtr hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                        if (hwnd != IntPtr.Zero)
                        {
                            long ticks = System.Environment.TickCount;
                            do
                            {
                                ClearOtherWnd();
                                hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                                if (hwnd == IntPtr.Zero)
                                {
                                    _NextState = State.Kaishi;
                                    break;
                                }

                                CommonApi.Mouse_Move(0, 0);

                                int x, y, w, h;
                                int dx, dy;

                                CommonApi.ShowWindow(hwnd);
                                Sleep(50);
                                CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);
                                x += 150;
                                y += 150;
                                w -= 150;
                                h -= 150;

                                if (x > 0 && y > 0 && w > 0 && h > 0)
                                {
                                    if (CommonApi.FindPic(x, y, w, h, @".\BMP\游戏登陆.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 135, dy - 97);
                                        Sleep(200);
                                        CommonApi.Left_Click(dx + 135, dy - 97);
                                        Sleep(500);
                                        SendKeys.SendWait("{Delete}");
                                        Sleep(100);
                                        SendKeys.SendWait(_curAccInfo.account);
                                        Sleep(500);
                                        CommonApi.Left_Click(dx + 135, dy - 64);
                                        Sleep(200);
                                        CommonApi.Left_Click(dx + 135, dy - 64);
                                        Sleep(500);
                                        SendKeys.SendWait("{Delete}");
                                        Sleep(100);
                                        SendKeys.SendWait(_curAccInfo.pwd);
                                        Sleep(200);
                                        CommonApi.Left_Click(dx, dy);

                                        CommonApi.TraceInfo("等待登陆完成..");
                                        //Sleep(5000, "点击登陆");

                                        long nowTick = System.Environment.TickCount;
                                        do
                                        {
                                            if (!CommonApi.FindPic(x, y, w, h, @".\BMP\游戏登陆.bmp", 0.99, out dx, out dy))
                                            {
                                                break;
                                            }
                                            Sleep(200);
                                        } while (System.Environment.TickCount - nowTick < 7 * 1000);

                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h, @".\BMP\新兵报到.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 72, dy + 115);
                                        Sleep(200);
                                        if (CommonApi.FindPic(x, y, w, h, @".\BMP\选择.bmp", 0.99, out dx, out dy))
                                        {
                                            CommonApi.Left_Click(dx + 5, dy + 5);
                                            Sleep(300);
                                        }

                                        if (CommonApi.FindPic(x, y, w, h, @".\BMP\关闭.bmp", 0.99, out dx, out dy))
                                        {
                                            CommonApi.Left_Click(dx + 5, dy + 5);
                                            Sleep(300);
                                        }
                                    }

                                    if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\战场补给.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(50);
                                        break;
                                    }

                                    if (CommonApi.FindPic(x + 200, y + 450, w - 200, h - 450, @".\BMP\领取奖励按钮.bmp", 0.99, out dx, out dy))
                                    {
                                        int nOldDay = 0;
                                        for (int i = 1; i <= 7; ++i)
                                        {
                                            int dx1, dy1;
                                            if (!CommonApi.FindPic(x, y, w, h, @".\BMP\领取" + i + "天.bmp", 0.99, out dx1, out dy1))
                                            {
                                                nOldDay = i;
                                            }
                                        }

                                        CommonApi.TraceInfo("点击之前,已经领过" + nOldDay + "天");

                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        Sleep(200);

                                        long nowTick = System.Environment.TickCount;
                                        do 
                                        {
                                            int nDay = 0;
                                            for (int i = 1; i <= 7; ++i)
                                            {
                                                int dx1, dy1;
                                                if (!CommonApi.FindPic(x, y, w, h, @".\BMP\领取" + i + "天.bmp", 0.99, out dx1, out dy1))
                                                {
                                                    nDay = i;
                                                }
                                            }

                                            if (nDay == nOldDay + 1)
                                            {
                                                CommonApi.TraceInfo("点击之后,已经领过" + nDay + "天");
                                                m_client.SendMsg("5$" + _AccInfo.account + "$" + nDay);
                                                SendLogSucess(_curAccInfo);
                                                CommonApi.CloseWindow(hwnd);
                                                _NextState = State.JieShu;
                                                Sleep(1000, "检测领取成功");
                                                break;
                                            }
                                        } while (System.Environment.TickCount - nowTick < 3*1000);

                                        break;
                                    }

                                    if (CommonApi.FindPic(x + 200, y + 450, w - 200, h - 450, @".\BMP\领过奖励.bmp", 0.999, out dx, out dy))
                                    {
                                        for (int i = 1; i <= 7; ++i)
                                        {
                                            if (!CommonApi.FindPic(x, y, w, h, @".\BMP\领取"+ i +"天.bmp", 0.99, out dx, out dy))
                                            {
                                                CommonApi.TraceInfo("账号领取过"+ i +"天");
                                                m_client.SendMsg("5$" + _AccInfo.account + "$" + i);
                                            }
                                        }

                                        Sleep(200);
                                        CommonApi.CloseWindow(hwnd);

                                        SendLogSucess(_curAccInfo);
                                        _NextState = State.JieShu;

                                        Sleep(1000, "领过奖励");
                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\密码错误.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.CloseWindow(hwnd);

                                        SendLogPasswordError(_curAccInfo);
                                        _NextState = State.JieShu;

                                        Sleep(1000, "密码错误,关闭游戏");
                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h, @".\BMP\验证码.bmp", 0.99, out dx, out dy))
                                    {
                                        bool boOpen = (bool)_loginTool.Invoke(new Delegate0<bool>(_loginTool.xiaoaiopenFunc));
                                        if (boOpen)
                                        {
                                            using (Bitmap screen = CommonApi.ScreenShot(dx, dy + 87, 160, 63))
                                            {
                                                screen.Save("验证码.bmp");

                                                string Tid = "";
                                                xa.SetRebate("xiaozhuhaoa");
                                                string ans = xa.SendFileEx(xiaoaiUserStr, "1040", @".\验证码.bmp", 60, 0, 0, Tid, 0).ToString();
                                                if (xa.IsRight(ans) == true)
                                                {
                                                    string retCode = ans;
                                                    CommonApi.Left_Click(dx + 170, dy + 200);
                                                    Sleep(300);
                                                    CommonApi.Left_Click(dx + 170, dy + 200);

                                                    SendKeys.SendWait(retCode);
                                                    Sleep(300);
                                                    CommonApi.Left_Click(dx + 120, dy + 255);

                                                    long nowTick = System.Environment.TickCount;
                                                    do
                                                    {
                                                        Sleep(100);
                                                        if (!CommonApi.FindPic(x, y, w, h, screen, 0.99, out dx, out dy))
                                                        {
                                                            break;
                                                        }
                                                    } while (System.Environment.TickCount - nowTick < 8000);

                                                    SaveCaptcha(screen, retCode + ".bmp", "小爱打码");

                                                    if (CommonApi.FindPic(x, y, w, h, screen, 0.99, out dx, out dy))
                                                    {
                                                        CommonApi.TraceInfo("验证码有误！！报告错误！！");
                                                        xa.SendError(Tid);
                                                    }
                                                }
                                                else
                                                {

                                                }
                                            }
                                            return;
                                        }

                                        boOpen = (bool)_loginTool.Invoke(new Delegate0<bool>(_loginTool.dama2OpenFunc));
                                        if (boOpen)
                                        {

                                            using (Bitmap screen = CommonApi.ScreenShot(dx, dy + 87, 160, 63))
                                            {
                                                MemoryStream ms = null;
                                                byte[] byteImage;
                                                try
                                                {
                                                    ms = new MemoryStream();
                                                    screen.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                                                    byteImage = new Byte[ms.Length];
                                                    byteImage = ms.ToArray();
                                                }
                                                catch (ArgumentNullException ex)
                                                {
                                                    throw ex;
                                                }
                                                finally
                                                {
                                                    ms.Close();
                                                }

                                                StringBuilder VCodeText = new StringBuilder(100);
                                                int ret = Dama2.D2Buf(
                                                    "c5aff0c4d218205cd5f762ca9acaa81e", //softawre key (software id)
                                                    "xiaozhuhaoa",    //user name
                                                    "19881226",     //password
                                                    byteImage,         //图片数据，图片数据不可大于4M
                                                    (uint)byteImage.Length, //图片数据长度
                                                    30,         //超时时间，单位为秒，更换为实际需要的超时时间
                                                    109,        //验证码类型ID，参见 http://wiki.dama2.com/index.php?n=ApiDoc.GetSoftIDandKEY
                                                    VCodeText); //成功时返回验证码文本（答案）

                                                //SendLogFailed(_accInfo);
                                                //_NextState = State.JieShu;

                                                if (ret > 0)
                                                {
                                                    string retCode = VCodeText.ToString();
                                                    CommonApi.Left_Click(dx + 170, dy + 200);
                                                    Sleep(300);
                                                    CommonApi.Left_Click(dx + 170, dy + 200);

                                                    SendKeys.SendWait(retCode);
                                                    Sleep(300);
                                                    CommonApi.Left_Click(dx + 120, dy + 255);

                                                    long nowTick = System.Environment.TickCount;
                                                    do
                                                    {
                                                        Sleep(100);
                                                        if (!CommonApi.FindPic(x, y, w, h, screen, 0.99, out dx, out dy))
                                                        {
                                                            break;
                                                        }
                                                    } while (System.Environment.TickCount - nowTick < 6000);

                                                    SaveCaptcha(screen, retCode + ".bmp", "打码兔");

                                                    if (CommonApi.FindPic(x, y, w, h, screen, 0.99, out dx, out dy))
                                                    {
                                                        CommonApi.TraceInfo("验证码有误！！报告错误！！");
                                                        Dama2.ReportResult((uint)ret, 0);

                                                        //CommonApi.Left_Click(dx + 210, dy + 120);       //换一个验证码
                                                    }
                                                }
                                            }

                                            Sleep(1000);
                                        }

                                        Sleep(60 * 1000, "遇到验证码");

                                        SendLogFailed(_curAccInfo);
                                        CommonApi.CloseWindow(hwnd);

                                        _NextState = State.JieShu;
                                        Sleep(1000, "关闭游戏");
                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\无角色.bmp", 0.99, out dx, out dy))
                                    {
                                        Random rd = new Random();
                                        string s = "";
                                        for (int i = 0; i < 10; ++i)
                                            s += (char)rd.Next('a', 'z');

                                        CommonApi.Left_Click(dx + 152, dy + 76);
                                        Sleep(300);
                                        CommonApi.Left_Click(dx + 152, dy + 76);
                                        Sleep(500);
                                        SendKeys.SendWait("{Delete}");
                                        Sleep(500);
                                        SendKeys.SendWait(s);
                                        Sleep(500);
                                        CommonApi.Left_Click(dx + 83, dy + 115);
                                        Sleep(500);
                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CommonApi.FindPic(x, y, w, h, @".\BMP\关闭.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }
                                }

                                if (System.Environment.TickCount - ticks > 30 * 1000)
                                {
                                    SendLogFailed(_curAccInfo);
                                    CommonApi.CloseWindow(hwnd);

                                    _NextState = State.JieShu;
                                    Sleep(1000,"游戏超时30秒,准备关闭");

                                    break;
                                }
                            } while (true);
                        }
                        else
                        {
                            _NextState = State.Kaishi;
                            Sleep(1000);
                        }
                    }break;
            }
        }

        private void Sleep(int ticks, string option = "")
        {
            bool x = false;
            if (ticks > 10 * 10000 || option != "")
            {
                if (option == "")
                {
                    option = "未知";
                }

                x = true;
                CommonApi.TraceInfo("等待时长{0}秒,原因:{1}...", ticks / 1000, option);
            }

            Thread.Sleep(ticks);
            

            if (x)
            {
                CommonApi.TraceInfo("结束等待");
            }
        }

        private void SendLogSucess(accountInfo account)
        {
            m_client.SendMsg("3$" + account.account + "$" + "OK");
        }

        private void SendLogFailed(accountInfo account)
        {
            m_client.SendMsg("3$" + account.account + "$" + "Failed");
        }

        private void SendLogPasswordError(accountInfo account)
        {
            m_client.SendMsg("3$" + account.account + "$" + "PasswordError");
        }

        private void SaveCaptcha(Bitmap bmp,string Name,string subdir)
        {
            string dir = string.Format(@".\验证码\{0}\{1:yyyyMMdd_HHmmss}", subdir, startTime);
            if (!Directory.Exists(dir))
            {
                DirectoryInfo di = Directory.CreateDirectory(dir);
            }
            bmp.Save(dir + @"\" + Name);
        }

        public static string xiaoaiUserStr
        {
            get
            {
                return _xiaoaiUserStr;
            }
            set
            {
                _xiaoaiUserStr = value;
                _iniFile.IniWriteValue("xiaoai", "userstr", value);
            }
        }

        private accountInfo _curAccInfo
        {
            get
            {
                return _AccInfo;
            }
            set
            {
                _AccInfo = value;
                //_loginTool.Invoke(new DelegateV1<string>(_loginTool.setListboxSel), value.rawInfo);
            }
        }

        State _currentState;
        State _NextState;

        accountInfo _AccInfo;
        Session m_client;

        public csLoginTool _loginTool = csLoginTool.Instance;
        static IniFile _iniFile = new IniFile(@".\config.ini");

        static DateTime startTime = DateTime.Now;

        public static xiaoai.dt xa = new xiaoai.dt();      //小爱打码
        private static string _xiaoaiUserStr = _iniFile.IniReadValue("xiaoai", "userstr");    //小爱密码串 

        static string logFileName = String.Format("{0:yyyyMMdd_HHmmss}", DateTime.Now);

        delegate void DelegateV1<T>(T t);
        delegate R Delegate0<R>();
        delegate R Delegate1<R,T>(T t); 
    }


}
