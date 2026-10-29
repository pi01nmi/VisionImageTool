using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Đổi tên một ảnh (menu chuột phải của danh sách ảnh). Giữ nguyên phần mở rộng, kiểm tra tên
    /// hợp lệ / trùng file / khớp schema ngay khi gõ, và tự đề xuất tên mới cho ảnh cặp Raw ↔ Mask.
    /// Giao diện sửa bằng Designer (frmRenameImage.Designer.cs).
    /// </summary>
    internal partial class frmRenameImage : Form
    {
        private static readonly Color OkColor = Color.FromArgb(21, 128, 61);
        private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);
        private static readonly Color InfoColor = Color.FromArgb(107, 114, 128);

        private readonly string _path;
        private readonly string _pairPath;
        private readonly Func<string, string> _describe;
        private bool _pairEdited;
        private bool _settingPair;

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmRenameImage()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        /// <param name="path">File cần đổi tên.</param>
        /// <param name="pairPath">Ảnh cặp Raw ↔ Mask (null nếu không có).</param>
        /// <param name="describe">Mô tả tên theo schema ("✔ ..." / "✖ ..."), null = không kiểm tra.</param>
        public frmRenameImage(string path, string pairPath, Func<string, string> describe)
            : this()
        {
            _path = path;
            _pairPath = pairPath;
            _describe = describe;

            txtName.Text = Path.GetFileNameWithoutExtension(path);
            lblExt.Text = Path.GetExtension(path);
            lblFolder.Text = "Thư mục: " + Path.GetDirectoryName(path);

            bool hasPair = pairPath != null;
            chkPair.Enabled = hasPair;
            chkPair.Checked = hasPair;
            chkPair.Text = hasPair ? "Đổi tên cả ảnh cặp Raw ↔ Mask" : "Không có ảnh cặp Raw ↔ Mask";

            if (hasPair)
            {
                lblPairOld.Text = "Hiện tại: " + Path.GetFileName(pairPath);
                lblPairExt.Text = Path.GetExtension(pairPath);
                SetPairText(Path.GetFileNameWithoutExtension(pairPath));
            }
            else
            {
                lblPairOld.Text = string.Empty;
                lblPairExt.Text = string.Empty;
            }

            UpdateUi();
        }

        public string PairPath { get { return _pairPath; } }

        /// <summary>Đường dẫn mới của file (đọc sau khi ShowDialog trả về OK).</summary>
        public string NewPath
        {
            get { return Path.Combine(Path.GetDirectoryName(_path), txtName.Text.Trim() + lblExt.Text); }
        }

        /// <summary>Đường dẫn mới của ảnh cặp, null nếu không đổi tên ảnh cặp.</summary>
        public string NewPairPath
        {
            get
            {
                return _pairPath != null && chkPair.Checked
                    ? Path.Combine(Path.GetDirectoryName(_pairPath), txtPairName.Text.Trim() + lblPairExt.Text)
                    : null;
            }
        }

        private void frmRenameImage_Shown(object sender, EventArgs e)
        {
            txtName.Focus();
            txtName.SelectAll();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            // Người dùng chưa tự sửa tên ảnh cặp -> đề xuất lại theo tên mới.
            if (_pairPath != null && !_pairEdited)
            {
                string derived = ImageFileOps.DerivePairName(
                    Path.GetFileNameWithoutExtension(_path), Path.GetFileNameWithoutExtension(_pairPath), txtName.Text.Trim());
                if (derived != null)
                {
                    SetPairText(derived);
                }
            }

            UpdateUi();
        }

        private void txtPairName_TextChanged(object sender, EventArgs e)
        {
            if (!_settingPair)
            {
                _pairEdited = true;
            }

            UpdateUi();
        }

        private void chkPair_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUi();
        }

        private void SetPairText(string text)
        {
            _settingPair = true;
            txtPairName.Text = text;
            _settingPair = false;
        }

        private void UpdateUi()
        {
            bool pair = _pairPath != null && chkPair.Checked;
            txtPairName.Enabled = pair;

            string error = Check(_path, txtName.Text.Trim(), lblExt.Text, lblCheck);
            string pairError = pair ? Check(_pairPath, txtPairName.Text.Trim(), lblPairExt.Text, lblPairCheck) : null;

            if (!pair)
            {
                lblPairCheck.Text = string.Empty;
            }

            if (error == null && pairError == null && pair
                && string.Equals(NewPath, NewPairPath, StringComparison.OrdinalIgnoreCase))
            {
                pairError = "Tên ảnh cặp trùng với tên mới ở trên.";
                ShowCheck(lblPairCheck, "✖ " + pairError, ErrorColor);
            }

            btnOk.Enabled = error == null && pairError == null;
        }

        /// <summary>Kiểm tra một tên mới, ghi kết quả ra nhãn. Trả về lỗi (null = hợp lệ).</summary>
        private string Check(string oldPath, string name, string ext, Label label)
        {
            string error = ImageFileOps.ValidateFileName(name + ext);

            if (error == null)
            {
                string target = Path.Combine(Path.GetDirectoryName(oldPath), name + ext);
                if (!string.Equals(target, oldPath, StringComparison.OrdinalIgnoreCase) && File.Exists(target))
                {
                    error = "Đã có file trùng tên trong thư mục.";
                }
            }

            if (error != null)
            {
                ShowCheck(label, "✖ " + error, ErrorColor);
                return error;
            }

            string description = _describe != null ? _describe(name + ext) : null;
            if (string.IsNullOrEmpty(description))
            {
                ShowCheck(label, "✔ Tên hợp lệ.", OkColor);
            }
            else
            {
                // Không khớp schema vẫn cho đổi (có thể người dùng đang sửa dần) - chỉ cảnh báo.
                ShowCheck(label, description, description.StartsWith("✔", StringComparison.Ordinal) ? OkColor : ErrorColor);
            }

            return null;
        }

        private static void ShowCheck(Label label, string text, Color color)
        {
            label.Text = text;
            label.ForeColor = color == OkColor || color == ErrorColor ? color : InfoColor;
        }
    }
}
