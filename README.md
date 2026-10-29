# Vision Image Tool

![Vision Image Tool](docs/Vision%20Image%20Tool.png)

Phần mềm Windows hỗ trợ kiểm thử hệ thống AI Vision **UnitX**: đẩy ảnh / chụp camera qua UnitX SDK, nhận kết quả OK/NG, và quản lý dataset ảnh Raw/Mask để lập báo cáo test.

## Tính năng chính

### Capture & Monitor
- Kết nối UnitX qua SDK: kiểm tra mạng (gRPC / Blob / ZMQ), trạng thái hệ thống, cấu hình Production.
- **Push ảnh** từ thư mục sang UnitX, tự lưu ảnh Mask kết quả cạnh ảnh gốc.
- **Trigger camera** (một ảnh / lặp / liên tục), lưu ảnh Raw + Mask kèm kết quả OK/NG.
- Đo thời gian từng chặng (push, xử lý, truyền ảnh, hiển thị) để tìm điểm nghẽn, xuất CSV.

### Dataset Manager
- Đọc dataset theo **schema** do người dùng định nghĩa, theo tên file và/hoặc cấp thư mục, ví dụ:
  `<ĐK>\<Raw/Mask>\<Model>\<Mã NG>\<SPL>\<Bỏ qua>_<Đánh giá>.<Extension>`
- Ghép cặp Raw ↔ Mask, nhiều điều kiện chụp (ĐK), nhiều lần chụp trên một hàng.
- So sánh **Vision** (kết quả AI) với **Đánh giá** (nhãn của người kiểm), thống kê O / Δ / X.
- Phát hiện vấn đề dữ liệu: thiếu Mask/Raw, sai schema, trùng mẫu, file ảnh hỏng.
- Xuất báo cáo **Excel** (kèm ảnh thumbnail) và **JSON**.

## Yêu cầu

- Windows 10/11 x64, .NET Framework 4.8
- Visual Studio 2022 trở lên (workload *.NET desktop development*)
- Thư viện UnitX SDK (`unitx_sdk.dll`, `unitx_sdk_interface.dll`) và `Basler.Pylon.dll`: **không có trong repo**, cần chép vào thư mục chạy (`Vision Image Tool\bin\<Configuration>\`).

## Build & chạy

1. Clone repo và mở `VisionImageTool.sln` bằng Visual Studio.
2. Chuột phải Solution → **Restore NuGet Packages**.
3. Chép các DLL của UnitX SDK / Basler vào thư mục output.
4. Chọn cấu hình **Debug** hoặc **Release | x64** → **Build** → chạy.

Cấu hình và log của phần mềm lưu tại thư mục `pushv13` cạnh file exe.

## Tài liệu

- [Hướng dẫn lập báo cáo test AVI (PDF)](docs/N-HD-NMIAVITEST-01_Huong-dan-lap-bao-cao-test-AVI.pdf)
- [Hướng dẫn sử dụng sản phẩm (DOCX)](docs/product-guide.docx)
- [Lưu đồ xử lý dataset (DOCX)](docs/Luu-do-xu-ly-dataset.docx)
