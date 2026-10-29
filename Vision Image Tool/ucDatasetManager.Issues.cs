using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerIssuesCodeFile { }

    /// <summary>
    /// Khung "Vấn đề dữ liệu" dưới bảng: tổng hợp mọi chỗ bộ dữ liệu chưa đúng / chưa xong để người
    /// dùng biết vị trí và sửa - lọc theo loại, double-click tới ô tương ứng, xuất ra Excel.
    ///
    /// Loại vấn đề:
    ///   - Thiếu Mask / Thiếu Raw  : block (mẫu × DK) chỉ có một nửa của cặp.
    ///   - Sai schema              : tên file không khớp schema (không vào báo cáo).
    ///   - Trùng tên               : hai file cùng Model / DK / SPL / hậu tố / loại (vd. ở hai thư mục con).
    ///   - Thiếu DK / DK lạ        : so giữa các mẫu cùng Model - mẫu thiếu hẳn một DK mà đa số mẫu có,
    ///                               hoặc có DK chỉ vài mẫu có (thường do gõ sai tên).
    ///   - Chưa đánh giá            : có Mask nhưng chưa có Evaluation (tên file thiếu kết quả Vision).
    ///   - File lỗi                : 0 byte, không phải ảnh, JPEG bị cắt (ghi dở) - kiểm tra nền.
    /// </summary>
    public partial class ucDatasetManager
    {
        private enum IssueKind
        {
            MissingMask,
            MissingRaw,
            SchemaMismatch,
            Duplicate,
            MissingCondition,
            ExtraCondition,
            NoEvaluation,
            BadFile
        }

        private static readonly string[] IssueKindTexts =
        {
            "Thiếu Mask", "Thiếu Raw", "Sai schema", "Trùng tên", "Thiếu DK", "DK lạ", "Chưa đánh giá", "File lỗi"
        };

        private static readonly Color[] IssueKindColors =
        {
            Color.FromArgb(107, 114, 128), Color.FromArgb(180, 83, 9), Color.FromArgb(202, 138, 4), Color.FromArgb(220, 38, 38),
            Color.FromArgb(29, 78, 216), Color.FromArgb(124, 58, 237), Color.FromArgb(75, 85, 99),
            Color.FromArgb(185, 28, 28)
        };

        private sealed class DatasetIssue
        {
            public IssueKind Kind;
            public string Location;
            public string File;
            public string Detail;

            /// <summary>Ảnh cần chọn khi double-click (null = chọn hàng / block).</summary>
            public DatasetImage Image;
            public DatasetRow Row;
            public string Condition;
        }

        private readonly CheckBox chkShowIssues = new CheckBox();
        private SplitContainer _issuesSplit;

        private List<DatasetIssue> _issues = new List<DatasetIssue>();
        private List<DatasetIssue> _shownIssues = new List<DatasetIssue>();
        private string _issuesSignature;

        /// <summary>Kết quả kiểm tra file (khoá = đường dẫn|kích thước|thời gian ghi) -> lỗi, "" = file tốt.</summary>
        private readonly ConcurrentDictionary<string, string> _fileChecks = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool _fileCheckRunning;

        #region Giao diện

        /// <summary>Khung vấn đề (Designer: pnlIssuesCard / tlpIssues) - gắn sự kiện.</summary>
        private void WireIssuesPanel()
        {
            cboIssueKind.SelectedIndexChanged += (s, e) => FillIssueList();
            btnExportIssues.Click += (s, e) => ExportIssues();

            lvIssues.VirtualListSize = 0;
            lvIssues.RetrieveVirtualItem += LvIssues_RetrieveVirtualItem;
            lvIssues.DoubleClick += (s, e) => GoToSelectedIssue();
            lvIssues.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    GoToSelectedIssue();
                }
            };
        }

        private void LvIssues_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var issue = e.ItemIndex >= 0 && e.ItemIndex < _shownIssues.Count ? _shownIssues[e.ItemIndex] : null;
            if (issue == null)
            {
                e.Item = new ListViewItem(new[] { string.Empty, string.Empty, string.Empty, string.Empty });
                return;
            }

            var item = new ListViewItem(new[] { IssueKindTexts[(int)issue.Kind], issue.Location, issue.File, issue.Detail })
            {
                UseItemStyleForSubItems = false
            };
            item.SubItems[0].ForeColor = Theme.Fore(IssueKindColors[(int)issue.Kind]);
            item.SubItems[0].Font = _issueKindBold;
            e.Item = item;
        }

        #endregion

        #region Phát hiện vấn đề

        /// <summary>Tính lại danh sách vấn đề (gọi từ UpdateSummary - sau tải, lọc, gán nhãn, xoá, đổi tên).</summary>
        private void RefreshIssues()
        {
            if (_issuesSplit == null || _schema == null)
            {
                return;
            }

            var issues = FindIssues();

            // Không đổi gì (vd. chỉ lọc bảng) thì giữ nguyên danh sách + dòng đang chọn.
            string signature = string.Join("\n", issues.Select(i => (int)i.Kind + "|" + i.Location + "|" + i.File + "|" + i.Detail));
            if (signature == _issuesSignature)
            {
                // Cùng nội dung, cùng thứ tự: chỉ thay đối tượng mới (hàng / ảnh vừa dựng lại), giữ nguyên danh sách.
                _issues = issues;
                var kind = SelectedIssueKind();
                _shownIssues = kind == null ? _issues : _issues.Where(i => i.Kind == kind.Value).ToList();
                return;
            }

            _issuesSignature = signature;
            _issues = issues;
            RefreshIssueKinds();
            FillIssueList();
            StartFileCheck();
        }

        private List<DatasetIssue> FindIssues()
        {
            var issues = new List<DatasetIssue>();
            bool hasCondition = _schema.ConditionField != null;

            foreach (var image in _images.Where(i => !i.Matched))
            {
                issues.Add(new DatasetIssue
                {
                    Kind = IssueKind.SchemaMismatch,
                    Location = image.RelativeDir == "." ? "(thư mục gốc)" : image.RelativeDir,
                    File = image.FileName,
                    Detail = "Tên file không khớp schema " + _schema.Text + " - không đưa vào báo cáo.",
                    Image = image
                });
            }

            // Trùng tên: cùng khoá mẫu (Model / DK / SPL / hậu tố) + cùng loại Raw / Mask.
            foreach (var group in _images.Where(i => i.Matched && !i.Skipped).GroupBy(i => i.PairKey + "|" + i.Type, StringComparer.OrdinalIgnoreCase)
                         .Where(g => g.Count() > 1))
            {
                var list = group.ToList();
                foreach (var image in list)
                {
                    issues.Add(new DatasetIssue
                    {
                        Kind = IssueKind.Duplicate,
                        Location = SampleLocation(image.Values, image.Condition, image.Suffix),
                        File = RelativeName(image),
                        Detail = $"{list.Count} file {image.Type} cùng mẫu: "
                            + string.Join(", ", list.Where(o => o != image).Select(RelativeName)),
                        Image = image
                    });
                }
            }

            // Dataset chỉ có ảnh Raw (không có Mask nào) mà ảnh đã ghi <Đánh giá>: kết quả lấy từ chính ảnh Raw,
            // không phải thiếu Mask -> không báo "Thiếu Mask" cho các ảnh đó.
            bool rawOnly = !_images.Any(i => i.Matched && string.Equals(i.Type, "Mask", StringComparison.OrdinalIgnoreCase));

            foreach (var row in _rows)
            {
                foreach (var block in row.Blocks.Values.OrderBy(b => b.Condition, NaturalComparer.Instance))
                {
                    string location = SampleLocation(row.Values, hasCondition ? block.Condition : null, row.Suffix);

                    // Nhiều lần chụp: kiểm tra từng lần chụp (ghi rõ lần chụp nào thiếu).
                    var pairs = block.Shots != null
                        ? block.Shots.Select(s => new { s.Raw, s.Mask, Shot = block.Shots.Count > 1 && s.Index.Length > 0 ? " (lần chụp " + s.Index + ")" : string.Empty })
                        : new[] { new { block.Raw, block.Mask, Shot = string.Empty } };

                    foreach (var pair in pairs)
                    {
                        if (pair.Raw != null && pair.Mask == null && !(rawOnly && !string.IsNullOrEmpty(pair.Raw.FileEvaluation)))
                        {
                            issues.Add(new DatasetIssue
                            {
                                Kind = IssueKind.MissingMask,
                                Location = location + pair.Shot,
                                File = RelativeName(pair.Raw),
                                Detail = "Có Raw, chưa có Mask - chưa có kết quả Vision (push lại ảnh Raw sang UnitX để lấy Mask).",
                                Image = pair.Raw,
                                Row = row,
                                Condition = block.Condition
                            });
                        }
                        else if (pair.Mask != null && pair.Raw == null)
                        {
                            issues.Add(new DatasetIssue
                            {
                                Kind = IssueKind.MissingRaw,
                                Location = location + pair.Shot,
                                File = RelativeName(pair.Mask),
                                Detail = "Có Mask, không tìm thấy ảnh Raw cùng mẫu.",
                                Image = pair.Mask,
                                Row = row,
                                Condition = block.Condition
                            });
                        }
                    }

                    if (block.Mask == null)
                    {
                        continue;
                    }

                    string evaluation = BlockEvaluation(block);
                    if (evaluation.Length == 0)
                    {
                        issues.Add(new DatasetIssue
                        {
                            Kind = IssueKind.NoEvaluation,
                            Location = location,
                            File = RelativeName(block.Mask),
                            Detail = "Chưa có Đánh giá (Vision: " + (BlockUnitX(block).Length == 0 ? "trống - tên file thiếu kết quả" : LabelText(BlockUnitX(block))) + ").",
                            Image = block.Mask,
                            Row = row,
                            Condition = block.Condition
                        });
                    }
                }
            }

            if (hasCondition)
            {
                AddConditionIssues(issues);
            }

            foreach (var image in _images)
            {
                string error;
                if (_fileChecks.TryGetValue(FileCheckKey(image.FullPath), out error) && !string.IsNullOrEmpty(error))
                {
                    issues.Add(new DatasetIssue
                    {
                        Kind = IssueKind.BadFile,
                        Location = image.Matched ? SampleLocation(image.Values, image.Condition, image.Suffix) : image.RelativeDir,
                        File = RelativeName(image),
                        Detail = error,
                        Image = image
                    });
                }
            }

            return issues;
        }

        /// <summary>
        /// Thiếu / thừa DK: gom theo MẪU (các trường Key, bỏ hậu tố - các lần chụp khác giờ của cùng mẫu
        /// tính chung) rồi so các mẫu cùng Model (trường Key đầu tiên) với nhau:
        ///   - DK có ở ít nhất một nửa số mẫu = DK chuẩn; mẫu thiếu DK chuẩn -> "Thiếu DK".
        ///   - DK chỉ có ở dưới một nửa số mẫu -> "DK lạ" ở các mẫu có nó (thường do gõ sai tên DK).
        /// Model chỉ có một mẫu thì không so được.
        /// </summary>
        private void AddConditionIssues(List<DatasetIssue> issues)
        {
            var keyFields = _schema.KeyFields;
            if (keyFields.Count == 0)
            {
                return;
            }

            string modelField = keyFields[0].Name;
            var samples = _rows
                .GroupBy(r => string.Join("\u0001", keyFields.Select(f => r.Values[f.Name])), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Model = g.First().Values[modelField],
                    Rows = g.ToList(),
                    Conditions = new HashSet<string>(g.SelectMany(r => r.Blocks.Keys), StringComparer.OrdinalIgnoreCase)
                })
                .ToList();

            foreach (var model in samples.GroupBy(s => s.Model, StringComparer.OrdinalIgnoreCase))
            {
                var list = model.ToList();
                if (list.Count < 2)
                {
                    continue;
                }

                var counts = list.SelectMany(s => s.Conditions)
                    .GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
                var standard = counts.Where(kv => kv.Value * 2 >= list.Count).Select(kv => kv.Key)
                    .OrderBy(c => c, NaturalComparer.Instance).ToList();
                string scope = keyFields.Count > 1 ? $"mẫu cùng {modelField} {model.Key}" : "mẫu";

                foreach (var sample in list)
                {
                    var row = sample.Rows[0];
                    string location = SampleLocation(row.Values, null, null);

                    var missing = standard.Where(c => !sample.Conditions.Contains(c)).ToList();
                    if (missing.Count > 0)
                    {
                        issues.Add(new DatasetIssue
                        {
                            Kind = IssueKind.MissingCondition,
                            Location = location,
                            File = string.Empty,
                            Detail = "Thiếu " + string.Join(", ", missing) + " - "
                                + string.Join(", ", missing.Select(c => $"{c} có ở {counts[c]}/{list.Count} {scope}")) + ".",
                            Row = row
                        });
                    }

                    foreach (var extra in sample.Conditions.Where(c => counts[c] * 2 < list.Count).OrderBy(c => c, NaturalComparer.Instance))
                    {
                        var block = sample.Rows.Select(r => r.Block(extra)).FirstOrDefault(b => b != null);
                        issues.Add(new DatasetIssue
                        {
                            Kind = IssueKind.ExtraCondition,
                            Location = SampleLocation(row.Values, extra, null),
                            File = block != null && block.Main != null ? RelativeName(block.Main) : string.Empty,
                            Detail = $"{extra} chỉ có ở {counts[extra]}/{list.Count} {scope} - DK lạ hoặc gõ sai tên DK?",
                            Image = block != null ? block.Main : null,
                            Row = sample.Rows.FirstOrDefault(r => r.Block(extra) != null) ?? row,
                            Condition = extra
                        });
                    }
                }
            }
        }

        /// <summary>"Model / DK / SPL · hậu tố" theo thứ tự trường của schema.</summary>
        private string SampleLocation(IDictionary<string, string> values, string condition, string suffix)
        {
            var parts = new List<string>();
            foreach (var field in _schema.IdentityFields)
            {
                string value;
                if (field.Role == DatasetFieldRole.Condition)
                {
                    if (!string.IsNullOrEmpty(condition))
                    {
                        parts.Add(condition);
                    }
                }
                else if (values.TryGetValue(field.Name, out value) && !string.IsNullOrEmpty(value))
                {
                    parts.Add(value);
                }
            }

            string text = string.Join(" / ", parts);
            return string.IsNullOrEmpty(suffix) ? text : text + " · " + suffix;
        }

        private static string RelativeName(DatasetImage image)
        {
            return image.RelativeDir == "." ? image.FileName : Path.Combine(image.RelativeDir, image.FileName);
        }

        #endregion

        #region Kiểm tra file (nền)

        private static string FileCheckKey(string path)
        {
            try
            {
                var info = new FileInfo(path);
                return path + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks;
            }
            catch
            {
                return path;
            }
        }

        /// <summary>
        /// Đọc đầu / cuối từng file (chỉ vài byte) trên luồng nền: 0 byte, không phải ảnh, JPEG thiếu dấu kết
        /// thúc (ghi dở / copy lỗi). Chỉ kiểm tra file chưa có trong bộ nhớ đệm; xong thì tính lại danh sách.
        /// </summary>
        private void StartFileCheck()
        {
            if (_fileCheckRunning || _images.Count == 0)
            {
                return;
            }

            var pending = _images.Select(i => i.FullPath)
                .Select(p => new KeyValuePair<string, string>(p, FileCheckKey(p)))
                .Where(kv => !_fileChecks.ContainsKey(kv.Value))
                .ToList();
            if (pending.Count == 0)
            {
                return;
            }

            _fileCheckRunning = true;
            Task.Run(() =>
            {
                bool anyError = false;
                Parallel.ForEach(pending, new ParallelOptions { MaxDegreeOfParallelism = 4 }, kv =>
                {
                    string error = CheckImageFile(kv.Key);
                    _fileChecks[kv.Value] = error;
                    if (error.Length > 0)
                    {
                        anyError = true;
                    }
                });
                return anyError;
            }).ContinueWith(t =>
            {
                _fileCheckRunning = false;
                if (IsDisposed)
                {
                    return;
                }

                if (!t.IsFaulted && t.Result)
                {
                    _issuesSignature = null;
                    RefreshIssues();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>Vùng cuối file JPEG đọc để tìm dấu kết thúc FF D9.</summary>
        private const int JpegTailWindow = 64 * 1024;

        /// <summary>
        /// null = JPEG kết thúc đúng; ngược lại là mô tả lỗi. Hai trường hợp KHÔNG phải lỗi (trước đây báo nhầm vì chỉ
        /// tìm FF D9 trong 64 byte cuối):
        ///   - sau FF D9 còn dữ liệu đính kèm (vd camera ghi thêm 1 918 byte - E:\Dataset\Intop, 204 ảnh);
        ///   - sau FF D9 là byte 0 đệm.
        /// Lỗi thật: không có FF D9, thường kèm cả đoạn cuối toàn byte 0 - file được cấp đủ dung lượng nhưng chỉ ghi
        /// được một phần (vd MEIKO KenhPJ2\3: dữ liệu dừng đúng ở 1 MiB, 855 KB sau là 0). Trình xem ảnh vẫn mở được
        /// nhưng phần dưới ảnh thành sọc màu / xám.
        /// </summary>
        private static string CheckJpegEnd(Stream stream, long length)
        {
            // Bỏ đoạn byte 0 ở cuối (đọc lùi từng khối) để biết dữ liệu thật kết thúc ở đâu.
            long dataEnd = length;
            var block = new byte[JpegTailWindow];
            while (dataEnd > 0)
            {
                int size = (int)Math.Min(block.Length, dataEnd);
                stream.Seek(dataEnd - size, SeekOrigin.Begin);
                int got = ReadFully(stream, block, size);
                int i = got - 1;
                while (i >= 0 && block[i] == 0)
                {
                    i--;
                }

                if (i >= 0)
                {
                    dataEnd = dataEnd - size + i + 1;
                    break;
                }

                dataEnd -= size;
            }

            long zeros = length - dataEnd;

            // Tìm FF D9 trong vùng cuối của phần dữ liệu thật.
            int window = (int)Math.Min(JpegTailWindow, dataEnd);
            stream.Seek(dataEnd - window, SeekOrigin.Begin);
            int read = ReadFully(stream, block, window);
            for (int i = read - 2; i >= 0; i--)
            {
                if (block[i] == 0xFF && block[i + 1] == 0xD9)
                {
                    return null;
                }
            }

            if (zeros >= 4096)
            {
                return $"Ảnh JPEG hỏng: dữ liệu dừng ở byte {dataEnd:N0}, {zeros:N0} byte cuối ({zeros * 100.0 / length:0}% file) toàn số 0 "
                    + "- ghi / copy dở. Vẫn mở được nhưng phần dưới ảnh bị sọc / xám.";
            }

            return "Ảnh JPEG bị cắt cụt (thiếu dấu kết thúc - ghi dở hoặc copy lỗi). Vẫn mở được nhưng phần dưới ảnh có thể bị sọc / xám.";
        }

        private static bool IsAllZero(Stream stream)
        {
            var block = new byte[JpegTailWindow];
            stream.Seek(0, SeekOrigin.Begin);
            int n;
            while ((n = stream.Read(block, 0, block.Length)) > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    if (block[i] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int ReadFully(Stream stream, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = stream.Read(buffer, total, count - total);
                if (n <= 0)
                {
                    break;
                }

                total += n;
            }

            return total;
        }

        /// <summary>"" = file tốt; ngược lại là mô tả lỗi.</summary>
        private static string CheckImageFile(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    long length = stream.Length;
                    if (length == 0)
                    {
                        return "File rỗng (0 byte).";
                    }

                    var head = new byte[8];
                    int read = stream.Read(head, 0, head.Length);
                    string ext = Path.GetExtension(path).ToLowerInvariant();

                    bool jpeg = read >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
                    bool png = read >= 8 && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
                    bool bmp = read >= 2 && head[0] == 0x42 && head[1] == 0x4D;
                    bool tiff = read >= 4 && ((head[0] == 0x49 && head[1] == 0x49 && head[2] == 0x2A) || (head[0] == 0x4D && head[1] == 0x4D && head[3] == 0x2A));

                    if (!(jpeg || png || bmp || tiff))
                    {
                        // File có dung lượng nhưng toàn byte 0 (dữ liệu chưa từng được ghi - copy / đồng bộ lỗi), vd 8 ảnh
                        // MEIKO ĐK2: trình xem ảnh cũng không mở được, chỉ có thể hiện thumbnail cũ còn trong cache.
                        if (head.Take(read).All(x => x == 0) && IsAllZero(stream))
                        {
                            return $"File toàn byte 0 ({length:N0} byte, không có dữ liệu ảnh) - copy / đồng bộ lỗi, cần chép lại từ nguồn.";
                        }

                        return "Không phải file ảnh hợp lệ (đầu file không đúng định dạng " + ext + ").";
                    }

                    if (jpeg)
                    {
                        string damage = CheckJpegEnd(stream, length);
                        if (damage != null)
                        {
                            return damage;
                        }
                    }
                    else if (png)
                    {
                        // Chunk IEND ở cuối file PNG.
                        if (length < 20)
                        {
                            return "Ảnh PNG bị cắt cụt.";
                        }

                        var tail = new byte[12];
                        stream.Seek(-12, SeekOrigin.End);
                        stream.Read(tail, 0, 12);
                        if (!(tail[4] == 0x49 && tail[5] == 0x45 && tail[6] == 0x4E && tail[7] == 0x44))
                        {
                            return "Ảnh PNG bị cắt cụt (thiếu IEND - ghi dở hoặc copy lỗi).";
                        }
                    }
                }

                return string.Empty;
            }
            catch (FileNotFoundException)
            {
                return string.Empty;   // vừa bị xoá / đổi tên - lần quét sau tự cập nhật
            }
            catch (DirectoryNotFoundException)
            {
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "Không đọc được file: " + ex.Message;
            }
        }

        #endregion

        #region Danh sách + lọc

        /// <summary>Ô lọc loại: "Tất cả (N)" + từng loại đang có kèm số lượng, giữ lựa chọn cũ.</summary>
        private void RefreshIssueKinds()
        {
            IssueKind? selected = SelectedIssueKind();
            var counts = _issues.GroupBy(i => i.Kind).ToDictionary(g => g.Key, g => g.Count());

            cboIssueKind.BeginUpdate();
            try
            {
                cboIssueKind.Items.Clear();
                cboIssueKind.Items.Add(new IssueKindItem(null, $"Tất cả ({_issues.Count})"));
                int index = 0;
                foreach (IssueKind kind in Enum.GetValues(typeof(IssueKind)))
                {
                    int count;
                    if (!counts.TryGetValue(kind, out count))
                    {
                        continue;
                    }

                    cboIssueKind.Items.Add(new IssueKindItem(kind, $"{IssueKindTexts[(int)kind]} ({count})"));
                    if (selected == kind)
                    {
                        index = cboIssueKind.Items.Count - 1;
                    }
                }

                cboIssueKind.SelectedIndex = index;
            }
            finally
            {
                cboIssueKind.EndUpdate();
            }

            lblIssuesTitle.Text = _issues.Count == 0
                ? "✓ Không có vấn đề dữ liệu"
                : "⚠ Vấn đề dữ liệu: " + _issues.Count + "   (double-click để tới vị trí)";
            lblIssuesTitle.ForeColor = Theme.Fore(_issues.Count == 0 ? Color.FromArgb(22, 163, 74) : Color.FromArgb(180, 83, 9));
            UpdateIssuesCompact();   // không có vấn đề: khung chỉ còn dòng tiêu đề

            // Số vấn đề hiện ở thanh trạng thái (nút ẩn/hiện chỉ ghi "Vấn đề" - tránh hiểu nhầm "vấn đề số 3").
            stIssues.Text = _issues.Count == 0 ? "✓ Không có vấn đề" : "⚠ " + _issues.Count + " vấn đề";
            stIssues.ForeColor = _issues.Count == 0 ? Color.FromArgb(22, 163, 74) : Color.FromArgb(180, 83, 9);
            stIssues.Font = _issues.Count == 0 ? statusBar.Font : _statusBold;
        }

        private void SetupIssueStatus()
        {
            stIssues.Click += (s, e) =>
            {
                chkShowIssues.Checked = true;
                lvIssues.Focus();
            };
        }

        private sealed class IssueKindItem
        {
            public readonly IssueKind? Kind;
            private readonly string _text;

            public IssueKindItem(IssueKind? kind, string text)
            {
                Kind = kind;
                _text = text;
            }

            public override string ToString()
            {
                return _text;
            }
        }

        private IssueKind? SelectedIssueKind()
        {
            var item = cboIssueKind.SelectedItem as IssueKindItem;
            return item == null ? null : item.Kind;
        }

        private void FillIssueList()
        {
            var kind = SelectedIssueKind();
            var selected = lvIssues.SelectedIndices.Count > 0 && lvIssues.SelectedIndices[0] < _shownIssues.Count
                ? _shownIssues[lvIssues.SelectedIndices[0]]
                : null;

            _shownIssues = kind == null ? _issues : _issues.Where(i => i.Kind == kind.Value).ToList();

            lvIssues.BeginUpdate();
            try
            {
                lvIssues.SelectedIndices.Clear();
                lvIssues.VirtualListSize = _shownIssues.Count;

                if (selected != null)
                {
                    int index = _shownIssues.FindIndex(i => i.Kind == selected.Kind && i.File == selected.File && i.Location == selected.Location);
                    if (index >= 0)
                    {
                        lvIssues.SelectedIndices.Add(index);
                        lvIssues.EnsureVisible(index);
                    }
                }
            }
            finally
            {
                lvIssues.EndUpdate();
            }

            lvIssues.Invalidate();
        }

        #endregion

        #region Tới vị trí vấn đề

        private void GoToSelectedIssue()
        {
            if (lvIssues.SelectedIndices.Count == 0 || lvIssues.SelectedIndices[0] >= _shownIssues.Count)
            {
                return;
            }

            var issue = _shownIssues[lvIssues.SelectedIndices[0]];
            bool unmatched = issue.Image != null && !issue.Image.Matched;

            if (chkUnmatchedOnly.Checked != unmatched)
            {
                chkUnmatchedOnly.Checked = unmatched;
                _filterTimer.Stop();
                FillGrid();
            }

            if (TrySelectIssue(issue))
            {
                return;
            }

            // Ảnh / mẫu đang bị bộ lọc ẩn: bỏ hết bộ lọc rồi thử lại.
            ClearGridFilters();
            if (!TrySelectIssue(issue))
            {
                ShowStatusMessage("Không tìm thấy vị trí này trong bảng (ảnh có thể vừa bị xoá / đổi tên).", true);
                StatusResetSoon();
            }
            else
            {
                ShowStatusMessage("Đã bỏ bộ lọc để hiện vị trí vấn đề.");
                StatusResetSoon();
            }
        }

        private bool TrySelectIssue(DatasetIssue issue)
        {
            if (issue.Image != null && SelectImageCell(issue.Image))
            {
                ShowPreview();
                return true;
            }

            if (issue.Row == null)
            {
                return false;
            }

            foreach (DataGridViewRow gridRow in grid.Rows)
            {
                // So theo khoá hàng: bảng dựng lại (lọc, tải lại) thì đối tượng DatasetRow là bản mới.
                var row = RowOf(gridRow);
                if (row == null || row.Unmatched != null || !string.Equals(row.Key, issue.Row.Key, StringComparison.OrdinalIgnoreCase)
                    || (issue.Condition != null && row.Block(issue.Condition) == null && issue.Kind != IssueKind.MissingCondition))
                {
                    continue;
                }

                var column = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Visible && BlockOf(c) != null
                        && BlockOf(c).Part == BlockPart.UnitX
                        && (issue.Condition == null || string.Equals(BlockOf(c).Condition, issue.Condition, StringComparison.OrdinalIgnoreCase)))
                    ?? grid.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
                if (column == null)
                {
                    return false;
                }

                grid.ClearSelection();
                grid.CurrentCell = gridRow.Cells[column.Index];
                grid.CurrentCell.Selected = true;
                ShowPreview();
                return true;
            }

            return false;
        }

        /// <summary>Bỏ mọi bộ lọc của bảng: ô Lọc, DK, UnitX, Evaluation, lọc cột ▼, nút đang chọn trên cây.</summary>
        private void ClearGridFilters()
        {
            _fillingGrid = true;
            try
            {
                txtFilter.Text = string.Empty;
                cboDkFilter.SelectedIndex = 0;
                cboUnitXFilter.SelectedIndex = 0;
                cboEvalFilter.SelectedIndex = 0;
            }
            finally
            {
                _fillingGrid = false;
            }

            _columnFilters.Clear();
            _treeKeyFilter.Clear();
            lblTreeTitle.Text = TreeTitleDefault;
            if (tvDataset.Nodes.Count > 0 && tvDataset.SelectedNode != tvDataset.Nodes[0])
            {
                tvDataset.SelectedNode = tvDataset.Nodes[0];   // AfterSelect -> ApplyTreeFilter(gốc)
            }

            UpdateBlockVisibility();
            _filterTimer.Stop();
            FillGrid();
            grid.Invalidate();
        }

        #endregion

        #region Xuất danh sách

        private void ExportIssues()
        {
            if (_shownIssues.Count == 0)
            {
                MessageBox.Show(this, "Không có vấn đề nào để xuất.", "Vấn đề dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string target;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dialog.FileName = "Dataset_Issues_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                if (_loadedFolder != null && Directory.Exists(_loadedFolder))
                {
                    dialog.InitialDirectory = _loadedFolder;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                target = dialog.FileName;
            }

            var rows = _shownIssues.Select(i => new[]
            {
                new XlsxCell { Text = IssueKindTexts[(int)i.Kind] },
                new XlsxCell { Text = i.Location },
                new XlsxCell
                {
                    Text = i.File,
                    Link = i.Image != null ? i.Image.FullPath : null
                },
                new XlsxCell { Text = i.Detail }
            }).ToList();

            try
            {
                XlsxWriter.Write(target, "Vấn đề", new[] { "Loại", "Vị trí", "File", "Mô tả" }, rows,
                    new[] { 16.0, 30.0, 50.0, 80.0 });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không ghi được file (đang mở trong Excel?):" + Environment.NewLine + ex.Message,
                    "Lỗi xuất danh sách", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show(this, $"Đã xuất {rows.Count} vấn đề ra:{Environment.NewLine}{target}{Environment.NewLine}{Environment.NewLine}Mở file ngay?",
                    "Xuất danh sách vấn đề", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không mở được file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion
    }
}
