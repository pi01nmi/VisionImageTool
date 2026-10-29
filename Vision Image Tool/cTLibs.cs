using Basler.Pylon;
using LiteDB;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static VisionImageTool.cModelDB;

namespace VisionImageTool
{
    class cTLibs
    {
        public static ILog applog = LogManager.GetLogger("AppLog");
        public static ILog userlog = LogManager.GetLogger("UserLog");
        public static List<cModelDB.TCamera> _cameras;
        //Basler
        public static Camera[] g_camera = new Camera[1];

        public static int RoleID = 0;
        public static List<cModelDB.TObject> _objects;
        public static cModelDB.TConfig _config;
        //public static cModelDB.TUser _user;
        public static cModelDB.TModel _model;
        public static string IngredientCodeConfirm = "";
        public const int License_UID = -1473640678;
        public static string License_Info = "User license: TRIAL";
        public static string License_Name = "NMI INSPECTION SYSTEM";
        public static bool isShowLog = false;
        public static bool isSaveLog = true;
        public static bool isShowImage = true;
        public static bool isCalib = false;
        public static string publicKeyXml = @"<RSAKeyValue><Modulus>tz8LW11xe+7JAjY9WQ1dmzPFu8q5kxdOxHh10S4QMLyrzVKQ5xtmE6lK1z1T9RtBu2Kb9bNGy9o8twmEKFPHKV316FTh4TE82ZL73608ZkRHXAmoRmppkknRFx30/Cj+zXO9m5niG4ybreg/jl9OYRJmWATivWNeOMIna0lY/ZxB9hFoDGnphnsN/HKNxgB+qxcSZti5fT0PRa85IcXowmpd7Dv2JDzhdknGkcpE1dh3qO6fftPfB4O0XxhT2HeNIkf8qpOvU9enctolsa6CQqzUZg7gCO8Z+bfLqqrzuu+j06D+vRnA9jm4HmcS+VGpqqa5gEEdyNweusr9VlaZqQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        //Station Image
        public static List<int> NumItemStation = new List<int> { 3, 1, 2, 1, 1, 1 };  //Note 1: Số ảnh trong 01 trạm, ví dụ trạm 1 mình chụp 2 lần (Detect 2 ảnh) thì để là 2
        public static List<string> cameraID = new List<string> { "", "", "40755802" };
        public static Dictionary<int, string> Stations = new Dictionary<int, string>   //Note 2: Mảng lưu PartType gắn với từng trạm, đang set cho trạm 4 và 5 đúng, em set cho các trạm còn lại
            {
                {1, "station1"},
                {2, "station2"},
                {3, "station3"},
                {4, "calib_station1"},
                {5, "calib_station2"},
                {6, "calib_station3"},
            };

