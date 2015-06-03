using System;
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

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                m_isOk = true;
                Socket socket = (Socket)ar.AsyncState;
                socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);

                SendMsg("100$" + CommonApi.GetMacAddress() + "$" + m_code);
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
        }

        public void SendMsg(string msg)
        {
            try
            {
                if (!m_isOk)
                {
                    Global.logger.Debug("msg:{0},由于套接字未连接,发送失败!", msg);
                    return;
                }
                stMsg s = new stMsg();
                s.Cmd = msg;
                byte[] buffer = Bit.StructToBytes<stMsg>(s);
                //byte[] buffer = System.Text.Encoding.Default.GetBytes(msg);
                m_sock.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), new Tuple<Socket, byte[]>(m_sock,buffer));
                Global.logger.Debug("发送内容:" + msg);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
            }
        }

        void OnError(Exception ex)
        {
            try
            {
                m_isOk = false;
                int Sec = 5;
                Global.logger.Debug(ex.ToString());
                Global.logger.Info("线程ID:" + Thread.CurrentThread.ManagedThreadId + " " + "连接" + IP + "失败,套接字句柄:" + m_sock.Handle + "," + Sec + "秒之后尝试重新连接...");

                Thread.Sleep(Sec * 1000);
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_sock.BeginConnect(IP, port, new AsyncCallback(OnConnect), m_sock);
            }
            catch (Exception ex1)
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
