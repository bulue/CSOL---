namespace 查号器
{
    partial class countDownReboot
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
            this.hour = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.min = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.sec = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.okBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hour
            // 
            this.hour.Location = new System.Drawing.Point(22, 25);
            this.hour.Name = "hour";
            this.hour.Size = new System.Drawing.Size(62, 21);
            this.hour.TabIndex = 0;
            this.hour.Text = "0";
            this.hour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "小时";
            // 
            // min
            // 
            this.min.Location = new System.Drawing.Point(125, 25);
            this.min.Name = "min";
            this.min.Size = new System.Drawing.Size(62, 21);
            this.min.TabIndex = 2;
            this.min.Text = "0";
            this.min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(193, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "分";
            // 
            // sec
            // 
            this.sec.Location = new System.Drawing.Point(216, 25);
            this.sec.Name = "sec";
            this.sec.Size = new System.Drawing.Size(62, 21);
            this.sec.TabIndex = 4;
            this.sec.Text = "0";
            this.sec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(284, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "秒";
            // 
            // okBtn
            // 
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(78, 52);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(69, 28);
            this.okBtn.TabIndex = 6;
            this.okBtn.Text = "确定";
            this.okBtn.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.button1.Location = new System.Drawing.Point(153, 52);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(69, 28);
            this.button1.TabIndex = 7;
            this.button1.Text = "停止";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(228, 52);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(69, 28);
            this.button2.TabIndex = 8;
            this.button2.Text = "取消重启";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // countDownReboot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 87);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sec);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.min);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hour);
            this.Name = "countDownReboot";
            this.Text = "countDownReboot";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button okBtn;
        public System.Windows.Forms.TextBox hour;
        public System.Windows.Forms.TextBox min;
        public System.Windows.Forms.TextBox sec;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}