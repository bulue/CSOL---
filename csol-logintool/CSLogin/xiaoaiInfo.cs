﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSLogin
{
    public partial class xiaoaiInfo : Form
    {
        public xiaoaiInfo()
        {
            InitializeComponent();
            this.userStr.Text = LoginState.xiaoaiUserStr;

            if (userStr.Text != "")
            {
            } 
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            LoginState.xiaoaiUserStr = this.userStr.Text;
            this.Close();
        }
    }
}
