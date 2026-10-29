using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// Bộ test hồi quy cho Vision Image Tool - KHÔNG dùng phần cứng (không kết nối UnitX, không trigger camera, không PLC).
/// Nạp "Vision Image Tool.exe" bằng reflection, chạy các logic thuần (schema, đọc dataset, kiểm tra file ảnh, dựng giao
/// diện...) trên dữ liệu giả tự sinh, in kết quả ra stdout theo thứ tự cố định. Chạy trên 2 bản build (trước / sau thay
/// đổi) rồi so sánh 2 output: giống nhau = không hồi quy.
///
/// Dùng: RegressionTests.exe "đường dẫn\Vision Image Tool.exe" [thư mục làm việc tạm]
/// </summary>
internal static class RegressionTests
{
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private static Assembly _app;
    private static string _work;
    private static int _fail;

    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        _app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
        _work = args.Length > 1 ? args[1] : Path.Combine(Path.GetTempPath(), "vit_regression");
        if (Directory.Exists(_work)) Directory.Delete(_work, true);
        Directory.CreateDirectory(_work);
        Application.EnableVisualStyles();

        Run("DatasetSchema.Parse/MatchPath", TestSchemas);
        Run("CheckImageFile (file ảnh hỏng / lành)", TestImageFileCheck);
        Run("ImageFileOps.ReadInfo", TestReadInfo);
        Run("Dataset Manager - dataset theo tên file (2 ĐK)", () => TestDataset("body", BuildBodyDataset(),
            @"<ĐK>\<No>_<Model>_<SPL>_<Mã NG>_<Nội dung NG>_<Mặt>_<Vị trí>_<Đánh giá>_<Raw/Mask>.<Extension>"));
        Run("Dataset Manager - dataset theo thư mục + <Bỏ qua>", () => TestDataset("folder", BuildFolderDataset(),
            @"<DK>\<Raw/Mask>\<Model>\<Mã NG>\<SPL>\<Bỏ qua>_<Đánh giá>.<Extension>"));
        Run("Dataset Manager - lần chụp", () => TestDataset("shots", BuildShotDataset(),
            @"<Model>\<No>\<Vị trí>\<Góc>\<Lần chụp>_<Raw/Mask>.<Extension>"));
        Run("Đo màu ảnh (SampleBmp / SamplePixels / ShouldSwapResult)", TestColorSampling);
        Run("frmMainV1 - dựng giao diện (InitializeComponent + Setup*)", TestMainForm);
        Run("Struct giao tiếp SDK - kích thước bộ nhớ", TestInteropLayout);

