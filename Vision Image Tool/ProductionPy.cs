using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VisionImageTool
{
    /// <summary>
    /// Cấu hình imaging của UnitX hiển thị theo dạng người dùng sửa trên máy UnitX: production.py
    /// (literal Python - True/False/None, dấu phẩy cuối dòng, chú thích "# ←" ở chỗ cần thêm / sửa).
    ///
    /// Kèm bộ dựng cấu hình ĐỀ XUẤT để push ảnh ngoài: part type đang chọn phải có một nguồn
    /// source = "external_image" với image_width / image_height đúng bằng kích thước ảnh sẽ push.
    /// </summary>
    internal static class ProductionPy
    {
        public const string ExternalSource = "external_image";

        /// <summary>Cấu hình hiện tại dạng production.py.</summary>
        public static string Format(ProdConfigData prod)
        {
            JToken root = ParseRoot(prod);
            var sb = new StringBuilder();
            sb.AppendLine("# production.py - cấu hình imaging HIỆN TẠI đọc từ UnitX");
            if (root == null)
            {
                sb.AppendLine("# (trống - UnitX không khai báo part type nào)");
                return sb.ToString();
            }

            Write(sb, root, 0, new Dictionary<JToken, string>());
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Cấu hình đề xuất: giữ nguyên mọi thứ đang có, chỉ thêm / sửa đúng phần cần cho việc push
        /// ảnh ngoài của <paramref name="partType"/>. Chỗ thay đổi được đánh dấu "# ← ...".
        /// </summary>
        public static string Suggest(ProdConfigData prod, string partType, string captureConfig, Size imageSize)
        {
            var root = ParseRoot(prod) as JArray ?? new JArray();
            var comments = new Dictionary<JToken, string>();

            var part = root.OfType<JObject>().FirstOrDefault(o => (string)o["part_type"] == partType);
            if (part == null)
            {
                part = new JObject { ["part_type"] = partType, ["part_config"] = new JArray() };
                root.Add(part);
                comments[part] = "← thêm mới part type";
            }

            var configs = part["part_config"] as JArray;
            if (configs == null)
            {
                configs = new JArray();
                part["part_config"] = configs;
            }

            var sources = configs.OfType<JObject>().Where(o => (string)o["source"] == ExternalSource).ToList();
            var entry = sources.FirstOrDefault(o => o["cc_network_mapping"] is JObject
                && ((JObject)o["cc_network_mapping"]).Property(captureConfig) != null) ?? sources.FirstOrDefault();

            if (entry == null)
            {
                entry = new JObject
                {
                    ["source"] = ExternalSource,
                    ["camera_id"] = ExternalSource,
                    ["cc_network_mapping"] = new JObject { [captureConfig] = new JArray(NetworksOf(prod, root, captureConfig).ToArray()) },
                    ["repeat"] = 1,
                    ["image_width"] = imageSize.Width,
                    ["image_height"] = imageSize.Height
                };
                configs.Add(entry);
                comments[entry] = "← thêm nguồn ảnh ngoài (dùng cho Push)";
                comments[entry.Property("camera_id")] = "← đặt ID nguồn ảnh ngoài theo UnitX";
            }
            else
            {
                SetSize(entry, "image_width", imageSize.Width, comments);
                SetSize(entry, "image_height", imageSize.Height, comments);

                var mapping = entry["cc_network_mapping"] as JObject;
                if (mapping == null)
                {
                    mapping = new JObject();
                    entry["cc_network_mapping"] = mapping;
                }

                if (mapping.Property(captureConfig) == null && !string.IsNullOrEmpty(captureConfig))
                {
                    mapping[captureConfig] = new JArray(NetworksOf(prod, root, captureConfig).ToArray());
                    comments[mapping.Property(captureConfig)] = "← thêm capture config đang chọn";
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("# production.py - cấu hình ĐỀ XUẤT để push ảnh ngoài cho Part Type '" + partType + "'");
            sb.AppendLine("# Ảnh push: " + imageSize.Width + " x " + imageSize.Height + " px. Dòng đánh dấu ← (nền vàng) là chỗ cần thêm / sửa trên UnitX.");
            Write(sb, root, 0, comments);
            sb.AppendLine();
            return sb.ToString();
        }

        private static void SetSize(JObject entry, string key, int value, Dictionary<JToken, string> comments)
        {
            var property = entry.Property(key);
            if (property == null)
            {
                entry[key] = value;
                comments[entry.Property(key)] = "← thêm";
            }
            else if (property.Value.Type != JTokenType.Integer || (int)property.Value != value)
            {
                string old = property.Value.ToString(Formatting.None);
                property.Value = value;
                comments[property] = "← sửa (đang là " + old + ")";
            }
        }

        /// <summary>Network của capture config: lấy ở chỗ khác trong cấu hình nếu có, không thì để chỗ trống cần điền.</summary>
        private static IEnumerable<string> NetworksOf(ProdConfigData prod, JArray root, string captureConfig)
        {
            List<string> networks;
            if (prod != null && prod.CaptureNetworks.TryGetValue(captureConfig ?? string.Empty, out networks) && networks.Count > 0)
            {
                return networks;
            }

            var found = root.SelectTokens("$..cc_network_mapping").OfType<JObject>()
                .Select(m => m.Property(captureConfig ?? string.Empty))
                .Where(p => p != null && p.Value is JArray)
                .SelectMany(p => p.Value.Select(v => v.ToString()))
                .Distinct()
                .ToList();

            return found.Count > 0 ? found : new List<string> { "<network>" };
        }

        private static JToken ParseRoot(ProdConfigData prod)
        {
            if (prod == null || string.IsNullOrWhiteSpace(prod.ImagingConfig))
            {
                return null;
            }

            try
            {
                var token = JToken.Parse(prod.ImagingConfig);
                return token is JObject ? new JArray(token) : token;
            }
            catch
            {
                return null;
            }
        }

        #region Ghi literal Python

        private static void Write(StringBuilder sb, JToken token, int indent, Dictionary<JToken, string> comments)
        {
            string pad = new string(' ', indent);
            string inner = new string(' ', indent + 4);

            var obj = token as JObject;
            var array = token as JArray;

            if (obj != null)
            {
                sb.Append("{").Append(Comment(comments, obj)).AppendLine();
                foreach (var property in obj.Properties())
                {
                    sb.Append(inner).Append(Quote(property.Name)).Append(": ");
                    WriteMember(sb, property.Value, indent + 4, comments, property);
                }

                sb.Append(pad).Append("}");
            }
            else if (array != null)
            {
                // Mảng giá trị đơn (network...) viết trên một dòng cho gọn.
                if (array.All(v => v is JValue))
                {
                    sb.Append("[").Append(string.Join(", ", array.Select(Scalar))).Append("]");
                    return;
                }

                sb.Append("[").Append(Comment(comments, array)).AppendLine();
                foreach (var item in array)
                {
                    sb.Append(inner);
                    WriteMember(sb, item, indent + 4, comments, null);
                }

                sb.Append(pad).Append("]");
            }
            else
            {
                sb.Append(Scalar(token));
            }
        }

        private static void WriteMember(StringBuilder sb, JToken value, int indent, Dictionary<JToken, string> comments, JProperty property)
        {
            bool container = value is JObject || (value is JArray && !((JArray)value).All(v => v is JValue));
            Write(sb, value, indent, comments);
            sb.Append(",");

            // Chú thích của giá trị đơn nằm cuối dòng; của object / mảng nằm sau dấu mở ngoặc.
            if (!container && property != null)
            {
                sb.Append(Comment(comments, property));
            }

            sb.AppendLine();
        }

        private static string Comment(Dictionary<JToken, string> comments, JToken token)
        {
            string comment;
            return comments.TryGetValue(token, out comment) ? "  # " + comment : string.Empty;
        }

        private static string Scalar(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return "None";
                case JTokenType.Boolean:
                    return (bool)token ? "True" : "False";
                case JTokenType.Integer:
                case JTokenType.Float:
                    return Convert.ToString(((JValue)token).Value, CultureInfo.InvariantCulture);
                default:
                    return Quote(token.ToString());
            }
        }

        private static string Quote(string text)
        {
            return JsonConvert.ToString(text ?? string.Empty);
        }

        #endregion
    }
}
