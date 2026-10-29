using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Menu chuột phải dùng chung cho danh sách ảnh Raw / Mask (tab Capture & Monitor) và bảng ảnh
    /// của Dataset Manager. Bên sở hữu danh sách chỉ cần cho biết: đang chọn những file nào, ảnh cặp
    /// của một file là gì, và làm mới danh sách ra sao sau khi xoá / đổi tên.
    ///
    ///   Mở ảnh | Mở bằng... | Mở bằng Paint | Mở thư mục chứa file
    ///   Copy ▸ Đường dẫn / Tên file / Ảnh
    ///   [mục riêng của từng nơi: Xem ảnh cặp, Gán nhãn, Push lại...]
    ///   Đổi tên... | Thuộc tính...
    ///   Xoá...
    ///
    /// Chọn nhiều file: các mục chỉ có nghĩa với MỘT file (Mở, Mở bằng, Đổi tên, Copy ảnh) bị làm mờ.
    /// </summary>
    internal sealed class ImageListMenu
    {
        private readonly Control _owner;

        public readonly ContextMenuStrip Menu = new ContextMenuStrip();

        private readonly ToolStripMenuItem mnuOpen = new ToolStripMenuItem("Mở ảnh");
        private readonly ToolStripMenuItem mnuOpenWith = new ToolStripMenuItem("Mở bằng...");
        private readonly ToolStripMenuItem mnuOpenPaint = new ToolStripMenuItem("Mở bằng Paint");
        private readonly ToolStripMenuItem mnuShowInFolder = new ToolStripMenuItem("Mở thư mục chứa file");
        private readonly ToolStripMenuItem mnuCopy = new ToolStripMenuItem("Copy");
        private readonly ToolStripMenuItem mnuCopyPath = new ToolStripMenuItem("Đường dẫn");
        private readonly ToolStripMenuItem mnuCopyName = new ToolStripMenuItem("Tên file");
        private readonly ToolStripMenuItem mnuCopyImage = new ToolStripMenuItem("Ảnh (dán vào Excel / Paint)");
        private readonly ToolStripMenuItem mnuRename = new ToolStripMenuItem("Đổi tên...");
        private readonly ToolStripMenuItem mnuProperties = new ToolStripMenuItem("Thuộc tính...");
        private readonly ToolStripMenuItem mnuDelete = new ToolStripMenuItem("Xoá...");
        private readonly ToolStripMenuItem mnuComment = new ToolStripMenuItem("Thêm / sửa ghi chú...");
        private readonly ToolStripMenuItem mnuDeleteComment = new ToolStripMenuItem("Xoá ghi chú");
        private readonly ToolStripMenuItem mnuUndo = new ToolStripMenuItem("Hoàn tác");
        private readonly ToolStripMenuItem mnuRedo = new ToolStripMenuItem("Làm lại");
        private readonly ToolStripSeparator _hostSeparator = new ToolStripSeparator();

        /// <summary>Đường dẫn các file đang chọn (theo thứ tự hiển thị).</summary>
        public Func<IList<string>> SelectedPaths;

        /// <summary>Ảnh cặp (Raw ↔ Mask) của một file, null nếu không có.</summary>
        public Func<string, string> PairPath;

        /// <summary>Mô tả tên file theo schema (hiện trong hộp Đổi tên), null = không kiểm tra.</summary>
        public Func<string, string> DescribeName;

        /// <summary>Thông tin thêm cho hộp Thuộc tính (kết quả tách schema, nhãn...).</summary>
        public Func<string, IList<KeyValuePair<string, string>>> ExtraProperties;

        /// <summary>true = đang push / chụp: không cho xoá, đổi tên.</summary>
        public Func<bool> IsBusy;

        /// <summary>true = dữ liệu đang KHOÁ (chỉ xem): không cho đổi tên, xoá, ghi chú, hoàn tác.</summary>
        public Func<bool> IsLocked;

        /// <summary>Báo cho nơi dùng khi một thao tác sửa bị chặn vì đang khoá.</summary>
        public Action LockedNotice;

        /// <summary>Gọi TRƯỚC khi xoá / đổi tên (giải phóng ảnh đang xem trước...).</summary>
        public Action BeforeFileChange;

        /// <summary>Các file đã bị xoá (đã vào Thùng rác).</summary>
        public Action<IList<string>> Deleted;

        /// <summary>Các cặp (đường dẫn cũ, đường dẫn mới) đã đổi tên.</summary>
        public Action<IList<KeyValuePair<string, string>>> Renamed;

        /// <summary>Phím xoá file: Delete (Capture) hoặc Shift+Delete (Dataset - Delete đang dùng để xoá nhãn).</summary>
        public Keys DeleteKey = Keys.Delete;

        public ImageListMenu(Control owner)
        {
            _owner = owner;

            Menu.Font = new Font("Segoe UI", 9F);
            Menu.ShowImageMargin = false;

            mnuOpen.Font = new Font(Menu.Font, FontStyle.Bold);
            mnuOpen.ShortcutKeyDisplayString = "Enter";
            mnuCopyPath.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            mnuCopyImage.ShortcutKeyDisplayString = "Ctrl+C";
            mnuRename.ShortcutKeyDisplayString = "F2";
            mnuProperties.ShortcutKeyDisplayString = "Alt+Enter";
            mnuComment.ShortcutKeyDisplayString = "Ctrl+M";
            mnuDeleteComment.ShortcutKeyDisplayString = "Ctrl+Shift+M";
            mnuUndo.ShortcutKeyDisplayString = "Ctrl+Z";
            mnuRedo.ShortcutKeyDisplayString = "Ctrl+Y";
            mnuDelete.ForeColor = Color.FromArgb(185, 28, 28);

            mnuOpen.Click += (s, e) => Run(OpenSelected);
            mnuOpenWith.Click += (s, e) => Run(() => ImageFileOps.OpenWith(Single()));
            mnuOpenPaint.Click += (s, e) => Run(() => ImageFileOps.OpenInPaint(Single()));
            mnuShowInFolder.Click += (s, e) => Run(() => ImageFileOps.ShowInExplorer(Selected()));
            mnuCopyPath.Click += (s, e) => Run(CopyPaths);
            mnuCopyName.Click += (s, e) => Run(() => ImageFileOps.CopyText(Selected().Select(Path.GetFileName)));
            mnuCopyImage.Click += (s, e) => Run(CopyImage);
            mnuRename.Click += (s, e) => Run(RenameSelected);
            mnuProperties.Click += (s, e) => Run(ShowProperties);
            mnuDelete.Click += (s, e) => Run(DeleteSelected);
            mnuComment.Click += (s, e) => Run(EditComment);
            mnuDeleteComment.Click += (s, e) => Run(DeleteComment);
            mnuUndo.Click += (s, e) => Run(UndoFile);
            mnuRedo.Click += (s, e) => Run(RedoFile);

            mnuCopy.DropDownItems.AddRange(new ToolStripItem[] { mnuCopyPath, mnuCopyName, mnuCopyImage });

            Menu.Items.AddRange(new ToolStripItem[]
            {
                mnuOpen, mnuOpenWith, mnuOpenPaint, mnuShowInFolder,
                new ToolStripSeparator(),
                mnuCopy,
                new ToolStripSeparator(),
                mnuComment, mnuDeleteComment,
                _hostSeparator,
                mnuRename, mnuProperties,
                new ToolStripSeparator(),
                mnuUndo, mnuRedo,
                new ToolStripSeparator(),
                mnuDelete
            });

            Menu.Opening += Menu_Opening;
        }

        /// <summary>Thêm mục riêng của nơi dùng menu (đặt ngay trên "Đổi tên...").</summary>
        public void AddHostItems(params ToolStripItem[] items)
        {
            int index = Menu.Items.IndexOf(mnuRename);
            foreach (var item in items)
            {
                Menu.Items.Insert(index++, item);
            }

            Menu.Items.Insert(index, new ToolStripSeparator());
        }

        private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var paths = Selected();
            if (paths.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            bool single = paths.Count == 1;
            bool busy = IsBusy != null && IsBusy();

            mnuOpen.Enabled = mnuOpenWith.Enabled = mnuOpenPaint.Enabled = single;
            mnuCopyImage.Enabled = single;
            mnuRename.Enabled = single && !busy;
            mnuDelete.Enabled = !busy;

            mnuCopyPath.Text = single ? "Đường dẫn" : $"Đường dẫn ({paths.Count} dòng)";
            mnuCopyName.Text = single ? "Tên file" : $"Tên file ({paths.Count} dòng)";
            mnuDelete.Text = single ? "Xoá..." : $"Xoá {paths.Count} ảnh...";
            mnuDelete.ShortcutKeyDisplayString = KeyText(DeleteKey);
            mnuProperties.Text = single ? "Thuộc tính..." : $"Thuộc tính ({paths.Count} ảnh)...";
            mnuDelete.ToolTipText = busy ? "Đang push / chụp - chờ xong rồi thao tác." : null;

            int comments = paths.Count(ImageComments.Has);
            mnuComment.Text = single
                ? (comments > 0 ? "Sửa ghi chú..." : "Thêm ghi chú...")
                : $"Thêm / sửa ghi chú ({paths.Count} ảnh)...";
            mnuDeleteComment.Enabled = comments > 0;
            mnuDeleteComment.Text = single || comments == 0 ? "Xoá ghi chú" : $"Xoá ghi chú ({comments} ảnh)";

            bool locked = IsLocked != null && IsLocked();
            mnuRename.Enabled &= !locked;
            mnuDelete.Enabled &= !locked;
            mnuComment.Enabled = !locked;
            mnuDeleteComment.Enabled &= !locked;
            mnuUndo.Enabled = FileHistory.CanUndo && !busy && !locked;
            mnuUndo.Text = FileHistory.CanUndo ? "Hoàn tác: " + FileHistory.UndoText : "Hoàn tác";
            mnuRedo.Enabled = FileHistory.CanRedo && !busy && !locked;
            mnuRedo.Text = FileHistory.CanRedo ? "Làm lại: " + FileHistory.RedoText : "Làm lại";
        }

        /// <summary>Phím tắt của menu khi danh sách đang được focus. Trả về true nếu đã xử lý.</summary>
        public bool HandleKey(Keys keyData)
        {
            // Đang khoá: nuốt các phím sửa (không để lọt xuống bảng) và báo cho người dùng.
            if (IsLocked != null && IsLocked() && IsEditKey(keyData))
            {
                if (LockedNotice != null)
                {
                    LockedNotice();
                }

                return true;
            }

            // Hoàn tác / làm lại không cần đang chọn ảnh (vừa xoá xong thì danh sách thường trống vùng chọn).
            if (keyData == (Keys.Control | Keys.Z) || keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
            {
                Run(keyData == (Keys.Control | Keys.Z) ? (Action)UndoFile : RedoFile);
                return true;
            }

            Action action = null;

            if (keyData == Keys.Enter) action = OpenSelected;
            else if (keyData == (Keys.Alt | Keys.Enter)) action = ShowProperties;
            else if (keyData == Keys.F2) action = RenameSelected;
            else if (keyData == (Keys.Control | Keys.Shift | Keys.C)) action = CopyPaths;
            else if (keyData == (Keys.Control | Keys.C)) action = CopyImage;
            else if (keyData == DeleteKey) action = DeleteSelected;
            else if (keyData == (Keys.Control | Keys.M)) action = EditComment;
            else if (keyData == (Keys.Control | Keys.Shift | Keys.M)) action = DeleteComment;

            if (action == null || Selected().Count == 0)
            {
                return false;
            }

            Run(action);
            return true;
        }

        private bool IsEditKey(Keys keyData)
        {
            return keyData == (Keys.Control | Keys.Z) || keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z)
                || keyData == Keys.F2 || keyData == DeleteKey || keyData == (Keys.Control | Keys.M) || keyData == (Keys.Control | Keys.Shift | Keys.M);
        }

        #region Hành động

        private IList<string> Selected()
        {
            var paths = SelectedPaths != null ? SelectedPaths() : null;
            return paths == null ? new List<string>() : paths.Where(p => !string.IsNullOrEmpty(p)).ToList();
        }

        private string Single()
        {
            var paths = Selected();
            return paths.Count == 1 ? paths[0] : null;
        }

        private void Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_owner, ex.Message, "Lỗi thao tác file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Busy()
        {
            if (IsBusy != null && IsBusy())
            {
                MessageBox.Show(_owner, "Đang push / chụp ảnh - chờ xong rồi mới xoá hoặc đổi tên file.",
                    "Đang bận", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            return false;
        }

        private void OpenSelected()
        {
            string path = Single();
            if (path != null)
            {
                ImageFileOps.OpenDefault(path);
            }
        }

        private void CopyPaths()
        {
            ImageFileOps.CopyText(Selected());
        }

        private void CopyImage()
        {
            string path = Single();
            if (path != null)
            {
                ImageFileOps.CopyImageToClipboard(path);
            }
        }

        private string Pair(string path)
        {
            string pair = PairPath != null ? PairPath(path) : null;
            return pair != null && File.Exists(pair) && !string.Equals(pair, path, StringComparison.OrdinalIgnoreCase)
                ? pair
                : null;
        }

        private void DeleteSelected()
        {
            var paths = Selected();
            if (paths.Count == 0 || Busy())
            {
                return;
            }

            var selectedSet = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
            var pairs = paths.Select(Pair)
                .Where(p => p != null && !selectedSet.Contains(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            IList<string> targets;
            using (var dialog = new frmDeleteImages(paths, pairs))
            {
                if (dialog.ShowDialog(_owner) != DialogResult.OK)
                {
                    return;
                }

                targets = dialog.Files;
            }

            if (BeforeFileChange != null)
            {
                BeforeFileChange();
            }

            List<string> failed;
            var deleted = ImageFileOps.DeleteToRecycleBin(_owner.FindForm(), targets, out failed);
            FileHistory.RecordDelete(deleted, deleted.ToDictionary(p => p, ImageComments.Get, StringComparer.OrdinalIgnoreCase));
            ImageComments.OnDeleted(deleted);

            if (deleted.Count > 0 && Deleted != null)
            {
                Deleted(deleted);
            }

            if (failed.Count > 0)
            {
                MessageBox.Show(_owner,
                    $"Không xoá được {failed.Count} file (đang mở ở chương trình khác / không có quyền):"
                    + Environment.NewLine + string.Join(Environment.NewLine, failed.Take(10).Select(Path.GetFileName))
                    + (failed.Count > 10 ? Environment.NewLine + "..." : string.Empty),
                    "Xoá ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RenameSelected()
        {
            string path = Single();
            if (path == null || Busy())
            {
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show(_owner, "File không còn tồn tại:" + Environment.NewLine + path,
                    "Đổi tên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var moves = new List<KeyValuePair<string, string>>();
            using (var dialog = new frmRenameImage(path, Pair(path), DescribeName))
            {
                if (dialog.ShowDialog(_owner) != DialogResult.OK)
                {
                    return;
                }

                moves.Add(new KeyValuePair<string, string>(path, dialog.NewPath));
                if (dialog.NewPairPath != null)
                {
                    moves.Add(new KeyValuePair<string, string>(dialog.PairPath, dialog.NewPairPath));
                }
            }

            moves.RemoveAll(m => string.Equals(m.Key, m.Value, StringComparison.Ordinal));
            if (moves.Count == 0)
            {
                return;
            }

            if (BeforeFileChange != null)
            {
                BeforeFileChange();
            }

            string error;
            if (!ImageFileOps.RenameAll(moves, out error))
            {
                MessageBox.Show(_owner, "Không đổi tên được (đã hoàn tác):" + Environment.NewLine + error,
                    "Đổi tên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ImageComments.OnRenamed(moves);
            FileHistory.RecordRename(moves);

            if (Renamed != null)
            {
                Renamed(moves);
            }
        }

        /// <summary>Thêm / sửa ghi chú cho mọi ảnh đang chọn (một hay nhiều ảnh đều qua một hộp).</summary>
        private void EditComment()
        {
            ImageComments.Edit(_owner, Selected());
        }

        private void DeleteComment()
        {
            ImageComments.Delete(_owner, Selected());
        }

        /// <summary>Ctrl+Z: hoàn tác thao tác file gần nhất (đổi tên / xoá) - dùng chung cho cả hai tab.</summary>
        private void UndoFile()
        {
            if (!FileHistory.CanUndo)
            {
                return;
            }

            if (!Busy())
            {
                FileHistory.Undo(_owner);
            }
        }

        private void RedoFile()
        {
            if (!FileHistory.CanRedo)
            {
                return;
            }

            if (!Busy())
            {
                FileHistory.Redo(_owner);
            }
        }

        private void ShowProperties()
        {
            var paths = Selected();
            if (paths.Count == 0)
            {
                return;
            }

            using (var dialog = new frmImageProperties())
            {
                if (paths.Count == 1)
                {
                    string pair = Pair(paths[0]);
                    var extra = new List<KeyValuePair<string, string>>(ExtraProperties != null ? ExtraProperties(paths[0]) ?? new List<KeyValuePair<string, string>>() : new List<KeyValuePair<string, string>>());
                    extra.Add(new KeyValuePair<string, string>("Ghi chú", ImageComments.Get(paths[0]) ?? "(không có)"));
                    dialog.ShowSingle(ImageFileOps.ReadInfo(paths[0]), pair != null ? ImageFileOps.ReadInfo(pair) : null, extra);
                }
                else
                {
                    dialog.ShowMany(paths.Select(ImageFileOps.ReadInfo).ToList());
                }

                dialog.ShowDialog(_owner);
            }
        }

        #endregion

        private static string KeyText(Keys keys)
        {
            return keys == (Keys.Shift | Keys.Delete) ? "Shift+Del" : keys == Keys.Delete ? "Del" : keys.ToString();
        }
    }
}
