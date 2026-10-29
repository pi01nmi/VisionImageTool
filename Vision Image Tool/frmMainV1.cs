
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// frmMainV1 - Vision Image Tool.
    ///
    /// Bản nâng cấp giao diện của frmMainV12: giữ nguyên toàn bộ cơ chế push/nhận kết quả đã được
    /// vá lỗi của V12 (hàng đợi đối soát FIFO _pendingPushes, lưu mask đúng tên ảnh, theo dõi thư
    /// mục, lịch sử push) nhưng bố trí lại theo bản thiết kế mới:
    ///
    ///   - Khu cấu hình Part + nhóm nút điều khiển hệ thống (Part Start / System Reset /
    ///     Remove Part / Camera Trigger / Camera Reset / Select Sequence).
    ///   - Khu Path và ảnh (Load Path, Push toàn bộ, xoá lịch sử, xoá log, quét & theo dõi thư mục).
    ///   - Danh sách ảnh Raw + danh sách ảnh Mask (có phân trang) và khung xem trước lớn,
    ///     chuyển Raw/Mask bằng phím Space, kết quả OK/NG ở góc dưới bên phải.
    ///
    /// Các tuỳ chọn sau được ĐẶT CỨNG TRONG CODE theo yêu cầu, không đưa lên giao diện:
    ///   Gồm cả thư mục con = true, Chỉ push ảnh còn thiếu = true, Xem trước ảnh đang push = bỏ,
    ///   Subscribe trước khi RunSDK = true, Đảo kênh R/B ảnh trả về = true,
    ///   Hiện console log của SDK = false, Chờ callback rồi mới push ảnh kế = true,
    ///   Kết nối UnitX = tự động khi mở form.
    ///
    /// Code được chia thành 3 file partial: frmMainV1.cs (giao diện), frmMainV1.Sdk.cs (giao tiếp
    /// UnitX SDK) và frmMainV1.Push.cs (xử lý ảnh + vòng push).
    /// </summary>
    public partial class frmMainV1 : Form
    {
        #region Hằng số cấu hình cứng (không đưa lên giao diện)

        /// <summary>Định dạng ảnh gửi sang UnitX. UnitX chỉ nhận PNG/BMP/TIFF; BMP là mặc định đã chạy ổn ở V12.</summary>
        private const ImageKind PushConvertMode = ImageKind.Bmp;

        /// <summary>Ảnh UnitX trả về bị đảo kênh R/B - luôn đảo lại khi hiển thị/lưu.</summary>
        private const bool SwapRedBlueChannels = true;

        /// <summary>Thời gian chờ SDK sẵn sàng sau RunSDK (ms).</summary>
        private const int WaitAfterRunSdkMs = 3000;

        /// <summary>Ảnh xem trước được thu nhỏ về kích thước này trước khi đưa lên PictureBox.</summary>
        // Đủ lớn để giữ nguyên ảnh camera 2472x2064 - khung xem trước có zoom soi tới 100%.
        private const int PreviewMaxSize = 2600;

        /// <summary>Số dòng log giữ lại trong bộ nhớ / trên cửa sổ log.</summary>
        private const int MaxLogLines = 5000;

        /// <summary>Số dòng mỗi trang của hai danh sách ảnh.</summary>
        private const int PageSize = 200;

        /// <summary>Ngưỡng phân biệt "số sequence" với camera_id (camera_id của hãng là chuỗi số dài).</summary>
        private const int MaxSequenceNumber = 64;

        /// <summary>
        /// Ảnh nhận về từ callback được lưu ngay trong PATH người dùng nhập, tách làm hai thư mục:
        ///     PATH\raw\   - ảnh gốc (blob[0])          - bật/tắt bằng checkbox "Lưu ảnh Raw"
        ///     PATH\mask\  - ảnh kết quả (blob[1..])    - bật/tắt bằng checkbox "Lưu ảnh Mask"
        /// Cấu trúc thư mục con của ảnh gốc được giữ nguyên bên dưới hai thư mục này.
        /// </summary>
        private const string MaskFolderName = "mask";

        private const string RawFolderName = "raw";

        private static readonly string[] ImageExtensions =
        {
            ".bmp", ".jpg", ".jpeg", ".png", ".tif", ".tiff"
        };

        // Mọi định dạng mask từng/đang hỗ trợ - dùng để nhận diện ảnh mask đã lưu trước đó dù người
        // dùng có đổi "Định dạng ảnh" giữa các lần chạy hay không.
        private static readonly string[] MaskExtensions = { ".png", ".jpg", ".bmp", ".tif" };

        // Hậu tố kết quả có thể có ở cuối tên ảnh mask: "" là các file lưu trước khi có tính năng này.
        private static readonly string[] MaskDecisionTags = { "", "_OK", "_NG" };

        // Thứ tự phải khớp các item nạp vào cboImageFormat.
        private static readonly ImageKind[] ImageFormatOptions =
        {
            ImageKind.Jpeg, ImageKind.Png, ImageKind.Bmp, ImageKind.Tiff
        };

        private static readonly string[] ImageFormatNames = { "JPG", "PNG", "BMP", "TIFF" };

        // Màu trạng thái của từng dòng trong danh sách Raw.
        private static readonly Color DoneColor = Color.FromArgb(232, 250, 238);
        private static readonly Color MissingColor = Color.FromArgb(255, 249, 219);
        private static readonly Color FailedColor = Color.FromArgb(255, 235, 235);
        private static readonly Color PushingColor = Color.FromArgb(232, 240, 254);

        /// <summary>
        /// Ảnh trong PATH\raw CHƯA có ảnh mask đi kèm: chưa ghi nhận kết quả (kết quả OK/NG trong tên
        /// file raw không được tính) - push lại được để lấy mask.
        /// </summary>
        private static readonly Color NoResultColor = Color.FromArgb(229, 231, 235);

        // Thư mục lưu cấu hình + lịch sử push (nằm cạnh exe, không đụng vào thư mục ảnh của người dùng).
        private static readonly string AppDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pushv13");
        private static readonly string SettingsPath = Path.Combine(AppDataDir, "settings.json");
        private static readonly string HistoryPath = Path.Combine(AppDataDir, "pushed_images.txt");
        private static readonly string SdkLogPath = Path.Combine(AppDataDir, "unitx_sdk.log");

        // Bản sao nội dung cửa sổ Log ra đĩa (mỗi ngày một file) để đọc lại / gửi đi khi debug.
        private static string AppLogPath
        {
            get { return Path.Combine(AppDataDir, "app_" + DateTime.Now.ToString("yyyyMMdd") + ".log"); }
        }

        /// <summary>
        /// File cấu hình mạng mà unitx_sdk.dll đọc khi RunSDK(): %USERPROFILE%\UnitX SDK\config\client.yaml.
        /// Đây là nơi khai báo "addresses" mà gRPC báo lỗi "failed to connect to all addresses".
        /// </summary>
        private static readonly string SdkConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "UnitX SDK", "config", "client.yaml");

        private enum ImageKind
        {
            Unknown = 0,
            Png,
            Bmp,
            Tiff,
            Jpeg
        }

        private enum PreviewMode
        {
            Raw = 0,
            Mask = 1
        }

        /// <summary>
        /// Hậu tố gắn vào CUỐI tên ảnh camera khi lưu, để nhiều ảnh trong cùng một thư mục không đè lên
        /// nhau (tên enum giữ "Prefix" vì giá trị đã lưu trong settings.json).
        /// Thứ tự phải khớp các item nạp vào cboNamePrefix.
        /// </summary>
        private enum NamePrefixMode
        {
            Timestamp = 0,
            Number = 1,
            None = 2
        }

        private static readonly string[] NamePrefixNames =
        {
            "Timestamp (ngày giờ)", "Số thứ tự", "Không có"
        };

        #endregion

        #region Kiểu dữ liệu

        private sealed class PushItem
        {
            public string FullPath { get; set; }
            public long Length { get; set; }
            public DateTime LastWriteUtc { get; set; }
            public DateTime LastWrite { get; set; }
            public string MaskPath { get; set; }
            public string State { get; set; }
            public Color StateColor { get; set; }
            public string Decision { get; set; }

            /// <summary>
            /// Ảnh raw nằm trong PATH\raw (camera chụp / ứng dụng lưu). Đã có mask thì chỉ để xem, không
            /// push lại; CHƯA có mask (nền xám) thì được push - khi đó chính ảnh này là ảnh RAW, chỉ
            /// lưu ảnh mask trả về (xem ShowReturnedImage).
            /// </summary>
            public bool FromCamera { get; set; }

            public string Key { get { return BuildKey(FullPath, LastWriteUtc, Length); } }
        }

        private sealed class MaskItem
        {
            public string FullPath { get; set; }
            public DateTime LastWrite { get; set; }
        }

        /// <summary>
        /// Một lượt push đang chờ IMAGE_RESULT trả về.
        ///
        /// Hàng đợi FIFO các lượt push là cách đối soát ĐÚNG ảnh đã push với ĐÚNG kết quả nhận về.
        /// Nếu dùng một field dùng chung (như bản trước V12) thì khi callback của ảnh N tới TRỄ - sau
        /// khi vòng lặp đã hết thời gian chờ và chuyển sang ảnh N+1 - field đó đã bị ghi đè thành
        /// đường dẫn N+1, khiến mask THẬT của N bị lưu NHẦM TÊN thành N+1. Với hàng đợi FIFO: N+1 chỉ
        /// được enqueue SAU N nên callback trễ của N vẫn dequeue đúng entry của N.
        /// </summary>
        private sealed class PendingPush
        {
            public string FullPath;
            public long PushTimestamp;
            public readonly ManualResetEventSlim Signal = new ManualResetEventSlim(false);
        }

        private sealed class PushSettings
        {
            public string Folder { get; set; }
            public string PartType { get; set; }
            public string PartID { get; set; }
            public string CaptureConfig { get; set; }
            public string CameraId { get; set; }
            public string SequenceIo { get; set; }
            public string Edge { get; set; }
            // Kiểu nullable: file settings.json cũ sẽ để null và giữ nguyên giá trị mặc định của
            // control thay vì bị ghi đè thành false/0.
            public int? DelayMs { get; set; }
            public int? TriggerRepeat { get; set; }
            public int? TimeoutMs { get; set; }
            public bool? SaveCallback { get; set; }
            public bool? SaveRaw { get; set; }
            public string ImageName { get; set; }
            public int? NamePrefix { get; set; }
            public int? ImageFormat { get; set; }
            public int? JpegQuality { get; set; }
            public bool? WatchFolder { get; set; }
            // "Dark" / "Light" - giao diện lần mở trước.
            public string Theme { get; set; }

            /// <summary>Vị trí / kích thước cửa sổ, sidebar, danh sách Raw/Mask (frmMainV1.Layout.cs).</summary>
            public WindowLayout Window { get; set; }
        }

        #endregion

        #region Trường dữ liệu

        // ---- SDK ----
        private bool _sdkRunning;
        private Task _sdkTask;
        private int _callbackCount;
        private long _lastCallbackTicks;          // DateTime.UtcNow.Ticks của callback gần nhất (0 = chưa có)
        private volatile string _lastUnitXError;  // lỗi gần nhất UnitX gửi qua topic ERROR_*
        private long _lastUnitXErrorTicks;
        private string _edge = string.Empty;
        private int _timeoutMs = 15000;

        // ---- Hàng đợi đối soát push <-> kết quả ----
        private readonly ConcurrentQueue<PendingPush> _pendingPushes = new ConcurrentQueue<PendingPush>();

        // Nếu một entry nằm trong hàng đợi quá lâu mà không có IMAGE_RESULT nào khớp (UnitX có thể đã
        // âm thầm bỏ qua ảnh đó), bỏ nó đi thay vì để callback của ảnh SAU bị gán nhầm vào tên NÀY
        // mãi mãi. Đây là van an toàn, độc lập với timeout hiển thị trên giao diện.
        private const double PendingPushMaxAgeMs = 120000;

        // Callback chạy trên luồng SDK nên đọc field thay vì đọc control.
        private volatile bool _saveMask = true;
        private volatile bool _saveRaw;

        // Tên ảnh + kiểu tiền tố: callback đọc từ field (không đụng control từ luồng SDK).
        private volatile string _imageNameTemplate = string.Empty;
        private volatile NamePrefixMode _namePrefixMode = NamePrefixMode.Timestamp;

        // Số thứ tự đang dùng cho chế độ "Số thứ tự". 0 = chưa dò thư mục lần nào.
        private int _imageSequence;
        private volatile ImageKind _maskFormat = ImageKind.Jpeg;

        /// <summary>Chất lượng nén JPEG khi lưu ảnh raw / mask (1-100, ô "JPG quality").</summary>
        private volatile int _jpegQuality = 95;

        // ---- Đo thời gian (callback ghi, vòng push đọc sau khi Signal bật) ----
        private long _pushDoneTs;
        private double _lastPushToCallbackMs;
        private double _lastCallbackTotalMs;
        private double _lastDecodeMs;
        private double _lastSaveMaskMs;
        private double _lastUnitXLatencyMs;
        private double _lastBlobCopyMs;
        private long _lastReturnBytes;

        // Encode ảnh mask: dùng chung bộ đệm và khoá lại vì callback của SDK có thể chạy song song.
        private readonly MemoryStream _maskStream = new MemoryStream();
        private readonly object _encodeLock = new object();

        // ---- Push ----
        private CancellationTokenSource _pushCts;
        private Task _pushTask;

        // ---- Lịch sử ----
        private readonly HashSet<string> _pushedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _historyLock = new object();

        // ---- Danh sách ảnh ----
        private readonly List<PushItem> _rawItems = new List<PushItem>();
        private readonly Dictionary<string, int> _rawIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _rawIndexByBaseName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<MaskItem> _maskItems = new List<MaskItem>();
        private int _rawPage;
        private int _maskPage;

        // ---- Xem trước ----
        private PreviewMode _previewMode = PreviewMode.Raw;
        private string _selectedRawPath;
        private string _selectedMaskPath;
        private bool _suppressSelectionEvents;

        // ---- Theo dõi thư mục ----
        private FileSystemWatcher _folderWatcher;
        private CancellationTokenSource _watchCts;
        private Task _watchConsumerTask;
        private BlockingCollection<string> _watchQueue;
        private readonly HashSet<string> _watchQueued = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _watchQueueLock = new object();

        // ---- Log ----
        private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private readonly List<LogEntry> _logLines = new List<LogEntry>();
        private System.Windows.Forms.Timer _logTimer;
        private System.Windows.Forms.Timer _clockTimer;
        private frmLogV1 _logWindow;

        // ---- Gợi ý part_type ----
        private readonly List<string> _partTypeSuggestions = new List<string>();

        // ---- Sequence ----
        // Ô "Sequence IO" được TỰ ĐIỀN bằng camera_id (theo yêu cầu), nhưng select_sequence() của
        // hãng nhận một SỐ NGUYÊN. Giá trị số hợp lệ gần nhất người dùng chọn được giữ ở đây để
        // dùng lại khi ô đang hiển thị camera_id.
        private int _sequenceNumber = 1;
        private bool _suppressCameraSync;

        // ---- Kiểm tra mạng ----
        private bool _networkTestRunning;

        #endregion

        #region Win32 - chữ gợi ý trong ô nhập (cue banner)

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;
        private const int CB_SETCUEBANNER = 0x1703;

        private static void SetCue(TextBox textBox, string text)
        {
            try
            {
                SendMessage(textBox.Handle, EM_SETCUEBANNER, IntPtr.Zero, text);
            }
            catch
            {
                // Chữ gợi ý chỉ là trang trí - không có thì thôi.
            }
        }

        private static void SetCue(ComboBox comboBox, string text)
        {
            try
            {
                SendMessage(comboBox.Handle, CB_SETCUEBANNER, IntPtr.Zero, text);
            }
            catch
            {
            }
        }

        #endregion

        public frmMainV1()
        {
            InitializeComponent();
            UseAppIcon();
            CheckForIllegalCrossThreadCalls = false;
            SetupImageListMenus();
            SetupCommentUi();
            SetupSplitView();
            SetupTheme();
            SetupFileHistory();
            SetupStatusBar();
            SetupAutoRefresh();
            SetupCaptureBehavior();   // hành vi bố cục Capture (frmMainV1.CaptureLayout.cs); bố cục nằm trong Designer
        }

        /// <summary>Icon cửa sổ / thanh taskbar = icon của exe (ApplicationIcon: Vision Image Tool.ico).</summary>
        private void UseAppIcon()
        {
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
                // Giữ icon mặc định nếu không đọc được.
            }
        }

        #region Vòng đời form

        private async void frmMainV1_Load(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(AppDataDir);
            }
            catch (Exception ex)
            {
                Log("Không tạo được thư mục cấu hình: " + ex.Message);
            }

            // Chữ gợi ý trong các ô nhập (giống bản thiết kế).
            SetCue(txtPartID, "Nhập Part ID...");
            SetCue(cboPartType, "Nhập Part Type...");
            SetCue(txtPath, "Nhập đường dẫn thư mục hoặc file ảnh...");
            SetCue(cboCaptureConfig, "Chọn cấu hình...");
            SetCue(cboCameraID, "Chọn Camera...");
            SetCue(cboSequenceIO, "Chọn Sequence IO...");

            lvRawImage.ShowItemToolTips = true;
            lvMaskImage.ShowItemToolTips = true;

            // Tab Dataset Manager: cấu hình riêng nằm cạnh settings.json, thư mục mặc định = ô Path.
            ucDataset.Initialize(Path.Combine(AppDataDir, "dataset_manager.json"), () => txtPath.Text);

            // V13: capture_config / part_type / camera_id KHÔNG còn danh sách tạo sẵn.
            // Tất cả được nạp từ UnitX ("Kiểm tra trạng thái" -> get_prod_configuration,
            // và các giá trị học được từ IMAGE_RESULT / PART_GROUP_RESULT).

            cboImageFormat.Items.AddRange(ImageFormatNames);
            cboImageFormat.SelectedIndex = 0;

            cboNamePrefix.Items.AddRange(NamePrefixNames);
            cboNamePrefix.SelectedIndex = (int)NamePrefixMode.Timestamp;

            // Sequence IO = sequence_number truyền vào select_sequence(edge, camera_id, số này).
            // Code mẫu của hãng dùng 1 hoặc 2 tuỳ từng camera, nên để sẵn vài số cho tiện chọn.
            cboSequenceIO.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5" });
            cboSequenceIO.Text = "1";
            _sequenceNumber = 1;
            cboSequenceIO.TextChanged += cboSequenceIO_TextChanged;

            // Cài trước mọi thứ khác: từ lúc này lỗi "invalid parameter" trong unitx_sdk.dll
            // được ghi log thay vì làm chết tiến trình.
            InstallCrtGuard();

            cboPartType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboPartType.AutoCompleteSource = AutoCompleteSource.ListItems;

            LoadSdkNetworkConfig();

            _logTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _logTimer.Tick += FlushLogQueue;
            _logTimer.Start();

            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, args) => tsslClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _clockTimer.Start();
            tsslClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            LoadHistory();
            LoadSettings();

            _saveMask = chkSaveCallback.Checked;
            _saveRaw = chkSaveRaw.Checked;
            _maskFormat = ImageFormatOptions[Math.Max(0, cboImageFormat.SelectedIndex)];

            chkSaveCallback.CheckedChanged += (s, args) =>
            {
                _saveMask = chkSaveCallback.Checked;
                UpdateImageFormatEnabled();
            };
            chkSaveRaw.CheckedChanged += (s, args) =>
            {
                _saveRaw = chkSaveRaw.Checked;
                UpdateImageFormatEnabled();
            };
            cboImageFormat.SelectedIndexChanged += (s, args) =>
            {
                _maskFormat = ImageFormatOptions[Math.Max(0, cboImageFormat.SelectedIndex)];
                ApplyImageNameSettings();
                UpdateImageFormatEnabled();
            };
            _jpegQuality = (int)numJpegQuality.Value;
            UpdateImageFormatEnabled();

            txtImageName.TextChanged += (s, args) => ApplyImageNameSettings();
            cboNamePrefix.SelectedIndexChanged += (s, args) =>
            {
                // Đổi kiểu tiền tố thì dò lại số thứ tự từ đầu cho đúng thư mục hiện tại.
                Interlocked.Exchange(ref _imageSequence, 0);
                ApplyImageNameSettings();
            };
            ApplyImageNameSettings();

            // Gắn SAU LoadSettings(): trạng thái đã lưu chỉ được PHỤC HỒI trên giao diện, việc theo
            // dõi thật sự chỉ bắt đầu sau khi kết nối UnitX - tránh popup ngay lúc mở form.
            chkWatchFolder.CheckedChanged += ChkWatchFolder_CheckedChanged;
            if (chkWatchFolder.Checked)
            {
                Log("Theo dõi thư mục đang BẬT (theo cấu hình đã lưu) - sẽ tự bắt đầu sau khi kết nối UnitX.");
            }

            UpdatePreviewToggle();
            ApplyResultLabel(null);
            UpdateActionButtons();

            Log($"frmMainV1 khởi động. Lịch sử đã push: {_pushedKeys.Count} ảnh.");
            UpdateStatus("Đang khởi động...");

            // Tự động nạp lại thư mục ảnh của lần chạy trước.
            if (!string.IsNullOrWhiteSpace(txtPath.Text) && Directory.Exists(txtPath.Text.Trim()))
            {
                ScanFolder(false);
            }

            // Mở app là vào thẳng Dataset Manager (tự tải thư mục dataset lần trước qua OnShown).
            if (tabMain.SelectedTab != tabDataset)
            {
                tabMain.SelectedTab = tabDataset;   // SelectedIndexChanged -> ucDataset.OnShown()
            }
            else
            {
                ucDataset.OnShown();
            }

            // Yêu cầu: "Kết nối UnitX: Auto kết nối".
            await ConnectAsync();
        }

        private void frmMainV1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_pushTask != null && !_pushTask.IsCompleted)
            {
                var answer = MessageBox.Show(this,
                    "Đang push ảnh sang UnitX. Bạn có chắc muốn thoát?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (answer != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                CancelPush();
            }

            StopContinuousTriggerAndWait();
            StopWatching();

            if (_logTimer != null)
            {
                _logTimer.Stop();
            }

            if (_clockTimer != null)
            {
                _clockTimer.Stop();
            }

            SaveSettings();
            ucDataset.SaveLayout();
            StopSdk();

            if (_logWindow != null && !_logWindow.IsDisposed)
            {
                _logWindow.Dispose();
                _logWindow = null;
            }
        }

        /// <summary>
        /// Bắt phím Space ở cấp Form để chuyển Raw/Mask hoạt động kể cả khi ListView đang được focus.
        /// ProcessCmdKey được gọi TRƯỚC khi control con nhận phím, nên ListView không kịp "nuốt" phím
        /// Space (KeyPreview + KeyDown là không đủ với ListView). Vẫn nhường Space lại cho các ô nhập
        /// liệu để người dùng gõ được dấu cách.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Chỉ ở tab Capture & Monitor - tab Dataset Manager dùng bàn phím cho bảng của nó.
            if (keyData == Keys.Space && tabMain.SelectedTab == tabCapture)
            {
                var active = ActiveControl;
                bool isTextInput = active is TextBox || active is ComboBox || active is NumericUpDown
                    || active is CheckBox || active is Button;

                if (!isTextInput)
                {
                    TogglePreviewMode();
                    return true;
                }
            }

            // Tab / Shift+Tab trong vùng ảnh: tới ảnh Raw chưa có Mask kế tiếp / trước đó.
            if (tabMain.SelectedTab == tabCapture && HandleMissingMaskKey(keyData))
            {
                return true;
            }

            // Phím tắt menu chuột phải của danh sách ảnh (Enter, F2, Delete, Ctrl+C...).
            if (tabMain.SelectedTab == tabCapture && HandleImageListKey(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Ẩn / hiện các vùng giao diện

        #endregion

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTabButtons();   // nút tab trên thanh tiêu đề + danh sách ☰ Hiển thị theo tab đang mở
            if (tabMain.SelectedTab == tabDataset)
            {
                ucDataset.OnShown();
            }
        }

        /// <summary>Vẽ viền mảnh cho các panel dạng "thẻ" (card) - thay cho BorderStyle mặc định khá thô.</summary>
        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null)
            {
                return;
            }

            using (var pen = new Pen(Color.FromArgb(222, 226, 233)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            }
        }

        #endregion

        #region Part type / edge

        /// <summary>
        /// Đổ danh sách part_type lên ô Part Type. V13 KHÔNG còn nạp part_type từ LiteDB
        /// (danh sách "camera{trạm}-{model}" tạo sẵn của frmMainV8/V12) nữa - theo yêu cầu, part_type
        /// chỉ đến từ chính UnitX:
        ///   - chuỗi part_type thấy trong IMAGE_RESULT / PART_GROUP_RESULT (học lúc chạy), và
        ///   - chuỗi tìm được trong imaging_config của get_prod_configuration (nếu có).
        /// Lưu ý kỹ thuật: struct UnitXProdConfiguration của hãng KHÔNG có trường part_type
        /// (chỉ có networks / controllers / cameras / imaging_config / camera-&gt;cc / cc-&gt;network),
        /// nên đây là hai nguồn part_type thật sự có thể đọc được từ SDK.
        /// </summary>
        private void ApplyPartTypes(IEnumerable<string> partTypes)
        {
            if (partTypes == null)
            {
                return;
            }

            Action apply = () =>
            {
                string current = cboPartType.Text;
                int added = 0;

                foreach (var partType in partTypes)
                {
                    if (string.IsNullOrWhiteSpace(partType))
                    {
                        continue;
                    }

                    if (!_partTypeSuggestions.Contains(partType))
                    {
                        _partTypeSuggestions.Add(partType);
                    }

                    if (!cboPartType.Items.Contains(partType))
                    {
                        cboPartType.Items.Add(partType);
                        added++;
                    }
                }

                if (added > 0)
                {
                    Log($"Đã nạp thêm {added} part_type từ UnitX (tổng {cboPartType.Items.Count}).");
                }

                if (string.IsNullOrWhiteSpace(current) && cboPartType.Items.Count > 0)
                {
                    cboPartType.SelectedIndex = 0;
                }
                else
                {
                    cboPartType.Text = current;
                }
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// "Định dạng ảnh" áp dụng cho CẢ ảnh raw lẫn ảnh mask lưu xuống, nên chỉ khoá lại khi cả
        /// hai checkbox đều tắt (lúc đó không lưu gì cả).
        /// </summary>
        private void UpdateImageFormatEnabled()
        {
            cboImageFormat.Enabled = chkSaveCallback.Checked || chkSaveRaw.Checked;

            // % nén chỉ có nghĩa với JPG (PNG / BMP / TIFF lưu không mất dữ liệu).
            bool jpeg = ImageFormatOptions[Math.Max(0, cboImageFormat.SelectedIndex)] == ImageKind.Jpeg;
            numJpegQuality.Enabled = cboImageFormat.Enabled && jpeg;
            lblJpegQuality.Enabled = lblJpegQualityHint.Enabled = numJpegQuality.Enabled;
        }

        /// <summary>Chất lượng JPEG (1-100) - callback của SDK đọc field này khi lưu ảnh.</summary>
        private void numJpegQuality_ValueChanged(object sender, EventArgs e)
        {
            _jpegQuality = (int)numJpegQuality.Value;
        }

        #region Tên ảnh lưu

        /// <summary>
        /// Dựng tên file (chưa có phần mở rộng) cho ảnh KHÔNG có ảnh gốc trên đĩa - tức ảnh do camera
        /// chụp. Ảnh push vẫn giữ nguyên tên ảnh gốc, không đụng tới, vì tên đó là thứ FindMaskPath
        /// dùng để biết ảnh nào đã xử lý xong ("chỉ push ảnh còn thiếu").
        ///
        /// Tên đầy đủ: {tên người dùng nhập}{_raw|_mask}{_OK|_NG}{hậu tố}
        ///   ví dụ MODEL1_DK1_VITRI1_raw_OK_20260925_112020_606 - hàm này chỉ trả phần tên; "_raw" /
        ///   "_mask" và kết quả được gắn lúc lưu file, hậu tố lấy từ BuildCameraNameTail.
        /// </summary>
        private string BuildCameraBaseName(string cameraTag)
        {
            string name = (_imageNameTemplate ?? string.Empty).Trim();

            // Bỏ trống thì quay về hành vi cũ: lấy camera_id làm tên.
            if (name.Length == 0)
            {
                name = string.IsNullOrEmpty(cameraTag) ? "camera" : cameraTag;
            }

            return SanitizeFileName(name);
        }

        /// <summary>
        /// Hậu tố ở CUỐI tên ảnh camera (sau _OK/_NG) để nhiều ảnh cùng tên không đè lên nhau:
        /// "_yyyyMMdd_HHmmss_fff", "_00001", hoặc rỗng. Tính MỘT LẦN cho mỗi lần nhận, dùng chung cho
        /// cả ảnh raw lẫn mask để hai file ghép cặp được.
        /// </summary>
        private string BuildCameraNameTail()
        {
            switch (_namePrefixMode)
            {
                case NamePrefixMode.Timestamp:
                    return "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

                case NamePrefixMode.Number:
                    return "_" + NextSequenceNumber().ToString("00000");

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Số thứ tự kế tiếp. Lần đầu dò thư mục raw/mask để tiếp tục từ số lớn nhất đã có - nếu
        /// không, mỗi lần mở lại ứng dụng sẽ đếm lại từ 1 và ghi đè hết ảnh cũ.
        /// </summary>
        private int NextSequenceNumber()
        {
            if (Interlocked.CompareExchange(ref _imageSequence, 0, 0) <= 0)
            {
                Interlocked.Exchange(ref _imageSequence, ScanExistingSequence());
            }

            return Interlocked.Increment(ref _imageSequence);
        }

        private int ScanExistingSequence()
        {
            int max = 0;

            try
            {
                string root = GetOutputRoot();

                foreach (var sub in new[] { RawFolderName, MaskFolderName })
                {
                    string folder = Path.Combine(root, sub);
                    if (!Directory.Exists(folder))
                    {
                        continue;
                    }

                    foreach (var file in Directory.EnumerateFiles(folder))
                    {
                        // Số thứ tự 5 chữ số nằm CUỐI tên (..._OK_00012); vẫn đọc dạng cũ đặt ở ĐẦU
                        // tên (00012_...) để đánh tiếp chứ không đếm lại từ 1 và ghi đè ảnh cũ.
                        string stem = Path.GetFileNameWithoutExtension(file);
                        var match = System.Text.RegularExpressions.Regex.Match(stem, @"_(\d{5})$");
                        if (!match.Success)
                        {
                            match = System.Text.RegularExpressions.Regex.Match(stem, @"^(\d+)_");
                        }

                        int value;
                        if (match.Success && int.TryParse(match.Groups[1].Value, out value) && value > max)
                        {
                            max = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Không dò được số thứ tự ảnh đã có: " + ex.Message);
            }

            if (max > 0)
            {
                Log($"Số thứ tự ảnh sẽ đánh tiếp từ {max + 1} (đã có ảnh đánh số tới {max}).");
            }

            return max;
        }

        /// <summary>Cập nhật field cho luồng callback + dòng ví dụ tên file trên giao diện.</summary>
        private void ApplyImageNameSettings()
        {
            _imageNameTemplate = txtImageName.Text ?? string.Empty;
            _namePrefixMode = (NamePrefixMode)Math.Max(0, cboNamePrefix.SelectedIndex);

            string name = (_imageNameTemplate).Trim();
            bool usingCameraId = name.Length == 0;
            if (usingCameraId)
            {
                name = "<camera_id>";
            }
            else
            {
                name = SanitizeFileName(name);
            }

            string tail;
            switch (_namePrefixMode)
            {
                case NamePrefixMode.Timestamp:
                    tail = "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    break;
                case NamePrefixMode.Number:
                    tail = "_00001";
                    break;
                default:
                    tail = string.Empty;
                    break;
            }

            string ext = GetExtension(ImageFormatOptions[Math.Max(0, cboImageFormat.SelectedIndex)]);

            lblNamePreview.Text = "Ví dụ:  raw\\" + name + RawMark + "_OK" + tail + ext
                + "   |   mask\\" + name + "_mask_OK" + tail + ext
                + (usingCameraId ? "   (bỏ trống = lấy camera_id)" : string.Empty);
        }

        #endregion

        #region Kiểm tra / tạo thư mục ảnh

        /// <summary>
        /// Kiểm tra ô Path: đúng định dạng chưa, thư mục có tồn tại không, thiếu thì tạo.
        /// Trả về đường dẫn đã chuẩn hoá, hoặc null nếu không dùng được (đã báo lỗi nếu showErrors).
        /// Đồng thời cập nhật _imageRootFolder cho luồng callback dùng khi lưu ảnh.
        /// </summary>
        private string ResolveImageRoot(bool createIfMissing, bool showErrors)
        {
            string raw = (txtPath.Text ?? string.Empty).Trim();

            // Người dùng hay copy đường dẫn kèm dấu nháy từ Explorer ("Copy as path").
            raw = raw.Trim('"');

            if (raw.Length == 0)
            {
                Warn(showErrors, "Chưa nhập đường dẫn thư mục ảnh.", "Thiếu Path");
                return null;
            }

            if (raw.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                Warn(showErrors, "Đường dẫn chứa ký tự không hợp lệ.", "Path không hợp lệ");
                return null;
            }

            string full;

            try
            {
                // Trỏ thẳng vào một file ảnh thì lấy thư mục chứa nó.
                if (File.Exists(raw))
                {
                    raw = Path.GetDirectoryName(raw);
                }

                full = Path.GetFullPath(raw);
            }
            catch (Exception ex)
            {
                // GetFullPath ném ArgumentException / NotSupportedException / PathTooLongException
                // cho hầu hết các kiểu sai định dạng.
                Warn(showErrors, "Đường dẫn không hợp lệ:" + Environment.NewLine + ex.Message, "Path không hợp lệ");
                return null;
            }

            if (!Path.IsPathRooted(full))
            {
                Warn(showErrors, "Hãy nhập đường dẫn tuyệt đối, ví dụ D:\\Images\\Line1.", "Path không hợp lệ");
                return null;
            }

            if (!Directory.Exists(full))
            {
                if (!createIfMissing)
                {
                    return null;
                }

                try
                {
                    Directory.CreateDirectory(full);
                    Log("Thư mục chưa có - đã tự động tạo: " + full);
                    UpdateStatus("Đã tạo thư mục: " + full);
                }
                catch (Exception ex)
                {
                    Warn(showErrors,
                        "Không tạo được thư mục:" + Environment.NewLine + full + Environment.NewLine + ex.Message,
                        "Không tạo được thư mục");
                    return null;
                }
            }

            // Hiển thị lại dạng đã chuẩn hoá để người dùng thấy đúng cái app đang dùng.
            if (!string.Equals(txtPath.Text, full, StringComparison.Ordinal))
            {
                txtPath.Text = full;
            }

            _imageRootFolder = full;
            return full;
        }

        private void Warn(bool show, string message, string caption)
        {
            Log(caption + ": " + message.Replace(Environment.NewLine, " "));

            if (show)
            {
                MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        private void cboPartType_TextChanged(object sender, EventArgs e)
        {
            ApplyStationDefaults();
        }

        /// <summary>
        /// Theo frmMainV8/V12: part_type "cameraN-..." => part_id = số trạm dạng "00000",
        /// edge = line_2 cho trạm 4 và 5, còn lại line_1. Edge không có trên giao diện V13 nên được
        /// suy ra ở đây (và có thể ghi đè bằng khoá "Edge" trong settings.json).
        /// </summary>
        private void ApplyStationDefaults()
        {
            int station = GetStationFromPartType(cboPartType.Text);
            if (station <= 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                txtPartID.Text = station.ToString("00000");
            }

            _edge = (station == 4 || station == 5) ? "line_2" : "line_1";
        }

        /// <summary>Suy ra số trạm từ part_type "cameraN-...". Trả về 0 nếu không khớp.</summary>
        private static int GetStationFromPartType(string partType)
        {
            if (string.IsNullOrEmpty(partType))
            {
                return 0;
            }

            var match = System.Text.RegularExpressions.Regex.Match(partType, @"^camera(\d+)-");
            int station;
            return match.Success && int.TryParse(match.Groups[1].Value, out station) ? station : 0;
        }

        #endregion

        #region Nút khu Path & ảnh

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Chọn thư mục chứa ảnh cần push sang UnitX";
                dlg.ShowNewFolderButton = false;

                string current = (txtPath.Text ?? string.Empty).Trim();
                if (Directory.Exists(current))
                {
                    dlg.SelectedPath = current;
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtPath.Text = dlg.SelectedPath;
                    ScanFolder(true);
                }
            }
        }

        /// <summary>
        /// "Quét thư mục" nay gộp luôn việc "Load Path" cũ: kiểm tra/chuẩn hoá đường dẫn trong ô
        /// Path (tự tạo thư mục nếu chưa có) rồi nạp lại hai danh sách raw/mask. Nút "Load Path"
        /// riêng đã bị bỏ vì làm đúng một việc giống hệt nút này.
        /// </summary>
        private void btnScan_Click(object sender, EventArgs e)
        {
            ScanFolder(true);
        }

        /// <summary>
        /// Push toàn bộ ảnh CÒN THIẾU mask. Khi đang push, chính nút này đổi thành "Dừng push".
        /// </summary>
        private void btnPushAll_Click(object sender, EventArgs e)
        {
            if (_pushTask != null && !_pushTask.IsCompleted)
            {
                CancelPush();
                return;
            }

            if (IsTriggering)
            {
                MessageBox.Show(this,
                    "Đang trigger liên tục. Bấm 'Dừng Trigger' trước khi push cả thư mục.",
                    "Đang trigger", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_folderWatcher != null)
            {
                MessageBox.Show(this,
                    "Đang bật 'Theo dõi thư mục'. Tắt theo dõi trước khi push thủ công cả thư mục.",
                    "Đang theo dõi thư mục", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_sdkRunning)
            {
                MessageBox.Show(this, "Chưa kết nối UnitX.", "Chưa sẵn sàng",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                return;
            }

            if (_rawItems.Count == 0)
            {
                ScanFolder(true);
            }

            // "Chỉ push ảnh còn thiếu" là mặc định cứng: bỏ qua ảnh đã có mask.
            // Ảnh raw camera (PATH\raw) chưa có mask - nền xám - cũng được push để lấy mask.
            var pending = _rawItems.Where(x => !HasMask(x.FullPath)).ToList();

            if (pending.Count == 0)
            {
                MessageBox.Show(this,
                    _rawItems.Count == 0
                        ? "Không có ảnh nào để push."
                        : "Tất cả ảnh trong thư mục đều đã có ảnh mask - không còn gì để push.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateProductionForPush(pending))
            {
                return;
            }

            SaveSettings();
            SetBusy(true);

            _pushCts = new CancellationTokenSource();
            var token = _pushCts.Token;

            _pushTask = Task.Run(() => PushAll(pending, config, token), token)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        Log("Lỗi push: " + t.Exception.GetBaseException().Message);
                    }

                    SetBusy(false);

                    // Nếu người dùng đã tick 'Theo dõi thư mục' trong lúc lượt push này còn chạy,
                    // StartWatching() đã tự HOÃN lại để không bao giờ có 2 vòng push chạy chồng nhau.
                    // Bật lại ngay bây giờ, đúng lúc push thủ công đã thật sự kết thúc.
                    if (chkWatchFolder.Checked && _folderWatcher == null && _sdkRunning)
                    {
                        StartWatching();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void btnClearPushHistory_Click(object sender, EventArgs e)
        {
            var answer = MessageBox.Show(this,
                "Xoá toàn bộ lịch sử ảnh đã push? Các ảnh cũ sẽ được push lại ở lần quét sau.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            lock (_historyLock)
            {
                _pushedKeys.Clear();
                try
                {
                    if (File.Exists(HistoryPath))
                    {
                        File.Delete(HistoryPath);
                    }
                }
                catch (Exception ex)
                {
                    Log("Không xoá được file lịch sử: " + ex.Message);
                }
            }

            Log("Đã xoá lịch sử push.");
            UpdateStatus("Đã xoá lịch sử push.");
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            ClearLog();
        }

        private void btnShowLog_Click(object sender, EventArgs e)
        {
            ShowLogWindow();
        }

        #endregion

        #region Danh sách ảnh + phân trang

        private static int PageCount(int itemCount)
        {
            return Math.Max(1, (itemCount + PageSize - 1) / PageSize);
        }

        private void RenderRawPage()
        {
            Action apply = () =>
            {
                int pages = PageCount(_rawItems.Count);
                _rawPage = Math.Min(Math.Max(_rawPage, 0), pages - 1);

                int start = _rawPage * PageSize;
                int end = Math.Min(start + PageSize, _rawItems.Count);

                _suppressSelectionEvents = true;
                lvRawImage.BeginUpdate();
                try
                {
                    lvRawImage.Items.Clear();
                    var rows = new List<ListViewItem>(Math.Max(0, end - start));

                    for (int i = start; i < end; i++)
                    {
                        var item = _rawItems[i];
                        var row = new ListViewItem((i + 1).ToString());
                        row.SubItems.Add(ListName(item.FullPath));
                        row.SubItems.Add(item.LastWrite.ToString("dd/MM HH:mm:ss"));
                        row.BackColor = Theme.Back(item.StateColor == Color.Empty ? Color.White : item.StateColor);
                        row.ToolTipText = item.FullPath + Environment.NewLine + "Trạng thái: " + (item.State ?? "-")
                            + (string.IsNullOrEmpty(item.Decision) ? "" : Environment.NewLine + "Kết quả: " + item.Decision)
                            + CommentTooltip(item.FullPath);
                        row.Tag = i;
                        rows.Add(row);
                    }

                    lvRawImage.Items.AddRange(rows.ToArray());
                }
                finally
                {
                    lvRawImage.EndUpdate();
                    _suppressSelectionEvents = false;
                }

                lblRawCount.Text = "(" + _rawItems.Count + ")";
                lblRawPage.Text = (_rawPage + 1) + " / " + pages;
                btnRawFirst.Enabled = btnRawPrev.Enabled = _rawPage > 0;
                btnRawNext.Enabled = btnRawLast.Enabled = _rawPage < pages - 1;
            };

            RunOnUi(apply);
        }

        private void RenderMaskPage()
        {
            Action apply = () =>
            {
                int pages = PageCount(_maskItems.Count);
                _maskPage = Math.Min(Math.Max(_maskPage, 0), pages - 1);

                int start = _maskPage * PageSize;
                int end = Math.Min(start + PageSize, _maskItems.Count);

                _suppressSelectionEvents = true;
                lvMaskImage.BeginUpdate();
                try
                {
                    lvMaskImage.Items.Clear();
                    var rows = new List<ListViewItem>(Math.Max(0, end - start));

                    for (int i = start; i < end; i++)
                    {
                        var item = _maskItems[i];
                        var row = new ListViewItem((i + 1).ToString());
                        row.SubItems.Add(ListName(item.FullPath));
                        row.SubItems.Add(item.LastWrite.ToString("dd/MM HH:mm:ss"));
                        row.ToolTipText = item.FullPath + CommentTooltip(item.FullPath);
                        row.Tag = i;
                        rows.Add(row);
                    }

                    lvMaskImage.Items.AddRange(rows.ToArray());
                }
                finally
                {
                    lvMaskImage.EndUpdate();
                    _suppressSelectionEvents = false;
                }

                lblMaskCount.Text = "(" + _maskItems.Count + ")";
                lblMaskPage.Text = (_maskPage + 1) + " / " + pages;
                btnMaskFirst.Enabled = btnMaskPrev.Enabled = _maskPage > 0;
                btnMaskNext.Enabled = btnMaskLast.Enabled = _maskPage < pages - 1;
            };

            RunOnUi(apply);
        }

        private void btnRawFirst_Click(object sender, EventArgs e)
        {
            _rawPage = 0;
            RenderRawPage();
        }

        private void btnRawPrev_Click(object sender, EventArgs e)
        {
            _rawPage--;
            RenderRawPage();
        }

        private void btnRawNext_Click(object sender, EventArgs e)
        {
            _rawPage++;
            RenderRawPage();
        }

        private void btnRawLast_Click(object sender, EventArgs e)
        {
            _rawPage = PageCount(_rawItems.Count) - 1;
            RenderRawPage();
        }

        private void btnMaskFirst_Click(object sender, EventArgs e)
        {
            _maskPage = 0;
            RenderMaskPage();
        }

        private void btnMaskPrev_Click(object sender, EventArgs e)
        {
            _maskPage--;
            RenderMaskPage();
        }

        private void btnMaskNext_Click(object sender, EventArgs e)
        {
            _maskPage++;
            RenderMaskPage();
        }

        private void btnMaskLast_Click(object sender, EventArgs e)
        {
            _maskPage = PageCount(_maskItems.Count) - 1;
            RenderMaskPage();
        }

        /// <summary>
        /// Tạo dòng danh sách Raw cho một ảnh raw do camera chụp (PATH\raw\{tên}_OK.jpg), gắn sẵn
        /// ảnh mask tương ứng (PATH\mask\{tên}_mask_OK.jpg) và kết quả đọc từ tên file.
        /// </summary>
        private PushItem CreateCameraRawItem(string rawPath, string maskPath, string decision)
        {
            var info = new FileInfo(rawPath);
            maskPath = maskPath ?? FindCameraMaskPath(rawPath);

            var item = new PushItem
            {
                FullPath = rawPath,
                Length = info.Exists ? info.Length : 0,
                LastWriteUtc = info.Exists ? info.LastWriteTimeUtc : DateTime.UtcNow,
                LastWrite = info.Exists ? info.LastWriteTime : DateTime.Now,
                MaskPath = maskPath,
                FromCamera = true
            };

            ApplyCameraResultState(item, decision);
            return item;
        }

        /// <summary>
        /// Trạng thái + màu dòng của ảnh trong PATH\raw. Kết quả chỉ được ghi nhận khi CÓ ẢNH MASK đi
        /// kèm (kết quả lấy theo tên file mask - ảnh mask là kết quả UnitX thật sự trả về). Thiếu mask:
        /// nền xám "Chưa có kết quả", kết quả trong tên file raw bị bỏ qua, và được phép push lại.
        /// </summary>
        private void ApplyCameraResultState(PushItem item, string decision)
        {
            if (item.MaskPath == null)
            {
                item.Decision = null;
                item.State = "Chưa có kết quả (thiếu mask)";
                item.StateColor = NoResultColor;
                return;
            }

            if (decision == null)
            {
                var m = System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(item.MaskPath),
                    @"_(OK|NG|LIMIT)(?=_\d|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                decision = m.Success ? m.Groups[1].Value.ToUpperInvariant() : null;
            }

            item.Decision = decision;
            item.State = "Camera" + (decision == null ? "" : " - " + decision);
            item.StateColor = decision == "NG" ? FailedColor : decision == "OK" ? DoneColor : Color.White;
        }

        /// <summary>"_mask" hoặc "_mask_N" (N = 1..3 chữ số, KHÔNG nuốt số thứ tự 5 chữ số ở cuối tên).</summary>
        private static readonly System.Text.RegularExpressions.Regex CameraMaskMark =
            new System.Text.RegularExpressions.Regex(@"_mask(_\d{1,3}(?=_|$))?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        /// <summary>
        /// Ảnh mask của ảnh raw camera: "_raw" (hoặc chỗ trước phần kết quả/hậu tố ở ảnh cũ) thành "_mask".
        ///   PATH\raw\{tên}_raw_OK_{hậu tố}.jpg (hoặc {tên}_OK_{hậu tố}.jpg) -> PATH\mask\{tên}_mask_OK_{hậu tố}.jpg
        /// </summary>
        private static string FindCameraMaskPath(string rawPath)
        {
            try
            {
                string baseName, decisionTag, suffix;
                SplitCameraName(rawPath, out baseName, out decisionTag, out suffix);

                string rawDir = Path.GetDirectoryName(rawPath);
                string maskDir = Path.Combine(Path.GetDirectoryName(rawDir) ?? string.Empty, MaskFolderName);

                // Kết quả của mask có thể KHÁC kết quả trong tên raw (ảnh raw thiếu mask được push lại
                // và lần này UnitX trả NG thay vì OK) - thử kết quả của raw trước, rồi các kết quả khác.
                var tags = new List<string> { decisionTag };
                tags.AddRange(new[] { "_OK", "_NG", "_LIMIT", string.Empty }.Where(t => !string.Equals(t, decisionTag, StringComparison.OrdinalIgnoreCase)));

                foreach (var tag in tags)
                {
                    foreach (var extension in new[] { Path.GetExtension(rawPath), ".jpg", ".png", ".bmp", ".tif" }.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        string candidate = Path.Combine(maskDir, baseName + "_mask" + tag + suffix + extension);
                        if (File.Exists(candidate))
                        {
                            return candidate;
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

        /// <summary>Dấu ảnh raw trong tên file, đặt đúng chỗ "_mask" của ảnh mask cùng cặp.</summary>
        private const string RawMark = "_raw";

        /// <summary>
        /// Tách tên ảnh raw camera: {tên}[_raw]{_OK|_NG|_LIMIT}{_hậu tố}. Ví dụ
        /// Model1_DK1_NG1_raw_OK_20260925_163141_686 -> "Model1_DK1_NG1" + "_OK" + "_20260925_163141_686".
        /// "_raw" không bắt buộc: ảnh lưu trước khi có quy ước này vẫn tách đúng.
        /// </summary>
        private static void SplitCameraName(string rawPath, out string baseName, out string decisionTag, out string suffix)
        {
            var m = System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(rawPath),
                @"^(.*?)(_raw)?(_(?:OK|NG|LIMIT))?(_\d{8}_\d{6}_\d{3}|_\d{5})?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            baseName = m.Groups[1].Value;
            decisionTag = m.Groups[3].Value;
            suffix = m.Groups[4].Value;
        }

        /// <summary>Thêm ảnh raw camera vừa lưu vào danh sách Raw (nhảy tới trang cuối, chọn sẵn).</summary>
        private void AddCameraRawItem(string rawPath, string maskPath, string decision)
        {
            Action apply = () =>
            {
                if (_rawIndexByPath.ContainsKey(rawPath))
                {
                    return;
                }

                var item = CreateCameraRawItem(rawPath, maskPath, decision);

                _rawIndexByPath[rawPath] = _rawItems.Count;
                _rawItems.Add(item);

                _rawPage = PageCount(_rawItems.Count) - 1;
                RenderRawPage();
            };

            RunOnUi(apply);
        }

        /// <summary>Thêm một ảnh Raw mới phát hiện được (chế độ theo dõi thư mục) vào danh sách.</summary>
        private void AddRawItem(PushItem item)
        {
            Action apply = () =>
            {
                if (_rawIndexByPath.ContainsKey(item.FullPath))
                {
                    return;
                }

                item.State = "Chờ push";
                item.StateColor = Color.White;

                _rawIndexByPath[item.FullPath] = _rawItems.Count;

                string baseName = Path.GetFileNameWithoutExtension(item.FullPath);
                if (!_rawIndexByBaseName.ContainsKey(baseName))
                {
                    _rawIndexByBaseName[baseName] = _rawItems.Count;
                }

                _rawItems.Add(item);

                // Nhảy sang trang cuối để nhìn thấy ảnh vừa vào.
                _rawPage = PageCount(_rawItems.Count) - 1;
                RenderRawPage();
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Ghi nhận một file mask vừa được lưu: thêm vào danh sách Mask và gắn vào ảnh Raw tương ứng.
        /// </summary>
        private void RegisterMaskFile(string maskPath, string sourcePath)
        {
            Action apply = () =>
            {
                int existing = _maskItems.FindIndex(x =>
                    string.Equals(x.FullPath, maskPath, StringComparison.OrdinalIgnoreCase));

                var maskItem = new MaskItem { FullPath = maskPath, LastWrite = DateTime.Now };

                if (existing >= 0)
                {
                    _maskItems[existing] = maskItem;
                }
                else
                {
                    _maskItems.Add(maskItem);
                }

                int rawIndex;
                if (sourcePath != null && _rawIndexByPath.TryGetValue(sourcePath, out rawIndex))
                {
                    _rawItems[rawIndex].MaskPath = maskPath;
                }

                if (existing < 0)
                {
                    _maskPage = PageCount(_maskItems.Count) - 1;
                }

                RenderMaskPage();
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Cập nhật trạng thái của một ảnh Raw theo ĐƯỜNG DẪN (không theo chỉ số dòng): danh sách có
        /// phân trang nên dòng đang hiển thị không nhất thiết trùng chỉ số trong _rawItems.
        /// </summary>
        private void SetRowState(string fullPath, string state, Color backColor)
        {
            Action apply = () =>
            {
                int index;
                if (!_rawIndexByPath.TryGetValue(fullPath, out index))
                {
                    return;
                }

                var item = _rawItems[index];
                item.State = state;
                item.StateColor = backColor;

                // Chỉ động vào ListView nếu dòng đó đang nằm trong trang hiện tại.
                int start = _rawPage * PageSize;
                int offset = index - start;

                if (offset >= 0 && offset < lvRawImage.Items.Count)
                {
                    var row = lvRawImage.Items[offset];
                    row.BackColor = backColor;
                    row.ToolTipText = item.FullPath + Environment.NewLine + "Trạng thái: " + state
                        + (string.IsNullOrEmpty(item.Decision) ? "" : Environment.NewLine + "Kết quả: " + item.Decision);
                    row.EnsureVisible();
                }
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Chọn nhiều dòng (Shift + Click / Shift + mũi tên) sinh ra một sự kiện cho TỪNG dòng - gộp
        /// lại thành một lần xem trước dòng đang đứng, không đọc hàng trăm ảnh liên tiếp.
        /// </summary>
        /// <summary>Tính lại trạng thái dòng ảnh raw camera theo mask hiện có (sau khi push lại xong).</summary>
        private void RefreshCameraRowState(string rawPath)
        {
            RunOnUi(() =>
            {
                int index;
                if (!_rawIndexByPath.TryGetValue(rawPath, out index))
                {
                    return;
                }

                var item = _rawItems[index];
                item.MaskPath = FindCameraMaskPath(rawPath);
                ApplyCameraResultState(item, null);
                SetRowState(rawPath, item.State, item.StateColor);
            });
        }

        private void lvRawImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressSelectionEvents || _rawSelectionPending)
            {
                return;
            }

            _rawSelectionPending = true;
            BeginInvoke((Action)ApplyRawSelection);
        }

        private void ApplyRawSelection()
        {
            _rawSelectionPending = false;

            var active = ActiveRow(lvRawImage);
            if (active == null)
            {
                return;
            }

            var tag = active.Tag;
            if (!(tag is int))
            {
                return;
            }

            int index = (int)tag;
            if (index < 0 || index >= _rawItems.Count)
            {
                return;
            }

            var item = _rawItems[index];
            _selectedRawPath = item.FullPath;
            _selectedMaskPath = item.MaskPath ?? ResolveMaskPath(item.FullPath);
            item.MaskPath = _selectedMaskPath;

            _previewMode = PreviewMode.Raw;
            ShowSelectedImage();
            ApplyResultLabel(item.Decision);
        }

        private void lvMaskImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressSelectionEvents || _maskSelectionPending)
            {
                return;
            }

            _maskSelectionPending = true;
            BeginInvoke((Action)ApplyMaskSelection);
        }

        private void ApplyMaskSelection()
        {
            _maskSelectionPending = false;

            var active = ActiveRow(lvMaskImage);
            if (active == null)
            {
                return;
            }

            var tag = active.Tag;
            if (!(tag is int))
            {
                return;
            }

            int index = (int)tag;
            if (index < 0 || index >= _maskItems.Count)
            {
                return;
            }

            _selectedMaskPath = _maskItems[index].FullPath;

            // Tìm ảnh Raw tương ứng theo quy ước tên: <tên ảnh gốc>mask.<ext> trong thư mục con mask.
            string rawPath = FindRawForMask(_selectedMaskPath);
            _selectedRawPath = rawPath;

            _previewMode = PreviewMode.Mask;
            ShowSelectedImage();

            int rawIndex;
            if (rawPath != null && _rawIndexByPath.TryGetValue(rawPath, out rawIndex))
            {
                ApplyResultLabel(_rawItems[rawIndex].Decision);
            }
            else
            {
                ApplyResultLabel(null);
            }
        }

        /// <summary>
        /// Ảnh Raw tương ứng với một file mask: bỏ hậu tố "mask"/"mask_N" khỏi tên file rồi tra
        /// trong danh sách Raw đã quét (khớp theo tên không phân biệt phần mở rộng).
        /// </summary>
        private string FindRawForMask(string maskPath)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(maskPath);
                // <tên gốc>mask[_N][_OK|_NG] - phải bóc cả hậu tố kết quả, nếu không sẽ không tra
                // ngược được về ảnh Raw tương ứng.
                var match = System.Text.RegularExpressions.Regex.Match(name, @"^(.*?)mask(_\d+)?(_(?:OK|NG|LIMIT))?$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                string baseName = match.Success ? match.Groups[1].Value : name;

                int index;
                if (_rawIndexByBaseName.TryGetValue(baseName, out index))
                {
                    return _rawItems[index].FullPath;
                }

                // Ảnh camera chụp: không nằm trong danh sách Raw (không phải ảnh để push) nhưng file
                // raw được lưu cạnh đó: "_mask[_N]" trong tên mask đổi thành "_raw" (quy ước mới
                // {tên}_raw_OK_{hậu tố}) hoặc bỏ hẳn (ảnh cũ {tên}_OK_{hậu tố}, dạng cũ {tiền tố}_{tên}_OK).
                // Kết quả trong tên raw có thể khác của mask (raw thiếu mask được push lại) - thử mọi kết quả.
                string rawName = CameraMaskMark.Replace(name, string.Empty, 1);
                if (!string.Equals(rawName, name, StringComparison.Ordinal))
                {
                    string maskDir = Path.GetDirectoryName(maskPath);
                    string rawDir = Path.Combine(Path.GetDirectoryName(maskDir) ?? string.Empty, RawFolderName);

                    string rawBase, decisionTag, suffix;
                    SplitCameraName(rawName, out rawBase, out decisionTag, out suffix);

                    var tags = new List<string> { decisionTag };
                    tags.AddRange(new[] { "_OK", "_NG", "_LIMIT", string.Empty }.Where(t => !string.Equals(t, decisionTag, StringComparison.OrdinalIgnoreCase)));

                    foreach (var mark in new[] { RawMark, string.Empty })
                    {
                        foreach (var tag in tags)
                        {
                            foreach (var extension in new[] { Path.GetExtension(maskPath), ".jpg", ".png", ".bmp", ".tif" }.Distinct(StringComparer.OrdinalIgnoreCase))
                            {
                                string rawPath = Path.Combine(rawDir, rawBase + mark + tag + suffix + extension);
                                if (File.Exists(rawPath))
                                {
                                    return rawPath;
                                }
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

        #endregion

        #region Xem trước ảnh + kết quả OK/NG

        private void btnViewRaw_Click(object sender, EventArgs e)
        {
            _previewMode = PreviewMode.Raw;
            ShowSelectedImage();
        }

        private void btnViewMask_Click(object sender, EventArgs e)
        {
            _previewMode = PreviewMode.Mask;
            ShowSelectedImage();
        }

        private void TogglePreviewMode()
        {
            // Space luôn là chuyển nhanh MỘT ảnh Raw ↔ Mask: đang xem song song thì thoát song song rồi chuyển.
            if (_splitView)
            {
                SetSplitView(false);
            }

            _previewMode = _previewMode == PreviewMode.Raw ? PreviewMode.Mask : PreviewMode.Raw;
            ShowSelectedImage();
        }

        private void UpdatePreviewToggle()
        {
            // Xem song song: nút "Raw | Mask" sáng, hai nút Raw / Mask cùng tắt.
            bool raw = !_splitView && _previewMode == PreviewMode.Raw;
            bool mask = !_splitView && _previewMode == PreviewMode.Mask;

            StyleToggle(btnViewRaw, raw);
            StyleToggle(btnViewMask, mask);
            if (btnViewSplit != null)
            {
                StyleToggle(btnViewSplit, _splitView);
            }
        }

        private static void StyleToggle(Button button, bool active)
        {
            button.BackColor = active ? Color.FromArgb(37, 99, 235) : Color.FromArgb(243, 244, 246);
            button.ForeColor = active ? Color.White : Color.FromArgb(55, 65, 81);
        }

        /// <summary>Hiển thị ảnh đang chọn theo chế độ Raw/Mask hiện tại.</summary>
        private void ShowSelectedImage()
        {
            UpdatePreviewToggle();
            UpdateCommentBar();

            if (_splitView)
            {
                ShowSplitImages();
                return;
            }

            string path = _previewMode == PreviewMode.Raw ? _selectedRawPath : _selectedMaskPath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                ClearPreview(_previewMode == PreviewMode.Raw
                    ? "Chưa chọn ảnh Raw\r\n(Nhấn Space để chuyển đổi Raw / Mask)"
                    : "Chưa có ảnh Mask cho ảnh này\r\n(Nhấn Space để chuyển đổi Raw / Mask)");
                return;
            }

            var bitmap = LoadPreviewFromFile(path);
            if (bitmap == null)
            {
                ClearPreview("Không mở được ảnh:\r\n" + Path.GetFileName(path));
                return;
            }

            SetPreviewBitmap(bitmap);
            lblPreviewHeader.Text = "Xem trước ảnh - " + Path.GetFileName(path)
                + (_previewMode == PreviewMode.Raw ? "  [Raw]" : "  [Mask]");
        }

        /// <summary>
        /// Đọc ảnh từ file thành bitmap thu nhỏ. KHÔNG dùng Image.FromFile vì hàm đó khoá file lại
        /// tới khi bitmap được giải phóng - ảnh đang xem sẽ không thể bị ghi đè/xoá.
        /// </summary>
        private Bitmap LoadPreviewFromFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);

                using (var ms = new MemoryStream(bytes, false))
                using (var decoded = Image.FromStream(ms, false, false))
                using (var full = new Bitmap(decoded.Width, decoded.Height,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(full))
                    {
                        g.DrawImage(decoded, 0, 0, full.Width, full.Height);
                    }

                    return CreateThumbnail(full, PreviewMaxSize);
                }
            }
            catch (Exception ex)
            {
                Log($"Không hiển thị được ảnh {Path.GetFileName(path)}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Điểm vào duy nhất để đổi ảnh trên picPreview. Bitmap truyền vào thuộc quyền quản lý của
        /// PictureBox, ảnh cũ được giải phóng sau khi thay.
        /// </summary>
        private void SetPreviewBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return;
            }

            Action apply = () =>
            {
                long uiStart = Stopwatch.GetTimestamp();

                // Xem song song: ảnh theo chế độ Mask (ảnh vừa nhận) vào panel Mask, còn lại vào panel Raw.
                var box = PreviewTarget();
                var old = box.Image;
                box.Image = bitmap;
                box.EmptyText = null;
                lblPreviewHint.Visible = false;

                if (old != null)
                {
                    old.Dispose();
                }

                // Đang đo thời gian chụp camera: vẽ ngay để đo được đúng lúc ảnh lên màn hình.
                if (Volatile.Read(ref _displayProbe) != null)
                {
                    box.Update();
                    CompleteDisplayProbe(uiStart);
                }
            };

            RunOnUi(apply);
        }

        private void ClearPreview(string hint)
        {
            Action apply = () =>
            {
                if (_splitView)
                {
                    ReleaseImage(picPreviewMask);
                    picPreviewMask.EmptyText = hint;
                    return;
                }

                var old = picPreview.Image;
                picPreview.Image = null;

                if (old != null)
                {
                    old.Dispose();
                }

                lblPreviewHint.Text = hint;
                lblPreviewHint.Visible = true;
                lblPreviewHint.BringToFront();
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Ảnh kết quả vừa nhận từ UnitX: hiển thị ngay ở chế độ Mask cho ảnh vừa push.
        /// </summary>
        private void ShowLiveMask(string sourcePath, Bitmap bitmap)
        {
            Action apply = () =>
            {
                _selectedRawPath = sourcePath;
                _selectedMaskPath = ResolveMaskPath(sourcePath);
                _previewMode = PreviewMode.Mask;
                UpdatePreviewToggle();
                UpdateCommentBar();
                if (_splitView)
                {
                    ShowSplitRaw();
                }

                lblPreviewHeader.Text = "Xem trước ảnh - " + Path.GetFileName(sourcePath) + "  [Mask - vừa nhận]";
            };

            RunOnUi(apply);
            SetPreviewBitmap(bitmap);
        }

        /// <summary>
        /// Ảnh do CAMERA chụp vừa nhận về: hiển thị ngay, không gắn với ảnh Raw nào.
        /// Nếu ảnh đã được lưu xuống đĩa thì trỏ luôn chế độ Mask vào file đó để bấm Space qua lại
        /// vẫn mở đúng ảnh; chưa lưu thì vẫn hiện bitmap vừa nhận.
        /// </summary>
        private void ShowLiveCameraImage(string cameraTag, string savedRawPath, string savedPath, Bitmap bitmap)
        {
            Action apply = () =>
            {
                // Trỏ Raw vào file raw vừa lưu để nút Raw / phím Space mở được ảnh gốc của lần chụp
                // này (trước đây để null nên bấm Raw chỉ hiện "Chưa chọn ảnh Raw").
                _selectedRawPath = savedRawPath;
                _selectedMaskPath = savedPath;
                _previewMode = PreviewMode.Mask;
                UpdatePreviewToggle();
                UpdateCommentBar();
                if (_splitView)
                {
                    ShowSplitRaw();
                }

                lblPreviewHeader.Text = "Xem trước ảnh - camera " + cameraTag + "  [ảnh vừa chụp]";
            };

            RunOnUi(apply);
            SetPreviewBitmap(bitmap);
        }

        /// <summary>Lưu kết quả OK/NG của một ảnh và cập nhật nhãn kết quả nếu ảnh đó đang được xem.</summary>
        private void SetResultForPath(string fullPath, string decision)
        {
            Action apply = () =>
            {
                int index;
                if (_rawIndexByPath.TryGetValue(fullPath, out index))
                {
                    _rawItems[index].Decision = decision;
                }

                if (string.Equals(_selectedRawPath, fullPath, StringComparison.OrdinalIgnoreCase)
                    || _selectedRawPath == null)
                {
                    ApplyResultLabel(decision);
                }
            };

            RunOnUi(apply);
        }

        private void ApplyResultLabel(string decision)
        {
            Action apply = () =>
            {
                if (string.IsNullOrEmpty(decision))
                {
                    lblResult.Text = "OK / NG";
                    lblResult.BackColor = Color.White;
                    lblResult.ForeColor = Color.FromArgb(107, 114, 128);
                    return;
                }

                lblResult.Text = decision;

                if (decision == "OK")
                {
                    lblResult.BackColor = Color.FromArgb(22, 163, 74);
                    lblResult.ForeColor = Color.White;
                }
                else if (decision == "NG")
                {
                    lblResult.BackColor = Color.FromArgb(220, 38, 38);
                    lblResult.ForeColor = Color.White;
                }
                else
                {
                    lblResult.BackColor = Color.FromArgb(217, 119, 6);
                    lblResult.ForeColor = Color.White;
                }
            };

            RunOnUi(apply);
        }

        #endregion

        #region Trạng thái giao diện

        /// <summary>Thực thi một hành động trên luồng giao diện.</summary>
        private void RunOnUi(Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                if (IsDisposed || !IsHandleCreated)
                {
                    action();
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
                // Form đang đóng - bỏ qua.
            }
            catch (InvalidOperationException)
            {
                // Handle vừa bị huỷ giữa chừng - bỏ qua.
            }
        }

        /// <summary>
        /// Khoá/mở các thao tác thủ công trong lúc đang push cả thư mục.
        /// Không được để "Push toàn bộ" chạy song song với bất kỳ lệnh SDK nào khác trên cùng part
        /// group (xem ghi chú ở StartWatching / PushAll).
        /// </summary>
        private void SetBusy(bool busy)
        {
            Action apply = () =>
            {
                btnScan.Enabled = !busy;
                btnBrowse.Enabled = !busy;
                btnPushSelected.Enabled = !busy;
                btnClearPushHistory.Enabled = !busy;
                chkWatchFolder.Enabled = !busy;
                _pushBusyUi = busy;
                UpdateSidebarLock();
                // Các lệnh Part / Camera nằm trên thanh công cụ (không còn trong pnlPartCard).
                pnlPartToolbar.Enabled = !busy;
                pnlCameraToolbar.Enabled = !busy;

                // Nút Push đổi vai trò thành nút Dừng khi đang chạy.
                btnPushAll.Text = busy ? "Dừng push" : "Push tất cả";
                btnPushAll.BackColor = busy ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 163, 74);

                if (!busy)
                {
                    tspbPush.Value = 0;
                    UpdateActionButtons();
                }
            };

            RunOnUi(apply);
        }

        /// <summary>
        /// Khoá/mở các nút thao tác theo lô khi bật theo dõi thư mục (cả hai đường push dùng chung
        /// hàng đợi _pendingPushes nên không an toàn để chạy song song).
        /// </summary>
        private void SetWatching(bool watching)
        {
            Action apply = () =>
            {
                bool pushBusy = _pushTask != null && !_pushTask.IsCompleted;
                btnScan.Enabled = !watching;
                btnBrowse.Enabled = !watching;
                btnPushSelected.Enabled = !watching && !pushBusy;
                btnPushAll.Enabled = !watching && !pushBusy;
            };

            RunOnUi(apply);
        }

        /// <summary>Bỏ tick "Theo dõi thư mục" khi StartWatching() thất bại.</summary>
        private void SetWatchToggle(bool value)
        {
            RunOnUi(() => { chkWatchFolder.Checked = value; });
        }

        private bool _pushBusyUi;

        /// <summary>Đang chờ kết quả của một lần Trigger Once (xem LockSidebarUntilResult).</summary>
        private bool _triggerOnceWaiting;

        /// <summary>
        /// Khoá 3 khung cấu hình bên trái (Part Parameters, Network &amp; Communication, File &amp; Folder
        /// Management) khi đang Trigger Once / Repeat / Continuous: đổi part type, IP, thư mục lưu hay
        /// định dạng ảnh giữa chừng làm ảnh nhận về bị lưu sai chỗ / sai tên. Đang push thì khoá
        /// Part Parameters như trước.
        /// </summary>
        private void UpdateSidebarLock()
        {
            RunOnUi(() =>
            {
                bool triggering = _triggerLoopRunning || _triggerOnceWaiting;
                pnlSdkCard.Enabled = !triggering;
                pnlPathCard.Enabled = !triggering;
                pnlPartCard.Enabled = !triggering && !_pushBusyUi;
            });
        }

        /// <summary>
        /// Trigger Once: giữ khoá 3 khung cấu hình tới khi ảnh kết quả về (hoặc hết thời gian chờ) - lệnh
        /// trigger trả về gần như tức thì nhưng ảnh còn đang được chụp / xử lý / lưu.
        /// </summary>
        private async void LockSidebarUntilResult(int resultBaseline)
        {
            _triggerOnceWaiting = true;
            UpdateSidebarLock();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (Volatile.Read(ref _cameraResultCount) <= resultBaseline && watch.ElapsedMilliseconds < MissingResultTimeoutMs)
            {
                await Task.Delay(50);
            }

            _triggerOnceWaiting = false;
            UpdateSidebarLock();
        }

        /// <summary>Bật/tắt các nút cần kết nối UnitX.</summary>
        private void UpdateActionButtons()
        {
            Action apply = () =>
            {
                // Đang trigger liên tục thì khoá mọi lệnh SDK khác lại: chúng dùng chung một kết nối
                // và cùng một part group, chen ngang giữa chuỗi chụp sẽ làm hỏng chuỗi ảnh.
                bool triggering = _triggerLoopRunning;
                bool repeatMode = triggering && _triggerRepeatTotal > 0;
                bool ready = _sdkRunning && !triggering;

                btnPartStart.Enabled = ready;
                btnRemovePart.Enabled = ready;
                btnSystemReset.Enabled = ready;
                btnCameraReset.Enabled = ready;
                btnSelectSequence.Enabled = ready;
                numTriggerRepeat.Enabled = !triggering;

                // Nút của vòng đang chạy là nút DỪNG (Trigger Repeat -> btnTriggerOnce, Trigger
                // Continuous -> btnTriggerContinuous); nút còn lại khoá.
                btnTriggerOnce.Enabled = _sdkRunning && (!triggering || repeatMode);
                btnTriggerContinuous.Enabled = _sdkRunning && (!triggering || !repeatMode);
                btnRestartPart.Enabled = ready;
                btnPushAll.Enabled = !triggering;
                btnPushSelected.Enabled = !triggering;
            };

            RunOnUi(apply);
        }

        /// <summary>Đổi hình dạng nút Trigger Continuous giữa hai trạng thái chạy / dừng.</summary>
        private void SetTriggerRunning(bool running)
        {
            Action apply = () =>
            {
                // Hàm này được gọi TỪ TRONG continuation của _triggerTask, lúc đó IsTriggering vẫn còn
                // true - nên các nút dựa vào cờ này thay vì IsTriggering.
                _triggerLoopRunning = running;
                bool repeatMode = running && _triggerRepeatTotal > 0;
                bool continuous = running && !repeatMode;

                btnTriggerContinuous.Text = continuous ? "Dừng chụp" : "Chụp liên tục";
                btnTriggerContinuous.BackColor = continuous
                    ? Color.FromArgb(220, 38, 38)
                    : Color.White;
                btnTriggerContinuous.ForeColor = continuous
                    ? Color.White
                    : Color.FromArgb(29, 78, 216);

                if (!running)
                {
                    _triggerRepeatTotal = 0;
                }

                ApplyTriggerOnceStyle(repeatMode);
                UpdateActionButtons();
                UpdateSidebarLock();
            };

            RunOnUi(apply);
        }

        private void UpdateProgress(int done, int total, int success, int missing, int failed)
        {
            Action apply = () =>
            {
                tspbPush.Maximum = Math.Max(total, 1);
                tspbPush.Value = Math.Min(Math.Max(done, 0), tspbPush.Maximum);
                tsslStatus.Text = $"{done}/{total} | Có mask: {success} | Thiếu: {missing} | Lỗi: {failed}";
            };

            RunOnUi(apply);
        }

        private void UpdateStatus(string text)
        {
            RunOnUi(() => { tsslStatus.Text = text; });
        }

        #endregion

        #region Log

        /// <summary>
        /// Ghi log KHÔNG chặn luồng gọi: chỉ đẩy chuỗi vào hàng đợi (vài chục nanô giây), một Timer
        /// gom lại và đổ sang cửa sổ log theo lô 200 ms một lần. Trong vòng push dày đặc, cách cũ
        /// (mỗi dòng một lần BeginInvoke + thêm item + EnsureVisible) làm nghẽn cả luồng giao diện.
        /// </summary>
        /// <summary>Mức log tự nhận theo nội dung (AppLog.Classify) - xem quy tắc ở AppLog.</summary>
        private void Log(string message)
        {
            Log(AppLog.Classify(message), message);
        }

        /// <summary>Bất thường nhưng vẫn chạy tiếp được - tô cam trong cửa sổ log.</summary>
        private void LogWarning(string message)
        {
            Log(AppLogLevel.Warning, message);
        }

        /// <summary>Thao tác thất bại / ngoại lệ - tô đỏ trong cửa sổ log.</summary>
        private void LogError(string message)
        {
            Log(AppLogLevel.Error, message);
        }

        private void Log(AppLogLevel level, string message)
        {
            _logQueue.Enqueue(new LogEntry { Time = DateTime.Now.ToString("HH:mm:ss.fff"), Level = level, Message = message });
        }

        private void FlushLogQueue(object sender, EventArgs e)
        {
            if (_logQueue.IsEmpty)
            {
                return;
            }

            var batch = new List<LogEntry>();
            LogEntry entry;

            while (batch.Count < 500 && _logQueue.TryDequeue(out entry))
            {
                batch.Add(entry);
            }

            if (batch.Count == 0)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(AppDataDir);
                File.AppendAllLines(AppLogPath, batch.Select(b => b.Time + "  " + b.LevelText.PadRight(5) + "  " + b.Message), Encoding.UTF8);
            }
            catch
            {
                // Không ghi được file log (đang bị khoá...) thì vẫn hiện trên cửa sổ Log như cũ.
            }

            _logLines.AddRange(batch);
            if (_logLines.Count > MaxLogLines)
            {
                _logLines.RemoveRange(0, _logLines.Count - MaxLogLines);
            }

            if (_logWindow != null && !_logWindow.IsDisposed && _logWindow.Visible)
            {
                _logWindow.AppendLines(batch, MaxLogLines);
            }
        }

        private void ShowLogWindow()
        {
            RunOnUi(() =>
            {
                if (_logWindow == null || _logWindow.IsDisposed)
                {
                    _logWindow = new frmLogV1();
                    _logWindow.Owner = this;
                    _logWindow.ClearRequested = ClearLog;
                }

                _logWindow.LoadLines(_logLines);

                if (!_logWindow.Visible)
                {
                    _logWindow.Show(this);
                }

                _logWindow.BringToFront();
            });
        }

        private void ClearLog()
        {
            _logLines.Clear();

            if (_logWindow != null && !_logWindow.IsDisposed)
            {
                _logWindow.ClearView();
            }

            Log("Đã xoá log.");
        }

        #endregion

        #region Camera ID <-> Sequence

        /// <summary>
        /// Camera ID đổi thì ô "Sequence IO" tự điền theo (yêu cầu: "Sequence id tự động load camera id").
        /// camera_id vẫn được thêm vào danh sách của combobox nên danh sách 0..5 sẵn có không mất đi -
        /// người dùng vẫn chọn lại số sequence bất cứ lúc nào.
        /// </summary>
        private void cboCameraID_Changed(object sender, EventArgs e)
        {
            if (_suppressCameraSync)
            {
                return;
            }

            string cameraId = (cboCameraID.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cameraId))
            {
                return;
            }

            _suppressCameraSync = true;
            try
            {
                if (!cboSequenceIO.Items.Contains(cameraId))
                {
                    cboSequenceIO.Items.Add(cameraId);
                }

                cboSequenceIO.Text = cameraId;
            }
            finally
            {
                _suppressCameraSync = false;
            }
        }

        /// <summary>
        /// Ghi nhớ số sequence hợp lệ gần nhất. Khi ô Sequence IO đang hiển thị camera_id (do tự
        /// điền) thì select_sequence() dùng lại số này thay vì cố ép camera_id thành số.
        /// </summary>
        private void cboSequenceIO_TextChanged(object sender, EventArgs e)
        {
            string text = (cboSequenceIO.Text ?? string.Empty).Trim();
            int value;

            // Chỉ nhận các số nhỏ: camera_id của hãng là chuỗi số dài (vd 40497555) nên nếu không
            // chặn ngưỡng thì chính camera_id sẽ bị hiểu nhầm là số sequence.
            if (int.TryParse(text, out value) && value >= 0 && value <= MaxSequenceNumber)
            {
                _sequenceNumber = value;
            }
        }

        #endregion

        #region Push một ảnh đang chọn

        /// <summary>
        /// Push đúng một ảnh đang chọn trong danh sách raw. Khác "Push toàn bộ" ở chỗ KHÔNG lọc bỏ
        /// ảnh đã có mask: người dùng chủ động chọn ảnh nào thì push lại ảnh đó, kể cả đã push rồi.
        /// </summary>
        private void btnPushSelected_Click(object sender, EventArgs e)
        {
            PushSelectedRawItems();
        }

        /// <summary>Push các ảnh raw đang chọn (một hoặc nhiều dòng) - nút "Push ảnh chọn" và menu chuột phải.</summary>
        private void PushSelectedRawItems()
        {
            if (_pushTask != null && !_pushTask.IsCompleted)
            {
                MessageBox.Show(this, "Đang có một lượt push chạy. Chờ xong hoặc bấm 'Dừng push'.",
                    "Đang push", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsTriggering)
            {
                MessageBox.Show(this, "Đang trigger liên tục. Bấm 'Dừng Trigger' trước khi push.",
                    "Đang trigger", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_folderWatcher != null)
            {
                MessageBox.Show(this,
                    "Đang bật 'Theo dõi thư mục'. Tắt theo dõi trước khi push thủ công.",
                    "Đang theo dõi thư mục", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_sdkRunning)
            {
                MessageBox.Show(this, "Chưa kết nối UnitX.", "Chưa sẵn sàng",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = GetSelectedRawItems();
            if (selected.Count == 0)
            {
                MessageBox.Show(this,
                    "Chưa chọn ảnh nào. Bấm vào một dòng trong danh sách 'Ảnh raw' rồi thử lại.",
                    "Chưa chọn ảnh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ảnh camera ĐÃ có mask (đã có kết quả) thì bỏ qua; thiếu mask (nền xám) thì push được.
            var single = selected.Where(i => !i.FromCamera || !HasMask(i.FullPath)).ToList();
            if (single.Count == 0)
            {
                MessageBox.Show(this,
                    "Các ảnh đã chọn đều đã có ảnh mask (đã có kết quả) - không push lại lên UnitX.",
                    "Ảnh camera", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var missing = single.Where(i => !File.Exists(i.FullPath)).ToList();
            if (missing.Count > 0)
            {
                MessageBox.Show(this, "Không tìm thấy file:" + Environment.NewLine
                    + string.Join(Environment.NewLine, missing.Take(10).Select(i => i.FullPath)),
                    "Thiếu file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = BuildConfig();
            if (config == null)
            {
                return;
            }

            if (!ValidateProductionForPush(single))
            {
                return;
            }

            SaveSettings();
            SetBusy(true);

            Log(single.Count == 1
                ? "Push 1 ảnh đang chọn: " + single[0].FullPath
                : $"Push {single.Count} ảnh đang chọn (bỏ qua {selected.Count - single.Count} ảnh camera đã có mask).");

            _pushCts = new CancellationTokenSource();
            var token = _pushCts.Token;

            _pushTask = Task.Run(() => PushAll(single, config, token), token)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        Log("Lỗi push: " + t.Exception.GetBaseException().Message);
                    }

                    SetBusy(false);

                    if (chkWatchFolder.Checked && _folderWatcher == null && _sdkRunning)
                    {
                        StartWatching();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Cấu hình mạng UnitX (client.yaml)

        /// <summary>
        /// Đọc %USERPROFILE%\UnitX SDK\config\client.yaml lên giao diện. Đây chính là file mà
        /// unitx_sdk.dll dùng để biết "addresses" của edge - thiếu/sai file này thì mọi lệnh gRPC
        /// đều trả về "failed to connect to all addresses".
        /// </summary>
        private void LoadSdkNetworkConfig()
        {
            // Giá trị mặc định = đúng file mẫu của hãng, dùng khi chưa có client.yaml.
            txtSdkHost.Text = "192.168.10.1";
            numSdkGrpc.Value = 20049;
            numSdkZmq.Value = 20046;
            numSdkBlob.Value = 20050;
            numSdkClient.Value = 20051;

            try
            {
                if (!File.Exists(SdkConfigPath))
                {
                    SetSdkConfigStatus("Chưa có client.yaml - bấm 'Lưu & Áp dụng' để tạo.", Color.FromArgb(180, 83, 9));
                    Log("Không thấy file cấu hình SDK: " + SdkConfigPath);
                    return;
                }

                string host;
                int grpc, zmq, blob, clientGrpc;
                if (TryParseClientYaml(File.ReadAllLines(SdkConfigPath), out host, out grpc, out zmq, out blob, out clientGrpc))
                {
                    if (!string.IsNullOrWhiteSpace(host))
                    {
                        txtSdkHost.Text = host;
                    }

                    if (grpc > 0) numSdkGrpc.Value = grpc;
                    if (zmq > 0) numSdkZmq.Value = zmq;
                    if (blob > 0) numSdkBlob.Value = blob;
                    if (clientGrpc > 0) numSdkClient.Value = clientGrpc;
                }

                SetSdkConfigStatus("Đã đọc client.yaml", Color.FromArgb(107, 114, 128));
                Log($"Cấu hình SDK: host={txtSdkHost.Text}, grpc={(int)numSdkGrpc.Value}, "
                    + $"zmq={(int)numSdkZmq.Value}, blob={(int)numSdkBlob.Value}, client_grpc={(int)numSdkClient.Value}"
                    + " (" + SdkConfigPath + ")");
            }
            catch (Exception ex)
            {
                SetSdkConfigStatus("Lỗi đọc client.yaml", Color.FromArgb(185, 28, 28));
                Log("Không đọc được client.yaml: " + ex.Message);
            }
        }

        /// <summary>
        /// Đọc các khoá cần thiết từ client.yaml. Không dùng thư viện YAML (dự án không tham chiếu)
        /// - file của hãng chỉ có 2 khối phẳng nên đọc theo dòng là đủ và không kéo thêm phụ thuộc.
        /// Phân biệt grpc_port của "client" và của "edges" bằng khối đang đứng.
        /// </summary>
        private static bool TryParseClientYaml(string[] lines, out string host,
            out int edgeGrpc, out int zmq, out int blob, out int clientGrpc)
        {
            host = null;
            edgeGrpc = zmq = blob = clientGrpc = 0;

            string section = null;
            bool any = false;

            foreach (var raw in lines)
            {
                string line = raw;
                int comment = line.IndexOf('#');
                if (comment >= 0)
                {
                    line = line.Substring(0, comment);
                }

                if (line.Trim().Length == 0)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(line[0]) && line.TrimEnd().EndsWith(":"))
                {
                    section = line.Trim().TrimEnd(':');
                    continue;
                }

                string body = line.TrimStart();
                if (body.StartsWith("- "))
                {
                    body = body.Substring(2).TrimStart();
                }

                int colon = body.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }

                string key = body.Substring(0, colon).Trim();
                string value = body.Substring(colon + 1).Trim().Trim('"', '\'');

                int number;
                switch (key)
                {
                    case "host":
                        if (section == "edges") { host = value; any = true; }
                        break;
                    case "zmq_port":
                        if (int.TryParse(value, out number)) { zmq = number; any = true; }
                        break;
                    case "blob_port":
                        if (int.TryParse(value, out number)) { blob = number; any = true; }
                        break;
                    case "grpc_port":
                        if (int.TryParse(value, out number))
                        {
                            if (section == "client") { clientGrpc = number; }
                            else { edgeGrpc = number; }
                            any = true;
                        }
                        break;
                }
            }

            return any;
        }

        private void btnSdkSave_Click(object sender, EventArgs e)
        {
            string host = (txtSdkHost.Text ?? string.Empty).Trim();

            IPAddress parsed;
            if (string.IsNullOrWhiteSpace(host) ||
                (!IPAddress.TryParse(host, out parsed) && host.IndexOf(' ') >= 0))
            {
                MessageBox.Show(this, "IP UnitX không hợp lệ.", "Sai cấu hình",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!WriteSdkNetworkConfig(host))
            {
                return;
            }

            // unitx_sdk.dll đọc client.yaml MỘT LẦN mỗi tiến trình: kết nối lại (StopSDK + RunSDK)
            // trong cùng tiến trình vẫn dùng cấu hình cũ và báo lỗi -> bắt buộc khởi động lại phần mềm.
            var answer = MessageBox.Show(this,
                "Đã lưu cấu hình mạng UnitX (client.yaml)." + Environment.NewLine + Environment.NewLine
                + "Cần KHỞI ĐỘNG LẠI phần mềm để áp dụng - SDK chỉ đọc cấu hình này lúc khởi động." + Environment.NewLine
                + "Khởi động lại ngay bây giờ?",
                "Áp dụng cấu hình mới", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                RestartApplication();
            }
            else
            {
                Log("Đã lưu client.yaml - cấu hình mới có hiệu lực ở lần mở phần mềm tiếp theo.");
                SetSdkConfigStatus("Đã lưu - khởi động lại để áp dụng", Color.FromArgb(180, 83, 9));
            }
        }

        /// <summary>
        /// Đóng form (FormClosing lưu cài đặt, dừng trigger / push / SDK như khi thoát bình thường)
        /// rồi Program.Main mở lại phần mềm. Người dùng huỷ đóng form (đang push) thì huỷ luôn restart.
        /// </summary>
        private void RestartApplication()
        {
            Log("Khởi động lại phần mềm để áp dụng cấu hình mới...");
            Program.RestartRequested = true;
            Close();

            if (!IsDisposed && Visible)
            {
                Program.RestartRequested = false;
                Log("Đã huỷ khởi động lại (form không đóng).");
            }
        }

        /// <summary>
        /// Ghi lại client.yaml, GIỮ NGUYÊN các dòng/khoá không liên quan (version, name, các edge
        /// khác...) thay vì sinh lại file từ đầu. Chỉ sửa edge đang dùng (_edge), nếu không tìm thấy
        /// thì sửa edge đầu tiên.
        /// </summary>
        private bool WriteSdkNetworkConfig(string host)
        {
            int edgeGrpc = (int)numSdkGrpc.Value;
            int zmq = (int)numSdkZmq.Value;
            int blob = (int)numSdkBlob.Value;
            int clientGrpc = (int)numSdkClient.Value;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SdkConfigPath));

                if (!File.Exists(SdkConfigPath))
                {
                    string edgeName = string.IsNullOrWhiteSpace(_edge) ? "line_1" : _edge;
                    var template = new StringBuilder();
                    template.AppendLine("client:");
                    template.AppendLine("  - name: \"client\"");
                    template.AppendLine("    grpc_port: " + clientGrpc);
                    template.AppendLine("edges:");
                    template.AppendLine("  - name: " + edgeName);
                    template.AppendLine("    host: " + host);
                    template.AppendLine("    zmq_port: " + zmq);
                    template.AppendLine("    grpc_port: " + edgeGrpc);
                    template.AppendLine("    blob_port: " + blob);
                    template.AppendLine("    version: 1.0");

                    File.WriteAllText(SdkConfigPath, template.ToString(), new UTF8Encoding(false));
                    Log("Đã tạo mới " + SdkConfigPath);
                    SetSdkConfigStatus("Đã tạo client.yaml", Color.FromArgb(22, 163, 74));
                    return true;
                }

                // Sao lưu một lần trước khi sửa file cấu hình của SDK.
                string backup = SdkConfigPath + ".bak";
                try
                {
                    if (!File.Exists(backup))
                    {
                        File.Copy(SdkConfigPath, backup);
                    }
                }
                catch (Exception exBackup)
                {
                    Log("Không tạo được bản sao lưu client.yaml: " + exBackup.Message);
                }

                var lines = new List<string>(File.ReadAllLines(SdkConfigPath));
                int targetEdgeStart = FindTargetEdge(lines);

                string section = null;
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && line.TrimEnd().EndsWith(":"))
                    {
                        section = line.Trim().TrimEnd(':');
                        continue;
                    }

                    if (section == "client")
                    {
                        TrySetYamlValue(lines, i, "grpc_port", clientGrpc.ToString());
                        continue;
                    }

                    if (section != "edges" || targetEdgeStart < 0)
                    {
                        continue;
                    }

                    // Chỉ sửa trong phạm vi edge được chọn: từ dòng "- name:" của nó tới edge kế tiếp.
                    if (i < targetEdgeStart || (i > targetEdgeStart && IsEdgeItemStart(lines[i])))
                    {
                        if (i > targetEdgeStart && IsEdgeItemStart(lines[i]))
                        {
                            targetEdgeStart = -1;
                        }
                        continue;
                    }

                    TrySetYamlValue(lines, i, "host", host);
                    TrySetYamlValue(lines, i, "zmq_port", zmq.ToString());
                    TrySetYamlValue(lines, i, "grpc_port", edgeGrpc.ToString());
                    TrySetYamlValue(lines, i, "blob_port", blob.ToString());
                }

                File.WriteAllLines(SdkConfigPath, lines, new UTF8Encoding(false));
                Log($"Đã ghi client.yaml: host={host}, grpc={edgeGrpc}, zmq={zmq}, blob={blob}, client_grpc={clientGrpc}");
                SetSdkConfigStatus("Đã lưu client.yaml", Color.FromArgb(22, 163, 74));
                return true;
            }
            catch (Exception ex)
            {
                Log("Không ghi được client.yaml: " + ex.Message);
                MessageBox.Show(this,
                    "Không ghi được file cấu hình:" + Environment.NewLine + SdkConfigPath
                    + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetSdkConfigStatus("Lỗi ghi client.yaml", Color.FromArgb(185, 28, 28));
                return false;
            }
        }

        private static bool IsEdgeItemStart(string line)
        {
            string body = line.TrimStart();
            return body.StartsWith("- ") && body.Substring(2).TrimStart().StartsWith("name:");
        }

        /// <summary>Chỉ số dòng "- name: &lt;_edge&gt;" trong khối edges; -1 nếu file không có edge nào.</summary>
        private int FindTargetEdge(List<string> lines)
        {
            string wanted = (_edge ?? string.Empty).Trim();
            int first = -1;
            string section = null;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && line.TrimEnd().EndsWith(":"))
                {
                    section = line.Trim().TrimEnd(':');
                    continue;
                }

                if (section != "edges" || !IsEdgeItemStart(line))
                {
                    continue;
                }

                if (first < 0)
                {
                    first = i;
                }

                string value = line.Substring(line.IndexOf("name:") + 5).Trim().Trim('"', '\'');
                if (!string.IsNullOrEmpty(wanted) && string.Equals(value, wanted, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return first;
        }

        /// <summary>Thay giá trị của khoá <paramref name="key"/> nếu dòng i đúng là khoá đó, giữ nguyên thụt lề.</summary>
        private static void TrySetYamlValue(List<string> lines, int i, string key, string value)
        {
            string line = lines[i];
            string body = line.TrimStart();
            string indent = line.Substring(0, line.Length - body.Length);

            string prefix = string.Empty;
            if (body.StartsWith("- "))
            {
                prefix = "- ";
                body = body.Substring(2).TrimStart();
                indent = line.Substring(0, line.Length - body.Length - 2);
            }

            if (!body.StartsWith(key + ":"))
            {
                return;
            }

            lines[i] = indent + prefix + key + ": " + value;
        }

        private void SetSdkConfigStatus(string text, Color color)
        {
            RunOnUi(() =>
            {
                lblSdkStatus.Text = text;
                lblSdkStatus.ForeColor = color;
            });
        }

        /// <summary>Ngắt rồi kết nối lại SDK để thử nạp client.yaml mới mà không phải tắt phần mềm.</summary>
        private async void ReconnectSdk()
        {
            try
            {
                UpdateStatus("Đang kết nối lại UnitX...");
                StopSdk();
                await Task.Delay(500);
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                Log("Kết nối lại thất bại: " + ex.Message);
            }
        }

        #endregion

        #region Kiểm tra mạng tới UnitX

        /// <summary>
        /// Kiểm tra đúng 4 thứ quyết định việc "trigger được nhưng không nhận được ảnh":
        ///   1. ping tới host  - máy UnitX có sống và có route không.
        ///   2. TCP gRPC port  - kênh RA lệnh (trigger/start part). Mở = lệnh đi được.
        ///   3. TCP ZMQ port   - kênh NHẬN kết quả IMAGE_RESULT. Đóng = trigger OK nhưng không có ảnh.
        ///   4. TCP blob port  - kênh tải dữ liệu ảnh. Đóng = có kết quả nhưng blob rỗng.
        /// Kèm kiểm tra cổng gRPC của client có đang được lắng nghe trên máy này không.
        /// </summary>
        private async void btnSdkTest_Click(object sender, EventArgs e)
        {
            if (_networkTestRunning)
            {
                return;
            }

            string host = (txtSdkHost.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show(this, "Chưa nhập IP UnitX.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int grpcPort = (int)numSdkGrpc.Value;
            int zmqPort = (int)numSdkZmq.Value;
            int blobPort = (int)numSdkBlob.Value;
            int clientPort = (int)numSdkClient.Value;

            _networkTestRunning = true;
            btnSdkTest.Enabled = false;
            SetSdkConfigStatus("Đang kiểm tra...", Color.FromArgb(180, 83, 9));

            // Kết quả hiện ngay trên nhãn trạng thái cạnh nút - KHÔNG bật cửa sổ log nữa; chi tiết
            // từng cổng vẫn được ghi vào log cho ai cần xem.
            try
            {
                var result = await RunNetworkCheckAsync(host, grpcPort, zmqPort, blobPort, clientPort);

                SetSdkConfigStatus(result.Summary, result.AllOpen
                    ? Color.FromArgb(22, 163, 74)
                    : Color.FromArgb(185, 28, 28));
            }
            finally
            {
                _networkTestRunning = false;
                btnSdkTest.Enabled = true;
            }
        }

        /// <summary>Kết quả kiểm tra mạng tới UnitX (dùng chung cho nút "Kiểm tra mạng" và cửa sổ trạng thái).</summary>
        private sealed class NetworkCheckResult
        {
            public string Host;
            public int GrpcPort, ZmqPort, BlobPort, ClientPort;
            public bool Ping, Grpc, Zmq, Blob, ClientListening;
            public string Conclusion;

            public bool AllOpen
            {
                get { return Grpc && Zmq && Blob; }
            }

            public string Summary
            {
                get
                {
                    return $"ping {(Ping ? "OK" : "X")} | gRPC {(Grpc ? "OK" : "X")} "
                        + $"| ZMQ {(Zmq ? "OK" : "X")} | Blob {(Blob ? "OK" : "X")}";
                }
            }
        }

        /// <summary>Ping + thử 3 cổng TCP (song song) + cổng client local; ghi log chi tiết và kết luận.</summary>
        private async Task<NetworkCheckResult> RunNetworkCheckAsync(string host, int grpcPort, int zmqPort,
            int blobPort, int clientPort)
        {
            Log("----- KIỂM TRA MẠNG TỚI UNITX -----");
            Log("Host: " + host);

            var pingTask = PingHostAsync(host);
            var grpcTask = TestPortAsync(host, grpcPort, "gRPC (ra lệnh)");
            var zmqTask = TestPortAsync(host, zmqPort, "ZMQ (nhận kết quả)");
            var blobTask = TestPortAsync(host, blobPort, "Blob (tải ảnh)");
            await Task.WhenAll(pingTask, grpcTask, zmqTask, blobTask);

            var r = new NetworkCheckResult
            {
                Host = host,
                GrpcPort = grpcPort,
                ZmqPort = zmqPort,
                BlobPort = blobPort,
                ClientPort = clientPort,
                Ping = pingTask.Result,
                Grpc = grpcTask.Result,
                Zmq = zmqTask.Result,
                Blob = blobTask.Result,
                ClientListening = IsLocalPortListening(clientPort)
            };

            Log($"Cổng gRPC của client ({clientPort}) đang được lắng nghe trên máy này: {r.ClientListening}");

            if (!r.Ping && !r.Grpc && !r.Zmq && !r.Blob)
            {
                r.Conclusion = "Không tới được máy UnitX. Kiểm tra dây/VLAN, IP của máy Ubuntu và route.";
            }
            else if (r.Ping && !r.Grpc && !r.Zmq && !r.Blob)
            {
                r.Conclusion = "Ping được nhưng cả 3 cổng đều đóng: dây mạng OK, dịch vụ UnitX Edge chưa chạy/chưa mở cổng, "
                    + "sai số cổng, firewall Ubuntu chặn, hoặc IP đang trùng với thiết bị khác.";
            }
            else if (r.Grpc && !r.Zmq)
            {
                r.Conclusion = "gRPC mở nhưng ZMQ ĐÓNG: trigger thành công nhưng sẽ KHÔNG nhận được kết quả/ảnh.";
            }
            else if (r.Grpc && r.Zmq && !r.Blob)
            {
                r.Conclusion = "ZMQ mở nhưng Blob ĐÓNG: nhận được OK/NG nhưng không tải được ảnh.";
            }
            else if (r.AllOpen)
            {
                r.Conclusion = "Cả 3 cổng đều mở - mạng không có vấn đề.";
            }
            else
            {
                r.Conclusion = "Kết quả hỗn hợp - xem chi tiết từng cổng.";
            }

            Log("KẾT LUẬN: " + r.Conclusion);
            Log("-----------------------------------");
            return r;
        }

        private async Task<bool> PingHostAsync(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, 1500);
                    bool ok = reply != null && reply.Status == IPStatus.Success;
                    Log($"ping {host} => {(reply == null ? "null" : reply.Status.ToString())}"
                        + (ok ? $" ({reply.RoundtripTime} ms)" : string.Empty));
                    return ok;
                }
            }
            catch (Exception ex)
            {
                Log($"ping {host} lỗi: {ex.Message} (một số hệ thống chặn ICMP - cứ xem kết quả cổng TCP)");
                return false;
            }
        }

        private async Task<bool> TestPortAsync(string host, int port, string label)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connect = client.ConnectAsync(host, port);
                    var done = await Task.WhenAny(connect, Task.Delay(2500));

                    if (done != connect)
                    {
                        Log($"TCP {host}:{port} [{label}] => TIMEOUT (không có ai trả lời)");
                        return false;
                    }

                    await connect;
                    Log($"TCP {host}:{port} [{label}] => MỞ");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"TCP {host}:{port} [{label}] => ĐÓNG ({ex.GetBaseException().Message})");
                return false;
            }
        }

        private static bool IsLocalPortListening(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(x => x.Port == port);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Cấu hình (settings.json)

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return;
                }

                var settings = JsonConvert.DeserializeObject<PushSettings>(File.ReadAllText(SettingsPath));
                if (settings == null)
                {
                    return;
                }

                txtPath.Text = settings.Folder ?? string.Empty;

                if (!string.IsNullOrEmpty(settings.PartType))
                {
                    // Chỉ khôi phục GIÁ TRỊ dùng lần trước cho tiện; danh sách part_type vẫn chỉ
                    // đến từ UnitX (ApplyPartTypes), không còn danh sách tạo sẵn trong code.
                    if (!cboPartType.Items.Contains(settings.PartType))
                    {
                        cboPartType.Items.Add(settings.PartType);
                    }

                    cboPartType.Text = settings.PartType;
                }

                if (!string.IsNullOrEmpty(settings.PartID))
                {
                    txtPartID.Text = settings.PartID;
                }

                if (!string.IsNullOrEmpty(settings.CaptureConfig))
                {
                    cboCaptureConfig.Text = settings.CaptureConfig;
                }

                if (!string.IsNullOrEmpty(settings.CameraId))
                {
                    if (!cboCameraID.Items.Contains(settings.CameraId))
                    {
                        cboCameraID.Items.Add(settings.CameraId);
                    }
                    cboCameraID.Text = settings.CameraId;
                }

                if (!string.IsNullOrEmpty(settings.SequenceIo))
                {
                    if (!cboSequenceIO.Items.Contains(settings.SequenceIo))
                    {
                        cboSequenceIO.Items.Add(settings.SequenceIo);
                    }
                    cboSequenceIO.Text = settings.SequenceIo;
                }

                // Edge không có trên giao diện: giá trị đã lưu được ưu tiên hơn giá trị suy ra từ
                // part_type (ApplyStationDefaults), để người dùng ghi đè được bằng settings.json.
                if (settings.Edge != null)
                {
                    _edge = settings.Edge;
                }

                if (settings.DelayMs.HasValue)
                {
                    numDelay.Value = Math.Min(Math.Max(settings.DelayMs.Value, (int)numDelay.Minimum), (int)numDelay.Maximum);
                }

                if (settings.TriggerRepeat.HasValue)
                {
                    numTriggerRepeat.Value = Math.Min(Math.Max(settings.TriggerRepeat.Value, (int)numTriggerRepeat.Minimum), (int)numTriggerRepeat.Maximum);
                }

                if (settings.TimeoutMs.HasValue && settings.TimeoutMs.Value > 0)
                {
                    _timeoutMs = settings.TimeoutMs.Value;
                }

                if (settings.SaveCallback.HasValue)
                {
                    chkSaveCallback.Checked = settings.SaveCallback.Value;
                }

                if (settings.SaveRaw.HasValue)
                {
                    chkSaveRaw.Checked = settings.SaveRaw.Value;
                }

                if (settings.ImageName != null)
                {
                    txtImageName.Text = settings.ImageName;
                }

                if (settings.NamePrefix.HasValue &&
                    settings.NamePrefix.Value >= 0 &&
                    settings.NamePrefix.Value < cboNamePrefix.Items.Count)
                {
                    cboNamePrefix.SelectedIndex = settings.NamePrefix.Value;
                }

                if (settings.ImageFormat.HasValue &&
                    settings.ImageFormat.Value >= 0 &&
                    settings.ImageFormat.Value < cboImageFormat.Items.Count)
                {
                    cboImageFormat.SelectedIndex = settings.ImageFormat.Value;
                }

                if (string.Equals(settings.Theme, "Dark", StringComparison.OrdinalIgnoreCase))
                {
                    VisionImageTool.Theme.Set(AppTheme.Dark);
                    UpdateThemeToggle();
                }

                if (settings.JpegQuality.HasValue)
                {
                    numJpegQuality.Value = Math.Min(Math.Max(settings.JpegQuality.Value, (int)numJpegQuality.Minimum), (int)numJpegQuality.Maximum);
                }

                if (settings.WatchFolder.HasValue)
                {
                    chkWatchFolder.Checked = settings.WatchFolder.Value;
                }

                RestoreWindowLayout(settings.Window);
            }
            catch (Exception ex)
            {
                Log("Không đọc được cấu hình: " + ex.Message);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new PushSettings
                {
                    Folder = txtPath.Text,
                    PartType = cboPartType.Text,
                    PartID = txtPartID.Text,
                    CaptureConfig = cboCaptureConfig.Text,
                    CameraId = cboCameraID.Text,
                    SequenceIo = cboSequenceIO.Text,
                    Edge = _edge,
                    DelayMs = (int)numDelay.Value,
                    TriggerRepeat = (int)numTriggerRepeat.Value,
                    TimeoutMs = _timeoutMs,
                    SaveCallback = chkSaveCallback.Checked,
                    SaveRaw = chkSaveRaw.Checked,
                    ImageName = txtImageName.Text,
                    NamePrefix = cboNamePrefix.SelectedIndex,
                    ImageFormat = cboImageFormat.SelectedIndex,
                    JpegQuality = (int)numJpegQuality.Value,
                    Theme = VisionImageTool.Theme.Current.ToString(),
                    WatchFolder = chkWatchFolder.Checked,
                    Window = CaptureWindowLayout()
                };

                Directory.CreateDirectory(AppDataDir);
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(settings, Formatting.Indented), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log("Không lưu được cấu hình: " + ex.Message);
            }
        }

        #endregion

        #region Tiện ích

        private static double ElapsedMs(long startTimestamp)
        {
            return (Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                return (bytes / 1024d / 1024d).ToString("0.0") + " MB";
            }

            if (bytes >= 1024)
            {
                return (bytes / 1024d).ToString("0") + " KB";
            }

            return bytes + " B";
        }

        #endregion

        private void lblPathHeader_Click(object sender, EventArgs e)
        {

        }

        private void tlpPathRow2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
