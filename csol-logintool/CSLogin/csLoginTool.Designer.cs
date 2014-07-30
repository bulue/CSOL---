namespace CSLogin
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
            this.textbox1 = new System.Windows.Forms.TextBox();
            this.dama2Btn = new System.Windows.Forms.Button();
            this.dama2Ckbox = new System.Windows.Forms.CheckBox();
            this.xiaoaiCkbox = new System.Windows.Forms.CheckBox();
            this.xiaoaiBtn = new System.Windows.Forms.Button();
            this.autoStartCkbox = new System.Windows.Forms.CheckBox();
            this.autostartTimer = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.rebootTimer = new System.Windows.Forms.Timer(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.CountdownTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_Code = new System.Windows.Forms.TextBox();
            this.SetCodeBtn = new System.Windows.Forms.Button();
            this.SetManageIpBtn = new System.Windows.Forms.Button();
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
            this.startBtn.Location = new System.Drawing.Point(482, 334);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(117, 38);
            this.startBtn.TabIndex = 5;
            this.startBtn.Text = "启动登陆";
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
            this.PauseBtn.Location = new System.Drawing.Point(620, 334);
            this.PauseBtn.Name = "PauseBtn";
            this.PauseBtn.Size = new System.Drawing.Size(117, 38);
            this.PauseBtn.TabIndex = 9;
            this.PauseBtn.Text = "暂停 Ctrl+f12";
            this.PauseBtn.UseVisualStyleBackColor = true;
            this.PauseBtn.Click += new System.EventHandler(this.PauseBtn_Click);
            // 
            // textbox1
            // 
            this.textbox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.textbox1.Location = new System.Drawing.Point(21, 97);
            this.textbox1.Multiline = true;
            this.textbox1.Name = "textbox1";
            this.textbox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textbox1.Size = new System.Drawing.Size(774, 219);
            this.textbox1.TabIndex = 10;
            // 
            // dama2Btn
            // 
            this.dama2Btn.Location = new System.Drawing.Point(648, 75);
            this.dama2Btn.Name = "dama2Btn";
            this.dama2Btn.Size = new System.Drawing.Size(40, 20);
            this.dama2Btn.TabIndex = 11;
            this.dama2Btn.Text = "...";
            this.dama2Btn.UseVisualStyleBackColor = true;
            this.dama2Btn.Click += new System.EventHandler(this.dama2Btn_Click);
            // 
            // dama2Ckbox
            // 
            this.dama2Ckbox.AutoSize = true;
            this.dama2Ckbox.Location = new System.Drawing.Point(558, 75);
            this.dama2Ckbox.Name = "dama2Ckbox";
            this.dama2Ckbox.Size = new System.Drawing.Size(84, 16);
            this.dama2Ckbox.TabIndex = 12;
            this.dama2Ckbox.Text = "开启打码兔";
            this.dama2Ckbox.UseVisualStyleBackColor = true;
            // 
            // xiaoaiCkbox
            // 
            this.xiaoaiCkbox.AutoSize = true;
            this.xiaoaiCkbox.Location = new System.Drawing.Point(410, 75);
            this.xiaoaiCkbox.Name = "xiaoaiCkbox";
            this.xiaoaiCkbox.Size = new System.Drawing.Size(96, 16);
            this.xiaoaiCkbox.TabIndex = 13;
            this.xiaoaiCkbox.Text = "开启小爱打码";
            this.xiaoaiCkbox.UseVisualStyleBackColor = true;
            this.xiaoaiCkbox.CheckedChanged += new System.EventHandler(this.xiaoaiCkbox_CheckedChanged);
            // 
            // xiaoaiBtn
            // 
            this.xiaoaiBtn.Location = new System.Drawing.Point(512, 71);
            this.xiaoaiBtn.Name = "xiaoaiBtn";
            this.xiaoaiBtn.Size = new System.Drawing.Size(40, 20);
            this.xiaoaiBtn.TabIndex = 14;
            this.xiaoaiBtn.Text = "...";
            this.xiaoaiBtn.UseVisualStyleBackColor = true;
            this.xiaoaiBtn.Click += new System.EventHandler(this.xiaoaiBtn_Click);
            // 
            // autoStartCkbox
            // 
            this.autoStartCkbox.AutoSize = true;
            this.autoStartCkbox.Location = new System.Drawing.Point(87, 334);
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
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(21, 334);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(44, 22);
            this.button2.TabIndex = 17;
            this.button2.Text = "重启";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // rebootTimer
            // 
            this.rebootTimer.Tick += new System.EventHandler(this.rebootTimer_Tick);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CountdownTime});
            this.statusBar.Location = new System.Drawing.Point(0, 411);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(807, 22);
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
            // csLoginTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 433);
            this.Controls.Add(this.SetManageIpBtn);
            this.Controls.Add(this.SetCodeBtn);
            this.Controls.Add(this.textBox_Code);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.autoStartCkbox);
            this.Controls.Add(this.xiaoaiBtn);
            this.Controls.Add(this.xiaoaiCkbox);
            this.Controls.Add(this.dama2Ckbox);
            this.Controls.Add(this.dama2Btn);
            this.Controls.Add(this.textbox1);
            this.Controls.Add(this.PauseBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.gamePathPickBtn);
            this.Controls.Add(this.gamePath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "csLoginTool";
            this.Text = "csLoginTool";
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
        private System.Windows.Forms.TextBox textbox1;
        private System.Windows.Forms.Button dama2Btn;
        private System.Windows.Forms.CheckBox dama2Ckbox;
        private System.Windows.Forms.CheckBox xiaoaiCkbox;
        private System.Windows.Forms.Button xiaoaiBtn;
        private System.Windows.Forms.CheckBox autoStartCkbox;
        private System.Windows.Forms.Timer autostartTimer;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer rebootTimer;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel CountdownTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_Code;
        private System.Windows.Forms.Button SetCodeBtn;
        private System.Windows.Forms.Button SetManageIpBtn;

    }
}

