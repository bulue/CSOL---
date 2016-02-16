//欢乐一线牵
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CommonQ;

namespace CSLogin
{
    class HuanLeYiXianQianState
    {
        enum State
        {
            Kaishi,
            Launncher,
            Wait_Counter_Strike,
            Counter_Strike,
            JieShu,
        };

        public void Run(userInfo info, Session s)
        {
            try
            {
                _curAccInfo = info;
                _currentState = State.Kaishi;
                //m_client = s;

                Global.logger.Info("欢乐一线牵");
                Global.logger.Info("=====账号:" + _curAccInfo.account + "开始=====");
                do
                {
                    Global.logger.Debug("State:" + _currentState);
                    Sleep(1);
                    ClearOtherWnd();
                    try
                    {
                        RunNextStation();
                    }
                    catch (System.Exception ex)
                    {
                        Global.logger.Info(ex.ToString());
                    }
                    _currentState = _NextState;

                    if (_currentState == State.JieShu)
                    {
                        WaitEnd();
                        Sleep(1000);
                        for (;;)
                        {
                            if (breportok)
                            {
                                break;
                            }
                            Global.logger.Info("=====账号:{0} 检测到结果上报失败，等待结果上报中...");
                            lock (LoginManage.m_session)
                            {
                                if (LoginManage.m_session != null)
                                {
                                    breportok = LoginManage.m_session.SendMsg(report + "$" + "reportagain");
                                }
                                else
                                {
                                    Global.logger.Info("Session还没有连接上");
                                }
                            }
                            Sleep(1000);
                        }
                        Global.logger.Info("=====账号:" + _curAccInfo.account + "结束=====");
                        Global.logger.Info("");
                        break;
                    }
                } while (true);
            }
            catch (ThreadAbortException)
            {

            }
            catch (System.Exception ex)
            {
                Global.logger.Info(ex.ToString());
            }
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
            } while (System.Environment.TickCount - nowTick < 15000);
        }

        void ClearOtherWnd()
        {
            IntPtr hwnd;

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

                        //RunNextStation();
                    } break;
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
                                    int tx, ty;
                                    int xoffset, yoffset;
                                    if (_AccInfo.zoneid == 1)
                                    {
                                        //1区
                                        xoffset = 435;
                                        yoffset = 19;
                                    }
                                    else if (_AccInfo.zoneid == 2)
                                    {
                                        //2区
                                        xoffset = 535;
                                        yoffset = 19;
                                    }
                                    else if (_AccInfo.zoneid == 3)
                                    {
                                        //3区
                                        xoffset = 635;
                                        yoffset = 19;
                                    }
                                    else if (_AccInfo.zoneid == 4)
                                    {
                                        //网1
                                        xoffset = 435;
                                        yoffset = 44;
                                    }
                                    else
                                    {
                                        //网2
                                        xoffset = 535;
                                        yoffset = 44;
                                    }

