using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dama2Lib;

namespace CSLogin
{
    public partial class dama2Info : Form
    {
        public dama2Info()
        {
            InitializeComponent();

            int ret = Dama2.Init("CSOL登陆器", "c5aff0c4d218205cd5f762ca9acaa81e");

            if (ret != 0)
            {
                MessageBox.Show("dama2初始化失败!!");
                return;
            }


            StringBuilder sysAnnUrl = new StringBuilder(4096);
            StringBuilder appAnnUrl = new StringBuilder(4096);
            ret = Dama2.Login("xiaozhuhaoa", "19881226", "", sysAnnUrl, appAnnUrl);

            if (ret != 0)
            {
                MessageBox.Show("dama2登陆失败!!");
            }

            this.userName.Text = "xiaozhuhaoa";

            uint ulBalance = 0;
            ret = Dama2.QueryBalance(ref ulBalance);
            if (ret != 0)
            {
                MessageBox.Show("dama2查询余额失败");
            }
            this.Socre.Text = "" + ulBalance;
        }
    }
}
