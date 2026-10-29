using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VisionImageTool
{
    /// <summary>Vai trò của một trường trong schema tên file.</summary>
    internal enum DatasetFieldRole
    {
        /// <summary>Trường định danh mẫu (Model, Condition, SPL...) - dùng để gom Raw/Mask thành một hàng.</summary>
        Key,

        /// <summary>Raw / Mask. Không bắt buộc có trong tên: ảnh raw do app lưu không có chữ "Raw".</summary>
        Type,

        /// <summary>Kết quả OK / NG (UnitX) hoặc O / Δ / X (người dùng) nằm sẵn trong tên.</summary>
        Evaluation,

        /// <summary>Hậu tố (timestamp / số thứ tự) để phân biệt các lần chụp cùng một mẫu.</summary>
        Suffix,

        /// <summary>
        /// Điều kiện chụp (DK). Bắt buộc như trường định danh, nhưng KHÔNG tách hàng: các ảnh chỉ khác
        /// DK nằm chung một hàng, mỗi DK một block cột Raw | Mask | UnitX | Evaluation.
        /// </summary>
        Condition,

        /// <summary>
        /// Lần chụp (số thứ tự / timestamp ở cuối tên). Khác Hậu tố: KHÔNG tách hàng - mọi lần chụp của cùng
        /// mẫu + DK gộp vào một block, Vision = phép nhân logic (X > Δ > O), một Đánh giá chung. Giá trị
        /// dùng để sắp xếp các ảnh và ghép Raw ↔ Mask cùng lần chụp.
        /// </summary>
        Shot
    }

    internal sealed class DatasetField
    {
        public string Name { get; set; }
        public DatasetFieldRole Role { get; set; }
        public string GroupName { get; set; }

        /// <summary>Chuỗi phân cách đứng NGAY TRƯỚC trường này trong schema (vd "_") - dùng để vẽ lại schema.</summary>
        public string LiteralBefore { get; set; }

        /// <summary>
        /// Trường "Tên file" tự thêm khi schema theo thư mục kết thúc bằng dấu  (không mô tả tên file): lấy
        /// tên file bỏ _raw / _mask / _OK / _NG làm tên mẫu để ghép Raw ↔ Mask.
        /// </summary>
        public bool Implicit { get; set; }

        /// <summary>Trường là một CẤP THƯ MỤC (có dấu \ đứng sau nó), không nằm trong tên file.</summary>
        public bool InFolder { get; set; }
    }

    internal sealed class DatasetParseResult
    {
        public bool Success { get; set; }

        /// <summary>Giá trị các trường Key theo đúng thứ tự trong schema.</summary>
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Type { get; set; }
        public string Evaluation { get; set; }
        public string Suffix { get; set; }

        /// <summary>Giá trị trường Lần chụp ("" nếu không có).</summary>
        public string Shot { get; set; }

        /// <summary>Kết quả _OK / _NG / _LIMIT đọc từ tên file (trường Tên file tự đọc), null nếu không có.</summary>
        public string FileDecision { get; set; }

        /// <summary>Giá trị trường DK ("" nếu schema không có trường DK).</summary>
        public string Condition { get; set; }
    }

    /// <summary>
    /// Schema tên file do người dùng tự định nghĩa, ví dụ
    ///     &lt;Model&gt;_&lt;Condition&gt;_&lt;SPL&gt;_&lt;Raw/Mask&gt;_&lt;Evaluation&gt;_&lt;Hậu tố&gt;.jpg
    /// được dịch thành một biểu thức chính quy để bóc tách tên file.
    ///
    /// Vai trò mỗi trường suy ra từ TÊN trường (không phân biệt hoa thường / dấu):
    ///   - chứa "raw", "mask", "loai", "type"            -> Type        (tuỳ chọn trong tên file)
    ///   - chứa "eval", "danh gia", "ket qua", "result"  -> Evaluation  (tuỳ chọn)
    ///   - chứa "lan chup", "shot"                       -> Shot        (tuỳ chọn, gộp nhiều ảnh vào 1 hàng)
    ///   - chứa "hau to", "suffix", "timestamp"          -> Suffix      (tuỳ chọn)
    ///   - "DK", "dieu kien", "condition"                 -> Condition   (bắt buộc, mỗi DK một block)
    ///   - còn lại                                        -> Key         (bắt buộc)
    ///   - "Bỏ qua", "*", "Ignore"                        -> không phải trường: phần tên bỏ qua, không dùng để ghép
    /// Trường tuỳ chọn được phép vắng mặt cùng dấu phân cách đứng trước nó, nhờ vậy cùng một schema
    /// đọc được cả MODEL1_DK1_VITRI1_OK_... (raw) lẫn MODEL1_DK1_VITRI1_mask_OK_... (mask).
    /// </summary>
    internal sealed class DatasetSchema
    {
        public const string DefaultText = "<Model>_<DK>_<SPL>_<Raw/Mask>_<Đánh giá>_<Hậu tố>";

        /// <summary>Schema mẫu duy nhất trong menu 🕘 (mỗi cấp thư mục một trường, tên file là Raw / Mask).</summary>
        public const string SampleText = @"<ĐK>\<No>\<Tên sản phẩm>\<Mã sheet-tray>\<Vị trí>\<Mặt>\<Nội dung NG>\<Đánh giá>\<Raw/Mask>.<Extension>";

        private static readonly Regex TokenRegex = new Regex(@"<([^<>]+)>");

        private Regex _regex;

        public string Text { get; private set; }
        public IList<DatasetField> Fields { get; private set; }

        public IList<DatasetField> KeyFields
        {
            get { return Fields.Where(f => f.Role == DatasetFieldRole.Key).ToList(); }
        }

        public bool HasSuffix
        {
            get { return Fields.Any(f => f.Role == DatasetFieldRole.Suffix); }
        }

        /// <summary>
        /// Phần đứng sau trường cuối (vd "_1", "_20261009") là phần không quan tâm - bỏ qua khi đọc tên. Chỉ khi trường
        /// cuối có tập giá trị cố định (&lt;Đánh giá&gt; / &lt;Raw/Mask&gt;) để không cắt nhầm trường tự do. Nhiều ảnh chỉ khác
        /// nhau ở phần này = cùng một mẫu: lấy ảnh đầu tiên theo tên file (xem ucDatasetManager.BuildRows).
        /// </summary>
        public bool IgnoresTail { get; private set; }

        /// <summary>Schema có &lt;Lần chụp&gt;: nhiều ảnh cùng mẫu + DK gộp vào một hàng.</summary>
        public bool HasShot
        {
            get { return Fields.Any(f => f.Role == DatasetFieldRole.Shot); }
        }

        /// <summary>Trường DK (điều kiện chụp), null nếu schema không có.</summary>
        public DatasetField ConditionField
        {
            get { return Fields.FirstOrDefault(f => f.Role == DatasetFieldRole.Condition); }
        }

        /// <summary>Các trường định danh MỘT ẢNH (Key + DK) theo thứ tự trong schema - để ghép Raw ↔ Mask.</summary>
        public IList<DatasetField> IdentityFields
        {
            get { return Fields.Where(f => f.Role == DatasetFieldRole.Key || f.Role == DatasetFieldRole.Condition).ToList(); }
        }

        /// <summary>Dịch chuỗi schema. Trả về null và thông báo lỗi nếu schema không dùng được.</summary>
        public static DatasetSchema Parse(string text, out string error)
        {
            error = null;
            string schema = (text ?? string.Empty).Trim();

            // Phần mở rộng (.jpg/.png) trong schema chỉ để minh hoạ - so khớp luôn bỏ phần mở rộng.
            int lastToken = schema.LastIndexOf('>');
            int dot = schema.LastIndexOf('.');
            if (dot > lastToken)
            {
                schema = schema.Substring(0, dot);
            }

            // <Extension> ở cuối (nút "+ Extension"): đuôi ảnh, tự nhận mọi định dạng - chỉ để hiển thị.
            var extension = ExtensionTokenRegex.Match(schema);
            bool hasExtension = extension.Success;
            if (hasExtension)
            {
                schema = schema.Substring(0, extension.Index);
            }

            // Kết thúc bằng dấu gạch chéo ngược: đoạn tên file không mô tả -> tự đọc (xem DatasetField.Implicit).
            bool implicitFileName = schema.EndsWith("\\", StringComparison.Ordinal);
            if (implicitFileName)
            {
                schema = schema.TrimEnd('\\');
            }

            var matches = TokenRegex.Matches(schema);
            if (matches.Count == 0)
            {
                error = "Schema chưa có trường nào. Mỗi trường đặt trong dấu <>, ví dụ <Model>_<SPL>.";
                return null;
            }

            var fields = new List<DatasetField>();
            var fieldEnds = new List<int>();
            var pattern = new StringBuilder("^");
            int position = 0;

            // <Bỏ qua> / <*>: phần tên có mặt nhưng không dùng để ghép (vd tên file Raw và Mask khác hẳn nhau trong
            // cùng thư mục mẫu). Không tạo trường; chữ của nó được ghép vào LiteralBefore của trường kế tiếp.
            string skipped = string.Empty;

            // Trường cuối là <Đánh giá> / <Raw/Mask> (tập giá trị cố định) và schema không ghi gì sau nó: phần còn lại
            // của tên (bắt đầu bằng "_" / "-", vd "_1", "_20261009_1530") là phần không quan tâm. Phần đuôi chỉ được
            // nhận SAU KHI trường cuối đã khớp (nằm trong cùng nhóm tuỳ chọn) - các trường tự do phía trước vẫn phải
            // "nhường" cho trường cuối như trước, không bị cắt sớm ("Bọt khí", "Xước_nhẹ" vẫn đọc đủ).
            var tailToken = matches[matches.Count - 1];
            var lastRole = DetectRole(tailToken.Groups[1].Value.Trim());
            bool ignoresTail = !implicitFileName
                && (lastRole == DatasetFieldRole.Evaluation || lastRole == DatasetFieldRole.Type)
                && schema.Substring(tailToken.Index + tailToken.Length).Trim().Length == 0;
            const string TailPattern = @"(?:[_\-][^\\]*)?";

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                string literal = schema.Substring(position, m.Index - position);
                string name = m.Groups[1].Value.Trim();

                if (name.Length == 0)
                {
                    error = "Có trường rỗng <> trong schema.";
                    return null;
                }

                if (IsSkipToken(name))
                {
                    pattern.Append(Regex.Escape(literal)).Append(i == matches.Count - 1 ? @"[^\\]+" : @"[^\\]+?");
                    skipped += literal + m.Value;
                    position = m.Index + m.Length;
                    continue;
                }

                if (fields.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    error = "Trường <" + name + "> bị lặp lại.";
                    return null;
                }

                var role = DetectRole(name);
                if (role != DatasetFieldRole.Key && fields.Any(f => f.Role == role))
                {
                    error = "Schema chỉ được có một trường loại " + role + " (trường <" + name + ">).";
                    return null;
                }

                if ((role == DatasetFieldRole.Shot && fields.Any(f => f.Role == DatasetFieldRole.Suffix))
                    || (role == DatasetFieldRole.Suffix && fields.Any(f => f.Role == DatasetFieldRole.Shot)))
                {
                    error = "Chỉ dùng một trong <Hậu tố> (mỗi lần chụp một hàng) hoặc <Lần chụp> (gộp các lần chụp vào một hàng).";
                    return null;
                }

                var field = new DatasetField { Name = name, Role = role, GroupName = "f" + i, LiteralBefore = skipped + literal };
                fields.Add(field);
                fieldEnds.Add(m.Index + m.Length);
                skipped = string.Empty;

                bool isLast = i == matches.Count - 1;
                string body;
                switch (role)
                {
                    case DatasetFieldRole.Type:
                        body = "(?i:raw|mask)";
                        break;
                    case DatasetFieldRole.Evaluation:
                        body = "(?i:OK|NG|LIMIT|O|X|Δ)";
                        break;
                    // Một trường không bao giờ vượt qua dấu \ (schema theo thư mục: mỗi cấp thư mục một trường).
                    case DatasetFieldRole.Suffix:
                    case DatasetFieldRole.Shot:
                        body = isLast ? @"[^\\]+" : @"[^\\]+?";
                        break;
                    default:
                        body = @"[^\\]+?";
                        break;
                }

                string group = "(?<" + field.GroupName + ">" + body + ")";
                if (isLast && ignoresTail)
                {
                    group += TailPattern;
                }

                if (role == DatasetFieldRole.Key || role == DatasetFieldRole.Condition)
                {
                    pattern.Append(Regex.Escape(literal)).Append(group);
                }
                else
                {
                    pattern.Append("(?:").Append(Regex.Escape(literal)).Append(group).Append(")?");
                }

                position = m.Index + m.Length;
            }

            pattern.Append(Regex.Escape(schema.Substring(position)));

            // Tên file tự đọc: thêm trường "Tên file" (vai trò hậu tố) nếu schema chưa có hậu tố, không thì
            // chỉ cần có một tên file bất kỳ ở cuối.
            if (implicitFileName)
            {
                if (fields.Any(f => f.Role == DatasetFieldRole.Suffix))
                {
                    pattern.Append(@"\\[^\\]+");
                }
                else
                {
                    var fileField = new DatasetField { Name = "Tên file", Role = DatasetFieldRole.Suffix, GroupName = "file", LiteralBefore = "\\", Implicit = true };
                    fields.Add(fileField);
                    pattern.Append(@"\\(?<file>[^\\]+)");
                }
            }

            pattern.Append("$");

            // Kết hợp thư mục + tên file: trường nào có dấu \ đứng sau là cấp thư mục, còn lại thuộc tên file.
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].InFolder = !fields[i].Implicit
                    && (i >= fieldEnds.Count || schema.Substring(fieldEnds[i]).Contains("\\") || implicitFileName);
            }

            if (!fields.Any(f => f.Role == DatasetFieldRole.Key))
            {
                error = "Schema cần ít nhất một trường định danh (ví dụ <Model>) để gom Raw/Mask.";
                return null;
            }

            string original = (text ?? string.Empty).Trim();
            return new DatasetSchema
            {
                Text = original,
                Fields = fields,
                TrailingLiteral = fields.Any(f => f.Implicit) ? string.Empty
                    : hasExtension ? skipped + schema.Substring(position)
                    : skipped + schema.Substring(position) + original.Substring(Math.Min(original.Length, schema.Length)),
                _regex = new Regex(pattern.ToString(), RegexOptions.CultureInvariant),
                HasExtension = hasExtension,
                IgnoresTail = ignoresTail
            };
        }

        private static readonly Regex ExtensionTokenRegex = new Regex(@"\.?<\s*(?:extension|ext|đuôi|duoi)\s*>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>Schema có &lt;Extension&gt; ở cuối (đuôi ảnh tự nhận .jpg / .png / .bmp / .tif...).</summary>
        public bool HasExtension { get; private set; }

        /// <summary>Phần đứng sau trường cuối cùng (ký tự phân cách + phần mở rộng, vd ".jpg").</summary>
        public string TrailingLiteral { get; private set; }

        /// <summary>Gợi ý ngắn cho trường có ý nghĩa cố định; trường tên gọi / phân cấp (Key) không cần gợi ý.</summary>
        public static string RoleHint(DatasetFieldRole role)
        {
            switch (role)
            {
                case DatasetFieldRole.Type: return "Raw | Mask · có thể vắng";
                case DatasetFieldRole.Evaluation: return "OK→O · NG→X · LIMIT→Δ";
                case DatasetFieldRole.Suffix: return "timestamp / số thứ tự";
                case DatasetFieldRole.Condition: return "mỗi DK một block";
                case DatasetFieldRole.Shot: return "gộp 1 hàng · Vision = NG×…×OK";
                default: return null;
            }
        }

        /// <summary>Bóc tách một tên file (không cần bỏ phần mở rộng).</summary>
        public DatasetParseResult Match(string fileName)
        {
            return MatchText(System.IO.Path.GetFileNameWithoutExtension(fileName ?? string.Empty));
        }

        /// <summary>
        /// Schema theo thư mục: bóc tách đường dẫn TƯƠNG ĐỐI (thư mục con + tên file, bỏ phần mở rộng),
        /// vd "Model1\DK1\NG\T1\P1\raw\OK\0001".
        /// </summary>
        public DatasetParseResult MatchPath(string relativePath)
        {
            string path = (relativePath ?? string.Empty).Replace('/', '\\');
            string extension = System.IO.Path.GetExtension(path);
            return MatchText(extension.Length > 0 ? path.Substring(0, path.Length - extension.Length) : path);
        }

        /// <summary>Có dấu \ trong schema = schema theo thư mục.</summary>
        public bool IsFolderSchema
        {
            get { return Fields.Any(f => (f.LiteralBefore ?? string.Empty).Contains("\\")); }
        }

        private DatasetParseResult MatchText(string stem)
        {
            var result = new DatasetParseResult();

            var m = _regex.Match(stem);
            if (!m.Success)
            {
                return result;
            }

            result.Success = true;
            result.Condition = string.Empty;

            foreach (var field in Fields)
            {
                var group = m.Groups[field.GroupName];
                string value = group.Success ? group.Value : string.Empty;

                switch (field.Role)
                {
                    case DatasetFieldRole.Type:
                        result.Type = value.Length == 0 ? null : NormalizeType(value);
                        break;
                    case DatasetFieldRole.Evaluation:
                        result.Evaluation = ToSymbol(value);
                        break;
                    case DatasetFieldRole.Suffix:
                        if (field.Implicit)
                        {
                            ReadFileName(value, result);
                        }
                        else
                        {
                            result.Suffix = value;
                        }

                        break;
                    case DatasetFieldRole.Condition:
                        result.Condition = value;
                        result.Values[field.Name] = value;
                        break;
                    case DatasetFieldRole.Shot:
                        result.Shot = value;
                        break;
                    default:
                        result.Values[field.Name] = value;
                        break;
                }
            }

            return result;
        }

        /// <summary>Kết quả UnitX / nhãn trong tên file -> ký hiệu đánh giá O / Δ / X ("" nếu không có).</summary>
        public static string ToSymbol(string value)
        {
            switch ((value ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "OK":
                case "O":
                    return "O";
                case "NG":
                case "X":
                    return "X";
                case "LIMIT":
                case "Δ":
                    return "Δ";
                default:
                    return string.Empty;
            }
        }

        private static readonly Regex FileTagRegex = new Regex(@"^(?<base>.*?)(?:_?(?<type>raw|mask)(?:_\d+)?)?(?:_(?<dec>OK|NG|LIMIT))?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Tên file tự đọc: bỏ _raw / _mask[_N] / _OK / _NG / _LIMIT ở cuối -> tên mẫu (hậu tố), để
        /// "X_01_raw_OK" và "X_01_mask_OK" ghép cùng một mẫu. Loại / kết quả đọc được dùng khi schema
        /// không có trường tương ứng.
        /// </summary>
        private void ReadFileName(string name, DatasetParseResult result)
        {
            var m = FileTagRegex.Match(name);
            string baseName = m.Success && m.Groups["base"].Value.Length > 0 ? m.Groups["base"].Value : name;
            result.Suffix = baseName;

            if (m.Success && m.Groups["type"].Success && result.Type == null)
            {
                result.Type = NormalizeType(m.Groups["type"].Value);
            }

            if (m.Success && m.Groups["dec"].Success)
            {
                result.FileDecision = m.Groups["dec"].Value.ToUpperInvariant();
                if (!Fields.Any(f => f.Role == DatasetFieldRole.Evaluation))
                {
                    result.Evaluation = ToSymbol(result.FileDecision);
                }
            }
        }

        private static string NormalizeType(string value)
        {
            return value.StartsWith("m", StringComparison.OrdinalIgnoreCase) ? "Mask" : "Raw";
        }

        /// <summary>&lt;Bỏ qua&gt; / &lt;*&gt; / &lt;Ignore&gt;: phần tên không quan tâm, không dùng để ghép Raw ↔ Mask.</summary>
        private static bool IsSkipToken(string name)
        {
            string n = Simplify(name);
            return name.Trim() == "*" || n == "boqua" || n == "ignore" || n == "skip";
        }

        private static DatasetFieldRole DetectRole(string name)
        {
            string n = Simplify(name);

            if (n == "dk" || n.Contains("dieukien") || n.Contains("condition"))
            {
                return DatasetFieldRole.Condition;
            }

            if (n.Contains("raw") || n.Contains("mask") || n == "loai" || n.Contains("type"))
            {
                return DatasetFieldRole.Type;
            }

            if (n.Contains("eval") || n.Contains("danhgia") || n.Contains("ketqua") || n.Contains("result")
                || n.Contains("judge"))
            {
                return DatasetFieldRole.Evaluation;
            }

            if (n.Contains("lanchup") || n == "shot" || n.Contains("shotindex"))
            {
                return DatasetFieldRole.Shot;
            }

            if (n.Contains("hauto") || n.Contains("suffix") || n.Contains("timestamp"))
            {
                return DatasetFieldRole.Suffix;
            }

            return DatasetFieldRole.Key;
        }

        /// <summary>Chữ thường, bỏ dấu tiếng Việt, bỏ mọi ký tự không phải chữ/số.</summary>
        private static string Simplify(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                char lower = char.ToLowerInvariant(c == 'đ' || c == 'Đ' ? 'd' : c);
                if (char.IsLetterOrDigit(lower))
                {
                    sb.Append(lower);
                }
            }

            return sb.ToString();
        }
    }
}
