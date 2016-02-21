using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace 档案汇总
{
    public delegate void OnMsgAction(byte[] msg);
    public delegate void OnDisConnectAction();

    class AmSession : QzTcpClinet
    {
        public OnMsgAction OnMsgRcive;
        public OnDisConnectAction OnDisconnet;
        public string m_mac = "";
        public string m_code = "";
        public bool m_sp = false;
        public int m_CreateTime;
        public int m_LastCheckActiveTime;

        public AmSession(TcpClient clinet)
            : base(clinet)
        {

        }

        protected override void OnMsg(byte[] msg)
        {
            if (OnMsgRcive != null){
               OnMsgRcive.Invoke(msg);
            }
        }

        public string RemoteIp
        {
            get { return getTcpClient().Client.RemoteEndPoint.ToString(); }
        }

        public void Send(string s)
        {
            byte[] sendbytes = Encoding.Default.GetBytes(s);
            base.SendCmd(sendbytes);
        }
    }
}
