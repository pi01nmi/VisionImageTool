using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio nhờ đó luôn mở file này bằng trình soạn
    // code (xem ghi chú đầu frmMainV1.View.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1LayoutCodeFile { }

    /// <summary>
    /// Bố cục cửa sổ lưu trong settings.json: vị trí / kích thước / phóng to, sidebar, danh sách Raw/Mask và
    /// vạch chia của tab Capture. Bố cục tab Dataset Manager nằm trong dataset_manager.json.
    /// </summary>
    public partial class frmMainV1
    {
        private sealed class WindowLayout
        {
            /// <summary>Vị trí + kích thước ở trạng thái thường (không phóng to).</summary>
            public Rectangle? Bounds { get; set; }
            public bool? Maximized { get; set; }
            /// <summary>Bề rộng danh sách Raw / Mask (px).</summary>
            public int? ListsWidth { get; set; }

            /// <summary>Khu vực tab Capture đang ẩn (☰ Hiển thị ở thanh dưới): Part, Control, Camera, List, Storage, Network.</summary>
            public List<string> HiddenCaptureSections { get; set; }

            // Bố cục cũ (sidebar trái + ẩn danh sách) - chỉ còn đọc để chuyển sang HiddenCaptureSections.
            public bool? SidebarVisible { get; set; }
            public bool? ListsCollapsed { get; set; }
        }

        private WindowLayout CaptureWindowLayout()
        {
            var layout = new WindowLayout
            {
                Bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds,
                Maximized = WindowState == FormWindowState.Maximized,
                HiddenCaptureSections = HiddenCaptureSections()
            };

            if (!scWork.Panel1Collapsed)
            {
                layout.ListsWidth = scWork.SplitterDistance;
            }
            else if (_savedListsWidth.HasValue)
            {
                layout.ListsWidth = _savedListsWidth;
            }

            return layout;
        }

        private int? _savedListsWidth;

        private void RestoreWindowLayout(WindowLayout layout)
        {
            if (layout == null)
            {
                return;
            }

            // Chỉ dùng vị trí cũ khi nó vẫn nằm trên một màn hình đang cắm (tránh cửa sổ "biến mất" khi rút màn phụ).
            if (layout.Bounds.HasValue)
            {
                var rect = layout.Bounds.Value;
                rect.Width = Math.Max(MinimumSize.Width, rect.Width);
                rect.Height = Math.Max(MinimumSize.Height, rect.Height);

                bool visible = Screen.AllScreens.Any(s =>
                {
                    var overlap = Rectangle.Intersect(s.WorkingArea, rect);
                    return overlap.Width >= 200 && overlap.Height >= 100;
                });

                if (visible)
                {
                    StartPosition = FormStartPosition.Manual;
                    Bounds = rect;
                }
            }

            if (layout.Maximized == true)
            {
                WindowState = FormWindowState.Maximized;
            }

            if (layout.ListsWidth.HasValue && layout.ListsWidth.Value >= scWork.Panel1MinSize)
            {
                _savedListsWidth = layout.ListsWidth;
                try
                {
                    scWork.SplitterDistance = layout.ListsWidth.Value;
                }
                catch
                {
                    // Kích thước chưa hợp lệ - giữ mặc định.
                }
            }

            // Khu vực đang ẩn; file cấu hình cũ: "ẩn danh sách" -> ẩn khu vực Danh sách ảnh.
            var hidden = layout.HiddenCaptureSections ?? new List<string>();
            if (layout.HiddenCaptureSections == null && layout.ListsCollapsed == true)
            {
                hidden.Add("List");
            }

            RestoreCaptureSections(hidden);
        }
    }
}
