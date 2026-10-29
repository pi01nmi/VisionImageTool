namespace VisionImageTool
{
    partial class frmMainV1
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
        /// BỐ CỤC tab Capture &amp; Monitor (2026-10-02, theo mockup):
        ///   pnlAppHeader (Top) : logo + tiêu đề + btnTabCapture / btnTabDataset có icon (dải tab của tabMain được ẩn)
        ///   tlpRoot = [tlpTopRow: pnlPartCard | pnlPartToolbar | pnlCameraToolbar]
        ///             [scWork: pnlListCard (Raw / Mask) | pnlPreviewCard]
        ///             [tlpBottomRow: pnlPathCard (Lưu trữ &amp; Push) | pnlSdkCard (Mạng &amp; Thiết bị)]
        ///   statusStrip: trạng thái + ☰ Hiển thị (ẩn / hiện khu vực) + Kiểm tra trạng thái / Xem log / Xoá log
        /// Các thẻ là UnitX.CardPanel (bo góc). Hành vi (đổi Raw / Mask, Trước / Sau, ô trạng thái, ẩn / hiện)
        /// nằm ở frmMainV1.CaptureLayout.cs. Toàn bộ dùng TableLayoutPanel, KHÔNG dùng FlowLayoutPanel.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlAppHeader = new System.Windows.Forms.Panel();
            this.btnTabDataset = new System.Windows.Forms.Button();
            this.btnTabCapture = new System.Windows.Forms.Button();
            this.lblAppSubtitle = new System.Windows.Forms.Label();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.picAppLogo = new System.Windows.Forms.PictureBox();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDataset = new System.Windows.Forms.TabPage();
            this.ucDataset = new VisionImageTool.ucDatasetManager();
            this.tabCapture = new System.Windows.Forms.TabPage();
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.tlpTopRow = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPartCard = new VisionImageTool.CardPanel();
            this.tlpPartFields = new System.Windows.Forms.TableLayoutPanel();
            this.lblPartCardTitle = new System.Windows.Forms.Label();
            this.lblPartType = new System.Windows.Forms.Label();
            this.cboPartType = new System.Windows.Forms.ComboBox();
            this.lblPartID = new System.Windows.Forms.Label();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.lblCaptureConfig = new System.Windows.Forms.Label();
            this.cboCaptureConfig = new System.Windows.Forms.ComboBox();
            this.lblCameraID = new System.Windows.Forms.Label();
            this.cboCameraID = new System.Windows.Forms.ComboBox();
            this.lblDelay = new System.Windows.Forms.Label();
            this.numDelay = new System.Windows.Forms.NumericUpDown();
            this.lblSequenceIO = new System.Windows.Forms.Label();
            this.cboSequenceIO = new System.Windows.Forms.ComboBox();
            this.pnlPartToolbar = new VisionImageTool.CardPanel();
            this.tlpPartButtons = new System.Windows.Forms.TableLayoutPanel();
            this.lblPartToolbar = new System.Windows.Forms.Label();
            this.btnPartStart = new System.Windows.Forms.Button();
            this.btnRemovePart = new System.Windows.Forms.Button();
            this.btnRestartPart = new System.Windows.Forms.Button();
            this.btnSelectSequence = new System.Windows.Forms.Button();
            this.btnSystemReset = new System.Windows.Forms.Button();
            this.pnlCameraToolbar = new VisionImageTool.CardPanel();
            this.tlpCameraButtons = new System.Windows.Forms.TableLayoutPanel();
            this.lblCameraToolbar = new System.Windows.Forms.Label();
            this.btnTriggerOnce = new System.Windows.Forms.Button();
            this.btnTriggerContinuous = new System.Windows.Forms.Button();
            this.btnCameraReset = new System.Windows.Forms.Button();
            this.tlpTriggerRepeat = new System.Windows.Forms.TableLayoutPanel();
            this.lblTriggerRepeat = new System.Windows.Forms.Label();
            this.numTriggerRepeat = new System.Windows.Forms.NumericUpDown();
            this.lblTriggerRepeatUnit = new System.Windows.Forms.Label();
            this.scWork = new System.Windows.Forms.SplitContainer();
            this.pnlListCard = new VisionImageTool.CardPanel();
            this.tlpListCard = new System.Windows.Forms.TableLayoutPanel();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.lblListTotal = new System.Windows.Forms.Label();
            this.tlpListTabs = new System.Windows.Forms.TableLayoutPanel();
            this.btnListRaw = new System.Windows.Forms.Button();
            this.btnListMask = new System.Windows.Forms.Button();
            this.pnlListHost = new System.Windows.Forms.Panel();
            this.pnlRawCard = new System.Windows.Forms.Panel();
            this.tlpRaw = new System.Windows.Forms.TableLayoutPanel();
            this.lblRawHeader = new System.Windows.Forms.Label();
            this.lblRawCount = new System.Windows.Forms.Label();
            this.lvRawImage = new System.Windows.Forms.ListView();
            this.chRawIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chRawName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chRawTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tlpRawPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnRawFirst = new System.Windows.Forms.Button();
            this.btnRawPrev = new System.Windows.Forms.Button();
            this.lblRawPage = new System.Windows.Forms.Label();
            this.btnRawNext = new System.Windows.Forms.Button();
            this.btnRawLast = new System.Windows.Forms.Button();
            this.pnlMaskCard = new System.Windows.Forms.Panel();
            this.tlpMask = new System.Windows.Forms.TableLayoutPanel();
            this.lblMaskHeader = new System.Windows.Forms.Label();
            this.lblMaskCount = new System.Windows.Forms.Label();
            this.lvMaskImage = new System.Windows.Forms.ListView();
            this.chMaskIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMaskName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMaskTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tlpMaskPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnMaskFirst = new System.Windows.Forms.Button();
            this.btnMaskPrev = new System.Windows.Forms.Button();
            this.lblMaskPage = new System.Windows.Forms.Label();
            this.btnMaskNext = new System.Windows.Forms.Button();
            this.btnMaskLast = new System.Windows.Forms.Button();
            this.pnlPreviewCard = new VisionImageTool.CardPanel();
            this.tlpPreview = new System.Windows.Forms.TableLayoutPanel();
            this.lblPreviewTitle = new System.Windows.Forms.Label();
            this.tlpPreviewToggle = new System.Windows.Forms.TableLayoutPanel();
            this.btnViewRaw = new System.Windows.Forms.Button();
            this.btnViewMask = new System.Windows.Forms.Button();
            this.lblPreviewKeyHint = new System.Windows.Forms.Label();
            this.lblPreviewName = new System.Windows.Forms.Label();
            this.lblPreviewMeta = new System.Windows.Forms.Label();
            this.pnlPreviewBody = new System.Windows.Forms.Panel();
            this.lblResult = new System.Windows.Forms.Label();
            this.lblPreviewHint = new System.Windows.Forms.Label();
            this.picPreview = new VisionImageTool.ZoomPictureBox();
            this.tlpPreviewNav = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevImage = new System.Windows.Forms.Button();
            this.lblImageNav = new System.Windows.Forms.Label();
            this.btnNextImage = new System.Windows.Forms.Button();
            this.lblPreviewHeader = new System.Windows.Forms.Label();
            this.tlpBottomRow = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPathCard = new VisionImageTool.CardPanel();
            this.tlpPath = new System.Windows.Forms.TableLayoutPanel();
            this.lblPathCardTitle = new System.Windows.Forms.Label();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblImageName = new System.Windows.Forms.Label();
            this.txtImageName = new System.Windows.Forms.TextBox();
            this.lblNamePrefix = new System.Windows.Forms.Label();
            this.cboNamePrefix = new System.Windows.Forms.ComboBox();
            this.lblImageFormat = new System.Windows.Forms.Label();
            this.cboImageFormat = new System.Windows.Forms.ComboBox();
            this.tlpPathOptions = new System.Windows.Forms.TableLayoutPanel();
            this.chkSaveRaw = new System.Windows.Forms.CheckBox();
            this.chkSaveCallback = new System.Windows.Forms.CheckBox();
            this.lblJpegQuality = new System.Windows.Forms.Label();
            this.numJpegQuality = new System.Windows.Forms.NumericUpDown();
            this.lblJpegQualityHint = new System.Windows.Forms.Label();
            this.chkWatchFolder = new System.Windows.Forms.CheckBox();
            this.lblNamePreview = new System.Windows.Forms.Label();
            this.tlpPathButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnPushAll = new System.Windows.Forms.Button();
            this.btnPushSelected = new System.Windows.Forms.Button();
            this.btnClearPushHistory = new System.Windows.Forms.Button();
            this.pnlSdkCard = new VisionImageTool.CardPanel();
            this.tlpSdk = new System.Windows.Forms.TableLayoutPanel();
            this.lblSdkCardTitle = new System.Windows.Forms.Label();
            this.lblSdkHost = new System.Windows.Forms.Label();
            this.txtSdkHost = new System.Windows.Forms.TextBox();
            this.btnSdkTest = new System.Windows.Forms.Button();
            this.btnSdkSave = new System.Windows.Forms.Button();
            this.lblSdkDivider = new System.Windows.Forms.Label();
            this.lblConnTitle = new System.Windows.Forms.Label();
            this.pnlConnTiles = new System.Windows.Forms.Panel();
            this.tlpPortsHead = new System.Windows.Forms.TableLayoutPanel();
            this.lblPortsTitle = new System.Windows.Forms.Label();
            this.lnkPorts = new System.Windows.Forms.LinkLabel();
            this.tlpPorts = new System.Windows.Forms.TableLayoutPanel();
            this.lblSdkGrpc = new System.Windows.Forms.Label();
            this.numSdkGrpc = new System.Windows.Forms.NumericUpDown();
            this.lblSdkZmq = new System.Windows.Forms.Label();
            this.numSdkZmq = new System.Windows.Forms.NumericUpDown();
            this.lblSdkBlob = new System.Windows.Forms.Label();
            this.numSdkBlob = new System.Windows.Forms.NumericUpDown();
            this.lblSdkClient = new System.Windows.Forms.Label();
            this.numSdkClient = new System.Windows.Forms.NumericUpDown();
            this.lblSdkStatus = new System.Windows.Forms.Label();
            this.lblPortsSummary = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslUnitXTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslUnitX = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslCameraTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslCamera = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslSdkTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslSdk = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsddSections = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiSecPart = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecControl = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecStorage = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecNetwork = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSepSections = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiSecShowAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSecHideAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbCheckStatus = new System.Windows.Forms.ToolStripButton();
            this.tsbShowLog = new System.Windows.Forms.ToolStripButton();
            this.tsbClearLog = new System.Windows.Forms.ToolStripButton();
            this.tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.tspbPush = new System.Windows.Forms.ToolStripProgressBar();
            this.tsslOverall = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslClock = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlAppHeader.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabDataset.SuspendLayout();
            this.tabCapture.SuspendLayout();
            this.tlpRoot.SuspendLayout();
            this.tlpTopRow.SuspendLayout();
            this.pnlPartCard.SuspendLayout();
            this.tlpPartFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            this.pnlPartToolbar.SuspendLayout();
            this.tlpPartButtons.SuspendLayout();
            this.pnlCameraToolbar.SuspendLayout();
            this.tlpCameraButtons.SuspendLayout();
            this.tlpTriggerRepeat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTriggerRepeat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scWork)).BeginInit();
            this.scWork.Panel1.SuspendLayout();
            this.scWork.Panel2.SuspendLayout();
            this.scWork.SuspendLayout();
            this.pnlListCard.SuspendLayout();
            this.tlpListCard.SuspendLayout();
            this.tlpListTabs.SuspendLayout();
            this.pnlListHost.SuspendLayout();
            this.pnlRawCard.SuspendLayout();
            this.tlpRaw.SuspendLayout();
            this.tlpRawPager.SuspendLayout();
            this.pnlMaskCard.SuspendLayout();
            this.tlpMask.SuspendLayout();
            this.tlpMaskPager.SuspendLayout();
            this.pnlPreviewCard.SuspendLayout();
            this.tlpPreview.SuspendLayout();
            this.tlpPreviewToggle.SuspendLayout();
            this.pnlPreviewBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tlpPreviewNav.SuspendLayout();
            this.tlpBottomRow.SuspendLayout();
            this.pnlPathCard.SuspendLayout();
            this.tlpPath.SuspendLayout();
            this.tlpPathOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numJpegQuality)).BeginInit();
            this.tlpPathButtons.SuspendLayout();
            this.pnlSdkCard.SuspendLayout();
            this.tlpSdk.SuspendLayout();
            this.tlpPortsHead.SuspendLayout();
            this.tlpPorts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkGrpc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkZmq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkBlob)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkClient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAppLogo)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlAppHeader
            // 
            this.pnlAppHeader.BackColor = System.Drawing.Color.White;
            this.pnlAppHeader.Controls.Add(this.btnTabDataset);
            this.pnlAppHeader.Controls.Add(this.btnTabCapture);
            this.pnlAppHeader.Controls.Add(this.lblAppSubtitle);
            this.pnlAppHeader.Controls.Add(this.lblAppTitle);
            this.pnlAppHeader.Controls.Add(this.picAppLogo);
            this.pnlAppHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAppHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlAppHeader.Name = "pnlAppHeader";
            this.pnlAppHeader.Size = new System.Drawing.Size(1320, 58);
            this.pnlAppHeader.TabIndex = 0;
            // 
            // btnTabDataset
            // 
            this.btnTabDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTabDataset.BackColor = System.Drawing.Color.White;
            this.btnTabDataset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabDataset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnTabDataset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabDataset.Image = global::VisionImageTool.Properties.Resources.IconDataset;
            this.btnTabDataset.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnTabDataset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabDataset.Location = new System.Drawing.Point(1112, 11);
            this.btnTabDataset.Padding = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.btnTabDataset.Name = "btnTabDataset";
            this.btnTabDataset.Size = new System.Drawing.Size(180, 36);
            this.btnTabDataset.TabIndex = 3;
            this.btnTabDataset.Text = "Dataset Manager";
            this.btnTabDataset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnTabDataset.UseVisualStyleBackColor = false;
            this.btnTabDataset.Click += new System.EventHandler(this.btnTabDataset_Click);
            // 
            // btnTabCapture
            // 
            this.btnTabCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTabCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(255)))));
            this.btnTabCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabCapture.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(213)))), ((int)(((byte)(250)))));
            this.btnTabCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabCapture.Image = global::VisionImageTool.Properties.Resources.IconCapture;
            this.btnTabCapture.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnTabCapture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(91)))), ((int)(((byte)(181)))));
            this.btnTabCapture.Location = new System.Drawing.Point(924, 11);
            this.btnTabCapture.Padding = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.btnTabCapture.Name = "btnTabCapture";
            this.btnTabCapture.Size = new System.Drawing.Size(180, 36);
            this.btnTabCapture.TabIndex = 2;
            this.btnTabCapture.Text = "Capture & Monitor";
            this.btnTabCapture.UseMnemonic = false;
            this.btnTabCapture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnTabCapture.UseVisualStyleBackColor = false;
            this.btnTabCapture.Click += new System.EventHandler(this.btnTabCapture_Click);
            // 
            // lblAppSubtitle
            // 
            this.lblAppSubtitle.AutoSize = true;
            this.lblAppSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblAppSubtitle.Location = new System.Drawing.Point(254, 22);
            this.lblAppSubtitle.Name = "lblAppSubtitle";
            this.lblAppSubtitle.Size = new System.Drawing.Size(244, 15);
            this.lblAppSubtitle.TabIndex = 1;
            this.lblAppSubtitle.Text = "Chụp ảnh · Theo dõi thiết bị · Quản lý dữ liệu";
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblAppTitle.Location = new System.Drawing.Point(60, 14);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(180, 28);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "Vision Image Tool";
            // 
            // picAppLogo
            // 
            this.picAppLogo.Image = global::VisionImageTool.Properties.Resources.AppLogo;
            this.picAppLogo.Location = new System.Drawing.Point(20, 12);
            this.picAppLogo.Name = "picAppLogo";
            this.picAppLogo.Size = new System.Drawing.Size(34, 34);
            this.picAppLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAppLogo.TabIndex = 4;
            this.picAppLogo.TabStop = false;
            // 
            // tabMain
            // 
            this.tabMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabMain.Controls.Add(this.tabDataset);
            this.tabMain.Controls.Add(this.tabCapture);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.tabMain.ItemSize = new System.Drawing.Size(0, 1);
            this.tabMain.Location = new System.Drawing.Point(0, 58);
            this.tabMain.Name = "tabMain";
            this.tabMain.Padding = new System.Drawing.Point(12, 4);
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1320, 782);
            this.tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabMain.TabIndex = 1;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // tabDataset
            // 
            this.tabDataset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.tabDataset.Controls.Add(this.ucDataset);
            this.tabDataset.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabDataset.Location = new System.Drawing.Point(4, 5);
            this.tabDataset.Margin = new System.Windows.Forms.Padding(0);
            this.tabDataset.Name = "tabDataset";
            this.tabDataset.Size = new System.Drawing.Size(1312, 773);
            this.tabDataset.TabIndex = 1;
            this.tabDataset.Text = "Dataset Manager";
            // 
            // ucDataset
            // 
            this.ucDataset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.ucDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucDataset.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ucDataset.Location = new System.Drawing.Point(0, 0);
            this.ucDataset.Name = "ucDataset";
            this.ucDataset.Padding = new System.Windows.Forms.Padding(8);
            this.ucDataset.Size = new System.Drawing.Size(1312, 773);
            this.ucDataset.TabIndex = 1;
            // 
            // tabCapture
            // 
            this.tabCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.tabCapture.Controls.Add(this.tlpRoot);
            this.tabCapture.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabCapture.Location = new System.Drawing.Point(4, 5);
            this.tabCapture.Margin = new System.Windows.Forms.Padding(0);
            this.tabCapture.Name = "tabCapture";
            this.tabCapture.Size = new System.Drawing.Size(1312, 773);
            this.tabCapture.TabIndex = 0;
            this.tabCapture.Text = "Capture & Monitor";
            // 
            // tlpRoot
            // 
            this.tlpRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.tlpTopRow, 0, 0);
            this.tlpRoot.Controls.Add(this.scWork, 0, 1);
            this.tlpRoot.Controls.Add(this.tlpBottomRow, 0, 2);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(12, 12, 0, 0);
            this.tlpRoot.RowCount = 3;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 262F));
            this.tlpRoot.Size = new System.Drawing.Size(1312, 773);
            this.tlpRoot.TabIndex = 0;
            // 
            // tlpTopRow
            // 
            this.tlpTopRow.ColumnCount = 3;
            this.tlpTopRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpTopRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpTopRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpTopRow.Controls.Add(this.pnlPartCard, 0, 0);
            this.tlpTopRow.Controls.Add(this.pnlPartToolbar, 1, 0);
            this.tlpTopRow.Controls.Add(this.pnlCameraToolbar, 2, 0);
            this.tlpTopRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopRow.Location = new System.Drawing.Point(12, 12);
            this.tlpTopRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRow.Name = "tlpTopRow";
            this.tlpTopRow.RowCount = 1;
            this.tlpTopRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRow.Size = new System.Drawing.Size(1300, 176);
            this.tlpTopRow.TabIndex = 0;
            // 
            // pnlPartCard
            // 
            this.pnlPartCard.BackColor = System.Drawing.Color.White;
            this.pnlPartCard.Controls.Add(this.tlpPartFields);
            this.pnlPartCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPartCard.Location = new System.Drawing.Point(0, 0);
            this.pnlPartCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.pnlPartCard.Name = "pnlPartCard";
            this.pnlPartCard.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlPartCard.HideButton = true;
            this.pnlPartCard.Size = new System.Drawing.Size(378, 164);
            this.pnlPartCard.TabIndex = 0;
            // 
            // tlpPartFields
            // 
            this.tlpPartFields.ColumnCount = 4;
            this.tlpPartFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpPartFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPartFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tlpPartFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPartFields.Controls.Add(this.lblPartCardTitle, 0, 0);
            this.tlpPartFields.Controls.Add(this.lblPartType, 0, 1);
            this.tlpPartFields.Controls.Add(this.cboPartType, 1, 1);
            this.tlpPartFields.Controls.Add(this.lblPartID, 2, 1);
            this.tlpPartFields.Controls.Add(this.txtPartID, 3, 1);
            this.tlpPartFields.Controls.Add(this.lblCaptureConfig, 0, 2);
            this.tlpPartFields.Controls.Add(this.cboCaptureConfig, 1, 2);
            this.tlpPartFields.Controls.Add(this.lblCameraID, 2, 2);
            this.tlpPartFields.Controls.Add(this.cboCameraID, 3, 2);
            this.tlpPartFields.Controls.Add(this.lblDelay, 0, 3);
            this.tlpPartFields.Controls.Add(this.numDelay, 1, 3);
            this.tlpPartFields.Controls.Add(this.lblSequenceIO, 2, 3);
            this.tlpPartFields.Controls.Add(this.cboSequenceIO, 3, 3);
            this.tlpPartFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPartFields.Location = new System.Drawing.Point(18, 10);
            this.tlpPartFields.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPartFields.Name = "tlpPartFields";
            this.tlpPartFields.RowCount = 4;
            this.tlpPartFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpPartFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpPartFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpPartFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpPartFields.Size = new System.Drawing.Size(342, 142);
            this.tlpPartFields.TabIndex = 0;
            // 
            // lblPartCardTitle
            // 
            this.lblPartCardTitle.AutoSize = true;
            this.tlpPartFields.SetColumnSpan(this.lblPartCardTitle, 4);
            this.lblPartCardTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPartCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPartCardTitle.Location = new System.Drawing.Point(0, 4);
            this.lblPartCardTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPartCardTitle.Name = "lblPartCardTitle";
            this.lblPartCardTitle.Size = new System.Drawing.Size(138, 19);
            this.lblPartCardTitle.TabIndex = 0;
            this.lblPartCardTitle.Text = "1   CẤU HÌNH PART";
            // 
            // lblPartType
            // 
            this.lblPartType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPartType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPartType.Location = new System.Drawing.Point(0, 30);
            this.lblPartType.Margin = new System.Windows.Forms.Padding(0);
            this.lblPartType.Name = "lblPartType";
            this.lblPartType.Size = new System.Drawing.Size(104, 38);
            this.lblPartType.TabIndex = 1;
            this.lblPartType.Text = "Part Type";
            this.lblPartType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboPartType
            // 
            this.cboPartType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboPartType.FormattingEnabled = true;
            this.cboPartType.Location = new System.Drawing.Point(104, 37);
            this.cboPartType.Margin = new System.Windows.Forms.Padding(0, 7, 12, 0);
            this.cboPartType.Name = "cboPartType";
            this.cboPartType.Size = new System.Drawing.Size(61, 23);
            this.cboPartType.TabIndex = 2;
            this.cboPartType.TextChanged += new System.EventHandler(this.cboPartType_TextChanged);
            // 
            // lblPartID
            // 
            this.lblPartID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPartID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPartID.Location = new System.Drawing.Point(177, 30);
            this.lblPartID.Margin = new System.Windows.Forms.Padding(0);
            this.lblPartID.Name = "lblPartID";
            this.lblPartID.Size = new System.Drawing.Size(92, 38);
            this.lblPartID.TabIndex = 3;
            this.lblPartID.Text = "Part ID";
            this.lblPartID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPartID
            // 
            this.txtPartID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPartID.Location = new System.Drawing.Point(269, 37);
            this.txtPartID.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(73, 23);
            this.txtPartID.TabIndex = 4;
            // 
            // lblCaptureConfig
            // 
            this.lblCaptureConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCaptureConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblCaptureConfig.Location = new System.Drawing.Point(0, 68);
            this.lblCaptureConfig.Margin = new System.Windows.Forms.Padding(0);
            this.lblCaptureConfig.Name = "lblCaptureConfig";
            this.lblCaptureConfig.Size = new System.Drawing.Size(104, 38);
            this.lblCaptureConfig.TabIndex = 5;
            this.lblCaptureConfig.Text = "Capture Config";
            this.lblCaptureConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboCaptureConfig
            // 
            this.cboCaptureConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboCaptureConfig.FormattingEnabled = true;
            this.cboCaptureConfig.Location = new System.Drawing.Point(104, 75);
            this.cboCaptureConfig.Margin = new System.Windows.Forms.Padding(0, 7, 12, 0);
            this.cboCaptureConfig.Name = "cboCaptureConfig";
            this.cboCaptureConfig.Size = new System.Drawing.Size(61, 23);
            this.cboCaptureConfig.TabIndex = 6;
            // 
            // lblCameraID
            // 
            this.lblCameraID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCameraID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblCameraID.Location = new System.Drawing.Point(177, 68);
            this.lblCameraID.Margin = new System.Windows.Forms.Padding(0);
            this.lblCameraID.Name = "lblCameraID";
            this.lblCameraID.Size = new System.Drawing.Size(92, 38);
            this.lblCameraID.TabIndex = 7;
            this.lblCameraID.Text = "Camera ID";
            this.lblCameraID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboCameraID
            // 
            this.cboCameraID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboCameraID.FormattingEnabled = true;
            this.cboCameraID.Location = new System.Drawing.Point(269, 75);
            this.cboCameraID.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.cboCameraID.Name = "cboCameraID";
            this.cboCameraID.Size = new System.Drawing.Size(73, 23);
            this.cboCameraID.TabIndex = 8;
            this.cboCameraID.SelectedIndexChanged += new System.EventHandler(this.cboCameraID_Changed);
            this.cboCameraID.TextChanged += new System.EventHandler(this.cboCameraID_Changed);
            // 
            // lblDelay
            // 
            this.lblDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDelay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblDelay.Location = new System.Drawing.Point(0, 106);
            this.lblDelay.Margin = new System.Windows.Forms.Padding(0);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(104, 38);
            this.lblDelay.TabIndex = 9;
            this.lblDelay.Text = "Delay (ms)";
            this.lblDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numDelay
            // 
            this.numDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numDelay.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numDelay.Location = new System.Drawing.Point(104, 113);
            this.numDelay.Margin = new System.Windows.Forms.Padding(0, 7, 12, 0);
            this.numDelay.Maximum = new decimal(new int[] {
            600000,
            0,
            0,
            0});
            this.numDelay.Name = "numDelay";
            this.numDelay.Size = new System.Drawing.Size(61, 23);
            this.numDelay.TabIndex = 10;
            this.numDelay.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // lblSequenceIO
            // 
            this.lblSequenceIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSequenceIO.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSequenceIO.Location = new System.Drawing.Point(177, 106);
            this.lblSequenceIO.Margin = new System.Windows.Forms.Padding(0);
            this.lblSequenceIO.Name = "lblSequenceIO";
            this.lblSequenceIO.Size = new System.Drawing.Size(92, 38);
            this.lblSequenceIO.TabIndex = 11;
            this.lblSequenceIO.Text = "Sequence IO";
            this.lblSequenceIO.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSequenceIO
            // 
            this.cboSequenceIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSequenceIO.FormattingEnabled = true;
            this.cboSequenceIO.Location = new System.Drawing.Point(269, 113);
            this.cboSequenceIO.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.cboSequenceIO.Name = "cboSequenceIO";
            this.cboSequenceIO.Size = new System.Drawing.Size(73, 23);
            this.cboSequenceIO.TabIndex = 12;
            // 
            // pnlPartToolbar
            // 
            this.pnlPartToolbar.BackColor = System.Drawing.Color.White;
            this.pnlPartToolbar.Controls.Add(this.tlpPartButtons);
            this.pnlPartToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPartToolbar.Location = new System.Drawing.Point(390, 0);
            this.pnlPartToolbar.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.pnlPartToolbar.Name = "pnlPartToolbar";
            this.pnlPartToolbar.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlPartToolbar.HideButton = true;
            this.pnlPartToolbar.Size = new System.Drawing.Size(508, 164);
            this.pnlPartToolbar.TabIndex = 1;
            // 
            // tlpPartButtons
            // 
            this.tlpPartButtons.ColumnCount = 6;
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpPartButtons.Controls.Add(this.lblPartToolbar, 0, 0);
            this.tlpPartButtons.Controls.Add(this.btnPartStart, 0, 1);
            this.tlpPartButtons.Controls.Add(this.btnRemovePart, 2, 1);
            this.tlpPartButtons.Controls.Add(this.btnRestartPart, 4, 1);
            this.tlpPartButtons.Controls.Add(this.btnSelectSequence, 0, 2);
            this.tlpPartButtons.Controls.Add(this.btnSystemReset, 3, 2);
            this.tlpPartButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPartButtons.Location = new System.Drawing.Point(18, 10);
            this.tlpPartButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPartButtons.Name = "tlpPartButtons";
            this.tlpPartButtons.RowCount = 3;
            this.tlpPartButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpPartButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpPartButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpPartButtons.Size = new System.Drawing.Size(472, 142);
            this.tlpPartButtons.TabIndex = 0;
            // 
            // lblPartToolbar
            // 
            this.lblPartToolbar.AutoSize = true;
            this.tlpPartButtons.SetColumnSpan(this.lblPartToolbar, 6);
            this.lblPartToolbar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPartToolbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPartToolbar.Location = new System.Drawing.Point(0, 4);
            this.lblPartToolbar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPartToolbar.Name = "lblPartToolbar";
            this.lblPartToolbar.Size = new System.Drawing.Size(235, 19);
            this.lblPartToolbar.TabIndex = 0;
            this.lblPartToolbar.Text = "2   ĐIỀU KHIỂN PART & SEQUENCE";
            this.lblPartToolbar.UseMnemonic = false;
            // 
            // btnPartStart
            // 
            this.btnPartStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
            this.tlpPartButtons.SetColumnSpan(this.btnPartStart, 2);
            this.btnPartStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPartStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPartStart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(239)))), ((int)(((byte)(172)))));
            this.btnPartStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPartStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnPartStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(101)))), ((int)(((byte)(52)))));
            this.btnPartStart.Location = new System.Drawing.Point(0, 36);
            this.btnPartStart.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            this.btnPartStart.Name = "btnPartStart";
            this.btnPartStart.Size = new System.Drawing.Size(148, 44);
            this.btnPartStart.TabIndex = 1;
            this.btnPartStart.Text = "▶  Bắt đầu Part";
            this.btnPartStart.UseVisualStyleBackColor = false;
            this.btnPartStart.Click += new System.EventHandler(this.btnPartStart_Click);
            // 
            // btnRemovePart
            // 
            this.btnRemovePart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.tlpPartButtons.SetColumnSpan(this.btnRemovePart, 2);
            this.btnRemovePart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemovePart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemovePart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(165)))), ((int)(((byte)(165)))));
            this.btnRemovePart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemovePart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRemovePart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.btnRemovePart.Location = new System.Drawing.Point(156, 36);
            this.btnRemovePart.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            this.btnRemovePart.Name = "btnRemovePart";
            this.btnRemovePart.Size = new System.Drawing.Size(148, 44);
            this.btnRemovePart.TabIndex = 2;
            this.btnRemovePart.Text = "■  Dừng Part";
            this.btnRemovePart.UseVisualStyleBackColor = false;
            this.btnRemovePart.Click += new System.EventHandler(this.btnRemovePart_Click);
            // 
            // btnRestartPart
            // 
            this.btnRestartPart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            this.tlpPartButtons.SetColumnSpan(this.btnRestartPart, 2);
            this.btnRestartPart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestartPart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRestartPart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(196)))), ((int)(((byte)(252)))));
            this.btnRestartPart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestartPart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRestartPart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            this.btnRestartPart.Location = new System.Drawing.Point(312, 36);
            this.btnRestartPart.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.btnRestartPart.Name = "btnRestartPart";
            this.btnRestartPart.Size = new System.Drawing.Size(160, 44);
            this.btnRestartPart.TabIndex = 3;
            this.btnRestartPart.Text = "↻  Khởi động lại";
            this.btnRestartPart.UseVisualStyleBackColor = false;
            this.btnRestartPart.Click += new System.EventHandler(this.btnRestartPart_Click);
            // 
            // btnSelectSequence
            // 
            this.btnSelectSequence.BackColor = System.Drawing.Color.White;
            this.tlpPartButtons.SetColumnSpan(this.btnSelectSequence, 3);
            this.btnSelectSequence.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSelectSequence.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnSelectSequence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectSequence.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSelectSequence.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnSelectSequence.Location = new System.Drawing.Point(0, 92);
            this.btnSelectSequence.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            this.btnSelectSequence.Name = "btnSelectSequence";
            this.btnSelectSequence.Size = new System.Drawing.Size(226, 44);
            this.btnSelectSequence.TabIndex = 4;
            this.btnSelectSequence.Text = "Chọn Sequence  ▾";
            this.btnSelectSequence.UseVisualStyleBackColor = false;
            this.btnSelectSequence.Click += new System.EventHandler(this.btnSelectSequence_Click);
            // 
            // btnSystemReset
            // 
            this.btnSystemReset.BackColor = System.Drawing.Color.White;
            this.tlpPartButtons.SetColumnSpan(this.btnSystemReset, 3);
            this.btnSystemReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSystemReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSystemReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnSystemReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSystemReset.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSystemReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnSystemReset.Location = new System.Drawing.Point(234, 92);
            this.btnSystemReset.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.btnSystemReset.Name = "btnSystemReset";
            this.btnSystemReset.Size = new System.Drawing.Size(238, 44);
            this.btnSystemReset.TabIndex = 5;
            this.btnSystemReset.Text = "Đặt lại hệ thống";
            this.btnSystemReset.UseVisualStyleBackColor = false;
            this.btnSystemReset.Click += new System.EventHandler(this.btnSystemReset_Click);
            // 
            // pnlCameraToolbar
            // 
            this.pnlCameraToolbar.BackColor = System.Drawing.Color.White;
            this.pnlCameraToolbar.Controls.Add(this.tlpCameraButtons);
            this.pnlCameraToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCameraToolbar.Location = new System.Drawing.Point(910, 0);
            this.pnlCameraToolbar.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.pnlCameraToolbar.Name = "pnlCameraToolbar";
            this.pnlCameraToolbar.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlCameraToolbar.HideButton = true;
            this.pnlCameraToolbar.Size = new System.Drawing.Size(378, 164);
            this.pnlCameraToolbar.TabIndex = 2;
            // 
            // tlpCameraButtons
            // 
            this.tlpCameraButtons.ColumnCount = 2;
            this.tlpCameraButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCameraButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCameraButtons.Controls.Add(this.lblCameraToolbar, 0, 0);
            this.tlpCameraButtons.Controls.Add(this.btnTriggerOnce, 0, 1);
            this.tlpCameraButtons.Controls.Add(this.btnTriggerContinuous, 1, 1);
            this.tlpCameraButtons.Controls.Add(this.btnCameraReset, 0, 2);
            this.tlpCameraButtons.Controls.Add(this.tlpTriggerRepeat, 1, 2);
            this.tlpCameraButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCameraButtons.Location = new System.Drawing.Point(18, 10);
            this.tlpCameraButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCameraButtons.Name = "tlpCameraButtons";
            this.tlpCameraButtons.RowCount = 3;
            this.tlpCameraButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpCameraButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpCameraButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpCameraButtons.Size = new System.Drawing.Size(342, 142);
            this.tlpCameraButtons.TabIndex = 0;
            // 
            // lblCameraToolbar
            // 
            this.lblCameraToolbar.AutoSize = true;
            this.tlpCameraButtons.SetColumnSpan(this.lblCameraToolbar, 2);
            this.lblCameraToolbar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblCameraToolbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCameraToolbar.Location = new System.Drawing.Point(0, 4);
            this.lblCameraToolbar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblCameraToolbar.Name = "lblCameraToolbar";
            this.lblCameraToolbar.Size = new System.Drawing.Size(87, 19);
            this.lblCameraToolbar.TabIndex = 0;
            this.lblCameraToolbar.Text = "3   CAMERA";
            // 
            // btnTriggerOnce
            // 
            this.btnTriggerOnce.BackColor = System.Drawing.Color.White;
            this.btnTriggerOnce.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTriggerOnce.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTriggerOnce.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(160)))), ((int)(((byte)(230)))));
            this.btnTriggerOnce.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTriggerOnce.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnTriggerOnce.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnTriggerOnce.Location = new System.Drawing.Point(0, 36);
            this.btnTriggerOnce.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            this.btnTriggerOnce.Name = "btnTriggerOnce";
            this.btnTriggerOnce.Size = new System.Drawing.Size(163, 44);
            this.btnTriggerOnce.TabIndex = 1;
            this.btnTriggerOnce.Text = "Chụp một ảnh";
            this.btnTriggerOnce.UseVisualStyleBackColor = false;
            this.btnTriggerOnce.Click += new System.EventHandler(this.btnTriggerOnce_Click);
            // 
            // btnTriggerContinuous
            // 
            this.btnTriggerContinuous.BackColor = System.Drawing.Color.White;
            this.btnTriggerContinuous.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTriggerContinuous.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTriggerContinuous.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(160)))), ((int)(((byte)(230)))));
            this.btnTriggerContinuous.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTriggerContinuous.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnTriggerContinuous.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnTriggerContinuous.Location = new System.Drawing.Point(171, 36);
            this.btnTriggerContinuous.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.btnTriggerContinuous.Name = "btnTriggerContinuous";
            this.btnTriggerContinuous.Size = new System.Drawing.Size(171, 44);
            this.btnTriggerContinuous.TabIndex = 2;
            this.btnTriggerContinuous.Text = "Chụp liên tục";
            this.btnTriggerContinuous.UseVisualStyleBackColor = false;
            this.btnTriggerContinuous.Click += new System.EventHandler(this.btnTriggerContinuous_Click);
            // 
            // btnCameraReset
            // 
            this.btnCameraReset.BackColor = System.Drawing.Color.White;
            this.btnCameraReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCameraReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCameraReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnCameraReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCameraReset.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCameraReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnCameraReset.Location = new System.Drawing.Point(0, 92);
            this.btnCameraReset.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            this.btnCameraReset.Name = "btnCameraReset";
            this.btnCameraReset.Size = new System.Drawing.Size(163, 44);
            this.btnCameraReset.TabIndex = 3;
            this.btnCameraReset.Text = "Đặt lại Camera";
            this.btnCameraReset.UseVisualStyleBackColor = false;
            this.btnCameraReset.Click += new System.EventHandler(this.btnCameraReset_Click);
            // 
            // tlpTriggerRepeat
            // 
            this.tlpTriggerRepeat.ColumnCount = 3;
            this.tlpTriggerRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tlpTriggerRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tlpTriggerRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTriggerRepeat.Controls.Add(this.lblTriggerRepeat, 0, 0);
            this.tlpTriggerRepeat.Controls.Add(this.numTriggerRepeat, 1, 0);
            this.tlpTriggerRepeat.Controls.Add(this.lblTriggerRepeatUnit, 2, 0);
            this.tlpTriggerRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTriggerRepeat.Location = new System.Drawing.Point(171, 86);
            this.tlpTriggerRepeat.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTriggerRepeat.Name = "tlpTriggerRepeat";
            this.tlpTriggerRepeat.RowCount = 1;
            this.tlpTriggerRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTriggerRepeat.Size = new System.Drawing.Size(171, 56);
            this.tlpTriggerRepeat.TabIndex = 4;
            // 
            // lblTriggerRepeat
            // 
            this.lblTriggerRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTriggerRepeat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblTriggerRepeat.Location = new System.Drawing.Point(0, 0);
            this.lblTriggerRepeat.Margin = new System.Windows.Forms.Padding(0);
            this.lblTriggerRepeat.Name = "lblTriggerRepeat";
            this.lblTriggerRepeat.Size = new System.Drawing.Size(64, 56);
            this.lblTriggerRepeat.TabIndex = 0;
            this.lblTriggerRepeat.Text = "Lặp lại";
            this.lblTriggerRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numTriggerRepeat
            // 
            this.numTriggerRepeat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numTriggerRepeat.Location = new System.Drawing.Point(72, 16);
            this.numTriggerRepeat.Margin = new System.Windows.Forms.Padding(8, 0, 4, 0);
            this.numTriggerRepeat.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numTriggerRepeat.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTriggerRepeat.Name = "numTriggerRepeat";
            this.numTriggerRepeat.Size = new System.Drawing.Size(60, 23);
            this.numTriggerRepeat.TabIndex = 1;
            this.numTriggerRepeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numTriggerRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTriggerRepeat.ValueChanged += new System.EventHandler(this.numTriggerRepeat_ValueChanged);
            // 
            // lblTriggerRepeatUnit
            // 
            this.lblTriggerRepeatUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTriggerRepeatUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblTriggerRepeatUnit.Location = new System.Drawing.Point(136, 0);
            this.lblTriggerRepeatUnit.Margin = new System.Windows.Forms.Padding(0);
            this.lblTriggerRepeatUnit.Name = "lblTriggerRepeatUnit";
            this.lblTriggerRepeatUnit.Size = new System.Drawing.Size(35, 56);
            this.lblTriggerRepeatUnit.TabIndex = 2;
            this.lblTriggerRepeatUnit.Text = "lần";
            this.lblTriggerRepeatUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // scWork
            // 
            this.scWork.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scWork.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scWork.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scWork.Location = new System.Drawing.Point(12, 188);
            this.scWork.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.scWork.Name = "scWork";
            // 
            // scWork.Panel1
            // 
            this.scWork.Panel1.Controls.Add(this.pnlListCard);
            this.scWork.Panel1MinSize = 200;
            // 
            // scWork.Panel2
            // 
            this.scWork.Panel2.Controls.Add(this.pnlPreviewCard);
            this.scWork.Panel2MinSize = 300;
            this.scWork.Size = new System.Drawing.Size(1288, 311);
            this.scWork.SplitterDistance = 400;
            this.scWork.SplitterWidth = 12;
            this.scWork.TabIndex = 1;
            // 
            // pnlListCard
            // 
            this.pnlListCard.BackColor = System.Drawing.Color.White;
            this.pnlListCard.Controls.Add(this.tlpListCard);
            this.pnlListCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListCard.Location = new System.Drawing.Point(0, 0);
            this.pnlListCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlListCard.Name = "pnlListCard";
            this.pnlListCard.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlListCard.HideButton = true;
            this.pnlListCard.Size = new System.Drawing.Size(400, 311);
            this.pnlListCard.TabIndex = 0;
            // 
            // tlpListCard
            // 
            this.tlpListCard.ColumnCount = 2;
            this.tlpListCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpListCard.Controls.Add(this.lblListTitle, 0, 0);
            this.tlpListCard.Controls.Add(this.lblListTotal, 1, 0);
            this.tlpListCard.Controls.Add(this.tlpListTabs, 0, 1);
            this.tlpListCard.Controls.Add(this.pnlListHost, 0, 2);
            this.tlpListCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpListCard.Location = new System.Drawing.Point(18, 10);
            this.tlpListCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpListCard.Name = "tlpListCard";
            this.tlpListCard.RowCount = 3;
            this.tlpListCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpListCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpListCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListCard.Size = new System.Drawing.Size(364, 289);
            this.tlpListCard.TabIndex = 0;
            // 
            // lblListTitle
            // 
            this.lblListTitle.AutoSize = true;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblListTitle.Location = new System.Drawing.Point(0, 4);
            this.lblListTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(129, 19);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "DANH SÁCH ẢNH";
            // 
            // lblListTotal
            // 
            this.lblListTotal.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblListTotal.AutoSize = true;
            this.lblListTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblListTotal.Location = new System.Drawing.Point(328, 7);
            this.lblListTotal.Margin = new System.Windows.Forms.Padding(0);
            this.lblListTotal.Name = "lblListTotal";
            this.lblListTotal.Size = new System.Drawing.Size(36, 15);
            this.lblListTotal.TabIndex = 1;
            this.lblListTotal.Text = "0 ảnh";
            // 
            // tlpListTabs
            // 
            this.tlpListTabs.ColumnCount = 2;
            this.tlpListCard.SetColumnSpan(this.tlpListTabs, 2);
            this.tlpListTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpListTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpListTabs.Controls.Add(this.btnListRaw, 0, 0);
            this.tlpListTabs.Controls.Add(this.btnListMask, 1, 0);
            this.tlpListTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpListTabs.Location = new System.Drawing.Point(0, 34);
            this.tlpListTabs.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.tlpListTabs.Name = "tlpListTabs";
            this.tlpListTabs.RowCount = 1;
            this.tlpListTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListTabs.Size = new System.Drawing.Size(364, 34);
            this.tlpListTabs.TabIndex = 2;
            // 
            // btnListRaw
            // 
            this.btnListRaw.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.btnListRaw.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnListRaw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.btnListRaw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListRaw.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnListRaw.ForeColor = System.Drawing.Color.White;
            this.btnListRaw.Location = new System.Drawing.Point(0, 0);
            this.btnListRaw.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnListRaw.Name = "btnListRaw";
            this.btnListRaw.Size = new System.Drawing.Size(178, 34);
            this.btnListRaw.TabIndex = 0;
            this.btnListRaw.Text = "Raw  0";
            this.btnListRaw.UseVisualStyleBackColor = false;
            this.btnListRaw.Click += new System.EventHandler(this.btnListRaw_Click);
            // 
            // btnListMask
            // 
            this.btnListMask.BackColor = System.Drawing.Color.White;
            this.btnListMask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListMask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnListMask.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnListMask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListMask.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnListMask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnListMask.Location = new System.Drawing.Point(182, 0);
            this.btnListMask.Margin = new System.Windows.Forms.Padding(0);
            this.btnListMask.Name = "btnListMask";
            this.btnListMask.Size = new System.Drawing.Size(182, 34);
            this.btnListMask.TabIndex = 1;
            this.btnListMask.Text = "Mask  0";
            this.btnListMask.UseVisualStyleBackColor = false;
            this.btnListMask.Click += new System.EventHandler(this.btnListMask_Click);
            // 
            // pnlListHost
            // 
            this.tlpListCard.SetColumnSpan(this.pnlListHost, 2);
            this.pnlListHost.Controls.Add(this.pnlRawCard);
            this.pnlListHost.Controls.Add(this.pnlMaskCard);
            this.pnlListHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListHost.Location = new System.Drawing.Point(0, 72);
            this.pnlListHost.Margin = new System.Windows.Forms.Padding(0);
            this.pnlListHost.Name = "pnlListHost";
            this.pnlListHost.Size = new System.Drawing.Size(364, 217);
            this.pnlListHost.TabIndex = 3;
            // 
            // pnlRawCard
            // 
            this.pnlRawCard.BackColor = System.Drawing.Color.White;
            this.pnlRawCard.Controls.Add(this.tlpRaw);
            this.pnlRawCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRawCard.Location = new System.Drawing.Point(0, 0);
            this.pnlRawCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlRawCard.Name = "pnlRawCard";
            this.pnlRawCard.Size = new System.Drawing.Size(364, 217);
            this.pnlRawCard.TabIndex = 0;
            // 
            // tlpRaw
            // 
            this.tlpRaw.ColumnCount = 2;
            this.tlpRaw.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaw.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tlpRaw.Controls.Add(this.lblRawHeader, 0, 0);
            this.tlpRaw.Controls.Add(this.lblRawCount, 1, 0);
            this.tlpRaw.Controls.Add(this.lvRawImage, 0, 1);
            this.tlpRaw.Controls.Add(this.tlpRawPager, 0, 2);
            this.tlpRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaw.Location = new System.Drawing.Point(0, 0);
            this.tlpRaw.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRaw.Name = "tlpRaw";
            this.tlpRaw.RowCount = 3;
            this.tlpRaw.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tlpRaw.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaw.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpRaw.Size = new System.Drawing.Size(364, 217);
            this.tlpRaw.TabIndex = 0;
            // 
            // lblRawHeader
            // 
            this.lblRawHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRawHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblRawHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblRawHeader.Location = new System.Drawing.Point(0, 0);
            this.lblRawHeader.Margin = new System.Windows.Forms.Padding(0);
            this.lblRawHeader.Name = "lblRawHeader";
            this.lblRawHeader.Size = new System.Drawing.Size(294, 1);
            this.lblRawHeader.TabIndex = 0;
            this.lblRawHeader.Text = "Danh sách ảnh Raw";
            this.lblRawHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRawHeader.Visible = false;
            // 
            // lblRawCount
            // 
            this.lblRawCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRawCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblRawCount.Location = new System.Drawing.Point(294, 0);
            this.lblRawCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblRawCount.Name = "lblRawCount";
            this.lblRawCount.Size = new System.Drawing.Size(70, 1);
            this.lblRawCount.TabIndex = 1;
            this.lblRawCount.Text = "(0)";
            this.lblRawCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRawCount.Visible = false;
            // 
            // lvRawImage
            // 
            this.lvRawImage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chRawIndex,
            this.chRawName,
            this.chRawTime});
            this.tlpRaw.SetColumnSpan(this.lvRawImage, 2);
            this.lvRawImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvRawImage.FullRowSelect = true;
            this.lvRawImage.GridLines = true;
            this.lvRawImage.HideSelection = false;
            this.lvRawImage.Location = new System.Drawing.Point(0, 0);
            this.lvRawImage.Margin = new System.Windows.Forms.Padding(0);
            this.lvRawImage.Name = "lvRawImage";
            this.lvRawImage.Size = new System.Drawing.Size(364, 183);
            this.lvRawImage.TabIndex = 2;
            this.lvRawImage.UseCompatibleStateImageBehavior = false;
            this.lvRawImage.View = System.Windows.Forms.View.Details;
            this.lvRawImage.SelectedIndexChanged += new System.EventHandler(this.lvRawImage_SelectedIndexChanged);
            // 
            // chRawIndex
            // 
            this.chRawIndex.Text = "#";
            this.chRawIndex.Width = 46;
            // 
            // chRawName
            // 
            this.chRawName.Text = "Tên file";
            this.chRawName.Width = 200;
            // 
            // chRawTime
            // 
            this.chRawTime.Text = "Thời gian";
            this.chRawTime.Width = 110;
            // 
            // tlpRawPager
            // 
            this.tlpRawPager.ColumnCount = 7;
            this.tlpRaw.SetColumnSpan(this.tlpRawPager, 2);
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpRawPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRawPager.Controls.Add(this.btnRawFirst, 1, 0);
            this.tlpRawPager.Controls.Add(this.btnRawPrev, 2, 0);
            this.tlpRawPager.Controls.Add(this.lblRawPage, 3, 0);
            this.tlpRawPager.Controls.Add(this.btnRawNext, 4, 0);
            this.tlpRawPager.Controls.Add(this.btnRawLast, 5, 0);
            this.tlpRawPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRawPager.Location = new System.Drawing.Point(0, 183);
            this.tlpRawPager.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRawPager.Name = "tlpRawPager";
            this.tlpRawPager.RowCount = 1;
            this.tlpRawPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRawPager.Size = new System.Drawing.Size(364, 34);
            this.tlpRawPager.TabIndex = 3;
            // 
            // btnRawFirst
            // 
            this.btnRawFirst.BackColor = System.Drawing.Color.White;
            this.btnRawFirst.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRawFirst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRawFirst.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnRawFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRawFirst.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnRawFirst.Location = new System.Drawing.Point(84, 2);
            this.btnRawFirst.Margin = new System.Windows.Forms.Padding(2);
            this.btnRawFirst.Name = "btnRawFirst";
            this.btnRawFirst.Size = new System.Drawing.Size(26, 30);
            this.btnRawFirst.TabIndex = 0;
            this.btnRawFirst.Text = "«";
            this.btnRawFirst.UseVisualStyleBackColor = false;
            this.btnRawFirst.Click += new System.EventHandler(this.btnRawFirst_Click);
            // 
            // btnRawPrev
            // 
            this.btnRawPrev.BackColor = System.Drawing.Color.White;
            this.btnRawPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRawPrev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRawPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnRawPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRawPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnRawPrev.Location = new System.Drawing.Point(114, 2);
            this.btnRawPrev.Margin = new System.Windows.Forms.Padding(2);
            this.btnRawPrev.Name = "btnRawPrev";
            this.btnRawPrev.Size = new System.Drawing.Size(26, 30);
            this.btnRawPrev.TabIndex = 1;
            this.btnRawPrev.Text = "‹";
            this.btnRawPrev.UseVisualStyleBackColor = false;
            this.btnRawPrev.Click += new System.EventHandler(this.btnRawPrev_Click);
            // 
            // lblRawPage
            // 
            this.lblRawPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRawPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblRawPage.Location = new System.Drawing.Point(142, 0);
            this.lblRawPage.Margin = new System.Windows.Forms.Padding(0);
            this.lblRawPage.Name = "lblRawPage";
            this.lblRawPage.Size = new System.Drawing.Size(80, 34);
            this.lblRawPage.TabIndex = 2;
            this.lblRawPage.Text = "1 / 1";
            this.lblRawPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRawNext
            // 
            this.btnRawNext.BackColor = System.Drawing.Color.White;
            this.btnRawNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRawNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRawNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnRawNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRawNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnRawNext.Location = new System.Drawing.Point(224, 2);
            this.btnRawNext.Margin = new System.Windows.Forms.Padding(2);
            this.btnRawNext.Name = "btnRawNext";
            this.btnRawNext.Size = new System.Drawing.Size(26, 30);
            this.btnRawNext.TabIndex = 3;
            this.btnRawNext.Text = "›";
            this.btnRawNext.UseVisualStyleBackColor = false;
            this.btnRawNext.Click += new System.EventHandler(this.btnRawNext_Click);
            // 
            // btnRawLast
            // 
            this.btnRawLast.BackColor = System.Drawing.Color.White;
            this.btnRawLast.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRawLast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRawLast.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnRawLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRawLast.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnRawLast.Location = new System.Drawing.Point(254, 2);
            this.btnRawLast.Margin = new System.Windows.Forms.Padding(2);
            this.btnRawLast.Name = "btnRawLast";
            this.btnRawLast.Size = new System.Drawing.Size(26, 30);
            this.btnRawLast.TabIndex = 4;
            this.btnRawLast.Text = "»";
            this.btnRawLast.UseVisualStyleBackColor = false;
            this.btnRawLast.Click += new System.EventHandler(this.btnRawLast_Click);
            // 
            // pnlMaskCard
            // 
            this.pnlMaskCard.BackColor = System.Drawing.Color.White;
            this.pnlMaskCard.Controls.Add(this.tlpMask);
            this.pnlMaskCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMaskCard.Location = new System.Drawing.Point(0, 0);
            this.pnlMaskCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMaskCard.Name = "pnlMaskCard";
            this.pnlMaskCard.Size = new System.Drawing.Size(364, 217);
            this.pnlMaskCard.TabIndex = 1;
            this.pnlMaskCard.Visible = false;
            // 
            // tlpMask
            // 
            this.tlpMask.ColumnCount = 2;
            this.tlpMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tlpMask.Controls.Add(this.lblMaskHeader, 0, 0);
            this.tlpMask.Controls.Add(this.lblMaskCount, 1, 0);
            this.tlpMask.Controls.Add(this.lvMaskImage, 0, 1);
            this.tlpMask.Controls.Add(this.tlpMaskPager, 0, 2);
            this.tlpMask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMask.Location = new System.Drawing.Point(0, 0);
            this.tlpMask.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMask.Name = "tlpMask";
            this.tlpMask.RowCount = 3;
            this.tlpMask.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tlpMask.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMask.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpMask.Size = new System.Drawing.Size(364, 217);
            this.tlpMask.TabIndex = 0;
            // 
            // lblMaskHeader
            // 
            this.lblMaskHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMaskHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblMaskHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblMaskHeader.Location = new System.Drawing.Point(0, 0);
            this.lblMaskHeader.Margin = new System.Windows.Forms.Padding(0);
            this.lblMaskHeader.Name = "lblMaskHeader";
            this.lblMaskHeader.Size = new System.Drawing.Size(294, 1);
            this.lblMaskHeader.TabIndex = 0;
            this.lblMaskHeader.Text = "Danh sách ảnh Mask";
            this.lblMaskHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMaskHeader.Visible = false;
            // 
            // lblMaskCount
            // 
            this.lblMaskCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMaskCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblMaskCount.Location = new System.Drawing.Point(294, 0);
            this.lblMaskCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblMaskCount.Name = "lblMaskCount";
            this.lblMaskCount.Size = new System.Drawing.Size(70, 1);
            this.lblMaskCount.TabIndex = 1;
            this.lblMaskCount.Text = "(0)";
            this.lblMaskCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMaskCount.Visible = false;
            // 
            // lvMaskImage
            // 
            this.lvMaskImage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chMaskIndex,
            this.chMaskName,
            this.chMaskTime});
            this.tlpMask.SetColumnSpan(this.lvMaskImage, 2);
            this.lvMaskImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMaskImage.FullRowSelect = true;
            this.lvMaskImage.GridLines = true;
            this.lvMaskImage.HideSelection = false;
            this.lvMaskImage.Location = new System.Drawing.Point(0, 0);
            this.lvMaskImage.Margin = new System.Windows.Forms.Padding(0);
            this.lvMaskImage.Name = "lvMaskImage";
            this.lvMaskImage.Size = new System.Drawing.Size(364, 183);
            this.lvMaskImage.TabIndex = 2;
            this.lvMaskImage.UseCompatibleStateImageBehavior = false;
            this.lvMaskImage.View = System.Windows.Forms.View.Details;
            this.lvMaskImage.SelectedIndexChanged += new System.EventHandler(this.lvMaskImage_SelectedIndexChanged);
            // 
            // chMaskIndex
            // 
            this.chMaskIndex.Text = "#";
            this.chMaskIndex.Width = 46;
            // 
            // chMaskName
            // 
            this.chMaskName.Text = "Tên file";
            this.chMaskName.Width = 200;
            // 
            // chMaskTime
            // 
            this.chMaskTime.Text = "Thời gian";
            this.chMaskTime.Width = 110;
            // 
            // tlpMaskPager
            // 
            this.tlpMaskPager.ColumnCount = 7;
            this.tlpMask.SetColumnSpan(this.tlpMaskPager, 2);
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpMaskPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaskPager.Controls.Add(this.btnMaskFirst, 1, 0);
            this.tlpMaskPager.Controls.Add(this.btnMaskPrev, 2, 0);
            this.tlpMaskPager.Controls.Add(this.lblMaskPage, 3, 0);
            this.tlpMaskPager.Controls.Add(this.btnMaskNext, 4, 0);
            this.tlpMaskPager.Controls.Add(this.btnMaskLast, 5, 0);
            this.tlpMaskPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMaskPager.Location = new System.Drawing.Point(0, 183);
            this.tlpMaskPager.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMaskPager.Name = "tlpMaskPager";
            this.tlpMaskPager.RowCount = 1;
            this.tlpMaskPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMaskPager.Size = new System.Drawing.Size(364, 34);
            this.tlpMaskPager.TabIndex = 3;
            // 
            // btnMaskFirst
            // 
            this.btnMaskFirst.BackColor = System.Drawing.Color.White;
            this.btnMaskFirst.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaskFirst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMaskFirst.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnMaskFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaskFirst.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnMaskFirst.Location = new System.Drawing.Point(84, 2);
            this.btnMaskFirst.Margin = new System.Windows.Forms.Padding(2);
            this.btnMaskFirst.Name = "btnMaskFirst";
            this.btnMaskFirst.Size = new System.Drawing.Size(26, 30);
            this.btnMaskFirst.TabIndex = 0;
            this.btnMaskFirst.Text = "«";
            this.btnMaskFirst.UseVisualStyleBackColor = false;
            this.btnMaskFirst.Click += new System.EventHandler(this.btnMaskFirst_Click);
            // 
            // btnMaskPrev
            // 
            this.btnMaskPrev.BackColor = System.Drawing.Color.White;
            this.btnMaskPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaskPrev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMaskPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnMaskPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaskPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnMaskPrev.Location = new System.Drawing.Point(114, 2);
            this.btnMaskPrev.Margin = new System.Windows.Forms.Padding(2);
            this.btnMaskPrev.Name = "btnMaskPrev";
            this.btnMaskPrev.Size = new System.Drawing.Size(26, 30);
            this.btnMaskPrev.TabIndex = 1;
            this.btnMaskPrev.Text = "‹";
            this.btnMaskPrev.UseVisualStyleBackColor = false;
            this.btnMaskPrev.Click += new System.EventHandler(this.btnMaskPrev_Click);
            // 
            // lblMaskPage
            // 
            this.lblMaskPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMaskPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblMaskPage.Location = new System.Drawing.Point(142, 0);
            this.lblMaskPage.Margin = new System.Windows.Forms.Padding(0);
            this.lblMaskPage.Name = "lblMaskPage";
            this.lblMaskPage.Size = new System.Drawing.Size(80, 34);
            this.lblMaskPage.TabIndex = 2;
            this.lblMaskPage.Text = "1 / 1";
            this.lblMaskPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMaskNext
            // 
            this.btnMaskNext.BackColor = System.Drawing.Color.White;
            this.btnMaskNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaskNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMaskNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnMaskNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaskNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnMaskNext.Location = new System.Drawing.Point(224, 2);
            this.btnMaskNext.Margin = new System.Windows.Forms.Padding(2);
            this.btnMaskNext.Name = "btnMaskNext";
            this.btnMaskNext.Size = new System.Drawing.Size(26, 30);
            this.btnMaskNext.TabIndex = 3;
            this.btnMaskNext.Text = "›";
            this.btnMaskNext.UseVisualStyleBackColor = false;
            this.btnMaskNext.Click += new System.EventHandler(this.btnMaskNext_Click);
            // 
            // btnMaskLast
            // 
            this.btnMaskLast.BackColor = System.Drawing.Color.White;
            this.btnMaskLast.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaskLast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMaskLast.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnMaskLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaskLast.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnMaskLast.Location = new System.Drawing.Point(254, 2);
            this.btnMaskLast.Margin = new System.Windows.Forms.Padding(2);
            this.btnMaskLast.Name = "btnMaskLast";
            this.btnMaskLast.Size = new System.Drawing.Size(26, 30);
            this.btnMaskLast.TabIndex = 4;
            this.btnMaskLast.Text = "»";
            this.btnMaskLast.UseVisualStyleBackColor = false;
            this.btnMaskLast.Click += new System.EventHandler(this.btnMaskLast_Click);
            // 
            // pnlPreviewCard
            // 
            this.pnlPreviewCard.BackColor = System.Drawing.Color.White;
            this.pnlPreviewCard.Controls.Add(this.tlpPreview);
            this.pnlPreviewCard.Controls.Add(this.lblPreviewHeader);
            this.pnlPreviewCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreviewCard.Location = new System.Drawing.Point(0, 0);
            this.pnlPreviewCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlPreviewCard.Name = "pnlPreviewCard";
            this.pnlPreviewCard.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlPreviewCard.Size = new System.Drawing.Size(876, 311);
            this.pnlPreviewCard.TabIndex = 0;
            // 
            // tlpPreview
            // 
            this.tlpPreview.ColumnCount = 3;
            this.tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 290F));
            this.tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tlpPreview.Controls.Add(this.lblPreviewTitle, 0, 0);
            this.tlpPreview.Controls.Add(this.tlpPreviewToggle, 1, 0);
            this.tlpPreview.Controls.Add(this.lblPreviewKeyHint, 2, 0);
            this.tlpPreview.Controls.Add(this.lblPreviewName, 0, 1);
            this.tlpPreview.Controls.Add(this.lblPreviewMeta, 0, 2);
            this.tlpPreview.Controls.Add(this.pnlPreviewBody, 0, 3);
            this.tlpPreview.Controls.Add(this.tlpPreviewNav, 0, 4);
            this.tlpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreview.Location = new System.Drawing.Point(18, 10);
            this.tlpPreview.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreview.Name = "tlpPreview";
            this.tlpPreview.RowCount = 5;
            this.tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPreview.Size = new System.Drawing.Size(840, 289);
            this.tlpPreview.TabIndex = 0;
            // 
            // lblPreviewTitle
            // 
            this.lblPreviewTitle.AutoSize = true;
            this.lblPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPreviewTitle.Location = new System.Drawing.Point(0, 4);
            this.lblPreviewTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Size = new System.Drawing.Size(126, 19);
            this.lblPreviewTitle.TabIndex = 0;
            this.lblPreviewTitle.Text = "XEM TRƯỚC ẢNH";
            // 
            // tlpPreviewToggle
            // 
            this.tlpPreviewToggle.ColumnCount = 2;
            this.tlpPreviewToggle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPreviewToggle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPreviewToggle.Controls.Add(this.btnViewRaw, 0, 0);
            this.tlpPreviewToggle.Controls.Add(this.btnViewMask, 1, 0);
            this.tlpPreviewToggle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewToggle.Location = new System.Drawing.Point(360, 2);
            this.tlpPreviewToggle.Margin = new System.Windows.Forms.Padding(0, 2, 10, 2);
            this.tlpPreviewToggle.Name = "tlpPreviewToggle";
            this.tlpPreviewToggle.RowCount = 1;
            this.tlpPreviewToggle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewToggle.Size = new System.Drawing.Size(280, 34);
            this.tlpPreviewToggle.TabIndex = 1;
            // 
            // btnViewRaw
            // 
            this.btnViewRaw.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnViewRaw.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnViewRaw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnViewRaw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewRaw.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewRaw.ForeColor = System.Drawing.Color.White;
            this.btnViewRaw.Location = new System.Drawing.Point(2, 2);
            this.btnViewRaw.Margin = new System.Windows.Forms.Padding(2);
            this.btnViewRaw.Name = "btnViewRaw";
            this.btnViewRaw.Size = new System.Drawing.Size(136, 30);
            this.btnViewRaw.TabIndex = 0;
            this.btnViewRaw.Text = "Raw";
            this.btnViewRaw.UseVisualStyleBackColor = false;
            this.btnViewRaw.Click += new System.EventHandler(this.btnViewRaw_Click);
            // 
            // btnViewMask
            // 
            this.btnViewMask.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.btnViewMask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewMask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnViewMask.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(231)))), ((int)(((byte)(235)))));
            this.btnViewMask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewMask.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewMask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnViewMask.Location = new System.Drawing.Point(142, 2);
            this.btnViewMask.Margin = new System.Windows.Forms.Padding(2);
            this.btnViewMask.Name = "btnViewMask";
            this.btnViewMask.Size = new System.Drawing.Size(136, 30);
            this.btnViewMask.TabIndex = 1;
            this.btnViewMask.Text = "Mask";
            this.btnViewMask.UseVisualStyleBackColor = false;
            this.btnViewMask.Click += new System.EventHandler(this.btnViewMask_Click);
            // 
            // lblPreviewKeyHint
            // 
            this.lblPreviewKeyHint.BackColor = System.Drawing.Color.White;
            this.lblPreviewKeyHint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPreviewKeyHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewKeyHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPreviewKeyHint.Location = new System.Drawing.Point(650, 4);
            this.lblPreviewKeyHint.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.lblPreviewKeyHint.Name = "lblPreviewKeyHint";
            this.lblPreviewKeyHint.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblPreviewKeyHint.Size = new System.Drawing.Size(190, 30);
            this.lblPreviewKeyHint.TabIndex = 2;
            this.lblPreviewKeyHint.Text = "Phím Space: đổi Raw / Mask";
            this.lblPreviewKeyHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPreviewName
            // 
            this.lblPreviewName.AutoEllipsis = true;
            this.tlpPreview.SetColumnSpan(this.lblPreviewName, 3);
            this.lblPreviewName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewName.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblPreviewName.Location = new System.Drawing.Point(0, 38);
            this.lblPreviewName.Margin = new System.Windows.Forms.Padding(0);
            this.lblPreviewName.Name = "lblPreviewName";
            this.lblPreviewName.Size = new System.Drawing.Size(840, 24);
            this.lblPreviewName.TabIndex = 3;
            this.lblPreviewName.Text = "Chưa chọn ảnh";
            this.lblPreviewName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPreviewMeta
            // 
            this.lblPreviewMeta.AutoEllipsis = true;
            this.tlpPreview.SetColumnSpan(this.lblPreviewMeta, 3);
            this.lblPreviewMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewMeta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPreviewMeta.Location = new System.Drawing.Point(0, 62);
            this.lblPreviewMeta.Margin = new System.Windows.Forms.Padding(0);
            this.lblPreviewMeta.Name = "lblPreviewMeta";
            this.lblPreviewMeta.Size = new System.Drawing.Size(840, 22);
            this.lblPreviewMeta.TabIndex = 4;
            this.lblPreviewMeta.Text = "Chọn ảnh trong danh sách để xem trước";
            this.lblPreviewMeta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPreviewBody
            // 
            this.pnlPreviewBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.tlpPreview.SetColumnSpan(this.pnlPreviewBody, 3);
            this.pnlPreviewBody.Controls.Add(this.lblResult);
            this.pnlPreviewBody.Controls.Add(this.lblPreviewHint);
            this.pnlPreviewBody.Controls.Add(this.picPreview);
            this.pnlPreviewBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreviewBody.Location = new System.Drawing.Point(0, 90);
            this.pnlPreviewBody.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.pnlPreviewBody.Name = "pnlPreviewBody";
            this.pnlPreviewBody.Size = new System.Drawing.Size(840, 153);
            this.pnlPreviewBody.TabIndex = 5;
            // 
            // lblResult
            // 
            this.lblResult.BackColor = System.Drawing.Color.White;
            this.lblResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblResult.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblResult.Location = new System.Drawing.Point(10, 44);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(110, 40);
            this.lblResult.TabIndex = 0;
            this.lblResult.Text = "OK / NG";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPreviewHint
            // 
            this.lblPreviewHint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lblPreviewHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPreviewHint.Location = new System.Drawing.Point(0, 0);
            this.lblPreviewHint.Name = "lblPreviewHint";
            this.lblPreviewHint.Size = new System.Drawing.Size(840, 153);
            this.lblPreviewHint.TabIndex = 1;
            this.lblPreviewHint.Text = "Chọn ảnh từ danh sách để xem trước\r\n(Nhấn Space để chuyển đổi Raw / Mask)";
            this.lblPreviewHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.picPreview.Cursor = System.Windows.Forms.Cursors.Default;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(840, 153);
            this.picPreview.TabIndex = 2;
            this.picPreview.TabStop = false;
            // 
            // tlpPreviewNav
            // 
            this.tlpPreviewNav.ColumnCount = 3;
            this.tlpPreview.SetColumnSpan(this.tlpPreviewNav, 3);
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpPreviewNav.Controls.Add(this.btnPrevImage, 0, 0);
            this.tlpPreviewNav.Controls.Add(this.lblImageNav, 1, 0);
            this.tlpPreviewNav.Controls.Add(this.btnNextImage, 2, 0);
            this.tlpPreviewNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewNav.Location = new System.Drawing.Point(0, 249);
            this.tlpPreviewNav.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreviewNav.Name = "tlpPreviewNav";
            this.tlpPreviewNav.RowCount = 1;
            this.tlpPreviewNav.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewNav.Size = new System.Drawing.Size(840, 40);
            this.tlpPreviewNav.TabIndex = 6;
            // 
            // btnPrevImage
            // 
            this.btnPrevImage.BackColor = System.Drawing.Color.White;
            this.btnPrevImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevImage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnPrevImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnPrevImage.Location = new System.Drawing.Point(0, 4);
            this.btnPrevImage.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.btnPrevImage.Name = "btnPrevImage";
            this.btnPrevImage.Size = new System.Drawing.Size(104, 36);
            this.btnPrevImage.TabIndex = 0;
            this.btnPrevImage.Text = "←  Trước";
            this.btnPrevImage.UseVisualStyleBackColor = false;
            this.btnPrevImage.Click += new System.EventHandler(this.btnPrevImage_Click);
            // 
            // lblImageNav
            // 
            this.lblImageNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImageNav.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblImageNav.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblImageNav.Location = new System.Drawing.Point(104, 0);
            this.lblImageNav.Margin = new System.Windows.Forms.Padding(0);
            this.lblImageNav.Name = "lblImageNav";
            this.lblImageNav.Size = new System.Drawing.Size(632, 40);
            this.lblImageNav.TabIndex = 1;
            this.lblImageNav.Text = "Ảnh 0 / 0";
            this.lblImageNav.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNextImage
            // 
            this.btnNextImage.BackColor = System.Drawing.Color.White;
            this.btnNextImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextImage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnNextImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnNextImage.Location = new System.Drawing.Point(736, 4);
            this.btnNextImage.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.btnNextImage.Name = "btnNextImage";
            this.btnNextImage.Size = new System.Drawing.Size(104, 36);
            this.btnNextImage.TabIndex = 2;
            this.btnNextImage.Text = "Sau  →";
            this.btnNextImage.UseVisualStyleBackColor = false;
            this.btnNextImage.Click += new System.EventHandler(this.btnNextImage_Click);
            // 
            // lblPreviewHeader
            // 
            this.lblPreviewHeader.Location = new System.Drawing.Point(18, 10);
            this.lblPreviewHeader.Name = "lblPreviewHeader";
            this.lblPreviewHeader.Size = new System.Drawing.Size(100, 15);
            this.lblPreviewHeader.TabIndex = 1;
            this.lblPreviewHeader.Text = "Xem trước ảnh";
            this.lblPreviewHeader.Visible = false;
            this.lblPreviewHeader.TextChanged += new System.EventHandler(this.lblPreviewHeader_TextChanged);
            // 
            // tlpBottomRow
            // 
            this.tlpBottomRow.ColumnCount = 2;
            this.tlpBottomRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66F));
            this.tlpBottomRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tlpBottomRow.Controls.Add(this.pnlPathCard, 0, 0);
            this.tlpBottomRow.Controls.Add(this.pnlSdkCard, 1, 0);
            this.tlpBottomRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBottomRow.Location = new System.Drawing.Point(12, 511);
            this.tlpBottomRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpBottomRow.Name = "tlpBottomRow";
            this.tlpBottomRow.RowCount = 1;
            this.tlpBottomRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottomRow.Size = new System.Drawing.Size(1300, 262);
            this.tlpBottomRow.TabIndex = 2;
            // 
            // pnlPathCard
            // 
            this.pnlPathCard.BackColor = System.Drawing.Color.White;
            this.pnlPathCard.Controls.Add(this.tlpPath);
            this.pnlPathCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPathCard.Location = new System.Drawing.Point(0, 0);
            this.pnlPathCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.pnlPathCard.Name = "pnlPathCard";
            this.pnlPathCard.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlPathCard.HideButton = true;
            this.pnlPathCard.Size = new System.Drawing.Size(846, 250);
            this.pnlPathCard.TabIndex = 0;
            // 
            // tlpPath
            // 
            this.tlpPath.ColumnCount = 7;
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tlpPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 109F));
            this.tlpPath.Controls.Add(this.lblPathCardTitle, 0, 0);
            this.tlpPath.Controls.Add(this.lblPath, 0, 1);
            this.tlpPath.Controls.Add(this.txtPath, 1, 1);
            this.tlpPath.Controls.Add(this.btnBrowse, 6, 1);
            this.tlpPath.Controls.Add(this.lblImageName, 0, 2);
            this.tlpPath.Controls.Add(this.txtImageName, 1, 2);
            this.tlpPath.Controls.Add(this.lblNamePrefix, 2, 2);
            this.tlpPath.Controls.Add(this.cboNamePrefix, 3, 2);
            this.tlpPath.Controls.Add(this.lblImageFormat, 4, 2);
            this.tlpPath.Controls.Add(this.cboImageFormat, 5, 2);
            this.tlpPath.Controls.Add(this.tlpPathOptions, 0, 3);
            this.tlpPath.Controls.Add(this.lblNamePreview, 0, 4);
            this.tlpPath.Controls.Add(this.tlpPathButtons, 0, 5);
            this.tlpPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPath.Location = new System.Drawing.Point(18, 10);
            this.tlpPath.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPath.Name = "tlpPath";
            this.tlpPath.RowCount = 7;
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tlpPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPath.Size = new System.Drawing.Size(810, 228);
            this.tlpPath.TabIndex = 0;
            // 
            // lblPathCardTitle
            // 
            this.lblPathCardTitle.AutoSize = true;
            this.tlpPath.SetColumnSpan(this.lblPathCardTitle, 7);
            this.lblPathCardTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPathCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPathCardTitle.Location = new System.Drawing.Point(0, 4);
            this.lblPathCardTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPathCardTitle.Name = "lblPathCardTitle";
            this.lblPathCardTitle.Size = new System.Drawing.Size(163, 19);
            this.lblPathCardTitle.TabIndex = 0;
            this.lblPathCardTitle.Text = "LƯU TRỮ & PUSH ẢNH";
            this.lblPathCardTitle.UseMnemonic = false;
            // 
            // lblPath
            // 
            this.lblPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPath.Location = new System.Drawing.Point(0, 30);
            this.lblPath.Margin = new System.Windows.Forms.Padding(0);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(110, 40);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "Thư mục lưu";
            this.lblPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPath
            // 
            this.tlpPath.SetColumnSpan(this.txtPath, 5);
            this.txtPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPath.Location = new System.Drawing.Point(110, 38);
            this.txtPath.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(582, 23);
            this.txtPath.TabIndex = 2;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.White;
            this.btnBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnBrowse.Location = new System.Drawing.Point(700, 34);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(110, 32);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Duyệt…";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblImageName
            // 
            this.lblImageName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImageName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblImageName.Location = new System.Drawing.Point(0, 70);
            this.lblImageName.Margin = new System.Windows.Forms.Padding(0);
            this.lblImageName.Name = "lblImageName";
            this.lblImageName.Size = new System.Drawing.Size(110, 40);
            this.lblImageName.TabIndex = 4;
            this.lblImageName.Text = "Mẫu tên";
            this.lblImageName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtImageName
            // 
            this.txtImageName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtImageName.Location = new System.Drawing.Point(110, 78);
            this.txtImageName.Margin = new System.Windows.Forms.Padding(0, 8, 14, 0);
            this.txtImageName.Name = "txtImageName";
            this.txtImageName.Size = new System.Drawing.Size(208, 23);
            this.txtImageName.TabIndex = 5;
            // 
            // lblNamePrefix
            // 
            this.lblNamePrefix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNamePrefix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblNamePrefix.Location = new System.Drawing.Point(332, 70);
            this.lblNamePrefix.Margin = new System.Windows.Forms.Padding(0);
            this.lblNamePrefix.Name = "lblNamePrefix";
            this.lblNamePrefix.Size = new System.Drawing.Size(70, 40);
            this.lblNamePrefix.TabIndex = 6;
            this.lblNamePrefix.Text = "Hậu tố";
            this.lblNamePrefix.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboNamePrefix
            // 
            this.cboNamePrefix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboNamePrefix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNamePrefix.FormattingEnabled = true;
            this.cboNamePrefix.Location = new System.Drawing.Point(402, 77);
            this.cboNamePrefix.Margin = new System.Windows.Forms.Padding(0, 7, 14, 0);
            this.cboNamePrefix.Name = "cboNamePrefix";
            this.cboNamePrefix.Size = new System.Drawing.Size(108, 23);
            this.cboNamePrefix.TabIndex = 7;
            // 
            // lblImageFormat
            // 
            this.lblImageFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImageFormat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblImageFormat.Location = new System.Drawing.Point(524, 70);
            this.lblImageFormat.Margin = new System.Windows.Forms.Padding(0);
            this.lblImageFormat.Name = "lblImageFormat";
            this.lblImageFormat.Size = new System.Drawing.Size(76, 40);
            this.lblImageFormat.TabIndex = 8;
            this.lblImageFormat.Text = "Định dạng";
            this.lblImageFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboImageFormat
            // 
            this.tlpPath.SetColumnSpan(this.cboImageFormat, 2);
            this.cboImageFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageFormat.FormattingEnabled = true;
            this.cboImageFormat.Location = new System.Drawing.Point(600, 77);
            this.cboImageFormat.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.cboImageFormat.Name = "cboImageFormat";
            this.cboImageFormat.Size = new System.Drawing.Size(210, 23);
            this.cboImageFormat.TabIndex = 9;
            // 
            // tlpPathOptions
            // 
            this.tlpPathOptions.ColumnCount = 7;
            this.tlpPath.SetColumnSpan(this.tlpPathOptions, 7);
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPathOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathOptions.Controls.Add(this.chkSaveRaw, 0, 0);
            this.tlpPathOptions.Controls.Add(this.chkSaveCallback, 1, 0);
            this.tlpPathOptions.Controls.Add(this.lblJpegQuality, 2, 0);
            this.tlpPathOptions.Controls.Add(this.numJpegQuality, 3, 0);
            this.tlpPathOptions.Controls.Add(this.lblJpegQualityHint, 4, 0);
            this.tlpPathOptions.Controls.Add(this.chkWatchFolder, 5, 0);
            this.tlpPathOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPathOptions.Location = new System.Drawing.Point(0, 110);
            this.tlpPathOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPathOptions.Name = "tlpPathOptions";
            this.tlpPathOptions.RowCount = 1;
            this.tlpPathOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathOptions.Size = new System.Drawing.Size(810, 34);
            this.tlpPathOptions.TabIndex = 10;
            // 
            // chkSaveRaw
            // 
            this.chkSaveRaw.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSaveRaw.AutoSize = true;
            this.chkSaveRaw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.chkSaveRaw.Location = new System.Drawing.Point(0, 7);
            this.chkSaveRaw.Margin = new System.Windows.Forms.Padding(0, 0, 26, 0);
            this.chkSaveRaw.Name = "chkSaveRaw";
            this.chkSaveRaw.Size = new System.Drawing.Size(71, 19);
            this.chkSaveRaw.TabIndex = 0;
            this.chkSaveRaw.Text = "Lưu Raw";
            this.chkSaveRaw.UseVisualStyleBackColor = true;
            // 
            // chkSaveCallback
            // 
            this.chkSaveCallback.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSaveCallback.AutoSize = true;
            this.chkSaveCallback.Checked = true;
            this.chkSaveCallback.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSaveCallback.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.chkSaveCallback.Location = new System.Drawing.Point(97, 7);
            this.chkSaveCallback.Margin = new System.Windows.Forms.Padding(0, 0, 26, 0);
            this.chkSaveCallback.Name = "chkSaveCallback";
            this.chkSaveCallback.Size = new System.Drawing.Size(77, 19);
            this.chkSaveCallback.TabIndex = 1;
            this.chkSaveCallback.Text = "Lưu Mask";
            this.chkSaveCallback.UseVisualStyleBackColor = true;
            // 
            // lblJpegQuality
            // 
            this.lblJpegQuality.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblJpegQuality.AutoSize = true;
            this.lblJpegQuality.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblJpegQuality.Location = new System.Drawing.Point(200, 9);
            this.lblJpegQuality.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblJpegQuality.Name = "lblJpegQuality";
            this.lblJpegQuality.Size = new System.Drawing.Size(88, 15);
            this.lblJpegQuality.TabIndex = 2;
            this.lblJpegQuality.Text = "Chất lượng JPG";
            // 
            // numJpegQuality
            // 
            this.numJpegQuality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numJpegQuality.Location = new System.Drawing.Point(296, 5);
            this.numJpegQuality.Margin = new System.Windows.Forms.Padding(0);
            this.numJpegQuality.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numJpegQuality.Name = "numJpegQuality";
            this.numJpegQuality.Size = new System.Drawing.Size(70, 23);
            this.numJpegQuality.TabIndex = 3;
            this.numJpegQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numJpegQuality.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            this.numJpegQuality.ValueChanged += new System.EventHandler(this.numJpegQuality_ValueChanged);
            // 
            // lblJpegQualityHint
            // 
            this.lblJpegQualityHint.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblJpegQualityHint.AutoSize = true;
            this.lblJpegQualityHint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblJpegQualityHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblJpegQualityHint.Location = new System.Drawing.Point(372, 10);
            this.lblJpegQualityHint.Margin = new System.Windows.Forms.Padding(6, 0, 26, 0);
            this.lblJpegQualityHint.Name = "lblJpegQualityHint";
            this.lblJpegQualityHint.Size = new System.Drawing.Size(98, 13);
            this.lblJpegQualityHint.TabIndex = 4;
            this.lblJpegQualityHint.Text = "(100% = nét nhất)";
            // 
            // chkWatchFolder
            // 
            this.chkWatchFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkWatchFolder.AutoSize = true;
            this.chkWatchFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.chkWatchFolder.Location = new System.Drawing.Point(496, 7);
            this.chkWatchFolder.Margin = new System.Windows.Forms.Padding(0);
            this.chkWatchFolder.Name = "chkWatchFolder";
            this.chkWatchFolder.Size = new System.Drawing.Size(101, 19);
            this.chkWatchFolder.TabIndex = 5;
            this.chkWatchFolder.Text = "Tự push UnitX";
            this.chkWatchFolder.UseVisualStyleBackColor = true;
            // 
            // lblNamePreview
            // 
            this.lblNamePreview.AutoEllipsis = true;
            this.tlpPath.SetColumnSpan(this.lblNamePreview, 7);
            this.lblNamePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNamePreview.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblNamePreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblNamePreview.Location = new System.Drawing.Point(0, 144);
            this.lblNamePreview.Margin = new System.Windows.Forms.Padding(0);
            this.lblNamePreview.Name = "lblNamePreview";
            this.lblNamePreview.Size = new System.Drawing.Size(810, 22);
            this.lblNamePreview.TabIndex = 11;
            this.lblNamePreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpPathButtons
            // 
            this.tlpPathButtons.ColumnCount = 5;
            this.tlpPath.SetColumnSpan(this.tlpPathButtons, 7);
            this.tlpPathButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpPathButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpPathButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tlpPathButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpPathButtons.Controls.Add(this.btnScan, 0, 0);
            this.tlpPathButtons.Controls.Add(this.btnPushAll, 1, 0);
            this.tlpPathButtons.Controls.Add(this.btnPushSelected, 2, 0);
            this.tlpPathButtons.Controls.Add(this.btnClearPushHistory, 4, 0);
            this.tlpPathButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPathButtons.Location = new System.Drawing.Point(0, 166);
            this.tlpPathButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPathButtons.Name = "tlpPathButtons";
            this.tlpPathButtons.RowCount = 1;
            this.tlpPathButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathButtons.Size = new System.Drawing.Size(810, 48);
            this.tlpPathButtons.TabIndex = 12;
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.Color.White;
            this.btnScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnScan.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnScan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnScan.Location = new System.Drawing.Point(0, 8);
            this.btnScan.Margin = new System.Windows.Forms.Padding(0, 8, 10, 2);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(140, 38);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Nạp đường dẫn";
            this.btnScan.UseVisualStyleBackColor = false;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnPushAll
            // 
            this.btnPushAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnPushAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPushAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPushAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnPushAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPushAll.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnPushAll.ForeColor = System.Drawing.Color.White;
            this.btnPushAll.Location = new System.Drawing.Point(150, 8);
            this.btnPushAll.Margin = new System.Windows.Forms.Padding(0, 8, 10, 2);
            this.btnPushAll.Name = "btnPushAll";
            this.btnPushAll.Size = new System.Drawing.Size(140, 38);
            this.btnPushAll.TabIndex = 1;
            this.btnPushAll.Text = "Push tất cả";
            this.btnPushAll.UseVisualStyleBackColor = false;
            this.btnPushAll.Click += new System.EventHandler(this.btnPushAll_Click);
            // 
            // btnPushSelected
            // 
            this.btnPushSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.btnPushSelected.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPushSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPushSelected.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.btnPushSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPushSelected.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnPushSelected.ForeColor = System.Drawing.Color.White;
            this.btnPushSelected.Location = new System.Drawing.Point(300, 8);
            this.btnPushSelected.Margin = new System.Windows.Forms.Padding(0, 8, 10, 2);
            this.btnPushSelected.Name = "btnPushSelected";
            this.btnPushSelected.Size = new System.Drawing.Size(160, 38);
            this.btnPushSelected.TabIndex = 2;
            this.btnPushSelected.Text = "Push đã chọn";
            this.btnPushSelected.UseVisualStyleBackColor = false;
            this.btnPushSelected.Click += new System.EventHandler(this.btnPushSelected_Click);
            // 
            // btnClearPushHistory
            // 
            this.btnClearPushHistory.BackColor = System.Drawing.Color.White;
            this.btnClearPushHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearPushHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClearPushHistory.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(164)))), ((int)(((byte)(175)))));
            this.btnClearPushHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearPushHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(18)))), ((int)(((byte)(60)))));
            this.btnClearPushHistory.Location = new System.Drawing.Point(690, 8);
            this.btnClearPushHistory.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            this.btnClearPushHistory.Name = "btnClearPushHistory";
            this.btnClearPushHistory.Size = new System.Drawing.Size(120, 38);
            this.btnClearPushHistory.TabIndex = 3;
            this.btnClearPushHistory.Text = "Xoá lịch sử";
            this.btnClearPushHistory.UseVisualStyleBackColor = false;
            this.btnClearPushHistory.Click += new System.EventHandler(this.btnClearPushHistory_Click);
            // 
            // pnlSdkCard
            // 
            this.pnlSdkCard.BackColor = System.Drawing.Color.White;
            this.pnlSdkCard.Controls.Add(this.tlpSdk);
            this.pnlSdkCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSdkCard.Location = new System.Drawing.Point(858, 0);
            this.pnlSdkCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.pnlSdkCard.Name = "pnlSdkCard";
            this.pnlSdkCard.Padding = new System.Windows.Forms.Padding(18, 10, 18, 12);
            this.pnlSdkCard.HideButton = true;
            this.pnlSdkCard.Size = new System.Drawing.Size(430, 250);
            this.pnlSdkCard.TabIndex = 1;
            // 
            // tlpSdk
            // 
            this.tlpSdk.ColumnCount = 4;
            this.tlpSdk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tlpSdk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSdk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpSdk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpSdk.Controls.Add(this.lblSdkCardTitle, 0, 0);
            this.tlpSdk.Controls.Add(this.lblSdkHost, 0, 1);
            this.tlpSdk.Controls.Add(this.txtSdkHost, 1, 1);
            this.tlpSdk.Controls.Add(this.btnSdkTest, 2, 1);
            this.tlpSdk.Controls.Add(this.btnSdkSave, 3, 1);
            this.tlpSdk.Controls.Add(this.lblSdkDivider, 0, 2);
            this.tlpSdk.Controls.Add(this.lblConnTitle, 0, 3);
            this.tlpSdk.Controls.Add(this.pnlConnTiles, 0, 4);
            this.tlpSdk.Controls.Add(this.tlpPortsHead, 0, 5);
            this.tlpSdk.Controls.Add(this.tlpPorts, 0, 6);
            this.tlpSdk.Controls.Add(this.lblPortsSummary, 0, 7);
            this.tlpSdk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSdk.Location = new System.Drawing.Point(18, 10);
            this.tlpSdk.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSdk.Name = "tlpSdk";
            this.tlpSdk.RowCount = 8;
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tlpSdk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSdk.Size = new System.Drawing.Size(394, 228);
            this.tlpSdk.TabIndex = 0;
            // 
            // lblSdkCardTitle
            // 
            this.lblSdkCardTitle.AutoSize = true;
            this.tlpSdk.SetColumnSpan(this.lblSdkCardTitle, 4);
            this.lblSdkCardTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSdkCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSdkCardTitle.Location = new System.Drawing.Point(0, 4);
            this.lblSdkCardTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblSdkCardTitle.Name = "lblSdkCardTitle";
            this.lblSdkCardTitle.Size = new System.Drawing.Size(128, 19);
            this.lblSdkCardTitle.TabIndex = 0;
            this.lblSdkCardTitle.Text = "MẠNG & THIẾT BỊ";
            this.lblSdkCardTitle.UseMnemonic = false;
            // 
            // lblSdkHost
            // 
            this.lblSdkHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkHost.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSdkHost.Location = new System.Drawing.Point(0, 30);
            this.lblSdkHost.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkHost.Name = "lblSdkHost";
            this.lblSdkHost.Size = new System.Drawing.Size(86, 40);
            this.lblSdkHost.TabIndex = 1;
            this.lblSdkHost.Text = "Địa chỉ IP";
            this.lblSdkHost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSdkHost
            // 
            this.txtSdkHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSdkHost.Location = new System.Drawing.Point(86, 38);
            this.txtSdkHost.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);
            this.txtSdkHost.Name = "txtSdkHost";
            this.txtSdkHost.Size = new System.Drawing.Size(106, 23);
            this.txtSdkHost.TabIndex = 2;
            // 
            // btnSdkTest
            // 
            this.btnSdkTest.BackColor = System.Drawing.Color.White;
            this.btnSdkTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSdkTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSdkTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnSdkTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSdkTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnSdkTest.Location = new System.Drawing.Point(200, 34);
            this.btnSdkTest.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.btnSdkTest.Name = "btnSdkTest";
            this.btnSdkTest.Size = new System.Drawing.Size(82, 32);
            this.btnSdkTest.TabIndex = 3;
            this.btnSdkTest.Text = "Kiểm tra";
            this.btnSdkTest.UseVisualStyleBackColor = false;
            this.btnSdkTest.Click += new System.EventHandler(this.btnSdkTest_Click);
            // 
            // btnSdkSave
            // 
            this.btnSdkSave.BackColor = System.Drawing.Color.White;
            this.btnSdkSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSdkSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSdkSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnSdkSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSdkSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnSdkSave.Location = new System.Drawing.Point(290, 34);
            this.btnSdkSave.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnSdkSave.Name = "btnSdkSave";
            this.btnSdkSave.Size = new System.Drawing.Size(104, 32);
            this.btnSdkSave.TabIndex = 4;
            this.btnSdkSave.Text = "Lưu cấu hình";
            this.btnSdkSave.UseVisualStyleBackColor = false;
            this.btnSdkSave.Click += new System.EventHandler(this.btnSdkSave_Click);
            // 
            // lblSdkDivider
            // 
            this.lblSdkDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.tlpSdk.SetColumnSpan(this.lblSdkDivider, 4);
            this.lblSdkDivider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkDivider.Location = new System.Drawing.Point(0, 74);
            this.lblSdkDivider.Margin = new System.Windows.Forms.Padding(0, 4, 0, 5);
            this.lblSdkDivider.Name = "lblSdkDivider";
            this.lblSdkDivider.Size = new System.Drawing.Size(394, 1);
            this.lblSdkDivider.TabIndex = 5;
            // 
            // lblConnTitle
            // 
            this.tlpSdk.SetColumnSpan(this.lblConnTitle, 4);
            this.lblConnTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblConnTitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblConnTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblConnTitle.Location = new System.Drawing.Point(0, 80);
            this.lblConnTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblConnTitle.Name = "lblConnTitle";
            this.lblConnTitle.Size = new System.Drawing.Size(394, 22);
            this.lblConnTitle.TabIndex = 6;
            this.lblConnTitle.Text = "TRẠNG THÁI KẾT NỐI";
            this.lblConnTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlConnTiles
            // 
            this.tlpSdk.SetColumnSpan(this.pnlConnTiles, 4);
            this.pnlConnTiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlConnTiles.Location = new System.Drawing.Point(0, 104);
            this.pnlConnTiles.Margin = new System.Windows.Forms.Padding(0, 2, 0, 4);
            this.pnlConnTiles.Name = "pnlConnTiles";
            this.pnlConnTiles.Size = new System.Drawing.Size(394, 50);
            this.pnlConnTiles.TabIndex = 7;
            this.pnlConnTiles.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlConnTiles_Paint);
            // 
            // tlpPortsHead
            // 
            this.tlpPortsHead.ColumnCount = 2;
            this.tlpSdk.SetColumnSpan(this.tlpPortsHead, 4);
            this.tlpPortsHead.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPortsHead.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPortsHead.Controls.Add(this.lblPortsTitle, 0, 0);
            this.tlpPortsHead.Controls.Add(this.lnkPorts, 1, 0);
            this.tlpPortsHead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPortsHead.Location = new System.Drawing.Point(0, 158);
            this.tlpPortsHead.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPortsHead.Name = "tlpPortsHead";
            this.tlpPortsHead.RowCount = 1;
            this.tlpPortsHead.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPortsHead.Size = new System.Drawing.Size(394, 26);
            this.tlpPortsHead.TabIndex = 8;
            // 
            // lblPortsTitle
            // 
            this.lblPortsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPortsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPortsTitle.Location = new System.Drawing.Point(0, 0);
            this.lblPortsTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblPortsTitle.Name = "lblPortsTitle";
            this.lblPortsTitle.Size = new System.Drawing.Size(356, 26);
            this.lblPortsTitle.TabIndex = 0;
            this.lblPortsTitle.Text = "Cấu hình cổng nâng cao";
            this.lblPortsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lnkPorts
            // 
            this.lnkPorts.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(91)))), ((int)(((byte)(181)))));
            this.lnkPorts.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lnkPorts.AutoSize = true;
            this.lnkPorts.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkPorts.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.lnkPorts.Location = new System.Drawing.Point(356, 5);
            this.lnkPorts.Margin = new System.Windows.Forms.Padding(0);
            this.lnkPorts.Name = "lnkPorts";
            this.lnkPorts.Size = new System.Drawing.Size(38, 15);
            this.lnkPorts.TabIndex = 1;
            this.lnkPorts.TabStop = true;
            this.lnkPorts.Text = "Mở ▾";
            this.lnkPorts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkPorts_LinkClicked);
            // 
            // tlpPorts
            // 
            this.tlpPorts.ColumnCount = 4;
            this.tlpSdk.SetColumnSpan(this.tlpPorts, 4);
            this.tlpPorts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpPorts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPorts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpPorts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPorts.Controls.Add(this.lblSdkGrpc, 0, 0);
            this.tlpPorts.Controls.Add(this.numSdkGrpc, 1, 0);
            this.tlpPorts.Controls.Add(this.lblSdkZmq, 2, 0);
            this.tlpPorts.Controls.Add(this.numSdkZmq, 3, 0);
            this.tlpPorts.Controls.Add(this.lblSdkBlob, 0, 1);
            this.tlpPorts.Controls.Add(this.numSdkBlob, 1, 1);
            this.tlpPorts.Controls.Add(this.lblSdkClient, 2, 1);
            this.tlpPorts.Controls.Add(this.numSdkClient, 3, 1);
            this.tlpPorts.Controls.Add(this.lblSdkStatus, 0, 2);
            this.tlpPorts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPorts.Location = new System.Drawing.Point(0, 184);
            this.tlpPorts.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPorts.Name = "tlpPorts";
            this.tlpPorts.RowCount = 3;
            this.tlpPorts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpPorts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpPorts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpPorts.Size = new System.Drawing.Size(394, 1);
            this.tlpPorts.TabIndex = 9;
            this.tlpPorts.Visible = false;
            // 
            // lblSdkGrpc
            // 
            this.lblSdkGrpc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkGrpc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSdkGrpc.Location = new System.Drawing.Point(0, 0);
            this.lblSdkGrpc.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkGrpc.Name = "lblSdkGrpc";
            this.lblSdkGrpc.Size = new System.Drawing.Size(50, 32);
            this.lblSdkGrpc.TabIndex = 0;
            this.lblSdkGrpc.Text = "gRPC";
            this.lblSdkGrpc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numSdkGrpc
            // 
            this.numSdkGrpc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numSdkGrpc.Location = new System.Drawing.Point(50, 5);
            this.numSdkGrpc.Margin = new System.Windows.Forms.Padding(0, 5, 10, 0);
            this.numSdkGrpc.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSdkGrpc.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSdkGrpc.Name = "numSdkGrpc";
            this.numSdkGrpc.Size = new System.Drawing.Size(137, 23);
            this.numSdkGrpc.TabIndex = 1;
            this.numSdkGrpc.Value = new decimal(new int[] {
            20049,
            0,
            0,
            0});
            this.numSdkGrpc.ValueChanged += new System.EventHandler(this.numSdkPort_ValueChanged);
            // 
            // lblSdkZmq
            // 
            this.lblSdkZmq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkZmq.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSdkZmq.Location = new System.Drawing.Point(197, 0);
            this.lblSdkZmq.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkZmq.Name = "lblSdkZmq";
            this.lblSdkZmq.Size = new System.Drawing.Size(50, 32);
            this.lblSdkZmq.TabIndex = 2;
            this.lblSdkZmq.Text = "ZMQ";
            this.lblSdkZmq.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numSdkZmq
            // 
            this.numSdkZmq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numSdkZmq.Location = new System.Drawing.Point(247, 5);
            this.numSdkZmq.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.numSdkZmq.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSdkZmq.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSdkZmq.Name = "numSdkZmq";
            this.numSdkZmq.Size = new System.Drawing.Size(147, 23);
            this.numSdkZmq.TabIndex = 3;
            this.numSdkZmq.Value = new decimal(new int[] {
            20046,
            0,
            0,
            0});
            this.numSdkZmq.ValueChanged += new System.EventHandler(this.numSdkPort_ValueChanged);
            // 
            // lblSdkBlob
            // 
            this.lblSdkBlob.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkBlob.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSdkBlob.Location = new System.Drawing.Point(0, 32);
            this.lblSdkBlob.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkBlob.Name = "lblSdkBlob";
            this.lblSdkBlob.Size = new System.Drawing.Size(50, 32);
            this.lblSdkBlob.TabIndex = 4;
            this.lblSdkBlob.Text = "Blob";
            this.lblSdkBlob.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numSdkBlob
            // 
            this.numSdkBlob.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numSdkBlob.Location = new System.Drawing.Point(50, 37);
            this.numSdkBlob.Margin = new System.Windows.Forms.Padding(0, 5, 10, 0);
            this.numSdkBlob.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSdkBlob.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSdkBlob.Name = "numSdkBlob";
            this.numSdkBlob.Size = new System.Drawing.Size(137, 23);
            this.numSdkBlob.TabIndex = 5;
            this.numSdkBlob.Value = new decimal(new int[] {
            20050,
            0,
            0,
            0});
            this.numSdkBlob.ValueChanged += new System.EventHandler(this.numSdkPort_ValueChanged);
            // 
            // lblSdkClient
            // 
            this.lblSdkClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkClient.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSdkClient.Location = new System.Drawing.Point(197, 32);
            this.lblSdkClient.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkClient.Name = "lblSdkClient";
            this.lblSdkClient.Size = new System.Drawing.Size(50, 32);
            this.lblSdkClient.TabIndex = 6;
            this.lblSdkClient.Text = "Client";
            this.lblSdkClient.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numSdkClient
            // 
            this.numSdkClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numSdkClient.Location = new System.Drawing.Point(247, 37);
            this.numSdkClient.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.numSdkClient.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSdkClient.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSdkClient.Name = "numSdkClient";
            this.numSdkClient.Size = new System.Drawing.Size(147, 23);
            this.numSdkClient.TabIndex = 7;
            this.numSdkClient.Value = new decimal(new int[] {
            20051,
            0,
            0,
            0});
            this.numSdkClient.ValueChanged += new System.EventHandler(this.numSdkPort_ValueChanged);
            // 
            // lblSdkStatus
            // 
            this.lblSdkStatus.AutoEllipsis = true;
            this.tlpPorts.SetColumnSpan(this.lblSdkStatus, 4);
            this.lblSdkStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSdkStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblSdkStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblSdkStatus.Location = new System.Drawing.Point(0, 64);
            this.lblSdkStatus.Margin = new System.Windows.Forms.Padding(0);
            this.lblSdkStatus.Name = "lblSdkStatus";
            this.lblSdkStatus.Size = new System.Drawing.Size(394, 24);
            this.lblSdkStatus.TabIndex = 8;
            this.lblSdkStatus.Text = "Chưa kiểm tra";
            this.lblSdkStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPortsSummary
            // 
            this.lblPortsSummary.AutoEllipsis = true;
            this.tlpSdk.SetColumnSpan(this.lblPortsSummary, 4);
            this.lblPortsSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPortsSummary.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblPortsSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPortsSummary.Location = new System.Drawing.Point(0, 184);
            this.lblPortsSummary.Margin = new System.Windows.Forms.Padding(0);
            this.lblPortsSummary.Name = "lblPortsSummary";
            this.lblPortsSummary.Size = new System.Drawing.Size(394, 44);
            this.lblPortsSummary.TabIndex = 10;
            this.lblPortsSummary.Text = "gRPC 20049  ·  ZMQ 20046  ·  Blob 20050  ·  Client 20051";
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.White;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslUnitXTitle,
            this.tsslUnitX,
            this.tsslCameraTitle,
            this.tsslCamera,
            this.tsslSdkTitle,
            this.tsslSdk,
            this.tsslStatus,
            this.tsSep1,
            this.tsddSections,
            this.tsbCheckStatus,
            this.tsbShowLog,
            this.tsbClearLog,
            this.tsSep2,
            this.tspbPush,
            this.tsslOverall,
            this.tsslClock});
            this.statusStrip.Location = new System.Drawing.Point(0, 840);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(1320, 24);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 2;
            // 
            // tsslUnitXTitle
            // 
            this.tsslUnitXTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslUnitXTitle.Margin = new System.Windows.Forms.Padding(4, 3, 0, 2);
            this.tsslUnitXTitle.Name = "tsslUnitXTitle";
            this.tsslUnitXTitle.Size = new System.Drawing.Size(39, 19);
            this.tsslUnitXTitle.Text = "UnitX:";
            // 
            // tsslUnitX
            // 
            this.tsslUnitX.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.tsslUnitX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslUnitX.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.tsslUnitX.Name = "tsslUnitX";
            this.tsslUnitX.Size = new System.Drawing.Size(73, 19);
            this.tsslUnitX.Text = "Chưa kết nối";
            this.tsslUnitX.TextChanged += new System.EventHandler(this.ConnectionStatus_Changed);
            // 
            // tsslCameraTitle
            // 
            this.tsslCameraTitle.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslCameraTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslCameraTitle.Name = "tsslCameraTitle";
            this.tsslCameraTitle.Size = new System.Drawing.Size(55, 19);
            this.tsslCameraTitle.Text = "Camera:";
            // 
            // tsslCamera
            // 
            this.tsslCamera.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.tsslCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslCamera.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.tsslCamera.Name = "tsslCamera";
            this.tsslCamera.Size = new System.Drawing.Size(48, 19);
            this.tsslCamera.Text = "Chưa rõ";
            this.tsslCamera.TextChanged += new System.EventHandler(this.ConnectionStatus_Changed);
            // 
            // tsslSdkTitle
            // 
            this.tsslSdkTitle.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslSdkTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslSdkTitle.Name = "tsslSdkTitle";
            this.tsslSdkTitle.Size = new System.Drawing.Size(35, 19);
            this.tsslSdkTitle.Text = "SDK:";
            // 
            // tsslSdk
            // 
            this.tsslSdk.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.tsslSdk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslSdk.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.tsslSdk.Name = "tsslSdk";
            this.tsslSdk.Size = new System.Drawing.Size(48, 19);
            this.tsslSdk.Text = "Chưa rõ";
            this.tsslSdk.TextChanged += new System.EventHandler(this.ConnectionStatus_Changed);
            // 
            // tsslStatus
            // 
            this.tsslStatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(400, 19);
            this.tsslStatus.Spring = true;
            this.tsslStatus.Text = "Sẵn sàng";
            this.tsslStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsSep1
            // 
            this.tsSep1.Name = "tsSep1";
            this.tsSep1.Size = new System.Drawing.Size(6, 24);
            // 
            // tsddSections
            // 
            this.tsddSections.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSecPart,
            this.tsmiSecControl,
            this.tsmiSecCamera,
            this.tsmiSecList,
            this.tsmiSecStorage,
            this.tsmiSecNetwork,
            this.tsSepSections,
            this.tsmiSecShowAll,
            this.tsmiSecHideAll});
            this.tsddSections.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsddSections.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(116)))), ((int)(((byte)(213)))));
            this.tsddSections.Name = "tsddSections";
            this.tsddSections.Size = new System.Drawing.Size(82, 22);
            this.tsddSections.Text = "☰  Hiển thị";
            this.tsddSections.ToolTipText = "Ẩn / hiện các khu vực của tab Capture & Monitor";
            // 
            // tsmiSecPart
            // 
            this.tsmiSecPart.Checked = true;
            this.tsmiSecPart.CheckOnClick = true;
            this.tsmiSecPart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecPart.Name = "tsmiSecPart";
            this.tsmiSecPart.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecPart.Tag = "Part";
            this.tsmiSecPart.Text = "Cấu hình Part";
            this.tsmiSecPart.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsmiSecControl
            // 
            this.tsmiSecControl.Checked = true;
            this.tsmiSecControl.CheckOnClick = true;
            this.tsmiSecControl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecControl.Name = "tsmiSecControl";
            this.tsmiSecControl.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecControl.Tag = "Control";
            this.tsmiSecControl.Text = "Điều khiển Part && Sequence";
            this.tsmiSecControl.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsmiSecCamera
            // 
            this.tsmiSecCamera.Checked = true;
            this.tsmiSecCamera.CheckOnClick = true;
            this.tsmiSecCamera.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecCamera.Name = "tsmiSecCamera";
            this.tsmiSecCamera.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecCamera.Tag = "Camera";
            this.tsmiSecCamera.Text = "Camera";
            this.tsmiSecCamera.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsmiSecList
            // 
            this.tsmiSecList.Checked = true;
            this.tsmiSecList.CheckOnClick = true;
            this.tsmiSecList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecList.Name = "tsmiSecList";
            this.tsmiSecList.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecList.Tag = "List";
            this.tsmiSecList.Text = "Danh sách ảnh";
            this.tsmiSecList.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsmiSecStorage
            // 
            this.tsmiSecStorage.Checked = true;
            this.tsmiSecStorage.CheckOnClick = true;
            this.tsmiSecStorage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecStorage.Name = "tsmiSecStorage";
            this.tsmiSecStorage.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecStorage.Tag = "Storage";
            this.tsmiSecStorage.Text = "Lưu trữ && Push ảnh";
            this.tsmiSecStorage.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsmiSecNetwork
            // 
            this.tsmiSecNetwork.Checked = true;
            this.tsmiSecNetwork.CheckOnClick = true;
            this.tsmiSecNetwork.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiSecNetwork.Name = "tsmiSecNetwork";
            this.tsmiSecNetwork.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecNetwork.Tag = "Network";
            this.tsmiSecNetwork.Text = "Mạng && Thiết bị";
            this.tsmiSecNetwork.CheckedChanged += new System.EventHandler(this.tsmiSection_CheckedChanged);
            // 
            // tsSepSections
            // 
            this.tsSepSections.Name = "tsSepSections";
            this.tsSepSections.Size = new System.Drawing.Size(228, 6);
            // 
            // tsmiSecShowAll
            // 
            this.tsmiSecShowAll.Name = "tsmiSecShowAll";
            this.tsmiSecShowAll.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecShowAll.Text = "Hiện tất cả";
            this.tsmiSecShowAll.Click += new System.EventHandler(this.tsmiSecShowAll_Click);
            //
            // tsmiSecHideAll
            //
            this.tsmiSecHideAll.Name = "tsmiSecHideAll";
            this.tsmiSecHideAll.Size = new System.Drawing.Size(231, 22);
            this.tsmiSecHideAll.Text = "Ẩn nhanh tất cả";
            this.tsmiSecHideAll.Click += new System.EventHandler(this.tsmiSecHideAll_Click);
            //
            // tsbCheckStatus
            // 
            this.tsbCheckStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.tsbCheckStatus.Name = "tsbCheckStatus";
            this.tsbCheckStatus.Size = new System.Drawing.Size(109, 22);
            this.tsbCheckStatus.Text = "Kiểm tra trạng thái";
            this.tsbCheckStatus.ToolTipText = "Kiểm tra kết nối UnitX / camera / SDK";
            this.tsbCheckStatus.Click += new System.EventHandler(this.btnCheckStatus_Click);
            // 
            // tsbShowLog
            // 
            this.tsbShowLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.tsbShowLog.Name = "tsbShowLog";
            this.tsbShowLog.Size = new System.Drawing.Size(55, 22);
            this.tsbShowLog.Text = "Xem log";
            this.tsbShowLog.Click += new System.EventHandler(this.btnShowLog_Click);
            // 
            // tsbClearLog
            // 
            this.tsbClearLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.tsbClearLog.Name = "tsbClearLog";
            this.tsbClearLog.Size = new System.Drawing.Size(51, 22);
            this.tsbClearLog.Text = "Xoá log";
            this.tsbClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // tsSep2
            // 
            this.tsSep2.Name = "tsSep2";
            this.tsSep2.Size = new System.Drawing.Size(6, 24);
            // 
            // tspbPush
            // 
            this.tspbPush.Name = "tspbPush";
            this.tspbPush.Size = new System.Drawing.Size(160, 18);
            this.tspbPush.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // tsslOverall
            // 
            this.tsslOverall.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslOverall.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.tsslOverall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslOverall.Name = "tsslOverall";
            this.tsslOverall.Size = new System.Drawing.Size(87, 19);
            this.tsslOverall.Text = "Chưa sẵn sàng";
            this.tsslOverall.ToolTipText = "Trạng thái tổng thể - bấm để kiểm tra chi tiết";
            this.tsslOverall.Click += new System.EventHandler(this.tsslOverall_Click);
            // 
            // tsslClock
            // 
            this.tsslClock.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.tsslClock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.tsslClock.Name = "tsslClock";
            this.tsslClock.Size = new System.Drawing.Size(21, 19);
            this.tsslClock.Text = "--";
            // 
            // frmMainV1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(1320, 864);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlAppHeader);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1120, 760);
            this.Name = "frmMainV1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vision Image Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMainV1_FormClosing);
            this.Load += new System.EventHandler(this.frmMainV1_Load);
            this.pnlAppHeader.ResumeLayout(false);
            this.pnlAppHeader.PerformLayout();
            this.tabMain.ResumeLayout(false);
            this.tabDataset.ResumeLayout(false);
            this.tabCapture.ResumeLayout(false);
            this.tlpRoot.ResumeLayout(false);
            this.tlpTopRow.ResumeLayout(false);
            this.pnlPartCard.ResumeLayout(false);
            this.tlpPartFields.ResumeLayout(false);
            this.tlpPartFields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            this.pnlPartToolbar.ResumeLayout(false);
            this.tlpPartButtons.ResumeLayout(false);
            this.tlpPartButtons.PerformLayout();
            this.pnlCameraToolbar.ResumeLayout(false);
            this.tlpCameraButtons.ResumeLayout(false);
            this.tlpCameraButtons.PerformLayout();
            this.tlpTriggerRepeat.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numTriggerRepeat)).EndInit();
            this.scWork.Panel1.ResumeLayout(false);
            this.scWork.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scWork)).EndInit();
            this.scWork.ResumeLayout(false);
            this.pnlListCard.ResumeLayout(false);
            this.tlpListCard.ResumeLayout(false);
            this.tlpListCard.PerformLayout();
            this.tlpListTabs.ResumeLayout(false);
            this.pnlListHost.ResumeLayout(false);
            this.pnlRawCard.ResumeLayout(false);
            this.tlpRaw.ResumeLayout(false);
            this.tlpRawPager.ResumeLayout(false);
            this.pnlMaskCard.ResumeLayout(false);
            this.tlpMask.ResumeLayout(false);
            this.tlpMaskPager.ResumeLayout(false);
            this.pnlPreviewCard.ResumeLayout(false);
            this.tlpPreview.ResumeLayout(false);
            this.tlpPreview.PerformLayout();
            this.tlpPreviewToggle.ResumeLayout(false);
            this.pnlPreviewBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tlpPreviewNav.ResumeLayout(false);
            this.tlpBottomRow.ResumeLayout(false);
            this.pnlPathCard.ResumeLayout(false);
            this.tlpPath.ResumeLayout(false);
            this.tlpPath.PerformLayout();
            this.tlpPathOptions.ResumeLayout(false);
            this.tlpPathOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numJpegQuality)).EndInit();
            this.tlpPathButtons.ResumeLayout(false);
            this.pnlSdkCard.ResumeLayout(false);
            this.tlpSdk.ResumeLayout(false);
            this.tlpSdk.PerformLayout();
            this.tlpPortsHead.ResumeLayout(false);
            this.tlpPortsHead.PerformLayout();
            this.tlpPorts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSdkGrpc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkZmq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkBlob)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSdkClient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAppLogo)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlAppHeader;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.PictureBox picAppLogo;
        private System.Windows.Forms.Label lblAppSubtitle;
        private System.Windows.Forms.Button btnTabCapture;
        private System.Windows.Forms.Button btnTabDataset;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabCapture;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel tlpTopRow;
        private VisionImageTool.CardPanel pnlPartCard;
        private System.Windows.Forms.TableLayoutPanel tlpPartFields;
        private System.Windows.Forms.Label lblPartCardTitle;
        private System.Windows.Forms.Label lblPartType;
        private System.Windows.Forms.ComboBox cboPartType;
        private System.Windows.Forms.Label lblPartID;
        private System.Windows.Forms.TextBox txtPartID;
        private System.Windows.Forms.Label lblCaptureConfig;
        private System.Windows.Forms.ComboBox cboCaptureConfig;
        private System.Windows.Forms.Label lblCameraID;
        private System.Windows.Forms.ComboBox cboCameraID;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown numDelay;
        private System.Windows.Forms.Label lblSequenceIO;
        private System.Windows.Forms.ComboBox cboSequenceIO;
        private VisionImageTool.CardPanel pnlPartToolbar;
        private System.Windows.Forms.TableLayoutPanel tlpPartButtons;
        private System.Windows.Forms.Label lblPartToolbar;
        private System.Windows.Forms.Button btnPartStart;
        private System.Windows.Forms.Button btnRemovePart;
        private System.Windows.Forms.Button btnRestartPart;
        private System.Windows.Forms.Button btnSelectSequence;
        private System.Windows.Forms.Button btnSystemReset;
        private VisionImageTool.CardPanel pnlCameraToolbar;
        private System.Windows.Forms.TableLayoutPanel tlpCameraButtons;
        private System.Windows.Forms.Label lblCameraToolbar;
        private System.Windows.Forms.Button btnTriggerOnce;
        private System.Windows.Forms.Button btnTriggerContinuous;
        private System.Windows.Forms.Button btnCameraReset;
        private System.Windows.Forms.TableLayoutPanel tlpTriggerRepeat;
        private System.Windows.Forms.Label lblTriggerRepeat;
        private System.Windows.Forms.NumericUpDown numTriggerRepeat;
        private System.Windows.Forms.Label lblTriggerRepeatUnit;
        private System.Windows.Forms.SplitContainer scWork;
        private VisionImageTool.CardPanel pnlListCard;
        private System.Windows.Forms.TableLayoutPanel tlpListCard;
        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Label lblListTotal;
        private System.Windows.Forms.TableLayoutPanel tlpListTabs;
        private System.Windows.Forms.Button btnListRaw;
        private System.Windows.Forms.Button btnListMask;
        private System.Windows.Forms.Panel pnlListHost;
        private System.Windows.Forms.Panel pnlMaskCard;
        private System.Windows.Forms.TableLayoutPanel tlpMask;
        private System.Windows.Forms.Label lblMaskHeader;
        private System.Windows.Forms.Label lblMaskCount;
        private System.Windows.Forms.ListView lvMaskImage;
        private System.Windows.Forms.ColumnHeader chMaskIndex;
        private System.Windows.Forms.ColumnHeader chMaskName;
        private System.Windows.Forms.ColumnHeader chMaskTime;
        private System.Windows.Forms.TableLayoutPanel tlpMaskPager;
        private System.Windows.Forms.Button btnMaskFirst;
        private System.Windows.Forms.Button btnMaskPrev;
        private System.Windows.Forms.Label lblMaskPage;
        private System.Windows.Forms.Button btnMaskNext;
        private System.Windows.Forms.Button btnMaskLast;
        private System.Windows.Forms.Panel pnlRawCard;
        private System.Windows.Forms.TableLayoutPanel tlpRaw;
        private System.Windows.Forms.Label lblRawHeader;
        private System.Windows.Forms.Label lblRawCount;
        private System.Windows.Forms.ListView lvRawImage;
        private System.Windows.Forms.ColumnHeader chRawIndex;
        private System.Windows.Forms.ColumnHeader chRawName;
        private System.Windows.Forms.ColumnHeader chRawTime;
        private System.Windows.Forms.TableLayoutPanel tlpRawPager;
        private System.Windows.Forms.Button btnRawFirst;
        private System.Windows.Forms.Button btnRawPrev;
        private System.Windows.Forms.Label lblRawPage;
        private System.Windows.Forms.Button btnRawNext;
        private System.Windows.Forms.Button btnRawLast;
        private VisionImageTool.CardPanel pnlPreviewCard;
        private System.Windows.Forms.TableLayoutPanel tlpPreview;
        private System.Windows.Forms.Label lblPreviewTitle;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewToggle;
        private System.Windows.Forms.Button btnViewRaw;
        private System.Windows.Forms.Button btnViewMask;
        private System.Windows.Forms.Label lblPreviewKeyHint;
        private System.Windows.Forms.Label lblPreviewName;
        private System.Windows.Forms.Label lblPreviewMeta;
        private System.Windows.Forms.Panel pnlPreviewBody;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Label lblPreviewHint;
        private VisionImageTool.ZoomPictureBox picPreview;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewNav;
        private System.Windows.Forms.Button btnPrevImage;
        private System.Windows.Forms.Label lblImageNav;
        private System.Windows.Forms.Button btnNextImage;
        private System.Windows.Forms.Label lblPreviewHeader;
        private System.Windows.Forms.TableLayoutPanel tlpBottomRow;
        private VisionImageTool.CardPanel pnlPathCard;
        private System.Windows.Forms.TableLayoutPanel tlpPath;
        private System.Windows.Forms.Label lblPathCardTitle;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblImageName;
        private System.Windows.Forms.TextBox txtImageName;
        private System.Windows.Forms.Label lblNamePrefix;
        private System.Windows.Forms.ComboBox cboNamePrefix;
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cboImageFormat;
        private System.Windows.Forms.TableLayoutPanel tlpPathOptions;
        private System.Windows.Forms.CheckBox chkSaveRaw;
        private System.Windows.Forms.CheckBox chkSaveCallback;
        private System.Windows.Forms.Label lblJpegQuality;
        private System.Windows.Forms.NumericUpDown numJpegQuality;
        private System.Windows.Forms.Label lblJpegQualityHint;
        private System.Windows.Forms.CheckBox chkWatchFolder;
        private System.Windows.Forms.Label lblNamePreview;
        private System.Windows.Forms.TableLayoutPanel tlpPathButtons;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnPushAll;
        private System.Windows.Forms.Button btnPushSelected;
        private System.Windows.Forms.Button btnClearPushHistory;
        private VisionImageTool.CardPanel pnlSdkCard;
        private System.Windows.Forms.TableLayoutPanel tlpSdk;
        private System.Windows.Forms.Label lblSdkCardTitle;
        private System.Windows.Forms.Label lblSdkHost;
        private System.Windows.Forms.TextBox txtSdkHost;
        private System.Windows.Forms.Button btnSdkTest;
        private System.Windows.Forms.Button btnSdkSave;
        private System.Windows.Forms.Label lblSdkDivider;
        private System.Windows.Forms.Label lblConnTitle;
        private System.Windows.Forms.Panel pnlConnTiles;
        private System.Windows.Forms.TableLayoutPanel tlpPortsHead;
        private System.Windows.Forms.Label lblPortsTitle;
        private System.Windows.Forms.LinkLabel lnkPorts;
        private System.Windows.Forms.TableLayoutPanel tlpPorts;
        private System.Windows.Forms.Label lblSdkGrpc;
        private System.Windows.Forms.NumericUpDown numSdkGrpc;
        private System.Windows.Forms.Label lblSdkZmq;
        private System.Windows.Forms.NumericUpDown numSdkZmq;
        private System.Windows.Forms.Label lblSdkBlob;
        private System.Windows.Forms.NumericUpDown numSdkBlob;
        private System.Windows.Forms.Label lblSdkClient;
        private System.Windows.Forms.NumericUpDown numSdkClient;
        private System.Windows.Forms.Label lblSdkStatus;
        private System.Windows.Forms.Label lblPortsSummary;
        private System.Windows.Forms.TabPage tabDataset;
        private VisionImageTool.ucDatasetManager ucDataset;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslUnitXTitle;
        private System.Windows.Forms.ToolStripStatusLabel tsslUnitX;
        private System.Windows.Forms.ToolStripStatusLabel tsslCameraTitle;
        private System.Windows.Forms.ToolStripStatusLabel tsslCamera;
        private System.Windows.Forms.ToolStripStatusLabel tsslSdkTitle;
        private System.Windows.Forms.ToolStripStatusLabel tsslSdk;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripDropDownButton tsddSections;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecPart;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecControl;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecCamera;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecList;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecStorage;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecNetwork;
        private System.Windows.Forms.ToolStripSeparator tsSepSections;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecShowAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiSecHideAll;
        private System.Windows.Forms.ToolStripButton tsbCheckStatus;
        private System.Windows.Forms.ToolStripButton tsbShowLog;
        private System.Windows.Forms.ToolStripButton tsbClearLog;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripProgressBar tspbPush;
        private System.Windows.Forms.ToolStripStatusLabel tsslOverall;
        private System.Windows.Forms.ToolStripStatusLabel tsslClock;
    }
}
