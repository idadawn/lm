using Newtonsoft.Json;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// 原始数据.
/// </summary>
[SugarTable("LAB_RAW_DATA")]
[Tenant(ClaimConst.TENANTID)]
public class RawDataEntity : CLDEntityBase
{
    /// <summary>
    /// 生产日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE")]
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 原始炉号.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO", Length = 100)]
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 产线（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_LINE_NO", IsNullable = true)]
    public int? LineNo { get; set; }

    /// <summary>
    /// 班次（从炉号解析，存储原始汉字：甲、乙、丙）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT", Length = 10, IsNullable = true)]
    public string Shift { get; set; }

    /// <summary>
    /// 班次数字（用于排序：甲=1, 乙=2, 丙=3）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT_NUMERIC", IsNullable = true)]
    public int? ShiftNumeric { get; set; }

    /// <summary>
    /// 炉号（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_PARSED", IsNullable = true)]
    public int? FurnaceNoParsed { get; set; }

    /// <summary>
    /// 卷号（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_NO", IsNullable = true)]
    public int? CoilNo { get; set; }

    /// <summary>
    /// 分卷号（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBCOIL_NO", IsNullable = true)]
    public int? SubcoilNo { get; set; }

    /// <summary>
    /// 特性描述（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FEATURE_SUFFIX", Length = 50, IsNullable = true)]
    public string FeatureSuffix { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_WIDTH", IsNullable = true)]
    public decimal? Width { get; set; }

    /// <summary>
    /// 带材重量.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_WEIGHT", IsNullable = true)]
    public decimal? CoilWeight { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", Length = 50, IsNullable = true)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产品规格代码.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_CODE", Length = 50, IsNullable = true)]
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 产品规格名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_NAME", Length = 100, IsNullable = true)]
    public string ProductSpecName { get; set; }

    /// <summary>
    /// 产品规格版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_VERSION", Length = 50, IsNullable = true)]
    public string ProductSpecVersion { get; set; }

    /// <summary>
    /// 检测列（从产品规格中获取）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", Length = 100, IsNullable = true)]
    public string DetectionColumns { get; set; }

    /// <summary>
    /// 检测数据列（JSON格式，存储动态检测数据，格式：{"1": 值1, "2": 值2, ...}）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_DATA", ColumnDataType = "json", IsNullable = true)]
    public string DetectionData { get; set; }

    /// <summary>
    /// 有效数据标识（0-非有效数据，1-有效数据，符合炉号解析规则）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_VALID_DATA", IsNullable = true)]
    public int IsValidData { get; set; } = 0;

    /// <summary>
    /// Excel源文件ID（关联文件服务）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_FILE_ID", Length = 500, IsNullable = true)]
    public string SourceFileId { get; set; }

    /// <summary>
    /// 导入会话ID（关联导入会话）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_SESSION_ID", Length = 50, IsNullable = true)]
    public string ImportSessionId { get; set; }

    /// <summary>
    /// 匹配后的特性ID列表（JSON格式，数组：["feature-id-1", "feature-id-2", ...]）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_APPEARANCE_FEATURE_IDS",
        ColumnDataType = "json",
        IsNullable = true
    )]
    public string AppearanceFeatureIds { get; set; }

    /// <summary>
    /// 特性匹配置信度（0-1）.
    /// </summary>
    [SugarColumn(ColumnName = "F_MATCH_CONFIDENCE", IsNullable = true)]
    public double? MatchConfidence { get; set; }

    /// <summary>
    /// 导入错误信息.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_ERROR", Length = 500, IsNullable = true)]
    public string ImportError { get; set; }

    /// <summary>
    /// 导入状态（0-成功，1-失败）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_STATUS")]
    public int ImportStatus { get; set; } = 0;

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    #region 辅助属性（不存储到数据库）

    /// <summary>
    /// 检测数据值字典（辅助属性，从JSON字段解析）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public Dictionary<int, decimal?> DetectionValues
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DetectionData))
                return new Dictionary<int, decimal?>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<int, decimal?>>(DetectionData)
                    ?? new Dictionary<int, decimal?>();
            }
            catch
            {
                return new Dictionary<int, decimal?>();
            }
        }
        set
        {
            DetectionData =
                value == null || value.Count == 0 ? null : JsonConvert.SerializeObject(value);
        }
    }

    /// <summary>
    /// 特性ID列表（辅助属性，从JSON字段解析）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<string> AppearanceFeatureIdsList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AppearanceFeatureIds))
                return new List<string>();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(AppearanceFeatureIds)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            AppearanceFeatureIds =
                value == null || value.Count == 0 ? null : JsonConvert.SerializeObject(value);
        }
    }

    #endregion
}
