using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code (xem ghi chú đầu frmMainV1.View.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1CaptureLayoutCodeFile { }

    /// <summary>
    /// HÀNH VI của bố cục tab Capture &amp; Monitor. Bố cục, chữ, màu, font nằm hết trong frmMainV1.Designer.cs
    /// (chỉnh được bằng Designer / Properties) - file này KHÔNG dựng hay đổi giao diện, chỉ:
    ///   - nút chuyển tab trên thanh tiêu đề;
    ///   - danh sách Raw n / Mask n (pnlRawCard / pnlMaskCard chồng nhau trong pnlListHost);
    ///   - tên ảnh + dòng phụ ở khung xem trước (tách từ lblPreviewHeader mà code cũ vẫn ghi);
    ///   - ← Trước · Ảnh i / N · Sau →;
    ///   - 3 ô trạng thái UnitX / Camera / SDK (vẽ theo thanh trạng thái), cấu hình cổng nâng cao thu gọn;
    ///   - ☰ Hiển thị ở thanh dưới cùng: ẩn / hiện khu vực (nhớ trong settings.json qua WindowLayout);
    ///     "Ẩn nhanh tất cả" trong menu đó + nút – ở góc phải mỗi thẻ (CardPanel.HideButton) để ẩn nhanh từng khối.
    /// </summary>
    public partial class frmMainV1
    {
        private static readonly Color CapPrimary = Color.FromArgb(51, 116, 213);
        private static readonly Color CapBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color CapSection = Color.FromArgb(51, 65, 85);
        private static readonly Color CapMuted = Color.FromArgb(100, 116, 139);

        /// <summary>Gọi cuối constructor: nối các sự kiện không đặt được trong Designer + trạng thái ban đầu.</summary>
        private void SetupCaptureBehavior()
        {
            // Đếm ảnh: code cũ ghi "(n)" vào lblRawCount / lblMaskCount (đang ẩn) -> cập nhật nút Raw n / Mask n.
            lblRawCount.TextChanged += (s, e) => UpdateListCounts();
            lblMaskCount.TextChanged += (s, e) => UpdateListCounts();
            lvRawImage.Enter += (s, e) => ShowCaptureList(false);
            lvMaskImage.Enter += (s, e) => ShowCaptureList(true);
            lvRawImage.SelectedIndexChanged += (s, e) => UpdateCaptureNav();
            lvMaskImage.SelectedIndexChanged += (s, e) => UpdateCaptureNav();

            foreach (var item in new ToolStripItem[] { tsslUnitX, tsslCamera, tsslSdk })
            {
                item.ForeColorChanged += (s, e) => pnlConnTiles.Invalidate();
            }

            // ☰ Hiển thị dùng chung cho cả hai tab: thêm các mục của Dataset Manager, chỉ hiện mục của tab đang mở.
            _datasetSectionItems = ucDataset.SectionMenuItems() ?? new ToolStripItem[0];
            tsddSections.DropDownItems.AddRange(_datasetSectionItems);
            UpdateSectionMenu();
            WireCardHideButtons();

            // Giữ menu ☰ Hiển thị mở khi tick (đóng khi bấm ra ngoài / "Hiện tất cả").
            tsddSections.DropDown.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked
                    && tsddSections.DropDownItems.OfType<ToolStripMenuItem>().Any(i => i.Selected && i.CheckOnClick))
                {
                    e.Cancel = true;
                }
            };

            tlpRoot.Resize += (s, e) => ApplyCaptureResponsive();
            CaptureDesignerSizes();

            ShowCaptureList(false);
            UpdateTabButtons();
            UpdatePortSummary();
            SplitPreviewHeader();
        }

        #region Thanh tiêu đề: chuyển tab

        private void btnTabCapture_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabCapture;
        }

        private void btnTabDataset_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabDataset;
        }

        private ToolStripItem[] _datasetSectionItems = new ToolStripItem[0];

        /// <summary>
        /// Nút ☰ Hiển thị (thanh dưới cùng) chỉ liệt kê khu vực của tab đang mở: tab Capture & Monitor = các mục
        /// trong Designer (tsmiSec*), tab Dataset Manager = các mục do ucDataset cung cấp.
        /// </summary>
        private void UpdateSectionMenu()
        {
            bool dataset = tabMain.SelectedTab == tabDataset;
            foreach (ToolStripItem item in tsddSections.DropDownItems)
            {
                item.Available = Array.IndexOf(_datasetSectionItems, item) >= 0 ? dataset : !dataset;
            }

            tsddSections.ToolTipText = dataset
                ? "Ẩn / hiện các khu vực của tab Dataset Manager"
                : "Ẩn / hiện các khu vực của tab Capture & Monitor";

            // Tab Dataset Manager không dùng kiểm tra trạng thái / log / số lỗi hệ thống của phần chụp ảnh.
            foreach (var item in new ToolStripItem[] { tsbCheckStatus, tsbShowLog, tsbClearLog, tsslOverall })
            {
                item.Available = !dataset;
            }
        }

        private void UpdateTabButtons()
        {
            UpdateSectionMenu();
            foreach (var pair in new[] { new { Button = btnTabCapture, Page = tabCapture }, new { Button = btnTabDataset, Page = tabDataset } })
            {
                bool active = tabMain.SelectedTab == pair.Page;
                pair.Button.BackColor = active ? Color.FromArgb(232, 241, 255) : Color.White;
                pair.Button.ForeColor = active ? Color.FromArgb(36, 91, 181) : CapSection;
                pair.Button.FlatAppearance.BorderColor = active ? Color.FromArgb(191, 213, 250) : CapBorder;
            }
        }

        #endregion

        #region Danh sách ảnh Raw / Mask

        private bool _capShowingMask;

        private void btnListRaw_Click(object sender, EventArgs e)
        {
            ShowCaptureList(false);
        }

        private void btnListMask_Click(object sender, EventArgs e)
        {
            ShowCaptureList(true);
        }

        private void ShowCaptureList(bool mask)
        {
            _capShowingMask = mask;
            pnlMaskCard.Visible = mask;
            pnlRawCard.Visible = !mask;
            foreach (var pair in new[] { new { Button = btnListRaw, Active = !mask }, new { Button = btnListMask, Active = mask } })
            {
                pair.Button.BackColor = pair.Active ? CapPrimary : Color.White;
                pair.Button.ForeColor = pair.Active ? Color.White : CapSection;
                pair.Button.FlatAppearance.BorderColor = pair.Active ? CapPrimary : CapBorder;
            }

            UpdateCaptureNav();
        }

        private void UpdateListCounts()
        {
            int raw = _rawItems == null ? 0 : _rawItems.Count;
            int mask = _maskItems == null ? 0 : _maskItems.Count;
            btnListRaw.Text = "Raw  " + raw;
            btnListMask.Text = "Mask  " + mask;
            lblListTotal.Text = (raw + mask) + " ảnh";
            UpdateCaptureNav();
        }

        #endregion

        #region Xem trước: tên + dòng phụ, Trước / Sau

        private void lblPreviewHeader_TextChanged(object sender, EventArgs e)
        {
            SplitPreviewHeader();
        }

        /// <summary>"Xem trước ảnh - X.bmp  [Mask - vừa nhận]" -> tên "X.bmp" + dòng phụ "Mask - vừa nhận".</summary>
        private void SplitPreviewHeader()
        {
            string text = lblPreviewHeader.Text ?? string.Empty;
            const string prefix = "Xem trước ảnh";
            string rest = text.StartsWith(prefix, StringComparison.Ordinal) ? text.Substring(prefix.Length).TrimStart(' ', '-') : text;
            string meta = string.Empty;
            int bracket = rest.IndexOf(" [", StringComparison.Ordinal);
            if (bracket >= 0)
            {
                meta = rest.Substring(bracket).Trim().Trim('[', ']');
                rest = rest.Substring(0, bracket).Trim();
            }

            lblPreviewName.Text = rest.Length == 0 ? "Chưa chọn ảnh" : rest;
            lblPreviewMeta.Text = meta.Length > 0 ? meta : rest.Length == 0 ? "Chọn ảnh trong danh sách để xem trước" : (_capShowingMask ? "Mask" : "Raw");
        }

        private ListView CurrentCaptureList
        {
            get { return _capShowingMask ? lvMaskImage : lvRawImage; }
        }

        private void btnPrevImage_Click(object sender, EventArgs e)
        {
            MoveCaptureSelection(-1);
        }

        private void btnNextImage_Click(object sender, EventArgs e)
        {
            MoveCaptureSelection(1);
        }

        /// <summary>Ảnh trước / sau trong danh sách đang xem; hết trang thì sang trang kế.</summary>
        private void MoveCaptureSelection(int delta)
        {
            var list = CurrentCaptureList;
            int count = list.VirtualMode ? list.VirtualListSize : list.Items.Count;
            if (count == 0)
            {
                return;
            }

            int current = list.SelectedIndices.Count > 0 ? list.SelectedIndices[0] : -1;
            int next = current < 0 ? 0 : Math.Min(count - 1, Math.Max(0, current + delta));
            if (next == current)
            {
                var pageButton = delta > 0 ? (_capShowingMask ? btnMaskNext : btnRawNext) : (_capShowingMask ? btnMaskPrev : btnRawPrev);
                if (!pageButton.Enabled)
                {
                    return;
                }

                InvokeOnClick(pageButton, EventArgs.Empty);
                count = list.VirtualMode ? list.VirtualListSize : list.Items.Count;
                if (count == 0)
                {
                    return;
                }

                next = delta > 0 ? 0 : count - 1;
            }

            list.SelectedIndices.Clear();
            list.SelectedIndices.Add(next);
            list.FocusedItem = list.Items[next];
            list.EnsureVisible(next);
            list.Focus();
            UpdateCaptureNav();
        }

        private void UpdateCaptureNav()
        {
            var list = CurrentCaptureList;
            int total = _capShowingMask ? (_maskItems == null ? 0 : _maskItems.Count) : (_rawItems == null ? 0 : _rawItems.Count);
            int page = _capShowingMask ? _maskPage : _rawPage;
            int index = list.SelectedIndices.Count > 0 ? page * PageSize + list.SelectedIndices[0] + 1 : 0;
            lblImageNav.Text = "Ảnh " + index + " / " + total;
            btnPrevImage.Enabled = index > 1;
            btnNextImage.Enabled = total > 0 && index < total;
        }

        #endregion

        #region Mạng & Thiết bị: ô trạng thái, cổng nâng cao

        private void ConnectionStatus_Changed(object sender, EventArgs e)
        {
            pnlConnTiles.Invalidate();
        }

        /// <summary>Ô trạng thái: chấm màu + tên + trạng thái (cùng nội dung / màu với thanh trạng thái).</summary>
        private void pnlConnTiles_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var items = new[] { new { Name = "UnitX", Item = (ToolStripItem)tsslUnitX }, new { Name = "Camera", Item = (ToolStripItem)tsslCamera }, new { Name = "SDK", Item = (ToolStripItem)tsslSdk } };
            const int gap = 10;
            int w = (pnlConnTiles.Width - gap * 2) / 3;
            int h = pnlConnTiles.Height - 2;
            using (var bold = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var small = new Font("Segoe UI", 8.25F))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var r = new Rectangle(i * (w + gap), 0, w, h);
                    Color color = items[i].Item.ForeColor;
                    bool good = color.G > color.R + 40 && color.G > 100;
                    using (var path = TileRound(r, 8))
                    using (var fill = new SolidBrush(good ? Color.FromArgb(236, 253, 243) : Color.FromArgb(241, 245, 249)))
                    {
                        g.FillPath(fill, path);
                    }

                    using (var dot = new SolidBrush(good ? Color.FromArgb(34, 160, 100) : Color.FromArgb(148, 163, 184)))
                    {
                        g.FillEllipse(dot, r.X + 12, r.Y + 13, 9, 9);
                    }

                    TextRenderer.DrawText(g, items[i].Name, bold, new System.Drawing.Point(r.X + 28, r.Y + 8), Color.FromArgb(30, 64, 120));
                    string state = (items[i].Item.Text ?? string.Empty).Trim();
                    TextRenderer.DrawText(g, state.Length == 0 ? "--" : state, small, new Rectangle(r.X + 12, r.Y + 28, r.Width - 16, 18),
                        CapMuted, TextFormatFlags.EndEllipsis);
                }
            }
        }

        private static GraphicsPath TileRound(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void lnkPorts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetPortsExpanded(!tlpPorts.Visible);
        }

        private void SetPortsExpanded(bool expanded)
        {
            tlpPorts.Visible = expanded;
            lblPortsSummary.Visible = !expanded;
            tlpSdk.RowStyles[6].Height = expanded ? 90F : 0F;
            lnkPorts.Text = expanded ? "Thu gọn ▴" : "Mở ▾";
            UpdatePortSummary();
        }

        private void numSdkPort_ValueChanged(object sender, EventArgs e)
        {
            UpdatePortSummary();
        }

        private void UpdatePortSummary()
        {
            lblPortsSummary.Text = "gRPC " + numSdkGrpc.Value + "  ·  ZMQ " + numSdkZmq.Value + "  ·  Blob " + numSdkBlob.Value + "  ·  Client " + numSdkClient.Value;
        }

        #endregion

        #region ☰ Hiển thị: ẩn / hiện khu vực

        private readonly HashSet<string> _hiddenCaptureSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private void tsmiSection_CheckedChanged(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            SetCaptureSectionVisible((string)item.Tag, item.Checked);
        }

        private void tsmiSecShowAll_Click(object sender, EventArgs e)
        {
            foreach (var item in SectionItems())
            {
                item.Checked = true;
            }
        }

        private void tsmiSecHideAll_Click(object sender, EventArgs e)
        {
            foreach (var item in SectionItems())
            {
                item.Checked = false;
            }
        }

        /// <summary>Nút – ở góc phải từng thẻ: bỏ tick mục tương ứng trong ☰ Hiển thị (hiện lại từ menu đó).</summary>
        private void WireCardHideButtons()
        {
            foreach (var item in SectionItems())
            {
                var menuItem = item;
                var card = (string)item.Tag == "List" ? pnlListCard : SectionCard((string)item.Tag) as CardPanel;
                if (card != null)
                {
                    card.HideButtonClick += (s, e) => menuItem.Checked = false;
                }
            }
        }

        private IEnumerable<ToolStripMenuItem> SectionItems()
        {
            return tsddSections.DropDownItems.OfType<ToolStripMenuItem>().Where(i => i.Tag is string);
        }

        private Control SectionCard(string key)
        {
            switch (key)
            {
                case "Part": return pnlPartCard;
                case "Control": return pnlPartToolbar;
                case "Camera": return pnlCameraToolbar;
                case "Storage": return pnlPathCard;
                case "Network": return pnlSdkCard;
                default: return null;
            }
        }

        private void SetCaptureSectionVisible(string key, bool visible)
        {
            if (visible)
            {
                _hiddenCaptureSections.Remove(key);
            }
            else
            {
                _hiddenCaptureSections.Add(key);
            }

            if (key == "List")
            {
                scWork.Panel1Collapsed = !visible;
                return;
            }

            var card = SectionCard(key);
            if (card != null)
            {
                card.Visible = visible;
            }

            RelayoutCaptureRows();
        }

        /// <summary>Chia lại cột cho các thẻ còn hiện (theo tỉ lệ đặt trong Designer); ẩn hết thì bỏ cả hàng.</summary>
        private void RelayoutCaptureRows()
        {
            RelayoutRow(tlpTopRow, new[] { "Part", "Control", "Camera" }, _topWeights, 0, _capCompact ? _topCompactHeight : _topHeight);
            RelayoutRow(tlpBottomRow, new[] { "Storage", "Network" }, _bottomWeights, 2, _capCompact ? _bottomCompactHeight : _bottomHeight);
        }

        private void RelayoutRow(TableLayoutPanel row, string[] keys, float[] weights, int rootRow, float height)
        {
            bool any = false;
            for (int i = 0; i < keys.Length; i++)
            {
                bool shown = !_hiddenCaptureSections.Contains(keys[i]);
                any |= shown;
                row.ColumnStyles[i].Width = shown ? weights[i] : 0F;
            }

            tlpRoot.RowStyles[rootRow].Height = any ? height : 0F;
            row.Visible = any;
        }

        /// <summary>Khu vực đang ẩn (lưu vào settings.json qua WindowLayout).</summary>
        private List<string> HiddenCaptureSections()
        {
            return _hiddenCaptureSections.ToList();
        }

        private void RestoreCaptureSections(IEnumerable<string> hidden)
        {
            if (hidden == null)
            {
                return;
            }

            var set = new HashSet<string>(hidden, StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var item in SectionItems())
                {
                    item.Checked = !set.Contains((string)item.Tag);
                }
            }
            finally
            {
            }
        }

        #endregion

        #region Tự co giãn khi cửa sổ thấp

        // Kích thước gốc đọc từ Designer (sửa trong Properties là có hiệu lực) - chỉ co lại khi cửa sổ thấp.
        private float _topHeight, _bottomHeight, _topCompactHeight, _bottomCompactHeight;
        private float[] _topWeights, _bottomWeights;
        private bool _capCompact;

        private void CaptureDesignerSizes()
        {
            _topHeight = tlpRoot.RowStyles[0].Height;
            _bottomHeight = tlpRoot.RowStyles[2].Height;
            _topCompactHeight = Math.Min(_topHeight, 150F);
            _bottomCompactHeight = Math.Min(_bottomHeight, 214F);
            _topWeights = tlpTopRow.ColumnStyles.Cast<ColumnStyle>().Select(c => c.Width).ToArray();
            _bottomWeights = tlpBottomRow.ColumnStyles.Cast<ColumnStyle>().Select(c => c.Width).ToArray();
        }

        /// <summary>Vùng tab thấp (&lt; 700px): thu hàng trên / dưới để Danh sách | Xem trước vẫn đủ chỗ.</summary>
        private void ApplyCaptureResponsive()
        {
            if (tlpRoot.Height <= 0 || _topWeights == null)
            {
                return;
            }

            bool compact = tlpRoot.Height < 700;
            if (compact == _capCompact)
            {
                return;
            }

            _capCompact = compact;
            tlpRoot.SuspendLayout();
            foreach (var grid in new[] { tlpPartFields, tlpPartButtons, tlpCameraButtons })
            {
                for (int i = 1; i < grid.RowStyles.Count; i++)
                {
                    grid.RowStyles[i].Height = compact ? (grid == tlpPartFields ? 32F : 44F) : (grid == tlpPartFields ? 38F : 56F);
                }
            }

            tlpPath.RowStyles[4].Height = compact ? 0F : 22F;
            foreach (var card in new Control[] { pnlPartCard, pnlPartToolbar, pnlCameraToolbar, pnlPathCard, pnlSdkCard })
            {
                card.Padding = compact ? new Padding(16, 6, 18, 6) : new Padding(18, 10, 18, 12);
            }

            RelayoutCaptureRows();
            tlpRoot.ResumeLayout(true);
        }

        #endregion
    }
}
