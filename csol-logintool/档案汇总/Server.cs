using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace 档案汇总
{
    class QzServer
    {
        TcpListener m_listener;

        public QzServer(IPAddress localaddr, int port)
        {
            m_listener = new TcpListener(localaddr, port);
        }

        public virtual void Start()
        {
            m_listener.Start();
            m_listener.BeginAcceptTcpClient(new AsyncCallback(OnAccept), this);
        }

        public virtual void Stop()
        {
            m_listener.Stop();
        }

        void OnAccept(IAsyncResult ar)
        {
            try
            {
                TcpClient newClient = m_listener.EndAcceptTcpClient(ar);
                QzTcpClinet qClient = CreateClient(newClient);
                qClient.Start();
                m_listener.BeginAcceptTcpClient(new AsyncCallback(OnAccept), this);
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
            }
        }

        protected virtual QzTcpClinet CreateClient(TcpClient client)
        {
            return new QzTcpClinet(client);
        }
    }
}
