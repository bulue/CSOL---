﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSLogin
{
    class Session
    {
        Socket m_sock = null;
        Byte[] m_recvBuffer = new Byte[1024];
        List<byte> m_buffer = new List<byte>();

        bool m_isOk = false;

        msgHandle m_msgHandle;

        public static string IP = "";
        static int port = 28015;

        public Session()
        {
            try
            {
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), m_sock);
                CommonApi.TraceInfo("线程ID:" + Thread.CurrentThread.ManagedThreadId + " 开始连接" + IP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                m_isOk = true;
                m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nRecv = m_sock.EndReceive(ar);
                if (nRecv > 0)
                {
                    byte[] recv = new byte[nRecv];
                    Array.Copy(m_recvBuffer, recv, nRecv);
                    Parse(recv);
                }
                m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch(System.ArgumentException )
            {

            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        private void Parse(byte[] ba)
        {
            m_buffer.AddRange(ba);
            do
            {
                bool loopBreak = true;

                byte[] buffer = m_buffer.ToArray();

                try
                {
                    stMsg s = Bit.BytesToStruct<stMsg>(buffer, 0);
                    OnMsg(s.Cmd);
                    m_buffer.RemoveRange(0, Marshal.SizeOf(typeof(stMsg)));
                }
                catch
                {
                    loopBreak = true;
                }

                if (loopBreak)
                    break;
            } while (true);
        }

        private void OnMsg(string s)
        {
            if (m_msgHandle != null)
            {
                m_msgHandle(s);
            }
            else
            {
                CommonApi.TraceInfo("套接字没有找到处理函数!!!!");
            }
        }

        public void SendMsg(string msg)
        {
            try
            {
                if (!m_isOk)
                {
                    return;
                }
                stMsg s = new stMsg();
                s.Cmd = msg;
                byte[] buffer = Bit.StructToBytes<stMsg>(s);
                //byte[] buffer = System.Text.Encoding.Default.GetBytes(msg);
                m_sock.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallBack), m_sock);
                CommonApi.TraceInfo("发送内容:" + msg);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        void OnError(System.Net.Sockets.SocketException ex)
        {
            try
            {
                m_isOk = false;
                int Sec = 5;
                if (ex.ErrorCode == 10057)
                {
                    CommonApi.TraceInfo("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + "连接" + IP + "失败,套接字句柄:" + m_sock.Handle + "," + Sec + "秒之后尝试重新连接...");
                }
                else
                {
                    CommonApi.TraceInfo("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + ex.Message + "......" +Sec + "秒之后尝试重新连接...");
                }

                Thread.Sleep(Sec * 1000);
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), null);
            }
            catch (System.Net.Sockets.SocketException ex1)
            {
                OnError(ex1);
            }
        }

        public void SetMsgHandle(msgHandle h)
        {
            m_msgHandle = h;
        }
    }

    delegate void msgHandle(string s);
}