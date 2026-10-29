using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Xác nhận xoá ảnh (menu chuột phải của danh sách ảnh): liệt kê file sẽ xoá, tuỳ chọn xoá luôn
    /// ảnh cặp Raw ↔ Mask (mặc định bật). File được chuyển vào Thùng rác.
    /// Giao diện sửa bằng Designer (frmDeleteImages.Designer.cs).
    /// </summary>
    internal partial class frmDeleteImages : Form
    {
        private readonly IList<string> _selected = new List<string>();
        private readonly IList<string> _pairs = new List<string>();

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmDeleteImages()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        public frmDeleteImages(IList<string> selected, IList<string> pairs)
            : this()
        {
            _selected = selected;
            _pairs = pairs;

            chkIncludePair.Enabled = pairs.Count > 0;
            chkIncludePair.Checked = pairs.Count > 0;
            chkIncludePair.Text = pairs.Count > 0
                ? $"Xoá cả ảnh cặp Raw ↔ Mask ({pairs.Count} file)"
                : "Không có ảnh cặp Raw ↔ Mask";

            UpdateList();
        }

        /// <summary>Danh sách file sẽ xoá (đọc sau khi ShowDialog trả về OK).</summary>
        public IList<string> Files
        {
            get { return chkIncludePair.Checked ? _selected.Concat(_pairs).ToList() : _selected.ToList(); }
        }

        private void chkIncludePair_CheckedChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void UpdateList()
        {
            var files = Files;

            lstFiles.BeginUpdate();
            lstFiles.Items.Clear();
            foreach (var file in files)
            {
                bool pair = !_selected.Contains(file);
                lstFiles.Items.Add((pair ? "  ↳ " : string.Empty) + Path.GetFileName(file)
                    + "    (" + Path.GetFileName(Path.GetDirectoryName(file)) + ")");
            }

            lstFiles.EndUpdate();

            lblMessage.Text = files.Count == 1
                ? "Xoá ảnh này khỏi dataset?"
                : $"Xoá {files.Count} ảnh khỏi dataset?";
            btnDelete.Text = files.Count == 1 ? "Xoá" : $"Xoá {files.Count} ảnh";
        }
    }
}
