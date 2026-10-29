using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace VisionImageTool
{
    /// <summary>
    /// Cửa sổ "Kiểm tra trạng thái": dòng kết luận tổng ở trên cùng (SẴN SÀNG / CẦN CHÚ Ý /
    /// CHƯA SẴN SÀNG) và hai tab:
    ///   - Tổng quan: các hạng mục gom theo khối (Kết nối / Hệ thống / Cấu hình chạy / Dữ liệu), mỗi
    ///     dòng có icon chức năng - chỉ hiện icon cảnh báo / lỗi khi dòng đó thực sự có vấn đề.
    ///   - Cấu hình sản xuất: cây production.py gộp theo Part Type + phần cứng khai báo trên UnitX.
    ///
    /// Khung sửa được bằng Designer - xem frmStatusV1.Designer.cs. Các DÒNG trạng thái và nút cây sinh
    /// bằng code vì số lượng thay đổi theo mỗi lần kiểm tra.
    ///
    /// btnRetry có DialogResult = Retry (người dùng bấm "Kiểm tra lại"), btnClose = OK.
    /// </summary>
    internal partial class frmStatusV1 : Form
    {
        private static readonly Color OkColor = Color.FromArgb(22, 163, 74);
        private static readonly Color WarnColor = Color.FromArgb(217, 119, 6);
        private static readonly Color ErrorColor = Color.FromArgb(220, 38, 38);

        private static Color TitleColor => Theme.Fore(Color.FromArgb(17, 24, 39));
        private static Color TextColor => Theme.Fore(Color.FromArgb(75, 85, 99));
        private static Color MutedColor => Theme.Fore(Color.FromArgb(156, 163, 175));
        private static Color IconColor => Theme.Fore(Color.FromArgb(100, 116, 139));
        private static Color GroupColor => Theme.Fore(Color.FromArgb(107, 114, 128));
        private static Color DividerColor => Theme.Back(Color.FromArgb(238, 240, 243));

        private const int IconColumn = 30;
        private const int TitleColumn = 140;

        private static readonly Dictionary<StatusGroup, string> GroupNames = new Dictionary<StatusGroup, string>
        {
            { StatusGroup.Connection, "KẾT NỐI" },
            { StatusGroup.System, "HỆ THỐNG UNITX" },
            { StatusGroup.Configuration, "CẤU HÌNH CHẠY" },
            { StatusGroup.Data, "DỮ LIỆU" }
        };

        private static readonly Regex Markup = new Regex(@"\*\*(.+?)\*\*|\[(ok|warn|fail)\]", RegexOptions.Compiled);

        private readonly Action _showLog;
        private readonly ProdConfigData _prod;

        /// <summary>Hàng của tlpItems là dòng hạng mục (có đường kẻ mờ phía dưới).</summary>
        private readonly HashSet<int> _itemRows = new HashSet<int>();

        private Font _iconFont;
        private Font _boldFont;
        private Font _smallFont;
        private bool _treeExpanded = true;

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmStatusV1()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        public frmStatusV1(IList<StatusItem> items, ProdConfigData prod, Action showLog)
            : this()
        {
            _showLog = showLog;
            _prod = prod;

            _iconFont = new Font(IconFontName(), 12F);
            _boldFont = new Font(Font, FontStyle.Bold);
            _smallFont = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);

            ApplyBanner(items);
            BuildItems(items);
            BuildProdTree();

            // Cao vừa đủ nội dung tab Tổng quan, tối đa 760.
            int content = tlpItems.GetPreferredSize(new Size(pnlList.ClientSize.Width, 0)).Height;
            int chrome = pnlBanner.Height + tlpButtons.Height + (tabView.Height - tabOverview.Height) + pnlList.Padding.Vertical + 8;
            ClientSize = new Size(ClientSize.Width, Math.Max(420, Math.Min(760, chrome + content)));
            ResizeDetailColumn();
        }

        /// <summary>Font icon: Segoe MDL2 Assets (Windows 10) hoặc Segoe Fluent Icons (Windows 11).</summary>
        private static string IconFontName()
        {
            var installed = FontFamily.Families.Select(f => f.Name).ToList();
            foreach (var name in new[] { "Segoe MDL2 Assets", "Segoe Fluent Icons" })
            {
                if (installed.Contains(name))
                {
                    return name;
                }
            }

            return "Segoe UI Symbol";
        }

        private void ApplyBanner(IList<StatusItem> items)
        {
            int errors = items.Count(i => i.Level == StatusLevel.Error);
            int warnings = items.Count(i => i.Level == StatusLevel.Warning);
            string sub;

            if (errors > 0)
            {
                pnlBanner.BackColor = ErrorColor;
                lblTitle.Text = "✖  CHƯA SẴN SÀNG";
                sub = $"{errors} lỗi" + (warnings > 0 ? $", {warnings} cảnh báo" : string.Empty)
                    + " - xử lý các mục màu đỏ bên dưới.";
            }
            else if (warnings > 0)
            {
                pnlBanner.BackColor = WarnColor;
                lblTitle.Text = "⚠  SẴN SÀNG - CẦN CHÚ Ý";
                sub = $"{warnings} cảnh báo - hệ thống chạy được nhưng nên xem các mục màu cam.";
            }
            else
            {
                pnlBanner.BackColor = OkColor;
                lblTitle.Text = "✔  SẴN SÀNG";
                sub = "Tất cả hạng mục đều OK.";
            }

            lblSub.Text = sub + "   (lúc " + DateTime.Now.ToString("HH:mm:ss") + ")";
        }

        #region Tab Tổng quan

        private void BuildItems(IList<StatusItem> items)
        {
            tlpItems.SuspendLayout();
            try
            {
                tlpItems.Controls.Clear();
                tlpItems.RowStyles.Clear();
                tlpItems.RowCount = 0;
                _itemRows.Clear();

                // Theo khối; trong khối: lỗi lên đầu, rồi cảnh báo, rồi OK - giữ thứ tự gốc trong cùng mức.
                foreach (var group in items
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.item.Group)
                    .OrderBy(g => g.Key))
                {
                    AddGroupRow(GroupNames[group.Key], group.Any(x => x.item.Level == StatusLevel.Error)
                        ? ErrorColor : group.Any(x => x.item.Level == StatusLevel.Warning) ? WarnColor : GroupColor);

                    foreach (var x in group
                        .OrderBy(x => x.item.Level == StatusLevel.Error ? 0 : x.item.Level == StatusLevel.Warning ? 1 : 2)
                        .ThenBy(x => x.index))
                    {
                        AddItemRow(x.item);
                    }
                }
            }
            finally
            {
                tlpItems.ResumeLayout();
            }
        }

        private void AddGroupRow(string text, Color color)
        {
            var label = new Label
            {
                Text = text,
                Font = _smallFont,
                ForeColor = color,
                AutoSize = true,
                Margin = new Padding(2, tlpItems.RowCount == 0 ? 8 : 18, 0, 4)
            };

            int row = tlpItems.RowCount;
            tlpItems.RowCount = row + 1;
            tlpItems.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpItems.Controls.Add(label, 0, row);
            tlpItems.SetColumnSpan(label, 3);
        }

        /// <summary>Một dòng: icon | tên hạng mục | chi tiết (chữ đậm, badge, thanh tài nguyên, cách xử lý).</summary>
        private void AddItemRow(StatusItem item)
        {
            Color levelColor = item.Level == StatusLevel.Warning ? WarnColor : ErrorColor;
            bool ok = item.Level == StatusLevel.Ok;

            // OK: icon chức năng màu xám xanh. Có vấn đề: icon cảnh báo / lỗi màu cam / đỏ.
            var lblIcon = new Label
            {
                Text = ok ? (item.Icon ?? StatusIcons.Plug)
                    : item.Level == StatusLevel.Warning ? StatusIcons.Warning : StatusIcons.Error,
                ForeColor = ok ? (item.Muted ? MutedColor : IconColor) : levelColor,
                Font = _iconFont,
                AutoSize = false,
                Size = new Size(IconColumn - 4, 24),
                Margin = new Padding(0, 7, 4, 6),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblItemTitle = new Label
            {
                Text = item.Title,
                Font = _boldFont,
                ForeColor = ok ? (item.Muted ? MutedColor : TitleColor) : levelColor,
                AutoSize = true,
                MaximumSize = new Size(TitleColumn - 6, 0),
                Margin = new Padding(0, 10, 6, 8)
            };

            var detail = BuildDetail(item, ok ? TextColor : levelColor);

            int row = tlpItems.RowCount;
            tlpItems.RowCount = row + 1;
            tlpItems.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpItems.Controls.Add(lblIcon, 0, row);
            tlpItems.Controls.Add(lblItemTitle, 1, row);
            tlpItems.Controls.Add(detail, 2, row);
            _itemRows.Add(row);
        }

        /// <summary>
        /// Cột chi tiết: FlowLayoutPanel gồm các mẩu chữ (thường / đậm / badge) theo đánh dấu của
        /// StatusItem.Detail, mỗi dòng xuống hàng riêng; dưới cùng là thanh tài nguyên và "→ cách xử lý".
        /// </summary>
        private Control BuildDetail(StatusItem item, Color plainColor)
        {
            var flow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Margin = new Padding(0, 9, 0, 7),
                Padding = new Padding(0)
            };

            Color textColor = item.Muted ? MutedColor : item.Level == StatusLevel.Ok ? TextColor : plainColor;
            Color boldColor = item.Muted ? MutedColor : TitleColor;

            var lines = (item.Detail ?? string.Empty).Replace("\r", string.Empty).Split('\n');
            foreach (var line in lines)
            {
                Control last = null;
                int position = 0;

                foreach (Match match in Markup.Matches(line))
                {
                    last = AddText(flow, line.Substring(position, match.Index - position), textColor, Font) ?? last;

                    if (match.Groups[1].Success)
                    {
                        last = AddText(flow, match.Groups[1].Value, boldColor, _boldFont) ?? last;
                    }
                    else
                    {
                        string kind = match.Groups[2].Value;
                        last = AddText(flow, "●", kind == "ok" ? OkColor : kind == "warn" ? WarnColor : ErrorColor, Font) ?? last;
                    }

                    position = match.Index + match.Length;
                }

                last = AddText(flow, line.Substring(position), textColor, Font) ?? last;
                if (last != null)
                {
                    flow.SetFlowBreak(last, true);
                }
            }

            if (item.Meters != null)
            {
                Control last = null;
                foreach (var meter in item.Meters)
                {
                    last = new MiniMeter(meter) { Margin = new Padding(0, 6, 18, 0) };
                    flow.Controls.Add(last);
                }

                if (last != null)
                {
                    flow.SetFlowBreak(last, true);
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Hint) && item.Level != StatusLevel.Ok)
            {
                var hint = AddText(flow, "→ " + item.Hint, plainColor, Font);
                if (hint != null)
                {
                    hint.Margin = new Padding(0, 4, 0, 0);
                }
            }

            return flow;
        }

        /// <summary>Thêm một mẩu chữ; dấu cách ở hai đầu đổi thành lề để khoảng cách không bị nuốt.</summary>
        private Label AddText(FlowLayoutPanel flow, string text, Color color, Font font)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            int leading = text.Length - text.TrimStart(' ').Length;
            int trailing = text.Length - text.TrimEnd(' ').Length;
            string trimmed = text.Trim(' ');
            if (trimmed.Length == 0)
            {
                // Chỉ có dấu cách: nới lề phải của mẩu trước.
                if (flow.Controls.Count > 0)
                {
                    var previous = flow.Controls[flow.Controls.Count - 1];
                    previous.Margin = new Padding(previous.Margin.Left, previous.Margin.Top, previous.Margin.Right + text.Length * 3, previous.Margin.Bottom);
                }

                return null;
            }

            var label = new Label
            {
                Text = trimmed,
                ForeColor = color,
                Font = font,
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(leading * 3, 0, trailing * 3, 0),
                Padding = new Padding(0)
            };

            flow.Controls.Add(label);
            return label;
        }

        /// <summary>Đường kẻ ngang mờ dưới mỗi dòng hạng mục - dóng hàng tên ↔ chi tiết dễ hơn.</summary>
        private void tlpItems_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            if (!_itemRows.Contains(e.Row))
            {
                return;
            }

            using (var pen = new Pen(DividerColor))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
            }
        }

        /// <summary>Cột chi tiết xuống dòng theo bề rộng cửa sổ.</summary>
        private void ResizeDetailColumn()
        {
            int width = Math.Max(200, pnlList.ClientSize.Width - pnlList.Padding.Horizontal - IconColumn - TitleColumn - 24);
            foreach (Control control in tlpItems.Controls)
            {
                if (tlpItems.GetColumn(control) != 2)
                {
                    continue;
                }

                control.MaximumSize = new Size(width, 0);
                foreach (Control part in control.Controls)
                {
                    if (part is Label)
                    {
                        part.MaximumSize = new Size(width, 0);
                    }
                }
            }
        }

        private void pnlList_Resize(object sender, EventArgs e)
        {
            ResizeDetailColumn();
        }

        /// <summary>Thanh tài nguyên mini: "CPU ▓▓▓░░░ 42%" - xanh &lt; 70%, cam &lt; 90%, đỏ từ 90%.</summary>
        private sealed class MiniMeter : Control
        {
            private readonly StatusMeter _meter;

            public MiniMeter(StatusMeter meter)
            {
                _meter = meter;
                Size = new Size(200, 18);
                DoubleBuffered = true;
                Font = new Font("Segoe UI", 8.5F);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                double percent = Math.Max(0, Math.Min(100, _meter.Percent));
                Color fill = percent >= 90 ? ErrorColor : percent >= 70 ? WarnColor : OkColor;

                TextRenderer.DrawText(g, _meter.Name, new Font(Font, FontStyle.Bold), new Rectangle(0, 0, 36, Height),
                    GroupColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                var bar = new Rectangle(38, 5, 80, 8);
                using (var back = new SolidBrush(Theme.Back(Color.FromArgb(229, 231, 235))))
                using (var front = new SolidBrush(fill))
                {
                    g.FillRectangle(back, bar);
                    g.FillRectangle(front, new Rectangle(bar.X, bar.Y, (int)(bar.Width * percent / 100), bar.Height));
                }

                TextRenderer.DrawText(g, _meter.Text, new Font(Font, FontStyle.Bold), new Rectangle(124, 0, Width - 124, Height),
                    percent >= 70 ? fill : TitleColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        #endregion

        #region Tab Cấu hình sản xuất

        /// <summary>
        /// Cây cấu hình sản xuất, gốc là từng Part Type (một production config có thể có nhiều):
        ///   Part Type
        ///     Nguồn n: camera 40566308
        ///       Source / Camera ID / Controller / Capture config → Network / Repeat / các khoá khác
        ///   Phần cứng khai báo trên UnitX (networks, controllers, cameras, bảng ánh xạ)
        /// </summary>
        private void BuildProdTree()
        {
            tvProd.BeginUpdate();
            tvProd.Nodes.Clear();

            // Nút đậm: đặt font của cây là đậm và gán font thường cho nút con - tránh lỗi TreeView cắt
            // chữ khi NodeFont đậm hơn font của cây.
            tvProd.Font = new Font(Font, FontStyle.Bold);

            if (_prod == null)
            {
                Add(tvProd.Nodes, "Chưa đọc được cấu hình sản xuất (SDK chưa kết nối).", MutedColor);
                lblProdSummary.Text = "Kết nối UnitX rồi bấm \"Kiểm tra lại\".";
                btnViewProduction.Enabled = false;
                tvProd.EndUpdate();
                return;
            }

            if (!_prod.Ok)
            {
                Add(tvProd.Nodes, "Lỗi đọc cấu hình: " + _prod.Error, ErrorColor);
            }

            foreach (var part in _prod.PartTypes)
            {
                var partNode = new TreeNode("Part Type:  " + part.Name) { ForeColor = TitleColor };
                tvProd.Nodes.Add(partNode);

                for (int i = 0; i < part.Sources.Count; i++)
                {
                    var source = part.Sources[i];
                    var sourceNode = Add(partNode.Nodes, $"Nguồn {i + 1}:  {source.Source} {source.CameraId}".TrimEnd(), TitleColor, true);

                    Field(sourceNode, "Source", source.Source);
                    Field(sourceNode, "Camera ID", source.CameraId);

                    string controller;
                    if (source.CameraId != null && _prod.CameraController.TryGetValue(source.CameraId, out controller))
                    {
                        Field(sourceNode, "Controller ID", controller);
                    }

                    foreach (var cc in source.CaptureNetworks)
                    {
                        var ccNode = Field(sourceNode, "Capture config", cc.Key);
                        foreach (var network in cc.Value)
                        {
                            Field(ccNode, "cc_network_mapping → Network", network);
                        }
                    }

                    Field(sourceNode, "Repeat", source.Repeat);
                    AddJson(sourceNode.Nodes, source.Extra);
                }

                AddJson(partNode.Nodes, part.Extra);
            }

            if (_prod.PartTypes.Count == 0)
            {
                Add(tvProd.Nodes, _prod.ImagingConfigError != null
                    ? "production.py không đọc được: " + _prod.ImagingConfigError
                    : "production.py trống - UnitX không khai báo Part Type.", MutedColor);
            }

            // Phần cứng / bảng ánh xạ của struct (không thuộc riêng part type nào).
            var hardware = new TreeNode("Phần cứng khai báo trên UnitX") { ForeColor = TitleColor };
            foreach (var network in _prod.Networks)
            {
                Field(hardware, "Network", network.Key + (string.IsNullOrEmpty(network.Value) ? string.Empty : "  (version " + network.Value + ")"));
            }

            foreach (var controller in _prod.Controllers)
            {
                Field(hardware, "Controller ID", controller);
            }

            foreach (var camera in _prod.Cameras)
            {
                string controller;
                _prod.CameraController.TryGetValue(camera, out controller);
                Field(hardware, "Camera ID", camera + (string.IsNullOrEmpty(controller) ? string.Empty : "  → controller " + controller));
            }

            foreach (var map in _prod.CameraCaptureConfigs)
            {
                Field(hardware, "Camera " + map.Key + " → Capture config", string.Join(", ", map.Value));
            }

            foreach (var map in _prod.CaptureNetworks)
            {
                Field(hardware, "Capture config " + map.Key + " → Network", string.Join(", ", map.Value));
            }

            if (hardware.Nodes.Count == 0)
            {
                Add(hardware.Nodes, "(trống)", MutedColor);
            }

            tvProd.Nodes.Add(hardware);
            tvProd.ExpandAll();
            if (tvProd.Nodes.Count > 0)
            {
                tvProd.Nodes[0].EnsureVisible();
            }

            tvProd.EndUpdate();

            lblProdSummary.Text = $"{_prod.PartTypes.Count} part type  •  {_prod.AllCameras.Count} camera  •  "
                + $"{_prod.AllCaptureConfigs.Count} capture config  •  {_prod.Networks.Count} network";
            btnViewProduction.Enabled = !string.IsNullOrWhiteSpace(_prod.ImagingConfig);
        }

        private TreeNode Add(TreeNodeCollection nodes, string text, Color color, bool bold = false)
        {
            var node = new TreeNode(text) { ForeColor = color };
            if (!bold)
            {
                node.NodeFont = Font;
            }

            nodes.Add(node);
            return node;
        }

        /// <summary>Nút "Tên:  giá trị" (bỏ qua nếu giá trị trống).</summary>
        private TreeNode Field(TreeNode parent, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return parent;
            }

            return Add(parent.Nodes, name + ":  " + value, TextColor);
        }

        /// <summary>Các khoá khác trong JSON (controller, image width / height...) - hiện nguyên cấu trúc.</summary>
        private void AddJson(TreeNodeCollection nodes, IEnumerable<KeyValuePair<string, JToken>> values)
        {
            foreach (var kv in values)
            {
                AddJson(nodes, kv.Key, kv.Value);
            }
        }

        private void AddJson(TreeNodeCollection nodes, string name, JToken value)
        {
            var obj = value as JObject;
            var array = value as JArray;

            if (obj != null)
            {
                var node = Add(nodes, name, TextColor);
                foreach (var property in obj.Properties())
                {
                    AddJson(node.Nodes, property.Name, property.Value);
                }
            }
            else if (array != null)
            {
                if (array.All(v => v is JValue))
                {
                    Add(nodes, name + ":  " + string.Join(", ", array.Select(v => v.ToString())), TextColor);
                    return;
                }

                var node = Add(nodes, $"{name}  [{array.Count}]", TextColor);
                for (int i = 0; i < array.Count; i++)
                {
                    AddJson(node.Nodes, "[" + i + "]", array[i]);
                }
            }
            else
            {
                Add(nodes, name + ":  " + value, TextColor);
            }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            _treeExpanded = !_treeExpanded;
            if (_treeExpanded)
            {
                tvProd.ExpandAll();
            }
            else
            {
                tvProd.CollapseAll();
            }
        }

        private void btnViewProduction_Click(object sender, EventArgs e)
        {
            if (_prod == null || string.IsNullOrWhiteSpace(_prod.ImagingConfig))
            {
                return;
            }

            // Cấu hình hiện tại dạng production.py (xem + copy trong cùng hộp thoại).
            using (var dialog = new frmProductionPy("production.py hiện tại", lblProdSummary.Text, ProductionPy.Format(_prod)))
            {
                dialog.ShowDialog(this);
            }
        }

        #endregion

        private void btnLog_Click(object sender, EventArgs e)
        {
            if (_showLog != null)
            {
                _showLog();
            }
        }
    }
}
