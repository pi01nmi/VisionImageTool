using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code (xem ghi chú đầu frmMainV1.View.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1DuplicateCodeFile { }

    internal enum DuplicateCaptureAction
    {
        /// <summary>Xoá ảnh cũ của mẫu (vào Thùng rác, Ctrl+Z lấy lại được) rồi lưu ảnh mới.</summary>
        Overwrite,

        /// <summary>Giữ ảnh cũ, KHÔNG lưu ảnh mới (vẫn hiện kết quả trên màn hình).</summary>
        KeepOld,

        /// <summary>Giữ cả hai: ảnh mới lưu thêm hậu tố _1, _2...</summary>
        SaveNew
    }

    /// <summary>
    /// Ảnh camera trùng TÊN MẪU với ảnh đã lưu (cùng tên, cùng hậu tố - bỏ qua _OK / _NG / _LIMIT): hỏi
    /// người dùng thay vì âm thầm ghi đè / sinh thêm một kết quả thứ hai cho cùng mẫu (thường do quên
    /// đổi tên mẫu, hậu tố đang để "Không có").
    ///
    /// Callback UnitX chạy trên luồng nền: hộp thoại mở trên luồng giao diện (Invoke) và callback đợi.
    /// Trong lúc hỏi, vòng Trigger liên tục / repeat tạm dừng (_triggerGate) cho tới khi người dùng chọn.
    /// Tick "áp dụng cho các lần sau" thì nhớ lựa chọn tới khi đổi tên mẫu hoặc tắt ứng dụng.
    /// </summary>
    public partial class frmMainV1
    {
        private readonly ManualResetEventSlim _triggerGate = new ManualResetEventSlim(true);
        private readonly object _duplicateAskLock = new object();
        private DuplicateCaptureAction? _duplicatePolicy;
        private string _duplicatePolicyName;

        /// <summary>
        /// Xử lý trùng tên trước khi lưu ảnh camera. false = không lưu lần này; có thể đổi
        /// <paramref name="nameTail"/> (lưu thêm hậu tố).
        /// </summary>
        private bool ResolveDuplicateCapture(string baseName, ref string nameTail)
        {
            if (FindSampleFiles(baseName, nameTail).Count == 0)
            {
                return true;
            }

            DuplicateCaptureAction action;
            List<string> existing;

            lock (_duplicateAskLock)
            {
                // Có thể vừa được xử lý ở lượt hỏi trước (đang chờ khoá) - dò lại.
                existing = FindSampleFiles(baseName, nameTail);
                if (existing.Count == 0)
                {
                    return true;
                }

                string template = _imageNameTemplate ?? string.Empty;
                if (_duplicatePolicy != null && string.Equals(_duplicatePolicyName, template, StringComparison.Ordinal))
                {
                    action = _duplicatePolicy.Value;
                }
                else
                {
                    _duplicatePolicy = null;
                    _triggerGate.Reset();
                    try
                    {
                        bool remember = false;
                        var answer = DuplicateCaptureAction.SaveNew;
                        string name = baseName;
                        var files = existing;
                        Action ask = () =>
                        {
                            using (var dialog = new frmDuplicateCapture(name, files))
                            {
                                dialog.ShowDialog(this);
                                answer = dialog.Action;
                                remember = dialog.Remember;
                            }
                        };

                        if (InvokeRequired)
                        {
                            Invoke(ask);
                        }
                        else
                        {
                            ask();
                        }

                        action = answer;
                        if (remember)
                        {
                            _duplicatePolicy = action;
                            _duplicatePolicyName = template;
                            Log("Trùng tên mẫu: nhớ lựa chọn '" + ActionText(action) + "' cho các lần sau (tới khi đổi tên mẫu).");
                        }
                    }
                    finally
                    {
                        _triggerGate.Set();
                    }
                }
            }

            switch (action)
            {
                case DuplicateCaptureAction.KeepOld:
                    LogWarning("Trùng tên mẫu '" + baseName + nameTail + "': giữ ảnh cũ, KHÔNG lưu ảnh mới.");
                    return false;

                case DuplicateCaptureAction.SaveNew:
                    string tail = NextFreeTail(baseName, nameTail);
                    Log("Trùng tên mẫu '" + baseName + nameTail + "': lưu thêm với hậu tố " + tail.Substring(nameTail.Length) + ".");
                    nameTail = tail;
                    return true;

                default:
                    // Xoá vào Thùng rác trên luồng giao diện (SHFileOperation cần luồng STA) - callback đợi xong.
                    var deleted = new List<string>();
                    var failed = new List<string>();
                    Action recycle = () =>
                    {
                        List<string> notDeleted;
                        deleted = ImageFileOps.DeleteToRecycleBin(this, existing, out notDeleted);
                        failed = notDeleted;
                        FileHistory.RecordDelete(deleted, deleted.ToDictionary(p => p, ImageComments.Get, StringComparer.OrdinalIgnoreCase));
                        ImageComments.OnDeleted(deleted);
                        if (deleted.Count > 0)
                        {
                            OnImagesDeleted(deleted);
                        }
                    };

                    if (InvokeRequired)
                    {
                        Invoke(recycle);
                    }
                    else
                    {
                        recycle();
                    }

                    LogWarning("Trùng tên mẫu '" + baseName + nameTail + "': ghi đè - " + deleted.Count + " ảnh cũ vào Thùng rác"
                        + (failed.Count > 0 ? ", " + failed.Count + " ảnh KHÔNG xoá được (đang mở?)" : string.Empty) + ".");
                    return true;
            }
        }

        private static string ActionText(DuplicateCaptureAction action)
        {
            switch (action)
            {
                case DuplicateCaptureAction.Overwrite: return "Ghi đè";
                case DuplicateCaptureAction.KeepOld: return "Giữ ảnh cũ";
                default: return "Lưu thêm hậu tố";
            }
        }

        /// <summary>
        /// Ảnh raw + mask đã lưu của CÙNG MẪU: {tên}[_raw | _mask[_N]][_OK | _NG | _LIMIT]{hậu tố}.{ảnh} trong
        /// PATH\raw và PATH\mask (ảnh camera lưu thẳng ở đó, không có thư mục con).
        /// </summary>
        private List<string> FindSampleFiles(string baseName, string nameTail)
        {
            var result = new List<string>();
            string root = GetOutputRoot();
            var pattern = new Regex("^" + Regex.Escape(baseName) + @"(?:_raw|_mask(?:_\d+)?)?(?:_(?:OK|NG|LIMIT))?"
                + Regex.Escape(nameTail ?? string.Empty) + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            foreach (var folder in new[] { Path.Combine(root, RawFolderName), Path.Combine(root, MaskFolderName) })
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                try
                {
                    result.AddRange(Directory.EnumerateFiles(folder, baseName + "*")
                        .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
                            && pattern.IsMatch(Path.GetFileNameWithoutExtension(f))));
                }
                catch (Exception ex)
                {
                    Log("Không dò được ảnh trùng tên trong " + folder + ": " + ex.Message);
                }
            }

            return result;
        }

        /// <summary>Hậu tố còn trống: {hậu tố}_1, _2, ...</summary>
        private string NextFreeTail(string baseName, string nameTail)
        {
            for (int n = 1; n < 10000; n++)
            {
                string tail = nameTail + "_" + n;
                if (FindSampleFiles(baseName, tail).Count == 0)
                {
                    return tail;
                }
            }

            return nameTail + "_" + DateTime.Now.ToString("HHmmssfff");
        }
    }

    /// <summary>Hộp thoại hỏi khi ảnh camera trùng tên mẫu đã lưu.</summary>
    internal sealed class frmDuplicateCapture : Form
    {
        private readonly CheckBox chkRemember = new CheckBox();

        public DuplicateCaptureAction Action { get; private set; }

        public bool Remember
        {
            get { return chkRemember.Checked; }
        }

        public frmDuplicateCapture(string sampleName, IList<string> existing)
        {
            Text = "Ảnh trùng tên mẫu";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(600, 300);
            Action = DuplicateCaptureAction.SaveNew;   // đóng bằng X / Esc: an toàn nhất - không mất ảnh nào

            var title = new Label
            {
                Text = "⚠  Đã có ảnh của mẫu \"" + sampleName + "\"",
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 83, 9),
                AutoSize = false,
                AutoEllipsis = true,
                Bounds = new Rectangle(16, 12, 568, 26)
            };

            var message = new Label
            {
                Text = "Ảnh vừa chụp trùng tên mẫu với " + existing.Count + " ảnh đã lưu (có thể do quên đổi tên mẫu):",
                AutoSize = false,
                Bounds = new Rectangle(16, 42, 568, 22)
            };

            var list = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                TabStop = false,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(249, 250, 251),
                Font = new Font("Consolas", 9.5F),
                Text = string.Join(Environment.NewLine, existing.Select(p => Path.GetFileName(Path.GetDirectoryName(p)) + "\\" + Path.GetFileName(p))),
                Bounds = new Rectangle(16, 66, 568, 96)
            };

            var question = new Label
            {
                Text = "Xử lý ảnh mới thế nào?  (Trigger liên tục / repeat nếu đang chạy sẽ tạm dừng chờ bạn chọn)",
                AutoSize = false,
                Bounds = new Rectangle(16, 170, 568, 22)
            };

            var btnSaveNew = MakeButton("Lưu thêm _1, _2...", DuplicateCaptureAction.SaveNew, new Rectangle(16, 200, 180, 36), true);
            var btnKeep = MakeButton("Giữ ảnh cũ, bỏ ảnh mới", DuplicateCaptureAction.KeepOld, new Rectangle(210, 200, 180, 36), false);
            var btnOverwrite = MakeButton("Ghi đè ảnh cũ", DuplicateCaptureAction.Overwrite, new Rectangle(404, 200, 180, 36), false);
            btnOverwrite.ForeColor = Color.FromArgb(185, 28, 28);
            new ToolTip().SetToolTip(btnOverwrite, "Ảnh cũ của mẫu chuyển vào Thùng rác (Ctrl+Z ở danh sách ảnh để lấy lại), rồi lưu ảnh mới.");

            chkRemember.Text = "Áp dụng cho các lần trùng sau (tới khi đổi tên mẫu hoặc tắt ứng dụng)";
            chkRemember.AutoSize = true;
            chkRemember.Location = new System.Drawing.Point(16, 252);

            Controls.AddRange(new Control[] { title, message, list, question, btnSaveNew, btnKeep, btnOverwrite, chkRemember });
            AcceptButton = btnSaveNew;
            Theme.Attach(this);
        }

        private Button MakeButton(string text, DuplicateCaptureAction action, Rectangle bounds, bool primary)
        {
            var button = new Button
            {
                Text = text,
                Bounds = bounds,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = primary ? Color.FromArgb(37, 99, 235) : Color.White,
                ForeColor = primary ? Color.White : Color.FromArgb(31, 41, 55)
            };
            button.FlatAppearance.BorderColor = primary ? Color.FromArgb(37, 99, 235) : Color.FromArgb(209, 213, 219);
            button.Click += (s, e) =>
            {
                Action = action;
                DialogResult = DialogResult.OK;
            };
            return button;
        }
    }
}
