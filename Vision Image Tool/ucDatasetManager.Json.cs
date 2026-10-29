using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerJsonCodeFile { }

    /// <summary>
    /// dataset.json - dữ liệu BẢNG cho web (chỉ xem): các cột Key, và mỗi DK có Evaluation / ghi chú / đường dẫn
    /// ảnh Raw - Mask (tương đối, dấu "/"). Web không cần hiểu schema - chỉ đọc file này.
    ///
    /// Hai file, hai vai trò:
    ///   - dataset_evaluation.json : phần NGƯỜI DÙNG chỉnh tay (nguồn gốc, bản app cũ vẫn đọc được).
    ///   - dataset.json            : sinh tự động từ dữ liệu trên (~2 giây sau mỗi thay đổi, chỉ ghi khi nội
    ///                               dung đổi) + nút "Xuất JSON" để ghi ngay / lưu ra chỗ khác.
    /// </summary>
    public partial class ucDatasetManager
    {
        public const string DatasetJsonFileName = "dataset.json";
        private const int DatasetJsonVersion = 2;

        private readonly Button btnExportJson = new Button();
        private Timer _jsonTimer;
        private string _lastJsonHash;
        private string _lastJsonFolder;

        private Control BuildJsonButton()
        {
            StyleButton(btnExportJson, "{ } Xuất JSON", false);
            btnExportJson.Dock = DockStyle.Fill;
            btnExportJson.Margin = new Padding(0, 6, 8, 6);
            btnExportJson.ForeColor = Primary;
            btnExportJson.Click += (s, e) => ExportDatasetJson();
            new ToolTip { AutoPopDelay = 15000 }.SetToolTip(btnExportJson,
                "dataset.json (dữ liệu bảng cho web) được TỰ ĐỘNG cập nhật trong thư mục ảnh sau mỗi thay đổi."
                + Environment.NewLine + "Bấm để ghi ngay, hoặc lưu thêm một bản ra chỗ khác.");
            return btnExportJson;
        }

        #region Tự cập nhật

        /// <summary>Hẹn ghi dataset.json ~2 giây sau thay đổi cuối (gọi từ UpdateSummary).</summary>
        private void ScheduleDatasetJson()
        {
            if (_loadedFolder == null)
            {
                return;
            }

            if (_jsonTimer == null)
            {
                _jsonTimer = new Timer { Interval = 2000 };
                _jsonTimer.Tick += (s, e) =>
                {
                    _jsonTimer.Stop();
                    if (_loading || _loadedFolder == null || _images.Count == 0)
                    {
                        return;
                    }

                    string error;
                    WriteDatasetJson(Path.Combine(_loadedFolder, DatasetJsonFileName), true, out error);
                    if (error != null)
                    {
                        ShowStatusMessage("KHÔNG ghi được " + DatasetJsonFileName + ": " + error, true);
                        StatusResetSoon();
                    }

                    // Bản đầy đủ (dataset_collection.json) cập nhật cùng lúc - bỏ qua nếu nội dung không đổi.
                    WriteCollectionNow(false);
                };
            }

            _jsonTimer.Stop();
            _jsonTimer.Start();
        }

        private void ExportDatasetJson()
        {
            if (_loadedFolder == null || _images.Count == 0)
            {
                MessageBox.Show(this, "Chưa quét thư mục ảnh nào.", "Xuất JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string target;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON (*.json)|*.json";
                dialog.FileName = DatasetJsonFileName;
                dialog.InitialDirectory = _loadedFolder;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                target = dialog.FileName;
            }

            string error;
            if (!WriteDatasetJson(target, false, out error))
            {
                MessageBox.Show(this, "Không ghi được file:" + Environment.NewLine + error, "Xuất JSON", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowStatusMessage("Đã xuất " + target);
            StatusResetSoon();

            if (MessageBox.Show(this, "Đã xuất dữ liệu bảng ra:" + Environment.NewLine + target
                    + Environment.NewLine + Environment.NewLine + "Mở thư mục chứa file JSON?",
                    "Xuất JSON", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                ImageFileOps.ShowInExplorer(new[] { target });
            }
        }

        /// <summary>
        /// Ghi dataset.json. <paramref name="skipIfSame"/>: nội dung (không tính mốc thời gian) giống lần ghi
        /// trước thì thôi - tránh ghi đĩa liên tục. Ghi ra file tạm rồi thay thế để web không bao giờ đọc
        /// phải file ghi dở.
        /// </summary>
        private bool WriteDatasetJson(string path, bool skipIfSame, out string error)
        {
            error = null;
            try
            {
                var json = BuildDatasetJson(Path.GetDirectoryName(Path.GetFullPath(path)));
                string hash = Hash(json.ToString(Formatting.None));
                if (skipIfSame && hash == _lastJsonHash && string.Equals(_lastJsonFolder, _loadedFolder, StringComparison.OrdinalIgnoreCase)
                    && File.Exists(path))
                {
                    return true;
                }

                json["generatedAt"] = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                string temp = path + ".tmp";
                File.WriteAllText(temp, json.ToString(Formatting.Indented), new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    File.Replace(temp, path, null);
                }
                else
                {
                    File.Move(temp, path);
                }

                if (skipIfSame)
                {
                    _lastJsonHash = hash;
                    _lastJsonFolder = _loadedFolder;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string Hash(string text)
        {
            using (var sha = SHA1.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }

        #endregion

        #region Nạp lại cấu hình của thư mục

        /// <summary>
        /// Mở một thư mục KHÁC thư mục đang mở: có dataset.json (settings) thì dùng lại chế độ schema, schema và
        /// logic đã lưu cùng thư mục đó. Trả về thông báo cho thanh trạng thái, null nếu không đổi gì.
        /// Trước khi rời thư mục cũ, ghi ngay dataset.json đang chờ để thư mục cũ nhớ đúng cấu hình mới nhất.
        /// </summary>
        private string RestoreDatasetSettings(string folder)
        {
            if (_loadedFolder != null && string.Equals(_loadedFolder, folder, StringComparison.OrdinalIgnoreCase))
            {
                return null;   // tải lại cùng thư mục: giữ cấu hình đang dùng
            }

            FlushDatasetJson();

            JObject settings;
            try
            {
                string path = Path.Combine(folder, DatasetJsonFileName);
                if (!File.Exists(path) && !File.Exists(Path.Combine(folder, EvaluationFileName)))
                {
                    return null;
                }

                // Cấu hình lưu trong dataset_collection.json; thư mục của bản trước có thể chỉ có dataset.json.
                string collection = Path.Combine(folder, EvaluationFileName);
                settings = (File.Exists(collection) ? JObject.Parse(File.ReadAllText(collection))["settings"] : null) as JObject
                    ?? JObject.Parse(File.ReadAllText(path))["settings"] as JObject;
            }
            catch
            {
                return null;   // file hỏng / bản cũ: bỏ qua, dùng cấu hình hiện tại
            }

            string schema = settings == null ? null : (string)settings["schema"];
            if (string.IsNullOrWhiteSpace(schema))
            {
                return null;
            }

            bool folderMode = string.Equals((string)settings["schemaMode"], "folder", StringComparison.OrdinalIgnoreCase);
            string logic = (string)settings["logic"];
            schema = NormalizeSchemaText(schema);

            bool changed = folderMode != _folderMode || !string.Equals(schema, txtSchema.Text.Trim(), StringComparison.Ordinal)
                || (logic != null && logic != GlobalRule);
            if (!changed)
            {
                return null;
            }

            _images.Clear();   // ảnh của thư mục cũ không cần bóc lại theo schema mới
            txtSchema.Text = schema;   // cách đọc (tên file / thư mục + tên) tự suy theo schema trong ApplySchema
            ApplySchema(false);
            if (logic == CompareMatch || logic == CompareDiff)
            {
                SetCompareMode(logic == CompareMatch ? CompareMode.Same : CompareMode.Inverted, false);
            }

            SaveSettings();
            return "Đã dùng lại cấu hình đã lưu của thư mục (dataset.json): " + (folderMode ? "thư mục + tên file" : "theo tên file")
                + " · " + schema + (logic != null ? " · logic Đánh giá " + logic + " Vision" : string.Empty);
        }

        /// <summary>Ghi ngay dataset.json đang chờ (bộ hẹn giờ 2 giây) của thư mục đang mở.</summary>
        private void FlushDatasetJson()
        {
            FlushCollection();
            if (_jsonTimer == null || !_jsonTimer.Enabled || _loadedFolder == null || _images.Count == 0)
            {
                return;
            }

            _jsonTimer.Stop();
            string error;
            WriteDatasetJson(Path.Combine(_loadedFolder, DatasetJsonFileName), true, out error);
            WriteCollectionNow(false);
        }

        #endregion

        #region Nội dung

        /// <summary>
        /// Chỉ dữ liệu của BẢNG (web chỉ xem):
        ///   { version, generatedAt, name, [imageBase], columns: [trường Key...], conditions: [DK...],
        ///     rows: [ { "&lt;cột&gt;": giá trị..., "&lt;DK&gt;": { evaluation, comment, raw, mask } } ] }
        /// Không có Vision / Đối soát / hậu tố - hàng phân biệt nhau bằng đường dẫn ảnh Raw / Mask.
        /// </summary>
        /// <param name="jsonFolder">Thư mục chứa file JSON - lưu ra ngoài thư mục ảnh thì thêm imageBase.</param>
        private JObject BuildDatasetJson(string jsonFolder)
        {
            // Cột theo thứ tự đã xác nhận ở hộp thoại Xuất (kéo thả) - cả bản xuất tay lẫn dataset.json tự cập nhật,
            // để bản tự cập nhật không ghi đè mất thứ tự người dùng đã chọn.
            var columns = ExportInfoFields().Where(f => f.Role == DatasetFieldRole.Key).Select(f => f.Name).ToList();
            var conditions = _conditions.Select(BlockName).ToList();
            bool singleShot = ExportSingleShot();

            var rows = new JArray();
            foreach (var row in _rows)
            {
                var o = new JObject();
                foreach (var column in columns)
                {
                    string value;
                    row.Values.TryGetValue(column, out value);
                    o[column] = value ?? string.Empty;
                }

                foreach (var condition in _conditions)
                {
                    var block = row.Block(condition);
                    if (block == null)
                    {
                        continue;
                    }

                    // Bỏ <Lần chụp> khi xuất: ảnh đầu tiên theo tên file; không thì lần chụp đại diện (Vision xấu nhất).
                    var pair = singleShot ? FirstShotPair(block) : new[] { block.Raw, block.Mask };
                    var blockJson = new JObject
                    {
                        ["evaluation"] = ExportLabel(BlockEvaluation(block)),
                        ["comment"] = BlockComment(block),
                        ["raw"] = pair[0] == null ? null : WebPath(RelativeName(pair[0])),
                        ["mask"] = pair[1] == null ? null : WebPath(RelativeName(pair[1]))
                    };

                    // Nhiều lần chụp: raw / mask ở trên là lần chụp đại diện (Vision xấu nhất); "shots" liệt kê
                    // đủ các lần chụp theo thứ tự.
                    if (!singleShot && block.Shots != null && block.Shots.Count > 1)
                    {
                        blockJson["shots"] = new JArray(block.Shots.Select(s => new JObject
                        {
                            ["raw"] = s.Raw == null ? null : WebPath(RelativeName(s.Raw)),
                            ["mask"] = s.Mask == null ? null : WebPath(RelativeName(s.Mask))
                        }));
                    }

                    o[BlockName(condition)] = blockJson;
                }

                rows.Add(o);
            }

            var json = new JObject
            {
                ["version"] = DatasetJsonVersion,
                ["generatedAt"] = null,
                ["name"] = Path.GetFileName(_loadedFolder.TrimEnd(Path.DirectorySeparatorChar))
            };

            // Đường dẫn ảnh tính từ thư mục ảnh: file JSON nằm chỗ khác thì ghi thêm đường dẫn tới thư mục ảnh.
            string imageBase = RelativeUrl(jsonFolder, _loadedFolder);
            if (imageBase.Length > 0)
            {
                json["imageBase"] = imageBase;
            }

            json["columns"] = new JArray(columns);
            json["conditions"] = new JArray(conditions);
            json["rows"] = rows;
            return json;
        }

        /// <summary>Tên khối DK trong hàng; schema không có DK thì một khối tên "values".</summary>
        private string BlockName(string condition)
        {
            return _schema.ConditionField == null || string.IsNullOrEmpty(condition) ? "values" : condition;
        }

        /// <summary>Đường dẫn cho web: dấu "/" thay "\".</summary>
        private static string WebPath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        /// <summary>Đường dẫn tương đối (dạng URL, kết thúc "/") từ thư mục JSON tới thư mục ảnh ("" = cùng thư mục).</summary>
        private static string RelativeUrl(string fromFolder, string toFolder)
        {
            string from = Path.GetFullPath(fromFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string to = Path.GetFullPath(toFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var relative = new Uri(from).MakeRelativeUri(new Uri(to));
            if (relative.IsAbsoluteUri)
            {
                return to.Replace('\\', '/');   // khác ổ đĩa: đành ghi đường dẫn tuyệt đối
            }

            return Uri.UnescapeDataString(relative.ToString());
        }

        #endregion
    }
}
