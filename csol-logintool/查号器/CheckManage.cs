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

using System.Net;
using System.Net.NetworkInformation;

namespace 查号器
{
    class userInfo{
        public string account;
        public string pwd;

        public userInfo(string a, string p)
        {
            account = a;
            pwd = p;
        }
    }

    class CheckManage
    {
        static public userInfo m_account;

        public string m_Code = "";
        public string m_ManageIp = "";

        Session m_session;
        string MacId;       //物理地址

        static public bool isStop = false;

        public CheckManage(csCheckTool loginTool)
        {
            MacId = CommonApi.GetMacAddress();
        }

        public void OnMsg(string s)
        {
            try
            {
                if (s != "0")
                {
                    Global.logger.Debug("Thread:" + Thread.CurrentThread.ManagedThreadId + " Recv:" + s);
                }
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
                            Global.logger.Debug("收到账号" + accName);
                            lock (this)
                            {
                                Global.logger.Debug("账号" + accName + " 准备完毕");
                                m_account = new userInfo(accName, passWord);
                            }
                        }
                        break;
                    case "101":
                        {
                            string reboot = split[1];
                            if (reboot == "reboot")
                            {
                                Global.logger.Info("收到重启系统的请求,执行");
                                csCheckTool.RegAutoStart(true);
                                System.Diagnostics.Process.Start("shutdown", @"/r");
                                System.Environment.Exit(0);
                            }
                        }break;
                }
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }

