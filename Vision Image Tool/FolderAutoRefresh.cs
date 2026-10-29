using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Tự cập nhật danh sách ảnh khi thư mục thay đổi (ảnh mới / xoá / đổi tên từ phần mềm khác: Pylon,
    /// MVS, Vision Master, copy tay...). CHỈ để hiển thị - không push gì cả; push ảnh mới sang UnitX vẫn
    /// là việc của checkbox "Tự push UnitX" ở tab Capture &amp; Monitor.
    ///
    /// Sự kiện của FileSystemWatcher bắn liên tục khi camera đang ghi file: gom lại, đợi thư mục yên
    /// <see cref="DebounceMs"/> ms rồi mới gọi <c>onChanged</c> MỘT lần trên luồng giao diện.
    /// </summary>
    internal sealed class FolderAutoRefresh : IDisposable
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };

        private readonly Control _owner;
        private readonly Action _onChanged;
        private readonly Timer _timer;
        private FileSystemWatcher _watcher;

        public FolderAutoRefresh(Control owner, Action onChanged, int debounceMs = 1000)
        {
            _owner = owner;
            _onChanged = onChanged;
            _timer = new Timer { Interval = debounceMs };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                if (_watcher != null)
                {
                    _onChanged();
                }
            };
        }

        public int DebounceMs
        {
            get { return _timer.Interval; }
        }

        /// <summary>Thư mục đang theo dõi (null = không theo dõi).</summary>
        public string Folder { get; private set; }

        public bool Recursive { get; private set; }

        /// <summary>Theo dõi thư mục mới (null / không tồn tại = dừng). Cùng thư mục thì giữ nguyên.</summary>
        public void Watch(string folder, bool recursive)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                try
                {
                    folder = Path.GetFullPath(folder);
                }
                catch
                {
                    folder = null;
                }
            }

            if (_watcher != null && string.Equals(Folder, folder, StringComparison.OrdinalIgnoreCase) && Recursive == recursive)
            {
                return;
            }

            Stop();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            try
            {
                var watcher = new FileSystemWatcher(folder)
                {
                    IncludeSubdirectories = recursive,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    InternalBufferSize = 64 * 1024
                };
                watcher.Created += (s, e) => OnEvent(e.FullPath, null);
                watcher.Deleted += (s, e) => OnEvent(e.FullPath, null);
                watcher.Changed += (s, e) => OnEvent(e.FullPath, null);
                watcher.Renamed += (s, e) => OnEvent(e.FullPath, e.OldFullPath);
                watcher.Error += (s, e) => Kick();   // tràn bộ đệm: quét lại cả thư mục cho chắc
                watcher.EnableRaisingEvents = true;

                _watcher = watcher;
                Folder = folder;
                Recursive = recursive;
            }
            catch
            {
                // Thư mục mạng không hỗ trợ theo dõi / không có quyền: bỏ qua, vẫn bấm quét tay được.
                Stop();
            }
        }

        public void Stop()
        {
            _timer.Stop();
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            Folder = null;
        }

        /// <summary>Hẹn quét lại sau <see cref="DebounceMs"/> ms (vd. lượt trước còn file đang ghi dở).</summary>
        public void Kick()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated)
            {
                return;
            }

            try
            {
                _owner.BeginInvoke((Action)(() =>
                {
                    if (_watcher != null)
                    {
                        _timer.Stop();
                        _timer.Start();
                    }
                }));
            }
            catch (InvalidOperationException)
            {
                // Form đang đóng.
            }
        }

        private void OnEvent(string path, string oldPath)
        {
            if (IsRelevant(path) || (oldPath != null && IsRelevant(oldPath)))
            {
                Kick();
            }
        }

        /// <summary>File ảnh, hoặc thư mục (không có phần mở rộng - xoá / đổi tên cả thư mục con).</summary>
        private static bool IsRelevant(string path)
        {
            string extension = Path.GetExtension(path ?? string.Empty);
            return extension.Length == 0 || ImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
        }
    }
}
