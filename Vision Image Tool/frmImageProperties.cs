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
    /// Thuộc tính ảnh (menu chuột phải của danh sách ảnh): độ phân giải, dung lượng, kênh màu, DPI,
    /// ngày tạo / sửa, so sánh với ảnh cặp Raw ↔ Mask và thông tin riêng của nơi gọi (kết quả tách
    /// theo schema, nhãn...). Chọn nhiều ảnh thì hiện bảng tóm tắt từng ảnh.
    /// Giao diện sửa bằng Designer (frmImageProperties.Designer.cs).
    /// </summary>
    internal partial class frmImageProperties : Form
    {
        private static Color WarnColor => Theme.Fore(Color.FromArgb(185, 28, 28));
        private static Color OkColor => Theme.Fore(Color.FromArgb(21, 128, 61));
        private static Color GroupColor => Theme.Back(Color.FromArgb(243, 244, 246));

        public frmImageProperties()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        /// <summary>Thuộc tính MỘT ảnh (+ ảnh cặp nếu có).</summary>
        public void ShowSingle(ImageFileInfo info, ImageFileInfo pair, IList<KeyValuePair<string, string>> extra)
        {
            Text = "Thuộc tính - " + Path.GetFileName(info.Path);
            lvInfo.BeginUpdate();
            lvInfo.Items.Clear();

            AddGroup("File");
            Add("Tên file", Path.GetFileName(info.Path));
            Add("Thư mục", Path.GetDirectoryName(info.Path));
            Add("Dung lượng", ImageFileOps.FormatSize(info.Length) + $"  ({info.Length:N0} byte)");
            Add("Ngày tạo", info.Created.ToString("dd/MM/yyyy HH:mm:ss"));
            Add("Ngày sửa", info.Modified.ToString("dd/MM/yyyy HH:mm:ss"));

            AddGroup("Ảnh");
            if (info.Error != null)
            {
                Add("Lỗi đọc ảnh", info.Error, WarnColor);
            }
            else
            {
                Add("Độ phân giải", $"{info.Width} × {info.Height} px  ({info.Width * (long)info.Height / 1000000.0:0.##} MP)");
                Add("Định dạng", info.Format);
                Add("Kênh màu", info.Channels);
                Add("Bit / pixel", info.BitsPerPixel.ToString());
                Add("DPI", $"{info.DpiX:0} × {info.DpiY:0}");
            }

            AddGroup("Ảnh cặp Raw ↔ Mask");
            if (pair == null)
            {
                Add("Ảnh cặp", "Không tìm thấy", WarnColor);
            }
            else
            {
                Add("Ảnh cặp", Path.GetFileName(pair.Path));
                if (info.Error == null && pair.Error == null)
                {
                    bool same = info.Width == pair.Width && info.Height == pair.Height;
                    Add("Độ phân giải ảnh cặp", $"{pair.Width} × {pair.Height} px  " + (same ? "✔ khớp" : "✖ KHÁC ảnh này"),
                        same ? OkColor : WarnColor);
                }
            }

            if (extra != null && extra.Count > 0)
            {
                AddGroup("Dataset");
                foreach (var kv in extra)
                {
                    Color color = kv.Value != null && kv.Value.StartsWith("✖", StringComparison.Ordinal) ? WarnColor
                        : kv.Value != null && kv.Value.StartsWith("✔", StringComparison.Ordinal) ? OkColor
                        : Color.Empty;
                    Add(kv.Key, kv.Value, color);
                }
            }

            lvInfo.EndUpdate();
        }

        /// <summary>Tóm tắt NHIỀU ảnh: mỗi ảnh một dòng + tổng dung lượng, cảnh báo khác độ phân giải.</summary>
        public void ShowMany(IList<ImageFileInfo> infos)
        {
            Text = $"Thuộc tính - {infos.Count} ảnh";

            lvInfo.BeginUpdate();
            lvInfo.Items.Clear();
            lvInfo.Columns.Clear();
            lvInfo.Columns.Add("Tên file", 250);
            lvInfo.Columns.Add("Độ phân giải", 110);
            lvInfo.Columns.Add("Kênh màu", 120);
            lvInfo.Columns.Add("Dung lượng", 80, HorizontalAlignment.Right);

            // Độ phân giải chiếm đa số = chuẩn; ảnh nào khác thì tô đỏ.
            var common = infos.Where(i => i.Error == null)
                .GroupBy(i => new Size(i.Width, i.Height))
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            foreach (var info in infos)
            {
                var item = new ListViewItem(Path.GetFileName(info.Path)) { UseItemStyleForSubItems = true };
                if (info.Error != null)
                {
                    item.SubItems.Add("Lỗi: " + info.Error);
                    item.SubItems.Add(string.Empty);
                    item.ForeColor = WarnColor;
                }
                else
                {
                    item.SubItems.Add($"{info.Width} × {info.Height}");
                    item.SubItems.Add(info.Channels);
                    if (new Size(info.Width, info.Height) != common)
                    {
                        item.ForeColor = WarnColor;
                    }
                }

                item.SubItems.Add(ImageFileOps.FormatSize(info.Length));
                lvInfo.Items.Add(item);
            }

            int different = infos.Count(i => i.Error != null || new Size(i.Width, i.Height) != common);
            var total = new ListViewItem($"Tổng: {infos.Count} ảnh") { BackColor = GroupColor, Font = new Font(lvInfo.Font, FontStyle.Bold) };
            total.SubItems.Add(common.IsEmpty ? "-" : $"{common.Width} × {common.Height}");
            total.SubItems.Add(different > 0 ? $"✖ {different} ảnh khác" : "✔ đồng nhất");
            total.SubItems.Add(ImageFileOps.FormatSize(infos.Sum(i => i.Length)));
            lvInfo.Items.Add(total);

            lvInfo.EndUpdate();
        }

        private void AddGroup(string title)
        {
            var item = new ListViewItem(title)
            {
                BackColor = GroupColor,
                Font = new Font(lvInfo.Font, FontStyle.Bold)
            };
            item.SubItems.Add(string.Empty);
            lvInfo.Items.Add(item);
        }

        private void Add(string name, string value, Color color = default(Color))
        {
            var item = new ListViewItem("   " + name) { UseItemStyleForSubItems = false };
            var sub = item.SubItems.Add(value ?? string.Empty);
            if (!color.IsEmpty)
            {
                sub.ForeColor = color;
            }

            lvInfo.Items.Add(item);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var text = new StringBuilder();
            foreach (ListViewItem item in lvInfo.Items)
            {
                text.AppendLine(string.Join("\t", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text.Trim())));
            }

            Clipboard.SetText(text.ToString());
        }
    }
}