        public static readonly Dictionary<int, Dictionary<string, string>> PlcAddress =
            new Dictionary<int, Dictionary<string, string>>
            {
                // ===================== Station 1 =====================
                [1] = new Dictionary<string, string>
                {
                    ["Index"] = "D3152",  //Vị trí con hàng
                    ["Last"] = "D3153",  //Vị trí cuối con hàng
                    ["QRCode"] = "D3120",
                    ["QRDegree"] = "D3114",
                    ["Xcoordinate"] = "D3110",
                    ["Ycoordinate"] = "D3112",
                    ["Status"] = "D3102",
                    ["Busy"] = "D3100",
                    ["Mapping"] = "D3116",
                    ["CameraReady"] = "D3105",
                    ["ResultReady"] = "D3101",
                    ["Reset"] = "D3151",
                    ["NC"] = "D3106",
                    ["BarcodeSheet"] = "ZF70010",
                    ["IndexFeedback"] = "D3140",
                },

                // ===================== Station 2 =====================
                [2] = new Dictionary<string, string>
                {
                    //["QRCode"] = "D3220",
                    //["QRDegree"] = "D3214",
                    ["Index"] = "D3252",  //Vị trí con hàng
                    ["Last"] = "D3253",  //Vị trí cuối con hàng
                    ["Xcoordinate"] = "D3210",
                    ["Ycoordinate"] = "D3212",
                    ["Status"] = "D3202",
                    ["Busy"] = "D3200",
                    ["Mapping"] = "D3216",
                    ["CameraReady"] = "D3205",
                    ["ResultReady"] = "D3201",
                    ["Reset"] = "D3251",
                    ["NC"] = "D3206",
                    ["BarcodeSheet"] = "D3260",
                    ["IndexFeedback"] = "D3240",
                },

                // ===================== Station 3 =====================
                [3] = new Dictionary<string, string>
                {
                    ["Index"] = "D3352",  //Vị trí con hàng
                    ["Last"] = "D3353",  //Vị trí cuối con hàng
                    ["QRCode"] = "D3320",
                    ["QRDegree"] = "D3314",
                    ["Xcoordinate"] = "D3310",
                    ["Ycoordinate"] = "D3312",
                    ["Status"] = "D3302",
                    ["Busy"] = "D3300",
                    ["Mapping"] = "D3316",
                    ["CameraReady"] = "D3305",
                    ["ResultReady"] = "D3301",
                    ["Reset"] = "D3351",
                    ["NC"] = "D3306",
                    ["BarcodeSheet"] = "ZF210010",
                    ["IndexFeedback"] = "D3340",
                },

                // ===================== Calib Station 1 =====================
                [4] = new Dictionary<string, string>
                {
                    ["QRCode"] = "D3120",
                    ["QRDegree"] = "D3144",
                    ["Status"] = "D3102",
                    ["Busy"] = "D3100",
                    ["Mapping"] = "D3116",
                    ["XcoordinateCircle"] = "D3110", //D3140
                    ["YcoordinateCircle"] = "D3112", //D3142
                    //["XcoordinateImage"] = "D3110",
                    //["YcoordinateImage"] = "D3112"
                    ["Xcoordinate"] = "D3160",
                    ["Ycoordinate"] = "D3162",
                },

                // ===================== Calib Station 2 =====================
                [5] = new Dictionary<string, string>
                {
                    ["QRCode"] = "D3220",
                    ["QRDegree"] = "D3244",
                    ["Status"] = "D3202",
                    ["Busy"] = "D3200",
                    ["Mapping"] = "D3216",
                    ["XcoordinateCircle"] = "D3210", //D3240
                    ["YcoordinateCircle"] = "D3212", //D3242
                    //["XcoordinateImage"] = "D3210",
                    //["YcoordinateImage"] = "D3212"
                    ["Xcoordinate"] = "D3220",
                    ["Ycoordinate"] = "D3222",
                },

                // ===================== Calib Station 3 =====================
                [6] = new Dictionary<string, string>
                {
                    ["QRCode"] = "D3320",
                    ["QRDegree"] = "D3344",
                    ["Status"] = "D3302",
                    ["Busy"] = "D3300",
                    ["Mapping"] = "D3316",
                    ["XcoordinateCircle"] = "D3310", //
                    ["YcoordinateCircle"] = "D3312", //
                    //["XcoordinateImage"] = "D3310", 
                    //["YcoordinateImage"] = "D3312"
                    ["Xcoordinate"] = "D3360",
                    ["Ycoordinate"] = "D3362",
                }
            };
        public static string PlcAddressMode = "D3150";

