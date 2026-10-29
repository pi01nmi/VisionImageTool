namespace VisionImageTool
{
    partial class frmLogV1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.chkError = new System.Windows.Forms.CheckBox();
            this.chkWarning = new System.Windows.Forms.CheckBox();
            this.chkInfo = new System.Windows.Forms.CheckBox();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.btnSaveLog = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lvLog = new System.Windows.Forms.ListView();
            this.chLogTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLogLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLogMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTop
            //
            this.pnlTop.BackColor = System.Drawing.Color.White;
            this.pnlTop.Controls.Add(this.txtSearch);
            this.pnlTop.Controls.Add(this.lblSearch);
            this.pnlTop.Controls.Add(this.chkError);
            this.pnlTop.Controls.Add(this.chkWarning);
            this.pnlTop.Controls.Add(this.chkInfo);
            this.pnlTop.Controls.Add(this.chkAutoScroll);
            this.pnlTop.Controls.Add(this.btnSaveLog);
            this.pnlTop.Controls.Add(this.btnClear);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlTop.Size = new System.Drawing.Size(1000, 44);
            this.pnlTop.TabIndex = 0;
            //
            // txtSearch
            //
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(758, 10);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(232, 23);
            this.txtSearch.TabIndex = 7;
            this.txtSearch.TextChanged += new System.EventHandler(this.Filter_Changed);
            //
            // lblSearch
            //
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(726, 13);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(28, 15);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Tìm";
            //
            // chkError
            //
            this.chkError.AutoSize = true;
            this.chkError.Checked = true;
            this.chkError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkError.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.chkError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.chkError.Location = new System.Drawing.Point(622, 12);
            this.chkError.Name = "chkError";
            this.chkError.Size = new System.Drawing.Size(55, 19);
            this.chkError.TabIndex = 5;
            this.chkError.Text = "Lỗi (0)";
            this.chkError.UseVisualStyleBackColor = true;
            this.chkError.CheckedChanged += new System.EventHandler(this.Filter_Changed);
            //
            // chkWarning
            //
            this.chkWarning.AutoSize = true;
            this.chkWarning.Checked = true;
            this.chkWarning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWarning.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.chkWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(65)))), ((int)(((byte)(12)))));
            this.chkWarning.Location = new System.Drawing.Point(506, 12);
            this.chkWarning.Name = "chkWarning";
            this.chkWarning.Size = new System.Drawing.Size(91, 19);
            this.chkWarning.TabIndex = 4;
            this.chkWarning.Text = "Cảnh báo (0)";
            this.chkWarning.UseVisualStyleBackColor = true;
            this.chkWarning.CheckedChanged += new System.EventHandler(this.Filter_Changed);
            //
            // chkInfo
            //
            this.chkInfo.AutoSize = true;
            this.chkInfo.Checked = true;
            this.chkInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInfo.Location = new System.Drawing.Point(404, 12);
            this.chkInfo.Name = "chkInfo";
            this.chkInfo.Size = new System.Drawing.Size(84, 19);
            this.chkInfo.TabIndex = 3;
            this.chkInfo.Text = "Thông tin (0)";
            this.chkInfo.UseVisualStyleBackColor = true;
            this.chkInfo.CheckedChanged += new System.EventHandler(this.Filter_Changed);
            //
            // chkAutoScroll
            //
            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.Location = new System.Drawing.Point(268, 12);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Size = new System.Drawing.Size(129, 19);
            this.chkAutoScroll.TabIndex = 2;
            this.chkAutoScroll.Text = "Tự cuộn xuống cuối";
            this.chkAutoScroll.UseVisualStyleBackColor = true;
            //
            // btnSaveLog
            //
            this.btnSaveLog.BackColor = System.Drawing.Color.White;
            this.btnSaveLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnSaveLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveLog.Location = new System.Drawing.Point(134, 8);
            this.btnSaveLog.Name = "btnSaveLog";
            this.btnSaveLog.Size = new System.Drawing.Size(122, 28);
            this.btnSaveLog.TabIndex = 1;
            this.btnSaveLog.Text = "Lưu log ra file...";
            this.btnSaveLog.UseVisualStyleBackColor = false;
            this.btnSaveLog.Click += new System.EventHandler(this.btnSaveLog_Click);
            //
            // btnClear
            //
            this.btnClear.BackColor = System.Drawing.Color.White;
            this.btnClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Location = new System.Drawing.Point(8, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(120, 28);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "Xóa log";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // lvLog
            //
            this.lvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chLogTime,
            this.chLogLevel,
            this.chLogMessage});
            this.lvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.lvLog.FullRowSelect = true;
            this.lvLog.HideSelection = false;
            this.lvLog.Location = new System.Drawing.Point(0, 44);
            this.lvLog.Name = "lvLog";
            this.lvLog.Size = new System.Drawing.Size(1000, 556);
            this.lvLog.TabIndex = 1;
            this.lvLog.UseCompatibleStateImageBehavior = false;
            this.lvLog.View = System.Windows.Forms.View.Details;
            //
            // chLogTime
            //
            this.chLogTime.Text = "Thời gian";
            this.chLogTime.Width = 100;
            //
            // chLogLevel
            //
            this.chLogLevel.Text = "Mức";
            this.chLogLevel.Width = 70;
            //
            // chLogMessage
            //
            this.chLogMessage.Text = "Nội dung";
            this.chLogMessage.Width = 800;
            //
            // frmLogV1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.lvLog);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmLogV1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nhật ký hoạt động - Vision Image Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLogV1_FormClosing);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.CheckBox chkAutoScroll;
        private System.Windows.Forms.Button btnSaveLog;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.CheckBox chkInfo;
        private System.Windows.Forms.CheckBox chkWarning;
        private System.Windows.Forms.CheckBox chkError;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListView lvLog;
        private System.Windows.Forms.ColumnHeader chLogTime;
        private System.Windows.Forms.ColumnHeader chLogLevel;
        private System.Windows.Forms.ColumnHeader chLogMessage;
    }
}
