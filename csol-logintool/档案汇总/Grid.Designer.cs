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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView = new System.Windows.Forms.DataGridView();
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
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.Menu_DeleteRow.SuspendLayout();
            this.Menu_ClearCell.SuspendLayout();
            this.Menu_Check.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowDrop = true;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Account,
            this.Password,
            this.FailedCount,
            this.CheckTime,
            this.State,
            this.Checked,
            this.Days,
            this.IP,
            this.Code});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView.Location = new System.Drawing.Point(0, 1);
            this.dataGridView.Name = "dataGridView";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView.RowTemplate.Height = 23;
            this.dataGridView.Size = new System.Drawing.Size(984, 381);
            this.dataGridView.TabIndex = 0;
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
            this.button2.Location = new System.Drawing.Point(909, 417);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "保存";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(909, 388);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "监听";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.listenBtn_Click);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(12, 388);
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
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 554);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "本机IP";
            // 
            // textBox_IP
            // 
            this.textBox_IP.Location = new System.Drawing.Point(69, 551);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.ReadOnly = true;
            this.textBox_IP.Size = new System.Drawing.Size(100, 21);
            this.textBox_IP.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(909, 446);
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
            this.statusStrip.Location = new System.Drawing.Point(0, 597);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(996, 22);
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
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(909, 475);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 10;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Grid
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 619);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Name = "Grid";
            this.Text = "账号管理";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.Menu_DeleteRow.ResumeLayout(false);
            this.Menu_ClearCell.ResumeLayout(false);
            this.Menu_Check.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
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
        private System.Windows.Forms.Button button4;
    }
}

