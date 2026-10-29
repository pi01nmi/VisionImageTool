using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên KHÔNG phải control: Visual Studio mở file này bằng trình soạn code, không mở
    // Component Designer (control vẽ tay hoàn toàn, Designer không có gì để sửa).
    internal static class ZoomPictureBoxCodeFile { }

    /// <summary>
    /// PictureBox xem ảnh có zoom:
    ///   - Cuộn chuột      : phóng to / thu nhỏ quanh vị trí con trỏ.
    ///   - Kéo chuột trái  : di chuyển ảnh khi đang phóng to.
    ///   - Double-click    : về chế độ vừa khung (fit); đang fit thì phóng 100% tại chỗ bấm.
    ///   - Navigator       : khung thu nhỏ góc dưới phải khi đang zoom - hình chữ nhật đỏ là vùng
    ///                       đang xem, bấm / kéo trong navigator để nhảy tới vùng khác.
    ///
    /// Dùng thay PictureBox ở chỗ nào cũng được: vẫn gán ảnh qua thuộc tính Image như cũ (người gọi vẫn
    /// tự Dispose ảnh cũ). Ảnh mới cùng kích thước ảnh cũ thì GIỮ NGUYÊN mức zoom / vị trí đang xem -
    /// ảnh camera chụp liên tục vẫn soi được đúng một chỗ.
    /// </summary>
    public class ZoomPictureBox : PictureBox
    {
        private const float ZoomStep = 1.25f;
        private const float MaxZoom = 40f;
        private const int NavigatorMaxSize = 170;
        private const int NavigatorMargin = 10;

        private float _zoom = 1f;          // pixel màn hình / pixel ảnh
        private PointF _origin;            // vị trí góc trên-trái của ảnh trên control
        private bool _fit = true;
        private Size _lastImageSize = Size.Empty;

        private bool _panning;
        private System.Drawing.Point _panStart;
        private PointF _panOrigin;
        private bool _navDragging;

        private Bitmap _navThumb;          // ảnh thu nhỏ cho navigator (tạo 1 lần mỗi ảnh)

        public ZoomPictureBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Cursor = Cursors.Default;
        }

        /// <summary>Ảnh đang hiển thị. Thay ảnh khác kích thước thì tự về chế độ vừa khung.</summary>
        [DefaultValue(null)]
        public new Image Image
        {
            get { return base.Image; }
            set
            {
                base.Image = value;
                OnDisplayedImageChanged();
            }
        }

        private string _caption;
        private string _emptyText;

        /// <summary>Nhãn nhỏ ở góc trên phải (vd "RAW" / "MASK" khi xem song song), null = không vẽ.</summary>
        [DefaultValue(null)]
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; Invalidate(); }
        }

        /// <summary>Chữ hiện giữa khung khi chưa có ảnh (vd "No Mask Available"), null = để trống.</summary>
        [DefaultValue(null)]
        public string EmptyText
        {
            get { return _emptyText; }
            set { _emptyText = value; Invalidate(); }
        }

        /// <summary>Mức zoom hiện tại (1 = 100%).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ZoomFactor
        {
            get { return _zoom; }
        }

        /// <summary>Đang ở chế độ vừa khung.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFit
        {
            get { return _fit; }
        }

        /// <summary>Mức zoom / chế độ vừa khung vừa đổi (cuộn chuột, nút zoom, đổi ảnh, đổi kích thước).</summary>
        public event EventHandler ZoomChanged;

        /// <summary>Về chế độ vừa khung.</summary>
        public void ZoomToFit()
        {
            _fit = true;
            ApplyFit();
            Invalidate();
        }

        /// <summary>Phóng to / thu nhỏ quanh tâm khung (factor &gt; 1 = phóng to).</summary>
        public void ZoomBy(float factor)
        {
            ZoomAt(_zoom * factor, new System.Drawing.Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        /// <summary>Đặt mức zoom (1 = 100%) quanh tâm khung; nhỏ hơn mức vừa khung thì về vừa khung.</summary>
        public void SetZoom(float zoom)
        {
            ZoomAt(zoom, new System.Drawing.Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        private void OnDisplayedImageChanged()
        {
            ReleaseNavThumb();

            var image = base.Image;
            if (image == null)
            {
                _lastImageSize = Size.Empty;
                _fit = true;
                Invalidate();
                return;
            }

            Size size;
            try
            {
                size = image.Size;
            }
            catch (ArgumentException)
            {
                // Ảnh đã bị Dispose ở nơi khác.
                return;
            }

            if (size != _lastImageSize)
            {
                _fit = true;
            }

            _lastImageSize = size;

            if (_fit)
            {
                ApplyFit();
            }
            else
            {
                ClampOrigin();
            }

            BuildNavThumb(image);
            Invalidate();
        }

        #region Toạ độ

        private float FitZoom()
        {
            if (_lastImageSize.IsEmpty || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return 1f;
            }

            return Math.Min((float)ClientSize.Width / _lastImageSize.Width, (float)ClientSize.Height / _lastImageSize.Height);
        }

        private void ApplyFit()
        {
            _zoom = FitZoom();
            _origin = new PointF(
                (ClientSize.Width - _lastImageSize.Width * _zoom) / 2f,
                (ClientSize.Height - _lastImageSize.Height * _zoom) / 2f);
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Không cho kéo ảnh ra ngoài khung; chiều nào ảnh nhỏ hơn khung thì căn giữa.</summary>
        private void ClampOrigin()
        {
            float w = _lastImageSize.Width * _zoom;
            float h = _lastImageSize.Height * _zoom;

            float x = w <= ClientSize.Width
                ? (ClientSize.Width - w) / 2f
                : Math.Min(0, Math.Max(ClientSize.Width - w, _origin.X));
            float y = h <= ClientSize.Height
                ? (ClientSize.Height - h) / 2f
                : Math.Min(0, Math.Max(ClientSize.Height - h, _origin.Y));

            _origin = new PointF(x, y);
        }

        /// <summary>Zoom tới <paramref name="newZoom"/> mà giữ nguyên điểm ảnh nằm dưới <paramref name="anchor"/>.</summary>
        private void ZoomAt(float newZoom, System.Drawing.Point anchor)
        {
            if (_lastImageSize.IsEmpty)
            {
                return;
            }

            float fit = FitZoom();
            newZoom = Math.Min(MaxZoom, newZoom);

            if (newZoom <= fit * 1.0001f)
            {
                ZoomToFit();
                return;
            }

            float imageX = (anchor.X - _origin.X) / _zoom;
            float imageY = (anchor.Y - _origin.Y) / _zoom;

            _zoom = newZoom;
            _fit = false;
            _origin = new PointF(anchor.X - imageX * _zoom, anchor.Y - imageY * _zoom);
            ClampOrigin();
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Chuột

        // KHÔNG tự Focus() khi rê chuột vào: làm vậy thì danh sách ảnh bên cạnh mất focus, phím mũi
        // tên không chọn ảnh được nữa. Windows 10/11 mặc định gửi cuộn chuột tới control nằm dưới con
        // trỏ ("Scroll inactive windows when I hover over them") nên vẫn zoom được.
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (base.Image == null)
            {
                return;
            }

            float factor = e.Delta > 0 ? ZoomStep : 1f / ZoomStep;
            ZoomAt(_zoom * factor, e.Location);

            var handled = e as HandledMouseEventArgs;
            if (handled != null)
            {
                handled.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || base.Image == null)
            {
                return;
            }

            if (NavigatorVisible && NavigatorRect.Contains(e.Location))
            {
                _navDragging = true;
                CenterOnNavigatorPoint(e.Location);
                return;
            }

            if (!_fit)
            {
                _panning = true;
                _panStart = e.Location;
                _panOrigin = _origin;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_navDragging)
            {
                CenterOnNavigatorPoint(e.Location);
                return;
            }

            if (_panning)
            {
                _origin = new PointF(_panOrigin.X + e.X - _panStart.X, _panOrigin.Y + e.Y - _panStart.Y);
                ClampOrigin();
                Invalidate();
                return;
            }

            Cursor = !_fit && !(NavigatorVisible && NavigatorRect.Contains(e.Location)) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _panning = false;
            _navDragging = false;
            Cursor = _fit ? Cursors.Default : Cursors.Hand;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (base.Image == null || e.Button != MouseButtons.Left
                || (NavigatorVisible && NavigatorRect.Contains(e.Location)))
            {
                return;
            }

            if (_fit)
            {
                ZoomAt(1f, e.Location);   // đang vừa khung -> xem 100% tại chỗ bấm
            }
            else
            {
                ZoomToFit();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_fit)
            {
                ApplyFit();
            }
            else
            {
                ClampOrigin();
            }
        }

        #endregion

        #region Navigator

        private bool NavigatorVisible
        {
            get
            {
                return !_fit && _navThumb != null && ClientSize.Width > NavigatorMaxSize * 2
                    && ClientSize.Height > NavigatorMaxSize;
            }
        }

        private Rectangle NavigatorRect
        {
            get
            {
                if (_navThumb == null)
                {
                    return Rectangle.Empty;
                }

                return new Rectangle(
                    ClientSize.Width - _navThumb.Width - NavigatorMargin,
                    ClientSize.Height - _navThumb.Height - NavigatorMargin,
                    _navThumb.Width, _navThumb.Height);
            }
        }

        private void CenterOnNavigatorPoint(System.Drawing.Point location)
        {
            var nav = NavigatorRect;
            if (nav.IsEmpty)
            {
                return;
            }

            float imageX = (location.X - nav.X) * (float)_lastImageSize.Width / nav.Width;
            float imageY = (location.Y - nav.Y) * (float)_lastImageSize.Height / nav.Height;

            _origin = new PointF(ClientSize.Width / 2f - imageX * _zoom, ClientSize.Height / 2f - imageY * _zoom);
            ClampOrigin();
            Invalidate();
        }

        private void BuildNavThumb(Image image)
        {
            try
            {
                float scale = Math.Min((float)NavigatorMaxSize / image.Width, (float)NavigatorMaxSize / image.Height);
                int w = Math.Max(1, (int)(image.Width * scale));
                int h = Math.Max(1, (int)(image.Height * scale));

                _navThumb = new Bitmap(w, h);
                using (var g = Graphics.FromImage(_navThumb))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(image, 0, 0, w, h);
                }
            }
            catch
            {
                ReleaseNavThumb();
            }
        }

        private void ReleaseNavThumb()
        {
            if (_navThumb != null)
            {
                _navThumb.Dispose();
                _navThumb = null;
            }
        }

        #endregion

        #region Vẽ

        protected override void OnPaint(PaintEventArgs pe)
        {
            var g = pe.Graphics;
            g.Clear(BackColor);

            var image = base.Image;
            if (image == null || _lastImageSize.IsEmpty)
            {
                DrawEmptyText(g);
                DrawCaption(g);
                return;
            }

            try
            {
                // Phóng to nhiều thì vẽ điểm ảnh sắc nét (soi lỗi), còn lại làm mịn.
                g.InterpolationMode = _zoom >= 2f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.DrawImage(image, new RectangleF(_origin.X, _origin.Y,
                    _lastImageSize.Width * _zoom, _lastImageSize.Height * _zoom));
            }
            catch (ArgumentException)
            {
                // Ảnh vừa bị Dispose giữa chừng - lần vẽ sau sẽ có ảnh mới.
                return;
            }

            DrawOverlay(g);
            DrawCaption(g);
        }

        private void DrawEmptyText(Graphics g)
        {
            if (string.IsNullOrEmpty(_emptyText))
            {
                return;
            }

            using (var font = new Font("Segoe UI", 11F))
            {
                TextRenderer.DrawText(g, _emptyText, font, ClientRectangle, Color.FromArgb(156, 163, 175),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }

        private void DrawCaption(Graphics g)
        {
            if (string.IsNullOrEmpty(_caption))
            {
                return;
            }

            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                var size = g.MeasureString(_caption, font);
                var box = new RectangleF(ClientSize.Width - size.Width - 18, 8, size.Width + 10, size.Height + 4);
                using (var bg = new SolidBrush(Color.FromArgb(200, 37, 99, 235)))
                {
                    g.FillRectangle(bg, box);
                }

                g.DrawString(_caption, font, Brushes.White, box.X + 5, box.Y + 2);
            }
        }

        private void DrawOverlay(Graphics g)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Default;

            // Mức zoom ở góc trên trái.
            string zoomText = _fit ? "Vừa khung (" + Math.Round(_zoom * 100) + "%)" : Math.Round(_zoom * 100) + "%";
            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                var size = g.MeasureString(zoomText, font);
                var box = new RectangleF(8, 8, size.Width + 10, size.Height + 4);
                using (var bg = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                {
                    g.FillRectangle(bg, box);
                }
                g.DrawString(zoomText, font, Brushes.White, box.X + 5, box.Y + 2);
            }

            if (!NavigatorVisible)
            {
                return;
            }

            var nav = NavigatorRect;

            using (var shadow = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                g.FillRectangle(shadow, Rectangle.Inflate(nav, 3, 3));
            }

            g.DrawImage(_navThumb, nav);

            // Vùng đang xem, quy đổi về toạ độ navigator.
            float sx = (float)nav.Width / _lastImageSize.Width;
            float sy = (float)nav.Height / _lastImageSize.Height;
            float viewX = -_origin.X / _zoom;
            float viewY = -_origin.Y / _zoom;
            float viewW = ClientSize.Width / _zoom;
            float viewH = ClientSize.Height / _zoom;

            var view = RectangleF.FromLTRB(
                nav.X + Math.Max(0, viewX) * sx,
                nav.Y + Math.Max(0, viewY) * sy,
                nav.X + Math.Min(_lastImageSize.Width, viewX + viewW) * sx,
                nav.Y + Math.Min(_lastImageSize.Height, viewY + viewH) * sy);

            using (var dim = new System.Drawing.Region(nav))
            {
                dim.Exclude(view);
                using (var brush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                {
                    g.FillRegion(brush, dim);
                }
            }

            using (var pen = new Pen(Color.FromArgb(239, 68, 68), 2f))
            {
                g.DrawRectangle(pen, view.X, view.Y, Math.Max(2, view.Width), Math.Max(2, view.Height));
            }

            using (var border = new Pen(Color.FromArgb(200, 255, 255, 255)))
            {
                g.DrawRectangle(border, nav);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseNavThumb();
            }

            base.Dispose(disposing);
        }
    }
}
