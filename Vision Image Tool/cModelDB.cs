using System;
using System.Runtime.InteropServices;

namespace VisionImageTool
{
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
