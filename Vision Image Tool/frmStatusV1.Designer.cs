namespace VisionImageTool
{
    partial class frmStatusV1
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
        ///
        /// Bố cục: pnlBanner (trên, màu nền đổi theo kết luận lúc chạy) - tabView (giữa) gồm
        ///   tabOverview: pnlList cuộn được, chứa tlpItems (các DÒNG trạng thái thêm bằng code, gom theo khối)
        ///   tabProd:     tvProd = cây cấu hình sản xuất theo Part Type + pnlProdBar (tóm tắt, Copy JSON)
        /// - tlpButtons (dưới). Kiểu chữ của từng dòng nằm trong AddItemRow.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlBanner = new System.Windows.Forms.Panel();
            this.lblSub = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tabView = new System.Windows.Forms.TabControl();
            this.tabOverview = new System.Windows.Forms.TabPage();
            this.pnlList = new System.Windows.Forms.Panel();
            this.tlpItems = new System.Windows.Forms.TableLayoutPanel();
            this.tabProd = new System.Windows.Forms.TabPage();
            this.tvProd = new System.Windows.Forms.TreeView();
            this.pnlProdBar = new System.Windows.Forms.Panel();
            this.lblProdSummary = new System.Windows.Forms.Label();
            this.btnExpandAll = new System.Windows.Forms.Button();
            this.btnViewProduction = new System.Windows.Forms.Button();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnLog = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlBanner.SuspendLayout();
            this.tabView.SuspendLayout();
            this.tabOverview.SuspendLayout();
            this.pnlList.SuspendLayout();
            this.tabProd.SuspendLayout();
            this.pnlProdBar.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlBanner
            //
            this.pnlBanner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.pnlBanner.Controls.Add(this.lblSub);
            this.pnlBanner.Controls.Add(this.lblTitle);
            this.pnlBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBanner.Location = new System.Drawing.Point(0, 0);
            this.pnlBanner.Name = "pnlBanner";
            this.pnlBanner.Padding = new System.Windows.Forms.Padding(18, 10, 18, 8);
            this.pnlBanner.Size = new System.Drawing.Size(820, 74);
            this.pnlBanner.TabIndex = 0;
            //
            // lblSub
            //
            this.lblSub.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSub.ForeColor = System.Drawing.Color.White;
            this.lblSub.Location = new System.Drawing.Point(18, 42);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(784, 22);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "Tất cả hạng mục đều OK.";
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(18, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(784, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "✔  SẴN SÀNG";
            //
            // tabView
            //
            this.tabView.Controls.Add(this.tabOverview);
            this.tabView.Controls.Add(this.tabProd);
            this.tabView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabView.Location = new System.Drawing.Point(0, 74);
            this.tabView.Name = "tabView";
            this.tabView.Padding = new System.Drawing.Point(14, 5);
            this.tabView.SelectedIndex = 0;
            this.tabView.Size = new System.Drawing.Size(820, 492);
            this.tabView.TabIndex = 1;
            //
            // tabOverview
            //
            this.tabOverview.BackColor = System.Drawing.Color.White;
            this.tabOverview.Controls.Add(this.pnlList);
            this.tabOverview.Location = new System.Drawing.Point(4, 30);
            this.tabOverview.Name = "tabOverview";
            this.tabOverview.Size = new System.Drawing.Size(812, 458);
            this.tabOverview.TabIndex = 0;
            this.tabOverview.Text = "Tổng quan";
            //
            // pnlList
            //
            this.pnlList.AutoScroll = true;
            this.pnlList.BackColor = System.Drawing.Color.White;
            this.pnlList.Controls.Add(this.tlpItems);
            this.pnlList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlList.Location = new System.Drawing.Point(0, 0);
            this.pnlList.Name = "pnlList";
            this.pnlList.Padding = new System.Windows.Forms.Padding(16, 4, 16, 8);
            this.pnlList.Size = new System.Drawing.Size(812, 458);
            this.pnlList.TabIndex = 0;
            this.pnlList.Resize += new System.EventHandler(this.pnlList_Resize);
            //
            // tlpItems
            //
            this.tlpItems.AutoSize = true;
            this.tlpItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpItems.ColumnCount = 3;
            this.tlpItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpItems.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpItems.Location = new System.Drawing.Point(16, 4);
            this.tlpItems.Margin = new System.Windows.Forms.Padding(0);
            this.tlpItems.Name = "tlpItems";
            this.tlpItems.RowCount = 1;
            this.tlpItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpItems.Size = new System.Drawing.Size(780, 0);
            this.tlpItems.TabIndex = 0;
            this.tlpItems.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.tlpItems_CellPaint);
            //
            // tabProd
            //
            this.tabProd.BackColor = System.Drawing.Color.White;
            this.tabProd.Controls.Add(this.tvProd);
            this.tabProd.Controls.Add(this.pnlProdBar);
            this.tabProd.Location = new System.Drawing.Point(4, 30);
            this.tabProd.Name = "tabProd";
            this.tabProd.Padding = new System.Windows.Forms.Padding(8);
            this.tabProd.Size = new System.Drawing.Size(812, 458);
            this.tabProd.TabIndex = 1;
            this.tabProd.Text = "Cấu hình sản xuất";
            //
            // tvProd
            //
            this.tvProd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvProd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvProd.FullRowSelect = true;
            this.tvProd.HideSelection = false;
            this.tvProd.ItemHeight = 24;
            this.tvProd.Location = new System.Drawing.Point(8, 8);
            this.tvProd.Name = "tvProd";
            this.tvProd.ShowLines = false;
            this.tvProd.Size = new System.Drawing.Size(796, 402);
            this.tvProd.TabIndex = 0;
            //
            // pnlProdBar
            //
            this.pnlProdBar.Controls.Add(this.lblProdSummary);
            this.pnlProdBar.Controls.Add(this.btnExpandAll);
            this.pnlProdBar.Controls.Add(this.btnViewProduction);
            this.pnlProdBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlProdBar.Location = new System.Drawing.Point(8, 410);
            this.pnlProdBar.Name = "pnlProdBar";
            this.pnlProdBar.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.pnlProdBar.Size = new System.Drawing.Size(796, 40);
            this.pnlProdBar.TabIndex = 1;
            //
            // lblProdSummary
            //
            this.lblProdSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProdSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblProdSummary.Location = new System.Drawing.Point(0, 6);
            this.lblProdSummary.Name = "lblProdSummary";
            this.lblProdSummary.Size = new System.Drawing.Size(526, 34);
            this.lblProdSummary.TabIndex = 0;
            this.lblProdSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnExpandAll
            //
            this.btnExpandAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExpandAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandAll.Location = new System.Drawing.Point(526, 6);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(110, 34);
            this.btnExpandAll.TabIndex = 1;
            this.btnExpandAll.Text = "Mở / thu gọn";
            this.btnExpandAll.UseVisualStyleBackColor = true;
            this.btnExpandAll.Click += new System.EventHandler(this.btnExpandAll_Click);
            //
            // btnViewProduction
            //
            this.btnViewProduction.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnViewProduction.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnViewProduction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewProduction.Location = new System.Drawing.Point(636, 6);
            this.btnViewProduction.Name = "btnViewProduction";
            this.btnViewProduction.Size = new System.Drawing.Size(160, 34);
            this.btnViewProduction.TabIndex = 2;
            this.btnViewProduction.Text = "Xem production.py";
            this.btnViewProduction.UseVisualStyleBackColor = true;
            this.btnViewProduction.Click += new System.EventHandler(this.btnViewProduction_Click);
            //
            // tlpButtons
            //
            this.tlpButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.tlpButtons.ColumnCount = 4;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpButtons.Controls.Add(this.btnLog, 0, 0);
            this.tlpButtons.Controls.Add(this.btnRetry, 2, 0);
            this.tlpButtons.Controls.Add(this.btnClose, 3, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpButtons.Location = new System.Drawing.Point(0, 566);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 10);
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(820, 54);
            this.tlpButtons.TabIndex = 2;
            //
            // btnLog
            //
            this.btnLog.BackColor = System.Drawing.Color.White;
            this.btnLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLog.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnLog.Location = new System.Drawing.Point(16, 8);
            this.btnLog.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(146, 36);
            this.btnLog.TabIndex = 0;
            this.btnLog.Text = "Xem log chi tiết";
            this.btnLog.UseVisualStyleBackColor = false;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            //
            // btnRetry
            //
            this.btnRetry.BackColor = System.Drawing.Color.White;
            this.btnRetry.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.btnRetry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRetry.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnRetry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRetry.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnRetry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnRetry.Location = new System.Drawing.Point(572, 8);
            this.btnRetry.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(126, 36);
            this.btnRetry.TabIndex = 1;
            this.btnRetry.Text = "Kiểm tra lại";
            this.btnRetry.UseVisualStyleBackColor = false;
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(702, 8);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(106, 36);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = false;
            //
            // frmStatusV1
            //
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(820, 620);
            this.Controls.Add(this.tabView);
            this.Controls.Add(this.tlpButtons);
            this.Controls.Add(this.pnlBanner);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 420);
            this.Name = "frmStatusV1";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trạng thái hệ thống UnitX";
            this.pnlBanner.ResumeLayout(false);
            this.tabView.ResumeLayout(false);
            this.tabOverview.ResumeLayout(false);
            this.pnlList.ResumeLayout(false);
            this.pnlList.PerformLayout();
            this.tabProd.ResumeLayout(false);
            this.pnlProdBar.ResumeLayout(false);
            this.tlpButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBanner;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TabControl tabView;
        private System.Windows.Forms.TabPage tabOverview;
        private System.Windows.Forms.Panel pnlList;
        private System.Windows.Forms.TableLayoutPanel tlpItems;
        private System.Windows.Forms.TabPage tabProd;
        private System.Windows.Forms.TreeView tvProd;
        private System.Windows.Forms.Panel pnlProdBar;
        private System.Windows.Forms.Label lblProdSummary;
        private System.Windows.Forms.Button btnExpandAll;
        private System.Windows.Forms.Button btnViewProduction;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnClose;
    }
}
