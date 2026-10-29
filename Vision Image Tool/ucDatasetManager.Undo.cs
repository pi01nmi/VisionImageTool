using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerUndoCodeFile { }

    /// <summary>Nhật ký một lần "Chuyển sang quản lý theo tên" (dataset_rename_journal.json).</summary>
    internal sealed class RenameJournal
    {
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
        public string Source { get; set; }
        public string SourceSchema { get; set; }
        public string Destination { get; set; }
        public string TargetSchema { get; set; }
        public List<RenameJournalFile> Files { get; set; } = new List<RenameJournalFile>();
    }

    internal sealed class RenameJournalFile
    {
        /// <summary>Đường dẫn tương đối của ảnh gốc trong thư mục nguồn.</summary>
        public string Original { get; set; }

        /// <summary>Đường dẫn tương đối của bản copy (tên mới) trong thư mục đích.</summary>
        public string Renamed { get; set; }

        public long Size { get; set; }
        public DateTime Modified { get; set; }
    }

    /// <summary>
    /// Hoàn tác "Chuyển sang quản lý theo tên". Việc chuyển COPY ảnh sang thư mục đích (gốc giữ nguyên), nên hoàn
    /// tác = xoá (vào Thùng rác) đúng các bản copy đã tạo, gỡ ghi chú đi kèm, xoá thư mục đích nếu trống, rồi mở
    /// lại thư mục nguồn với schema nguồn.
    ///   - Nhật ký ghi ở CẢ thư mục nguồn lẫn đích -> mở thư mục nào cũng thấy nút Hoàn tác, trạng thái đúng sau
    ///     khi tắt / mở lại app.
    ///   - Còn nhật ký = đã đổi tên: KHOÁ "Chuyển sang quản lý theo tên" (không chuyển lần 2, tránh mất dấu tên
    ///     gốc). Chưa có nhật ký: KHOÁ "Hoàn tác". Chỉ một cấp hoàn tác.
    ///   - File trong thư mục đích bị sửa sau khi chuyển (khác kích thước / thời gian) hoặc file mới thêm vào:
    ///     KHÔNG xoá tự động, liệt kê để người dùng tự xử lý.
    /// </summary>
    public partial class ucDatasetManager
    {
        internal const string JournalFileName = "dataset_rename_journal.json";

        private readonly ToolTip _convertTips = new ToolTip { AutoPopDelay = 15000 };

        #region Nhật ký

        private static RenameJournal ReadJournal(string folder)
        {
            try
            {
                string path = folder == null ? null : Path.Combine(folder, JournalFileName);
                if (path != null && File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<RenameJournal>(File.ReadAllText(path));
                }
            }
            catch
            {
                // Nhật ký hỏng: coi như không có (không khoá người dùng vì một file hỏng).
            }

            return null;
        }

        /// <summary>Nhật ký còn hiệu lực của thư mục đang mở (nguồn hoặc đích), null nếu chưa chuyển lần nào.</summary>
        private RenameJournal ActiveJournal()
        {
            return ReadJournal(_loadedFolder ?? (txtFolder.Text ?? string.Empty).Trim().Trim('"'));
        }

        private void WriteJournal(string source, string sourceSchema, string targetSchema, string destination, List<ConvertItem> copied)
        {
            var journal = new RenameJournal
            {
                CreatedAt = DateTime.Now,
                Source = source,
                SourceSchema = sourceSchema,
                Destination = destination,
                TargetSchema = targetSchema
            };

            foreach (var item in copied)
            {
                var info = new FileInfo(Path.Combine(destination, item.Target));
                journal.Files.Add(new RenameJournalFile
                {
                    Original = item.SourceRelative,
                    Renamed = item.Target,
                    Size = info.Exists ? info.Length : -1,
                    Modified = info.Exists ? info.LastWriteTimeUtc : DateTime.MinValue
                });
            }

            string json = JsonConvert.SerializeObject(journal, Formatting.Indented);
            var failed = new List<string>();
            foreach (var folder in new[] { source, destination })
            {
                try
                {
                    File.WriteAllText(Path.Combine(folder, JournalFileName), json, new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    failed.Add(folder + ": " + ex.Message);
                }
            }

            if (failed.Count > 0)
            {
                MessageBox.Show(this, "Không ghi được nhật ký đổi tên - sẽ không Hoàn tác được tự động:" + Environment.NewLine
                    + string.Join(Environment.NewLine, failed), "Chuyển sang quản lý theo tên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ConvertLockedReason(RenameJournal journal)
        {
            return "Thư mục này đã được chuyển sang quản lý theo tên lúc " + journal.CreatedAt.ToString("HH:mm dd/MM/yyyy")
                + Environment.NewLine + "→ " + journal.Destination + "  (" + journal.Files.Count + " ảnh)."
                + Environment.NewLine + Environment.NewLine + "Chỉ chuyển một lần. Muốn chuyển lại: bấm \"↶ Hoàn tác đổi tên\" trước.";
        }

        #endregion

        #region Trạng thái nút

        /// <summary>
        /// Máy trạng thái hai nút:
        ///   chưa chuyển  : Chuyển BẬT (khi schema có cấp thư mục) · Hoàn tác KHOÁ
        ///   đã chuyển    : Chuyển KHOÁ · Hoàn tác BẬT   (mở thư mục nguồn hay đích đều vậy)
        /// Schema chỉ theo tên và chưa từng chuyển: ẩn cả hai.
        /// </summary>
        private void UpdateConvertButtons()
        {
            var journal = ActiveJournal();
            flpConvert.Visible = _folderMode || journal != null;
            btnConvert.Visible = _folderMode || journal != null;

            btnConvert.Enabled = journal == null && _folderMode;
            btnUndoConvert.Enabled = journal != null;

            _convertTips.SetToolTip(btnConvert, journal != null
                ? "Đã chuyển lúc " + journal.CreatedAt.ToString("HH:mm dd/MM") + " → " + journal.Destination + "." + Environment.NewLine
                  + "Chỉ chuyển một lần - Hoàn tác trước nếu muốn chuyển lại."
                : "Copy dữ liệu sang thư mục mới, đặt tên file theo schema tên file (xếp vào raw\\ và mask\\)." + Environment.NewLine
                  + "Dữ liệu gốc giữ nguyên. Nhãn Đánh giá, Đối soát, ghi chú được mang theo. Hoàn tác được.");
            _convertTips.SetToolTip(btnUndoConvert, journal != null
                ? "Xoá (vào Thùng rác) " + journal.Files.Count + " ảnh đã copy sang " + journal.Destination + Environment.NewLine
                  + "rồi mở lại thư mục gốc với schema gốc. Ảnh gốc không bị đụng tới."
                : "Chưa có lần đổi tên nào để hoàn tác.");

            lblConvertState.Text = journal != null
                ? "Đã chuyển lúc " + journal.CreatedAt.ToString("HH:mm dd/MM") + " · bấm Hoàn tác để chuyển lại"
                : _folderMode ? "Hoàn tác: khoá - chưa có lần đổi tên nào" : string.Empty;

            foreach (var button in new[] { btnConvert, btnUndoConvert })
            {
                button.BackColor = button.Enabled ? Color.White : Color.FromArgb(241, 243, 246);
            }
        }

        #endregion

        #region Hoàn tác

        private void UndoConvert()
        {
            var journal = ActiveJournal();
            if (journal == null)
            {
                UpdateConvertButtons();
                return;
            }

            string destination = journal.Destination;

            // Phân loại bản copy: còn nguyên (xoá được) / đã bị sửa (giữ) / đã mất.
            var intact = new List<string>();
            var modified = new List<string>();
            int missing = 0;
            foreach (var file in journal.Files)
            {
                var info = new FileInfo(Path.Combine(destination, file.Renamed));
                if (!info.Exists)
                {
                    missing++;
                }
                else if (info.Length == file.Size && Math.Abs((info.LastWriteTimeUtc - file.Modified).TotalSeconds) < 2)
                {
                    intact.Add(info.FullName);
                }
                else
                {
                    modified.Add(file.Renamed);
                }
            }

            // File ảnh mới thêm vào thư mục đích sau khi chuyển: không thuộc nhật ký -> giữ nguyên.
            var known = new HashSet<string>(journal.Files.Select(f => Path.GetFullPath(Path.Combine(destination, f.Renamed))), StringComparer.OrdinalIgnoreCase);
            var extra = Directory.Exists(destination)
                ? Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
                    .Where(p => IsImageFile(p) && !known.Contains(Path.GetFullPath(p))).ToList()
                : new List<string>();

            string message = "Hoàn tác lần chuyển lúc " + journal.CreatedAt.ToString("HH:mm dd/MM/yyyy") + ":"
                + Environment.NewLine + "  • Xoá (vào Thùng rác) " + intact.Count + " ảnh đã copy trong" + Environment.NewLine + "    " + destination
                + Environment.NewLine + "  • Mở lại thư mục gốc " + journal.Source + Environment.NewLine + "    với schema " + journal.SourceSchema
                + (missing > 0 ? Environment.NewLine + "  • " + missing + " ảnh copy đã không còn (bỏ qua)" : string.Empty)
                + (modified.Count > 0 ? Environment.NewLine + "  • GIỮ LẠI " + modified.Count + " ảnh đã bị sửa sau khi chuyển: " + string.Join(", ", modified.Take(5)) + (modified.Count > 5 ? "…" : string.Empty) : string.Empty)
                + (extra.Count > 0 ? Environment.NewLine + "  • GIỮ LẠI " + extra.Count + " ảnh mới thêm vào thư mục đích (không thuộc lần chuyển)" : string.Empty)
                + Environment.NewLine + Environment.NewLine + "Ảnh gốc không bị đụng tới. Nhãn / ghi chú đã chỉnh trong thư mục đích sẽ mất theo các bản copy."
                + Environment.NewLine + "Tiếp tục?";

            if (MessageBox.Show(this, message, "↶ Hoàn tác đổi tên", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            btnConvert.Enabled = btnUndoConvert.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                // Đang mở thư mục đích: chuyển về thư mục nguồn TRƯỚC (ghi nốt dataset.json / collection đang chờ của
                // thư mục đích), để không có gì ghi ngược vào thư mục đích sau khi đã xoá.
                bool openingDestination = _loadedFolder != null
                    && string.Equals(Path.GetFullPath(_loadedFolder).TrimEnd('\\'), Path.GetFullPath(destination).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
                if (openingDestination)
                {
                    FlushDatasetJson();
                    _autoRefresh.Stop();
                    _loadedFolder = null;
                }

                List<string> failed;
                ImageFileOps.DeleteToRecycleBin(this, intact, out failed);
                ImageComments.OnDeleted(intact.Except(failed, StringComparer.OrdinalIgnoreCase).ToList());

                // Còn ảnh nào trong thư mục đích không? Không còn -> dọn luôn các file của app + thư mục trống.
                bool anyImageLeft = Directory.Exists(destination)
                    && Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories).Any(IsImageFile);
                if (!anyImageLeft && Directory.Exists(destination))
                {
                    var appFiles = new[] { JournalFileName, DatasetJsonFileName, EvaluationFileName }
                        .Select(n => Path.Combine(destination, n)).Where(File.Exists).ToList();
                    List<string> appFailed;
                    ImageFileOps.DeleteToRecycleBin(this, appFiles, out appFailed);
                    RemoveEmptyDirectories(destination);
                }
                else
                {
                    TryDelete(Path.Combine(destination, JournalFileName));
                }

                TryDelete(Path.Combine(journal.Source, JournalFileName));

                // Mở lại thư mục nguồn với schema nguồn.
                txtFolder.Text = journal.Source;
                if (!string.IsNullOrWhiteSpace(journal.SourceSchema))
                {
                    txtSchema.Text = journal.SourceSchema;
                    ApplySchema(false);
                }

                SaveSettings();
                _loadedFolder = null;
                LoadFolder();

                ShowStatusMessage("↶ Đã hoàn tác: xoá " + (intact.Count - failed.Count) + " ảnh copy"
                    + (failed.Count > 0 ? " (" + failed.Count + " ảnh không xoá được - đang mở?)" : string.Empty)
                    + (modified.Count + extra.Count > 0 ? " · giữ lại " + (modified.Count + extra.Count) + " ảnh trong thư mục đích" : string.Empty) + ".");
                StatusResetSoon();
            }
            finally
            {
                Cursor = Cursors.Default;
                UpdateConvertButtons();
            }
        }

        private static bool IsImageFile(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".jpg": case ".jpeg": case ".png": case ".bmp": case ".tif": case ".tiff":
                    return true;
                default:
                    return false;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Không xoá được nhật ký: lần sau nút vẫn ở trạng thái "đã chuyển" - người dùng xoá tay được.
            }
        }

        /// <summary>Xoá các thư mục con trống (raw\, mask\...) rồi chính thư mục đích nếu trống.</summary>
        private static void RemoveEmptyDirectories(string root)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
                {
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                    }
                }

                if (!Directory.EnumerateFileSystemEntries(root).Any())
                {
                    Directory.Delete(root);
                }
            }
            catch
            {
                // Thư mục đang mở trong Explorer... - để lại thư mục trống, không ảnh hưởng gì.
            }
        }

        #endregion
    }
}
