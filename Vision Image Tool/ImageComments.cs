using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên KHÔNG phải Form: Visual Studio mở file này bằng trình soạn code, không mở Designer
    // (frmImageComment dựng giao diện bằng code).
    internal static class ImageCommentsCodeFile { }

    /// <summary>
    /// Ghi chú (comment) gắn với TỪNG ảnh, dùng chung cho tab Capture &amp; Monitor và Dataset Manager.
    ///
    /// Lưu cạnh ảnh: mỗi thư mục có một file image_comments.json { "tên file": { Text, Updated } }. Nhờ
    /// vậy cả hai tab (cùng trỏ tới một file ảnh) thấy cùng một ghi chú, và copy thư mục dataset sang
    /// máy khác là mang theo ghi chú - giống dataset_evaluation.json của nhãn đánh giá.
    ///
    /// Một ảnh hay nhiều ảnh đều đi qua Set / Remove - không có đường xử lý riêng cho "batch".
    /// </summary>
    internal static class ImageComments
    {
        public const string FileName = "image_comments.json";

        internal sealed class Entry
        {
            public string Text { get; set; }
            public DateTime Updated { get; set; }
        }

        private static readonly object Sync = new object();

        /// <summary>Thư mục -> (tên file -> ghi chú), đọc lười từ image_comments.json.</summary>
        private static readonly Dictionary<string, Dictionary<string, Entry>> Cache =
            new Dictionary<string, Dictionary<string, Entry>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Ghi chú của các ảnh vừa đổi (đường dẫn đầy đủ) - hai tab nghe để vẽ lại dấu ghi chú.</summary>
        public static event Action<IList<string>> Changed;

        public static string Get(string path)
        {
            Entry entry = Find(path);
            return entry == null ? null : entry.Text;
        }

        public static bool Has(string path)
        {
            return !string.IsNullOrEmpty(Get(path));
        }

        /// <summary>Đặt cùng một ghi chú cho mọi ảnh (rỗng = xoá). Trả về số ảnh đã đổi.</summary>
        public static int Set(IEnumerable<string> paths, string text)
        {
            text = (text ?? string.Empty).Trim();
            var changed = new List<string>();

            lock (Sync)
            {
                foreach (var group in Valid(paths).GroupBy(Path.GetDirectoryName, StringComparer.OrdinalIgnoreCase))
                {
                    var store = Load(group.Key);
                    bool dirty = false;

                    foreach (var path in group)
                    {
                        string name = Path.GetFileName(path);
                        Entry old;
                        store.TryGetValue(name, out old);

                        if (text.Length == 0)
                        {
                            if (old != null)
                            {
                                store.Remove(name);
                                dirty = true;
                                changed.Add(path);
                            }
                        }
                        else if (old == null || old.Text != text)
                        {
                            store[name] = new Entry { Text = text, Updated = DateTime.Now };
                            dirty = true;
                            changed.Add(path);
                        }
                    }

                    if (dirty)
                    {
                        Save(group.Key, store);
                    }
                }
            }

            Raise(changed);
            return changed.Count;
        }

        public static int Remove(IEnumerable<string> paths)
        {
            return Set(paths, null);
        }

        /// <summary>Ảnh bị xoá: bỏ ghi chú đi kèm (không để lại rác trong file json).</summary>
        public static void OnDeleted(IEnumerable<string> paths)
        {
            Remove(paths);
        }

        /// <summary>Ảnh đổi tên / chuyển thư mục: ghi chú đi theo file.</summary>
        public static void OnRenamed(IEnumerable<KeyValuePair<string, string>> moves)
        {
            var changed = new List<string>();

            lock (Sync)
            {
                foreach (var move in moves)
                {
                    var entry = Find(move.Key);
                    if (entry == null)
                    {
                        continue;
                    }

                    string oldDir = Path.GetDirectoryName(move.Key);
                    var oldStore = Load(oldDir);
                    oldStore.Remove(Path.GetFileName(move.Key));
                    Save(oldDir, oldStore);

                    string newDir = Path.GetDirectoryName(move.Value);
                    var newStore = Load(newDir);
                    newStore[Path.GetFileName(move.Value)] = entry;
                    Save(newDir, newStore);

                    changed.Add(move.Key);
                    changed.Add(move.Value);
                }
            }

            Raise(changed);
        }

        /// <summary>Đọc lại từ đĩa lần sau (thư mục được tải lại, file json có thể bị sửa ngoài phần mềm).</summary>
        public static void Invalidate(string directory)
        {
            lock (Sync)
            {
                if (directory == null)
                {
                    Cache.Clear();
                    return;
                }

                foreach (var key in Cache.Keys.Where(k => k.StartsWith(directory, StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    Cache.Remove(key);
                }
            }
        }

        private static IEnumerable<string> Valid(IEnumerable<string> paths)
        {
            return (paths ?? Enumerable.Empty<string>())
                .Where(p => !string.IsNullOrEmpty(p) && !string.IsNullOrEmpty(Path.GetDirectoryName(p)))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static Entry Find(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
            {
                return null;
            }

            lock (Sync)
            {
                Entry entry;
                return Load(dir).TryGetValue(Path.GetFileName(path), out entry) ? entry : null;
            }
        }

        private static Dictionary<string, Entry> Load(string directory)
        {
            Dictionary<string, Entry> store;
            if (Cache.TryGetValue(directory, out store))
            {
                return store;
            }

            store = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string file = Path.Combine(directory, FileName);
                if (File.Exists(file))
                {
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, Entry>>(File.ReadAllText(file));
                    if (loaded != null)
                    {
                        foreach (var kv in loaded.Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.Text)))
                        {
                            store[kv.Key] = kv.Value;
                        }
                    }
                }
            }
            catch
            {
                // File hỏng: coi như chưa có ghi chú, không chặn việc xem ảnh.
            }

            Cache[directory] = store;
            return store;
        }

        private static void Save(string directory, Dictionary<string, Entry> store)
        {
            string file = Path.Combine(directory, FileName);
            if (store.Count == 0)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                return;
            }

            // Ghi file tạm rồi thay: mất điện giữa chừng không làm hỏng ghi chú cũ.
            string temp = file + ".tmp";
            File.WriteAllText(temp, JsonConvert.SerializeObject(
                store.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(kv => kv.Key, kv => kv.Value),
                Formatting.Indented));

            if (File.Exists(file))
            {
                File.Replace(temp, file, null);
            }
            else
            {
                File.Move(temp, file);
            }
        }

        private static void Raise(List<string> changed)
        {
            if (changed.Count > 0 && Changed != null)
            {
                Changed(changed);
            }
        }

        /// <summary>
        /// Thêm / sửa ghi chú cho các ảnh (hỏi bằng frmImageComment). Trả về true nếu có thay đổi.
        /// Dùng chung cho một ảnh và nhiều ảnh.
        /// </summary>
        public static bool Edit(IWin32Window owner, IList<string> paths)
        {
            paths = Valid(paths).ToList();
            if (paths.Count == 0)
            {
                return false;
            }

            using (var dialog = new frmImageComment(paths))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return false;
                }

                if (dialog.CommentText.Trim().Length == 0)
                {
                    return Remove(paths) > 0;
                }

                return Set(paths, dialog.CommentText) > 0;
            }
        }

        /// <summary>Xoá ghi chú của các ảnh (hỏi xác nhận khi xoá nhiều ảnh). Trả về số ảnh đã xoá ghi chú.</summary>
        public static int Delete(IWin32Window owner, IList<string> paths)
        {
            var withComment = Valid(paths).Where(Has).ToList();
            if (withComment.Count == 0)
            {
                return 0;
            }

            if (withComment.Count > 1 && MessageBox.Show(owner,
                    $"Xoá ghi chú của {withComment.Count} ảnh đang chọn?", "Xoá ghi chú",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return 0;
            }

            return Remove(withComment);
        }
    }

    /// <summary>
    /// Hộp thêm / sửa ghi chú cho một hoặc nhiều ảnh:
    ///
    ///     Đã chọn: 15 ảnh
    ///     [danh sách tên file]
    ///     Ghi chú: ______________
    ///                  [OK] [Huỷ]
    ///
    /// Một ảnh (hoặc các ảnh cùng ghi chú) thì điền sẵn ghi chú hiện có để sửa. Các ảnh có ghi chú khác
    /// nhau thì để trống và nhắc rằng nội dung mới sẽ THAY cho tất cả. Ctrl+Enter = OK.
    /// </summary>
    internal sealed class frmImageComment : Form
    {
        private readonly TextBox txtComment = new TextBox();

        public string CommentText
        {
            get { return txtComment.Text ?? string.Empty; }
        }

        public frmImageComment(IList<string> paths)
        {
            Text = paths.Count == 1 ? "Ghi chú ảnh" : "Ghi chú cho nhiều ảnh";
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 360);
            KeyPreview = true;

            var existing = paths.Select(ImageComments.Get).ToList();
            var distinct = existing.Select(c => c ?? string.Empty).Distinct().ToList();
            int withComment = existing.Count(c => !string.IsNullOrEmpty(c));

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(14, 12, 14, 12) };
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            var lblCount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                Text = paths.Count == 1 ? "Ảnh: " + Path.GetFileName(paths[0]) : $"Đã chọn: {paths.Count} ảnh",
                Margin = new Padding(0, 0, 0, 6)
            };

            var files = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.5F),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.FromArgb(249, 250, 251),
                Text = string.Join(Environment.NewLine, paths.Take(200).Select(p =>
                    Path.GetFileName(p) + (ImageComments.Has(p) ? "   ✎ " + ImageComments.Get(p).Replace(Environment.NewLine, " ") : string.Empty)))
                    + (paths.Count > 200 ? Environment.NewLine + $"... và {paths.Count - 200} ảnh khác" : string.Empty)
            };

            var lblComment = new Label { AutoSize = true, Text = "Ghi chú:", Margin = new Padding(0, 10, 0, 4) };

            txtComment.Dock = DockStyle.Fill;
            txtComment.Multiline = true;
            txtComment.AcceptsReturn = true;
            txtComment.ScrollBars = ScrollBars.Vertical;
            txtComment.Font = new Font("Segoe UI", 10F);
            txtComment.Text = distinct.Count == 1 ? distinct[0] : string.Empty;

            string hint;
            if (paths.Count > 1 && distinct.Count > 1)
            {
                hint = $"⚠ {withComment} / {paths.Count} ảnh đang có ghi chú khác nhau - nội dung mới sẽ THAY cho tất cả ảnh đã chọn.";
            }
            else if (paths.Count > 1)
            {
                hint = "Ghi chú được áp dụng cho tất cả ảnh đã chọn. Để trống rồi bấm OK = xoá ghi chú.";
            }
            else
            {
                hint = "Để trống rồi bấm OK = xoá ghi chú.   Ctrl+Enter = OK";
            }

            var lblHint = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(490, 0),
                ForeColor = paths.Count > 1 && distinct.Count > 1 ? Color.FromArgb(180, 83, 9) : Color.FromArgb(107, 114, 128),
                Text = hint,
                Margin = new Padding(0, 6, 0, 0)
            };

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 6, 0, 0) };
            var btnCancel = new Button { Text = "Huỷ", Width = 90, Height = 30, DialogResult = DialogResult.Cancel };
            var btnOk = new Button
            {
                Text = "OK",
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
            buttons.Controls.Add(btnCancel);
            buttons.Controls.Add(btnOk);

            table.Controls.Add(lblCount, 0, 0);
            table.Controls.Add(files, 0, 1);
            table.Controls.Add(lblComment, 0, 2);
            table.Controls.Add(txtComment, 0, 3);
            table.Controls.Add(lblHint, 0, 4);
            table.Controls.Add(buttons, 0, 5);
            Controls.Add(table);

            // Nhập nhiều dòng được (Enter xuống dòng), nên KHÔNG gán AcceptButton; Ctrl+Enter = OK.
            CancelButton = btnCancel;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && e.Control)
                {
                    e.SuppressKeyPress = true;
                    DialogResult = DialogResult.OK;
                }
            };

            Shown += (s, e) =>
            {
                txtComment.Focus();
                txtComment.SelectAll();
            };

            Theme.Attach(this);
        }
    }
}
