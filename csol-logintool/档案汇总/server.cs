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

namespace 档案汇总
{
    class Sever
    {
        Socket m_Acceptor;

        string IP = GetLocalIp();
        int port = 723;

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
                newClient.Start();
                lock (m_Clinets)
                {
                    m_Clinets.Add(newClient);
                    bChanged = true;
                }

                acceptor.BeginAccept(new AsyncCallback(OnAcceptSocket), acceptor);
            }
            catch (System.Exception ex)
            {
                //Print(ex.ToString());
                OnError(ex);
            }

        }

        private void OnError(System.Exception ex)
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

        private void OnClientError(Exception ex,Session s)
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

        public string m_mac = "";
        public string m_code = "";
        public bool m_sp = false;

        private int m_lastactivetime = 0;

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

        public bool IsActive
        {
            get { return (System.Environment.TickCount - m_lastactivetime < 300 * 1000); }
        }

        public void Terminate()
        {
            handle.Shutdown(SocketShutdown.Both);
            handle.Disconnect(false);
            handle.Dispose();
        }

        public void Start()
        {
            try
            {
                m_lastactivetime = System.Environment.TickCount;
                m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                m_lastactivetime = System.Environment.TickCount;
                int nRecv = m_sock.EndReceive(ar);
                if (nRecv > 0)
                {
                    byte[] recv = new byte[nRecv];
                    Array.Copy(m_recvBuffer, recv, nRecv);
                    Parse(recv);
                    m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                }
                //else
                //{
                //    lock (Sever.m_Clinets)
                //    {
                //        Sever.bChanged = true;
                //        Sever.m_Clinets.Remove(this);
                //    }
                //    m_sock.Shutdown(SocketShutdown.Both);
                //    m_sock.Disconnect(false);
                //    m_sock.Dispose();
                //}
            }
            catch (Exception ex)
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
                            s = Encoding.Default.GetString(raw);
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
                m_lastactivetime = System.Environment.TickCount;
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
            }
            catch (Exception ex)
            {
                OnError(ex);
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

        private void OnError(Exception ex)
        {
            //Global.logger.Debug("客户连接断开:{0},mac{1},远程机代号:{2};{3}", handle.RemoteEndPoint.ToString(), m_mac, m_code, ex.ToString());
            Global.logger.Debug(ex.ToString());
            m_errorHandle(ex, this);
        }
    }

    delegate void msgHandle(string s,Session c);
    delegate void logHandle(string s);
    delegate void errorHanle(Exception ex,Session s);
}
