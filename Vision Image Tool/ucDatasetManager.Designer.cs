namespace VisionImageTool
{
    partial class ucDatasetManager
    {
        /// <summary>
        /// Bố cục tab Dataset Manager - chỉnh trực tiếp trong Designer (cửa sổ Properties / Document Outline).
        ///   tlpRoot = [pnlLoadCard: Nạp dataset]  [pnlSchemaCard: Schema hiện hành]
        ///             [pnlFilterCard: Bộ lọc & trạng thái = thành phần schema + bộ lọc]   (3 khối thu gọn được bằng nút ˅)
        ///             [scIssues: (scPreview: (scTree: Cây | Bảng) | Xem trước) / (scBottom: Vấn đề | Thống kê)]
        ///             [pnlStatusCard: thanh trạng thái]
        ///   Xem trước: [Raw][Mask][Raw | Mask] [⤢ mở rộng] ... [← Trước] n / N [Sau →]  [−][%][+][Fit]
        /// Code (ucDatasetManager.Shell.cs ...) chỉ gắn sự kiện / dữ liệu, không đặt lại kích thước, màu, chữ.
        /// Ngoại lệ (đổi lúc chạy): ẩn / hiện khu vực (☰ Hiển thị), vị trí vạch chia đã lưu, ◀ ▶ lần chụp.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLoadCard = new VisionImageTool.CardPanel();
            this.tlpLoadCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpLoadHeader = new System.Windows.Forms.TableLayoutPanel();
            this.btnCollapseLoad = new System.Windows.Forms.Button();
            this.btnCollapseSchema = new System.Windows.Forms.Button();
            this.btnCollapseFilter = new System.Windows.Forms.Button();
            this.pnlSchemaCard = new VisionImageTool.CardPanel();
            this.tlpSchemaCard = new System.Windows.Forms.TableLayoutPanel();
            this.lblLoadTitle = new System.Windows.Forms.Label();
            this.tlpFolderRow = new System.Windows.Forms.TableLayoutPanel();
            this.pnlFolderBox = new VisionImageTool.CardPanel();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.lblFolderCaption = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.flpSubfolders = new System.Windows.Forms.FlowLayoutPanel();
            this.chkSubfolders = new System.Windows.Forms.CheckBox();
            this.lblSubfoldersNote = new System.Windows.Forms.Label();
            this.tlpSchemaTitle = new System.Windows.Forms.TableLayoutPanel();
            this.lblSchemaTitle = new System.Windows.Forms.Label();
            this.lblSchemaCaption = new System.Windows.Forms.Label();
            this.lblConvertState = new System.Windows.Forms.Label();
            this.tlpSchemaRow = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSchemaBox = new VisionImageTool.CardPanel();
            this.txtSchema = new System.Windows.Forms.TextBox();
            this.btnRecentSchemas = new System.Windows.Forms.Button();
            this.lblReadMode = new System.Windows.Forms.Label();
            this.btnApplySchema = new System.Windows.Forms.Button();
            this.flpConvert = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnUndoConvert = new System.Windows.Forms.Button();
            this.lblSchemaInfo = new System.Windows.Forms.Label();
            this.pnlFilterCard = new VisionImageTool.CardPanel();
            this.tlpFilterCard = new System.Windows.Forms.TableLayoutPanel();
            this.flpChipsTitle = new System.Windows.Forms.FlowLayoutPanel();
            this.lblChipsTitle = new System.Windows.Forms.Label();
            this.tlpChipsRow = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSchemaChips = new VisionImageTool.SchemaChipsPanel();
            this.btnSchemaHint = new System.Windows.Forms.Button();
            this.btnComponents = new System.Windows.Forms.Button();
            this.tlpFilterRow = new System.Windows.Forms.TableLayoutPanel();
            this.flpFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFilterTitle = new System.Windows.Forms.Label();
            this.btnShowConfig = new System.Windows.Forms.Button();
            this.flpFind = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFind = new System.Windows.Forms.Label();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.flpDk = new System.Windows.Forms.FlowLayoutPanel();
            this.lblDk = new System.Windows.Forms.Label();
            this.cboDkFilter = new System.Windows.Forms.ComboBox();
            this.flpVision = new System.Windows.Forms.FlowLayoutPanel();
            this.lblVision = new System.Windows.Forms.Label();
            this.cboUnitXFilter = new System.Windows.Forms.ComboBox();
            this.flpEval = new System.Windows.Forms.FlowLayoutPanel();
            this.lblEval = new System.Windows.Forms.Label();
            this.cboEvalFilter = new System.Windows.Forms.ComboBox();
            this.chkUnmatchedOnly = new System.Windows.Forms.CheckBox();
            this.lblResultCount = new System.Windows.Forms.Label();
            this.flpRule = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRuleSep = new System.Windows.Forms.Label();
            this.lblRule = new System.Windows.Forms.Label();
            this.cboCompareMode = new System.Windows.Forms.ComboBox();
            this.chkLock = new System.Windows.Forms.CheckBox();
            this.btnResetFilters = new System.Windows.Forms.Button();
            this.scIssues = new System.Windows.Forms.SplitContainer();
            this.scPreview = new System.Windows.Forms.SplitContainer();
            this.scTree = new System.Windows.Forms.SplitContainer();
            this.pnlTreeCard = new VisionImageTool.CardPanel();
            this.tlpTreeCard = new System.Windows.Forms.TableLayoutPanel();
            this.lblTreeTitle = new System.Windows.Forms.Label();
            this.tvDataset = new System.Windows.Forms.TreeView();
            this.pnlTreeSummary = new System.Windows.Forms.Panel();
            this.pnlTableCard = new VisionImageTool.CardPanel();
            this.tlpTableCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpTableToolbar = new System.Windows.Forms.TableLayoutPanel();
            this.lblTableTitle = new System.Windows.Forms.Label();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.flpTableActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnKeys = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.pnlPreviewCard = new VisionImageTool.CardPanel();
            this.tlpPreviewCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPreviewTitle = new System.Windows.Forms.TableLayoutPanel();
            this.lblPreviewTitle = new System.Windows.Forms.Label();
            this.flpViewMode = new System.Windows.Forms.FlowLayoutPanel();
            this.btnShowRaw = new System.Windows.Forms.Button();
            this.btnShowMask = new System.Windows.Forms.Button();
            this.btnSplit = new System.Windows.Forms.Button();
            this.btnExpandPreview = new System.Windows.Forms.Button();
            this.lblPreview = new System.Windows.Forms.Label();
            this.tlpPreviewMeta = new System.Windows.Forms.TableLayoutPanel();
            this.lblPreviewMeta = new System.Windows.Forms.Label();
            this.btnShotPrev = new System.Windows.Forms.Button();
            this.btnShotNext = new System.Windows.Forms.Button();
            this.picPreview = new VisionImageTool.ZoomPictureBox();
            this.tlpPreviewNav = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevRow = new System.Windows.Forms.Button();
            this.lblNavPos = new System.Windows.Forms.Label();
            this.btnNextRow = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.cboZoom = new System.Windows.Forms.ComboBox();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnZoomFit = new System.Windows.Forms.Button();
            this.scBottom = new System.Windows.Forms.SplitContainer();
            this.pnlIssuesCard = new VisionImageTool.CardPanel();
            this.tlpIssues = new System.Windows.Forms.TableLayoutPanel();
            this.lblIssuesTitle = new System.Windows.Forms.Label();
            this.lblIssueKind = new System.Windows.Forms.Label();
            this.cboIssueKind = new System.Windows.Forms.ComboBox();
            this.btnExportIssues = new System.Windows.Forms.Button();
            this.lvIssues = new System.Windows.Forms.ListView();
            this.chIssueKind = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chIssueLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chIssueFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chIssueDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlStatsCard = new VisionImageTool.CardPanel();
            this.tlpStats = new System.Windows.Forms.TableLayoutPanel();
            this.flpStatsLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.lblStatsTitle = new System.Windows.Forms.Label();
            this.lblStatsSub = new System.Windows.Forms.Label();
            this.pnlStatCards = new System.Windows.Forms.Panel();
            this.chartStats = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.pnlStatusCard = new VisionImageTool.CardPanel();
            this.tlpStatus = new System.Windows.Forms.TableLayoutPanel();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.stProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.stMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.stImagesCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.stImages = new System.Windows.Forms.ToolStripStatusLabel();
            this.stRawCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.stRaw = new System.Windows.Forms.ToolStripStatusLabel();
            this.stMaskCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.stMask = new System.Windows.Forms.ToolStripStatusLabel();
            this.stSchema = new System.Windows.Forms.ToolStripStatusLabel();
            this.stIssues = new System.Windows.Forms.ToolStripStatusLabel();
            this.stSelection = new System.Windows.Forms.ToolStripStatusLabel();
            this.stSpringLeft = new System.Windows.Forms.ToolStripStatusLabel();
            this.stEvalCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.stO = new System.Windows.Forms.ToolStripStatusLabel();
            this.stDelta = new System.Windows.Forms.ToolStripStatusLabel();
            this.stX = new System.Windows.Forms.ToolStripStatusLabel();
            this.stNone = new System.Windows.Forms.ToolStripStatusLabel();
            this.stSpringRight = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tlpRoot.SuspendLayout();
            this.pnlLoadCard.SuspendLayout();
            this.tlpLoadCard.SuspendLayout();
            this.tlpLoadHeader.SuspendLayout();
            this.pnlSchemaCard.SuspendLayout();
            this.tlpSchemaCard.SuspendLayout();
            this.tlpFolderRow.SuspendLayout();
            this.pnlFolderBox.SuspendLayout();
            this.flpSubfolders.SuspendLayout();
            this.tlpSchemaTitle.SuspendLayout();
            this.tlpSchemaRow.SuspendLayout();
            this.pnlSchemaBox.SuspendLayout();
            this.flpConvert.SuspendLayout();
            this.pnlFilterCard.SuspendLayout();
            this.tlpFilterCard.SuspendLayout();
            this.flpChipsTitle.SuspendLayout();
            this.tlpChipsRow.SuspendLayout();
            this.pnlSchemaChips.SuspendLayout();
            this.tlpFilterRow.SuspendLayout();
            this.flpFilter.SuspendLayout();
            this.flpFind.SuspendLayout();
            this.flpDk.SuspendLayout();
            this.flpVision.SuspendLayout();
            this.flpEval.SuspendLayout();
            this.flpRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scIssues)).BeginInit();
            this.scIssues.Panel1.SuspendLayout();
            this.scIssues.Panel2.SuspendLayout();
            this.scIssues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scPreview)).BeginInit();
            this.scPreview.Panel1.SuspendLayout();
            this.scPreview.Panel2.SuspendLayout();
            this.scPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTree)).BeginInit();
            this.scTree.Panel1.SuspendLayout();
            this.scTree.Panel2.SuspendLayout();
            this.scTree.SuspendLayout();
            this.pnlTreeCard.SuspendLayout();
            this.tlpTreeCard.SuspendLayout();
            this.pnlTableCard.SuspendLayout();
            this.tlpTableCard.SuspendLayout();
            this.tlpTableToolbar.SuspendLayout();
            this.flpTableActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.pnlPreviewCard.SuspendLayout();
            this.tlpPreviewCard.SuspendLayout();
            this.tlpPreviewTitle.SuspendLayout();
            this.flpViewMode.SuspendLayout();
            this.tlpPreviewMeta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tlpPreviewNav.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).BeginInit();
            this.scBottom.Panel1.SuspendLayout();
            this.scBottom.Panel2.SuspendLayout();
            this.scBottom.SuspendLayout();
            this.pnlIssuesCard.SuspendLayout();
            this.tlpIssues.SuspendLayout();
            this.pnlStatsCard.SuspendLayout();
            this.tlpStats.SuspendLayout();
            this.flpStatsLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartStats)).BeginInit();
            this.pnlStatusCard.SuspendLayout();
            this.tlpStatus.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpRoot
            //
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlLoadCard, 0, 0);
            this.tlpRoot.Controls.Add(this.pnlSchemaCard, 0, 1);
            this.tlpRoot.Controls.Add(this.pnlFilterCard, 0, 2);
            this.tlpRoot.Controls.Add(this.scIssues, 0, 3);
            this.tlpRoot.Controls.Add(this.pnlStatusCard, 0, 4);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(12, 10);
            this.tlpRoot.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 5;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpRoot.Size = new System.Drawing.Size(1376, 882);
            this.tlpRoot.TabIndex = 0;
            //
            // pnlLoadCard
            //
            this.pnlLoadCard.BackColor = System.Drawing.Color.White;
            this.pnlLoadCard.Controls.Add(this.tlpLoadCard);
            this.pnlLoadCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLoadCard.HideButton = true;
            this.pnlLoadCard.Location = new System.Drawing.Point(0, 0);
            this.pnlLoadCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlLoadCard.Name = "pnlLoadCard";
            this.pnlLoadCard.Padding = new System.Windows.Forms.Padding(14, 6, 18, 8);
            this.pnlLoadCard.Size = new System.Drawing.Size(1376, 80);
            this.pnlLoadCard.TabIndex = 0;
            //
            // tlpLoadCard
            //
            this.tlpLoadCard.ColumnCount = 1;
            this.tlpLoadCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoadCard.Controls.Add(this.tlpLoadHeader, 0, 0);
            this.tlpLoadCard.Controls.Add(this.tlpFolderRow, 0, 1);
            this.tlpLoadCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLoadCard.Location = new System.Drawing.Point(14, 6);
            this.tlpLoadCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLoadCard.Name = "tlpLoadCard";
            this.tlpLoadCard.RowCount = 2;
            this.tlpLoadCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpLoadCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoadCard.Size = new System.Drawing.Size(1344, 66);
            this.tlpLoadCard.TabIndex = 0;
            //
            // tlpLoadHeader
            //
            this.tlpLoadHeader.ColumnCount = 3;
            this.tlpLoadHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpLoadHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpLoadHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoadHeader.Controls.Add(this.btnCollapseLoad, 0, 0);
            this.tlpLoadHeader.Controls.Add(this.lblLoadTitle, 1, 0);
            this.tlpLoadHeader.Controls.Add(this.flpSubfolders, 2, 0);
            this.tlpLoadHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLoadHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpLoadHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLoadHeader.Name = "tlpLoadHeader";
            this.tlpLoadHeader.RowCount = 1;
            this.tlpLoadHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoadHeader.Size = new System.Drawing.Size(1344, 28);
            this.tlpLoadHeader.TabIndex = 0;
            //
            // btnCollapseLoad
            //
            this.btnCollapseLoad.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseLoad.FlatAppearance.BorderSize = 0;
            this.btnCollapseLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnCollapseLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseLoad.Font = new System.Drawing.Font("Segoe MDL2 Assets", 8F);
            this.btnCollapseLoad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnCollapseLoad.Location = new System.Drawing.Point(0, 2);
            this.btnCollapseLoad.Margin = new System.Windows.Forms.Padding(0, 2, 6, 0);
            this.btnCollapseLoad.Name = "btnCollapseLoad";
            this.btnCollapseLoad.Size = new System.Drawing.Size(24, 24);
            this.btnCollapseLoad.TabIndex = 0;
            this.btnCollapseLoad.Text = "";
            this.toolTip.SetToolTip(this.btnCollapseLoad, "Thu gọn / mở rộng khối Nạp dataset");
            this.btnCollapseLoad.UseVisualStyleBackColor = true;
            //
            // pnlSchemaCard
            //
            this.pnlSchemaCard.BackColor = System.Drawing.Color.White;
            this.pnlSchemaCard.Controls.Add(this.tlpSchemaCard);
            this.pnlSchemaCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSchemaCard.Location = new System.Drawing.Point(0, 88);
            this.pnlSchemaCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlSchemaCard.Name = "pnlSchemaCard";
            this.pnlSchemaCard.Padding = new System.Windows.Forms.Padding(14, 6, 18, 4);
            this.pnlSchemaCard.HideButton = true;
            this.pnlSchemaCard.Size = new System.Drawing.Size(1376, 96);
            this.pnlSchemaCard.TabIndex = 0;
            //
            // tlpSchemaCard
            //
            this.tlpSchemaCard.ColumnCount = 1;
            this.tlpSchemaCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaCard.Controls.Add(this.tlpSchemaTitle, 0, 0);
            this.tlpSchemaCard.Controls.Add(this.tlpSchemaRow, 0, 1);
            this.tlpSchemaCard.Controls.Add(this.lblSchemaInfo, 0, 2);
            this.tlpSchemaCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSchemaCard.Location = new System.Drawing.Point(14, 6);
            this.tlpSchemaCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSchemaCard.Name = "tlpSchemaCard";
            this.tlpSchemaCard.RowCount = 3;
            this.tlpSchemaCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpSchemaCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpSchemaCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaCard.Size = new System.Drawing.Size(1344, 86);
            this.tlpSchemaCard.TabIndex = 0;
            //
            // lblLoadTitle
            //
            this.lblLoadTitle.AutoSize = true;
            this.lblLoadTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblLoadTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblLoadTitle.Location = new System.Drawing.Point(30, 5);
            this.lblLoadTitle.Margin = new System.Windows.Forms.Padding(0, 5, 16, 0);
            this.lblLoadTitle.Name = "lblLoadTitle";
            this.lblLoadTitle.Size = new System.Drawing.Size(140, 19);
            this.lblLoadTitle.TabIndex = 0;
            this.lblLoadTitle.Text = "NẠP DATASET";
            //
            // tlpFolderRow
            //
            this.tlpFolderRow.ColumnCount = 3;
            this.tlpFolderRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFolderRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpFolderRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpFolderRow.Controls.Add(this.pnlFolderBox, 0, 0);
            this.tlpFolderRow.Controls.Add(this.btnBrowse, 1, 0);
            this.tlpFolderRow.Controls.Add(this.btnLoad, 2, 0);
            this.tlpFolderRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFolderRow.Location = new System.Drawing.Point(0, 28);
            this.tlpFolderRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpFolderRow.Name = "tlpFolderRow";
            this.tlpFolderRow.RowCount = 1;
            this.tlpFolderRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFolderRow.Size = new System.Drawing.Size(1340, 38);
            this.tlpFolderRow.TabIndex = 1;
            //
            // pnlFolderBox
            //
            this.pnlFolderBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlFolderBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.pnlFolderBox.Controls.Add(this.txtFolder);
            this.pnlFolderBox.Controls.Add(this.lblFolderCaption);
            this.pnlFolderBox.CornerRadius = 6;
            this.pnlFolderBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFolderBox.Location = new System.Drawing.Point(0, 2);
            this.pnlFolderBox.Margin = new System.Windows.Forms.Padding(0, 2, 8, 2);
            this.pnlFolderBox.Name = "pnlFolderBox";
            this.pnlFolderBox.Padding = new System.Windows.Forms.Padding(12, 9, 6, 4);
            this.pnlFolderBox.Size = new System.Drawing.Size(1108, 34);
            this.pnlFolderBox.TabIndex = 0;
            //
            // txtFolder
            //
            this.txtFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtFolder.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFolder.Location = new System.Drawing.Point(116, 9);
            this.txtFolder.Margin = new System.Windows.Forms.Padding(0);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(986, 16);
            this.txtFolder.TabIndex = 0;
            //
            // lblFolderCaption
            //
            this.lblFolderCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.lblFolderCaption.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFolderCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblFolderCaption.Location = new System.Drawing.Point(12, 9);
            this.lblFolderCaption.Name = "lblFolderCaption";
            this.lblFolderCaption.Size = new System.Drawing.Size(104, 21);
            this.lblFolderCaption.TabIndex = 1;
            this.lblFolderCaption.Text = "Thư mục ảnh";
            //
            // btnBrowse
            //
            this.btnBrowse.BackColor = System.Drawing.Color.White;
            this.btnBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBrowse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnBrowse.Location = new System.Drawing.Point(1116, 2);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(0, 2, 8, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(96, 34);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Duyệt…";
            this.btnBrowse.UseVisualStyleBackColor = false;
            //
            // btnLoad
            //
            this.btnLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnLoad.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoad.FlatAppearance.BorderSize = 0;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnLoad.ForeColor = System.Drawing.Color.White;
            this.btnLoad.Location = new System.Drawing.Point(1220, 2);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(120, 34);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Quét thư mục";
            this.btnLoad.UseVisualStyleBackColor = false;
            //
            // flpSubfolders
            //
            this.flpSubfolders.Controls.Add(this.chkSubfolders);
            this.flpSubfolders.Controls.Add(this.lblSubfoldersNote);
            this.flpSubfolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSubfolders.Location = new System.Drawing.Point(140, 2);
            this.flpSubfolders.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpSubfolders.Name = "flpSubfolders";
            this.flpSubfolders.Size = new System.Drawing.Size(1204, 26);
            this.flpSubfolders.TabIndex = 2;
            this.flpSubfolders.WrapContents = false;
            //
            // chkSubfolders
            //
            this.chkSubfolders.AutoSize = true;
            this.chkSubfolders.Checked = true;
            this.chkSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSubfolders.Location = new System.Drawing.Point(2, 4);
            this.chkSubfolders.Margin = new System.Windows.Forms.Padding(2, 4, 8, 0);
            this.chkSubfolders.Name = "chkSubfolders";
            this.chkSubfolders.Size = new System.Drawing.Size(119, 19);
            this.chkSubfolders.TabIndex = 0;
            this.chkSubfolders.Text = "Quét thư mục con";
            this.chkSubfolders.UseVisualStyleBackColor = true;
            //
            // lblSubfoldersNote
            //
            this.lblSubfoldersNote.AutoSize = true;
            this.lblSubfoldersNote.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblSubfoldersNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSubfoldersNote.Location = new System.Drawing.Point(129, 4);
            this.lblSubfoldersNote.Margin = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.lblSubfoldersNote.Name = "lblSubfoldersNote";
            this.lblSubfoldersNote.Size = new System.Drawing.Size(229, 13);
            this.lblSubfoldersNote.TabIndex = 1;
            this.lblSubfoldersNote.Text = "(tự bật và khoá khi schema có cấp thư mục)";
            //
            // btnCollapseSchema
            //
            this.btnCollapseSchema.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseSchema.FlatAppearance.BorderSize = 0;
            this.btnCollapseSchema.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnCollapseSchema.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseSchema.Font = new System.Drawing.Font("Segoe MDL2 Assets", 8F);
            this.btnCollapseSchema.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnCollapseSchema.Location = new System.Drawing.Point(0, 2);
            this.btnCollapseSchema.Margin = new System.Windows.Forms.Padding(0, 2, 6, 0);
            this.btnCollapseSchema.Name = "btnCollapseSchema";
            this.btnCollapseSchema.Size = new System.Drawing.Size(24, 24);
            this.btnCollapseSchema.TabIndex = 0;
            this.btnCollapseSchema.Text = "";
            this.toolTip.SetToolTip(this.btnCollapseSchema, "Thu gọn / mở rộng khối Schema");
            this.btnCollapseSchema.UseVisualStyleBackColor = true;
            //
            // tlpSchemaTitle
            //
            this.tlpSchemaTitle.ColumnCount = 4;
            this.tlpSchemaTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpSchemaTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSchemaTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSchemaTitle.Controls.Add(this.btnCollapseSchema, 0, 0);
            this.tlpSchemaTitle.Controls.Add(this.lblSchemaTitle, 1, 0);
            this.tlpSchemaTitle.Controls.Add(this.lblSchemaCaption, 2, 0);
            this.tlpSchemaTitle.Controls.Add(this.lblConvertState, 3, 0);
            this.tlpSchemaTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSchemaTitle.Location = new System.Drawing.Point(0, 0);
            this.tlpSchemaTitle.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSchemaTitle.Name = "tlpSchemaTitle";
            this.tlpSchemaTitle.RowCount = 1;
            this.tlpSchemaTitle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaTitle.Size = new System.Drawing.Size(1344, 28);
            this.tlpSchemaTitle.TabIndex = 4;
            //
            // lblSchemaTitle
            //
            this.lblSchemaTitle.AutoSize = true;
            this.lblSchemaTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSchemaTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSchemaTitle.Location = new System.Drawing.Point(30, 5);
            this.lblSchemaTitle.Margin = new System.Windows.Forms.Padding(0, 5, 16, 0);
            this.lblSchemaTitle.Name = "lblSchemaTitle";
            this.lblSchemaTitle.Size = new System.Drawing.Size(180, 19);
            this.lblSchemaTitle.TabIndex = 0;
            this.lblSchemaTitle.Text = "SCHEMA HIỆN HÀNH";
            //
            // lblSchemaCaption
            //
            this.lblSchemaCaption.AutoEllipsis = true;
            this.lblSchemaCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSchemaCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblSchemaCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSchemaCaption.Location = new System.Drawing.Point(192, 8);
            this.lblSchemaCaption.Margin = new System.Windows.Forms.Padding(0, 9, 8, 0);
            this.lblSchemaCaption.Name = "lblSchemaCaption";
            this.lblSchemaCaption.Size = new System.Drawing.Size(1140, 16);
            this.lblSchemaCaption.TabIndex = 1;
            this.lblSchemaCaption.Text = "CẤU TRÚC ĐANG ÁP DỤNG  ·  tự nhận cách đọc theo dấu \\ (trước dấu \\ cuối = cấp thư mục, sau = tên file) · không cần chọn chế độ";
            //
            // lblConvertState
            //
            this.lblConvertState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConvertState.AutoSize = true;
            this.lblConvertState.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblConvertState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblConvertState.Location = new System.Drawing.Point(1337, 11);
            this.lblConvertState.Name = "lblConvertState";
            this.lblConvertState.Size = new System.Drawing.Size(0, 13);
            this.lblConvertState.TabIndex = 2;
            //
            // tlpSchemaRow
            //
            this.tlpSchemaRow.ColumnCount = 4;
            this.tlpSchemaRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSchemaRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpSchemaRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSchemaRow.Controls.Add(this.pnlSchemaBox, 0, 0);
            this.tlpSchemaRow.Controls.Add(this.lblReadMode, 1, 0);
            this.tlpSchemaRow.Controls.Add(this.btnApplySchema, 2, 0);
            this.tlpSchemaRow.Controls.Add(this.flpConvert, 3, 0);
            this.tlpSchemaRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSchemaRow.Location = new System.Drawing.Point(0, 28);
            this.tlpSchemaRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSchemaRow.Name = "tlpSchemaRow";
            this.tlpSchemaRow.RowCount = 1;
            this.tlpSchemaRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSchemaRow.Size = new System.Drawing.Size(1340, 38);
            this.tlpSchemaRow.TabIndex = 5;
            //
            // pnlSchemaBox
            //
            this.pnlSchemaBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlSchemaBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.pnlSchemaBox.Controls.Add(this.txtSchema);
            this.pnlSchemaBox.Controls.Add(this.btnRecentSchemas);
            this.pnlSchemaBox.CornerRadius = 6;
            this.pnlSchemaBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSchemaBox.Location = new System.Drawing.Point(0, 2);
            this.pnlSchemaBox.Margin = new System.Windows.Forms.Padding(0, 2, 8, 2);
            this.pnlSchemaBox.Name = "pnlSchemaBox";
            this.pnlSchemaBox.Padding = new System.Windows.Forms.Padding(12, 9, 6, 4);
            this.pnlSchemaBox.Size = new System.Drawing.Size(640, 34);
            this.pnlSchemaBox.TabIndex = 0;
            //
            // txtSchema
            //
            this.txtSchema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtSchema.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSchema.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtSchema.Location = new System.Drawing.Point(12, 9);
            this.txtSchema.Margin = new System.Windows.Forms.Padding(0);
            this.txtSchema.Name = "txtSchema";
            this.txtSchema.Size = new System.Drawing.Size(586, 16);
            this.txtSchema.TabIndex = 0;
            //
            // btnRecentSchemas
            //
            this.btnRecentSchemas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.btnRecentSchemas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRecentSchemas.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRecentSchemas.FlatAppearance.BorderSize = 0;
            this.btnRecentSchemas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecentSchemas.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
            this.btnRecentSchemas.Location = new System.Drawing.Point(598, 9);
            this.btnRecentSchemas.Margin = new System.Windows.Forms.Padding(6, 4, 0, 1);
            this.btnRecentSchemas.Name = "btnRecentSchemas";
            this.btnRecentSchemas.Size = new System.Drawing.Size(36, 21);
            this.btnRecentSchemas.TabIndex = 1;
            this.btnRecentSchemas.Text = "🕘";
            this.toolTip.SetToolTip(this.btnRecentSchemas, "Schema đã dùng gần đây - chọn để áp dụng lại");
            this.btnRecentSchemas.UseVisualStyleBackColor = false;
            //
            // lblReadMode
            //
            this.lblReadMode.AutoEllipsis = true;
            this.lblReadMode.AutoSize = true;
            this.lblReadMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.lblReadMode.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblReadMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(91)))), ((int)(((byte)(181)))));
            this.lblReadMode.Location = new System.Drawing.Point(648, 6);
            this.lblReadMode.Margin = new System.Windows.Forms.Padding(0, 6, 0, 4);
            this.lblReadMode.Name = "lblReadMode";
            this.lblReadMode.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.lblReadMode.Size = new System.Drawing.Size(110, 27);
            this.lblReadMode.TabIndex = 1;
            this.lblReadMode.Text = "Đọc: Tên file";
            this.lblReadMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip.SetToolTip(this.lblReadMode, "Cách đọc tự suy từ schema - không cần chọn chế độ:\r\n  • Không có dấu \\  →  chỉ đọc TÊN FILE, vd  <Model>_<DK>_<SPL>_<Raw/Mask>_<Đánh giá>_<Hậu tố>\r\n  • Có dấu \\  →  đọc ĐƯỜNG DẪN: trước dấu \\ cuối là các cấp thư mục, sau là tên file (nối bằng _)\r\n     vd  <Raw/Mask>\\<SPL>\\<Name>_<Class>_<Đánh giá>_<Lần chụp>\r\n     (tự bật và khoá \"Quét thư mục con\")");
            //
            // btnApplySchema
            //
            this.btnApplySchema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnApplySchema.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplySchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApplySchema.FlatAppearance.BorderSize = 0;
            this.btnApplySchema.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplySchema.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnApplySchema.ForeColor = System.Drawing.Color.White;
            this.btnApplySchema.Location = new System.Drawing.Point(766, 2);
            this.btnApplySchema.Margin = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.btnApplySchema.Name = "btnApplySchema";
            this.btnApplySchema.Size = new System.Drawing.Size(134, 34);
            this.btnApplySchema.TabIndex = 2;
            this.btnApplySchema.Text = "Áp dụng Schema";
            this.btnApplySchema.UseVisualStyleBackColor = false;
            //
            // flpConvert
            //
            this.flpConvert.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpConvert.AutoSize = true;
            this.flpConvert.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpConvert.Controls.Add(this.btnConvert);
            this.flpConvert.Controls.Add(this.btnUndoConvert);
            this.flpConvert.Location = new System.Drawing.Point(908, 1);
            this.flpConvert.Margin = new System.Windows.Forms.Padding(0);
            this.flpConvert.Name = "flpConvert";
            this.flpConvert.Size = new System.Drawing.Size(378, 37);
            this.flpConvert.TabIndex = 3;
            this.flpConvert.WrapContents = false;
            //
            // btnConvert
            //
            this.btnConvert.BackColor = System.Drawing.Color.White;
            this.btnConvert.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConvert.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvert.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnConvert.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnConvert.Location = new System.Drawing.Point(0, 5);
            this.btnConvert.Margin = new System.Windows.Forms.Padding(0, 5, 8, 0);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(200, 32);
            this.btnConvert.TabIndex = 0;
            this.btnConvert.Text = "⇄  Chuyển sang theo tên…";
            this.btnConvert.UseVisualStyleBackColor = false;
            //
            // btnUndoConvert
            //
            this.btnUndoConvert.BackColor = System.Drawing.Color.White;
            this.btnUndoConvert.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUndoConvert.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnUndoConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUndoConvert.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnUndoConvert.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(83)))), ((int)(((byte)(9)))));
            this.btnUndoConvert.Location = new System.Drawing.Point(208, 5);
            this.btnUndoConvert.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnUndoConvert.Name = "btnUndoConvert";
            this.btnUndoConvert.Size = new System.Drawing.Size(170, 32);
            this.btnUndoConvert.TabIndex = 1;
            this.btnUndoConvert.Text = "↶  Hoàn tác đổi tên";
            this.btnUndoConvert.UseVisualStyleBackColor = false;
            //
            // lblSchemaInfo
            //
            this.lblSchemaInfo.AutoEllipsis = true;
            this.lblSchemaInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSchemaInfo.Location = new System.Drawing.Point(3, 66);
            this.lblSchemaInfo.Name = "lblSchemaInfo";
            this.lblSchemaInfo.Size = new System.Drawing.Size(1334, 15);
            this.lblSchemaInfo.TabIndex = 6;
            this.lblSchemaInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlFilterCard
            //
            this.pnlFilterCard.BackColor = System.Drawing.Color.White;
            this.pnlFilterCard.Controls.Add(this.tlpFilterCard);
            this.pnlFilterCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFilterCard.Location = new System.Drawing.Point(0, 192);
            this.pnlFilterCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlFilterCard.Name = "pnlFilterCard";
            this.pnlFilterCard.Padding = new System.Windows.Forms.Padding(14, 6, 18, 4);
            this.pnlFilterCard.Size = new System.Drawing.Size(1376, 120);
            this.pnlFilterCard.TabIndex = 1;
            //
            // tlpFilterCard
            //
            this.tlpFilterCard.ColumnCount = 1;
            this.tlpFilterCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilterCard.Controls.Add(this.flpChipsTitle, 0, 0);
            this.tlpFilterCard.Controls.Add(this.tlpChipsRow, 0, 1);
            this.tlpFilterCard.Controls.Add(this.tlpFilterRow, 0, 2);
            this.tlpFilterCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilterCard.Location = new System.Drawing.Point(14, 6);
            this.tlpFilterCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpFilterCard.Name = "tlpFilterCard";
            this.tlpFilterCard.RowCount = 3;
            this.tlpFilterCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpFilterCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpFilterCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilterCard.Size = new System.Drawing.Size(1344, 110);
            this.tlpFilterCard.TabIndex = 0;
            //
            // flpChipsTitle
            //
            this.flpChipsTitle.Controls.Add(this.btnCollapseFilter);
            this.flpChipsTitle.Controls.Add(this.lblChipsTitle);
            this.flpChipsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpChipsTitle.Location = new System.Drawing.Point(0, 0);
            this.flpChipsTitle.Margin = new System.Windows.Forms.Padding(0);
            this.flpChipsTitle.Name = "flpChipsTitle";
            this.flpChipsTitle.Size = new System.Drawing.Size(1344, 28);
            this.flpChipsTitle.TabIndex = 0;
            this.flpChipsTitle.WrapContents = false;
            //
            // btnCollapseFilter
            //
            this.btnCollapseFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseFilter.FlatAppearance.BorderSize = 0;
            this.btnCollapseFilter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnCollapseFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseFilter.Font = new System.Drawing.Font("Segoe MDL2 Assets", 8F);
            this.btnCollapseFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnCollapseFilter.Location = new System.Drawing.Point(0, 2);
            this.btnCollapseFilter.Margin = new System.Windows.Forms.Padding(0, 2, 6, 0);
            this.btnCollapseFilter.Name = "btnCollapseFilter";
            this.btnCollapseFilter.Size = new System.Drawing.Size(24, 24);
            this.btnCollapseFilter.TabIndex = 0;
            this.btnCollapseFilter.Text = "";
            this.toolTip.SetToolTip(this.btnCollapseFilter, "Thu gọn / mở rộng khối Bộ lọc & trạng thái");
            this.btnCollapseFilter.UseVisualStyleBackColor = true;
            //
            // lblChipsTitle
            //
            this.lblChipsTitle.AutoSize = true;
            this.lblChipsTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblChipsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblChipsTitle.Location = new System.Drawing.Point(30, 5);
            this.lblChipsTitle.Margin = new System.Windows.Forms.Padding(0, 5, 12, 0);
            this.lblChipsTitle.Name = "lblChipsTitle";
            this.lblChipsTitle.Size = new System.Drawing.Size(150, 19);
            this.lblChipsTitle.TabIndex = 0;
            this.lblChipsTitle.Text = "BỘ LỌC && TRẠNG THÁI";
            //
            // tlpChipsRow
            //
            this.tlpChipsRow.ColumnCount = 3;
            this.tlpChipsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChipsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpChipsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpChipsRow.Controls.Add(this.pnlSchemaChips, 0, 0);
            this.tlpChipsRow.Controls.Add(this.btnComponents, 1, 0);
            this.tlpChipsRow.Controls.Add(this.flpRule, 2, 0);
            this.tlpChipsRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpChipsRow.Location = new System.Drawing.Point(0, 28);
            this.tlpChipsRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpChipsRow.Name = "tlpChipsRow";
            this.tlpChipsRow.RowCount = 1;
            this.tlpChipsRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChipsRow.Size = new System.Drawing.Size(1344, 44);
            this.tlpChipsRow.TabIndex = 1;
            //
            // pnlSchemaChips
            //
            this.pnlSchemaChips.BackColor = System.Drawing.Color.White;
            this.pnlSchemaChips.Controls.Add(this.btnSchemaHint);
            this.pnlSchemaChips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSchemaChips.Location = new System.Drawing.Point(0, 4);
            this.pnlSchemaChips.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.pnlSchemaChips.Name = "pnlSchemaChips";
            this.pnlSchemaChips.Size = new System.Drawing.Size(720, 42);
            this.pnlSchemaChips.TabIndex = 0;
            //
            // btnSchemaHint
            //
            this.btnSchemaHint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSchemaHint.FlatAppearance.BorderSize = 0;
            this.btnSchemaHint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSchemaHint.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSchemaHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSchemaHint.Location = new System.Drawing.Point(0, 1);
            this.btnSchemaHint.Margin = new System.Windows.Forms.Padding(0);
            this.btnSchemaHint.Name = "btnSchemaHint";
            this.btnSchemaHint.Size = new System.Drawing.Size(26, 26);
            this.btnSchemaHint.TabIndex = 0;
            this.btnSchemaHint.Text = "ⓘ";
            this.toolTip.SetToolTip(this.btnSchemaHint, "Cách gom hàng + ví dụ tách tên file");
            this.btnSchemaHint.UseVisualStyleBackColor = true;
            //
            // btnComponents
            //
            this.btnComponents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnComponents.AutoSize = true;
            this.btnComponents.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnComponents.BackColor = System.Drawing.Color.White;
            this.btnComponents.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnComponents.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnComponents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnComponents.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnComponents.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnComponents.Location = new System.Drawing.Point(728, 8);
            this.btnComponents.Margin = new System.Windows.Forms.Padding(8, 6, 10, 0);
            this.btnComponents.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnComponents.Name = "btnComponents";
            this.btnComponents.Size = new System.Drawing.Size(112, 30);
            this.btnComponents.TabIndex = 1;
            this.btnComponents.Text = "+ Thành phần  ▾";
            this.toolTip.SetToolTip(this.btnComponents, "Chèn trường có ý nghĩa cố định vào schema - tại con trỏ trong ô schema (chưa đặt con trỏ thì chèn cuối)");
            this.btnComponents.UseVisualStyleBackColor = false;
            //
            // tlpFilterRow
            //
            this.tlpFilterRow.ColumnCount = 1;
            this.tlpFilterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilterRow.Controls.Add(this.flpFilter, 0, 0);
            this.tlpFilterRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilterRow.Location = new System.Drawing.Point(0, 72);
            this.tlpFilterRow.Margin = new System.Windows.Forms.Padding(0);
            this.tlpFilterRow.Name = "tlpFilterRow";
            this.tlpFilterRow.RowCount = 1;
            this.tlpFilterRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilterRow.Size = new System.Drawing.Size(1344, 38);
            this.tlpFilterRow.TabIndex = 3;
            //
            // flpFilter
            //
            this.flpFilter.Controls.Add(this.btnShowConfig);
            this.flpFilter.Controls.Add(this.lblFilterTitle);
            this.flpFilter.Controls.Add(this.flpFind);
            this.flpFilter.Controls.Add(this.flpDk);
            this.flpFilter.Controls.Add(this.flpVision);
            this.flpFilter.Controls.Add(this.flpEval);
            this.flpFilter.Controls.Add(this.chkUnmatchedOnly);
            this.flpFilter.Controls.Add(this.lblResultCount);
            this.flpFilter.Controls.Add(this.btnResetFilters);
            this.flpFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpFilter.Location = new System.Drawing.Point(0, 0);
            this.flpFilter.Margin = new System.Windows.Forms.Padding(0);
            this.flpFilter.Name = "flpFilter";
            this.flpFilter.Size = new System.Drawing.Size(1344, 38);
            this.flpFilter.TabIndex = 0;
            this.flpFilter.WrapContents = false;
            // 
            // btnShowConfig
            // 
            this.btnShowConfig.AutoSize = true;
            this.btnShowConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnShowConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.btnShowConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowConfig.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.btnShowConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowConfig.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnShowConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnShowConfig.Location = new System.Drawing.Point(0, 3);
            this.btnShowConfig.Margin = new System.Windows.Forms.Padding(0, 3, 14, 0);
            this.btnShowConfig.MinimumSize = new System.Drawing.Size(0, 28);
            this.btnShowConfig.Name = "btnShowConfig";
            this.btnShowConfig.Size = new System.Drawing.Size(150, 28);
            this.btnShowConfig.TabIndex = 7;
            this.btnShowConfig.Text = "▸ Hiện Thư mục && Schema";
            this.toolTip.SetToolTip(this.btnShowConfig, "Khu vực Thư mục ảnh & Schema đang ẩn - bấm để hiện lại (hoặc ☰ Hiển thị ở thanh dưới cùng)");
            this.btnShowConfig.UseVisualStyleBackColor = false;
            this.btnShowConfig.Visible = false;
            //
            // lblFilterTitle
            //
            this.lblFilterTitle.AutoSize = true;
            this.lblFilterTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblFilterTitle.Location = new System.Drawing.Point(0, 7);
            this.lblFilterTitle.Margin = new System.Windows.Forms.Padding(6, 9, 14, 0);
            this.lblFilterTitle.Name = "lblFilterTitle";
            this.lblFilterTitle.Size = new System.Drawing.Size(60, 19);
            this.lblFilterTitle.TabIndex = 0;
            this.lblFilterTitle.Text = "Bộ lọc";
            //
            // flpFind
            //
            this.flpFind.AutoSize = true;
            this.flpFind.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpFind.Controls.Add(this.lblFind);
            this.flpFind.Controls.Add(this.txtFilter);
            this.flpFind.Location = new System.Drawing.Point(78, 0);
            this.flpFind.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.flpFind.Name = "flpFind";
            this.flpFind.Size = new System.Drawing.Size(140, 28);
            this.flpFind.TabIndex = 1;
            this.flpFind.WrapContents = false;
            //
            // lblFind
            //
            this.lblFind.AutoSize = true;
            this.lblFind.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblFind.Location = new System.Drawing.Point(0, 9);
            this.lblFind.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.lblFind.Name = "lblFind";
            this.lblFind.Size = new System.Drawing.Size(24, 15);
            this.lblFind.TabIndex = 0;
            this.lblFind.Text = "Tìm";
            //
            // txtFilter
            //
            this.txtFilter.Location = new System.Drawing.Point(30, 5);
            this.txtFilter.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(110, 23);
            this.txtFilter.TabIndex = 1;
            //
            // flpDk
            //
            this.flpDk.AutoSize = true;
            this.flpDk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDk.Controls.Add(this.lblDk);
            this.flpDk.Controls.Add(this.cboDkFilter);
            this.flpDk.Location = new System.Drawing.Point(234, 0);
            this.flpDk.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.flpDk.Name = "flpDk";
            this.flpDk.Size = new System.Drawing.Size(127, 28);
            this.flpDk.TabIndex = 2;
            this.flpDk.WrapContents = false;
            //
            // lblDk
            //
            this.lblDk.AutoSize = true;
            this.lblDk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblDk.Location = new System.Drawing.Point(0, 9);
            this.lblDk.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.lblDk.Name = "lblDk";
            this.lblDk.Size = new System.Drawing.Size(21, 15);
            this.lblDk.TabIndex = 0;
            this.lblDk.Text = "DK";
            //
            // cboDkFilter
            //
            this.cboDkFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDkFilter.Location = new System.Drawing.Point(27, 5);
            this.cboDkFilter.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.cboDkFilter.Name = "cboDkFilter";
            this.cboDkFilter.Size = new System.Drawing.Size(100, 23);
            this.cboDkFilter.TabIndex = 1;
            //
            // flpVision
            //
            this.flpVision.AutoSize = true;
            this.flpVision.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpVision.Controls.Add(this.lblVision);
            this.flpVision.Controls.Add(this.cboUnitXFilter);
            this.flpVision.Location = new System.Drawing.Point(377, 0);
            this.flpVision.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.flpVision.Name = "flpVision";
            this.flpVision.Size = new System.Drawing.Size(140, 28);
            this.flpVision.TabIndex = 3;
            this.flpVision.WrapContents = false;
            //
            // lblVision
            //
            this.lblVision.AutoSize = true;
            this.lblVision.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblVision.Location = new System.Drawing.Point(0, 9);
            this.lblVision.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.lblVision.Name = "lblVision";
            this.lblVision.Size = new System.Drawing.Size(38, 15);
            this.lblVision.TabIndex = 0;
            this.lblVision.Text = "Vision";
            //
            // cboUnitXFilter
            //
            this.cboUnitXFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUnitXFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboUnitXFilter.Location = new System.Drawing.Point(44, 5);
            this.cboUnitXFilter.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.cboUnitXFilter.Name = "cboUnitXFilter";
            this.cboUnitXFilter.Size = new System.Drawing.Size(96, 25);
            this.cboUnitXFilter.TabIndex = 1;
            //
            // flpEval
            //
            this.flpEval.AutoSize = true;
            this.flpEval.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpEval.Controls.Add(this.lblEval);
            this.flpEval.Controls.Add(this.cboEvalFilter);
            this.flpEval.Location = new System.Drawing.Point(533, 0);
            this.flpEval.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.flpEval.Name = "flpEval";
            this.flpEval.Size = new System.Drawing.Size(155, 30);
            this.flpEval.TabIndex = 4;
            this.flpEval.WrapContents = false;
            //
            // lblEval
            //
            this.lblEval.AutoSize = true;
            this.lblEval.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblEval.Location = new System.Drawing.Point(0, 9);
            this.lblEval.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.lblEval.Name = "lblEval";
            this.lblEval.Size = new System.Drawing.Size(53, 15);
            this.lblEval.TabIndex = 0;
            this.lblEval.Text = "Đánh giá";
            //
            // cboEvalFilter
            //
            this.cboEvalFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEvalFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboEvalFilter.Location = new System.Drawing.Point(59, 5);
            this.cboEvalFilter.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.cboEvalFilter.Name = "cboEvalFilter";
            this.cboEvalFilter.Size = new System.Drawing.Size(96, 25);
            this.cboEvalFilter.TabIndex = 1;
            //
            // chkUnmatchedOnly
            //
            this.chkUnmatchedOnly.AutoSize = true;
            this.chkUnmatchedOnly.Location = new System.Drawing.Point(704, 8);
            this.chkUnmatchedOnly.Margin = new System.Windows.Forms.Padding(0, 8, 16, 0);
            this.chkUnmatchedOnly.Name = "chkUnmatchedOnly";
            this.chkUnmatchedOnly.Size = new System.Drawing.Size(126, 19);
            this.chkUnmatchedOnly.TabIndex = 5;
            this.chkUnmatchedOnly.Text = "Chỉ ảnh sai schema";
            this.toolTip.SetToolTip(this.chkUnmatchedOnly, "Chỉ hiện ảnh có tên file không khớp schema (không đưa vào báo cáo)");
            this.chkUnmatchedOnly.UseVisualStyleBackColor = true;
            //
            // lblResultCount
            //
            this.lblResultCount.AutoSize = true;
            this.lblResultCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblResultCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblResultCount.Location = new System.Drawing.Point(854, 9);
            this.lblResultCount.Margin = new System.Windows.Forms.Padding(8, 9, 0, 0);
            this.lblResultCount.Name = "lblResultCount";
            this.lblResultCount.Size = new System.Drawing.Size(0, 17);
            this.lblResultCount.TabIndex = 6;
            //
            // flpRule
            //
            this.flpRule.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRule.AutoSize = true;
            this.flpRule.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRule.Controls.Add(this.lblRuleSep);
            this.flpRule.Controls.Add(this.lblRule);
            this.flpRule.Controls.Add(this.cboCompareMode);
            this.flpRule.Controls.Add(this.chkLock);
            this.flpRule.Location = new System.Drawing.Point(940, 2);
            this.flpRule.Margin = new System.Windows.Forms.Padding(0);
            this.flpRule.Name = "flpRule";
            this.flpRule.Size = new System.Drawing.Size(300, 33);
            this.flpRule.TabIndex = 1;
            this.flpRule.WrapContents = false;
            //
            // lblRuleSep
            //
            this.lblRuleSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.lblRuleSep.Location = new System.Drawing.Point(0, 7);
            this.lblRuleSep.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.lblRuleSep.Name = "lblRuleSep";
            this.lblRuleSep.Size = new System.Drawing.Size(1, 24);
            this.lblRuleSep.TabIndex = 0;
            //
            // lblRule
            //
            this.lblRule.AutoSize = true;
            this.lblRule.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblRule.Location = new System.Drawing.Point(13, 10);
            this.lblRule.Margin = new System.Windows.Forms.Padding(12, 10, 6, 0);
            this.lblRule.Name = "lblRule";
            this.lblRule.Size = new System.Drawing.Size(96, 15);
            this.lblRule.TabIndex = 1;
            this.lblRule.Text = "Quy tắc đánh giá";
            this.toolTip.SetToolTip(this.lblRule, "Quy tắc tính Đánh giá từ Vision (không phải bộ lọc): == giữ nguyên Vision, != lấy ngược (bộ mẫu NG)");
            //
            // cboCompareMode
            //
            this.cboCompareMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCompareMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboCompareMode.Location = new System.Drawing.Point(115, 6);
            this.cboCompareMode.Margin = new System.Windows.Forms.Padding(0, 6, 4, 0);
            this.cboCompareMode.Name = "cboCompareMode";
            this.cboCompareMode.Size = new System.Drawing.Size(150, 25);
            this.cboCompareMode.TabIndex = 2;
            this.toolTip.SetToolTip(this.cboCompareMode, "Logic chung cho các mẫu CHƯA chỉnh gì:\r\n• Đánh giá == Vision: Đánh giá = kết quả Vision (OK → O, NG → X).\r\n• Đánh giá != Vision: Đánh giá = ngược kết quả Vision (NG → O, OK → X).\r\n\r\nMẫu đã chọn Đối soát riêng (== / !=) hoặc đã chỉnh tay Đánh giá\r\n(Đối soát trống) giữ nguyên khi đổi logic.\r\nXoá nhãn (phím 0 / Delete) để mẫu quay về theo logic chung.");
            //
            // chkLock
            //
            this.chkLock.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkLock.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkLock.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.chkLock.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.chkLock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkLock.Location = new System.Drawing.Point(269, 5);
            this.chkLock.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.chkLock.Name = "chkLock";
            this.chkLock.Size = new System.Drawing.Size(34, 28);
            this.chkLock.TabIndex = 3;
            this.chkLock.Text = "🔓";
            this.chkLock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkLock.UseVisualStyleBackColor = true;
            //
            // btnResetFilters
            //
            this.btnResetFilters.AutoSize = true;
            this.btnResetFilters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnResetFilters.BackColor = System.Drawing.Color.White;
            this.btnResetFilters.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResetFilters.Enabled = false;
            this.btnResetFilters.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnResetFilters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetFilters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnResetFilters.Location = new System.Drawing.Point(1255, 5);
            this.btnResetFilters.Margin = new System.Windows.Forms.Padding(14, 3, 0, 0);
            this.btnResetFilters.MinimumSize = new System.Drawing.Size(0, 26);
            this.btnResetFilters.Name = "btnResetFilters";
            this.btnResetFilters.Size = new System.Drawing.Size(85, 27);
            this.btnResetFilters.TabIndex = 2;
            this.btnResetFilters.Text = "✕  Đặt lại";
            this.toolTip.SetToolTip(this.btnResetFilters, "Bỏ mọi bộ lọc: ô Lọc, DK, Vision, Đánh giá, lọc cột ▼, nút đang chọn trên cây");
            this.btnResetFilters.UseVisualStyleBackColor = false;
            //
            // scIssues
            //
            this.scIssues.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scIssues.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.scIssues.Location = new System.Drawing.Point(0, 336);
            this.scIssues.Margin = new System.Windows.Forms.Padding(0);
            this.scIssues.Name = "scIssues";
            this.scIssues.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // scIssues.Panel1
            //
            this.scIssues.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scIssues.Panel1.Controls.Add(this.scPreview);
            //
            // scIssues.Panel2
            //
            this.scIssues.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scIssues.Panel2.Controls.Add(this.scBottom);
            this.scIssues.Size = new System.Drawing.Size(1376, 506);
            this.scIssues.SplitterDistance = 378;
            this.scIssues.SplitterWidth = 10;
            this.scIssues.TabIndex = 2;
            //
            // scPreview
            //
            this.scPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scPreview.Location = new System.Drawing.Point(0, 0);
            this.scPreview.Name = "scPreview";
            //
            // scPreview.Panel1
            //
            this.scPreview.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scPreview.Panel1.Controls.Add(this.scTree);
            //
            // scPreview.Panel2
            //
            this.scPreview.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scPreview.Panel2.Controls.Add(this.pnlPreviewCard);
            this.scPreview.Size = new System.Drawing.Size(1376, 378);
            this.scPreview.SplitterDistance = 908;
            this.scPreview.SplitterWidth = 10;
            this.scPreview.TabIndex = 0;
            //
            // scTree
            //
            this.scTree.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scTree.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scTree.Location = new System.Drawing.Point(0, 0);
            this.scTree.Name = "scTree";
            //
            // scTree.Panel1
            //
            this.scTree.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scTree.Panel1.Controls.Add(this.pnlTreeCard);
            //
            // scTree.Panel2
            //
            this.scTree.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scTree.Panel2.Controls.Add(this.pnlTableCard);
            this.scTree.Size = new System.Drawing.Size(908, 378);
            this.scTree.SplitterDistance = 210;
            this.scTree.SplitterWidth = 10;
            this.scTree.TabIndex = 0;
            //
            // pnlTreeCard
            //
            this.pnlTreeCard.BackColor = System.Drawing.Color.White;
            this.pnlTreeCard.Controls.Add(this.tlpTreeCard);
            this.pnlTreeCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTreeCard.Location = new System.Drawing.Point(0, 0);
            this.pnlTreeCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTreeCard.Name = "pnlTreeCard";
            this.pnlTreeCard.Padding = new System.Windows.Forms.Padding(14, 8, 18, 10);
            this.pnlTreeCard.HideButton = true;
            this.pnlTreeCard.Size = new System.Drawing.Size(240, 378);
            this.pnlTreeCard.TabIndex = 0;
            //
            // tlpTreeCard
            //
            this.tlpTreeCard.ColumnCount = 1;
            this.tlpTreeCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTreeCard.Controls.Add(this.lblTreeTitle, 0, 0);
            this.tlpTreeCard.Controls.Add(this.tvDataset, 0, 1);
            this.tlpTreeCard.Controls.Add(this.pnlTreeSummary, 0, 2);
            this.tlpTreeCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTreeCard.Location = new System.Drawing.Point(14, 8);
            this.tlpTreeCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTreeCard.Name = "tlpTreeCard";
            this.tlpTreeCard.RowCount = 3;
            this.tlpTreeCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTreeCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTreeCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpTreeCard.Size = new System.Drawing.Size(216, 360);
            this.tlpTreeCard.TabIndex = 0;
            //
            // lblTreeTitle
            //
            this.lblTreeTitle.AutoEllipsis = true;
            this.lblTreeTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTreeTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTreeTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblTreeTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTreeTitle.Name = "lblTreeTitle";
            this.lblTreeTitle.Size = new System.Drawing.Size(210, 30);
            this.lblTreeTitle.TabIndex = 0;
            this.lblTreeTitle.Text = "CÂY DATASET";
            this.lblTreeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // tvDataset
            //
            this.tvDataset.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDataset.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.tvDataset.FullRowSelect = true;
            this.tvDataset.HideSelection = false;
            this.tvDataset.ItemHeight = 26;
            this.tvDataset.Location = new System.Drawing.Point(3, 33);
            this.tvDataset.Name = "tvDataset";
            this.tvDataset.ShowLines = false;
            this.tvDataset.ShowNodeToolTips = true;
            this.tvDataset.Size = new System.Drawing.Size(210, 212);
            this.tvDataset.TabIndex = 1;
            //
            // pnlTreeSummary
            //
            this.pnlTreeSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTreeSummary.Location = new System.Drawing.Point(0, 254);
            this.pnlTreeSummary.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.pnlTreeSummary.Name = "pnlTreeSummary";
            this.pnlTreeSummary.Size = new System.Drawing.Size(216, 106);
            this.pnlTreeSummary.TabIndex = 2;
            //
            // pnlTableCard
            //
            this.pnlTableCard.BackColor = System.Drawing.Color.White;
            this.pnlTableCard.Controls.Add(this.tlpTableCard);
            this.pnlTableCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTableCard.Location = new System.Drawing.Point(0, 0);
            this.pnlTableCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTableCard.Name = "pnlTableCard";
            this.pnlTableCard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 10);
            this.pnlTableCard.Size = new System.Drawing.Size(658, 378);
            this.pnlTableCard.TabIndex = 0;
            //
            // tlpTableCard
            //
            this.tlpTableCard.ColumnCount = 1;
            this.tlpTableCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableCard.Controls.Add(this.tlpTableToolbar, 0, 0);
            this.tlpTableCard.Controls.Add(this.grid, 0, 1);
            this.tlpTableCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTableCard.Location = new System.Drawing.Point(16, 8);
            this.tlpTableCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTableCard.Name = "tlpTableCard";
            this.tlpTableCard.RowCount = 2;
            this.tlpTableCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpTableCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableCard.Size = new System.Drawing.Size(626, 360);
            this.tlpTableCard.TabIndex = 0;
            //
            // tlpTableToolbar
            //
            this.tlpTableToolbar.ColumnCount = 3;
            this.tlpTableToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTableToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTableToolbar.Controls.Add(this.lblTableTitle, 0, 0);
            this.tlpTableToolbar.Controls.Add(this.lblRowCount, 1, 0);
            this.tlpTableToolbar.Controls.Add(this.flpTableActions, 2, 0);
            this.tlpTableToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTableToolbar.Location = new System.Drawing.Point(0, 0);
            this.tlpTableToolbar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.tlpTableToolbar.Name = "tlpTableToolbar";
            this.tlpTableToolbar.RowCount = 1;
            this.tlpTableToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableToolbar.Size = new System.Drawing.Size(626, 38);
            this.tlpTableToolbar.TabIndex = 0;
            //
            // lblTableTitle
            //
            this.lblTableTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTableTitle.AutoSize = true;
            this.lblTableTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTableTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblTableTitle.Location = new System.Drawing.Point(0, 9);
            this.lblTableTitle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblTableTitle.Name = "lblTableTitle";
            this.lblTableTitle.Size = new System.Drawing.Size(110, 19);
            this.lblTableTitle.TabIndex = 0;
            this.lblTableTitle.Text = "BẢNG KẾT QUẢ";
            //
            // lblRowCount
            //
            this.lblRowCount.AutoEllipsis = true;
            this.lblRowCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRowCount.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblRowCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblRowCount.Location = new System.Drawing.Point(125, 0);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(290, 38);
            this.lblRowCount.TabIndex = 1;
            this.lblRowCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flpTableActions
            //
            this.flpTableActions.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpTableActions.AutoSize = true;
            this.flpTableActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpTableActions.Controls.Add(this.btnKeys);
            this.flpTableActions.Controls.Add(this.btnExport);
            this.flpTableActions.Location = new System.Drawing.Point(418, 2);
            this.flpTableActions.Margin = new System.Windows.Forms.Padding(0);
            this.flpTableActions.Name = "flpTableActions";
            this.flpTableActions.Size = new System.Drawing.Size(208, 34);
            this.flpTableActions.TabIndex = 2;
            this.flpTableActions.WrapContents = false;
            //
            // btnKeys
            //
            this.btnKeys.AutoSize = true;
            this.btnKeys.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnKeys.BackColor = System.Drawing.Color.White;
            this.btnKeys.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKeys.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnKeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKeys.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnKeys.Location = new System.Drawing.Point(56, 2);
            this.btnKeys.Margin = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.btnKeys.MinimumSize = new System.Drawing.Size(36, 30);
            this.btnKeys.Name = "btnKeys";
            this.btnKeys.Size = new System.Drawing.Size(36, 30);
            this.btnKeys.TabIndex = 1;
            this.btnKeys.Text = "⌨";
            this.toolTip.SetToolTip(this.btnKeys, "Phím tắt");
            this.btnKeys.UseVisualStyleBackColor = false;
            //
            // btnExport
            //
            this.btnExport.AutoSize = true;
            this.btnExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.FlatAppearance.BorderSize = 0;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location = new System.Drawing.Point(98, 2);
            this.btnExport.Margin = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.btnExport.MinimumSize = new System.Drawing.Size(96, 30);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(96, 30);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "⬇  Xuất  ▾";
            this.btnExport.UseVisualStyleBackColor = false;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grid.ColumnHeadersHeight = 30;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(254)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grid.DefaultCellStyle = dataGridViewCellStyle2;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.Location = new System.Drawing.Point(3, 45);
            this.grid.Name = "grid";
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grid.Size = new System.Drawing.Size(620, 312);
            this.grid.TabIndex = 1;
            //
            // pnlPreviewCard
            //
            this.pnlPreviewCard.BackColor = System.Drawing.Color.White;
            this.pnlPreviewCard.Controls.Add(this.tlpPreviewCard);
            this.pnlPreviewCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreviewCard.Location = new System.Drawing.Point(0, 0);
            this.pnlPreviewCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlPreviewCard.Name = "pnlPreviewCard";
            this.pnlPreviewCard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 10);
            this.pnlPreviewCard.Size = new System.Drawing.Size(458, 378);
            this.pnlPreviewCard.TabIndex = 0;
            //
            // tlpPreviewCard
            //
            this.tlpPreviewCard.ColumnCount = 1;
            this.tlpPreviewCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewCard.Controls.Add(this.tlpPreviewTitle, 0, 0);
            this.tlpPreviewCard.Controls.Add(this.lblPreview, 0, 1);
            this.tlpPreviewCard.Controls.Add(this.tlpPreviewMeta, 0, 2);
            this.tlpPreviewCard.Controls.Add(this.picPreview, 0, 3);
            this.tlpPreviewCard.Controls.Add(this.tlpPreviewNav, 0, 4);
            this.tlpPreviewCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewCard.Location = new System.Drawing.Point(16, 8);
            this.tlpPreviewCard.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreviewCard.Name = "tlpPreviewCard";
            this.tlpPreviewCard.RowCount = 5;
            this.tlpPreviewCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpPreviewCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpPreviewCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpPreviewCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPreviewCard.Size = new System.Drawing.Size(426, 360);
            this.tlpPreviewCard.TabIndex = 0;
            //
            // tlpPreviewTitle
            //
            this.tlpPreviewTitle.ColumnCount = 3;
            this.tlpPreviewTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpPreviewTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPreviewTitle.Controls.Add(this.lblPreviewTitle, 0, 0);
            this.tlpPreviewTitle.Controls.Add(this.flpViewMode, 1, 0);
            this.tlpPreviewTitle.Controls.Add(this.btnExpandPreview, 2, 0);
            this.tlpPreviewTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewTitle.Location = new System.Drawing.Point(0, 0);
            this.tlpPreviewTitle.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreviewTitle.Name = "tlpPreviewTitle";
            this.tlpPreviewTitle.RowCount = 1;
            this.tlpPreviewTitle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewTitle.Size = new System.Drawing.Size(426, 30);
            this.tlpPreviewTitle.TabIndex = 0;
            //
            // lblPreviewTitle
            //
            this.lblPreviewTitle.AutoSize = true;
            this.lblPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPreviewTitle.Location = new System.Drawing.Point(0, 4);
            this.lblPreviewTitle.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Size = new System.Drawing.Size(120, 19);
            this.lblPreviewTitle.TabIndex = 0;
            this.lblPreviewTitle.Text = "XEM TRƯỚC ẢNH";
            //
            // flpViewMode
            //
            this.flpViewMode.AutoSize = true;
            this.flpViewMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpViewMode.Controls.Add(this.btnShowRaw);
            this.flpViewMode.Controls.Add(this.btnShowMask);
            this.flpViewMode.Controls.Add(this.btnSplit);
            this.flpViewMode.Location = new System.Drawing.Point(180, 0);
            this.flpViewMode.Margin = new System.Windows.Forms.Padding(0);
            this.flpViewMode.Name = "flpViewMode";
            this.flpViewMode.Size = new System.Drawing.Size(206, 30);
            this.flpViewMode.TabIndex = 1;
            this.flpViewMode.WrapContents = false;
            //
            // btnShowRaw
            //
            this.btnShowRaw.BackColor = System.Drawing.Color.White;
            this.btnShowRaw.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowRaw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnShowRaw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowRaw.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnShowRaw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnShowRaw.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnShowRaw.Name = "btnShowRaw";
            this.btnShowRaw.Size = new System.Drawing.Size(60, 26);
            this.btnShowRaw.TabIndex = 0;
            this.btnShowRaw.Text = "Raw";
            this.toolTip.SetToolTip(this.btnShowRaw, "Xem ảnh Raw (phím Space: đổi Raw ↔ Mask)");
            this.btnShowRaw.UseVisualStyleBackColor = false;
            //
            // btnShowMask
            //
            this.btnShowMask.BackColor = System.Drawing.Color.White;
            this.btnShowMask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowMask.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnShowMask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowMask.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnShowMask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnShowMask.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnShowMask.Name = "btnShowMask";
            this.btnShowMask.Size = new System.Drawing.Size(60, 26);
            this.btnShowMask.TabIndex = 0;
            this.btnShowMask.Text = "Mask";
            this.toolTip.SetToolTip(this.btnShowMask, "Xem ảnh Mask (phím Space: đổi Raw ↔ Mask)");
            this.btnShowMask.UseVisualStyleBackColor = false;
            //
            // btnExpandPreview
            //
            this.btnExpandPreview.BackColor = System.Drawing.Color.White;
            this.btnExpandPreview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExpandPreview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnExpandPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandPreview.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.btnExpandPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnExpandPreview.Margin = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.btnExpandPreview.Name = "btnExpandPreview";
            this.btnExpandPreview.Size = new System.Drawing.Size(34, 26);
            this.btnExpandPreview.TabIndex = 0;
            this.btnExpandPreview.Text = "";
            this.toolTip.SetToolTip(this.btnExpandPreview, "Mở rộng vùng xem ảnh (Esc để thu lại)");
            this.btnExpandPreview.UseVisualStyleBackColor = false;
            //
            // btnSplit
            //
            this.btnSplit.BackColor = System.Drawing.Color.White;
            this.btnSplit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSplit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnSplit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSplit.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSplit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnSplit.Location = new System.Drawing.Point(326, 2);
            this.btnSplit.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(86, 26);
            this.btnSplit.TabIndex = 1;
            this.btnSplit.Text = "Raw | Mask";
            this.toolTip.SetToolTip(this.btnSplit, "Xem song song ảnh Raw và Mask - zoom / kéo mỗi bên độc lập");
            this.btnSplit.UseVisualStyleBackColor = false;
            //
            // lblPreview
            //
            this.lblPreview.AutoEllipsis = true;
            this.lblPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblPreview.Location = new System.Drawing.Point(3, 30);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(420, 24);
            this.lblPreview.TabIndex = 1;
            this.lblPreview.Text = "Chưa chọn ảnh";
            this.lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // tlpPreviewMeta
            //
            this.tlpPreviewMeta.ColumnCount = 3;
            this.tlpPreviewMeta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewMeta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpPreviewMeta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpPreviewMeta.Controls.Add(this.lblPreviewMeta, 0, 0);
            this.tlpPreviewMeta.Controls.Add(this.btnShotPrev, 1, 0);
            this.tlpPreviewMeta.Controls.Add(this.btnShotNext, 2, 0);
            this.tlpPreviewMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewMeta.Location = new System.Drawing.Point(0, 54);
            this.tlpPreviewMeta.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreviewMeta.Name = "tlpPreviewMeta";
            this.tlpPreviewMeta.RowCount = 1;
            this.tlpPreviewMeta.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewMeta.Size = new System.Drawing.Size(426, 26);
            this.tlpPreviewMeta.TabIndex = 2;
            //
            // lblPreviewMeta
            //
            this.lblPreviewMeta.AutoEllipsis = true;
            this.lblPreviewMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewMeta.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblPreviewMeta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPreviewMeta.Location = new System.Drawing.Point(3, 0);
            this.lblPreviewMeta.Name = "lblPreviewMeta";
            this.lblPreviewMeta.Size = new System.Drawing.Size(352, 26);
            this.lblPreviewMeta.TabIndex = 0;
            this.lblPreviewMeta.Text = "Chọn một ô trong bảng để xem ảnh";
            this.lblPreviewMeta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnShotPrev
            //
            this.btnShotPrev.BackColor = System.Drawing.Color.White;
            this.btnShotPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShotPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnShotPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShotPrev.Font = new System.Drawing.Font("Segoe UI Symbol", 8.5F);
            this.btnShotPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnShotPrev.Location = new System.Drawing.Point(362, 2);
            this.btnShotPrev.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnShotPrev.Name = "btnShotPrev";
            this.btnShotPrev.Size = new System.Drawing.Size(30, 26);
            this.btnShotPrev.TabIndex = 1;
            this.btnShotPrev.Text = "◀";
            this.toolTip.SetToolTip(this.btnShotPrev, "Lần chụp trước (phím ,)");
            this.btnShotPrev.UseVisualStyleBackColor = false;
            this.btnShotPrev.Visible = false;
            //
            // btnShotNext
            //
            this.btnShotNext.BackColor = System.Drawing.Color.White;
            this.btnShotNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShotNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnShotNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShotNext.Font = new System.Drawing.Font("Segoe UI Symbol", 8.5F);
            this.btnShotNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnShotNext.Location = new System.Drawing.Point(396, 2);
            this.btnShotNext.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnShotNext.Name = "btnShotNext";
            this.btnShotNext.Size = new System.Drawing.Size(30, 26);
            this.btnShotNext.TabIndex = 2;
            this.btnShotNext.Text = "▶";
            this.toolTip.SetToolTip(this.btnShotNext, "Lần chụp sau (phím .)");
            this.btnShotNext.UseVisualStyleBackColor = false;
            this.btnShotNext.Visible = false;
            //
            // picPreview
            //
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 84);
            this.picPreview.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(426, 232);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 3;
            this.picPreview.TabStop = false;
            //
            // tlpPreviewNav
            //
            this.tlpPreviewNav.ColumnCount = 8;
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpPreviewNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpPreviewNav.Controls.Add(this.btnPrevRow, 0, 0);
            this.tlpPreviewNav.Controls.Add(this.lblNavPos, 1, 0);
            this.tlpPreviewNav.Controls.Add(this.btnNextRow, 2, 0);
            this.tlpPreviewNav.Controls.Add(this.btnZoomOut, 4, 0);
            this.tlpPreviewNav.Controls.Add(this.cboZoom, 5, 0);
            this.tlpPreviewNav.Controls.Add(this.btnZoomIn, 6, 0);
            this.tlpPreviewNav.Controls.Add(this.btnZoomFit, 7, 0);
            this.tlpPreviewNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPreviewNav.Location = new System.Drawing.Point(0, 320);
            this.tlpPreviewNav.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPreviewNav.Name = "tlpPreviewNav";
            this.tlpPreviewNav.RowCount = 1;
            this.tlpPreviewNav.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPreviewNav.Size = new System.Drawing.Size(426, 40);
            this.tlpPreviewNav.TabIndex = 4;
            //
            // btnPrevRow
            //
            this.btnPrevRow.BackColor = System.Drawing.Color.White;
            this.btnPrevRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevRow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnPrevRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnPrevRow.Location = new System.Drawing.Point(0, 6);
            this.btnPrevRow.Margin = new System.Windows.Forms.Padding(0, 6, 0, 2);
            this.btnPrevRow.Name = "btnPrevRow";
            this.btnPrevRow.Size = new System.Drawing.Size(92, 32);
            this.btnPrevRow.TabIndex = 0;
            this.btnPrevRow.Text = "←  Trước";
            this.toolTip.SetToolTip(this.btnPrevRow, "Hàng trước trong bảng (↑)");
            this.btnPrevRow.UseVisualStyleBackColor = false;
            //
            // lblNavPos
            //
            this.lblNavPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNavPos.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNavPos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblNavPos.Location = new System.Drawing.Point(103, 0);
            this.lblNavPos.Name = "lblNavPos";
            this.lblNavPos.Size = new System.Drawing.Size(220, 40);
            this.lblNavPos.TabIndex = 1;
            this.lblNavPos.Text = "0 / 0";
            this.lblNavPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnNextRow
            //
            this.btnNextRow.BackColor = System.Drawing.Color.White;
            this.btnNextRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextRow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnNextRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnNextRow.Location = new System.Drawing.Point(326, 6);
            this.btnNextRow.Margin = new System.Windows.Forms.Padding(0, 6, 0, 2);
            this.btnNextRow.Name = "btnNextRow";
            this.btnNextRow.Size = new System.Drawing.Size(92, 32);
            this.btnNextRow.TabIndex = 2;
            this.btnNextRow.Text = "Sau  →";
            this.toolTip.SetToolTip(this.btnNextRow, "Hàng sau trong bảng (↓)");
            this.btnNextRow.UseVisualStyleBackColor = false;
            //
            // btnZoomOut
            //
            this.btnZoomOut.BackColor = System.Drawing.Color.White;
            this.btnZoomOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnZoomOut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(0, 6, 4, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(32, 32);
            this.btnZoomOut.TabIndex = 0;
            this.btnZoomOut.Text = "−";
            this.toolTip.SetToolTip(this.btnZoomOut, "Thu nhỏ (cuộn chuột trên ảnh)");
            this.btnZoomOut.UseVisualStyleBackColor = false;
            //
            // cboZoom
            //
            this.cboZoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboZoom.Margin = new System.Windows.Forms.Padding(0, 10, 4, 0);
            this.cboZoom.Name = "cboZoom";
            this.cboZoom.Size = new System.Drawing.Size(72, 23);
            this.cboZoom.TabIndex = 4;
            this.toolTip.SetToolTip(this.cboZoom, "Mức phóng ảnh - chọn hoặc gõ % rồi Enter");
            //
            // btnZoomIn
            //
            this.btnZoomIn.BackColor = System.Drawing.Color.White;
            this.btnZoomIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomIn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnZoomIn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(0, 6, 4, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(32, 32);
            this.btnZoomIn.TabIndex = 0;
            this.btnZoomIn.Text = "+";
            this.toolTip.SetToolTip(this.btnZoomIn, "Phóng to (cuộn chuột trên ảnh)");
            this.btnZoomIn.UseVisualStyleBackColor = false;
            //
            // btnZoomFit
            //
            this.btnZoomFit.BackColor = System.Drawing.Color.White;
            this.btnZoomFit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomFit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnZoomFit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomFit.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnZoomFit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnZoomFit.Margin = new System.Windows.Forms.Padding(0, 6, 0, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(52, 32);
            this.btnZoomFit.TabIndex = 0;
            this.btnZoomFit.Text = "Fit";
            this.toolTip.SetToolTip(this.btnZoomFit, "Vừa khung (double-click ảnh)");
            this.btnZoomFit.UseVisualStyleBackColor = false;
            //
            // scBottom
            //
            this.scBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scBottom.Location = new System.Drawing.Point(0, 0);
            this.scBottom.Name = "scBottom";
            //
            // scBottom.Panel1
            //
            this.scBottom.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scBottom.Panel1.Controls.Add(this.pnlIssuesCard);
            //
            // scBottom.Panel2
            //
            this.scBottom.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.scBottom.Panel2.Controls.Add(this.pnlStatsCard);
            this.scBottom.Size = new System.Drawing.Size(1376, 118);
            this.scBottom.SplitterDistance = 619;
            this.scBottom.SplitterWidth = 10;
            this.scBottom.TabIndex = 0;
            //
            // pnlIssuesCard
            //
            this.pnlIssuesCard.BackColor = System.Drawing.Color.White;
            this.pnlIssuesCard.Controls.Add(this.tlpIssues);
            this.pnlIssuesCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIssuesCard.Location = new System.Drawing.Point(0, 0);
            this.pnlIssuesCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlIssuesCard.Name = "pnlIssuesCard";
            this.pnlIssuesCard.Padding = new System.Windows.Forms.Padding(18, 8, 18, 8);
            this.pnlIssuesCard.HideButton = true;
            this.pnlIssuesCard.Size = new System.Drawing.Size(619, 118);
            this.pnlIssuesCard.TabIndex = 0;
            //
            // tlpIssues
            //
            this.tlpIssues.ColumnCount = 4;
            this.tlpIssues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIssues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpIssues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tlpIssues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpIssues.Controls.Add(this.lblIssuesTitle, 0, 0);
            this.tlpIssues.Controls.Add(this.lblIssueKind, 1, 0);
            this.tlpIssues.Controls.Add(this.cboIssueKind, 2, 0);
            this.tlpIssues.Controls.Add(this.btnExportIssues, 3, 0);
            this.tlpIssues.Controls.Add(this.lvIssues, 0, 1);
            this.tlpIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpIssues.Location = new System.Drawing.Point(18, 8);
            this.tlpIssues.Margin = new System.Windows.Forms.Padding(0);
            this.tlpIssues.Name = "tlpIssues";
            this.tlpIssues.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.tlpIssues.RowCount = 2;
            this.tlpIssues.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpIssues.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIssues.Size = new System.Drawing.Size(583, 102);
            this.tlpIssues.TabIndex = 0;
            //
            // lblIssuesTitle
            //
            this.lblIssuesTitle.AutoEllipsis = true;
            this.lblIssuesTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIssuesTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblIssuesTitle.Location = new System.Drawing.Point(3, 4);
            this.lblIssuesTitle.Name = "lblIssuesTitle";
            this.lblIssuesTitle.Size = new System.Drawing.Size(223, 30);
            this.lblIssuesTitle.TabIndex = 0;
            this.lblIssuesTitle.Text = "⚠ Vấn đề dữ liệu";
            this.lblIssuesTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblIssueKind
            //
            this.lblIssueKind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIssueKind.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblIssueKind.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblIssueKind.Location = new System.Drawing.Point(232, 4);
            this.lblIssueKind.Name = "lblIssueKind";
            this.lblIssueKind.Size = new System.Drawing.Size(38, 30);
            this.lblIssueKind.TabIndex = 1;
            this.lblIssueKind.Text = "Loại";
            this.lblIssueKind.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cboIssueKind
            //
            this.cboIssueKind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboIssueKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIssueKind.Location = new System.Drawing.Point(273, 7);
            this.cboIssueKind.Margin = new System.Windows.Forms.Padding(0, 3, 8, 0);
            this.cboIssueKind.Name = "cboIssueKind";
            this.cboIssueKind.Size = new System.Drawing.Size(182, 23);
            this.cboIssueKind.TabIndex = 2;
            //
            // btnExportIssues
            //
            this.btnExportIssues.BackColor = System.Drawing.Color.White;
            this.btnExportIssues.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportIssues.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnExportIssues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportIssues.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportIssues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.btnExportIssues.Location = new System.Drawing.Point(463, 5);
            this.btnExportIssues.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnExportIssues.Name = "btnExportIssues";
            this.btnExportIssues.Size = new System.Drawing.Size(120, 28);
            this.btnExportIssues.TabIndex = 3;
            this.btnExportIssues.Text = "Xuất danh sách";
            this.btnExportIssues.UseVisualStyleBackColor = false;
            //
            // lvIssues
            //
            this.lvIssues.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvIssues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chIssueKind,
            this.chIssueLocation,
            this.chIssueFile,
            this.chIssueDetail});
            this.tlpIssues.SetColumnSpan(this.lvIssues, 4);
            this.lvIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvIssues.FullRowSelect = true;
            this.lvIssues.HideSelection = false;
            this.lvIssues.Location = new System.Drawing.Point(3, 37);
            this.lvIssues.MultiSelect = false;
            this.lvIssues.Name = "lvIssues";
            this.lvIssues.Size = new System.Drawing.Size(577, 62);
            this.lvIssues.TabIndex = 4;
            this.toolTip.SetToolTip(this.lvIssues, "Double-click (hoặc Enter) để tới ảnh / mẫu trong bảng");
            this.lvIssues.UseCompatibleStateImageBehavior = false;
            this.lvIssues.View = System.Windows.Forms.View.Details;
            this.lvIssues.VirtualMode = true;
            //
            // chIssueKind
            //
            this.chIssueKind.Text = "Loại";
            this.chIssueKind.Width = 110;
            //
            // chIssueLocation
            //
            this.chIssueLocation.Text = "Vị trí";
            this.chIssueLocation.Width = 190;
            //
            // chIssueFile
            //
            this.chIssueFile.Text = "File";
            this.chIssueFile.Width = 300;
            //
            // chIssueDetail
            //
            this.chIssueDetail.Text = "Mô tả";
            this.chIssueDetail.Width = 420;
            //
            // pnlStatsCard
            //
            this.pnlStatsCard.BackColor = System.Drawing.Color.White;
            this.pnlStatsCard.Controls.Add(this.tlpStats);
            this.pnlStatsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatsCard.Location = new System.Drawing.Point(0, 0);
            this.pnlStatsCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStatsCard.Name = "pnlStatsCard";
            this.pnlStatsCard.Padding = new System.Windows.Forms.Padding(18, 8, 18, 8);
            this.pnlStatsCard.HideButton = true;
            this.pnlStatsCard.Size = new System.Drawing.Size(747, 118);
            this.pnlStatsCard.TabIndex = 0;
            //
            // tlpStats
            //
            this.tlpStats.ColumnCount = 3;
            this.tlpStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 470F));
            this.tlpStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpStats.Controls.Add(this.flpStatsLeft, 0, 0);
            this.tlpStats.Controls.Add(this.pnlStatCards, 1, 0);
            this.tlpStats.Controls.Add(this.chartStats, 2, 0);
            this.tlpStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpStats.Location = new System.Drawing.Point(18, 8);
            this.tlpStats.Margin = new System.Windows.Forms.Padding(0);
            this.tlpStats.Name = "tlpStats";
            this.tlpStats.RowCount = 1;
            this.tlpStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpStats.Size = new System.Drawing.Size(717, 102);
            this.tlpStats.TabIndex = 0;
            //
            // flpStatsLeft
            //
            this.flpStatsLeft.Controls.Add(this.lblStatsTitle);
            this.flpStatsLeft.Controls.Add(this.lblStatsSub);
            this.flpStatsLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpStatsLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpStatsLeft.Location = new System.Drawing.Point(0, 0);
            this.flpStatsLeft.Margin = new System.Windows.Forms.Padding(0);
            this.flpStatsLeft.Name = "flpStatsLeft";
            this.flpStatsLeft.Size = new System.Drawing.Size(150, 102);
            this.flpStatsLeft.TabIndex = 0;
            this.flpStatsLeft.WrapContents = false;
            //
            // lblStatsTitle
            //
            this.lblStatsTitle.AutoSize = true;
            this.lblStatsTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblStatsTitle.Location = new System.Drawing.Point(0, 4);
            this.lblStatsTitle.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.lblStatsTitle.Name = "lblStatsTitle";
            this.lblStatsTitle.Size = new System.Drawing.Size(74, 19);
            this.lblStatsTitle.TabIndex = 0;
            this.lblStatsTitle.Text = "THỐNG KÊ";
            //
            // lblStatsSub
            //
            this.lblStatsSub.AutoSize = true;
            this.lblStatsSub.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblStatsSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblStatsSub.Location = new System.Drawing.Point(0, 29);
            this.lblStatsSub.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblStatsSub.Name = "lblStatsSub";
            this.lblStatsSub.Size = new System.Drawing.Size(0, 15);
            this.lblStatsSub.TabIndex = 1;
            //
            // pnlStatCards
            //
            this.pnlStatCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatCards.Location = new System.Drawing.Point(150, 4);
            this.pnlStatCards.Margin = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.pnlStatCards.Name = "pnlStatCards";
            this.pnlStatCards.Size = new System.Drawing.Size(462, 98);
            this.pnlStatCards.TabIndex = 1;
            this.toolTip.SetToolTip(this.pnlStatCards, "Số mẫu theo Đánh giá trong các hàng đang hiện (sau lọc). Đối soát != không phải lỗi nên không có thẻ \"Mismatch\".");
            //
            // chartStats
            //
            this.chartStats.AntiAliasing = System.Windows.Forms.DataVisualization.Charting.AntiAliasingStyles.All;
            this.chartStats.BackColor = System.Drawing.Color.White;
            this.chartStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartStats.Location = new System.Drawing.Point(620, 0);
            this.chartStats.Margin = new System.Windows.Forms.Padding(0);
            this.chartStats.Name = "chartStats";
            this.chartStats.Size = new System.Drawing.Size(97, 102);
            this.chartStats.TabIndex = 2;
            this.toolTip.SetToolTip(this.chartStats, "Cột sọc = Vision (kết quả máy), cột đặc = Đánh giá (nhãn cuối) - theo O / Δ / X / Chưa có");
            //
            // pnlStatusCard
            //
            this.pnlStatusCard.BackColor = System.Drawing.Color.White;
            this.pnlStatusCard.Controls.Add(this.tlpStatus);
            this.pnlStatusCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatusCard.Location = new System.Drawing.Point(0, 850);
            this.pnlStatusCard.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStatusCard.Name = "pnlStatusCard";
            this.pnlStatusCard.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pnlStatusCard.Size = new System.Drawing.Size(1376, 32);
            this.pnlStatusCard.TabIndex = 3;
            //
            // tlpStatus
            //
            this.tlpStatus.ColumnCount = 1;
            this.tlpStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpStatus.Controls.Add(this.statusBar, 0, 0);
            this.tlpStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpStatus.Location = new System.Drawing.Point(10, 2);
            this.tlpStatus.Margin = new System.Windows.Forms.Padding(0);
            this.tlpStatus.Name = "tlpStatus";
            this.tlpStatus.RowCount = 1;
            this.tlpStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpStatus.Size = new System.Drawing.Size(1356, 36);
            this.tlpStatus.TabIndex = 0;
            //
            // statusBar
            //
            this.statusBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBar.BackColor = System.Drawing.Color.White;
            this.statusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.statusBar.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stProgress,
            this.stMessage,
            this.stImagesCaption,
            this.stImages,
            this.stRawCaption,
            this.stRaw,
            this.stMaskCaption,
            this.stMask,
            this.stSchema,
            this.stIssues,
            this.stSelection,
            this.stSpringLeft,
            this.stEvalCaption,
            this.stO,
            this.stDelta,
            this.stX,
            this.stNone,
            this.stSpringRight});
            this.statusBar.Location = new System.Drawing.Point(0, 4);
            this.statusBar.Margin = new System.Windows.Forms.Padding(0);
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(0);
            this.statusBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusBar.ShowItemToolTips = true;
            this.statusBar.Size = new System.Drawing.Size(1356, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 0;
            //
            // stProgress
            //
            this.stProgress.Margin = new System.Windows.Forms.Padding(0, 5, 10, 4);
            this.stProgress.Name = "stProgress";
            this.stProgress.Size = new System.Drawing.Size(220, 15);
            this.stProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.stProgress.Visible = false;
            //
            // stMessage
            //
            this.stMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stMessage.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stMessage.Name = "stMessage";
            this.stMessage.Size = new System.Drawing.Size(240, 19);
            this.stMessage.Spring = true;
            this.stMessage.Text = "Chọn thư mục ảnh rồi bấm \'Quét thư mục\'.";
            this.stMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // stImagesCaption
            //
            this.stImagesCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stImagesCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 2);
            this.stImagesCaption.Name = "stImagesCaption";
            this.stImagesCaption.Size = new System.Drawing.Size(31, 19);
            this.stImagesCaption.Text = "Ảnh:";
            //
            // stImages
            //
            this.stImages.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stImages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.stImages.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stImages.Name = "stImages";
            this.stImages.Size = new System.Drawing.Size(16, 19);
            this.stImages.Text = "0";
            this.stImages.ToolTipText = "Ảnh đang hiện trong bảng / tổng số ảnh trong thư mục";
            //
            // stRawCaption
            //
            this.stRawCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stRawCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 2);
            this.stRawCaption.Name = "stRawCaption";
            this.stRawCaption.Size = new System.Drawing.Size(33, 19);
            this.stRawCaption.Text = "Raw:";
            //
            // stRaw
            //
            this.stRaw.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stRaw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.stRaw.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stRaw.Name = "stRaw";
            this.stRaw.Size = new System.Drawing.Size(16, 19);
            this.stRaw.Text = "0";
            this.stRaw.ToolTipText = "Số ảnh Raw khớp schema";
            //
            // stMaskCaption
            //
            this.stMaskCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stMaskCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 2);
            this.stMaskCaption.Name = "stMaskCaption";
            this.stMaskCaption.Size = new System.Drawing.Size(38, 19);
            this.stMaskCaption.Text = "Mask:";
            //
            // stMask
            //
            this.stMask.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stMask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.stMask.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stMask.Name = "stMask";
            this.stMask.Size = new System.Drawing.Size(16, 19);
            this.stMask.Text = "0";
            this.stMask.ToolTipText = "Số ảnh Mask khớp schema";
            //
            // stSchema
            //
            this.stSchema.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stSchema.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stSchema.Name = "stSchema";
            this.stSchema.Size = new System.Drawing.Size(0, 19);
            this.stSchema.ToolTipText = "Ảnh có tên file không khớp schema - không đưa vào báo cáo Excel";
            //
            // stIssues
            //
            this.stIssues.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stIssues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stIssues.Margin = new System.Windows.Forms.Padding(8, 3, 6, 2);
            this.stIssues.Name = "stIssues";
            this.stIssues.Size = new System.Drawing.Size(0, 19);
            this.stIssues.ToolTipText = "Số vấn đề của bộ dữ liệu (thiếu / thừa ảnh, sai tên, chưa đánh giá...) - bấm để mở danh sách";
            //
            // stSelection
            //
            this.stSelection.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stSelection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.stSelection.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stSelection.Name = "stSelection";
            this.stSelection.Size = new System.Drawing.Size(0, 19);
            this.stSelection.ToolTipText = "Số hàng đang chọn - phím O / D / X, Ctrl+M áp dụng cho tất cả ô đang chọn";
            //
            // stSpringLeft
            //
            this.stSpringLeft.Name = "stSpringLeft";
            this.stSpringLeft.Size = new System.Drawing.Size(200, 19);
            this.stSpringLeft.Spring = true;
            //
            // stEvalCaption
            //
            this.stEvalCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.stEvalCaption.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stEvalCaption.Name = "stEvalCaption";
            this.stEvalCaption.Size = new System.Drawing.Size(60, 19);
            this.stEvalCaption.Text = "Đánh giá:";
            this.stEvalCaption.ToolTipText = "Nhãn Đánh giá theo mẫu (Raw + Mask cùng vị trí), lấy mức xấu nhất";
            //
            // stO
            //
            this.stO.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stO.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.stO.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stO.Name = "stO";
            this.stO.Size = new System.Drawing.Size(30, 19);
            this.stO.Text = "O: 0";
            this.stO.ToolTipText = "O - đạt";
            //
            // stDelta
            //
            this.stDelta.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stDelta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.stDelta.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stDelta.Name = "stDelta";
            this.stDelta.Size = new System.Drawing.Size(30, 19);
            this.stDelta.Text = "Δ: 0";
            this.stDelta.ToolTipText = "Δ - giới hạn";
            //
            // stX
            //
            this.stX.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.stX.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stX.Name = "stX";
            this.stX.Size = new System.Drawing.Size(30, 19);
            this.stX.Text = "X: 0";
            this.stX.ToolTipText = "X - không đạt";
            //
            // stNone
            //
            this.stNone.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.stNone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.stNone.Margin = new System.Windows.Forms.Padding(0, 3, 6, 2);
            this.stNone.Name = "stNone";
            this.stNone.Size = new System.Drawing.Size(70, 19);
            this.stNone.Text = "Chưa có: 0";
            this.stNone.ToolTipText = "Chưa có nhãn";
            //
            // stSpringRight
            //
            this.stSpringRight.Name = "stSpringRight";
            this.stSpringRight.Size = new System.Drawing.Size(200, 19);
            this.stSpringRight.Spring = true;
            //
            // toolTip
            //
            this.toolTip.AutoPopDelay = 20000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            //
            // ucDatasetManager
            //
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ucDatasetManager";
            this.Padding = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.Size = new System.Drawing.Size(1400, 900);
            this.tlpRoot.ResumeLayout(false);
            this.pnlLoadCard.ResumeLayout(false);
            this.tlpLoadCard.ResumeLayout(false);
            this.tlpLoadHeader.ResumeLayout(false);
            this.tlpLoadHeader.PerformLayout();
            this.pnlSchemaCard.ResumeLayout(false);
            this.tlpSchemaCard.ResumeLayout(false);
            this.tlpSchemaCard.PerformLayout();
            this.tlpFolderRow.ResumeLayout(false);
            this.pnlFolderBox.ResumeLayout(false);
            this.pnlFolderBox.PerformLayout();
            this.flpSubfolders.ResumeLayout(false);
            this.flpSubfolders.PerformLayout();
            this.tlpSchemaTitle.ResumeLayout(false);
            this.tlpSchemaTitle.PerformLayout();
            this.tlpSchemaRow.ResumeLayout(false);
            this.tlpSchemaRow.PerformLayout();
            this.pnlSchemaBox.ResumeLayout(false);
            this.pnlSchemaBox.PerformLayout();
            this.flpConvert.ResumeLayout(false);
            this.pnlFilterCard.ResumeLayout(false);
            this.tlpFilterCard.ResumeLayout(false);
            this.flpChipsTitle.ResumeLayout(false);
            this.flpChipsTitle.PerformLayout();
            this.tlpChipsRow.ResumeLayout(false);
            this.tlpChipsRow.PerformLayout();
            this.pnlSchemaChips.ResumeLayout(false);
            this.tlpFilterRow.ResumeLayout(false);
            this.tlpFilterRow.PerformLayout();
            this.flpFilter.ResumeLayout(false);
            this.flpFilter.PerformLayout();
            this.flpFind.ResumeLayout(false);
            this.flpFind.PerformLayout();
            this.flpDk.ResumeLayout(false);
            this.flpDk.PerformLayout();
            this.flpVision.ResumeLayout(false);
            this.flpVision.PerformLayout();
            this.flpEval.ResumeLayout(false);
            this.flpEval.PerformLayout();
            this.flpRule.ResumeLayout(false);
            this.flpRule.PerformLayout();
            this.scIssues.Panel1.ResumeLayout(false);
            this.scIssues.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scIssues)).EndInit();
            this.scIssues.ResumeLayout(false);
            this.scPreview.Panel1.ResumeLayout(false);
            this.scPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scPreview)).EndInit();
            this.scPreview.ResumeLayout(false);
            this.scTree.Panel1.ResumeLayout(false);
            this.scTree.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTree)).EndInit();
            this.scTree.ResumeLayout(false);
            this.pnlTreeCard.ResumeLayout(false);
            this.tlpTreeCard.ResumeLayout(false);
            this.pnlTableCard.ResumeLayout(false);
            this.tlpTableCard.ResumeLayout(false);
            this.tlpTableToolbar.ResumeLayout(false);
            this.tlpTableToolbar.PerformLayout();
            this.flpTableActions.ResumeLayout(false);
            this.flpTableActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.pnlPreviewCard.ResumeLayout(false);
            this.tlpPreviewCard.ResumeLayout(false);
            this.tlpPreviewTitle.ResumeLayout(false);
            this.tlpPreviewTitle.PerformLayout();
            this.flpViewMode.ResumeLayout(false);
            this.tlpPreviewMeta.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tlpPreviewNav.ResumeLayout(false);
            this.scBottom.Panel1.ResumeLayout(false);
            this.scBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).EndInit();
            this.scBottom.ResumeLayout(false);
            this.pnlIssuesCard.ResumeLayout(false);
            this.tlpIssues.ResumeLayout(false);
            this.pnlStatsCard.ResumeLayout(false);
            this.tlpStats.ResumeLayout(false);
            this.flpStatsLeft.ResumeLayout(false);
            this.flpStatsLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartStats)).EndInit();
            this.pnlStatusCard.ResumeLayout(false);
            this.tlpStatus.ResumeLayout(false);
            this.tlpStatus.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private VisionImageTool.CardPanel pnlLoadCard;
        private System.Windows.Forms.TableLayoutPanel tlpLoadCard;
        private System.Windows.Forms.TableLayoutPanel tlpLoadHeader;
        private System.Windows.Forms.Button btnCollapseLoad;
        private System.Windows.Forms.Button btnCollapseSchema;
        private System.Windows.Forms.Button btnCollapseFilter;
        private VisionImageTool.CardPanel pnlSchemaCard;
        private System.Windows.Forms.TableLayoutPanel tlpSchemaCard;
        private System.Windows.Forms.Label lblLoadTitle;
        private System.Windows.Forms.TableLayoutPanel tlpFolderRow;
        private VisionImageTool.CardPanel pnlFolderBox;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label lblFolderCaption;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.FlowLayoutPanel flpSubfolders;
        private System.Windows.Forms.CheckBox chkSubfolders;
        private System.Windows.Forms.Label lblSubfoldersNote;
        private System.Windows.Forms.TableLayoutPanel tlpSchemaTitle;
        private System.Windows.Forms.Label lblSchemaTitle;
        private System.Windows.Forms.Label lblSchemaCaption;
        private System.Windows.Forms.Label lblConvertState;
        private System.Windows.Forms.TableLayoutPanel tlpSchemaRow;
        private VisionImageTool.CardPanel pnlSchemaBox;
        private System.Windows.Forms.TextBox txtSchema;
        private System.Windows.Forms.Button btnRecentSchemas;
        private System.Windows.Forms.Label lblReadMode;
        private System.Windows.Forms.Button btnApplySchema;
        private System.Windows.Forms.FlowLayoutPanel flpConvert;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnUndoConvert;
        private System.Windows.Forms.Label lblSchemaInfo;
        private VisionImageTool.CardPanel pnlFilterCard;
        private System.Windows.Forms.TableLayoutPanel tlpFilterCard;
        private System.Windows.Forms.FlowLayoutPanel flpChipsTitle;
        private System.Windows.Forms.Label lblChipsTitle;
        private System.Windows.Forms.TableLayoutPanel tlpChipsRow;
        private VisionImageTool.SchemaChipsPanel pnlSchemaChips;
        private System.Windows.Forms.Button btnSchemaHint;
        private System.Windows.Forms.Button btnComponents;
        private System.Windows.Forms.TableLayoutPanel tlpFilterRow;
        private System.Windows.Forms.FlowLayoutPanel flpFilter;
        private System.Windows.Forms.Label lblFilterTitle;
        private System.Windows.Forms.Button btnShowConfig;
        private System.Windows.Forms.FlowLayoutPanel flpFind;
        private System.Windows.Forms.Label lblFind;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.FlowLayoutPanel flpDk;
        private System.Windows.Forms.Label lblDk;
        private System.Windows.Forms.ComboBox cboDkFilter;
        private System.Windows.Forms.FlowLayoutPanel flpVision;
        private System.Windows.Forms.Label lblVision;
        private System.Windows.Forms.ComboBox cboUnitXFilter;
        private System.Windows.Forms.FlowLayoutPanel flpEval;
        private System.Windows.Forms.Label lblEval;
        private System.Windows.Forms.ComboBox cboEvalFilter;
        private System.Windows.Forms.CheckBox chkUnmatchedOnly;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.FlowLayoutPanel flpRule;
        private System.Windows.Forms.Label lblRuleSep;
        private System.Windows.Forms.Label lblRule;
        private System.Windows.Forms.ComboBox cboCompareMode;
        private System.Windows.Forms.CheckBox chkLock;
        private System.Windows.Forms.Button btnResetFilters;
        private System.Windows.Forms.SplitContainer scIssues;
        private System.Windows.Forms.SplitContainer scPreview;
        private System.Windows.Forms.SplitContainer scTree;
        private VisionImageTool.CardPanel pnlTreeCard;
        private System.Windows.Forms.TableLayoutPanel tlpTreeCard;
        private System.Windows.Forms.Label lblTreeTitle;
        private System.Windows.Forms.TreeView tvDataset;
        private System.Windows.Forms.Panel pnlTreeSummary;
        private VisionImageTool.CardPanel pnlTableCard;
        private System.Windows.Forms.TableLayoutPanel tlpTableCard;
        private System.Windows.Forms.TableLayoutPanel tlpTableToolbar;
        private System.Windows.Forms.Label lblTableTitle;
        private System.Windows.Forms.Label lblRowCount;
        private System.Windows.Forms.FlowLayoutPanel flpTableActions;
        private System.Windows.Forms.Button btnKeys;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridView grid;
        private VisionImageTool.CardPanel pnlPreviewCard;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewCard;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewTitle;
        private System.Windows.Forms.Label lblPreviewTitle;
        private System.Windows.Forms.FlowLayoutPanel flpViewMode;
        private System.Windows.Forms.Button btnShowRaw;
        private System.Windows.Forms.Button btnShowMask;
        private System.Windows.Forms.Button btnSplit;
        private System.Windows.Forms.Button btnExpandPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewMeta;
        private System.Windows.Forms.Label lblPreviewMeta;
        private System.Windows.Forms.Button btnShotPrev;
        private System.Windows.Forms.Button btnShotNext;
        private VisionImageTool.ZoomPictureBox picPreview;
        private System.Windows.Forms.TableLayoutPanel tlpPreviewNav;
        private System.Windows.Forms.Button btnPrevRow;
        private System.Windows.Forms.Label lblNavPos;
        private System.Windows.Forms.Button btnNextRow;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.ComboBox cboZoom;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.SplitContainer scBottom;
        private VisionImageTool.CardPanel pnlIssuesCard;
        private System.Windows.Forms.TableLayoutPanel tlpIssues;
        private System.Windows.Forms.Label lblIssuesTitle;
        private System.Windows.Forms.Label lblIssueKind;
        private System.Windows.Forms.ComboBox cboIssueKind;
        private System.Windows.Forms.Button btnExportIssues;
        private System.Windows.Forms.ListView lvIssues;
        private System.Windows.Forms.ColumnHeader chIssueKind;
        private System.Windows.Forms.ColumnHeader chIssueLocation;
        private System.Windows.Forms.ColumnHeader chIssueFile;
        private System.Windows.Forms.ColumnHeader chIssueDetail;
        private VisionImageTool.CardPanel pnlStatsCard;
        private System.Windows.Forms.TableLayoutPanel tlpStats;
        private System.Windows.Forms.FlowLayoutPanel flpStatsLeft;
        private System.Windows.Forms.Label lblStatsTitle;
        private System.Windows.Forms.Label lblStatsSub;
        private System.Windows.Forms.Panel pnlStatCards;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartStats;
        private VisionImageTool.CardPanel pnlStatusCard;
        private System.Windows.Forms.TableLayoutPanel tlpStatus;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripProgressBar stProgress;
        private System.Windows.Forms.ToolStripStatusLabel stMessage;
        private System.Windows.Forms.ToolStripStatusLabel stImagesCaption;
        private System.Windows.Forms.ToolStripStatusLabel stImages;
        private System.Windows.Forms.ToolStripStatusLabel stRawCaption;
        private System.Windows.Forms.ToolStripStatusLabel stRaw;
        private System.Windows.Forms.ToolStripStatusLabel stMaskCaption;
        private System.Windows.Forms.ToolStripStatusLabel stMask;
        private System.Windows.Forms.ToolStripStatusLabel stSchema;
        private System.Windows.Forms.ToolStripStatusLabel stIssues;
        private System.Windows.Forms.ToolStripStatusLabel stSelection;
        private System.Windows.Forms.ToolStripStatusLabel stSpringLeft;
        private System.Windows.Forms.ToolStripStatusLabel stEvalCaption;
        private System.Windows.Forms.ToolStripStatusLabel stO;
        private System.Windows.Forms.ToolStripStatusLabel stDelta;
        private System.Windows.Forms.ToolStripStatusLabel stX;
        private System.Windows.Forms.ToolStripStatusLabel stNone;
        private System.Windows.Forms.ToolStripStatusLabel stSpringRight;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
