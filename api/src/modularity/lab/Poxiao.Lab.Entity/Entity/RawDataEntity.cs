using Newtonsoft.Json;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Attributes;
using SqlSugar;
using System.ComponentModel;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 原始数据.
/// </summary>
[SugarTable("LAB_RAW_DATA")]
[Tenant(ClaimConst.TENANTID)]
public class RawDataEntity : CLDEntityBase
{
    /// <summary>
    /// 生产日期（从原始炉号FurnaceNo中解析，格式：yyyyMMdd，如20251101）.
    /// 如果炉号解析失败，则使用检测日期DetectionDate作为后备.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE")]
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 检测日期（从Excel的"日期"列直接读取）.
    /// </summary>
    [ExcelImportColumn("检测日期", Sort = 1)]
    [SugarColumn(ColumnName = "F_DETECTION_DATE")]
    public DateTime? DetectionDate { get; set; }

    /// <summary>
    /// 原始炉号.
    /// </summary>
    [ExcelImportColumn("炉号", Sort = 2)]
    [SugarColumn(ColumnName = "F_FURNACE_NO", Length = 100)]
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）.
    /// 从原始炉号解析后重新构建，不包含特性描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_FORMATTED", Length = 200, IsNullable = true)]
    public string FurnaceNoFormatted { get; set; }

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
    /// 炉次号（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_BATCH_NO", IsNullable = true)]
    public int? FurnaceBatchNo { get; set; }

    /// <summary>
    /// 卷号（从炉号解析，支持小数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_NO", IsNullable = true)]
    public decimal? CoilNo { get; set; }

    /// <summary>
    /// 分卷号（从炉号解析，支持小数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBCOIL_NO", IsNullable = true)]
    public decimal? SubcoilNo { get; set; }

