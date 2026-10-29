using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code. Nếu class đầu tiên là partial frmMainV1, VS tự gắn SubType=Form rồi mở Designer - Designer
    // không thấy lớp cha ("base class System.Void") và đã từng ghi đè frmMainV1.Designer.cs thành form
    // trống (2026-09-25). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1FilesCodeFile { }

    /// <summary>
    /// frmMainV1 - menu chuột phải của danh sách ảnh Raw / Mask: mở, copy, đổi tên, thuộc tính,
    /// xoá (vào Thùng rác) - có chọn nhiều dòng (Shift / Ctrl). Sau mỗi thao tác danh sách được sửa
    /// ngay trong bộ nhớ và vẽ lại trang đang xem, không quét lại cả thư mục.
    /// </summary>
    public partial class frmMainV1
    {
        private ImageListMenu _rawMenu;
        private ImageListMenu _maskMenu;

        /// <summary>
        /// File vừa được người dùng đổi tên trong phần mềm: "Theo dõi thư mục" sẽ thấy sự kiện Renamed
        /// và tưởng là ảnh mới - bỏ qua trong vài giây để không push lại.
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _manualRenames =
            new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan ManualRenameIgnoreWindow = TimeSpan.FromSeconds(10);

        private bool _rawSelectionPending;
        private bool _maskSelectionPending;

        private void SetupImageListMenus()
        {
            _rawMenu = CreateImageMenu(lvRawImage, RawPairPath);
            _maskMenu = CreateImageMenu(lvMaskImage, FindRawForMask);

            var mnuViewMask = new ToolStripMenuItem("Xem ảnh Mask") { ShortcutKeyDisplayString = "Space" };
            mnuViewMask.Click += (s, e) => { _previewMode = PreviewMode.Mask; ShowSelectedImage(); };
            var mnuPush = new ToolStripMenuItem("Push lại ảnh đã chọn lên UnitX");
            mnuPush.Click += (s, e) => PushSelectedRawItems();
            _rawMenu.AddHostItems(mnuViewMask, mnuPush);
            _rawMenu.Menu.Opening += (s, e) =>
            {
                var items = GetSelectedRawItems();
                int pushable = items.Count(i => !i.FromCamera || !HasMask(i.FullPath));
                mnuViewMask.Enabled = items.Count == 1;
                mnuPush.Enabled = pushable > 0 && _sdkRunning && !IsImageListBusy() && _folderWatcher == null;
                mnuPush.Text = pushable > 1 ? $"Push lại {pushable} ảnh đã chọn lên UnitX" : "Push lại ảnh đã chọn lên UnitX";
                mnuPush.ToolTipText = items.Count > 0 && pushable == 0 ? "Ảnh camera đã có mask (đã có kết quả) - không push lại." : null;
            };

            var mnuViewRaw = new ToolStripMenuItem("Xem ảnh Raw") { ShortcutKeyDisplayString = "Space" };
            mnuViewRaw.Click += (s, e) => { _previewMode = PreviewMode.Raw; ShowSelectedImage(); };
            _maskMenu.AddHostItems(mnuViewRaw);
            _maskMenu.Menu.Opening += (s, e) => mnuViewRaw.Enabled = SelectedPaths(lvMaskImage).Count == 1;
        }

        private ImageListMenu CreateImageMenu(ListView list, Func<string, string> pairPath)
        {
            var menu = new ImageListMenu(list)
            {
                SelectedPaths = () => SelectedPaths(list),
                PairPath = pairPath,
                IsBusy = IsImageListBusy,
                Deleted = OnImagesDeleted,
                Renamed = OnImagesRenamed,
                DeleteKey = Keys.Delete
            };

            list.ContextMenuStrip = menu.Menu;
            return menu;
        }

        /// <summary>
        /// Phím tắt của danh sách Raw / Mask đang focus (gọi từ ProcessCmdKey - nhận được cả Enter,
        /// Alt+Enter mà sự kiện KeyDown của ListView có thể không thấy). Ctrl+A = chọn cả trang.
        /// </summary>
        private bool HandleImageListKey(Keys keyData)
        {
            ListView list = lvRawImage.Focused ? lvRawImage : lvMaskImage.Focused ? lvMaskImage : null;
            if (list == null)
            {
                return false;
            }

            if (keyData == (Keys.Control | Keys.A))
            {
                list.BeginUpdate();
                foreach (ListViewItem item in list.Items)
                {
                    item.Selected = true;
                }

                list.EndUpdate();
                return true;
            }

            return (list == lvRawImage ? _rawMenu : _maskMenu).HandleKey(keyData);
        }

        /// <summary>Đang push / trigger: không cho xoá, đổi tên file (file có thể đang được đọc / ghi).</summary>
        private bool IsImageListBusy()
        {
            return (_pushTask != null && !_pushTask.IsCompleted) || IsTriggering;
        }

        private List<string> SelectedPaths(ListView list)
        {
            var paths = new List<string>();
            foreach (ListViewItem row in list.SelectedItems)
            {
                if (!(row.Tag is int))
                {
                    continue;
                }

                int index = (int)row.Tag;
                if (list == lvRawImage && index >= 0 && index < _rawItems.Count)
                {
                    paths.Add(_rawItems[index].FullPath);
                }
                else if (list == lvMaskImage && index >= 0 && index < _maskItems.Count)
                {
                    paths.Add(_maskItems[index].FullPath);
                }
            }

            return paths;
        }

        /// <summary>Các ảnh raw đang chọn (nhiều dòng).</summary>
        private List<PushItem> GetSelectedRawItems()
        {
            var items = new List<PushItem>();
            foreach (ListViewItem row in lvRawImage.SelectedItems)
            {
                if (row.Tag is int && (int)row.Tag >= 0 && (int)row.Tag < _rawItems.Count)
                {
                    items.Add(_rawItems[(int)row.Tag]);
                }
            }

            return items;
        }

        private string RawPairPath(string rawPath)
        {
            int index;
            if (_rawIndexByPath.TryGetValue(rawPath, out index))
            {
                var item = _rawItems[index];
                item.MaskPath = item.MaskPath != null && File.Exists(item.MaskPath)
                    ? item.MaskPath
                    : item.FromCamera ? FindCameraMaskPath(rawPath) : FindMaskPath(rawPath);
                return item.MaskPath;
            }

            return FindMaskPath(rawPath);
        }

        /// <summary>Dòng "đang đứng" của danh sách: dòng có focus nếu nó đang được chọn, không thì dòng chọn đầu tiên.</summary>
        private static ListViewItem ActiveRow(ListView list)
        {
            if (list.FocusedItem != null && list.FocusedItem.Selected)
            {
                return list.FocusedItem;
            }

            return list.SelectedItems.Count > 0 ? list.SelectedItems[0] : null;
        }

        #region Làm mới danh sách sau khi xoá / đổi tên

        private void OnImagesDeleted(IList<string> deleted)
        {
            var set = new HashSet<string>(deleted, StringComparer.OrdinalIgnoreCase);

            _rawItems.RemoveAll(i => set.Contains(i.FullPath));
            _maskItems.RemoveAll(i => set.Contains(i.FullPath));

            foreach (var item in _rawItems.Where(i => i.MaskPath != null && set.Contains(i.MaskPath)))
            {
                item.MaskPath = null;
                if (item.FromCamera)
                {
                    // Mất mask = chưa ghi nhận kết quả -> nền xám, push lại được.
                    ApplyCameraResultState(item, null);
                }
                else
                {
                    item.State = "Chờ push";
                    item.StateColor = Color.White;
                }
            }

            if (_selectedRawPath != null && set.Contains(_selectedRawPath))
            {
                _selectedRawPath = null;
            }

            if (_selectedMaskPath != null && set.Contains(_selectedMaskPath))
            {
                _selectedMaskPath = null;
            }

            RebuildRawIndexes();
            RenderRawPage();
            RenderMaskPage();
            ShowSelectedImage();

            foreach (var path in deleted)
            {
                Log("Đã xoá (Thùng rác): " + path);
            }

            UpdateStatus($"Đã xoá {deleted.Count} ảnh (vào Thùng rác).");
        }

        private void OnImagesRenamed(IList<KeyValuePair<string, string>> moves)
        {
            foreach (var move in moves)
            {
                _manualRenames[move.Value] = DateTime.UtcNow;

                foreach (var item in _rawItems)
                {
                    if (string.Equals(item.FullPath, move.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        item.FullPath = move.Value;
                    }

                    if (item.MaskPath != null && string.Equals(item.MaskPath, move.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        item.MaskPath = move.Value;
                    }
                }

                foreach (var item in _maskItems.Where(i => string.Equals(i.FullPath, move.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    item.FullPath = move.Value;
                }

                if (string.Equals(_selectedRawPath, move.Key, StringComparison.OrdinalIgnoreCase))
                {
                    _selectedRawPath = move.Value;
                }

                if (string.Equals(_selectedMaskPath, move.Key, StringComparison.OrdinalIgnoreCase))
                {
                    _selectedMaskPath = move.Value;
                }

                Log($"Đổi tên: {move.Key} -> {Path.GetFileName(move.Value)}");
            }

            RebuildRawIndexes();
            RenderRawPage();
            RenderMaskPage();

            // Chọn lại dòng vừa đổi tên để người dùng thấy ngay kết quả.
            string first = moves[0].Value;
            ReselectRow(lvRawImage, _rawItems.FindIndex(i => string.Equals(i.FullPath, first, StringComparison.OrdinalIgnoreCase)), _rawPage);
            ReselectRow(lvMaskImage, _maskItems.FindIndex(i => string.Equals(i.FullPath, first, StringComparison.OrdinalIgnoreCase)), _maskPage);
            ShowSelectedImage();

            UpdateStatus("Đã đổi tên: " + string.Join(", ", moves.Select(m => Path.GetFileName(m.Value))));
        }

        private void ReselectRow(ListView list, int index, int page)
        {
            int offset = index - page * PageSize;
            if (index < 0 || offset < 0 || offset >= list.Items.Count)
            {
                return;
            }

            _suppressSelectionEvents = true;
            try
            {
                list.SelectedItems.Clear();
                list.Items[offset].Selected = true;
                list.Items[offset].Focused = true;
                list.Items[offset].EnsureVisible();
            }
            finally
            {
                _suppressSelectionEvents = false;
            }
        }

        /// <summary>Dựng lại các bảng tra đường dẫn / tên -> chỉ số sau khi danh sách Raw thay đổi.</summary>
        private void RebuildRawIndexes()
        {
            _rawIndexByPath.Clear();
            _rawIndexByBaseName.Clear();

            for (int i = 0; i < _rawItems.Count; i++)
            {
                var item = _rawItems[i];
                _rawIndexByPath[item.FullPath] = i;

                // Giống ScanFolder: chỉ ảnh nguồn mới dùng để dò ngược từ tên mask.
                string baseName = Path.GetFileNameWithoutExtension(item.FullPath);
                if (!item.FromCamera && !_rawIndexByBaseName.ContainsKey(baseName))
                {
                    _rawIndexByBaseName[baseName] = i;
                }
            }
        }

        /// <summary>Theo dõi thư mục: bỏ qua sự kiện Renamed do chính người dùng đổi tên trong phần mềm.</summary>
        private bool IsManualRename(string fullPath)
        {
            DateTime at;
            if (_manualRenames.TryGetValue(fullPath, out at))
            {
                if (DateTime.UtcNow - at < ManualRenameIgnoreWindow)
                {
                    return true;
                }

                _manualRenames.TryRemove(fullPath, out at);
            }

            return false;
        }

        #endregion
    }
}
