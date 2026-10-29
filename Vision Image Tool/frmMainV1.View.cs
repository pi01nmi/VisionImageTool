using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code. Nếu class đầu tiên là partial frmMainV1, VS tự gắn SubType=Form rồi mở Designer - Designer
    // không thấy lớp cha ("base class System.Void") và đã từng ghi đè frmMainV1.Designer.cs thành form
    // trống (2026-09-25). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1ViewCodeFile { }

    /// <summary>
    /// frmMainV1 - các phần xem ảnh bổ sung của tab Capture &amp; Monitor, dựng bằng code (không sửa
    /// Designer):
    ///   - Ghi chú ảnh: dấu ✎ trong danh sách + thanh ghi chú dưới khung xem trước.
    /// </summary>
    public partial class frmMainV1
    {
        #region Ghi chú ảnh

        /// <summary>Dấu ảnh có ghi chú, đặt trước tên file trong danh sách Raw / Mask.</summary>
        private const string CommentMark = "✎ ";

        private Label lblCommentBar;

        private void SetupCommentUi()
        {
            lblCommentBar = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 30,
                Visible = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                BackColor = Color.FromArgb(254, 243, 199),
                ForeColor = Color.FromArgb(120, 53, 15),
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            pnlPreviewBody.Controls.Add(lblCommentBar);
            lblCommentBar.BringToFront();

            // Bấm vào thanh ghi chú = sửa ghi chú của ảnh đang xem.
            lblCommentBar.Click += (s, e) => EditCommentOfPreview();

            // Ghi chú đổi ở tab nào (hoặc do đổi tên / xoá ảnh) thì cả hai danh sách + thanh ghi chú
            // cập nhật ngay.
            ImageComments.Changed += OnImageCommentsChanged;
            FormClosed += (s, e) => ImageComments.Changed -= OnImageCommentsChanged;
        }

        private void OnImageCommentsChanged(IList<string> paths)
        {
            RunOnUi(() =>
            {
                RenderRawPage();
                RenderMaskPage();
                UpdateCommentBar();
            });
        }

        /// <summary>Tên hiển thị trong danh sách: có ghi chú thì thêm dấu ✎ phía trước.</summary>
        private static string ListName(string path)
        {
            return (ImageComments.Has(path) ? CommentMark : string.Empty) + Path.GetFileName(path);
        }

        /// <summary>Dòng ghi chú trong tooltip của danh sách.</summary>
        private static string CommentTooltip(string path)
        {
            string comment = ImageComments.Get(path);
            return string.IsNullOrEmpty(comment) ? string.Empty : Environment.NewLine + "Ghi chú: " + comment;
        }

        /// <summary>Thanh ghi chú dưới khung xem trước: ghi chú của ảnh Raw và / hoặc Mask đang xem.</summary>
        private void UpdateCommentBar()
        {
            if (lblCommentBar == null)
            {
                return;
            }

            var parts = new List<string>();
            string raw = ImageComments.Get(_selectedRawPath);
            string mask = ImageComments.Get(_selectedMaskPath);
            if (!string.IsNullOrEmpty(raw))
            {
                parts.Add((string.IsNullOrEmpty(mask) ? string.Empty : "Raw: ") + raw);
            }

            if (!string.IsNullOrEmpty(mask))
            {
                parts.Add((string.IsNullOrEmpty(raw) ? string.Empty : "Mask: ") + mask);
            }

            lblCommentBar.Visible = parts.Count > 0;
            lblCommentBar.Text = "✎  " + string.Join("   |   ", parts).Replace(Environment.NewLine, "  ");
        }

        private void EditCommentOfPreview()
        {
            var paths = new[] { _selectedRawPath, _selectedMaskPath }
                .Where(p => !string.IsNullOrEmpty(p) && ImageComments.Has(p))
                .ToList();

            if (paths.Count > 0)
            {
                ImageComments.Edit(this, paths);
            }
        }

        #endregion

        #region Hoàn tác / làm lại thao tác file

        private void SetupFileHistory()
        {
            FileHistory.Applied += OnFileHistoryApplied;
            FormClosed += (s, e) => FileHistory.Applied -= OnFileHistoryApplied;
        }

        /// <summary>
        /// Ctrl+Z / Ctrl+Y ở tab này hay tab Dataset: đổi tên / xoá cập nhật thẳng danh sách như thao tác
        /// thường; file lấy lại từ Thùng rác thì quét lại thư mục để nó hiện đúng chỗ (kèm mask / kết quả).
        /// </summary>
        private void OnFileHistoryApplied(FileHistoryChange change)
        {
            RunOnUi(() =>
            {
                if (change.Renamed.Count > 0)
                {
                    OnImagesRenamed(change.Renamed);
                }

                if (change.Deleted.Count > 0)
                {
                    OnImagesDeleted(change.Deleted);
                }

                if (change.Restored.Count > 0)
                {
                    ScanFolder(false);
                    foreach (var path in change.Restored)
                    {
                        Log("Đã lấy lại từ Thùng rác: " + path);
                    }

                    UpdateStatus($"Hoàn tác: đã lấy lại {change.Restored.Count} ảnh từ Thùng rác.");
                }
            });
        }

        #endregion

        #region Giao diện Sáng / Tối

        private ToolStripStatusLabel tsslTheme;

        /// <summary>Nút "☾ Dark / ☀ Light" trên thanh trạng thái + gắn giao diện cho cửa sổ chính.</summary>
        private void SetupTheme()
        {
            tsslTheme = new ToolStripStatusLabel
            {
                IsLink = false,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Margin = new Padding(6, 3, 6, 2),
                ToolTipText = "Đổi giao diện Sáng / Tối (được nhớ cho lần mở sau)"
            };
            tsslTheme.Click += (s, e) => ToggleTheme();
            statusStrip.Items.Insert(statusStrip.Items.IndexOf(tsslClock), tsslTheme);
            UpdateThemeToggle();

            VisionImageTool.Theme.Attach(this);
            VisionImageTool.Theme.Changed += OnThemeChanged;
            FormClosed += (s, e) => VisionImageTool.Theme.Changed -= OnThemeChanged;
        }

        private void ToggleTheme()
        {
            VisionImageTool.Theme.Set(VisionImageTool.Theme.IsDark ? AppTheme.Light : AppTheme.Dark);
            UpdateThemeToggle();
            SaveSettings();
        }

        private void UpdateThemeToggle()
        {
            if (tsslTheme != null)
            {
                tsslTheme.Text = VisionImageTool.Theme.IsDark ? "☀ Light" : "☾ Dark";
            }
        }

        /// <summary>Màu từng dòng danh sách không có sự kiện đổi màu - vẽ lại để quy đổi.</summary>
        private void OnThemeChanged()
        {
            RenderRawPage();
            RenderMaskPage();
        }

        #endregion

        #region Tab: tới ảnh chưa có Mask

        /// <summary>
        /// Tab / Shift+Tab khi danh sách Raw hoặc khung xem trước đang focus: chọn ảnh Raw kế tiếp / trước
        /// đó CHƯA CÓ MASK (chưa được UnitX xử lý), chuyển trang nếu cần và hiển thị ảnh.
        ///
        /// "Chưa có Mask" = không có file mask ghép cặp (PushItem.MaskPath == null, kiểm tra lại trên đĩa
        /// phòng khi mask vừa về). Kết quả OK / NG đọc từ tên file mask, nên ảnh đã có OK / NG là ảnh
        /// đã có mask; ảnh camera nền xám "Chưa có kết quả" là ảnh chưa có mask.
        /// </summary>
        private bool HandleMissingMaskKey(Keys keyData)
        {
            if (keyData != Keys.Tab && keyData != (Keys.Shift | Keys.Tab))
            {
                return false;
            }

            bool inImageArea = lvRawImage.Focused || lvMaskImage.Focused || picPreview.Focused
                || (picPreviewMask != null && picPreviewMask.Focused);
            if (!inImageArea)
            {
                return false;   // Tab ở các ô nhập liệu vẫn chuyển focus như bình thường
            }

            JumpToMissingMask(keyData == Keys.Tab);
            return true;
        }

        private void JumpToMissingMask(bool forward)
        {
            int count = _rawItems.Count;
            if (count == 0)
            {
                UpdateStatus("Danh sách Raw trống.");
                return;
            }

            int current = -1;
            int selected;
            if (_selectedRawPath != null && _rawIndexByPath.TryGetValue(_selectedRawPath, out selected))
            {
                current = selected;
            }

            int found = -1;
            bool wrapped = false;
            for (int step = 1; step <= count; step++)
            {
                int raw = current + (forward ? step : -step);
                int index = ((raw % count) + count) % count;
                if (forward ? raw >= count : raw < 0)
                {
                    wrapped = true;
                }

                if (IsMissingMask(_rawItems[index]))
                {
                    found = index;
                    break;
                }
            }

            int missing = _rawItems.Count(i => i.MaskPath == null);
            if (found < 0)
            {
                UpdateStatus("✔ Đã duyệt hết: mọi ảnh Raw đều đã có Mask / kết quả.");
                return;
            }

            if (found == current)
            {
                UpdateStatus("Chỉ còn đúng ảnh đang chọn là chưa có Mask.");
                return;
            }

            _rawPage = found / PageSize;
            RenderRawPage();
            ReselectRow(lvRawImage, found, _rawPage);
            lvRawImage.Focus();
            ApplyRawSelection();

            UpdateStatus((wrapped ? "↻ Đã quay lại " + (forward ? "đầu" : "cuối") + " danh sách - " : string.Empty)
                + $"Ảnh chưa có Mask: {Path.GetFileName(_rawItems[found].FullPath)}  ({missing} ảnh chưa có Mask)");
        }

        /// <summary>Ảnh Raw chưa có mask. Mask có thể vừa về mà danh sách chưa biết - dò lại trên đĩa.</summary>
        private bool IsMissingMask(PushItem item)
        {
            if (item.MaskPath != null)
            {
                return false;
            }

            string mask = ResolveMaskPath(item.FullPath);
            if (mask == null)
            {
                return true;
            }

            item.MaskPath = mask;
            if (item.FromCamera)
            {
                ApplyCameraResultState(item, null);
            }

            return false;
        }

        #endregion

        #region Xem song song Raw | Mask

        /// <summary>true = khung xem trước chia đôi: Raw bên trái, Mask bên phải.</summary>
        private bool _splitView;

        private Button btnViewSplit;
        private ZoomPictureBox picPreviewMask;
        private TableLayoutPanel tlpSplit;

        /// <summary>Ảnh đang nạp ở panel Raw khi xem song song - chọn lại cùng ảnh thì không đọc lại file.</summary>
        private string _splitRawShown;

        private void SetupSplitView()
        {
            // Thêm nút thứ ba "Raw | Mask" cạnh Raw / Mask (nới cột chứa nhóm nút).
            tlpPreview.ColumnStyles[1].Width = 290F;   // cột nhóm nút Raw / Mask / Raw | Mask
            tlpPreviewToggle.ColumnCount = 3;
            tlpPreviewToggle.ColumnStyles.Clear();
            tlpPreviewToggle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpPreviewToggle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpPreviewToggle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            btnViewSplit = new Button
            {
                Text = "Raw | Mask",
                Dock = DockStyle.Fill,
                Margin = btnViewMask.Margin,
                FlatStyle = FlatStyle.Flat,
                Font = btnViewMask.Font,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnViewSplit.FlatAppearance.BorderColor = btnViewMask.FlatAppearance.BorderColor;
            btnViewSplit.Click += (s, e) => SetSplitView(!_splitView);
            new ToolTip().SetToolTip(btnViewSplit, "Xem song song ảnh Raw và Mask - zoom / kéo mỗi bên độc lập");
            tlpPreviewToggle.Controls.Add(btnViewSplit, 2, 0);

            picPreviewMask = new ZoomPictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = picPreview.BackColor,
                Margin = new Padding(1, 0, 0, 0),
                TabStop = false,
                Caption = "MASK"
            };

            tlpSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.FromArgb(55, 65, 81)
            };
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        }

        /// <summary>
        /// Bật / tắt xem song song. picPreview (panel Raw) được chuyển vào bảng chia đôi rồi trả lại chỗ
        /// cũ - chế độ xem một ảnh giữ nguyên như trước.
        /// </summary>
        private void SetSplitView(bool on)
        {
            if (on == _splitView)
            {
                return;
            }

            _splitView = on;
            pnlPreviewBody.SuspendLayout();

            if (on)
            {
                pnlPreviewBody.Controls.Remove(picPreview);
                picPreview.Margin = new Padding(0, 0, 1, 0);
                picPreview.Caption = "RAW";
                tlpSplit.Controls.Add(picPreview, 0, 0);
                tlpSplit.Controls.Add(picPreviewMask, 1, 0);
                pnlPreviewBody.Controls.Add(tlpSplit);
                tlpSplit.SendToBack();
                lblPreviewHint.Visible = false;
            }
            else
            {
                tlpSplit.Controls.Remove(picPreview);
                pnlPreviewBody.Controls.Remove(tlpSplit);
                picPreview.Caption = null;
                picPreview.EmptyText = null;
                picPreview.Margin = new Padding(0);
                pnlPreviewBody.Controls.Add(picPreview);
                picPreview.SendToBack();
                ReleaseImage(picPreviewMask);
                _splitRawShown = null;
            }

            lblResult.BringToFront();
            if (lblCommentBar != null)
            {
                lblCommentBar.BringToFront();
            }

            pnlPreviewBody.ResumeLayout();

            // Ảnh đang hiện trên picPreview có thể là Mask (chế độ Mask) - nạp lại đúng ảnh cho từng bên.
            _splitRawShown = null;
            ShowSelectedImage();
        }

        /// <summary>Panel sẽ nhận ảnh Mask vừa nhận / ảnh theo chế độ hiện tại.</summary>
        private ZoomPictureBox PreviewTarget()
        {
            return _splitView && _previewMode == PreviewMode.Mask ? picPreviewMask : picPreview;
        }

        private static void ReleaseImage(ZoomPictureBox box)
        {
            var old = box.Image;
            box.Image = null;
            if (old != null)
            {
                old.Dispose();
            }
        }

        /// <summary>Xem song song: nạp ảnh Raw và Mask của dòng đang chọn vào hai panel.</summary>
        private void ShowSplitImages()
        {
            ShowSplitRaw();
            LoadInto(picPreviewMask, _selectedMaskPath, "No Mask Available\r\nẢnh này chưa có Mask / kết quả");

            string name = Path.GetFileName(_selectedRawPath ?? _selectedMaskPath ?? string.Empty);
            lblPreviewHeader.Text = name.Length == 0 ? "Xem trước ảnh - Raw | Mask" : "Xem trước ảnh - " + name + "  [Raw | Mask]";
        }

        private void ShowSplitRaw()
        {
            if (_splitRawShown != null && string.Equals(_splitRawShown, _selectedRawPath, StringComparison.OrdinalIgnoreCase)
                && picPreview.Image != null)
            {
                return;
            }

            _splitRawShown = LoadInto(picPreview, _selectedRawPath, "Chưa có ảnh Raw") ? _selectedRawPath : null;
        }

        /// <summary>Nạp một file vào panel; không có file thì để trống kèm chữ báo. True nếu đã nạp được ảnh.</summary>
        private bool LoadInto(ZoomPictureBox box, string path, string emptyText)
        {
            Bitmap bitmap = !string.IsNullOrEmpty(path) && File.Exists(path) ? LoadPreviewFromFile(path) : null;

            var old = box.Image;
            box.Image = bitmap;
            box.EmptyText = bitmap == null
                ? (string.IsNullOrEmpty(path) || !File.Exists(path) ? emptyText : "Không mở được ảnh:\r\n" + Path.GetFileName(path))
                : null;

            if (old != null)
            {
                old.Dispose();
            }

            return bitmap != null;
        }

        #endregion
    }
}
