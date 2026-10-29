namespace VisionImageTool
{
    partial class frmExportOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.grpContent = new System.Windows.Forms.GroupBox();
            this.lblModeNote = new System.Windows.Forms.Label();
            this.chkThumbnail = new System.Windows.Forms.CheckBox();
            this.rbImage = new System.Windows.Forms.RadioButton();
            this.rbHyperlink = new System.Windows.Forms.RadioButton();
            this.grpSize = new System.Windows.Forms.GroupBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.lblPx = new System.Windows.Forms.Label();
            this.numManualHeight = new System.Windows.Forms.NumericUpDown();
            this.lblManualX = new System.Windows.Forms.Label();
            this.numManualWidth = new System.Windows.Forms.NumericUpDown();
            this.rbManual = new System.Windows.Forms.RadioButton();
            this.lblRatioResult = new System.Windows.Forms.Label();
            this.numFactor = new System.Windows.Forms.NumericUpDown();
            this.lblTimes = new System.Windows.Forms.Label();
            this.numRatioHeight = new System.Windows.Forms.NumericUpDown();
            this.lblColon = new System.Windows.Forms.Label();
            this.numRatioWidth = new System.Windows.Forms.NumericUpDown();
            this.rbRatio = new System.Windows.Forms.RadioButton();
            this.lblDetected = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpColumns = new System.Windows.Forms.GroupBox();
            this.chkVision = new System.Windows.Forms.CheckBox();
            this.grpContent.SuspendLayout();
            this.grpSize.SuspendLayout();
            this.grpColumns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numManualHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numManualWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioWidth)).BeginInit();
            this.SuspendLayout();
            //
            // grpContent
            //
            this.grpContent.Controls.Add(this.chkThumbnail);
            this.grpContent.Controls.Add(this.lblModeNote);
            this.grpContent.Controls.Add(this.rbImage);
            this.grpContent.Controls.Add(this.rbHyperlink);
            this.grpContent.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpContent.Location = new System.Drawing.Point(12, 12);
            this.grpContent.Name = "grpContent";
            this.grpContent.Size = new System.Drawing.Size(516, 136);
            this.grpContent.TabIndex = 0;
            this.grpContent.TabStop = false;
            this.grpContent.Text = "Cột Raw / Mask chứa";
            //
            // lblModeNote
            // 
            this.lblModeNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblModeNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblModeNote.Location = new System.Drawing.Point(36, 104);
            this.lblModeNote.Name = "lblModeNote";
            this.lblModeNote.Size = new System.Drawing.Size(470, 24);
            this.lblModeNote.TabIndex = 2;
            this.lblModeNote.Text = "Thumbnail nén nhẹ, bấm vào ảnh để mở ảnh gốc.";
            //
            // chkThumbnail
            //
            this.chkThumbnail.AutoSize = true;
            this.chkThumbnail.Checked = true;
            this.chkThumbnail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkThumbnail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkThumbnail.Location = new System.Drawing.Point(40, 52);
            this.chkThumbnail.Name = "chkThumbnail";
            this.chkThumbnail.Size = new System.Drawing.Size(360, 21);
            this.chkThumbnail.TabIndex = 3;
            this.chkThumbnail.Text = "Hiện thumbnail (tắt = chỉ tên file, nhanh cho dataset lớn)";
            this.chkThumbnail.UseVisualStyleBackColor = true;
            this.chkThumbnail.CheckedChanged += new System.EventHandler(this.Option_Changed);
            //
            // rbImage
            //
            this.rbImage.AutoSize = true;
            this.rbImage.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.rbImage.Location = new System.Drawing.Point(18, 78);
            this.rbImage.Name = "rbImage";
            this.rbImage.Size = new System.Drawing.Size(340, 21);
            this.rbImage.TabIndex = 1;
            this.rbImage.Text = "Chèn ảnh gốc vừa ô - không hyperlink, không nén ảnh";
            this.rbImage.UseVisualStyleBackColor = true;
            this.rbImage.CheckedChanged += new System.EventHandler(this.Option_Changed);
            //
            // rbHyperlink
            //
            this.rbHyperlink.AutoSize = true;
            this.rbHyperlink.Checked = true;
            this.rbHyperlink.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.rbHyperlink.Location = new System.Drawing.Point(18, 28);
            this.rbHyperlink.Name = "rbHyperlink";
            this.rbHyperlink.Size = new System.Drawing.Size(340, 21);
            this.rbHyperlink.TabIndex = 0;
            this.rbHyperlink.TabStop = true;
            this.rbHyperlink.Text = "Hyperlink - ảnh thumbnail, bấm vào để mở ảnh gốc";
            this.rbHyperlink.UseVisualStyleBackColor = true;
            this.rbHyperlink.CheckedChanged += new System.EventHandler(this.Option_Changed);
            //
            // grpSize
            //
            this.grpSize.Controls.Add(this.lblHint);
            this.grpSize.Controls.Add(this.lblPx);
            this.grpSize.Controls.Add(this.numManualHeight);
            this.grpSize.Controls.Add(this.lblManualX);
            this.grpSize.Controls.Add(this.numManualWidth);
            this.grpSize.Controls.Add(this.rbManual);
            this.grpSize.Controls.Add(this.lblRatioResult);
            this.grpSize.Controls.Add(this.numFactor);
            this.grpSize.Controls.Add(this.lblTimes);
            this.grpSize.Controls.Add(this.numRatioHeight);
            this.grpSize.Controls.Add(this.lblColon);
            this.grpSize.Controls.Add(this.numRatioWidth);
            this.grpSize.Controls.Add(this.rbRatio);
            this.grpSize.Controls.Add(this.lblDetected);
            this.grpSize.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpSize.Location = new System.Drawing.Point(12, 158);
            this.grpSize.Name = "grpSize";
            this.grpSize.Size = new System.Drawing.Size(516, 196);
            this.grpSize.TabIndex = 1;
            this.grpSize.TabStop = false;
            this.grpSize.Text = "Kích thước ô ảnh";
            //
            // lblHint
            //
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblHint.Location = new System.Drawing.Point(16, 130);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(484, 56);
            this.lblHint.TabIndex = 13;
            this.lblHint.Text = "Cột Raw/Mask và mỗi hàng được đặt đúng kích thước này; ảnh vừa khít ô, giữ nguyên tỷ " +
    "lệ. Áp dụng cho cả hai chế độ.";
            //
            // lblPx
            //
            this.lblPx.AutoSize = true;
            this.lblPx.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblPx.Location = new System.Drawing.Point(318, 95);
            this.lblPx.Name = "lblPx";
            this.lblPx.Size = new System.Drawing.Size(21, 17);
            this.lblPx.TabIndex = 12;
            this.lblPx.Text = "px";
            //
            // numManualHeight
            //
            this.numManualHeight.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numManualHeight.Location = new System.Drawing.Point(246, 93);
            this.numManualHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numManualHeight.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            this.numManualHeight.Name = "numManualHeight";
            this.numManualHeight.Size = new System.Drawing.Size(66, 24);
            this.numManualHeight.TabIndex = 11;
            this.numManualHeight.Value = new decimal(new int[] { 60, 0, 0, 0 });
            this.numManualHeight.ValueChanged += new System.EventHandler(this.Option_Changed);
            //
            // lblManualX
            //
            this.lblManualX.AutoSize = true;
            this.lblManualX.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblManualX.Location = new System.Drawing.Point(226, 95);
            this.lblManualX.Name = "lblManualX";
            this.lblManualX.Size = new System.Drawing.Size(15, 17);
            this.lblManualX.TabIndex = 10;
            this.lblManualX.Text = "×";
            //
            // numManualWidth
            //
            this.numManualWidth.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numManualWidth.Location = new System.Drawing.Point(154, 93);
            this.numManualWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numManualWidth.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            this.numManualWidth.Name = "numManualWidth";
            this.numManualWidth.Size = new System.Drawing.Size(66, 24);
            this.numManualWidth.TabIndex = 9;
            this.numManualWidth.Value = new decimal(new int[] { 80, 0, 0, 0 });
            this.numManualWidth.ValueChanged += new System.EventHandler(this.Option_Changed);
            //
            // rbManual
            //
            this.rbManual.AutoSize = true;
            this.rbManual.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.rbManual.Location = new System.Drawing.Point(18, 94);
            this.rbManual.Name = "rbManual";
            this.rbManual.Size = new System.Drawing.Size(76, 21);
            this.rbManual.TabIndex = 8;
            this.rbManual.Text = "Nhập tay";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.CheckedChanged += new System.EventHandler(this.Option_Changed);
            //
            // lblRatioResult
            //
            this.lblRatioResult.AutoSize = true;
            this.lblRatioResult.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblRatioResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lblRatioResult.Location = new System.Drawing.Point(376, 61);
            this.lblRatioResult.Name = "lblRatioResult";
            this.lblRatioResult.Size = new System.Drawing.Size(96, 17);
            this.lblRatioResult.TabIndex = 7;
            this.lblRatioResult.Text = "= 80 × 60 px";
            //
            // numFactor
            //
            this.numFactor.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numFactor.Location = new System.Drawing.Point(306, 59);
            this.numFactor.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            this.numFactor.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numFactor.Name = "numFactor";
            this.numFactor.Size = new System.Drawing.Size(60, 24);
            this.numFactor.TabIndex = 6;
            this.numFactor.Value = new decimal(new int[] { 20, 0, 0, 0 });
            this.numFactor.ValueChanged += new System.EventHandler(this.Option_Changed);
            //
            // lblTimes
            //
            this.lblTimes.AutoSize = true;
            this.lblTimes.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTimes.Location = new System.Drawing.Point(286, 61);
            this.lblTimes.Name = "lblTimes";
            this.lblTimes.Size = new System.Drawing.Size(15, 17);
            this.lblTimes.TabIndex = 5;
            this.lblTimes.Text = "×";
            //
            // numRatioHeight
            //
            this.numRatioHeight.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numRatioHeight.Location = new System.Drawing.Point(226, 59);
            this.numRatioHeight.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numRatioHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRatioHeight.Name = "numRatioHeight";
            this.numRatioHeight.Size = new System.Drawing.Size(54, 24);
            this.numRatioHeight.TabIndex = 4;
            this.numRatioHeight.Value = new decimal(new int[] { 3, 0, 0, 0 });
            this.numRatioHeight.ValueChanged += new System.EventHandler(this.Option_Changed);
            //
            // lblColon
            //
            this.lblColon.AutoSize = true;
            this.lblColon.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblColon.Location = new System.Drawing.Point(212, 61);
            this.lblColon.Name = "lblColon";
            this.lblColon.Size = new System.Drawing.Size(11, 17);
            this.lblColon.TabIndex = 3;
            this.lblColon.Text = ":";
            //
            // numRatioWidth
            //
            this.numRatioWidth.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numRatioWidth.Location = new System.Drawing.Point(154, 59);
            this.numRatioWidth.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numRatioWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRatioWidth.Name = "numRatioWidth";
            this.numRatioWidth.Size = new System.Drawing.Size(54, 24);
            this.numRatioWidth.TabIndex = 2;
            this.numRatioWidth.Value = new decimal(new int[] { 4, 0, 0, 0 });
            this.numRatioWidth.ValueChanged += new System.EventHandler(this.Option_Changed);
            //
            // rbRatio
            //
            this.rbRatio.AutoSize = true;
            this.rbRatio.Checked = true;
            this.rbRatio.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.rbRatio.Location = new System.Drawing.Point(18, 60);
            this.rbRatio.Name = "rbRatio";
            this.rbRatio.Size = new System.Drawing.Size(114, 21);
            this.rbRatio.TabIndex = 1;
            this.rbRatio.TabStop = true;
            this.rbRatio.Text = "Theo tỷ lệ ảnh";
            this.rbRatio.UseVisualStyleBackColor = true;
            this.rbRatio.CheckedChanged += new System.EventHandler(this.Option_Changed);
            //
            // lblDetected
            //
            this.lblDetected.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDetected.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lblDetected.Location = new System.Drawing.Point(16, 28);
            this.lblDetected.Name = "lblDetected";
            this.lblDetected.Size = new System.Drawing.Size(484, 20);
            this.lblDetected.TabIndex = 0;
            this.lblDetected.Text = "Ảnh mẫu: (chưa có)";
            //
            // grpColumns
            //
            this.grpColumns.Controls.Add(this.chkVision);
            this.grpColumns.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpColumns.Location = new System.Drawing.Point(12, 362);
            this.grpColumns.Name = "grpColumns";
            this.grpColumns.Size = new System.Drawing.Size(516, 58);
            this.grpColumns.TabIndex = 2;
            this.grpColumns.TabStop = false;
            this.grpColumns.Text = "Cột kết quả";
            //
            // chkVision
            //
            this.chkVision.AutoSize = true;
            this.chkVision.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkVision.Location = new System.Drawing.Point(16, 26);
            this.chkVision.Name = "chkVision";
            this.chkVision.Size = new System.Drawing.Size(380, 21);
            this.chkVision.TabIndex = 0;
            this.chkVision.Text = "Xuất cột Vision (kết quả máy, cạnh cột Đánh giá của mỗi ĐK)";
            this.chkVision.UseVisualStyleBackColor = true;
            //
            // btnOk
            //
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(316, 430);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(114, 36);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Xuất Excel";
            this.btnOk.UseVisualStyleBackColor = false;
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(436, 430);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 36);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            //
            // frmExportOptions
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(540, 478);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.grpColumns);
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.grpContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmExportOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tuỳ chọn xuất Excel";
            this.grpContent.ResumeLayout(false);
            this.grpContent.PerformLayout();
            this.grpSize.ResumeLayout(false);
            this.grpColumns.ResumeLayout(false);
            this.grpColumns.PerformLayout();
            this.grpSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numManualHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numManualWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpContent;
        private System.Windows.Forms.Label lblModeNote;
        private System.Windows.Forms.CheckBox chkThumbnail;
        private System.Windows.Forms.GroupBox grpColumns;
        private System.Windows.Forms.CheckBox chkVision;
        private System.Windows.Forms.RadioButton rbImage;
        private System.Windows.Forms.RadioButton rbHyperlink;
        private System.Windows.Forms.GroupBox grpSize;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblPx;
        private System.Windows.Forms.NumericUpDown numManualHeight;
        private System.Windows.Forms.Label lblManualX;
        private System.Windows.Forms.NumericUpDown numManualWidth;
        private System.Windows.Forms.RadioButton rbManual;
        private System.Windows.Forms.Label lblRatioResult;
        private System.Windows.Forms.NumericUpDown numFactor;
        private System.Windows.Forms.Label lblTimes;
        private System.Windows.Forms.NumericUpDown numRatioHeight;
        private System.Windows.Forms.Label lblColon;
        private System.Windows.Forms.NumericUpDown numRatioWidth;
        private System.Windows.Forms.RadioButton rbRatio;
        private System.Windows.Forms.Label lblDetected;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
