namespace VisionImageTool
{
    partial class frmRenameImage
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
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblExt = new System.Windows.Forms.Label();
            this.lblCheck = new System.Windows.Forms.Label();
            this.chkPair = new System.Windows.Forms.CheckBox();
            this.lblPairOld = new System.Windows.Forms.Label();
            this.txtPairName = new System.Windows.Forms.TextBox();
            this.lblPairExt = new System.Windows.Forms.Label();
            this.lblPairCheck = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblFolder
            //
            this.lblFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFolder.AutoEllipsis = true;
            this.lblFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblFolder.Location = new System.Drawing.Point(12, 10);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(576, 18);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "Thư mục:";
            //
            // lblName
            //
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblName.Location = new System.Drawing.Point(12, 34);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(56, 15);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "Tên mới:";
            //
            // txtName
            //
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtName.Location = new System.Drawing.Point(12, 54);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(520, 23);
            this.txtName.TabIndex = 2;
            this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
            //
            // lblExt
            //
            this.lblExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExt.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblExt.Location = new System.Drawing.Point(536, 57);
            this.lblExt.Name = "lblExt";
            this.lblExt.Size = new System.Drawing.Size(52, 18);
            this.lblExt.TabIndex = 3;
            this.lblExt.Text = ".jpg";
            //
            // lblCheck
            //
            this.lblCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCheck.Location = new System.Drawing.Point(12, 82);
            this.lblCheck.Name = "lblCheck";
            this.lblCheck.Size = new System.Drawing.Size(576, 34);
            this.lblCheck.TabIndex = 4;
            //
            // chkPair
            //
            this.chkPair.AutoSize = true;
            this.chkPair.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.chkPair.Location = new System.Drawing.Point(12, 124);
            this.chkPair.Name = "chkPair";
            this.chkPair.Size = new System.Drawing.Size(206, 19);
            this.chkPair.TabIndex = 5;
            this.chkPair.Text = "Đổi tên cả ảnh cặp Raw ↔ Mask";
            this.chkPair.UseVisualStyleBackColor = true;
            this.chkPair.CheckedChanged += new System.EventHandler(this.chkPair_CheckedChanged);
            //
            // lblPairOld
            //
            this.lblPairOld.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPairOld.AutoEllipsis = true;
            this.lblPairOld.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblPairOld.Location = new System.Drawing.Point(30, 148);
            this.lblPairOld.Name = "lblPairOld";
            this.lblPairOld.Size = new System.Drawing.Size(558, 18);
            this.lblPairOld.TabIndex = 6;
            this.lblPairOld.Text = "Hiện tại:";
            //
            // txtPairName
            //
            this.txtPairName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPairName.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtPairName.Location = new System.Drawing.Point(30, 168);
            this.txtPairName.Name = "txtPairName";
            this.txtPairName.Size = new System.Drawing.Size(502, 23);
            this.txtPairName.TabIndex = 7;
            this.txtPairName.TextChanged += new System.EventHandler(this.txtPairName_TextChanged);
            //
            // lblPairExt
            //
            this.lblPairExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPairExt.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblPairExt.Location = new System.Drawing.Point(536, 171);
            this.lblPairExt.Name = "lblPairExt";
            this.lblPairExt.Size = new System.Drawing.Size(52, 18);
            this.lblPairExt.TabIndex = 8;
            this.lblPairExt.Text = ".jpg";
            //
            // lblPairCheck
            //
            this.lblPairCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPairCheck.Location = new System.Drawing.Point(30, 196);
            this.lblPairCheck.Name = "lblPairCheck";
            this.lblPairCheck.Size = new System.Drawing.Size(558, 34);
            this.lblPairCheck.TabIndex = 9;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(402, 244);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(95, 32);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "Đổi tên";
            this.btnOk.UseVisualStyleBackColor = false;
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(503, 244);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Huỷ";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // frmRenameImage
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(600, 288);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblPairCheck);
            this.Controls.Add(this.lblPairExt);
            this.Controls.Add(this.txtPairName);
            this.Controls.Add(this.lblPairOld);
            this.Controls.Add(this.chkPair);
            this.Controls.Add(this.lblCheck);
            this.Controls.Add(this.lblExt);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblFolder);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRenameImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Đổi tên ảnh";
            this.Shown += new System.EventHandler(this.frmRenameImage_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblExt;
        private System.Windows.Forms.Label lblCheck;
        private System.Windows.Forms.CheckBox chkPair;
        private System.Windows.Forms.Label lblPairOld;
        private System.Windows.Forms.TextBox txtPairName;
        private System.Windows.Forms.Label lblPairExt;
        private System.Windows.Forms.Label lblPairCheck;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
