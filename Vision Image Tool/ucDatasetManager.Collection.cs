using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerCollectionCodeFile { }

    /// <summary>
    /// dataset_collection.json (thay dataset_evaluation.json của các bản trước) - TOÀN BỘ dữ liệu của thư mục,
    /// không lược bỏ:
    ///   - labels   : nhãn chỉnh tay / đối soát chọn riêng theo từng ảnh ("raw\x.jpg" -> O / Δ / X / == / !=).
    ///                Đây là NGUỒN GỐC - app đọc lại phần này khi mở thư mục.
    ///   - settings : chế độ schema, schema, logic -> mở lại thư mục là dùng lại đúng cấu hình.
    ///   - dataset / summary / rows / unmatched / issues : ảnh chụp đầy đủ bảng (Vision, Đánh giá, Đối soát,
    ///                nguồn nhãn, hậu tố, ghi chú, đường dẫn...) cho tra cứu / công cụ khác.
    /// dataset.json vẫn là bản RÚT GỌN chỉ dữ liệu bảng cho web.
    ///
    /// Ghi: ~0,3 giây sau mỗi lần chỉnh nhãn (gộp khi bấm liên tục), ~2 giây sau các thay đổi khác, ghi ngay
    /// khi đổi thư mục / đóng app. File tạm rồi thay thế - không bao giờ để lại file ghi dở.
    /// </summary>
    public partial class ucDatasetManager
    {
        private const string CollectionFormat = "vision-image-tool/collection";
        private const int CollectionVersion = 1;

        private Timer _collectionTimer;
        private string _lastCollectionHash;
        private string _lastCollectionFolder;

        /// <summary>Thư mục vừa mở còn dataset_evaluation.json cũ: ghi collection xong thì đổi tên file cũ thành .bak.</summary>
        private bool _legacyLabelsPending;

        #region Đọc / ghi nhãn

        /// <summary>Ký hiệu "giới hạn" khi XUẤT ra Excel / JSON (trong app vẫn hiện Δ).</summary>
        internal const string ExportDelta = "▲";

        /// <summary>Nhãn trong app -> nhãn xuất file: Δ -> ▲, còn lại giữ nguyên.</summary>
        internal static string ExportLabel(string label)
        {
            return label == "Δ" ? ExportDelta : label;
        }

        /// <summary>Nhãn đọc từ file -> nhãn trong app: ▲ / △ -> Δ (file cũ ghi Δ vẫn đọc được).</summary>
        internal static string ImportLabel(string label)
        {
            return label == ExportDelta || label == "△" ? "Δ" : label;
        }

        /// <summary>Nhãn chỉnh tay của thư mục: dataset_collection.json (labels), không có thì dataset_evaluation.json cũ.</summary>
        private Dictionary<string, string> LoadEvaluations(string folder)
        {
            _legacyLabelsPending = false;
            try
            {
                string path = Path.Combine(folder, EvaluationFileName);
                if (File.Exists(path))
                {
                    var labels = JObject.Parse(File.ReadAllText(path))["labels"] as JObject;
                    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (labels != null)
                    {
                        foreach (var property in labels.Properties())
                        {
                            result[property.Name] = ImportLabel((string)property.Value ?? string.Empty);
                        }
                    }

                    return result;
                }

                string legacy = Path.Combine(folder, LegacyEvaluationFileName);
                if (File.Exists(legacy))
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(legacy));
                    _legacyLabelsPending = true;
                    return data != null
                        ? data.ToDictionary(kv => kv.Key, kv => ImportLabel(kv.Value ?? string.Empty), StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không đọc được nhãn (" + EvaluationFileName + "): " + ex.Message
                    + Environment.NewLine + "Các nhãn thủ công sẽ bắt đầu lại từ đầu.",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Nhãn vừa đổi: ghi dataset_collection.json sau ~0,3 giây (bấm phím liên tục chỉ ghi một lần).</summary>
        private void SaveEvaluations()
        {
            if (_loadedFolder == null)
            {
                return;
            }

            if (_collectionTimer == null)
            {
                _collectionTimer = new Timer { Interval = 300 };
                _collectionTimer.Tick += (s, e) =>
                {
                    _collectionTimer.Stop();
                    WriteCollectionNow(true);
                };
                Disposed += (s, e) => FlushCollection();
            }

            _collectionTimer.Stop();
            _collectionTimer.Start();
        }

        /// <summary>Ghi ngay collection đang chờ (đổi thư mục, đóng app).</summary>
        private void FlushCollection()
        {
            if (_collectionTimer != null && _collectionTimer.Enabled)
            {
                _collectionTimer.Stop();
                WriteCollectionNow(true);
            }
        }

        private void WriteCollectionNow(bool reportError)
        {
            if (_loadedFolder == null || _loading)
            {
                return;
            }

            string error;
            if (!WriteCollection(_loadedFolder, out error) && reportError && !IsDisposed)
            {
                ShowStatusMessage("KHÔNG lưu được nhãn vào " + EvaluationFileName + ": " + error, true);
                StatusResetSoon();
            }
        }

        /// <summary>Ghi dataset_collection.json của thư mục (bỏ qua nếu nội dung không đổi so với lần trước).</summary>
        private bool WriteCollection(string folder, out string error)
        {
            error = null;
            string path = Path.Combine(folder, EvaluationFileName);
            try
            {
                var json = BuildCollectionJson();
                string hash = Hash(json.ToString(Formatting.None));
                if (hash == _lastCollectionHash && string.Equals(_lastCollectionFolder, folder, StringComparison.OrdinalIgnoreCase) && File.Exists(path))
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

                _lastCollectionHash = hash;
                _lastCollectionFolder = folder;

                // Đã có collection: file nhãn cũ đổi thành .bak (giữ lại phòng khi cần, app không đọc nữa).
                string legacy = Path.Combine(folder, LegacyEvaluationFileName);
                if (_legacyLabelsPending && File.Exists(legacy))
                {
                    string backup = legacy + ".bak";
                    if (File.Exists(backup))
                    {
                        File.Delete(backup);
                    }

                    File.Move(legacy, backup);
                }

                _legacyLabelsPending = false;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        #endregion

        #region Nội dung đầy đủ

        private JObject BuildCollectionJson()
        {
            var blocks = _rows.SelectMany(r => r.Blocks.Values).ToList();

            var fields = new JArray(_schema.Fields.Select(f =>
            {
                var o = new JObject { ["name"] = f.Name, ["role"] = RoleCode(f.Role) };
                if (f.Implicit)
                {
                    o["implicit"] = true;
                }

                return o;
            }));

            var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var rows = new JArray();
            foreach (var row in _rows)
            {
                int seen;
                ids.TryGetValue(row.Key, out seen);
                ids[row.Key] = seen + 1;

                var keys = new JObject();
                foreach (var field in _schema.KeyFields)
                {
                    string value;
                    row.Values.TryGetValue(field.Name, out value);
                    keys[field.Name] = value ?? string.Empty;
                }

                var blockObject = new JObject();
                foreach (var condition in _conditions)
                {
                    var block = row.Block(condition);
                    if (block != null)
                    {
                        blockObject[BlockName(condition)] = FullBlockJson(block);
                    }
                }

                rows.Add(new JObject
                {
                    ["id"] = seen == 0 ? row.Key : row.Key + "#" + (seen + 1),
                    ["keys"] = keys,
                    ["suffix"] = row.Suffix ?? string.Empty,
                    ["blocks"] = blockObject
                });
            }

            // Nhãn chỉnh tay theo thứ tự đường dẫn - diff giữa các lần lưu dễ đọc.
            var labels = new JObject();
            foreach (var kv in _evaluations.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            {
                labels[kv.Key] = ExportLabel(kv.Value);
            }

            return new JObject
            {
                ["format"] = CollectionFormat,
                ["version"] = CollectionVersion,
                ["generatedAt"] = null,
                ["settings"] = new JObject
                {
                    ["schemaMode"] = _folderMode ? "folder" : "name",
                    ["schema"] = _schema.Text,
                    ["logic"] = GlobalRule
                },
                ["labels"] = labels,
                ["dataset"] = new JObject
                {
                    ["name"] = Path.GetFileName(_loadedFolder.TrimEnd(Path.DirectorySeparatorChar)),
                    ["root"] = _loadedFolder,
                    ["fields"] = fields,
                    ["conditions"] = new JArray(_conditions.Select(BlockName))
                },
                ["summary"] = new JObject
                {
                    ["images"] = _images.Count,
                    ["unmatchedImages"] = _images.Count(i => !i.Matched),
                    ["rows"] = _rows.Count,
                    ["blocks"] = blocks.Count,
                    ["vision"] = CountBy(blocks, BlockUnitX, new[] { "O", ExportDelta, "X", "" }),
                    ["evaluation"] = CountBy(blocks, BlockEvaluation, new[] { "O", ExportDelta, "X", "" }),
                    ["compare"] = CountBy(blocks, BlockCompare, new[] { CompareMatch, CompareDiff, "" }),
                    ["issues"] = _issues.Count
                },
                ["rows"] = rows,
                ["unmatched"] = new JArray(_images.Where(i => !i.Matched).Select(i => WebPath(RelativeName(i)))),
                ["issues"] = new JArray(_issues.Select(i => new JObject
                {
                    ["kind"] = char.ToLowerInvariant(i.Kind.ToString()[0]) + i.Kind.ToString().Substring(1),
                    ["kindText"] = IssueKindTexts[(int)i.Kind],
                    ["location"] = i.Location,
                    ["file"] = WebPath(i.File),
                    ["detail"] = i.Detail
                }))
            };
        }

        private JObject FullBlockJson(DatasetBlock block)
        {
            string stored = BlockStored(block);
            var o = new JObject
            {
                ["vision"] = ExportLabel(BlockUnitX(block)),
                ["evaluation"] = ExportLabel(BlockEvaluation(block)),
                ["compare"] = BlockCompare(block),
                ["source"] = stored == null ? (BlockUnitX(block).Length > 0 ? "logic" : "none") : IsRule(stored) ? "rule" : "manual",
                ["comment"] = BlockComment(block),
                ["raw"] = FullImageJson(block.Raw),
                ["mask"] = FullImageJson(block.Mask)
            };

            // <Lần chụp>: raw / mask = lần chụp đại diện; vision ở trên = phép nhân logic của "shots".
            if (block.Shots != null)
            {
                o["shots"] = new JArray(block.Shots.Select(s => new JObject
                {
                    ["shot"] = s.Index,
                    ["vision"] = ExportLabel(ShotVision(s)),
                    ["raw"] = FullImageJson(s.Raw),
                    ["mask"] = FullImageJson(s.Mask)
                }));
            }

            return o;
        }

        private JToken FullImageJson(DatasetImage image)
        {
            if (image == null)
            {
                return JValue.CreateNull();
            }

            var o = new JObject
            {
                ["path"] = WebPath(RelativeName(image)),
                ["vision"] = ExportLabel(UnitXValue(image)),
                ["evaluation"] = ExportLabel(EffectiveEvaluation(image))
            };

            string stored = StoredValue(image);
            if (stored != null)
            {
                o["label"] = ExportLabel(stored);
            }

            string comment = ImageComments.Get(image.FullPath);
            if (!string.IsNullOrEmpty(comment))
            {
                o["comment"] = comment;
            }

            return o;
        }

        private static JObject CountBy(IEnumerable<DatasetBlock> blocks, Func<DatasetBlock, string> value, IEnumerable<string> keys)
        {
            var counts = keys.ToDictionary(k => k, k => 0);
            foreach (var block in blocks)
            {
                string v = ExportLabel(value(block) ?? string.Empty);
                int n;
                counts.TryGetValue(v, out n);
                counts[v] = n + 1;
            }

            var o = new JObject();
            foreach (var kv in counts)
            {
                o[kv.Key] = kv.Value;
            }

            return o;
        }

        private static string RoleCode(DatasetFieldRole role)
        {
            switch (role)
            {
                case DatasetFieldRole.Type: return "type";
                case DatasetFieldRole.Evaluation: return "evaluation";
                case DatasetFieldRole.Suffix: return "suffix";
                case DatasetFieldRole.Condition: return "condition";
                case DatasetFieldRole.Shot: return "shot";
                default: return "key";
            }
        }

        #endregion
    }
}
