using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl / Form: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (Designer chỉ mở ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerExportOrderCodeFile { }

    /// <summary>
    /// Nút ⬇ Xuất: hộp thoại xác nhận schema + KÉO THẢ thứ tự trường khi xuất, rồi chọn Excel hoặc JSON.
    /// Thứ tự chỉ là lớp sắp xếp của file xuất ra: không copy / đổi tên ảnh, không đổi schema, không nạp lại -
    /// xuất xong dataset giữ nguyên. Áp dụng cho:
    ///   - Excel: thứ tự các cột thông tin (trường Key + Hậu tố); ĐK là nhóm cột, Raw/Mask · Đánh giá · Lần chụp
    ///     nằm trong từng nhóm nên vị trí của chúng không đổi cột Excel (chỉ đổi tên ví dụ trong hộp thoại).
    ///   - JSON (cả bản xuất tay lẫn dataset.json tự cập nhật): thứ tự các trường Key trong mỗi hàng.
    /// Cột thông tin (Key / Hậu tố) và Lần chụp có thể BỎ khi xuất (✕); ĐK, Raw/Mask, Đánh giá là bắt buộc; luôn còn >= 1 Key.
    /// Thứ tự + cột bỏ lưu trong dataset_manager.json (ExportOrder / ExportHidden); schema đổi tập trường thì tự về thứ tự schema.
    /// </summary>
    public partial class ucDatasetManager
    {
        /// <summary>Tên trường theo thứ tự xuất người dùng đã xác nhận (null = theo schema).</summary>
        private List<string> _exportOrder;

        /// <summary>Mọi trường của schema theo thứ tự xuất (thứ tự đã lưu không còn khớp schema thì theo schema).</summary>
        private List<DatasetField> ExportFieldsAll()
        {
            var fields = _schema == null ? new List<DatasetField>() : _schema.Fields.ToList();
            if (_exportOrder == null || _exportOrder.Count != fields.Count
                || !new HashSet<string>(_exportOrder, StringComparer.OrdinalIgnoreCase).SetEquals(fields.Select(f => f.Name)))
            {
                return fields;
            }

            return _exportOrder.Select(n => fields.First(f => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        /// <summary>Cột thông tin người dùng đã bỏ khi xuất (null = xuất đủ). Chỉ trường Key / Hậu tố bỏ được.</summary>
        private List<string> _exportHidden;
        /// <summary>
        /// Trường bỏ được khi xuất: cột thông tin (Key / Hậu tố) và &lt;Lần chụp&gt; (bỏ = mỗi ĐK chỉ xuất một cặp Raw / Mask:
        /// ảnh đầu tiên theo tên file). ĐK, Raw/Mask, Đánh giá là bắt buộc.
        /// </summary>
        internal static bool IsRemovableExportRole(DatasetFieldRole role)
        {
            return IsInfoRole(role) || role == DatasetFieldRole.Shot;
        }

        /// <summary>Vai trò của cột thông tin trong Excel / JSON (trường Key + Hậu tố).</summary>
        internal static bool IsInfoRole(DatasetFieldRole role)
        {
            return role == DatasetFieldRole.Key || role == DatasetFieldRole.Suffix;
        }

        /// <summary>Schema có &lt;Lần chụp&gt; nhưng người dùng đã bỏ khi xuất: mỗi ĐK chỉ xuất ảnh đầu tiên theo tên file.</summary>
        private bool ExportSingleShot()
        {
            var shot = _schema == null ? null : _schema.Fields.FirstOrDefault(f => f.Role == DatasetFieldRole.Shot);
            return shot != null && _exportHidden != null && _exportHidden.Contains(shot.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cặp Raw / Mask của lần chụp đầu tiên theo tên file (so theo ảnh Raw, thiếu thì Mask) - dùng khi xuất bỏ
        /// &lt;Lần chụp&gt;. Block không có &lt;Lần chụp&gt; thì chính cặp Raw / Mask của block.
        /// </summary>
        private static DatasetImage[] FirstShotPair(DatasetBlock block)
        {
            if (block.Shots == null)
            {
                return new[] { block.Raw, block.Mask };
            }

            var shot = block.Shots
                .Where(s => s.Main != null)
                .OrderBy(s => (s.Raw ?? s.Mask).FileName, NaturalComparer.Instance)
                .FirstOrDefault();
            return shot == null ? new DatasetImage[] { null, null } : new[] { shot.Raw, shot.Mask };
        }

        /// <summary>Cột thông tin của Excel / JSON (trường Key + Hậu tố) theo thứ tự xuất, trừ các cột đã bỏ.</summary>
        private List<DatasetField> ExportInfoFields()
        {
            var info = ExportFieldsAll().Where(f => IsInfoRole(f.Role)).ToList();
            if (_exportHidden == null || _exportHidden.Count == 0)
            {
                return info;
            }

            var hidden = new HashSet<string>(_exportHidden, StringComparer.OrdinalIgnoreCase);
            var shown = info.Where(f => !hidden.Contains(f.Name)).ToList();

            // Luôn còn ít nhất một trường định danh (Key) - cấu hình cũ bỏ hết thì xuất đủ.
            return shown.Any(f => f.Role == DatasetFieldRole.Key) ? shown : info;
        }

        private static string InfoValue(DatasetRow row, DatasetField field)
        {
            if (field.Role == DatasetFieldRole.Suffix)
            {
                return row.Suffix;
            }

            string value;
            return row.Values.TryGetValue(field.Name, out value) ? value : string.Empty;
        }

        /// <summary>Giá trị của một trường ở một ảnh (cho tên ví dụ trong hộp thoại).</summary>
        private static string ExampleValue(DatasetImage image, DatasetField field)
        {
            string value;
            switch (field.Role)
            {
                case DatasetFieldRole.Type: return image.Type;
                case DatasetFieldRole.Evaluation:
                    switch (image.FileEvaluation)
                    {
                        case "O": return "OK";
                        case "X": return "NG";
                        case "Δ": return "LIMIT";
                        default: return string.Empty;
                    }
                case DatasetFieldRole.Suffix: return image.Suffix;
                case DatasetFieldRole.Shot: return image.Shot;
                default: return image.Values.TryGetValue(field.Name, out value) ? value : string.Empty;
            }
        }

        private void ShowExportDialog()
        {
            if (_schema == null || _images.Count == 0)
            {
                MessageBox.Show(this, "Chưa quét thư mục ảnh nào. Chọn thư mục rồi bấm 'Quét thư mục' trước.", "Xuất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sample = _images.FirstOrDefault(i => i.Matched);
            var chips = _schema.Fields.Select(f => new ExportChip
            {
                Name = f.Name,
                Role = f.Role,
                InFolder = f.InFolder,
                Example = sample == null ? null : ExampleValue(sample, f)
            }).ToList();

            var names = new HashSet<string>(_schema.Fields.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
            var hidden = (_exportHidden ?? new List<string>()).Where(names.Contains).ToList();
            using (var dialog = new frmExportSchema(txtSchema.Text.Trim(), chips, ExportFieldsAll().Select(f => f.Name).ToList(), hidden, _rows.Count))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // Đúng thứ tự schema thì không lưu (schema đổi sau này vẫn theo schema).
                var order = dialog.Order;
                _exportOrder = order.SequenceEqual(_schema.Fields.Select(f => f.Name), StringComparer.OrdinalIgnoreCase) ? null : order;
                _exportHidden = dialog.Hidden.Count == 0 ? null : dialog.Hidden;
                SaveSettings();
                ScheduleDatasetJson();   // dataset.json tự cập nhật theo thứ tự mới

                if (!ConfirmExportWithIssues(dialog.Choice == ExportChoice.Json ? "JSON" : "Excel"))
                {
                    return;
                }

                if (dialog.Choice == ExportChoice.Json)
                {
                    ExportDatasetJson();
                }
                else
                {
                    ExportExcel();
                }
            }
        }

        /// <summary>
        /// Dữ liệu còn cảnh báo (khung Vấn đề dữ liệu) thì hỏi trước khi xuất: Yes = vẫn xuất, No = mở danh sách
        /// vấn đề để sửa. Không có vấn đề thì xuất luôn.
        /// </summary>
        private bool ConfirmExportWithIssues(string format)
        {
            if (_issues == null || _issues.Count == 0)
            {
                return true;
            }

            string nl = Environment.NewLine;
            string summary = string.Join(nl, _issues
                .GroupBy(i => i.Kind)
                .OrderBy(g => g.Key)
                .Select(g => "   •  " + IssueKindTexts[(int)g.Key] + ": " + g.Count()));
            var answer = MessageBox.Show(this,
                "Dữ liệu đang có " + _issues.Count + " cảnh báo:" + nl + summary + nl + nl
                + "Ảnh sai schema không có trong file xuất; mẫu thiếu ảnh / thiếu đánh giá sẽ để trống ô tương ứng." + nl + nl
                + "Vẫn xuất " + format + "?" + nl
                + "Yes = vẫn xuất   •   No = xem danh sách vấn đề để sửa",
                "Cảnh báo trước khi xuất " + format, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer == DialogResult.Yes)
            {
                return true;
            }

            chkShowIssues.Checked = true;
            lvIssues.Focus();
            return false;
        }
    }

    internal enum ExportChoice { Excel, Json }

    internal sealed class ExportChip
    {
        public string Name;
        public DatasetFieldRole Role;
        public bool InFolder;
        public string Example;
    }

    /// <summary>
    /// Hộp thoại Xuất: schema đang dùng (ô chữ chỉ đọc), [Thay đổi thứ tự cột] mở ra thẻ trường kéo thả + ✕ để bỏ cột thông tin
    /// (trường bắt buộc có 🔒), xem trước cột xuất, chọn Excel / JSON.
    /// </summary>
    internal sealed class frmExportSchema : Form
    {
        private readonly List<ExportChip> _chips;
        private readonly List<string> _schemaOrder;
        private readonly HashSet<string> _hidden;
        private readonly FlowLayoutPanel flpChips = new FlowLayoutPanel();
        private readonly Label lblColumns = new Label();
        private readonly Button btnExcel;
        private readonly Button btnJson;
        private readonly Button btnToggleOrder;
        private readonly Button btnReset;
        private readonly Label lblOrderHint;
        private readonly TableLayoutPanel _table;
        private readonly ToolTip _tips = new ToolTip();

        public List<string> Order { get; private set; }

        /// <summary>Cột thông tin bị bỏ khi xuất.</summary>
        public List<string> Hidden { get; private set; }

        public ExportChoice Choice { get; private set; }

        public frmExportSchema(string schemaText, List<ExportChip> chips, List<string> order, List<string> hidden, int rowCount)
        {
            _chips = chips;
            _schemaOrder = chips.Select(c => c.Name).ToList();
            _hidden = new HashSet<string>(hidden ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

            Text = "Xuất dữ liệu - xác nhận schema   ·   " + rowCount + " hàng (toàn bộ dataset, không theo bộ lọc)";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(900, 360);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(18, 14, 18, 12) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            foreach (var h in new[] { 22F, 32F, 32F, 112F, 30F })
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
            }

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblSchemaCaption = new Label { Text = "SCHEMA ĐANG DÙNG", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85) };

            // Ô chữ chỉ đọc: schema dài vẫn xem / chọn / copy được đủ (không bị cắt "...").
            var txtSchema = new TextBox
            {
                Text = schemaText,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                BackColor = Color.FromArgb(248, 250, 252),
                Margin = new Padding(0, 2, 0, 4),
                TabStop = false
            };

            // Thứ tự / bỏ cột thu gọn sau nút "Thay đổi thứ tự cột" (bấm mới hiện); dòng xem trước cột luôn hiện.
            var orderRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            btnToggleOrder = new Button
            {
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 10, 0),
                ForeColor = Color.FromArgb(37, 99, 235),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                UseMnemonic = false
            };
            btnToggleOrder.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnToggleOrder.Click += (s, e) => SetOrderExpanded(!flpChips.Visible);
            _tips.SetToolTip(btnToggleOrder, "Kéo thả đổi thứ tự cột, ✕ bỏ cột không cần xuất");
            lblOrderHint = new Label
            {
                Text = "kéo thả để sắp xếp  ·  ✕ bỏ cột (bấm lại để lấy lại)  ·  thẻ có khoá = bắt buộc",
                AutoSize = true,
                Margin = new Padding(0, 9, 0, 0),
                ForeColor = Color.FromArgb(100, 116, 139),
                UseMnemonic = false
            };
            orderRow.Controls.AddRange(new Control[] { btnToggleOrder, lblOrderHint });

            btnReset = new Button { Text = "↺ Theo schema", AutoSize = true, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(8, 2, 0, 0), ForeColor = Color.FromArgb(37, 99, 235) };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnReset.Click += (s, e) =>
            {
                _hidden.Clear();
                FillChips(_schemaOrder);
            };
            _tips.SetToolTip(btnReset, "Trả về đúng thứ tự schema và xuất đủ các cột");

            flpChips.Dock = DockStyle.Fill;
            flpChips.AllowDrop = true;
            flpChips.AutoScroll = true;
            flpChips.BackColor = Color.FromArgb(248, 250, 252);
            flpChips.Padding = new Padding(8, 10, 8, 4);
            flpChips.DragEnter += Chip_DragOver;
            flpChips.DragOver += Chip_DragOver;
            flpChips.DragDrop += (s, e) => UpdatePreview();

            lblColumns.Dock = DockStyle.Fill;
            lblColumns.TextAlign = ContentAlignment.MiddleLeft;
            lblColumns.AutoEllipsis = true;
            lblColumns.UseMnemonic = false;

            // [Huỷ]  [{ } Xuất JSON…]  [⬇ Xuất Excel…]
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0), Margin = new Padding(0) };
            btnExcel = DialogButton("⬇  Xuất Excel (.xlsx)…", Color.FromArgb(22, 163, 74), Color.White, 190);
            btnExcel.Click += (s, e) => Finish(ExportChoice.Excel);
            btnJson = DialogButton("{ }  Xuất JSON…", Color.White, Color.FromArgb(37, 99, 235), 150);
            btnJson.Click += (s, e) => Finish(ExportChoice.Json);
            var btnCancel = DialogButton("Huỷ", Color.White, Color.FromArgb(31, 41, 55), 90);
            btnCancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.AddRange(new Control[] { btnExcel, btnJson, btnCancel });
            AcceptButton = btnExcel;
            CancelButton = btnCancel;

            table.Controls.Add(lblSchemaCaption, 0, 0);
            table.SetColumnSpan(lblSchemaCaption, 2);
            table.Controls.Add(txtSchema, 0, 1);
            table.SetColumnSpan(txtSchema, 2);
            table.Controls.Add(orderRow, 0, 2);
            table.Controls.Add(btnReset, 1, 2);
            table.Controls.Add(flpChips, 0, 3);
            table.SetColumnSpan(flpChips, 2);
            table.Controls.Add(lblColumns, 0, 4);
            table.SetColumnSpan(lblColumns, 2);
            table.Controls.Add(buttons, 0, 5);
            table.SetColumnSpan(buttons, 2);
            Controls.Add(table);
            _table = table;

            FillChips(order);
            SetOrderExpanded(false);
            Theme.Attach(this);
        }

        private const int ChipsRowHeight = 112;
        private const int ExpandedHeight = 360;

        /// <summary>Hiện / thu gọn khu vực thẻ cột (kéo thả, ✕ bỏ cột) - thu gọn thì hộp thoại thấp lại.</summary>
        private void SetOrderExpanded(bool expanded)
        {
            _table.RowStyles[3].Height = expanded ? ChipsRowHeight : 0F;
            flpChips.Visible = expanded;
            btnReset.Visible = expanded;
            lblOrderHint.Visible = expanded;
            btnToggleOrder.Text = expanded ? "▴  Thu gọn thứ tự cột" : "⇅  Thay đổi thứ tự cột  ▾";
            ClientSize = new Size(ClientSize.Width, expanded ? ExpandedHeight : ExpandedHeight - ChipsRowHeight);
        }

        private static Button DialogButton(string text, Color back, Color fore, int width)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Margin = new Padding(8, 0, 0, 0),
                UseMnemonic = false
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            button.FlatAppearance.BorderSize = back == Color.White ? 1 : 0;
            return button;
        }

        private void Finish(ExportChoice choice)
        {
            Order = CurrentOrder();
            Hidden = Order.Where(_hidden.Contains).ToList();
            Choice = choice;
            DialogResult = DialogResult.OK;
        }

        private List<string> CurrentOrder()
        {
            return flpChips.Controls.OfType<Chip>().Select(c => c.Info.Name).ToList();
        }

        #region Thẻ kéo thả / bỏ cột

        private void FillChips(IEnumerable<string> order)
        {
            flpChips.SuspendLayout();
            foreach (Control old in flpChips.Controls.Cast<Control>().ToList())
            {
                old.Dispose();
            }

            flpChips.Controls.Clear();
            foreach (var name in order)
            {
                var info = _chips.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
                if (info == null)
                {
                    continue;
                }

                var chip = new Chip(info, _hidden);
                chip.MouseDown += (s, e) =>
                {
                    if (e.Button != MouseButtons.Left)
                    {
                        return;
                    }

                    // Bấm vào ✕ ở mép phải: bỏ / lấy lại cột (không kéo).
                    if (chip.Removable && e.X >= chip.Width - Chip.ToggleWidth)
                    {
                        ToggleHidden(chip);
                        return;
                    }

                    chip.DoDragDrop(chip, DragDropEffects.Move);
                    UpdatePreview();
                };
                chip.DragEnter += Chip_DragOver;
                chip.DragOver += Chip_DragOver;
                chip.DragDrop += (s, e) => UpdatePreview();
                _tips.SetToolTip(chip, (info.InFolder ? "Cấp thư mục" : "Trong tên file")
                    + (string.IsNullOrEmpty(info.Example) ? string.Empty : "  ·  ví dụ: " + info.Example)
                    + (chip.Removable ? "  ·  kéo để đổi chỗ · ✕ để bỏ cột" : "  ·  bắt buộc (không bỏ được)"));
                flpChips.Controls.Add(chip);
            }

            flpChips.ResumeLayout(true);
            UpdatePreview();
        }

        /// <summary>Bỏ / lấy lại một cột thông tin - luôn phải còn ít nhất một trường định danh (Key).</summary>
        private void ToggleHidden(Chip chip)
        {
            string name = chip.Info.Name;
            if (_hidden.Contains(name))
            {
                _hidden.Remove(name);
            }
            else
            {
                bool lastKey = chip.Info.Role == DatasetFieldRole.Key
                    && _chips.Count(c => c.Role == DatasetFieldRole.Key && !_hidden.Contains(c.Name)) <= 1;
                if (lastKey)
                {
                    MessageBox.Show(this, "Cần giữ ít nhất một trường định danh (vd No / SPL) để phân biệt các hàng.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _hidden.Add(name);
            }

            chip.Invalidate();
            UpdatePreview();
        }

        /// <summary>Kéo qua thẻ khác: đổi chỗ ngay (thấy thứ tự mới trong lúc kéo).</summary>
        private void Chip_DragOver(object sender, DragEventArgs e)
        {
            var dragged = e.Data.GetData(typeof(Chip)) as Chip;
            if (dragged == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;
            var target = flpChips.GetChildAtPoint(flpChips.PointToClient(new System.Drawing.Point(e.X, e.Y))) as Chip;
            if (target != null && target != dragged)
            {
                flpChips.Controls.SetChildIndex(dragged, flpChips.Controls.GetChildIndex(target));
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            var info = CurrentOrder()
                .Select(n => _chips.First(c => c.Name == n))
                .Where(c => ucDatasetManager.IsInfoRole(c.Role) && !_hidden.Contains(c.Name))
                .Select(c => c.Name)
                .ToList();
            lblColumns.Text = "Cột thông tin Excel / JSON:   " + (info.Count == 0 ? "(không có)" : string.Join("  |  ", info))
                + (_hidden.Count > 0 ? "      ·  bỏ " + _hidden.Count + " cột" : string.Empty)
                + (_chips.Any(c => c.Role == DatasetFieldRole.Shot && _hidden.Contains(c.Name)) ? "  ·  Lần chụp: chỉ ảnh đầu tiên theo tên file" : string.Empty);
        }

        /// <summary>
        /// Thẻ một trường: màu theo vai trò (cùng màu thanh Thành phần schema), viền đứt = trường trong tên file.
        /// Cột thông tin có ✕ ở mép phải (đã bỏ: mờ + gạch ngang, nút thành ↺); trường bắt buộc có 🔒.
        /// </summary>
        private sealed class Chip : Label
        {
            public const int ToggleWidth = 24;
            public readonly ExportChip Info;
            private readonly HashSet<string> _hidden;

            public Chip(ExportChip info, HashSet<string> hidden)
            {
                Info = info;
                _hidden = hidden;
                Text = info.Name;
                AutoSize = false;
                UseMnemonic = false;
                AllowDrop = true;
                Cursor = Cursors.SizeAll;
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
                Margin = new Padding(4, 4, 4, 4);
                Size = new Size(TextRenderer.MeasureText("⠿ " + info.Name, Font).Width + 22 + ToggleWidth, 34);
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            public bool Removable
            {
                get { return ucDatasetManager.IsRemovableExportRole(Info.Role); }
            }

            private bool IsHidden
            {
                get { return _hidden.Contains(Info.Name); }
            }

            private Color RoleColor()
            {
                switch (Info.Role)
                {
                    case DatasetFieldRole.Type: return SchemaChipsPanel.TypeColor;
                    case DatasetFieldRole.Evaluation: return SchemaChipsPanel.EvalColor;
                    case DatasetFieldRole.Suffix: return SchemaChipsPanel.SuffixColor;
                    case DatasetFieldRole.Condition: return SchemaChipsPanel.ConditionColor;
                    case DatasetFieldRole.Shot: return SchemaChipsPanel.ShotColor;
                    default: return Color.FromArgb(71, 85, 105);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                Cursor = Removable && e.X >= Width - ToggleWidth ? Cursors.Hand : Cursors.SizeAll;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Parent != null ? Parent.BackColor : Color.White);
                bool hidden = IsHidden;
                var color = Theme.Fore(hidden ? Color.FromArgb(160, 168, 180) : RoleColor());
                var rect = new RectangleF(1, 1, Width - 3, Height - 3);
                using (var path = Round(rect, 14))
                using (var fill = new SolidBrush(Theme.Back(hidden ? Color.FromArgb(241, 245, 249) : Color.White)))
                using (var pen = new Pen(color, 1.4F) { DashStyle = Info.InFolder ? DashStyle.Solid : DashStyle.Dash })
                {
                    g.FillPath(fill, path);
                    g.DrawPath(pen, path);
                }

                var textRect = new Rectangle(0, 0, Width - ToggleWidth + 4, Height);
                var font = hidden ? new Font(Font, Font.Style | FontStyle.Strikeout) : Font;
                TextRenderer.DrawText(g, "⠿ " + Info.Name, font, textRect, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                if (hidden)
                {
                    font.Dispose();
                }

                string mark = Removable ? (hidden ? "↺" : "✕") : "🔒";
                using (var markFont = new Font(Removable ? "Segoe UI" : "Segoe UI Emoji", Removable ? 9.5F : 8F, Removable ? FontStyle.Bold : FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, mark, markFont, new Rectangle(Width - ToggleWidth - 4, 0, ToggleWidth, Height),
                        Removable ? color : Theme.Fore(Color.FromArgb(148, 163, 184)),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }

            private static GraphicsPath Round(RectangleF r, float radius)
            {
                float d = radius * 2;
                var path = new GraphicsPath();
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        #endregion
    }
}
