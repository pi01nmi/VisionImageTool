using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerLayoutCodeFile { }

    /// <summary>Bố cục người dùng đã kéo (vạch chia, độ rộng cột) - lưu trong dataset_manager.json.</summary>
    internal sealed class DatasetLayout
    {
        /// <summary>Phiên bản bố cục: 2 = bố cục khung xem ảnh rộng (cũ hơn thì bỏ vạch chia đã lưu, về mặc định mới).</summary>
        public const int CurrentVersion = 2;
        public int? Version { get; set; }

        /// <summary>Tỉ lệ bảng / xem trước (0..1) - giữ đúng tỉ lệ khi cửa sổ đổi kích thước.</summary>
        public double? PreviewSplitRatio { get; set; }

        /// <summary>Bề rộng cây dataset (px).</summary>
        public int? TreeWidth { get; set; }

        /// <summary>Chiều cao khung danh sách vấn đề (px).</summary>
        public int? IssuesHeight { get; set; }

        /// <summary>
        /// Độ rộng cột theo tên cột ("key_Model", "colSuffix", "blk_Evaluation_ĐK1"...). Cột block còn lưu
        /// thêm khoá chung "blk_Evaluation" để DK mới xuất hiện cũng dùng độ rộng đã chỉnh.
        /// </summary>
        public Dictionary<string, int> ColumnWidths { get; set; }
    }

    public partial class ucDatasetManager
    {
        private DatasetLayout _layout = new DatasetLayout { Version = DatasetLayout.CurrentVersion };
        private SplitContainer _previewSplit;

        /// <summary>Đang dựng cột / áp bố cục đã lưu - thay đổi kích thước lúc này không phải do người dùng kéo.</summary>
        private bool _applyingLayout;
        private bool _layoutRestored;
        private readonly Timer _layoutSaveTimer = new Timer { Interval = 600 };

        private void SetupLayoutPersistence()
        {
            _layoutSaveTimer.Tick += (s, e) =>
            {
                _layoutSaveTimer.Stop();
                SaveSettings();
            };

            _previewSplit.SplitterMoved += (s, e) => OnSplitterMoved();
            _treeSplit.SplitterMoved += (s, e) => OnSplitterMoved();
            _issuesSplit.SplitterMoved += (s, e) => OnSplitterMoved();

            grid.ColumnWidthChanged += (s, e) =>
            {
                if (_applyingLayout || e.Column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                {
                    return;
                }

                if (_layout.ColumnWidths == null)
                {
                    _layout.ColumnWidths = new Dictionary<string, int>();
                }

                // Ghi cả các cột đang có để cột cùng loại của DK khác giữ nguyên độ rộng riêng
                // (khoá chung "blk_Evaluation" chỉ dành cho DK mới xuất hiện sau này).
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
                    {
                        _layout.ColumnWidths[column.Name] = column.Width;
                    }
                }

                var block = BlockOf(e.Column);
                if (block != null)
                {
                    _layout.ColumnWidths["blk_" + block.Part] = e.Column.Width;
                }

                ScheduleLayoutSave();
            };
        }

        /// <summary>Form chính sắp đóng: ghi ngay bố cục (không chờ bộ hẹn giờ).</summary>
        public void SaveLayout()
        {
            _layoutSaveTimer.Stop();
            if (_layoutRestored)
            {
                SaveSettings();
            }
        }

        private void OnSplitterMoved()
        {
            // SplitterMoved cũng nổ khi control tự co giãn lúc dựng - chỉ ghi sau khi đã khôi phục xong.
            if (!_layoutRestored || _applyingLayout)
            {
                return;
            }

            CaptureSplitLayout();
            ScheduleLayoutSave();
        }

        private void ScheduleLayoutSave()
        {
            _layoutSaveTimer.Stop();
            _layoutSaveTimer.Start();
        }

        /// <summary>Đọc vị trí các vạch chia đang hiện (khu vực đang ẩn thì giữ giá trị cũ).</summary>
        private void CaptureSplitLayout()
        {
            if (_previewSplit.Width > 0 && !_previewSplit.Panel2Collapsed)
            {
                _layout.PreviewSplitRatio = Math.Round((double)_previewSplit.SplitterDistance / _previewSplit.Width, 4);
            }

            if (!_treeSplit.Panel1Collapsed)
            {
                _layout.TreeWidth = _treeSplit.SplitterDistance;
            }

            // Khung Vấn đề đang tự thu gọn (không có vấn đề) thì không phải cỡ người dùng kéo - giữ giá trị cũ.
            if (!_issuesSplit.Panel2Collapsed && _issuesSplit.Height > 0 && !_issuesCompact && chkShowIssues.Checked)
            {
                _layout.IssuesHeight = _issuesSplit.Height - _issuesSplit.SplitterDistance - _issuesSplit.SplitterWidth;
            }
        }

        /// <summary>Gọi khi tab hiện lần đầu (kích thước đã thật): đặt lại các vạch chia theo lần trước.</summary>
        private void RestoreSplitLayout()
        {
            if (_layoutRestored)
            {
                return;
            }

            _applyingLayout = true;
            try
            {
                if (_previewSplit.Width > 0)
                {
                    double ratio = Math.Min(0.9, Math.Max(0.2, _layout.PreviewSplitRatio ?? DefaultPreviewSplitRatio));
                    TrySetDistance(_previewSplit, (int)(_previewSplit.Width * ratio));
                }

            }
            finally
            {
                _applyingLayout = false;
                _layoutRestored = true;
            }

            ApplyPanelSizes();
        }

        /// <summary>Cây / khung vấn đề (panel cố định kích thước) - chỉ đặt được khi đang hiện.</summary>
        private void ApplyPanelSizes()
        {
            if (!_layoutRestored)
            {
                return;
            }

            _applyingLayout = true;
            try
            {
                if (_layout.TreeWidth.HasValue && !_treeSplit.Panel1Collapsed)
                {
                    TrySetDistance(_treeSplit, _layout.TreeWidth.Value);
                }

            }
            finally
            {
                _applyingLayout = false;
            }

            ApplyIssuesHeight();
        }

        private static void TrySetDistance(SplitContainer split, int distance)
        {
            int size = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
            int max = size - split.Panel2MinSize - split.SplitterWidth;
            if (max <= split.Panel1MinSize)
            {
                return;
            }

            try
            {
                split.SplitterDistance = Math.Min(max, Math.Max(split.Panel1MinSize, distance));
            }
            catch
            {
                // Kích thước chưa hợp lệ - giữ mặc định.
            }
        }

        /// <summary>Áp độ rộng cột đã lưu sau khi dựng lại cột (đổi schema / thư mục / chế độ xem).</summary>
        private void ApplyColumnWidths()
        {
            var widths = _layout.ColumnWidths;
            if (widths == null || widths.Count == 0)
            {
                return;
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                {
                    continue;
                }

                int width;
                var block = BlockOf(column);
                if (widths.TryGetValue(column.Name, out width) ||
                    (block != null && widths.TryGetValue("blk_" + block.Part, out width)))
                {
                    column.Width = Math.Max(column.MinimumWidth, Math.Min(1000, width));
                }
            }
        }

        /// <summary>Bố cục để ghi vào dataset_manager.json (lấy vị trí vạch chia hiện tại nếu đã khôi phục).</summary>
        private DatasetLayout CurrentLayout()
        {
            if (_layoutRestored)
            {
                CaptureSplitLayout();
            }

            if (_layout.ColumnWidths != null && _layout.ColumnWidths.Count == 0)
            {
                _layout.ColumnWidths = null;
            }

            return _layout;
        }

        private void LoadLayout(DatasetLayout layout)
        {
            if (layout == null)
            {
                return;
            }

            _layout = layout;
            if ((_layout.Version ?? 1) < DatasetLayout.CurrentVersion)
            {
                // Bố cục cũ (bảng rộng, xem trước hẹp): bỏ vạch chia đã lưu để dùng bố cục mới.
                _layout.PreviewSplitRatio = null;
                _layout.TreeWidth = null;
            }

            _layout.Version = DatasetLayout.CurrentVersion;
            if (_layout.ColumnWidths != null)
            {
                _layout.ColumnWidths = new Dictionary<string, int>(_layout.ColumnWidths.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value));
            }

            _applyingLayout = true;
            try
            {
                ApplyColumnWidths();
            }
            finally
            {
                _applyingLayout = false;
            }
        }
    }
}
