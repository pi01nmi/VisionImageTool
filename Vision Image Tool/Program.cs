using System;
using System.Threading;
using System.Windows.Forms;

namespace VisionImageTool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8FWW}");

        /// <summary>Tham số dòng lệnh của phiên được mở lại sau khi phiên cũ tự khởi động lại.</summary>
        private const string RestartArgument = "--restart";

        /// <summary>
        /// Đặt true rồi đóng form chính = khởi động lại phần mềm (xem frmMainV1.RestartApplication).
        /// Cần khi đổi client.yaml: unitx_sdk.dll chỉ đọc cấu hình mạng MỘT LẦN mỗi tiến trình, kết nối
        /// lại trong cùng tiến trình vẫn dùng cấu hình cũ và báo lỗi.
        /// </summary>
        internal static bool RestartRequested { get; set; }

        /// <summary>
        /// Giành quyền chạy duy nhất. Phiên cũ thoát mà không nhả mutex (bị kill) thì mutex thành
        /// "abandoned" - vẫn coi như giành được.
        /// </summary>
        private static bool AcquireSingleInstance(TimeSpan timeout)
        {
            try
            {
                return mutex.WaitOne(timeout, true);
            }
            catch (AbandonedMutexException)
            {
                return true;
            }
        }

        [STAThread]
        static void Main()
        {
            try
            {
                // Phiên được mở bởi RestartApplication() chờ phiên cũ thoát hẳn rồi mới giành mutex,
                // nếu không sẽ báo nhầm "Only one instance".
                bool restarted = Array.IndexOf(Environment.GetCommandLineArgs(), RestartArgument) >= 0;
                if (AcquireSingleInstance(restarted ? TimeSpan.FromSeconds(30) : TimeSpan.Zero))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMainV1());     // Vision Image Tool

                    if (RestartRequested)
                    {
                        // Form đã đóng hẳn (FormClosing đã StopSDK). Nhả mutex ngay (được giữ 2 lần:
                        // initiallyOwned + WaitOne) để phiên mới không phải đợi tiến trình này thoát
                        // hết các thread native của SDK, rồi mở phiên mới.
                        try
                        {
                            while (true)
                            {
                                mutex.ReleaseMutex();
                            }
                        }
                        catch (ApplicationException)
                        {
                            // Đã nhả hết.
                        }

                        System.Diagnostics.Process.Start(Application.ExecutablePath, RestartArgument);
                    }
                }
                else
                {
                    MessageBox.Show("Vision Image Tool đang chạy - chỉ mở được một cửa sổ.", "Vision Image Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Trước đây lỗi khởi động bị nuốt im lặng (chỉ báo lỗi của VisionMaster) - giờ luôn báo.
                MessageBox.Show("Vision Image Tool không khởi động được:" + Environment.NewLine + ex,
                    "Vision Image Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}