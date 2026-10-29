using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace VisionImageTool
{
    internal enum AppTheme
    {
        Light,
        Dark
    }

    /// <summary>
    /// Quản lý giao diện Sáng / Tối cho toàn ứng dụng.
    ///
    /// Giao diện được thiết kế ở chế độ Sáng (màu trong Designer và trong code). Chế độ Tối KHÔNG khai
    /// báo lại từng màu mà quy đổi tự động theo độ sáng:
    ///   - Nền sáng (trắng, xám nhạt, nền pastel O/Δ/X...) → nền tối cùng tông màu.
    ///   - Chữ tối → chữ sáng; chữ có màu (đỏ, xanh...) được làm sáng lên để đọc rõ trên nền tối.
    ///   - Màu nhấn độ sáng trung bình (nút xanh, nút xanh lá...) và nền vốn đã tối (khung xem ảnh) giữ nguyên.
    ///
    /// Màu gốc (Sáng) của mỗi control được ghi nhớ, nên chuyển qua lại không bị lệch màu. Màu do code
    /// đổi lúc chạy (nút bật / tắt, nhãn OK/NG...) được bắt qua sự kiện BackColorChanged / ForeColorChanged
    /// và quy đổi lại. Ô DataGridView quy đổi lúc vẽ (CellFormatting). Chỗ tự vẽ bằng GDI+ gọi
    /// Theme.Back / Theme.Fore.
    ///
    /// Ảnh Raw / Mask không bị ảnh hưởng: chỉ màu control thay đổi, bitmap giữ nguyên.
    /// </summary>
    internal static class Theme
    {
        public static AppTheme Current { get; private set; } = AppTheme.Light;

        public static bool IsDark
        {
            get { return Current == AppTheme.Dark; }
        }

        /// <summary>Đổi giao diện xong - nơi tự vẽ / tự tô màu từng dòng (ListView) vẽ lại.</summary>
        public static event Action Changed;

        /// <summary>Đổi giao diện cho mọi cửa sổ đang mở.</summary>
        public static void Set(AppTheme theme)
        {
            if (theme == Current)
            {
                return;
            }

            Current = theme;
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                Apply(form);
            }

            if (Changed != null)
            {
                Changed();
            }
        }

        #region Quy đổi màu

        /// <summary>Màu nền thiết kế ở chế độ Sáng → màu nền theo giao diện hiện tại.</summary>
        public static Color Back(Color light)
        {
            if (!IsDark || light.IsEmpty || light.A == 0 || light == Color.Transparent)
            {
                return light;
            }

            double h, s, l;
            ToHsl(light, out h, out s, out l);

            if (l >= 0.80)
            {
                // Trắng → 0.12, xám nhạt → hơi sáng hơn một chút; nền pastel giữ tông, giảm bão hoà.
                double dark = 0.12 + (1.0 - l) * 0.55;
                return FromHsl(h, Math.Min(s, 1.0) * (s > 0.2 ? 0.45 : 0.3), dark, light.A);
            }

            return light;
        }

        /// <summary>Màu chữ / nét vẽ thiết kế ở chế độ Sáng → màu theo giao diện hiện tại.</summary>
        public static Color Fore(Color light)
        {
            if (!IsDark || light.IsEmpty || light.A == 0)
            {
                return light;
            }

            double h, s, l;
            ToHsl(light, out h, out s, out l);

            if (s < 0.25)
            {
                // Chữ đen / xám: đảo độ sáng, xám nhạt (chữ mờ) vẫn mờ hơn chữ chính.
                return l >= 0.8 ? light : FromHsl(h, s, Math.Max(0.55, 0.93 - l * 0.55), light.A);
            }

            // Chữ có màu: giữ tông, nâng độ sáng để đọc rõ trên nền tối.
            return FromHsl(h, s, Math.Max(l, 0.66), light.A);
        }

        private static void ToHsl(Color c, out double h, out double s, out double l)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2;
            if (max == min)
            {
                h = s = 0;
                return;
            }

            double d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;
            h /= 6;
        }

        private static Color FromHsl(double h, double s, double l, int alpha)
        {
            double r, g, b;
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = Hue(p, q, h + 1.0 / 3);
                g = Hue(p, q, h);
                b = Hue(p, q, h - 1.0 / 3);
            }

            return Color.FromArgb(alpha, Clamp(r), Clamp(g), Clamp(b));
        }

        private static double Hue(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        private static int Clamp(double v)
        {
            return Math.Max(0, Math.Min(255, (int)Math.Round(v * 255)));
        }

        #endregion

        #region Áp giao diện cho control

        /// <summary>Màu gốc (thiết kế ở chế độ Sáng) của một control / mục menu.</summary>
        private sealed class Original
        {
            public Color Back;
            public Color Fore;
            public FlatStyle? FlatStyle;
            public Color? FlatBorder;
            public BorderStyle? Border;
            public ToolStripRenderer Renderer;
            public bool Hooked;
        }

        private static readonly ConditionalWeakTable<object, Original> Originals = new ConditionalWeakTable<object, Original>();

        /// <summary>Đang đổi màu (không coi là code đổi màu).</summary>
        [ThreadStatic]
        private static bool _applying;

        /// <summary>Gắn giao diện cho một cửa sổ: áp ngay + thanh tiêu đề tối khi handle được tạo.</summary>
        public static void Attach(Form form)
        {
            Apply(form);
            form.HandleCreated += (s, e) => ApplyTitleBar(form);
            if (form.IsHandleCreated)
            {
                ApplyTitleBar(form);
            }
        }

        /// <summary>Áp giao diện hiện tại cho control và toàn bộ control con.</summary>
        public static void Apply(Control root)
        {
            if (root == null)
            {
                return;
            }

            ApplyOne(root);
            foreach (Control child in root.Controls)
            {
                Apply(child);
            }

            if (root.ContextMenuStrip != null)
            {
                ApplyStrip(root.ContextMenuStrip);
            }

            var form = root as Form;
            if (form != null && form.IsHandleCreated)
            {
                ApplyTitleBar(form);
            }
        }

        private static Original GetOriginal(Control control)
        {
            Original original;
            if (!Originals.TryGetValue(control, out original))
            {
                original = new Original { Back = control.BackColor, Fore = control.ForeColor };
                Originals.Add(control, original);
            }

            return original;
        }

        private static void ApplyOne(Control control)
        {
            var original = GetOriginal(control);

            if (!original.Hooked)
            {
                original.Hooked = true;

                // Code đổi màu lúc chạy (màu thiết kế cho Sáng) → ghi nhận làm màu gốc mới rồi quy đổi.
                control.BackColorChanged += (s, e) =>
                {
                    if (!_applying)
                    {
                        original.Back = control.BackColor;
                        if (IsDark)
                        {
                            SetColors(control, original);
                        }
                    }
                };
                control.ForeColorChanged += (s, e) =>
                {
                    if (!_applying)
                    {
                        original.Fore = control.ForeColor;
                        if (IsDark)
                        {
                            SetColors(control, original);
                        }
                    }
                };

                // Control thêm sau (danh sách động, bảng chia đôi...) cũng nhận giao diện.
                control.ControlAdded += (s, e) => Apply(e.Control);

                if (control is DataGridView)
                {
                    ((DataGridView)control).CellFormatting += Grid_CellFormatting;
                }
            }

            SetColors(control, original);
            ApplySpecial(control, original);
        }

        private static void SetColors(Control control, Original original)
        {
            _applying = true;
            try
            {
                // Label / CheckBox trong suốt thì giữ trong suốt.
                if (!original.Back.IsEmpty && original.Back.A > 0 && !(control is Label && original.Back == Color.Transparent))
                {
                    var back = Back(original.Back);
                    if (control.BackColor != back)
                    {
                        control.BackColor = back;
                    }
                }

                var fore = Fore(original.Fore);
                if (control.ForeColor != fore)
                {
                    control.ForeColor = fore;
                }
            }
            catch (ArgumentException)
            {
                // Control không nhận màu nền trong suốt... - bỏ qua.
            }
            finally
            {
                _applying = false;
            }
        }

        private static void ApplySpecial(Control control, Original original)
        {
            var button = control as ButtonBase;
            if (button != null)
            {
                // Nút kiểu hệ thống không đổi màu nền được - chuyển sang Flat khi tối.
                if (original.FlatStyle == null)
                {
                    original.FlatStyle = button.FlatStyle;
                    original.FlatBorder = button.FlatAppearance.BorderColor;
                }

                if (control is Button)
                {
                    if (IsDark && original.FlatStyle != FlatStyle.Flat)
                    {
                        button.FlatStyle = FlatStyle.Flat;
                        button.UseVisualStyleBackColor = false;
                    }
                    else if (!IsDark)
                    {
                        button.FlatStyle = original.FlatStyle.Value;
                    }

                    var border = original.FlatBorder.Value;
                    button.FlatAppearance.BorderColor = IsDark
                        ? (border.IsEmpty ? Color.FromArgb(75, 85, 99) : Back(border) == border ? border : Color.FromArgb(75, 85, 99))
                        : border;
                }
            }

            var combo = control as ComboBox;
            if (combo != null)
            {
                if (original.FlatStyle == null)
                {
                    original.FlatStyle = combo.FlatStyle;
                }

                // Kiểu có viền 3D của Windows bỏ qua BackColor ở ô đóng - dùng Flat khi tối.
                combo.FlatStyle = IsDark ? FlatStyle.Flat : original.FlatStyle.Value;
            }

            var grid = control as DataGridView;
            if (grid != null)
            {
                ApplyGrid(grid);
            }

            var chart = control as Chart;
            if (chart != null)
            {
                ApplyChart(chart);
            }

            var strip = control as ToolStrip;
            if (strip != null)
            {
                ApplyStrip(strip);
            }

            var list = control as ListView;
            if (list != null)
            {
                list.Invalidate();
            }

            if (control is ListView || control is TreeView || control is DataGridView || control is TextBoxBase
                || control is ScrollBar)
            {
                ApplyScrollTheme(control);
            }

            var tab = control as TabControl;
            if (tab != null)
            {
                ApplyTabs(tab);
            }
        }

        #endregion

        #region DataGridView

        /// <summary>
        /// Style thiết kế (Sáng) của bảng được nhớ trong Tag riêng rồi quy đổi, nên bật / tắt nhiều lần
        /// không bị lệch màu.
        /// </summary>
        private sealed class GridOriginal
        {
            public Color Background;
            public Color GridColor;
            public DataGridViewCellStyle Cells;
            public DataGridViewCellStyle Headers;
        }

        private static readonly ConditionalWeakTable<DataGridView, GridOriginal> GridOriginals = new ConditionalWeakTable<DataGridView, GridOriginal>();

        private static void ApplyGrid(DataGridView grid)
        {
            GridOriginal original;
            if (!GridOriginals.TryGetValue(grid, out original))
            {
                original = new GridOriginal
                {
                    Background = grid.BackgroundColor,
                    GridColor = grid.GridColor,
                    Cells = new DataGridViewCellStyle(grid.DefaultCellStyle),
                    Headers = new DataGridViewCellStyle(grid.ColumnHeadersDefaultCellStyle)
                };
                GridOriginals.Add(grid, original);
            }

            grid.BackgroundColor = Back(original.Background);
            grid.GridColor = IsDark ? Color.FromArgb(55, 60, 70) : original.GridColor;
            MapStyle(grid.DefaultCellStyle, original.Cells);
            MapStyle(grid.ColumnHeadersDefaultCellStyle, original.Headers);
            grid.Invalidate();
        }

        private static void MapStyle(DataGridViewCellStyle target, DataGridViewCellStyle light)
        {
            target.BackColor = Back(light.BackColor.IsEmpty ? Color.White : light.BackColor);
            target.ForeColor = Fore(light.ForeColor.IsEmpty ? Color.Black : light.ForeColor);
            if (!light.SelectionBackColor.IsEmpty)
            {
                target.SelectionBackColor = IsDark ? Color.FromArgb(37, 72, 130) : light.SelectionBackColor;
            }

            if (!light.SelectionForeColor.IsEmpty)
            {
                target.SelectionForeColor = IsDark ? Color.White : light.SelectionForeColor;
            }
        }

        /// <summary>Màu từng ô (nền O/Δ/X, ô xám, dòng đang chọn...) quy đổi ngay lúc vẽ.</summary>
        private static void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!IsDark || e.CellStyle == null)
            {
                return;
            }

            e.CellStyle.BackColor = Back(e.CellStyle.BackColor);
            e.CellStyle.ForeColor = Fore(e.CellStyle.ForeColor);
        }

        #endregion

        #region Chart, ToolStrip, Tab, thanh cuộn, thanh tiêu đề

        private static void ApplyChart(Chart chart)
        {
            var back = Back(Color.White);
            var text = Fore(Color.FromArgb(55, 65, 81));
            var line = IsDark ? Color.FromArgb(75, 85, 99) : Color.FromArgb(209, 213, 219);
            var grid = IsDark ? Color.FromArgb(45, 50, 58) : Color.FromArgb(243, 244, 246);

            chart.BackColor = back;
            foreach (var area in chart.ChartAreas)
            {
                area.BackColor = back;
                area.AxisX.LineColor = area.AxisY.LineColor = line;
                area.AxisY.MajorGrid.LineColor = grid;
                area.AxisY.LabelStyle.ForeColor = text;
            }

            foreach (var legend in chart.Legends)
            {
                legend.BackColor = back;
                legend.ForeColor = text;
            }

            foreach (var title in chart.Titles)
            {
                title.ForeColor = Fore(Color.FromArgb(17, 24, 39));
            }

            foreach (var series in chart.Series)
            {
                series.LabelForeColor = text;
            }
        }

        /// <summary>Menu, menu chuột phải, thanh trạng thái.</summary>
        public static void ApplyStrip(ToolStrip strip)
        {
            Original original;
            if (!Originals.TryGetValue(strip, out original))
            {
                original = new Original { Back = strip.BackColor, Fore = strip.ForeColor, Renderer = strip.Renderer };
                Originals.Add(strip, original);
                strip.ItemAdded += (s, e) => ApplyItem(e.Item);
            }

            if (IsDark)
            {
                strip.Renderer = new ToolStripProfessionalRenderer(new DarkColors());
            }
            else if (original.Renderer != null)
            {
                strip.Renderer = original.Renderer;
            }

            _applying = true;
            try
            {
                strip.BackColor = Back(original.Back);
                strip.ForeColor = Fore(original.Fore);
            }
            finally
            {
                _applying = false;
            }

            foreach (ToolStripItem item in strip.Items)
            {
                ApplyItem(item);
            }
        }

        private static void ApplyItem(ToolStripItem item)
        {
            Original original;
            if (!Originals.TryGetValue(item, out original))
            {
                original = new Original { Back = item.BackColor, Fore = item.ForeColor };
                Originals.Add(item, original);

                item.ForeColorChanged += (s, e) =>
                {
                    if (!_applying)
                    {
                        original.Fore = item.ForeColor;
                        if (IsDark)
                        {
                            SetItemColors(item, original);
                        }
                    }
                };
                item.BackColorChanged += (s, e) =>
                {
                    if (!_applying)
                    {
                        original.Back = item.BackColor;
                        if (IsDark)
                        {
                            SetItemColors(item, original);
                        }
                    }
                };
            }

            SetItemColors(item, original);

            var menu = item as ToolStripDropDownItem;
            if (menu != null && menu.HasDropDownItems)
            {
                ApplyStrip(menu.DropDown);
            }
        }

        private static void SetItemColors(ToolStripItem item, Original original)
        {
            _applying = true;
            try
            {
                item.ForeColor = Fore(original.Fore);
                if (!original.Back.IsEmpty)
                {
                    item.BackColor = Back(original.Back);
                }
            }
            finally
            {
                _applying = false;
            }
        }

        /// <summary>Bảng màu tối cho menu / thanh công cụ / thanh trạng thái.</summary>
        private sealed class DarkColors : ProfessionalColorTable
        {
            private static readonly Color Surface = Color.FromArgb(32, 34, 38);
            private static readonly Color Raised = Color.FromArgb(44, 47, 53);
            private static readonly Color Hover = Color.FromArgb(37, 72, 130);
            private static readonly Color Line = Color.FromArgb(70, 75, 85);

            public override Color ToolStripDropDownBackground { get { return Raised; } }
            public override Color ImageMarginGradientBegin { get { return Raised; } }
            public override Color ImageMarginGradientMiddle { get { return Raised; } }
            public override Color ImageMarginGradientEnd { get { return Raised; } }
            public override Color MenuBorder { get { return Line; } }
            public override Color MenuItemBorder { get { return Hover; } }
            public override Color MenuItemSelected { get { return Hover; } }
            public override Color MenuItemSelectedGradientBegin { get { return Hover; } }
            public override Color MenuItemSelectedGradientEnd { get { return Hover; } }
            public override Color MenuItemPressedGradientBegin { get { return Raised; } }
            public override Color MenuItemPressedGradientEnd { get { return Raised; } }
            public override Color MenuStripGradientBegin { get { return Surface; } }
            public override Color MenuStripGradientEnd { get { return Surface; } }
            public override Color StatusStripGradientBegin { get { return Surface; } }
            public override Color StatusStripGradientEnd { get { return Surface; } }
            public override Color ToolStripGradientBegin { get { return Surface; } }
            public override Color ToolStripGradientMiddle { get { return Surface; } }
            public override Color ToolStripGradientEnd { get { return Surface; } }
            public override Color ToolStripBorder { get { return Line; } }
            public override Color SeparatorDark { get { return Line; } }
            public override Color SeparatorLight { get { return Line; } }
            public override Color ButtonSelectedHighlight { get { return Hover; } }
            public override Color ButtonSelectedGradientBegin { get { return Hover; } }
            public override Color ButtonSelectedGradientEnd { get { return Hover; } }
            public override Color CheckBackground { get { return Hover; } }
        }

        /// <summary>
        /// Tiêu đề thẻ (tab) tự vẽ khi tối: TabControl của Windows không nhận màu nền cho phần tiêu đề.
        /// </summary>
        private static void ApplyTabs(TabControl tab)
        {
            Original original;
            if (!Originals.TryGetValue(tab, out original) || original.Border == null)
            {
                original = GetOriginal(tab);
                original.Border = BorderStyle.None;   // đánh dấu đã gắn DrawItem
                tab.DrawItem += (s, e) =>
                {
                    bool selected = e.Index == tab.SelectedIndex;
                    var bounds = tab.GetTabRect(e.Index);
                    using (var back = new SolidBrush(selected ? Color.FromArgb(44, 47, 53) : Color.FromArgb(28, 30, 34)))
                    {
                        e.Graphics.FillRectangle(back, bounds);
                    }

                    if (selected)
                    {
                        using (var accent = new SolidBrush(Color.FromArgb(59, 130, 246)))
                        {
                            e.Graphics.FillRectangle(accent, bounds.X, bounds.Bottom - 3, bounds.Width, 3);
                        }
                    }

                    TextRenderer.DrawText(e.Graphics, tab.TabPages[e.Index].Text, tab.Font, bounds,
                        selected ? Color.White : Color.FromArgb(170, 176, 186),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                };

                new TabStripPainter(tab);
            }

            tab.DrawMode = IsDark ? TabDrawMode.OwnerDrawFixed : TabDrawMode.Normal;
            tab.Invalidate();
        }

        /// <summary>
        /// Vẽ lại dải nền phía sau các tiêu đề thẻ và viền khung thẻ khi tối: phần này TabControl của
        /// Windows luôn tô màu hệ thống (sáng), kể cả khi tự vẽ tiêu đề.
        /// </summary>
        private sealed class TabStripPainter : NativeWindow
        {
            private const int WmPaint = 0x000F;
            private readonly TabControl _tab;

            public TabStripPainter(TabControl tab)
            {
                _tab = tab;
                if (tab.IsHandleCreated)
                {
                    AssignHandle(tab.Handle);
                }

                tab.HandleCreated += (s, e) => AssignHandle(tab.Handle);
                tab.HandleDestroyed += (s, e) => ReleaseHandle();
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg != WmPaint || !IsDark || _tab.TabCount == 0)
                {
                    return;
                }

                var last = _tab.GetTabRect(_tab.TabCount - 1);
                var back = _tab.Parent != null ? _tab.Parent.BackColor : Color.FromArgb(28, 30, 34);
                using (var g = Graphics.FromHwnd(_tab.Handle))
                using (var brush = new SolidBrush(back))
                using (var pen = new Pen(back, 4))
                {
                    g.FillRectangle(brush, last.Right, 0, _tab.Width - last.Right, last.Bottom + 2);
                    g.DrawRectangle(pen, 1, last.Bottom + 1, _tab.Width - 3, _tab.Height - last.Bottom - 3);
                }
            }
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>Thanh cuộn tối (Windows 10 1809+). Không hỗ trợ thì giữ nguyên.</summary>
        private static void ApplyScrollTheme(Control control)
        {
            Action apply = () =>
            {
                try
                {
                    SetWindowTheme(control.Handle, IsDark ? "DarkMode_Explorer" : "Explorer", null);
                }
                catch
                {
                    // Windows cũ - bỏ qua.
                }
            };

            if (control.IsHandleCreated)
            {
                apply();
            }
            else
            {
                control.HandleCreated += (s, e) => apply();
            }
        }

        /// <summary>Thanh tiêu đề cửa sổ tối (Windows 10 20H1+ / Windows 11).</summary>
        private static void ApplyTitleBar(Form form)
        {
            try
            {
                int value = IsDark ? 1 : 0;
                if (DwmSetWindowAttribute(form.Handle, 20, ref value, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(form.Handle, 19, ref value, sizeof(int));
                }
            }
            catch
            {
                // Windows cũ - bỏ qua.
            }
        }

        #endregion
    }
}
