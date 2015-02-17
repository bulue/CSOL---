using System;
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

namespace 档案汇总
{
    public class Connecter
    {
        Socket m_sock = null;
        Byte[] m_recvBuffer = new Byte[1024];
        List<byte> m_buffer = new List<byte>();

        bool m_isOk = false;

        Action<string> m_msgHandle;
        Action m_onconnecthanle;
        Action<string> m_logHandle;

        string _ip = "";
        int _port = 0;

        public Connecter()
        {
        }

        public void SetAddress(string ip,int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Start()
        {
            try
            {
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(_ip, _port, new AsyncCallback(OnConnect), m_sock);
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

                Socket socket = (Socket)ar.AsyncState;
                socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
                OnStart();
                if (m_onconnecthanle != null)
                {
                    m_onconnecthanle();
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        protected virtual void OnStart()
        {

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

                    Socket socket = (Socket)ar.AsyncState;
                    socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
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
                    MsgHeader1 head = Bit.BytesToStruct<MsgHeader1>(buffer, 0);
                    int head_len = Marshal.SizeOf(typeof(MsgHeader1));

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

        protected virtual void OnMsg(string s)
        {
            if (m_msgHandle != null)
            {
                m_msgHandle(s);
            }
        }

        public void Close()
        {
            if (m_isOk)
            {
                m_sock.Close();
                m_isOk = false;
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
                MsgHeader1 head = new MsgHeader1();
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
                byte[] head_buffer = Bit.StructToBytes<MsgHeader1>(head);

                List<byte> send_buffer = new List<byte>();
                send_buffer.AddRange(head_buffer);
                send_buffer.AddRange(msg_buffer);
                m_sock.BeginSend(send_buffer.ToArray(), 0, send_buffer.Count, 0, new AsyncCallback(onSend), m_sock);
                //Global.logger.Debug("发送字节:"+ send_buffer.Count + "&发送内容:" + msg);
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
                    if (m_logHandle != null)
                    {
                        m_logHandle("连接" + "验证服务器" + "失败,套接字句柄:" + m_sock.Handle + "," + Sec + "秒之后尝试重新连接...");
                    }
                }
                else
                {
                    if (m_logHandle != null)
                    {
                        m_logHandle(ex.Message + "......" + Sec + "秒之后尝试重新连接...");
                    }
                }

                Thread.Sleep(Sec * 1000);
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(_ip, _port, new AsyncCallback(OnConnect), m_sock);
            }
            catch (System.Net.Sockets.SocketException ex1)
            {
                OnError(ex1);
            }
        }

        public void SetMsgHandle(Action<string> h)
        {
            m_msgHandle = h;
        }
    }
}
