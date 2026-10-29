using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerFolderCodeFile { }

    /// <summary>
    /// Cách đọc schema tự suy theo nội dung (không còn nút gạt chế độ):
    ///   - Không có dấu \ : &lt;Model&gt;_&lt;DK&gt;_... - chỉ đọc TÊN file.
    ///   - Có dấu \       : đọc đường dẫn tính từ thư mục ảnh - các cấp thư mục rồi tên file, kết hợp được
    ///                      (vd &lt;Raw/Mask&gt;\&lt;SPL&gt;\&lt;Name&gt;_&lt;Class&gt;_&lt;Đánh giá&gt;). Luôn quét thư mục con.
    /// Schema có cấp thư mục thì có nút "Chuyển sang quản lý theo tên…": copy dữ liệu sang thư mục mới, đặt tên
    /// file theo schema tên (xếp vào raw\ và mask\), mang theo nhãn / đối soát / ghi chú. Dữ liệu gốc giữ nguyên.
    /// Lần chuyển được ghi nhật ký (dataset_rename_journal.json) để Hoàn tác được - xem ucDatasetManager.Undo.cs.
    /// </summary>
    public partial class ucDatasetManager
    {
        /// <summary>Schema hiện hành có cấp thư mục (dấu \) - tự suy, xem SyncSchemaMode.</summary>
        private bool _folderMode;

        /// <summary>Dòng "Định dạng ảnh" trong khung ⓘ: schema không cần (không nên) ghi đuôi ảnh.</summary>
        private const string ImageFormatsHint = "\nĐịnh dạng ảnh\t.jpg .jpeg .png .bmp .tif .tiff - tự nhận, không cần ghi đuôi trong schema";

        /// <summary>Đổi tên trường cũ &lt;Eval&gt; / &lt;Evaluation&gt; thành &lt;Đánh giá&gt; (không phân biệt hoa thường).</summary>
        private static string NormalizeSchemaText(string text)
        {
            string result = System.Text.RegularExpressions.Regex.Replace(text ?? string.Empty, "<(?:Eval|Evaluation)>", "<Đánh giá>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            // Dấu \ trong TÊN trường (gõ nhầm <Raw\Mask>) -> "/"; bỏ \ thừa ở đầu (đường dẫn tính từ thư mục ảnh).
            result = System.Text.RegularExpressions.Regex.Replace(result, "<[^<>]*>", m => m.Value.Replace('\\', '/'));
            result = result.TrimStart('\\', '/', ' ');

            // Bỏ đuôi ảnh (.jpg / .bmp...) ở cuối schema: app tự nhận mọi định dạng ảnh, đuôi chỉ gây hiểu nhầm.
            int lastToken = result.LastIndexOf('>');
            int dot = result.LastIndexOf('.');
            if (dot > lastToken && lastToken >= 0 && result.IndexOf('\\', dot) < 0)
            {
                result = result.Substring(0, dot).TrimEnd();
            }

            return result;
        }

        #region Cách đọc schema (tự suy) + schema gần đây

        // Không còn nút gạt "Theo tên file / Thư mục + tên": một ô schema duy nhất, cách đọc tự suy theo dấu \
        // (có \ = đọc đường dẫn: các cấp thư mục + tên file; không có = chỉ tên file). Nhãn "Đọc: …" bên trái ô
        // schema cho biết app đang hiểu thế nào; nút 🕘 bên phải mở các schema đã dùng gần đây.

        private readonly List<string> _recentSchemas = new List<string>();
        private const int MaxRecentSchemas = 12;

        /// <summary>Cập nhật nhãn "Đọc: …" theo schema đang gõ (chưa cần Áp dụng).</summary>
        private void UpdateReadModeLabel(DatasetSchema schema)
        {
            if (schema == null)
            {
                lblReadMode.Text = "Đọc: (schema lỗi)";
                return;
            }

            if (!schema.IsFolderSchema && !schema.Fields.Any(f => f.Implicit))
            {
                lblReadMode.Text = "Đọc: Tên file";
                return;
            }

            int levels = schema.Fields.Count(f => f.InFolder);
            bool fileFields = schema.Fields.Any(f => !f.InFolder);
            lblReadMode.Text = "Đọc: Thư mục" + (fileFields ? " + tên file" : string.Empty) + " · " + levels + " cấp";
        }

        private void ShowRecentSchemas()
        {
            var menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Consolas", 9.5F) };
            string current = (txtSchema.Text ?? string.Empty).Trim();
            foreach (var schema in _recentSchemas)
            {
                string text = schema;
                var item = new ToolStripMenuItem((string.Equals(text, current, StringComparison.Ordinal) ? "✓  " : "    ") + text);
                item.Click += (s, e) =>
                {
                    txtSchema.Text = text;
                    ApplySchema(true);
                };
                menu.Items.Add(item);
            }

            if (menu.Items.Count == 0)
            {
                menu.Items.Add(new ToolStripMenuItem("(chưa có schema nào)") { Enabled = false });
            }

            menu.Items.Add(new ToolStripSeparator());
            var sample = new ToolStripMenuItem("Mẫu:  " + DatasetSchema.SampleText) { ForeColor = Color.FromArgb(100, 116, 139) };
            sample.Click += (s, e) => { txtSchema.Text = DatasetSchema.SampleText; txtSchema.Focus(); };
            menu.Items.Add(sample);

            menu.Closed += (s, e) => menu.BeginInvoke((Action)menu.Dispose);
            menu.Show(btnRecentSchemas, new System.Drawing.Point(btnRecentSchemas.Width - 2, btnRecentSchemas.Height), ToolStripDropDownDirection.BelowLeft);
        }

        /// <summary>Đưa schema lên đầu danh sách gần đây (không trùng, tối đa MaxRecentSchemas).</summary>
        private void RememberSchema(string schema)
        {
            schema = (schema ?? string.Empty).Trim();
            if (schema.Length == 0)
            {
                return;
            }

            _recentSchemas.RemoveAll(s => string.Equals(s, schema, StringComparison.Ordinal));
            _recentSchemas.Insert(0, schema);
            if (_recentSchemas.Count > MaxRecentSchemas)
            {
                _recentSchemas.RemoveRange(MaxRecentSchemas, _recentSchemas.Count - MaxRecentSchemas);
            }
        }

        /// <summary>
        /// Chế độ đi theo NỘI DUNG schema: có dấu \ thì đọc đường dẫn (cấp thư mục + tên file, kết hợp được
        /// cả hai, vd  &lt;Raw/Mask&gt;\&lt;SPL&gt;\&lt;Name&gt;_&lt;Class&gt;_&lt;Đánh giá&gt;_&lt;Lần chụp&gt;), không có thì chỉ đọc tên
        /// file. Trả về true nếu vừa phải bật quét thư mục con (cần tải lại thư mục).
        /// </summary>
        private bool SyncSchemaMode(string text)
        {
            bool folder = (text ?? string.Empty).IndexOf('\\') >= 0;
            if (folder == _folderMode)
            {
                return false;
            }

            _folderMode = folder;
            UpdateConvertButtons();
            chkSubfolders.Enabled = !folder;

            if (folder && !chkSubfolders.Checked)
            {
                chkSubfolders.Checked = true;
                return _loadedFolder != null;
            }

            return false;
        }

        #endregion

        #region Chuyển sang quản lý theo tên

        private void WireConvertButtons()
        {
            // Khoá thì vẫn hiện (xám + tooltip lý do) - nhìn là biết đang ở trạng thái nào.
            btnConvert.Click += (s, e) => ConvertToNameSchema();
            btnUndoConvert.Click += (s, e) => UndoConvert();
            UpdateConvertButtons();
        }

        private void ConvertToNameSchema()
        {
            if (_images.Count == 0 || _loadedFolder == null)
            {
                MessageBox.Show(this, "Chưa tải thư mục ảnh nào.", "Chuyển sang quản lý theo tên", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Đã chuyển một lần (còn nhật ký) thì không cho chuyển lần 2 - hoàn tác trước.
            var journal = ActiveJournal();
            if (journal != null)
            {
                MessageBox.Show(this, ConvertLockedReason(journal), "Chuyển sang quản lý theo tên", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Schema đích gợi ý: cùng các trường, nối bằng "_" (đuôi ảnh giữ theo từng file gốc).
            string target = _schema.Fields.Any(x => x.Implicit)
                ? string.Join("_", _schema.Fields.Select(x => x.Implicit ? (_schema.HasShot ? "<" + ImplicitNameField + ">" : "<Hậu tố>") : "<" + x.Name + ">"))
                : txtSchema.Text.Replace("\\", "_");

            var items = ConvertItems();

            string destination = _loadedFolder.TrimEnd(Path.DirectorySeparatorChar) + "_theo_ten";

            using (var dialog = new frmConvertToName(items, target, destination))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // Ghi nhật ký (ở cả thư mục nguồn lẫn đích) TRƯỚC khi mở thư mục mới: căn cứ để Hoàn tác và để
                // khoá chuyển lần 2, vẫn đúng sau khi tắt / mở lại app.
                if (dialog.Copied.Count > 0)
                {
                    WriteJournal(_loadedFolder, txtSchema.Text.Trim(), dialog.TargetSchema, dialog.Destination, dialog.Copied);
                }

                UpdateConvertButtons();
                if (!dialog.OpenAfter)
                {
                    return;
                }

                // Mở thư mục mới với schema đích (theo tên file - cách đọc tự suy).
                txtSchema.Text = dialog.TargetSchema;
                ApplySchema(false);
                txtFolder.Text = dialog.Destination;
                SaveSettings();
                LoadFolder();
            }
        }

        /// <summary>Mọi ảnh đang tải (kèm giá trị đã bóc tách, nhãn, ghi chú) - đầu vào của hộp thoại copy.</summary>
        private List<ConvertItem> ConvertItems()
        {
            return _images.Select(i => new ConvertItem
            {
                Source = i.FullPath,
                SourceRelative = i.RelativeDir == "." ? i.FileName : Path.Combine(i.RelativeDir, i.FileName),
                Matched = i.Matched,
                Values = i.Values,
                Type = i.Type,
                Evaluation = i.FileEvaluation,
                Suffix = i.Suffix,
                Shot = i.Shot,
                Stored = StoredValue(i),
                Comment = ImageComments.Get(i.FullPath)
            }).ToList();
        }

        #endregion
    }

    /// <summary>Một ảnh cần chuyển (dữ liệu chụp lại từ Dataset Manager, không giữ tham chiếu vào bảng).</summary>
    internal sealed class ConvertItem
    {
        public string Source;
        public string SourceRelative;
        public bool Matched;
        public Dictionary<string, string> Values;
        public string Type;
        public string Evaluation;
        public string Suffix;
        public string Shot;
        public string Stored;
        public string Comment;

        /// <summary>Đường dẫn tương đối trong thư mục đích (raw\... / mask\...), null = bỏ qua.</summary>
        public string Target;
        public string Status;
    }

    /// <summary>Hộp thoại "Chuyển sang quản lý theo tên": schema đích, thư mục đích, xem trước, copy.</summary>
    internal sealed class frmConvertToName : Form
    {
        private readonly List<ConvertItem> _items;
        private readonly TextBox txtTarget = new TextBox();
        private readonly TextBox txtDestination = new TextBox();
        private readonly CheckBox chkCarry = new CheckBox();
        private readonly Label lblSummary = new Label();
        private readonly ListView lvPreview = new ListView();
        private readonly Button btnRun = new Button();
        private readonly Timer _previewTimer = new Timer { Interval = 300 };
        private DatasetSchema _target;

        public string TargetSchema { get; private set; }
        public string Destination { get; private set; }
        public bool OpenAfter { get; private set; }

        /// <summary>Các ảnh đã copy thành công (Target = đường dẫn tương đối trong thư mục đích).</summary>
        public List<ConvertItem> Copied { get; private set; } = new List<ConvertItem>();

        public frmConvertToName(List<ConvertItem> items, string targetSchema, string destination)
        {
            _items = items;
            Text = "Chuyển sang quản lý theo tên file";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(1040, 640);
            MinimumSize = new Size(760, 480);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 6, Padding = new Padding(14, 12, 14, 12) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            txtTarget.Text = targetSchema;
            txtTarget.Font = new Font("Consolas", 10F);
            txtTarget.Dock = DockStyle.Fill;
            txtTarget.Margin = new Padding(0, 5, 0, 0);
            txtTarget.TextChanged += (s, e) => { _previewTimer.Stop(); _previewTimer.Start(); };

            txtDestination.Text = destination;
            txtDestination.Dock = DockStyle.Fill;
            txtDestination.Margin = new Padding(0, 5, 8, 0);
            txtDestination.TextChanged += (s, e) => { _previewTimer.Stop(); _previewTimer.Start(); };

            var btnBrowse = new Button { Text = "Chọn...", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 3, 0, 3) };
            btnBrowse.Click += (s, e) =>
            {
                using (var browse = new FolderBrowserDialog { Description = "Thư mục đích (nên là thư mục mới / trống)", SelectedPath = txtDestination.Text })
                {
                    if (browse.ShowDialog(this) == DialogResult.OK)
                    {
                        txtDestination.Text = browse.SelectedPath;
                    }
                }
            };

            chkCarry.Text = "Mang theo nhãn Đánh giá / Đối soát và ghi chú ảnh";
            chkCarry.Checked = true;
            chkCarry.AutoSize = true;

            lblSummary.Dock = DockStyle.Fill;
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblSummary.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);

            lvPreview.Dock = DockStyle.Fill;
            lvPreview.View = View.Details;
            lvPreview.FullRowSelect = true;
            lvPreview.BorderStyle = BorderStyle.FixedSingle;
            lvPreview.Columns.Add("Trạng thái", 190);
            lvPreview.Columns.Add("Đường dẫn cũ", 400);
            lvPreview.Columns.Add("Tên mới (trong thư mục đích)", 400);

            btnRun.Text = "Copy";
            btnRun.Dock = DockStyle.Right;
            btnRun.Width = 200;
            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.BackColor = Color.FromArgb(22, 163, 74);
            btnRun.ForeColor = Color.White;
            btnRun.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnRun.Click += async (s, e) => await RunAsync();

            var btnCancel = new Button { Text = "Đóng", Dock = DockStyle.Right, Width = 100, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            var buttons = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            buttons.Controls.Add(btnRun);
            buttons.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 8 });
            buttons.Controls.Add(btnCancel);
            CancelButton = btnCancel;

            table.Controls.Add(Caption("Schema đích (tên file)"), 0, 0);
            table.Controls.Add(txtTarget, 1, 0);
            table.SetColumnSpan(txtTarget, 2);
            table.Controls.Add(Caption("Thư mục đích"), 0, 1);
            table.Controls.Add(txtDestination, 1, 1);
            table.Controls.Add(btnBrowse, 2, 1);
            table.Controls.Add(chkCarry, 1, 2);
            table.Controls.Add(lblSummary, 0, 3);
            table.SetColumnSpan(lblSummary, 3);
            table.Controls.Add(lvPreview, 0, 4);
            table.SetColumnSpan(lvPreview, 3);
            table.Controls.Add(buttons, 0, 5);
            table.SetColumnSpan(buttons, 3);
            Controls.Add(table);

            _previewTimer.Tick += (s, e) =>
            {
                _previewTimer.Stop();
                RefreshPreview();
            };

            Theme.Attach(this);
            Shown += (s, e) => RefreshPreview();
        }

        private static Label Caption(string text)
        {
            return new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        }

        #region Tính tên mới

        private static string EvaluationText(string symbol)
        {
            switch (symbol)
            {
                case "O": return "OK";
                case "X": return "NG";
                case "Δ": return "LIMIT";
                default: return string.Empty;
            }
        }

        /// <summary>Tên file mới theo schema đích; null + lý do nếu không dựng được.</summary>
        private static string BuildName(DatasetSchema target, ConvertItem item, out string error)
        {
            error = null;
            var sb = new StringBuilder();
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var field in target.Fields)
            {
                string value;
                switch (field.Role)
                {
                    case DatasetFieldRole.Type:
                        value = item.Type == "Mask" ? "mask" : "raw";
                        break;
                    case DatasetFieldRole.Evaluation:
                        value = EvaluationText(item.Evaluation);
                        break;
                    case DatasetFieldRole.Suffix:
                        value = item.Suffix;
                        break;
                    case DatasetFieldRole.Shot:
                        value = item.Shot;
                        break;
                    default:
                        // Tên file tự đọc của schema thư mục (đã có <Lần chụp> nên không gọi là Hậu tố).
                        if (string.Equals(field.Name, ucDatasetManager.ImplicitNameField, StringComparison.OrdinalIgnoreCase))
                        {
                            value = item.Suffix;
                            if (string.IsNullOrEmpty(value))
                            {
                                error = "Thiếu tên file";
                                return null;
                            }

                            break;
                        }

                        if (!item.Values.TryGetValue(field.Name, out value) || string.IsNullOrEmpty(value))
                        {
                            error = "Thiếu trường <" + field.Name + "> (không có trong schema thư mục)";
                            return null;
                        }

                        break;
                }

                if (string.IsNullOrEmpty(value))
                {
                    if (field.InFolder)
                    {
                        // Cấp thư mục không được trống (bỏ đi sẽ làm lệch các cấp phía sau).
                        error = "Thiếu giá trị <" + field.Name + "> cho cấp thư mục";
                        return null;
                    }

                    continue;   // trường tuỳ chọn vắng -> bỏ luôn dấu phân cách đứng trước
                }

                // Ký tự không hợp lệ chỉ thay trong GIÁ TRỊ - dấu \ phân cấp thư mục của schema giữ nguyên.
                foreach (char c in invalid)
                {
                    value = value.Replace(c, '-');
                }

                sb.Append(field.LiteralBefore).Append(value);
            }

            return sb.ToString().TrimStart('\\') + Path.GetExtension(item.Source);
        }

        private void RefreshPreview()
        {
            string error;
            _target = DatasetSchema.Parse(txtTarget.Text, out error);
            if (_target != null && _target.IsFolderSchema)
            {
                _target = null;
                error = "Schema đích phải là schema TÊN FILE (không có dấu \\).";
            }

            string destination = txtDestination.Text.Trim().Trim('"');
            var used = new Dictionary<string, ConvertItem>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in _items)
            {
                item.Target = null;
                if (_target == null)
                {
                    item.Status = "✖ Schema đích lỗi";
                    continue;
                }

                if (!item.Matched)
                {
                    item.Status = "Bỏ qua - sai schema thư mục";
                    continue;
                }

                string buildError;
                string name = BuildName(_target, item, out buildError);
                if (name == null)
                {
                    item.Status = "Bỏ qua - " + buildError;
                    continue;
                }

                // Đọc lại tên mới bằng schema đích phải ra đúng các giá trị (giá trị có chứa "_" sẽ lệch).
                var check = _target.IsFolderSchema ? _target.MatchPath(name) : _target.Match(name);
                if (!check.Success || _target.Fields.Where(f => f.Role == DatasetFieldRole.Key || f.Role == DatasetFieldRole.Condition)
                        .Any(f => !string.Equals(check.Values[f.Name], item.Values[f.Name], StringComparison.OrdinalIgnoreCase)))
                {
                    item.Status = "Bỏ qua - tên mới đọc lại không khớp (giá trị có dấu phân cách?)";
                    continue;
                }

                string relative = Path.Combine(item.Type == "Mask" ? "mask" : "raw", name);
                ConvertItem first;
                if (used.TryGetValue(relative, out first))
                {
                    item.Status = "Bỏ qua - trùng tên với " + first.SourceRelative;
                    continue;
                }

                if (destination.Length > 0 && File.Exists(Path.Combine(destination, relative)))
                {
                    item.Status = "Bỏ qua - đã có ở thư mục đích";
                    continue;
                }

                used[relative] = item;
                item.Target = relative;
                item.Status = "✓ Copy";
            }

            int copy = _items.Count(i => i.Target != null);
            int skip = _items.Count - copy;
            lblSummary.Text = _target == null
                ? "✖ " + error
                : $"{copy} ảnh sẽ copy" + (skip > 0 ? $"   ·   {skip} ảnh bỏ qua (xem cột Trạng thái)" : string.Empty);
            lblSummary.ForeColor = Theme.Fore(_target == null ? Color.FromArgb(185, 28, 28) : skip > 0 ? Color.FromArgb(180, 83, 9) : Color.FromArgb(22, 163, 74));
            btnRun.Text = "Copy " + copy + " ảnh";
            btnRun.Enabled = copy > 0 && destination.Length > 0;

            lvPreview.BeginUpdate();
            try
            {
                lvPreview.Items.Clear();
                lvPreview.Items.AddRange(_items
                    .OrderBy(i => i.Target == null ? 0 : 1)
                    .Select(i => new ListViewItem(new[] { i.Status, i.SourceRelative, i.Target ?? string.Empty })
                    {
                        ForeColor = Theme.Fore(i.Target == null ? Color.FromArgb(180, 83, 9) : Color.FromArgb(31, 41, 55))
                    })
                    .ToArray());
            }
            finally
            {
                lvPreview.EndUpdate();
            }
        }

        #endregion

        #region Copy

        private async Task RunAsync()
        {
            RefreshPreview();
            string destination = txtDestination.Text.Trim().Trim('"');
            var work = _items.Where(i => i.Target != null).ToList();
            if (work.Count == 0)
            {
                return;
            }

            try
            {
                destination = Path.GetFullPath(destination);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Thư mục đích không hợp lệ: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool carry = chkCarry.Checked;
            btnRun.Enabled = false;
            txtTarget.Enabled = txtDestination.Enabled = chkCarry.Enabled = false;
            int done = 0;
            var failed = new List<string>();
            var progress = new Progress<int>(n => lblSummary.Text = $"Đang copy {n} / {work.Count} ảnh...");

            await Task.Run(() =>
            {
                foreach (var item in work)
                {
                    string target = Path.Combine(destination, item.Target);
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.Copy(item.Source, target, false);
                    }
                    catch (Exception ex)
                    {
                        failed.Add(item.SourceRelative + ": " + ex.Message);
                        item.Target = null;
                    }

                    done++;
                    if (done % 20 == 0 || done == work.Count)
                    {
                        ((IProgress<int>)progress).Report(done);
                    }
                }
            });

            var copied = work.Where(i => i.Target != null).ToList();
            Copied = copied;
            if (carry)
            {
                CarryLabels(destination, copied);
            }

            TargetSchema = txtTarget.Text.Trim();
            Destination = destination;
            string message = $"Đã copy {copied.Count} ảnh sang:{Environment.NewLine}{destination}"
                + (carry ? $"{Environment.NewLine}Nhãn / đối soát: {copied.Count(i => i.Stored != null)} ảnh · ghi chú: {copied.Count(i => !string.IsNullOrEmpty(i.Comment))} ảnh." : string.Empty)
                + (failed.Count > 0 ? $"{Environment.NewLine}{Environment.NewLine}{failed.Count} ảnh copy lỗi:{Environment.NewLine}" + string.Join(Environment.NewLine, failed.Take(8)) : string.Empty)
                + $"{Environment.NewLine}{Environment.NewLine}Mở thư mục mới trong Dataset Manager (đọc theo tên file)?{Environment.NewLine}(Có thể hoàn tác bằng nút ↶ Hoàn tác đổi tên.)";

            OpenAfter = MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo,
                failed.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information) == DialogResult.Yes;
            DialogResult = DialogResult.OK;
        }

        /// <summary>Nhãn / đối soát (dataset_evaluation.json) + ghi chú đi theo ảnh sang thư mục đích.</summary>
        private void CarryLabels(string destination, List<ConvertItem> copied)
        {
            // Nhãn đi theo ảnh vào dataset_collection.json của thư mục đích (mục "labels"); phần dữ liệu đầy đủ
            // được app tự dựng lại khi mở thư mục đích.
            string collectionPath = Path.Combine(destination, "dataset_collection.json");
            try
            {
                var root = File.Exists(collectionPath)
                    ? Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(collectionPath))
                    : new Newtonsoft.Json.Linq.JObject { ["format"] = "vision-image-tool/collection", ["version"] = 1 };
                var labels = root["labels"] as Newtonsoft.Json.Linq.JObject ?? new Newtonsoft.Json.Linq.JObject();

                foreach (var item in copied.Where(i => i.Stored != null))
                {
                    labels[item.Target] = ucDatasetManager.ExportLabel(item.Stored);
                }

                if (labels.Count > 0)
                {
                    root["labels"] = labels;
                    File.WriteAllText(collectionPath, root.ToString(Formatting.Indented), new UTF8Encoding(false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không ghi được nhãn sang thư mục đích: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            foreach (var group in copied.Where(i => !string.IsNullOrEmpty(i.Comment)).GroupBy(i => i.Comment))
            {
                ImageComments.Set(group.Select(i => Path.Combine(destination, i.Target)).ToList(), group.Key);
            }
        }

        #endregion
    }
}
