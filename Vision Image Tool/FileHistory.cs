using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>Thay đổi file vừa được hoàn tác / làm lại - hai tab nghe để cập nhật danh sách.</summary>
    internal sealed class FileHistoryChange
    {
        /// <summary>Các cặp (tên cũ, tên mới) vừa đổi tên.</summary>
        public IList<KeyValuePair<string, string>> Renamed = new List<KeyValuePair<string, string>>();

        /// <summary>File vừa bị xoá (vào Thùng rác) - khi làm lại thao tác xoá.</summary>
        public IList<string> Deleted = new List<string>();

        /// <summary>File vừa được lấy lại từ Thùng rác - khi hoàn tác thao tác xoá.</summary>
        public IList<string> Restored = new List<string>();
    }

    /// <summary>
    /// Lịch sử thao tác file (đổi tên, xoá) để Hoàn tác (Ctrl+Z) / Làm lại (Ctrl+Y), dùng chung cho tab
    /// Capture &amp; Monitor và Dataset Manager vì cả hai thao tác trên cùng các file ảnh.
    ///
    ///   - Đổi tên : hoàn tác = đổi về tên cũ (cả ảnh cặp nếu đã đổi cùng lúc); ghi chú đi theo file.
    ///   - Xoá     : hoàn tác = lấy file ra khỏi Thùng rác về đúng chỗ cũ; ghi chú được gắn lại.
    ///
    /// Làm thao tác mới thì xoá danh sách "làm lại" (giống mọi trình soạn thảo).
    /// </summary>
    internal static class FileHistory
    {
        private const int MaxSteps = 50;

        private abstract class Operation
        {
            public string Description;
            public abstract FileHistoryChange Undo(IWin32Window owner);
            public abstract FileHistoryChange Redo(IWin32Window owner);
        }

        private static readonly Stack<Operation> UndoStack = new Stack<Operation>();
        private static readonly Stack<Operation> RedoStack = new Stack<Operation>();

        /// <summary>Hoàn tác / làm lại xong - mọi nơi đang hiển thị file cập nhật theo.</summary>
        public static event Action<FileHistoryChange> Applied;

        public static bool CanUndo
        {
            get { return UndoStack.Count > 0; }
        }

        public static bool CanRedo
        {
            get { return RedoStack.Count > 0; }
        }

        public static string UndoText
        {
            get { return CanUndo ? UndoStack.Peek().Description : null; }
        }

        public static string RedoText
        {
            get { return CanRedo ? RedoStack.Peek().Description : null; }
        }

        #region Ghi nhận

        public static void RecordRename(IList<KeyValuePair<string, string>> moves)
        {
            if (moves == null || moves.Count == 0)
            {
                return;
            }

            Push(new RenameOperation
            {
                Moves = moves.ToList(),
                Description = moves.Count == 1
                    ? "Đổi tên " + Path.GetFileName(moves[0].Key)
                    : $"Đổi tên {Path.GetFileName(moves[0].Key)} (+{moves.Count - 1} ảnh cặp)"
            });
        }

        /// <summary>Ghi nhận các file vừa vào Thùng rác cùng ghi chú của chúng (để gắn lại khi hoàn tác).</summary>
        public static void RecordDelete(IList<string> deleted, IDictionary<string, string> comments)
        {
            if (deleted == null || deleted.Count == 0)
            {
                return;
            }

            Push(new DeleteOperation
            {
                Paths = deleted.ToList(),
                Comments = comments == null
                    ? new Dictionary<string, string>()
                    : comments.Where(kv => !string.IsNullOrEmpty(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase),
                Description = deleted.Count == 1 ? "Xoá " + Path.GetFileName(deleted[0]) : $"Xoá {deleted.Count} ảnh"
            });
        }

        private static void Push(Operation operation)
        {
            UndoStack.Push(operation);
            RedoStack.Clear();

            if (UndoStack.Count > MaxSteps)
            {
                // Bỏ bước cũ nhất.
                var keep = UndoStack.Take(MaxSteps).Reverse().ToList();
                UndoStack.Clear();
                foreach (var op in keep)
                {
                    UndoStack.Push(op);
                }
            }
        }

        #endregion

        #region Hoàn tác / làm lại

        public static bool Undo(IWin32Window owner)
        {
            return Step(owner, UndoStack, RedoStack, true);
        }

        public static bool Redo(IWin32Window owner)
        {
            return Step(owner, RedoStack, UndoStack, false);
        }

        private static bool Step(IWin32Window owner, Stack<Operation> from, Stack<Operation> to, bool undo)
        {
            if (from.Count == 0)
            {
                return false;
            }

            var operation = from.Pop();
            FileHistoryChange change;
            try
            {
                change = undo ? operation.Undo(owner) : operation.Redo(owner);
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, (undo ? "Không hoàn tác được: " : "Không làm lại được: ") + operation.Description
                    + Environment.NewLine + ex.Message, undo ? "Hoàn tác" : "Làm lại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (change == null)
            {
                return false;
            }

            to.Push(operation);
            if (Applied != null)
            {
                Applied(change);
            }

            return true;
        }

        private sealed class RenameOperation : Operation
        {
            public List<KeyValuePair<string, string>> Moves;

            public override FileHistoryChange Undo(IWin32Window owner)
            {
                return Apply(Moves.Select(m => new KeyValuePair<string, string>(m.Value, m.Key)).ToList());
            }

            public override FileHistoryChange Redo(IWin32Window owner)
            {
                return Apply(Moves);
            }

            private static FileHistoryChange Apply(IList<KeyValuePair<string, string>> moves)
            {
                string error;
                if (!ImageFileOps.RenameAll(moves, out error))
                {
                    throw new IOException(error);
                }

                ImageComments.OnRenamed(moves);
                return new FileHistoryChange { Renamed = moves };
            }
        }

        private sealed class DeleteOperation : Operation
        {
            public List<string> Paths;
            public Dictionary<string, string> Comments;

            public override FileHistoryChange Undo(IWin32Window owner)
            {
                List<string> failed;
                var restored = RecycleBin.Restore(Paths, out failed);

                foreach (var path in restored)
                {
                    string comment;
                    if (Comments.TryGetValue(path, out comment))
                    {
                        ImageComments.Set(new[] { path }, comment);
                    }
                }

                if (failed.Count > 0)
                {
                    MessageBox.Show(owner,
                        $"Không lấy lại được {failed.Count} file từ Thùng rác (đã bị xoá hẳn / Thùng rác đã dọn / đã có file cùng tên):"
                        + Environment.NewLine + string.Join(Environment.NewLine, failed.Take(10).Select(Path.GetFileName)),
                        "Hoàn tác xoá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (restored.Count == 0)
                {
                    return null;
                }

                // Chỉ những file lấy lại được mới có thể "làm lại" (xoá lần nữa).
                Paths = restored;
                return new FileHistoryChange { Restored = restored };
            }

            public override FileHistoryChange Redo(IWin32Window owner)
            {
                // Ghi chú có thể đã sửa sau khi hoàn tác - chụp lại trước khi xoá lần nữa.
                foreach (var path in Paths)
                {
                    string comment = ImageComments.Get(path);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        Comments[path] = comment;
                    }
                }

                List<string> failed;
                var deleted = ImageFileOps.DeleteToRecycleBin(owner, Paths, out failed);
                ImageComments.OnDeleted(deleted);
                if (deleted.Count == 0)
                {
                    return null;
                }

                Paths = deleted.ToList();
                return new FileHistoryChange { Deleted = deleted };
            }
        }

        #endregion
    }

    /// <summary>
    /// Lấy file ra khỏi Thùng rác của Windows về đúng chỗ cũ.
    ///
    /// Mỗi file trong Thùng rác gồm 2 file trong X:\$Recycle.Bin\&lt;SID người dùng&gt;\ :
    ///   $Rxxxxxx.ext : nội dung file,   $Ixxxxxx.ext : thông tin (đường dẫn gốc, thời điểm xoá).
    /// Đọc thẳng $I (không dùng lệnh "Restore" của Explorer vì tên lệnh đổi theo ngôn ngữ Windows), chuyển
    /// $R về đường dẫn gốc rồi xoá $I - Thùng rác không còn mục đó nữa.
    /// </summary>
    internal static class RecycleBin
    {
        private sealed class Entry
        {
            public string OriginalPath;
            public DateTime Deleted;
            public string InfoFile;
            public string DataFile;
        }

        public static List<string> Restore(IList<string> paths, out List<string> failed)
        {
            failed = new List<string>();
            var restored = new List<string>();
            string sid = WindowsIdentity.GetCurrent().User.Value;
            var entriesByRoot = new Dictionary<string, List<Entry>>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                try
                {
                    string root = Path.GetPathRoot(path);
                    List<Entry> entries;
                    if (!entriesByRoot.TryGetValue(root, out entries))
                    {
                        entries = ReadEntries(Path.Combine(root, "$Recycle.Bin", sid));
                        entriesByRoot[root] = entries;
                    }

                    // Cùng một đường dẫn có thể bị xoá nhiều lần - lấy bản xoá gần nhất.
                    var entry = entries
                        .Where(e => string.Equals(e.OriginalPath, path, StringComparison.OrdinalIgnoreCase) && File.Exists(e.DataFile))
                        .OrderByDescending(e => e.Deleted)
                        .FirstOrDefault();

                    if (entry == null || File.Exists(path))
                    {
                        failed.Add(path);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.Move(entry.DataFile, path);
                    File.Delete(entry.InfoFile);
                    entries.Remove(entry);
                    restored.Add(path);
                }
                catch
                {
                    failed.Add(path);
                }
            }

            return restored;
        }

        private static List<Entry> ReadEntries(string folder)
        {
            var list = new List<Entry>();
            if (!Directory.Exists(folder))
            {
                return list;
            }

            foreach (var info in Directory.GetFiles(folder, "$I*"))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(info);
                    if (data.Length < 24)
                    {
                        continue;
                    }

                    long version = BitConverter.ToInt64(data, 0);
                    long fileTime = BitConverter.ToInt64(data, 16);
                    string original;

                    if (version == 2 && data.Length >= 28)
                    {
                        // Windows 10+: độ dài (số ký tự, kể cả \0) rồi tới đường dẫn UTF-16.
                        int chars = BitConverter.ToInt32(data, 24);
                        original = Encoding.Unicode.GetString(data, 28, Math.Min(chars * 2, data.Length - 28));
                    }
                    else
                    {
                        // Vista - 8.1: đường dẫn cố định 260 ký tự.
                        original = Encoding.Unicode.GetString(data, 24, Math.Min(520, data.Length - 24));
                    }

                    list.Add(new Entry
                    {
                        OriginalPath = original.TrimEnd('\0'),
                        Deleted = DateTime.FromFileTime(fileTime),
                        InfoFile = info,
                        DataFile = Path.Combine(folder, "$R" + Path.GetFileName(info).Substring(2))
                    });
                }
                catch
                {
                    // $I hỏng / không đọc được - bỏ qua mục đó.
                }
            }

            return list;
        }
    }
}
