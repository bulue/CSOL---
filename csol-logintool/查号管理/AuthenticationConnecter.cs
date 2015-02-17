using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 查号管理
{
    public class AuthenticationConnecter : Connecter
    {
        public GridManage m_manage;

        protected override void OnStart()
        {
            m_manage.OnAuthenticationConnect();
        }

        protected override void OnMsg(string s)
        {
            m_manage.OnAuthenticationMsg(s);
        }
    }
}
