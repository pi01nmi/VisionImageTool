using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerWatchCodeFile { }

    /// <summary>
    /// Tự cập nhật bảng khi thư mục dataset có ảnh mới / bị xoá / đổi tên từ bên ngoài (luôn bật, không
    /// cần bấm "Quét thư mục" lại). Ảnh mới được bóc tên theo schema đang dùng ngay -> vào đúng hàng Model /
    /// DK / vị trí. Nhãn đánh giá, ghi chú, bộ lọc, ô đang chọn giữ nguyên.
    /// </summary>
    public partial class ucDatasetManager
    {
        private FolderAutoRefresh _autoRefresh;
        private bool _quietReloading;

        private void SetupAutoRefresh()
        {
            _autoRefresh = new FolderAutoRefresh(this, QuietReload);
            Disposed += (s, e) => _autoRefresh.Dispose();
        }

        private DatasetImage CreateImage(string folder, string file)
        {
            var image = new DatasetImage
            {
                FullPath = file,
                FileName = Path.GetFileName(file),
                RelativeDir = RelativeDir(folder, file)
            };
            image.EvalKey = image.RelativeDir == "."
                ? image.FileName
                : Path.Combine(image.RelativeDir, image.FileName);
            ParseImage(image);
            return image;
        }

        private static List<string> EnumerateImages(string folder, bool recursive)
        {
            return Directory
                .EnumerateFiles(folder, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Quét lại thư mục đang mở trên luồng nền; danh sách file không đổi thì thôi, đổi thì chỉ thêm /
        /// bớt ảnh khác biệt rồi dựng lại bảng tại chỗ.
        /// </summary>
        private async void QuietReload()
        {
            string folder = _loadedFolder;
            if (_loading || _quietReloading || folder == null)
            {
                return;
            }

            // Đang sửa ô Evaluation: đợi người dùng sửa xong rồi mới dựng lại bảng.
            if (grid.IsCurrentCellInEditMode)
            {
                _autoRefresh.Kick();
                return;
            }

            bool recursive = _autoRefresh.Recursive;
            _quietReloading = true;
            try
            {
                var files = await Task.Run(() => EnumerateImages(folder, recursive));
                if (_loading || !string.Equals(folder, _loadedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var known = new HashSet<string>(_images.Select(i => i.FullPath), StringComparer.OrdinalIgnoreCase);
                var current = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
                var added = files.Where(f => !known.Contains(f)).ToList();
                int removed = known.Count(p => !current.Contains(p));
                if (added.Count == 0 && removed == 0)
                {
                    return;
                }

                var select = CurrentImage();
                _images.RemoveAll(i => !current.Contains(i.FullPath));
                var addedImages = added.Select(f => CreateImage(folder, f)).ToList();
                _images.AddRange(addedImages);
                _images.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullPath, b.FullPath));

                RefillKeepingPosition(select != null && current.Contains(select.FullPath) ? select : null);

                string text = "Thư mục vừa thay đổi - đã tự cập nhật"
                    + (added.Count > 0 ? $": +{added.Count} ảnh mới" : ":")
                    + (removed > 0 ? $" −{removed} ảnh" : string.Empty)
                    + (addedImages.Any(i => !i.Matched) ? " (" + addedImages.Count(i => !i.Matched) + " sai schema)" : string.Empty);
                ShowStatusMessage(text);
                StatusResetSoon();
            }
            catch (Exception ex)
            {
                ShowStatusMessage("Tự cập nhật thư mục lỗi: " + ex.Message, true);
                StatusResetSoon();
            }
            finally
            {
                _quietReloading = false;
            }
        }
    }
}
