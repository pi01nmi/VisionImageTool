using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace VisionImageTool
{
    /// <summary>
    /// Cấu hình sản xuất đọc từ UnitX (get_prod_configuration): phần struct cố định (networks,
    /// controllers, cameras, các bảng ánh xạ) + imaging_config (JSON) đã tách thành cây theo Part Type.
    /// </summary>
    internal sealed class ProdConfigData
    {
        public bool Ok;
        public string Error;

        public List<KeyValuePair<string, string>> Networks = new List<KeyValuePair<string, string>>();
        public List<string> Controllers = new List<string>();
        public List<string> Cameras = new List<string>();
        public Dictionary<string, string> CameraController = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<string>> CameraCaptureConfigs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<string>> CaptureNetworks = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public string ImagingConfig;

        /// <summary>imaging_config gộp theo Part Type (một production config có thể có nhiều part type).</summary>
        public List<PartTypeConfig> PartTypes = new List<PartTypeConfig>();

        /// <summary>Lỗi đọc imaging_config (không phải JSON...), null nếu đọc được.</summary>
        public string ImagingConfigError;

        /// <summary>Mọi camera: từ struct + từ imaging_config.</summary>
        public List<string> AllCameras
        {
            get
            {
                return Cameras
                    .Concat(CameraCaptureConfigs.Keys)
                    .Concat(PartTypes.SelectMany(p => p.Sources).Select(s => s.CameraId))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        /// <summary>
        /// Mọi capture config: từ các bảng ánh xạ của struct + cc_network_mapping trong imaging_config
        /// (UnitX có thể chỉ điền một trong hai chỗ - log 2026-09-26: struct trống, cc nằm trong JSON).
        /// </summary>
        public List<string> AllCaptureConfigs
        {
            get
            {
                return CameraCaptureConfigs.Values.SelectMany(v => v)
                    .Concat(CaptureNetworks.Keys)
                    .Concat(PartTypes.SelectMany(p => p.Sources).SelectMany(s => s.CaptureNetworks.Select(c => c.Key)))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        public List<string> AllPartTypes
        {
            get { return PartTypes.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList(); }
        }

        /// <summary>
        /// Tách imaging_config thành cây theo Part Type. Dạng hãng dùng:
        ///   [{"part_type": "...", "part_config": [{"source": "camera", "camera_id": "...",
        ///     "cc_network_mapping": {"cc": ["network"]}, "repeat": 100, ...}]}]
        /// Part type trùng tên được gộp làm một; khoá lạ (controller, width, height...) giữ lại trong Extra.
        /// </summary>
        public void ParseImagingConfig()
        {
            PartTypes.Clear();
            ImagingConfigError = null;

            if (string.IsNullOrWhiteSpace(ImagingConfig))
            {
                return;
            }

            JToken root;
            try
            {
                root = JToken.Parse(ImagingConfig);
            }
            catch (Exception ex)
            {
                ImagingConfigError = ex.Message;
                return;
            }

            IEnumerable<JToken> entries = root is JArray ? (IEnumerable<JToken>)root.Children() : new[] { root };

            foreach (var entry in entries.OfType<JObject>())
            {
                string name = entry.Value<string>("part_type") ?? "(không có part_type)";
                var part = PartTypes.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal));
                if (part == null)
                {
                    part = new PartTypeConfig { Name = name };
                    PartTypes.Add(part);
                }

                foreach (var property in entry.Properties())
                {
                    if (property.Name == "part_type")
                    {
                        continue;
                    }

                    if (property.Name == "part_config" && property.Value is JArray)
                    {
                        foreach (var source in property.Value.Children().OfType<JObject>())
                        {
                            part.Sources.Add(ParseSource(source));
                        }
                    }
                    else
                    {
                        part.Extra.Add(new KeyValuePair<string, JToken>(property.Name, property.Value));
                    }
                }
            }
        }

        private static PartSource ParseSource(JObject json)
        {
            var source = new PartSource();

            foreach (var property in json.Properties())
            {
                switch (property.Name)
                {
                    case "source":
                        source.Source = property.Value.ToString();
                        break;
                    case "camera_id":
                        source.CameraId = property.Value.ToString();
                        break;
                    case "repeat":
                        source.Repeat = property.Value.ToString();
                        break;
                    case "cc_network_mapping":
                        var mapping = property.Value as JObject;
                        if (mapping != null)
                        {
                            foreach (var cc in mapping.Properties())
                            {
                                var networks = cc.Value is JArray
                                    ? cc.Value.Children().Select(n => n.ToString()).ToList()
                                    : new List<string> { cc.Value.ToString() };
                                source.CaptureNetworks.Add(new KeyValuePair<string, List<string>>(cc.Name, networks));
                            }
                        }
                        else
                        {
                            source.Extra.Add(new KeyValuePair<string, JToken>(property.Name, property.Value));
                        }

                        break;
                    default:
                        source.Extra.Add(new KeyValuePair<string, JToken>(property.Name, property.Value));
                        break;
                }
            }

            return source;
        }
    }

    internal sealed class PartTypeConfig
    {
        public string Name;
        public List<PartSource> Sources = new List<PartSource>();
        public List<KeyValuePair<string, JToken>> Extra = new List<KeyValuePair<string, JToken>>();
    }

    /// <summary>Một nguồn ảnh của part type (một phần tử part_config).</summary>
    internal sealed class PartSource
    {
        public string Source;
        public string CameraId;
        public string Repeat;

        /// <summary>cc_network_mapping: capture config -> các network.</summary>
        public List<KeyValuePair<string, List<string>>> CaptureNetworks = new List<KeyValuePair<string, List<string>>>();

        /// <summary>Các khoá khác (controller, image width / height...).</summary>
        public List<KeyValuePair<string, JToken>> Extra = new List<KeyValuePair<string, JToken>>();
    }
}