                                    if (CommonApi.FindPic(x + xoffset, y + yoffset, 30, 30, @".\BMP\选区按钮.bmp", 0.99, out tx, out ty))
                                    {
                                        Sleep(3000);
                                        CommonApi.Left_Click(tx + 3, ty + 3);
                                        Global.logger.Debug("执行选区" + _AccInfo.zoneid);
                                    }

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

                    } break;
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
                    } break;
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
                                Global.logger.Debug("在这里卡死了????");
                                //_NextState = State.JieShu;
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
                            int close_Lasttime = System.Environment.TickCount;
                            int wjdc_Lasttime = System.Environment.TickCount;       //问卷调查
                            int mmx_Lasttime = System.Environment.TickCount;        //密码箱

                            int wujuese_Interval = 0;
                            int queren_Interval = 5000;
                            int quxiao_Interval = 4000;
                            int close_Interval = 4000;

                            int movemouse_Lasttime = System.Environment.TickCount;

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
                                else
                                {
                                    CommonApi.GetWindowXYWH(hwnd, out sX, out sY, out sW, out sH);
                                    if (sW <= 800 || sH <= 600)
                                    {
                                        Global.logger.Debug("窗口过小 {0}*{1}", sW, sH);
                                        break;
                                    }
                                }

                                if (System.Environment.TickCount - movemouse_Lasttime > 5000)
                                {
                                    CommonApi.Mouse_Move(0, 0);
                                    movemouse_Lasttime = System.Environment.TickCount;
                                }

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
                                        Sleep(500);
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
                                                else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\密码有误停封.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogForbidden(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(1000, "账号停封,关闭游戏");
                                                }
                                                else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\连续输入错误.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogFailed(_curAccInfo);
                                                    SendMsg("6$" + "changeip$连续输入错误", true);
                                                    //Global.logger.Debug("连续输入错误,执行重启操作");
                                                    //csLoginTool.RegAutoStart(true);
                                                    //System.Diagnostics.Process.Start("shutdown", @"/r");

                                                    _NextState = State.JieShu;
                                                    Sleep(3000, "连续输入错误,关闭游戏");
                                                }
                                                else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\服务器连接中断.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);

                                                    SendLogFailed(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(3000, "服务器连接中断,关闭游戏");

                                                }
                                                else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\管理员中断.bmp", 0.97, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);
                                                    SendLogForbidden(_curAccInfo);
                                                    _NextState = State.JieShu;

                                                    Sleep(3000, "服务器连接中断,关闭游戏");
                                                }
                                                else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\过于频繁.bmp", 0.97, out dx, out dy))
                                                {
                                                    CommonApi.CloseWindow(hwnd);
                                                    SendLogFailed(_curAccInfo);
                                                    _NextState = State.JieShu;
                                                    SendMsg("6$" + "changeip$过于频繁", true);
                                                    Sleep(3000, "过于频繁");
                                                }

                                                yanzhengma_Lastime = System.Environment.TickCount;
                                                xinbinbaodao_Lasttime = System.Environment.TickCount;
                                                wujuese_Lasttime = System.Environment.TickCount;
                                                queren_Lasttime = System.Environment.TickCount;
                                                quxiao_Lasttime = System.Environment.TickCount;
                                                mimacuowu_Lasttime = System.Environment.TickCount;
                                                tingfeng_Lasttime = System.Environment.TickCount;
                                                close_Lasttime = System.Environment.TickCount;
                                                wjdc_Lasttime = System.Environment.TickCount;
                                                mmx_Lasttime = System.Environment.TickCount;

                                                break;
                                            }
                                            Sleep(2000);
                                        } while (System.Environment.TickCount - nowTick < 10 * 1000);

                                        break;
                                    }

                                    if (!bInputPwd)
                                    {
                                        break;
                                    }

                                    if (bInputPwd
                                        //&& CheckInterLastTime(ref yanzhengma_Lastime, 2000 + _Rand(2000)) 
                                        && CommonApi.FindPic(sX + 352, sY + 255, 100, 50, @".\BMP\验证码.bmp", 0.97, out dx, out dy))
                                    {
                                        string path = csLoginTool.GamePath.Substring(0, csLoginTool.GamePath.LastIndexOf("\\") + 1) + "capt.jpg";
                                        if (File.Exists(path))
                                        {
                                            string old_md5 = "";
                                            while (true)
                                            {
                                                Sleep(1000);
                                                byte[] img_buffer = File.ReadAllBytes(path);
                                                MD5 md5 = new MD5CryptoServiceProvider();
                                                byte[] md5_bytes = md5.ComputeHash(img_buffer, 0, img_buffer.Length);
                                                string md5_str = BitConverter.ToString(md5_bytes).Replace("-", "");
                                                if (csLoginTool.Lg.ContainsKey(md5_str))
                                                {
                                                    CommonApi.Left_Click(dx + 125, dy + 200);
                                                    Sleep(200);
                                                    CommonApi.Left_Click(dx + 125, dy + 200);
                                                    Sleep(200);
                                                    SendKeys.SendWait("{Delete}");
                                                    Sleep(200);
                                                    SendKeys.SendWait(csLoginTool.Lg[md5_str]);
                                                    Sleep(1000);
                                                    CommonApi.Left_Click(dx + 144, dy + 265);
                                                    Sleep(200);
                                                    CommonApi.Left_Click(dx + 144, dy + 265);
                                                    Sleep(500);

                                                    int nowTick = System.Environment.TickCount;
                                                    do
                                                    {
                                                        if (!CommonApi.FindPic(sX + 352, sY + 255, 100, 50, @".\BMP\验证码.bmp", 0.97, out dx, out dy))
                                                        {
                                                            if (CommonApi.FindPic(x, y, w, h - 30, @".\BMP\密码错误.bmp", 0.99, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);

                                                                SendLogPasswordError(_curAccInfo);
                                                                _NextState = State.JieShu;

                                                                Sleep(3000, "密码错误,关闭游戏");

                                                            }
                                                            else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\密码有误停封.bmp", 0.99, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);

                                                                SendLogForbidden(_curAccInfo);
                                                                _NextState = State.JieShu;

                                                                Sleep(1000, "账号停封,关闭游戏");
                                                            }
                                                            else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\连续输入错误.bmp", 0.99, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);

                                                                SendMsg("6$" + "changeip$连续输入错误", true);
                                                                //Sleep(60 * 1000, "连续输入错误");

                                                                SendLogFailed(_curAccInfo);

                                                                _NextState = State.JieShu;
                                                                Sleep(3000, "连续输入错误,关闭游戏");
                                                            }
                                                            else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\服务器连接中断.bmp", 0.99, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);

                                                                SendLogFailed(_curAccInfo);
                                                                _NextState = State.JieShu;

                                                                Sleep(3000, "服务器连接中断,关闭游戏");

                                                            }
                                                            else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\管理员中断.bmp", 0.97, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);
                                                                SendLogForbidden(_curAccInfo);
                                                                _NextState = State.JieShu;

                                                                Sleep(3000, "服务器连接中断,关闭游戏");
                                                            }
                                                            else if (CommonApi.FindPic(sX + 292, sY + 292, 439, 238, @".\BMP\过于频繁.bmp", 0.97, out dx, out dy))
                                                            {
                                                                CommonApi.CloseWindow(hwnd);
                                                                SendLogFailed(_curAccInfo);
                                                                _NextState = State.JieShu;
                                                                SendMsg("6$" + "changeip$过于频繁", true);
                                                                Sleep(3000, "过于频繁");
                                                            }


                                                            yanzhengma_Lastime = System.Environment.TickCount;
                                                            xinbinbaodao_Lasttime = System.Environment.TickCount;
                                                            wujuese_Lasttime = System.Environment.TickCount;
                                                            queren_Lasttime = System.Environment.TickCount;
                                                            quxiao_Lasttime = System.Environment.TickCount;
                                                            mimacuowu_Lasttime = System.Environment.TickCount;
                                                            tingfeng_Lasttime = System.Environment.TickCount;
                                                            close_Lasttime = System.Environment.TickCount;
                                                            wjdc_Lasttime = System.Environment.TickCount;
                                                            mmx_Lasttime = System.Environment.TickCount;

                                                            break;
                                                        }
                                                        Sleep(2000);
                                                    } while (System.Environment.TickCount - nowTick < 5 * 1000);
                                                    break;
                                                }
                                                else
                                                {
                                                    CommonApi.Left_Click(dx + 207, dy + 118);
                                                    if (md5_str != old_md5)
                                                    {
                                                        SendMsg("7$" + md5_str + "$" + Convert.ToBase64String(img_buffer));
                                                    }
                                                    old_md5 = md5_str;
                                                }
                                            }
                                        }
                                        break;
                                    }

                                    if (bInputPwd
                                        && CheckInterLastTime(ref wujuese_Lasttime, wujuese_Interval)
                                        && CommonApi.FindPic(sX + 411, sY + 340, 200, 40, @".\BMP\无角色.bmp", 0.99, out dx, out dy))
                                    {
                                        wujuese_Interval = 5000;
                                        queren_Interval = 500;
                                        quxiao_Lasttime = 500;

                                        string s = RandomString.Next(1, "A-Z") + RandomString.Next(9, "a-z1-9");

                                        CommonApi.Left_Click(dx + 152, dy + 76);
                                        Sleep(300);
                                        CommonApi.Left_Click(dx + 152, dy + 76);
                                        Sleep(500);
                                        SendKeys.SendWait("{Delete}");
                                        Sleep(500);
                                        SendKeys.SendWait(s);
                                        Sleep(500);
                                        CommonApi.Left_Click(dx + 83, dy + 115);
                                        Sleep(1000);

                                        int bt = System.Environment.TickCount;
                                        do
                                        {
                                            if (CommonApi.FindPic(sX + 405, sY + 386, 68, 24, @".\BMP\确认昵称.bmp", 0.99, out dx, out dy))
                                            {
                                                Sleep(200);
                                                CommonApi.Left_Click(dx + 69, dy + 38);
                                                Sleep(500);
                                                break;
                                            }
                                            Sleep(2000);
                                        } while (System.Environment.TickCount - bt > 7 * 1000);
                                        break;
                                    }

                                    if (bInputPwd
                                        && CheckInterLastTime(ref wjdc_Lasttime, 2000)
                                        && CommonApi.FindPic(sX + 318, sY + 167, 72, 26, @".\BMP\问卷调查.bmp", 0.97, out dx, out dy))
                                    {
                                        CommonApi.Left_Click(dx + 225, dy + 441);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd && CommonApi.FindPic(sX + 424, sY + 608, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd && CommonApi.FindPic(sX + 596, sY + 616, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd && CommonApi.FindPic(sX + 458, sY + 426, 80, 30, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd && CommonApi.FindPic(sX + 481, sY + 419, 80, 35, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd 
                                        && CommonApi.FindPic(sX + 480, sY + 600, 80, 48, @".\BMP\毫礼确认.bmp", 0.97, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd
                                        && CommonApi.FindPic(sX + 486, sY + 460, 63, 27, @".\BMP\毫礼确认.bmp", 0.97, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CheckInterLastTime(ref queren_Lasttime, queren_Interval)
                                        && CommonApi.FindPic(x, y, w, h, @".\BMP\毫礼确认.bmp", 0.99, out dx, out dy))
                                    {
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (bInputPwd
                                        && CommonApi.FindPic(sX + 531, sY + 598, 88, 44, @".\BMP\调查取消.bmp", 0.97, out dx, out dy))
                                    {
                                        queren_Lasttime = System.Environment.TickCount;
                                        CommonApi.Mouse_Move(dx + 5, dy + 5);
                                        Sleep(100);
                                        CommonApi.Left_Click(dx + 5, dy + 5);
                                        Sleep(200);

                                        break;
                                    }

                                    if (CheckInterLastTime(ref quxiao_Lasttime, quxiao_Interval)
                                        && CommonApi.FindPic(x, y, w, h, @".\BMP\关闭.bmp", 0.98, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 关闭");
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    if (CheckInterLastTime(ref close_Lasttime, close_Interval)
                                        && CommonApi.FindPic(sX + 550, sY + 469, 50, 50, @".\BMP\关闭B.bmp", 0.98, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 关闭B");
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    if (bInputPwd
                                        && CommonApi.FindPic(sX + 672, sY + 640, 58, 29, @".\BMP\关闭B.bmp", 0.99, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 关闭B");
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    if (bInputPwd
                                        && CommonApi.FindPic(sX + 676, sY + 582, 58, 29, @".\BMP\关闭B.bmp", 0.98, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 关闭B");
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    //关闭战场补给
                                    if (bInputPwd
                                        && CommonApi.FindPic(sX + 787, sY + 724, 58, 29, @".\BMP\关闭B.bmp", 0.99, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 关闭B  787,724");
                                        CommonApi.Left_Click(dx + 5, dy + 5);

                                        break;
                                    }

                                    if (bInputPwd
                                         && CommonApi.FindPic(sX + 652, sY + 493, 94, 35, @".\BMP\路路通奖励.bmp", 0.99, out dx, out dy))
                                    {
                                        Global.logger.Debug("打开路路通界面");

                                        string ret = "无神器";
                                        if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\生命收割者.bmp", 0.99, out dx, out dy))
                                        {
                                            ret = "生命收割者";
                                        }
                                        //else if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\爆炎蒸汽.bmp", 0.99, out dx, out dy))
                                        //{
                                        //    ret = "爆炎蒸汽";
                                        //}
                                        else if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\雷神.bmp", 0.99, out dx, out dy))
                                        {
                                            ret = "雷神";
                                        }
                                        else if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\黑龙炮.bmp", 0.99, out dx, out dy))
                                        {
                                            ret = "黑龙炮";
                                        }
                                        else if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\海皇之怒.bmp", 0.99, out dx, out dy))
                                        {
                                            ret = "海皇之怒";
                                        }
                                        else if (CommonApi.FindPic(sX + 650, sY + 586, 125, 29, @".\BMP\盘龙血煞.bmp", 0.99, out dx, out dy))
                                        {
                                            ret = "盘龙血煞";
                                        }

                                        Bitmap screen = new Bitmap(54, 34);
                                        Graphics g = Graphics.FromImage(screen);
                                        g.CopyFromScreen(sX + 652, sY + 245, 0, 0, screen.Size);
                                        g.Dispose();
                                        screen.Save("ttmp.bmp");
                                        string jifen = CommonApi.RecognizeNumber(screen);
                                        screen.Dispose();
                                        if (jifen == "")
                                        {
                                            jifen = "-1";
                                        }
                                        if (ret == "无神器")
                                        {
                                            CommonApi.Left_Click(sX + 699, sY + 625);
                                            Sleep(1000);
                                            if (CommonApi.FindPic(sX + 374, sY + 376, 289, 64, @".\BMP\重置提示.bmp", 0.99, out dx, out dy))
                                            {
                                                if (CommonApi.FindPic(sX + 430, sY + 410, 122, 92, @".\BMP\免费充值确认.bmp", 0.99, out dx, out dy))
                                                {
                                                    CommonApi.Left_Click(dx + 5, dy + 5);
                                                }
                                                Sleep(1000);
                                                if (CommonApi.FindPic(sX + 435, sY + 378, 138, 40, @".\BMP\欢乐积分不足.bmp", 0.99, out dx, out dy))
                                                {
                                                    SendLogSucess(_AccInfo, ret, jifen);
                                                    Sleep(2000);

                                                    _NextState = State.JieShu;
                                                    CommonApi.CloseWindow(hwnd);

                                                    Sleep(4000, "欢乐一线牵查询结束----" + ret + "&" + jifen);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            SendLogSucess(_AccInfo, ret, jifen);
                                            Sleep(2000);

                                            _NextState = State.JieShu;
                                            CommonApi.CloseWindow(hwnd);

                                            Sleep(4000, "欢乐一线牵查询结束----" + ret + "&" + jifen);
                                        }

                                        break;
                                    }

                                    notFindPic = true;

                                    if (bInputPwd 
                                        && CheckInterLastTime(ref mmx_Lasttime, 1000)
                                        && CommonApi.FindPic(sX + sW - 380, sY + sH - 42, 122, 42, @".\BMP\欢乐一线牵.bmp", 0.999, out dx, out dy))
                                    {
                                        Global.logger.Debug("click 欢乐一线牵");
                                        CommonApi.Mouse_Move(dx + 6, dy + 6);
                                        Sleep(1500);
                                        CommonApi.Left_Click(dx + 6, dy + 6);
                                        Sleep(1000);
                                        CommonApi.Double_Click(dx + 6, dy + 6);

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
                        } while (true);
                    } break;
            }
        }

        bool bInputPwd = false;

        Random rd = new Random();
        private int _Rand(int maxValue)
        {
            return rd.Next(maxValue);
        }

        private bool CheckInterLastTime(ref int lastTime, int interval)
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
            while (LoginManage.isStop)
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

        private bool SendMsg(string msg, bool waitsucess = false)
        {
            do
            {
                lock (LoginManage.m_session)
                {
                    if (LoginManage.m_session != null)
                    {
                        if (LoginManage.m_session.SendMsg(msg))
                        {
                            return true;
                        }
                    }
                }
                if (!waitsucess)
                    break;
                Sleep(1000);
                Global.logger.Debug("发送失败, 等待发送:" + msg);
            } while (true);
            return false;
        }

        private void SendLogSucess(userInfo account,string guns, string jifen)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                breportok = SendMsg("3$" + account.account + "$" + "OK" + "$0$" + guns + "$" + jifen);
                if (!breportok)
                {
                    report = "3$" + account.account + "$" + "OK" + "$0$" + guns + "$" + jifen;
                }
            }
        }

        private void SendLogFailed(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                breportok = SendMsg("3$" + account.account + "$" + "Failed");
                if (!breportok)
                {
                    report = "3$" + account.account + "$" + "Failed";
                }
            }
        }

        private void SendLogPasswordError(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                breportok = SendMsg("3$" + account.account + "$" + "PasswordError");
                if (!breportok)
                {
                    report = "3$" + account.account + "$" + "PasswordError";
                }
            }
        }

        private void SendLogForbidden(userInfo account)
        {
            if (!bSendRet)
            {
                bSendRet = true;
                breportok = SendMsg("3$" + account.account + "$" + "Forbidden");
                if (!breportok)
                {
                    report = "3$" + account.account + "$" + "Forbidden";
                }
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
        //Session m_client;

        bool bSendRet = false;

        bool breportok = false;
        string report = "";

        public csLoginTool _loginTool = csLoginTool.Instance;

        static UidBackup m_uidbacker = new UidBackup();
        delegate R Delegate0<R>();

        static NumberAnalysis s_numberanalysis = new NumberAnalysis();
    }
}
