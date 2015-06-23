using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.IO.Compression;

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
        static int port = 723;

        public string m_code = "";

        public Action<Exception, Session> OnException;

        public Session()
        {
            try
            {
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), m_sock);
                Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " 开始连接" + IP);
            }
            catch (Exception ex)
            {
                OnError(ex, this);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var tuple = ar.AsyncState as Tuple<Socket, byte[]>;
                Socket so = tuple.Item1;
                byte[] buf = tuple.Item2;

                int bytes = so.EndSend(ar);
                if (bytes < buf.Length)
                {
                    byte[] newbuf = new byte[buf.Length - bytes];
                    Array.Copy(buf, 0, newbuf, 0, newbuf.Length);
                    so.BeginSend(newbuf, 0, newbuf.Length, SocketFlags.None, SendCallback, new Tuple<Socket, byte[]>(so, newbuf));
                }
            }
            catch (Exception ex)
            {
                OnError(ex, this);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                m_isOk = true;
                Socket socket = (Socket)ar.AsyncState;
                socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);

                SendMsg("100$" + CommonApi.GetMacAddress() + "$" + m_code + "$" + string.Format(" {0:yy-MM-dd HH:mm:ss} Version {1}.{2}.{3}"
                    , System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location)
                    , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
                    , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor
                    , System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build));
            }
            catch (Exception ex)
            {
                OnError(ex, this);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int nRecv = socket.EndReceive(ar);
                
                if (nRecv > 0)
                {
                    try
                    {
                        byte[] recv = new byte[nRecv];
                        lock (m_recvBuffer)
                        {
                            Array.Copy(m_recvBuffer, recv, nRecv);
                            Parse(recv);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Global.logger.Debug(ex.ToString());
                    }
                    socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
                }
            }
            catch (Exception ex)
            {
                OnError(ex, this);
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
                        }
                        else
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

        public bool SendMsg(string msg)
        {
            try
            {
                if (!m_isOk)
                {
                    Global.logger.Debug("msg:{0},由于套接字未连接,发送失败!", msg);
                    return false;
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
                m_sock.BeginSend(send_buffer.ToArray(), 0, send_buffer.Count, 0, new AsyncCallback(SendCallback), new Tuple<Socket, byte[]>(m_sock, send_buffer.ToArray()));
                Global.logger.Debug("发送内容:" + msg);
                return true;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex, this);
            }
            return false;
        }

        void OnError(Exception ex , Session s)
        {
            if (OnException != null)
            {
                OnException.Invoke(ex, s);
            }
            //try
            //{
            //    m_isOk = false;
            //    m_recvBuffer = new Byte[1024];
            //    m_buffer = new List<byte>();

            //    int Sec = 3;
            //    Global.logger.Debug(ex.ToString());
            //    Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + "连接" + IP + "失败,套接字句柄:" + m_sock.Handle + "," + Sec + "秒之后尝试重新连接...");

            //    Thread.Sleep(Sec * 1000);
            //    m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), m_sock);
            //}
            //catch (Exception ex1)
            //{
            //    OnError(ex1);
            //}
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
        static byte[] Decompress(byte[] gzip, int idx, int len)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip, idx, len), CompressionMode.Decompress))
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

        public void SetMsgHandle(msgHandle h)
        {
            m_msgHandle = h;
        }
    }

    delegate void msgHandle(string s);
}
