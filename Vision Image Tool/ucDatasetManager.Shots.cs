using System;
using System.Collections.Generic;
using System.Linq;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải UserControl: Visual Studio mở file này bằng trình soạn code
    // thay vì Designer (xem ghi chú đầu ucDatasetManager.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class ucDatasetManagerShotsCodeFile { }

    /// <summary>
    /// &lt;Lần chụp&gt;: một mẫu ở một DK chụp nhiều lần (hậu tố số thứ tự / timestamp) -> MỘT hàng.
    ///   - Các lần chụp sắp theo giá trị lần chụp (thứ tự tự nhiên: 1 &lt; 2 &lt; 10, timestamp theo thời gian).
    ///   - Raw ↔ Mask ghép theo cùng lần chụp; Raw không có số lần chụp dùng chung cho mọi lần chụp.
    ///   - Vision của block = phép nhân logic các lần chụp: có X là X, không X mà có Δ là Δ, tất cả O là O.
    ///   - Một Đánh giá chung: gán nhãn = gán cho mọi ảnh của block (lần chụp mới thêm sau vẫn theo nhãn đó).
    ///   - Raw / Mask của block = lần chụp ĐẠI DIỆN (Vision xấu nhất, bằng nhau thì lần sớm nhất) - ảnh hiện
    ///     ở bảng, thumbnail Excel. Phím , / . chuyển lần chụp trong khung xem trước.
    /// </summary>
    public partial class ucDatasetManager
    {
        /// <summary>Tên trường thay cho "Tên file tự đọc" khi chuyển schema thư mục có &lt;Lần chụp&gt; sang theo tên.</summary>
        internal const string ImplicitNameField = "Tên mẫu";

        /// <summary>
        /// Thư mục có từ 2 DK mà KHÔNG hàng nào có ảnh của hơn một DK: gần như chắc chắn tên file chưa có mã
        /// SPL (ảnh chỉ có timestamp) nên không ghép được giữa các DK. null nếu không có gì đáng báo.
        /// </summary>
        private string PairingWarning()
        {
            if (_conditions.Count < 2 || _schema.ConditionField == null)
            {
                return null;
            }

            var rows = _rows.Where(r => r.Unmatched == null).ToList();
            if (rows.Count < 2 || rows.Any(r => r.Blocks.Count > 1))
            {
                return null;
            }

            return "⚠ Không hàng nào ghép được giữa các " + _schema.ConditionField.Name
                + " - tên file có lẽ chưa chứa mã SPL (chỉ có timestamp). Đổi tên thêm mã SPL hoặc sửa schema - xem ⓘ.";
        }

        private static void AddToShot(DatasetBlock block, DatasetImage image, bool isMask)
        {
            var shot = block.Shots.FirstOrDefault(s => string.Equals(s.Index, image.Shot, StringComparison.OrdinalIgnoreCase)
                && (isMask ? s.Mask == null : s.Raw == null));
            if (shot == null)
            {
                shot = new DatasetShot { Index = image.Shot ?? string.Empty };
                block.Shots.Add(shot);
            }

            if (isMask)
            {
                shot.Mask = image;
            }
            else
            {
                shot.Raw = image;
            }
        }

        /// <summary>Sắp lần chụp, gắn Raw dùng chung, ghép cặp, chọn lần chụp đại diện.</summary>
        private static void FinishShots(DatasetBlock block)
        {
            var shots = block.Shots;

            // Raw không có số lần chụp (một ảnh Raw cho nhiều Mask): dùng chung cho các lần chụp thiếu Raw.
            var shared = shots.FirstOrDefault(s => s.Index.Length == 0 && s.Raw != null && s.Mask == null);
            if (shared != null && shots.Any(s => s != shared && s.Raw == null))
            {
                shots.Remove(shared);
                foreach (var s in shots.Where(s => s.Raw == null))
                {
                    s.Raw = shared.Raw;
                }
            }

            shots.Sort((a, b) => NaturalComparer.Instance.Compare(a.Index, b.Index));

            foreach (var shot in shots)
            {
                if (shot.Raw != null)
                {
                    // Raw dùng chung: lấy Mask đầu tiên làm cặp (đủ để biết đã có kết quả).
                    if (shot.Raw.PairMask == null)
                    {
                        shot.Raw.PairMask = shot.Mask;
                    }

                    shot.Raw.NoResult = shot.Raw.PairMask == null && string.IsNullOrEmpty(shot.Raw.FileEvaluation);
                }
            }

            // Đại diện: Vision xấu nhất (X > Δ > O > chưa có), bằng nhau thì lần chụp sớm nhất.
            var representative = shots
                .Select((s, i) => new { Shot = s, Order = i, Rank = LabelRank(ShotVision(s)) })
                .OrderByDescending(x => x.Rank).ThenBy(x => x.Order)
                .Select(x => x.Shot)
                .FirstOrDefault();

            block.Raw = representative == null ? null : representative.Raw;
            block.Mask = representative == null ? null : representative.Mask;
            block.ShotView = Math.Max(0, shots.IndexOf(representative));
        }

        private static int LabelRank(string label)
        {
            switch (label)
            {
                case "X": return 3;
                case "Δ": return 2;
                case "O": return 1;
                default: return 0;
            }
        }

        /// <summary>Vision của một lần chụp (từ ảnh Mask; chỉ có Raw thì chưa có kết quả).</summary>
        private static string ShotVision(DatasetShot shot)
        {
            return shot == null || shot.Main == null ? string.Empty : UnitXValue(shot.Main);
        }

        /// <summary>Vision của block nhiều lần chụp = phép nhân logic (xấu nhất) các lần chụp đã có kết quả.</summary>
        private static string ShotsVision(DatasetBlock block)
        {
            return WorstLabel(block.Shots.Select(ShotVision).Where(v => v.Length > 0));
        }

        /// <summary>"3 lần chụp: X · X · O" - tooltip ô Vision / Raw / Mask.</summary>
        private static string ShotsSummary(DatasetBlock block)
        {
            if (block == null || block.Shots == null)
            {
                return null;
            }

            var visions = block.Shots.Select(s => ShotVision(s)).ToList();
            int bad = visions.Count(v => v == "X");
            return block.Shots.Count + " lần chụp: "
                + string.Join(" × ", visions.Select(v => v.Length == 0 ? "–" : v))
                + " = " + (ShotsVision(block).Length == 0 ? "chưa có kết quả" : ShotsVision(block))
                + (bad > 0 ? "   (" + bad + "/" + visions.Count + " NG)" : string.Empty);
        }

        /// <summary>Ô Raw / Mask: "✓" (một ảnh) hoặc "✓ 3" (3 lần chụp có ảnh loại đó), null nếu không có.</summary>
        private static string ShotMark(DatasetBlock block, bool mask)
        {
            if (block.Shots == null)
            {
                return (mask ? block.Mask : block.Raw) != null ? PresentMark : null;
            }

            int count = block.Shots.Select(s => mask ? s.Mask : s.Raw).Where(i => i != null).Distinct().Count();
            return count == 0 ? null : count == 1 ? PresentMark : PresentMark + " " + count;
        }

        /// <summary>Lần chụp đang xem của block (khung xem trước), null nếu block không có lần chụp.</summary>
        private static DatasetShot ViewedShot(DatasetBlock block)
        {
            if (block == null || block.Shots == null || block.Shots.Count == 0)
            {
                return null;
            }

            return block.Shots[Math.Min(Math.Max(0, block.ShotView), block.Shots.Count - 1)];
        }

        /// <summary>Phím , / . : lần chụp trước / sau của block ở ô đang chọn.</summary>
        private bool ShiftShot(int delta)
        {
            var cell = grid.CurrentCell;
            var block = cell == null ? null : CellBlock(cell.RowIndex, cell.ColumnIndex);
            if (block == null || block.Shots == null || block.Shots.Count < 2)
            {
                return false;
            }

            int count = block.Shots.Count;
            block.ShotView = ((block.ShotView + delta) % count + count) % count;
            _previewShowsPair = false;
            ShowPreviewOf(CurrentImage());
            return true;
        }

        /// <summary>" · Lần chụp 2/3 (00002) · X" cho tiêu đề khung xem trước.</summary>
        private string ShotCaption(DatasetImage image)
        {
            if (image == null || !_schema.HasShot)
            {
                return string.Empty;
            }

            var block = _rows.Where(r => r.Unmatched == null).Select(r => r.Block(image.Condition))
                .FirstOrDefault(b => b != null && b.Shots != null && b.Images.Contains(image));
            if (block == null || block.Shots.Count < 2)
            {
                return string.Empty;
            }

            var shot = block.Shots.FirstOrDefault(s => s.Raw == image || s.Mask == image);
            int index = shot == null ? 0 : block.Shots.IndexOf(shot) + 1;
            return "Lần chụp " + index + "/" + block.Shots.Count
                + (shot == null || shot.Index.Length == 0 ? string.Empty : " (" + shot.Index + ")");
        }

        /// <summary>Dòng phụ dưới tên ảnh ở khung xem trước: "DK1 · Lần chụp 1/6 (0001) · Mask · thư mục".</summary>
        private string PreviewMeta(DatasetImage image, string kind)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(image.Condition))
            {
                parts.Add(image.Condition);
            }

            string shot = ShotCaption(image);
            if (shot.Length > 0)
            {
                parts.Add(shot);
            }

            parts.Add(kind);
            if (!string.IsNullOrEmpty(image.RelativeDir) && image.RelativeDir != ".")
            {
                parts.Add(image.RelativeDir);
            }

            return string.Join("  ·  ", parts);
        }
    }
}
