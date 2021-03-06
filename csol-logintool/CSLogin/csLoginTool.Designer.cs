﻿namespace CSLogin
{
    partial class csLoginTool
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(csLoginTool));
            this.label1 = new System.Windows.Forms.Label();
            this.startBtn = new System.Windows.Forms.Button();
            this.gamePath = new System.Windows.Forms.TextBox();
            this.gamePathPickBtn = new System.Windows.Forms.Button();
            this.PauseBtn = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.autoStartCkbox = new System.Windows.Forms.CheckBox();
            this.autostartTimer = new System.Windows.Forms.Timer(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.CountdownTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_Code = new System.Windows.Forms.TextBox();
            this.SetCodeBtn = new System.Windows.Forms.Button();
            this.SetManageIpBtn = new System.Windows.Forms.Button();
            this.tbxMac = new System.Windows.Forms.TextBox();
            this.cbModeHangup = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cbHangupPerHour = new System.Windows.Forms.CheckBox();
            this.timerHangup = new System.Windows.Forms.Timer(this.components);
            this.tbHangeUpInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "游戏路径";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startBtn.Location = new System.Drawing.Point(339, 270);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(100, 28);
            this.startBtn.TabIndex = 5;
            this.startBtn.Text = "启动";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // gamePath
            // 
            this.gamePath.Location = new System.Drawing.Point(71, 6);
            this.gamePath.Name = "gamePath";
            this.gamePath.Size = new System.Drawing.Size(228, 21);
            this.gamePath.TabIndex = 0;
            // 
            // gamePathPickBtn
            // 
            this.gamePathPickBtn.Location = new System.Drawing.Point(305, 9);
            this.gamePathPickBtn.Name = "gamePathPickBtn";
            this.gamePathPickBtn.Size = new System.Drawing.Size(81, 21);
            this.gamePathPickBtn.TabIndex = 6;
            this.gamePathPickBtn.Text = "选择";
            this.gamePathPickBtn.UseVisualStyleBackColor = true;
            this.gamePathPickBtn.Click += new System.EventHandler(this.gamePathPickBtn_Click);
            // 
            // PauseBtn
            // 
            this.PauseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PauseBtn.Location = new System.Drawing.Point(447, 270);
            this.PauseBtn.Name = "PauseBtn";
            this.PauseBtn.Size = new System.Drawing.Size(100, 28);
            this.PauseBtn.TabIndex = 9;
            this.PauseBtn.Text = "暂停 Ctrl+f12";
            this.PauseBtn.UseVisualStyleBackColor = true;
            this.PauseBtn.Click += new System.EventHandler(this.PauseBtn_Click);
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox.Location = new System.Drawing.Point(21, 97);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(543, 139);
            this.textBox.TabIndex = 10;
            // 
            // autoStartCkbox
            // 
            this.autoStartCkbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autoStartCkbox.AutoSize = true;
            this.autoStartCkbox.Location = new System.Drawing.Point(21, 277);
            this.autoStartCkbox.Name = "autoStartCkbox";
            this.autoStartCkbox.Size = new System.Drawing.Size(96, 16);
            this.autoStartCkbox.TabIndex = 15;
            this.autoStartCkbox.Text = "开机自动启动";
            this.autoStartCkbox.UseVisualStyleBackColor = true;
            this.autoStartCkbox.CheckedChanged += new System.EventHandler(this.autoStartCkbox_CheckedChanged);
            // 
            // autostartTimer
            // 
            this.autostartTimer.Tick += new System.EventHandler(this.autostartTime);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CountdownTime});
            this.statusBar.Location = new System.Drawing.Point(0, 354);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(619, 22);
            this.statusBar.TabIndex = 18;
            this.statusBar.Text = "状态栏";
            // 
            // CountdownTime
            // 
            this.CountdownTime.Name = "CountdownTime";
            this.CountdownTime.Size = new System.Drawing.Size(0, 17);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 19;
            this.label2.Text = "管理机IP";
            // 
            // textBox_IP
            // 
            this.textBox_IP.Location = new System.Drawing.Point(71, 32);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.Size = new System.Drawing.Size(228, 21);
            this.textBox_IP.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "代号";
            // 
            // textBox_Code
            // 
            this.textBox_Code.Location = new System.Drawing.Point(71, 62);
            this.textBox_Code.Name = "textBox_Code";
            this.textBox_Code.Size = new System.Drawing.Size(228, 21);
            this.textBox_Code.TabIndex = 22;
            // 
            // SetCodeBtn
            // 
            this.SetCodeBtn.Location = new System.Drawing.Point(305, 62);
            this.SetCodeBtn.Name = "SetCodeBtn";
            this.SetCodeBtn.Size = new System.Drawing.Size(81, 21);
            this.SetCodeBtn.TabIndex = 23;
            this.SetCodeBtn.Text = "修改";
            this.SetCodeBtn.UseVisualStyleBackColor = true;
            this.SetCodeBtn.Click += new System.EventHandler(this.SetCodeBtn_Click);
            // 
            // SetManageIpBtn
            // 
            this.SetManageIpBtn.Location = new System.Drawing.Point(305, 35);
            this.SetManageIpBtn.Name = "SetManageIpBtn";
            this.SetManageIpBtn.Size = new System.Drawing.Size(81, 21);
            this.SetManageIpBtn.TabIndex = 24;
            this.SetManageIpBtn.Text = "修改";
            this.SetManageIpBtn.UseVisualStyleBackColor = true;
            this.SetManageIpBtn.Click += new System.EventHandler(this.SetManageIpBtn_Click);
            // 
            // tbxMac
            // 
            this.tbxMac.Location = new System.Drawing.Point(14, 330);
            this.tbxMac.Name = "tbxMac";
            this.tbxMac.ReadOnly = true;
            this.tbxMac.Size = new System.Drawing.Size(124, 21);
            this.tbxMac.TabIndex = 25;
            // 
            // cbModeHangup
            // 
            this.cbModeHangup.AutoSize = true;
            this.cbModeHangup.Location = new System.Drawing.Point(21, 299);
            this.cbModeHangup.Name = "cbModeHangup";
            this.cbModeHangup.Size = new System.Drawing.Size(72, 16);
            this.cbModeHangup.TabIndex = 27;
            this.cbModeHangup.Text = "断网模式";
            this.cbModeHangup.UseVisualStyleBackColor = true;
            this.cbModeHangup.CheckedChanged += new System.EventHandler(this.cbModeHangup_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(339, 304);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 28;
            this.button1.Text = "测试断网";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbHangupPerHour
            // 
            this.cbHangupPerHour.AutoSize = true;
            this.cbHangupPerHour.Location = new System.Drawing.Point(139, 277);
            this.cbHangupPerHour.Name = "cbHangupPerHour";
            this.cbHangupPerHour.Size = new System.Drawing.Size(72, 16);
            this.cbHangupPerHour.TabIndex = 29;
            this.cbHangupPerHour.Text = "定时断网";
            this.cbHangupPerHour.UseVisualStyleBackColor = true;
            this.cbHangupPerHour.CheckedChanged += new System.EventHandler(this.cbHangupPerHour_CheckedChanged);
            // 
            // timerHangup
            // 
            this.timerHangup.Tick += new System.EventHandler(this.timerHangup_Tick);
            // 
            // tbHangeUpInterval
            // 
            this.tbHangeUpInterval.Location = new System.Drawing.Point(217, 275);
            this.tbHangeUpInterval.Name = "tbHangeUpInterval";
            this.tbHangeUpInterval.Size = new System.Drawing.Size(47, 21);
            this.tbHangeUpInterval.TabIndex = 30;
            this.tbHangeUpInterval.Text = "60";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(270, 278);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 31;
            this.label4.Text = "分钟";
            // 
            // csLoginTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 376);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbHangeUpInterval);
            this.Controls.Add(this.cbHangupPerHour);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cbModeHangup);
            this.Controls.Add(this.tbxMac);
            this.Controls.Add(this.SetManageIpBtn);
            this.Controls.Add(this.SetCodeBtn);
            this.Controls.Add(this.textBox_Code);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.autoStartCkbox);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.PauseBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.gamePathPickBtn);
            this.Controls.Add(this.gamePath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "csLoginTool";
            this.Text = "csLoginTool";
            this.Load += new System.EventHandler(this.csLoginTool_Load);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.TextBox gamePath;
        private System.Windows.Forms.Button gamePathPickBtn;
        private System.Windows.Forms.Button PauseBtn;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.CheckBox autoStartCkbox;
        private System.Windows.Forms.Timer autostartTimer;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel CountdownTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_Code;
        private System.Windows.Forms.Button SetCodeBtn;
        private System.Windows.Forms.Button SetManageIpBtn;
        private System.Windows.Forms.TextBox tbxMac;
        private System.Windows.Forms.CheckBox cbModeHangup;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox cbHangupPerHour;
        private System.Windows.Forms.Timer timerHangup;
        private System.Windows.Forms.TextBox tbHangeUpInterval;
        private System.Windows.Forms.Label label4;

    }
}