        Console.WriteLine(_fail == 0 ? "== KẾT QUẢ: TẤT CẢ TEST CHẠY XONG" : "== KẾT QUẢ: " + _fail + " TEST LỖI");
        return _fail == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        Console.WriteLine("### " + name);
        try { test(); }
        catch (Exception ex)
        {
            var e = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
            _fail++;
            Console.WriteLine("  !! LỖI: " + e.GetType().Name + ": " + e.Message);
        }
    }

    private static Type T(string name) { return _app.GetTypes().First(t => t.Name == name); }

    // ---------------- Schema ----------------
    private static void TestSchemas()
    {
        var st = T("DatasetSchema");
        var cases = new[]
        {
            new[] { @"<ĐK>\<No>_<Model>_<SPL>_<Mã NG>_<Nội dung NG>_<Mặt>_<Vị trí>_<Đánh giá>_<Raw/Mask>.<Extension>",
                    @"ĐK1\15_A_14_505_Ba vớ_1_5_LIMIT_raw.jpg", @"ĐK2\3_C1_3_501_Dập lệch_1_2_OK_Mask.jpg", @"ĐK1\sai.jpg" },
            new[] { @"<DK>\<Raw/Mask>\<Model>\<Mã NG>\<SPL>\<Bỏ qua>_<Đánh giá>.<Extension>",
                    @"DK1\raw\9504\001\2\Image__2026-10-13__18-01-21_raw_NG.jpg", @"DK2\mask\9504\001\2\Image__2026-10-13__20-20-07mask_OK.jpg" },
            new[] { @"<Model>\<No>\<Vị trí>\<Góc>\<Lần chụp>_<Raw/Mask>.<Extension>",
                    @"A3\11\1\5\3_mask.jpg", @"A2\1\1\1\A2_1_1_1_raw.jpg" },
            new[] { "<Model>_<DK>_<SPL>_<Raw/Mask>_<Đánh giá>_<Hậu tố>", "M1_DK1_007_mask_NG_20261009.bmp", "M1_DK2_007_OK_1.bmp" },
            new[] { @"<ĐK>\<Raw/Mask>\<Model>\<No>_<Tên sản phẩm>_<SPL>_<Mã NG>_<Nội dung NG>_<Mặt>_<Vị trí>_<Đánh giá>",
                    @"DK1\raw\9928\1_C09928_1_501_Dập lệch_1_1_OK.jpg" },
            new[] { "<Model>\\<No>\\", @"M1\5\abc_raw_OK.jpg", @"M1\5\abc_mask_OK.jpg" },
            new[] { "<>", "x.jpg" },
            new[] { "<Đánh giá>_<Raw/Mask>", "OK_raw.jpg" },
        };
        foreach (var c in cases)
        {
            var args = new object[] { c[0], null };
            var schema = st.GetMethod("Parse", All).Invoke(null, args);
            Console.WriteLine("  schema " + c[0]);
            if (schema == null) { Console.WriteLine("    -> lỗi: " + args[1]); continue; }
            var fields = (IEnumerable)st.GetProperty("Fields").GetValue(schema);
            Console.WriteLine("    fields: " + string.Join(" | ", fields.Cast<object>().Select(f =>
                Prop(f, "Name") + ":" + Prop(f, "Role") + (Equals(Prop(f, "InFolder"), true) ? "/dir" : "") + (Equals(Prop(f, "Implicit"), true) ? "/implicit" : ""))));
            Console.WriteLine("    folder=" + st.GetProperty("IsFolderSchema").GetValue(schema) + " ignoresTail=" + st.GetProperty("IgnoresTail").GetValue(schema));
            for (int i = 1; i < c.Length; i++)
            {
                var r = st.GetMethod("MatchPath").Invoke(schema, new object[] { c[i] });
                Console.WriteLine("    " + c[i] + " -> " + DescribeResult(r));
            }
        }
    }

    private static string DescribeResult(object r)
    {
        if (!(bool)Prop(r, "Success")) return "KHÔNG KHỚP";
        var values = (IDictionary)Prop(r, "Values");
        var keys = values.Keys.Cast<object>().Select(k => k.ToString()).OrderBy(k => k, StringComparer.Ordinal);
        return string.Join(", ", keys.Select(k => k + "=" + values[k]))
            + " | type=" + Prop(r, "Type") + " eval=" + Prop(r, "Evaluation") + " suffix=" + Prop(r, "Suffix")
            + " shot=" + Prop(r, "Shot") + " cond=" + Prop(r, "Condition") + " decision=" + Prop(r, "FileDecision");
    }

    private static object Prop(object o, string name)
    {
        var p = o.GetType().GetProperty(name, All);
        if (p != null) return p.GetValue(o);
        var f = o.GetType().GetField(name, All);
        return f != null ? f.GetValue(o) : null;
    }

    // ---------------- File ảnh ----------------
    private static string MakeJpeg(string path, int w, int h, Color c)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp)) g.Clear(c);
            bmp.Save(path, ImageFormat.Jpeg);
        }
        return path;
    }

    private static void TestImageFileCheck()
    {
        var check = T("ucDatasetManager").GetMethod("CheckImageFile", All);
        string d = Path.Combine(_work, "imgcheck");
        string good = MakeJpeg(Path.Combine(d, "good.jpg"), 64, 48, Color.SteelBlue);
        byte[] g = File.ReadAllBytes(good);

        var files = new List<KeyValuePair<string, byte[]>>
        {
            new KeyValuePair<string, byte[]>("truncated.jpg", g.Take(g.Length / 2).ToArray()),
            new KeyValuePair<string, byte[]>("zero_tail.jpg", g.Take(g.Length / 2).Concat(new byte[8192]).ToArray()),
            new KeyValuePair<string, byte[]>("trailer.jpg", g.Concat(Enumerable.Repeat((byte)0x15, 1918)).ToArray()),
            new KeyValuePair<string, byte[]>("padded.jpg", g.Concat(new byte[100]).ToArray()),
            new KeyValuePair<string, byte[]>("all_zero.jpg", new byte[5000]),
            new KeyValuePair<string, byte[]>("empty.jpg", new byte[0]),
            new KeyValuePair<string, byte[]>("text.jpg", Encoding.ASCII.GetBytes("not an image")),
        };
        foreach (var f in files) File.WriteAllBytes(Path.Combine(d, f.Key), f.Value);
        using (var bmp = new Bitmap(10, 10)) bmp.Save(Path.Combine(d, "ok.png"), ImageFormat.Png);
        byte[] png = File.ReadAllBytes(Path.Combine(d, "ok.png"));
        File.WriteAllBytes(Path.Combine(d, "cut.png"), png.Take(png.Length - 12).ToArray());

        foreach (var name in new[] { "good.jpg" }.Concat(files.Select(f => f.Key)).Concat(new[] { "ok.png", "cut.png" }))
        {
            string r = (string)check.Invoke(null, new object[] { Path.Combine(d, name) });
            Console.WriteLine("  " + name + " -> " + (r.Length == 0 ? "OK" : r));
        }
        Console.WriteLine("  (file không tồn tại) -> " + Show((string)check.Invoke(null, new object[] { Path.Combine(d, "missing.jpg") })));
    }

    private static string Show(string s) { return s.Length == 0 ? "OK" : s; }

    private static void TestReadInfo()
    {
        var read = T("ImageFileOps").GetMethod("ReadInfo", All);
        string d = Path.Combine(_work, "readinfo");
        string jpg = MakeJpeg(Path.Combine(d, "a.jpg"), 321, 123, Color.Red);
        using (var bmp = new Bitmap(77, 33, PixelFormat.Format24bppRgb)) bmp.Save(Path.Combine(d, "b.bmp"), ImageFormat.Bmp);
        File.WriteAllBytes(Path.Combine(d, "c.jpg"), new byte[100]);
        foreach (var f in new[] { "a.jpg", "b.bmp", "c.jpg" })
        {
            var info = read.Invoke(null, new object[] { Path.Combine(d, f) });
            Console.WriteLine("  " + f + " -> " + Prop(info, "Width") + "x" + Prop(info, "Height") + " err=" + (Prop(info, "Error") ?? "-"));
        }
    }

    // ---------------- Dataset Manager (đọc dataset giả, đếm hàng / thống kê / vấn đề) ----------------
    private static string BuildBodyDataset()
    {
        string root = Path.Combine(_work, "ds_body");
        var samples = new[] { "1_A_1_506_Rách nứt_1_2_OK", "2_A_2_506_Rách nứt_1_1_OK", "3_B_3_505_Ba vớ_1_5_LIMIT", "4_B_4_501_Dập lệch_1_4_NG" };
        foreach (var dk in new[] { "ĐK1", "ĐK2" })
            foreach (var s in samples)
            {
                if (dk == "ĐK2" && s.StartsWith("4_")) continue;                       // thiếu ĐK2 ở một mẫu
                MakeJpeg(Path.Combine(root, dk, s + "_raw.jpg"), 16, 12, Color.Gray);
                if (!(dk == "ĐK1" && s.StartsWith("2_"))) MakeJpeg(Path.Combine(root, dk, s + "_mask.jpg"), 16, 12, Color.Green); // thiếu mask
            }
        File.WriteAllBytes(Path.Combine(root, "ĐK1", "5_A_5_506_Rách nứt_1_1_OK_raw.jpg"), new byte[3000]);   // file hỏng
        MakeJpeg(Path.Combine(root, "ĐK1", "khong_dung_schema.jpg"), 8, 8, Color.Black);                    // sai schema
        return root;
    }

    private static string BuildFolderDataset()
    {
        string root = Path.Combine(_work, "ds_folder");
        int t = 0;
        foreach (var dk in new[] { "DK1", "DK2" })
            foreach (var model in new[] { "9504", "9552" })
                foreach (var code in new[] { "001", "004" })
                    for (int spl = 1; spl <= 2; spl++)
                    {
                        string ts = "Image__2026-10-13__17-" + (t / 60).ToString("00") + "-" + (t % 60).ToString("00"); t++;
                        string dec = (spl + t) % 3 == 0 ? "OK" : "NG";
                        MakeJpeg(Path.Combine(root, dk, "raw", model, code, spl.ToString(), ts + "_raw_" + dec + ".jpg"), 16, 12, Color.Gray);
                        MakeJpeg(Path.Combine(root, dk, "mask", model, code, spl.ToString(), ts + "mask_" + dec + ".jpg"), 16, 12, Color.Green);
                    }
        return root;
    }

    private static string BuildShotDataset()
    {
        string root = Path.Combine(_work, "ds_shots");
        foreach (var model in new[] { "A2", "A3" })
            for (int no = 1; no <= 2; no++)
                for (int shot = 1; shot <= (no == 2 ? 3 : 1); shot++)
                {
                    MakeJpeg(Path.Combine(root, model, no.ToString(), "1", "2", shot + "_raw.jpg"), 16, 12, Color.Gray);
                    MakeJpeg(Path.Combine(root, model, no.ToString(), "1", "2", shot + "_mask.jpg"), 16, 12, Color.Green);
                }
        return root;
    }

    private static object F(object o, string n) { return o.GetType().GetField(n, All).GetValue(o); }

    private static void TestDataset(string tag, string folder, string schema)
    {
        string settings = Path.Combine(_work, "settings_" + tag + ".json");
        File.WriteAllText(settings, "{ \"Folder\": " + Json(folder) + ", \"Schema\": " + Json(schema) + ", \"IncludeSubfolders\": true }", new UTF8Encoding(false));
        var type = T("ucDatasetManager");
        var uc = (Control)Activator.CreateInstance(type, true);
        var form = new Form { ClientSize = new Size(1400, 900) };
        uc.Dock = DockStyle.Fill;
        form.Controls.Add(uc);
        type.GetMethod("Initialize").Invoke(uc, new object[] { settings, (Func<string>)(() => folder) });
        var timer = new Timer { Interval = 2500 };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            try
            {
                var rows = (IList)F(uc, "_rows");
                Console.WriteLine("  ảnh=" + ((IList)F(uc, "_images")).Count + " hàng=" + rows.Count);
                var counts = (int[])F(uc, "_statCounts");
                Console.WriteLine("  thống kê O/Δ/X/chưa có = " + string.Join("/", counts));
                var issues = ((IList)F(uc, "_issues")).Cast<object>()
                    .Select(i => F(i, "Kind") + " @ " + F(i, "Location")).OrderBy(x => x, StringComparer.Ordinal).ToList();
                Console.WriteLine("  vấn đề: " + issues.Count);
                foreach (var i in issues) Console.WriteLine("    " + i);
                var grid = (DataGridView)F(uc, "grid");
                var cols = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
                Console.WriteLine("  cột: " + string.Join(" | ", cols.Select(c => c.HeaderText)));
                var lines = new List<string>();
                for (int r = 0; r < grid.Rows.Count; r++)
                    lines.Add(string.Join(" | ", cols.Select(c => Convert.ToString(grid.Rows[r].Cells[c.Index].FormattedValue))));
                foreach (var l in lines.OrderBy(x => x, StringComparer.Ordinal)) Console.WriteLine("    " + l);
            }
            catch (Exception ex) { _fail++; Console.WriteLine("  !! LỖI: " + ex.GetBaseException().Message); }
            form.Close();
        };
        form.Shown += (s, e) => { type.GetMethod("OnShown").Invoke(uc, null); timer.Start(); };
        form.WindowState = FormWindowState.Minimized;
        form.ShowInTaskbar = false;
        Application.Run(form);
    }

    private static string Json(string s) { return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""; }

    // ---------------- Đo màu (tự nhận biết đảo kênh R/B) ----------------
    private static void TestColorSampling()
    {
        var main = T("frmMainV1");
        string bmpPath = Path.Combine(_work, "color.bmp");
        using (var bmp = new Bitmap(300, 200, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp)) { g.Clear(Color.FromArgb(38, 52, 122)); g.FillRectangle(Brushes.Yellow, 0, 0, 150, 200); }
            bmp.Save(bmpPath, ImageFormat.Bmp);
        }
        var samples = main.GetMethod("SampleBmp", All).Invoke(null, new object[] { File.ReadAllBytes(bmpPath) });
        var bgr = (byte[])Prop(samples, "Bgr");
        long sum = 0; foreach (var b in bgr) sum += b;
        Console.WriteLine("  SampleBmp: " + Prop(samples, "Width") + "x" + Prop(samples, "Height") + " mẫu=" + bgr.Length / 3 + " tổng=" + sum);
        using (var bmp = new Bitmap(bmpPath))
        using (var b24 = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb))
        {
            var s2 = main.GetMethod("SamplePixels", All).Invoke(null, new object[] { b24 });
            Console.WriteLine("  SamplePixels khớp SampleBmp: " + ((byte[])Prop(s2, "Bgr")).SequenceEqual(bgr));
        }
        foreach (var v in new long[] { 0, 1792036277, 1792036277494, 1792036277494000, 1792036277494000000, 999 })
            Console.WriteLine("  NormalizeUnitXMs(" + v + ") = " + main.GetMethod("NormalizeUnitXMs", All).Invoke(null, new object[] { v, 1792036277494L }));
    }

    // ---------------- Giao diện chính ----------------
    private static void TestMainForm()
    {
        var type = T("frmMainV1");
        var form = (Form)Activator.CreateInstance(type, true);
        try
        {
            var controls = new List<Control>();
            Action<Control> walk = null;
            walk = c => { controls.Add(c); foreach (Control ch in c.Controls) walk(ch); };
            walk(form);
            Console.WriteLine("  title=" + form.Text + " controls=" + controls.Count);
            foreach (var g in controls.GroupBy(c => c.GetType().Name).OrderBy(g => g.Key, StringComparer.Ordinal))
                Console.WriteLine("    " + g.Key + ": " + g.Count());
            // Tên các control có tên (Designer) - phát hiện control bị mất.
            var names = controls.Where(c => !string.IsNullOrEmpty(c.Name)).Select(c => c.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();
            Console.WriteLine("  named=" + names.Count + " hash=" + string.Join(",", names).GetHashCode().ToString("X8"));
            // Số event handler gắn trên các control (Designer + code) - phát hiện handler bị mất.
            int handlers = 0;
            foreach (var c in controls) handlers += CountHandlers(c);
            Console.WriteLine("  event handlers trên control: " + handlers);
        }
        finally { form.Dispose(); }
    }

    private static int CountHandlers(Control c)
    {
        var list = (System.ComponentModel.EventHandlerList)typeof(System.ComponentModel.Component).GetProperty("Events", All).GetValue(c);
        var head = typeof(System.ComponentModel.EventHandlerList).GetField("head", All).GetValue(list);
        int n = 0;
        while (head != null)
        {
            var d = (Delegate)head.GetType().GetField("handler", All).GetValue(head);
            if (d != null) n += d.GetInvocationList().Length;
            head = head.GetType().GetField("next", All).GetValue(head);
        }
        return n;
    }

    // ---------------- Struct interop ----------------
    private static void TestInteropLayout()
    {
        foreach (var name in new[] { "SDKMetaData", "ImageDimension", "SystemStatus", "ImageResult", "PartGroupResult", "ErrorInfo",
                                     "SubscriptionPayload", "SubscriptionBlobs", "PartImageResult", "DefectStats", "Region" })
        {
            var t = _app.GetTypes().FirstOrDefault(x => x.Name == name && x.IsValueType);
            Console.WriteLine("  " + name + ": " + (t == null ? "THIẾU" : System.Runtime.InteropServices.Marshal.SizeOf(t) + " byte"));
        }
    }
}
