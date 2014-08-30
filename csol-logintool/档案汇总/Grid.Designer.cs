namespace 档案汇总
{
    partial class Grid
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvUserData = new System.Windows.Forms.DataGridView();
            this.Account = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FailedCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CheckTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.State = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Checked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Days = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.Menu_DeleteRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_DeleteRow = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ClearCell = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_ClearCell = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Check = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_GouXuan = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_ClearGouXuan = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLab_SessionNum = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_StatusBarRefresh = new System.Windows.Forms.Timer(this.components);
            this.timer_FlushTextbox = new System.Windows.Forms.Timer(this.components);
            this.button5 = new System.Windows.Forms.Button();
            this.cbClearData = new System.Windows.Forms.CheckBox();
            this.cbFailedFirst = new System.Windows.Forms.CheckBox();
            this.btnClearLoginData = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvSession = new System.Windows.Forms.DataGridView();
            this.sIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sMac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sFinishedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sLoginState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sDisconnect = new System.Windows.Forms.DataGridViewButtonColumn();
            this.reboot = new System.Windows.Forms.DataGridViewButtonColumn();
            this.cbRemoteReboot = new System.Windows.Forms.CheckBox();
            this.tmReboot = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserData)).BeginInit();
            this.Menu_DeleteRow.SuspendLayout();
            this.Menu_ClearCell.SuspendLayout();
            this.Menu_Check.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSession)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvUserData
            // 
            this.dgvUserData.AllowDrop = true;
            this.dgvUserData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUserData.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUserData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvUserData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Account,
            this.Password,
            this.FailedCount,
            this.CheckTime,
            this.State,
            this.Checked,
            this.Days,
            this.IP,
            this.Code});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserData.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvUserData.Location = new System.Drawing.Point(0, 0);
            this.dgvUserData.Name = "dgvUserData";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUserData.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvUserData.RowTemplate.Height = 23;
            this.dgvUserData.Size = new System.Drawing.Size(979, 400);
            this.dgvUserData.TabIndex = 0;
            // 
            // Account
            // 
            this.Account.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Account.HeaderText = "账号";
            this.Account.Name = "Account";
            this.Account.Width = 54;
            // 
            // Password
            // 
            this.Password.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Password.HeaderText = "密码";
            this.Password.Name = "Password";
            this.Password.Width = 54;
            // 
            // FailedCount
            // 
            this.FailedCount.HeaderText = "失败次数";
            this.FailedCount.Name = "FailedCount";
            this.FailedCount.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // CheckTime
            // 
            this.CheckTime.HeaderText = "签到时间";
            this.CheckTime.Name = "CheckTime";
            // 
            // State
            // 
            this.State.HeaderText = "状态";
            this.State.Name = "State";
            // 
            // Checked
            // 
            this.Checked.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Checked.HeaderText = "签到";
            this.Checked.Name = "Checked";
            this.Checked.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Checked.Width = 54;
            // 
            // Days
            // 
            this.Days.HeaderText = "签到天数";
            this.Days.Name = "Days";
            // 
            // IP
            // 
            this.IP.HeaderText = "登陆机IP";
            this.IP.Name = "IP";
            // 
            // Code
            // 
            this.Code.HeaderText = "登陆机代号";
            this.Code.Name = "Code";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(909, 504);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "保存";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(909, 475);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "监听";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.listenBtn_Click);
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Location = new System.Drawing.Point(4, 465);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(879, 157);
            this.textBox.TabIndex = 5;
            // 
            // Menu_DeleteRow
            // 
            this.Menu_DeleteRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_DeleteRow});
            this.Menu_DeleteRow.Name = "contextMenuStrip1";
            this.Menu_DeleteRow.Size = new System.Drawing.Size(113, 26);
            // 
            // MenuItem_DeleteRow
            // 
            this.MenuItem_DeleteRow.Name = "MenuItem_DeleteRow";
            this.MenuItem_DeleteRow.Size = new System.Drawing.Size(112, 22);
            this.MenuItem_DeleteRow.Text = "删除行";
            this.MenuItem_DeleteRow.Click += new System.EventHandler(this.MenuItem_DeleteRow_Click);
            // 
            // Menu_ClearCell
            // 
            this.Menu_ClearCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_ClearCell});
            this.Menu_ClearCell.Name = "contextMenuStrip1";
            this.Menu_ClearCell.Size = new System.Drawing.Size(137, 26);
            // 
            // MenuItem_ClearCell
            // 
            this.MenuItem_ClearCell.Name = "MenuItem_ClearCell";
            this.MenuItem_ClearCell.Size = new System.Drawing.Size(136, 22);
            this.MenuItem_ClearCell.Text = "清空单元格";
            this.MenuItem_ClearCell.Click += new System.EventHandler(this.MenuItem_ClearCell_Click);
            // 
            // Menu_Check
            // 
            this.Menu_Check.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_GouXuan,
            this.MenuItem_ClearGouXuan});
            this.Menu_Check.Name = "contextMenuStrip1";
            this.Menu_Check.Size = new System.Drawing.Size(125, 48);
            // 
            // MenuItem_GouXuan
            // 
            this.MenuItem_GouXuan.Name = "MenuItem_GouXuan";
            this.MenuItem_GouXuan.Size = new System.Drawing.Size(124, 22);
            this.MenuItem_GouXuan.Text = "勾选";
            this.MenuItem_GouXuan.Click += new System.EventHandler(this.MenuItem_GouXuan_Click);
            // 
            // MenuItem_ClearGouXuan
            // 
            this.MenuItem_ClearGouXuan.Name = "MenuItem_ClearGouXuan";
            this.MenuItem_ClearGouXuan.Size = new System.Drawing.Size(124, 22);
            this.MenuItem_ClearGouXuan.Text = "清除勾选";
            this.MenuItem_ClearGouXuan.Click += new System.EventHandler(this.MenuItem_ClearGouXuan_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 647);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "本机IP";
            // 
            // textBox_IP
            // 
            this.textBox_IP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_IP.Location = new System.Drawing.Point(57, 644);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.ReadOnly = true;
            this.textBox_IP.Size = new System.Drawing.Size(100, 21);
            this.textBox_IP.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(909, 533);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "清理日志";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.StatusLab_SessionNum});
            this.statusStrip.Location = new System.Drawing.Point(0, 670);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1062, 22);
            this.statusStrip.TabIndex = 9;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            // 
            // StatusLab_SessionNum
            // 
            this.StatusLab_SessionNum.Name = "StatusLab_SessionNum";
            this.StatusLab_SessionNum.Size = new System.Drawing.Size(66, 17);
            this.StatusLab_SessionNum.Text = "当前连接:0";
            // 
            // timer_StatusBarRefresh
            // 
            this.timer_StatusBarRefresh.Interval = 1000;
            this.timer_StatusBarRefresh.Tick += new System.EventHandler(this.timer_StatusBarRefresh_Tick);
            // 
            // timer_FlushTextbox
            // 
            this.timer_FlushTextbox.Interval = 300;
            this.timer_FlushTextbox.Tick += new System.EventHandler(this.timer_FlushTextbox_Tick);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(909, 562);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 11;
            this.button5.Text = "导出数据";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // cbClearData
            // 
            this.cbClearData.AutoSize = true;
            this.cbClearData.Location = new System.Drawing.Point(12, 12);
            this.cbClearData.Name = "cbClearData";
            this.cbClearData.Size = new System.Drawing.Size(72, 16);
            this.cbClearData.TabIndex = 12;
            this.cbClearData.Text = "六点清零";
            this.cbClearData.UseVisualStyleBackColor = true;
            this.cbClearData.CheckedChanged += new System.EventHandler(this.cbClearData_CheckedChanged);
            // 
            // cbFailedFirst
            // 
            this.cbFailedFirst.AutoSize = true;
            this.cbFailedFirst.Location = new System.Drawing.Point(108, 12);
            this.cbFailedFirst.Name = "cbFailedFirst";
            this.cbFailedFirst.Size = new System.Drawing.Size(72, 16);
            this.cbFailedFirst.TabIndex = 13;
            this.cbFailedFirst.Text = "失败优先";
            this.cbFailedFirst.UseVisualStyleBackColor = true;
            this.cbFailedFirst.CheckedChanged += new System.EventHandler(this.cbFailedFirst_CheckedChanged);
            // 
            // btnClearLoginData
            // 
            this.btnClearLoginData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLoginData.Location = new System.Drawing.Point(909, 591);
            this.btnClearLoginData.Name = "btnClearLoginData";
            this.btnClearLoginData.Size = new System.Drawing.Size(75, 23);
            this.btnClearLoginData.TabIndex = 14;
            this.btnClearLoginData.Text = "清理登陆数据";
            this.btnClearLoginData.UseVisualStyleBackColor = true;
            this.btnClearLoginData.Click += new System.EventHandler(this.btnClearLoginData_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 34);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(983, 425);
            this.tabControl1.TabIndex = 15;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvUserData);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(975, 399);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "账号";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvSession);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(975, 399);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "连接";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvSession
            // 
            this.dgvSession.AllowUserToOrderColumns = true;
            this.dgvSession.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSession.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dgvSession.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSession.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sIP,
            this.sCode,
            this.sMac,
            this.sState,
            this.sFinishedNum,
            this.sLoginState,
            this.sDisconnect,
            this.reboot});
            this.dgvSession.Location = new System.Drawing.Point(-3, 0);
            this.dgvSession.Name = "dgvSession";
            this.dgvSession.RowTemplate.Height = 23;
            this.dgvSession.Size = new System.Drawing.Size(978, 403);
            this.dgvSession.TabIndex = 0;
            this.dgvSession.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSession_CellContentClick);
            // 
            // sIP
            // 
            this.sIP.HeaderText = "IP";
            this.sIP.Name = "sIP";
            // 
            // sCode
            // 
            this.sCode.HeaderText = "登陆机代号";
            this.sCode.Name = "sCode";
            // 
            // sMac
            // 
            this.sMac.HeaderText = "Mac";
            this.sMac.Name = "sMac";
            // 
            // sState
            // 
            this.sState.HeaderText = "连接状态";
            this.sState.Name = "sState";
            // 
            // sFinishedNum
            // 
            this.sFinishedNum.HeaderText = "已经登陆账号";
            this.sFinishedNum.Name = "sFinishedNum";
            // 
            // sLoginState
            // 
            this.sLoginState.HeaderText = "登陆情况";
            this.sLoginState.Name = "sLoginState";
            // 
            // sDisconnect
            // 
            this.sDisconnect.HeaderText = "断开连接";
            this.sDisconnect.Name = "sDisconnect";
            this.sDisconnect.Text = "";
            // 
            // reboot
            // 
            this.reboot.HeaderText = "重启登陆机";
            this.reboot.Name = "reboot";
            this.reboot.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.reboot.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // cbRemoteReboot
            // 
            this.cbRemoteReboot.AutoSize = true;
            this.cbRemoteReboot.Location = new System.Drawing.Point(206, 12);
            this.cbRemoteReboot.Name = "cbRemoteReboot";
            this.cbRemoteReboot.Size = new System.Drawing.Size(84, 16);
            this.cbRemoteReboot.TabIndex = 16;
            this.cbRemoteReboot.Text = "重启登陆机";
            this.cbRemoteReboot.UseVisualStyleBackColor = true;
            this.cbRemoteReboot.CheckedChanged += new System.EventHandler(this.cbRemoteReboot_CheckedChanged);
            // 
            // tmReboot
            // 
            this.tmReboot.Interval = 30000;
            this.tmReboot.Tick += new System.EventHandler(this.tmReboot_Tick);
            // 
            // Grid
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 692);
            this.Controls.Add(this.cbRemoteReboot);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnClearLoginData);
            this.Controls.Add(this.cbFailedFirst);
            this.Controls.Add(this.cbClearData);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Name = "Grid";
            this.Text = "账号管理";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserData)).EndInit();
            this.Menu_DeleteRow.ResumeLayout(false);
            this.Menu_ClearCell.ResumeLayout(false);
            this.Menu_Check.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSession)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvUserData;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.ContextMenuStrip Menu_DeleteRow;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_DeleteRow;
        private System.Windows.Forms.ContextMenuStrip Menu_ClearCell;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_ClearCell;
        private System.Windows.Forms.ContextMenuStrip Menu_Check;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_GouXuan;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_ClearGouXuan;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.DataGridViewTextBoxColumn Account;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewTextBoxColumn FailedCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn CheckTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn State;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Checked;
        private System.Windows.Forms.DataGridViewTextBoxColumn Days;
        private System.Windows.Forms.DataGridViewTextBoxColumn IP;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel StatusLab_SessionNum;
        private System.Windows.Forms.Timer timer_StatusBarRefresh;
        private System.Windows.Forms.Timer timer_FlushTextbox;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckBox cbClearData;
        private System.Windows.Forms.CheckBox cbFailedFirst;
        private System.Windows.Forms.Button btnClearLoginData;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvSession;
        private System.Windows.Forms.DataGridViewTextBoxColumn sIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn sCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn sMac;
        private System.Windows.Forms.DataGridViewTextBoxColumn sState;
        private System.Windows.Forms.DataGridViewTextBoxColumn sFinishedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn sLoginState;
        private System.Windows.Forms.DataGridViewButtonColumn sDisconnect;
        private System.Windows.Forms.DataGridViewButtonColumn reboot;
        private System.Windows.Forms.CheckBox cbRemoteReboot;
        private System.Windows.Forms.Timer tmReboot;
    }
}