    /// <summary>
    /// 特性描述（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FEATURE_SUFFIX", Length = 50, IsNullable = true)]
    public string FeatureSuffix { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    [ExcelImportColumn("宽度", Sort = 3)]
    [SugarColumn(ColumnName = "F_WIDTH", DecimalDigits = 6, IsNullable = true)]
    public decimal? Width { get; set; }

    /// <summary>
    /// 带材重量.
    /// </summary>
    [ExcelImportColumn("带材重量", Sort = 4)]
    [SugarColumn(ColumnName = "F_COIL_WEIGHT", DecimalDigits = 6, IsNullable = true)]
    public decimal? CoilWeight { get; set; }

    /// <summary>
    /// 断头数(个).
    /// </summary>
    [ExcelImportColumn("断头数", Sort = 5)]
    [SugarColumn(ColumnName = "F_BREAK_COUNT", IsNullable = true)]
    public int? BreakCount { get; set; }

    /// <summary>
    /// 单卷重量(kg).
    /// </summary>
    [ExcelImportColumn("单卷重量", Sort = 6)]
    [SugarColumn(ColumnName = "F_SINGLE_COIL_WEIGHT", DecimalDigits = 6, IsNullable = true)]
    public decimal? SingleCoilWeight { get; set; }

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
    /// 检测列（从产品规格中获取 - 数量）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", IsNullable = true)]
    public int? DetectionColumns { get; set; }

    /// <summary>
    /// 检测数据列1-22（固定22列）.
    /// </summary>
    [ExcelImportColumn("检测1", Sort = 7)]
    [SugarColumn(ColumnName = "F_DETECTION_1", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection1 { get; set; }

    [ExcelImportColumn("检测2", Sort = 8)]
    [SugarColumn(ColumnName = "F_DETECTION_2", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection2 { get; set; }

    [ExcelImportColumn("检测3", Sort = 9)]
    [SugarColumn(ColumnName = "F_DETECTION_3", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection3 { get; set; }

    [ExcelImportColumn("检测4", Sort = 10)]
    [SugarColumn(ColumnName = "F_DETECTION_4", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection4 { get; set; }

    [ExcelImportColumn("检测5", Sort = 11)]
    [SugarColumn(ColumnName = "F_DETECTION_5", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection5 { get; set; }

    [ExcelImportColumn("检测6", Sort = 12)]
    [SugarColumn(ColumnName = "F_DETECTION_6", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection6 { get; set; }

    [ExcelImportColumn("检测7", Sort = 13)]
    [SugarColumn(ColumnName = "F_DETECTION_7", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection7 { get; set; }

    [ExcelImportColumn("检测8", Sort = 14)]
    [SugarColumn(ColumnName = "F_DETECTION_8", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection8 { get; set; }

    [ExcelImportColumn("检测9", Sort = 15)]
    [SugarColumn(ColumnName = "F_DETECTION_9", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection9 { get; set; }

    [ExcelImportColumn("检测10", Sort = 16)]
    [SugarColumn(ColumnName = "F_DETECTION_10", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection10 { get; set; }

    [ExcelImportColumn("检测11", Sort = 17)]
    [SugarColumn(ColumnName = "F_DETECTION_11", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection11 { get; set; }

    [ExcelImportColumn("检测12", Sort = 18)]
    [SugarColumn(ColumnName = "F_DETECTION_12", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection12 { get; set; }

    [ExcelImportColumn("检测13", Sort = 19)]
    [SugarColumn(ColumnName = "F_DETECTION_13", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection13 { get; set; }

    [ExcelImportColumn("检测14", Sort = 20)]
    [SugarColumn(ColumnName = "F_DETECTION_14", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection14 { get; set; }

    [ExcelImportColumn("检测15", Sort = 21)]
    [SugarColumn(ColumnName = "F_DETECTION_15", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection15 { get; set; }

    [ExcelImportColumn("检测16", Sort = 22)]
    [SugarColumn(ColumnName = "F_DETECTION_16", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection16 { get; set; }

    [ExcelImportColumn("检测17", Sort = 23)]
    [SugarColumn(ColumnName = "F_DETECTION_17", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection17 { get; set; }

    [ExcelImportColumn("检测18", Sort = 24)]
    [SugarColumn(ColumnName = "F_DETECTION_18", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection18 { get; set; }

    [ExcelImportColumn("检测19", Sort = 25)]
    [SugarColumn(ColumnName = "F_DETECTION_19", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection19 { get; set; }

    [ExcelImportColumn("检测20", Sort = 26)]
    [SugarColumn(ColumnName = "F_DETECTION_20", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection20 { get; set; }

    [ExcelImportColumn("检测21", Sort = 27)]
    [SugarColumn(ColumnName = "F_DETECTION_21", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection21 { get; set; }

    [ExcelImportColumn("检测22", Sort = 28)]
    [SugarColumn(ColumnName = "F_DETECTION_22", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection22 { get; set; }

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
    /// 匹配后的特性大类ID列表（JSON格式，数组：["category-id-1", ...]）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_APPEARANCE_FEATURE_CATEGORY_IDS",
        ColumnDataType = "json",
        IsNullable = true
    )]
    public string AppearanceFeatureCategoryIds { get; set; }

    /// <summary>
    /// 匹配后的特性等级ID列表（JSON格式，数组：["level-id-1", ...]）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_APPEARANCE_FEATURE_LEVEL_IDS",
        ColumnDataType = "json",
        IsNullable = true
    )]
    public string AppearanceFeatureLevelIds { get; set; }

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

    /// <summary>
    /// 特性大类ID列表（辅助属性，从JSON字段解析）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<string> AppearanceFeatureCategoryIdsList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AppearanceFeatureCategoryIds))
                return new List<string>();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(AppearanceFeatureCategoryIds)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            AppearanceFeatureCategoryIds =
                value == null || value.Count == 0 ? null : JsonConvert.SerializeObject(value);
        }
    }

    /// <summary>
    /// 特性等级ID列表（辅助属性，从JSON字段解析）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<string> AppearanceFeatureLevelIdsList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AppearanceFeatureLevelIds))
                return new List<string>();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(AppearanceFeatureLevelIds)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            AppearanceFeatureLevelIds =
                value == null || value.Count == 0 ? null : JsonConvert.SerializeObject(value);
        }
    }

    #endregion
}
