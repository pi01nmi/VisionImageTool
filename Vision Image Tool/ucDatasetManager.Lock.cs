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
    internal static class ucDatasetManagerLockCodeFile { }

    /// <summary>
    /// - Khoá dữ liệu (chỉ xem): chặn gán nhãn / đối soát (phím + ô), đổi tên, xoá, ghi chú, hoàn tác.
    ///   Hiển thị: nút khoá màu hổ phách, viền hổ phách quanh bảng, lớp phủ xám kẻ sọc chéo mờ trên mọi ô.
    /// - Phím đối soát 4 / 5 / 6 (==, !=, trống) cho mọi ô đang chọn.
    /// - Làm mới thống kê / cây / danh sách vấn đề GỘP sau khi gán nhãn (bấm phím liên tục không giật bảng).
    /// - Gợi ý schema + phím tắt: bấm biểu tượng mới hiện (không chiếm chỗ thường trực).
    /// </summary>
    public partial class ucDatasetManager
    {
        private static readonly Color LockColor = Color.FromArgb(217, 119, 6);

        private bool _locked;
        private readonly ToolTip _lockTips = new ToolTip { AutoPopDelay = 15000 };

        #region Khoá dữ liệu

        private void WireLockButton()
        {
            chkLock.CheckedChanged += (s, e) => SetLocked(chkLock.Checked, !_restoringSections);
            UpdateLockLook();
        }

        private void SetLocked(bool locked, bool save)
        {
            if (chkLock.Checked != locked)
            {
                chkLock.Checked = locked;   // gọi lại hàm này qua CheckedChanged
                return;
            }

            bool changed = _locked != locked;
            _locked = locked;

            // Khoá: bảng không tự mở ô sửa khi chọn (EditOnEnter) - đóng luôn ô đang sửa.
            // (Đổi EditMode TRƯỚC: ở EditOnEnter, EndEdit không thoát được ô đang sửa.)
            grid.EditMode = locked ? DataGridViewEditMode.EditProgrammatically : DataGridViewEditMode.EditOnEnter;
            if (locked && grid.IsCurrentCellInEditMode)
            {
                grid.CancelEdit();
                grid.EndEdit();
            }
            if (!locked && grid.CurrentCell != null && grid.Focused && !grid.CurrentCell.ReadOnly)
            {
                grid.BeginEdit(false);
            }

            UpdateLockLook();
            grid.Invalidate();

            if (save)
            {
                SaveSettings();
            }

            if (changed && save)
            {
                ShowStatusMessage(locked ? "🔒 Đã khoá dữ liệu - chỉ xem." : "🔓 Đã mở khoá - có thể chỉnh sửa.");
                StatusResetSoon();
            }
        }

        private void UpdateLockLook()
        {
            chkLock.Text = _locked ? "\U0001F512" : "\U0001F513";   // 🔒 / 🔓
            _lockTips.SetToolTip(chkLock, (_locked ? "ĐANG KHOÁ - chỉ xem. Bấm để mở khoá." : "Khoá dữ liệu - chỉ xem.")
                + Environment.NewLine + "Khi khoá: không gán nhãn / đối soát, không đổi tên, xoá, ghi chú, hoàn tác"
                + Environment.NewLine + "(tránh bấm nhầm phím tắt khi đang xem / trình chiếu).");
            chkLock.ForeColor = _locked ? Color.White : Color.FromArgb(55, 65, 81);
            chkLock.BackColor = _locked ? LockColor : Color.White;
            chkLock.Font = new Font("Segoe UI Symbol", 11F, FontStyle.Regular);

            // Viền hổ phách quanh bảng + nền vùng trống của bảng hơi xám khi khoá.
            if (_issuesSplit != null)
            {
                _issuesSplit.Panel1.Padding = _locked ? new Padding(2) : new Padding(0);
                _issuesSplit.Panel1.BackColor = _locked ? Theme.Back(LockColor) : Theme.Back(Color.White);
            }

            grid.BackgroundColor = Theme.Back(_locked ? Color.FromArgb(241, 245, 249) : Color.White);
        }

        /// <summary>Báo khi người dùng thao tác sửa trong lúc khoá.</summary>
        private void NotifyLocked()
        {
            ShowStatusMessage("🔒 Dữ liệu đang khoá (chỉ xem) - bấm nút 🔒 cạnh ô lọc để mở khoá rồi sửa.", true);
            StatusResetSoon();

            // Nháy nút khoá cho người dùng thấy.
            chkLock.BackColor = Color.FromArgb(180, 83, 9);
            var timer = new Timer { Interval = 250 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                UpdateLockLook();
            };
            timer.Start();
        }

        /// <summary>Lớp phủ khi khoá: phủ màu xám-xanh mờ + sọc chéo lên ô dữ liệu (sau khi ô đã vẽ xong).</summary>
        private void PaintLockOverlay(Graphics g, Rectangle bounds)
        {
            using (var tint = new SolidBrush(Color.FromArgb(40, 71, 85, 105)))
            using (var hatch = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(34, 71, 85, 105), Color.Transparent))
            {
                g.FillRectangle(tint, bounds);
                g.FillRectangle(hatch, bounds);
            }
        }

        #endregion

        #region Phím đối soát 4 / 5 / 6

        /// <summary>Đối soát ứng với phím: 4 = "==", 5 = "!=", 6 = trống; null nếu không phải phím đối soát.</summary>
        private static string CompareForKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.D4: case Keys.NumPad4: return CompareMatch;
                case Keys.D5: case Keys.NumPad5: return CompareDiff;
                case Keys.D6: case Keys.NumPad6: return string.Empty;
                default: return null;
            }
        }

        #endregion

        #region Làm mới gộp

        private Timer _summaryTimer;

        /// <summary>
        /// Thống kê, biểu đồ, cây, danh sách vấn đề tính lại SAU khi người dùng ngừng bấm ~0,2 giây -
        /// bấm phím gán nhãn liên tục chỉ cập nhật đúng các ô vừa đổi, không làm mới cả màn hình mỗi phím.
        /// </summary>
        private void ScheduleSummaryRefresh()
        {
            if (_summaryTimer == null)
            {
                _summaryTimer = new Timer { Interval = 200 };
                _summaryTimer.Tick += (s, e) =>
                {
                    _summaryTimer.Stop();
                    UpdateSummary();
                    RefreshTreeCounts();
                };
            }

            _summaryTimer.Stop();
            _summaryTimer.Start();
        }

        #endregion

        #region Gợi ý (bấm biểu tượng mới hiện)

        private string _schemaHint = string.Empty;
        private ToolStripDropDown _hintPopup;

        private void WireSchemaHintButton()
        {
            btnSchemaHint.Click += (s, e) => ShowHint(btnSchemaHint, _folderMode ? "Cách đọc thư mục + tên file theo schema" : "Cách đọc tên file theo schema", _schemaHint, false);
        }

        /// <summary>Nút ⓘ đứng ngay sau thẻ cuối của schema (cạnh ".jpg").</summary>
        private void PlaceSchemaHint()
        {
            var location = new System.Drawing.Point((int)Math.Ceiling(pnlSchemaChips.ContentEnd) + 4, 1);
            if (btnSchemaHint.Location != location)
            {
                btnSchemaHint.Location = location;
            }
        }

        /// <summary>Khung gợi ý nhỏ cạnh <paramref name="anchor"/>; bấm ra ngoài là đóng.</summary>
        private void ShowHint(Control anchor, string title, string text, bool above, System.Drawing.Point? at = null)
        {
            if (_hintPopup != null)
            {
                _hintPopup.Close();
                _hintPopup.Dispose();
            }

            // Nội dung: mỗi dòng "phím<TAB>ý nghĩa" thành bảng 2 cột thẳng hàng; dòng không có TAB trải cả bề ngang.
            var table = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                BackColor = Theme.Back(Color.White),
                Padding = new Padding(12, 10, 14, 10),
                Margin = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var textColor = Theme.Fore(Color.FromArgb(31, 41, 55));
            var bodyFont = new Font("Segoe UI", 9.5F);
            var keyFont = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            Func<string, Font, Color, Label> make = (s, font, color) => new Label
            {
                Text = s,
                Font = font,
                ForeColor = color,
                AutoSize = true,
                MaximumSize = new Size(560, 0),
                Margin = new Padding(0, 2, 16, 2)
            };

            var lblTitle = make(title, new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), Theme.Fore(Primary));
            lblTitle.Margin = new Padding(0, 0, 0, 6);
            table.Controls.Add(lblTitle, 0, 0);
            table.SetColumnSpan(lblTitle, 2);

            int row = 1;
            foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
            {
                int tab = line.IndexOf('\t');
                if (tab >= 0)
                {
                    table.Controls.Add(make(line.Substring(0, tab), keyFont, textColor), 0, row);
                    table.Controls.Add(make(line.Substring(tab + 1), bodyFont, textColor), 1, row);
                }
                else
                {
                    var label = make(line.Length == 0 ? " " : line, bodyFont, textColor);
                    table.Controls.Add(label, 0, row);
                    table.SetColumnSpan(label, 2);
                }

                row++;
            }

            var panel = table;
            panel.Size = table.GetPreferredSize(Size.Empty);

            var host = new ToolStripControlHost(panel) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = false, Size = panel.Size };
            _hintPopup = new ToolStripDropDown { Padding = new Padding(1), BackColor = Theme.Back(Color.FromArgb(203, 213, 225)) };
            _hintPopup.Items.Add(host);

            var point = at ?? (above ? new System.Drawing.Point(0, 0) : new System.Drawing.Point(0, anchor.Height));
            _hintPopup.Show(anchor, point, above ? ToolStripDropDownDirection.AboveRight : ToolStripDropDownDirection.BelowRight);
        }

        private static string ShortcutHelpText()
        {
            return string.Join("\n", new[]
            {
                "Shift + Click,  Shift + ↑ ↓\tchọn nhiều mẫu - các phím dưới áp dụng cho mọi ô đang chọn",
                "O / 1     D / 2     X / 3\tĐánh giá = O / Δ / X (chỉnh tay)",
                "4     5     6\tĐối soát = ==  /  !=  /  trống",
                "0 / Delete\txoá nhãn - về theo logic chung",
                "Space\txem ảnh cặp Raw ↔ Mask",
                ",  /  .\tlần chụp trước / sau (schema có <Lần chụp>)",
                "Tab / Shift + Tab\ttới ảnh chưa có Mask kế tiếp / trước đó",
                "Ctrl + M / Ctrl + Shift + M\tthêm - sửa / xoá ghi chú",
                "F2 / Shift + Delete\tđổi tên / xoá ảnh (vào Thùng rác)",
                "Ctrl + Z / Ctrl + Y\thoàn tác / làm lại đổi tên, xoá ảnh",
                "",
                "Nút khoá (cạnh ô Lọc): khi khoá, mọi phím sửa ở trên đều bị chặn - chỉ xem."
            });
        }

        #endregion
    }
}
