using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Cửa sổ nhật ký (log) tách rời của form chính.
    ///
    /// Form chính giữ toàn bộ nội dung log trong bộ nhớ và chỉ đẩy sang cửa sổ này khi nó đang mở.
    /// Đóng cửa sổ log KHÔNG làm mất log (chỉ Hide).
    ///
    /// Mỗi dòng có mức Info / Warning / Error: cảnh báo nền cam nhạt + chữ cam, lỗi nền đỏ nhạt + chữ
    /// đỏ, dòng tiêu đề khối ("=== ...") chữ xanh. Lọc theo mức (kèm số dòng mỗi mức) và theo từ khoá.
    /// </summary>
    public partial class frmLogV1 : Form
    {
        /// <summary>Được form chính gán để nút "Xóa log" xoá cả bộ đệm log bên form chính.</summary>
        public Action ClearRequested;

        /// <summary>Toàn bộ dòng log (kể cả dòng đang bị lọc ẩn).</summary>
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        private readonly Timer _searchTimer = new Timer { Interval = 250 };
        private Font _boldFont;

        public frmLogV1()
        {
            InitializeComponent();
            Theme.Attach(this);
            _boldFont = new Font(lvLog.Font, FontStyle.Bold);
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); Rebuild(); };
            SetCue(txtSearch, "Tìm trong nội dung log...");

            // Màu từng dòng (cảnh báo / lỗi) quy đổi theo giao diện Sáng / Tối - đổi giao diện thì vẽ lại.
            Action rebuild = () => { if (!IsDisposed) Rebuild(); };
            Theme.Changed += rebuild;
            Disposed += (s, e) => Theme.Changed -= rebuild;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private static void SetCue(TextBox box, string text)
        {
            const int EM_SETCUEBANNER = 0x1501;
            box.HandleCreated += (s, e) => SendMessage(box.Handle, EM_SETCUEBANNER, IntPtr.Zero, text);
        }

        /// <summary>Nạp lại toàn bộ log (dùng khi mở cửa sổ lần đầu / mở lại).</summary>
        internal void LoadLines(IEnumerable<LogEntry> lines)
        {
            _entries.Clear();
            _entries.AddRange(lines);
            Rebuild();
        }

        /// <summary>Thêm một lô dòng log mới vào cuối danh sách.</summary>
        internal void AppendLines(IList<LogEntry> lines, int maxLines)
        {
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            _entries.AddRange(lines);
            int overflow = _entries.Count - maxLines;
            if (overflow > 0)
            {
                _entries.RemoveRange(0, overflow);
            }

            lvLog.BeginUpdate();
            try
            {
                var rows = lines.Where(Visible).Select(CreateRow).ToArray();

                // Giữ số dòng trong tầm kiểm soát: cắt bớt phần đầu thay vì để ListView phình vô hạn.
                int extra = lvLog.Items.Count + rows.Length - maxLines;
                for (int i = 0; i < extra && lvLog.Items.Count > 0; i++)
                {
                    lvLog.Items.RemoveAt(0);
                }

                lvLog.Items.AddRange(rows);
                ScrollToEnd();
            }
            finally
            {
                lvLog.EndUpdate();
            }

            UpdateCounts();
        }

        public void ClearView()
        {
            _entries.Clear();
            lvLog.Items.Clear();
            UpdateCounts();
        }

        private bool Visible(LogEntry entry)
        {
            if ((entry.Level == AppLogLevel.Info && !chkInfo.Checked)
                || (entry.Level == AppLogLevel.Warning && !chkWarning.Checked)
                || (entry.Level == AppLogLevel.Error && !chkError.Checked))
            {
                return false;
            }

            string term = txtSearch.Text.Trim();
            return term.Length == 0 || (entry.Message ?? string.Empty).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ListViewItem CreateRow(LogEntry entry)
        {
            var row = new ListViewItem(entry.Time) { UseItemStyleForSubItems = true, Tag = entry };
            row.SubItems.Add(entry.LevelText);
            row.SubItems.Add(entry.Message ?? string.Empty);

            switch (entry.Level)
            {
                case AppLogLevel.Warning:
                    row.BackColor = AppLog.WarningBack;
                    row.ForeColor = AppLog.WarningText;
                    break;
                case AppLogLevel.Error:
                    row.BackColor = AppLog.ErrorBack;
                    row.ForeColor = AppLog.ErrorText;
                    row.Font = _boldFont;
                    break;
                default:
                    if (AppLog.IsSection(entry.Message))
                    {
                        row.ForeColor = AppLog.SectionText;
                        row.Font = _boldFont;
                    }
                    else
                    {
                        row.ForeColor = AppLog.InfoText;
                    }

                    break;
            }

            return row;
        }

        private void Rebuild()
        {
            lvLog.BeginUpdate();
            try
            {
                lvLog.Items.Clear();
                lvLog.Items.AddRange(_entries.Where(Visible).Select(CreateRow).ToArray());
                ScrollToEnd();
            }
            finally
            {
                lvLog.EndUpdate();
            }

            UpdateCounts();
        }

        private void UpdateCounts()
        {
            chkInfo.Text = $"Thông tin ({_entries.Count(e => e.Level == AppLogLevel.Info)})";
            chkWarning.Text = $"Cảnh báo ({_entries.Count(e => e.Level == AppLogLevel.Warning)})";
            chkError.Text = $"Lỗi ({_entries.Count(e => e.Level == AppLogLevel.Error)})";
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            // Gõ tìm kiếm: đợi ngừng gõ rồi mới lọc (5000 dòng).
            if (sender == txtSearch)
            {
                _searchTimer.Stop();
                _searchTimer.Start();
                return;
            }

            Rebuild();
        }

        private void ScrollToEnd()
        {
            if (chkAutoScroll.Checked && lvLog.Items.Count > 0)
            {
                lvLog.Items[lvLog.Items.Count - 1].EnsureVisible();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearView();

            var handler = ClearRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Text file (*.txt)|*.txt|Tất cả (*.*)|*.*";
                dlg.FileName = "vision_log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    // Lưu đúng những dòng đang hiển thị (theo bộ lọc hiện tại).
                    var sb = new StringBuilder();
                    foreach (var entry in _entries.Where(Visible))
                    {
                        sb.Append(entry.Time).Append('\t').Append(entry.LevelText).Append('\t').AppendLine(entry.Message);
                    }

                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show(this, "Đã lưu log: " + dlg.FileName, "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không lưu được log:" + Environment.NewLine + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void frmLogV1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Người dùng bấm X: chỉ ẩn đi để mở lại tức thì (và không mất nội dung đang xem).
            // Khi ứng dụng thoát (FormOwnerClosing/ApplicationExitCall) thì cho đóng thật.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
