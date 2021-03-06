﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Compression;
using System.IO;

namespace 查号器
{
    class Session
    {
        Socket m_sock = null;
        Byte[] m_recvBuffer = new Byte[1024];
        List<byte> m_buffer = new List<byte>();

        bool m_isOk = false;

        msgHandle m_msgHandle;

        public static string IP = "";
        static int port = 28016;

        public string m_code = "";

        public Session()
        {
            try
            {
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), m_sock);
                Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " 开始连接" + IP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        private void onSend(IAsyncResult ar)
        {
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                m_isOk = true;
                m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

                SendMsg("100$" + CommonApi.GetMacAddress() + "$" + m_code);
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
                    lock (m_recvBuffer)
                    {
                        Array.Copy(m_recvBuffer, recv, nRecv);
                        Parse(recv);
                    }
                    m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                }
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
                try
                {
                    byte[] buffer = m_buffer.ToArray();
                    MsgHeader head = Bit.BytesToStruct<MsgHeader>(buffer, 0);
                    int head_len = Marshal.SizeOf(typeof(MsgHeader));

                    if (head.wMsgLen + head_len <= buffer.Length)
                    {
                        string s = null;
                        if (head.btRar == 1)
                        {
                            byte[] raw = Decompress(buffer, head_len, head.wMsgLen);
                            s = Encoding.Default.GetString(buffer, head_len, head.wMsgLen);
                        }else
                            s = Encoding.Default.GetString(buffer, head_len, head.wMsgLen);
                        OnMsg(s);
                        m_buffer.RemoveRange(0, head.wMsgLen + head_len);
                        if (m_buffer.Count == 0)
                            break;
                    }
                    else
                        break;
                }
                catch
                {
                    break;
                }
            } while (true);
        }

        private void OnMsg(string s)
        {
            if (m_msgHandle != null)
            {
                m_msgHandle(s);
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
                MsgHeader head = new MsgHeader();
                byte[] msg_buffer = System.Text.Encoding.Default.GetBytes(msg);
                if (msg_buffer.Length > 300)
                {
                    head.btRar = 1;
                    msg_buffer = Compress(msg_buffer);
                }
                else
                {
                    head.btRar = 0;
                }
                head.wMsgLen = msg_buffer.Length;
                byte[] head_buffer = Bit.StructToBytes<MsgHeader>(head);

                List<byte> send_buffer = new List<byte>();
                send_buffer.AddRange(head_buffer);
                send_buffer.AddRange(msg_buffer);
                m_sock.BeginSend(send_buffer.ToArray(), 0, send_buffer.Count, 0, new AsyncCallback(onSend), m_sock);
                Global.logger.Debug("发送字节:"+ send_buffer.Count + "&发送内容:" + msg);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
        static byte[] Decompress(byte[] gzip,int idx ,int len)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip,idx,len), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
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
                    Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + "连接" + IP + "失败,套接字句柄:" + m_sock.Handle + "," + Sec + "秒之后尝试重新连接...");
                }
                else
                {
                    Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + ex.Message + "......" + Sec + "秒之后尝试重新连接...");
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
