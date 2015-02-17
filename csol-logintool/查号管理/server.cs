using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;

namespace 查号管理
{
    class Sever
    {
        Socket m_Acceptor;

        string IP = GetLocalIp();
        int port = 28016;

        static public bool bChanged = false;

        logHandle m_logHandle;

        static public List<Session> m_Clinets = new List<Session>();

        public void BeginListen()
        {
            //try
            //{
                Session.m_errorHandle = OnClientError;
                IPAddress ip = IPAddress.Parse(IP);
                m_Acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IP = GetLocalIp();
                m_Acceptor.Bind(new IPEndPoint(ip, port));  //绑定IP地址：端口  
                m_Acceptor.Listen(1000);
                m_Acceptor.BeginAccept(new AsyncCallback(OnAcceptSocket), m_Acceptor);
            //}
            //catch (System.ObjectDisposedException ex)
            //{
            //    ;
            //}
            //catch (System.Net.Sockets.SocketException ex)
            //{
            //    OnError(ex);
            //}
        }


        private void OnAcceptSocket(IAsyncResult ar)
        {
            try
            {
                Socket acceptor = (Socket)ar.AsyncState;
                Session newClient = new Session(acceptor.EndAccept(ar));

                lock (m_Clinets)
                {
                    m_Clinets.Add(newClient);
                    newClient.Start();
                    bChanged = true;
                }
                
                acceptor.BeginAccept(new AsyncCallback(OnAcceptSocket), acceptor);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                //Print(ex.ToString());
                OnError(ex);
            }

        }

        private void OnError(System.Net.Sockets.SocketException ex)
        {
            if (m_logHandle != null)
            {
                m_logHandle(ex.ToString());
            }
        }

        public static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

        private void OnClientError(System.Net.Sockets.SocketException ex,Session s)
        {
            lock (m_Clinets)
            {
                m_Clinets.Remove(s);
                bChanged = true;
            }
        }
    }


    class Session
    {
        Socket m_sock;
        byte[] m_recvBuffer;
        List<byte> m_buffer;

        public string m_mac;
        public string m_code;

        int m_lastCheckTime;

        static public msgHandle m_msgHandle;
        static public logHandle m_logHandle;
        static public errorHanle m_errorHandle;

        public Session(Socket s)
        {
            m_sock = s;
            m_recvBuffer = new byte[1024];
            m_buffer = new List<byte>();
        }

        public Socket handle
        {
            get
            {
                return m_sock;
            }
        }

        public void Start()
        {
            try
            {
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
                    m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                }
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
                            s = Encoding.Default.GetString(raw, 0, raw.Length);
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
                m_msgHandle.BeginInvoke(s, this,null,null);
            }
        }

        public void Send(string msg)
        {
            try
            {
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
                m_sock.BeginSend(send_buffer.ToArray(), 0, send_buffer.Count, 0, new AsyncCallback(SendCallback), null);
                if (msg != "0")
                {
                    Global.logger.Debug("发送字节:" + send_buffer.Count + "&发送内容:" + msg);
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                //OnError(ex);
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

        static byte[] Decompress(byte[] gzip,int idx,int len)
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

        private void SendCallback(IAsyncResult ar)
        {
        }

        private void OnError(System.Net.Sockets.SocketException ex)
        {
            if (m_logHandle != null)
            {
                m_logHandle(ex.ToString());
            }

            if (ex.SocketErrorCode == SocketError.ConnectionReset
                || ex.SocketErrorCode == SocketError.ConnectionAborted)
            {
                m_errorHandle(ex, this);
            }
        }
    }

    delegate void msgHandle(string s,Session c);
    delegate void logHandle(string s);
    delegate void errorHanle(System.Net.Sockets.SocketException ex,Session s);
}
