using System;
using System.Globalization;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (Designer chỉ mở ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerPreviewCodeFile { }

    /// <summary>
    /// Khung XEM TRƯỚC ẢNH (Designer: tlpPreviewTitle / tlpPreviewNav):
    ///   - ⤢ Mở rộng: ẩn tạm các khối khác để khung xem ảnh chiếm cả tab (bấm lại hoặc Esc để thu lại);
    ///   - [−] [%] [+] [Fit]: zoom ảnh (cùng mức với cuộn chuột / double-click trên ảnh).
    /// </summary>
    public partial class ucDatasetManager
    {
        private const string GlyphFullScreen = "";     // Segoe MDL2: FullScreen
        private const string GlyphBackToWindow = "";   // Segoe MDL2: BackToWindow
        private static readonly float[] ZoomPresets = { 0.25F, 0.5F, 1F, 2F, 4F, 8F };

        private bool _previewExpanded;
        private bool _updatingZoomText;

        #region Mở rộng khung xem ảnh

        private void WirePreviewExpand()
        {
            btnExpandPreview.Click += (s, e) => SetPreviewExpanded(!_previewExpanded);
        }

        private void SetPreviewExpanded(bool expanded)
        {
            if (expanded == _previewExpanded)
            {
                return;
            }

            _previewExpanded = expanded;
            btnExpandPreview.Text = expanded ? GlyphBackToWindow : GlyphFullScreen;
            toolTip.SetToolTip(btnExpandPreview, expanded ? "Thu lại (Esc)" : "Mở rộng vùng xem ảnh (Esc để thu lại)");

            // Thu lại: dựng lại đúng các khu vực người dùng đang để hiện (☰ Hiển thị / thu gọn khối).
            ApplySectionVisibility();
            if (!expanded)
            {
                _previewSplit.Panel1Collapsed = false;
                RestorePreviewSplit();
            }

            picPreview.Focus();
        }

        /// <summary>Gọi cuối ApplySectionVisibility: đang mở rộng thì chỉ còn khung xem ảnh (+ thanh trạng thái).</summary>
        private void ApplyPreviewExpand()
        {
            if (!_previewExpanded)
            {
                return;
            }

            for (int i = 0; i < SectionCount; i++)
            {
                _root.RowStyles[i].Height = 0F;
                SectionCard(i).Visible = false;
            }

            _issuesSplit.Panel2Collapsed = true;
            _previewSplit.Panel1Collapsed = true;
        }

        /// <summary>Đặt lại vạch chia Bảng | Xem trước theo tỉ lệ đã lưu (hoặc mặc định) sau khi thu lại.</summary>
        private void RestorePreviewSplit()
        {
            double ratio = _layout.PreviewSplitRatio ?? DefaultPreviewSplitRatio;
            bool wasApplying = _applyingLayout;
            _applyingLayout = true;
            try
            {
                TrySetDistance(_previewSplit, (int)(_previewSplit.Width * Math.Min(0.9, Math.Max(0.2, ratio))));
            }
            finally
            {
                _applyingLayout = wasApplying;
            }

            SyncBottomSplit();
        }

        #endregion

        #region Zoom

        private void WirePreviewZoom()
        {
            cboZoom.Items.Add("Fit");
            foreach (float preset in ZoomPresets)
            {
                cboZoom.Items.Add(FormatZoom(preset));
            }

            btnZoomIn.Click += (s, e) => ZoomPreview(z => z.ZoomBy(1.25F));
            btnZoomOut.Click += (s, e) => ZoomPreview(z => z.ZoomBy(0.8F));
            btnZoomFit.Click += (s, e) => ZoomPreview(z => z.ZoomToFit());
            cboZoom.SelectionChangeCommitted += (s, e) => ApplyZoomText(cboZoom.SelectedItem as string);
            cboZoom.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyZoomText(cboZoom.Text);
                    picPreview.Focus();
                }
            };
            cboZoom.Leave += (s, e) => UpdateZoomText();
            picPreview.ZoomChanged += (s, e) => UpdateZoomText();
            UpdateZoomText();
        }

        /// <summary>Áp lên ảnh đang xem; đang xem song song thì áp cho cả hai bên (Raw và Mask).</summary>
        private void ZoomPreview(Action<ZoomPictureBox> action)
        {
            action(picPreview);
            if (_splitView)
            {
                action(picPreviewMask);
            }
        }

        private void ApplyZoomText(string text)
        {
            text = (text ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                UpdateZoomText();
                return;
            }

            if (text.StartsWith("Fit", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Vừa", StringComparison.OrdinalIgnoreCase))
            {
                ZoomPreview(z => z.ZoomToFit());
                return;
            }

            float percent;
            if (float.TryParse(text.TrimEnd('%', ' ').Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out percent) && percent > 0)
            {
                float zoom = percent / 100F;
                ZoomPreview(z => z.SetZoom(zoom));
            }
            else
            {
                UpdateZoomText();
            }
        }

        /// <summary>Ô % hiện mức zoom thật của ảnh (vừa khung: "54%" như nhãn "Vừa khung (54%)" trên ảnh).</summary>
        private void UpdateZoomText()
        {
            if (_updatingZoomText || cboZoom.Focused)
            {
                return;
            }

            _updatingZoomText = true;
            try
            {
                cboZoom.Text = picPreview.Image == null ? "Fit" : FormatZoom(picPreview.ZoomFactor);
            }
            finally
            {
                _updatingZoomText = false;
            }
        }

        private static string FormatZoom(float zoom)
        {
            return Math.Round(zoom * 100).ToString(CultureInfo.InvariantCulture) + "%";
        }

        #endregion
    }
}
