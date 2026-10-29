using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VisionImageTool
{
    // Class đầu tiên trong file KHÔNG phải Form: Visual Studio mở file này bằng trình soạn code thay vì
    // Designer (xem ghi chú đầu frmMainV1.cs). Đừng xoá / đừng đưa xuống dưới.
    internal static class frmMainV1CameraTimingCodeFile { }

    /// <summary>
    /// Đo thời gian chu trình CHỤP CAMERA (Trigger) - chế độ push đã có CycleTiming riêng.
    ///
    /// Một lần chụp đi qua các chặng (mốc PC đo bằng Stopwatch, mốc UnitX đọc từ IMAGE_RESULT):
    ///
    ///   [PC] gửi trigger_camera ──► [UnitX] chụp (image_timestamp) ──► [UnitX] xong kết quả (meta_data.timestamp)
    ///        ──► [PC] SDK tải blob ảnh xong, gọi callback ──► copy / giải mã / lưu / thumbnail ──► [PC] ảnh lên màn hình
    ///
    /// Đồng hồ UnitX lệch đồng hồ PC (log 2026-10-15: ~5 s) nên độ lệch được ƯỚC LƯỢNG = min(image_timestamp -
    /// lúc gửi trigger) trên cả phiên: lần chụp nhanh nhất coi như chụp ngay khi gửi lệnh. Vì vậy "Trigger -> chụp"
    /// là thời gian chờ THÊM so với lần nhanh nhất (hàng đợi lệnh), và "Truyền về" có thể dư vài chục ms.
    /// Hai chặng "UnitX xử lý" (cùng đồng hồ UnitX) và "Trigger -> callback" (cùng đồng hồ PC) là chính xác.
    ///
    /// Kết quả ghép với lần trigger theo thứ tự (FIFO). Thống kê + CSV in ra khi hết ảnh chờ được 3 giây.
    /// </summary>
    public partial class frmMainV1
    {
        private sealed class CameraCycle
        {
            public int Index;
            public long TriggerTs;                 // Stopwatch, lúc gọi trigger_camera
            public double TriggerUnixMs;           // giờ PC (theo neo Stopwatch) lúc gọi trigger_camera
            public double TriggerCallMs;           // thời gian hàm trigger_camera chạy (gửi lệnh gRPC)
            public long NicBytesAtTrigger = -1;

            public long ImageUnitXMs;              // image_timestamp (đồng hồ UnitX)
            public long ResultUnitXMs;             // meta_data.timestamp đã quy về ms (0 = không dùng được)
            public double ArrivedUnixMs;           // giờ PC lúc callback bắt đầu
            public long ArrivedTs;
            public long NicBytesAtArrival = -1;
            public int QueueAtArrival;             // còn bao nhiêu trigger chưa có kết quả khi kết quả này về

            public double BlobCopyMs;
            public double DecodeMs;
            public double SaveMs;
            public double ThumbMs;
            public double CallbackMs;
            public long ReturnBytes;
            public int BlobCount;
            public int EmptyBlobs;
            public string Decision;

            public double DisplayWaitMs = -1;      // từ lúc callback giao ảnh cho UI tới lúc UI bắt đầu vẽ
            public double DisplayPaintMs = -1;     // UI gán + vẽ ảnh
            public double TriggerToScreenMs = -1;
            public long HandOffTs;
            public int Pending = 2;                // còn chờ: cuối callback + ảnh lên màn hình

            // Tính sau khi biết độ lệch đồng hồ.
            public double TriggerToCaptureMs = double.NaN;
            public double UnitXProcessMs = double.NaN;
            public double TransferMs = double.NaN;      // UnitX xong -> callback (gồm cả chờ ảnh trước truyền xong)
            public double TransferWaitMs = double.NaN;  // phần chờ: ảnh trước vẫn đang truyền
            public double TransferNetMs = double.NaN;   // phần truyền thực của chính ảnh này

            // Tách theo lưu lượng card mạng (lấy mẫu ~1 ms): UnitX chưa gửi gì / dữ liệu ảnh đang chạy trên dây / đã nhận đủ.
            public double PreWireMs = double.NaN;       // chụp -> UnitX bắt đầu gửi ảnh (xử lý AI + dựng ảnh kết quả)
            public double WireMs = double.NaN;          // dữ liệu ảnh chạy trên mạng (3% -> 97% số byte)
            public double PostWireMs = double.NaN;      // nhận đủ byte -> SDK gọi callback
            public double WireMBps = double.NaN;
            public double TriggerToCallbackMs = double.NaN;
            public double SinceLastResultMs = double.NaN;
        }

        private readonly ConcurrentQueue<CameraCycle> _cameraTriggers = new ConcurrentQueue<CameraCycle>();
        private readonly List<CameraCycle> _cameraCycles = new List<CameraCycle>();
        private int _cameraTriggerIndex;
        private double _cameraClockOffsetMs = double.NaN;   // đồng hồ UnitX - đồng hồ PC (ước lượng)
        private double _lastCameraArrivalUnixMs = double.NaN;
        private double _prevSegmentArrivalUnixMs = double.NaN;
        private Timer _cameraSummaryTimer;
        private CameraCycle _displayProbe;
        private bool _cameraMetaLogged;

        /// <summary>Thời gian tạo thumbnail khung xem trước của callback gần nhất.</summary>
        private double _lastThumbMs;

        /// <summary>Số blob rỗng của callback gần nhất (UnitX đã xoá blob trước khi SDK tải về).</summary>
        private int _lastEmptyBlobs;

        /// <summary>Chu trình chụp đang xử lý trong callback (luồng SDK) - để ShowReturnedImage đánh dấu lúc giao ảnh cho UI.</summary>
        private CameraCycle _currentCameraCycle;

        // Giờ PC độ phân giải cao: DateTime.UtcNow chỉ chính xác ~1-15 ms, neo một lần rồi cộng Stopwatch.
        private static readonly double ClockAnchorUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private static readonly long ClockAnchorTs = Stopwatch.GetTimestamp();

        private static double UnixMsFrom(long stopwatchTs)
        {
            return ClockAnchorUnixMs + (stopwatchTs - ClockAnchorTs) * 1000.0 / Stopwatch.Frequency;
        }

        private volatile NetworkInterface _unitXNic;
        private string _unitXNicHost;

        /// <summary>
        /// Gọi trên luồng UI trước khi trigger: tìm card mạng đang nối tới UnitX (cùng dải IP với Host) để đếm
        /// số byte nhận về trong lúc chờ ảnh - biết được băng thông thật và tốc độ link.
        /// </summary>
        private void PrepareCameraTiming()
        {
            string host = (txtSdkHost.Text ?? string.Empty).Trim();
            if (host == _unitXNicHost && _unitXNic != null)
            {
                return;
            }

            _unitXNicHost = host;
            _unitXNic = FindNicFor(host);
            var nic = _unitXNic;
            if (nic != null)
            {
                Log($"Đo thời gian chụp: card mạng tới UnitX ({host}) = '{nic.Name}' ({nic.Description}), "
                    + $"tốc độ link {nic.Speed / 1000000.0:0} Mbit/s (tối đa ~{nic.Speed / 8.0 / 1048576.0:0} MB/s).");
            }
            else
            {
                Log($"Đo thời gian chụp: không xác định được card mạng tới UnitX ('{host}') - bỏ qua đo băng thông.");
            }
        }

        private static NetworkInterface FindNicFor(string host)
        {
            try
            {
                IPAddress target;
                if (!IPAddress.TryParse(host, out target))
                {
                    target = Dns.GetHostAddresses(host).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                }

                if (target == null || target.AddressFamily != AddressFamily.InterNetwork)
                {
                    return null;
                }

                byte[] t = target.GetAddressBytes();
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != AddressFamily.InterNetwork || unicast.IPv4Mask == null)
                        {
                            continue;
                        }

                        byte[] a = unicast.Address.GetAddressBytes();
                        byte[] m = unicast.IPv4Mask.GetAddressBytes();
                        bool same = true;
                        for (int i = 0; i < 4 && same; i++)
                        {
                            same = (a[i] & m[i]) == (t[i] & m[i]);
                        }

                        if (same)
                        {
                            return nic;
                        }
                    }
                }
            }
            catch
            {
                // Không đo được băng thông thì thôi, không ảnh hưởng việc chụp.
            }

            return null;
        }

        private long NicBytesReceived()
        {
            try
            {
                var nic = _unitXNic;
                return nic == null ? -1 : nic.GetIPv4Statistics().BytesReceived;
            }
            catch
            {
                return -1;
            }
        }

        // ---- Lấy mẫu số byte card mạng nhận được, ~1 ms / mẫu, chỉ chạy khi đang đo ----
        private readonly List<KeyValuePair<long, long>> _nicSamples = new List<KeyValuePair<long, long>>();
        private Thread _nicSampler;
        private long _nicSamplerUntilTs;

        /// <summary>Giữ bộ lấy mẫu chạy thêm 30 giây kể từ bây giờ (tự dừng khi không còn chụp).</summary>
        private void KeepNicSampling()
        {
            if (_unitXNic == null)
            {
                return;
            }

            Interlocked.Exchange(ref _nicSamplerUntilTs, Stopwatch.GetTimestamp() + 30 * Stopwatch.Frequency);
            lock (_nicSamples)
            {
                if (_nicSampler != null && _nicSampler.IsAlive)
                {
                    return;
                }

                _nicSampler = new Thread(NicSamplerLoop) { IsBackground = true, Name = "NIC sampler", Priority = ThreadPriority.AboveNormal };
                _nicSampler.Start();
            }
        }

        private void NicSamplerLoop()
        {
            while (Stopwatch.GetTimestamp() < Interlocked.Read(ref _nicSamplerUntilTs))
            {
                long bytes = NicBytesReceived();
                long ts = Stopwatch.GetTimestamp();
                if (bytes >= 0)
                {
                    lock (_nicSamples)
                    {
                        _nicSamples.Add(new KeyValuePair<long, long>(ts, bytes));

                        // Giữ ~60 giây gần nhất.
                        long oldest = ts - 60 * Stopwatch.Frequency;
                        if (_nicSamples.Count > 20000 && _nicSamples[0].Key < oldest)
                        {
                            _nicSamples.RemoveAll(s => s.Key < oldest);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Tách chặng [từ fromTs tới lúc callback] theo lưu lượng mạng: lúc nhận được 3% và 97% số byte của chu trình.
        /// </summary>
        private void SplitByNetwork(CameraCycle c, long fromTs)
        {
            List<KeyValuePair<long, long>> window;
            lock (_nicSamples)
            {
                window = _nicSamples.Where(s => s.Key >= fromTs && s.Key <= c.ArrivedTs).ToList();
            }

            if (window.Count < 20)
            {
                return;
            }

            long baseBytes = window[0].Value;
            long total = window[window.Count - 1].Value - baseBytes;

            // Phải thấy ít nhất ~70% dung lượng ảnh đi qua card mạng thì mới đáng tin.
            if (c.ReturnBytes <= 0 || total < c.ReturnBytes * 0.7)
            {
                return;
            }

            long first = window.First(s => s.Value - baseBytes >= total * 0.03).Key;
            long last = window.First(s => s.Value - baseBytes >= total * 0.97).Key;
            double toMs = 1000.0 / Stopwatch.Frequency;

            c.PreWireMs = (first - fromTs) * toMs;
            c.WireMs = Math.Max(1, (last - first) * toMs);
            c.PostWireMs = (c.ArrivedTs - last) * toMs;
            c.WireMBps = total * 0.94 / 1048576.0 / (c.WireMs / 1000.0);
        }

        /// <summary>Ngay TRƯỚC khi gọi trigger_camera. Trả về bản ghi để điền thời gian gọi hàm.</summary>
        private CameraCycle BeginCameraTrigger()
        {
            KeepNicSampling();
            var cycle = new CameraCycle
            {
                Index = Interlocked.Increment(ref _cameraTriggerIndex),
                NicBytesAtTrigger = NicBytesReceived(),
                TriggerTs = Stopwatch.GetTimestamp()
            };
            cycle.TriggerUnixMs = UnixMsFrom(cycle.TriggerTs);
            _cameraTriggers.Enqueue(cycle);
            _cameraSummaryTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            return cycle;
        }

        /// <summary>Ngay SAU khi trigger_camera trả về. Lệnh thất bại thì bỏ khỏi hàng chờ.</summary>
        private void EndCameraTrigger(CameraCycle cycle, bool ok)
        {
            if (cycle == null)
            {
                return;
            }

            cycle.TriggerCallMs = ElapsedMs(cycle.TriggerTs);
            if (!ok)
            {
                // Lệnh hỏng nằm cuối hàng (vừa thêm) - dựng lại hàng đợi không có nó.
                var keep = _cameraTriggers.ToArray().Where(c => c != cycle).ToList();
                CameraCycle ignored;
                while (_cameraTriggers.TryDequeue(out ignored))
                {
                }

                foreach (var c in keep)
                {
                    _cameraTriggers.Enqueue(c);
                }
            }
        }

        /// <summary>
        /// meta_data.timestamp của SDK không ghi rõ đơn vị: quy về ms theo độ lớn (s / ms / µs / ns). 0 nếu không
        /// hợp lý (lệch image_timestamp quá 10 phút).
        /// </summary>
        private static long NormalizeUnitXMs(long value, long imageMs)
        {
            if (value <= 0)
            {
                return 0;
            }

            double ms = value < 100000000000L ? value * 1000.0       // giây
                : value < 100000000000000L ? value                   // ms
                : value < 100000000000000000L ? value / 1000.0       // µs
                : value / 1000000.0;                                 // ns
            return Math.Abs(ms - imageMs) > 600000 ? 0 : (long)ms;
        }

        /// <summary>Đầu callback IMAGE_RESULT của ảnh camera: ghép với lần trigger tương ứng.</summary>
        private CameraCycle BeginCameraResult(ImageResult image, long arrivedTs)
        {
            CameraCycle cycle = null;
            CameraCycle candidate;
            while (_cameraTriggers.TryDequeue(out candidate))
            {
                // Trigger quá cũ (không bao giờ có kết quả, vd UnitX bỏ ảnh): không ghép nhầm vào kết quả này.
                if (ElapsedMs(candidate.TriggerTs) > ManualTriggerWindowMs)
                {
                    continue;
                }

                cycle = candidate;
                break;
            }

            if (cycle == null)
            {
                // Kết quả không do mình trigger (dây chuyền tự chạy / trigger ngoài): vẫn đo phần nhận về.
                cycle = new CameraCycle { Index = 0 };
            }

            // Callback không kèm blob thoát sớm trước khi ShowReturnedImage đặt lại các số đo này.
            _lastBlobCopyMs = _lastDecodeMs = _lastSaveMaskMs = _lastThumbMs = 0;
            _lastReturnBytes = 0;
            _lastEmptyBlobs = 0;

            cycle.ArrivedTs = arrivedTs;
            cycle.ArrivedUnixMs = UnixMsFrom(arrivedTs);
            cycle.NicBytesAtArrival = NicBytesReceived();
            cycle.QueueAtArrival = _cameraTriggers.Count;
            cycle.ImageUnitXMs = image.image_timestamp;
            cycle.ResultUnitXMs = NormalizeUnitXMs(image.meta_data.timestamp, image.image_timestamp);

            if (!_cameraMetaLogged)
            {
                _cameraMetaLogged = true;
                Log($"    [đo thời gian] mốc UnitX: image_timestamp={image.image_timestamp}, meta_data.timestamp={image.meta_data.timestamp}"
                    + (cycle.ResultUnitXMs > 0 ? $" (= {cycle.ResultUnitXMs - image.image_timestamp:0} ms sau lúc chụp)" : " (không dùng được)"));
            }

            return cycle;
        }

        /// <summary>Cuối callback: điền số liệu xử lý phía PC, chờ ảnh lên màn hình rồi ghi log.</summary>
        private void EndCameraResult(CameraCycle cycle, long blobCount, string decision)
        {
            bool willDisplay = cycle != null && cycle.HandOffTs > 0;
            if (cycle == null)
            {
                return;
            }

            cycle.CallbackMs = ElapsedMs(cycle.ArrivedTs);
            cycle.BlobCopyMs = _lastBlobCopyMs;
            cycle.DecodeMs = _lastDecodeMs;
            cycle.SaveMs = _lastSaveMaskMs;
            cycle.ThumbMs = _lastThumbMs;
            cycle.ReturnBytes = _lastReturnBytes;
            cycle.BlobCount = (int)blobCount;
            cycle.EmptyBlobs = _lastEmptyBlobs;
            cycle.Decision = decision;

            if (!double.IsNaN(_lastCameraArrivalUnixMs))
            {
                cycle.SinceLastResultMs = cycle.ArrivedUnixMs - _lastCameraArrivalUnixMs;
            }

            _lastCameraArrivalUnixMs = cycle.ArrivedUnixMs;

            lock (_cameraCycles)
            {
                _cameraCycles.Add(cycle);
            }

            // Không có ảnh để hiện thì không chờ phần màn hình.
            if (!willDisplay)
            {
                Interlocked.Decrement(ref cycle.Pending);
            }

            if (Interlocked.Decrement(ref cycle.Pending) == 0)
            {
                FinishCameraCycle(cycle);
            }
        }

        /// <summary>Callback vừa giao ảnh cho luồng UI (ShowLiveCameraImage).</summary>
        private void MarkDisplayHandOff(CameraCycle cycle)
        {
            if (cycle != null)
            {
                cycle.HandOffTs = Stopwatch.GetTimestamp();

                // Ảnh trước chưa kịp lên màn hình đã có ảnh mới (UI quá tải): chốt chu trình đó, không đo phần hiển thị.
                var previous = Interlocked.Exchange(ref _displayProbe, cycle);
                if (previous != null && Interlocked.Decrement(ref previous.Pending) == 0)
                {
                    FinishCameraCycle(previous);
                }
            }
        }

        /// <summary>Gọi trong SetPreviewBitmap (luồng UI) sau khi ảnh đã gán + vẽ xong.</summary>
        private void CompleteDisplayProbe(long uiStartTs)
        {
            var cycle = Interlocked.Exchange(ref _displayProbe, null);
            if (cycle == null)
            {
                return;
            }

            cycle.DisplayWaitMs = (uiStartTs - cycle.HandOffTs) * 1000.0 / Stopwatch.Frequency;
            cycle.DisplayPaintMs = ElapsedMs(uiStartTs);
            if (cycle.TriggerTs > 0)
            {
                cycle.TriggerToScreenMs = ElapsedMs(cycle.TriggerTs);
            }

            if (Interlocked.Decrement(ref cycle.Pending) == 0)
            {
                FinishCameraCycle(cycle);
            }
        }

        private void FinishCameraCycle(CameraCycle cycle)
        {
            ComputeCameraSegments(cycle);
            LogCameraCycle(cycle);

            // Hết ảnh chờ 3 giây -> in thống kê cả loạt (cả Trigger Once lẻ lẫn Trigger lặp / liên tục).
            if (_cameraSummaryTimer == null)
            {
                _cameraSummaryTimer = new Timer(_ => LogCameraTimingSummary(), null, Timeout.Infinite, Timeout.Infinite);
            }

            _cameraSummaryTimer.Change(_cameraTriggers.IsEmpty ? 3000 : 15000, Timeout.Infinite);
        }

        private void ComputeCameraSegments(CameraCycle c)
        {
            if (c.TriggerTs > 0)
            {
                c.TriggerToCallbackMs = c.ArrivedUnixMs - c.TriggerUnixMs;

                // Độ lệch đồng hồ = min(image_timestamp - lúc gửi trigger).
                double candidate = c.ImageUnitXMs - c.TriggerUnixMs;
                if (c.ImageUnitXMs > 0 && (double.IsNaN(_cameraClockOffsetMs) || candidate < _cameraClockOffsetMs))
                {
                    _cameraClockOffsetMs = candidate;
                }
            }

            if (c.ResultUnitXMs > 0 && c.ImageUnitXMs > 0)
            {
                c.UnitXProcessMs = c.ResultUnitXMs - c.ImageUnitXMs;
            }

            if (!double.IsNaN(_cameraClockOffsetMs) && c.ImageUnitXMs > 0)
            {
                if (c.TriggerTs > 0)
                {
                    c.TriggerToCaptureMs = c.ImageUnitXMs - _cameraClockOffsetMs - c.TriggerUnixMs;
                }

                long sentUnitX = c.ResultUnitXMs > 0 ? c.ResultUnitXMs : c.ImageUnitXMs;
                c.TransferMs = c.ArrivedUnixMs + _cameraClockOffsetMs - sentUnitX;

                // Kênh tải blob chỉ chạy một ảnh một lúc: ảnh xong khi ảnh trước chưa về hết thì phải chờ.
                double readyPc = sentUnitX - _cameraClockOffsetMs;
                double start = double.IsNaN(_prevSegmentArrivalUnixMs) ? readyPc : Math.Max(readyPc, _prevSegmentArrivalUnixMs);
                c.TransferWaitMs = Math.Max(0, start - readyPc);
                c.TransferNetMs = c.ArrivedUnixMs - start;
            }

            // Mốc bắt đầu: lúc trigger, hoặc lúc ảnh trước về xong nếu ảnh trước còn đang truyền.
            if (c.TriggerTs > 0)
            {
                long fromTs = c.TriggerTs;
                if (!double.IsNaN(_prevSegmentArrivalUnixMs) && _prevSegmentArrivalUnixMs > c.TriggerUnixMs)
                {
                    fromTs = ClockAnchorTs + (long)((_prevSegmentArrivalUnixMs - ClockAnchorUnixMs) * Stopwatch.Frequency / 1000.0);
                }

                SplitByNetwork(c, fromTs);
            }

            _prevSegmentArrivalUnixMs = c.ArrivedUnixMs;
        }

        private static string Ms(double value)
        {
            return double.IsNaN(value) || value < 0 ? "–" : value.ToString("0");
        }

        private void LogCameraCycle(CameraCycle c)
        {
            string net = string.Empty;
            if (c.NicBytesAtTrigger >= 0 && c.NicBytesAtArrival >= 0 && c.TriggerTs > 0)
            {
                long bytes = c.NicBytesAtArrival - c.NicBytesAtTrigger;
                net = $" | mạng nhận {bytes / 1048576.0:0.0} MB";
            }

            Log($"    THỜI GIAN CHỤP #{(c.Index > 0 ? c.Index.ToString() : "?")} | gửi lệnh {Ms(c.TriggerCallMs)} | "
                + $"trigger→chụp +{Ms(c.TriggerToCaptureMs)} | CHỤP→CALLBACK {Ms(c.TransferMs)} = UNITX CHƯA GỬI {Ms(c.PreWireMs)} + TRUYỀN MẠNG {Ms(c.WireMs)}{(double.IsNaN(c.WireMBps) ? string.Empty : " (" + c.WireMBps.ToString("0") + " MB/s)")} + SDK→callback {Ms(c.PostWireMs)} | "
                + $"callback {Ms(c.CallbackMs)} (copy {c.BlobCopyMs:0}, giải mã {c.DecodeMs:0}, thumbnail {c.ThumbMs:0}, lưu {c.SaveMs:0}) | "
                + $"chờ UI {Ms(c.DisplayWaitMs)} + vẽ {Ms(c.DisplayPaintMs)} | TRIGGER→CALLBACK {Ms(c.TriggerToCallbackMs)} | "
                + $"TRIGGER→MÀN HÌNH {Ms(c.TriggerToScreenMs)} ms | cách ảnh trước {Ms(c.SinceLastResultMs)} | còn chờ {c.QueueAtArrival}{net}");
        }

        private void LogCameraTimingSummary()
        {
            List<CameraCycle> data;
            lock (_cameraCycles)
            {
                if (_cameraCycles.Count == 0)
                {
                    return;
                }

                data = _cameraCycles.ToList();
                _cameraCycles.Clear();
            }

            _lastCameraArrivalUnixMs = double.NaN;
            _prevSegmentArrivalUnixMs = double.NaN;

            Log($"===== THỐNG KÊ THỜI GIAN CHỤP CAMERA (n={data.Count}) =====");
            if (!double.IsNaN(_cameraClockOffsetMs))
            {
                Log($"Đồng hồ UnitX lệch PC ~{_cameraClockOffsetMs / 1000.0:+0.0;-0.0} s (ước lượng từ lần chụp nhanh nhất).");
            }

            var stages = new List<KeyValuePair<string, Func<CameraCycle, double>>>
            {
                new KeyValuePair<string, Func<CameraCycle, double>>("Gửi lệnh trigger     ", x => x.TriggerCallMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("Trigger → chụp (+)   ", x => x.TriggerToCaptureMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("UNITX XỬ LÝ          ", x => x.UnitXProcessMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("Chờ lượt truyền      ", x => x.TransferWaitMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("UNITX CHƯA GỬI ẢNH   ", x => x.PreWireMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("TRUYỀN QUA MẠNG      ", x => x.WireMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("SDK nhận đủ→callback ", x => x.PostWireMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("Callback (PC xử lý)  ", x => x.CallbackMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("Chờ UI + vẽ ảnh      ", x => x.DisplayWaitMs < 0 ? double.NaN : x.DisplayWaitMs + x.DisplayPaintMs)
            };

            // Không tách được theo lưu lượng mạng: xếp hạng bằng cả chặng chụp -> callback (UnitX xử lý + truyền gộp chung).
            if (!data.Any(x => !double.IsNaN(x.WireMs)))
            {
                stages.Add(new KeyValuePair<string, Func<CameraCycle, double>>("CHỤP→CALLBACK (gộp UnitX + truyền)", x => x.TransferMs));
            }

            var details = new List<KeyValuePair<string, Func<CameraCycle, double>>>
            {
                new KeyValuePair<string, Func<CameraCycle, double>>("CHỤP → CALLBACK      ", x => x.TransferMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("  copy blob          ", x => x.BlobCopyMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("  giải mã ảnh        ", x => x.DecodeMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("  thumbnail          ", x => x.ThumbMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("  lưu ảnh            ", x => x.SaveMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("TRIGGER → CALLBACK   ", x => x.TriggerToCallbackMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("TRIGGER → MÀN HÌNH   ", x => x.TriggerToScreenMs),
                new KeyValuePair<string, Func<CameraCycle, double>>("Nhịp giữa 2 kết quả  ", x => x.SinceLastResultMs)
            };

            foreach (var metric in stages.Concat(details))
            {
                var values = data.Select(metric.Value).Where(v => !double.IsNaN(v) && v >= 0).ToList();
                Log(values.Count == 0
                    ? $"{metric.Key}: không đo được"
                    : $"{metric.Key}: min {values.Min():0} | avg {values.Average():0} | max {values.Max():0} ms");
            }

            // Băng thông thật trên card mạng: byte nhận được giữa trigger và callback / thời gian đó.
            var withNet = data.Where(x => x.TriggerTs > 0 && x.NicBytesAtTrigger >= 0 && x.NicBytesAtArrival > x.NicBytesAtTrigger).ToList();
            if (withNet.Count > 0)
            {
                double mb = withNet.Average(x => (x.NicBytesAtArrival - x.NicBytesAtTrigger) / 1048576.0);
                var nic = _unitXNic;
                Log($"Dữ liệu ảnh mỗi lần chụp: {data.Average(x => x.ReturnBytes) / 1048576.0:0.0} MB ({data.Average(x => x.BlobCount):0.#} blob)"
                    + $"; card mạng nhận TB {mb:0.0} MB / chu trình"
                    + (nic != null ? $", link {nic.Speed / 1000000.0:0} Mbit/s" : string.Empty) + ".");
            }

            // Tốc độ truyền thực so với giới hạn của link: sát giới hạn = nghẽn ở đường mạng / dung lượng ảnh.
            // Ưu tiên thời gian dữ liệu thật sự chạy trên dây (đo theo lưu lượng card mạng); không có thì dùng chụp→callback.
            Func<CameraCycle, double> wire = x => !double.IsNaN(x.WireMs) ? x.WireMs : x.TransferNetMs;
            var transfer = data.Where(x => !double.IsNaN(wire(x)) && wire(x) > 0 && x.ReturnBytes > 0).ToList();
            if (transfer.Count > 0)
            {
                double mbPerImage = transfer.Average(x => x.ReturnBytes) / 1048576.0;
                double rate = mbPerImage / (transfer.Average(wire) / 1000.0);
                var link = _unitXNic;
                string limit = string.Empty;
                if (link != null && link.Speed > 0)
                {
                    double linkMBps = link.Speed / 8.0 / 1048576.0;
                    limit = $" - link {link.Speed / 1000000.0:0} Mbit/s tối đa ~{linkMBps:0} MB/s, {mbPerImage:0} MB cần tối thiểu "
                        + $"~{mbPerImage / linkMBps * 1000:0} ms ({rate / linkMBps * 100:0}% công suất link)";
                }

                Log($"Truyền ảnh về đạt ~{rate:0} MB/s ({mbPerImage:0.0} MB/ảnh){limit}.");
            }

            int empty = data.Count(x => x.EmptyBlobs > 0);
            if (empty > 0)
            {
                LogWarning($"{empty}/{data.Count} kết quả có blob RỖNG - ảnh đã bị UnitX xoá khỏi bộ nhớ blob trước khi PC tải kịp "
                    + "(kết quả dồn hàng đợi quá lâu).");
            }

            var rhythm = data.Where(x => !double.IsNaN(x.SinceLastResultMs)).Select(x => x.SinceLastResultMs).ToList();
            if (rhythm.Count > 0)
            {
                Log($"Năng suất thực: {60000.0 / rhythm.Average():0.0} ảnh/phút ({rhythm.Average():0} ms giữa 2 kết quả liên tiếp).");
            }

            // Khâu nghẽn = chặng nối tiếp có trung bình lớn nhất.
            // "Chờ lượt truyền" là hệ quả của dồn hàng, không phải nguyên nhân -> không xếp hạng.
            var ranked = stages
                .Where(s => !s.Key.StartsWith("Chờ lượt"))
                .Select(s => new { Name = s.Key.Trim(), Values = data.Select(s.Value).Where(v => !double.IsNaN(v) && v >= 0).ToList() })
                .Where(s => s.Values.Count > 0)
                .Select(s => new { s.Name, Avg = s.Values.Average() })
                .OrderByDescending(s => s.Avg)
                .ToList();
            double endToEnd = data.Select(x => x.TriggerToCallbackMs).Where(v => !double.IsNaN(v)).DefaultIfEmpty(0).Average();
            if (ranked.Count > 0)
            {
                Log($"KHÂU NGHẼN: {ranked[0].Name} ({ranked[0].Avg:0} ms/ảnh"
                    + (endToEnd > 0 ? $", {ranked[0].Avg / endToEnd * 100:0}% của trigger→callback {endToEnd:0} ms" : string.Empty) + ").");
            }

            // Nhịp trả kết quả ~ thời gian một chặng nối tiếp = chặng đó giới hạn năng suất.
            var netValues = data.Select(wire).Where(v => !double.IsNaN(v) && v > 0).ToList();
            if (rhythm.Count > 0 && netValues.Count > 0 && netValues.Average() >= rhythm.Average() * 0.8)
            {
                Log($"GIỚI HẠN NĂNG SUẤT: truyền ảnh về ({netValues.Average():0} ms/ảnh) gần bằng nhịp trả kết quả ({rhythm.Average():0} ms) - "
                    + "trigger nhanh hơn nhịp này chỉ làm ảnh dồn hàng đợi. Giảm dung lượng ảnh trả về hoặc tăng băng thông mạng.");
            }

            // Hàng đợi tăng dần: trigger nhanh hơn khả năng trả kết quả.
            var waits = data.Where(x => !double.IsNaN(x.TriggerToCaptureMs) || !double.IsNaN(x.TriggerToCallbackMs))
                .Select(x => x.TriggerToCallbackMs).Where(v => !double.IsNaN(v)).ToList();
            if (waits.Count >= 4 && waits.Skip(waits.Count / 2).Average() > waits.Take(waits.Count / 2).Average() * 1.5)
            {
                LogWarning($"Trigger → callback TĂNG DẦN trong loạt ({waits.Take(waits.Count / 2).Average():0} -> {waits.Skip(waits.Count / 2).Average():0} ms): "
                    + "lệnh chụp gửi nhanh hơn nhịp trả kết quả nên dồn hàng đợi - giảm nhịp trigger hoặc tối ưu khâu nghẽn.");
            }

            ExportCameraTimingCsv(data);
            Log("==========================================");
        }

        private void ExportCameraTimingCsv(List<CameraCycle> data)
        {
            try
            {
                Directory.CreateDirectory(AppDataDir);
                string path = Path.Combine(AppDataDir, $"camera_timing_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                var ci = System.Globalization.CultureInfo.InvariantCulture;
                Func<double, string> f = v => double.IsNaN(v) || v < 0 ? string.Empty : v.ToString("0.0", ci);

                var sb = new StringBuilder();
                sb.AppendLine("index,decision,trigger_call_ms,trigger_to_capture_ms,unitx_process_ms,transfer_ms,callback_ms,"
                    + "blob_copy_ms,decode_ms,thumbnail_ms,save_ms,display_wait_ms,display_paint_ms,trigger_to_callback_ms,"
                    + "trigger_to_screen_ms,since_last_result_ms,queue,return_bytes,blob_count,empty_blobs,nic_bytes,"
                    + "image_timestamp,result_timestamp,arrived_unix_ms");

                foreach (var c in data)
                {
                    long nic = c.NicBytesAtTrigger >= 0 && c.NicBytesAtArrival >= 0 && c.TriggerTs > 0 ? c.NicBytesAtArrival - c.NicBytesAtTrigger : -1;
                    sb.AppendLine(string.Join(",", c.Index, c.Decision ?? string.Empty, f(c.TriggerCallMs), f(c.TriggerToCaptureMs),
                        f(c.UnitXProcessMs), f(c.TransferMs), f(c.CallbackMs), f(c.BlobCopyMs), f(c.DecodeMs), f(c.ThumbMs), f(c.SaveMs),
                        f(c.DisplayWaitMs), f(c.DisplayPaintMs), f(c.TriggerToCallbackMs), f(c.TriggerToScreenMs), f(c.SinceLastResultMs),
                        c.QueueAtArrival, c.ReturnBytes, c.BlobCount, c.EmptyBlobs, nic >= 0 ? nic.ToString() : string.Empty,
                        c.ImageUnitXMs, c.ResultUnitXMs, c.ArrivedUnixMs.ToString("0", ci)));
                }

                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                Log("Đã xuất số liệu thời gian chụp: " + path);
            }
            catch (Exception ex)
            {
                Log("Xuất CSV thời gian chụp thất bại: " + ex.Message);
            }
        }
    }
}
