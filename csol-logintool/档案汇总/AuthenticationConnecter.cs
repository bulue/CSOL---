using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 档案汇总
{
    public class AuthenticationConnecter : Connecter
    {
        public Grid m_manage;

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
