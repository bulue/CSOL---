namespace CSLogin
{
    partial class xiaoaiInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.refreshBtn = new System.Windows.Forms.Button();
            this.userStr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.jiFen = new System.Windows.Forms.TextBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // refreshBtn
            // 
            this.refreshBtn.Location = new System.Drawing.Point(138, 86);
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Size = new System.Drawing.Size(81, 26);
            this.refreshBtn.TabIndex = 0;
            this.refreshBtn.Text = "刷新积分";
            this.refreshBtn.UseVisualStyleBackColor = true;
            this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
            // 
            // userStr
            // 
            this.userStr.Location = new System.Drawing.Point(83, 24);
            this.userStr.Name = "userStr";
            this.userStr.Size = new System.Drawing.Size(136, 21);
            this.userStr.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "小爱密码串";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "积分";
            // 
            // jiFen
            // 
            this.jiFen.Location = new System.Drawing.Point(83, 59);
            this.jiFen.Name = "jiFen";
            this.jiFen.Size = new System.Drawing.Size(136, 21);
            this.jiFen.TabIndex = 5;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(51, 86);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(81, 26);
            this.okBtn.TabIndex = 6;
            this.okBtn.Text = "确认";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // xiaoaiInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(245, 124);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.jiFen);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userStr);
            this.Controls.Add(this.refreshBtn);
            this.Name = "xiaoaiInfo";
            this.Text = "小爱打码";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button refreshBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox jiFen;
        public System.Windows.Forms.TextBox userStr;
        private System.Windows.Forms.Button okBtn;
    }
}