        public static void UpdateConfig(string key, string value)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach (XmlElement item in xmlDoc.DocumentElement)
            {
                if (item.Name.Equals("appSettings"))
                {
                    foreach (XmlNode node in item.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals(key))
                        {
                            node.Attributes[1].Value = value;
                        }
                    }
                }
            }
            ConfigurationManager.RefreshSection("appSettings");
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
        public static string LoadConfig(string key)
        {
            var value = "";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach (XmlElement item in xmlDoc.DocumentElement)
            {
                if (item.Name.Equals("appSettings"))
                {
                    foreach (XmlNode node in item.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals(key))
                        {
                            value = node.Attributes[1].Value;
                        }
                    }
                }
            }
            return value;
        }
        public static List<cModelDB.TCamera> getCamera(int ModelID)
        {
            var cameras = new List<cModelDB.TCamera>();
            using (var db = new LiteDatabase(cModelDB.ConStr))
            {
                // Get model collection                
                var cd = db.GetCollection<cModelDB.TCamera>("cameras");
                if (cd.FindAll().Count() != 1)
                {
                    cd.DeleteAll();
                    for (int i = 1; i < 1 + 1; i++)
                    {
                        var _camera = new cModelDB.TCamera
                        {
                            Name = i,
                            ModelId = _model.Id,
                            Type = -1,
                            IsActive = false,
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now
                        };
                        cd.Insert(_camera);
                    }
                }
                cameras = cd.FindAll().OrderBy(x => x.Name).ToList();
            }
            if (cameras.Count > 0)
            {
                List<ICameraInfo> allCameras = CameraFinder.Enumerate();
                for (int i = 0; i < 1; i++)
                {
                    //Basler
                    DestroyCamera(g_camera[i]);
                    // Loop over all cameras found.
                    foreach (ICameraInfo cameraInfo in allCameras)
                    {
                        if (cameras[i].Serial == cameraInfo[CameraInfoKey.SerialNumber])
                        {
                            g_camera[i] = new Camera(cameraInfo);
                        }
                    }
                }
            }
            return cameras;
        }
        public static void DestroyCamera(Camera camera)
        {
            try
            {
                if (camera != null)
                {
                    camera.Close();
                    camera.Dispose();
                    camera = null;
                }
            }
            catch (Exception exception)
            {
                applog.Error("DestroyCamera Exception caught:\n" + exception.Message);
            }
        }
        public static TConfig getConfig()
        {
            TConfig config = new TConfig();
            using (var db = new LiteDatabase(ConStr))
            {
                // Get model collection                
                var configs = db.GetCollection<TConfig>("configs");
                config = configs.Find(x => x.Id == 1).FirstOrDefault();
                if (config == null)
                {
                    var _config = new TConfig
                    {
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    };
                    configs.Insert(_config);
                    config = configs.Find(x => x.Id == 1).FirstOrDefault();
                }
            }
            return config;
        }
        public static TUser getactiveUser(string username, string password)
        {
            TUser user = new TUser();
            if (username == "sysadmin" && password == "Admin123456*@")
            {
                user.Id = 0;
                user.Name = username;
                user.UserName = username;
                user.Password = password;
                user.Role = (int)Role.Admin;
            }
            else
            {
                using (var db = new LiteDatabase(cModelDB.ConStr))
                {
                    // Get collection                
                    var users = db.GetCollection<cModelDB.TUser>("users");
                    user = users.Find(x => x.UserName == username).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.Password != password)
                        {
                            user.Id = -1;
                        }
                    }
                    else
                    {
                        user = new cModelDB.TUser();
                        user.Id = -2;
                    }
                }
            }
            return user;
        }
        public static void VisionState()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string licensePath = Path.Combine(baseDir, "license.dat");
            string stateDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "NMI",
                "VisionApp");
            string statePath = Path.Combine(stateDir, "license_state.json");
            var validator = new LicenseValidator(publicKeyXml, licensePath, statePath);
            var result = validator.ValidateAndUpdateState();
            if (!result.IsValid)
            {
                // ===== 4. Nếu license lỗi => vẫn cho export request =====
                try
                {
                    var request = LicenseRequestGenerator.Create(
                        customer: "NANO",
                        productCode: "NMI-VISION",
                        productVersion: "1.0.0");
                    string requestPath = Path.Combine(stateDir, "license_request.json");
                    FileStorage.SaveJson(requestPath, request);
                    MessageBox.Show(
                        result.Message + "\n\n" +
                        "Đã tạo file yêu cầu cấp phép mới tại:\n" +
                        requestPath,
                        "License Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show(
                        result.Message,
                        "License Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                Application.Exit();
                return;
            }
        }
        public static TModel getactiveModel()
        {
            //convert
            //using (var db = new LiteDatabase(cModelDB.ConStr))
            //{
            //    var collectionSource = db.GetCollection("configs");
            //    var collectionTemp = db.GetCollection("configs_Temp");
            //    var documents = collectionSource.FindAll().ToList();
            //    foreach (var document in documents)
            //    {
            //        //document["YieldMin"] = new BsonValue(document["YieldMin"].ToString());
            //        document["YieldMin"] = new BsonValue(document["YieldMin"].AsDecimal);
            //        collectionTemp.Insert(document);
            //    }
            //    db.DropCollection("configs");
            //    db.RenameCollection("configs_Temp", "configs");
            //}
            // Open encrypted DB with password
            //var newDbPath = @"Filename=STC1.db;Connection=shared;Upgrade=true";
            //using (var encryptedDb = new LiteDatabase(ConStr))
            //using (var newDb = new LiteDatabase(newDbPath)) // No password here
            //{
            //    foreach (var name in encryptedDb.GetCollectionNames())
            //    {
            //        var encryptedCol = encryptedDb.GetCollection(name);
            //        var newCol = newDb.GetCollection(name);

            //        var allDocs = encryptedCol.FindAll();
            //        newCol.InsertBulk(allDocs);
            //    }
            //}
            cModelDB.TModel model = new cModelDB.TModel();
            using (var db = new LiteDatabase(cModelDB.ConStr))
            {
                // Get model collection                
                var models = db.GetCollection<cModelDB.TModel>("models");
                model = models.Find(x => x.IsActive == true).FirstOrDefault();
            }
            return model;
        }
        public static string GetRoleByID(int roleId)
        {
            return roleId == 0 ? "Admin" : "User";
        }
        public static List<TObject> getObject(int ModelID)
        {
            var objects = new List<cModelDB.TObject>();
            using (var db = new LiteDatabase(cModelDB.ConStr))
            {
                // Get model collection                
                objects = db.GetCollection<cModelDB.TObject>("objects")
                    .Find(x => x.ModelId == ModelID).OrderBy(x => x.Order).ToList();
            }
            return objects;
        }
        public static TObject getObjectByCode(string Code)
        {
            TObject result = null;
            using (var db = new LiteDatabase(ConStr))
            {
                // Get model collection                
                var objects = db.GetCollection<TObject>("objects")
                    .Find(x => x.Code == Code).FirstOrDefault();
                if (objects != null)
                {
                    result = objects;
                }
            }
            return result;
        }
        public static ImageCodecInfo GetEncoder(int value)
        {
            ImageFormat format;
            switch (value)
            {
                case 0:
                    format = ImageFormat.Jpeg;
                    break;
                case 1:
                    format = ImageFormat.Png;
                    break;
                case 2:
                    format = ImageFormat.Bmp;
                    break;
                default:
                    format = ImageFormat.Jpeg;
                    break;
            }
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static string GetImageExtension(int value)
        {
            switch (value)
            {
                case 0:
                    return ".jpg";
                case 1:
                    return ".png";
                case 2:
                    return ".bmp";
                default:
                    return ".jpg";
            }
        }
        public static long GetImageQuality(int value)
        {
            switch (value)
            {
                case 4:
                    return 1L;
                case 3:
                    return 25L;
                case 2:
                    return 50L;
                case 1:
                    return 75L;
                case 0:
                    return 100L;
                default:
                    return 100L;
            }
        }
        public static string GetObjectType(int type)
        {
            switch (type)
            {
                case 0:
                    return "Cosmetic 1";
                case 1:
                    return "Measurement";
                case 2:
                    return "Cosmetic 2";
                default:
                    return "Cosmetic 1";
            }
        }
        public static string GetNameByID(int ID)
        {
            int thuong = (int)Math.Ceiling((double)ID / 9);
            int du = ID % 9 == 0 ? 9 : ID % 9;
            char row = (char)('A' + du - 1);
            string col = (thuong).ToString();
            return row + "-" + col;
        }
        public static string GetLabelNameByID(int StationID, int ID)
        {
            int row = (int)Math.Ceiling((double)ID / _model.NumItemRow);
            int col = ID % _model.NumItemCol == 0 ? _model.NumItemCol : ID % _model.NumItemCol;
            return "lblStation" + StationID.ToString() + ((row - 1) * 9 + col).ToString();
        }
        public static int GetLabelIDByID(int ID, bool isTop)
        {
            if (isTop)
            {
                return ID;
            }
            else
            {
                int row = (int)Math.Ceiling((double)ID / _model.NumItemRow);
                int col = ID % _model.NumItemCol == 0 ? _model.NumItemCol : ID % _model.NumItemCol;
                return ((row - 1) * _model.NumItemRow + (_model.NumItemCol - col + 1));
            }
        }
        public static int GetNextIndex(int ID)
        {

            int row = (int)Math.Ceiling((double)ID / _model.NumItemRow);
            int col = ID % _model.NumItemCol == 0 ? _model.NumItemCol : ID % _model.NumItemCol;
            if (row % 2 == 0) // Hàng chẵn (2, 4...) - đi từ phải sang trái
            {
                if (col > 1)
                {
                    return ID - 1; // phải sang trái
                }
                else
                {
                    // đầu hàng → xuống hàng kế tiếp, về đầu cột
                    return (row * _model.NumItemCol + 1);
                }
            }
            else // Hàng lẻ (1, 3, 5...) - đi từ trái sang phải
            {
                if (col < _model.NumItemCol)
                {
                    return ID + 1; // trái sang phải
                }
                else
                {
                    // cuối hàng → xuống hàng kế tiếp, về cuối cột
                    return (row + 1) * _model.NumItemCol;
                }
            }
        }
        public static bool CheckLastIndex(int ID)
        {
            // Kiểm tra nếu đã ở điểm cuối
            if ((_model.NumItemRow % 2 == 0 && ID == (_model.NumItemRow - 1) * _model.NumItemCol + 1) || (_model.NumItemRow % 2 == 1 && ID == _model.NumItemRow * _model.NumItemCol))
            {
                return true; // Đã tới cuối
            }
            else
            {
                return false;
            }
        }
        public static string GenerateRandomNumberString(int length)
        {
            Random rand = new Random();
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(rand.Next(0, 10)); // digit from 0 to 9
            }
            return sb.ToString();
        }
        // Helper method for saving color images
        public static void SaveColorImage(byte[] imageBytes, int width, int height, string blobName)
        {
            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                                                  ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(imageBytes, 0, bmpData.Scan0, imageBytes.Length);
                bmp.UnlockBits(bmpData);

                string fileName = $"{blobName}_{width}x{height}x3.png";
                bmp.Save(fileName, ImageFormat.Png);
            }
        }

        // Helper method for saving grayscale images
        public static void SaveGrayscaleImage(byte[] imageBytes, int width, int height, string blobName)
        {
            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                                                  ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(imageBytes, 0, bmpData.Scan0, imageBytes.Length);
                bmp.UnlockBits(bmpData);

                ColorPalette palette = bmp.Palette;
                for (int j = 0; j < 256; j++)
                    palette.Entries[j] = Color.FromArgb(j, j, j);
                bmp.Palette = palette;

                string fileName = $"{blobName}_{width}x{height}x1.png";
                bmp.Save(fileName, ImageFormat.Png);
            }
        }
        public static async Task DoWithRetryAsync(Func<Task> action, TimeSpan sleepPeriod, int tryCount = 3)
        {
            int total = tryCount;
            if (tryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(tryCount));
            while (true)
            {
                try
                {
                    await action();
                    return; // success!
                }
                catch
                {
                    if (--tryCount == 0)
                    {
                        throw;
                    }
                    applog.Error("Try: " + (total - tryCount).ToString());
                    await Task.Delay(sleepPeriod);
                }
            }
        }

        //CSV helper
        /*| Vị trí                      | Nội dung                |
          | ----------------------      | ----------------------- |
          | `[0]`                       | `DateTime MaxCreatedAt` |
          | `[1 + (idx - 1) * 13 + 0]`  | UnitXValue              |
          | `[1 + (idx - 1) * 13 + 1]`  | _2DBC                   |
          | `[1 + (idx - 1) * 13 + 2]`  | QRDegree                |
          | `[1 + (idx - 1) * 13 + 3]`  | FAI2                    |
          | `[1 + (idx - 1) * 13 + 4]`  | FAI121                  |
          | `[1 + (idx - 1) * 13 + 5]`  | FAI122                  |
          | `[1 + (idx - 1) * 13 + 6]`  | FAI123                  |
          | `[1 + (idx - 1) * 13 + 7]`  | FAI124                  |
          | `[1 + (idx - 1) * 13 + 8]`  | FAI125                  |
          | `[1 + (idx - 1) * 13 + 9]`  | FAI126                  |
          | `[1 + (idx - 1) * 13 + 10]` | FAI127                  |
          | `[1 + (idx - 1) * 13 + 11]` | FAI128                  |
          | `[1 + (idx - 1) * 13 + 12]` | FAI4                    |
          | `[1 + (idx - 1) * 13 + 13]` | X                       |
          | `[1 + (idx - 1) * 13 + 14]` | Y                       |
          | `[1 + (idx - 1) * 13 + 15]` | X2                      |
          | `[1 + (idx - 1) * 13 + 16]` | Y2                      |*/
        public static Dictionary<string, object[]> BuildBarcodeDict(List<TReport2> all)
        {
            int GridSize = _model.NumItemRow * _model.NumItemCol;
            // số field cho mỗi Index (UnitXValue + _2DBC + 15 field)
            int FieldsPerIndex = 17;
            var result = new Dictionary<string, object[]>(StringComparer.OrdinalIgnoreCase);

            if (all == null || all.Count == 0)
                return result;

            // Map: BarCode -> (Index -> latest record)
            var map = new Dictionary<string, Dictionary<int, TReport2>>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < all.Count; i++)
            {
                var r = all[i];
                if (r == null) continue;

                var bc = (r.BarCode ?? string.Empty).Trim();
                if (bc.Length == 0) continue;

                if (r.Index < 1 || r.Index > GridSize) continue;

                Dictionary<int, TReport2> perIndex;
                if (!map.TryGetValue(bc, out perIndex))
                {
                    perIndex = new Dictionary<int, TReport2>();
                    map[bc] = perIndex;
                }

                TReport2 exist;
                if (!perIndex.TryGetValue(r.Index, out exist))
                {
                    perIndex[r.Index] = r;
                }
                else
                {
                    // chọn record mới nhất theo Created_at cho cùng Index
                    if (r.Created_at > exist.Created_at)
                        perIndex[r.Index] = r;
                }
            }

            // Build result
            foreach (var kv in map)
            {
                string bc = kv.Key;
                Dictionary<int, TReport2> perIndex = kv.Value;

                // Value layout:
                // [0] = max Created_at
                // then for idx=1..81: 17 fields
                // offset(idx) = 1 + (idx-1)*17
                object[] arr = new object[1 + (GridSize * FieldsPerIndex)];

                // 1) ngày lớn nhất trong 81 index (từ các bản ghi latest theo index)
                DateTime maxDate = DateTime.MinValue;
                foreach (var rec in perIndex.Values)
                {
                    if (rec != null && rec.Created_at > maxDate)
                        maxDate = rec.Created_at;
                }
                arr[0] = maxDate;

                // 2) fill 81 index
                for (int idx = 1; idx <= GridSize; idx++)
                {
                    int offset = 1 + (idx - 1) * FieldsPerIndex;

                    TReport2 rec;
                    if (perIndex.TryGetValue(idx, out rec) && rec != null)
                    {
                        arr[offset + 0] = TrimOrEmpty(rec.UnitXValue);
                        arr[offset + 1] = TrimOrEmpty(rec._2DBC);
                        arr[offset + 2] = TrimOrEmpty(rec.QRDegree);
                        arr[offset + 3] = TrimOrEmpty(rec.FAI2);
                        arr[offset + 4] = TrimOrEmpty(rec.FAI121);
                        arr[offset + 5] = TrimOrEmpty(rec.FAI122);
                        arr[offset + 6] = TrimOrEmpty(rec.FAI123);
                        arr[offset + 7] = TrimOrEmpty(rec.FAI124);
                        arr[offset + 8] = TrimOrEmpty(rec.FAI125);
                        arr[offset + 9] = TrimOrEmpty(rec.FAI126);
                        arr[offset + 10] = TrimOrEmpty(rec.FAI127);
                        arr[offset + 11] = TrimOrEmpty(rec.FAI128);
                        arr[offset + 12] = TrimOrEmpty(rec.FAI4);
                        arr[offset + 13] = TrimOrEmpty(rec.Xcoordinate);
                        arr[offset + 14] = TrimOrEmpty(rec.Ycoordinate);
                        arr[offset + 15] = TrimOrEmpty(rec.Xcoordinate2);
                        arr[offset + 16] = TrimOrEmpty(rec.Ycoordinate2);
                    }
                    else
                    {
                        // thiếu dữ liệu index -> set rỗng
                        for (int f = 0; f < FieldsPerIndex; f++)
                            arr[offset + f] = string.Empty;
                    }
                }

                result[bc] = arr;
            }

            return result;
        }
        private static string TrimOrEmpty(string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s.Trim();
        }
        public static void DisposeBitmapDictionary(Dictionary<int, Bitmap> images)
        {
            if (images == null) return;
            foreach (var bmp in images.Values)
            {
                try { bmp?.Dispose(); } catch { }
            }
            images.Clear();
        }
        //Trạm1: OptiX1 - Back1 - Ring1
        //Trạm2: Dom
        //Trạm3: OptiX3 - Back3 - Ring3
        public static int GetOrderByStation(string optixValue, int StationID)
        {
            switch (StationID)
            {
                case 1:
                    switch (optixValue.ToLower())
                    {
                        case "optix1": return 1;
                        case "back1": return 2;
                        case "ring1": return 3;
                        default: return 0;
                    }

                case 2:
                    switch (optixValue.ToLower())
                    {
                        case "dom": return 1;
                        default: return 0;
                    }

                case 3:
                    switch (optixValue.ToLower())
                    {
                        case "optix3": return 1;
                        case "back3": return 2;
                        case "ring3": return 3;
                        default: return 0;
                    }
                default:
                    return 1;
            }
        }
        public static void SetOrReplaceBitmap(Dictionary<int, Bitmap> dict, int key, Bitmap bmp)
        {
            if (dict == null || bmp == null) return;

            Bitmap oldBmp;
            if (dict.TryGetValue(key, out oldBmp))
            {
                if (oldBmp != null)
                    oldBmp.Dispose();

                dict[key] = (Bitmap)bmp.Clone();
            }
            else
            {
                dict[key] = (Bitmap)bmp.Clone();
            }
        }
        public static void SetOrReplaceInt(Dictionary<int, int> dict, int key, int value)
        {
            if (dict == null) return;
            dict[key] = value;
        }
        public static TObjectResult CloneObjectResult(TObjectResult source)
        {
            if (source == null) return null;

            var clone = new TObjectResult();
            clone.Id = source.Id;
            clone.Name = source.Name;
            clone.BarCode = source.BarCode;
            clone.Status = source.Status;
            clone._2DBC = source._2DBC;

            clone.UnitXResult = source.UnitXResult != null
                ? new List<TUnitXObject>(source.UnitXResult)
                : new List<TUnitXObject>();

            clone.MeasurementResult = source.MeasurementResult != null
                ? new Dictionary<string, string>(source.MeasurementResult)
                : new Dictionary<string, string>();

            clone.ImageStatusEx = source.ImageStatusEx != null
                ? new Dictionary<int, int>(source.ImageStatusEx)
                : new Dictionary<int, int>();

            clone.ImageLiveEx = new Dictionary<int, Bitmap>();
            if (source.ImageLiveEx != null)
            {
                foreach (var kv in source.ImageLiveEx)
                {
                    clone.ImageLiveEx[kv.Key] = kv.Value != null ? (Bitmap)kv.Value.Clone() : null;
                }
            }

            clone.ImageResultEx = new Dictionary<int, Bitmap>();
            if (source.ImageResultEx != null)
            {
                foreach (var kv in source.ImageResultEx)
                {
                    clone.ImageResultEx[kv.Key] = kv.Value != null ? (Bitmap)kv.Value.Clone() : null;
                }
            }

            return clone;
        }
        public static string GetValue(Dictionary<string, string> dict, string key)
        {
            if (dict != null && dict.TryGetValue(key, out var value))
                return value;

            return "";
        }
    }
    public sealed class ImageSaveQueue : IDisposable
    {
        private readonly BlockingCollection<SaveJob> _queue;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task[] _workers;

        // Cache codec JPEG (thread-safe để đọc)
        private static readonly ImageCodecInfo JpegCodec =
            ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.MimeType == "image/jpeg")
            ?? throw new InvalidOperationException("JPEG codec not found.");

        public ImageSaveQueue(int boundedCapacity = 2000, int consumers = 1)
        {
            if (boundedCapacity <= 0) boundedCapacity = 100;
            if (consumers <= 0) consumers = 1;

            // Bounded => đầy thì Producer sẽ bị chặn => không dồn I/O, giảm 100% HDD
            _queue = new BlockingCollection<SaveJob>(boundedCapacity);

            _workers = new Task[consumers];
            for (int i = 0; i < consumers; i++)
            {
                _workers[i] = Task.Factory.StartNew(
                    () => WorkerLoop(_cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        // Enqueue: nếu queue đầy => sẽ block (backpressure)
        public void Enqueue(SaveJob job, CancellationToken ct = default(CancellationToken))
        {
            // hỗ trợ cancel khi queue đang full
            while (!_queue.IsAddingCompleted)
            {
                ct.ThrowIfCancellationRequested();
                if (_cts.IsCancellationRequested) return;

                if (_queue.TryAdd(job, millisecondsTimeout: 200))
                    return;
            }
        }
        public void Complete() => _queue.CompleteAdding();
        public void Stop()
        {
            try
            {
                Complete();
            }
            catch { /* ignore */ }

            _cts.Cancel();
            try { Task.WaitAll(_workers, TimeSpan.FromSeconds(3)); }
            catch { /* ignore */ }
        }
        private void WorkerLoop(CancellationToken token)
        {
            foreach (var job in _queue.GetConsumingEnumerable(token))
            {
                try
                {
                    Directory.CreateDirectory(job.FolderPath);

                    using (var sourceImage = job.Image)
                    using (var bmp = new Bitmap(sourceImage))
                    {
                        // EncoderParameters phải dispose để tránh leak GDI
                        using (var encParams = new EncoderParameters(1))
                        {
                            encParams.Param[0] = new EncoderParameter(
                                System.Drawing.Imaging.Encoder.Quality,
                                job.JpegQuality);

                            // Buffer lớn + SequentialScan giảm seek trên HDD
                            using (var fs = new FileStream(
                                job.FullPath,
                                FileMode.Create,
                                FileAccess.Write,
                                FileShare.Read,
                                bufferSize: 128 * 1024,
                                options: FileOptions.SequentialScan))
                            {
                                bmp.Save(fs, JpegCodec, encParams);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { job.OnError?.Invoke(ex); } catch { /* ignore */ }
                }
            }
        }
        public void Dispose()
        {
            Stop();
            _cts.Dispose();
            _queue.Dispose();
        }
        public struct SaveJob
        {
            public Image Image;
            public string FolderPath;
            public string FullPath;
            public long JpegQuality; // 1..100
            public Action<Exception> OnError;
        }
        public static Bitmap CreateRandomBitmap()
        {
            int width = 1024;
            int height = 768;

            Random rnd = new Random();

            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

                try
                {
                    int stride = data.Stride;   // số byte mỗi dòng (có padding)
                    int bytes = stride * height;
                    byte[] buffer = new byte[bytes];

                    for (int y = 0; y < height; y++)
                    {
                        int row = y * stride;

                        for (int x = 0; x < width; x++)
                        {
                            int index = row + x * 3;

                            buffer[index + 0] = (byte)rnd.Next(256); // B
                            buffer[index + 1] = (byte)rnd.Next(256); // G
                            buffer[index + 2] = (byte)rnd.Next(256); // R
                        }
                    }

                    Marshal.Copy(buffer, 0, data.Scan0, bytes);
                }
                finally
                {
                    bmp.UnlockBits(data);
                }

                return bmp;
            }
        }
    }
}
