using System;
using System.Drawing;

namespace VisionImageTool
{
    /// <summary>Cột Raw / Mask trong file Excel chứa gì.</summary>
    internal enum ExportCellMode
    {
        /// <summary>Ảnh thumbnail (nén, nhẹ) vừa ô - bấm vào để mở ảnh gốc.</summary>
        Hyperlink = 0,

        /// <summary>
        /// Ảnh GỐC nhúng vừa ô, KHÔNG có hyperlink, workbook bật "Do not compress images in file" để
        /// Excel giữ nguyên độ phân giải (phóng to thấy rõ chi tiết). File nặng hơn nhiều.
        /// </summary>
        Image = 1
    }

    /// <summary>Kích thước ô ảnh lấy theo tỷ lệ ảnh × hệ số, hay nhập tay.</summary>
    internal enum ExportSizeMode
    {
        Ratio = 0,
        Manual = 1
    }

    /// <summary>Tuỳ chọn xuất Excel của tab Dataset Manager (được lưu trong dataset_manager.json).</summary>
    internal sealed class DatasetExportOptions
    {
        public ExportCellMode CellMode { get; set; } = ExportCellMode.Hyperlink;

        /// <summary>
        /// Chế độ Hyperlink: true = ô chứa thumbnail (chất lượng thấp) bấm được; false = chỉ tên file
        /// bấm được - không xử lý ảnh nào, xuất dataset hàng nghìn ảnh gần như tức thì.
        /// </summary>
        public bool HyperlinkThumbnail { get; set; } = true;

        /// <summary>Thêm cột Vision (kết quả máy) cạnh cột Đánh giá của mỗi ĐK. Mặc định KHÔNG xuất.</summary>
        public bool IncludeVision { get; set; }

        public ExportSizeMode SizeMode { get; set; } = ExportSizeMode.Ratio;

        public int RatioWidth { get; set; } = 4;
        public int RatioHeight { get; set; } = 3;

        /// <summary>Hệ số nhân: tỷ lệ 4:3 × 20 = ô 80 × 60 px.</summary>
        public int Factor { get; set; } = 20;

        public int ManualWidth { get; set; } = 80;
        public int ManualHeight { get; set; } = 60;

        /// <summary>Kích thước ô ảnh (px) theo chế độ đang chọn.</summary>
        [Newtonsoft.Json.JsonIgnore]
        public Size CellSize
        {
            get
            {
                return SizeMode == ExportSizeMode.Ratio
                    ? new Size(Math.Max(1, RatioWidth * Factor), Math.Max(1, RatioHeight * Factor))
                    : new Size(Math.Max(1, ManualWidth), Math.Max(1, ManualHeight));
            }
        }

        public DatasetExportOptions Clone()
        {
            return (DatasetExportOptions)MemberwiseClone();
        }

        /// <summary>
        /// Tỷ lệ gọn gần nhất của một kích thước ảnh: ưu tiên các tỷ lệ quen thuộc (4:3, 16:9, 6:5...)
        /// nếu lệch dưới 2%, không thì tìm phân số gần nhất có mẫu số ≤ 20.
        /// Ví dụ 2472×2064 → 6:5, 1920×1080 → 16:9, 640×480 → 4:3.
        /// </summary>
        public static Size NiceRatio(Size size)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                return new Size(4, 3);
            }

            double target = (double)size.Width / size.Height;

            var common = new[]
            {
                new Size(1, 1), new Size(5, 4), new Size(4, 3), new Size(3, 2), new Size(16, 10),
                new Size(16, 9), new Size(6, 5), new Size(2, 1), new Size(4, 5), new Size(3, 4),
                new Size(2, 3), new Size(9, 16), new Size(5, 6)
            };

            Size best = common[0];
            double bestError = double.MaxValue;
            foreach (var ratio in common)
            {
                double error = Math.Abs((double)ratio.Width / ratio.Height - target) / target;
                if (error < bestError)
                {
                    bestError = error;
                    best = ratio;
                }
            }

            if (bestError <= 0.02)
            {
                return best;
            }

            bestError = double.MaxValue;
            for (int h = 1; h <= 20; h++)
            {
                int w = Math.Max(1, (int)Math.Round(target * h));
                double error = Math.Abs((double)w / h - target) / target;
                if (error < bestError - 1e-9)
                {
                    bestError = error;
                    best = new Size(w, h);
                }
            }

            return best;
        }
    }
}
