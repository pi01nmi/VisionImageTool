using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (Designer chỉ mở ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerShellCodeFile { }

    /// <summary>
    /// Bố cục theo mockup (Mockup-UI-Dataset-Manager.png / Danh-gia-va-de-xuat-UI-Dataset-Manager.md mục 9) - nằm trong
    /// ucDatasetManager.Designer.cs, chỉnh bằng Designer:
    ///   ┌ pnlLoadCard:   ˅ NẠP DATASET  ☑ Quét thư mục con  /  [Thư mục ảnh ……] [Duyệt…] [Quét thư mục]
    ///   ├ pnlSchemaCard: ˅ SCHEMA HIỆN HÀNH  /  [schema 🕘] [Đọc: …] [Áp dụng Schema] [⇄ Chuyển sang theo tên…] [↶ Hoàn tác]
    ///   ├ pnlFilterCard: ˅ BỘ LỌC & TRẠNG THÁI  /  thẻ schema… ⓘ [+ Thành phần ▾] Quy tắc 🔒
    ///   │                Bộ lọc · Tìm · DK · Vision · Đánh giá · □ Chỉ ảnh sai schema · n kết quả [Đặt lại]
    ///   │  (nút ˅ thu gọn khối chỉ còn dòng tiêu đề - nhớ trong dataset_manager.json)
    ///   ├ scIssues.Panel1 = scPreview: (scTree: CÂY DATASET | BẢNG KẾT QUẢ) | XEM TRƯỚC ẢNH
    ///   ├ scIssues.Panel2 = scBottom: VẤN ĐỀ DỮ LIỆU | THỐNG KÊ
    ///   └ pnlStatusCard: thanh trạng thái
    /// File này chỉ gắn sự kiện + phần tự vẽ (chú giải thẻ, tóm tắt dưới cây, thẻ thống kê).
    /// </summary>
    public partial class ucDatasetManager
    {
        private const int StatsCardHeight = 118;

        private TableLayoutPanel _filterCardTable;
        private SplitContainer _bottomSplit;

        /// <summary>Số mẫu O / Δ / X / Chưa có của các mẫu đang hiện (sau lọc) - thẻ thống kê + tóm tắt dưới cây.</summary>
        private readonly int[] _statCounts = new int[4];

        private static readonly Color SectionColor = Color.FromArgb(51, 65, 85);
        private static readonly Color MutedText = Color.FromArgb(100, 116, 139);

        #region 01 Nạp dataset + 02 Schema hiện hành

        private void WireSchemaCard()
        {
            txtFolder.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadFolder();
                }
            };

            btnBrowse.Click += (s, e) => BrowseFolder();
            btnLoad.Click += (s, e) => LoadFolder();

            TrackSchemaCaret();
            txtSchema.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplySchema(true);
                }
            };
            txtSchema.TextChanged += (s, e) => UpdateSchemaChips();
            btnApplySchema.Click += (s, e) => ApplySchema(true);

            btnRecentSchemas.Click += (s, e) => ShowRecentSchemas();
            WireConvertButtons();
        }

        #endregion

        #region Thành phần schema + Bộ lọc

        private void WireFilterCard()
        {
            pnlSchemaChips.ContentEndChanged += (s, e) => PlaceSchemaHint();
            WireSchemaHintButton();

            // Các nút chèn giữ làm nguồn trạng thái / token cho menu "+ Thành phần" (không đặt lên giao diện).
            StyleInsertButton(btnInsertDk, "+ DK", "<DK>", SchemaChipsPanel.ConditionColor);
            StyleInsertButton(btnInsertType, "+ Raw/Mask", "<Raw/Mask>", SchemaChipsPanel.TypeColor);
            StyleInsertButton(btnInsertEval, "+ Đánh giá", "<Đánh giá>", SchemaChipsPanel.EvalColor);
            StyleInsertButton(btnInsertSuffix, "+ Hậu tố", "<Hậu tố>", SchemaChipsPanel.SuffixColor);
            StyleInsertButton(btnInsertShot, "+ Lần chụp", "<Lần chụp>", SchemaChipsPanel.ShotColor);
            StyleInsertButton(btnInsertExt, "+ Extension", "<Extension>", SchemaChipsPanel.ExtensionColor);
            btnComponents.Click += (s, e) => ShowComponentsMenu();

            WireCompareModeBox();
            WireLockButton();
            WireFilterFlow();
        }

        private void WireFilterFlow()
        {
            txtFilter.TextChanged += (s, e) => ApplyFilter();

            // Chỉ xem một DK: ẩn block cột của các DK khác + ẩn hàng không có ảnh ở DK đó.
            cboDkFilter.Items.Add("Tất cả");
            cboDkFilter.SelectedIndex = 0;
            cboDkFilter.SelectedIndexChanged += (s, e) =>
            {
                if (!_fillingGrid)
                {
                    UpdateBlockVisibility();
                    ApplyFilter();
                }
            };

            chkUnmatchedOnly.CheckedChanged += (s, e) => ApplyFilter();

            foreach (var combo in new[] { cboUnitXFilter, cboEvalFilter })
            {
                combo.Items.AddRange(LabelFilterChoices.Cast<object>().ToArray());
                combo.SelectedIndex = 0;
                combo.SelectedIndexChanged += (s, e) => ApplyFilter();
            }

            SetupFilterHeader();
        }

        #endregion

        #region Vùng làm việc: Cây | Bảng | Xem trước, dưới: Vấn đề | Thống kê

        private void WireGridArea()
        {
            // Vạch chia mặc định theo tỉ lệ khi chưa có bố cục đã lưu (Designer chỉ giữ cỡ của lúc thiết kế).
            // Khung xem ảnh là trọng tâm: mặc định rộng hơn phần Cây + Bảng.
            _previewSplit.HandleCreated += (s, e) => TrySetDistance(_previewSplit, Math.Max(300, (int)(_previewSplit.Width * DefaultPreviewSplitRatio)));
            _bottomSplit.HandleCreated += (s, e) => SyncBottomSplit();

            // Hàng dưới chia theo hàng trên: Vấn đề nằm dưới Cây + Bảng, Thống kê nằm dưới Xem trước.
            _previewSplit.SplitterMoved += (s, e) => SyncBottomSplit();
            _previewSplit.SizeChanged += (s, e) => SyncBottomSplit();
            _bottomSplit.SplitterMoved += (s, e) =>
            {
                if (!_syncingSplit && !_previewSplit.Panel1Collapsed && !_previewSplit.Panel2Collapsed && !_bottomSplit.Panel1Collapsed && !_bottomSplit.Panel2Collapsed)
                {
                    _syncingSplit = true;
                    TrySetDistance(_previewSplit, _bottomSplit.SplitterDistance);
                    _syncingSplit = false;
                }
            };
            _issuesSplit.HandleCreated += (s, e) => TrySetDistance(_issuesSplit, _issuesSplit.Height - _issuesSplit.SplitterWidth - StatsCardHeight);

            WireTreeCard();
            WireTableToolbar();
            SetupGrid();
            WirePreviewCard();
            WireIssuesPanel();
            WireStatsCard();
        }

        /// <summary>Tỉ lệ mặc định của phần Cây + Bảng (phần còn lại là khung xem ảnh).</summary>
        private const double DefaultPreviewSplitRatio = 0.42;
        private bool _syncingSplit;

        private void SyncBottomSplit()
        {
            if (_syncingSplit || _bottomSplit == null || _previewSplit.Panel1Collapsed || _previewSplit.Panel2Collapsed
                || _bottomSplit.Panel1Collapsed || _bottomSplit.Panel2Collapsed || _bottomSplit.Width != _previewSplit.Width)
            {
                return;
            }

            _syncingSplit = true;
            try
            {
                TrySetDistance(_bottomSplit, _previewSplit.SplitterDistance);
            }
            finally
            {
                _syncingSplit = false;
            }
        }

        private void WirePreviewCard()
        {
            SetupPreviewHeader();
            btnPrevRow.Click += (s, e) => MoveRow(-1);
            btnNextRow.Click += (s, e) => MoveRow(1);
            grid.CurrentCellChanged += (s, e) => UpdateNavPosition();
        }

        private void MoveRow(int delta)
        {
            if (grid.Rows.Count == 0)
            {
                return;
            }

            var cell = grid.CurrentCell;
            int row = cell == null ? 0 : Math.Min(grid.Rows.Count - 1, Math.Max(0, cell.RowIndex + delta));
            int column = cell == null ? grid.Columns.Cast<DataGridViewColumn>().First(c => c.Visible).Index : cell.ColumnIndex;
            grid.CurrentCell = grid.Rows[row].Cells[column];
            grid.Focus();
        }

        private void UpdateNavPosition()
        {
            var cell = grid.CurrentCell;
            lblNavPos.Text = grid.Rows.Count == 0 ? "0 / 0" : ((cell == null ? 0 : cell.RowIndex + 1) + " / " + grid.Rows.Count);
            btnPrevRow.Enabled = cell != null && cell.RowIndex > 0;
            btnNextRow.Enabled = grid.Rows.Count > 0 && (cell == null || cell.RowIndex < grid.Rows.Count - 1);
        }

        #endregion


        #region Cây dataset + tóm tắt

        private void WireTreeCard()
        {
            tvDataset.AfterSelect += (s, e) =>
            {
                if (!_treeUpdating)
                {
                    ApplyTreeFilter(e.Node);
                }
            };

            pnlTreeSummary.Paint += PaintTreeSummary;
            pnlTreeSummary.Resize += (s, e) => pnlTreeSummary.Invalidate();

            // Icon thư mục cho mọi nút (Dataset / DK / ...).
            var icons = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            icons.Images.Add(FolderIcon());
            tvDataset.ImageList = icons;
            tvDataset.ImageIndex = 0;
            tvDataset.SelectedImageIndex = 0;
        }

        /// <summary>Icon thư mục nét mảnh (cùng phong cách nút trên giao diện).</summary>
        private static Bitmap FolderIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.FromArgb(100, 116, 139), 1.3F))
            using (var path = new GraphicsPath())
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                path.AddLines(new[] { new PointF(1.5F, 3.5F), new PointF(6F, 3.5F), new PointF(7.5F, 5F), new PointF(14.5F, 5F), new PointF(14.5F, 13F), new PointF(1.5F, 13F) });
                path.CloseFigure();
                g.DrawPath(pen, path);
                g.DrawLine(pen, 1.5F, 7F, 14.5F, 7F);
            }

            return bmp;
        }

        private const string TreeTitleDefault = "CÂY DATASET";

        private static readonly string[] StatNames = { "O", "Δ", "X", "Chưa có" };

        /// <summary>Thanh O / Δ / X / Chưa có của các mẫu đang hiện (theo nút cây + bộ lọc).</summary>
        private void PaintTreeSummary(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int total = _statCounts.Sum();
            int width = pnlTreeSummary.Width - 4;
            using (var font = new Font("Segoe UI", 8.5F))
            using (var bold = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "Đánh giá · " + total + " mẫu đang hiện", font, new System.Drawing.Point(0, 0), Theme.Fore(MutedText));
                for (int i = 0; i < 4; i++)
                {
                    int y = 20 + i * 22;
                    if (QuickFilterIndex == i)
                    {
                        using (var path = RoundedRect(new Rectangle(0, y - 3, width + 2, 21), 5))
                        using (var hl = new SolidBrush(Theme.Back(Color.FromArgb(232, 241, 255))))
                        {
                            g.FillPath(hl, path);
                        }
                    }

                    TextRenderer.DrawText(g, StatNames[i], bold, new System.Drawing.Point(0, y), Theme.Fore(ChartCategoryColors[i]));
                    TextRenderer.DrawText(g, _statCounts[i].ToString(), font, new Rectangle(width - 40, y, 40, 16), Theme.Fore(Color.FromArgb(51, 65, 85)), TextFormatFlags.Right);
                    var track = new Rectangle(52, y + 6, Math.Max(10, width - 100), 5);
                    using (var back = new SolidBrush(Theme.Back(Color.FromArgb(226, 232, 240))))
                    using (var fill = new SolidBrush(Theme.Fore(ChartCategoryColors[i])))
                    {
                        g.FillRectangle(back, track);
                        if (total > 0 && _statCounts[i] > 0)
                        {
                            g.FillRectangle(fill, new Rectangle(track.X, track.Y, Math.Max(2, track.Width * _statCounts[i] / total), track.Height));
                        }
                    }
                }
            }
        }

        #endregion


        #region Thống kê

        private void WireStatsCard()
        {
            SetupChart();
            chartStats.Titles["title"].Visible = false;
            // Biểu đồ nhỏ: bỏ chú thích (không đủ chỗ) - giải thích bằng tooltip (Designer).
            chartStats.Legends["legend"].Enabled = false;
            var area = chartStats.ChartAreas["main"];
            area.AxisY.LabelStyle.Enabled = false;
            area.AxisY.MajorTickMark.Enabled = false;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            area.AxisX.LabelStyle.Angle = 0;
            area.AxisX.IsLabelAutoFit = false;

            pnlStatCards.Paint += PaintStatCards;
            pnlStatCards.Resize += (s, e) => pnlStatCards.Invalidate();

            // Bộ lọc nhanh: bấm thẻ O / Δ / X / Chưa có (hoặc dòng tương ứng dưới cây) = lọc Đánh giá, bấm lại = bỏ lọc.
            pnlStatCards.MouseClick += (s, e) => ToggleEvalQuickFilter(StatCardAt(e.Location));
            pnlStatCards.MouseMove += (s, e) => pnlStatCards.Cursor = StatCardAt(e.Location) >= 0 ? Cursors.Hand : Cursors.Default;
            pnlTreeSummary.MouseClick += (s, e) => ToggleEvalQuickFilter(TreeSummaryRowAt(e.Location));
            pnlTreeSummary.MouseMove += (s, e) => pnlTreeSummary.Cursor = TreeSummaryRowAt(e.Location) >= 0 ? Cursors.Hand : Cursors.Default;
            toolTip.SetToolTip(pnlStatCards, "Bấm thẻ để lọc nhanh theo Đánh giá - bấm lại để bỏ lọc");
            toolTip.SetToolTip(pnlTreeSummary, "Bấm một dòng để lọc nhanh theo Đánh giá - bấm lại để bỏ lọc");
            cboEvalFilter.SelectedIndexChanged += (s, e) =>
            {
                pnlStatCards.Invalidate();
                pnlTreeSummary.Invalidate();
            };
        }

        private Rectangle StatCardRect(int index)
        {
            int gap = 10;
            int w = (pnlStatCards.Width - gap * 3) / 4;
            int h = Math.Min(70, pnlStatCards.Height - 4);
            return new Rectangle(index * (w + gap), 2, w, h);
        }

        private int StatCardAt(System.Drawing.Point point)
        {
            for (int i = 0; i < 4; i++)
            {
                if (StatCardRect(i).Contains(point))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int TreeSummaryRowAt(System.Drawing.Point point)
        {
            int index = (point.Y - 18) / 22;
            return point.Y >= 18 && index >= 0 && index < 4 ? index : -1;
        }

        /// <summary>Chỉ mục thẻ (0 O · 1 Δ · 2 X · 3 Chưa có) = mục 1..4 của ô lọc Đánh giá.</summary>
        private void ToggleEvalQuickFilter(int index)
        {
            if (index < 0)
            {
                return;
            }

            int target = index + 1;
            cboEvalFilter.SelectedIndex = cboEvalFilter.SelectedIndex == target ? 0 : target;
        }

        private int QuickFilterIndex
        {
            get { return cboEvalFilter.SelectedIndex - 1; }
        }

        private void PaintStatCards(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int total = _statCounts.Sum();
            Color[] fills = { Color.FromArgb(236, 253, 243), Color.FromArgb(255, 247, 230), Color.FromArgb(254, 240, 240), Color.FromArgb(241, 245, 249) };

            using (var small = new Font("Segoe UI", 8.25F))
            using (var big = new Font("Segoe UI", 18F, FontStyle.Bold))
            {
                for (int i = 0; i < 4; i++)
                {
                    var r = StatCardRect(i);
                    using (var path = RoundedRect(r, 8))
                    using (var fill = new SolidBrush(Theme.Back(fills[i])))
                    {
                        g.FillPath(fill, path);
                    }

                    // Thẻ đang là bộ lọc nhanh: viền đậm màu của nhóm.
                    if (QuickFilterIndex == i)
                    {
                        using (var path = RoundedRect(new Rectangle(r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2), 8))
                        using (var pen = new Pen(Theme.Fore(ChartCategoryColors[i]), 2F))
                        {
                            g.DrawPath(pen, path);
                        }
                    }

                    TextRenderer.DrawText(g, StatNames[i], small, new System.Drawing.Point(r.X + 12, r.Y + 8), Theme.Fore(MutedText));
                    var size = TextRenderer.MeasureText(_statCounts[i].ToString(), big);
                    TextRenderer.DrawText(g, _statCounts[i].ToString(), big, new System.Drawing.Point(r.X + 8, r.Y + 26), Theme.Fore(ChartCategoryColors[i]));
                    string percent = total == 0 ? "0%" : Math.Round(_statCounts[i] * 100.0 / total, 1) + "%";
                    TextRenderer.DrawText(g, percent, small, new System.Drawing.Point(r.X + 12 + size.Width, r.Y + 40), Theme.Fore(MutedText));
                }
            }
        }

        /// <summary>Đếm O / Δ / X / Chưa có của các mẫu đang hiện (gọi từ UpdateSummary).</summary>
        private void UpdateStatCards()
        {
            Array.Clear(_statCounts, 0, 4);
            foreach (var block in ShownBlocks())
            {
                switch (BlockEvaluation(block))
                {
                    case "O": _statCounts[0]++; break;
                    case "Δ": _statCounts[1]++; break;
                    case "X": _statCounts[2]++; break;
                    default: _statCounts[3]++; break;
                }
            }

            int total = _statCounts.Sum();
            int all = _rows.Sum(r => r.Blocks.Count);
            lblStatsSub.Text = total == all ? total + " mẫu" : total + " mẫu sau lọc  (/" + all + ")";
            pnlStatCards.Invalidate();
            pnlTreeSummary.Invalidate();
        }

        #endregion
    }
}
