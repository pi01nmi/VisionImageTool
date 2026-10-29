using System;
using System.Drawing;

namespace VisionImageTool
{
    internal enum AppLogLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>Một dòng nhật ký: thời gian + mức + nội dung.</summary>
    internal sealed class LogEntry
    {
        public string Time;
        public AppLogLevel Level;
        public string Message;

        /// <summary>Nhãn mức ngắn (cột "Mức" và file log).</summary>
        public string LevelText
        {
            get { return AppLog.LevelText(Level); }
        }
    }

    /// <summary>
    /// Quy tắc log của ứng dụng:
    ///   Info    - diễn biến bình thường (lệnh gửi đi, kết quả nhận về, số liệu thời gian...).
    ///   Warning - bất thường nhưng vẫn chạy tiếp được (trễ, thiếu kết quả, bỏ qua file...).
    ///   Error   - thao tác thất bại / ngoại lệ, cần người dùng xử lý.
    /// Code mới gọi thẳng LogWarning / LogError. Các dòng Log(...) cũ được TỰ phân loại theo từ khoá
    /// trong nội dung (xem Classify) để không phải sửa hàng trăm chỗ gọi.
    /// </summary>
    internal static class AppLog
    {
        // Màu hiển thị trong cửa sổ log: nền nhạt + chữ đậm màu để dễ quét mắt mà không chói.
        public static Color WarningBack => Theme.Back(Color.FromArgb(255, 247, 230));
        public static Color WarningText => Theme.Fore(Color.FromArgb(180, 83, 9));
        public static Color ErrorBack => Theme.Back(Color.FromArgb(254, 236, 236));
        public static Color ErrorText => Theme.Fore(Color.FromArgb(185, 28, 28));
        public static Color SectionText => Theme.Fore(Color.FromArgb(29, 78, 216));
        public static Color InfoText => Theme.Fore(Color.FromArgb(31, 41, 55));

        private static readonly string[] ErrorMarks =
        {
            "LỖI", "Lỗi ", "lỗi:", " lỗi ", "thất bại", "THẤT BẠI", "FAIL", "Exception", "=> False", "=> FALSE",
            "KHÔNG ĐẠT", "không kết nối được", "Không kết nối được", "KHÔNG CÓ ẢNH", "KHÔNG HIỂN THỊ",
            "Không đọc được", "Không giải mã được", "không lưu được", "Không lưu được", "trả về NULL", "= NULL"
        };

        private static readonly string[] WarningMarks =
        {
            "CẢNH BÁO", "Cảnh báo", "LƯU Ý", "Lưu ý", "THIẾU", "HẾT ", "TRỄ", "quá nhỏ", "Bỏ qua", "bỏ qua",
            "không tồn tại", "chưa sẵn sàng", "Chưa kết nối", "chưa nhận được", "không nhận được", "rỗng"
        };

        public static AppLogLevel Classify(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return AppLogLevel.Info;
            }

            foreach (var mark in ErrorMarks)
            {
                if (message.IndexOf(mark, StringComparison.Ordinal) >= 0)
                {
                    return AppLogLevel.Error;
                }
            }

            foreach (var mark in WarningMarks)
            {
                if (message.IndexOf(mark, StringComparison.Ordinal) >= 0)
                {
                    return AppLogLevel.Warning;
                }
            }

            return AppLogLevel.Info;
        }

        public static string LevelText(AppLogLevel level)
        {
            switch (level)
            {
                case AppLogLevel.Warning: return "WARN";
                case AppLogLevel.Error: return "ERROR";
                default: return "INFO";
            }
        }

        /// <summary>Dòng tiêu đề khối ("=== ... ===", "----- ... -----") - tô xanh để chia đoạn.</summary>
        public static bool IsSection(string message)
        {
            string text = (message ?? string.Empty).TrimStart();
            return text.StartsWith("===", StringComparison.Ordinal) || text.StartsWith("-----", StringComparison.Ordinal);
        }
    }
}
