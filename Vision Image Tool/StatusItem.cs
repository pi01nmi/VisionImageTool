using System.Collections.Generic;

namespace VisionImageTool
{
    internal enum StatusLevel
    {
        Ok,
        Warning,
        Error
    }

    /// <summary>Khối gom nhóm các hạng mục trong cửa sổ trạng thái (theo thứ tự hiển thị).</summary>
    internal enum StatusGroup
    {
        Connection,
        System,
        Configuration,
        Data
    }

    /// <summary>Một thanh tài nguyên mini (CPU / GPU / RAM...).</summary>
    internal sealed class StatusMeter
    {
        public string Name;

        /// <summary>0..100.</summary>
        public double Percent;

        /// <summary>Chữ bên phải thanh ("12%", "3.2 / 16 GB").</summary>
        public string Text;
    }

    /// <summary>
    /// Một mục trong cửa sổ trạng thái: kết luận + giá trị đo được + việc cần làm.
    ///
    /// Detail hỗ trợ đánh dấu đơn giản:
    ///   **chữ đậm**   - thông số quan trọng (IP, cổng, ID)
    ///   [ok] [warn] [fail] - chấm tròn màu (badge) xanh / cam / đỏ
    /// </summary>
    internal sealed class StatusItem
    {
        public StatusLevel Level { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

        /// <summary>Cách xử lý khi mục này chưa OK (để trống nếu OK).</summary>
        public string Hint { get; set; }

        public StatusGroup Group { get; set; }

        /// <summary>Ký tự icon chức năng (font Segoe MDL2 Assets) - hiện khi mục OK.</summary>
        public string Icon { get; set; }

        /// <summary>Chữ xám mờ: thông tin phụ không cần chú ý (vd. "không có lỗi").</summary>
        public bool Muted { get; set; }

        /// <summary>Thanh tài nguyên mini hiện dưới dòng chi tiết.</summary>
        public List<StatusMeter> Meters { get; set; }

        public StatusItem(StatusLevel level, string title, string detail, string hint = null)
        {
            Level = level;
            Title = title;
            Detail = detail;
            Hint = hint;
        }

        public StatusItem In(StatusGroup group, string icon)
        {
            Group = group;
            Icon = icon;
            return this;
        }
    }

    /// <summary>Mã ký tự icon trong font Segoe MDL2 Assets (có sẵn từ Windows 10).</summary>
    internal static class StatusIcons
    {
        public const string Plug = "";       // Connect
        public const string Server = "";     // PC1
        public const string Globe = "";      // Globe
        public const string Pulse = "";      // Diagnostic
        public const string Mode = "";       // Setting
        public const string History = "";    // History
        public const string Camera = "";     // Camera
        public const string Sliders = "";    // Equalizer
        public const string Package = "";    // Package
        public const string Picture = "";    // Picture
        public const string Download = "";   // Download
        public const string Folder = "";     // Folder
        public const string Warning = "";    // Warning
        public const string Error = "";      // ErrorBadge
    }
}
