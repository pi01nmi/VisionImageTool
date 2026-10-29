using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code. Nếu class đầu tiên là partial frmMainV1, VS tự gắn SubType=Form rồi mở Designer - Designer
    // không thấy lớp cha ("base class System.Void") và đã từng ghi đè frmMainV1.Designer.cs thành form
    // trống (2026-09-25). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1StatusCodeFile { }

    /// <summary>
    /// frmMainV1 - nút "Kiểm tra trạng thái": chạy toàn bộ chẩn đoán rồi đưa ra KẾT LUẬN cho từng
    /// hạng mục trong cửa sổ frmStatusV1, thay vì đổ log cho người dùng tự đọc.
    ///
    /// Log chi tiết vẫn được ghi đầy đủ như trước (RunDiagnostics / ReadSystemStatus /
    /// ReadProdConfiguration / RunNetworkCheckAsync) để khi cần vẫn tra được.
    /// </summary>
    public partial class frmMainV1
    {
        /// <summary>Lỗi UnitX gửi về trong khoảng này thì vẫn coi là "đang xảy ra".</summary>
        private static readonly TimeSpan RecentErrorWindow = TimeSpan.FromMinutes(10);

        private bool _statusCheckRunning;

        #region Thanh trạng thái

        /// <summary>Trạng thái một thành phần trên thanh trạng thái - quyết định màu chấm và màu chữ.</summary>
        private enum ComponentState
        {
            Unknown = 0,
            Ok = 1,
            Warning = 2,
            Error = 3
        }

        private ComponentState _unitXState;
        private ComponentState _cameraState;
        private ComponentState _sdkState;
        private string _unitXText = "Chưa kết nối";

        // Kết quả lần "Kiểm tra trạng thái" gần nhất (null = chưa kiểm tra hoặc đã có thay đổi sau đó).
        private int? _checkErrors;
        private int? _checkWarnings;

        private static readonly Dictionary<ComponentState, Bitmap> StateDots = new Dictionary<ComponentState, Bitmap>();

        private static Color DotColor(ComponentState state)
        {
            switch (state)
            {
                case ComponentState.Ok: return Color.FromArgb(22, 163, 74);
                case ComponentState.Warning: return Color.FromArgb(245, 158, 11);
                case ComponentState.Error: return Color.FromArgb(220, 38, 38);
                default: return Color.FromArgb(156, 163, 175);
            }
        }

        private static Color TextColor(ComponentState state)
        {
            switch (state)
            {
                case ComponentState.Ok: return Color.FromArgb(21, 128, 61);
                case ComponentState.Warning: return Color.FromArgb(180, 83, 9);
                case ComponentState.Error: return Color.FromArgb(185, 28, 28);
                default: return Color.FromArgb(107, 114, 128);
            }
        }

        /// <summary>Chấm tròn màu trạng thái (vẽ bằng code - không cần file icon trong resource).</summary>
        private static Bitmap Dot(ComponentState state)
        {
            Bitmap dot;
            if (!StateDots.TryGetValue(state, out dot))
            {
                dot = new Bitmap(10, 10);
                using (var g = Graphics.FromImage(dot))
                using (var brush = new SolidBrush(DotColor(state)))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(brush, 0.5f, 0.5f, 9f, 9f);
                }

                StateDots[state] = dot;
            }

            return dot;
        }

        private void SetupStatusBar()
        {
            foreach (var title in new[] { tsslUnitXTitle, tsslCameraTitle, tsslSdkTitle })
            {
                title.ImageScaling = ToolStripItemImageScaling.None;
                title.TextImageRelation = TextImageRelation.ImageBeforeText;
            }

            ApplyComponentState(tsslUnitXTitle, tsslUnitX, ComponentState.Unknown, "Chưa kết nối");
            ApplyComponentState(tsslCameraTitle, tsslCamera, ComponentState.Unknown, "Chưa rõ");
            ApplyComponentState(tsslSdkTitle, tsslSdk, ComponentState.Unknown, "Chưa rõ");
            UpdateOverallState();
        }

        private void SetUnitXState(ComponentState state, string text)
        {
            RunOnUi(() =>
            {
                _unitXState = state;
                _unitXText = text;
                ApplyComponentState(tsslUnitXTitle, tsslUnitX, state, text);
            });
        }

        private void SetCameraState(ComponentState state, string text)
        {
            RunOnUi(() =>
            {
                _cameraState = state;
                ApplyComponentState(tsslCameraTitle, tsslCamera, state, text);
            });
        }

        private void SetSdkState(ComponentState state, string text)
        {
            RunOnUi(() =>
            {
                _sdkState = state;
                ApplyComponentState(tsslSdkTitle, tsslSdk, state, text);
            });
        }

        private void ApplyComponentState(ToolStripStatusLabel title, ToolStripStatusLabel value, ComponentState state, string text)
        {
            title.Image = Dot(state);
            value.Text = text;
            value.ForeColor = TextColor(state);

            // Thành phần vừa đổi trạng thái -> kết quả kiểm tra cũ không còn đúng nữa.
            _checkErrors = null;
            _checkWarnings = null;
            UpdateOverallState();
        }

        /// <summary>
        /// Ô tổng thể góc phải: xấu nhất trong UnitX / Camera / SDK và lần kiểm tra trạng thái gần nhất.
        ///   ✖ lỗi (đỏ)  >  ⚠ cảnh báo / đang kết nối (cam)  >  ✔ Hệ thống sẵn sàng (xanh)  >  Chưa sẵn sàng (xám)
        /// </summary>
        private void UpdateOverallState()
        {
            var states = new[] { _unitXState, _cameraState, _sdkState };
            ComponentState overall;
            string text;

            if (states.Contains(ComponentState.Error) || _checkErrors > 0)
            {
                overall = ComponentState.Error;
                text = _checkErrors > 0 ? $"✖ {_checkErrors} lỗi" : "✖ Hệ thống lỗi";
            }
            else if (states.Contains(ComponentState.Warning) || _checkWarnings > 0)
            {
                overall = ComponentState.Warning;
                text = _checkWarnings > 0
                    ? $"⚠ {_checkWarnings} cảnh báo"
                    : _unitXState == ComponentState.Warning ? "⚠ " + _unitXText : "⚠ Cần kiểm tra";
            }
            else if (states.All(s => s == ComponentState.Ok) || (_checkErrors == 0 && _unitXState == ComponentState.Ok))
            {
                overall = ComponentState.Ok;
                text = "✔ Hệ thống sẵn sàng";
            }
            else
            {
                overall = ComponentState.Unknown;
                text = "● Chưa sẵn sàng";
            }

            tsslOverall.Text = text;
            tsslOverall.ForeColor = TextColor(overall);
        }

        private void tsslOverall_Click(object sender, EventArgs e)
        {
            btnCheckStatus_Click(tsbCheckStatus, e);
        }

        #endregion

        private async void btnCheckStatus_Click(object sender, EventArgs e)
        {
            if (_statusCheckRunning)
            {
                return;
            }

            _statusCheckRunning = true;
            string originalText = tsbCheckStatus.Text;
            tsbCheckStatus.Enabled = false;
            tsbCheckStatus.Text = "Đang kiểm tra...";

            try
            {
                while (true)
                {
                    UpdateStatus("Đang kiểm tra trạng thái hệ thống...");
                    var items = await CollectStatusAsync();

                    // Kết luận hiện ở ô trạng thái tổng thể góc phải, không lặp lại ở ô thông báo.
                    _checkErrors = items.Count(i => i.Level == StatusLevel.Error);
                    _checkWarnings = items.Count(i => i.Level == StatusLevel.Warning);
                    UpdateOverallState();
                    UpdateStatus("Đã kiểm tra trạng thái hệ thống.");

                    using (var dialog = new frmStatusV1(items, _lastProdConfig, ShowLogWindow))
                    {
                        if (dialog.ShowDialog(this) != DialogResult.Retry)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Kiểm tra trạng thái lỗi: " + ex);
                MessageBox.Show(this, "Kiểm tra trạng thái lỗi:" + Environment.NewLine + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                tsbCheckStatus.Text = originalText;
                tsbCheckStatus.Enabled = true;
                _statusCheckRunning = false;
            }
        }

        /// <summary>
        /// Thu thập trạng thái theo đúng thứ tự phụ thuộc: kết nối SDK -> edge -> mạng -> hệ thống
        /// UnitX -> cấu hình (camera / capture config / part type) -> dữ liệu nhận về -> lỗi gần đây.
        /// </summary>
        private async Task<List<StatusItem>> CollectStatusAsync()
        {
            var items = new List<StatusItem>();
            _lastProdConfig = null;   // SDK chưa chạy thì không hiện cấu hình cũ

            // ---- Mạng: chạy song song trong lúc đọc các thứ khác ----
            string host = (txtSdkHost.Text ?? string.Empty).Trim();
            Task<NetworkCheckResult> networkTask = string.IsNullOrWhiteSpace(host)
                ? null
                : RunNetworkCheckAsync(host, (int)numSdkGrpc.Value, (int)numSdkZmq.Value,
                    (int)numSdkBlob.Value, (int)numSdkClient.Value);

            // ---- 1. Kết nối SDK ----
            bool hasClientYaml = File.Exists(SdkConfigPath);
            items.Add((!hasClientYaml
                ? new StatusItem(StatusLevel.Error, "Kết nối SDK", $"Chưa có file cấu hình mạng **client.yaml** ({SdkConfigPath})",
                    "Nhập IP / cổng UnitX ở thẻ Network & Communication rồi bấm Lưu - phần mềm tự khởi động lại và kết nối.")
                : _sdkRunning
                    ? new StatusItem(StatusLevel.Ok, "Kết nối SDK", "Đang chạy - đã subscribe kết quả từ UnitX")
                    : new StatusItem(StatusLevel.Error, "Kết nối SDK", "SDK chưa chạy",
                        "Khởi động lại phần mềm; nếu vẫn lỗi, kiểm tra IP/cổng ở thẻ cấu hình UnitX rồi lưu client.yaml."))
                .In(StatusGroup.Connection, StatusIcons.Plug));

            ProdConfigData prod = null;

            // Thiếu client.yaml thì KHÔNG gọi vào unitx_sdk.dll (SEHException) - xem ConnectAsync.
            if (_sdkRunning && hasClientYaml)
            {
                // Các hàm này vẫn ghi log chi tiết như trước.
                RunDiagnostics();

                // ---- 2. Edge ----
                items.Add(CheckEdge().In(StatusGroup.Connection, StatusIcons.Server));

                // ---- 3. Hệ thống UnitX ----
                _lastSystemStatus = null;
                _lastMode = null;
                bool systemReady = ReadSystemStatus();
                items.Add(CheckSystem(systemReady).In(StatusGroup.System, StatusIcons.Pulse));
                items.Add(CheckMode().In(StatusGroup.System, StatusIcons.Mode));

                // ---- 4. Cấu hình sản xuất ----
                prod = ReadProdConfiguration();
            }

            // ---- Mạng (đợi kết quả) ----
            if (networkTask == null)
            {
                items.Add(new StatusItem(StatusLevel.Error, "Mạng tới UnitX", "Chưa nhập IP UnitX",
                    "Nhập IP máy UnitX ở thẻ cấu hình rồi kiểm tra lại.").In(StatusGroup.Connection, StatusIcons.Globe));
            }
            else
            {
                var net = await networkTask;
                string ports = $"**{net.Host}**    {Badge(net.Ping)} ping    {Badge(net.Grpc)} gRPC **{net.GrpcPort}**    "
                    + $"{Badge(net.Zmq)} ZMQ **{net.ZmqPort}**    {Badge(net.Blob)} Blob **{net.BlobPort}**";

                items.Add((net.AllOpen
                    ? new StatusItem(StatusLevel.Ok, "Mạng tới UnitX", ports)
                    : new StatusItem(StatusLevel.Error, "Mạng tới UnitX", ports + Environment.NewLine + net.Conclusion,
                        "Kiểm tra dây mạng / IP / tường lửa cho các cổng bị đóng."))
                    .In(StatusGroup.Connection, StatusIcons.Globe));
            }

            // ---- 5. Camera / Capture Config / Part Type ----
            items.Add(CheckCamera(prod).In(StatusGroup.Configuration, StatusIcons.Camera));
            items.Add(CheckCaptureConfig(prod).In(StatusGroup.Configuration, StatusIcons.Sliders));
            items.Add(CheckPartType(prod).In(StatusGroup.Configuration, StatusIcons.Package));
            items.Add(CheckExternalSource(prod).In(StatusGroup.Configuration, StatusIcons.Picture));

            // ---- 6. Dữ liệu nhận về + lỗi gần đây ----
            items.Add(CheckResults().In(StatusGroup.Data, StatusIcons.Download));
            items.Add(CheckRecentErrors().In(StatusGroup.System, StatusIcons.History));
            items.Add(CheckSaveFolder().In(StatusGroup.Data, StatusIcons.Folder));

            return items;
        }

        /// <summary>Chấm tròn màu trong dòng chi tiết (xem đánh dấu của StatusItem.Detail).</summary>
        private static string Badge(bool ok)
        {
            return ok ? "[ok]" : "[fail]";
        }

        private StatusItem CheckEdge()
        {
            int edgeCount = -1;
            IntPtr handler = IntPtr.Zero;

            try
            {
                edgeCount = get_edge_count();
                handler = GetEdgeHandler();
            }
            catch (Exception ex)
            {
                return new StatusItem(StatusLevel.Error, "Edge (máy UnitX)", "Không gọi được SDK: " + ex.Message,
                    "Kiểm tra unitx_sdk.dll cùng thư mục với phần mềm.");
            }

            string edge = (_edge ?? string.Empty).Trim();

            if (edgeCount == 0)
            {
                return new StatusItem(StatusLevel.Error, "Edge (máy UnitX)", "SDK không thấy edge nào (get_edge_count = **0**)",
                    "Máy UnitX đã bật chưa? Đúng IP trong client.yaml chưa? Xem mục Mạng.");
            }

            if (handler == IntPtr.Zero)
            {
                return new StatusItem(StatusLevel.Error, "Edge (máy UnitX)",
                    string.IsNullOrEmpty(edge) ? "Không có edge mặc định" : $"Edge **{edge}** không tồn tại",
                    "Để trống ô Edge trong settings.json để dùng edge mặc định.");
            }

            return new StatusItem(StatusLevel.Ok, "Edge (máy UnitX)",
                $"**{edgeCount}** edge - đang dùng " + (string.IsNullOrEmpty(edge) ? "edge mặc định" : $"**{edge}**"));
        }

        private StatusItem CheckSystem(bool ready)
        {
            if (_lastSystemStatus == null)
            {
                return new StatusItem(StatusLevel.Error, "Hệ thống UnitX", "Không đọc được trạng thái (get_system_status)",
                    "Kiểm tra kết nối tới UnitX, rồi thử 'Restart Part'.");
            }

            var s = _lastSystemStatus.Value;
            string state = string.IsNullOrWhiteSpace(s.status) ? (s.code == 1 ? "Sẵn sàng" : "Chưa sẵn sàng") : s.status;
            string detail = $"**{state}**  ·  code **{s.code}**";

            // UnitX có bản không gửi số liệu tài nguyên (cả 4 số = 0) - không vẽ thanh 0% gây hiểu nhầm.
            bool hasResources = s.total_memory > 0 || s.cpu > 0 || s.gpu > 0;
            List<StatusMeter> meters = null;
            if (hasResources)
            {
                meters = new List<StatusMeter>
                {
                    new StatusMeter { Name = "CPU", Percent = s.cpu, Text = s.cpu + "%" },
                    new StatusMeter { Name = "GPU", Percent = s.gpu, Text = s.gpu + "%" }
                };

                if (s.total_memory > 0)
                {
                    meters.Add(new StatusMeter
                    {
                        Name = "RAM",
                        Percent = s.use_memory * 100.0 / s.total_memory,
                        Text = $"{s.use_memory / 1024.0:0.#} / {s.total_memory / 1024.0:0.#} GB"
                    });
                }
            }
            else
            {
                detail += Environment.NewLine + "UnitX không cung cấp số liệu CPU / GPU / RAM";
            }

            var item = ready
                ? new StatusItem(StatusLevel.Ok, "Hệ thống UnitX", detail)
                : new StatusItem(StatusLevel.Error, "Hệ thống UnitX", detail + " - cần code **1**",
                    "UnitX chưa sẵn sàng: bấm 'Restart Part' (System Reset + Part Start) rồi kiểm tra lại.");
            item.Meters = meters;
            return item;
        }

        private StatusItem CheckMode()
        {
            if (_lastMode == null)
            {
                return new StatusItem(StatusLevel.Warning, "Chế độ UnitX", "Không đọc được chế độ (get_mode)");
            }

            switch (_lastMode.Value)
            {
                case 0:
                    return new StatusItem(StatusLevel.Ok, "Chế độ UnitX", "**PRODUCTION**");
                case 1:
                    return new StatusItem(StatusLevel.Warning, "Chế độ UnitX", "**VALIDATION** - không phải chế độ sản xuất",
                        "Chuyển UnitX về PRODUCTION nếu đang chạy thật.");
                default:
                    return new StatusItem(StatusLevel.Warning, "Chế độ UnitX", $"Không xác định (mode = **{_lastMode.Value}**)");
            }
        }

        private StatusItem CheckCamera(ProdConfigData prod)
        {
            string camera = (cboCameraID.Text ?? string.Empty).Trim();

            if (camera.Length == 0)
            {
                return new StatusItem(StatusLevel.Warning, "Camera ID", "Chưa chọn camera",
                    "Chọn Camera ID (danh sách đã được nạp từ UnitX nếu kết nối được).");
            }

            if (prod == null || !prod.Ok)
            {
                return new StatusItem(StatusLevel.Warning, "Camera ID", $"**{camera}** - chưa xác minh được"
                    + (prod != null && prod.Error != null ? $" ({prod.Error})" : string.Empty));
            }

            var cameras = prod.AllCameras;
            if (cameras.Count > 0 && !cameras.Contains(camera, StringComparer.OrdinalIgnoreCase))
            {
                return new StatusItem(StatusLevel.Error, "Camera ID",
                    $"**{camera}** - không có trong cấu hình UnitX (có: **{string.Join(", ", cameras)}**)",
                    "Chọn lại Camera ID trong danh sách.");
            }

            string controller;
            prod.CameraController.TryGetValue(camera, out controller);
            return new StatusItem(StatusLevel.Ok, "Camera ID", $"**{camera}** - Hợp lệ"
                + (string.IsNullOrEmpty(controller) ? string.Empty : $"  ·  controller **{controller}**"));
        }

        private StatusItem CheckCaptureConfig(ProdConfigData prod)
        {
            string cc = (cboCaptureConfig.Text ?? string.Empty).Trim();

            if (cc.Length == 0)
            {
                return new StatusItem(StatusLevel.Error, "Capture Config", "Chưa chọn capture config",
                    "Chọn Capture Config trong danh sách.");
            }

            if (prod == null || !prod.Ok)
            {
                return new StatusItem(StatusLevel.Warning, "Capture Config", $"**{cc}** - chưa xác minh được");
            }

            var configs = prod.AllCaptureConfigs;
            if (configs.Count > 0 && !configs.Contains(cc, StringComparer.OrdinalIgnoreCase))
            {
                return new StatusItem(StatusLevel.Error, "Capture Config",
                    $"**{cc}** - không có trong cấu hình UnitX (có: **{string.Join(", ", configs)}**)",
                    "Chọn lại Capture Config trong danh sách.");
            }

            // Network của capture config: từ struct hoặc từ cc_network_mapping của imaging_config.
            List<string> networks;
            if (!prod.CaptureNetworks.TryGetValue(cc, out networks))
            {
                networks = prod.PartTypes.SelectMany(p => p.Sources).SelectMany(s => s.CaptureNetworks)
                    .Where(m => string.Equals(m.Key, cc, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(m => m.Value).Distinct().ToList();
            }

            return new StatusItem(StatusLevel.Ok, "Capture Config", $"**{cc}** - Hợp lệ"
                + (networks.Count > 0 ? $"  ·  network **{string.Join(", ", networks)}**" : string.Empty));
        }

        /// <summary>
        /// Part type: đối chiếu với các part_type khai báo trong imaging_config; bản UnitX không có
        /// imaging_config thì chỉ xác nhận được khi UnitX đã gửi part_type về trong kết quả.
        /// </summary>
        private StatusItem CheckPartType(ProdConfigData prod)
        {
            string partType = (cboPartType.Text ?? string.Empty).Trim();
            string partId = (txtPartID.Text ?? string.Empty).Trim();

            if (partType.Length == 0)
            {
                return new StatusItem(StatusLevel.Error, "Part Type / Part ID", "Chưa nhập Part Type",
                    "Nhập Part Type đúng như cấu hình trên UnitX.");
            }

            if (partId.Length == 0)
            {
                return new StatusItem(StatusLevel.Error, "Part Type / Part ID", $"**{partType}** - chưa nhập Part ID",
                    "Nhập Part ID.");
            }

            var declared = prod != null ? prod.AllPartTypes : new List<string>();
            if (declared.Contains(partType) || _partTypeSuggestions.Contains(partType))
            {
                return new StatusItem(StatusLevel.Ok, "Part Type / Part ID", $"**{partType}** / **{partId}** - Hợp lệ");
            }

            if (declared.Count > 0)
            {
                return new StatusItem(StatusLevel.Error, "Part Type / Part ID",
                    $"**{partType}** - không có trong cấu hình UnitX (có: **{string.Join(", ", declared)}**)",
                    "Chọn lại Part Type trong danh sách.");
            }

            return new StatusItem(StatusLevel.Warning, "Part Type / Part ID",
                $"**{partType}** / **{partId}** - chưa được UnitX xác nhận",
                "Bấm 'Restart Part' rồi Trigger một lần: part_type đúng sẽ được xác nhận khi có kết quả.");
        }

        /// <summary>
        /// Điều kiện để PUSH ảnh (giống bước kiểm tra trước khi push): part type đang chọn có nguồn
        /// source = "external_image" khai báo image_width / image_height trong production.py.
        /// Chỉ cảnh báo - chỉ dùng trigger camera thì không cần nguồn ảnh ngoài.
        /// </summary>
        private StatusItem CheckExternalSource(ProdConfigData prod)
        {
            const string title = "Nguồn ảnh ngoài (Push)";
            string partType = (cboPartType.Text ?? string.Empty).Trim();

            if (prod == null || !prod.Ok)
            {
                return new StatusItem(StatusLevel.Warning, title, "Chưa đọc được production.py");
            }

            var part = prod.PartTypes.FirstOrDefault(p => p.Name == partType);
            var external = part == null ? null
                : part.Sources.FirstOrDefault(s => string.Equals(s.Source, ProductionPy.ExternalSource, StringComparison.OrdinalIgnoreCase));

            if (external == null)
            {
                return new StatusItem(StatusLevel.Warning, title,
                    $"Part Type **{(partType.Length == 0 ? "(trống)" : partType)}** chưa có source **\"{ProductionPy.ExternalSource}\"** - chưa push ảnh được",
                    "Chỉ cần khi Push ảnh. Bấm Push All / Push Selected để xem production.py đề xuất.");
            }

            int? width = ExtraInt(external, "image_width");
            int? height = ExtraInt(external, "image_height");
            if (width == null || height == null)
            {
                return new StatusItem(StatusLevel.Warning, title,
                    $"**\"{ProductionPy.ExternalSource}\"** chưa khai báo image_width / image_height",
                    "Bổ sung kích thước ảnh push vào production.py.");
            }

            return new StatusItem(StatusLevel.Ok, title,
                $"**\"{ProductionPy.ExternalSource}\"**  ·  ảnh **{width} × {height}** px - Hợp lệ");
        }

        private StatusItem CheckResults()
        {
            int count = Volatile.Read(ref _callbackCount);
            long ticks = Interlocked.Read(ref _lastCallbackTicks);

            if (count == 0 || ticks == 0)
            {
                return new StatusItem(StatusLevel.Warning, "Nhận kết quả", "Chưa nhận được kết quả nào từ lúc kết nối",
                    "Bình thường nếu chưa Trigger/Push. Đã Trigger mà vẫn chưa có thì xem mục Mạng (cổng ZMQ).");
            }

            var ago = DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc);
            return new StatusItem(StatusLevel.Ok, "Nhận kết quả", $"**{count}** kết quả - gần nhất **{FormatAgo(ago)}** trước");
        }

        private StatusItem CheckRecentErrors()
        {
            int crt = Volatile.Read(ref _crtFaultCount);
            if (crt > 0)
            {
                return new StatusItem(StatusLevel.Error, "Lỗi gần đây",
                    $"unitx_sdk.dll đã gặp lỗi native **{crt}** lần (đã chặn để phần mềm không bị tắt)",
                    "Trạng thái SDK có thể đã sai - khởi động lại phần mềm trước khi chạy tiếp.");
            }

            string error = _lastUnitXError;
            long ticks = Interlocked.Read(ref _lastUnitXErrorTicks);
            if (!string.IsNullOrEmpty(error) && ticks > 0)
            {
                var ago = DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc);
                if (ago <= RecentErrorWindow)
                {
                    return new StatusItem(StatusLevel.Warning, "Lỗi gần đây",
                        $"UnitX báo lỗi **{FormatAgo(ago)}** trước: {error}",
                        "Xem log chi tiết; nếu lỗi lặp lại, bấm 'Restart Part'.");
                }
            }

            // Không có gì đáng chú ý -> chữ xám, nhường chỗ cho các mục quan trọng hơn.
            return new StatusItem(StatusLevel.Ok, "Lỗi gần đây", "Không có lỗi nào trong 10 phút qua") { Muted = true };
        }

        private StatusItem CheckSaveFolder()
        {
            string folder = ResolveImageRoot(false, false);
            if (folder == null)
            {
                return new StatusItem(StatusLevel.Warning, "Thư mục lưu ảnh",
                    string.IsNullOrWhiteSpace(txtPath.Text) ? "Chưa nhập Path" : $"**{txtPath.Text.Trim()}** không tồn tại",
                    "Nhập Path hợp lệ - ảnh nhận về đang được lưu tạm vào thư mục cấu hình của phần mềm.");
            }

            string saving = (_saveRaw ? "Raw" : string.Empty)
                + (_saveRaw && _saveMask ? " + " : string.Empty)
                + (_saveMask ? "Mask" : string.Empty);

            return new StatusItem(StatusLevel.Ok, "Thư mục lưu ảnh",
                $"**{folder}**" + (saving.Length == 0 ? "  ·  đang TẮT lưu ảnh" : $"  ·  lưu **{saving}**"));
        }

        private static string FormatAgo(TimeSpan span)
        {
            if (span.TotalSeconds < 60)
            {
                return Math.Max(0, (int)span.TotalSeconds) + " giây";
            }

            if (span.TotalMinutes < 60)
            {
                return (int)span.TotalMinutes + " phút";
            }

            return (int)span.TotalHours + " giờ " + span.Minutes + " phút";
        }
    }
}
