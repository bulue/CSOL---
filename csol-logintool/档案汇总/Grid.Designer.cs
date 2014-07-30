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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView_ttt = new System.Windows.Forms.DataGridView();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLab_SessionNum = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ttt)).BeginInit();
            this.Menu_DeleteRow.SuspendLayout();
            this.Menu_ClearCell.SuspendLayout();
            this.Menu_Check.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowDrop = true;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
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
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView.Location = new System.Drawing.Point(3, 3);
            this.dataGridView.Name = "dataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView.RowTemplate.Height = 23;
            this.dataGridView.Size = new System.Drawing.Size(971, 340);
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
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(982, 370);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(974, 344);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "账号";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView_ttt);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(974, 344);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "监听";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView_ttt
            // 
            this.dataGridView_ttt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_ttt.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column7});
            this.dataGridView_ttt.Location = new System.Drawing.Point(3, 0);
            this.dataGridView_ttt.Name = "dataGridView_ttt";
            this.dataGridView_ttt.RowTemplate.Height = 23;
            this.dataGridView_ttt.Size = new System.Drawing.Size(826, 353);
            this.dataGridView_ttt.TabIndex = 0;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "IP";
            this.Column7.Name = "Column7";
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
            this.button3.Click += new System.EventHandler(this.button3_Click);
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
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.StatusLab_SessionNum});
            this.statusStrip1.Location = new System.Drawing.Point(0, 597);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(996, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
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
            // Grid
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 619);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tabControl1);
            this.Name = "Grid";
            this.Text = "账号管理";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ttt)).EndInit();
            this.Menu_DeleteRow.ResumeLayout(false);
            this.Menu_ClearCell.ResumeLayout(false);
            this.Menu_Check.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView dataGridView_ttt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
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
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel StatusLab_SessionNum;
    }
}

