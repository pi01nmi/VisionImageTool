using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Hộp thoại "Tuỳ chọn xuất Excel" của tab Dataset Manager: cột Raw/Mask chứa thumbnail có hyperlink
    /// hay ảnh gốc không nén, và kích thước ô ảnh (theo tỷ lệ ảnh × hệ số, hoặc nhập tay).
    /// Giao diện sửa bằng Designer (frmExportOptions.Designer.cs).
    /// </summary>
    internal partial class frmExportOptions : Form
    {
        private bool _loading;

        /// <summary>Chỉ dùng cho Designer.</summary>
        public frmExportOptions()
        {
            InitializeComponent();
            Theme.Attach(this);
        }

        /// <param name="options">Tuỳ chọn lần trước.</param>
        /// <param name="sampleSize">Kích thước một ảnh trong dataset (Size.Empty nếu không đọc được).</param>
        public frmExportOptions(DatasetExportOptions options, Size sampleSize)
            : this()
        {
            _loading = true;

            rbHyperlink.Checked = options.CellMode == ExportCellMode.Hyperlink;
            rbImage.Checked = options.CellMode == ExportCellMode.Image;
            chkThumbnail.Checked = options.HyperlinkThumbnail;
            chkVision.Checked = options.IncludeVision;
            rbRatio.Checked = options.SizeMode == ExportSizeMode.Ratio;
            rbManual.Checked = options.SizeMode == ExportSizeMode.Manual;
            numFactor.Value = Clamp(numFactor, options.Factor);
            numManualWidth.Value = Clamp(numManualWidth, options.ManualWidth);
            numManualHeight.Value = Clamp(numManualHeight, options.ManualHeight);

            // Tỷ lệ luôn lấy theo ảnh thật của dataset (nếu đọc được) - người dùng vẫn sửa tay được.
            Size ratio = new Size(options.RatioWidth, options.RatioHeight);
            if (!sampleSize.IsEmpty)
            {
                ratio = DatasetExportOptions.NiceRatio(sampleSize);
                lblDetected.Text = $"Ảnh mẫu: {sampleSize.Width} × {sampleSize.Height} px  →  tỷ lệ {ratio.Width}:{ratio.Height}";
            }
            else
            {
                lblDetected.Text = "Không đọc được kích thước ảnh - dùng tỷ lệ lần trước.";
            }

            numRatioWidth.Value = Clamp(numRatioWidth, ratio.Width);
            numRatioHeight.Value = Clamp(numRatioHeight, ratio.Height);

            _loading = false;
            UpdateUi();
        }

        /// <summary>Tuỳ chọn người dùng đã chọn (đọc sau khi ShowDialog trả về OK).</summary>
        public DatasetExportOptions Options
        {
            get
            {
                return new DatasetExportOptions
                {
                    CellMode = rbImage.Checked ? ExportCellMode.Image : ExportCellMode.Hyperlink,
                    HyperlinkThumbnail = chkThumbnail.Checked,
                    IncludeVision = chkVision.Checked,
                    SizeMode = rbManual.Checked ? ExportSizeMode.Manual : ExportSizeMode.Ratio,
                    RatioWidth = (int)numRatioWidth.Value,
                    RatioHeight = (int)numRatioHeight.Value,
                    Factor = (int)numFactor.Value,
                    ManualWidth = (int)numManualWidth.Value,
                    ManualHeight = (int)numManualHeight.Value
                };
            }
        }

        private static decimal Clamp(NumericUpDown control, int value)
        {
            return Math.Max(control.Minimum, Math.Min(control.Maximum, value));
        }

        private void Option_Changed(object sender, EventArgs e)
        {
            if (!_loading)
            {
                UpdateUi();
            }
        }

        private void UpdateUi()
        {
            bool image = rbImage.Checked;
            bool ratio = rbRatio.Checked;
            bool thumbnail = !image && chkThumbnail.Checked;

            chkThumbnail.Enabled = !image;

            if (image)
            {
                lblModeNote.Text = "Nhúng nguyên ảnh gốc, bật \"Do not compress images in file\" - nét khi phóng to, file nặng.";
            }
            else if (thumbnail)
            {
                lblModeNote.Text = "Thumbnail chất lượng thấp, Excel được nén ảnh - bấm vào ảnh để mở ảnh gốc.";
            }
            else
            {
                lblModeNote.Text = "Chỉ tên file (hyperlink), không xử lý ảnh - nhanh nhất cho dataset lớn.";
            }

            // Kích thước ô chỉ có ý nghĩa khi trong ô có ảnh.
            grpSize.Enabled = image || thumbnail;

            numRatioWidth.Enabled = numRatioHeight.Enabled = numFactor.Enabled = ratio;
            numManualWidth.Enabled = numManualHeight.Enabled = !ratio;

            var size = Options.CellSize;
            lblRatioResult.Text = $"= {(int)numRatioWidth.Value * (int)numFactor.Value} × "
                + $"{(int)numRatioHeight.Value * (int)numFactor.Value} px";
            lblRatioResult.Enabled = ratio;

            btnOk.Text = "Xuất Excel";
            Text = grpSize.Enabled ? $"Tuỳ chọn xuất Excel - ô ảnh {size.Width} × {size.Height} px" : "Tuỳ chọn xuất Excel";
        }
    }
}
