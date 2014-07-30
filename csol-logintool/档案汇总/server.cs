using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace 档案汇总
{
    class Sever
    {
        Socket m_Acceptor;

        string IP = GetLocalIp();
        int port = 28015;

        logHandle m_logHandle;

        public void BeginListen()
        {
            //try
            //{
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
    }


    class Session
    {
        Socket m_sock;
        byte[] m_recvBuffer;
        List<byte> m_buffer;

        static public msgHandle m_msgHandle;
        static public logHandle m_logHandle;

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
                    Array.Copy(m_recvBuffer,recv,nRecv);
                    Parse(recv);
                }
                m_sock.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
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
                m_msgHandle(s, this);
            }
        }

        public void Send(string msg)
        {
            try
            {
                stMsg s = new stMsg();
                s.Cmd = msg;
                byte[] buffer = Bit.StructToBytes<stMsg>(s);
                m_sock.BeginSend(buffer,0,buffer.Length,SocketFlags.None,new AsyncCallback(SendCallback),null);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                OnError(ex);
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
        }
    }

    class SessionManage
    {

    }

    delegate void msgHandle(string s,Session c);
    delegate void logHandle(string s);
}
