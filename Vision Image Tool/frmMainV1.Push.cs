using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code. Nếu class đầu tiên là partial frmMainV1, VS tự gắn SubType=Form rồi mở Designer - Designer
    // không thấy lớp cha ("base class System.Void") và đã từng ghi đè frmMainV1.Designer.cs thành form
    // trống (2026-09-25). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1PushCodeFile { }

    /// <summary>
    /// frmMainV1 - phần xử lý ảnh và push sang UnitX: quét thư mục, chuẩn bị payload, vòng push,
    /// theo dõi thư mục tự động, lịch sử đã push và đo thời gian.
    ///
    /// Toàn bộ phần này KHÔNG chứa event handler của giao diện (trừ vài nút gọi thẳng vào đây),
    /// và chạy ở luồng nền - mọi cập nhật giao diện đi qua các helper trong frmMainV1.cs.
    /// </summary>
    public partial class frmMainV1
    {
        #region Kiểm tra cấu hình Production trước khi push

        /// <summary>
        /// Điều kiện bắt buộc trước khi push (đọc lại production.py từ UnitX mỗi lần):
        ///   1. Part Type đang chọn có nguồn source = "external_image" (ảnh ngoài - ảnh push lên).
        ///   2. image_width / image_height của nguồn đó khớp HOÀN TOÀN kích thước thật của ảnh sẽ push.
        /// Không đạt thì KHÔNG push, hiện danh sách lỗi + nút "Hiển thị production.py đề xuất".
        /// </summary>
        private bool ValidateProductionForPush(IList<PushItem> items)
        {
            string partType = (cboPartType.Text ?? string.Empty).Trim();
            string captureConfig = (cboCaptureConfig.Text ?? string.Empty).Trim();
            var issues = new List<string>();

            ProdConfigData prod;
            List<KeyValuePair<string, ImageFileInfo>> sizes;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                prod = ReadProdConfiguration();

                // Chỉ đọc header ảnh - vài ms / ảnh kể cả ảnh 20 MP.
                sizes = items.Select(i => new KeyValuePair<string, ImageFileInfo>(i.FullPath, ImageFileOps.ReadInfo(i.FullPath))).ToList();
            }
            finally
            {
                Cursor.Current = previousCursor;
            }

            var readable = sizes.Where(s => s.Value.Error == null).ToList();
            var sizeGroups = readable
                .GroupBy(s => new Size(s.Value.Width, s.Value.Height))
                .OrderByDescending(g => g.Count())
                .ToList();
            Size common = sizeGroups.Count > 0 ? sizeGroups[0].Key : Size.Empty;

            if (!prod.Ok)
            {
                issues.Add("Không đọc được cấu hình Production từ UnitX: " + (prod.Error ?? "không rõ lỗi"));
            }
            else if (prod.PartTypes.Count == 0)
            {
                issues.Add("production.py chưa khai báo Part Type nào"
                    + (prod.ImagingConfigError != null ? " (cấu hình không đọc được: " + prod.ImagingConfigError + ")." : "."));
            }
            else
            {
                var part = prod.PartTypes.FirstOrDefault(p => string.Equals(p.Name, partType, StringComparison.Ordinal));
                if (part == null)
                {
                    issues.Add($"Part Type \"{partType}\" không có trong production.py (đang có: {string.Join(", ", prod.AllPartTypes)}).");
                }
                else
                {
                    var external = part.Sources
                        .Where(s => string.Equals(s.Source, ProductionPy.ExternalSource, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (external.Count == 0)
                    {
                        issues.Add($"Part Type \"{partType}\": source phải là \"{ProductionPy.ExternalSource}\" "
                            + $"(đang là: {string.Join(", ", part.Sources.Select(s => "\"" + s.Source + "\""))}).");
                    }
                    else
                    {
                        var entry = external.FirstOrDefault(s => s.CaptureNetworks.Any(c => c.Key == captureConfig)) ?? external[0];

                        // Capture config không thuộc Part Type: UnitX từ chối mọi ảnh với "'NoneType' object is not iterable"
                        // (log 2026-10-14: part_type nitto_pr3_dome1 chỉ có cc "test2" nhưng push với cc "test").
                        var validConfigs = external.SelectMany(s => s.CaptureNetworks.Select(c => c.Key)).Distinct().ToList();
                        if (validConfigs.Count > 0 && !validConfigs.Contains(captureConfig))
                        {
                            issues.Add($"Capture config \"{captureConfig}\" không thuộc Part Type \"{partType}\" "
                                + $"(cc_network_mapping chỉ có: {string.Join(", ", validConfigs.Select(c => "\"" + c + "\""))}). "
                                + "Chọn đúng capture config rồi push lại.");
                        }

                        int? width = ExtraInt(entry, "image_width");
                        int? height = ExtraInt(entry, "image_height");

                        if (width == null || height == null)
                        {
                            issues.Add($"Nguồn \"{ProductionPy.ExternalSource}\" của Part Type \"{partType}\" chưa khai báo "
                                + "image_width / image_height.");
                        }
                        else
                        {
                            var wrong = readable.Where(s => s.Value.Width != width || s.Value.Height != height).ToList();
                            if (wrong.Count > 0)
                            {
                                issues.Add($"{wrong.Count} / {items.Count} ảnh có kích thước khác production.py ({width} × {height} px), "
                                    + $"ví dụ {Path.GetFileName(wrong[0].Key)}: {wrong[0].Value.Width} × {wrong[0].Value.Height} px.");
                            }
                        }
                    }
                }
            }

            if (sizeGroups.Count > 1)
            {
                issues.Add($"Ảnh cần push có {sizeGroups.Count} kích thước khác nhau "
                    + $"({string.Join(", ", sizeGroups.Take(4).Select(g => g.Key.Width + "×" + g.Key.Height + ": " + g.Count() + " ảnh"))}) - "
                    + "một nguồn external_image chỉ khai báo được một kích thước.");
            }

            foreach (var bad in sizes.Where(s => s.Value.Error != null).Take(3))
            {
                issues.Add($"Không đọc được kích thước ảnh {Path.GetFileName(bad.Key)}: {bad.Value.Error}");
            }

            if (issues.Count == 0)
            {
                Log($"Kiểm tra production.py: OK - Part Type '{partType}', source external_image, ảnh {common.Width} x {common.Height}.");
                return true;
            }

            Log("KIỂM TRA production.py KHÔNG ĐẠT - không push:");
            foreach (var issue in issues)
            {
                Log("   - " + issue);
            }

            string subtitle = $"Part Type \"{partType}\"  •  capture config \"{captureConfig}\"  •  {items.Count} ảnh"
                + (common.IsEmpty ? string.Empty : $" {common.Width} × {common.Height} px");
            Func<string> suggestion = prod.Ok && !common.IsEmpty && partType.Length > 0
                ? () => ProductionPy.Suggest(prod, partType, captureConfig, common)
                : (Func<string>)null;

            using (var dialog = new frmProdCheck(issues, subtitle, suggestion))
            {
                dialog.ShowDialog(this);
            }

            UpdateStatus("Chưa push: cấu hình production.py không khớp - xem hộp kiểm tra.");
            return false;
        }

        private static int? ExtraInt(PartSource source, string key)
        {
            foreach (var kv in source.Extra)
            {
                int value;
                if (kv.Key == key && kv.Value != null
                    && int.TryParse(kv.Value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
            }

            return null;
        }

        #endregion

        #region Quét thư mục

        /// <summary>
        /// Quét thư mục ảnh: nạp danh sách ảnh Raw (bỏ qua thư mục con "mask") và danh sách ảnh
        /// Mask đã có. "Gồm cả thư mục con" là mặc định cứng trong code (Recursive = true).
        /// </summary>
        private void ScanFolder(bool showErrors)
        {
            // Kiểm tra định dạng đường dẫn, chuẩn hoá, và tạo thư mục nếu chưa có. Lúc khởi động
            // (showErrors = false) thì KHÔNG tự tạo - chỉ bỏ qua, tránh việc mở app lên là tự dựng
            // lại một thư mục người dùng vừa xoá.
            // ResolveImageRoot cũng gán luôn _imageRootFolder cho luồng callback dùng khi lưu ảnh.
            string folder = ResolveImageRoot(showErrors, showErrors);
            if (folder == null)
            {
                return;
            }

            // Thư mục đổi -> số thứ tự ảnh phải dò lại từ đầu trong thư mục mới.
            Interlocked.Exchange(ref _imageSequence, 0);

            _rawItems.Clear();
            _rawIndexByPath.Clear();
            _rawIndexByBaseName.Clear();
            _maskItems.Clear();
            _rawPage = 0;
            _maskPage = 0;

            try
            {
                var files = Directory
                    .EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int maskCount = 0;
                var rawOutputs = new List<string>();

                foreach (var file in files)
                {
                    // Ảnh do CHÍNH ỨNG DỤNG lưu ra (PATH\raw, PATH\mask) tuyệt đối không được coi là
                    // ảnh nguồn, nếu không sẽ bị push ngược trở lại UnitX ở lần quét sau.
                    var kind = ClassifyScannedFile(folder, file);

                    if (kind == ScannedKind.RawOutput)
                    {
                        // Bản sao ảnh gốc do callback trả về - ảnh gốc đã nằm trong danh sách rồi.
                        // Riêng ảnh camera chụp thì KHÔNG có ảnh gốc nào: xét lại sau vòng lặp.
                        rawOutputs.Add(file);
                        continue;
                    }

                    bool isMask = kind == ScannedKind.MaskOutput;

                    FileInfo info;
                    try
                    {
                        info = new FileInfo(file);
                    }
                    catch (Exception ex)
                    {
                        Log($"Bỏ qua {Path.GetFileName(file)}: {ex.Message}");
                        continue;
                    }

                    if (isMask)
                    {
                        maskCount++;
                        _maskItems.Add(new MaskItem
                        {
                            FullPath = file,
                            LastWrite = info.LastWriteTime
                        });
                        continue;
                    }

                    var item = new PushItem
                    {
                        FullPath = file,
                        Length = info.Length,
                        LastWriteUtc = info.LastWriteTimeUtc,
                        LastWrite = info.LastWriteTime
                    };

                    string maskPath = FindMaskPath(file);
                    item.MaskPath = maskPath;
                    item.State = maskPath != null ? "Đã có mask" : "Chờ push";
                    item.StateColor = maskPath != null ? DoneColor : Color.White;

                    _rawIndexByPath[file] = _rawItems.Count;

                    string baseName = Path.GetFileNameWithoutExtension(file);
                    if (!_rawIndexByBaseName.ContainsKey(baseName))
                    {
                        _rawIndexByBaseName[baseName] = _rawItems.Count;
                    }

                    _rawItems.Add(item);
                }

                // Ảnh trong PATH\raw mà (bỏ đuôi [_raw][_OK/_NG]) không trùng tên ảnh nguồn nào = ảnh camera
                // chụp -> đưa vào danh sách để xem lại được, đánh dấu FromCamera để không bị push.
                foreach (var file in rawOutputs)
                {
                    string stripped = System.Text.RegularExpressions.Regex.Replace(
                        Path.GetFileNameWithoutExtension(file), @"(_raw)?(_(OK|NG|LIMIT))?$", string.Empty,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (_rawIndexByBaseName.ContainsKey(stripped))
                    {
                        continue;
                    }

                    _rawIndexByPath[file] = _rawItems.Count;
                    _rawItems.Add(CreateCameraRawItem(file, null, null));
                }

                _maskItems.Sort((a, b) => string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase));

                int missing = _rawItems.Count(x => x.MaskPath == null);
                RenderRawPage();
                RenderMaskPage();

                OnFolderScanned(folder, files);
                Log($"Quét xong: {_rawItems.Count} ảnh Raw, {maskCount} ảnh Mask, CÒN THIẾU {missing}.");
                UpdateStatus($"Raw: {_rawItems.Count} | Mask: {maskCount} | Còn thiếu: {missing}");
            }
            catch (Exception ex)
            {
                Log("Lỗi khi quét thư mục: " + ex.Message);

                if (showErrors)
                {
                    MessageBox.Show(this, "Lỗi khi quét thư mục:" + Environment.NewLine + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Một file ảnh tìm thấy khi quét thuộc loại nào.</summary>
        private enum ScannedKind
        {
            /// <summary>Ảnh nguồn của người dùng - đưa vào danh sách Raw, được phép push.</summary>
            Source = 0,
            /// <summary>Ảnh kết quả (mask/overlay) do ứng dụng lưu ra - đưa vào danh sách Mask.</summary>
            MaskOutput = 1,
            /// <summary>Bản sao ảnh gốc do ứng dụng lưu ra trong PATH\raw - bỏ qua hẳn.</summary>
            RawOutput = 2
        }

        /// <summary>
        /// Phân loại file ảnh theo VỊ TRÍ THẬT của nó chứ không chỉ theo tên thư mục cha: chỉ những
        /// file nằm đúng trong PATH\raw, PATH\mask (và PATH\camera của bản cũ) mới bị coi là ảnh do
        /// ứng dụng sinh ra. So khớp bằng đường dẫn đầy đủ để một thư mục ảnh nguồn tình cờ tên là
        /// "raw" nằm chỗ khác vẫn được quét bình thường.
        /// </summary>
        private ScannedKind ClassifyScannedFile(string root, string file)
        {
            string dir = Path.GetDirectoryName(file) ?? string.Empty;

            if (IsUnderSubFolder(root, RawFolderName, dir))
            {
                return ScannedKind.RawOutput;
            }

            if (IsUnderSubFolder(root, MaskFolderName, dir) || IsUnderSubFolder(root, CameraFolderName, dir))
            {
                return ScannedKind.MaskOutput;
            }

            // Các bản trước lưu mask ngay cạnh ảnh gốc (<thư mục ảnh>\mask\) và ảnh camera vào
            // <PATH>\camera\ - vẫn phải nhận ra để không push lại chúng.
            string parentName = Path.GetFileName(dir);
            if (string.Equals(parentName, MaskFolderName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parentName, CameraFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return ScannedKind.MaskOutput;
            }

            return ScannedKind.Source;
        }

        private static bool IsUnderSubFolder(string root, string subFolder, string dir)
        {
            if (string.IsNullOrWhiteSpace(root) || string.IsNullOrEmpty(dir))
            {
                return false;
            }

            try
            {
                string target = Path.GetFullPath(Path.Combine(root, subFolder))
                    .TrimEnd(Path.DirectorySeparatorChar);
                string full = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar);

                return string.Equals(full, target, StringComparison.OrdinalIgnoreCase)
                    || full.StartsWith(target + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Cấu hình push

        /// <summary>
        /// Gom cấu hình từ giao diện. Trả về null nếu thiếu thông tin bắt buộc.
        /// Edge KHÔNG có trên giao diện: được suy ra từ part_type (quy ước camera{trạm}-{model} của
        /// frmMainV8/V12) và có thể ghi đè bằng khoá "Edge" trong settings.json.
        /// </summary>
        private PushConfig BuildConfig()
        {
            string partType = (cboPartType.Text ?? string.Empty).Trim();
            string partId = (txtPartID.Text ?? string.Empty).Trim();
            string captureConfig = (cboCaptureConfig.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(partType) || string.IsNullOrEmpty(partId) || string.IsNullOrEmpty(captureConfig))
            {
                MessageBox.Show(this,
                    "Cần nhập đủ Part Type, Part ID và Capture Config." + Environment.NewLine +
                    "(PushImage trả về false ngay nếu một trong các tham số này rỗng.)",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // Callback đọc giá trị này từ field (không truy cập control từ luồng SDK).
            _saveMask = chkSaveCallback.Checked;

            return new PushConfig
            {
                PartType = partType,
                PartId = partId,
                CaptureConfig = captureConfig,
                Edge = ResolveEdgeName(),
                DelayMs = (int)numDelay.Value,
                TimeoutMs = _timeoutMs
            };
        }

        private sealed class PushConfig
        {
            public string PartType { get; set; }
            public string PartId { get; set; }
            public string CaptureConfig { get; set; }
            public string Edge { get; set; }
            public int DelayMs { get; set; }
            public int TimeoutMs { get; set; }
        }

        #endregion

        #region Định dạng ảnh

        /// <summary>
        /// Nhận diện định dạng bằng magic bytes (không tin phần mở rộng, vì file .bmp có thể thực
        /// chất là JPEG và ngược lại).
        /// </summary>
        private static ImageKind DetectKind(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8)
            {
                return ImageKind.Unknown;
            }

            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return ImageKind.Png;
            }

            if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            {
                return ImageKind.Bmp;
            }

            if ((bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) ||
                (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A))
            {
                return ImageKind.Tiff;
            }

            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return ImageKind.Jpeg;
            }

            return ImageKind.Unknown;
        }

        private static System.Drawing.Imaging.ImageFormat ToImageFormat(ImageKind kind)
        {
            switch (kind)
            {
                case ImageKind.Bmp: return System.Drawing.Imaging.ImageFormat.Bmp;
                case ImageKind.Tiff: return System.Drawing.Imaging.ImageFormat.Tiff;
                case ImageKind.Jpeg: return System.Drawing.Imaging.ImageFormat.Jpeg;
                default: return System.Drawing.Imaging.ImageFormat.Png;
            }
        }

        /// <summary>
        /// Kết quả chuẩn bị dữ liệu để push. Buffer có thể DÀI HƠN Length (dùng lại bộ đệm),
        /// nên luôn phải đọc theo Length chứ không phải Buffer.Length.
        /// </summary>
        private sealed class PreparedPayload
        {
            public byte[] Buffer;
            public int Length;
            public string Note;
            public double DecodeMs;
            public double EncodeMs;
        }

        // Bộ đệm dùng lại giữa các ảnh: tránh cấp phát ~37 MB trên Large Object Heap mỗi vòng.
        private byte[] _payloadBuffer = new byte[0];
        private readonly MemoryStream _encodeStream = new MemoryStream();

        private byte[] EnsureBuffer(int size)
        {
            if (_payloadBuffer.Length < size)
            {
                _payloadBuffer = new byte[size];
            }

            return _payloadBuffer;
        }

        /// <summary>
        /// Giải mã ảnh nguồn rồi encode lại theo định dạng UnitX nhận được.
        /// UnitX chỉ nhận PNG/BMP/TIFF ("Image type is not supported, only support ['png','bmp',
        /// 'tif','tiff']") nên JPEG bắt buộc phải convert. V13 dùng cố định BMP như mặc định đã
        /// chạy ổn ở V12 (PushConvertMode), không đưa lựa chọn này lên giao diện.
        /// </summary>
        private PreparedPayload PreparePayload(byte[] raw)
        {
            var result = new PreparedPayload();
            ImageKind kind = DetectKind(raw);
            ImageKind target = PushConvertMode;

            if (target == kind)
            {
                _lastPushSamples = kind == ImageKind.Bmp ? SampleBmp(raw) : null;
                result.Buffer = raw;
                result.Length = raw.Length;
                result.Note = kind.ToString().ToLowerInvariant() + " (giữ nguyên)";
                return result;
            }

            long tDecode = Stopwatch.GetTimestamp();

            using (var input = new MemoryStream(raw, false))
            using (var decoded = Image.FromStream(input, false, false))
            {
                var asBitmap = decoded as Bitmap;
                Bitmap decoded24;
                bool ownDecoded24;

                // Đa số JPEG giải mã ra đã là 24bpp -> dùng thẳng, khỏi vẽ lại tốn ~150 ms.
                if (asBitmap != null && asBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    decoded24 = asBitmap;
                    ownDecoded24 = false;
                }
                else
                {
                    decoded24 = new Bitmap(decoded.Width, decoded.Height,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(decoded24))
                    {
                        g.DrawImage(decoded, 0, 0, decoded24.Width, decoded24.Height);
                    }
                    ownDecoded24 = true;
                }

                try
                {
                    // GDI+ giải mã LƯỜI: Image.FromStream mới chỉ đọc header, pixel chỉ được bung ra
                    // ở lần truy cập đầu tiên. Không ép bung ở đây thì toàn bộ chi phí giải mã JPEG
                    // bị tính nhầm sang bước encode.
                    var probe = decoded24.LockBits(
                        new Rectangle(0, 0, decoded24.Width, decoded24.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    decoded24.UnlockBits(probe);

                    // Màu ảnh push để so với ảnh UnitX trả về (tự nhận biết đảo kênh R/B, xem ShouldSwapResult).
                    _lastPushSamples = SamplePixels(decoded24);

                    result.DecodeMs = ElapsedMs(tDecode);

                    long tEncode = Stopwatch.GetTimestamp();

                    if (target == ImageKind.Bmp)
                    {
                        // Tự ghi BMP từ dữ liệu pixel, không qua bộ encode của GDI+.
                        result.Length = WriteBmp24(decoded24, out result.Buffer);
                    }
                    else
                    {
                        _encodeStream.SetLength(0);
                        _encodeStream.Position = 0;
                        decoded24.Save(_encodeStream, ToImageFormat(target));

                        // GetBuffer() trả về mảng nội bộ - không sao chép như ToArray().
                        result.Buffer = _encodeStream.GetBuffer();
                        result.Length = (int)_encodeStream.Length;
                    }

                    result.EncodeMs = ElapsedMs(tEncode);
                    result.Note = $"{kind.ToString().ToLowerInvariant()} -> {target.ToString().ToLowerInvariant()}, "
                        + $"{FormatSize(raw.Length)} -> {FormatSize(result.Length)}";

                    return result;
                }
                finally
                {
                    if (ownDecoded24)
                    {
                        decoded24.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Ghi bitmap 24bpp thành file BMP trong bộ đệm dùng lại. Nhanh hơn Image.Save(ImageFormat.Bmp)
        /// vì bỏ qua toàn bộ lớp encoder của GDI+, chỉ dựng header rồi sao chép từng dòng pixel.
        /// </summary>
        private int WriteBmp24(Bitmap bitmap, out byte[] buffer)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = ((width * 3) + 3) / 4 * 4;   // mỗi dòng căn theo bội số 4 byte
            int imageSize = stride * height;
            int fileSize = 54 + imageSize;

            buffer = EnsureBuffer(fileSize);

            // BITMAPFILEHEADER (14 byte)
            buffer[0] = (byte)'B';
            buffer[1] = (byte)'M';
            WriteInt32(buffer, 2, fileSize);
            WriteInt32(buffer, 6, 0);
            WriteInt32(buffer, 10, 54);

            // BITMAPINFOHEADER (40 byte)
            WriteInt32(buffer, 14, 40);
            WriteInt32(buffer, 18, width);
            WriteInt32(buffer, 22, height);       // dương = ảnh lưu từ dưới lên
            buffer[26] = 1; buffer[27] = 0;       // planes
            buffer[28] = 24; buffer[29] = 0;      // bit/pixel
            WriteInt32(buffer, 30, 0);            // BI_RGB, không nén
            WriteInt32(buffer, 34, imageSize);
            WriteInt32(buffer, 38, 2835);         // 72 DPI
            WriteInt32(buffer, 42, 2835);
            WriteInt32(buffer, 46, 0);
            WriteInt32(buffer, 50, 0);

            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                for (int y = 0; y < height; y++)
                {
                    // BMP lưu ngược từ dòng cuối lên dòng đầu.
                    IntPtr src = IntPtr.Add(data.Scan0, (height - 1 - y) * data.Stride);
                    Marshal.Copy(src, buffer, 54 + y * stride, stride);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return fileSize;
        }

        private static void WriteInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        /// <summary>
        /// Tạo ảnh thu nhỏ để hiển thị. Giữ bitmap 12 MP trong PictureBox vừa tốn RAM vừa làm UI
        /// thread phải co giãn ảnh khổng lồ mỗi lần vẽ lại.
        /// </summary>
        private static Bitmap CreateThumbnail(Bitmap source, int maxSize)
        {
            if (maxSize <= 0 || (source.Width <= maxSize && source.Height <= maxSize))
            {
                return (Bitmap)source.Clone();
            }

            double scale = Math.Min((double)maxSize / source.Width, (double)maxSize / source.Height);
            int w = Math.Max(1, (int)(source.Width * scale));
            int h = Math.Max(1, (int)(source.Height * scale));

            var thumb = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(source, 0, 0, w, h);
            }

            return thumb;
        }

        #endregion

        #region Vòng push

        /// <summary>
        /// Push lần lượt toàn bộ ảnh CÒN THIẾU MASK trong danh sách sang UnitX với part_id cố định.
        /// Chạy ở luồng nền, cập nhật giao diện qua Log/SetRowState/UpdateProgress.
        ///
        /// BẤT BIẾN bắt buộc: PushImage -> chờ callback kết quả (hoặc timeout) -> mới push ảnh kế.
        /// Không bao giờ được có 2 vòng push chạy song song trên cùng part group (xem SetBusy /
        /// StartWatching).
        /// </summary>
        private void PushAll(List<PushItem> items, PushConfig config, CancellationToken token)
        {
            var watch = Stopwatch.StartNew();
            int success = 0;   // đã có ảnh mask
            int missing = 0;   // push được nhưng UnitX chưa trả kết quả
            int failed = 0;    // PushImage trả về false hoặc lỗi đọc file
            int consecutiveFailures = 0;

            lock (_timings)
            {
                _timings.Clear();
            }

            // unitx_sdk_sample.cpp: kiểm tra system status trước khi push loạt ảnh ngoài.
            ReadSystemStatus();

            Log($"=== BẮT ĐẦU PUSH {items.Count} ảnh | part_type={config.PartType} | part_id={config.PartId} | "
                + $"capture_config={config.CaptureConfig} | edge={(string.IsNullOrEmpty(config.Edge) ? "(mặc định)" : config.Edge)} ===");
            Log("Mỗi ảnh: PushImage -> chờ callback kết quả -> lưu mask -> ảnh kế tiếp.");

            for (int i = 0; i < items.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Log("Đã dừng theo yêu cầu người dùng.");
                    break;
                }

                var item = items[i];

                try
                {
                    var timing = new CycleTiming
                    {
                        FileName = Path.GetFileName(item.FullPath),
                        SourceBytes = item.Length
                    };

                    long cycleStart = Stopwatch.GetTimestamp();

                    SetRowState(item.FullPath, "Đang push", PushingColor);

                    // 1. Đọc file từ đĩa
                    long t = Stopwatch.GetTimestamp();
                    byte[] raw = File.ReadAllBytes(item.FullPath);
                    timing.ReadMs = ElapsedMs(t);

                    // 2. Giải mã + encode payload
                    var prepared = PreparePayload(raw);
                    timing.DecodeMs = prepared.DecodeMs;
                    timing.EncodeMs = prepared.EncodeMs;
                    timing.PayloadBytes = prepared.Length;
                    string note = prepared.Note;

                    _lastPushToCallbackMs = 0;
                    _lastCallbackTotalMs = 0;
                    _lastDecodeMs = 0;
                    _lastSaveMaskMs = 0;
                    _lastUnitXLatencyMs = 0;
                    _lastBlobCopyMs = 0;
                    _lastReturnBytes = 0;

                    // 3. PushImage - tách riêng thời gian ghim bộ nhớ và lời gọi native
                    double marshalMs, nativeMs;
                    long sdkLogMark = UnitXSdkLog.Mark(SdkLogPath);
                    bool ok = PushBytes(prepared.Buffer, prepared.Length, config, out marshalMs, out nativeMs);
                    timing.MarshalMs = marshalMs;
                    timing.PushImageMs = nativeMs;
                    Interlocked.Exchange(ref _pushDoneTs, Stopwatch.GetTimestamp());

                    // Chỉ enqueue lượt chờ kết quả khi PushImage thật sự thành công - tránh để lại
                    // một entry "mồ côi" vĩnh viễn trong hàng đợi đối soát.
                    PendingPush pending = null;
                    if (ok)
                    {
                        pending = new PendingPush { FullPath = item.FullPath, PushTimestamp = Stopwatch.GetTimestamp() };
                        _pendingPushes.Enqueue(pending);
                    }

                    // 4. Chờ UnitX xử lý và trả kết quả về callback
                    if (ok)
                    {
                        long waitStart = Stopwatch.GetTimestamp();
                        bool received = pending.Signal.Wait(config.TimeoutMs, token);
                        timing.WaitCallbackMs = ElapsedMs(waitStart);

                        if (!received)
                        {
                            Log($"    HẾT {config.TimeoutMs} ms chưa thấy callback kết quả - UnitX có thể chưa xử lý xong. "
                                + "Lượt chờ vẫn nằm trong hàng đợi, nếu kết quả về trễ vẫn sẽ được lưu đúng tên ảnh này "
                                + "thay vì bị gán nhầm sang ảnh sau.");
                        }
                    }

                    // Số liệu do callback ghi lại
                    timing.PushToCallbackMs = _lastPushToCallbackMs;
                    timing.BlobCopyMs = _lastBlobCopyMs;
                    timing.ReturnBytes = _lastReturnBytes;
                    timing.CallbackDecodeMs = _lastDecodeMs;
                    timing.SaveMaskMs = _lastSaveMaskMs;
                    timing.UnitXLatencyMs = _lastUnitXLatencyMs;
                    timing.TotalMs = ElapsedMs(cycleStart);

                    if (ok && HasMask(item.FullPath))
                    {
                        // Chỉ khi đã có ảnh mask mới coi là XONG -> lần quét sau sẽ bỏ qua ảnh này.
                        success++;
                        consecutiveFailures = 0;
                        timing.Status = "OK";
                        if (item.FromCamera)
                        {
                            // Ảnh raw thiếu mask vừa được push lại: nền xám -> màu theo kết quả mới.
                            RefreshCameraRowState(item.FullPath);
                        }
                        else
                        {
                            SetRowState(item.FullPath, "Đã có mask", DoneColor);
                        }
                        AppendHistory(item.Key);
                        Log($"[{i + 1}/{items.Count}] XONG {timing.FileName} [{note}]");
                    }
                    else if (ok)
                    {
                        missing++;
                        consecutiveFailures = 0;
                        timing.Status = "THIEU_KET_QUA";
                        SetRowState(item.FullPath, "Thiếu kết quả", MissingColor);
                        Log($"[{i + 1}/{items.Count}] THIẾU {timing.FileName} - đã push nhưng chưa có ảnh mask");
                    }
                    else
                    {
                        failed++;
                        consecutiveFailures++;
                        timing.Status = "LOI_PUSH";
                        SetRowState(item.FullPath, "Lỗi push", FailedColor);
                        LogError($"[{i + 1}/{items.Count}] FAIL {timing.FileName} [{note}] - PushImage trả về false: "
                            + UnitXSdkLog.Explain(SdkLogPath, sdkLogMark).Reason);

                        if (failed == 1)
                        {
                            Log("Đọc dòng \"Push image response\" trong " + SdkLogPath + " để biết server từ chối vì sao:");
                            Log("  - \"Image type is not supported\"      => ảnh gửi đi sai định dạng.");
                            Log("  - \"'NoneType' object is not iterable\" => part_type hoặc capture_config KHÔNG tồn tại bên UnitX.");
                        }
                    }

                    LogTiming(timing);

                    lock (_timings)
                    {
                        _timings.Add(timing);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log("Đã dừng theo yêu cầu người dùng.");
                    break;
                }
                catch (Exception ex)
                {
                    failed++;
                    consecutiveFailures++;
                    SetRowState(item.FullPath, "Lỗi", FailedColor);
                    Log($"[{i + 1}/{items.Count}] LỖI {Path.GetFileName(item.FullPath)}: {ex.Message}");
                }

                // Nếu 5 ảnh đầu đều hỏng thì nguyên nhân là cấu hình/kết nối chứ không phải từng file.
                if (consecutiveFailures >= 5 && success == 0)
                {
                    Log("Đã 5 ảnh liên tiếp thất bại và chưa ảnh nào thành công -> DỪNG.");
                    RunDiagnostics();
                    break;
                }

                UpdateProgress(i + 1, items.Count, success, missing, failed);

                if (config.DelayMs > 0 && i < items.Count - 1)
                {
                    try
                    {
                        Task.Delay(config.DelayMs, token).Wait(token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (AggregateException)
                    {
                        break;
                    }
                }
            }

            watch.Stop();
            int remaining = missing + failed;
            Log($"=== KẾT THÚC: {success} ảnh đã có mask, {missing} thiếu kết quả, {failed} lỗi, "
                + $"tổng {watch.Elapsed.TotalSeconds:0.0}s ===");

            if (remaining > 0)
            {
                Log($"Còn {remaining} ảnh CHƯA XONG. Bấm 'Quét thư mục' rồi 'Push toàn bộ' để chạy tiếp "
                    + "- chỉ những ảnh chưa có mask mới được push lại.");
            }

            LogTimingSummary();
            UpdateStatus($"Xong: {success} có mask / {remaining} còn thiếu");
        }

        /// <summary>
        /// Ghim mảng bằng GCHandle rồi gọi PushImage. Cách này không sao chép byte nào (khác với
        /// AllocHGlobal + Marshal.Copy vốn tốn thêm một lần cấp phát + copy ~37 MB mỗi ảnh).
        /// marshalMs = thời gian ghim bộ nhớ, nativeMs = thời gian SDK nhận + đẩy blob sang UnitX.
        /// </summary>
        private bool PushBytes(byte[] imageBytes, int length, PushConfig config,
            out double marshalMs, out double nativeMs)
        {
            marshalMs = 0;
            nativeMs = 0;

            if (imageBytes == null || length <= 0)
            {
                return false;
            }

            long t0 = Stopwatch.GetTimestamp();
            var handle = GCHandle.Alloc(imageBytes, GCHandleType.Pinned);

            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                marshalMs = ElapsedMs(t0);

                long t1 = Stopwatch.GetTimestamp();
                bool result = PushImage(
                    ptr,
                    length,
                    config.PartType,
                    config.PartId,
                    config.CaptureConfig,
                    config.Edge);
                nativeMs = ElapsedMs(t1);

                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        private void CancelPush()
        {
            try
            {
                if (_pushCts != null && !_pushCts.IsCancellationRequested)
                {
                    _pushCts.Cancel();
                    Log("Đang dừng push...");
                    UpdateStatus("Đang dừng push...");
                }
            }
            catch (Exception ex)
            {
                Log("Lỗi khi dừng: " + ex.Message);
            }
        }

        #endregion

        #region Đo thời gian

        /// <summary>Thời gian (ms) của từng bước trong một chu trình xử lý ảnh.</summary>
        private sealed class CycleTiming
        {
            public string FileName { get; set; }
            public long SourceBytes { get; set; }
            public long PayloadBytes { get; set; }
            public double ReadMs { get; set; }
            public double DecodeMs { get; set; }
            public double EncodeMs { get; set; }
            public double MarshalMs { get; set; }
            public double PushImageMs { get; set; }
            public double WaitCallbackMs { get; set; }
            public double BlobCopyMs { get; set; }
            public long ReturnBytes { get; set; }
            public double CallbackDecodeMs { get; set; }
            public double SaveMaskMs { get; set; }
            public double UnitXLatencyMs { get; set; }
            public double PushToCallbackMs { get; set; }
            public double TotalMs { get; set; }
            public string Status { get; set; }
        }

        private readonly List<CycleTiming> _timings = new List<CycleTiming>();

        private void LogTiming(CycleTiming t)
        {
            Log($"    THỜI GIAN | đọc {t.ReadMs:0} | giải mã {t.DecodeMs:0} | encode {t.EncodeMs:0} | "
                + $"ghim {t.MarshalMs:0} | ĐẨY SANG UNITX {t.PushImageMs:0} | "
                + $"UNITX XỬ LÝ {t.PushToCallbackMs:0} | NHẬN ẢNH VỀ {t.BlobCopyMs:0} | "
                + $"giải mã kq {t.CallbackDecodeMs:0} | lưu mask {t.SaveMaskMs:0} | TỔNG {t.TotalMs:0} ms");
        }

        /// <summary>
        /// Thống kê min/trung bình/max cho từng bước sau khi push xong cả loạt, kèm xuất CSV.
        /// </summary>
        private void LogTimingSummary()
        {
            List<CycleTiming> data;
            lock (_timings)
            {
                data = _timings.ToList();
            }

            if (data.Count == 0)
            {
                return;
            }

            Log($"===== THỐNG KÊ THỜI GIAN (n={data.Count}) =====");

            var metrics = new List<KeyValuePair<string, Func<CycleTiming, double>>>
            {
                new KeyValuePair<string, Func<CycleTiming, double>>("Đọc file            ", x => x.ReadMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Giải mã ảnh nguồn   ", x => x.DecodeMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Encode payload      ", x => x.EncodeMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Ghim bộ nhớ         ", x => x.MarshalMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("ĐẨY ẢNH SANG UNITX  ", x => x.PushImageMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("UNITX XỬ LÝ         ", x => x.PushToCallbackMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("NHẬN ẢNH TRẢ VỀ     ", x => x.BlobCopyMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Giải mã ảnh kết quả ", x => x.CallbackDecodeMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Lưu ảnh mask        ", x => x.SaveMaskMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("Trễ mốc UnitX       ", x => x.UnitXLatencyMs),
                new KeyValuePair<string, Func<CycleTiming, double>>("TỔNG mỗi ảnh        ", x => x.TotalMs)
            };

            foreach (var metric in metrics)
            {
                var values = data.Select(metric.Value).ToList();
                Log($"{metric.Key}: min {values.Min():0} | avg {values.Average():0} | max {values.Max():0} ms");
            }

            var sent = data.Where(x => x.PushImageMs > 0).ToList();
            if (sent.Count > 0)
            {
                double upMbps = sent.Average(x => x.PayloadBytes / 1048576.0 / (x.PushImageMs / 1000.0));
                Log($"Băng thông ĐẨY ĐI : {upMbps:0.0} MB/s (payload TB {sent.Average(x => x.PayloadBytes) / 1048576.0:0.0} MB)");
            }

            var received = data.Where(x => x.BlobCopyMs > 0 && x.ReturnBytes > 0).ToList();
            if (received.Count > 0)
            {
                double downMbps = received.Average(x => x.ReturnBytes / 1048576.0 / (x.BlobCopyMs / 1000.0));
                Log($"Băng thông NHẬN VỀ: {downMbps:0.0} MB/s (nhận TB {received.Average(x => x.ReturnBytes) / 1048576.0:0.0} MB)");
            }

            double totalSeconds = data.Sum(x => x.TotalMs) / 1000.0;
            double throughput = totalSeconds > 0 ? data.Count / totalSeconds * 60.0 : 0;
            Log($"Năng suất: {throughput:0.0} ảnh/phút (trung bình {data.Average(x => x.TotalMs) / 1000.0:0.00} s/ảnh)");

            var heaviest = metrics
                .Take(metrics.Count - 1)
                .OrderByDescending(m => data.Average(m.Value))
                .First();
            Log($"Bước tốn thời gian nhất: {heaviest.Key.Trim()} "
                + $"({data.Average(heaviest.Value):0} ms/ảnh, chiếm {data.Average(heaviest.Value) / data.Average(x => x.TotalMs) * 100:0}%)");

            ExportTimingCsv(data);
            Log("==========================================");
        }

        private void ExportTimingCsv(List<CycleTiming> data)
        {
            try
            {
                Directory.CreateDirectory(AppDataDir);
                string path = Path.Combine(AppDataDir, $"timing_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                var sb = new StringBuilder();
                sb.AppendLine("file,source_bytes,payload_bytes,return_bytes,read_ms,decode_ms,encode_ms,"
                    + "marshal_ms,push_image_ms,wait_callback_ms,push_to_callback_ms,"
                    + "blob_copy_ms,callback_decode_ms,save_mask_ms,unitx_latency_ms,total_ms,status");

                var ci = System.Globalization.CultureInfo.InvariantCulture;

                foreach (var t in data)
                {
                    sb.AppendLine(string.Join(",",
                        "\"" + t.FileName + "\"",
                        t.SourceBytes,
                        t.PayloadBytes,
                        t.ReturnBytes,
                        t.ReadMs.ToString("0.0", ci),
                        t.DecodeMs.ToString("0.0", ci),
                        t.EncodeMs.ToString("0.0", ci),
                        t.MarshalMs.ToString("0.0", ci),
                        t.PushImageMs.ToString("0.0", ci),
                        t.WaitCallbackMs.ToString("0.0", ci),
                        t.PushToCallbackMs.ToString("0.0", ci),
                        t.BlobCopyMs.ToString("0.0", ci),
                        t.CallbackDecodeMs.ToString("0.0", ci),
                        t.SaveMaskMs.ToString("0.0", ci),
                        t.UnitXLatencyMs.ToString("0.0", ci),
                        t.TotalMs.ToString("0.0", ci),
                        t.Status));
                }

                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                Log("Đã xuất số liệu thời gian: " + path);
            }
            catch (Exception ex)
            {
                Log("Xuất CSV thời gian thất bại: " + ex.Message);
            }
        }

        #endregion

        #region Theo dõi thư mục (tự động push ảnh mới)

        /// <summary>
        /// Bật/tắt theo dõi qua checkbox. TẮT lúc nào cũng dừng ngay được; BẬT thì chỉ thật sự khởi
        /// động FileSystemWatcher khi đã kết nối UnitX - nếu chưa, ghi log chờ và ConnectAsync() sẽ
        /// gọi lại StartWatching() ngay sau khi kết nối xong.
        /// </summary>
        private void ChkWatchFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWatchFolder.Checked)
            {
                if (_sdkRunning)
                {
                    StartWatching();
                }
                else
                {
                    Log("Đã bật theo dõi thư mục - sẽ tự động push ảnh mới ngay khi kết nối UnitX.");
                    UpdateStatus("Theo dõi thư mục: đang chờ kết nối UnitX...");
                }
            }
            else
            {
                StopWatching();
                UpdateStatus("Đã tắt theo dõi thư mục.");
            }
        }

        /// <summary>
        /// Khởi động FileSystemWatcher trên thư mục ảnh hiện tại.
        ///
        /// BẮT BUỘC: không được để vòng theo dõi chạy chồng lên một lượt "Push toàn bộ" đang dở -
        /// cả hai dùng chung part group và chung hàng đợi _pendingPushes, chạy song song sẽ khiến
        /// push/chờ callback của hai ảnh cắt ngang nhau. HOÃN lại (không bỏ tick) - ContinueWith của
        /// btnPushAll_Click sẽ tự gọi lại StartWatching() ngay khi push xong.
        /// </summary>
        private void StartWatching()
        {
            if (_folderWatcher != null)
            {
                return; // đã bật rồi
            }

            if (_pushTask != null && !_pushTask.IsCompleted)
            {
                Log("Chưa bật theo dõi thư mục: đang có lượt push thủ công chạy - "
                    + "sẽ tự bật theo dõi ngay khi lượt push đó kết thúc.");
                UpdateStatus("Theo dõi thư mục: đang chờ push thủ công hiện tại kết thúc...");
                return;
            }

            string folder = (txtPath.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                Log("Không bật được theo dõi thư mục: thư mục ảnh không tồn tại.");
                SetWatchToggle(false);
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                Log("Không bật được theo dõi thư mục: thiếu Part Type / Part ID / Capture Config.");
                SetWatchToggle(false);
                return;
            }

            try
            {
                lock (_watchQueueLock)
                {
                    _watchQueued.Clear();
                }
                _watchQueue = new BlockingCollection<string>();

                _folderWatcher = new FileSystemWatcher(folder)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };
                _folderWatcher.Created += OnWatchedFileChanged;
                _folderWatcher.Renamed += OnWatchedFileRenamed;
                _folderWatcher.Error += OnWatchedError;
                _folderWatcher.EnableRaisingEvents = true;

                _watchCts = new CancellationTokenSource();
                var token = _watchCts.Token;
                _watchConsumerTask = Task.Run(() => WatchConsumerLoop(token), token);

                SetWatching(true);
                Log($"Đã bật theo dõi thư mục: {folder} (ảnh mới sẽ tự động push sang UnitX).");
                UpdateStatus("Đang theo dõi thư mục - tự động push ảnh mới.");
            }
            catch (Exception ex)
            {
                Log("Không bật được theo dõi thư mục: " + ex.Message);
                StopWatching();
                SetWatchToggle(false);
            }
        }

        /// <summary>
        /// Tắt FileSystemWatcher + dừng vòng lặp xử lý hàng đợi. An toàn để gọi nhiều lần.
        /// KHÔNG bỏ tick chkWatchFolder - xem ChkWatchFolder_CheckedChanged.
        /// </summary>
        private void StopWatching()
        {
            if (_folderWatcher != null)
            {
                try
                {
                    _folderWatcher.EnableRaisingEvents = false;
                    _folderWatcher.Created -= OnWatchedFileChanged;
                    _folderWatcher.Renamed -= OnWatchedFileRenamed;
                    _folderWatcher.Error -= OnWatchedError;
                    _folderWatcher.Dispose();
                }
                catch (Exception ex)
                {
                    Log("Lỗi khi tắt theo dõi thư mục: " + ex.Message);
                }
                finally
                {
                    _folderWatcher = null;
                }
            }

            if (_watchCts != null)
            {
                try { _watchCts.Cancel(); }
                catch { }
            }

            var consumerTask = _watchConsumerTask;
            if (consumerTask != null)
            {
                try { consumerTask.Wait(2000); }
                catch { }
            }

            if (_watchCts != null)
            {
                _watchCts.Dispose();
                _watchCts = null;
            }
            _watchConsumerTask = null;

            lock (_watchQueueLock)
            {
                _watchQueued.Clear();
            }

            SetWatching(false);
        }

        private void OnWatchedFileChanged(object sender, FileSystemEventArgs e)
        {
            EnqueueWatchedFile(e.FullPath);
        }

        private void OnWatchedFileRenamed(object sender, RenamedEventArgs e)
        {
            // Một số camera ghi ra tên tạm rồi rename thành tên cuối - coi như ảnh mới xuất hiện.
            EnqueueWatchedFile(e.FullPath);
        }

        private void OnWatchedError(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            Log("Lỗi FileSystemWatcher (có thể do bộ đệm sự kiện bị tràn): " + (ex != null ? ex.Message : "không rõ"));
        }

        /// <summary>
        /// Lọc + khử trùng lặp rồi đưa file vào hàng đợi xử lý của WatchConsumerLoop.
        /// Chạy trên luồng của FileSystemWatcher (ThreadPool), không đụng giao diện trực tiếp.
        /// </summary>
        private void EnqueueWatchedFile(string fullPath)
        {
            try
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    return;
                }

                if (!ImageExtensions.Contains(Path.GetExtension(fullPath), StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                // Bỏ qua ảnh do chính ứng dụng lưu ra (PATH\raw, PATH\mask) - tránh push ngược ảnh
                // kết quả lên UnitX thành một vòng lặp vô tận.
                if (ClassifyScannedFile(_imageRootFolder, fullPath) != ScannedKind.Source)
                {
                    return;
                }

                // Người dùng vừa đổi tên file này ngay trong phần mềm - không phải ảnh mới.
                if (IsManualRename(fullPath))
                {
                    return;
                }

                lock (_watchQueueLock)
                {
                    if (!_watchQueued.Add(fullPath))
                    {
                        return; // đã có trong hàng đợi, chờ xử lý
                    }
                }

                var queue = _watchQueue;
                if (queue != null && !queue.IsAddingCompleted)
                {
                    queue.Add(fullPath);
                }
            }
            catch (Exception ex)
            {
                Log("Lỗi khi xếp hàng ảnh mới: " + ex.Message);
            }
        }

        /// <summary>
        /// Vòng lặp nền: lấy từng file mới ra khỏi hàng đợi và push lần lượt (KHÔNG song song -
        /// dùng chung _pendingPushes với PushAll nên chỉ được push từng ảnh một).
        /// </summary>
        private void WatchConsumerLoop(CancellationToken token)
        {
            var queue = _watchQueue;

            while (!token.IsCancellationRequested)
            {
                string filePath;
                try
                {
                    filePath = queue.Take(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                try
                {
                    ProcessWatchedFile(filePath, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Lỗi xử lý ảnh mới {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            Log("Đã dừng vòng lặp theo dõi thư mục.");
        }

        private void ProcessWatchedFile(string filePath, CancellationToken token)
        {
            lock (_watchQueueLock)
            {
                _watchQueued.Remove(filePath);
            }

            if (!File.Exists(filePath))
            {
                return; // đã bị xoá/đổi tên trước khi kịp xử lý
            }

            FileInfo info = WaitUntilFileReady(filePath, token);
            if (info == null)
            {
                if (!token.IsCancellationRequested)
                {
                    Log($"[Theo dõi] Bỏ qua {Path.GetFileName(filePath)}: file không sẵn sàng "
                        + "(đang ghi dở hoặc bị khoá quá lâu).");
                }
                return;
            }

            var item = new PushItem
            {
                FullPath = filePath,
                Length = info.Length,
                LastWriteUtc = info.LastWriteTimeUtc,
                LastWrite = info.LastWriteTime
            };

            // "Chỉ push ảnh còn thiếu" là mặc định cứng của V13.
            if (HasMask(filePath))
            {
                Log($"[Theo dõi] Bỏ qua {Path.GetFileName(filePath)}: đã có ảnh mask.");
                return;
            }

            if (!_sdkRunning)
            {
                Log($"[Theo dõi] Bỏ qua {Path.GetFileName(filePath)}: chưa kết nối UnitX.");
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                Log($"[Theo dõi] Bỏ qua {Path.GetFileName(filePath)}: thiếu Part Type / Part ID / Capture Config.");
                return;
            }

            AddRawItem(item);
            PushWatchedItem(item, config, token);
        }

        /// <summary>
        /// Chờ file ghi xong (camera/ứng dụng khác có thể vẫn đang ghi dở khi Created bắn ra).
        /// Thử mở exclusive-read theo chu kỳ tới khi thành công hoặc hết lượt thử.
        /// </summary>
        private FileInfo WaitUntilFileReady(string filePath, CancellationToken token,
            int maxAttempts = 20, int delayMs = 250)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                try
                {
                    var info = new FileInfo(filePath);
                    if (!info.Exists)
                    {
                        return null;
                    }

                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (stream.Length > 0)
                        {
                            return info;
                        }
                    }
                }
                catch (IOException)
                {
                    // File vẫn đang được ghi/khoá bởi tiến trình khác - thử lại sau.
                }
                catch (Exception ex)
                {
                    Log($"Lỗi kiểm tra file {Path.GetFileName(filePath)}: {ex.Message}");
                    return null;
                }

                try
                {
                    Task.Delay(delayMs, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (AggregateException)
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Push đúng 1 ảnh mới phát hiện được sang UnitX - CÙNG cơ chế với chu trình mỗi ảnh trong
        /// PushAll, chỉ khác là chạy cho một file thay vì lặp qua danh sách đã quét.
        /// </summary>
        private void PushWatchedItem(PushItem item, PushConfig config, CancellationToken token)
        {
            string fileName = Path.GetFileName(item.FullPath);

            try
            {
                long cycleStart = Stopwatch.GetTimestamp();

                SetRowState(item.FullPath, "Đang push", PushingColor);

                byte[] raw = File.ReadAllBytes(item.FullPath);
                var prepared = PreparePayload(raw);
                string note = prepared.Note;

                _lastPushToCallbackMs = 0;
                _lastCallbackTotalMs = 0;
                _lastDecodeMs = 0;
                _lastSaveMaskMs = 0;
                _lastUnitXLatencyMs = 0;
                _lastBlobCopyMs = 0;
                _lastReturnBytes = 0;

                double marshalMs, nativeMs;
                bool ok = PushBytes(prepared.Buffer, prepared.Length, config, out marshalMs, out nativeMs);
                Interlocked.Exchange(ref _pushDoneTs, Stopwatch.GetTimestamp());

                PendingPush pending = null;
                if (ok)
                {
                    pending = new PendingPush { FullPath = item.FullPath, PushTimestamp = Stopwatch.GetTimestamp() };
                    _pendingPushes.Enqueue(pending);
                }

                if (ok)
                {
                    bool received = pending.Signal.Wait(config.TimeoutMs, token);
                    if (!received)
                    {
                        Log($"[Theo dõi] HẾT {config.TimeoutMs} ms chưa thấy callback kết quả cho {fileName}. "
                            + "Lượt chờ vẫn nằm trong hàng đợi, kết quả về trễ vẫn sẽ được lưu đúng tên ảnh này.");
                    }
                }

                if (ok && HasMask(item.FullPath))
                {
                    AppendHistory(item.Key);
                    SetRowState(item.FullPath, "Đã có mask", DoneColor);
                    Log($"[Theo dõi] XONG {fileName} [{note}] sau {ElapsedMs(cycleStart):0} ms.");
                    UpdateStatus($"Theo dõi thư mục: vừa push {fileName}");
                }
                else if (ok)
                {
                    SetRowState(item.FullPath, "Thiếu kết quả", MissingColor);
                    Log($"[Theo dõi] THIẾU {fileName} - đã push nhưng chưa thấy ảnh mask.");
                }
                else
                {
                    SetRowState(item.FullPath, "Lỗi push", FailedColor);
                    Log($"[Theo dõi] LỖI push {fileName} [{note}] - PushImage trả về false.");
                }
            }
            catch (OperationCanceledException)
            {
                // Đang tắt theo dõi hoặc đóng ứng dụng - bỏ qua yên lặng.
            }
            catch (Exception ex)
            {
                Log($"[Theo dõi] LỖI {fileName}: {ex.Message}");
            }
        }

        #endregion

        #region Lịch sử đã push

        private static string BuildKey(string fullPath, DateTime lastWriteUtc, long length)
        {
            return string.Format("{0}|{1}|{2}", fullPath.ToLowerInvariant(), lastWriteUtc.Ticks, length);
        }

        private void LoadHistory()
        {
            try
            {
                if (!File.Exists(HistoryPath))
                {
                    return;
                }

                foreach (var line in File.ReadAllLines(HistoryPath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _pushedKeys.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Không đọc được lịch sử push: " + ex.Message);
            }
        }

        private void AppendHistory(string key)
        {
            lock (_historyLock)
            {
                if (!_pushedKeys.Add(key))
                {
                    return;
                }

                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    File.AppendAllText(HistoryPath, key + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Log("Không ghi được lịch sử push: " + ex.Message);
                }
            }
        }

        #endregion
    }
}
