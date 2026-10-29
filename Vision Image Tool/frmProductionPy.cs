using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Xem cấu hình imaging của UnitX dưới dạng production.py (hiện tại hoặc đề xuất). Dòng có
    /// "# ←" (chỗ cần thêm / sửa) được tô nền vàng. Giao diện sửa bằng Designer.
    /// </summary>
    internal partial class frmProductionPy : Form
    {
        private static Color ChangedLine => Theme.Back(Color.FromArgb(254, 243, 199));

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmProductionPy()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        public frmProductionPy(string title, string subtitle, string code)
            : this()
        {
            Text = title;
            lblTitle.Text = title;
            lblSubtitle.Text = subtitle;
            rtbCode.Text = code ?? string.Empty;
            HighlightChanges();
        }

        /// <summary>
        /// Tô nền vàng dòng có "# ←"; khối được THÊM MỚI ("{  # ← thêm ...") thì tô cả khối tới dấu
        /// đóng ngoặc cùng mức thụt lề. Dòng chú thích thuần ("# ...") in màu xám.
        /// </summary>
        private void HighlightChanges()
        {
            var lines = rtbCode.Lines;
            int changed = 0;
            int blockIndent = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int indent = line.Length - line.TrimStart(' ').Length;
                bool comment = line.TrimStart().StartsWith("#", StringComparison.Ordinal);
                bool marked = !comment && line.Contains("# ←");
                bool inBlock = blockIndent >= 0;

                if (marked && blockIndent < 0 && line.Contains("# ← thêm") && line.TrimEnd().Contains("{  #"))
                {
                    blockIndent = indent;
                }

                if (marked || inBlock)
                {
                    PaintLine(i, line, ChangedLine, null);
                    if (marked)
                    {
                        changed++;
                    }
                }
                else if (comment)
                {
                    PaintLine(i, line, null, Theme.Fore(Color.FromArgb(107, 114, 128)));
                }

                if (inBlock && indent == blockIndent && line.TrimStart().StartsWith("}", StringComparison.Ordinal))
                {
                    blockIndent = -1;
                }
            }

            rtbCode.Select(0, 0);
            if (changed > 0)
            {
                lblSubtitle.Text += "   •   " + changed + " dòng cần thêm / sửa (nền vàng)";
            }
        }

        private void PaintLine(int lineIndex, string line, Color? back, Color? fore)
        {
            rtbCode.Select(rtbCode.GetFirstCharIndexFromLine(lineIndex), line.Length);
            if (back.HasValue)
            {
                rtbCode.SelectionBackColor = back.Value;
            }

            if (fore.HasValue)
            {
                rtbCode.SelectionColor = fore.Value;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbCode.Text);
            btnCopy.Text = "Đã copy ✔";
        }
    }
}
