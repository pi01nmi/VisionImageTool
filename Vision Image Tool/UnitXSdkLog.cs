using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace VisionImageTool
{
    /// <summary>Lý do một lệnh SDK thất bại, đọc từ unitx_sdk.log và diễn giải sang tiếng Việt.</summary>
    internal sealed class SdkFailure
    {
        /// <summary>Dòng trả lời nguyên văn của UnitX (vd "Trigger camera response: 0 Failed ...").</summary>
        public string Response;

        /// <summary>Các dòng lỗi / cảnh báo khác SDK ghi trong lúc chạy lệnh (gRPC...).</summary>
        public List<string> Errors = new List<string>();

        /// <summary>Nguyên nhân (đã diễn giải).</summary>
        public string Reason;

        /// <summary>Cách xử lý.</summary>
        public string Fix;

        public string ToMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Nguyên nhân: " + Reason);
            if (!string.IsNullOrEmpty(Fix))
            {
                sb.AppendLine().AppendLine("Cách xử lý: " + Fix);
            }

            if (!string.IsNullOrEmpty(Response) || Errors.Count > 0)
            {
                sb.AppendLine().AppendLine("UnitX trả lời:");
                foreach (var line in Errors.Concat(string.IsNullOrEmpty(Response) ? new string[0] : new[] { Response }).Distinct().Take(6))
                {
                    sb.AppendLine("   " + line);
                }
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Đọc phần log unitx_sdk.log mà SDK ghi TRONG LÚC chạy một lệnh (đánh dấu độ dài file trước lệnh,
    /// đọc phần ghi thêm sau lệnh) rồi diễn giải các câu trả lời thường gặp của UnitX.
    /// </summary>
    internal static class UnitXSdkLog
    {
        private static readonly Regex LevelPrefix = new Regex(@"^LogLevel:\s*(\d+)\s*\|\s*", RegexOptions.Compiled);

        /// <summary>Độ dài file log hiện tại - gọi TRƯỚC lệnh SDK.</summary>
        public static long Mark(string path)
        {
            try
            {
                return File.Exists(path) ? new FileInfo(path).Length : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Đọc phần log ghi thêm từ <paramref name="mark"/> và diễn giải lý do thất bại.</summary>
        public static SdkFailure Explain(string path, long mark)
        {
            var lines = ReadSince(path, mark);

            // SDK có thể ghi log trễ vài chục ms sau khi hàm trả về.
            if (!lines.Any(l => l.IndexOf("response:", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Thread.Sleep(150);
                lines = ReadSince(path, mark);
            }

            var failure = new SdkFailure();
            foreach (var raw in lines)
            {
                var m = LevelPrefix.Match(raw);
                int level = m.Success ? int.Parse(m.Groups[1].Value) : 0;
                string text = m.Success ? raw.Substring(m.Length).Trim() : raw.Trim();

                if (text.IndexOf("response:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Nhiều dòng trả lời: giữ dòng THẤT BẠI ("response: 0 ...") thay vì dòng thành công đến sau.
                    bool failed = Regex.IsMatch(text, @"response:\s*0\b", RegexOptions.IgnoreCase);
                    bool currentFailed = failure.Response != null && Regex.IsMatch(failure.Response, @"response:\s*0\b", RegexOptions.IgnoreCase);
                    if (failed || !currentFailed)
                    {
                        failure.Response = text;
                    }
                }
                else if (level >= 2 || text.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0
                    || text.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    failure.Errors.Add(text);
                }
            }

            Interpret(failure);
            return failure;
        }

        private static List<string> ReadSince(string path, long mark)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return new List<string>();
                }

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    // File bị SDK tạo lại (ngắn hơn mốc) -> đọc từ đầu.
                    stream.Position = mark <= stream.Length ? mark : 0;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd()
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>Các câu trả lời hay gặp (rút từ unitx_sdk.log thực tế) -> nguyên nhân + cách xử lý.</summary>
        private static void Interpret(SdkFailure f)
        {
            string all = string.Join("\n", f.Errors.Concat(new[] { f.Response ?? string.Empty }));
            Match m;

            if (Contains(all, "failed to connect to all addresses") || Contains(all, "OS Error"))
            {
                f.Reason = "Không kết nối được tới UnitX (lệnh gRPC không tới được máy UnitX).";
                f.Fix = "Kiểm tra máy UnitX đã bật, dây mạng, IP và cổng gRPC trong thẻ cấu hình mạng - bấm 'Kiểm tra mạng'.";
            }
            else if ((m = Regex.Match(all, @"Camera id (\S+) does not exist", RegexOptions.IgnoreCase)).Success)
            {
                f.Reason = $"Camera \"{m.Groups[1].Value}\" không tồn tại trong cấu hình Production đang chạy trên UnitX.";
                f.Fix = "Chọn đúng Camera ID (Check Status > Cấu hình sản xuất liệt kê camera hợp lệ), hoặc kiểm tra Part Type "
                    + "đang start có dùng camera này không, rồi 'Restart Part'.";
            }
            else if ((m = Regex.Match(all, @"Part type (\S+) does not exist", RegexOptions.IgnoreCase)).Success)
            {
                f.Reason = $"Part Type \"{m.Groups[1].Value}\" không có trong production.py (phân biệt chữ hoa / thường).";
                f.Fix = "Chọn lại Part Type đúng như production.py (Check Status > Cấu hình sản xuất).";
            }
            else if ((m = Regex.Match(all, @"External image part type (\S+) not supported", RegexOptions.IgnoreCase)).Success)
            {
                f.Reason = $"Part Type \"{m.Groups[1].Value}\" không có nguồn source = \"external_image\" nên không nhận ảnh push.";
                f.Fix = "Thêm nguồn external_image cho part type này trong production.py (Push sẽ hiện cấu hình đề xuất).";
            }
            else if ((m = Regex.Match(all, @"shape \((\d+),\s*(\d+),\s*(\d+)\) not match.*?hwc \((\d+),\s*(\d+),\s*(\d+)\)", RegexOptions.IgnoreCase)).Success)
            {
                f.Reason = $"Ảnh push {m.Groups[2].Value} × {m.Groups[1].Value} px ({m.Groups[3].Value} kênh) KHÁC kích thước khai báo trong "
                    + $"production.py: {m.Groups[5].Value} × {m.Groups[4].Value} px ({m.Groups[6].Value} kênh).";
                f.Fix = "Sửa image_width / image_height của nguồn external_image cho đúng kích thước ảnh, hoặc dùng ảnh đúng kích thước.";
            }
            else if (f.Response != null && Regex.IsMatch(f.Response, @"response:\s*0\s*Failed to [^:]*:\s*$", RegexOptions.IgnoreCase))
            {
                f.Reason = "UnitX từ chối lệnh nhưng không nêu lý do.";
                f.Fix = "Thường do chưa có part group đang chạy hoặc UnitX đang bận / kẹt - bấm 'Restart Part' (System Reset + "
                    + "Part Start) rồi thử lại; vẫn lỗi thì xem log trên máy UnitX.";
            }
            else if (f.Response != null && (m = Regex.Match(f.Response, @"response:\s*0\s*(.+)$", RegexOptions.IgnoreCase)).Success)
            {
                f.Reason = "UnitX báo: " + m.Groups[1].Value.Trim();
                f.Fix = "Xem log chi tiết; nếu lỗi lặp lại, bấm 'Restart Part'.";
            }
            else if (f.Response == null && f.Errors.Count == 0)
            {
                f.Reason = "SDK không ghi lý do nào vào unitx_sdk.log cho lệnh này.";
                f.Fix = "Kiểm tra kết nối (Check Status) rồi thử lại.";
            }
            else
            {
                f.Reason = "UnitX từ chối lệnh.";
                f.Fix = "Xem các dòng UnitX trả lời bên dưới.";
            }
        }

        private static bool Contains(string text, string value)
        {
            return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
