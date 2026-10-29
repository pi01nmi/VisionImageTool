using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerToolbarCodeFile { }

    /// <summary>
    /// Bố cục P0 (Danh-gia-va-de-xuat-UI-Dataset-Manager.md):
    ///   - Thanh công cụ trên bảng: "Bảng kết quả · n / N hàng" | ⌨ Phím tắt | ⬇ Xuất ▾ (Excel / JSON).
    ///     Thanh trạng thái dưới cùng chỉ còn số liệu.
    ///   - Bộ lọc: tiêu đề "Bộ lọc (n)" đếm số bộ lọc đang bật + nút "Đặt lại".
    ///   - Thành phần schema: một nút "+ Thành phần ▾" thay 6 nút; mục đã có trong schema được tick ✓ và khoá.
    ///   - Ô Vision / Đánh giá vẽ thành badge O / Δ / X (có chữ, không chỉ dựa vào màu); Đối soát là badge trung tính
    ///     (== / != là quy tắc được chọn, không phải lỗi).
    ///   - Khung Vấn đề tự thu gọn còn một dòng khi không có vấn đề.
    ///   - Khung xem trước: nút ◀ ▶ chuyển lần chụp (cùng phím , .).
    /// </summary>
    public partial class ucDatasetManager
    {
        #region Thanh công cụ trên bảng

        /// <summary>
        /// Tiêu đề thẻ Bảng kết quả (Designer: tlpTableToolbar): "BẢNG KẾT QUẢ   n / N dòng" | ⌨ · ⬇ Xuất (☰ Hiển thị nằm ở thanh dưới cùng của form chính).
        /// </summary>
        private void WireTableToolbar()
        {
            WireSectionToggles();
            btnKeys.Click += (s, e) => ShowHint(btnKeys, "Phím tắt (khi bảng đang được chọn)", ShortcutHelpText(), true,
                new System.Drawing.Point(Math.Min(0, btnKeys.Width - 560), btnKeys.Height));

            // Xuất: hộp thoại xác nhận schema + kéo thả thứ tự trường, rồi chọn Excel / JSON (ucDatasetManager.ExportOrder.cs).
            btnExport.Click += (s, e) => ShowExportDialog();
        }

        /// <summary>Tiêu đề bảng "n / N hàng" + "Bộ lọc (k)" + nút Đặt lại - gọi sau mỗi lần dựng / lọc bảng.</summary>
        private void UpdateFilterState()
        {
            int total = chkUnmatchedOnly.Checked ? _unmatchedRows.Count : _rows.Count;
            int shown = grid.Rows.Count;
            lblRowCount.Text = TableBlocked
                ? "⚠ Bảng tạm ẩn: schema cho ra " + _conditions.Count + " giá trị ĐK - kiểm tra vị trí <ĐK> trong schema rồi bấm Áp dụng"
                : shown + " / " + total + " dòng";
            lblRowCount.ForeColor = TableBlocked ? Color.FromArgb(185, 28, 28) : MutedText;
            lblResultCount.Text = shown + " kết quả";
            lblResultCount.ForeColor = shown == 0 && total > 0 ? Color.FromArgb(185, 28, 28) : Color.FromArgb(30, 41, 59);
            UpdateNavPosition();
            UpdateStatCards();

            int active = 0;
            if (txtFilter.Text.Trim().Length > 0) active++;
            if (cboDkFilter.SelectedIndex > 0) active++;
            if (cboUnitXFilter.SelectedIndex > 0) active++;
            if (cboEvalFilter.SelectedIndex > 0) active++;
            if (chkUnmatchedOnly.Checked) active++;
            active += _columnFilters.Count + (_treeKeyFilter.Count > 0 ? 1 : 0);

            lblFilterTitle.Text = active > 0 ? "Bộ lọc (" + active + ")" : "Bộ lọc";
            lblFilterTitle.ForeColor = active > 0 ? Primary : SectionColor;
            btnResetFilters.Enabled = active > 0;
        }

        private void SetupFilterHeader()
        {
            btnResetFilters.Click += (s, e) =>
            {
                chkUnmatchedOnly.Checked = false;
                ClearGridFilters();
            };
        }

        #endregion

        #region "+ Thành phần" (thay 6 nút chèn trường)

        private void ShowComponentsMenu()
        {
            string error;
            var schema = DatasetSchema.Parse(txtSchema.Text, out error);
            var roles = schema == null ? new HashSet<DatasetFieldRole>() : new HashSet<DatasetFieldRole>(schema.Fields.Where(f => !f.Implicit).Select(f => f.Role));

            var menu = new ContextMenuStrip { ShowCheckMargin = true, ShowImageMargin = false, Font = new Font("Segoe UI", 9.5F) };
            var entries = new[]
            {
                new { Button = btnInsertDk, Text = "DK", Hint = "điều kiện chụp - mỗi DK một nhóm cột", Present = roles.Contains(DatasetFieldRole.Condition) },
                new { Button = btnInsertType, Text = "Raw / Mask", Hint = "loại ảnh - có thể vắng trong tên", Present = roles.Contains(DatasetFieldRole.Type) },
                new { Button = btnInsertEval, Text = "Đánh giá", Hint = "OK → O · NG → X · LIMIT → Δ", Present = roles.Contains(DatasetFieldRole.Evaluation) },
                new { Button = btnInsertSuffix, Text = "Hậu tố", Hint = "mỗi lần chụp một HÀNG riêng", Present = roles.Contains(DatasetFieldRole.Suffix) },
                new { Button = btnInsertShot, Text = "Lần chụp", Hint = "gộp các lần chụp vào MỘT hàng (NG×…×OK)", Present = roles.Contains(DatasetFieldRole.Shot) },
                new { Button = btnInsertExt, Text = "Extension", Hint = "đuôi ảnh - chỉ để hiển thị, tự nhận", Present = schema != null && schema.HasExtension }
            };

            foreach (var entry in entries)
            {
                var button = entry.Button;
                string reason = entry.Present ? "   (đã có)"
                    : !button.Enabled ? "   (không dùng chung với " + (button == btnInsertSuffix ? "Lần chụp" : "Hậu tố") + ")"
                    : string.Empty;
                var item = new ToolStripMenuItem(entry.Text + "   —   " + entry.Hint + reason)
                {
                    Checked = entry.Present,
                    Enabled = button.Enabled && !entry.Present,
                    ForeColor = button.ForeColor
                };
                item.Click += (s, e) => InsertSchemaToken((string)button.Tag);
                menu.Items.Add(item);
            }

            menu.Closed += (s, e) => menu.BeginInvoke((Action)menu.Dispose);
            menu.Show(btnComponents, new System.Drawing.Point(btnComponents.Width, btnComponents.Height), ToolStripDropDownDirection.BelowLeft);
        }

        #endregion

        #region Badge O / Δ / X

        private static bool IsBadgeColumn(DataGridViewColumn column)
        {
            var info = BlockOf(column);
            return info != null && (info.Part == BlockPart.UnitX || info.Part == BlockPart.Evaluation || info.Part == BlockPart.Compare);
        }

        /// <summary>Màu chữ / viền đậm của badge theo nhãn (cùng màu thanh trạng thái và biểu đồ).</summary>
        private static Color BadgeStrong(string label)
        {
            switch (label)
            {
                case "O": return Color.FromArgb(21, 128, 61);
                case "Δ": return Color.FromArgb(180, 83, 9);
                case "X": return Color.FromArgb(185, 28, 28);
                default: return Color.FromArgb(71, 85, 105);
            }
        }

        private static Color BadgeFill(string label)
        {
            switch (label)
            {
                case "O": return Color.FromArgb(220, 252, 231);
                case "Δ": return Color.FromArgb(254, 243, 199);
                case "X": return Color.FromArgb(254, 226, 226);
                default: return Color.FromArgb(241, 245, 249);
            }
        }

        /// <summary>
        /// Vẽ ô Vision / Đánh giá / Đối soát: nền trơn + badge bo tròn có chữ. Ô sửa được (Đánh giá, Đối soát) có
        /// mũi tên ▾ nhỏ bên phải. Trả về false nếu ô trống (để vẽ mặc định).
        /// </summary>
        private bool PaintBadgeCell(DataGridViewCellPaintingEventArgs e, DataGridViewColumn column)
        {
            string text = Convert.ToString(e.FormattedValue);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            var r = e.CellBounds;

            using (var back = new SolidBrush(Theme.Back(selected ? Color.FromArgb(219, 234, 254) : Color.White)))
            {
                e.Graphics.FillRectangle(back, r);
            }

            e.Paint(r, DataGridViewPaintParts.Border);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool compare = BlockOf(column).Part == BlockPart.Compare;
            bool editable = !cell.ReadOnly && !_locked;
            int arrow = editable ? 12 : 0;

            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                var size = TextRenderer.MeasureText(text, font);
                int width = Math.Min(r.Width - 8 - arrow, Math.Max(compare ? 34 : 30, size.Width + 14));
                int height = Math.Min(r.Height - 8, 20);
                var pill = new Rectangle(r.Left + (r.Width - arrow - width) / 2, r.Top + (r.Height - height) / 2, width, height);

                string label = compare ? null : text;
                using (var path = RoundedRect(pill, height / 2))
                using (var fill = new SolidBrush(Theme.Back(BadgeFill(label))))
                using (var pen = new Pen(Theme.Fore(Color.FromArgb(90, BadgeStrong(label)))))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(pen, path);
                }

                TextRenderer.DrawText(g, text, font, pill, Theme.Fore(BadgeStrong(label)),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            if (editable)
            {
                TextRenderer.DrawText(g, "▾", grid.Font, new Rectangle(r.Right - arrow - 2, r.Top, arrow, r.Height),
                    Theme.Fore(Color.FromArgb(148, 163, 184)), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            g.SmoothingMode = SmoothingMode.Default;
            return true;
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Khung Vấn đề tự thu gọn

        private const int IssuesCompactHeight = 56;
        private bool _issuesCompact;

        /// <summary>Không có vấn đề: khung chỉ còn dòng "✓ Không có vấn đề dữ liệu"; có vấn đề: mở lại cỡ cũ.</summary>
        private void UpdateIssuesCompact()
        {
            bool compact = _issues.Count == 0;
            lvIssues.Visible = cboIssueKind.Visible = btnExportIssues.Visible = lblIssueKind.Visible = !compact;

            if (compact == _issuesCompact)
            {
                return;
            }

            _issuesCompact = compact;
            ApplyIssuesHeight();
        }

        /// <summary>Chiều cao khung Vấn đề: thu gọn = một dòng tiêu đề, không thì cỡ người dùng đã kéo (mặc định 170).</summary>
        private void ApplyIssuesHeight()
        {
            if (_issuesSplit.Panel2Collapsed || _issuesSplit.Height <= 0)
            {
                return;
            }

            // Hàng dưới = Vấn đề | Thống kê: thu gọn (không có vấn đề / ẩn khung vấn đề) thì vừa thẻ thống kê.
            bool small = _issuesCompact || !chkShowIssues.Checked;
            _issuesSplit.IsSplitterFixed = small;
            bool wasApplying = _applyingLayout;
            _applyingLayout = true;
            try
            {
                int height = small ? (chkShowStats.Checked ? StatsCardHeight : IssuesCompactHeight) : Math.Max(StatsCardHeight, _layout.IssuesHeight ?? 150);

                // Không để hàng dưới lấn quá 40% vùng làm việc (cây / bảng / xem trước là trọng tâm).
                height = Math.Min(height, Math.Max(IssuesCompactHeight, (int)(_issuesSplit.Height * 0.4)));
                TrySetDistance(_issuesSplit, _issuesSplit.Height - _issuesSplit.SplitterWidth - height);
            }
            finally
            {
                _applyingLayout = wasApplying;
            }
        }

        #endregion

        #region ◀ ▶ lần chụp ở khung xem trước

        private void SetupShotNavButtons()
        {
            btnShotPrev.Click += (s, e) => ShiftShot(-1);
            btnShotNext.Click += (s, e) => ShiftShot(1);
        }

        /// <summary>Hiện ◀ ▶ khi ô đang chọn thuộc block có nhiều lần chụp.</summary>
        private void UpdateShotNav()
        {
            var cell = grid.CurrentCell;
            var block = cell == null ? null : CellBlock(cell.RowIndex, cell.ColumnIndex);
            bool show = block != null && block.Shots != null && block.Shots.Count > 1;
            btnShotPrev.Visible = btnShotNext.Visible = show;
        }

        #endregion
    }
}
