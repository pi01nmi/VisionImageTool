using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionImageTool
{

    /// <summary>
    /// Tab "Dataset Manager": đọc ảnh trong một thư mục, bóc tách tên file theo schema do người dùng
    /// định nghĩa, cho gán nhãn O / Δ / X và xuất Excel (mỗi mẫu Raw + Mask thành một hàng, có link).
    ///
    /// Bố cục ba khu vực:
    ///   A. Cấu hình  : thư mục ảnh + schema tên file + dòng giải thích schema.
    ///   B. Bảng      : DataGridView (trái) + xem trước ảnh đang chọn (phải).
    ///   C. Xuất      : tóm tắt số liệu + nút "Xuất Excel".
    ///
    /// Bố cục nằm trong ucDatasetManager.Designer.cs (mở bằng Designer, chỉnh trong cửa sổ Properties);
    /// các file partial khác chỉ gắn sự kiện / dữ liệu.
    ///
    /// Nhãn đánh giá lưu vào file dataset_evaluation.json NGAY TRONG thư mục ảnh, để mang thư mục đi
    /// máy khác vẫn giữ nguyên nhãn đã gán.
    /// </summary>
    public partial class ucDatasetManager : UserControl
    {
        /// <summary>Nhãn chỉnh tay + toàn bộ dữ liệu bảng của thư mục (xem ucDatasetManager.Collection.cs).</summary>
        private const string EvaluationFileName = "dataset_collection.json";

        /// <summary>File nhãn của các bản trước - đọc một lần rồi chuyển sang dataset_collection.json.</summary>
        private const string LegacyEvaluationFileName = "dataset_evaluation.json";

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };
        private static readonly string[] EvaluationChoices = { "", "O", "Δ", "X" };

        private static readonly Color Primary = Color.FromArgb(37, 99, 235);
        private static readonly Color UnmatchedColor = Color.FromArgb(255, 249, 219);

        /// <summary>Ảnh Raw chưa có Mask (chưa ghi nhận kết quả) - cùng màu xám với tab Capture & Monitor.</summary>
        private static readonly Color NoResultColor = Color.FromArgb(229, 231, 235);
        private static readonly Color EvalOColor = Color.FromArgb(220, 252, 231);
        private static readonly Color EvalDeltaColor = Color.FromArgb(254, 243, 199);
        private static readonly Color EvalXColor = Color.FromArgb(254, 226, 226);

        // Control trên giao diện khai báo trong ucDatasetManager.Designer.cs. Các nút chèn trường dưới đây không
        // nằm trên giao diện - chỉ giữ token / trạng thái cho menu "+ Thành phần".
        private readonly Button btnInsertDk = new Button();
        private readonly Button btnInsertType = new Button();
        private readonly Button btnInsertEval = new Button();
        private readonly Button btnInsertSuffix = new Button();
        private readonly Button btnInsertExt = new Button();
        private readonly Button btnInsertShot = new Button();

        /// <summary>Các lựa chọn của bộ lọc UnitX / Đánh giá. "(trống)" = chưa có nhãn.</summary>
        private static readonly string[] LabelFilterChoices = { "Tất cả", "O", "Δ", "X", "(trống)" };

        /// <summary>Số giá trị ĐK hợp lý tối đa - vượt quá thì cảnh báo schema có thể đặt nhầm trường &lt;ĐK&gt;.</summary>
        private const int MaxReasonableConditions = 20;

        /// <summary>
        /// Quá số ĐK này thì KHÔNG dựng các khối cột ĐK / hàng của bảng (mỗi ĐK 5 cột: 162 ĐK = 817 cột, đổ bảng mất
        /// ~17 giây mỗi lần lọc -> phần mềm như bị treo). Gần như luôn do schema đặt nhầm &lt;ĐK&gt;.
        /// </summary>
        private const int MaxTableConditions = 40;

        private bool TableBlocked
        {
            get { return _conditions.Count > MaxTableConditions && !chkUnmatchedOnly.Checked; }
        }

        private readonly List<DatasetImage> _images = new List<DatasetImage>();
        private Dictionary<string, string> _evaluations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private DatasetSchema _schema;
        private string _loadedFolder;
        private string _settingsPath;
        private Func<string> _defaultFolder;
        private bool _loading;
        private bool _fillingGrid;
        private readonly Timer _filterTimer = new Timer { Interval = 250 };

        /// <summary>Một ảnh trong thư mục dataset.</summary>
        private sealed class DatasetImage
        {
            public string FullPath;
            public string FileName;
            public string RelativeDir;
            public bool Matched;
            public Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public string Suffix;
            public string Type;
            public string FileEvaluation;

            /// <summary>Giá trị trường &lt;Lần chụp&gt; ("" nếu schema không có / tên không có).</summary>
            public string Shot = string.Empty;

            /// <summary>Ảnh Mask cùng cặp của ảnh Raw (null nếu thiếu) - xem BuildRows.</summary>
            public DatasetImage PairMask;

            /// <summary>Ảnh Raw khớp schema nhưng chưa có ảnh Mask: chưa ghi nhận kết quả (nền xám).</summary>
            public bool NoResult;

            /// <summary>
            /// Schema bỏ qua phần đuôi (DatasetSchema.IgnoresTail) và đã có ảnh cùng mẫu / ĐK / loại đứng trước theo tên file:
            /// ảnh này không đưa vào bảng / Excel (chỉ lấy ảnh đầu tiên theo tên).
            /// </summary>
            public bool Skipped;

            /// <summary>Khoá ghép Raw ↔ Mask cùng mẫu (các trường Key + DK + hậu tố).</summary>
            public string PairKey;

            /// <summary>Giá trị DK ("" nếu schema không có trường DK).</summary>
            public string Condition;

            /// <summary>Khoá hàng của bảng / Excel: các trường Key + hậu tố, KHÔNG gồm DK (mỗi DK một block).</summary>
            public string RowKey;

            /// <summary>
            /// Khoá lưu nhãn đánh giá: đường dẫn tương đối của CHÍNH ảnh này. Mỗi ảnh một nhãn riêng -
            /// gán nhãn cho ảnh Raw không được làm đổi nhãn ảnh Mask cùng mẫu (lỗi 2026-09-25).
            /// </summary>
            public string EvalKey;
        }

        /// <summary>Raw + Mask của một mẫu ở MỘT DK - một block cột Raw | Mask | UnitX | Evaluation.</summary>
        private sealed class DatasetBlock
        {
            public string Condition;
            public DatasetImage Raw;
            public DatasetImage Mask;

            /// <summary>Ảnh mang kết quả của block: Mask (ảnh UnitX trả về), thiếu thì Raw.</summary>
            public DatasetImage Main
            {
                get { return Mask ?? Raw; }
            }

            /// <summary>
            /// Schema có &lt;Lần chụp&gt;: mọi lần chụp của mẫu ở DK này (đã sắp theo lần chụp), null nếu không.
            /// Khi đó Raw / Mask ở trên là lần chụp ĐẠI DIỆN (Vision xấu nhất) - ảnh hiện trong bảng / Excel.
            /// </summary>
            public List<DatasetShot> Shots;

            /// <summary>Lần chụp đang xem trong khung xem trước (phím , và .).</summary>
            public int ShotView;

            public IEnumerable<DatasetImage> Images
            {
                get
                {
                    if (Shots != null)
                    {
                        var seen = new HashSet<DatasetImage>();
                        foreach (var shot in Shots)
                        {
                            if (shot.Raw != null && seen.Add(shot.Raw)) yield return shot.Raw;
                            if (shot.Mask != null && seen.Add(shot.Mask)) yield return shot.Mask;
                        }

                        yield break;
                    }

                    if (Raw != null) yield return Raw;
                    if (Mask != null) yield return Mask;
                }
            }
        }

        /// <summary>Một lần chụp (Raw + Mask cùng giá trị &lt;Lần chụp&gt;) trong block.</summary>
        private sealed class DatasetShot
        {
            public string Index;
            public DatasetImage Raw;
            public DatasetImage Mask;

            public DatasetImage Main
            {
                get { return Mask ?? Raw; }
            }
        }

        /// <summary>
        /// Một hàng của bảng / Excel: các ảnh cùng trường Key + hậu tố, chỉ khác DK. Mỗi lần chụp (hậu tố
        /// khác nhau) là một hàng riêng. Ở chế độ "Chỉ ảnh không khớp schema" mỗi hàng là một ảnh
        /// (<see cref="Unmatched"/> != null).
        /// </summary>
        private sealed class DatasetRow
        {
            public string Key;
            public Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public string Suffix;
            public readonly Dictionary<string, DatasetBlock> Blocks = new Dictionary<string, DatasetBlock>(StringComparer.OrdinalIgnoreCase);
            public DatasetImage Unmatched;

            public DatasetBlock Block(string condition)
            {
                DatasetBlock block;
                return Blocks.TryGetValue(condition ?? string.Empty, out block) ? block : null;
            }

            public IEnumerable<DatasetImage> Images
            {
                get { return Unmatched != null ? new[] { Unmatched } : Blocks.Values.SelectMany(b => b.Images); }
            }
        }

        private enum BlockPart { Raw, Mask, UnitX, Evaluation, Compare }

        /// <summary>Gắn vào Tag của cột thuộc một block DK.</summary>
        private sealed class BlockColumn
        {
            public string Condition;
            public int GroupIndex;
            public BlockPart Part;
        }

        private readonly List<DatasetRow> _rows = new List<DatasetRow>();

        /// <summary>Các giá trị DK (thứ tự tự nhiên) - mỗi giá trị một block cột. Không có trường DK: [""].</summary>
        private readonly List<string> _conditions = new List<string>();

        private sealed class DatasetSettings
        {
            public string Folder { get; set; }
            public string Schema { get; set; }
            public bool? IncludeSubfolders { get; set; }
            public DatasetExportOptions Export { get; set; }

            // Khu vực đang hiện (null = file cũ -> hiện tất cả).
            public bool? ShowConfig { get; set; }
            public bool? ShowTree { get; set; }
            public bool? ShowStats { get; set; }

            /// <summary>Cách hiểu Evaluation khi đối soát với UnitX (null = file cũ -> Correctness).</summary>
            public string CompareMode { get; set; }

            /// <summary>Khung danh sách vấn đề đang hiện.</summary>
            public bool? ShowIssues { get; set; }

            /// <summary>Khối trên cùng đang thu gọn: 0 Nạp dataset · 1 Schema · 2 Bộ lọc & trạng thái (null = mở hết).</summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<int> CollapsedSections { get; set; }

            /// <summary>Dữ liệu đang khoá (chỉ xem).</summary>
            public bool? Locked { get; set; }

            /// <summary>"Name" (theo tên file) / "Folder" (theo thư mục). Mỗi chế độ giữ schema riêng.</summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string SchemaMode { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string FolderSchema { get; set; }

            /// <summary>Schema dùng gần đây (mới nhất trước) - thay cho việc cất riêng schema của từng chế độ.</summary>
            public List<string> RecentSchemas { get; set; }

            /// <summary>Vạch chia + độ rộng cột người dùng đã kéo.</summary>
            public DatasetLayout Layout { get; set; }

            /// <summary>Thứ tự trường khi xuất Excel / JSON (kéo thả ở hộp thoại Xuất); null = theo schema.</summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<string> ExportOrder { get; set; }

            /// <summary>Cột thông tin bỏ khi xuất (✕ ở hộp thoại Xuất); null = xuất đủ.</summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<string> ExportHidden { get; set; }
        }

        /// <summary>Tuỳ chọn xuất Excel lần trước (hyperlink / ảnh, kích thước ô).</summary>
        private DatasetExportOptions _exportOptions = new DatasetExportOptions();

        public ucDatasetManager()
        {
            InitializeComponent();
            SetupLayout();
            SetupLayoutPersistence();
            SetupFileMenu();
            ApplySectionVisibility();
            SetupCommentUi();
            Theme.Changed += OnThemeChanged;
            FileHistory.Applied += OnFileHistoryApplied;
            SetupColumnFilters();
            SetupAutoRefresh();
            _filterTimer.Tick += FilterTimer_Tick;
            _schema = DatasetSchema.Parse(DatasetSchema.DefaultText, out _);
            txtSchema.Text = DatasetSchema.DefaultText;
            RebuildColumns();
            UpdateSchemaInfo(null);
            UpdateSummary();
        }

        /// <summary>
        /// Gọi một lần khi form chính mở: nơi lưu cấu hình của tab + nơi lấy thư mục mặc định (ô Path
        /// của tab Capture) khi người dùng chưa chọn thư mục riêng.
        /// </summary>
        public void Initialize(string settingsPath, Func<string> defaultFolder)
        {
            _settingsPath = settingsPath;
            _defaultFolder = defaultFolder;

            var settings = LoadSettings();
            if (settings != null)
            {
                // Một ô schema duy nhất (chế độ tự nhận theo dấu \). File cấu hình cũ có hai schema theo
                // nút gạt: schema của chế độ đang lưu là schema hiện hành, schema kia vào danh sách gần đây.
                bool oldFolder = string.Equals(settings.SchemaMode, "Folder", StringComparison.OrdinalIgnoreCase);
                string current = oldFolder ? settings.FolderSchema : settings.Schema;
                string other = oldFolder ? settings.Schema : settings.FolderSchema;
                if (settings.RecentSchemas != null)
                {
                    _recentSchemas.AddRange(settings.RecentSchemas.Where(s => !string.IsNullOrWhiteSpace(s)).Select(NormalizeSchemaText));
                }

                if (!string.IsNullOrWhiteSpace(other))
                {
                    RememberSchema(NormalizeSchemaText(other));
                }

                txtSchema.Text = NormalizeSchemaText(string.IsNullOrWhiteSpace(current) ? DatasetSchema.DefaultText : current);
                ApplySchema(false);

                if (!string.IsNullOrWhiteSpace(settings.Folder))
                {
                    txtFolder.Text = settings.Folder;
                }

                // Schema có cấp thư mục thì luôn phải quét thư mục con.
                chkSubfolders.Checked = _folderMode || (settings.IncludeSubfolders ?? true);
                chkSubfolders.Enabled = !_folderMode;

                if (settings.Export != null)
                {
                    _exportOptions = settings.Export;
                }

                _restoringSections = true;
                chkShowConfig.Checked = settings.ShowConfig ?? true;
                chkShowTree.Checked = settings.ShowTree ?? true;
                chkShowStats.Checked = settings.ShowStats ?? true;
                chkShowIssues.Checked = settings.ShowIssues ?? true;
                for (int i = 0; i < SectionCount; i++)
                {
                    _sectionCollapsed[i] = settings.CollapsedSections != null && settings.CollapsedSections.Contains(i);
                }

                ApplySectionVisibility();
                SetLocked(settings.Locked ?? false, false);
                _restoringSections = false;

                CompareMode mode = ParseCompareMode(settings.CompareMode);
                SetCompareMode(mode, false);

                LoadLayout(settings.Layout);
                _exportOrder = settings.ExportOrder;
                _exportHidden = settings.ExportHidden;
            }
        }

        /// <summary>Tab vừa được mở: lần đầu thì tự tải thư mục (lấy Path của tab Capture nếu trống).</summary>
        public void OnShown()
        {
            if (string.IsNullOrWhiteSpace(txtFolder.Text) && _defaultFolder != null)
            {
                txtFolder.Text = (_defaultFolder() ?? string.Empty).Trim().Trim('"');
            }

            if (_loadedFolder == null && !_loading && Directory.Exists(txtFolder.Text.Trim().Trim('"')))
            {
                LoadFolder();
            }

            // Vạch chia chỉ đặt lại được khi control đã có kích thước thật (sau khi form hiện).
            if (!_layoutRestored)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke((Action)RestoreSplitLayout);
                }
                else
                {
                    HandleCreated += (s, e) => BeginInvoke((Action)RestoreSplitLayout);
                }
            }
        }

        #region Bố cục

        /// <summary>
        /// Bố cục nằm trong ucDatasetManager.Designer.cs. Ở đây chỉ nối các tên dùng trong code (_root, _issuesSplit...)
        /// với control của Designer và gắn sự kiện - không đặt lại kích thước / màu / chữ đã chỉnh trong Designer.
        /// </summary>
        private void SetupLayout()
        {
            _root = tlpRoot;
            _schemaCard = pnlSchemaCard;
            _filterCardTable = tlpFilterCard;
            _issuesSplit = scIssues;
            _previewSplit = scPreview;
            _treeSplit = scTree;
            _bottomSplit = scBottom;
            CaptureDesignerHeights();
            WireSectionCollapse();

            WireSchemaCard();
            WireFilterCard();
            WireGridArea();
            SetupStatusBar();
            BuildJsonButton();   // nút JSON nằm trong menu Xuất; giữ control cho tooltip / trạng thái cũ
        }

        private static void StyleButton(Button button, string text, bool primary)
        {
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.Margin = new Padding(4, 2, 0, 2);
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;

            if (primary)
            {
                button.BackColor = Primary;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderSize = 0;
            }
            else
            {
                button.BackColor = Color.White;
                button.ForeColor = Color.FromArgb(31, 41, 55);
                button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            }
        }

        private void StyleInsertButton(Button button, string text, string token, Color color)
        {
            StyleButton(button, text, false);
            button.Margin = new Padding(2, 0, 2, 0);
            button.Dock = DockStyle.None;
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.Padding = new Padding(4, 0, 4, 0);
            button.MinimumSize = new Size(0, 30);
            button.ForeColor = color;
            button.FlatAppearance.BorderColor = color;
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.Tag = token;
            button.Click += (s, e) => InsertSchemaToken(token);

            // Trường đã có trong schema: nút bị khoá + gạch chéo -> nhìn là biết đã thêm, không thêm được nữa.
            button.Paint += (s, e) =>
            {
                if (button.Enabled)
                {
                    return;
                }

                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var veil = new SolidBrush(Color.FromArgb(150, Theme.Back(Color.White))))
                using (var pen = new Pen(Color.FromArgb(170, color), 1.6F))
                {
                    g.FillRectangle(veil, 1, 1, button.Width - 2, button.Height - 2);
                    g.DrawLine(pen, 3, button.Height - 4, button.Width - 4, 3);
                }
            };
            button.EnabledChanged += (s, e) => button.Invalidate();
        }

        /// <summary>Vị trí con trỏ người dùng đặt trong ô schema (-1 = chưa đặt / ô vừa bị đổi bằng code).</summary>
        private int _schemaCaret = -1;

        private static readonly System.Text.RegularExpressions.Regex ExtensionTokenAtEnd = new System.Text.RegularExpressions.Regex(
            @"\.?<\s*(?:extension|ext|đuôi|duoi)\s*>\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        /// <summary>Ghi nhớ con trỏ trong ô schema - bấm nút "+" làm ô mất focus nhưng vẫn chèn đúng chỗ đó.</summary>
        private void TrackSchemaCaret()
        {
            txtSchema.MouseUp += (s, e) => _schemaCaret = txtSchema.SelectionStart;
            txtSchema.KeyUp += (s, e) => _schemaCaret = txtSchema.SelectionStart;
            txtSchema.Leave += (s, e) => _schemaCaret = txtSchema.SelectionStart;
            txtSchema.TextChanged += (s, e) =>
            {
                if (!txtSchema.Focused)
                {
                    _schemaCaret = -1;   // đổi bằng code (nạp cấu hình, đổi chế độ...) - con trỏ cũ không còn nghĩa
                }
            };
        }

        /// <summary>
        /// Chèn một trường cố định TẠI CON TRỎ trong ô schema (chưa đặt con trỏ thì chèn cuối, DK thì sau trường
        /// đầu tiên). Con trỏ đang nằm giữa một trường &lt;...&gt; thì chèn sau trường đó. Dấu nối lấy theo chỗ
        /// chèn: đang ở tên file thì "_", giữa hai cấp thư mục thì "\". Không bao giờ chèn sau &lt;Extension&gt;
        /// (đuôi ảnh luôn đứng cuối, nối bằng dấu chấm).
        /// </summary>
        private void InsertSchemaToken(string token)
        {
            string text = txtSchema.Text ?? string.Empty;

            // Đuôi ảnh luôn đứng CUỐI, nối bằng dấu chấm: ..._<Hậu tố>.<Extension>
            if (token == "<Extension>")
            {
                string trimmed = NormalizeSchemaText(text).TrimEnd();
                txtSchema.Text = trimmed + ".<Extension>";
                txtSchema.SelectionStart = txtSchema.Text.Length;
                _schemaCaret = txtSchema.Text.Length;
                txtSchema.Focus();
                return;
            }

            // Phần có thể chèn: trước ".<Extension>" (hoặc ".jpg" cũ) ở cuối.
            int limit = text.Length;
            var ext = ExtensionTokenAtEnd.Match(text);
            if (ext.Success)
            {
                limit = ext.Index;
            }
            else
            {
                int dot = text.LastIndexOf('.');
                if (dot > text.LastIndexOf('>'))
                {
                    limit = dot;
                }
            }

            int pos;
            if (_schemaCaret >= 0 && _schemaCaret <= text.Length)
            {
                pos = Math.Min(_schemaCaret, limit);

                // Con trỏ giữa một trường <...> -> chèn sau trường đó.
                int open = text.LastIndexOf('<', Math.Max(0, pos - 1));
                int close = pos > 0 ? text.LastIndexOf('>', pos - 1) : -1;
                if (pos > 0 && open > close)
                {
                    int end = text.IndexOf('>', open);
                    pos = end >= 0 ? Math.Min(end + 1, limit) : limit;
                }
            }
            else if (token == "<DK>" && text.IndexOf('>') >= 0 && text.IndexOf('>') < limit)
            {
                pos = text.IndexOf('>') + 1;   // DK thường đứng ngay sau trường đầu tiên (<Model>_<DK>_...)
            }
            else
            {
                pos = limit;
            }

            string before = text.Substring(0, pos);
            string after = text.Substring(pos);
            string separator = token == "<Lần chụp>" ? "_" : SeparatorAt(text.Substring(0, limit), pos);

            string insert = token;
            if (before.Length > 0 && before.EndsWith(">"))
            {
                insert = separator + insert;
            }

            if (after.Length > 0 && after[0] == '<')
            {
                insert += separator;
            }

            txtSchema.Text = before + insert + after;
            txtSchema.SelectionStart = before.Length + insert.Length;
            _schemaCaret = txtSchema.SelectionStart;
            txtSchema.Focus();
        }

        /// <summary>
        /// Dấu nối cho trường chèn ở vị trí pos (text đã bỏ phần .&lt;Extension&gt;): dấu nối đang có ngay tại chỗ
        /// chèn; chèn sát một trường thì lấy dấu nối gần nhất bên trái (rồi bên phải); không có thì theo chế độ.
        /// </summary>
        private string SeparatorAt(string text, int pos)
        {
            pos = Math.Min(pos, text.Length);
            int prevClose = pos > 0 ? text.LastIndexOf('>', pos - 1) : -1;
            int nextOpen = text.IndexOf('<', pos);

            // Literal đang chứa vị trí chèn (giữa hai trường).
            if (prevClose >= 0 && nextOpen > prevClose + 1)
            {
                return Pick(text.Substring(prevClose + 1, nextOpen - prevClose - 1));
            }

            // Sát trường: literal gần nhất bên trái, rồi bên phải.
            foreach (var m in System.Text.RegularExpressions.Regex.Matches(text.Substring(0, Math.Max(0, prevClose + 1)), ">([^<>]+)<")
                .Cast<System.Text.RegularExpressions.Match>().Reverse())
            {
                return Pick(m.Groups[1].Value);
            }

            var right = System.Text.RegularExpressions.Regex.Match(text.Substring(pos), ">([^<>]+)<");
            if (right.Success)
            {
                return Pick(right.Groups[1].Value);
            }

            return _folderMode ? "\\" : "_";
        }

        private static string Pick(string literal)
        {
            if (literal.Contains("\\")) return "\\";
            if (literal.Contains("_")) return "_";
            return literal.Length > 0 && literal != "." ? literal : "_";
        }

        /// <summary>Vẽ lại các thẻ schema theo nội dung ô schema (kể cả khi chưa bấm Áp dụng).</summary>
        private void UpdateSchemaChips()
        {
            string error;
            var schema = DatasetSchema.Parse(txtSchema.Text, out error);
            pnlSchemaChips.SetSchema(schema, error);
            UpdateReadModeLabel(schema);

            // Trường cố định đã có trong schema thì không cần chèn nữa.
            var roles = schema == null ? new DatasetFieldRole[0] : schema.Fields.Select(f => f.Role).ToArray();
            btnInsertDk.Enabled = !roles.Contains(DatasetFieldRole.Condition);
            btnInsertType.Enabled = !roles.Contains(DatasetFieldRole.Type);
            btnInsertEval.Enabled = !roles.Contains(DatasetFieldRole.Evaluation);
            // Hậu tố và Lần chụp loại trừ nhau (Tên file tự đọc của chế độ thư mục không tính).
            bool explicitSuffix = schema != null && schema.Fields.Any(f => f.Role == DatasetFieldRole.Suffix && !f.Implicit);
            btnInsertSuffix.Enabled = !explicitSuffix && !roles.Contains(DatasetFieldRole.Shot);
            btnInsertShot.Enabled = !explicitSuffix && !roles.Contains(DatasetFieldRole.Shot);
            btnInsertExt.Enabled = schema == null || !schema.HasExtension;

            bool pending = schema != null && _schema != null && schema.Text != _schema.Text;
            btnApplySchema.Text = pending ? "Áp dụng Schema •" : "Áp dụng Schema";
        }

        #region Thanh trạng thái

        private static readonly Color StatusText = Color.FromArgb(75, 85, 99);
        private static readonly Color StatusError = Color.FromArgb(220, 38, 38);

        private const string ShortcutText = "⌨ Phím tắt";
        private const string ShortcutShortText = "⌨ Phím tắt";

        /// <summary>Không nằm trên thanh (phím tắt đã lên thanh công cụ trên bảng) - giữ cho FitShortcutText.</summary>
        private readonly ToolStripStatusLabel stKeys = new ToolStripStatusLabel { Text = ShortcutText, Margin = new Padding(0, 3, 6, 2) };

        /// <summary>Các ô thuộc phần thống kê (ẩn khi thanh đang hiện một thông báo).</summary>
        private ToolStripItem[] _summaryItems;
        private readonly Font _statusBold = new Font("Segoe UI", 8.25F, FontStyle.Bold);
        private readonly Font _issueKindBold = new Font("Segoe UI", 9F, FontStyle.Bold);

        /// <summary>
        /// Thanh trạng thái (statusBar trong Designer): [trái] số ảnh + lỗi schema | [giữa] Đánh giá O / Δ / X.
        /// Hai ô Spring trống (stSpringLeft / stSpringRight) đẩy vùng giữa ra giữa. Màu / chữ / tooltip chỉnh trong
        /// Designer; ở đây chỉ gắn renderer phẳng + sự kiện.
        /// </summary>
        private void SetupStatusBar()
        {
            statusBar.Renderer = new ToolStripProfessionalRenderer(new FlatStatusColors());
            SetupIssueStatus();

            // Các ô của phần số liệu: mọi ô sau stMessage (theo thứ tự trên thanh trong Designer).
            _summaryItems = statusBar.Items.Cast<ToolStripItem>().SkipWhile(i => i != stMessage).Skip(1).ToArray();

            // Cửa sổ hẹp: rút gọn phím tắt thành biểu tượng (di chuột vào để xem tooltip).
            statusBar.SizeChanged += (s, e) => FitShortcutText();

            ShowStatusMessage(stMessage.Text);
        }

        /// <summary>Thanh trạng thái chỉ hiện một thông báo (đang tải, đang xuất, lỗi...).</summary>
        private void ShowStatusMessage(string text, bool error = false)
        {
            statusBar.SuspendLayout();
            foreach (var item in _summaryItems)
            {
                item.Visible = false;
            }

            stMessage.Text = text;
            stMessage.ForeColor = error ? StatusError : StatusText;
            stMessage.Spring = true;
            stMessage.TextAlign = ContentAlignment.MiddleLeft;
            stMessage.Visible = true;
            statusBar.ResumeLayout();
            FitShortcutText();
        }

        /// <summary>
        /// Tiến độ xuất Excel: thanh % + "Đang tạo thumbnail 1234 / 3042 (40%) - còn khoảng 35 giây".
        /// <paramref name="total"/> = 0: bước không đo được tiến độ (ghi file) - thanh chạy liên tục.
        /// </summary>
        private void ShowExportProgress(string phase, int done, int total, System.Diagnostics.Stopwatch watch)
        {
            stProgress.Visible = true;

            if (total <= 0)
            {
                stProgress.Style = ProgressBarStyle.Marquee;
                ShowStatusMessage(phase + "...");
                return;
            }

            int percent = (int)(done * 100L / total);
            stProgress.Style = ProgressBarStyle.Continuous;
            stProgress.Maximum = 100;
            stProgress.Value = Math.Max(0, Math.Min(100, percent));

            string remaining = string.Empty;
            if (done > 0 && done < total && watch.Elapsed.TotalSeconds >= 2)
            {
                double seconds = watch.Elapsed.TotalSeconds / done * (total - done);
                remaining = seconds >= 90 ? $"  -  còn khoảng {Math.Ceiling(seconds / 60)} phút" : $"  -  còn khoảng {Math.Ceiling(seconds)} giây";
            }

            ShowStatusMessage($"{phase}: {done} / {total}  ({percent}%){remaining}");
        }

        private void ShowStatusSummary()
        {
            statusBar.SuspendLayout();
            stProgress.Visible = false;
            stMessage.Visible = false;
            foreach (var item in _summaryItems)
            {
                item.Visible = true;
            }

            statusBar.ResumeLayout();
            UpdateSelectionStatus();
            FitShortcutText();
        }

        private void FitShortcutText()
        {
            int needed = 0;
            foreach (ToolStripItem item in statusBar.Items)
            {
                var label = item as ToolStripStatusLabel;
                if (item.Visible && (label == null || !label.Spring) && item != stKeys)
                {
                    needed += item.GetPreferredSize(Size.Empty).Width + item.Margin.Horizontal;
                }
            }

            string full = ShortcutText;
            int fullWidth = TextRenderer.MeasureText(full, stKeys.Font).Width + stKeys.Margin.Horizontal + 8;
            string text = needed + fullWidth <= statusBar.Width ? full : ShortcutShortText;
            if (stKeys.Text != text)
            {
                stKeys.Text = text;
            }
        }

        /// <summary>Nền trắng phẳng cho StatusStrip (bỏ gradient / viền mặc định).</summary>
        private sealed class FlatStatusColors : ProfessionalColorTable
        {
            public override Color StatusStripGradientBegin { get { return Color.White; } }
            public override Color StatusStripGradientEnd { get { return Color.White; } }
        }

        #endregion

        private void SetupGrid()
        {
            // Thuộc tính bảng (chọn theo Ô, EditOnEnter, màu tiêu đề...) chỉnh trong Designer. Chọn theo Ô: một hàng có
            // nhiều block DK, ô đang chọn cho biết block / ảnh nào (Raw hay Mask). Cột dựng bằng code (RebuildColumns).
            grid.AutoGenerateColumns = false;

            // Đổi nhãn trong ComboBox thì ghi nhận ngay, không đợi rời ô.
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.CellValueChanged += Grid_CellValueChanged;
            // Khoá dữ liệu: không cho mở ô sửa (ComboBox Evaluation / Đối soát).
            grid.CellBeginEdit += (s, e) => { if (_locked) e.Cancel = true; };
            grid.DataError += (s, e) => { e.ThrowException = false; };
            grid.CurrentCellChanged += (s, e) => ShowPreview();
            grid.SelectionChanged += (s, e) => OnGridSelectionChanged();
            grid.CellFormatting += Grid_CellFormatting;
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && !IsEvalColumn(grid.Columns[e.ColumnIndex]))
                {
                    OpenFile(CellImage(e.RowIndex, e.ColumnIndex));
                }
            };

        }

        #endregion

        #region Schema

        private void ApplySchema(bool reportErrors)
        {
            // Tên trường cũ <Eval> / <Evaluation> đổi thành <Đánh giá> (cùng vai trò - chỉ đổi tên hiển thị).
            string normalized = NormalizeSchemaText(txtSchema.Text);
            if (normalized != txtSchema.Text)
            {
                txtSchema.Text = normalized;
            }

            string error;
            var schema = DatasetSchema.Parse(txtSchema.Text, out error);

            if (schema == null)
            {
                UpdateSchemaInfo(error);
                if (reportErrors)
                {
                    MessageBox.Show(this, error, "Schema không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            // Schema có dấu \ = kết hợp thư mục + tên file -> tự chuyển chế độ (không cần bấm nút gạt).
            bool reload = SyncSchemaMode(txtSchema.Text);

            _schema = schema;
            RememberSchema(txtSchema.Text);
            SaveSettings();
            if (reload)
            {
                LoadFolder();   // vừa bật quét thư mục con
                return;
            }

            RebuildColumns();

            foreach (var image in _images)
            {
                ParseImage(image);
            }

            FillGrid();
            UpdateSchemaInfo(null);
        }

        private void UpdateSchemaInfo(string error)
        {
            if (error != null)
            {
                lblSchemaInfo.ForeColor = Color.FromArgb(185, 28, 28);
                lblSchemaInfo.Text = "Lỗi schema: " + error;
                return;
            }

            // Không lỗi: nhãn chỉ còn cảnh báo ghép hàng (thư mục nhiều SPL mà tên file không có mã SPL).
            string warning = PairingWarning();
            lblSchemaInfo.ForeColor = Color.FromArgb(180, 83, 9);
            lblSchemaInfo.Text = warning ?? string.Empty;

            // Các trường đã hiện thành thẻ màu ở trên - khung ⓘ chỉ còn cách gom nhóm + ví dụ thật.
            // Theo thư mục: ví dụ là đường dẫn tương đối (thư mục con + tên file), không chỉ tên file.
            var sampleImage = _images.FirstOrDefault(i => i.Matched);
            string sampleText = string.Empty;
            if (sampleImage != null)
            {
                string sample = _folderMode
                    ? (sampleImage.RelativeDir == "." ? sampleImage.FileName : Path.Combine(sampleImage.RelativeDir, sampleImage.FileName))
                    : sampleImage.FileName;
                var r = _folderMode ? _schema.MatchPath(sample) : _schema.Match(sample);
                sampleText = "\nVí dụ\t" + sample + "\nTách được\t"
                    + string.Join(", ", r.Values.Select(kv => kv.Key + "=" + kv.Value))
                    + (r.Type != null ? ", Loại=" + r.Type : string.Empty)
                    + (string.IsNullOrEmpty(r.Evaluation) ? string.Empty : ", Đánh giá=" + r.Evaluation)
                    + (string.IsNullOrEmpty(r.Suffix) ? string.Empty : ", " + (_schema.Fields.Any(f => f.Implicit) ? "Tên mẫu" : "Hậu tố") + "=" + r.Suffix)
                    + (string.IsNullOrEmpty(r.Shot) ? string.Empty : ", Lần chụp=" + r.Shot);
            }

            var condition = _schema.ConditionField;
            string rows = "Mỗi hàng\t"
                + string.Join(" + ", _schema.KeyFields.Select(f => f.Name))
                + (_schema.Fields.Any(f => f.Implicit) ? " + tên mẫu" : _schema.HasSuffix ? " + hậu tố" : string.Empty)
                + (_schema.HasShot
                    ? "\nLần chụp\tcác ảnh chỉ khác <Lần chụp> GỘP VÀO MỘT HÀNG (sắp theo số / timestamp)"
                      + "\n\tVision = phép nhân logic: NG × NG × OK = NG  (X > Δ > O)  ·  một Đánh giá chung"
                      + "\n\tRaw ↔ Mask ghép theo cùng lần chụp  ·  phím  ,  .  xem lần chụp trước / sau"
                    : string.Empty)
                + "\nCột\t"
                + (condition != null
                    ? "Vision | Đánh giá | Đối soát theo từng " + condition.Name + ", rồi Raw | Mask theo từng " + condition.Name
                    : "Vision | Đánh giá | Đối soát | Raw | Mask (schema chưa có trường DK)");

            if (_folderMode)
            {
                var levels = _schema.Fields.Where(f => f.InFolder).Select(f => "<" + f.Name + ">").ToList();
                var implicitField = _schema.Fields.FirstOrDefault(f => f.Implicit);
                var fileFields = _schema.Fields.Where(f => !f.InFolder && !f.Implicit).ToList();
                string fileText = string.Concat(fileFields.Select((f, i) => (i == 0 ? string.Empty : f.LiteralBefore) + "<" + f.Name + ">"));
                _schemaHint = ChipLegendHint + "Chế độ\tThư mục + tên file - đọc đường dẫn tính từ Thư mục ảnh: trước dấu \\ cuối cùng là"
                    + "\n\tcác CẤP THƯ MỤC, sau đó là TÊN FILE (các trường nối bằng _ như chế độ theo tên)"
                    + "\nCấp thư mục\t" + (levels.Count > 0 ? string.Join("  ›  ", levels) : "(không có)")
                    + "\nTên file\t" + (implicitField != null
                        ? "tự đọc: bỏ _raw / _mask / _OK / _NG ở cuối → tên mẫu (ghép Raw ↔ Mask)"
                        : fileText)
                    + (_schema.Fields.Any(f => f.Role == DatasetFieldRole.Evaluation)
                        ? string.Empty
                        : "\nVision\tlấy từ đuôi _OK / _NG / _LIMIT trong tên file")
                    + "\n" + rows
                    + "\nNhiều SPL\tmột thư mục chứa ảnh của nhiều sản phẩm → TÊN FILE PHẢI CÓ MÃ SPL"
                    + "\n\tđể ảnh cùng SPL ở các DK ghép chung một hàng. Mô tả tên file ở đoạn cuối schema:"
                    + "\n\t<Model>\\<DK>\\<Tray>\\<Đánh giá>\\<Raw/Mask>\\<SPL>_<Lần chụp>.<Extension>"
                    + "\n\tvd  Model 1\\DK1\\Tray1\\OK\\mask\\P01_0001.jpg  →  SPL = P01, Lần chụp = 0001"
                    + "\nKết hợp\tcấp thư mục và tên file dùng chung, vd"
                    + "\n\t<Raw/Mask>\\<SPL>\\<Name>_<Class>_<Đánh giá>_<Lần chụp>"
                    + "\n\tvd  mask\\P01\\Housing_Xuoc_NG_0001.jpg  →  Raw/Mask = mask, SPL = P01, Name = Housing,"
                    + "\n\tClass = Xuoc, Đánh giá = NG, Lần chụp = 0001"
                    + "\nẢnh timestamp\ttên chỉ có giờ chụp (20261002_101530.jpg) thì mỗi ảnh là một hàng riêng,"
                    + "\n\tKHÔNG ghép được với DK khác → đổi tên thêm mã SPL trước (F2 / Đổi tên, hoặc đặt"
                    + "\n\ttên ảnh ở tab Capture có mã SPL), vd  P01_20261002_101530.jpg  →  <SPL>_<Lần chụp>"
                    + "\nGợi ý\tkết thúc schema bằng dấu \\ nếu không muốn mô tả tên file"
                    + ImageFormatsHint
                    + sampleText;
            }
            else
            {
                _schemaHint = ChipLegendHint + rows + ImageFormatsHint + sampleText;
            }

            UpdateSchemaChips();
        }

        /// <summary>Chú giải thẻ thành phần schema (trước đây vẽ cạnh tiêu đề, nay nằm trong gợi ý ⓘ).</summary>
        private const string ChipLegendHint = "Thẻ schema\tviền liền = trường ở CẤP THƯ MỤC  ·  viền đứt = trường TRONG TÊN FILE\n";

        /// <summary>Chuỗi nhận diện bộ cột hiện tại - đổi thì phải dựng lại cột.</summary>
        private string _columnsSignature;

        private string ColumnsSignature()
        {
            // Có số lượng: schema không có DK thì _conditions = [""] - Join ra "" giống hệt danh sách rỗng
            // (lúc chưa có ảnh), bảng sẽ không dựng block Vision | Đánh giá | Raw | Mask.
            return (chkUnmatchedOnly.Checked ? "U|" : "P|") + _schema.Text + "|" + _conditions.Count + "|" + string.Join("\u0001", _conditions);
        }

        /// <summary>Schema có trường DK -> tiêu đề hai tầng (tên DK phủ trên block cột của nó).</summary>
        private bool HasBlockHeader
        {
            // Luôn hai tầng (trừ chế độ "Chỉ ảnh sai schema"): THÔNG TIN | KẾT QUẢ (· DK) | TỆP (· DK).
            get { return !chkUnmatchedOnly.Checked; }
        }

        /// <summary>
        /// Dựng cột theo schema + các DK đang có trong thư mục:
        ///     [trường Key...] [Hậu tố] | DK1: UnitX Evaluation | DK2: UnitX Evaluation ... | DK1: Raw Mask | DK2: Raw Mask ...
        /// Chế độ "Chỉ ảnh không khớp schema": mỗi hàng một ảnh, chỉ có Tên ảnh + Thư mục.
        /// </summary>
        private void RebuildColumns()
        {
            _columnsSignature = ColumnsSignature();
            _applyingLayout = true;
            grid.Columns.Clear();

            if (chkUnmatchedOnly.Checked)
            {
                grid.Columns.Add(TextColumn("colName", "Tên ảnh", 260, DataGridViewAutoSizeColumnMode.Fill));
                grid.Columns.Add(TextColumn("colDir", "Thư mục", 160, DataGridViewAutoSizeColumnMode.None));
            }
            else
            {
                foreach (var field in _schema.KeyFields)
                {
                    grid.Columns.Add(TextColumn("key_" + field.Name, field.Name, 100, DataGridViewAutoSizeColumnMode.None));
                }

                if (_schema.HasSuffix)
                {
                    var suffixField = _schema.Fields.First(f => f.Role == DatasetFieldRole.Suffix);
                    grid.Columns.Add(TextColumn("colSuffix", suffixField.Name, 150, DataGridViewAutoSizeColumnMode.None));
                }

                // Kết quả trước (UnitX | Evaluation của từng DK nối nhau), ảnh sau (Raw | Mask của
                // từng DK nối nhau): nhìn một lượt là so được kết quả các DK của cùng mẫu.
                // Quá nhiều ĐK (schema đặt nhầm <ĐK>): không dựng hàng trăm khối cột - xem TableBlocked.
                int count = TableBlocked ? 0 : _conditions.Count;
                for (int i = 0; i < count; i++)
                {
                    AddBlockColumn(i, i, BlockPart.UnitX, "Vision", 58);
                    AddBlockColumn(i, i, BlockPart.Evaluation, "Đánh giá", 82);
                    AddBlockColumn(i, i, BlockPart.Compare, "Đối soát", 66);
                }

                for (int i = 0; i < count; i++)
                {
                    AddBlockColumn(i, count + i, BlockPart.Raw, "Raw", 44);
                    AddBlockColumn(i, count + i, BlockPart.Mask, "Mask", 48);
                }
            }

            grid.ColumnHeadersHeight = HasBlockHeader ? 46 : 30;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (!IsEvalColumn(column) && !IsCompareColumn(column))
                {
                    column.ReadOnly = true;
                }

                // Chừa chỗ cho nút lọc ▼ ở mép phải tiêu đề (mọi cột trừ Tên ảnh).
                if (IsFilterableColumn(column))
                {
                    column.HeaderCell.Style.Padding = new Padding(0, 0, FilterGlyphWidth, 0);
                    column.Width += FilterGlyphWidth / 2;
                }
            }

            // Schema đổi -> bỏ bộ lọc của các cột không còn tồn tại.
            foreach (var name in _columnFilters.Keys.Where(k => !grid.Columns.Contains(k)).ToList())
            {
                _columnFilters.Remove(name);
            }

            if (_sortColumn != null && !grid.Columns.Contains(_sortColumn))
            {
                _sortColumn = null;
            }

            ApplyColumnWidths();
            _applyingLayout = false;

            UpdateBlockVisibility();
        }

        /// <summary>
        /// Một cột của block DK. Tên cột theo GIÁ TRỊ DK (không theo thứ tự) để bộ lọc / sắp xếp của cột
        /// vẫn giữ khi thư mục có thêm DK mới.
        /// </summary>
        /// <param name="groupIndex">Nhóm cột trên tiêu đề (tên DK gộp ô trên các cột cùng nhóm).</param>
        private void AddBlockColumn(int conditionIndex, int groupIndex, BlockPart part, string header, int width)
        {
            string condition = _conditions[conditionIndex];
            string name = "blk_" + part + "_" + condition;
            var info = new BlockColumn { Condition = condition, GroupIndex = groupIndex, Part = part };

            DataGridViewColumn column;
            if (part == BlockPart.Evaluation || part == BlockPart.Compare)
            {
                var combo = new DataGridViewComboBoxColumn
                {
                    FlatStyle = FlatStyle.Flat,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
                };
                combo.Items.AddRange((part == BlockPart.Compare ? CompareChoices : EvaluationChoices).Cast<object>().ToArray());
                combo.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                column = combo;
            }
            else
            {
                column = new DataGridViewTextBoxColumn();
                if (part == BlockPart.UnitX)
                {
                    column.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
            }

            column.Name = name;
            column.HeaderText = header;
            column.Width = width;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.FillWeight = 1F;   // xem TextColumn: giới hạn tổng FillWeight 65535
            column.SortMode = DataGridViewColumnSortMode.Programmatic;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.Tag = info;
            grid.Columns.Add(column);
        }

        private static BlockColumn BlockOf(DataGridViewColumn column)
        {
            return column == null ? null : column.Tag as BlockColumn;
        }

        private static bool IsEvalColumn(DataGridViewColumn column)
        {
            var info = BlockOf(column);
            return info != null && info.Part == BlockPart.Evaluation;
        }

        /// <summary>DK đang chọn ở ô lọc DK, null = tất cả.</summary>
        private string SelectedCondition()
        {
            return cboDkFilter.SelectedIndex <= 0 ? null : cboDkFilter.SelectedItem as string;
        }

        /// <summary>Các DK đang hiển thị (theo ô lọc DK).</summary>
        private List<string> VisibleConditions()
        {
            string only = SelectedCondition();
            return only == null ? _conditions.ToList() : _conditions.Where(c => string.Equals(c, only, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>Ẩn block cột của các DK không được chọn ở ô lọc DK.</summary>
        private void UpdateBlockVisibility()
        {
            string only = SelectedCondition();
            foreach (DataGridViewColumn column in grid.Columns)
            {
                var info = BlockOf(column);
                if (info != null)
                {
                    column.Visible = only == null || string.Equals(info.Condition, only, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        /// <summary>Danh sách ô lọc DK theo các DK đang có, giữ lựa chọn cũ nếu DK đó còn.</summary>
        private void RefreshConditionFilter()
        {
            string selected = SelectedCondition();
            bool wasFilling = _fillingGrid;
            _fillingGrid = true;
            try
            {
                cboDkFilter.BeginUpdate();
                cboDkFilter.Items.Clear();
                cboDkFilter.Items.Add("Tất cả");
                if (_schema.ConditionField != null)
                {
                    foreach (var condition in _conditions)
                    {
                        cboDkFilter.Items.Add(condition);
                    }
                }

                int index = selected == null ? 0 : cboDkFilter.Items.IndexOf(selected);
                cboDkFilter.SelectedIndex = Math.Max(0, index);
                cboDkFilter.Enabled = cboDkFilter.Items.Count > 2;
                cboDkFilter.EndUpdate();
            }
            finally
            {
                _fillingGrid = wasFilling;
            }
        }

        private static DataGridViewTextBoxColumn TextColumn(string name, string header, int width,
            DataGridViewAutoSizeColumnMode mode)
        {
            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = width,
                AutoSizeMode = mode,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                // Tổng FillWeight của mọi cột không được vượt 65535 (mặc định 100/cột -> tối đa ~655 cột). Cột rộng
                // cố định không dùng FillWeight nên đặt 1 - schema sai sinh hàng trăm DK vẫn không làm sập bảng.
                FillWeight = mode == DataGridViewAutoSizeColumnMode.Fill ? 100F : 1F
            };

            if (mode == DataGridViewAutoSizeColumnMode.Fill)
            {
                column.MinimumWidth = 160;
            }

            return column;
        }

        #endregion

        #region Tải thư mục

        private void BrowseFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục chứa ảnh dataset";
                string current = txtFolder.Text.Trim().Trim('"');
                if (Directory.Exists(current))
                {
                    dialog.SelectedPath = current;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                    LoadFolder();
                }
            }
        }

        private async void LoadFolder()
        {
            if (_loading)
            {
                return;
            }

            string folder = txtFolder.Text.Trim().Trim('"');
            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                MessageBox.Show(this, "Thư mục không tồn tại:" + Environment.NewLine + folder,
                    "Thiếu thư mục", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            folder = Path.GetFullPath(folder);
            txtFolder.Text = folder;
            ImageComments.Invalidate(folder);   // bấm Quét thư mục = đọc lại cả ghi chú (file json có thể đã đổi)

            _loading = true;

            // Thư mục có dataset.json (đã từng mở bằng app) -> dùng lại chế độ / schema / logic của CHÍNH thư mục đó.
            string restored = RestoreDatasetSettings(folder);
            bool recursive = chkSubfolders.Checked || _folderMode;   // schema có cấp thư mục: luôn quét thư mục con
            btnLoad.Enabled = false;
            btnExport.Enabled = false;
            ShowStatusMessage("Đang đọc thư mục...");

            try
            {
                var files = await Task.Run(() => EnumerateImages(folder, recursive));

                _images.Clear();
                foreach (var file in files)
                {
                    _images.Add(CreateImage(folder, file));
                }

                _loadedFolder = folder;
                _autoRefresh.Watch(folder, recursive);
                _evaluations = LoadEvaluations(folder);
                MigrateLegacyEvaluations();
                if (_legacyLabelsPending)
                {
                    SaveEvaluations();   // chuyển dataset_evaluation.json cũ sang dataset_collection.json
                }
                SaveSettings();

                FillGrid();
                UpdateSchemaInfo(null);
                UpdateConvertButtons();   // thư mục này đã chuyển sang quản lý theo tên chưa (nhật ký)
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không đọc được thư mục:" + Environment.NewLine + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _loading = false;
                btnLoad.Enabled = true;
                btnExport.Enabled = true;
                UpdateSummary();
                if (restored != null)
                {
                    ShowStatusMessage(restored);
                    StatusResetSoon();
                }
            }
        }

        private static string RelativeDir(string root, string file)
        {
            string dir = Path.GetDirectoryName(file) ?? string.Empty;
            string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar), root.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
            {
                return ".";
            }

            return dir.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? dir.Substring(prefix.Length) : dir;
        }

        private void ParseImage(DatasetImage image)
        {
            // Theo thư mục: khớp cả đường dẫn tương đối (thư mục con + tên file); theo tên: chỉ tên file.
            var result = _folderMode
                ? _schema.MatchPath(image.RelativeDir == "." ? image.FileName : Path.Combine(image.RelativeDir, image.FileName))
                : _schema.Match(image.FileName);

            image.Matched = result.Success;
            image.Values = result.Values;
            image.Suffix = result.Suffix ?? string.Empty;
            image.FileEvaluation = result.Evaluation ?? string.Empty;

            // Không có trường Raw/Mask trong tên (ảnh raw do app lưu) -> suy ra từ thư mục / "_mask".
            image.Type = result.Type ?? InferType(image);

            image.Condition = result.Condition ?? string.Empty;
            image.Shot = result.Shot ?? string.Empty;

            // PairKey giữ đúng dạng cũ (mọi trường định danh theo thứ tự schema + hậu tố) để nhãn đã lưu
            // theo khoá cũ vẫn chuyển đổi được (MigrateLegacyEvaluations). Có <Lần chụp>: Raw ↔ Mask ghép
            // theo CÙNG lần chụp (RowKey thì không gồm lần chụp - các lần chụp chung một hàng).
            image.PairKey = result.Success
                ? string.Join("|", _schema.IdentityFields.Select(f => result.Values[f.Name]).Concat(new[] { image.Suffix }))
                    .ToUpperInvariant()
                    + (_schema.HasShot ? "|#" + image.Shot.ToUpperInvariant() : string.Empty)
                : "?" + image.FullPath.ToUpperInvariant();

            image.RowKey = result.Success
                ? string.Join("|", _schema.KeyFields.Select(f => result.Values[f.Name]).Concat(new[] { image.Suffix }))
                    .ToUpperInvariant()
                : image.PairKey;

            // Theo thư mục mà Evaluation là một cấp thư mục (OK , NG): cùng tên mẫu ở hai thư mục kết quả là
            // HAI mẫu khác nhau - thêm vào khoá để Raw chỉ ghép với Mask trong cùng thư mục kết quả.
            // (Đánh giá nằm trong TÊN file thì không: Raw có thể không ghi kết quả trong tên.)
            if (result.Success && _folderMode && _schema.Fields.Any(f => f.Role == DatasetFieldRole.Evaluation && f.InFolder))
            {
                image.PairKey += "|" + result.Evaluation;

                // Có <Lần chụp>: các lần chụp OK / NG của cùng mẫu vẫn gộp một hàng (Vision = phép nhân logic).
                if (!_schema.HasShot)
                {
                    image.RowKey += "|" + result.Evaluation;
                }
            }
        }

        private static string InferType(DatasetImage image)
        {
            string dirName = Path.GetFileName(Path.GetDirectoryName(image.FullPath) ?? string.Empty);
            if (string.Equals(dirName, "mask", StringComparison.OrdinalIgnoreCase)
                || image.FileName.IndexOf("_mask", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Mask";
            }

            return "Raw";
        }

        #endregion

        #region Bảng

        /// <summary>Nền ô của block không có ảnh nào (mẫu chưa chụp ở DK đó).</summary>
        private static readonly Color EmptyBlockColor = Color.FromArgb(249, 250, 251);
        private static readonly Color PresentColor = Color.FromArgb(22, 163, 74);
        private const string PresentMark = "✓";

        /// <summary>Hàng của chế độ "Chỉ ảnh không khớp schema" (mỗi ảnh một hàng).</summary>
        private readonly List<DatasetRow> _unmatchedRows = new List<DatasetRow>();

        /// <summary>
        /// Gom ảnh khớp schema thành hàng: cùng trường Key + hậu tố = một hàng, mỗi DK một block
        /// Raw + Mask. Đồng thời ghép cặp Raw ↔ Mask: ảnh Raw không có Mask trong block = chưa ghi nhận
        /// kết quả (NoResult - nền xám, UnitX / Evaluation trống).
        /// </summary>
        private void BuildRows()
        {
            _rows.Clear();
            _unmatchedRows.Clear();
            var conditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var byKey = new Dictionary<string, List<DatasetRow>>(StringComparer.OrdinalIgnoreCase);
            bool shotMode = _schema.HasShot;

            // Schema bỏ qua phần đuôi (vd "_1" sau <Đánh giá>) mà không có <Lần chụp>: các ảnh chỉ khác nhau ở phần đuôi
            // là cùng một mẫu -> lấy ảnh ĐẦU TIÊN theo tên file (sắp tự nhiên), các ảnh sau bỏ qua.
            bool firstOnly = _schema.IgnoresTail && !shotMode;
            var ordered = firstOnly ? _images.OrderBy(i => i.FileName, NaturalComparer.Instance).ToList() : _images;

            foreach (var image in ordered)
            {
                image.PairMask = null;
                image.NoResult = false;
                image.Skipped = false;

                if (!image.Matched)
                {
                    _unmatchedRows.Add(new DatasetRow { Key = image.FullPath, Unmatched = image });
                    continue;
                }

                string condition = image.Condition ?? string.Empty;
                bool isMask = image.Type == "Mask";
                conditions.Add(condition);

                List<DatasetRow> candidates;
                if (!byKey.TryGetValue(image.RowKey, out candidates))
                {
                    candidates = new List<DatasetRow>();
                    byKey[image.RowKey] = candidates;
                }

                // Trùng hoàn toàn tên (vd. cùng tên ở hai thư mục con) thì ảnh sau sang hàng mới, không
                // đè mất ảnh trước. Có <Lần chụp>: chỉ trùng khi cùng lần chụp + cùng loại.
                DatasetRow target = candidates.FirstOrDefault(r =>
                {
                    var b = r.Block(condition);
                    if (b == null)
                    {
                        return true;
                    }

                    if (shotMode)
                    {
                        var shot = b.Shots.FirstOrDefault(s => string.Equals(s.Index, image.Shot, StringComparison.OrdinalIgnoreCase));
                        return shot == null || (isMask ? shot.Mask == null : shot.Raw == null);
                    }

                    return isMask ? b.Mask == null : b.Raw == null;
                });

                if (target == null && firstOnly && candidates.Count > 0)
                {
                    image.Skipped = true;   // đã có ảnh cùng mẫu / ĐK / loại đứng trước theo tên file
                    continue;
                }

                if (target == null)
                {
                    target = new DatasetRow { Key = image.RowKey, Suffix = image.Suffix };
                    foreach (var field in _schema.KeyFields)
                    {
                        string value;
                        image.Values.TryGetValue(field.Name, out value);
                        target.Values[field.Name] = value ?? string.Empty;
                    }

                    candidates.Add(target);
                    _rows.Add(target);
                }

                var block = target.Block(condition);
                if (block == null)
                {
                    block = new DatasetBlock { Condition = condition, Shots = shotMode ? new List<DatasetShot>() : null };
                    target.Blocks[condition] = block;
                }

                if (shotMode)
                {
                    AddToShot(block, image, isMask);
                }
                else if (isMask)
                {
                    block.Mask = image;
                }
                else
                {
                    block.Raw = image;
                }
            }

            foreach (var block in _rows.SelectMany(r => r.Blocks.Values))
            {
                if (block.Shots != null)
                {
                    FinishShots(block);
                    continue;
                }

                if (block.Raw != null)
                {
                    block.Raw.PairMask = block.Mask;
                    // Raw chưa có Mask vẫn có kết quả nếu tên file / thư mục đã ghi <Đánh giá> (dataset chỉ có ảnh Raw,
                    // vd ảnh từ phần mềm đánh giá khác): Vision = <Đánh giá> của chính ảnh Raw.
                    block.Raw.NoResult = block.Mask == null && string.IsNullOrEmpty(block.Raw.FileEvaluation);
                }
            }

            var keyFields = _schema.KeyFields;
            _rows.Sort((a, b) =>
            {
                int c = NaturalComparer.Instance.Compare(
                    string.Join("\u0001", keyFields.Select(f => a.Values[f.Name])),
                    string.Join("\u0001", keyFields.Select(f => b.Values[f.Name])));
                return c != 0 ? c : StringComparer.OrdinalIgnoreCase.Compare(a.Suffix, b.Suffix);
            });

            _conditions.Clear();
            _conditions.AddRange(conditions.OrderBy(c => c, NaturalComparer.Instance));
        }

        private void FillGrid()
        {
            // Mọi thay đổi danh sách ảnh (tải, đổi schema, xoá, đổi tên) đều dựng lại bảng qua đây.
            BuildRows();

            if (_columnsSignature != ColumnsSignature())
            {
                RefreshConditionFilter();
                RebuildColumns();
            }

            RebuildTreeIfChanged();
            tvDataset.Enabled = !chkUnmatchedOnly.Checked;

            _fillingGrid = true;
            grid.SuspendLayout();

            try
            {
                grid.Rows.Clear();
                // Schema cho ra quá nhiều ĐK: bảng để trống (đổ hàng nghìn ô làm treo phần mềm) - thanh trạng thái /
                // tiêu đề bảng báo cần sửa schema.
                var source = chkUnmatchedOnly.Checked ? _unmatchedRows : TableBlocked ? new List<DatasetRow>() : _rows;
                var rows = new List<DataGridViewRow>(source.Count);
                var filter = BuildFilter();

                // Lọc bằng cách dựng lại bảng chỉ với các hàng khớp - KHÔNG ẩn dòng bằng row.Visible:
                // DataGridView âm thầm bỏ qua / ném lỗi khi ẩn dòng đang được chọn hoặc đang sửa ô.
                foreach (var item in source.Where(filter))
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(grid);
                    row.Tag = item;
                    SetRowValues(row, item);
                    rows.Add(row);
                }

                grid.Rows.AddRange(rows.ToArray());

                // Dựng lại bảng (lọc, đổi nhãn...) không làm mất thứ tự sắp xếp người dùng đã chọn.
                if (_sortColumn != null && grid.Columns.Contains(_sortColumn))
                {
                    ApplySort();
                }
            }
            finally
            {
                grid.ResumeLayout();
                _fillingGrid = false;
            }

            UpdateSummary();
        }

        private void SetRowValues(DataGridViewRow gridRow, DatasetRow row)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                var cell = gridRow.Cells[column.Index];
                string value = ColumnValue(row, column.Name);
                value = value == BlankValue ? string.Empty : value;
                // Chỉ ghi ô thật sự đổi: gán lại cùng giá trị vẫn làm bảng vẽ lại ô (giật khi gán nhãn liên tục).
                if (!string.Equals(Convert.ToString(cell.Value), value, StringComparison.Ordinal))
                {
                    cell.Value = value;
                }
            }

            PaintRow(gridRow, row);
        }

        /// <summary>Màu nền + tooltip từng ô theo trạng thái block (thiếu ảnh, chưa có mask, nhãn O/Δ/X).</summary>
        private void PaintRow(DataGridViewRow gridRow, DatasetRow row)
        {
            if (row.Unmatched != null)
            {
                gridRow.DefaultCellStyle.BackColor = UnmatchedColor;
                gridRow.Cells[0].ToolTipText = "Tên file không khớp schema - không đưa vào báo cáo.";
                return;
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                var info = BlockOf(column);
                if (info == null)
                {
                    continue;
                }

                var cell = gridRow.Cells[column.Index];
                var block = row.Block(info.Condition);
                string where = _schema.ConditionField == null ? string.Empty : " (" + info.Condition + ")";

                // Block không có ảnh: ô nhãn không cho chọn, ẩn luôn mũi tên ▼ cho đỡ rối.
                var combo = cell as DataGridViewComboBoxCell;
                if (combo != null)
                {
                    combo.DisplayStyle = block == null
                        ? DataGridViewComboBoxDisplayStyle.Nothing
                        : DataGridViewComboBoxDisplayStyle.ComboBox;
                }

                if (block == null)
                {
                    cell.Style.BackColor = EmptyBlockColor;
                    cell.ToolTipText = "Mẫu này chưa có ảnh" + where + ".";
                    cell.ReadOnly = true;
                    continue;
                }

                bool noResult = block.Raw != null && block.Raw.NoResult;
                Color back = noResult ? NoResultColor : Color.White;

                switch (info.Part)
                {
                    case BlockPart.Raw:
                    case BlockPart.Mask:
                        var image = info.Part == BlockPart.Raw ? block.Raw : block.Mask;
                        cell.Style.ForeColor = PresentColor;
                        cell.ToolTipText = image != null
                            ? image.FileName + (image.RelativeDir == "." ? string.Empty : "   [" + image.RelativeDir + "]")
                                + (ImageComments.Has(image.FullPath) ? Environment.NewLine + "✎ Ghi chú: " + ImageComments.Get(image.FullPath) : string.Empty)
                            : info.Part == BlockPart.Mask
                                ? "Chưa có ảnh Mask" + where + " - chưa ghi nhận kết quả. Push lại ảnh Raw ở tab Capture & Monitor để lấy mask."
                                : "Không có ảnh Raw" + where + ".";
                        if (block.Shots != null && block.Shots.Count > 1)
                        {
                            cell.ToolTipText = ShotsSummary(block) + Environment.NewLine + "Đang hiện (Vision xấu nhất): " + cell.ToolTipText
                                + Environment.NewLine + "Phím , / . trong bảng: xem lần chụp trước / sau.";
                        }

                        break;

                    case BlockPart.UnitX:
                        cell.ToolTipText = noResult ? "Chưa có ảnh Mask - chưa ghi nhận kết quả." : "Kết quả Vision (từ tên ảnh Mask)";
                        if (block.Shots != null && block.Shots.Count > 1)
                        {
                            cell.ToolTipText = "Vision gộp " + ShotsSummary(block);
                        }

                        back = LabelColor(BlockUnitX(block), back);
                        break;

                    case BlockPart.Evaluation:
                        back = LabelColor(BlockEvaluation(block), back);
                        cell.ToolTipText = EvaluationTooltip(block);
                        cell.ReadOnly = false;
                        break;

                    case BlockPart.Compare:
                        string compare = BlockCompare(block);
                        if (compare == CompareMatch) back = EvalOColor;
                        else if (compare == CompareDiff) back = EvalXColor;
                        cell.ToolTipText = CompareTooltip(block);
                        // Chưa có kết quả Vision thì không có gì để đối soát.
                        bool canCompare = BlockUnitX(block).Length > 0;
                        cell.ReadOnly = !canCompare;
                        if (combo != null && !canCompare)
                        {
                            combo.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                        }
                        break;
                }

                cell.Style.BackColor = back;
            }
        }

        /// <summary>Nhãn Evaluation của một block = mức xấu nhất của Raw + Mask (gán Mask thì Raw theo).</summary>
        private string BlockEvaluation(DatasetBlock block)
        {
            return BlockEvaluationCore(block);
        }

        /// <summary>
        /// Ghi chú của một block cho báo cáo Excel: ghi chú của ảnh Raw và / hoặc Mask. Cả hai cùng có thì
        /// ghi rõ "Raw: ... | Mask: ...", trùng nội dung thì ghi một lần.
        /// </summary>
        private static string BlockComment(DatasetBlock block)
        {
            if (block == null)
            {
                return string.Empty;
            }

            string raw = block.Raw == null ? null : ImageComments.Get(block.Raw.FullPath);
            string mask = block.Mask == null ? null : ImageComments.Get(block.Mask.FullPath);
            if (string.IsNullOrEmpty(raw) || string.IsNullOrEmpty(mask) || raw == mask)
            {
                return string.IsNullOrEmpty(mask) ? raw ?? string.Empty : mask;
            }

            return "Raw: " + raw + " | Mask: " + mask;
        }

        /// <summary>Kết quả UnitX của một block: lấy từ ảnh Mask; Raw chưa có Mask -> trống.</summary>
        private static string BlockUnitX(DatasetBlock block)
        {
            if (block != null && block.Shots != null)
            {
                return ShotsVision(block);   // nhiều lần chụp: phép nhân logic
            }

            return block == null || block.Main == null ? string.Empty : UnitXValue(block.Main);
        }

        /// <summary>
        /// Nhãn Evaluation của ảnh: nhãn người dùng gán tay nếu có; không thì theo logic đang chọn (ô Logic):
        /// "giống Vision" = lấy đúng kết quả Vision; "ngược Vision" = Vision X -> O, Vision O -> X (bộ mẫu NG:
        /// AI bắt được lỗi là đạt), Δ giữ nguyên Δ. Ảnh chưa ghi nhận kết quả (raw thiếu mask) -> trống.
        /// </summary>
        private string EffectiveEvaluation(DatasetImage image)
        {
            string stored = StoredValue(image);
            if (stored != null && !IsRule(stored))
            {
                return stored;
            }

            return ApplyRule(UnitXValue(image), IsRule(stored) ? stored : GlobalRule);
        }

        private static string InvertLabel(string label)
        {
            switch (label)
            {
                case "O": return "X";
                case "X": return "O";
                default: return label ?? string.Empty;
            }
        }

        /// <summary>
        /// Cột UnitX: kết quả lấy từ ẢNH MASK (ảnh kết quả UnitX thật sự trả về). Ảnh Raw lấy kết quả
        /// của mask cùng cặp - kết quả trong tên file raw có thể đã cũ nếu ảnh được push lại. Raw chưa
        /// có mask = chưa ghi nhận kết quả -> để trống.
        /// </summary>
        private static string UnitXValue(DatasetImage image)
        {
            if (image.NoResult)
            {
                return string.Empty;
            }

            if (image.Type == "Raw" && image.PairMask != null)
            {
                return image.PairMask.FileEvaluation ?? string.Empty;
            }

            return image.FileEvaluation ?? string.Empty;
        }

        private static DatasetRow RowOf(DataGridViewRow row)
        {
            return row == null ? null : row.Tag as DatasetRow;
        }

        /// <summary>
        /// Block của một ô: ô thuộc block DK -> block đó; ô trường Key / hậu tố -> block ĐANG HIỂN THỊ
        /// đầu tiên có ảnh của hàng.
        /// </summary>
        private DatasetBlock CellBlock(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
            {
                return null;
            }

            var row = RowOf(grid.Rows[rowIndex]);
            if (row == null || row.Unmatched != null)
            {
                return null;
            }

            var info = columnIndex >= 0 && columnIndex < grid.Columns.Count ? BlockOf(grid.Columns[columnIndex]) : null;
            if (info != null)
            {
                return row.Block(info.Condition);
            }

            return VisibleConditions().Select(row.Block).FirstOrDefault(b => b != null);
        }

        /// <summary>Ảnh của một ô: cột Raw -> ảnh Raw, các cột khác của block -> ảnh Mask (thiếu thì Raw).</summary>
        private DatasetImage CellImage(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
            {
                return null;
            }

            var row = RowOf(grid.Rows[rowIndex]);
            if (row != null && row.Unmatched != null)
            {
                return row.Unmatched;
            }

            var block = CellBlock(rowIndex, columnIndex);
            if (block == null)
            {
                return null;
            }

            var info = columnIndex >= 0 && columnIndex < grid.Columns.Count ? BlockOf(grid.Columns[columnIndex]) : null;

            // Nhiều lần chụp: ảnh của lần chụp đang xem (mặc định lần chụp đại diện, phím , . để chuyển).
            var shot = ViewedShot(block);
            if (shot != null)
            {
                return info != null && info.Part == BlockPart.Raw ? shot.Raw ?? shot.Mask : shot.Main;
            }

            return info != null && info.Part == BlockPart.Raw ? block.Raw ?? block.Mask : block.Main;
        }

        private DatasetImage CurrentImage()
        {
            var cell = grid.CurrentCell;
            return cell == null ? null : CellImage(cell.RowIndex, cell.ColumnIndex);
        }

        /// <summary>
        /// Các block đang chọn: ô của block -> block đó; ô trường Key / hậu tố -> mọi block đang hiển
        /// thị của hàng. Không chọn gì thì lấy ô hiện tại.
        /// </summary>
        private List<DatasetBlock> SelectedBlocks()
        {
            var cells = grid.SelectedCells.Cast<DataGridViewCell>().ToList();
            if (cells.Count == 0 && grid.CurrentCell != null)
            {
                cells.Add(grid.CurrentCell);
            }

            var visible = VisibleConditions();
            var blocks = new List<DatasetBlock>();
            foreach (var cell in cells.OrderBy(c => c.RowIndex).ThenBy(c => c.ColumnIndex))
            {
                var row = RowOf(grid.Rows[cell.RowIndex]);
                if (row == null || row.Unmatched != null)
                {
                    continue;
                }

                var info = BlockOf(grid.Columns[cell.ColumnIndex]);
                var found = info != null
                    ? new[] { row.Block(info.Condition) }
                    : visible.Select(row.Block).ToArray();

                blocks.AddRange(found.Where(b => b != null && !blocks.Contains(b)));
            }

            return blocks;
        }

        #region Ghi chú ảnh

        private static readonly Color CommentMarkColor = Color.FromArgb(234, 88, 12);

        /// <summary>Thanh ghi chú ở đáy khung xem trước (ghi chú của ảnh đang xem).</summary>
        private readonly Label lblPreviewComment = new Label();

        private void SetupCommentUi()
        {
            lblPreviewComment.Dock = DockStyle.Bottom;
            lblPreviewComment.Height = 28;
            lblPreviewComment.Visible = false;
            lblPreviewComment.AutoEllipsis = true;
            lblPreviewComment.TextAlign = ContentAlignment.MiddleLeft;
            lblPreviewComment.Padding = new Padding(8, 0, 8, 0);
            lblPreviewComment.BackColor = Color.FromArgb(254, 243, 199);
            lblPreviewComment.ForeColor = Color.FromArgb(120, 53, 15);
            lblPreviewComment.Font = new Font("Segoe UI", 9.5F);
            lblPreviewComment.Cursor = Cursors.Hand;
            lblPreviewComment.Click += (s, e) =>
            {
                var shown = PreviewImages().Where(i => ImageComments.Has(i.FullPath)).Select(i => i.FullPath).ToList();
                if (shown.Count > 0)
                {
                    ImageComments.Edit(this, shown);
                }
            };

            picPreview.Controls.Add(lblPreviewComment);   // thanh ghi chú nằm đè đáy ảnh (không đặt trong Designer)
            ImageComments.Changed += OnImageCommentsChanged;
        }

        /// <summary>Ô Raw / Mask của ảnh có ghi chú (vẽ tam giác cam ở góc).</summary>
        private bool HasCommentMark(int rowIndex, int columnIndex)
        {
            var info = BlockOf(grid.Columns[columnIndex]);
            if (info == null || (info.Part != BlockPart.Raw && info.Part != BlockPart.Mask))
            {
                return false;
            }

            var row = RowOf(grid.Rows[rowIndex]);
            var block = row == null ? null : row.Block(info.Condition);
            var image = block == null ? null : info.Part == BlockPart.Raw ? block.Raw : block.Mask;
            return image != null && ImageComments.Has(image.FullPath);
        }

        /// <summary>Ghi chú đổi (ở tab này, tab Capture, hoặc do đổi tên / xoá ảnh): vẽ lại hàng liên quan.</summary>
        private void OnImageCommentsChanged(IList<string> paths)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => OnImageCommentsChanged(paths)));
                return;
            }

            var set = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
            _fillingGrid = true;
            try
            {
                foreach (DataGridViewRow gridRow in grid.Rows)
                {
                    var row = RowOf(gridRow);
                    if (row != null && row.Images.Any(i => set.Contains(i.FullPath)))
                    {
                        SetRowValues(gridRow, row);
                    }
                }
            }
            finally
            {
                _fillingGrid = false;
            }

            UpdatePreviewComment();
            RefreshTreeCounts();
        }

        /// <summary>Ảnh đang hiện ở khung xem trước (hai ảnh khi đang xem song song Raw | Mask).</summary>
        private IEnumerable<DatasetImage> PreviewImages()
        {
            if (_previewImage != null)
            {
                yield return _previewImage;
            }

            if (_splitView && _splitMaskImage != null && _splitMaskImage != _previewImage)
            {
                yield return _splitMaskImage;
            }
        }

        private void UpdatePreviewComment()
        {
            var parts = PreviewImages()
                .Where(i => ImageComments.Has(i.FullPath))
                .Select(i => (PreviewImages().Count() > 1 ? i.Type + ": " : string.Empty) + ImageComments.Get(i.FullPath).Replace(Environment.NewLine, "  "))
                .ToList();

            lblPreviewComment.Visible = parts.Count > 0;
            lblPreviewComment.Text = "✎  " + string.Join("   |   ", parts);
        }

        #endregion

        // Xem song song Raw | Mask (bước 3) - khai báo sớm để phần ghi chú biết ảnh Mask đang hiện.
        private bool _splitView;
        private DatasetImage _splitMaskImage;

        #region Cây dataset

        private bool _treeUpdating;
        private int _treeSignature;

        /// <summary>Lọc theo nút cây đang chọn: trường Key -> giá trị (DK lọc qua ô DK để ẩn block khác).</summary>
        private readonly Dictionary<string, string> _treeKeyFilter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Một mẫu (hàng × DK) trong cây.</summary>
        private sealed class TreeEntry
        {
            public DatasetRow Row;
            public DatasetBlock Block;
        }

        /// <summary>Đường dẫn của nút: các cặp (tên trường, giá trị) từ gốc xuống.</summary>
        private sealed class TreeTag
        {
            public List<KeyValuePair<string, string>> Path = new List<KeyValuePair<string, string>>();

            public string Key
            {
                get { return string.Join("\u0001", Path.Select(p => p.Key + "=" + p.Value)); }
            }
        }

        /// <summary>Dữ liệu ảnh đổi (tải, đổi schema, xoá, đổi tên) thì dựng lại cây; chỉ lọc thì giữ nguyên.</summary>
        private void RebuildTreeIfChanged()
        {
            int signature = _schema.Text.GetHashCode();
            foreach (var image in _images)
            {
                signature = unchecked(signature * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(image.FullPath));
            }

            if (signature != _treeSignature || tvDataset.Nodes.Count == 0)
            {
                _treeSignature = signature;
                RebuildTree();
            }
        }

        /// <summary>Nhãn / ghi chú vừa đổi: tính lại số liệu trên cây (giữ nút đang mở / đang chọn).</summary>
        /// <summary>
        /// Cập nhật số liệu trên các nút cây TẠI CHỖ (chữ + tooltip + màu), không dựng lại cây - gán nhãn /
        /// ghi chú không làm cây nháy, không mất vị trí cuộn. Cấu trúc cây đổi (thêm / xoá ảnh) thì
        /// RebuildTreeIfChanged lo.
        /// </summary>
        private void RefreshTreeCounts()
        {
            if (tvDataset.Nodes.Count == 0)
            {
                RebuildTree();
                return;
            }

            var entries = _rows
                .SelectMany(r => r.Blocks.Values.Select(b => new TreeEntry { Row = r, Block = b }))
                .ToList();

            tvDataset.BeginUpdate();
            try
            {
                UpdateTreeNode(tvDataset.Nodes[0], entries, null);
            }
            finally
            {
                tvDataset.EndUpdate();
            }
        }

        private void UpdateTreeNode(TreeNode node, List<TreeEntry> entries, string field)
        {
            var tag = (TreeTag)node.Tag;
            string text = tag.Path.Count == 0 ? "Dataset" : tag.Path[tag.Path.Count - 1].Value;
            var fresh = MakeNode(text.Length == 0 ? "(trống)" : text, tag, entries, field);
            if (node.Text != fresh.Text) node.Text = fresh.Text;
            if (node.ToolTipText != fresh.ToolTipText) node.ToolTipText = fresh.ToolTipText;
            if (node.ForeColor != fresh.ForeColor) node.ForeColor = fresh.ForeColor;

            var conditionField = _schema.ConditionField;
            foreach (TreeNode child in node.Nodes)
            {
                var part = ((TreeTag)child.Tag).Path.Last();
                bool isCondition = conditionField != null && string.Equals(part.Key, conditionField.Name, StringComparison.OrdinalIgnoreCase);
                var subset = entries.Where(e => string.Equals(isCondition ? e.Block.Condition : ValueOf(e.Row, part.Key), part.Value,
                    StringComparison.OrdinalIgnoreCase)).ToList();
                UpdateTreeNode(child, subset, part.Key);
            }
        }

        /// <summary>
        /// Dựng cây theo thứ tự các trường định danh trong schema (vd Model → DK → SPL). Mỗi nút:
        /// "giá trị  [số ảnh]  ⚠ thiếu Mask  ✎ có ghi chú", tooltip đủ Total / đánh giá tay / thiếu Mask /
        /// ghi chú. Giữ lại các nút đang mở và nút đang chọn.
        /// </summary>
        private void RebuildTree()
        {
            var expanded = new HashSet<string>();
            CollectExpanded(tvDataset.Nodes, expanded);
            string selected = tvDataset.SelectedNode != null ? ((TreeTag)tvDataset.SelectedNode.Tag).Key : null;

            var entries = _rows
                .SelectMany(r => r.Blocks.Values.Select(b => new TreeEntry { Row = r, Block = b }))
                .ToList();

            _treeUpdating = true;
            tvDataset.BeginUpdate();
            try
            {
                tvDataset.Nodes.Clear();
                var root = MakeNode("Dataset", new TreeTag(), entries, null);
                AddTreeChildren(root, entries, _schema.IdentityFields, 0);
                tvDataset.Nodes.Add(root);

                root.Expand();
                RestoreExpanded(tvDataset.Nodes, expanded);

                var select = selected == null ? null : FindNode(tvDataset.Nodes, selected);
                if (select == null && selected != null && _treeKeyFilter.Count > 0)
                {
                    // Nút đang lọc không còn (vd ảnh bị xoá hết) -> bỏ lọc cây.
                    _treeKeyFilter.Clear();
                }

                tvDataset.SelectedNode = select ?? root;
            }
            finally
            {
                tvDataset.EndUpdate();
                _treeUpdating = false;
            }
        }

        private void AddTreeChildren(TreeNode parent, List<TreeEntry> entries, IList<DatasetField> levels, int depth)
        {
            if (depth >= levels.Count)
            {
                return;
            }

            var field = levels[depth];
            bool isCondition = field.Role == DatasetFieldRole.Condition;
            var parentTag = (TreeTag)parent.Tag;

            var groups = entries
                .GroupBy(e => isCondition ? e.Block.Condition : ValueOf(e.Row, field.Name), StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, NaturalComparer.Instance);

            foreach (var group in groups)
            {
                var tag = new TreeTag();
                tag.Path.AddRange(parentTag.Path);
                tag.Path.Add(new KeyValuePair<string, string>(field.Name, group.Key));

                var list = group.ToList();
                var node = MakeNode(group.Key.Length == 0 ? "(trống)" : group.Key, tag, list, field.Name);
                parent.Nodes.Add(node);
                AddTreeChildren(node, list, levels, depth + 1);
            }
        }

        private static string ValueOf(DatasetRow row, string field)
        {
            string value;
            return row.Values.TryGetValue(field, out value) ? value ?? string.Empty : string.Empty;
        }

        private TreeNode MakeNode(string text, TreeTag tag, List<TreeEntry> entries, string field)
        {
            var images = entries.SelectMany(e => e.Block.Images).ToList();
            int missing = entries.Count(e => e.Block.Raw != null && e.Block.Mask == null);
            int comments = images.Count(i => ImageComments.Has(i.FullPath));
            int manual = entries.Count(e => e.Block.Images.Any(i => i.EvalKey != null && _evaluations.ContainsKey(i.EvalKey)));

            string label = text + "  [" + images.Count + "]"
                + (missing > 0 ? "  ⚠" + missing : string.Empty)
                + (comments > 0 ? "  ✎" + comments : string.Empty);

            var node = new TreeNode(label) { Tag = tag };
            node.ToolTipText = (field == null ? "Toàn bộ dataset" : field + " = " + text)
                + Environment.NewLine + $"Tổng: {images.Count} ảnh ({entries.Count} mẫu)"
                + Environment.NewLine + $"Đã đánh giá tay: {manual} mẫu"
                + Environment.NewLine + $"Chưa có Mask: {missing}"
                + Environment.NewLine + $"Có ghi chú: {comments} ảnh";

            if (missing > 0)
            {
                node.ForeColor = Theme.Fore(Color.FromArgb(180, 83, 9));
            }

            return node;
        }

        private static void CollectExpanded(TreeNodeCollection nodes, HashSet<string> expanded)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded)
                {
                    expanded.Add(((TreeTag)node.Tag).Key);
                }

                CollectExpanded(node.Nodes, expanded);
            }
        }

        private static void RestoreExpanded(TreeNodeCollection nodes, HashSet<string> expanded)
        {
            foreach (TreeNode node in nodes)
            {
                if (expanded.Contains(((TreeTag)node.Tag).Key))
                {
                    node.Expand();
                }

                RestoreExpanded(node.Nodes, expanded);
            }
        }

        private static TreeNode FindNode(TreeNodeCollection nodes, string key)
        {
            foreach (TreeNode node in nodes)
            {
                if (((TreeTag)node.Tag).Key == key)
                {
                    return node;
                }

                var found = FindNode(node.Nodes, key);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Bấm một nút: lọc bảng theo đường dẫn của nút. Trường Key lọc hàng; DK chọn luôn ô lọc DK
        /// (chỉ hiện block của DK đó). Bấm "Dataset" = bỏ lọc cây.
        /// </summary>
        private void ApplyTreeFilter(TreeNode node)
        {
            var tag = node == null ? new TreeTag() : (TreeTag)node.Tag;
            var conditionField = _schema.ConditionField;

            _treeKeyFilter.Clear();
            string condition = null;
            foreach (var part in tag.Path)
            {
                if (conditionField != null && string.Equals(part.Key, conditionField.Name, StringComparison.OrdinalIgnoreCase))
                {
                    condition = part.Value;
                }
                else
                {
                    _treeKeyFilter[part.Key] = part.Value;
                }
            }

            lblTreeTitle.Text = tag.Path.Count == 0
                ? TreeTitleDefault
                : "Đang lọc: " + string.Join(" / ", tag.Path.Select(p => p.Value));

            // Bấm cây thì lọc NGAY (không chờ bộ hẹn giờ gõ phím của ô Lọc).
            int index = condition == null ? 0 : Math.Max(0, cboDkFilter.Items.IndexOf(condition));
            if (cboDkFilter.SelectedIndex != index)
            {
                cboDkFilter.SelectedIndex = index;
            }

            _filterTimer.Stop();
            FillGrid();
        }

        #endregion

        #region Hoàn tác / làm lại thao tác file

        /// <summary>Nhãn gán tay của ảnh đã xoá (đường dẫn đầy đủ -> nhãn) - hoàn tác xoá thì gắn lại.</summary>
        private readonly Dictionary<string, string> _deletedLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Ctrl+Z / Ctrl+Y (ở tab này hay tab Capture): đổi tên / xoá cập nhật bảng như thao tác thường;
        /// ảnh lấy lại từ Thùng rác được thêm lại vào bảng cùng nhãn gán tay cũ, giữ nguyên vị trí đang xem.
        /// </summary>
        private void OnFileHistoryApplied(FileHistoryChange change)
        {
            if (IsDisposed || _loadedFolder == null)
            {
                return;
            }

            if (change.Renamed.Count > 0)
            {
                OnImagesRenamed(change.Renamed);
            }

            if (change.Deleted.Count > 0)
            {
                OnImagesDeleted(change.Deleted);
            }

            if (change.Restored.Count == 0)
            {
                return;
            }

            DatasetImage first = null;
            string root = _loadedFolder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            foreach (var path in change.Restored)
            {
                bool inside = path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                    && (chkSubfolders.Checked || string.Equals(Path.GetDirectoryName(path), _loadedFolder.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
                if (!inside || FindImage(path) != null)
                {
                    continue;
                }

                var image = new DatasetImage
                {
                    FullPath = path,
                    FileName = Path.GetFileName(path),
                    RelativeDir = RelativeDir(_loadedFolder, path)
                };
                image.EvalKey = image.RelativeDir == "." ? image.FileName : Path.Combine(image.RelativeDir, image.FileName);
                ParseImage(image);
                _images.Add(image);
                first = first ?? image;

                string label;
                if (_deletedLabels.TryGetValue(path, out label))
                {
                    _evaluations[image.EvalKey] = label;
                    _deletedLabels.Remove(path);
                }
            }

            _images.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullPath, b.FullPath));
            SaveEvaluations();
            RefillKeepingPosition(first);
            ShowStatusMessage($"Hoàn tác: đã lấy lại {change.Restored.Count} ảnh từ Thùng rác.");
            StatusResetSoon();
        }

        #endregion

        #region Ẩn / hiện khu vực

        private TableLayoutPanel _root;
        private Control _schemaCard;
        private SplitContainer _treeSplit;

        /// <summary>Chiều cao các hàng như trong Designer - ẩn khu vực Schema rồi hiện lại thì trả về đúng cỡ đó.</summary>
        // Ba khối trên cùng = hàng 0..2 của tlpRoot: 0 Nạp dataset · 1 Schema hiện hành · 2 Bộ lọc & trạng thái.
        // Chiều cao mở rộng lấy từ Designer; thu gọn = chỉ còn dòng tiêu đề (hàng 0 của bảng trong thẻ).
        private const int SectionCount = 3;
        private const string GlyphExpanded = "";    // Segoe MDL2: ChevronDown
        private const string GlyphCollapsed = "";   // Segoe MDL2: ChevronRight
        private readonly float[] _sectionHeights = new float[SectionCount];
        private readonly bool[] _sectionCollapsed = new bool[SectionCount];

        private void CaptureDesignerHeights()
        {
            for (int i = 0; i < SectionCount; i++)
            {
                _sectionHeights[i] = tlpRoot.RowStyles[i].Height;
            }
        }

        private CardPanel SectionCard(int index)
        {
            return index == 0 ? pnlLoadCard : index == 1 ? pnlSchemaCard : pnlFilterCard;
        }

        private TableLayoutPanel SectionTable(int index)
        {
            return index == 0 ? tlpLoadCard : index == 1 ? tlpSchemaCard : tlpFilterCard;
        }

        private Button SectionCollapseButton(int index)
        {
            return index == 0 ? btnCollapseLoad : index == 1 ? btnCollapseSchema : btnCollapseFilter;
        }

        private float SectionHeight(int index)
        {
            if (!_sectionCollapsed[index])
            {
                return _sectionHeights[index];
            }

            var card = SectionCard(index);
            return SectionTable(index).RowStyles[0].Height + card.Padding.Vertical + card.Margin.Vertical;
        }

        /// <summary>Nút ˅ / › ở đầu mỗi khối: thu gọn chỉ còn dòng tiêu đề - nhường chỗ cho bảng và khung xem ảnh.</summary>
        private void WireSectionCollapse()
        {
            for (int i = 0; i < SectionCount; i++)
            {
                int index = i;
                SectionCollapseButton(i).Click += (s, e) => SetSectionCollapsed(index, !_sectionCollapsed[index]);
            }
        }

        private void SetSectionCollapsed(int index, bool collapsed)
        {
            if (_sectionCollapsed[index] == collapsed)
            {
                return;
            }

            _sectionCollapsed[index] = collapsed;
            ApplySectionVisibility();
            if (!_restoringSections)
            {
                SaveSettings();
            }
        }

        private List<int> CollapsedSections()
        {
            var list = Enumerable.Range(0, SectionCount).Where(i => _sectionCollapsed[i]).ToList();
            return list.Count == 0 ? null : list;
        }

        private readonly CheckBox chkShowConfig = new CheckBox();
        private readonly CheckBox chkShowTree = new CheckBox();
        private readonly CheckBox chkShowStats = new CheckBox();

        /// <summary>Đang nạp trạng thái ẩn / hiện từ file cấu hình - không ghi ngược lại.</summary>
        private bool _restoringSections;


        /// <summary>
        /// Mục "☰ Hiển thị" của tab này: danh sách tick các khu vực (Schema, Cây, Thống kê, Vấn đề) + "Hiện tất cả".
        /// Không có nút riêng trên tab - form chính đặt các mục này vào nút ☰ Hiển thị ở thanh dưới cùng khi tab
        /// Dataset Manager đang mở (xem SectionMenuItems). Các CheckBox chỉ giữ trạng thái (không nằm trên giao diện).
        /// </summary>
        private void WireSectionToggles()
        {
            var tips = new ToolTip();
            SetupToggle(chkShowConfig, "Schema", "Ẩn / hiện khu vực Thư mục ảnh & Schema tên file", tips);
            SetupToggle(chkShowTree, "Cây", "Ẩn / hiện cây cấu trúc dataset", tips);
            SetupToggle(chkShowStats, "Thống kê", "Ẩn / hiện biểu đồ thống kê", tips);
            SetupToggle(chkShowIssues, "Vấn đề", "Ẩn / hiện danh sách vấn đề của bộ dữ liệu (dưới bảng)", tips);

            var entries = new[]
            {
                new { Box = chkShowConfig, Text = "Thư mục ảnh && Schema tên file" },
                new { Box = chkShowTree, Text = "Cây cấu trúc dataset" },
                new { Box = chkShowStats, Text = "Biểu đồ thống kê" },
                new { Box = chkShowIssues, Text = "Danh sách vấn đề dữ liệu" }
            };

            var items = new List<ToolStripItem>();
            foreach (var entry in entries)
            {
                var box = entry.Box;
                var item = new ToolStripMenuItem(entry.Text) { CheckOnClick = true, Checked = box.Checked };
                item.CheckedChanged += (s, e) => box.Checked = item.Checked;
                box.CheckedChanged += (s, e) => item.Checked = box.Checked;
                items.Add(item);
            }

            items.Add(new ToolStripSeparator());
            var showAll = new ToolStripMenuItem("Hiện tất cả");
            showAll.Click += (s, e) =>
            {
                foreach (var entry in entries)
                {
                    entry.Box.Checked = true;
                }
            };
            items.Add(showAll);
            var hideAll = new ToolStripMenuItem("Ẩn nhanh tất cả");
            hideAll.Click += (s, e) =>
            {
                foreach (var entry in entries)
                {
                    entry.Box.Checked = false;
                }
            };
            items.Add(hideAll);
            _sectionMenuItems = items.ToArray();

            // Nút – ở góc phải từng thẻ (CardPanel.HideButton bật trong Designer): ẩn nhanh khối đó.
            pnlLoadCard.HideButtonClick += (s, e) => chkShowConfig.Checked = false;
            pnlSchemaCard.HideButtonClick += (s, e) => chkShowConfig.Checked = false;
            pnlTreeCard.HideButtonClick += (s, e) => chkShowTree.Checked = false;
            pnlStatsCard.HideButtonClick += (s, e) => chkShowStats.Checked = false;
            pnlIssuesCard.HideButtonClick += (s, e) => chkShowIssues.Checked = false;

            btnShowConfig.Click += (s, e) => chkShowConfig.Checked = true;
        }

        private ToolStripItem[] _sectionMenuItems;

        /// <summary>
        /// Các mục ẩn / hiện khu vực của tab Dataset Manager (mục tick dùng CheckOnClick) - form chính thêm vào nút
        /// ☰ Hiển thị ở thanh dưới cùng và chỉ hiện chúng khi tab này đang mở.
        /// </summary>
        public ToolStripItem[] SectionMenuItems()
        {
            return _sectionMenuItems;
        }

        private void SetupToggle(CheckBox box, string text, string tip, ToolTip tips)
        {
            box.Appearance = Appearance.Button;
            box.FlatStyle = FlatStyle.Flat;
            box.TextAlign = ContentAlignment.MiddleCenter;
            box.Text = text;
            box.Height = 28;
            box.Margin = new Padding(0, 0, 4, 0);
            box.Cursor = Cursors.Hand;
            box.Checked = true;
            box.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            box.FlatAppearance.CheckedBackColor = Color.FromArgb(219, 234, 254);
            box.CheckedChanged += (s, e) =>
            {
                ApplySectionVisibility();
                if (!_restoringSections)
                {
                    SaveSettings();
                }
            };
            tips.SetToolTip(box, tip);
        }

        private void ApplySectionVisibility()
        {
            if (_root == null)
            {
                return;
            }

            SuspendLayout();

            // "Schema": thẻ Nạp dataset + thẻ Schema (khối Bộ lọc & trạng thái luôn hiện).
            bool config = chkShowConfig.Checked;
            pnlLoadCard.Visible = config;
            _schemaCard.Visible = config;
            btnShowConfig.Visible = !config;   // đang ẩn thì luôn có nút mở lại ngay trên hàng Bộ lọc
            _root.RowStyles[0].Height = config ? SectionHeight(0) : 0F;
            _root.RowStyles[1].Height = config ? SectionHeight(1) : 0F;
            _root.RowStyles[2].Height = SectionHeight(2);

            // Khối thu gọn: chỉ còn dòng tiêu đề (ẩn các hàng dưới để không lọt Tab / phím vào ô bị che).
            for (int i = 0; i < SectionCount; i++)
            {
                var table = SectionTable(i);
                foreach (Control control in table.Controls)
                {
                    if (table.GetRow(control) > 0)
                    {
                        control.Visible = !_sectionCollapsed[i];
                    }
                }

                SectionCollapseButton(i).Text = _sectionCollapsed[i] ? GlyphCollapsed : GlyphExpanded;
            }

            _treeSplit.Panel1Collapsed = !chkShowTree.Checked;

            // Hàng dưới: Vấn đề | Thống kê - ẩn cả hai thì bỏ luôn hàng dưới.
            bool issues = chkShowIssues.Checked;
            bool stats = chkShowStats.Checked;
            _issuesSplit.Panel2Collapsed = !issues && !stats;
            if (issues || stats)
            {
                _bottomSplit.Panel1Collapsed = false;
                _bottomSplit.Panel2Collapsed = false;
                if (!issues)
                {
                    _bottomSplit.Panel1Collapsed = true;
                }
                else if (!stats)
                {
                    _bottomSplit.Panel2Collapsed = true;
                }
            }

            pnlFilterCard.Visible = true;
            ApplyPreviewExpand();
            ResumeLayout(true);

            // Khu vực vừa hiện lại -> đúng kích thước đã kéo lần trước.
            ApplyPanelSizes();
            SyncBottomSplit();
        }

        #endregion

        /// <summary>Đổi giao diện Sáng / Tối: vẽ lại phần tự vẽ (thẻ schema, tiêu đề bảng, màu nút cây).</summary>
        private void OnThemeChanged()
        {
            if (IsDisposed)
            {
                return;
            }

            pnlSchemaChips.Invalidate();
            UpdateLockLook();
            grid.Invalidate();
            RebuildTree();
        }

        #region Vùng chọn nhiều mẫu

        /// <summary>Nền nhạt cho cột Model / SPL / Hậu tố của hàng đang có ô được chọn.</summary>
        private static readonly Color SelectedRowTint = Color.FromArgb(224, 236, 255);

        /// <summary>Các hàng đang có ít nhất một ô được chọn (tô nhạt cột định danh để thấy rõ vùng chọn).</summary>
        private HashSet<int> _selectedRowSet = new HashSet<int>();

        private void OnGridSelectionChanged()
        {
            if (_fillingGrid)
            {
                return;
            }

            var rows = new HashSet<int>(grid.SelectedCells.Cast<DataGridViewCell>().Select(c => c.RowIndex));
            if (!rows.SetEquals(_selectedRowSet))
            {
                var changed = new HashSet<int>(rows);
                changed.SymmetricExceptWith(_selectedRowSet);
                _selectedRowSet = rows;
                foreach (int row in changed.Where(r => r >= 0 && r < grid.Rows.Count))
                {
                    grid.InvalidateRow(row);
                }
            }

            UpdateSelectionStatus();
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || !_selectedRowSet.Contains(e.RowIndex)
                || BlockOf(grid.Columns[e.ColumnIndex]) != null)
            {
                return;
            }

            e.CellStyle.BackColor = SelectedRowTint;
        }

        /// <summary>Thanh trạng thái: "Đang chọn N mẫu" khi chọn từ 2 mẫu trở lên.</summary>
        /// <summary>Số hàng đang có ô được chọn (không chọn gì thì tính ô hiện tại).</summary>
        private int SelectedRowCount()
        {
            var rows = new HashSet<int>(grid.SelectedCells.Cast<DataGridViewCell>().Select(c => c.RowIndex));
            if (rows.Count == 0 && grid.CurrentCell != null)
            {
                rows.Add(grid.CurrentCell.RowIndex);
            }

            return rows.Count;
        }

        private void UpdateSelectionStatus()
        {
            if (!stImages.Visible)
            {
                stSelection.Visible = false;   // thanh đang hiện một thông báo
                return;
            }

            // Đếm theo HÀNG (mỗi hàng = một mẫu Model / SPL / hậu tố), không đếm từng block DK hay
            // từng ô Raw / Mask - chọn 2 hàng là 2, không phải 4.
            int rows = chkUnmatchedOnly.Checked ? 0 : SelectedRowCount();
            stSelection.Visible = rows > 1;
            stSelection.Text = $"▣ Đang chọn {rows} hàng";
        }

        #endregion

        /// <summary>Gán nhãn cho block: gán lên ảnh Mask (Raw cùng block tự theo), block chỉ có Raw thì gán lên Raw.</summary>
        private void SetBlockEvaluation(IEnumerable<DatasetBlock> blocks, string value)
        {
            // Nhiều lần chụp: một Đánh giá chung -> ghi lên mọi ảnh của block.
            SetEvaluation(blocks.Where(b => b != null)
                .SelectMany(b => b.Shots != null ? b.Images : new[] { b.Main })
                .ToList(), value);
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var column = e.ColumnIndex >= 0 ? grid.Columns[e.ColumnIndex] : null;
            if (_fillingGrid || e.RowIndex < 0 || column == null || (!IsEvalColumn(column) && !IsCompareColumn(column)))
            {
                return;
            }

            var block = CellBlock(e.RowIndex, e.ColumnIndex);
            if (block == null)
            {
                return;
            }

            string value = Convert.ToString(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? string.Empty;
            if (IsCompareColumn(column))
            {
                SetBlockCompare(new[] { block }, value);
            }
            else
            {
                SetBlockEvaluation(new[] { block }, value);
            }
        }

        /// <summary>
        /// Phím tắt gán nhãn nhanh cho các dòng đang chọn: 1 = O, 2 = Δ, 3 = X, 0/Delete = xoá nhãn
        /// (quay về kết quả UnitX). Gán xong tự xuống dòng kế tiếp.
        /// </summary>
        /// <summary>
        /// Nhãn ứng với một phím gán nhãn, null nếu không phải phím gán nhãn:
        ///   O / 1 = O,  D / 2 = Δ (D = Delta - bàn phím không có phím △),  X / 3 = X,  0 / Delete = xoá nhãn.
        /// Phím có Ctrl / Alt / Shift không tính (Shift + mũi tên để chọn nhiều ô).
        /// </summary>
        private static string EvaluationForKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.O: case Keys.D1: case Keys.NumPad1: return "O";
                case Keys.D: case Keys.D2: case Keys.NumPad2: return "Δ";
                case Keys.X: case Keys.D3: case Keys.NumPad3: return "X";
                case Keys.D0: case Keys.NumPad0: case Keys.Delete: return string.Empty;
                default: return null;
            }
        }

        /// <summary>
        /// Phím gán nhãn khi bảng đang focus - dùng cho CẢ một ô lẫn nhiều ô đang chọn (cùng một hàm
        /// SetBlockEvaluation). Chọn nhiều thì giữ nguyên vùng chọn sau khi gán; chọn một block thì tự
        /// xuống hàng kế tiếp (cùng cột) để gán liên tục như trước.
        /// </summary>
        private bool HandleEvaluationKey(Keys keyData)
        {
            string value = EvaluationForKey(keyData);
            string compare = value == null ? CompareForKey(keyData) : null;
            if (value == null && compare == null)
            {
                return false;
            }

            // Đang mở danh sách thả xuống của ô Evaluation: Delete để ô đó tự xử lý như trước.
            if (keyData == Keys.Delete && grid.IsCurrentCellInEditMode && grid.CurrentCell is DataGridViewComboBoxCell)
            {
                return false;
            }

            var blocks = SelectedBlocks();
            if (blocks.Count == 0)
            {
                return false;
            }

            // Thoát chế độ sửa ô trước (BỎ giá trị đang gõ dở trong ô, không ghi nhận): nếu không, ô
            // ComboBox đang mở vẫn giữ giá trị cũ và đè lại nhãn vừa gán.
            if (grid.IsCurrentCellInEditMode)
            {
                grid.CancelEdit();
                grid.EndEdit();
            }

            if (compare != null)
            {
                SetBlockCompare(blocks, compare);
            }
            else
            {
                SetBlockEvaluation(blocks, value);
            }

            var current = grid.CurrentCell;
            if (blocks.Count == 1 && current != null && current.RowIndex + 1 < grid.Rows.Count)
            {
                grid.ClearSelection();
                grid.CurrentCell = grid.Rows[current.RowIndex + 1].Cells[current.ColumnIndex];
                grid.CurrentCell.Selected = true;
            }
            else if (blocks.Count > 1)
            {
                ShowStatusMessage(compare != null
                    ? $"Đã đặt Đối soát {(compare.Length == 0 ? "trống" : compare)} cho {SelectedRowCount()} hàng đang chọn."
                    : $"Đã gán {(value.Length == 0 ? "xoá nhãn" : value)} cho {SelectedRowCount()} hàng đang chọn.");
                StatusResetSoon();
            }

            return true;
        }

        /// <summary>
        /// Tab / Shift+Tab khi bảng đang focus: chọn ô Raw của block (hàng × DK) kế tiếp / trước đó CHƯA
        /// CÓ MASK - ảnh Raw chưa được UnitX xử lý (nền xám). Duyệt theo thứ tự đang hiển thị (sau lọc,
        /// sắp xếp) và chỉ trong các DK đang hiện. Hết danh sách thì quay vòng và báo trên thanh trạng thái.
        /// </summary>
        private bool HandleMissingMaskKey(Keys keyData)
        {
            if (keyData != Keys.Tab && keyData != (Keys.Shift | Keys.Tab))
            {
                return false;
            }

            if (chkUnmatchedOnly.Checked)
            {
                return false;
            }

            if (grid.IsCurrentCellInEditMode)
            {
                grid.EndEdit();
            }

            JumpToMissingMask(keyData == Keys.Tab);
            return true;
        }

        private void JumpToMissingMask(bool forward)
        {
            var visible = VisibleConditions();

            // Vị trí (hàng, thứ tự DK) của mọi block có Raw mà chưa có Mask, theo thứ tự hiển thị.
            var targets = new List<KeyValuePair<int, int>>();
            for (int r = 0; r < grid.Rows.Count; r++)
            {
                var row = RowOf(grid.Rows[r]);
                if (row == null || row.Unmatched != null)
                {
                    continue;
                }

                for (int c = 0; c < visible.Count; c++)
                {
                    var block = row.Block(visible[c]);
                    if (block != null && block.Raw != null && block.Mask == null)
                    {
                        targets.Add(new KeyValuePair<int, int>(r, c));
                    }
                }
            }

            if (targets.Count == 0)
            {
                ShowStatusMessage("✔ Đã duyệt hết: mọi mẫu đang hiển thị đều đã có Mask / kết quả.");
                StatusResetSoon();
                return;
            }

            int currentRow = grid.CurrentCell == null ? -1 : grid.CurrentCell.RowIndex;
            int currentBlock = -1;
            if (grid.CurrentCell != null)
            {
                var info = BlockOf(grid.Columns[grid.CurrentCell.ColumnIndex]);
                currentBlock = info == null ? -1 : visible.FindIndex(c => string.Equals(c, info.Condition, StringComparison.OrdinalIgnoreCase));
            }

            Func<KeyValuePair<int, int>, bool> after = p => p.Key > currentRow || (p.Key == currentRow && p.Value > currentBlock);
            Func<KeyValuePair<int, int>, bool> before = p => p.Key < currentRow || (p.Key == currentRow && p.Value < currentBlock);

            bool wrapped = false;
            KeyValuePair<int, int> target;
            if (forward)
            {
                var next = targets.Where(after).ToList();
                wrapped = next.Count == 0;
                target = wrapped ? targets[0] : next[0];
            }
            else
            {
                var previous = targets.Where(before).ToList();
                wrapped = previous.Count == 0;
                target = wrapped ? targets[targets.Count - 1] : previous[previous.Count - 1];
            }

            string condition = visible[target.Value];
            var column = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => BlockOf(c) != null
                && BlockOf(c).Part == BlockPart.Raw && string.Equals(BlockOf(c).Condition, condition, StringComparison.OrdinalIgnoreCase));
            if (column == null)
            {
                return;
            }

            grid.ClearSelection();
            grid.CurrentCell = grid.Rows[target.Key].Cells[column.Index];
            grid.CurrentCell.Selected = true;

            var image = RowOf(grid.Rows[target.Key]).Block(condition).Raw;
            int position = targets.IndexOf(target) + 1;
            ShowStatusMessage((wrapped ? "↻ Đã quay lại " + (forward ? "đầu" : "cuối") + " danh sách - " : string.Empty)
                + $"Chưa có Mask {position}/{targets.Count}: {image.FileName}"
                + (_schema.ConditionField != null ? "  (" + condition + ")" : string.Empty));
            StatusResetSoon();
        }

        private Timer _statusResetTimer;

        /// <summary>Thông báo tạm trên thanh trạng thái, 2,5 giây sau quay về phần tóm tắt.</summary>
        private void StatusResetSoon()
        {
            if (_statusResetTimer == null)
            {
                _statusResetTimer = new Timer { Interval = 2500 };
                _statusResetTimer.Tick += (s, e) => { _statusResetTimer.Stop(); UpdateSummary(); };
            }

            _statusResetTimer.Stop();
            _statusResetTimer.Start();
        }

        /// <summary>
        /// Gán nhãn cho các ảnh được chọn rồi lưu ngay. Nhãn rỗng = xoá nhãn thủ công, quay về kết
        /// quả UnitX.
        ///
        /// Lan truyền MỘT CHIỀU: gán cho ảnh Mask thì ảnh Raw cùng mẫu (cùng Model/Condition/SPL/hậu tố)
        /// nhận cùng nhãn - mask là ảnh người dùng nhìn để đánh giá. Gán cho ảnh Raw thì chỉ đổi ảnh
        /// Raw đó, không đụng tới Mask.
        /// </summary>
        private void SetEvaluation(IEnumerable<DatasetImage> images, string value)
        {
            var changed = new HashSet<DatasetImage>();
            var targets = new List<DatasetImage>();

            foreach (var image in images)
            {
                if (image == null)
                {
                    continue;
                }

                targets.Add(image);

                if (image.Matched && image.Type == "Mask")
                {
                    targets.AddRange(_images.Where(i => i.Matched && i.Type == "Raw"
                        && string.Equals(i.PairKey, image.PairKey, StringComparison.OrdinalIgnoreCase)));
                }
            }

            foreach (var image in targets)
            {
                if (image == null || image.EvalKey == null)
                {
                    continue;
                }

                changed.Add(image);

                if (string.IsNullOrEmpty(value))
                {
                    _evaluations.Remove(image.EvalKey);
                }
                else
                {
                    _evaluations[image.EvalKey] = value;
                }
            }

            if (changed.Count == 0)
            {
                return;
            }

            _fillingGrid = true;
            try
            {
                foreach (DataGridViewRow gridRow in grid.Rows)
                {
                    var row = RowOf(gridRow);
                    if (row != null && row.Images.Any(changed.Contains))
                    {
                        SetRowValues(gridRow, row);
                    }
                }
            }
            finally
            {
                _fillingGrid = false;
            }

            // Ô đang mở sửa (ComboBox) giữ giá trị cũ trong ô soạn - nạp lại cho khớp nhãn vừa gán.
            if (grid.IsCurrentCellInEditMode)
            {
                grid.RefreshEdit();
            }

            SaveEvaluations();
            ScheduleSummaryRefresh();
        }

        /// <summary>Gõ phím trong ô Lọc: đợi ngừng gõ 250 ms rồi mới lọc (thư mục vài nghìn ảnh).</summary>
        private void ApplyFilter()
        {
            _filterTimer.Stop();
            _filterTimer.Start();
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            _filterTimer.Stop();
            FillGrid();
        }

        /// <summary>
        /// Điều kiện lọc hiện tại. Ô Lọc tìm trong MỌI cột (tên file, các trường schema, hậu tố, loại,
        /// kết quả UnitX, nhãn đánh giá, thư mục); nhiều từ cách nhau bởi dấu cách = phải khớp TẤT CẢ.
        /// Ví dụ: "DK1 X" = ảnh điều kiện DK1 đang bị đánh giá X.
        /// </summary>
        #region Biểu đồ thống kê

        private static readonly string[] ChartCategories = { "O", "Δ", "X", "Chưa có" };

        private static readonly Color[] ChartCategoryColors =
        {
            Color.FromArgb(22, 163, 74), Color.FromArgb(217, 119, 6), Color.FromArgb(220, 38, 38), Color.FromArgb(156, 163, 175)
        };

        /// <summary>
        /// Biểu đồ cột: số lượng + % O / Δ / X / chưa có nhãn của các ảnh ĐANG HIỂN THỊ trong bảng
        /// (theo mọi bộ lọc). Hai nhóm cột: "UnitX" = kết quả trong tên file, "Evaluation" = nhãn hiện
        /// tại (sau khi người dùng sửa) - nhìn vào là thấy người dùng đã sửa kết quả UnitX bao nhiêu.
        /// </summary>
        private void SetupChart()
        {

            var area = new System.Windows.Forms.DataVisualization.Charting.ChartArea("main") { BackColor = Color.White };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            area.AxisX.LineColor = Color.FromArgb(209, 213, 219);
            area.AxisX.MajorTickMark.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(243, 244, 246);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LineColor = Color.FromArgb(209, 213, 219);
            area.AxisY.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
            chartStats.ChartAreas.Add(area);

            var legend = new System.Windows.Forms.DataVisualization.Charting.Legend("legend")
            {
                Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top,
                Alignment = StringAlignment.Far,
                Font = new Font("Segoe UI", 8.5F)
            };
            chartStats.Legends.Add(legend);

            chartStats.Titles.Add(new System.Windows.Forms.DataVisualization.Charting.Title
            {
                Name = "title",
                Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top,
                Alignment = ContentAlignment.TopLeft,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Text = "Thống kê đánh giá"
            });

            foreach (var name in new[] { "UnitX", "Evaluation" })
            {
                var series = new System.Windows.Forms.DataVisualization.Charting.Series(name)
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    ChartArea = "main",
                    Legend = "legend",
                    IsValueShownAsLabel = false,   // nhãn "n / x%" đặt riêng từng cột, cột 0 thì bỏ trống
                    Font = new Font("Segoe UI", 8F),
                    LabelForeColor = Color.FromArgb(55, 65, 81)
                };
                series["PointWidth"] = "0.8";
                series.SmartLabelStyle.Enabled = false;   // nhãn luôn nằm ngay trên cột của nó
                chartStats.Series.Add(series);
            }

            // Màu = LOẠI nhãn (O xanh lá, Δ cam, X đỏ, chưa có xám) - trùng màu chữ trên trục X.
            // Kiểu tô = NGUỒN: UnitX kẻ sọc, Evaluation tô đặc. Ô chú thích vẽ riêng đúng hai kiểu tô
            // đó (màu trung tính) thay cho ô tự sinh theo màu series vốn không khớp màu từng cột.
            foreach (var series in chartStats.Series)
            {
                series.IsVisibleInLegend = false;
            }

            var neutral = Color.FromArgb(107, 114, 128);
            legend.CustomItems.Add(new System.Windows.Forms.DataVisualization.Charting.LegendItem
            {
                Name = "Vision (kết quả máy)",
                Color = neutral,
                BackHatchStyle = UnitXHatch,
                BackSecondaryColor = Color.White,
                BorderColor = neutral
            });
            legend.CustomItems.Add(new System.Windows.Forms.DataVisualization.Charting.LegendItem
            {
                Name = "Đánh giá (nhãn cuối)",
                Color = neutral,
                BorderColor = neutral
            });

            for (int i = 0; i < ChartCategories.Length; i++)
            {
                area.AxisX.CustomLabels.Add(new System.Windows.Forms.DataVisualization.Charting.CustomLabel(
                    i + 0.5, i + 1.5, ChartCategories[i], 0, System.Windows.Forms.DataVisualization.Charting.LabelMarkStyle.None)
                {
                    ForeColor = ChartCategoryColors[i]
                });
            }
        }

        private const System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle UnitXHatch =
            System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle.WideUpwardDiagonal;

        private void UpdateChart()
        {
            // Đếm theo MẪU = mỗi block (hàng × DK) đang hiển thị và có ảnh.
            var shown = ShownBlocks();
            int total = shown.Count;

            var unitX = new int[ChartCategories.Length];
            var evaluation = new int[ChartCategories.Length];
            foreach (var block in shown)
            {
                unitX[CategoryIndex(BlockUnitX(block))]++;
                evaluation[CategoryIndex(BlockEvaluation(block))]++;
            }

            chartStats.Titles["title"].Text = total == 0
                ? "Thống kê đánh giá (chưa có mẫu)"
                : $"Thống kê {total} mẫu đang hiển thị";

            // Chừa khoảng trống phía trên cho nhãn 2 dòng.
            int max = Math.Max(unitX.Max(), evaluation.Max());
            chartStats.ChartAreas["main"].AxisY.Maximum = Math.Max(2, Math.Ceiling(max * 1.35));

            FillSeries(chartStats.Series["UnitX"], unitX, total, false);
            FillSeries(chartStats.Series["Evaluation"], evaluation, total, true);
        }

        private void FillSeries(System.Windows.Forms.DataVisualization.Charting.Series series, int[] counts, int total, bool solid)
        {
            series.Points.Clear();
            for (int i = 0; i < ChartCategories.Length; i++)
            {
                int index = series.Points.AddXY(ChartCategories[i], counts[i]);
                var point = series.Points[index];
                double percent = total == 0 ? 0 : counts[i] * 100.0 / total;
                // Biểu đồ nhỏ trong thẻ Thống kê: số / % đã có trên các thẻ O / Δ / X / Chưa có - không ghi đè lên cột.
                point.Label = counts[i] == 0 || chartStats.Height < 200 ? string.Empty : $"{counts[i]}\n{percent:0.#}%";
                point.ToolTip = $"{series.Name} - {ChartCategories[i]}: {counts[i]} mẫu ({percent:0.#}%)";
                point.Color = ChartCategoryColors[i];
                point.BorderColor = ChartCategoryColors[i];
                point.BorderWidth = 1;
                if (!solid)
                {
                    point.BackHatchStyle = UnitXHatch;
                    point.BackSecondaryColor = Color.White;
                }
            }
        }

        private static int CategoryIndex(string label)
        {
            switch (label)
            {
                case "O": return 0;
                case "Δ": return 1;
                case "X": return 2;
                default: return 3;
            }
        }

        #endregion

        /// <summary>Giá trị cần khớp của ô lọc UnitX / Đánh giá: null = Tất cả, "" = (trống), còn lại O/Δ/X.</summary>
        private static string LabelFilterValue(ComboBox combo)
        {
            string choice = combo.SelectedItem as string;
            if (combo.SelectedIndex <= 0 || choice == null)
            {
                return null;
            }

            return choice == "(trống)" ? string.Empty : choice;
        }

        /// <summary>
        /// Điều kiện lọc hàng. Ô lọc DK / UnitX / Evaluation và ô Lọc chữ xét TỪNG BLOCK: hàng được giữ
        /// khi có ít nhất một block đang hiển thị thoả tất cả (vd. "DK1 X" = hàng có block DK1 bị đánh
        /// giá X). Bộ lọc tiêu đề cột xét đúng giá trị ô của cột đó.
        /// </summary>
        private Func<DatasetRow, bool> BuildFilter()
        {
            string[] terms = txtFilter.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string unitX = LabelFilterValue(cboUnitXFilter);
            string evalFilter = LabelFilterValue(cboEvalFilter);
            var visible = VisibleConditions();

            // Chụp lại bộ lọc cột NGAY LÚC NÀY: hàm lọc chạy trễ (Where lười) - nếu đọc thẳng _columnFilters
            // thì ShowColumnFilter bỏ tạm bộ lọc của cột đang mở rồi gắn lại vẫn bị tính (lỗi 2026-09-26:
            // mở lại hộp lọc chỉ còn đúng giá trị vừa chọn).
            var columnFilters = _columnFilters.ToList();
            var treeFilter = _treeKeyFilter.ToList();

            return row =>
            {
                // Lọc theo nút đang chọn trên cây dataset.
                if (row.Unmatched == null && treeFilter.Any(kv => !string.Equals(ValueOf(row, kv.Key), kv.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Lọc theo tiêu đề cột (tick giá trị trong nút ▼ của từng cột).
                foreach (var columnFilter in columnFilters)
                {
                    if (!columnFilter.Value.Contains(ColumnValue(row, columnFilter.Key)))
                    {
                        return false;
                    }
                }

                if (row.Unmatched != null)
                {
                    var image = row.Unmatched;
                    return terms.All(term => image.FileName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                        || image.RelativeDir.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                foreach (var condition in visible)
                {
                    var block = row.Block(condition);
                    if (block == null)
                    {
                        continue;
                    }

                    string blockUnitX = BlockUnitX(block);
                    string evaluation = BlockEvaluation(block);

                    if (unitX != null && !string.Equals(blockUnitX, unitX, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (evalFilter != null && !string.Equals(evaluation, evalFilter, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var fields = new List<string> { row.Suffix, condition, blockUnitX, evaluation, BlockCompare(block) };
                    fields.AddRange(row.Values.Values);
                    fields.AddRange(block.Images.SelectMany(i => new[] { i.FileName, i.RelativeDir }));

                    // Gõ đúng một ký hiệu nhãn (O / X / Δ) thì so khớp CHÍNH XÁC với Evaluation, nếu
                    // không "O" sẽ khớp mọi tên file có chữ o.
                    if (terms.All(term =>
                        (term == "O" || term == "X" || term == "Δ" || term == "o" || term == "x")
                            ? string.Equals(evaluation, term.ToUpperInvariant(), StringComparison.Ordinal)
                            : fields.Any(f => f != null && f.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        return true;
                    }
                }

                return false;
            };
        }

        /// <summary>Các block (hàng × DK) đang hiển thị trong bảng và có ảnh.</summary>
        private List<DatasetBlock> ShownBlocks()
        {
            var visible = VisibleConditions();
            return grid.Rows.Cast<DataGridViewRow>()
                .Select(RowOf)
                .Where(r => r != null && r.Unmatched == null)
                .SelectMany(r => visible.Select(r.Block))
                .Where(b => b != null)
                .ToList();
        }

        private void UpdateSummary()
        {
            UpdateChart();
            RefreshIssues();
            ScheduleDatasetJson();
            UpdateFilterState();   // số dòng / kết quả, vị trí Trước-Sau, thẻ thống kê O / Δ / X / Chưa có

            if (_images.Count == 0)
            {
                ShowStatusMessage(_loadedFolder == null
                    ? "Chọn thư mục ảnh rồi bấm 'Quét thư mục'."
                    : "Thư mục không có ảnh nào.");
                return;
            }

            var matched = _images.Where(i => i.Matched).ToList();
            var pairs = _rows.SelectMany(r => r.Blocks.Values).ToList();

            int o = 0, delta = 0, x = 0, none = 0;
            foreach (var pair in pairs)
            {
                switch (BlockEvaluation(pair))
                {
                    case "O": o++; break;
                    case "Δ": delta++; break;
                    case "X": x++; break;
                    default: none++; break;
                }
            }

            int visible = chkUnmatchedOnly.Checked
                ? grid.Rows.Count
                : ShownBlocks().Sum(b => b.Images.Count());

            int unmatched = _images.Count - matched.Count;

            statusBar.SuspendLayout();
            stImages.Text = $"{visible}/{_images.Count}";
            stRaw.Text = matched.Count(i => i.Type == "Raw").ToString();
            stMask.Text = matched.Count(i => i.Type == "Mask").ToString();
            // Schema bỏ qua phần đuôi: ảnh chỉ khác phần đuôi với ảnh đã lấy (cùng mẫu) không đưa vào bảng.
            int skipped = _images.Count(i => i.Skipped);
            // Quá nhiều giá trị ĐK (mỗi ĐK một khối cột): gần như chắc chắn schema đặt nhầm vị trí <ĐK>
            // (vd schema tên file áp cho dataset chia thư mục -> ĐK đọc ra No).
            bool tooManyConditions = _conditions.Count > MaxReasonableConditions;
            stSchema.Text = $"⚠ Lỗi schema: {unmatched}" + (skipped > 0 ? $" · bỏ qua {skipped} ảnh trùng mẫu" : string.Empty)
                + (tooManyConditions ? $" · {_conditions.Count} giá trị ĐK - kiểm tra lại vị trí <ĐK> trong schema" : string.Empty);
            stSchema.ForeColor = unmatched > 0 || tooManyConditions ? StatusError : StatusText;
            stSchema.Font = unmatched > 0 || tooManyConditions ? _statusBold : statusBar.Font;
            stEvalCaption.Text = $"Đánh giá ({pairs.Count} mẫu):";
            stO.Text = $"O:{o}";
            stDelta.Text = $"Δ:{delta}";
            stX.Text = $"X:{x}";
            stNone.Text = $"Trống:{none}";
            statusBar.ResumeLayout();
            ShowStatusSummary();
        }

        /// <summary>Nhãn xấu nhất trong danh sách: X, rồi Δ, rồi O; không có nhãn nào thì "".</summary>
        private static string WorstLabel(IEnumerable<string> labels)
        {
            var values = labels.ToList();
            if (values.Contains("X")) return "X";
            if (values.Contains("Δ")) return "Δ";
            if (values.Contains("O")) return "O";
            return string.Empty;
        }

        #endregion

        #region Lọc theo tiêu đề cột

        /// <summary>Bề rộng vùng nút lọc ▼ ở mép phải tiêu đề cột (px).</summary>
        private const int FilterGlyphWidth = 18;

        /// <summary>Tên cột -> các giá trị được tick. Cột không có trong đây = không lọc.</summary>
        private readonly Dictionary<string, HashSet<string>> _columnFilters =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        private string _sortColumn;
        private System.ComponentModel.ListSortDirection _sortDirection = System.ComponentModel.ListSortDirection.Ascending;

        private const string BlankValue = "(trống)";

        private static bool IsFilterableColumn(DataGridViewColumn column)
        {
            return column.Name != "colName";
        }

        private void SetupColumnFilters()
        {
            grid.CellPainting += Grid_HeaderPainting;
            grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;
            grid.CellMouseMove += (s, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    grid.InvalidateCell(e.ColumnIndex, -1);
                }
            };
        }

        /// <summary>Giá trị hiển thị của một hàng ở một cột - dùng chung cho bảng, bộ lọc cột và sắp xếp.</summary>
        private string ColumnValue(DatasetRow row, string columnName)
        {
            string value = null;

            if (row.Unmatched != null)
            {
                switch (columnName)
                {
                    case "colName": value = row.Unmatched.FileName; break;
                    case "colDir": value = row.Unmatched.RelativeDir; break;
                }
            }
            else if (columnName == "colSuffix")
            {
                value = row.Suffix;
            }
            else if (columnName.StartsWith("key_", StringComparison.Ordinal))
            {
                row.Values.TryGetValue(columnName.Substring(4), out value);
            }
            else if (grid.Columns.Contains(columnName))
            {
                var info = BlockOf(grid.Columns[columnName]);
                var block = info == null ? null : row.Block(info.Condition);
                if (block != null)
                {
                    switch (info.Part)
                    {
                        case BlockPart.Raw: value = ShotMark(block, false); break;
                        case BlockPart.Mask: value = ShotMark(block, true); break;
                        case BlockPart.UnitX: value = BlockUnitX(block); break;
                        case BlockPart.Evaluation: value = BlockEvaluation(block); break;
                        case BlockPart.Compare: value = BlockCompare(block); break;
                    }
                }
            }

            return string.IsNullOrEmpty(value) ? BlankValue : value;
        }

        /// <summary>Chiều cao tầng trên (tên DK) của tiêu đề hai tầng.</summary>
        private const int BandHeight = 20;

        private static readonly Color BlockSeparatorColor = Color.FromArgb(148, 163, 184);
        // Tầng tên DK: vùng kết quả (UnitX | Evaluation) tông xanh dương, vùng ảnh (Raw | Mask) tông tím;
        // hai DK kề nhau đậm nhạt xen kẽ.
        private static readonly Color[] ResultBandColors = { Color.FromArgb(219, 234, 254), Color.FromArgb(239, 246, 255) };
        private static readonly Color[] ImageBandColors = { Color.FromArgb(237, 233, 254), Color.FromArgb(245, 243, 255) };

        /// <summary>Vùng tên cột: tiêu đề hai tầng thì cột của block chỉ dùng tầng dưới.</summary>
        private Rectangle HeaderLabelBounds(DataGridViewColumn column, Rectangle cell)
        {
            return HasBlockHeader
                ? new Rectangle(cell.X, cell.Y + BandHeight, cell.Width, cell.Height - BandHeight)
                : cell;
        }

        private Rectangle GlyphBounds(DataGridViewColumn column, Rectangle cell)
        {
            var area = HeaderLabelBounds(column, cell);
            return new Rectangle(area.Right - FilterGlyphWidth - 2, area.Top + (area.Height - 16) / 2, FilterGlyphWidth, 16);
        }

        /// <summary>Cột nằm ở mép phải một nhóm (cuối block DK, hoặc cột cuối trước block đầu tiên).</summary>
        private bool IsBlockBoundary(DataGridViewColumn column)
        {
            var next = grid.Columns.GetNextColumn(column, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
            var info = BlockOf(column);
            var nextInfo = next == null ? null : BlockOf(next);

            if (info == null)
            {
                return nextInfo != null;
            }

            return nextInfo == null || nextInfo.GroupIndex != info.GroupIndex;
        }

        /// <summary>
        /// Vẽ ô: đường phân cách đậm giữa các block DK; tiêu đề hai tầng (tên DK gộp trên các cột của
        /// block); nút lọc ở mép phải tiêu đề: ▼ xám khi chưa lọc, nền xanh + phễu khi cột đang được lọc.
        /// </summary>
        private void Grid_HeaderPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }

            var column = grid.Columns[e.ColumnIndex];
            bool boundary = IsBlockBoundary(column);

            if (e.RowIndex >= 0)
            {
                // Ô Raw / Mask của ảnh có ghi chú: tam giác cam ở góc trên phải (giống ô có comment trong Excel).
                bool comment = HasCommentMark(e.RowIndex, e.ColumnIndex);
                bool selectedRow = _selectedRowSet.Contains(e.RowIndex);

                // Vision / Đánh giá / Đối soát: badge có chữ (O / Δ / X, == / !=) thay cho tô kín cả ô.
                bool badge = IsBadgeColumn(column) && PaintBadgeCell(e, column);
                if (!badge && !boundary && !comment && !_locked && !selectedRow)
                {
                    return;
                }

                if (!badge)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                }
                if (comment)
                {
                    var r = e.CellBounds;
                    using (var brush = new SolidBrush(Theme.Fore(CommentMarkColor)))
                    {
                        e.Graphics.FillPolygon(brush, new[]
                        {
                            new PointF(r.Right - 10, r.Top + 1), new PointF(r.Right - 2, r.Top + 1), new PointF(r.Right - 2, r.Top + 9)
                        });
                    }
                }

                if (_locked)
                {
                    PaintLockOverlay(e.Graphics, e.CellBounds);
                }

                // Hàng đang chọn: viền xanh trên + dưới chạy suốt hàng (các ô Vision / Evaluation vẫn giữ màu O / Δ / X).
                if (selectedRow)
                {
                    using (var pen = new Pen(Theme.Fore(Primary), 2))
                    {
                        var r = e.CellBounds;
                        e.Graphics.DrawLine(pen, r.Left, r.Top + 1, r.Right, r.Top + 1);
                        e.Graphics.DrawLine(pen, r.Left, r.Bottom - 2, r.Right, r.Bottom - 2);
                    }
                }

                if (boundary)
                {
                    using (var pen = new Pen(Theme.Fore(BlockSeparatorColor), 2))
                    {
                        e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                    }
                }

                e.Handled = true;
                return;
            }

            if (!IsFilterableColumn(column))
            {
                return;
            }

            var info = BlockOf(column);
            if (HasBlockHeader)
            {
                PaintBlockHeader(e, column, info);
            }
            else
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            }

            if (boundary)
            {
                using (var pen = new Pen(Theme.Fore(BlockSeparatorColor), 2))
                {
                    e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                }
            }

            var glyph = GlyphBounds(column, e.CellBounds);
            bool active = _columnFilters.ContainsKey(grid.Columns[e.ColumnIndex].Name);
            bool hover = glyph.Contains(grid.PointToClient(Cursor.Position));
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (active || hover)
            {
                using (var back = new SolidBrush(Theme.Back(active ? Color.FromArgb(219, 234, 254) : Color.FromArgb(229, 231, 235))))
                {
                    g.FillRectangle(back, glyph);
                }
            }

            int cx = glyph.X + glyph.Width / 2;
            int cy = glyph.Y + glyph.Height / 2;
            Color color = Theme.Fore(active ? Primary : Color.FromArgb(107, 114, 128));

            using (var brush = new SolidBrush(color))
            {
                if (active)
                {
                    // Phễu lọc.
                    g.FillPolygon(brush, new[]
                    {
                        new PointF(cx - 5, cy - 4), new PointF(cx + 5, cy - 4), new PointF(cx + 1, cy + 1),
                        new PointF(cx + 1, cy + 5), new PointF(cx - 1, cy + 4), new PointF(cx - 1, cy + 1)
                    });
                }
                else
                {
                    g.FillPolygon(brush, new[] { new PointF(cx - 4, cy - 2), new PointF(cx + 4, cy - 2), new PointF(cx, cy + 3) });
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Tiêu đề cột của block khi có trường DK: tầng trên tô màu theo block, tên DK căn giữa trên CẢ
        /// block (mỗi cột vẽ phần của mình, cắt theo ô); tầng dưới là tên cột + mũi tên sắp xếp.
        /// </summary>
        private void PaintBlockHeader(DataGridViewCellPaintingEventArgs e, DataGridViewColumn column, BlockColumn info)
        {
            var g = e.Graphics;
            var cell = e.CellBounds;
            e.PaintBackground(cell, false);

            // info == null: cột thông tin (trường Key / hậu tố) - tầng trên "THÔNG TIN" màu trung tính.
            var band = new Rectangle(cell.X, cell.Y, cell.Width, BandHeight);
            bool imagePart = info != null && (info.Part == BlockPart.Raw || info.Part == BlockPart.Mask);
            var bandColors = imagePart ? ImageBandColors : ResultBandColors;
            int group = info == null ? -1 : info.GroupIndex;
            using (var brush = new SolidBrush(Theme.Back(info == null ? Color.FromArgb(241, 245, 249) : bandColors[group % bandColors.Length])))
            {
                g.FillRectangle(brush, band);
            }

            using (var pen = new Pen(Theme.Back(Color.FromArgb(203, 213, 225))))
            {
                g.DrawLine(pen, band.Left, band.Bottom - 1, band.Right, band.Bottom - 1);
            }

            // Vùng của cả block = ô này nới sang trái / phải bằng các cột cùng block đang hiển thị.
            int left = cell.Left;
            int right = cell.Right;
            Func<DataGridViewColumn, int> groupOf = c => BlockOf(c) == null ? -1 : BlockOf(c).GroupIndex;
            for (var c = grid.Columns.GetPreviousColumn(column, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                 c != null && groupOf(c) == group;
                 c = grid.Columns.GetPreviousColumn(c, DataGridViewElementStates.Visible, DataGridViewElementStates.None))
            {
                left -= c.Width;
            }

            for (var c = grid.Columns.GetNextColumn(column, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                 c != null && groupOf(c) == group;
                 c = grid.Columns.GetNextColumn(c, DataGridViewElementStates.Visible, DataGridViewElementStates.None))
            {
                right += c.Width;
            }

            string bandText = info == null ? "THÔNG TIN"
                : (imagePart ? "TỆP" : "KẾT QUẢ") + (string.IsNullOrEmpty(info.Condition) ? string.Empty : "  ·  " + info.Condition);
            Color bandFore = info == null ? Color.FromArgb(71, 85, 105) : imagePart ? Color.FromArgb(91, 33, 182) : Color.FromArgb(30, 64, 175);

            var headerFont = grid.ColumnHeadersDefaultCellStyle.Font;
            var state = g.Save();
            g.SetClip(band);
            using (var bandFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, bandText, bandFont, new Rectangle(left, band.Top, right - left, BandHeight),
                    Theme.Fore(bandFore),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }

            g.Restore(state);

            var label = HeaderLabelBounds(column, cell);
            label.Width -= FilterGlyphWidth;
            string text = column.HeaderText;
            if (column.HeaderCell.SortGlyphDirection == SortOrder.Ascending)
            {
                text += " ▲";
            }
            else if (column.HeaderCell.SortGlyphDirection == SortOrder.Descending)
            {
                text += " ▼";
            }

            TextRenderer.DrawText(g, text, headerFont, label, Theme.Fore(Color.FromArgb(17, 24, 39)),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        /// <summary>Bấm vào nút ▼ -> mở bộ lọc cột; bấm chỗ khác trên tiêu đề -> sắp xếp.</summary>
        private void Grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.ColumnIndex < 0)
            {
                return;
            }

            var column = grid.Columns[e.ColumnIndex];
            var cell = grid.GetCellDisplayRectangle(e.ColumnIndex, -1, false);
            var clickPoint = new System.Drawing.Point(cell.X + e.X, cell.Y + e.Y);

            if (IsFilterableColumn(column) && GlyphBounds(column, cell).Contains(clickPoint))
            {
                ShowColumnFilter(column, cell);
                return;
            }

            _sortDirection = _sortColumn == column.Name && _sortDirection == System.ComponentModel.ListSortDirection.Ascending
                ? System.ComponentModel.ListSortDirection.Descending
                : System.ComponentModel.ListSortDirection.Ascending;
            _sortColumn = column.Name;
            ApplySort();
        }

        /// <summary>Sắp xếp theo cột người dùng đã chọn (chế độ Programmatic: tự vẽ mũi tên sắp xếp).</summary>
        private void ApplySort()
        {
            if (_sortColumn == null || !grid.Columns.Contains(_sortColumn))
            {
                return;
            }

            var column = grid.Columns[_sortColumn];
            grid.Sort(column, _sortDirection);

            foreach (DataGridViewColumn other in grid.Columns)
            {
                other.HeaderCell.SortGlyphDirection = other == column
                    ? (_sortDirection == System.ComponentModel.ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending)
                    : SortOrder.None;
            }
        }

        private void ShowColumnFilter(DataGridViewColumn column, Rectangle headerCell)
        {
            // Giá trị có thể chọn = giá trị của các ảnh qua được MỌI bộ lọc KHÁC (giống Excel), để
            // danh sách không liệt kê giá trị đã bị bộ lọc cột khác loại hết.
            HashSet<string> current;
            _columnFilters.TryGetValue(column.Name, out current);
            _columnFilters.Remove(column.Name);
            var others = BuildFilter();
            if (current != null)
            {
                _columnFilters[column.Name] = current;
            }

            var counts = (chkUnmatchedOnly.Checked ? _unmatchedRows : _rows).Where(others)
                .GroupBy(r => ColumnValue(r, column.Name))
                .ToDictionary(g => g.Key, g => g.Count());

            // Giá trị đang tick nhưng hiện không còn ảnh nào vẫn giữ trong danh sách (0 ảnh).
            if (current != null)
            {
                foreach (var value in current.Where(v => !counts.ContainsKey(v)))
                {
                    counts[value] = 0;
                }
            }

            var values = counts.Keys
                .OrderBy(v => v == BlankValue ? 1 : 0)
                .ThenBy(v => v, NaturalComparer.Instance)
                .ToList();

            // Cột của block DK: ghi kèm DK để biết đang lọc Evaluation của DK nào.
            var info = BlockOf(column);
            string title = info != null && _schema.ConditionField != null
                ? column.HeaderText + " - " + info.Condition
                : column.HeaderText;
            var popup = new ColumnFilterPopup(title, values, counts, current);
            popup.Applied += selected =>
            {
                if (selected == null)
                {
                    _columnFilters.Remove(column.Name);
                }
                else
                {
                    _columnFilters[column.Name] = selected;
                }

                FillGrid();

                // Vẽ lại hàng tiêu đề (biểu tượng phễu). KHÔNG dùng InvalidateRow(-1): DataGridView không
                // nhận chỉ số -1 và ném ArgumentOutOfRangeException (lỗi 2026-09-26).
                grid.Invalidate();
            };

            // Giải phóng hộp lọc sau khi đóng - hoãn một nhịp để các lệnh BeginInvoke còn chờ của nó
            // chạy xong trước.
            popup.Closed += (s, e) => grid.BeginInvoke((Action)popup.Dispose);

            popup.Show(grid, new System.Drawing.Point(Math.Max(0, headerCell.Right - popup.Width), headerCell.Bottom));
        }

        /// <summary>
        /// Hộp lọc của một cột (kiểu Excel): ô tìm, "Chọn tất cả", danh sách giá trị kèm số ảnh, OK /
        /// Huỷ / Bỏ lọc. Applied(null) = bỏ lọc cột này.
        /// </summary>
        private sealed class ColumnFilterPopup : ToolStripDropDown
        {
            public event Action<HashSet<string>> Applied;

            private readonly List<string> _values;
            private readonly Dictionary<string, int> _counts;
            private readonly HashSet<string> _checked;
            private readonly TextBox _search = new TextBox();
            private readonly CheckBox _all = new CheckBox();
            private readonly CheckedListBox _list = new CheckedListBox();
            private bool _updating;

            public ColumnFilterPopup(string title, List<string> values, Dictionary<string, int> counts, HashSet<string> current)
            {
                _values = values;
                _counts = counts;
                _checked = current != null ? new HashSet<string>(current) : new HashSet<string>(values);

                AutoSize = false;
                Padding = new Padding(1);
                Size = new Size(260, 340);

                var panel = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 5,
                    Size = new Size(258, 338),
                    Padding = new Padding(8),
                    BackColor = Color.White,
                    Font = new Font("Segoe UI", 9F)
                };
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

                var header = new Label
                {
                    Text = "Lọc cột: " + title,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                _search.Dock = DockStyle.Fill;
                _search.Margin = new Padding(0, 3, 0, 3);
                _search.TextChanged += (s, e) => RefreshList();

                _all.Text = "(Chọn tất cả)";
                _all.Dock = DockStyle.Fill;
                _all.CheckedChanged += (s, e) =>
                {
                    if (_updating)
                    {
                        return;
                    }

                    foreach (var value in VisibleValues())
                    {
                        if (_all.Checked) _checked.Add(value); else _checked.Remove(value);
                    }

                    RefreshList();
                };

                _list.Dock = DockStyle.Fill;
                _list.CheckOnClick = true;
                _list.BorderStyle = BorderStyle.FixedSingle;
                _list.IntegralHeight = false;
                _list.ItemCheck += (s, e) =>
                {
                    if (_updating)
                    {
                        return;
                    }

                    string value = ((FilterItem)_list.Items[e.Index]).Value;
                    if (e.NewValue == CheckState.Checked) _checked.Add(value); else _checked.Remove(value);
                    BeginInvoke((Action)UpdateAllBox);
                };

                var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 6, 0, 0) };
                var ok = new Button { Text = "OK", Width = 70, Height = 28, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                ok.FlatAppearance.BorderSize = 0;
                var cancel = new Button { Text = "Huỷ", Width = 60, Height = 28, FlatStyle = FlatStyle.Flat };
                var clear = new Button { Text = "Bỏ lọc", Width = 70, Height = 28, FlatStyle = FlatStyle.Flat };
                ok.Click += (s, e) =>
                {
                    Close();
                    if (Applied != null) Applied(_checked.Count == _values.Count ? null : new HashSet<string>(_checked));
                };
                cancel.Click += (s, e) => Close();
                clear.Click += (s, e) =>
                {
                    Close();
                    if (Applied != null) Applied(null);
                };
                buttons.Controls.AddRange(new Control[] { ok, cancel, clear });

                panel.Controls.Add(header, 0, 0);
                panel.Controls.Add(_search, 0, 1);
                panel.Controls.Add(_all, 0, 2);
                panel.Controls.Add(_list, 0, 3);
                panel.Controls.Add(buttons, 0, 4);

                Items.Add(new ToolStripControlHost(panel) { AutoSize = false, Size = panel.Size, Margin = Padding.Empty, Padding = Padding.Empty });
                Theme.Apply(panel);
                Theme.ApplyStrip(this);

                RefreshList();
                Opened += (s, e) => _search.Focus();
            }

            private IEnumerable<string> VisibleValues()
            {
                string term = _search.Text.Trim();
                return term.Length == 0
                    ? _values
                    : _values.Where(v => v.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            private void RefreshList()
            {
                _updating = true;
                _list.BeginUpdate();
                _list.Items.Clear();
                foreach (var value in VisibleValues())
                {
                    _list.Items.Add(new FilterItem { Value = value, Count = _counts[value] }, _checked.Contains(value));
                }

                _list.EndUpdate();
                _updating = false;
                UpdateAllBox();
            }

            private void UpdateAllBox()
            {
                if (IsDisposed || _all.IsDisposed)
                {
                    return;
                }

                _updating = true;
                var visible = VisibleValues().ToList();
                _all.CheckState = visible.Count > 0 && visible.All(_checked.Contains) ? CheckState.Checked
                    : visible.Any(_checked.Contains) ? CheckState.Indeterminate : CheckState.Unchecked;
                _updating = false;
            }

            private sealed class FilterItem
            {
                public string Value;
                public int Count;

                public override string ToString()
                {
                    return Value + "   (" + Count + ")";
                }
            }
        }

        #endregion

        #region Menu chuột phải (xoá, đổi tên, copy, thuộc tính...)

        private ImageListMenu _fileMenu;

        private void SetupFileMenu()
        {
            _fileMenu = new ImageListMenu(grid)
            {
                SelectedPaths = () => SelectedImages().Select(i => i.FullPath).ToList(),
                PairPath = path => PairOf(FindImage(path)) == null ? null : PairOf(FindImage(path)).FullPath,
                DescribeName = DescribeName,
                ExtraProperties = ExtraProperties,
                IsBusy = () => _loading || !btnExport.Enabled,
                IsLocked = () => _locked,
                LockedNotice = NotifyLocked,
                Deleted = OnImagesDeleted,
                Renamed = OnImagesRenamed,
                // Delete đang là phím xoá NHÃN - xoá file dùng Shift+Delete để không bấm nhầm.
                DeleteKey = Keys.Shift | Keys.Delete
            };

            var mnuViewPair = new ToolStripMenuItem("Xem ảnh cặp (Raw ↔ Mask)");
            mnuViewPair.Click += (s, e) => ViewPair();

            var mnuLabel = new ToolStripMenuItem("Gán nhãn Đánh giá");
            foreach (var choice in new[] { new[] { "O", "1" }, new[] { "Δ", "2" }, new[] { "X", "3" }, new[] { "", "0" } })
            {
                string value = choice[0];
                var item = new ToolStripMenuItem(value.Length == 0 ? "Xoá nhãn (về mặc định theo Vision)" : value)
                {
                    ShortcutKeyDisplayString = choice[1]
                };
                item.Click += (s, e) => SetBlockEvaluation(SelectedBlocks(), value);
                mnuLabel.DropDownItems.Add(item);
            }

            var mnuCompare = new ToolStripMenuItem("Đối soát");
            foreach (var choice in new[]
            {
                new[] { CompareMatch, "==  (Đánh giá giống Vision)" },
                new[] { CompareDiff, "!=  (Đánh giá ngược Vision)" },
                new[] { string.Empty, "Trống  (giữ Đánh giá hiện tại, không theo Vision)" },
                new[] { (string)null, "Theo logic chung" }
            })
            {
                string value = choice[0];
                var item = new ToolStripMenuItem(choice[1]);
                item.Click += (s, e) => SetBlockCompare(SelectedBlocks(), value);
                mnuCompare.DropDownItems.Add(item);
            }

            _fileMenu.AddHostItems(mnuViewPair, mnuLabel, mnuCompare);
            _fileMenu.Menu.Opening += (s, e) =>
            {
                var images = SelectedImages();
                int blocks = SelectedBlocks().Count;
                int rows = SelectedRowCount();
                mnuViewPair.Enabled = images.Count == 1 && PairOf(images[0]) != null;
                mnuLabel.Enabled = blocks > 0 && !_locked;
                mnuCompare.Enabled = blocks > 0 && !_locked;
                mnuLabel.Text = rows > 1 ? $"Gán nhãn Đánh giá ({rows} hàng)" : "Gán nhãn Đánh giá";
            };

            grid.ContextMenuStrip = _fileMenu.Menu;

            // Chuột phải lên một ô CHƯA chọn: chuyển vùng chọn sang ô đó (giống Explorer).
            grid.CellMouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return;
                }

                var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!cell.Selected)
                {
                    grid.ClearSelection();
                    grid.CurrentCell = cell;
                    cell.Selected = true;
                }
            };
        }

        /// <summary>
        /// Phím tắt của menu khi bảng đang focus. Đặt ở ProcessCmdKey vì DataGridView tự "ăn" Enter
        /// (xuống dòng) trước khi KeyDown kịp chạy. Bỏ qua khi đang gõ trong ô nhập chữ.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Esc: thu lại khung xem ảnh đang mở rộng.
            if (keyData == Keys.Escape && _previewExpanded)
            {
                SetPreviewExpanded(false);
                return true;
            }

            // Space trong bảng / khung xem trước: Raw ↔ Mask (không áp dụng khi đang gõ chữ).
            if (keyData == Keys.Space && (grid.ContainsFocus || picPreview.ContainsFocus) && !(ActiveControlDeep() is TextBox))
            {
                TogglePreviewPair();
                return true;
            }

            // , / . : lần chụp trước / sau (schema có <Lần chụp>).
            if ((keyData == Keys.Oemcomma || keyData == Keys.OemPeriod) && (grid.ContainsFocus || picPreview.ContainsFocus)
                && !(ActiveControlDeep() is TextBox) && ShiftShot(keyData == Keys.Oemcomma ? -1 : 1))
            {
                return true;
            }

            if (_fileMenu != null && grid.ContainsFocus && !(grid.EditingControl is TextBox)
                && _fileMenu.HandleKey(keyData))
            {
                return true;
            }

            // Đang khoá: nuốt phím gán nhãn / đối soát (không để lọt vào ô ComboBox) và báo.
            if (_locked && grid.ContainsFocus && !(grid.EditingControl is TextBox)
                && (EvaluationForKey(keyData) != null || CompareForKey(keyData) != null))
            {
                NotifyLocked();
                return true;
            }

            // Gán nhãn O / Δ / X cho mọi ô đang chọn. Xử lý ở đây (không ở KeyDown của bảng) để vẫn
            // nhận phím khi ô Evaluation đang ở chế độ sửa (ComboBox giữ focus).
            if (grid.ContainsFocus && !(grid.EditingControl is TextBox) && HandleEvaluationKey(keyData))
            {
                return true;
            }

            // Tab / Shift+Tab trong bảng: tới ảnh chưa có Mask kế tiếp / trước đó (thay cho chuyển ô).
            if ((grid.ContainsFocus || picPreview.ContainsFocus || picPreviewMask.ContainsFocus) && HandleMissingMaskKey(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Control đang có focus thật sự (đi xuống qua các container lồng nhau).</summary>
        private Control ActiveControlDeep()
        {
            Control active = ActiveControl;
            var container = active as ContainerControl;
            while (container != null && container.ActiveControl != null)
            {
                active = container.ActiveControl;
                container = active as ContainerControl;
            }

            return active;
        }

        /// <summary>
        /// Các ảnh đang chọn theo thứ tự hiển thị: ô Raw -> ảnh Raw, ô Mask / UnitX / Evaluation -> ảnh
        /// Mask (thiếu thì Raw), ô trường Key / hậu tố -> ảnh chính của mọi block đang hiển thị của hàng.
        /// Không chọn ô nào thì lấy ô hiện tại.
        /// </summary>
        private List<DatasetImage> SelectedImages()
        {
            var cells = grid.SelectedCells.Cast<DataGridViewCell>().ToList();
            if (cells.Count == 0 && grid.CurrentCell != null)
            {
                cells.Add(grid.CurrentCell);
            }

            var visible = VisibleConditions();
            var images = new List<DatasetImage>();
            foreach (var cell in cells.OrderBy(c => c.RowIndex).ThenBy(c => c.ColumnIndex))
            {
                var row = RowOf(grid.Rows[cell.RowIndex]);
                if (row == null)
                {
                    continue;
                }

                IEnumerable<DatasetImage> found;
                if (row.Unmatched != null || BlockOf(grid.Columns[cell.ColumnIndex]) != null)
                {
                    found = new[] { CellImage(cell.RowIndex, cell.ColumnIndex) };
                }
                else
                {
                    found = visible.Select(row.Block).Where(b => b != null).Select(b => b.Main);
                }

                images.AddRange(found.Where(i => i != null && !images.Contains(i)));
            }

            return images;
        }

        /// <summary>Chọn ô chứa ảnh <paramref name="image"/> (ô Raw hoặc Mask của block). False nếu ảnh không hiện trong bảng.</summary>
        private bool SelectImageCell(DatasetImage image)
        {
            if (image == null)
            {
                return false;
            }

            foreach (DataGridViewRow gridRow in grid.Rows)
            {
                var row = RowOf(gridRow);
                if (row == null || !row.Images.Contains(image))
                {
                    continue;
                }

                DataGridViewColumn target = null;
                if (row.Unmatched != null)
                {
                    target = grid.Columns[0];
                }
                else
                {
                    var part = image.Type == "Mask" ? BlockPart.Mask : BlockPart.Raw;
                    target = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Visible && BlockOf(c) != null
                        && BlockOf(c).Part == part && string.Equals(BlockOf(c).Condition, image.Condition, StringComparison.OrdinalIgnoreCase));
                }

                if (target == null)
                {
                    return false;
                }

                grid.ClearSelection();
                grid.CurrentCell = gridRow.Cells[target.Index];
                grid.CurrentCell.Selected = true;
                return true;
            }

            return false;
        }

        private DatasetImage FindImage(string path)
        {
            return _images.FirstOrDefault(i => string.Equals(i.FullPath, path, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Ảnh cặp cùng mẫu (cùng Key + hậu tố), khác loại Raw / Mask.</summary>
        private DatasetImage PairOf(DatasetImage image)
        {
            if (image == null || !image.Matched)
            {
                return null;
            }

            return _images.FirstOrDefault(i => i != image && i.Matched && i.Type != image.Type
                && string.Equals(i.PairKey, image.PairKey, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Kiểm tra tên mới theo schema - hiện ngay trong hộp Đổi tên.</summary>
        private string DescribeName(string fileName)
        {
            var result = _schema.Match(fileName);
            if (!result.Success)
            {
                return "✖ Không khớp schema " + _schema.Text + " - ảnh sẽ không được đưa vào báo cáo Excel.";
            }

            return "✔ Khớp schema:  " + string.Join(",  ", result.Values.Select(kv => kv.Key + " = " + kv.Value))
                + (result.Type != null ? ",  Loại = " + result.Type : string.Empty)
                + (string.IsNullOrEmpty(result.Evaluation) ? string.Empty : ",  Vision = " + result.Evaluation)
                + (string.IsNullOrEmpty(result.Suffix) ? string.Empty : ",  Hậu tố = " + result.Suffix)
                + (string.IsNullOrEmpty(result.Shot) ? string.Empty : ",  Lần chụp = " + result.Shot);
        }

        private IList<KeyValuePair<string, string>> ExtraProperties(string path)
        {
            var image = FindImage(path);
            var list = new List<KeyValuePair<string, string>>();
            if (image == null)
            {
                return list;
            }

            list.Add(new KeyValuePair<string, string>("Schema", image.Matched ? "✔ Khớp" : "✖ Không khớp - không vào báo cáo"));
            foreach (var kv in image.Values)
            {
                list.Add(new KeyValuePair<string, string>(kv.Key, kv.Value));
            }

            list.Add(new KeyValuePair<string, string>("Loại", image.Type));
            if (!string.IsNullOrEmpty(image.Suffix))
            {
                list.Add(new KeyValuePair<string, string>("Hậu tố", image.Suffix));
            }

            string unitX = UnitXValue(image);
            list.Add(new KeyValuePair<string, string>("Vision", image.NoResult
                ? "✖ Chưa có ảnh Mask - chưa ghi nhận kết quả"
                : string.IsNullOrEmpty(unitX) ? "(trống)" : unitX));
            string manual = StoredValue(image);
            bool hasManual = manual != null;
            string effective = EffectiveEvaluation(image);
            list.Add(new KeyValuePair<string, string>("Đánh giá",
                (string.IsNullOrEmpty(effective) ? "(trống)" : effective) + (hasManual && !IsRule(manual) ? "  (chỉnh tay)" : "  (" + RuleText(hasManual ? manual : GlobalRule) + (hasManual ? ", chọn riêng" : ", logic chung") + ")")));
            return list;
        }

        private void ViewPair()
        {
            var images = SelectedImages();
            var pair = images.Count == 1 ? PairOf(images[0]) : null;
            if (pair == null)
            {
                return;
            }

            // Chọn ô Raw / Mask của ảnh cặp trong cùng block; không hiện được (bị lọc) thì chỉ đưa
            // lên khung xem trước.
            if (!SelectImageCell(pair))
            {
                ShowPreviewOf(pair);
            }
        }

        private void OnImagesDeleted(IList<string> deleted)
        {
            var set = new HashSet<string>(deleted, StringComparer.OrdinalIgnoreCase);
            var removed = _images.Where(i => set.Contains(i.FullPath)).ToList();

            foreach (var image in removed)
            {
                if (image.EvalKey != null)
                {
                    // Giữ lại nhãn gán tay: hoàn tác xoá (Ctrl+Z) thì gắn lại.
                    string label;
                    if (_evaluations.TryGetValue(image.EvalKey, out label))
                    {
                        _deletedLabels[image.FullPath] = label;
                    }

                    _evaluations.Remove(image.EvalKey);
                }
            }

            _images.RemoveAll(i => set.Contains(i.FullPath));
            SaveEvaluations();
            RefillKeepingPosition(null);
        }

        private void OnImagesRenamed(IList<KeyValuePair<string, string>> moves)
        {
            DatasetImage first = null;

            foreach (var move in moves)
            {
                var image = FindImage(move.Key);
                if (image == null)
                {
                    continue;
                }

                // Nhãn gán tay đi theo file sang tên mới.
                string oldKey = image.EvalKey;
                image.FullPath = move.Value;
                image.FileName = Path.GetFileName(move.Value);
                image.EvalKey = image.RelativeDir == "." ? image.FileName : Path.Combine(image.RelativeDir, image.FileName);

                string label;
                if (oldKey != null && _evaluations.TryGetValue(oldKey, out label))
                {
                    _evaluations.Remove(oldKey);
                    _evaluations[image.EvalKey] = label;
                }

                ParseImage(image);
                first = first ?? image;
            }

            SaveEvaluations();
            RefillKeepingPosition(first);
        }

        /// <summary>Dựng lại bảng (giữ bộ lọc) và giữ vị trí cuộn; chọn lại ảnh <paramref name="select"/> nếu có.</summary>
        private void RefillKeepingPosition(DatasetImage select)
        {
            _previewImage = null;   // tên / đường dẫn ảnh có thể vừa đổi -> luôn đọc lại
            int scroll = grid.FirstDisplayedScrollingRowIndex;
            int current = grid.CurrentCell != null ? grid.CurrentCell.RowIndex : 0;
            string currentColumn = grid.CurrentCell != null ? grid.Columns[grid.CurrentCell.ColumnIndex].Name : null;

            FillGrid();
            UpdateSchemaInfo(null);

            if (grid.Rows.Count == 0)
            {
                ShowPreviewOf(null);
                return;
            }

            if (scroll >= 0 && scroll < grid.Rows.Count)
            {
                grid.FirstDisplayedScrollingRowIndex = scroll;
            }

            if (SelectImageCell(select))
            {
                ShowPreview();
                return;
            }

            // Không có ảnh cần chọn (vd. vừa xoá): đứng lại đúng hàng / cột cũ.
            var column = currentColumn != null && grid.Columns.Contains(currentColumn) && grid.Columns[currentColumn].Visible
                ? grid.Columns[currentColumn]
                : grid.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
            if (column != null)
            {
                grid.ClearSelection();
                grid.CurrentCell = grid.Rows[Math.Min(current, grid.Rows.Count - 1)].Cells[column.Index];
                grid.CurrentCell.Selected = true;
            }

            ShowPreview();
        }

        #endregion

        #region Xem trước

        /// <summary>true = khung xem trước đang hiện ẢNH CẶP của dòng đang chọn (bấm Space).</summary>
        private bool _previewShowsPair;

        /// <summary>Ảnh đang hiện ở khung xem trước - đổi ô trong cùng block không phải đọc lại ảnh.</summary>
        private DatasetImage _previewImage;

        private void ShowPreview()
        {
            var image = CurrentImage();
            // Xem song song: đổi giữa ô Raw và ô Mask của CÙNG block thì hai panel đã đúng ảnh.
            if (image != null && _splitView && (image == _previewImage || image == _splitMaskImage))
            {
                return;
            }

            if (image != null && image == _previewImage && !_previewShowsPair)
            {
                return;
            }

            _previewShowsPair = false;
            ShowPreviewOf(image);
        }

        /// <summary>
        /// Space: chuyển khung xem trước qua lại giữa ảnh của ô đang chọn và ảnh cặp (Raw ↔ Mask) cùng
        /// block DK.
        /// </summary>
        /// <summary>
        /// Phím Space: chuyển nhanh MỘT ảnh Raw ↔ Mask của ô đang chọn. Đang xem song song (Raw | Mask) thì thoát
        /// song song rồi chuyển sang ảnh còn lại.
        /// </summary>
        private void TogglePreviewPair()
        {
            string shown = _previewImage != null ? _previewImage.Type : CurrentImage()?.Type;
            ShowPreviewType(string.Equals(shown, "Mask", StringComparison.OrdinalIgnoreCase) ? "Raw" : "Mask");
        }

        /// <summary>Nút Raw / Mask (và phím Space): xem một ảnh loại <paramref name="type"/> của ô đang chọn.</summary>
        private void ShowPreviewType(string type)
        {
            if (_splitView)
            {
                SetSplitView(false);
            }

            var image = CurrentImage();
            if (image == null)
            {
                UpdateViewModeButtons();
                return;
            }

            var target = string.Equals(image.Type, type, StringComparison.OrdinalIgnoreCase) ? image : PairOf(image);
            if (target == null)
            {
                ShowStatusMessage(image.FileName + " không có ảnh cặp " + type + ".");
                UpdateViewModeButtons();
                return;
            }

            _previewShowsPair = target != image;
            ShowPreviewOf(target);
        }

        /// <summary>Tô nút đang chọn trong nhóm [Raw][Mask][Raw | Mask] theo ảnh đang hiện.</summary>
        private void UpdateViewModeButtons()
        {
            string type = _previewImage == null ? null : _previewImage.Type;
            StyleViewModeButton(btnShowRaw, !_splitView && string.Equals(type, "Raw", StringComparison.OrdinalIgnoreCase));
            StyleViewModeButton(btnShowMask, !_splitView && string.Equals(type, "Mask", StringComparison.OrdinalIgnoreCase));
            StyleViewModeButton(btnSplit, _splitView);
        }

        private static void StyleViewModeButton(Button button, bool active)
        {
            button.BackColor = Theme.Back(active ? Primary : Color.White);
            button.ForeColor = active ? Color.White : Theme.Fore(Color.FromArgb(31, 41, 55));
            button.FlatAppearance.BorderColor = active ? Primary : Theme.Fore(Color.FromArgb(209, 213, 219));
        }

        private void ShowPreviewOf(DatasetImage image)
        {
            if (_splitView)
            {
                ShowSplitOf(image);
                UpdateViewModeButtons();
                return;
            }

            ReleaseImage(picPreview);
            _previewImage = image;
            UpdateViewModeButtons();
            UpdatePreviewComment();

            if (image == null)
            {
                lblPreview.Text = "Chưa chọn ảnh";
                lblPreviewMeta.Text = "Chọn một ô trong bảng để xem ảnh";
                return;
            }

            // Dòng 1: tên file; dòng 2: DK · lần chụp · loại · thư mục.
            lblPreview.Text = image.FileName;
            lblPreviewMeta.Text = PreviewMeta(image, image.Type);

            string error;
            picPreview.Image = LoadPreviewBitmap(image.FullPath, out error);
            if (error != null)
            {
                lblPreviewMeta.Text = "Không mở được ảnh: " + error;
            }
        }

        /// <summary>
        /// Xem song song: Raw của block bên trái, Mask bên phải - mỗi bên zoom / kéo / navigator riêng.
        /// Block chưa có Mask thì panel Mask báo "No Mask Available".
        /// </summary>
        private void ShowSplitOf(DatasetImage image)
        {
            DatasetImage raw = null, mask = null;
            if (image != null)
            {
                var pair = PairOf(image);
                raw = image.Type == "Mask" ? pair : image;
                mask = image.Type == "Mask" ? image : pair;
            }

            _previewImage = raw ?? mask;
            _splitMaskImage = mask;
            UpdatePreviewComment();

            LoadInto(picPreview, raw, "Chưa có ảnh Raw");
            LoadInto(picPreviewMask, mask, "No Mask Available\r\nẢnh này chưa có Mask / kết quả");

            var main = raw ?? mask;
            lblPreview.Text = main == null ? "Raw | Mask" : main.FileName;
            lblPreviewMeta.Text = main == null ? "Chọn một ô trong bảng để xem ảnh" : PreviewMeta(main, "Raw | Mask song song");
        }

        private static void LoadInto(ZoomPictureBox box, DatasetImage image, string emptyText)
        {
            ReleaseImage(box);
            if (image == null)
            {
                box.EmptyText = emptyText;
                return;
            }

            string error;
            box.Image = LoadPreviewBitmap(image.FullPath, out error);
            box.EmptyText = error == null ? null : "Không mở được ảnh:\r\n" + image.FileName;
        }

        private static void ReleaseImage(ZoomPictureBox box)
        {
            var old = box.Image;
            box.Image = null;
            if (old != null)
            {
                old.Dispose();
            }
        }

        /// <summary>
        /// Đọc ảnh để xem (thu nhỏ tối đa 2600 px - vẫn đủ chi tiết để zoom soi). Đọc qua MemoryStream:
        /// Image.FromFile khoá file tới khi bitmap được giải phóng.
        /// </summary>
        private static Bitmap LoadPreviewBitmap(string path, out string error)
        {
            error = null;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(bytes, false))
                using (var decoded = Image.FromStream(ms, false, false))
                {
                    const int maxSize = 2600;
                    double scale = Math.Min(1.0, Math.Min((double)maxSize / decoded.Width, (double)maxSize / decoded.Height));
                    int w = Math.Max(1, (int)(decoded.Width * scale));
                    int h = Math.Max(1, (int)(decoded.Height * scale));

                    var bitmap = new Bitmap(w, h);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        g.DrawImage(decoded, 0, 0, w, h);
                    }

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        #region Xem song song Raw | Mask

        private readonly ZoomPictureBox picPreviewMask = new ZoomPictureBox();
        private readonly TableLayoutPanel tlpSplit = new TableLayoutPanel();
        private Padding _picPreviewMargin;
        private int _previewImageRow;

        /// <summary>Khung xem trước (Designer: tlpPreviewCard): gắn ◀ ▶ lần chụp, nút "Raw | Mask" + dựng bảng chia đôi.</summary>
        private void SetupPreviewHeader()
        {
            SetupShotNavButtons();
            grid.CurrentCellChanged += (s, e) => UpdateShotNav();
            btnSplit.Click += (s, e) => SetSplitView(!_splitView);
            btnShowRaw.Click += (s, e) => ShowPreviewType("Raw");
            btnShowMask.Click += (s, e) => ShowPreviewType("Mask");
            WirePreviewExpand();
            WirePreviewZoom();
            UpdateViewModeButtons();
            _picPreviewMargin = picPreview.Margin;
            _previewImageRow = tlpPreviewCard.GetRow(picPreview);

            picPreviewMask.Dock = DockStyle.Fill;
            picPreviewMask.SizeMode = PictureBoxSizeMode.Zoom;
            picPreviewMask.BackColor = picPreview.BackColor;
            picPreviewMask.Margin = new Padding(1, 0, 0, 0);
            picPreviewMask.Caption = "MASK";

            tlpSplit.Dock = DockStyle.Fill;
            tlpSplit.ColumnCount = 2;
            tlpSplit.RowCount = 2;
            tlpSplit.Margin = picPreview.Margin;
            tlpSplit.BackColor = Color.FromArgb(55, 65, 81);
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSplit.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        /// <summary>Bật / tắt xem song song: picPreview chuyển vào bảng chia đôi rồi trả lại chỗ cũ.</summary>
        private void SetSplitView(bool on)
        {
            if (on == _splitView)
            {
                return;
            }

            _splitView = on;
            var host = (TableLayoutPanel)(on ? picPreview.Parent : tlpSplit.Parent);
            host.SuspendLayout();

            if (on)
            {
                host.Controls.Remove(picPreview);
                picPreview.Margin = new Padding(0, 0, 1, 0);
                picPreview.Caption = "RAW";
                tlpSplit.Controls.Add(picPreview, 0, 0);
                tlpSplit.Controls.Add(picPreviewMask, 1, 0);

                // Thanh ghi chú đặt dưới CẢ hai panel.
                picPreview.Controls.Remove(lblPreviewComment);
                lblPreviewComment.Dock = DockStyle.Fill;
                lblPreviewComment.Margin = new Padding(0);
                tlpSplit.Controls.Add(lblPreviewComment, 0, 1);
                tlpSplit.SetColumnSpan(lblPreviewComment, 2);

                host.Controls.Add(tlpSplit, 0, _previewImageRow);
            }
            else
            {
                tlpSplit.Controls.Remove(lblPreviewComment);
                lblPreviewComment.Dock = DockStyle.Bottom;
                picPreview.Controls.Add(lblPreviewComment);

                tlpSplit.Controls.Remove(picPreview);
                host.Controls.Remove(tlpSplit);
                picPreview.Caption = null;
                picPreview.EmptyText = null;
                picPreview.Margin = _picPreviewMargin;
                host.Controls.Add(picPreview, 0, _previewImageRow);
                ReleaseImage(picPreviewMask);
                _splitMaskImage = null;
            }

            host.ResumeLayout();

            _previewShowsPair = false;
            ShowPreviewOf(CurrentImage());
        }

        #endregion

        private void OpenFile(DatasetImage image)
        {
            if (image == null)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(image.FullPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không mở được ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Xuất Excel

        /// <summary>
        /// Gom ảnh cùng mẫu (cùng giá trị các trường Key + hậu tố) thành MỘT hàng, mỗi DK một block:
        ///     [các trường Key] [Hậu tố] | DK1: UnitX | Evaluation | DK2: ... | DK1: Raw | Mask | DK2: ...
        /// Raw / Mask là hyperlink (tên file) hoặc hình ảnh vừa khít ô - chọn trong frmExportOptions.
        /// Hậu tố nằm trong khoá gom để các lần chụp khác nhau của cùng một mẫu không bị gộp làm một
        /// (khi đó chỉ giữ được một ảnh và mất các ảnh còn lại). Ảnh không khớp schema không đưa vào.
        /// </summary>
        private async void ExportExcel()
        {
            var matched = _images.Where(i => i.Matched).ToList();
            if (matched.Count == 0)
            {
                MessageBox.Show(this,
                    _images.Count == 0
                        ? "Chưa có ảnh nào. Chọn thư mục rồi bấm 'Quét thư mục' trước."
                        : "Không có ảnh nào khớp schema - kiểm tra lại schema tên file.",
                    "Không có dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ---- Tuỳ chọn: hyperlink hay hình ảnh, kích thước ô ----
            DatasetExportOptions options;
            using (var dialog = new frmExportOptions(_exportOptions, ReadImageSize(matched)))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                options = dialog.Options;
            }

            _exportOptions = options;
            SaveSettings();

            // Hyperlink ghi TƯƠNG ĐỐI theo vị trí file Excel -> luôn lưu thẳng vào thư mục dataset (cùng cấp
            // với các thư mục / ảnh của dataset), không hỏi nơi lưu: gửi cả thư mục là link ảnh vẫn mở được.
            await ExportExcelTo(ExcelReportPath(), options, true);
        }

        /// <summary>
        /// File báo cáo trong thư mục dataset: &lt;tên thư mục&gt;_Report_yyyyMMdd_HHmmss.xlsx (mốc thời gian -> không
        /// ghi đè báo cáo cũ, không đụng file đang mở trong Excel).
        /// </summary>
        private string ExcelReportPath()
        {
            string folder = Path.GetFullPath(_loadedFolder);
            string name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(name))
            {
                name = "Dataset";
            }

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(folder, name + "_Report_" + stamp + ".xlsx");
            for (int i = 2; File.Exists(path); i++)
            {
                path = Path.Combine(folder, name + "_Report_" + stamp + "_" + i + ".xlsx");
            }

            return path;
        }

        /// <summary>
        /// Ghi file Excel (phần sau các hộp thoại tuỳ chọn / chọn nơi lưu). <paramref name="interactive"/> = false:
        /// không hỏi / thông báo gì (lỗi ném ra ngoài) - dùng để tạo file mẫu / kiểm thử.
        /// </summary>
        internal async Task ExportExcelTo(string target, DatasetExportOptions options, bool interactive)
        {
            var matched = _images.Where(i => i.Matched).ToList();

            // Ba cách xuất cột Raw / Mask:
            //   - Hyperlink + thumbnail : thumbnail JPEG chất lượng thấp + bấm vào ảnh mở ảnh gốc; workbook
            //                             TẮT "Do not compress images" (Excel được nén) - tránh treo với
            //                             dataset ~1000 mẫu.
            //   - Hyperlink, tắt thumbnail : chỉ tên file bấm được, KHÔNG đọc ảnh nào - nhanh nhất.
            //   - Image                 : nhúng NGUYÊN ảnh gốc, không hyperlink, bật "Do not compress images".
            bool originalImages = options.CellMode == ExportCellMode.Image;
            bool withThumbnails = !originalImages && options.HyperlinkThumbnail;
            bool withImages = originalImages || withThumbnails;
            Size cellSize = options.CellSize;

            // Cột: [trường Key...] [Hậu tố] | UnitX Evaluation từng DK | Raw Mask từng DK - giống bảng.
            // Schema có trường DK thì tiêu đề hai tầng, tên DK gộp ô phía trên block của nó.
            // Cột thông tin (trường Key + Hậu tố) theo thứ tự người dùng xác nhận ở hộp thoại Xuất (kéo thả).
            var infoFields = ExportInfoFields();
            var conditions = _conditions.ToList();
            double imageColumn = XlsxWriter.ColumnWidthForPixels(cellSize.Width);

            var headers = infoFields.Select(f => f.Name).ToList();
            var widths = infoFields.Select(f => f.Role == DatasetFieldRole.Suffix ? 22.0 : 14.0).ToList();

            int fixedColumns = headers.Count;
            var headerGroups = new List<XlsxHeaderGroup>();
            // Kết quả trước: [Vision] | Đánh giá | Ghi chú của từng DK nối nhau (Vision chỉ khi tick ở tuỳ chọn xuất)...
            bool includeVision = options.IncludeVision;
            foreach (var condition in conditions)
            {
                if (_schema.ConditionField != null)
                {
                    headerGroups.Add(new XlsxHeaderGroup { Text = condition, FirstColumn = headers.Count, ColumnCount = includeVision ? 3 : 2 });
                }

                headers.AddRange(includeVision ? new[] { "Vision", "Đánh giá", "Ghi chú" } : new[] { "Đánh giá", "Ghi chú" });
                widths.AddRange(includeVision ? new[] { 10.0, 14.0, 30.0 } : new[] { 14.0, 30.0 });
            }

            // ...rồi tới ảnh: Raw | Mask của từng DK nối nhau. <Lần chụp>: mỗi lần chụp một cặp cột
            // Raw 1 | Mask 1 | Raw 2 | Mask 2 ... (số cặp = số lần chụp nhiều nhất của DK đó).
            // Hộp thoại Xuất đã bỏ <Lần chụp>: mỗi ĐK một cặp Raw | Mask = lần chụp đầu tiên theo tên file.
            bool singleShot = ExportSingleShot();
            var shotColumns = conditions.Select(c => Math.Max(1, _rows
                .Select(r => r.Block(c))
                .Where(b => b != null && b.Shots != null)
                .Select(b => singleShot ? 1 : b.Shots.Count)
                .DefaultIfEmpty(1)
                .Max())).ToList();

            for (int ci = 0; ci < conditions.Count; ci++)
            {
                int shots = shotColumns[ci];
                if (_schema.ConditionField != null || shots > 1)
                {
                    headerGroups.Add(new XlsxHeaderGroup
                    {
                        Text = _schema.ConditionField != null ? conditions[ci] : "Lần chụp",
                        FirstColumn = headers.Count,
                        ColumnCount = 2 * shots
                    });
                }

                for (int k = 1; k <= shots; k++)
                {
                    headers.AddRange(shots > 1 ? new[] { "Raw " + k, "Mask " + k } : new[] { "Raw", "Mask" });
                    widths.AddRange(withImages ? new[] { imageColumn, imageColumn } : new[] { 48.0, 52.0 });
                }
            }

            // Dữ liệu từng hàng lấy trên luồng giao diện (nhãn đánh giá đọc từ _evaluations). _rows đã
            // sắp theo trường Key (thứ tự tự nhiên) rồi hậu tố.
            var groups = _rows
                .Select(r => new
                {
                    Keys = infoFields.Select(f => InfoValue(r, f)).ToList(),
                    Blocks = conditions.Select(c =>
                    {
                        var block = r.Block(c);
                        return new
                        {
                            Raw = block == null ? null : block.Raw,
                            Mask = block == null ? null : block.Mask,
                            // Ảnh theo từng lần chụp (thứ tự lần chụp); không có <Lần chụp> thì một cặp.
                            Pairs = block == null ? new List<DatasetImage[]>()
                                : block.Shots != null
                                    ? (singleShot
                                        ? new List<DatasetImage[]> { FirstShotPair(block) }
                                        : block.Shots.Select(s => new[] { s.Raw, s.Mask }).ToList())
                                : new List<DatasetImage[]> { new[] { block.Raw, block.Mask } },
                            UnitX = BlockUnitX(block),
                            Evaluation = BlockEvaluation(block),
                            // Nhiều lần chụp: ảnh trong Excel là lần chụp đại diện (Vision xấu nhất) - ghi thêm
                            // Vision từng lần chụp vào Ghi chú.
                            Comment = !singleShot && block != null && block.Shots != null && block.Shots.Count > 1
                                ? string.Join(Environment.NewLine, new[] { BlockComment(block), ShotsSummary(block).Replace("Δ", ExportDelta) }.Where(t => !string.IsNullOrEmpty(t)))
                                : BlockComment(block)
                        };
                    }).ToList()
                })
                .ToList();

            btnExport.Enabled = false;
            btnLoad.Enabled = false;
            int failedImages = 0;
            int rowCount = groups.Count;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                string phase = originalImages ? "Đang đọc ảnh gốc" : "Đang tạo thumbnail";

                // done = -1: đã xong phần ảnh, đang ghi file Excel.
                var progress = new Progress<int[]>(p =>
                {
                    if (p[0] < 0)
                    {
                        ShowExportProgress("Đang ghi file Excel", 0, 0, watch);
                    }
                    else
                    {
                        ShowExportProgress(phase, p[0], p[1], watch);
                    }
                });
                ShowExportProgress(withImages ? phase : "Đang ghi file Excel", 0, withImages ? matched.Count : 0, watch);

                await Task.Run(() =>
                {
                    // Chuẩn bị ảnh song song. Tắt thumbnail thì không đọc ảnh nào: ImageCell tự lùi về
                    // hyperlink tên file.
                    var thumbs = new System.Collections.Concurrent.ConcurrentDictionary<DatasetImage, Thumbnail>();
                    var all = withImages
                        ? groups.SelectMany(g => g.Blocks).SelectMany(b => b.Pairs.SelectMany(p => p)).Where(i => i != null).Distinct().ToList()
                        : new List<DatasetImage>();
                    int done = 0;
                    int lastPercent = -1;

                    // Đọc file + giải mã chen nhau giữa các luồng: dùng đủ số nhân CPU.
                    Parallel.ForEach(all, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) },
                        image =>
                        {
                            var thumb = originalImages
                                ? PrepareOriginal(image.FullPath, cellSize)
                                : MakeThumbnail(image.FullPath, cellSize);
                            if (thumb == null)
                            {
                                System.Threading.Interlocked.Increment(ref failedImages);
                            }
                            else
                            {
                                thumbs[image] = thumb;
                            }

                            // Chỉ báo khi % đổi - hàng nghìn lần báo liên tiếp làm giao diện ì.
                            int current = System.Threading.Interlocked.Increment(ref done);
                            int percent = (int)(current * 100L / all.Count);
                            if (percent != System.Threading.Interlocked.Exchange(ref lastPercent, percent) || current == all.Count)
                            {
                                ((IProgress<int[]>)progress).Report(new[] { current, all.Count });
                            }
                        });

                    ((IProgress<int[]>)progress).Report(new[] { -1, 0 });

                    var rows = new List<XlsxCell[]>();
                    foreach (var group in groups)
                    {
                        var cells = group.Keys.Select(k => new XlsxCell { Text = k }).ToList();

                        foreach (var block in group.Blocks)
                        {
                            if (includeVision)
                            {
                                cells.Add(new XlsxCell { Text = ExportLabel(block.UnitX), Center = true });
                            }

                            cells.Add(new XlsxCell { Text = ExportLabel(block.Evaluation), Center = true });
                            cells.Add(new XlsxCell { Text = block.Comment });
                        }

                        for (int ci = 0; ci < group.Blocks.Count; ci++)
                        {
                            var block = group.Blocks[ci];
                            for (int k = 0; k < shotColumns[ci]; k++)
                            {
                                var pair = k < block.Pairs.Count ? block.Pairs[k] : new DatasetImage[2];
                                cells.Add(ImageCell(pair[0], !originalImages, thumbs));
                                cells.Add(ImageCell(pair[1], !originalImages, thumbs));
                            }
                        }

                        rows.Add(cells.ToArray());
                    }

                    XlsxWriter.Write(target, "Dataset", headers, rows, widths, withImages ? cellSize.Height : 0,
                        doNotCompressImages: originalImages, headerGroups: headerGroups, freezeColumns: fixedColumns);
                });
            }
            catch (IOException ex) when (interactive)
            {
                MessageBox.Show(this, "Không ghi được file Excel (file đang mở trong Excel?):" + Environment.NewLine + ex.Message,
                    "Lỗi xuất Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex) when (interactive)
            {
                MessageBox.Show(this, "Xuất Excel lỗi:" + Environment.NewLine + ex.Message,
                    "Lỗi xuất Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                btnExport.Enabled = true;
                btnLoad.Enabled = true;
                UpdateSummary();
            }

            if (!interactive)
            {
                return;
            }

            int skipped = _images.Count - matched.Count;
            var answer = MessageBox.Show(this,
                $"Đã xuất {rowCount} hàng"
                + (conditions.Count > 1 ? $" × {conditions.Count} DK" : string.Empty)
                + $" ra:{Environment.NewLine}{target}"
                + $"{Environment.NewLine}Thời gian xuất: {FormatDuration(watch.Elapsed)}"
                + $"{Environment.NewLine}"
                + (originalImages
                    ? $"Ảnh gốc (không nén) trong ô {cellSize.Width} × {cellSize.Height} px."
                    : withThumbnails
                        ? $"Thumbnail chất lượng thấp có hyperlink trong ô {cellSize.Width} × {cellSize.Height} px."
                        : "Hyperlink tên file (không thumbnail).")
                + (failedImages > 0 ? $"{Environment.NewLine}{failedImages} ảnh không đọc được - ô đó để hyperlink tên file thay cho ảnh." : string.Empty)
                + (skipped > 0 ? $"{Environment.NewLine}{Environment.NewLine}Bỏ qua {skipped} ảnh không khớp schema." : string.Empty)
                + $"{Environment.NewLine}{Environment.NewLine}Mở thư mục chứa file Excel?",
                "Xuất Excel xong", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (answer == DialogResult.Yes)
            {
                ImageFileOps.ShowInExplorer(new[] { target });
            }
        }

        /// <summary>Ảnh thu nhỏ đã mã hoá JPEG để nhúng vào Excel + kích thước hiển thị trong ô.</summary>
        private sealed class Thumbnail
        {
            public byte[] Jpeg;
            public int Width;
            public int Height;

            /// <summary>"jpeg" hoặc "png" - định dạng của Jpeg (ảnh gốc có thể là PNG).</summary>
            public string Extension = "jpeg";
        }

        /// <summary>Lề giữa ảnh và viền ô (px mỗi bên).</summary>
        private const int ImageCellPadding = 2;

        /// <summary>
        /// Ô Raw / Mask: ảnh vừa ô, có hyperlink tới ảnh gốc hay không tuỳ chế độ. Đọc ảnh lỗi thì lùi
        /// về hyperlink tên file để hàng đó vẫn mở được ảnh.
        /// </summary>
        private static XlsxCell ImageCell(DatasetImage image, bool withLink, IDictionary<DatasetImage, Thumbnail> thumbs)
        {
            if (image == null)
            {
                return null;
            }

            Thumbnail thumb;
            if (thumbs.TryGetValue(image, out thumb))
            {
                return new XlsxCell
                {
                    Text = image.FileName,
                    Link = withLink ? image.FullPath : null,
                    ImageJpeg = thumb.Jpeg,
                    ImageExtension = thumb.Extension,
                    ImageWidthPx = thumb.Width,
                    ImageHeightPx = thumb.Height
                };
            }

            return new XlsxCell { Text = image.FileName, Link = image.FullPath };
        }

        /// <summary>
        /// Chế độ "ảnh gốc": nhúng NGUYÊN byte của file JPEG/PNG (không giải mã - mã hoá lại, không mất
        /// chất lượng); BMP/TIFF không nhúng trực tiếp được thì chuyển sang PNG (vẫn không mất dữ liệu).
        /// Kích thước HIỂN THỊ vẫn thu vừa ô, giữ tỷ lệ - Excel co ảnh lại nhưng giữ đủ độ phân giải.
        /// </summary>
        private static Thumbnail PrepareOriginal(string path, Size cell)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string ext = Path.GetExtension(path).ToLowerInvariant();

                using (var ms = new MemoryStream(bytes, false))
                using (var source = Image.FromStream(ms, false, false))
                {
                    Size fit = FitInCell(source.Size, cell);
                    var result = new Thumbnail { Width = fit.Width, Height = fit.Height };

                    if (ext == ".jpg" || ext == ".jpeg")
                    {
                        result.Jpeg = bytes;
                        result.Extension = "jpeg";
                    }
                    else if (ext == ".png")
                    {
                        result.Jpeg = bytes;
                        result.Extension = "png";
                    }
                    else
                    {
                        using (var png = new MemoryStream())
                        {
                            source.Save(png, System.Drawing.Imaging.ImageFormat.Png);
                            result.Jpeg = png.ToArray();
                            result.Extension = "png";
                        }
                    }

                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Kích thước hiển thị của ảnh khi thu vừa ô (trừ lề), giữ nguyên tỷ lệ.</summary>
        private static Size FitInCell(Size image, Size cell)
        {
            int boxW = Math.Max(1, cell.Width - 2 * ImageCellPadding);
            int boxH = Math.Max(1, cell.Height - 2 * ImageCellPadding);
            double scale = Math.Min((double)boxW / image.Width, (double)boxH / image.Height);
            return new Size(Math.Max(1, (int)Math.Floor(image.Width * scale)), Math.Max(1, (int)Math.Floor(image.Height * scale)));
        }

        /// <summary>
        /// Chất lượng JPEG của thumbnail chế độ Hyperlink. Cố ý để THẤP: thumbnail chỉ để nhận diện ảnh
        /// trong Excel, bấm vào là mở ảnh gốc. Dataset ~1000 mẫu = ~2000 thumbnail - ảnh nặng làm Excel
        /// (và bước xuất) chậm tới mức như treo.
        /// </summary>
        private const int ThumbnailJpegQuality = 60;

        /// <summary>
        /// Thumbnail chất lượng thấp cho chế độ Hyperlink: thu vừa khít ô (trừ lề), giữ nguyên tỷ lệ,
        /// lưu ĐÚNG kích thước hiển thị (không phóng 2×), JPEG chất lượng 60 - vài KB mỗi ảnh.
        /// Dataset rất lớn mà vẫn chậm thì tắt thumbnail trong hộp thoại xuất (chỉ còn hyperlink tên file).
        /// </summary>
        private static Thumbnail MakeThumbnail(string path, Size cell)
        {
            try
            {
                // Xuất lại trong cùng phiên (đổi nhãn rồi xuất tiếp...) thì dùng lại thumbnail đã tạo.
                var info = new FileInfo(path);
                string cacheKey = path + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks + "|" + cell.Width + "x" + cell.Height;
                Thumbnail cached;
                if (ThumbnailCache.TryGetValue(cacheKey, out cached))
                {
                    return cached;
                }

                Thumbnail thumb;
                using (var bitmap = DecodeScaled(path, cell))
                {
                    var codec = JpegCodec.Value;
                    using (var parameters = new System.Drawing.Imaging.EncoderParameters(1))
                    using (var output = new MemoryStream())
                    {
                        parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, (long)ThumbnailJpegQuality);
                        bitmap.Save(output, codec, parameters);
                        thumb = new Thumbnail { Jpeg = output.ToArray(), Width = bitmap.Width, Height = bitmap.Height };
                    }
                }

                // Giới hạn bộ nhớ: ~3-5 KB/thumbnail, 20 000 thumbnail ≈ 100 MB.
                if (ThumbnailCache.Count < 20000)
                {
                    ThumbnailCache[cacheKey] = thumb;
                }

                return thumb;
            }
            catch
            {
                return null;
            }
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Thumbnail> ThumbnailCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Thumbnail>(StringComparer.OrdinalIgnoreCase);

        private static readonly Lazy<System.Drawing.Imaging.ImageCodecInfo> JpegCodec =
            new Lazy<System.Drawing.Imaging.ImageCodecInfo>(() => System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid));

        /// <summary>
        /// Ảnh thu vừa ô: ưu tiên WIC (giải mã JPEG thẳng ở độ phân giải nhỏ - nhanh gấp nhiều lần),
        /// lỗi thì lùi về GDI+ (giải mã toàn bộ rồi thu nhỏ).
        /// </summary>
        private static Bitmap DecodeScaled(string path, Size cell)
        {
            try
            {
                return WicImaging.LoadScaled(path, size => FitInCell(size, cell));
            }
            catch
            {
                byte[] bytes = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(bytes, false))
                using (var source = Image.FromStream(ms, false, false))
                {
                    Size fit = FitInCell(source.Size, cell);
                    var bitmap = new Bitmap(fit.Width, fit.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        g.DrawImage(source, 0, 0, fit.Width, fit.Height);
                    }

                    return bitmap;
                }
            }
        }

        /// <summary>"8,4 giây" / "2 phút 05 giây".</summary>
        private static string FormatDuration(TimeSpan span)
        {
            return span.TotalSeconds < 60
                ? span.TotalSeconds.ToString("0.0") + " giây"
                : $"{(int)span.TotalMinutes} phút {span.Seconds:00} giây";
        }

        /// <summary>Kích thước một ảnh của dataset (để đề xuất tỷ lệ ô), chỉ đọc header - không giải mã.</summary>
        private static Size ReadImageSize(IEnumerable<DatasetImage> images)
        {
            foreach (var image in images.Take(5))
            {
                try
                {
                    using (var stream = File.OpenRead(image.FullPath))
                    using (var decoded = Image.FromStream(stream, false, false))
                    {
                        return decoded.Size;
                    }
                }
                catch
                {
                    // Ảnh này lỗi - thử ảnh kế tiếp.
                }
            }

            return Size.Empty;
        }

        /// <summary>So sánh "tự nhiên": SPL2 đứng trước SPL10.</summary>
        private sealed class NaturalComparer : IComparer<string>
        {
            public static readonly NaturalComparer Instance = new NaturalComparer();

            [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            private static extern int StrCmpLogicalW(string a, string b);

            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x ?? string.Empty, y ?? string.Empty);
            }
        }

        #endregion

        #region Lưu trữ

        /// <summary>
        /// dataset_evaluation.json cũ lưu nhãn theo MẪU (khoá "MODEL|DK|SPL|HẬU TỐ"). Chuyển sang khoá
        /// theo từng ảnh: nhãn cũ của mẫu được chép cho mọi ảnh của mẫu đó (đúng như trước đây vẫn
        /// hiển thị), sau đó mỗi ảnh sửa độc lập. Khoá mới là đường dẫn tương đối nên không chứa '|'.
        /// </summary>
        private void MigrateLegacyEvaluations()
        {
            var legacy = _evaluations.Where(kv => kv.Key.IndexOf('|') >= 0).ToList();
            if (legacy.Count == 0)
            {
                return;
            }

            foreach (var kv in legacy)
            {
                _evaluations.Remove(kv.Key);

                foreach (var image in _images.Where(i => string.Equals(i.PairKey, kv.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!_evaluations.ContainsKey(image.EvalKey))
                    {
                        _evaluations[image.EvalKey] = kv.Value;
                    }
                }
            }

            SaveEvaluations();
        }

        private DatasetSettings LoadSettings()
        {
            try
            {
                if (_settingsPath != null && File.Exists(_settingsPath))
                {
                    return JsonConvert.DeserializeObject<DatasetSettings>(File.ReadAllText(_settingsPath));
                }
            }
            catch
            {
                // File cấu hình hỏng thì dùng mặc định.
            }

            return null;
        }

        private void SaveSettings()
        {
            if (_settingsPath == null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(new DatasetSettings
                {
                    Folder = txtFolder.Text.Trim().Trim('"'),
                    // Một schema hiện hành (chế độ tự suy theo dấu \) + danh sách gần đây; không ghi SchemaMode nữa.
                    Schema = txtSchema.Text.Trim(),
                    RecentSchemas = _recentSchemas.ToList(),
                    IncludeSubfolders = chkSubfolders.Checked,
                    Export = _exportOptions,
                    ShowConfig = chkShowConfig.Checked,
                    ShowTree = chkShowTree.Checked,
                    ShowStats = chkShowStats.Checked,
                    ShowIssues = chkShowIssues.Checked,
                    CollapsedSections = CollapsedSections(),
                    Locked = _locked,
                    CompareMode = _compareMode.ToString(),
                    Layout = CurrentLayout(),
                    ExportOrder = _exportOrder,
                    ExportHidden = _exportHidden
                }, Formatting.Indented));
            }
            catch
            {
                // Không lưu được cấu hình thì lần sau dùng mặc định - không đáng chặn người dùng.
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _filterTimer.Dispose();
                ImageComments.Changed -= OnImageCommentsChanged;
                Theme.Changed -= OnThemeChanged;
                FileHistory.Applied -= OnFileHistoryApplied;
                components?.Dispose();
            }

            if (disposing && picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }

            base.Dispose(disposing);
        }
    }
}
