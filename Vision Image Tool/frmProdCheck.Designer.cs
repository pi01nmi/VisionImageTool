namespace VisionImageTool
{
    partial class frmProdCheck
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
            this.pnlBanner = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lstIssues = new System.Windows.Forms.ListBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnSuggestion = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlBanner.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlBanner
            //
            this.pnlBanner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.pnlBanner.Controls.Add(this.lblSubtitle);
            this.pnlBanner.Controls.Add(this.lblTitle);
            this.pnlBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBanner.Location = new System.Drawing.Point(0, 0);
            this.pnlBanner.Name = "pnlBanner";
            this.pnlBanner.Padding = new System.Windows.Forms.Padding(16, 10, 16, 8);
            this.pnlBanner.Size = new System.Drawing.Size(640, 70);
            this.pnlBanner.TabIndex = 0;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.ForeColor = System.Drawing.Color.White;
            this.lblSubtitle.Location = new System.Drawing.Point(16, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(608, 22);
            this.lblSubtitle.TabIndex = 1;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(608, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "✖  Cấu hình Production chưa đúng - chưa push";
            //
            // lstIssues
            //
            this.lstIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstIssues.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstIssues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lstIssues.FormattingEnabled = true;
            this.lstIssues.HorizontalScrollbar = true;
            this.lstIssues.IntegralHeight = false;
            this.lstIssues.ItemHeight = 17;
            this.lstIssues.Location = new System.Drawing.Point(16, 82);
            this.lstIssues.Name = "lstIssues";
            this.lstIssues.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstIssues.Size = new System.Drawing.Size(608, 150);
            this.lstIssues.TabIndex = 1;
            //
            // lblHint
            //
            this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblHint.Location = new System.Drawing.Point(16, 238);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(608, 36);
            this.lblHint.TabIndex = 2;
            this.lblHint.Text = "Sửa production.py trên máy UnitX theo đề xuất, khởi động lại UnitX rồi push lại. Phần mềm không ghi được cấu hình xuống UnitX (SDK chỉ đọc).";
            //
            // btnSuggestion
            //
            this.btnSuggestion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSuggestion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSuggestion.FlatAppearance.BorderSize = 0;
            this.btnSuggestion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSuggestion.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSuggestion.ForeColor = System.Drawing.Color.White;
            this.btnSuggestion.Location = new System.Drawing.Point(318, 282);
            this.btnSuggestion.Name = "btnSuggestion";
            this.btnSuggestion.Size = new System.Drawing.Size(210, 34);
            this.btnSuggestion.TabIndex = 3;
            this.btnSuggestion.Text = "Hiển thị production.py đề xuất";
            this.btnSuggestion.UseVisualStyleBackColor = false;
            this.btnSuggestion.Click += new System.EventHandler(this.btnSuggestion_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(534, 282);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 34);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // frmProdCheck
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(640, 328);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSuggestion);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lstIssues);
            this.Controls.Add(this.pnlBanner);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 300);
            this.Name = "frmProdCheck";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kiểm tra cấu hình Production";
            this.pnlBanner.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBanner;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox lstIssues;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnSuggestion;
        private System.Windows.Forms.Button btnClose;
    }
}
