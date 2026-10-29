using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code. Nếu class đầu tiên là partial frmMainV1, VS tự gắn SubType=Form rồi mở Designer - Designer
    // không thấy lớp cha ("base class System.Void") và đã từng ghi đè frmMainV1.Designer.cs thành form
    // trống (2026-09-25). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1SdkCodeFile { }

    /// <summary>
    /// frmMainV1 - phần giao tiếp với UnitX SDK: khai báo P/Invoke, kết nối, callback kết quả,
    /// lưu ảnh mask, chẩn đoán và các lệnh điều khiển Part/Camera.
    ///
    /// Tách riêng khỏi frmMainV1.cs (giao diện) để logic SDK không nằm lẫn trong event handler.
    /// </summary>
    public partial class frmMainV1
    {
        #region UnitX SDK interop

        // ---- unitx_sdk_interface.dll (wrapper C++, nạp động unitx_sdk.dll bằng LoadLibrary) ----
        // PushImage kiểm tra null cho imageData/length/part_type/part_id/capture_config; riêng edge
        // được phép NULL/rỗng -> khi đó wrapper gọi get_default_edge_handler(). Truyền sai tên edge
        // thì get_edge_handler_with_name() trả về null và push_image luôn trả về false.

        [DllImport("unitx_sdk_interface.dll")]
        private static extern void RunSDK();

        [DllImport("unitx_sdk_interface.dll")]
        private static extern void StopSDK();

        [DllImport("unitx_sdk_interface.dll")]
        private static extern void CleanSDK();

        [DllImport("unitx_sdk_interface.dll")]
        private static extern void SetLogFilePath(string path);

        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool PushImage(IntPtr imageData, int length, string part_type, string part_id, string capture_config, string edge);

        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool StartPart(string part_type, string part_id, string edge);

        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool RemovePart(string part_type, string part_id, string edge);

        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool ResetCamera(string p_camera_id, string p_part_id, string edge);

        // QUAN TRỌNG: ResetSystem của wrapper CÓ tham số edge (theo code mẫu C# của hãng,
        // ConsoleApp1/Program.cs: `bool ResetSystem(string edge = "")`). Khai báo thiếu tham số này
        // thì trên x64 hàm vẫn được gọi nhưng đọc rác từ thanh ghi RCX làm tên edge -> hỏng/false.
        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool ResetSystem(string edge);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SubscriptionCallback(IntPtr msg_timestamp, IntPtr payload, IntPtr blobs, long blob_count);

        [DllImport("unitx_sdk_interface.dll")]
        private static extern bool SubscribeWithCallback(UnitXTopic topic, SubscriptionCallback callback, string edge_name = "");

        // ---- unitx_sdk.dll (SDK lõi) - dùng để chẩn đoán kết nối ----

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_edge_count();

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_default_edge_handler();

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_edge_handler_with_name(string edge_name);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_system_status(IntPtr edge_handler);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_system_status(IntPtr system_status);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_prod_configuration(IntPtr edge_handler);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_prod_configuration(IntPtr prod_configuration);

        // ---- Lệnh camera: KHÔNG có trong unitx_sdk_interface.dll ----
        //
        // Đã kiểm tra bảng export của unitx_sdk_interface.dll (bản đang dùng, md5
        // 678f7a857e8bb38195ecbcffb1932102): wrapper chỉ export ĐÚNG 10 hàm - CleanSDK, PushImage,
        // RemovePart, ResetCamera, ResetSystem, RunSDK, SetLogFilePath, StartPart, StopSDK,
        // SubscribeWithCallback. KHÔNG có TriggerCamera, KHÔNG có SelectSequence. Các bản
        // frmMainV3/V8/V10/V11 khai báo hai hàm đó nhưng CHƯA BAO GIỜ gọi, nên lỗi không lộ ra;
        // hễ gọi thật là ném EntryPointNotFoundException.
        //
        // Hai lệnh này chỉ tồn tại trong SDK lõi unitx_sdk.dll (interface/unitx_sdk_interface.h của
        // hãng), đều nhận edge_handler làm tham số đầu:
        //     bool trigger_camera(UnitXEdgeHandler edge_handler, const char* camera_id);
        //     bool select_sequence(UnitXEdgeHandler edge_handler, const char* camera_id,
        //                          int sequence_number);
        // (Hãng còn có user_select_sequence(edge, camera_id, seq) = chọn sequence RỒI trigger luôn
        // một lần - chưa dùng ở đây; nếu cần bắn ảnh ngay sau khi đổi sequence thì dùng hàm đó.)
        //
        // Trình tự chuẩn theo unitx_sdk_sample.cpp của hãng:
        //     start_part_group -> select_sequence -> trigger_camera (n lần) -> reset_camera
        // nghĩa là phải có part group đang mở thì trigger mới có tác dụng.

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool trigger_camera(IntPtr edge_handler, string camera_id);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool select_sequence(IntPtr edge_handler, string camera_id, int sequence_number);

        // Theo unitx_sdk_interface.h:
        //     bool reset_camera(UnitXEdgeHandler, const char* camera_id,
        //                       const char* part_group_id, int timestamp);
        //     bool reset_system(UnitXEdgeHandler);
        // Gọi THẲNG vào SDK lõi thay vì qua wrapper ResetCamera/ResetSystem: wrapper nhận TÊN edge
        // rồi tự gọi get_edge_handler_with_name(); nếu tên sai (vd part_type "camera4-..." suy ra
        // edge "line_2" mà client.yaml chỉ khai báo line_1) thì nó truyền thẳng handler NULL xuống
        // reset_camera và tiến trình chết ngay trong native code.
        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool reset_camera(IntPtr edge_handler, string camera_id,
            string part_group_id, int timestamp);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool reset_system(IntPtr edge_handler);

        [DllImport("unitx_sdk.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_mode(IntPtr edge_handler);

        // Theo unitx_sdk_interface.h
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
        private struct UnitXSystemStatus
        {
            public int code;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string status;
            public int cpu;
            public int gpu;
            public int total_memory;
            public int use_memory;
        }

        // Giữ tham chiếu delegate để GC không thu hồi trong lúc SDK còn callback về.
        private static SubscriptionCallback _subscriptionCallback;

        #endregion

        #region Nhận biết ảnh do camera chụp (khác với ảnh mình push lên)

        /// <summary>Camera vừa được bấm Trigger - callback đọc field này nên phải volatile.</summary>
        private volatile string _lastTriggeredCameraId;

        /// <summary>Mốc thời gian của lần bấm Trigger gần nhất (Stopwatch timestamp).</summary>
        private long _lastTriggerTs;

        /// <summary>
        /// Trong bao lâu kể từ lúc bấm Trigger thì một IMAGE_RESULT của đúng camera đó còn được coi
        /// là kết quả của lần trigger ấy. Đủ rộng để camera chụp + UnitX xử lý xong, đủ hẹp để không
        /// "nhận vơ" mãi mãi.
        /// </summary>
        private const double ManualTriggerWindowMs = 60000;

        /// <summary>
        /// Thư mục ảnh đang làm việc (bản sao của txtPath để luồng callback dùng mà không đụng control).
        /// </summary>
        private volatile string _imageRootFolder;

        /// <summary>Thư mục con chứa ảnh do camera chụp, nằm cạnh thư mục ảnh đang chọn.</summary>
        private const string CameraFolderName = "camera";

        /// <summary>
        /// IMAGE_RESULT này có phải kết quả của lần bấm Camera Trigger gần đây không?
        /// Chạy trên luồng của SDK.
        /// </summary>
        private bool IsManualTriggerResult(string cameraId)
        {
            string triggered = _lastTriggeredCameraId;

            if (string.IsNullOrWhiteSpace(triggered) || string.IsNullOrWhiteSpace(cameraId))
            {
                return false;
            }

            if (!string.Equals(triggered.Trim(), cameraId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            long ts = Interlocked.Read(ref _lastTriggerTs);
            return ts > 0 && ElapsedMs(ts) <= ManualTriggerWindowMs;
        }

        /// <summary>
        /// Xử lý ảnh kết quả KHÔNG gắn với lượt push nào: camera chụp thật.
        /// Luôn hiển thị lên khung xem trước; lưu xuống đĩa nếu bật "Lưu ảnh Callback".
        /// TUYỆT ĐỐI không đặt tên theo ảnh push nào đang chờ - đó chính là lỗi lệch tên đã sửa
        /// ngày 2026-08-28.
        /// </summary>
        private void HandleCameraResult(IntPtr blobs, long blobCount, ImageResult image, string decision)
        {
            string cameraId = string.IsNullOrWhiteSpace(image.camera_id) ? "camera" : image.camera_id.Trim();

            Log($"    Ảnh từ CAMERA (không thuộc lượt push nào): camera_id='{cameraId}'"
                + (string.IsNullOrWhiteSpace(image.image_id) ? "" : $", image_id='{image.image_id}'")
                + (decision == null ? "" : $", kết quả={decision}"));

            ApplyResultLabel(decision);

            // Tên file an toàn: bỏ mọi ký tự không hợp lệ trong camera_id/image_id.
            string tag = SanitizeFileName(cameraId);
            ShowReturnedImage(blobs, blobCount, null, tag, decision, image.image_dimension);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "camera";
            }

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            }

            return sb.ToString();
        }

        #endregion

        #region Bộ chặn lỗi CRT (ucrtbase invalid parameter)

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void InvalidParameterHandler(
            [MarshalAs(UnmanagedType.LPWStr)] string expression,
            [MarshalAs(UnmanagedType.LPWStr)] string function,
            [MarshalAs(UnmanagedType.LPWStr)] string file,
            uint line,
            UIntPtr reserved);

        [DllImport("ucrtbase.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "_set_invalid_parameter_handler")]
        private static extern IntPtr SetInvalidParameterHandler(IntPtr newHandler);

        [DllImport("ucrtbase.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "_set_abort_behavior")]
        private static extern uint SetAbortBehavior(uint flags, uint mask);

        private const uint WriteAbortMsg = 0x1;
        private const uint CallReportFault = 0x2;

        // Giữ tham chiếu ở field tĩnh: nếu để GC thu hồi delegate thì con trỏ hàm đã đưa cho CRT
        // trở thành rác và lần gọi kế tiếp sẽ làm chết tiến trình đúng theo cách đang muốn tránh.
        private static InvalidParameterHandler _crtHandler;
        private static frmMainV1 _crtOwner;
        private static int _crtFaultCount;

        /// <summary>
        /// Chặn cơ chế "invalid parameter is fatal" của UCRT.
        ///
        /// Mặc định, khi một hàm CRT an toàn (strcpy_s, sprintf_s, localtime_s...) trong
        /// unitx_sdk.dll nhận tham số không hợp lệ, ucrtbase.dll gọi thẳng abort/__fastfail và
        /// TOÀN BỘ tiến trình chết ngay - đúng thông báo "An invalid parameter was passed to a
        /// function that considers invalid parameters fatal" ở ucrtbase.dll.
        ///
        /// Cài handler riêng biến sự cố đó thành một dòng log: hàm CRT trả về lỗi, hàm gọi nó trả
        /// về false, phần mềm vẫn sống. Log in ra ĐÚNG tên hàm CRT và biểu thức bị lỗi - đó là manh
        /// mối cần thiết để biết SDK đang hỏng ở đâu.
        ///
        /// CẢNH BÁO: đây là lưới an toàn, KHÔNG phải bản vá. Sau một lần bị chặn, trạng thái bên
        /// trong SDK có thể đã sai; thấy log này thì nên kết nối lại SDK trước khi chạy tiếp.
        /// </summary>
        private void InstallCrtGuard()
        {
            if (_crtHandler != null)
            {
                _crtOwner = this;
                return;
            }

            try
            {
                _crtOwner = this;
                _crtHandler = OnCrtInvalidParameter;

                SetInvalidParameterHandler(Marshal.GetFunctionPointerForDelegate(_crtHandler));
                SetAbortBehavior(0, WriteAbortMsg | CallReportFault);

                Log("Đã cài bộ chặn lỗi CRT: lỗi 'invalid parameter' trong unitx_sdk.dll "
                    + "sẽ được ghi log thay vì làm chết phần mềm.");
            }
            catch (Exception ex)
            {
                _crtHandler = null;
                Log("Không cài được bộ chặn lỗi CRT (" + ex.Message
                    + ") - lỗi native vẫn sẽ làm chết tiến trình như trước.");
            }
        }

        private static void OnCrtInvalidParameter(string expression, string function,
            string file, uint line, UIntPtr reserved)
        {
            int count = Interlocked.Increment(ref _crtFaultCount);
            var owner = _crtOwner;
            if (owner == null)
            {
                return;
            }

            try
            {
                owner.Log("!!! LỖI CRT TRONG NATIVE SDK (lần " + count + ") !!!");
                owner.Log("    hàm       : " + (function ?? "<không rõ>"));
                owner.Log("    biểu thức : " + (expression ?? "<không rõ>"));
                owner.Log("    file/dòng : " + (file ?? "<không rõ>") + ":" + line);
                owner.Log("    Bình thường lỗi này làm chết cả phần mềm; đã chặn lại. "
                    + "Nên bấm kết nối lại UnitX trước khi chạy tiếp.");
                owner.UpdateStatus("SDK báo lỗi CRT - đã chặn, xem log.");
            }
            catch
            {
                // Handler chạy trên luồng của SDK: tuyệt đối không được ném gì ra ngoài.
            }
        }

        #endregion

        #region Kết nối / ngắt kết nối SDK

        /// <summary>
        /// Kết nối UnitX. Ở V13 hàm này được gọi TỰ ĐỘNG khi form vừa mở (yêu cầu "Kết nối UnitX:
        /// Auto kết nối") nên không còn nút Kết nối trên giao diện.
        /// Thứ tự subscribe TRƯỚC RunSDK giữ nguyên theo frmMainV11/V12: subscribe chính là bước
        /// khởi tạo kết nối tới edge, thiếu bước này push thường trả về false.
        /// </summary>
        private async Task ConnectAsync()
        {
            if (_sdkRunning)
            {
                return;
            }

            // Thiếu client.yaml: RunSDK vẫn "chạy" nhưng SDK ở trạng thái hỏng - mọi lệnh gọi sau đó
            // (get_default_edge_handler, get_edge_count...) ném SEHException "External component has
            // thrown an exception". KHÔNG khởi động SDK, hướng người dùng tạo cấu hình mạng trước.
            if (!File.Exists(SdkConfigPath))
            {
                LogError("Chưa có file cấu hình mạng UnitX: " + SdkConfigPath + " -> KHÔNG khởi động SDK.");
                LogWarning("Nhập IP / cổng UnitX trong thẻ 'Network & Communication' rồi bấm Lưu để tạo client.yaml.");
                SetUnitXState(ComponentState.Error, "Chưa cấu hình mạng");
                UpdateStatus("Chưa có client.yaml - nhập IP UnitX ở thẻ Network & Communication rồi bấm Lưu.");
                return;
            }

            try
            {
                UpdateStatus("Đang kết nối UnitX...");
                SetUnitXState(ComponentState.Warning, "Đang kết nối...");

                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    SetLogFilePath(SdkLogPath);
                    Log("SDK log file: " + SdkLogPath);
                }
                catch (Exception ex)
                {
                    Log("SetLogFilePath lỗi (bỏ qua): " + ex.Message);
                }

                // Subscribe = luôn BẬT ở V13 (mặc định cứng trong code, không đưa lên giao diện).
                try
                {
                    _subscriptionCallback = new SubscriptionCallback(OnSubscriptionCallback);

                    Log("SubscribeWithCallback(IMAGE_RESULT) => "
                        + SubscribeWithCallback(UnitXTopic.UNITX_TOPIC_IMAGE_RESULT, _subscriptionCallback));
                    Log("SubscribeWithCallback(PART_GROUP_RESULT) => "
                        + SubscribeWithCallback(UnitXTopic.UNITX_TOPIC_PART_GROUP_RESULT, _subscriptionCallback));

                    // Các topic lỗi: lỗi phía server (ví dụ "'NoneType' object is not iterable")
                    // hiện ngay trong log ứng dụng chứ không chỉ nằm trong log SDK.
                    foreach (var topic in new[]
                    {
                        UnitXTopic.UNITX_TOPIC_ERROR_OVERALL,
                        UnitXTopic.UNITX_TOPIC_ERROR_PRODUCTION,
                        UnitXTopic.UNITX_TOPIC_ERROR_OPTIX,
                        UnitXTopic.UNITX_TOPIC_ERROR_CORTEX,
                        UnitXTopic.UNITX_TOPIC_ERROR_BLOB_STORAGE
                    })
                    {
                        try
                        {
                            Log($"SubscribeWithCallback({topic}) => " + SubscribeWithCallback(topic, _subscriptionCallback));
                        }
                        catch (Exception exTopic)
                        {
                            Log($"Subscribe {topic} lỗi: {exTopic.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Subscribe lỗi: " + ex.Message);
                }

                _sdkTask = Task.Run(() =>
                {
                    try
                    {
                        RunSDK();
                    }
                    catch (Exception ex)
                    {
                        Log("Lỗi RunSDK: " + ex.Message);
                    }
                });

                _sdkRunning = true;
                Log("Đã gọi RunSDK ở luồng nền.");

                if (WaitAfterRunSdkMs > 0)
                {
                    UpdateStatus($"Đang chờ SDK sẵn sàng ({WaitAfterRunSdkMs} ms)...");
                    await Task.Delay(WaitAfterRunSdkMs);
                }

                RunDiagnostics();
                SetUnitXState(ComponentState.Ok, "Đã kết nối (Auto)");
                SetSdkState(ComponentState.Ok, "Sẵn sàng");
                UpdateStatus("Đã kết nối UnitX.");
                UpdateActionButtons();

                // Nếu "Theo dõi thư mục" đã bật sẵn (phục hồi từ settings.json) thì bắt đầu ngay
                // bây giờ - không bật lúc form Load để tránh popup trước khi biết thư mục/cấu hình.
                if (chkWatchFolder.Checked && _folderWatcher == null)
                {
                    StartWatching();
                }
            }
            catch (Exception ex)
            {
                Log("Không khởi động được SDK: " + ex.Message);
                SetUnitXState(ComponentState.Error, "Lỗi kết nối");
                UpdateStatus("Không kết nối được UnitX.");
            }
        }

        private void StopSdk()
        {
            // Trigger liên tục gọi thẳng vào SDK - phải dừng hẳn TRƯỚC khi StopSDK/CleanSDK, nếu
            // không sẽ có lệnh trigger chạy vào một SDK đã bị dọn dẹp.
            StopContinuousTriggerAndWait();

            // Theo dõi thư mục phụ thuộc vào SDK đang chạy - dừng trước cho gọn. KHÔNG bỏ tick
            // chkWatchFolder để lần kết nối kế tiếp tự bật lại.
            StopWatching();

            if (!_sdkRunning)
            {
                return;
            }

            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        StopSDK();
                    }
                    catch (Exception ex)
                    {
                        Log("Lỗi StopSDK: " + ex.Message);
                    }
                }).Wait(3000);

                Log("Đã dừng UnitX SDK.");

                try
                {
                    CleanSDK();
                }
                catch (Exception ex)
                {
                    Log("CleanSDK lỗi (bỏ qua): " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log("Lỗi khi dừng SDK: " + ex.Message);
            }
            finally
            {
                _sdkRunning = false;
                SetUnitXState(ComponentState.Unknown, "Chưa kết nối");
                SetSdkState(ComponentState.Unknown, "Chưa rõ");
            }
        }

        #endregion

        #region Callback kết quả từ UnitX

        /// <summary>
        /// Callback từ SDK (chạy trên luồng của SDK). Ngoài việc đếm kết quả, hàm này "học" các giá
        /// trị part_type / capture_config / camera_id mà UnitX thực sự dùng rồi đổ vào combobox.
        /// </summary>
        private void OnSubscriptionCallback(IntPtr msgTimestamp, IntPtr payload, IntPtr blobs, long blobCount)
        {
            try
            {
                Interlocked.Increment(ref _callbackCount);
                Interlocked.Exchange(ref _lastCallbackTicks, DateTime.UtcNow.Ticks);

                if (payload == IntPtr.Zero)
                {
                    Log("<- Callback nhưng payload NULL");
                    return;
                }

                var data = Marshal.PtrToStructure<SubscriptionPayload>(payload);

                switch (data.type)
                {
                    case UnitXPayloadType.UNITX_PAYLOAD_IMAGE_RESULT:
                        long callbackStart = Stopwatch.GetTimestamp();

                        long pushDone = Interlocked.Read(ref _pushDoneTs);
                        _lastPushToCallbackMs = pushDone > 0 ? ElapsedMs(pushDone) : 0;

                        var image = data.image_result;

                        try
                        {
                            var imageTime = DateTimeOffset.FromUnixTimeMilliseconds(image.image_timestamp);
                            _lastUnitXLatencyMs = (DateTimeOffset.UtcNow - imageTime).TotalMilliseconds;
                        }
                        catch
                        {
                            _lastUnitXLatencyMs = 0;
                        }

                        string decision = ReadDecision(image);

                        Log($"<- IMAGE_RESULT sau {_lastPushToCallbackMs:0} ms: part_type='{image.part_type}', "
                            + $"capture_config='{image.capture_config}', camera_id='{image.camera_id}', "
                            + $"sequence='{image.sequence}', n_capture={image.n_capture}"
                            + (decision == null ? "" : $", kết quả={decision}"));

                        // Khoá blob UnitX gán cho ảnh này + số blob SDK thực sự tải về: có khoá mà
                        // blob_count = 0 là SDK không tải được blob (cổng blob_port), không có khoá là
                        // UnitX không publish ảnh cho lần chụp này.
                        Log($"    blob: raw_key='{image.raw_image_blob_key}', overlay_key='{image.overlaid_mask_blob_key}', "
                            + $"bin_mask_keys={image.bin_mask_blob_keys_count}, blob_count={blobCount}");

                        Discover(image.part_type, image.capture_config, image.camera_id);

                        // MỘT IMAGE_RESULT CÓ THỂ ĐẾN TỪ HAI NGUỒN KHÁC NHAU:
                        //
                        //   (a) Ảnh MÌNH ĐẨY LÊN bằng PushImage -> phải đối soát với hàng đợi FIFO
                        //       _pendingPushes để lưu mask ĐÚNG TÊN ảnh gốc (xem ghi chú 2026-08-28).
                        //   (b) Ảnh do CAMERA CHỤP THẬT (bấm Camera Trigger, hoặc dây chuyền tự chạy)
                        //       -> KHÔNG có lượt push nào tương ứng.
                        //
                        // Bản đầu chỉ xử lý (a): hễ hàng đợi rỗng là bỏ qua cả việc HIỂN THỊ, nên ảnh
                        // camera chụp về không bao giờ hiện lên. Tệ hơn, nếu còn một lượt push cũ đang
                        // chờ thì ảnh của camera sẽ bị lưu đè lên tên ảnh push đó.
                        //
                        // Phân biệt bằng camera_id: nguồn ảnh ngoài (dùng cho PushImage) và camera thật
                        // có camera_id khác nhau, nên nếu camera_id trùng camera vừa bấm Trigger thì
                        // chắc chắn là (b) - không được đụng vào hàng đợi push.
                        bool isCameraResult = IsManualTriggerResult(image.camera_id) || _pendingPushes.IsEmpty;

                        if (isCameraResult)
                        {
                            var cycle = BeginCameraResult(image, callbackStart);
                            _currentCameraCycle = cycle;
                            try
                            {
                                HandleCameraResult(blobs, blobCount, image, decision);
                            }
                            finally
                            {
                                _currentCameraCycle = null;
                                EndCameraResult(cycle, blobCount, decision);

                                // Báo cho vòng trigger liên tục: kết quả của lần chụp trước đã về (và
                                // đã lưu xong) -> được phép gửi lệnh trigger kế tiếp.
                                Interlocked.Increment(ref _cameraResultCount);
                                _cameraResultEvent.Set();
                            }

                            _lastCallbackTotalMs = ElapsedMs(callbackStart);
                            Log($"    Xử lý trong callback: {_lastCallbackTotalMs:0} ms "
                                + $"(giải mã {_lastDecodeMs:0} ms, lưu ảnh {_lastSaveMaskMs:0} ms), "
                                + $"độ trễ từ mốc ảnh của UnitX: {_lastUnitXLatencyMs:0} ms");
                            break;
                        }

                        // Đối soát ĐÚNG ảnh push với ĐÚNG kết quả: dequeue theo FIFO thay vì đọc một
                        // field dùng chung - xem ghi chú tại khai báo _pendingPushes trong frmMainV1.cs.
                        {
                            PendingPush pending = null;
                            PendingPush candidate;

                            while (_pendingPushes.TryDequeue(out candidate))
                            {
                                double ageMs = ElapsedMs(candidate.PushTimestamp);
                                if (ageMs > PendingPushMaxAgeMs)
                                {
                                    Log($"    CẢNH BÁO: bỏ một lượt push mồ côi đã chờ {ageMs / 1000:0} giây "
                                        + $"không có kết quả ({Path.GetFileName(candidate.FullPath)}) - UnitX có thể "
                                        + "đã bỏ qua ảnh này, KHÔNG dùng tên ảnh này cho kết quả hiện tại.");
                                    candidate.Signal.Set();
                                    continue;
                                }

                                pending = candidate;
                                break;
                            }

                            if (pending != null)
                            {
                                double sinceQueuedMs = ElapsedMs(pending.PushTimestamp);
                                if (sinceQueuedMs > 1500)
                                {
                                    Log($"    CẢNH BÁO: IMAGE_RESULT này khớp với ảnh đã push từ {sinceQueuedMs:0} ms "
                                        + $"trước ({Path.GetFileName(pending.FullPath)}) - callback bị TRỄ đáng kể "
                                        + "so với lúc push.");
                                }

                                if (!_pendingPushes.IsEmpty)
                                {
                                    Log($"    CẢNH BÁO: còn {_pendingPushes.Count} lượt push khác đang chờ kết quả "
                                        + "trong hàng đợi - UnitX có thể đang trả kết quả chậm hơn tốc độ push.");
                                }

                                if (decision != null)
                                {
                                    SetResultForPath(pending.FullPath, decision);
                                }

                                ShowReturnedImage(blobs, blobCount, pending.FullPath, null, decision, image.image_dimension);
                                pending.Signal.Set();
                            }
                            else
                            {
                                // Hàng đợi chỉ còn toàn entry "mồ côi" quá hạn -> coi như ảnh camera.
                                Log("    Không còn lượt push hợp lệ nào đang chờ - xử lý như ảnh chụp từ camera.");
                                HandleCameraResult(blobs, blobCount, image, decision);
                            }
                        }

                        _lastCallbackTotalMs = ElapsedMs(callbackStart);
                        Log($"    Xử lý trong callback: {_lastCallbackTotalMs:0} ms "
                            + $"(giải mã {_lastDecodeMs:0} ms, lưu mask {_lastSaveMaskMs:0} ms), "
                            + $"độ trễ từ mốc ảnh của UnitX: {_lastUnitXLatencyMs:0} ms");
                        break;

                    case UnitXPayloadType.UNITX_PAYLOAD_PART_GROUP_RESULT:
                        Log($"<- PART_GROUP_RESULT: part_type='{data.part_group_result.part_type}', "
                            + $"part_group_id='{data.part_group_result.part_group_id}'");
                        Discover(data.part_group_result.part_type, null);
                        // KHÔNG set tín hiệu chờ ảnh (_pendingPushes) ở đây - PART_GROUP_RESULT là sự
                        // kiện cấp part-group, không gắn với một ảnh cụ thể nào (xem ghi chú 2026-08-28).
                        break;

                    case UnitXPayloadType.UNITX_PAYLOAD_ERROR_INFO:
                        Log($"<- LỖI TỪ UNITX: {data.error_info.description} (nguồn: {data.error_info.source})");
                        _lastUnitXError = $"{data.error_info.description} (nguồn: {data.error_info.source})";
                        Interlocked.Exchange(ref _lastUnitXErrorTicks, DateTime.UtcNow.Ticks);
                        break;

                    default:
                        Log($"<- Nhận payload loại {data.type} ({blobCount} blob)");
                        break;
                }
            }
            catch
            {
                // Callback chạy trên luồng của SDK, tuyệt đối không để exception thoát ra ngoài.
            }
        }

        /// <summary>
        /// Đọc kết quả OK/NG của ảnh từ phần tử đầu tiên của mảng part_image_results.
        /// Trả về null nếu UnitX không kèm kết quả nào (không phải lỗi - ví dụ ảnh calib).
        /// </summary>
        private string ReadDecision(ImageResult image)
        {
            try
            {
                if (image.part_image_results == IntPtr.Zero || image.part_image_results_count <= 0)
                {
                    return null;
                }

                int structSize = Marshal.SizeOf<PartImageResult>();
                var first = Marshal.PtrToStructure<PartImageResult>(image.part_image_results);

                // Với nhiều part trên một ảnh: NG nếu BẤT KỲ part nào NG (nguyên tắc an toàn).
                var worst = first.decision;
                for (int i = 1; i < image.part_image_results_count; i++)
                {
                    var item = Marshal.PtrToStructure<PartImageResult>(IntPtr.Add(image.part_image_results, i * structSize));
                    if (item.decision == Decision.DECISION_NG)
                    {
                        worst = Decision.DECISION_NG;
                        break;
                    }

                    if (item.decision == Decision.DECISION_LIMIT && worst == Decision.DECISION_OK)
                    {
                        worst = Decision.DECISION_LIMIT;
                    }
                }

                switch (worst)
                {
                    case Decision.DECISION_OK: return "OK";
                    case Decision.DECISION_NG: return "NG";
                    case Decision.DECISION_LIMIT: return "LIMIT";
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                Log("    Không đọc được kết quả OK/NG: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Ảnh UnitX trả về kèm kết quả nằm trong mảng blob của callback - hiển thị blob kết quả lên
        /// khung xem trước và (nếu bật "Lưu ảnh Callback") lưu xuống đĩa.
        ///
        /// Hai chế độ, tuỳ nguồn của kết quả:
        ///   - sourcePath != null : ảnh mình đã push -> lưu vào thư mục con "mask" cạnh ảnh gốc,
        ///                          tên = tên ảnh gốc + "mask" (cameraTag bị bỏ qua).
        ///   - sourcePath == null : ảnh camera chụp  -> lưu vào thư mục con "camera",
        ///                          tên = cameraTag + mốc thời gian.
        /// </summary>
        private void ShowReturnedImage(IntPtr blobs, long blobCount, string sourcePath, string cameraTag,
            string decision, ImageDimension dimension)
        {
            if (blobs == IntPtr.Zero || blobCount <= 0)
            {
                Log("    KHÔNG CÓ ẢNH: callback kèm kết quả nhưng không kèm blob ảnh nào (xem dòng 'blob:' ở trên).");
                ClearPreview("Có kết quả " + (decision ?? "") + " nhưng UnitX không gửi ảnh kèm theo - xem log.");
                return;
            }

            try
            {
                int structSize = Marshal.SizeOf<SubscriptionBlobs>();
                var images = new List<KeyValuePair<string, byte[]>>();
                _lastEmptyBlobs = 0;
                _lastThumbMs = 0;
                _lastBlobCopyMs = 0;
                _lastDecodeMs = 0;
                _lastSaveMaskMs = 0;
                _lastReturnBytes = 0;

                long copyStart = Stopwatch.GetTimestamp();
                long returnBytes = 0;

                for (int i = 0; i < blobCount; i++)
                {
                    var blob = Marshal.PtrToStructure<SubscriptionBlobs>(IntPtr.Add(blobs, i * structSize));

                    if (blob.blob_data == IntPtr.Zero || blob.blob_size <= 0)
                    {
                        Log($"    Blob[{i}] '{blob.blob_name}' rỗng.");
                        _lastEmptyBlobs++;
                        continue;
                    }

                    var bytes = new byte[blob.blob_size];
                    Marshal.Copy(blob.blob_data, bytes, 0, (int)blob.blob_size);

                    Log($"    Blob[{i}] '{blob.blob_name}' / '{blob.sub_blob_name}' "
                        + $"{FormatSize(bytes.Length)}, định dạng {DetectKind(bytes)}");

                    images.Add(new KeyValuePair<string, byte[]>(blob.blob_name, bytes));
                    returnBytes += bytes.Length;
                }

                _lastBlobCopyMs = ElapsedMs(copyStart);
                _lastReturnBytes = returnBytes;

                if (images.Count == 0)
                {
                    Log($"    KHÔNG CÓ ẢNH: cả {blobCount} blob đều rỗng - SDK có khoá blob nhưng không tải được dữ liệu "
                        + "(kiểm tra cổng blob_port).");
                    ClearPreview("Có kết quả " + (decision ?? "") + " nhưng blob ảnh rỗng - xem log.");
                    return;
                }

                double copySeconds = _lastBlobCopyMs / 1000.0;
                Log($"    Nhận về {FormatSize(returnBytes)} trong {_lastBlobCopyMs:0} ms"
                    + (copySeconds > 0 ? $" ({returnBytes / 1048576.0 / copySeconds:0} MB/s)" : ""));

                // Theo frmMainV11: blob[0] là ảnh RAW (ảnh gốc), blob[1..] là ảnh kết quả
                // (overlay/mask). Khi chỉ có đúng 1 blob thì blob đó chính là ảnh kết quả.
                bool hasRawBlob = images.Count > 1;
                int firstResult = hasRawBlob ? 1 : 0;
                Log($"    blob[0]={(hasRawBlob ? "ảnh RAW" : "(không có)")}, "
                    + $"ảnh kết quả = blob[{firstResult}..{images.Count - 1}], đảo kênh R/B: tự nhận biết theo màu ảnh push, "
                    + $"image_dimension={dimension.width}x{dimension.height}x{dimension.channel}");

                // Giải mã + đảo kênh MỘT LẦN cho mỗi blob rồi dùng chung cho cả hiển thị lẫn lưu file.
                long decodeStart = Stopwatch.GetTimestamp();
                var bitmaps = new List<Bitmap>();
                Bitmap rawBitmap = null;

                try
                {
                    // Chỉ giải mã ảnh raw khi thật sự cần lưu - giải mã một ảnh 12 MP không rẻ.
                    if (hasRawBlob && _saveRaw)
                    {
                        rawBitmap = DecodeResultBitmap(images[0].Value, dimension, sourcePath);
                    }

                    for (int i = firstResult; i < images.Count; i++)
                    {
                        bitmaps.Add(DecodeResultBitmap(images[i].Value, dimension, sourcePath));
                    }

                    _lastDecodeMs = ElapsedMs(decodeStart);

                    // Lưu TRƯỚC rồi mới hiển thị: hiển thị cần biết đường dẫn file vừa lưu để phím
                    // Space chuyển qua lại Raw/Mask vẫn mở đúng ảnh.
                    //
                    // Tên file tính MỘT LẦN rồi dùng cho cả ảnh raw lẫn ảnh mask của cùng lần nhận,
                    // để hai file ghép cặp được với nhau (nếu tính hai lần thì mốc thời gian / số
                    // thứ tự của chúng sẽ lệch nhau).
                    //   - Ảnh push  : giữ nguyên tên ảnh gốc - FindMaskPath dựa vào đó để biết ảnh
                    //                 nào đã xử lý xong, đổi tên là hỏng "chỉ push ảnh còn thiếu".
                    //   - Ảnh camera: {tên người dùng nhập}{_raw|_mask}{_OK|_NG}{_hậu tố}, xem BuildCameraBaseName
                    //                 và BuildCameraNameTail - hậu tố (timestamp / số thứ tự) nằm CUỐI tên.
                    //   - Ảnh trong PATH\raw được push lại (thiếu mask): CHÍNH NÓ là ảnh RAW -> không lưu
                    //                 thêm bản raw, mask đặt tên theo quy ước camera
                    //                 {tên}_mask_{OK|NG}{_hậu tố của raw} để ghép đúng cặp với ảnh raw.
                    bool rawFileSource = sourcePath != null && IsAppRawFile(sourcePath);
                    string baseName;
                    string nameTail;
                    if (rawFileSource)
                    {
                        string ignoredDecision;
                        SplitCameraName(sourcePath, out baseName, out ignoredDecision, out nameTail);
                    }
                    else
                    {
                        baseName = sourcePath != null
                            ? Path.GetFileNameWithoutExtension(sourcePath)
                            : BuildCameraBaseName(cameraTag);
                        nameTail = sourcePath != null ? string.Empty : BuildCameraNameTail();
                    }

                    _lastSaveMaskMs = 0;
                    string savedMaskPath = null;
                    string savedRawPath = null;

                    // Ảnh camera trùng tên mẫu đã lưu: hỏi người dùng (ghi đè / giữ ảnh cũ / lưu thêm hậu tố).
                    bool saveThisCapture = true;
                    if (sourcePath == null && ((_saveRaw && rawBitmap != null) || _saveMask))
                    {
                        saveThisCapture = ResolveDuplicateCapture(baseName, ref nameTail);
                    }

                    if (saveThisCapture && ((_saveRaw && rawBitmap != null) || _saveMask))
                    {
                        long saveStart = Stopwatch.GetTimestamp();

                        if (_saveRaw && rawBitmap != null && !rawFileSource)
                        {
                            savedRawPath = SaveRawImage(rawBitmap, sourcePath, baseName, nameTail, decision);
                        }

                        if (_saveMask)
                        {
                            savedMaskPath = SaveMaskImages(bitmaps, sourcePath, baseName, nameTail, decision, rawFileSource);
                        }

                        _lastSaveMaskMs = ElapsedMs(saveStart);
                    }

                    if (_saveRaw && !hasRawBlob)
                    {
                        Log("    Đang bật 'Lưu ảnh Raw' nhưng callback chỉ kèm 1 blob - UnitX không "
                            + "trả ảnh raw về cho lần này, chỉ lưu được ảnh kết quả.");
                    }

                    var first = bitmaps.FirstOrDefault(b => b != null);

                    // Ảnh kết quả không giải mã được (lý do đã ghi ở dòng log ngay trên) thì hiện tạm
                    // ảnh RAW, còn không có gì hiện được thì báo thẳng lên khung xem trước - trước đây
                    // khung giữ nguyên ảnh cũ nên trông như "có OK/NG mà không có ảnh".
                    if (first == null && hasRawBlob)
                    {
                        if (rawBitmap == null)
                        {
                            rawBitmap = DecodeResultBitmap(images[0].Value, dimension, sourcePath);
                        }

                        if (rawBitmap != null)
                        {
                            Log("    Ảnh kết quả không giải mã được - hiển thị tạm ảnh RAW (blob[0]).");
                            first = rawBitmap;
                        }
                    }

                    if (first == null)
                    {
                        Log("    KHÔNG HIỂN THỊ ĐƯỢC: không blob nào giải mã được thành ảnh.");
                        ClearPreview("Có kết quả " + (decision ?? "") + " nhưng không giải mã được ảnh - xem log.");
                    }
                    else
                    {
                        // Ảnh kết quả vừa nhận được hiển thị ngay ở chế độ Mask.
                        if (sourcePath != null)
                        {
                            ShowLiveMask(sourcePath, CreateThumbnail(first, PreviewMaxSize));
                        }
                        else
                        {
                            long thumbStart = Stopwatch.GetTimestamp();
                            var thumb = CreateThumbnail(first, PreviewMaxSize);
                            _lastThumbMs = ElapsedMs(thumbStart);
                            MarkDisplayHandOff(_currentCameraCycle);
                            ShowLiveCameraImage(cameraTag, savedRawPath, savedMaskPath, thumb);
                        }
                    }

                    if (sourcePath == null && savedRawPath != null)
                    {
                        AddCameraRawItem(savedRawPath, savedMaskPath, decision);
                    }
                }
                finally
                {
                    if (rawBitmap != null)
                    {
                        rawBitmap.Dispose();
                    }

                    foreach (var bitmap in bitmaps)
                    {
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("    Không đọc được blob trả về: " + ex.Message);
            }
        }

        /// <summary>
        /// Giải mã blob thành bitmap 24bpp và sửa thứ tự kênh màu nếu cần.
        /// Ảnh UnitX trả về được encode từ buffer BGR nhưng đánh dấu là RGB nên khi hiển thị bằng
        /// GDI+ thì đỏ hoá xanh và ngược lại - đảo lại kênh R/B là xong.
        /// </summary>
        private Bitmap DecodeResultBitmap(byte[] bytes, ImageDimension dimension, string sourcePath)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            // Ảnh camera chụp (Trigger) UnitX gửi về dạng PIXEL THÔ chưa nén (không có header
            // PNG/JPG/BMP) - Image.FromStream sẽ ném "Parameter is not valid". Kích thước lấy từ
            // image_dimension của chính IMAGE_RESULT đó.
            if (DetectKind(bytes) == ImageKind.Unknown)
            {
                return DecodeRawPixels(bytes, dimension, sourcePath);
            }

            Bitmap bitmap = null;

            try
            {
                // Không dùng new Bitmap(Image): GDI+ ném ArgumentException với blob của UnitX.
                using (var ms = new MemoryStream(bytes))
                using (var decoded = Image.FromStream(ms, false, false))
                {
                    bitmap = new Bitmap(decoded.Width, decoded.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(decoded, 0, 0, bitmap.Width, bitmap.Height);
                    }
                }

                if (ShouldSwapResult(bitmap, sourcePath, ref _detectedSwap, SwapRedBlueChannels))
                {
                    SwapRedBlue(bitmap);
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }

                Log($"    Không giải mã được ảnh trả về ({FormatSize(bytes.Length)}, {DetectKind(bytes)}): {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Buffer pixel thô của UnitX theo thứ tự BGR (kiểu OpenCV) - trùng thứ tự byte trong bộ nhớ
        /// của Format24bppRgb nên chép thẳng là đúng màu. Nếu ảnh camera hiện đỏ hoá xanh thì đổi
        /// thành false.
        /// </summary>
        private const bool RawPixelsAreBgr = true;

        /// <summary>
        /// Dựng bitmap 24bpp từ buffer pixel thô height x width x channel (hàng liền nhau, không
        /// padding). Hỗ trợ 1 kênh (xám), 3 kênh (BGR/RGB) và 4 kênh (bỏ kênh alpha).
        /// </summary>
        private Bitmap DecodeRawPixels(byte[] bytes, ImageDimension dimension, string sourcePath)
        {
            long width = dimension.width;
            long height = dimension.height;
            long channel = dimension.channel;

            if (width <= 0 || height <= 0)
            {
                Log($"    Không giải mã được ảnh trả về ({bytes.Length} byte): không có header ảnh và "
                    + $"image_dimension không hợp lệ ({width}x{height}x{channel}).");
                return null;
            }

            // Tin số byte thực nhận hơn là trường channel: suy ra số kênh từ kích thước buffer.
            long pixels = width * height;
            if (bytes.Length % pixels == 0)
            {
                channel = bytes.Length / pixels;
            }

            if ((channel != 1 && channel != 3 && channel != 4) || bytes.Length != pixels * channel)
            {
                Log($"    Không giải mã được ảnh trả về: {bytes.Length} byte không khớp image_dimension "
                    + $"{dimension.width}x{dimension.height}x{dimension.channel} (cần {pixels * dimension.channel} byte).");
                return null;
            }

            int w = (int)width;
            int h = (int)height;
            int c = (int)channel;

            // Chép thẳng theo BGR rồi mới quyết định đảo (so màu với ảnh push, xem ShouldSwapResult): log
            // 2026-10-14 ảnh Dome xanh dương push sang, pixel thô nhận về là RGB nên hiện thành đỏ.
            const bool swap = false;

            var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                var row = new byte[w * 3];
                int srcRow = w * c;

                for (int y = 0; y < h; y++)
                {
                    int src = y * srcRow;

                    if (c == 3 && !swap)
                    {
                        Buffer.BlockCopy(bytes, src, row, 0, row.Length);
                    }
                    else
                    {
                        for (int x = 0, dst = 0; x < w; x++, dst += 3, src += c)
                        {
                            if (c == 1)
                            {
                                row[dst] = row[dst + 1] = row[dst + 2] = bytes[src];
                            }
                            else
                            {
                                row[dst] = bytes[swap ? src + 2 : src];
                                row[dst + 1] = bytes[src + 1];
                                row[dst + 2] = bytes[swap ? src : src + 2];
                            }
                        }
                    }

                    Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            if (c != 1 && ShouldSwapResult(bitmap, sourcePath, ref _detectedRawSwap, !RawPixelsAreBgr))
            {
                SwapRedBlue(bitmap);
            }

            return bitmap;
        }

        /// <summary>Như _detectedSwap nhưng cho ảnh trả về dạng pixel thô (không header PNG/BMP).</summary>
        private bool? _detectedRawSwap;

        /// <summary>Mẫu điểm ảnh của ảnh push gần nhất (null nếu chưa có) - để tự nhận biết đảo kênh R/B.</summary>
        private volatile PixelSamples _lastPushSamples;

        /// <summary>
        /// Đảo kênh R/B đã nhận biết được từ ảnh push (null = chưa biết, dùng SwapRedBlueChannels).
        /// Có bản UnitX trả ảnh đúng màu, có bản trả ảnh đảo R/B (log 2026-10-14: ảnh Dome xanh dương nhận
        /// về thành đỏ vì luôn đảo theo SwapRedBlueChannels) - so ảnh nhận về với ảnh đã push để chọn.
        /// </summary>
        private bool? _detectedSwap;

        /// <summary>
        /// Lưới SampleGrid × SampleGrid điểm ảnh (B, G, R) phủ đều ảnh, toạ độ tính từ góc TRÊN-trái. So TỪNG
        /// điểm chứ không so màu trung bình: ảnh Dome gốc (.bmp) trung bình gần xám (R 71 / B 69) nhưng từng
        /// vùng vẫn lệch màu rõ (đường mạch tím, nền vàng) nên vẫn phân biệt được đúng / đảo kênh.
        /// </summary>
        internal sealed class PixelSamples
        {
            public const int SampleGrid = 200;
            public int Width;
            public int Height;
            public byte[] Bgr;

            public static int At(int index, int size)
            {
                return (int)((long)index * (size - 1) / (SampleGrid - 1));
            }
        }

        /// <summary>Lấy mẫu bitmap 24bpp.</summary>
        private static PixelSamples SamplePixels(Bitmap bitmap)
        {
            int w = bitmap.Width, h = bitmap.Height;
            var data = bitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                const int n = PixelSamples.SampleGrid;
                var samples = new PixelSamples { Width = w, Height = h, Bgr = new byte[n * n * 3] };
                var row = new byte[w * 3];
                for (int j = 0, k = 0; j < n; j++)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, PixelSamples.At(j, h) * data.Stride), row, 0, row.Length);
                    for (int i = 0; i < n; i++, k += 3)
                    {
                        Buffer.BlockCopy(row, PixelSamples.At(i, w) * 3, samples.Bgr, k, 3);
                    }
                }

                return samples;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Lấy mẫu thẳng từ file BMP 24/32 bpp không nén (ảnh .bmp push nguyên trạng, không có bitmap đã giải mã).
        /// null nếu không phải dạng đó.
        /// </summary>
        private static PixelSamples SampleBmp(byte[] bmp)
        {
            if (bmp == null || bmp.Length < 54)
            {
                return null;
            }

            int offset = BitConverter.ToInt32(bmp, 10);
            int w = BitConverter.ToInt32(bmp, 18);
            int rawHeight = BitConverter.ToInt32(bmp, 22);
            int h = Math.Abs(rawHeight);
            int bpp = BitConverter.ToInt16(bmp, 28);
            int compression = BitConverter.ToInt32(bmp, 30);
            if (w <= 0 || h <= 0 || (bpp != 24 && bpp != 32) || (compression != 0 && compression != 3))
            {
                return null;
            }

            int bytesPerPixel = bpp / 8;
            int stride = (w * bytesPerPixel + 3) / 4 * 4;
            if (offset < 0 || (long)offset + (long)stride * h > bmp.Length)
            {
                return null;
            }

            const int n = PixelSamples.SampleGrid;
            var samples = new PixelSamples { Width = w, Height = h, Bgr = new byte[n * n * 3] };
            for (int j = 0, k = 0; j < n; j++)
            {
                int y = PixelSamples.At(j, h);
                int fileRow = rawHeight > 0 ? h - 1 - y : y;   // chiều cao dương = BMP lưu từ dưới lên
                int rowStart = offset + fileRow * stride;
                for (int i = 0; i < n; i++, k += 3)
                {
                    Buffer.BlockCopy(bmp, rowStart + PixelSamples.At(i, w) * bytesPerPixel, samples.Bgr, k, 3);
                }
            }

            return samples;
        }

        /// <summary>
        /// Ảnh kết quả (CHƯA đảo kênh) có cần đảo R/B không: so từng điểm mẫu với ảnh đã push. Không phân biệt
        /// được (ảnh xám thật, khác kích thước, ảnh camera) -> theo lần nhận biết trước (detected), chưa có thì fallback.
        /// </summary>
        private bool ShouldSwapResult(Bitmap result, string sourcePath, ref bool? detected, bool fallback)
        {
            var source = sourcePath == null ? null : _lastPushSamples;
            if (source != null && source.Width == result.Width && source.Height == result.Height)
            {
                var got = SamplePixels(result);
                long same = 0, swapped = 0;
                for (int k = 0; k < got.Bgr.Length; k += 3)
                {
                    same += Math.Abs(got.Bgr[k] - source.Bgr[k]) + Math.Abs(got.Bgr[k + 2] - source.Bgr[k + 2]);
                    swapped += Math.Abs(got.Bgr[k] - source.Bgr[k + 2]) + Math.Abs(got.Bgr[k + 2] - source.Bgr[k]);
                }

                int count = got.Bgr.Length / 3;
                double sameAvg = (double)same / count, swappedAvg = (double)swapped / count;

                // Chênh nhau dưới 1 mức xám / điểm: ảnh gần như xám hoàn toàn, không kết luận được.
                if (Math.Abs(sameAvg - swappedAvg) >= 1)
                {
                    bool swap = swappedAvg < sameAvg;
                    if (detected != swap)
                    {
                        Log($"    Màu ảnh trả về: {(swap ? "ĐẢO kênh R/B so với ảnh push -> đảo lại" : "đúng màu ảnh push -> giữ nguyên")} "
                            + $"(lệch R+B trung bình / điểm: giữ nguyên {sameAvg:0.0}, đảo {swappedAvg:0.0}).");
                    }

                    detected = swap;
                    return swap;
                }
            }

            return detected ?? fallback;
        }

        /// <summary>
        /// Hoán đổi kênh đỏ và xanh dương tại chỗ trên bitmap 24bpp (byte 0 và byte 2 mỗi pixel).
        /// </summary>
        private static void SwapRedBlue(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                int bytesPerRow = bitmap.Width * 3;
                var row = new byte[bytesPerRow];

                for (int y = 0; y < bitmap.Height; y++)
                {
                    IntPtr rowPtr = IntPtr.Add(data.Scan0, y * data.Stride);
                    Marshal.Copy(rowPtr, row, 0, bytesPerRow);

                    for (int x = 0; x < bytesPerRow; x += 3)
                    {
                        byte temp = row[x];
                        row[x] = row[x + 2];
                        row[x + 2] = temp;
                    }

                    Marshal.Copy(row, 0, rowPtr, bytesPerRow);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        #endregion

        #region Lưu ảnh mask

        /// <summary>
        /// Lưu ảnh UnitX trả về vào thư mục con "mask" nằm cạnh ảnh gốc (tự tạo nếu chưa có),
        /// tên = tên ảnh gốc + "mask". Ví dụ: D:\anh\IMG_0001.bmp -> D:\anh\mask\IMG_0001mask.png
        /// Định dạng lưu chọn qua cboImageFormat (xem _maskFormat).
        /// </summary>
        /// <param name="cameraStyle">
        /// true: ảnh nguồn là ảnh trong PATH\raw -> lưu thẳng vào PATH\mask với tên kiểu camera
        /// ({tên}_mask_{OK|NG}{_hậu tố}), vẫn gắn mask vào đúng dòng raw của ảnh nguồn.
        /// </param>
        private string SaveMaskImages(List<Bitmap> bitmaps, string sourcePath, string baseName, string nameTail,
            string decision, bool cameraStyle = false)
        {
            string firstSaved = null;
            string maskFolder = null;

            try
            {
                maskFolder = EnsureOutputFolder(MaskFolderName, cameraStyle ? null : sourcePath);

                ImageKind format = _maskFormat;
                string extension = GetExtension(format);
                int saved = 0;

                foreach (var bitmap in bitmaps)
                {
                    if (bitmap == null)
                    {
                        continue;
                    }

                    saved++;

                    // Ảnh push: "<tên gốc>mask[_N]" - DÍNH LIỀN, giữ đúng quy ước cũ để FindMaskPath
                    //           dò ra được (đổi là hỏng tính năng "chỉ push ảnh còn thiếu").
                    // Ảnh camera: "_mask" cho dễ đọc - không có ai tra ngược theo tên nên thoải mái.
                    string maskMark = sourcePath != null && !cameraStyle ? "mask" : "_mask";
                    string suffix = (saved == 1 ? maskMark : maskMark + "_" + saved) + DecisionTag(decision) + nameTail;
                    string target = BuildMaskPath(maskFolder, baseName, suffix, extension);

                    SaveBitmapAs(bitmap, target, format);
                    Log($"    Đã lưu ảnh MASK: {DescribeOutputPath(target)}");

                    if (saved == 1)
                    {
                        // Ảnh mask đầu tiên là ảnh đại diện cho ảnh gốc -> đưa vào danh sách Mask.
                        firstSaved = target;
                        RegisterMaskFile(target, sourcePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"    Lưu ảnh mask thất bại [{ex.GetType().Name}]: {ex.Message}");
                Log($"    Thư mục đích: {maskFolder ?? "(không xác định)"}");

                if (ex.InnerException != null)
                {
                    Log("    Nguyên nhân gốc: " + ex.InnerException.Message);
                }
            }

            return firstSaved;
        }

        /// <summary>
        /// Lưu ảnh RAW nhận được từ callback (blob[0] - ảnh gốc trước khi vẽ kết quả lên).
        /// Bật/tắt bằng checkbox "Lưu ảnh Raw". Trả về đường dẫn đã lưu, null nếu không lưu được.
        /// </summary>
        private string SaveRawImage(Bitmap bitmap, string sourcePath, string baseName, string nameTail,
            string decision)
        {
            string folder = null;

            try
            {
                folder = EnsureOutputFolder(RawFolderName, sourcePath);

                ImageKind format = _maskFormat;
                string extension = GetExtension(format);

                // {Tên ảnh}_raw_{OK|NG}[_{hậu tố}].<ext> - cùng cấu trúc với ảnh mask ({tên}_mask_OK...)
                // để nhìn tên file là biết ảnh raw; hậu tố timestamp/số thứ tự nằm cuối.
                string target = BuildMaskPath(folder, baseName, RawMark + DecisionTag(decision) + nameTail, extension);

                if (_namePrefixMode == NamePrefixMode.None && File.Exists(target))
                {
                    Log("    LƯU Ý: đã có file cùng tên và đang để Hậu tố = 'Không có' -> ghi đè "
                        + Path.GetFileName(target) + ". Chọn Timestamp hoặc Số thứ tự để giữ lại ảnh cũ.");
                }

                SaveBitmapAs(bitmap, target, format);
                Log($"    Đã lưu ảnh RAW : {DescribeOutputPath(target)}");
                return target;
            }
            catch (Exception ex)
            {
                Log($"    Lưu ảnh raw thất bại [{ex.GetType().Name}]: {ex.Message}");
                Log($"    Thư mục đích: {folder ?? "(không xác định)"}");
                return null;
            }
        }

        #region Thư mục lưu ảnh nhận về

        /// <summary>
        /// Thư mục gốc để lưu ảnh nhận về = PATH người dùng nhập. Chưa nhập/không tồn tại thì lùi về
        /// thư mục cấu hình của ứng dụng để không bao giờ mất ảnh.
        /// </summary>
        private string GetOutputRoot()
        {
            string root = _imageRootFolder;
            return !string.IsNullOrWhiteSpace(root) && Directory.Exists(root) ? root : AppDataDir;
        }

        /// <summary>
        /// Dựng (và tạo nếu chưa có) thư mục đích: PATH\raw\... hoặc PATH\mask\...
        ///
        /// Nếu ảnh gốc nằm trong thư mục con của PATH thì cấu trúc thư mục con được GIỮ NGUYÊN bên
        /// dưới raw/mask - nếu gom phẳng hết vào một thư mục thì hai ảnh trùng tên ở hai thư mục con
        /// khác nhau sẽ ghi đè lên nhau (quét thư mục luôn bao gồm thư mục con).
        /// </summary>
        private string EnsureOutputFolder(string subFolder, string sourcePath)
        {
            string root = GetOutputRoot();
            string folder = Path.Combine(root, subFolder);

            string relativeDir = GetRelativeDir(root, sourcePath);
            if (!string.IsNullOrEmpty(relativeDir))
            {
                folder = Path.Combine(folder, relativeDir);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        /// <summary>
        /// Thư mục con của ảnh gốc tính từ thư mục gốc (rỗng nếu ảnh nằm ngay trong thư mục gốc,
        /// null nếu ảnh nằm ngoài thư mục gốc hoặc không xác định được).
        /// </summary>
        private static string GetRelativeDir(string root, string sourcePath)
        {
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(sourcePath))
            {
                return null;
            }

            try
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(sourcePath));
                string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);

                if (string.IsNullOrEmpty(dir))
                {
                    return null;
                }

                if (string.Equals(dir, fullRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }

                string prefix = fullRoot + Path.DirectorySeparatorChar;
                if (dir.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return dir.Substring(prefix.Length);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hậu tố kết quả gắn vào CUỐI tên file: "_OK" hoặc "_NG" (rỗng khi UnitX không kèm kết quả,
        /// ví dụ ảnh calib - lúc đó tên file giữ nguyên như trước).
        ///
        /// Theo đúng quy ước của frmMainV11 (`partImageResult.decision != Decision.DECISION_NG`):
        /// chỉ NG mới là NG, mọi giá trị khác (kể cả DECISION_LIMIT) đều coi là đạt. Muốn tách riêng
        /// LIMIT thành nhãn thứ ba thì sửa đúng dòng dưới đây.
        /// </summary>
        private static string DecisionTag(string decision)
        {
            if (string.IsNullOrEmpty(decision))
            {
                return string.Empty;
            }

            return string.Equals(decision, "NG", StringComparison.OrdinalIgnoreCase) ? "_NG" : "_OK";
        }

        /// <summary>Rút gọn đường dẫn để ghi log cho dễ đọc (bỏ phần thư mục gốc).</summary>
        private string DescribeOutputPath(string fullPath)
        {
            try
            {
                string root = Path.GetFullPath(GetOutputRoot()).TrimEnd(Path.DirectorySeparatorChar);
                string full = Path.GetFullPath(fullPath);
                string prefix = root + Path.DirectorySeparatorChar;

                return full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? full.Substring(prefix.Length)
                    : full;
            }
            catch
            {
                return fullPath;
            }
        }

        #endregion

        /// <summary>
        /// Ghi bitmap ra file theo định dạng chỉ định (PNG/JPG/BMP/TIFF).
        ///
        /// KHÔNG dùng Bitmap.Save(đường_dẫn): GDI+ nuốt mọi lỗi file (không có quyền ghi, file đang
        /// bị khoá, đường dẫn quá dài, đầy đĩa, ghi qua ổ mạng) rồi ném đúng một câu vô nghĩa
        /// "A generic error occurred in GDI+". Encode vào RAM rồi ghi bằng File I/O của .NET thì lỗi
        /// thật hiện nguyên hình. Việc encode được khoá lại vì nhiều callback có thể chạy song song.
        /// </summary>
        private static readonly Lazy<System.Drawing.Imaging.ImageCodecInfo> JpegEncoder = new Lazy<System.Drawing.Imaging.ImageCodecInfo>(() =>
            System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid));

        private void SaveBitmapAs(Bitmap bitmap, string targetPath, ImageKind format)
        {
            byte[] encoded;
            int length;

            lock (_encodeLock)
            {
                _maskStream.SetLength(0);
                _maskStream.Position = 0;

                if (format == ImageKind.Jpeg)
                {
                    // JPG: mã hoá theo % chất lượng người dùng nhập (mặc định của GDI+ là 75).
                    using (var parameters = new System.Drawing.Imaging.EncoderParameters(1))
                    {
                        parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality,
                            (long)Math.Max(1, Math.Min(100, _jpegQuality)));
                        bitmap.Save(_maskStream, JpegEncoder.Value, parameters);
                    }
                }
                else
                {
                    bitmap.Save(_maskStream, ToImageFormat(format));
                }

                encoded = _maskStream.GetBuffer();
                length = (int)_maskStream.Length;

                using (var file = new FileStream(targetPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, 1 << 20, FileOptions.SequentialScan))
                {
                    file.Write(encoded, 0, length);
                }
            }
        }

        /// <summary>
        /// Dựng đường dẫn ảnh mask, cắt bớt tên nếu vượt giới hạn 260 ký tự của Windows.
        /// </summary>
        private static string BuildMaskPath(string folder, string baseName, string suffix, string extension)
        {
            string target = Path.Combine(folder, baseName + suffix + extension);

            if (target.Length < 250)
            {
                return target;
            }

            int room = 250 - folder.Length - suffix.Length - extension.Length - 2;
            if (room > 8)
            {
                baseName = baseName.Substring(0, room);
            }
            else
            {
                baseName = Math.Abs(baseName.GetHashCode()).ToString();
            }

            return Path.Combine(folder, baseName + suffix + extension);
        }

        /// <summary>
        /// Đường dẫn ảnh mask của một ảnh gốc nếu đã tồn tại, dò cả 4 định dạng có thể đã lưu
        /// (đổi định dạng lưu giữa các lần chạy sẽ không khiến ảnh cũ bị coi là "còn thiếu").
        /// Trả về null nếu chưa có.
        /// </summary>
        private string FindMaskPath(string sourcePath)
        {
            try
            {
                string baseName = Path.GetFileNameWithoutExtension(sourcePath) + "mask";

                // 1. Vị trí HIỆN TẠI: <PATH>\mask\<thư mục con của ảnh gốc>\
                string root = GetOutputRoot();
                string newFolder = Path.Combine(root, MaskFolderName);
                string relativeDir = GetRelativeDir(root, sourcePath);
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    newFolder = Path.Combine(newFolder, relativeDir);
                }

                // 2. Vị trí CŨ (các bản trước lưu mask ngay cạnh ảnh gốc) - vẫn phải dò, nếu không
                //    những ảnh đã xử lý xong từ trước sẽ bị coi là "còn thiếu" và push lại.
                string legacyFolder = Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, MaskFolderName);

                foreach (var folder in new[] { newFolder, legacyFolder })
                {
                    if (string.IsNullOrEmpty(folder))
                    {
                        continue;
                    }

                    // Tên mask giờ có thể kèm hậu tố kết quả (_OK/_NG). Dò đúng tập hữu hạn các tên
                    // mà ứng dụng từng ghi ra thay vì liệt kê cả thư mục - hàm này chạy cho TỪNG ảnh
                    // khi quét, liệt kê thư mục mỗi lần sẽ thành O(n²) trên thư mục vài nghìn ảnh.
                    foreach (var tag in MaskDecisionTags)
                    {
                        foreach (var ext in MaskExtensions)
                        {
                            string candidate = Path.Combine(folder, baseName + tag + ext);
                            if (File.Exists(candidate))
                            {
                                return candidate;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Ảnh đã được UnitX xử lý xong hay chưa - căn cứ vào sự tồn tại của ảnh mask.
        /// Đây là tiêu chí duy nhất để biết ảnh nào CÒN THIẾU khi chạy lại.
        /// </summary>
        private bool HasMask(string sourcePath)
        {
            return ResolveMaskPath(sourcePath) != null;
        }

        /// <summary>
        /// File nằm trong PATH\raw (ảnh raw do ứng dụng lưu / camera chụp). Push loại ảnh này thì CHÍNH
        /// NÓ là ảnh RAW: chỉ lưu ảnh mask trả về (đặt tên theo quy ước camera), không lưu thêm bản raw.
        /// </summary>
        private bool IsAppRawFile(string path)
        {
            string root = _imageRootFolder;
            return !string.IsNullOrEmpty(root) && !string.IsNullOrEmpty(path)
                && ClassifyScannedFile(root, path) == ScannedKind.RawOutput;
        }

        /// <summary>Ảnh mask của một ảnh raw: quy ước camera cho ảnh trong PATH\raw, quy ước push cho ảnh nguồn.</summary>
        private string ResolveMaskPath(string rawPath)
        {
            return IsAppRawFile(rawPath) ? FindCameraMaskPath(rawPath) : FindMaskPath(rawPath);
        }

        private static string GetExtension(ImageKind kind)
        {
            switch (kind)
            {
                case ImageKind.Png: return ".png";
                case ImageKind.Bmp: return ".bmp";
                case ImageKind.Tiff: return ".tif";
                case ImageKind.Jpeg: return ".jpg";
                default: return ".png";
            }
        }

        #endregion

        #region Chẩn đoán / trạng thái hệ thống

        /// <summary>
        /// Ghi nhận part_type / capture_config / camera_id hợp lệ do UnitX báo về và bổ sung vào
        /// các combobox tương ứng. Đây là nguồn đáng tin nhất để biết phải điền chuỗi gì.
        /// </summary>
        private void Discover(string partType, string captureConfig, string cameraId = null)
        {
            Action apply = () =>
            {
                if (!string.IsNullOrWhiteSpace(partType) && !_partTypeSuggestions.Contains(partType))
                {
                    ApplyPartTypes(new[] { partType });
                    Log($"   => Đã thêm part_type hợp lệ (UnitX báo về): '{partType}'");
                }

                if (!string.IsNullOrWhiteSpace(captureConfig) && !cboCaptureConfig.Items.Contains(captureConfig))
                {
                    cboCaptureConfig.Items.Add(captureConfig);
                    Log($"   => Đã thêm capture_config hợp lệ vào danh sách: '{captureConfig}'");
                }

                if (!string.IsNullOrWhiteSpace(cameraId))
                {
                    if (!cboCameraID.Items.Contains(cameraId))
                    {
                        cboCameraID.Items.Add(cameraId);
                    }

                    // Không ghi đè nếu người dùng đã tự nhập/chọn giá trị khác.
                    if (string.IsNullOrWhiteSpace(cboCameraID.Text))
                    {
                        cboCameraID.Text = cameraId;
                        Log($"   => Đã tự động điền Camera ID lấy từ IMAGE_RESULT: '{cameraId}'");
                    }

                    SetCameraState(ComponentState.Ok, "Sẵn sàng");
                }
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Lấy edge handler theo đúng logic của wrapper: edge rỗng thì dùng edge mặc định.
        /// </summary>
        private IntPtr GetEdgeHandler()
        {
            string edge = (_edge ?? string.Empty).Trim();
            return string.IsNullOrEmpty(edge) ? get_default_edge_handler() : get_edge_handler_with_name(edge);
        }

        /// <summary>Kết quả get_system_status / get_mode lần gần nhất (null = chưa đọc được).</summary>
        private UnitXSystemStatus? _lastSystemStatus;
        private int? _lastMode;

        /// <summary>
        /// unitx_sdk_sample.cpp kiểm tra system_status.code == 1 trước khi push ảnh ngoài.
        /// </summary>
        private bool ReadSystemStatus()
        {
            IntPtr statusPtr = IntPtr.Zero;

            try
            {
                IntPtr edgeHandler = GetEdgeHandler();
                if (edgeHandler == IntPtr.Zero)
                {
                    Log("System status: không lấy được edge handler.");
                    return false;
                }

                statusPtr = get_system_status(edgeHandler);
                if (statusPtr == IntPtr.Zero)
                {
                    Log("System status: NULL.");
                    return false;
                }

                var status = Marshal.PtrToStructure<UnitXSystemStatus>(statusPtr);
                _lastSystemStatus = status;
                Log($"System status: code={status.code} ({status.status}), CPU={status.cpu}%, GPU={status.gpu}%, "
                    + $"RAM={status.use_memory}/{status.total_memory} MB");

                LogCurrentMode(edgeHandler);

                if (status.code == 1)
                {
                    SetSdkState(ComponentState.Ok, "Sẵn sàng");
                }
                else
                {
                    SetSdkState(ComponentState.Error, "Lỗi (code " + status.code + ")");
                }

                if (status.code != 1)
                {
                    Log("=> Máy UnitX KHÔNG ở trạng thái sẵn sàng (code != 1), push ảnh sẽ hỏng.");
                }

                return status.code == 1;
            }
            catch (Exception ex)
            {
                Log("get_system_status lỗi: " + ex.Message);
                return false;
            }
            finally
            {
                if (statusPtr != IntPtr.Zero)
                {
                    try { free_system_status(statusPtr); } catch { }
                }
            }
        }

        #region UnitXProdConfiguration

        // Kích thước theo unitx_sdk_interface.h. Struct chứa mảng char 2 chiều nên đọc thủ công
        // theo offset thay vì để CLR marshal - vừa an toàn vừa dễ đối chiếu với header.
        private const int MaxNameLength = 128;
        private const int MaxVersionLength = 32;
        private const int MaxIdLength = 64;
        private const int MaxImagingConfigLength = 4096;
        private const int MaxCameraCount = 6;
        private const int MaxControllerCount = 6;
        private const int MaxCaptureCount = 16;
        private const int MaxNetworkCount = 16;

        private const int NetworkSize = MaxNameLength + MaxVersionLength + 4;                        // 164
        private const int CameraControllerSize = MaxIdLength + MaxIdLength;                          // 128
        private const int CameraCaptureSize = MaxIdLength + MaxCaptureCount * MaxNameLength + 4;     // 2116
        private const int CaptureNetworkSize = MaxNameLength + MaxNetworkCount * MaxNameLength + 4;  // 2180

        private static string ReadAnsi(byte[] buffer, int offset, int maxLength)
        {
            int end = offset;
            int limit = Math.Min(offset + maxLength, buffer.Length);

            while (end < limit && buffer[end] != 0)
            {
                end++;
            }

            return Encoding.ASCII.GetString(buffer, offset, end - offset);
        }

        /// <summary>
        /// Đọc cấu hình sản xuất của UnitX để biết CHÍNH XÁC danh sách camera và capture config hợp
        /// lệ thay vì đoán. Camera đọc được sẽ nạp vào Camera ID / Sequence IO, cc nạp vào Capture Config.
        /// </summary>
        private ProdConfigData ReadProdConfiguration()
        {
            IntPtr configPtr = IntPtr.Zero;
            var info = new ProdConfigData();
            _lastProdConfig = info;

            try
            {
                IntPtr edgeHandler = GetEdgeHandler();
                if (edgeHandler == IntPtr.Zero)
                {
                    Log("Prod configuration: không lấy được edge handler.");
                    info.Error = "không lấy được edge handler";
                    return info;
                }

                configPtr = get_prod_configuration(edgeHandler);
                if (configPtr == IntPtr.Zero)
                {
                    Log("get_prod_configuration() trả về NULL.");
                    info.Error = "get_prod_configuration() trả về NULL";
                    return info;
                }

                int totalSize =
                    MaxNetworkCount * NetworkSize + 4 +
                    MaxControllerCount * MaxIdLength + 4 +
                    MaxCameraCount * MaxIdLength + 4 +
                    MaxImagingConfigLength +
                    MaxControllerCount * CameraControllerSize + 4 +
                    MaxCaptureCount * CameraCaptureSize + 4 +
                    MaxNetworkCount * CaptureNetworkSize + 4;

                var buffer = new byte[totalSize];
                Marshal.Copy(configPtr, buffer, 0, totalSize);

                int offset = 0;

                int networksOffset = offset;
                offset += MaxNetworkCount * NetworkSize;
                int networkCount = BitConverter.ToInt32(buffer, offset);
                offset += 4;

                int controllersOffset = offset;
                offset += MaxControllerCount * MaxIdLength;
                int controllerCount = BitConverter.ToInt32(buffer, offset);
                offset += 4;

                int camerasOffset = offset;
                offset += MaxCameraCount * MaxIdLength;
                int cameraCount = BitConverter.ToInt32(buffer, offset);
                offset += 4;

                int imagingConfigOffset = offset;
                offset += MaxImagingConfigLength;

                int cameraMapControllerOffset = offset;
                offset += MaxControllerCount * CameraControllerSize;
                int cameraMapControllerCount = BitConverter.ToInt32(buffer, offset);
                offset += 4;

                int cameraMapCcOffset = offset;
                offset += MaxCaptureCount * CameraCaptureSize;
                int cameraMapCcCount = BitConverter.ToInt32(buffer, offset);
                offset += 4;

                int ccMapNetworkOffset = offset;
                offset += MaxNetworkCount * CaptureNetworkSize;
                int ccMapNetworkCount = BitConverter.ToInt32(buffer, offset);

                Log("===== CẤU HÌNH SẢN XUẤT CỦA UNITX =====");

                for (int i = 0; i < Math.Min(networkCount, MaxNetworkCount); i++)
                {
                    int b = networksOffset + i * NetworkSize;
                    string network = ReadAnsi(buffer, b, MaxNameLength);
                    string version = ReadAnsi(buffer, b + MaxNameLength, MaxVersionLength);
                    info.Networks.Add(new KeyValuePair<string, string>(network, version));
                    Log($"network: {network} (version {version})");
                }

                for (int i = 0; i < Math.Min(controllerCount, MaxControllerCount); i++)
                {
                    string controller = ReadAnsi(buffer, controllersOffset + i * MaxIdLength, MaxIdLength);
                    info.Controllers.Add(controller);
                    Log("controller: " + controller);
                }

                var cameraNames = new List<string>();

                for (int i = 0; i < Math.Min(cameraCount, MaxCameraCount); i++)
                {
                    string camera = ReadAnsi(buffer, camerasOffset + i * MaxIdLength, MaxIdLength);
                    info.Cameras.Add(camera);
                    Log("camera: " + camera);

                    if (!string.IsNullOrWhiteSpace(camera) && !cameraNames.Contains(camera))
                    {
                        cameraNames.Add(camera);
                    }
                }

                for (int i = 0; i < Math.Min(cameraMapControllerCount, MaxControllerCount); i++)
                {
                    int b = cameraMapControllerOffset + i * CameraControllerSize;
                    string mappedCamera = ReadAnsi(buffer, b, MaxIdLength);
                    string mappedController = ReadAnsi(buffer, b + MaxIdLength, MaxIdLength);
                    info.CameraController[mappedCamera] = mappedController;
                    Log($"camera {mappedCamera} -> controller {mappedController}");
                }

                var captureNames = new List<string>();

                for (int i = 0; i < Math.Min(cameraMapCcCount, MaxCaptureCount); i++)
                {
                    int b = cameraMapCcOffset + i * CameraCaptureSize;
                    string cameraId = ReadAnsi(buffer, b, MaxIdLength);
                    int nameCount = BitConverter.ToInt32(buffer, b + MaxIdLength + MaxCaptureCount * MaxNameLength);

                    if (!string.IsNullOrWhiteSpace(cameraId) && !cameraNames.Contains(cameraId))
                    {
                        cameraNames.Add(cameraId);
                    }

                    for (int j = 0; j < Math.Min(nameCount, MaxCaptureCount); j++)
                    {
                        string cc = ReadAnsi(buffer, b + MaxIdLength + j * MaxNameLength, MaxNameLength);
                        Log($"camera {cameraId} -> capture_config HỢP LỆ: '{cc}'");

                        List<string> ccs;
                        if (!info.CameraCaptureConfigs.TryGetValue(cameraId, out ccs))
                        {
                            info.CameraCaptureConfigs[cameraId] = ccs = new List<string>();
                        }

                        ccs.Add(cc);

                        if (!string.IsNullOrWhiteSpace(cc) && !captureNames.Contains(cc))
                        {
                            captureNames.Add(cc);
                        }
                    }
                }

                for (int i = 0; i < Math.Min(ccMapNetworkCount, MaxNetworkCount); i++)
                {
                    int b = ccMapNetworkOffset + i * CaptureNetworkSize;
                    string cc = ReadAnsi(buffer, b, MaxNameLength);
                    int nameCount = BitConverter.ToInt32(buffer, b + MaxNameLength + MaxNetworkCount * MaxNameLength);

                    var ccNetworks = new List<string>();
                    for (int j = 0; j < Math.Min(nameCount, MaxNetworkCount); j++)
                    {
                        string ccNetwork = ReadAnsi(buffer, b + MaxNameLength + j * MaxNameLength, MaxNameLength);
                        ccNetworks.Add(ccNetwork);
                        Log($"capture_config '{cc}' -> network {ccNetwork}");
                    }

                    info.CaptureNetworks[cc] = ccNetworks;

                    if (!string.IsNullOrWhiteSpace(cc) && !captureNames.Contains(cc))
                    {
                        captureNames.Add(cc);
                    }
                }

                string imagingConfig = ReadAnsi(buffer, imagingConfigOffset, MaxImagingConfigLength);
                info.ImagingConfig = imagingConfig;
                if (!string.IsNullOrWhiteSpace(imagingConfig))
                {
                    Log("imaging_config (đầy đủ): " + imagingConfig);
                    MinePartTypesAndSequences(imagingConfig);

                    // Camera / capture config khai báo trong imaging_config: struct có thể để trống
                    // các bảng ánh xạ (log 2026-09-26: cc "test" chỉ nằm trong cc_network_mapping).
                    info.ParseImagingConfig();
                    foreach (var part in info.PartTypes)
                    {
                        foreach (var source in part.Sources)
                        {
                            Log($"part_type '{part.Name}' -> {source.Source} {source.CameraId}: "
                                + string.Join(", ", source.CaptureNetworks.Select(c => c.Key + " -> " + string.Join("/", c.Value)))
                                + (source.Repeat != null ? ", repeat " + source.Repeat : string.Empty));
                        }
                    }

                    if (info.ImagingConfigError != null)
                    {
                        Log("imaging_config không đọc được thành cây: " + info.ImagingConfigError);
                    }
                }
                else
                {
                    Log("imaging_config rỗng.");
                }

                cameraNames = cameraNames.Union(info.AllCameras, StringComparer.OrdinalIgnoreCase).ToList();
                captureNames = captureNames.Union(info.AllCaptureConfigs, StringComparer.OrdinalIgnoreCase).ToList();

                ApplyProdConfiguration(captureNames, cameraNames);
                Log("=======================================");

                info.Ok = true;
            }
            catch (Exception ex)
            {
                Log("get_prod_configuration lỗi: " + ex.Message);
                info.Error = ex.Message;
            }
            finally
            {
                if (configPtr != IntPtr.Zero)
                {
                    try { free_prod_configuration(configPtr); } catch { }
                }
            }

            return info;
        }

        /// <summary>Cấu hình sản xuất đọc được lần gần nhất - tab "Cấu hình sản xuất" của cửa sổ trạng thái.</summary>
        private ProdConfigData _lastProdConfig;

        /// <summary>
        /// Đổ danh sách capture_config / camera đọc được từ UnitX lên các combobox tương ứng.
        /// </summary>
        private void ApplyProdConfiguration(List<string> captureNames, List<string> cameraNames)
        {
            Action apply = () =>
            {
                if (captureNames.Count > 0)
                {
                    string current = cboCaptureConfig.Text;
                    cboCaptureConfig.Items.Clear();

                    foreach (var cc in captureNames)
                    {
                        cboCaptureConfig.Items.Add(cc);
                    }

                    if (!captureNames.Contains(current))
                    {
                        cboCaptureConfig.SelectedIndex = 0;
                        Log($"=> Đã thay capture_config bằng giá trị hợp lệ đầu tiên: '{cboCaptureConfig.Text}'");
                    }
                    else
                    {
                        cboCaptureConfig.Text = current;
                    }
                }
                else
                {
                    Log("Không đọc được capture config nào từ cấu hình sản xuất.");
                }

                foreach (var camera in cameraNames)
                {
                    if (!cboCameraID.Items.Contains(camera))
                    {
                        cboCameraID.Items.Add(camera);
                    }
                }

                if (cameraNames.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(cboCameraID.Text))
                    {
                        cboCameraID.Text = cameraNames[0];
                    }

                    SetCameraState(ComponentState.Ok, "Sẵn sàng");
                }
            };

            RunOnUi(apply);
        }

        #endregion

        /// <summary>
        /// Kiểm tra vì sao PushImage trả về false. Ba nguyên nhân thường gặp:
        ///   1. SDK chưa kết nối được edge  -> get_edge_count() = 0.
        ///   2. Sai tên edge                -> get_edge_handler_with_name() = NULL.
        ///   3. Part chưa được StartPart / sai part_type / sai capture_config.
        /// </summary>
        private void RunDiagnostics()
        {
            // Không có client.yaml / SDK chưa chạy: gọi vào unitx_sdk.dll lúc này chỉ nhận SEHException.
            if (!_sdkRunning || !File.Exists(SdkConfigPath))
            {
                LogWarning("Bỏ qua chẩn đoán UnitX: " + (!File.Exists(SdkConfigPath)
                    ? "chưa có file cấu hình mạng " + SdkConfigPath + "."
                    : "SDK chưa chạy."));
                return;
            }

            Log("----- CHẨN ĐOÁN UNITX -----");
            Log("SDK đang chạy (RunSDK đã gọi): " + _sdkRunning);

            int edgeCount = -1;
            try
            {
                edgeCount = get_edge_count();
                Log($"get_edge_count() = {edgeCount}");
            }
            catch (Exception ex)
            {
                Log("get_edge_count() lỗi: " + ex.Message);
            }

            if (edgeCount == 0)
            {
                Log("=> KHÔNG có edge nào. SDK chưa kết nối được tới máy UnitX.");
                Log("   Kiểm tra: máy UnitX đã bật? đúng mạng/IP? cấu hình edge của SDK? tường lửa?");
            }

            try
            {
                IntPtr defaultEdge = get_default_edge_handler();
                Log($"get_default_edge_handler() = 0x{defaultEdge.ToInt64():X}"
                    + (defaultEdge == IntPtr.Zero ? "  (NULL - không có edge mặc định)" : ""));
            }
            catch (Exception ex)
            {
                Log("get_default_edge_handler() lỗi: " + ex.Message);
            }

            string edge = (_edge ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(edge))
            {
                Log("Edge để trống => wrapper sẽ dùng edge mặc định (get_default_edge_handler).");
            }
            else
            {
                try
                {
                    IntPtr handler = get_edge_handler_with_name(edge);
                    if (handler == IntPtr.Zero)
                    {
                        Log($"get_edge_handler_with_name(\"{edge}\") = NULL => TÊN EDGE SAI.");
                        Log("   => Sửa \"Edge\" trong " + SettingsPath + " thành rỗng để dùng edge mặc định.");
                    }
                    else
                    {
                        Log($"get_edge_handler_with_name(\"{edge}\") = 0x{handler.ToInt64():X}  (OK)");
                    }
                }
                catch (Exception ex)
                {
                    Log("get_edge_handler_with_name() lỗi: " + ex.Message);
                }
            }

            Log($"Cấu hình hiện tại: part_type='{cboPartType.Text}', part_id='{txtPartID.Text}', "
                + $"capture_config='{cboCaptureConfig.Text}', edge='{edge}'");
            Log($"Số callback nhận được từ UnitX: {_callbackCount}");
            Log("Xem thêm chi tiết trong file log của SDK: " + SdkLogPath);
            Log("---------------------------");
        }

        #endregion

        #region Lệnh điều khiển Part / Camera

        /// <summary>
        /// Kiểm tra điều kiện chung cho mọi lệnh gọi SDK thủ công (Part Start, Camera Trigger...).
        /// </summary>
        private bool EnsureSdkReady()
        {
            if (!_sdkRunning)
            {
                MessageBox.Show(this, "Chưa kết nối UnitX.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Camera ID lấy từ ô Camera ID - do người dùng chọn/gõ, hoặc do Discover() tự điền từ
        /// camera_id thấy được trong IMAGE_RESULT gần nhất.
        /// </summary>
        private string GetCameraId()
        {
            string cameraId = (cboCameraID.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cameraId))
            {
                MessageBox.Show(this,
                    "Chưa có Camera ID. Chọn/nhập trực tiếp vào ô 'Camera ID', hoặc chờ một IMAGE_RESULT "
                    + "để ô này tự động điền.",
                    "Thiếu Camera ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return cameraId;
        }

        /// <summary>
        /// Sequence IO = sequence_number (số nguyên) truyền vào select_sequence(edge, camera_id,
        /// sequence_number). Code mẫu của hãng dùng 1 hoặc 2 tuỳ camera.
        /// Trả về -1 nếu người dùng nhập không hợp lệ (đã báo bằng MessageBox).
        /// </summary>
        private int GetSequenceNumber()
        {
            string text = (cboSequenceIO.Text ?? string.Empty).Trim();
            string cameraId = (cboCameraID.Text ?? string.Empty).Trim();

            int sequenceNumber;
            bool numeric = int.TryParse(text, out sequenceNumber);

            // Ô này được TỰ ĐIỀN bằng camera_id. camera_id của hãng là chuỗi số dài (vd 40497555)
            // nên nếu cứ ép thành số thì select_sequence() sẽ nhận một sequence_number vô nghĩa.
            // Trường hợp đó dùng lại số sequence hợp lệ gần nhất người dùng đã chọn.
            if (!string.IsNullOrEmpty(cameraId) && string.Equals(text, cameraId, StringComparison.Ordinal))
            {
                Log($"Ô Sequence IO đang hiển thị camera_id '{cameraId}' -> "
                    + $"select_sequence dùng sequence_number = {_sequenceNumber}. "
                    + "Chọn một số 0..5 trong danh sách nếu muốn sequence khác.");
                return _sequenceNumber;
            }

            if (!numeric || sequenceNumber < 0)
            {
                MessageBox.Show(this,
                    "Sequence IO phải là một số nguyên không âm (số thứ tự sequence của camera, "
                    + "ví dụ 1 hoặc 2 như trong code mẫu của hãng), hoặc để nguyên camera_id "
                    + "được tự điền.",
                    "Sequence IO không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            if (sequenceNumber <= MaxSequenceNumber)
            {
                _sequenceNumber = sequenceNumber;
            }

            return sequenceNumber;
        }

        /// <summary>
        /// Tên edge AN TOÀN để đưa cho wrapper (StartPart/RemovePart/PushImage). Wrapper chỉ kiểm
        /// tra chuỗi rỗng rồi gọi get_edge_handler_with_name() - tên sai thì nó truyền handler NULL
        /// xuống SDK lõi. Trả về chuỗi rỗng (= dùng edge mặc định) khi tên không phân giải được.
        /// </summary>
        private string ResolveEdgeName()
        {
            string edge = (_edge ?? string.Empty).Trim();
            if (edge.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                if (get_edge_handler_with_name(edge) == IntPtr.Zero)
                {
                    Log($"Edge '{edge}' không có trong client.yaml -> dùng edge mặc định.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log("Không kiểm tra được edge '" + edge + "': " + ex.Message + " -> dùng edge mặc định.");
                return string.Empty;
            }

            return edge;
        }

        /// <summary>
        /// Edge handler để gọi thẳng vào SDK lõi. Ưu tiên edge có tên (_edge), nếu tên sai/không có
        /// thì lùi về edge mặc định như wrapper vẫn làm - và ghi log để lỗi cấu hình không bị giấu.
        /// Trả về IntPtr.Zero kèm thông báo nếu không lấy được edge nào.
        /// </summary>
        private IntPtr GetEdgeHandlerOrWarn()
        {
            IntPtr handler = IntPtr.Zero;
            string edge = (_edge ?? string.Empty).Trim();

            try
            {
                if (!string.IsNullOrEmpty(edge))
                {
                    handler = get_edge_handler_with_name(edge);
                    if (handler == IntPtr.Zero)
                    {
                        Log($"get_edge_handler_with_name(\"{edge}\") = NULL -> dùng edge mặc định thay thế.");
                    }
                }

                if (handler == IntPtr.Zero)
                {
                    handler = get_default_edge_handler();
                }
            }
            catch (Exception ex)
            {
                Log("Không lấy được edge handler: " + ex.Message);
                handler = IntPtr.Zero;
            }

            if (handler == IntPtr.Zero)
            {
                MessageBox.Show(this,
                    "Không lấy được edge handler (get_default_edge_handler trả về NULL)."
                    + Environment.NewLine
                    + "SDK chưa kết nối được tới máy UnitX - bấm 'Kiểm tra' để xem chẩn đoán.",
                    "Chưa có edge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return handler;
        }

        /// <summary>Gọi một hàm SDK trả về bool, ghi log và báo lỗi thống nhất cho mọi nút.</summary>
        private void InvokeSdkCommand(string description, Func<bool> action)
        {
            try
            {
                Log("-> " + description);
                long logMark = UnitXSdkLog.Mark(SdkLogPath);
                bool ok = action();
                Log($"<- {description} => {ok}");

                UpdateStatus(ok ? description + ": OK" : description + ": THẤT BẠI");

                if (!ok)
                {
                    // Lý do thật nằm trong unitx_sdk.log (vd "Camera id ... does not exist") - đọc ra và
                    // diễn giải thay vì bảo người dùng tự mở file log.
                    var failure = UnitXSdkLog.Explain(SdkLogPath, logMark);
                    LogError($"{description} thất bại: {failure.Reason}"
                        + (failure.Response != null ? " | UnitX: " + failure.Response : string.Empty));

                    MessageBox.Show(this, failure.ToMessage(), description + " thất bại",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log(description + " lỗi: " + ex.Message);
                MessageBox.Show(this, description + " lỗi:" + Environment.NewLine + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPartStart_Click(object sender, EventArgs e)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                return;
            }

            config.Edge = ResolveEdgeName();

            InvokeSdkCommand($"StartPart('{config.PartType}', '{config.PartId}', '{config.Edge}')",
                () => StartPart(config.PartType, config.PartId, config.Edge));
        }

        private void btnRemovePart_Click(object sender, EventArgs e)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                return;
            }

            var answer = MessageBox.Show(this,
                $"Xoá part group hiện tại trên UnitX?{Environment.NewLine}"
                + $"part_type = {config.PartType}{Environment.NewLine}part_id = {config.PartId}",
                "Xác nhận Remove Part", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            config.Edge = ResolveEdgeName();

            InvokeSdkCommand($"RemovePart('{config.PartType}', '{config.PartId}', '{config.Edge}')",
                () => RemovePart(config.PartType, config.PartId, config.Edge));
        }

        private void btnSystemReset_Click(object sender, EventArgs e)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            var answer = MessageBox.Show(this,
                "Reset toàn bộ hệ thống UnitX (ResetSystem)?",
                "Xác nhận System Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            IntPtr systemEdge = GetEdgeHandlerOrWarn();
            if (systemEdge == IntPtr.Zero)
            {
                return;
            }

            InvokeSdkCommand($"reset_system(edge=0x{systemEdge.ToInt64():X})",
                () => reset_system(systemEdge));
        }

        /// <summary>
        /// Chụp thủ công một camera. Gọi thẳng trigger_camera(edge_handler, camera_id) của SDK lõi
        /// vì wrapper unitx_sdk_interface.dll KHÔNG export hàm này (xem ghi chú ở phần khai báo).
        /// </summary>
        private void btnTriggerOnce_Click(object sender, EventArgs e)
        {
            // Đang chạy Trigger Repeat: chính nút này là nút Dừng.
            if (IsTriggering && _triggerRepeatTotal > 0)
            {
                StopContinuousTrigger("người dùng bấm dừng repeat");
                return;
            }

            // Trigger repeat > 1: chụp đúng số lần đó, mỗi lần đợi kết quả về + nhịp "Delay ms".
            int repeat = (int)numTriggerRepeat.Value;
            if (repeat > 1)
            {
                StartTriggerLoop(repeat);
                return;
            }

            if (!EnsureSdkReady())
            {
                return;
            }

            string cameraId = GetCameraId();
            if (cameraId == null)
            {
                return;
            }

            IntPtr edgeHandler = GetEdgeHandlerOrWarn();
            if (edgeHandler == IntPtr.Zero)
            {
                return;
            }

            LogCurrentMode(edgeHandler);

            // Ghi nhớ camera vừa trigger + mốc thời gian: callback dùng hai giá trị này để biết
            // IMAGE_RESULT sắp tới là ảnh CAMERA CHỤP chứ không phải kết quả của một lượt push,
            // nhờ đó không đụng vào hàng đợi _pendingPushes và vẫn hiển thị được ảnh.
            _lastTriggeredCameraId = cameraId;
            Interlocked.Exchange(ref _lastTriggerTs, Stopwatch.GetTimestamp());
            // Kiểm tra + tạo thư mục NGAY LÚC BẤM, không đợi tới lúc callback trả ảnh về: lỗi
            // đường dẫn lúc đó nằm trong luồng SDK, chỉ hiện trong log chứ không chặn được người dùng.
            if (ResolveImageRoot(true, true) == null)
            {
                return;
            }

            PrepareCameraTiming();
            bool ok = false;
            int resultBaseline = Volatile.Read(ref _cameraResultCount);
            InvokeSdkCommand($"trigger_camera(edge, '{cameraId}')", () =>
            {
                var timing = BeginCameraTrigger();
                ok = trigger_camera(edgeHandler, cameraId);
                EndCameraTrigger(timing, ok);
                return ok;
            });

            if (!ok)
            {
                LogTriggerHints(cameraId);
                return;
            }

            LockSidebarUntilResult(resultBaseline);
            WatchForMissingResult(cameraId);
        }

        /// <summary>
        /// "Trigger thành công nhưng không nhận được ảnh nào".
        ///
        /// trigger_camera() trả về true CHỈ có nghĩa là lệnh đã được gửi đi qua gRPC (cổng
        /// grpc_port, chiều client -&gt; UnitX). Ảnh thì đi đường khác hẳn:
        ///   - IMAGE_RESULT về qua ZMQ  (zmq_port),
        ///   - dữ liệu ảnh tải qua blob (blob_port).
        /// Nên hoàn toàn có thể trigger OK mà không bao giờ có callback. Hàm này đợi một lúc, nếu
        /// số callback không tăng thì in ra đúng danh sách nguyên nhân cần kiểm tra.
        /// </summary>
        private async void WatchForMissingResult(string cameraId)
        {
            int before = Volatile.Read(ref _callbackCount);
            await Task.Delay(MissingResultTimeoutMs);

            if (Volatile.Read(ref _callbackCount) != before)
            {
                return;
            }

            Log("----- TRIGGER OK NHƯNG KHÔNG CÓ KẾT QUẢ -----");
            Log($"Đã trigger camera '{cameraId}' thành công nhưng sau {MissingResultTimeoutMs} ms "
                + "không có callback nào từ UnitX.");
            Log("trigger_camera() = true chỉ nghĩa là LỆNH đã gửi đi qua gRPC. Ảnh về bằng đường khác:");
            Log("  1. ZMQ (zmq_port)  - kênh nhận IMAGE_RESULT. Bị chặn = trigger OK, không bao giờ có ảnh.");
            Log("  2. Blob (blob_port) - kênh tải dữ liệu ảnh. Bị chặn = có kết quả nhưng ảnh rỗng.");
            Log("  3. Chưa mở part group: hãng yêu cầu start_part_group(part_type, group_id) TRƯỚC "
                + "khi trigger, nếu không UnitX không publish IMAGE_RESULT. Bấm 'Part Start' rồi trigger lại.");
            Log("  4. Sai capture_config / part_type: UnitX nhận lệnh nhưng không chạy network nào.");
            Log("=> Bấm 'Kiểm tra mạng' ở thẻ cấu hình IP để biết ngay cổng nào đang đóng.");
            Log("---------------------------------------------");

            UpdateStatus("Trigger thành công nhưng không nhận được kết quả - xem log.");
        }

        #region Trigger liên tục

        /// <summary>Thời gian chờ callback sau một lần trigger thủ công trước khi coi là "không có ảnh".</summary>
        private const int MissingResultTimeoutMs = 6000;

        private CancellationTokenSource _triggerCts;
        private Task _triggerTask;

        /// <summary>
        /// Khoảng cách tối thiểu giữa hai lần trigger. "Delay ms" = 0 là hợp lệ cho vòng push (push
        /// xong ảnh này là push ngay ảnh kế) nhưng với trigger liên tục thì nó thành vòng lặp quay
        /// hết công suất, dội lệnh vào UnitX và làm nghẽn luồng nền - nên luôn kẹp sàn.
        /// </summary>
        private const int MinTriggerIntervalMs = 50;

        /// <summary>Bao nhiêu lần trigger thất bại LIÊN TIẾP thì tự dừng, thay vì dội lỗi mãi.</summary>
        private const int MaxConsecutiveTriggerFailures = 5;

        /// <summary>Số kết quả ảnh camera đã về (callback tăng) - vòng trigger đợi số này tăng.</summary>
        private int _cameraResultCount;

        private readonly AutoResetEvent _cameraResultEvent = new AutoResetEvent(false);

        /// <summary>
        /// Chờ kết quả của lần trigger vừa gửi: true = đã về, false = hết thời gian. Người dùng bấm
        /// dừng thì ném OperationCanceledException ngay (không đợi hết timeout).
        /// </summary>
        private bool WaitForCameraResult(int baseline, int timeoutMs, CancellationToken token)
        {
            var watch = Stopwatch.StartNew();
            var handles = new[] { _cameraResultEvent, token.WaitHandle };

            while (Volatile.Read(ref _cameraResultCount) <= baseline)
            {
                int remaining = timeoutMs - (int)watch.ElapsedMilliseconds;
                if (remaining <= 0)
                {
                    return false;
                }

                if (WaitHandle.WaitAny(handles, remaining) == 1)
                {
                    token.ThrowIfCancellationRequested();
                }
            }

            return true;
        }

        private bool IsTriggering
        {
            get { return _triggerTask != null && !_triggerTask.IsCompleted; }
        }

        /// <summary>
        /// Bật/tắt trigger liên tục. Nhịp giữa hai lần chụp lấy từ ô "Delay ms" (numDelay) - cùng ô
        /// đang dùng làm giãn cách giữa hai lần push ảnh.
        /// </summary>
        private void btnTriggerContinuous_Click(object sender, EventArgs e)
        {
            if (IsTriggering)
            {
                StopContinuousTrigger("người dùng bấm dừng");
                return;
            }

            StartTriggerLoop(0);
        }

        /// <summary>
        /// Số lần chụp của vòng trigger đang chạy: 0 = liên tục (Trigger Continuous), &gt; 0 = Trigger
        /// Repeat (nút Trigger Once khi ô "Trigger repeat" &gt; 1). Quyết định nút nào đang là nút Dừng.
        /// </summary>
        private int _triggerRepeatTotal;

        /// <summary>Vòng trigger đang chạy (theo SetTriggerRunning) - dùng để bật / tắt nút.</summary>
        private bool _triggerLoopRunning;

        /// <summary>Trigger repeat = 1 -> "Trigger Once"; &gt; 1 -> "Trigger Repeat".</summary>
        private void numTriggerRepeat_ValueChanged(object sender, EventArgs e)
        {
            if (!(IsTriggering && _triggerRepeatTotal > 0))
            {
                ApplyTriggerOnceStyle(false);
            }
        }

        private void ApplyTriggerOnceStyle(bool running)
        {
            int repeat = (int)numTriggerRepeat.Value;

            btnTriggerOnce.Text = running ? "Dừng lặp" : repeat > 1 ? "Chụp lặp lại" : "Chụp một ảnh";
            btnTriggerOnce.BackColor = running ? Color.FromArgb(220, 38, 38) : Color.White;
            btnTriggerOnce.ForeColor = running ? Color.White : Color.FromArgb(29, 78, 216);
        }

        /// <summary>
        /// Khởi động vòng trigger nền (có điều tiết - xem ContinuousTriggerLoop).
        /// <paramref name="repeatCount"/> = 0: chụp liên tục tới khi bấm dừng; &gt; 0: chụp đúng số lần đó.
        /// </summary>
        private void StartTriggerLoop(int repeatCount)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            string cameraId = GetCameraId();
            if (cameraId == null)
            {
                return;
            }

            IntPtr edgeHandler = GetEdgeHandlerOrWarn();
            if (edgeHandler == IntPtr.Zero)
            {
                return;
            }

            int interval = (int)numDelay.Value;
            if (interval < MinTriggerIntervalMs)
            {
                Log($"Delay ms = {interval} quá nhỏ để trigger lặp - dùng {MinTriggerIntervalMs} ms. "
                    + "Chỉnh ô 'Delay ms' để đổi nhịp chụp.");
                interval = MinTriggerIntervalMs;
            }

            LogCurrentMode(edgeHandler);

            // Giống Trigger Once: callback cần hai giá trị này để biết IMAGE_RESULT sắp tới là ảnh
            // CAMERA CHỤP chứ không phải kết quả của một lượt push.
            // Kiểm tra + tạo thư mục NGAY LÚC BẤM, không đợi tới lúc callback trả ảnh về: lỗi
            // đường dẫn lúc đó nằm trong luồng SDK, chỉ hiện trong log chứ không chặn được người dùng.
            if (ResolveImageRoot(true, true) == null)
            {
                return;
            }
            _lastTriggeredCameraId = cameraId;
            Interlocked.Exchange(ref _lastTriggerTs, Stopwatch.GetTimestamp());

            PrepareCameraTiming();
            _triggerCts = new CancellationTokenSource();
            var token = _triggerCts.Token;

            _triggerRepeatTotal = repeatCount;

            Log(repeatCount > 0
                ? $"=== BẮT ĐẦU trigger repeat camera '{cameraId}': {repeatCount} lần, nhịp {interval} ms ==="
                : $"=== BẮT ĐẦU trigger liên tục camera '{cameraId}', nhịp {interval} ms ===");
            UpdateStatus(repeatCount > 0
                ? $"Trigger repeat camera {cameraId}: 0 / {repeatCount} - nhịp {interval} ms"
                : $"Trigger liên tục camera {cameraId} - nhịp {interval} ms");

            _triggerTask = Task.Run(() => ContinuousTriggerLoop(edgeHandler, cameraId, interval, repeatCount, token), token)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        Log("Lỗi trigger lặp: " + t.Exception.GetBaseException().Message);
                    }

                    SetTriggerRunning(false);
                }, TaskScheduler.FromCurrentSynchronizationContext());

            SetTriggerRunning(true);
        }

        /// <summary>
        /// Vòng trigger nền, có ĐIỀU TIẾT: gửi trigger -> ĐỢI kết quả ảnh về -> đủ nhịp tối thiểu
        /// ("Delay ms") mới gửi lệnh kế tiếp.
        ///
        /// Lỗi cũ (2026-09-26): trigger_camera() = true chỉ nghĩa là UnitX đã NHẬN lệnh vào hàng đợi.
        /// Gửi theo nhịp cố định nhanh hơn tốc độ UnitX chụp + xử lý thì lệnh dồn lại ngày càng nhiều;
        /// bấm dừng xong UnitX vẫn chụp tiếp hết hàng đợi ("trôi"), Delay càng nhỏ trôi càng lâu.
        /// Giờ mỗi lúc chỉ có MỘT lệnh đang chờ, nên dừng là dừng ngay (tối đa về thêm 1 ảnh), và
        /// nhịp chụp thực tế tự khớp với tốc độ của UnitX.
        /// </summary>
        /// <param name="repeatCount">0 = chụp tới khi bấm dừng; &gt; 0 = dừng sau đúng số lần trigger đó.</param>
        private void ContinuousTriggerLoop(IntPtr edgeHandler, string cameraId, int intervalMs, int repeatCount, CancellationToken token)
        {
            string mode = repeatCount > 0 ? "Trigger repeat" : "Trigger liên tục";
            int count = 0;
            int results = 0;
            int consecutiveFailures = 0;
            int resultTimeoutMs = Math.Max(3000, _timeoutMs);
            var total = Stopwatch.StartNew();

            try
            {
                while (!token.IsCancellationRequested && (repeatCount == 0 || count < repeatCount))
                {
                    var cycle = Stopwatch.StartNew();
                    int baseline = Volatile.Read(ref _cameraResultCount);
                    bool ok;
                    long sdkLogMark = UnitXSdkLog.Mark(SdkLogPath);

                    // Đang hỏi người dùng xử lý ảnh trùng tên: tạm dừng, chưa gửi lệnh kế tiếp.
                    _triggerGate.Wait(token);

                    try
                    {
                        // Gia hạn cửa sổ nhận diện ảnh camera TRƯỚC mỗi lần trigger: chạy lâu hơn
                        // ManualTriggerWindowMs thì các ảnh sau sẽ không còn được nhận là ảnh camera nữa.
                        Interlocked.Exchange(ref _lastTriggerTs, Stopwatch.GetTimestamp());
                        var timing = BeginCameraTrigger();
                        ok = trigger_camera(edgeHandler, cameraId);
                        EndCameraTrigger(timing, ok);
                    }
                    catch (Exception ex)
                    {
                        Log("trigger_camera lỗi: " + ex.Message + " -> dừng " + mode + ".");
                        break;
                    }

                    count++;
                    string failure = null;

                    if (!ok)
                    {
                        failure = $"trigger_camera('{cameraId}') => FALSE: " + UnitXSdkLog.Explain(SdkLogPath, sdkLogMark).Reason;
                    }
                    else if (!WaitForCameraResult(baseline, resultTimeoutMs, token))
                    {
                        // Callback đang chờ người dùng chọn ở hộp thoại trùng tên - không tính là lỗi, đợi chọn xong.
                        if (!_triggerGate.IsSet)
                        {
                            Log($"[Trigger {count}] tạm dừng - chờ xử lý ảnh trùng tên...");
                            _triggerGate.Wait(token);
                        }

                        if (!WaitForCameraResult(baseline, resultTimeoutMs, token))
                        {
                            failure = $"không nhận được kết quả sau {resultTimeoutMs} ms";
                        }
                    }

                    if (failure == null)
                    {
                        results++;
                        consecutiveFailures = 0;
                        Log($"[Trigger {count}] kết quả về sau {cycle.ElapsedMilliseconds} ms");
                        double perImage = total.Elapsed.TotalMilliseconds / count;
                        UpdateStatus((repeatCount > 0 ? $"{mode}: {count} / {repeatCount} - {results} ảnh" : $"{mode}: {results} ảnh") + $"  -  {perImage:0} ms/ảnh "
                            + $"(nhịp tối thiểu {intervalMs} ms)");
                    }
                    else
                    {
                        consecutiveFailures++;
                        LogWarning($"[Trigger {count}] {failure} ({consecutiveFailures}/{MaxConsecutiveTriggerFailures} lần liên tiếp)");

                        if (consecutiveFailures >= MaxConsecutiveTriggerFailures)
                        {
                            Log($"Đã {consecutiveFailures} lần trigger thất bại liên tiếp -> DỪNG {mode}.");
                            LogTriggerHints(cameraId);
                            UpdateStatus(mode + " đã dừng vì thất bại liên tiếp - xem log.");
                            break;
                        }
                    }

                    // Đủ số lần của Trigger Repeat: dừng luôn, không chờ thêm một nhịp.
                    if (repeatCount > 0 && count >= repeatCount)
                    {
                        UpdateStatus($"{mode} xong: {results} / {repeatCount} ảnh trong {total.Elapsed.TotalSeconds:0.0} giây");
                        break;
                    }

                    // Nhịp tối thiểu tính từ lúc GỬI lệnh: UnitX xử lý lâu hơn nhịp thì gửi ngay.
                    int wait = intervalMs - (int)cycle.ElapsedMilliseconds;
                    if (wait > 0 && token.WaitHandle.WaitOne(wait))
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Người dùng bấm dừng trong lúc đang đợi kết quả.
            }

            Log($"=== KẾT THÚC {mode}: {count} lệnh, {results} ảnh trong {total.Elapsed.TotalSeconds:0.0} giây ===");
        }

        /// <summary>Dừng vòng trigger liên tục. An toàn khi gọi nhiều lần / lúc chưa chạy.</summary>
        private void StopContinuousTrigger(string reason)
        {
            var cts = _triggerCts;
            if (cts == null)
            {
                return;
            }

            try
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    Log("Đang dừng trigger liên tục" + (string.IsNullOrEmpty(reason) ? "" : " (" + reason + ")") + "...");
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>Chờ vòng trigger dừng hẳn - dùng khi đóng form / ngắt SDK.</summary>
        private void StopContinuousTriggerAndWait()
        {
            StopContinuousTrigger(null);

            var task = _triggerTask;
            if (task != null)
            {
                try { task.Wait(2000); }
                catch { }
            }
        }

        #endregion

        /// <summary>
        /// Những nguyên nhân thường gặp khiến trigger_camera trả về false, in ra log để đỡ phải đoán.
        /// Rút từ trình tự chuẩn trong unitx_sdk_sample.cpp của hãng.
        /// </summary>
        private void LogTriggerHints(string cameraId)
        {
            Log("   Vì sao trigger_camera có thể trả về false:");
            Log($"   1. '{cameraId}' không phải camera THẬT. camera_id là số serial của camera "
                + "(code mẫu của hãng dùng \"40497555\", \"DA3287738\"...). Bấm 'Kiểm tra' để xem "
                + "danh sách camera hợp lệ mà UnitX báo về (dòng \"camera: ...\").");
            Log("   2. Camera ID đang là nguồn ẢNH NGOÀI (external image source) - nguồn ảnh ngoài "
                + "được cấp ảnh bằng PushImage chứ không chụp được, nên không trigger được. Lưu ý ô "
                + "Camera ID tự điền theo IMAGE_RESULT của ảnh vừa push, có thể chính là nguồn ảnh ngoài.");
            Log("   3. Chưa có part group đang mở. Trình tự của hãng: Part Start -> Select Sequence "
                + "-> Camera Trigger -> Camera Reset.");
            Log("   4. Sai edge, hoặc máy UnitX không ở trạng thái sẵn sàng (system status code != 1).");
        }

        private void LogCurrentMode(IntPtr edgeHandler)
        {
            try
            {
                int mode = get_mode(edgeHandler);
                _lastMode = mode;
                string name;
                switch (mode)
                {
                    case 0: name = "PRODUCTION"; break;
                    case 1: name = "VALIDATION"; break;
                    default: name = "UNKNOWN"; break;
                }
                Log($"   get_mode(edge) = {mode} ({name})");
            }
            catch (Exception ex)
            {
                Log("   get_mode lỗi (bỏ qua): " + ex.Message);
            }
        }

        private void btnCameraReset_Click(object sender, EventArgs e)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            string cameraId = GetCameraId();
            if (cameraId == null)
            {
                return;
            }

            IntPtr edgeHandler = GetEdgeHandlerOrWarn();
            if (edgeHandler == IntPtr.Zero)
            {
                return;
            }

            string partId = (txtPartID.Text ?? string.Empty).Trim();

            if (partId.Length == 0)
            {
                var answer = MessageBox.Show(this,
                    "Ô 'Part ID' đang trống nên reset_camera sẽ nhận part_group_id rỗng."
                    + Environment.NewLine + Environment.NewLine
                    + "Một số bản unitx_sdk.dll xử lý chuỗi rỗng bằng hàm CRT an toàn và báo lỗi "
                    + "'invalid parameter' - trước đây lỗi này làm chết cả phần mềm."
                    + Environment.NewLine + Environment.NewLine
                    + "Nên điền Part ID đúng nhóm đang mở (vd 00001). Vẫn muốn gọi với part_id rỗng?",
                    "Part ID đang trống", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }

            int before = Volatile.Read(ref _crtFaultCount);

            InvokeSdkCommand(
                $"reset_camera(edge=0x{edgeHandler.ToInt64():X}, '{cameraId}', '{partId}', 0)",
                () => reset_camera(edgeHandler, cameraId, partId, 0));

            if (Volatile.Read(ref _crtFaultCount) != before)
            {
                MessageBox.Show(this,
                    "unitx_sdk.dll báo lỗi CRT trong lúc reset camera (đã chặn để phần mềm không "
                    + "bị đóng). Xem log để biết hàm nào lỗi." + Environment.NewLine + Environment.NewLine
                    + "Trạng thái SDK lúc này có thể đã sai - nên kết nối lại UnitX trước khi chạy tiếp.",
                    "SDK báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShowLogWindow();
            }
        }

        /// <summary>
        /// Chọn sequence chụp cho một camera: select_sequence(edge_handler, camera_id,
        /// sequence_number) của SDK lõi (wrapper cũng không export hàm này).
        /// </summary>
        private void btnSelectSequence_Click(object sender, EventArgs e)
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            string cameraId = GetCameraId();
            if (cameraId == null)
            {
                return;
            }

            int sequenceNumber = GetSequenceNumber();
            if (sequenceNumber < 0)
            {
                return;
            }

            IntPtr edgeHandler = GetEdgeHandlerOrWarn();
            if (edgeHandler == IntPtr.Zero)
            {
                return;
            }

            bool ok = false;
            InvokeSdkCommand($"select_sequence(edge, '{cameraId}', {sequenceNumber})", () =>
            {
                ok = select_sequence(edgeHandler, cameraId, sequenceNumber);
                return ok;
            });

            if (ok)
            {
                Log("   Đã chọn sequence. Muốn chụp luôn thì bấm tiếp 'Camera Trigger' "
                    + "(hãng còn có user_select_sequence = chọn sequence rồi trigger ngay một lần).");
            }
            else
            {
                LogTriggerHints(cameraId);
            }
        }

        // Nút "Nạp cấu hình UnitX" cũ đã bỏ: nó chỉ gọi ReadProdConfiguration(), việc mà
        // "Kiểm tra trạng thái" vẫn làm (kèm chẩn đoán + trạng thái hệ thống). Ô của nó trên lưới
        // nút giờ là "Restart Part".
        //
        // Giới hạn của get_prod_configuration: struct UnitXProdConfiguration (unitx_sdk_interface.h)
        // chỉ có networks / controllers / cameras / imaging_config / camera-&gt;capture_config /
        // capture_config-&gt;network - KHÔNG có part_type và KHÔNG có danh sách sequence. Part Type chỉ
        // lấy được từ imaging_config (nếu có) hoặc học từ IMAGE_RESULT / PART_GROUP_RESULT.

        private void btnRestartPart_Click(object sender, EventArgs e)
        {
            RestartPart();
        }

        /// <summary>
        /// "Restart Part" = System Reset rồi Part Start ngay, một lần bấm - đúng trình tự vẫn làm tay
        /// mỗi khi UnitX kẹt part group cũ. Không hỏi xác nhận (mục đích của nút là thao tác nhanh);
        /// reset thất bại thì dừng, không start part lên một hệ thống chưa reset xong.
        /// </summary>
        private void RestartPart()
        {
            if (!EnsureSdkReady())
            {
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                return;
            }

            IntPtr systemEdge = GetEdgeHandlerOrWarn();
            if (systemEdge == IntPtr.Zero)
            {
                return;
            }

            config.Edge = ResolveEdgeName();
            Log("=== RESTART PART: System Reset -> Part Start ===");

            bool resetOk = false;
            InvokeSdkCommand($"reset_system(edge=0x{systemEdge.ToInt64():X})", () =>
            {
                resetOk = reset_system(systemEdge);
                return resetOk;
            });

            if (!resetOk)
            {
                Log("Restart Part dừng lại: System Reset thất bại nên KHÔNG chạy Part Start.");
                return;
            }

            bool startOk = false;
            InvokeSdkCommand($"StartPart('{config.PartType}', '{config.PartId}', '{config.Edge}')", () =>
            {
                startOk = StartPart(config.PartType, config.PartId, config.Edge);
                return startOk;
            });

            UpdateStatus(startOk
                ? $"Restart Part: OK (part_type '{config.PartType}', part_id '{config.PartId}')"
                : "Restart Part: System Reset OK nhưng Part Start THẤT BẠI - xem log.");
        }

        /// <summary>
        /// Tìm part_type / sequence trong chuỗi imaging_config (UnitX trả về dạng JSON).
        /// Đây là nguồn part_type duy nhất đọc được TRƯỚC khi có kết quả thật từ dây chuyền.
        /// </summary>
        private void MinePartTypesAndSequences(string imagingConfig)
        {
            var partTypes = new List<string>();
            var sequences = new List<int>();

            try
            {
                var root = JsonConvert.DeserializeObject<JToken>(imagingConfig);
                if (root != null)
                {
                    WalkJson(root, partTypes, sequences);
                }
            }
            catch (Exception ex)
            {
                Log("imaging_config không phải JSON hợp lệ (" + ex.Message + ") - dò bằng biểu thức thường.");

                foreach (System.Text.RegularExpressions.Match m in
                    System.Text.RegularExpressions.Regex.Matches(imagingConfig,
                        "\"part_types?\"\\s*:\\s*\"([^\"]+)\""))
                {
                    partTypes.Add(m.Groups[1].Value);
                }
            }

            if (partTypes.Count > 0)
            {
                Log("part_type tìm thấy trong imaging_config: " + string.Join(", ", partTypes.Distinct()));
                ApplyPartTypes(partTypes.Distinct());
            }
            else
            {
                Log("Không thấy part_type nào trong imaging_config.");
            }

            if (sequences.Count > 0)
            {
                Log("sequence tìm thấy trong imaging_config: "
                    + string.Join(", ", sequences.Distinct().OrderBy(x => x)));
                ApplySequenceNumbers(sequences.Distinct().OrderBy(x => x));
            }
        }

        private static void WalkJson(JToken token, List<string> partTypes, List<int> sequences)
        {
            var property = token as JProperty;
            if (property != null)
            {
                string name = property.Name.ToLowerInvariant();
                var value = property.Value;

                if ((name == "part_type" || name == "part_types" || name == "parttype")
                    && value.Type == JTokenType.String)
                {
                    partTypes.Add(value.ToString());
                }
                else if ((name == "part_type" || name == "part_types") && value.Type == JTokenType.Array)
                {
                    foreach (var element in value)
                    {
                        if (element.Type == JTokenType.String)
                        {
                            partTypes.Add(element.ToString());
                        }
                    }
                }
                else if (name == "sequence" || name == "sequence_number" || name == "sequence_id")
                {
                    int number;
                    if (int.TryParse(value.ToString(), out number))
                    {
                        sequences.Add(number);
                    }
                }
            }

            foreach (var child in token.Children())
            {
                WalkJson(child, partTypes, sequences);
            }
        }

        /// <summary>Bổ sung các số sequence đọc được từ UnitX vào combobox Sequence IO.</summary>
        private void ApplySequenceNumbers(IEnumerable<int> numbers)
        {
            Action apply = () =>
            {
                foreach (var number in numbers)
                {
                    string text = number.ToString();
                    if (!cboSequenceIO.Items.Contains(text))
                    {
                        cboSequenceIO.Items.Add(text);
                    }
                }
            };

            RunOnUi(apply);
        }

        // btnCheckStatus_Click nằm ở frmMainV1.Status.cs (cửa sổ trạng thái có kết luận).

        #endregion
    }
}
