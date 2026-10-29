using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace VisionImageTool
{
    public enum RunMode
    {
        Production = 0,
        PositionStation1 = 1,
        PositionStation2 = 2,
        PositionStation3 = 3,
    }
    public enum Role
    {
        Admin,
        User
    };
    public enum Mode
    {
        None,
        Add,
        Edit,
        Delete
    };
    class cModelDB
    {
        //public static string ConStr = @"Filename=STC.db;Password=Admin123456*@;Connection=shared;Upgrade=true";
        public static string ConStr = @"Filename=STC.db;Connection=shared;Upgrade=true";
        public class TUser
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public int Role { get; set; }
            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }
        }
        public class TModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int NumItemRow { get; set; }
            public int NumItemCol { get; set; }
            public bool IsActive { get; set; }
            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }
        }
        public class TObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ModelId { get; set; }
            public string Code { get; set; }
            public int Order { get; set; }
            public int Low { get; set; }
            public int High { get; set; }
            public bool OK_NG { get; set; }
            public double Count { get; set; }
            public int Offset { get; set; }
            public int Color { get; set; }
            public int CosmeticType { get; set; }
            public int Type { get; set; }
            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }
        }
        public class TObjectResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ModelId { get; set; }
            public string BarCode { get; set; }
            public string _2DBC { get; set; }
            public List<TUnitXObject> UnitXResult { get; set; }
            public Dictionary<string, string> MeasurementResult { get; set; }
            //public List<string> MeasurementResult { get; set; }
            //public List<Bitmap>  ImageLive { get; set; }
            //public List<string> ImageLiveString { get; set; }
            //public List<Bitmap> ImageResult { get; set; }           
            //public List<string> ImageResultString { get; set; }
            //public List<int> ImageStatus { get; set; }
            public int  Status { get; set; } //0-OK , 1-NG
            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }

            //New for Fast PLC
            public Dictionary<int, Bitmap> ImageLiveEx { get; set; }
            public Dictionary<int, Bitmap> ImageResultEx { get; set; }
            public Dictionary<int, int> ImageStatusEx { get; set; }
        }
        public class TUnitXObject
        {
            public int Id { get; set; } 
            public string Code { get; set; }
            public int Order { get; set; }
            public string Name { get; set; }
            public int Low { get; set; }
            public int High { get; set; }
            public int Type { get; set; }
            public int Status { get; set; }
            public long Count { get; set; }
            public List<TUnitXSegment> Segments { get; set; }
        }
        public class TUnitXSegment
        {
            public float original_x { get; set; }
            public float original_y { get; set; }
            public float original_mr_length { get; set; }
            public float original_mr_width { get; set; }
            public float original_vh_width { get; set; }
            public float original_vh_height { get; set; }
            public float original_area { get; set; }
            public List<Contour> contours { get; set; }
        }
        public class TList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public int Type { get; set; }
            public object Tag { get; set; }
            public int Color { get; set; }
        }
        public class TConfig
        {
            public int Id { get; set; }
            public int Communication { get; set; }
            public string ComPort { get; set; }
            public int ComDelay { get; set; }
            public int isSaveData { get; set; }
            public int isSaveLive { get; set; }
            public int isSaveOK { get; set; }
            public int isSaveNG { get; set; }
            public int isAutoCheck { get; set; }
            public int isCalib { get; set; }
            public string SaveFolder { get; set; }
            public int ImageType { get; set; }
            public int ImageQuality { get; set; }            
            public int MaxObject { get; set; }
            public decimal YieldMin { get; set; }
            public int Yield100Max { get; set; }
            public int TimeLog { get; set; }
            public string MachineName { get; set; }
            public string Password { get; set; }
            public string PLCIP { get; set; }
            public int PLCPort { get; set; }
            public string PLCReset { get; set; }
            public string PLCProgram { get; set; }
            public string PLCBarcode { get; set; }
            public string PLCAngle { get; set; }
            public string PLCStart { get; set; }

            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }

        }
        public class TYield
        {
            public int Id { get; set; }
            public int Station { get; set; }
            public string Barcode { get; set; }
            public decimal OK { get; set; }
            public decimal Yield { get; set; }
        }
        public class TReport
        {
            public int Id { get; set; }
            public int ObjectID { get; set; }
            public string ObjectCode { get; set; }
            public string ObjectName { get; set; }
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public int Count { get; set; }
        }

        public class TReport2
        {
            public int Id { get; set; }
            public string BarCode { get; set; } // Sheet
            public string _2DBC { get; set; }  //A-1 - Part ID
            public DateTime Created_at { get; set; } //  Date
            public int Index { get; set; } // index
            public string Name  { get; set; } // A-1
            public string UnitXValue { get; set; } // OK - Contamination-Epoxy
            public string Xcoordinate { get; set; }
            public string Ycoordinate { get; set; }
            public string Xcoordinate2 { get; set; }
            public string Ycoordinate2 { get; set; }

            public string QRDegree { get; set; }
            public string FAI2 { get; set; }
            public string FAI121 { get; set; }
            public string FAI122 { get; set; }
            public string FAI123 { get; set; }
            public string FAI124 { get; set; }
            public string FAI125 { get; set; }
            public string FAI126 { get; set; }
            public string FAI127 { get; set; }
            public string FAI128 { get; set; }
            public string FAI4 { get; set; }
        }
        public sealed class ImageResultCopiedData
        {
            public int StationID { get; set; }
            public List<TUnitXObject> UnitXObjects { get; set; }
            public int optixIndex { get; set; }
            public bool isNotCalib { get; set; }
            public ImageDimension Dimension { get; set; }
            public List<byte[]> Blobs { get; set; }
            public int ImageStatus { get; set; }
        }
        public class TCamera
        {
            public int Id { get; set; }
            public int ModelId { get; set; }
            public int Name { get; set; }
            public string Serial { get; set; }
            // 0 GigE 1 Webcam 2 Webcam
            public int Type { get; set; }
            public int Format { get; set; }
            public bool IsActive { get; set; }
            public DateTime Created_at { get; set; }
            public DateTime Updated_at { get; set; }
        }
    }
    [Serializable]
    public class LicenseData
    {
        public int SchemaVersion { get; set; }
        public string LicenseId { get; set; }
        public string Customer { get; set; }
        public string ProductCode { get; set; }
        public string Edition { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ExpireAtUtc { get; set; }
        public int GraceDays { get; set; }
        public string MachineId { get; set; }
        public List<string> Features { get; set; }
        public int MaxStations { get; set; }
        public int RenewalCount { get; set; }
        public string Notes { get; set; }
        public string Signature { get; set; }
    }
    [Serializable]
    public class LicenseRequestData
    {
        public int SchemaVersion { get; set; }
        public string RequestId { get; set; }
        public string Customer { get; set; }
        public string ProductCode { get; set; }
        public string ProductVersion { get; set; }
        public string MachineName { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public MachineFingerprint MachineFingerprint { get; set; }
        public string MachineId { get; set; }
        public string AppInstanceId { get; set; }
        public string RequestChecksum { get; set; }
    }
    [Serializable]
    public class MachineFingerprint
    {
        public string BiosSerial { get; set; }
        public string BaseboardSerial { get; set; }
        public string CpuId { get; set; }
        public string SystemDriveSerial { get; set; }
    }
    public static class MachineIdProvider
    {
        public static MachineFingerprint GetFingerprint()
        {
            return new MachineFingerprint
            {
                BiosSerial = GetFirstWmiValue("Win32_BIOS", "SerialNumber"),
                BaseboardSerial = GetFirstWmiValue("Win32_BaseBoard", "SerialNumber"),
                CpuId = GetFirstWmiValue("Win32_Processor", "ProcessorId"),
                SystemDriveSerial = GetSystemDriveSerial()
            };
        }

        public static string GetMachineId()
        {
            var fp = GetFingerprint();

            var raw = string.Join("|", new[]
            {
                Normalize(fp.BiosSerial),
                Normalize(fp.BaseboardSerial),
                Normalize(fp.CpuId),
                Normalize(fp.SystemDriveSerial)
            });

            return HashUtility.Sha256Hex(raw);
        }

        private static string GetFirstWmiValue(string className, string propertyName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM " + className))
                using (var collection = searcher.Get())
                {
                    foreach (var obj in collection.Cast<ManagementObject>())
                    {
                        var value = obj[propertyName];
                        if (value != null)
                            return value.ToString();
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string GetSystemDriveSerial()
        {
            try
            {
                var systemDrive = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);

                using (var logicalDiskSearcher = new ManagementObjectSearcher(
                    "SELECT DeviceID, VolumeSerialNumber FROM Win32_LogicalDisk"))
                using (var logicalDisks = logicalDiskSearcher.Get())
                {
                    foreach (var disk in logicalDisks.Cast<ManagementObject>())
                    {
                        var deviceId = Convert.ToString(disk["DeviceID"]);
                        if (string.Equals(deviceId, systemDrive, StringComparison.OrdinalIgnoreCase))
                        {
                            return Convert.ToString(disk["VolumeSerialNumber"]) ?? string.Empty;
                        }
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
    public static class HashUtility
    {
        public static string Sha256Hex(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
    public static class JsonCanonicalizer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
        };

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
    public static class LicenseCrypto
    {
        public static string Sign(string content, string privateKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(privateKeyXml);
                var data = Encoding.UTF8.GetBytes(content);
                var signature = rsa.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(signature);
            }
        }

        public static bool Verify(string content, string signatureBase64, string publicKeyXml)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(publicKeyXml);
                    var data = Encoding.UTF8.GetBytes(content);
                    var signature = Convert.FromBase64String(signatureBase64);
                    return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature);
                }
            }
            catch
            {
                return false;
            }
        }
    }
    [Serializable]
    public class LicenseSigningPayload
    {
        public int SchemaVersion { get; set; }
        public string LicenseId { get; set; }
        public string Customer { get; set; }
        public string ProductCode { get; set; }
        public string Edition { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ExpireAtUtc { get; set; }
        public int GraceDays { get; set; }
        public string MachineId { get; set; }
        public List<string> Features { get; set; }
        public int MaxStations { get; set; }
        public int RenewalCount { get; set; }
        public string Notes { get; set; }
    }
    public static class LicenseMapper
    {
        public static LicenseSigningPayload ToSigningPayload(LicenseData license)
        {
            return new LicenseSigningPayload
            {
                SchemaVersion = license.SchemaVersion,
                LicenseId = license.LicenseId,
                Customer = license.Customer,
                ProductCode = license.ProductCode,
                Edition = license.Edition,
                IssuedAtUtc = license.IssuedAtUtc,
                ValidFromUtc = license.ValidFromUtc,
                ExpireAtUtc = license.ExpireAtUtc,
                GraceDays = license.GraceDays,
                MachineId = license.MachineId,
                Features = license.Features,
                MaxStations = license.MaxStations,
                RenewalCount = license.RenewalCount,
                Notes = license.Notes
            };
        }
    }
    [Serializable]
    public class RuntimeState
    {
        public DateTime LastRunUtc { get; set; }
        public DateTime LastVerifiedLicenseExpiryUtc { get; set; }
        public string MachineId { get; set; }
        public string StateHmac { get; set; }
    }
    public static class RuntimeStateProtector
    {
        private const string EmbeddedSecret = "NMI_LICENSE_STATE_SECRET_V1_CHANGE_THIS";

        public static string ComputeHmac(RuntimeState state)
        {
            var payload = string.Join("|",
                state.LastRunUtc.ToString("O"),
                state.LastVerifiedLicenseExpiryUtc.ToString("O"),
                state.MachineId ?? string.Empty);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(EmbeddedSecret)))
            {
                var bytes = Encoding.UTF8.GetBytes(payload);
                var hash = hmac.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool IsValid(RuntimeState state)
        {
            if (state == null) return false;
            var expected = ComputeHmac(state);
            return string.Equals(expected, state.StateHmac, StringComparison.Ordinal);
        }

        public static void Seal(RuntimeState state)
        {
            state.StateHmac = ComputeHmac(state);
        }
    }
    public static class FileStorage
    {
        public static T LoadJson<T>(string path) where T : class
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveJson<T>(string path, T data)
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public bool IsInGracePeriod { get; set; }
        public bool IsTimeRollbackDetected { get; set; }
        public string Message { get; set; }

        public static LicenseValidationResult Fail(string message)
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = message
            };
        }

        public static LicenseValidationResult Success(string message, bool expired, bool inGrace, bool rollback)
        {
            return new LicenseValidationResult
            {
                IsValid = true,
                IsExpired = expired,
                IsInGracePeriod = inGrace,
                IsTimeRollbackDetected = rollback,
                Message = message
            };
        }
    }
    public class LicenseValidator
    {
        private readonly string _publicKeyXml;
        private readonly string _licensePath;
        private readonly string _statePath;

        public LicenseValidator(string publicKeyXml, string licensePath, string statePath)
        {
            _publicKeyXml = publicKeyXml;
            _licensePath = licensePath;
            _statePath = statePath;
        }

        public LicenseValidationResult ValidateAndUpdateState()
        {
            var license = FileStorage.LoadJson<LicenseData>(_licensePath);
            if (license == null)
                return LicenseValidationResult.Fail("Không tìm thấy license.");

            if (!VerifySignature(license))
                return LicenseValidationResult.Fail("License signature không hợp lệ.");

            var currentMachineId = MachineIdProvider.GetMachineId();
            if (!string.Equals(license.MachineId, currentMachineId, StringComparison.OrdinalIgnoreCase))
                return LicenseValidationResult.Fail("License không đúng máy.");

            var nowUtc = DateTime.UtcNow;

            var state = FileStorage.LoadJson<RuntimeState>(_statePath);
            var rollbackDetected = false;

            if (state != null)
            {
                if (!RuntimeStateProtector.IsValid(state))
                    return LicenseValidationResult.Fail("Runtime state bị sửa hoặc không hợp lệ.");

                if (!string.Equals(state.MachineId, currentMachineId, StringComparison.OrdinalIgnoreCase))
                    return LicenseValidationResult.Fail("Runtime state không khớp máy hiện tại.");

                if (nowUtc < state.LastRunUtc.AddMinutes(-5))
                {
                    rollbackDetected = true;
                }
            }

            var expired = nowUtc > license.ExpireAtUtc;
            var graceExpireAt = license.ExpireAtUtc.AddDays(license.GraceDays);
            var inGrace = expired && nowUtc <= graceExpireAt;

            if (rollbackDetected)
            {
                return LicenseValidationResult.Fail("Phát hiện thời gian hệ thống bị lùi.");
            }

            if (nowUtc < license.ValidFromUtc)
            {
                return LicenseValidationResult.Fail("License chưa có hiệu lực.");
            }

            if (expired && !inGrace)
            {
                return LicenseValidationResult.Fail("License đã hết hạn.");
            }

            var newState = new RuntimeState
            {
                LastRunUtc = nowUtc,
                LastVerifiedLicenseExpiryUtc = license.ExpireAtUtc,
                MachineId = currentMachineId
            };
            RuntimeStateProtector.Seal(newState);
            FileStorage.SaveJson(_statePath, newState);

            return LicenseValidationResult.Success(
                inGrace ? "License đã hết hạn nhưng đang trong thời gian grace." : "License hợp lệ.",
                expired,
                inGrace,
                false);
        }

        private bool VerifySignature(LicenseData license)
        {
            var signature = license.Signature;
            if (string.IsNullOrWhiteSpace(signature))
                return false;

            var payload = LicenseMapper.ToSigningPayload(license);
            var canonicalJson = JsonCanonicalizer.SerializeObject(payload);
            return LicenseCrypto.Verify(canonicalJson, signature, _publicKeyXml);
        }
    }
    public static class LicenseRequestGenerator
    {
        public static LicenseRequestData Create(string customer, string productCode, string productVersion)
        {
            var fp = MachineIdProvider.GetFingerprint();
            var machineId = MachineIdProvider.GetMachineId();

            var req = new LicenseRequestData
            {
                SchemaVersion = 1,
                RequestId = "REQ-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                Customer = customer,
                ProductCode = productCode,
                ProductVersion = productVersion,
                MachineName = Environment.MachineName,
                GeneratedAtUtc = DateTime.UtcNow,
                MachineFingerprint = fp,
                MachineId = machineId,
                AppInstanceId = Guid.NewGuid().ToString("N")
            };

            req.RequestChecksum = HashUtility.Sha256Hex(
                req.RequestId + "|" + req.Customer + "|" + req.ProductCode + "|" + req.MachineId + "|" + req.GeneratedAtUtc.ToString("O"));

            return req;
        }
    }
    #region Struct
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SDKMetaData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string version;
        public long timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string token;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ImageDimension
    {
        public long height;
        public long width;
        public long channel;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SystemStatus
    {
        public SDKMetaData meta_data;
        public int status_code;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string message;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartGroupStart
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_ts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1280)]
        public byte[] part_ids;
        public long part_ids_count;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartGroupEnd
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_ts;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartGroupReset
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_ts;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartGroupUpdate
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_ts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1280)]
        public byte[] part_ids;
        public long part_ids_count;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CameraReset
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_ts;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string camera_id;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartGroupResult
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        public long part_group_timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        public IntPtr part_results;
        public long part_results_count;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ImageResult
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string image_id;
        public long image_timestamp;
        public ImageDimension image_dimension;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        public long part_group_timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string camera_id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string capture_config;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string capture_config_detail;
        public int capture_index;
        public int repeat_index;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string sequence;
        public int n_capture;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string raw_image_blob_key;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string overlaid_mask_blob_key;
        public IntPtr bin_mask_blob_keys;
        public long bin_mask_blob_keys_count;
        public IntPtr part_image_results;
        public long part_image_results_count;
        public IntPtr defect_network_results;
        public long defect_network_results_count;
        public IntPtr location_network_results;
        public long location_network_results_count;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CameraResult
    {
        public SDKMetaData meta_data;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_group_id;
        public long part_group_timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_type;
        public IntPtr part_results;
        public long part_results_count;
        public IntPtr camera_cc_image_messages;
        public long camera_cc_image_messages_count;
        public IntPtr image_count_mismatches;
        public long image_count_mismatches_count;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string camera_id;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ErrorInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string description;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string source;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string trace_stack;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string timestamp;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SubscriptionPayload
    {
        public UnitXPayloadType type;
        public SystemStatus system_status;
        public PartGroupStart part_group_start;
        public PartGroupEnd part_group_end;
        public PartGroupUpdate part_group_update;
        public PartGroupReset part_group_reset;
        public CameraReset camera_reset;
        public PartGroupResult part_group_result;
        public ImageResult image_result;
        public CameraResult camera_result;
        public ErrorInfo error_info;
    }
    #endregion
    #region New Structs
    public enum Decision
    {
        DECISION_OK = 0,      // Decision is OK.
        DECISION_NG = 1,      // Decision is NG.
        DECISION_LIMIT = 2    // Decision is at the limit.
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SegmentStats
    {
        public float original_x;
        public float original_y;
        public float original_mr_length;
        public float original_mr_width;
        public float original_vh_width;
        public float original_vh_height;
        public float original_area;
        public float original_circumference;

        public IntPtr original_mr_box;       // Pointer to Point[]
        public long original_mr_box_count;

        public Decision decision;

        public IntPtr original_contours;     // Pointer to Contour[] (define late)
        public long original_contours_count;

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct DefectStats
    {
        public long defect_class_id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string defect_name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string defect_color;

        public Decision decision;

        public long count;

        public IntPtr segments;     // SegmentStats*
        public long segments_count;

        public long feature_id;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct DefectNetworkResult
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string network_name;
        public long model_version;
        public IntPtr defects;
        public long defects_count;
        public Decision decision;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct PartImageResult
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_id;

        public Decision decision;

        public PartRegion part_region;

        public IntPtr defect_network_results;
        public long defect_network_results_count;

        public IntPtr location_network_results;
        public long location_network_results_count;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct PartResult
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_id;
        public Decision decision;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Point
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Contour
    {
        public IntPtr contour;     // Pointer to Point[]
        public long contour_count;
        [MarshalAs(UnmanagedType.I1)]
        public bool is_parent;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct Region
    {
        public long region_index;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string region_code;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string part_id;

        public IntPtr polygon;        // -> array of Point
        public long polygon_count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PartRegion
    {
        public IntPtr regions;       // -> array of Region
        public long regions_count;
    }
    public enum UnitXPayloadType
    {
        UNITX_PAYLOAD_NULL = 0,
        UNITX_PAYLOAD_SYSTEM_STATUS = 1,
        UNITX_PAYLOAD_PART_GROUP_START = 2,
        UNITX_PAYLOAD_PART_GROUP_END = 3,
        UNITX_PAYLOAD_PART_GROUP_UPDATE = 4,
        UNITX_PAYLOAD_PART_GROUP_RESET = 5,
        UNITX_PAYLOAD_CAMERA_RESET = 6,
        UNITX_PAYLOAD_PART_GROUP_RESULT = 7,
        UNITX_PAYLOAD_IMAGE_RESULT = 8,
        UNITX_PAYLOAD_CAMERA_RESULT = 9,
        UNITX_PAYLOAD_ERROR_INFO = 10
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SubscriptionBlobs
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string blob_name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string sub_blob_name;
        public IntPtr blob_data;
        public long blob_size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StringKeyValue
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string key;
        [MarshalAs(UnmanagedType.LPStr)]
        public string value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageResultKeyValue
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string key;
        public ImageResult value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageCountMismatches
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string capture_config;
        public long expected;
        public long received;
    }
    public enum UnitXTopic
    {
        UNITX_TOPIC_SYSTEM_STATUS = 1,
        UNITX_TOPIC_PART_GROUP_START = 2,
        UNITX_TOPIC_PART_GROUP_END = 3,
        UNITX_TOPIC_PART_GROUP_UPDATE = 4,
        UNITX_TOPIC_PART_GROUP_RESET = 5,
        UNITX_TOPIC_CAMERA_RESET = 6,
        UNITX_TOPIC_PART_GROUP_RESULT = 11,
        UNITX_TOPIC_IMAGE_RESULT = 12,
        UNITX_TOPIC_CAMERA_RESULT = 13,
        UNITX_TOPIC_ERROR_OVERALL = 21,
        UNITX_TOPIC_ERROR_PRODUCTION = 22,
        UNITX_TOPIC_ERROR_OPTIX = 23,
        UNITX_TOPIC_ERROR_CORTEX = 24,
        UNITX_TOPIC_ERROR_BLOB_STORAGE = 25
    }
    #endregion
}
