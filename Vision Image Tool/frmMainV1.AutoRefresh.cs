using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code (xem ghi chú đầu frmMainV1.View.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1AutoRefreshCodeFile { }

    /// <summary>
    /// Tab Capture &amp; Monitor tự cập nhật danh sách Raw / Mask khi thư mục ảnh thay đổi (luôn bật, không
    /// cần "Subscribe folder"). Chỉ để HIỂN THỊ: không push gì. Push ảnh mới sang UnitX vẫn do checkbox
    /// "Tự push UnitX" (tên cũ "Subscribe folder") đảm nhận.
    ///
    /// Không quét lại từ đầu: chỉ thêm / bớt các file khác biệt so với lần quét trước, giữ nguyên trạng
    /// thái từng dòng (đang push, lỗi...), trang đang xem và ảnh đang chọn. Ảnh do CHÍNH ứng dụng vừa lưu
    /// (PATH\raw, PATH\mask - callback UnitX) thì để callback tự đưa vào danh sách (kèm kết quả OK/NG),
    /// ở đây chỉ nhận khi file đã cũ hơn vài giây mà danh sách vẫn chưa có.
    /// </summary>
    public partial class frmMainV1
    {
        /// <summary>Ảnh kết quả mới hơn chừng này (giây) coi như callback đang xử lý - để lượt sau.</summary>
        private const int FreshOutputSeconds = 3;

        private FolderAutoRefresh _imageFolderRefresh;
        private HashSet<string> _lastScanFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _folderRefreshing;

        private void SetupAutoRefresh()
        {
            _imageFolderRefresh = new FolderAutoRefresh(this, RefreshImageFolderQuiet);
            FormClosed += (s, e) => _imageFolderRefresh.Dispose();

            chkWatchFolder.Text = "Tự push UnitX";
            new ToolTip { AutoPopDelay = 15000 }.SetToolTip(chkWatchFolder,
                "Bật: ảnh MỚI xuất hiện trong thư mục được tự động push sang UnitX (cần đã kết nối)."
                + Environment.NewLine + "Danh sách ảnh luôn tự cập nhật khi thư mục thay đổi - không cần bật ô này.");
        }

        /// <summary>ScanFolder quét xong: ghi nhớ danh sách file + theo dõi thư mục (đổi thư mục thì theo dõi thư mục mới).</summary>
        private void OnFolderScanned(string folder, IEnumerable<string> files)
        {
            _lastScanFiles = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
            _imageFolderRefresh.Watch(folder, true);
        }

        private bool IsFreshOutput(string file)
        {
            try
            {
                return _sdkRunning && (DateTime.UtcNow - File.GetLastWriteTimeUtc(file)).TotalSeconds < FreshOutputSeconds;
            }
            catch
            {
                return false;
            }
        }

        private async void RefreshImageFolderQuiet()
        {
            string folder = _imageRootFolder;
            if (_folderRefreshing || string.IsNullOrEmpty(folder))
            {
                return;
            }

            // Đang push / trigger: danh sách đang được cập nhật từng dòng - để sau.
            if (IsImageListBusy())
            {
                _imageFolderRefresh.Kick();
                return;
            }

            _folderRefreshing = true;
            try
            {
                var files = await Task.Run(() => Directory
                    .EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList());

                if (!string.Equals(folder, _imageRootFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var current = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
                if (current.SetEquals(_lastScanFiles))
                {
                    return;
                }

                if (IsImageListBusy())
                {
                    _imageFolderRefresh.Kick();
                    return;
                }

                ApplyFolderChanges(folder, files, current);
            }
            catch (Exception ex)
            {
                Log("Tự cập nhật thư mục lỗi: " + ex.Message);
            }
            finally
            {
                _folderRefreshing = false;
            }
        }

        private void ApplyFolderChanges(string folder, List<string> files, HashSet<string> current)
        {
            var added = files.Where(f => !_lastScanFiles.Contains(f)).ToList();
            var removed = new HashSet<string>(_lastScanFiles.Where(f => !current.Contains(f)), StringComparer.OrdinalIgnoreCase);
            var deferred = new List<string>();
            int addedRaw = 0, addedMask = 0, removedCount = 0;

            // ---- File đã mất (xoá / đổi tên / chuyển đi) ----
            if (removed.Count > 0)
            {
                removedCount += _rawItems.RemoveAll(i => removed.Contains(i.FullPath));
                removedCount += _maskItems.RemoveAll(i => removed.Contains(i.FullPath));

                foreach (var item in _rawItems.Where(i => i.MaskPath != null && removed.Contains(i.MaskPath)))
                {
                    item.MaskPath = null;
                    if (item.FromCamera)
                    {
                        ApplyCameraResultState(item, null);
                    }
                    else
                    {
                        item.State = "Chờ push";
                        item.StateColor = Color.White;
                    }
                }

                if (_selectedRawPath != null && removed.Contains(_selectedRawPath))
                {
                    _selectedRawPath = null;
                }

                if (_selectedMaskPath != null && removed.Contains(_selectedMaskPath))
                {
                    _selectedMaskPath = null;
                }

                RebuildRawIndexes();
            }

            var byKind = added.Select(f => new { File = f, Kind = ClassifyScannedFile(folder, f) }).ToList();

            // ---- Ảnh nguồn mới (từ Pylon / MVS / copy tay...) ----
            foreach (var entry in byKind.Where(e => e.Kind == ScannedKind.Source))
            {
                if (_rawIndexByPath.ContainsKey(entry.File))
                {
                    continue;
                }

                var info = new FileInfo(entry.File);
                if (!info.Exists)
                {
                    continue;
                }

                string maskPath = FindMaskPath(entry.File);
                _rawItems.Add(new PushItem
                {
                    FullPath = entry.File,
                    Length = info.Length,
                    LastWriteUtc = info.LastWriteTimeUtc,
                    LastWrite = info.LastWriteTime,
                    MaskPath = maskPath,
                    State = maskPath != null ? "Đã có mask" : "Chờ push",
                    StateColor = maskPath != null ? DoneColor : Color.White
                });
                addedRaw++;
            }

            RebuildRawIndexes();

            // ---- Ảnh Mask mới: thêm vào danh sách Mask + gắn vào ảnh Raw cùng tên ----
            var maskPaths = new HashSet<string>(_maskItems.Select(m => m.FullPath), StringComparer.OrdinalIgnoreCase);
            foreach (var entry in byKind.Where(e => e.Kind == ScannedKind.MaskOutput))
            {
                if (maskPaths.Contains(entry.File))
                {
                    continue;
                }

                if (IsFreshOutput(entry.File))
                {
                    deferred.Add(entry.File);
                    continue;
                }

                _maskItems.Add(new MaskItem { FullPath = entry.File, LastWrite = File.GetLastWriteTime(entry.File) });
                maskPaths.Add(entry.File);
                addedMask++;

                string raw = FindRawForMask(entry.File);
                int index;
                // MaskPath có thể đã được gán sẵn (chọn ảnh thì khung xem trước tự dò mask) - vẫn phải
                // cập nhật trạng thái dòng.
                if (raw != null && _rawIndexByPath.TryGetValue(raw, out index)
                    && (_rawItems[index].MaskPath == null || string.Equals(_rawItems[index].MaskPath, entry.File, StringComparison.OrdinalIgnoreCase)))
                {
                    var item = _rawItems[index];
                    item.MaskPath = entry.File;
                    if (item.FromCamera)
                    {
                        ApplyCameraResultState(item, null);
                    }
                    else
                    {
                        item.State = "Đã có mask";
                        item.StateColor = DoneColor;
                    }
                }
            }

            // ---- Ảnh trong PATH\raw không trùng ảnh nguồn nào = ảnh camera (giống ScanFolder) ----
            foreach (var entry in byKind.Where(e => e.Kind == ScannedKind.RawOutput))
            {
                string stripped = System.Text.RegularExpressions.Regex.Replace(
                    Path.GetFileNameWithoutExtension(entry.File), @"(_raw)?(_(OK|NG|LIMIT))?$", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (_rawIndexByBaseName.ContainsKey(stripped) || _rawIndexByPath.ContainsKey(entry.File))
                {
                    continue;
                }

                if (IsFreshOutput(entry.File))
                {
                    deferred.Add(entry.File);
                    continue;
                }

                _rawIndexByPath[entry.File] = _rawItems.Count;
                _rawItems.Add(CreateCameraRawItem(entry.File, null, null));
                addedRaw++;
            }

            _maskItems.Sort((a, b) => string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase));
            RebuildRawIndexes();

            _lastScanFiles = current;
            foreach (var file in deferred)
            {
                _lastScanFiles.Remove(file);   // lượt sau xét lại
            }

            if (deferred.Count > 0)
            {
                _imageFolderRefresh.Kick();
            }

            if (addedRaw + addedMask + removedCount == 0)
            {
                return;   // chỉ là ảnh ứng dụng tự lưu / tự xoá - danh sách đã đúng
            }

            RenderRawPage();
            RenderMaskPage();
            ReselectRow(lvRawImage, _selectedRawPath == null ? -1 : _rawItems.FindIndex(i => string.Equals(i.FullPath, _selectedRawPath, StringComparison.OrdinalIgnoreCase)), _rawPage);
            ReselectRow(lvMaskImage, _selectedMaskPath == null ? -1 : _maskItems.FindIndex(i => string.Equals(i.FullPath, _selectedMaskPath, StringComparison.OrdinalIgnoreCase)), _maskPage);
            if (removedCount > 0)
            {
                ShowSelectedImage();
            }

            string summary = "Thư mục thay đổi - tự cập nhật: "
                + string.Join(", ", new[]
                {
                    addedRaw > 0 ? "+" + addedRaw + " Raw" : null,
                    addedMask > 0 ? "+" + addedMask + " Mask" : null,
                    removedCount > 0 ? "−" + removedCount + " ảnh" : null
                }.Where(s => s != null));
            Log(summary);
            UpdateStatus(summary + $"  |  Raw: {_rawItems.Count} | Mask: {_maskItems.Count} | Còn thiếu: {_rawItems.Count(x => x.MaskPath == null)}");
        }
    }
}
