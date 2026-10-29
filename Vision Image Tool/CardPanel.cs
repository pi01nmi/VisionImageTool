using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Thẻ nền trắng bo góc, viền mảnh (bố cục mockup tab Capture &amp; Monitor). Dùng được trong Designer:
    /// chỉnh BackColor, BorderColor, CornerRadius, Padding trong cửa sổ Properties.
    /// Góc ngoài tô bằng màu nền của control cha để bo góc trông liền với nền.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class CardPanel : Panel
    {
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private int _cornerRadius = 10;

        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Padding = new Padding(18, 10, 18, 12);
        }

        [Category("Appearance")]
        [Description("Màu viền thẻ.")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(10)]
        [Description("Bán kính bo góc (px).")]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value < 0 ? 0 : value; Invalidate(); }
        }

        private bool _hideButton;
        private bool _hideHot;
        private ToolTip _hideTip;

        /// <summary>Bấm nút "–" ở góc phải trên (chỉ khi HideButton = true).</summary>
        [Category("Action")]
        [Description("Bấm nút – (ẩn nhanh khu vực) ở góc phải trên của thẻ.")]
        public event System.EventHandler HideButtonClick;

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Hiện nút – ở góc phải trên để ẩn nhanh thẻ này (nằm trong lề phải - giữ Padding.Right >= 18).")]
        public bool HideButton
        {
            get { return _hideButton; }
            set { _hideButton = value; Invalidate(); }
        }

        [Category("Behavior")]
        [DefaultValue("Ẩn nhanh khu vực này  ·  hiện lại: ☰ Hiển thị")]
        [Description("Chú thích khi rê chuột lên nút –.")]
        public string HideButtonToolTip { get; set; } = "Ẩn nhanh khu vực này  ·  hiện lại: ☰ Hiển thị";

        /// <summary>Nút vẽ trong lề phải / trên của thẻ (không phải control con -> không chồng lên nội dung).</summary>
        private Rectangle HideButtonBounds
        {
            get { return new Rectangle(Width - 18, 4, 16, 16); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!_hideButton)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = HideButtonBounds;
            using (var path = Round(new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1), 4))
            using (var fill = new SolidBrush(Theme.Back(_hideHot ? Color.FromArgb(219, 234, 254) : Color.FromArgb(248, 250, 252))))
            using (var border = new Pen(Theme.Fore(_hideHot ? Color.FromArgb(96, 165, 250) : Color.FromArgb(203, 213, 225))))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            using (var pen = new Pen(Theme.Fore(_hideHot ? Color.FromArgb(29, 78, 216) : Color.FromArgb(71, 85, 105)), 2F))
            {
                int y = r.Y + r.Height / 2;
                g.DrawLine(pen, r.X + 4, y, r.Right - 5, y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            SetHideHot(_hideButton && HideButtonBounds.Contains(e.Location));
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            SetHideHot(false);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_hideButton && e.Button == MouseButtons.Left && HideButtonBounds.Contains(e.Location))
            {
                SetHideHot(false);
                HideButtonClick?.Invoke(this, System.EventArgs.Empty);
            }
        }

        private void SetHideHot(bool hot)
        {
            if (hot == _hideHot)
            {
                return;
            }

            _hideHot = hot;
            Cursor = hot ? Cursors.Hand : Cursors.Default;
            Invalidate(HideButtonBounds);
            if (hot && !string.IsNullOrEmpty(HideButtonToolTip))
            {
                if (_hideTip == null)
                {
                    _hideTip = new ToolTip();
                }

                var r = HideButtonBounds;
                _hideTip.Show(HideButtonToolTip, this, r.X - 150, r.Bottom + 4, 2500);
            }
            else if (_hideTip != null)
            {
                _hideTip.Hide(this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _hideTip != null)
            {
                _hideTip.Dispose();
                _hideTip = null;
            }

            base.Dispose(disposing);
        }

        private bool ShouldSerializeBorderColor()
        {
            return _borderColor != Color.FromArgb(226, 232, 240);
        }

        private void ResetBorderColor()
        {
            BorderColor = Color.FromArgb(226, 232, 240);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : SystemColors.Control);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Round(rect, _cornerRadius))
            using (var fill = new SolidBrush(BackColor))
            using (var pen = new Pen(_borderColor))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0 || r.Width <= 0 || r.Height <= 0)
            {
                path.AddRectangle(r);
                return path;
            }

            int d = radius * 2;
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
