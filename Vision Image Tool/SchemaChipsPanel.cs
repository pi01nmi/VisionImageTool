using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisionImageTool
{
    /// <summary>
    /// Vẽ schema thành các thẻ: trường tên gọi / phân cấp (Model, Condition, SPL...) là thẻ xám,
    /// KHÔNG kèm chú thích; chỉ trường có ý nghĩa cố định mới có màu riêng + dòng gợi ý nhỏ bên
    /// dưới (Raw/Mask tím, Evaluation xanh lá, Hậu tố cam). Ký tự phân cách vẽ mờ giữa các thẻ.
    /// </summary>
    [ToolboxItem(false)]
    [DesignerCategory("Code")]
    internal sealed class SchemaChipsPanel : Panel
    {
        public static readonly Color TypeColor = Color.FromArgb(124, 58, 237);
        public static readonly Color EvalColor = Color.FromArgb(21, 128, 61);
        public static readonly Color SuffixColor = Color.FromArgb(180, 83, 9);
        public static readonly Color ConditionColor = Color.FromArgb(29, 78, 216);
        public static readonly Color ExtensionColor = Color.FromArgb(15, 118, 110);
        public static readonly Color ShotColor = Color.FromArgb(190, 24, 93);

        private DatasetSchema _schema;
        private string _error;

        public SchemaChipsPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        public void SetSchema(DatasetSchema schema, string error)
        {
            _schema = schema;
            _error = error;
            Invalidate();
        }

        private static void RoleColors(DatasetFieldRole role, out Color fill, out Color border, out Color text)
        {
            switch (role)
            {
                case DatasetFieldRole.Type:
                    fill = Color.FromArgb(237, 233, 254); border = TypeColor; text = TypeColor; break;
                case DatasetFieldRole.Evaluation:
                    fill = Color.FromArgb(220, 252, 231); border = EvalColor; text = EvalColor; break;
                case DatasetFieldRole.Suffix:
                    fill = Color.FromArgb(254, 243, 199); border = SuffixColor; text = SuffixColor; break;
                case DatasetFieldRole.Condition:
                    fill = Color.FromArgb(219, 234, 254); border = ConditionColor; text = ConditionColor; break;
                case DatasetFieldRole.Shot:
                    fill = Color.FromArgb(252, 231, 243); border = ShotColor; text = ShotColor; break;
                default:
                    fill = Color.FromArgb(243, 244, 246); border = Color.FromArgb(209, 213, 219); text = Color.FromArgb(31, 41, 55); break;
            }

            fill = Theme.Back(fill);
            border = Theme.Fore(border);
            text = Theme.Fore(text);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var chipFont = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold))
            using (var hintFont = new Font("Segoe UI", 7.5F))
            using (var sepFont = new Font("Consolas", 11F, FontStyle.Bold))
            using (var sepBrush = new SolidBrush(Theme.Fore(Color.FromArgb(156, 163, 175))))
            {
                if (_schema == null)
                {
                    using (var red = new SolidBrush(Theme.Fore(Color.FromArgb(185, 28, 28))))
                    {
                        string message = "✖ " + (_error ?? "Schema chưa hợp lệ");
                        g.DrawString(message, chipFont, red, 2, 8);
                        SetContentEnd(2 + g.MeasureString(message, chipFont).Width);
                    }
                    return;
                }

                float x = 2;
                const float chipY = 2;
                const float chipH = 24;

                foreach (var field in _schema.Fields)
                {
                    if (!string.IsNullOrEmpty(field.LiteralBefore))
                    {
                        var sepSize = g.MeasureString(field.LiteralBefore, sepFont);
                        g.DrawString(field.LiteralBefore, sepFont, sepBrush, x, chipY + 2);
                        x += sepSize.Width - 2;
                    }

                    Color fill, border, text;
                    RoleColors(field.Role, out fill, out border, out text);

                    var textSize = g.MeasureString(field.Name, chipFont);
                    string hint = field.Implicit ? "tự đọc tên file" : DatasetSchema.RoleHint(field.Role);
                    float hintWidth = hint == null ? 0 : g.MeasureString(hint, hintFont).Width;
                    float chipW = Math.Max(textSize.Width + 14, hintWidth + 4);
                    var rect = new RectangleF(x, chipY, chipW, chipH);

                    // Viền liền = cấp thư mục, viền đứt = trường trong tên file (chỉ phân biệt khi schema có thư mục).
                    bool inFileName = _schema.IsFolderSchema && !field.InFolder;
                    using (var path = RoundedRect(rect, chipH / 2))
                    using (var fillBrush = new SolidBrush(fill))
                    using (var pen = new Pen(border, 1.2F) { DashStyle = inFileName ? System.Drawing.Drawing2D.DashStyle.Dash : System.Drawing.Drawing2D.DashStyle.Solid })
                    {
                        g.FillPath(fillBrush, path);
                        g.DrawPath(pen, path);
                    }

                    using (var textBrush = new SolidBrush(text))
                    {
                        g.DrawString(field.Name, chipFont, textBrush, x + (chipW - textSize.Width) / 2, chipY + (chipH - textSize.Height) / 2);

                        if (hint != null)
                        {
                            g.DrawString(hint, hintFont, textBrush, x + (chipW - hintWidth) / 2, chipY + chipH + 1);
                        }
                    }

                    x += chipW + 2;
                }

                if (!string.IsNullOrEmpty(_schema.TrailingLiteral))
                {
                    g.DrawString(_schema.TrailingLiteral, sepFont, sepBrush, x, chipY + 2);
                    x += g.MeasureString(_schema.TrailingLiteral, sepFont).Width;
                }

                // Phần đuôi sau <Đánh giá> / <Raw/Mask> được bỏ qua (vd "_1"): thẻ xám viền đứt "_…".
                if (_schema.IgnoresTail)
                {
                    const string tailHint = "bỏ qua";
                    var textSize = g.MeasureString("_…", chipFont);
                    float hintWidth = g.MeasureString(tailHint, hintFont).Width;
                    float chipW = Math.Max(textSize.Width + 14, hintWidth + 4);
                    var rect = new RectangleF(x, chipY, chipW, chipH);
                    var color = Theme.Fore(Color.FromArgb(148, 163, 184));
                    using (var path = RoundedRect(rect, chipH / 2))
                    using (var pen = new Pen(color, 1.2F) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    using (var textBrush = new SolidBrush(color))
                    {
                        g.DrawPath(pen, path);
                        g.DrawString("_…", chipFont, textBrush, x + (chipW - textSize.Width) / 2, chipY + (chipH - textSize.Height) / 2);
                        g.DrawString(tailHint, hintFont, textBrush, x + (chipW - hintWidth) / 2, chipY + chipH + 1);
                    }

                    x += chipW + 2;
                }

                // <Extension>: dấu chấm + thẻ "Extension" (đuôi ảnh tự nhận).
                if (_schema.HasExtension)
                {
                    g.DrawString(".", sepFont, sepBrush, x, chipY + 2);
                    x += g.MeasureString(".", sepFont).Width - 4;

                    const string extHint = "jpg · png · bmp · tif";
                    var textSize = g.MeasureString("Extension", chipFont);
                    float hintWidth = g.MeasureString(extHint, hintFont).Width;
                    float chipW = Math.Max(textSize.Width + 14, hintWidth + 4);
                    var rect = new RectangleF(x, chipY, chipW, chipH);
                    var color = Theme.Fore(ExtensionColor);
                    using (var path = RoundedRect(rect, 6))
                    using (var fillBrush = new SolidBrush(Theme.Back(Color.FromArgb(204, 251, 241))))
                    using (var pen = new Pen(color))
                    using (var textBrush = new SolidBrush(color))
                    {
                        g.FillPath(fillBrush, path);
                        g.DrawPath(pen, path);
                        g.DrawString("Extension", chipFont, textBrush, x + (chipW - textSize.Width) / 2, chipY + (chipH - textSize.Height) / 2);
                        g.DrawString(extHint, hintFont, textBrush, x + (chipW - hintWidth) / 2, chipY + chipH + 1);
                    }

                    x += chipW + 2;
                }

                SetContentEnd(x);
            }
        }

        /// <summary>Mép phải phần schema vừa vẽ (px) - nút ⓘ đứng ngay sau đó.</summary>
        public float ContentEnd { get; private set; }

        public event EventHandler ContentEndChanged;

        private void SetContentEnd(float end)
        {
            if (Math.Abs(end - ContentEnd) < 0.5f)
            {
                return;
            }

            ContentEnd = end;
            if (ContentEndChanged != null && IsHandleCreated)
            {
                BeginInvoke((Action)(() => ContentEndChanged(this, EventArgs.Empty)));
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