        public void Run()
        {
            try
            {
                long nLastQueryTime = 0;
                do
                {
                    Thread.Sleep(1000);

                    if (m_session == null)
                    {
                        Session.IP = m_ManageIp;
                        m_session = new Session();
                        m_session.m_code = m_Code;
                        m_session.SetMsgHandle(csCheckTool.Instance.OnMsg);
                    }

                    lock (this)
                    {
                        if (m_account == null)
                        {
                            if (m_session != null && Environment.TickCount - nLastQueryTime > 5000)
                            {
                                m_session.SendMsg("1$" + MacId + "$" + m_Code);
                                nLastQueryTime = Environment.TickCount;
                            }
                        }
                        else
                        {
                            LoginState stateMachine = new LoginState();
                            stateMachine.Run(m_account, m_session);
                            m_session.SendMsg("4$" + MacId);
                            nLastQueryTime = 0;

                            //do 
                            //{
                            //    int x = 100;
                            //    int y = 100;
                            //    int w = 100;
                            //    int h = 100;
                            //    Bitmap screen = new Bitmap(w, h);

                            //    Graphics g = Graphics.FromImage(screen);
                            //    g.CopyFromScreen(x, y, 0, 0, screen.Size);

                            //    MemoryStream ms = new MemoryStream();
                            //    screen.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            //    byte[] arr = new byte[ms.Length];
                            //    ms.Position = 0;
                            //    ms.Read(arr, 0, (int)ms.Length);
                            //    ms.Close();
                            //    string pic = Convert.ToBase64String(arr);

                            //    m_session.SendMsg("7$" + pic);

                            //} while (false);

                            //do
                            //{
                            //    int x1 = 100;
                            //    int y1 = 100;
                            //    int w1 = 100;
                            //    int h1 = 100;

                            //    int x2 = 200;
                            //    int y2 = 200;
                            //    int w2 = 200;
                            //    int h2 = 200;

                            //    Bitmap b1 = new Bitmap(w1, h1);
                            //    Graphics g1 = Graphics.FromImage(b1);
                            //    g1.CopyFromScreen(x1, y1, 0, 0, b1.Size);

                            //    Bitmap b2 = new Bitmap(w2, h2);
                            //    Graphics g2 = Graphics.FromImage(b2);
                            //    g2.CopyFromScreen(x2, y2, 0, 0, b2.Size);


                            //    SendLogSucess(m_account, b1, b2);
                            //} while (false);

                            //do
                            //{
                            //    Random r = new Random();
                            //    int a = r.Next(1, 4);
                            //    if (a == 1)
                            //    {
                            //        //m_session.SendMsg("3$" + m_account.account + "$" + "OK");
                            //        //m_session.SendMsg("4$" + MacId);
                            //    }
                            //    else if (a == 2)
                            //    {
                            //        m_session.SendMsg("3$" + m_account.account + "$" + "Failed");
                            //        m_session.SendMsg("4$" + MacId);
                            //    }
                            //    else
                            //    {
                            //        m_session.SendMsg("3$" + m_account.account + "$" + "PasswordError");
                            //        m_session.SendMsg("4$" + MacId);
                            //    }

                            //    m_session.SendMsg("4$" + MacId);

                            //    Thread.Sleep(1000 / 34);
                            //} while (false);

                            //nLastQueryTime = 0;

                            Global.logger.Debug("清除账号>>>>>" + m_account.account);
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

        private void SendLogSucess(userInfo account, Bitmap b1, Bitmap b2)
        {
            //if (!bSendRet)
            {
                //bSendRet = true;
                string s1 = bitmap2string(b1);
                string s2 = bitmap2string(b2);
                m_session.SendMsg("3$" + account.account + "$" + "OK" + "$" + s1 + "$" + s2);
            }
        }

        private string bitmap2string(Bitmap b)
        {
            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
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

        public void Run(userInfo info,Session s)
        {
            _curAccInfo = info;
            _currentState = State.Kaishi;
            m_client = s;

            Global.logger.Info("");
            Global.logger.Info("=====账号:" + _curAccInfo.account + "开始=====");
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
                    Global.logger.Info("=====账号:" + _curAccInfo.account + "结束=====");
                    Global.logger.Info("");
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

                Sleep(1000);
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
                        Global.logger.Debug("关闭 Counter-Strike Online 广告窗口");
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
                        Sleep(500);
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
                            Sleep(1000);
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
                            Sleep(1000);
                        } while (System.Environment.TickCount - ticks < 5 * 1000);

                        if (bCheck)
                        {
                            int x, y, w, h;
                            int dx, dy;

                            CommonApi.ShowWindow(hwnd);
                            Sleep(500);
                            CommonApi.GetWindowXYWH(hwnd, out x, out y, out w, out h);

                            if (x > 0 && y > 0 && w > 0 && h > 0)
                            {
                                if (CommonApi.FindPic(x + 558, y + 70, 55, 34, @".\BMP\界面登陆.bmp", 0.99, out dx, out dy))
                                {
                                    Sleep(3000);
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
                            Sleep(1000);
                        } while (System.Environment.TickCount - ticks < 15 * 1000);

                        _NextState = State.Kaishi;
                    }break;
                case State.Counter_Strike:
                    {

                        long beginStateTime = System.Environment.TickCount;

                        do
                        {
                            int sX, sY, sW, sH;
                            int x, y, w, h;
                            int dx, dy;

                            IntPtr hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                            CommonApi.GetWindowXYWH(hwnd, out sX, out sY, out sW, out sH);
                            if (hwnd == IntPtr.Zero
                             || _currentState != State.Counter_Strike
                             || sW <= 800 || sH <= 600)
                            {
                                break;
                            }

                            long beginCheckTime = System.Environment.TickCount;

                            int yanzhengma_Lastime = System.Environment.TickCount;
                            int xinbinbaodao_Lasttime = System.Environment.TickCount;
                            int wujuese_Lasttime = System.Environment.TickCount;
                            int queren_Lasttime = System.Environment.TickCount;
                            int quxiao_Lasttime = System.Environment.TickCount;
                            int mimacuowu_Lasttime = System.Environment.TickCount;
                            int tingfeng_Lasttime = System.Environment.TickCount;
                            int mimaxiang_Lasttime = System.Environment.TickCount;

                            int wujuese_Interval = 0;
                            int queren_Interval = 2000;
                            int quxiao_Interval = 2000;

                            bool notFindPic = false;
                            do
                            {
                                ClearOtherWnd();
                                hwnd = CommonApi.FindWindow(null, "Counter-Strike Online");
                                if (hwnd == IntPtr.Zero)
                                {
                                    if (bInputPwd)
                                    {
                                        Global.logger.Debug("账号 " + _curAccInfo.account + " 检测到游戏窗口关闭，登陆失败");
                                        _NextState = State.JieShu;
                                        SendLogFailed(_curAccInfo);
                                    }
                                    else
                                    {
                                        _NextState = State.Kaishi;
                                    }
                                    break;
                                }

                                CommonApi.Mouse_Move(0, 0);

                                CommonApi.ShowWindow(hwnd);
                                if (notFindPic || !bInputPwd)
                                {
                                    Sleep(2000);
                                    notFindPic = false;
                                }
                                else
                                {
                                    Sleep(500);

                                    yanzhengma_Lastime = System.Environment.TickCount;
                                    xinbinbaodao_Lasttime = System.Environment.TickCount;
                                    wujuese_Lasttime = System.Environment.TickCount;
                                    queren_Lasttime = System.Environment.TickCount;
                                    quxiao_Lasttime = System.Environment.TickCount;
                                    mimacuowu_Lasttime = System.Environment.TickCount;
                                    tingfeng_Lasttime = System.Environment.TickCount;
                                    mimaxiang_Lasttime = System.Environment.TickCount;
                                }
                                CommonApi.GetWindowXYWH(hwnd, out sX, out sY, out sW, out sH);
                                x = sX + 150;
                                y = sY + 150;
                                w = sW - 150;
                                h = sH - 150;

                                if (x > 0 && y > 0 && w > 0 && h > 0)
                                {
                                    if (CommonApi.FindPic(x + 300, y + 300, 100, 50, @".\BMP\游戏登陆.bmp", 0.99, out dx, out dy))
                                    {
                                        bInputPwd = true;

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

                                        Global.logger.Info("等待登陆完成..");
                                        //Sleep(5000, "点击登陆");

                                        long nowTick = System.Environment.TickCount;
                                        do
                                        {
                                            if (!CommonApi.FindPic(x + 300, y + 300, 100, 50, @".\BMP\游戏登陆.bmp", 0.99, out dx, out dy))
                                            {
                                                if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\密码错误.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogPasswordError(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(3000, "密码错误,关闭游戏");

                                                }
                                                else if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\密码有误停封.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogForbidden(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(1000, "账号停封,关闭游戏");
                                                }
                                                else if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\连续输入错误.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    m_client.SendMsg("6$" + "changeip");
                                                    Sleep(60 * 1000, "连续输入错误");

                                                    SendLogFailed(_curAccInfo);

                                                    _NextState = State.JieShu;
                                                    Sleep(3000, "连续输入错误,关闭游戏");
                                                }
                                                else if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\服务器连接中断.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogFailed(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(3000, "服务器连接中断,关闭游戏");
                                                }

                                                break;
                                            }
                                            Sleep(2000);
                                        } while (System.Environment.TickCount - nowTick < 7 * 1000);

                                        break;
                                    }

                                    if (!bInputPwd)
                                    {
                                        break;
                                    }

                                    if (bInputPwd && CheckInterLastTime(ref xinbinbaodao_Lasttime, 3000 + _Rand(1000)) && CommonApi.FindPic(x, y, w, h, @".\BMP\新兵报到.bmp", 0.99, out dx, out dy))
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

                                    if (CommonApi.FindPic(sX + 268, sY + 584, 133, 40, @".\BMP\战场补给.bmp", 0.99, out dx, out dy))
                                    {
                                        if (CommonApi.FindPic(x, y, w, h, @".\BMP\关闭.bmp", 0.99, out dx, out dy))
                                        {
                                            CommonApi.Left_Click(dx + 5, dy + 5);
                                            Sleep(300);
                                        }
                                        break;
                                    }


                                    if (CommonApi.FindPic(sX + 0, sY + 0, sW, sH, @".\BMP\密码箱界面.bmp", 0.99, out dx, out dy))
                                    {
                                        Bitmap b1 = new Bitmap(25, 18);
                                        Graphics g1 = Graphics.FromImage(b1);
                                        g1.CopyFromScreen(sX + 222,sY + 369, 0, 0, b1.Size);

                                        Bitmap b2 = new Bitmap(40, 18);
                                        Graphics g2 = Graphics.FromImage(b2);
                                        g2.CopyFromScreen(sX + 773 ,sY + 532, 0, 0, b2.Size);

                                        SendLogSucess(_curAccInfo, b1, b2);

                                        g1.Dispose();
                                        g2.Dispose();
                                        b1.Dispose();
                                        b2.Dispose();

                                        Sleep(1000);

                                        _NextState = State.JieShu;
                                        Global.logger.Debug("查号成功，关闭游戏");
                                        CommonApi.CloseWindow(hwnd);
                                        break;
                                    }

                                    //if (CommonApi.FindPic(sX + 450, sY + 650, 120, 90, @".\BMP\领取奖励按钮.bmp", 0.99, out dx, out dy))
                                    //{
                                    //    int nOldDay = 0;
                                    //    for (int i = 1; i <= 7; ++i)
                                    //    {
                                    //        int dx1, dy1;
                                    //        int offset = (i - 1) * 92;
                                    //        if (!CommonApi.FindPic(sX + 180 + offset, sY + 280, 108, 75, @".\BMP\领取" + i + "天.bmp", 0.99, out dx1, out dy1))
                                    //        {
                                    //            nOldDay = i;
                                    //        }
                                    //    }

                                    //    Global.logger.Debug("点击之前,已经领过" + nOldDay + "天");

                                    //    CommonApi.Left_Click(dx + 5, dy + 5);

                                    //    long nowTick = System.Environment.TickCount;
                                    //    do
                                    //    {
                                    //        Sleep(1500);

                                    //        int nDay = 0;
                                    //        for (int i = 1; i <= 7; ++i)
                                    //        {
                                    //            int dx1, dy1;
                                    //            int offset = (i - 1) * 92;
                                    //            if (!CommonApi.FindPic(sX + 180 + offset, sY + 280, 108, 75, @".\BMP\领取" + i + "天.bmp", 0.99, out dx1, out dy1))
                                    //            {
                                    //                nDay = i;
                                    //            }
                                    //        }

                                    //        if (nDay == nOldDay + 1)
                                    //        {
                                    //            Global.logger.Debug("点击之后,已经领过" + nDay + "天");
                                    //            m_client.SendMsg("5$" + _AccInfo.account + "$" + nDay);
                                    //            SendLogSucess(_curAccInfo);
                                    //            CommonApi.CloseWindow(hwnd);
                                    //            _NextState = State.JieShu;
                                    //            Sleep(1000, "检测领取成功");

                                    //            long n = System.Environment.TickCount;
                                    //            IntPtr oldPtr = hwnd;
                                    //            do
                                    //            {
                                    //                CommonApi.CloseWindow(oldPtr);
                                    //                IntPtr p = CommonApi.FindWindow(null, "Counter-Strike Online");
                                    //                if (p.ToInt32() != oldPtr.ToInt32())
                                    //                {
                                    //                    break;
                                    //                }
                                    //                Sleep(1000);
                                    //            } while (System.Environment.TickCount - n < 10 * 1000);

                                    //            break;
                                    //        }

                                    //    } while (System.Environment.TickCount - nowTick < 10 * 1000);

                                    //    break;
                                    //}

                                    //if (CommonApi.FindPic(sX + 450, sY + 650, 120, 90, @".\BMP\领过奖励.bmp", 0.999, out dx, out dy))
                                    //{
                                    //    for (int i = 1; i <= 7; ++i)
                                    //    {
                                    //        int offset = (i - 1) * 92;
                                    //        if (!CommonApi.FindPic(sX + 180 + offset, sY + 280, 108, 75, @".\BMP\领取" + i + "天.bmp", 0.99, out dx, out dy))
                                    //        {
                                    //            Global.logger.Debug("账号领取过" + i + "天");
                                    //            m_client.SendMsg("5$" + _AccInfo.account + "$" + i);
                                    //        }
                                    //    }

                                    //    Sleep(200);
                                    //    CommonApi.CloseWindow(hwnd);

                                    //    SendLogSucess(_curAccInfo);
                                    //    _NextState = State.JieShu;

                                    //    Sleep(1000, "领过奖励");
                                    //    break;
                                    //}


                                    if (bInputPwd && CheckInterLastTime(ref yanzhengma_Lastime, 2000 + _Rand(2000)) && CommonApi.FindPic(x, y, w, h, @".\BMP\验证码.bmp", 0.99, out dx, out dy))
                                    {
                                        m_client.SendMsg("6$" + "changeip");
                                        Sleep(60 * 1000, "遇到验证码");

                                        SendLogFailed(_curAccInfo);
                                        CommonApi.CloseWindow(hwnd);

                                        _NextState = State.JieShu;
                                        Sleep(1000, "关闭游戏");
                                        break;
                                    }

                                    if (bInputPwd && CheckInterLastTime(ref wujuese_Lasttime, wujuese_Interval) && CommonApi.FindPic(sX + 411, sY + 340, 200, 40, @".\BMP\无角色.bmp", 0.99, out dx, out dy))
                                    {
                                        wujuese_Interval = 5000;
                                        queren_Interval = 500;
                                        quxiao_Lasttime = 500;

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

                                    if (CommonApi.FindPic(sX + 424, sY + 608, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CommonApi.FindPic(sX + 596, sY + 616, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CommonApi.FindPic(sX + 458, sY + 426, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CommonApi.FindPic(sX + 481, sY + 419, 80, 35, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CheckInterLastTime(ref queren_Lasttime, queren_Interval + _Rand(1000)) && CommonApi.FindPic(x, y, w, h, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CheckInterLastTime(ref quxiao_Lasttime, quxiao_Interval + _Rand(1000)) && CommonApi.FindPic(x, y, w, h, @".\BMP\关闭.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    notFindPic = true;

                                    if (bInputPwd && CheckInterLastTime(ref mimaxiang_Lasttime, 1500) && CommonApi.FindPic(sX + sW - 122, sY + sH - 50, 122, 50, @".\BMP\密码箱.bmp", 0.999, out dx, out dy))
                                    {
                                        CommonApi.Mouse_Move(dx + 6, dy + 6);
                                        Sleep(1500);
                                        CommonApi.Left_Click(200);
                                        Sleep(1000);
                                        CommonApi.Double_Click(dx + 6, dy + 6);

                                        notFindPic = false;
                                        break;
                                    }
                                }

                                if (System.Environment.TickCount - beginCheckTime > 50 * 1000)
                                {
                                    SendLogFailed(_curAccInfo);
                                    CommonApi.CloseWindow(hwnd);

                                    _NextState = State.JieShu;
                                    Sleep(1000, "游戏超时50秒,准备关闭");

                                    long endGameTime = System.Environment.TickCount;
                                    while (CommonApi.FindWindow(null, "Counter-Strike Online") != null)
                                    {
                                        Sleep(1000);
                                        CommonApi.CloseWindow(hwnd);
                                    }

                                    break;
                                }
                            } while (true);

                            //if (System.Environment.TickCount - beginStateTime > 3 * 60 * 1000)
                            //{
                            //    CommonApi.TraceInfo("超时3分钟,执行重启计算机!!");
                            //    System.Diagnostics.Process.Start("shutdown", @"/r");
                            //    System.Environment.Exit(0);
                            //}
                        } while (true);
                    }break;
            }
        }

        bool bInputPwd = false;

        Random rd = new Random();
        private int _Rand(int maxValue)
        {
            return rd.Next(maxValue);
        }

        private bool CheckInterLastTime(ref int lastTime,int interval)
        {
            if (System.Environment.TickCount > lastTime + interval)
            {
                lastTime = System.Environment.TickCount;
                return true;
            }
            return false;
        }

        private void Sleep(int ticks, string option = "")
        {
            while (CheckManage.isStop)
            {
                Thread.Sleep(1);
            }

            bool x = false;
            if (ticks > 10 * 10000 || option != "")
            {
                if (option == "")
                {
                    option = "未知";
                }

                x = true;
                Global.logger.Debug("等待时长{0}秒,原因:{1}...", ticks / 1000, option);
            }

            Thread.Sleep(ticks);
            

            if (x)
            {
                Global.logger.Debug("结束等待");
            }
        }

        private string bitmap2string(Bitmap b)
        {
            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
        }

        private void SendLogSucess(userInfo account,Bitmap b1,Bitmap b2)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                string s1 = bitmap2string(b1);
                string s2 = bitmap2string(b2); 
                m_client.SendMsg("3$" + account.account + "$" + "OK" + "$" + s1 + "$" + s2);
            }
        }

        private void SendLogFailed(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                m_client.SendMsg("3$" + account.account + "$" + "Failed");
            }
        }

        private void SendLogPasswordError(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                m_client.SendMsg("3$" + account.account + "$" + "PasswordError");
            }
        }

        private void SendLogForbidden(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                m_client.SendMsg("3$" + account.account + "$" + "Forbidden");
            }
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
                return "";
            }
            set
            {
            }
        }

        private userInfo _curAccInfo
        {
            get
            {
                return _AccInfo;
            }
            set
            {
                _AccInfo = value;
            }
        }

        State _currentState;
        State _NextState;

        userInfo _AccInfo;
        Session m_client;

        bool bSendRet = false;

        public csCheckTool _loginTool = csCheckTool.Instance;
        static IniFile _ini = new IniFile(@".\config.ini");

        static DateTime startTime = DateTime.Now;

        static string logFileName = String.Format("{0:yyyyMMdd_HHmmss}", DateTime.Now);

        delegate void DelegateV1<T>(T t);
        delegate R Delegate0<R>();
        delegate R Delegate1<R,T>(T t); 
    }


}
