using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Kết quả kiểm tra cấu hình Production trước khi push: liệt kê từng điều kiện không đạt
    /// (source = "external_image", image_width / image_height khớp ảnh thật...) và nút mở
    /// production.py đề xuất. Không đạt thì KHÔNG push. Giao diện sửa bằng Designer.
    /// </summary>
    internal partial class frmProdCheck : Form
    {
        private readonly Func<string> _suggestion;
        private readonly string _subtitle;

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmProdCheck()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        /// <param name="issues">Các điều kiện không đạt.</param>
        /// <param name="subtitle">Tóm tắt thứ đang kiểm (part type, số ảnh...).</param>
        /// <param name="suggestion">Sinh production.py đề xuất (null = không có đề xuất).</param>
        public frmProdCheck(IList<string> issues, string subtitle, Func<string> suggestion)
            : this()
        {
            _suggestion = suggestion;
            _subtitle = subtitle;

            lblSubtitle.Text = subtitle;
            lstIssues.Items.Clear();
            foreach (var issue in issues)
            {
                lstIssues.Items.Add("✖  " + issue);
            }

            btnSuggestion.Enabled = suggestion != null;
        }

        private void btnSuggestion_Click(object sender, EventArgs e)
        {
            string code;
            try
            {
                code = _suggestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không dựng được cấu hình đề xuất: " + ex.Message, "production.py",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new frmProductionPy("production.py đề xuất", _subtitle, code))
            {
                dialog.ShowDialog(this);
            }
        }
    }
}
