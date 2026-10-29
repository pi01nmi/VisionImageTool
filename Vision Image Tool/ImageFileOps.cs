using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>Thông tin nhanh của một file ảnh (chỉ đọc phần đầu file, không giải mã toàn bộ).</summary>
    internal sealed class ImageFileInfo
    {
        public string Path;
        public long Length;
        public DateTime Created;
        public DateTime Modified;
        public int Width;
        public int Height;
        public string Format;
        public string Channels;
        public int BitsPerPixel;
        public float DpiX;
        public float DpiY;
        public string Error;
    }

    /// <summary>
    /// Thao tác file ảnh dùng chung cho menu chuột phải của danh sách Raw / Mask (tab Capture) và
    /// bảng Dataset Manager: xoá vào Thùng rác, mở thư mục và chọn sẵn file, "Mở bằng...", copy ảnh
    /// vào clipboard, đổi tên có hoàn tác, đọc thuộc tính ảnh.
    ///
    /// Chỉ dùng Win32 (shell32) có sẵn trong Windows - không cần thêm reference nào vào project.
    /// </summary>
    internal static class ImageFileOps
    {
        #region Win32

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_SILENT = 0x0004;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOERRORUI = 0x0400;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr[] apidl, uint dwFlags);

        #endregion

        /// <summary>
        /// Chuyển các file vào Thùng rác (lấy lại được). Trả về danh sách file ĐÃ biến mất khỏi ổ
        /// đĩa; file nào còn lại (đang bị khoá, không có quyền...) nằm trong <paramref name="failed"/>.
        /// </summary>
        public static List<string> DeleteToRecycleBin(IWin32Window owner, IList<string> paths, out List<string> failed)
        {
            var existing = paths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            failed = new List<string>();

            if (existing.Count > 0)
            {
                var op = new SHFILEOPSTRUCT
                {
                    hwnd = owner != null ? owner.Handle : IntPtr.Zero,
                    wFunc = FO_DELETE,
                    // Danh sách đường dẫn ngăn cách bằng '\0', kết thúc bằng hai '\0'.
                    pFrom = string.Join("\0", existing) + "\0\0",
                    fFlags = (ushort)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI)
                };

                SHFileOperation(ref op);
            }

            var deleted = new List<string>();
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    failed.Add(path);
                }
                else
                {
                    deleted.Add(path);
                }
            }

            return deleted;
        }

        /// <summary>
        /// Mở Explorer và chọn sẵn các file. Các file khác thư mục thì mỗi thư mục mở một cửa sổ
        /// (tối đa 5 cửa sổ để tránh bung hàng loạt).
        /// </summary>
        public static void ShowInExplorer(IList<string> paths)
        {
            foreach (var group in paths.Where(File.Exists)
                .GroupBy(p => Path.GetDirectoryName(p), StringComparer.OrdinalIgnoreCase).Take(5))
            {
                IntPtr folder = ILCreateFromPathW(group.Key);
                var items = group.Select(ILCreateFromPathW).Where(p => p != IntPtr.Zero).ToArray();

                try
                {
                    if (folder == IntPtr.Zero || SHOpenFolderAndSelectItems(folder, (uint)items.Length, items, 0) != 0)
                    {
                        Process.Start("explorer.exe", "/select,\"" + group.First() + "\"");
                    }
                }
                finally
                {
                    foreach (var item in items)
                    {
                        ILFree(item);
                    }

                    if (folder != IntPtr.Zero)
                    {
                        ILFree(folder);
                    }
                }
            }
        }

        public static void OpenDefault(string path)
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        /// <summary>Hộp thoại "Mở bằng..." của Windows (chọn ứng dụng: Paint, Photoshop...).</summary>
        public static void OpenWith(string path)
        {
            Process.Start(new ProcessStartInfo("rundll32.exe", "shell32.dll,OpenAs_RunDLL " + path) { UseShellExecute = false });
        }

        public static void OpenInPaint(string path)
        {
            Process.Start(new ProcessStartInfo("mspaint.exe", "\"" + path + "\"") { UseShellExecute = true });
        }

        /// <summary>
        /// Đưa ảnh vào clipboard: dán vào Excel / Teams / Paint ra ẢNH, dán vào Explorer ra FILE.
        /// </summary>
        public static void CopyImageToClipboard(string path)
        {
            using (var bitmap = LoadBitmap(path))
            {
                var data = new DataObject();
                data.SetImage(new Bitmap(bitmap));
                var files = new StringCollection { path };
                data.SetFileDropList(files);
                Clipboard.SetDataObject(data, true);
            }
        }

        public static void CopyText(IEnumerable<string> lines)
        {
            string text = string.Join(Environment.NewLine, lines);
            if (text.Length > 0)
            {
                Clipboard.SetText(text);
            }
        }

        /// <summary>Đọc ảnh qua MemoryStream (Image.FromFile khoá file tới khi bitmap được giải phóng).</summary>
        public static Bitmap LoadBitmap(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(bytes, false))
            using (var decoded = Image.FromStream(ms, false, false))
            {
                return new Bitmap(decoded);
            }
        }

        public static ImageFileInfo ReadInfo(string path)
        {
            var info = new ImageFileInfo { Path = path };

            try
            {
                var file = new FileInfo(path);
                info.Length = file.Length;
                info.Created = file.CreationTime;
                info.Modified = file.LastWriteTime;

                // validateImageData = false: chỉ đọc phần header - vài ms kể cả ảnh 20 MP.
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var image = Image.FromStream(stream, false, false))
                {
                    info.Width = image.Width;
                    info.Height = image.Height;
                    info.DpiX = image.HorizontalResolution;
                    info.DpiY = image.VerticalResolution;
                    info.BitsPerPixel = Image.GetPixelFormatSize(image.PixelFormat);
                    info.Format = FormatName(image.RawFormat);
                    info.Channels = ChannelName(image);
                }
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
            }

            return info;
        }

        private static string FormatName(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg)) return "JPEG";
            if (format.Equals(ImageFormat.Png)) return "PNG";
            if (format.Equals(ImageFormat.Bmp)) return "BMP";
            if (format.Equals(ImageFormat.Tiff)) return "TIFF";
            if (format.Equals(ImageFormat.Gif)) return "GIF";
            return format.ToString();
        }

        private static string ChannelName(Image image)
        {
            // Ảnh xám 8 bit được GDI+ đọc thành 8bpp có bảng màu - kiểm tra bảng màu có phải thang xám.
            if ((image.PixelFormat & PixelFormat.Indexed) != 0)
            {
                var entries = image.Palette.Entries;
                bool gray = entries.Length > 0 && entries.All(c => c.R == c.G && c.G == c.B);
                return gray ? "1 kênh (Grayscale)" : "Bảng màu (" + entries.Length + " màu)";
            }

            switch (image.PixelFormat)
            {
                case PixelFormat.Format16bppGrayScale: return "1 kênh (Grayscale 16 bit)";
                case PixelFormat.Format24bppRgb: return "3 kênh (RGB)";
                case PixelFormat.Format32bppRgb: return "3 kênh (RGB)";
                case PixelFormat.Format48bppRgb: return "3 kênh (RGB 16 bit)";
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb: return "4 kênh (RGBA)";
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb: return "4 kênh (RGBA 16 bit)";
                default: return image.PixelFormat.ToString();
            }
        }

        public static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return (bytes / 1024.0 / 1024.0).ToString("0.##") + " MB";
            if (bytes >= 1024) return (bytes / 1024.0).ToString("0.#") + " KB";
            return bytes + " B";
        }

        /// <summary>Lỗi của một tên file mới (không kèm thư mục), null nếu hợp lệ.</summary>
        public static string ValidateFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Tên file không được để trống.";
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return "Tên file chứa ký tự không hợp lệ ( \\ / : * ? \" < > | ).";
            }

            if (name.EndsWith(".", StringComparison.Ordinal) || name.EndsWith(" ", StringComparison.Ordinal))
            {
                return "Tên file không được kết thúc bằng dấu chấm hoặc dấu cách.";
            }

            return null;
        }

        /// <summary>
        /// Suy ra tên mới cho ảnh CẶP khi đổi tên một ảnh, dựa trên chỗ khác nhau giữa hai tên cũ.
        ///   Raw  M1_DK1_V1_Raw_OK_ts   / Mask M1_DK1_V1_Mask_OK_ts  -> đổi "Raw" thành "Mask"
        ///   Raw  X_OK_ts               / Mask X_mask_OK_ts          -> chèn "mask_" trước "OK_ts"
        ///   Raw  abc                   / Mask abcmask_1_OK          -> nối "mask_1_OK" vào cuối
        /// Áp cùng phép biến đổi đó lên tên mới. Trả về null nếu không suy ra được (người dùng tự
        /// nhập tên cho ảnh cặp). Các tên ở đây KHÔNG kèm phần mở rộng.
        /// </summary>
        public static string DerivePairName(string oldMain, string oldPair, string newMain)
        {
            if (string.IsNullOrEmpty(oldMain) || string.IsNullOrEmpty(oldPair) || string.IsNullOrEmpty(newMain))
            {
                return null;
            }

            int prefix = 0;
            while (prefix < oldMain.Length && prefix < oldPair.Length && oldMain[prefix] == oldPair[prefix])
            {
                prefix++;
            }

            int suffix = 0;
            while (suffix < oldMain.Length - prefix && suffix < oldPair.Length - prefix
                && oldMain[oldMain.Length - 1 - suffix] == oldPair[oldPair.Length - 1 - suffix])
            {
                suffix++;
            }

            string mainMid = oldMain.Substring(prefix, oldMain.Length - prefix - suffix);
            string pairMid = oldPair.Substring(prefix, oldPair.Length - prefix - suffix);
            string tail = oldMain.Substring(oldMain.Length - suffix);

            // Ưu tiên neo theo phần ĐUÔI chung (thường là kết quả + timestamp, ít bị sửa), rồi mới
            // neo theo phần đầu.
            int tailStart = newMain.EndsWith(tail, StringComparison.Ordinal) ? newMain.Length - tail.Length : -1;
            if (tailStart >= mainMid.Length
                && string.CompareOrdinal(newMain, tailStart - mainMid.Length, mainMid, 0, mainMid.Length) == 0)
            {
                int at = tailStart - mainMid.Length;
                return newMain.Substring(0, at) + pairMid + newMain.Substring(tailStart);
            }

            string head = oldMain.Substring(0, prefix);
            if (newMain.StartsWith(head, StringComparison.Ordinal) && newMain.Length >= prefix + mainMid.Length
                && string.CompareOrdinal(newMain, prefix, mainMid, 0, mainMid.Length) == 0)
            {
                return newMain.Substring(0, prefix) + pairMid + newMain.Substring(prefix + mainMid.Length);
            }

            // Tên mới không còn giữ phần khác biệt ở vị trí cũ: thử thay lần xuất hiện CUỐI của nó.
            if (mainMid.Length > 0)
            {
                int last = newMain.LastIndexOf(mainMid, StringComparison.Ordinal);
                if (last >= 0)
                {
                    return newMain.Substring(0, last) + pairMid + newMain.Substring(last + mainMid.Length);
                }
            }

            return null;
        }

        /// <summary>
        /// Đổi tên nhiều file như MỘT thao tác: lỗi giữa chừng thì đổi ngược các file đã đổi.
        /// </summary>
        public static bool RenameAll(IList<KeyValuePair<string, string>> moves, out string error)
        {
            error = null;
            var done = new List<KeyValuePair<string, string>>();

            foreach (var move in moves)
            {
                if (!string.Equals(move.Key, move.Value, StringComparison.OrdinalIgnoreCase) && File.Exists(move.Value))
                {
                    error = "Đã có file trùng tên:" + Environment.NewLine + move.Value;
                    return false;
                }
            }

            try
            {
                foreach (var move in moves)
                {
                    if (string.Equals(move.Key, move.Value, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    File.Move(move.Key, move.Value);
                    done.Add(move);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;

                for (int i = done.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        File.Move(done[i].Value, done[i].Key);
                    }
                    catch
                    {
                        // Không hoàn tác được thì thôi - thông báo lỗi gốc là đủ để người dùng kiểm tra.
                    }
                }

                return false;
            }
        }
    }
}
