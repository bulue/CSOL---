using System;
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
using System.Security.Cryptography;
using CommonQ;

namespace CSLogin
{
    class userInfo{
        public string account;
        public string pwd;
        public int zoneid = 1;
        public int state = 0;

        public userInfo(string a, string p,int z, int s)
        {
            account = a;
            pwd = p;
            zoneid = z;
            state = s;
        }
    }

    class LoginManage
    {
        static public userInfo m_account;

        public string m_Code = "";
        public string m_ManageIp = "";
        public bool m_modeHangup = false;

        static public Session m_session;
        static public bool IsConnecting = false;
        string MacId;       //物理地址

        public Thread thread;

        int _lastCheckRebootTick = System.Environment.TickCount;

        static public bool isStop = false;

        public LoginManage(csLoginTool loginTool)
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
                            string zoneId = split[3];
                            string state = split[4];

                            lock (this)
                            {
                                m_account = new userInfo(accName, passWord, Convert.ToInt32(zoneId), Convert.ToInt32(state));
                            }
                        }
                        break;
                    case "101":
                        {
                            string reboot = split[1];
                            if (reboot == "reboot" && !m_modeHangup)
                            {
                                Global.logger.Info("收到重启系统的请求,执行");
                                csLoginTool.RegAutoStart(true);
                                System.Diagnostics.Process.Start("shutdown", @"/r");
                                System.Environment.Exit(0);
                            }
                        }break;
                    case "102":
                        {
                            string changeip = split[1];
                            if (changeip == "changeip")
                            {
                                Global.logger.Info("收到换ip的系统的请求,执行");
                                Process.Start("rasdial", "/DISCONNECT");
                            }
                        }break;
                }
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }

        void OnSessionException(Exception ex, Session s)
        {
            lock (this)
            {
                Global.logger.Debug(ex.ToString());
                if (m_session == s)
                {
                    m_session = null;
                    Global.logger.Debug("连接断开,3秒之后尝试重新连接...");
                    m_session = new Session();
                    m_session.m_code = m_Code;
                    m_session.SetMsgHandle(csLoginTool.Instance.OnMsg);
                    m_session.OnException = OnSessionException;
                }
            }
        }

        public void Run()
        {
            try
            {
                lock (this)
                {
                    if (m_session == null)
                    {
                        Session.IP = m_ManageIp;
                        Session.spmode = m_modeHangup;
                        m_session = new Session();
                        m_session.m_code = m_Code;
                        m_session.SetMsgHandle(csLoginTool.Instance.OnMsg);
                        m_session.OnException = OnSessionException;
                        Thread.Sleep(2000);
                    }
                }

                if (!m_modeHangup)
                {
                    thread = new Thread(new ThreadStart(RunReboot));
                    thread.Start();

                    long nLastQueryTime = 0;
                    do
                    {
                        Thread.Sleep(1000);

                        lock (this)
                        {
                            if (m_account == null)
                            {
                                if (System.Environment.TickCount - nLastQueryTime > 10 * 1000)
                                {
                                    if (m_session.SendMsg("1$" + MacId + "$" + m_Code))
                                    {
                                        nLastQueryTime = System.Environment.TickCount;
                                    }
                                }
                            }
                            else
                            {
                                if (m_account.state == 1)
                                {
                                    LoginState stateMachine = new LoginState(0);
                                    stateMachine.Run(m_account, m_session);
                                }
                                else if (m_account.state == 2)
                                {
                                    ChipState stateMachine = new ChipState();
                                    stateMachine.Run(m_account, m_session);
                                }
                                else if (m_account.state == 3)
                                {
                                    HuanLeYiXianQianState stateMachine = new HuanLeYiXianQianState();
                                    stateMachine.Run(m_account, m_session);
                                }
                                else if (m_account.state == 4)
                                {
                                    LoginState stateMachine = new LoginState(1);
                                    stateMachine.Run(m_account, m_session);
                                }
                                m_session.SendMsg("4$" + MacId);
                                nLastQueryTime = 0;
                                m_account = null;
                                _lastCheckRebootTick = System.Environment.TickCount;
                            }
                        }
                    } while (true);
                }
                else
                {
                    long beat_Ticks = 0;
                    do
                    {
                        if (System.Environment.TickCount - beat_Ticks > 0)
                        {
                            beat_Ticks = System.Environment.TickCount + 30 * 1000;
                            m_session.SendMsg("1008$heart");
                        }

                        Thread.Sleep(50);
                    } while (true);
                }

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thread Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RunReboot()
        {
            do 
            {
                Thread.Sleep(20 * 1000);
                Global.logger.Debug("自检测检测 {0} sec", (System.Environment.TickCount - _lastCheckRebootTick)/1000);
                if (System.Environment.TickCount - _lastCheckRebootTick > 5 * 60 * 1000)
                {
                    Global.logger.Error("自检测登陆超时,执行重启");
                    csLoginTool.RegAutoStart(true);
                    System.Diagnostics.Process.Start("shutdown", @"/r");
                    System.Environment.Exit(0);
                }
            } while (true);
        }
    }

}
