using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerCompareCodeFile { }

    /// <summary>
    /// Đối soát Evaluation với phán định của hệ thống Vision (UnitX hoặc hệ thống khác sau này).
    ///
    /// Mỗi mẫu (block Raw + Mask ở một DK) có MỘT trong ba trạng thái, lưu trong dataset_evaluation.json
    /// cùng chỗ với nhãn (đổi tên / xoá / hoàn tác file đều mang theo):
    ///   1. Theo logic chung (không lưu gì)  : Đối soát = logic ở ô "Logic" (== hoặc !=),
    ///                                          Evaluation tính từ Vision theo logic đó.
    ///   2. Chọn đối soát riêng ("==" / "!=") : Evaluation = Vision (==) hoặc ngược Vision (!=);
    ///                                          đổi logic chung KHÔNG ảnh hưởng.
    ///   3. Chỉnh tay Evaluation ("O"/"Δ"/"X"): Đối soát tự về trống - Evaluation cố định, không phụ
    ///                                          thuộc Vision lẫn logic chung.
    /// Xoá nhãn (0 / Delete) hoặc menu "Theo logic chung" -> về trạng thái 1.
    ///
    /// Nhờ vậy đổi logic chung chỉ tác động tới mẫu CHƯA chỉnh gì - không còn cảnh Evaluation đã chỉnh
    /// tay tự "nhảy" khi đổi logic.
    /// </summary>
    public partial class ucDatasetManager
    {
        /// <summary>Thứ tự = thứ tự trong ô Logic.</summary>
        private enum CompareMode { Same, Inverted }

        private const string CompareMatch = "==";
        private const string CompareDiff = "!=";

        /// <summary>Lựa chọn của ô Đối soát: trống = Evaluation chỉnh tay, không theo Vision.</summary>
        private static readonly string[] CompareChoices = { "", CompareMatch, CompareDiff };

        private CompareMode _compareMode = CompareMode.Inverted;

        private static readonly string[] CompareModeTexts =
        {
            "Đánh giá == Vision",
            "Đánh giá != Vision"
        };

        /// <summary>Đọc cấu hình đã lưu (cả tên cũ "Actual" / "Correctness" của bản trước).</summary>
        private static CompareMode ParseCompareMode(string text)
        {
            switch ((text ?? string.Empty).Trim())
            {
                case "Same":
                case "Actual":
                    return CompareMode.Same;
                default:
                    return CompareMode.Inverted;   // mặc định giữ logic cũ: ngược Vision
            }
        }

        #region Ô Logic

        private void WireCompareModeBox()
        {
            // Kiểu ô / tooltip trong Designer; danh sách lựa chọn theo enum CompareMode nên nạp bằng code.
            cboCompareMode.Items.AddRange(CompareModeTexts.Cast<object>().ToArray());
            cboCompareMode.SelectedIndex = (int)_compareMode;
            cboCompareMode.SelectedIndexChanged += (s, e) =>
            {
                if (cboCompareMode.SelectedIndex >= 0)
                {
                    SetCompareMode((CompareMode)cboCompareMode.SelectedIndex, true);
                }
            };
        }

        /// <summary>
        /// Đổi logic chung. Người dùng đổi (save = true) mà đang có mẫu theo logic cũ thì hỏi:
        ///   - Chỉ ảnh mới (mặc định): chốt các mẫu hiện có bằng logic CŨ (lưu quy tắc riêng), ảnh chạy tiếp
        ///     theo logic mới - vd chạy một loạt với !=, đổi sang == rồi chạy tiếp: loạt trước vẫn !=.
        ///   - Tất cả: mọi mẫu chưa chỉnh gì đổi theo logic mới.
        ///   - Huỷ: giữ logic cũ.
        /// </summary>
        private void SetCompareMode(CompareMode mode, bool save)
        {
            if (mode == _compareMode)
            {
                if (cboCompareMode.SelectedIndex != (int)mode)
                {
                    cboCompareMode.SelectedIndex = (int)mode;
                }

                return;
            }

            var following = save
                ? _rows.SelectMany(r => r.Blocks.Values).Where(b => BlockStored(b) == null && BlockUnitX(b).Length > 0).ToList()
                : new List<DatasetBlock>();

            if (save && following.Count > 0)
            {
                if (_locked)
                {
                    cboCompareMode.SelectedIndex = (int)_compareMode;
                    NotifyLocked();
                    return;
                }

                bool? onlyNew = AskLogicScope(CompareModeTexts[(int)_compareMode], CompareModeTexts[(int)mode], following.Count);
                if (onlyNew == null)
                {
                    cboCompareMode.SelectedIndex = (int)_compareMode;   // huỷ
                    return;
                }

                if (onlyNew.Value)
                {
                    // Chốt các mẫu hiện có bằng logic cũ -> đổi logic không làm chúng đổi theo.
                    SetBlockCompare(following, GlobalRule);
                }
                else
                {
                    following.Clear();
                }
            }

            _compareMode = mode;
            if (cboCompareMode.SelectedIndex != (int)mode)
            {
                cboCompareMode.SelectedIndex = (int)mode;
            }

            if (save)
            {
                SaveSettings();
            }

            if (_images.Count == 0)
            {
                return;
            }

            RefillKeepingPosition(CurrentImage());
            RefreshTreeCounts();

            if (save)
            {
                ShowStatusMessage(following.Count > 0
                    ? $"Logic \"{CompareModeTexts[(int)mode]}\" áp dụng cho ảnh MỚI; {following.Count} mẫu hiện có giữ logic cũ."
                    : $"Logic \"{CompareModeTexts[(int)mode]}\" áp dụng cho mọi mẫu chưa chỉnh tay / chưa chọn đối soát riêng.");
                StatusResetSoon();
            }
        }

        /// <summary>true = chỉ ảnh mới, false = tất cả mẫu đang theo logic chung, null = huỷ.</summary>
        private bool? AskLogicScope(string oldText, string newText, int count)
        {
            using (var dialog = new Form
            {
                Text = "Đổi logic đánh giá",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.White,
                ClientSize = new Size(560, 196)
            })
            {
                var message = new Label
                {
                    Text = $"Đổi logic từ  \"{oldText}\"  sang  \"{newText}\"." + Environment.NewLine + Environment.NewLine
                        + $"Đang có {count} mẫu theo logic chung (chưa chỉnh tay, chưa chọn đối soát riêng). Áp dụng logic mới cho:",
                    Bounds = new Rectangle(16, 14, 528, 80)
                };

                bool? result = null;
                Func<string, bool?, Rectangle, bool, Button> make = (text, value, bounds, primary) =>
                {
                    var button = new Button
                    {
                        Text = text,
                        Bounds = bounds,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        BackColor = primary ? Primary : Color.White,
                        ForeColor = primary ? Color.White : Color.FromArgb(31, 41, 55)
                    };
                    button.FlatAppearance.BorderColor = primary ? Primary : Color.FromArgb(209, 213, 219);
                    button.Click += (s, e) =>
                    {
                        result = value;
                        dialog.DialogResult = value == null ? DialogResult.Cancel : DialogResult.OK;
                    };
                    return button;
                };

                var onlyNew = make("Chỉ ảnh mới", true, new Rectangle(16, 104, 200, 40), true);
                var all = make($"Tất cả {count} mẫu", false, new Rectangle(226, 104, 200, 40), false);
                var cancel = make("Huỷ", null, new Rectangle(436, 104, 108, 40), false);
                var tips = new ToolTip { AutoPopDelay = 15000 };
                tips.SetToolTip(onlyNew, $"{count} mẫu hiện có giữ nguyên logic cũ (lưu thành đối soát riêng); ảnh chạy tiếp theo logic mới.");
                tips.SetToolTip(all, $"{count} mẫu hiện có đổi theo logic mới (Đánh giá tính lại).");

                var note = new Label
                {
                    Text = "Mẫu đã chỉnh tay / đã chọn đối soát riêng luôn giữ nguyên.",
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Bounds = new Rectangle(16, 158, 528, 22)
                };

                dialog.Controls.AddRange(new Control[] { message, onlyNew, all, cancel, note });
                dialog.AcceptButton = onlyNew;
                dialog.CancelButton = cancel;
                Theme.Attach(dialog);
                dialog.ShowDialog(this);
                return result;
            }
        }

        #endregion

        #region Tính Evaluation / Đối soát

        /// <summary>Giá trị đã lưu của ảnh: "O" / "Δ" / "X" (chỉnh tay), "==" / "!=" (đối soát riêng), null.</summary>
        private string StoredValue(DatasetImage image)
        {
            string value;
            return image != null && image.EvalKey != null && _evaluations.TryGetValue(image.EvalKey, out value) ? value : null;
        }

        private static bool IsRule(string value)
        {
            return value == CompareMatch || value == CompareDiff;
        }

        /// <summary>Giá trị đã lưu của block: ảnh Mask (ảnh mang kết quả) trước, rồi ảnh Raw.</summary>
        private string BlockStored(DatasetBlock block)
        {
            if (block == null)
            {
                return null;
            }

            string value = StoredValue(block.Main) ?? StoredValue(block.Raw);
            if (value == null && block.Shots != null)
            {
                // Nhiều lần chụp: nhãn chung nằm trên mọi ảnh - lần chụp mới thêm sau chưa có thì lấy của ảnh khác.
                value = block.Images.OrderBy(i => i.Type == "Mask" ? 0 : 1).Select(StoredValue).FirstOrDefault(v => v != null);
            }

            return value;
        }

        private string GlobalRule
        {
            get { return _compareMode == CompareMode.Same ? CompareMatch : CompareDiff; }
        }

        private static string ApplyRule(string vision, string rule)
        {
            if (string.IsNullOrEmpty(vision))
            {
                return string.Empty;
            }

            return rule == CompareMatch ? vision : InvertLabel(vision);
        }

        private static string RuleText(string rule)
        {
            return rule == CompareMatch ? "== Vision" : "!= Vision";
        }

        /// <summary>Evaluation của block theo trạng thái (xem ghi chú đầu file).</summary>
        private string BlockEvaluationCore(DatasetBlock block)
        {
            if (block == null)
            {
                return string.Empty;
            }

            string stored = BlockStored(block);
            if (stored != null && !IsRule(stored))
            {
                return stored;
            }

            return ApplyRule(BlockUnitX(block), IsRule(stored) ? stored : GlobalRule);
        }

        /// <summary>Ô Đối soát: "==" / "!=" (riêng hoặc theo logic chung), "" = đã chỉnh tay / chưa có Vision.</summary>
        private string BlockCompare(DatasetBlock block)
        {
            if (block == null || BlockUnitX(block).Length == 0)
            {
                return string.Empty;
            }

            string stored = BlockStored(block);
            if (stored != null && !IsRule(stored))
            {
                return string.Empty;
            }

            return IsRule(stored) ? stored : GlobalRule;
        }

        #endregion

        #region Gán Đối soát

        /// <summary>
        /// Chọn đối soát cho các block:
        ///   "==" / "!="  : lưu quy tắc riêng (Evaluation tính lại theo Vision).
        ///   ""           : giữ nguyên Evaluation đang hiển thị thành giá trị chỉnh tay (không theo Vision nữa).
        ///   null         : bỏ chọn riêng - về theo logic chung.
        /// </summary>
        private void SetBlockCompare(IEnumerable<DatasetBlock> blocks, string value)
        {
            var list = blocks.Where(b => b != null && BlockUnitX(b).Length > 0).ToList();
            if (list.Count == 0)
            {
                return;
            }

            if (value == null)
            {
                SetBlockEvaluation(list, string.Empty);
                return;
            }

            if (value.Length > 0)
            {
                SetBlockEvaluation(list, value);
                return;
            }

            // Trống: chốt Evaluation hiện tại của TỪNG block (mỗi block một giá trị riêng).
            foreach (var group in list.GroupBy(BlockEvaluationCore).ToList())
            {
                SetBlockEvaluation(group, group.Key);
            }
        }

        #endregion

        #region Hiển thị

        private bool IsCompareColumn(DataGridViewColumn column)
        {
            var info = BlockOf(column);
            return info != null && info.Part == BlockPart.Compare;
        }

        /// <summary>Màu nền ô theo nhãn O / Δ / X (dùng cho cả cột Vision lẫn Evaluation).</summary>
        private static Color LabelColor(string label, Color fallback)
        {
            switch (label)
            {
                case "O": return EvalOColor;
                case "Δ": return EvalDeltaColor;
                case "X": return EvalXColor;
                default: return fallback;
            }
        }

        private string EvaluationTooltip(DatasetBlock block)
        {
            string stored = BlockStored(block);
            if (stored != null && !IsRule(stored))
            {
                return "Đánh giá chỉnh tay - không theo Vision / logic chung. Xoá nhãn (0 / Delete) để về logic chung.";
            }

            if (BlockUnitX(block).Length == 0)
            {
                return "Chưa có kết quả Vision - gán O / Δ / X bằng tay.";
            }

            return "Đánh giá tính theo " + RuleText(IsRule(stored) ? stored : GlobalRule)
                + (IsRule(stored) ? " (đối soát chọn riêng)" : " (logic chung)") + ". Gán O / Δ / X để chỉnh tay.";
        }

        private string CompareTooltip(DatasetBlock block)
        {
            string vision = BlockUnitX(block);
            if (vision.Length == 0)
            {
                return "Chưa có kết quả Vision (thiếu ảnh Mask / tên file thiếu kết quả) - chưa đối soát được.";
            }

            string stored = BlockStored(block);
            if (stored != null && !IsRule(stored))
            {
                return $"Trống: Đánh giá chỉnh tay ({stored}) - không theo Vision. Chọn == hoặc != để tính lại theo Vision.";
            }

            string rule = IsRule(stored) ? stored : GlobalRule;
            return $"Vision {LabelText(vision)} · Đánh giá {LabelText(ApplyRule(vision, rule))} → {(rule == CompareMatch ? "Trùng khớp (==)" : "Khác biệt (!=)")}"
                + Environment.NewLine + (IsRule(stored) ? "Đối soát chọn riêng - đổi logic chung không ảnh hưởng." : "Theo logic chung (ô Logic).")
                + Environment.NewLine + "Chọn trống = giữ Đánh giá hiện tại, không theo Vision nữa.";
        }

        private static string LabelText(string symbol)
        {
            switch (symbol)
            {
                case "O": return "OK";
                case "X": return "NG";
                default: return symbol;
            }
        }

        #endregion
    }
}
