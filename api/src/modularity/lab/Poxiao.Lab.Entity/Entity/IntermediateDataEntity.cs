using Newtonsoft.Json;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Attributes;
using Poxiao.Lab.Entity.Enums;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 中间数据表.
/// </summary>
[SugarTable("LAB_INTERMEDIATE_DATA")]
[Tenant(ClaimConst.TENANTID)]
public class IntermediateDataEntity : CLDEntityBase
{
    /// <summary>
    /// 原始数据ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_RAW_DATA_ID", Length = 50)]
    public string RawDataId { get; set; }

    /// <summary>
    /// 检测日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_DATE", Length = 10)]
    public DateTime? DetectionDate { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE")]
    public DateTime? ProdDate { get; set; }

    #region 基础信息

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
    #endregion

    /// <summary>
    /// 喷次（8位日期-炉号）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SPRAY_NO", Length = 50, IsNullable = true)]
    public string SprayNo { get; set; }

    /// <summary>
    /// 贴标.
    /// </summary>
    [IntermediateDataColumn("贴标", sort: 1, dataType: "string", description: "贴标")]
    [SugarColumn(ColumnName = "F_LABELING", Length = 50, IsNullable = true)]
    public string Labeling { get; set; }

    /// <summary>
    /// 炉号.
    /// （格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_FORMATTED", Length = 200, IsNullable = true)]
    public string FurnaceNoFormatted { get; set; }

    /// <summary>
    /// 班次.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT_NO", Length = 20, IsNullable = true)]
    public string ShiftNo { get; set; }

    #region 性能数据（可编辑）

    /// <summary>
    /// 1.35T 50Hz Ss激磁功率 (VA/kg).
    /// </summary>
    [IntermediateDataColumn(
        "Ss激磁功率",
        sort: 2,
        dataType: "decimal",
        description: "1.35T 50Hz Ss激磁功率"
    )]
    [SugarColumn(ColumnName = "F_PERF_SS_POWER", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfSsPower { get; set; }

    /// <summary>
    /// 1.35T 50Hz Ps铁损 (W/kg).
    /// </summary>
    [IntermediateDataColumn(
        "Ps铁损",
        sort: 3,
        dataType: "decimal",
        description: "1.35T 50Hz Ps铁损"
    )]
    [SugarColumn(ColumnName = "F_PERF_PS_LOSS", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfPsLoss { get; set; }

    /// <summary>
    /// 1.35T 50Hz Hc (A/m).
    /// </summary>
    [IntermediateDataColumn("Hc", sort: 4, dataType: "decimal", description: "1.35T 50Hz Hc")]
    [SugarColumn(ColumnName = "F_PERF_HC", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfHc { get; set; }

    /// <summary>
    /// 刻痕后性能 Ss激磁功率 (VA/kg).
    /// </summary>
    [IntermediateDataColumn(
        "刻痕后Ss激磁功率",
        sort: 5,
        dataType: "decimal",
        description: "刻痕后性能 Ss激磁功率"
    )]
    [SugarColumn(ColumnName = "F_AFTER_SS_POWER", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfAfterSsPower { get; set; }

    /// <summary>
    /// 刻痕后性能 Ps铁损 (W/kg).
    /// </summary>
    [IntermediateDataColumn(
        "刻痕后Ps铁损",
        sort: 6,
        dataType: "decimal",
        description: "刻痕后性能 Ps铁损"
    )]
    [SugarColumn(ColumnName = "F_AFTER_PS_LOSS", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfAfterPsLoss { get; set; }

    /// <summary>
    /// 刻痕后性能 Hc (A/m).
    /// </summary>
    [IntermediateDataColumn("刻痕后Hc", sort: 7, dataType: "decimal", description: "刻痕后性能 Hc")]
    [SugarColumn(ColumnName = "F_AFTER_HC", DecimalDigits = 6, IsNullable = true)]
    public decimal? PerfAfterHc { get; set; }

    /// <summary>
    /// 是否刻痕（0-否，1-是，标识是否有刻痕数据）.
    /// </summary>
    [IntermediateDataColumn(
        "是否刻痕",
        sort: 7,
        dataType: "int",
        description: "是否刻痕 0-否，1-是"
    )]
    [SugarColumn(ColumnName = "F_IS_SCRATCHED", IsNullable = true)]
    public int? IsScratched { get; set; }

    /// <summary>
    /// 性能数据编辑人ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_EDITOR_ID", Length = 50, IsNullable = true)]
    public string PerfEditorId { get; set; }

    /// <summary>
    /// 性能数据编辑人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_EDITOR_NAME", Length = 50, IsNullable = true)]
    public string PerfEditorName { get; set; }

    /// <summary>
    /// 性能数据编辑时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_EDIT_TIME", IsNullable = true)]
    public DateTime? PerfEditTime { get; set; }

    /// <summary>
    /// 性能录入员，自动获取当前用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_EDITOR", Length = 50, IsNullable = true)]
    public string PerfEditor { get; set; }

    #endregion

    /// <summary>
    /// 一米带材重量(g)：F_FOUR_METER_WT / STD_LENGTH.
    /// </summary>
    [IntermediateDataColumn(
        "一米带材重量",
        sort: 8,
        dataType: "decimal",
        description: "一米带材重量"
    )]
    [SugarColumn(ColumnName = "F_ONE_METER_WT", DecimalDigits = 6, IsNullable = true)]
    public decimal? OneMeterWeight { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    [IntermediateDataColumn("带宽", sort: 40, dataType: "decimal", description: "带宽")]
    [SugarColumn(ColumnName = "F_WIDTH", DecimalDigits = 6, IsNullable = true)]
    public decimal? Width { get; set; }

    #region 带厚分布

    /// <summary>
    /// 带厚1 (μm)，前端动态计算列：F_LAM_DIST_i / LAYERS.
    /// </summary>
    [IntermediateDataColumn("带厚1", sort: 101, dataType: "decimal", description: "带厚1")]
    [SugarColumn(ColumnName = "F_THICK_1", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness1 { get; set; }

    /// <summary>
    /// 带厚2.
    /// </summary>
    [IntermediateDataColumn("带厚2", sort: 102, dataType: "decimal", description: "带厚2")]
    [SugarColumn(ColumnName = "F_THICK_2", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness2 { get; set; }

    /// <summary>
    /// 带厚3.
    /// </summary>
    [IntermediateDataColumn("带厚3", sort: 103, dataType: "decimal", description: "带厚3")]
    [SugarColumn(ColumnName = "F_THICK_3", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness3 { get; set; }

    /// <summary>
    /// 带厚4.
    /// </summary>
    [IntermediateDataColumn("带厚4", sort: 104, dataType: "decimal", description: "带厚4")]
    [SugarColumn(ColumnName = "F_THICK_4", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness4 { get; set; }

    /// <summary>
    /// 带厚5.
    /// </summary>
    [IntermediateDataColumn("带厚5", sort: 105, dataType: "decimal", description: "带厚5")]
    [SugarColumn(ColumnName = "F_THICK_5", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness5 { get; set; }

    /// <summary>
    /// 带厚6.
    /// </summary>
    [IntermediateDataColumn("带厚6", sort: 106, dataType: "decimal", description: "带厚6")]
    [SugarColumn(ColumnName = "F_THICK_6", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness6 { get; set; }

    /// <summary>
    /// 带厚7.
    /// </summary>
    [IntermediateDataColumn("带厚7", sort: 107, dataType: "decimal", description: "带厚7")]
    [SugarColumn(ColumnName = "F_THICK_7", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness7 { get; set; }

    /// <summary>
    /// 带厚8.
    /// </summary>
    [IntermediateDataColumn("带厚8", sort: 108, dataType: "decimal", description: "带厚8")]
    [SugarColumn(ColumnName = "F_THICK_8", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness8 { get; set; }

    /// <summary>
    /// 带厚9.
    /// </summary>
    [IntermediateDataColumn("带厚9", sort: 109, dataType: "decimal", description: "带厚9")]
    [SugarColumn(ColumnName = "F_THICK_9", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness9 { get; set; }

    /// <summary>
    /// 带厚10.
    /// </summary>
    [IntermediateDataColumn("带厚10", sort: 110, dataType: "decimal", description: "带厚10")]
    [SugarColumn(ColumnName = "F_THICK_10", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness10 { get; set; }

    /// <summary>
    /// 带厚11.
    /// </summary>
    [IntermediateDataColumn("带厚11", sort: 111, dataType: "decimal", description: "带厚11")]
    [SugarColumn(ColumnName = "F_THICK_11", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness11 { get; set; }

    /// <summary>
    /// 带厚12.
    /// </summary>
    [IntermediateDataColumn("带厚12", sort: 112, dataType: "decimal", description: "带厚12")]
    [SugarColumn(ColumnName = "F_THICK_12", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness12 { get; set; }

    /// <summary>
    /// 带厚13.
    /// </summary>
    [IntermediateDataColumn("带厚13", sort: 113, dataType: "decimal", description: "带厚13")]
    [SugarColumn(ColumnName = "F_THICK_13", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness13 { get; set; }

    /// <summary>
    /// 带厚14.
    /// </summary>
    [IntermediateDataColumn("带厚14", sort: 114, dataType: "decimal", description: "带厚14")]
    [SugarColumn(ColumnName = "F_THICK_14", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness14 { get; set; }

    /// <summary>
    /// 带厚15.
    /// </summary>
    [IntermediateDataColumn("带厚15", sort: 115, dataType: "decimal", description: "带厚15")]
    [SugarColumn(ColumnName = "F_THICK_15", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness15 { get; set; }

    /// <summary>
    /// 带厚16.
    /// </summary>
    [IntermediateDataColumn("带厚16", sort: 116, dataType: "decimal", description: "带厚16")]
    [SugarColumn(ColumnName = "F_THICK_16", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness16 { get; set; }

    /// <summary>
    /// 带厚17.
    /// </summary>
    [IntermediateDataColumn("带厚17", sort: 117, dataType: "decimal", description: "带厚17")]
    [SugarColumn(ColumnName = "F_THICK_17", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness17 { get; set; }

    /// <summary>
    /// 带厚18.
    /// </summary>
    [IntermediateDataColumn("带厚18", sort: 118, dataType: "decimal", description: "带厚18")]
    [SugarColumn(ColumnName = "F_THICK_18", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness18 { get; set; }

    /// <summary>
    /// 带厚19.
    /// </summary>
    [IntermediateDataColumn("带厚19", sort: 119, dataType: "decimal", description: "带厚19")]
    [SugarColumn(ColumnName = "F_THICK_19", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness19 { get; set; }

    /// <summary>
    /// 带厚20.
    /// </summary>
    [IntermediateDataColumn("带厚20", sort: 120, dataType: "decimal", description: "带厚20")]
    [SugarColumn(ColumnName = "F_THICK_20", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness20 { get; set; }

    /// <summary>
    /// 带厚21.
    /// </summary>
    [IntermediateDataColumn("带厚21", sort: 121, dataType: "decimal", description: "带厚21")]
    [SugarColumn(ColumnName = "F_THICK_21", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness21 { get; set; }

    /// <summary>
    /// 带厚22.
    /// </summary>
    [IntermediateDataColumn("带厚22", sort: 122, dataType: "decimal", description: "带厚22")]
    [SugarColumn(ColumnName = "F_THICK_22", DecimalDigits = 6, IsNullable = true)]
    public decimal? Thickness22 { get; set; }

    /// <summary>
    /// 带厚异常标记（JSON格式，标记哪些带厚需要红色标注）.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_ABNORMAL", Length = 200, IsNullable = true)]
    public string ThicknessAbnormal { get; set; }

    #endregion

    /// <summary>
    /// 带厚范围（最小值～最大值）.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_RANGE", Length = 50, IsNullable = true)]
    public string ThicknessRange { get; set; }

    /// <summary>
    /// 带厚最小值.
    /// </summary>
    [IntermediateDataColumn("带厚最小值", sort: 32, dataType: "decimal", description: "带厚最小值")]
    [SugarColumn(ColumnName = "F_THICK_MIN", DecimalDigits = 6, IsNullable = true)]
    public decimal? ThicknessMin { get; set; }

    /// <summary>
    /// 带厚最大值.
    /// </summary>
    [IntermediateDataColumn("带厚最大值", sort: 33, dataType: "decimal", description: "带厚最大值")]
    [SugarColumn(ColumnName = "F_THICK_MAX", DecimalDigits = 6, IsNullable = true)]
    public decimal? ThicknessMax { get; set; }

    /// <summary>
    /// 带厚极差：最大值-最小值，一位小数.
    /// </summary>
    [IntermediateDataColumn("带厚极差", sort: 34, dataType: "decimal", description: "带厚极差")]
    [SugarColumn(ColumnName = "F_THICK_DIFF", DecimalDigits = 6, IsNullable = true)]
    public decimal? ThicknessDiff { get; set; }

    /// <summary>
    /// 产品长度（来自产品规格，导入时写入）.
    /// </summary>
    [IntermediateDataColumn("产品长度", sort: 34_1, dataType: "decimal", description: "产品长度")]
    [SugarColumn(ColumnName = "F_PRODUCT_LENGTH", DecimalDigits = 6, IsNullable = true)]
    public decimal? ProductLength { get; set; }

    /// <summary>
    /// 产品层数（来自产品规格，导入时写入）.
    /// </summary>
    [IntermediateDataColumn("产品层数", sort: 34_2, dataType: "int", description: "产品层数")]
    [SugarColumn(ColumnName = "F_PRODUCT_LAYERS", IsNullable = true)]
    public int? ProductLayers { get; set; }

    /// <summary>
    /// 产品密度（来自产品规格，导入时写入）.
    /// </summary>
    [IntermediateDataColumn("产品密度", sort: 34_3, dataType: "decimal", description: "产品密度")]
    [SugarColumn(ColumnName = "F_PRODUCT_DENSITY", DecimalDigits = 6, IsNullable = true)]
    public decimal? ProductDensity { get; set; }

    /// <summary>
    /// 密度 (g/cm³)：(F_ONE_M_WT * 1000) / (F_WIDTH * F_AVG_THICK).
    /// </summary>
    [IntermediateDataColumn("密度", sort: 35, dataType: "decimal", description: "密度")]
    [SugarColumn(ColumnName = "F_DENSITY", DecimalDigits = 6, IsNullable = true)]
    public decimal? Density { get; set; }

    /// <summary>
    /// 叠片系数 (%)：F_FOUR_M_WT / (F_WIDTH * 400 * F_AVG_THICK * THEO_DENSITY * 10^-7).
    /// </summary>
    [IntermediateDataColumn("叠片系数", sort: 36, dataType: "decimal", description: "叠片系数")]
    [SugarColumn(ColumnName = "F_LAM_FACTOR", DecimalDigits = 6, IsNullable = true)]
    public decimal? LaminationFactor { get; set; }

    #region 外观特性（可编辑）

    /// <summary>
    /// 特性描述（从炉号解析）.
    /// </summary>
    [IntermediateDataColumn("所有特性", sort: 36, dataType: "string", description: "所有特性")]
    [SugarColumn(ColumnName = "F_FEATURE_SUFFIX", Length = 50, IsNullable = true)]
    public string FeatureSuffix { get; set; }

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
    [IntermediateDataColumn("特性大类", sort: 36_1, dataType: "string", description: "特性大类ID列表")]
    [SugarColumn(
        ColumnName = "F_APPEARANCE_FEATURE_CATEGORY_IDS",
        ColumnDataType = "json",
        IsNullable = true
    )]
    public string AppearanceFeatureCategoryIds { get; set; }

    /// <summary>
    /// 匹配后的特性等级ID列表（JSON格式，数组：["level-id-1", ...]）.
    /// </summary>
    [IntermediateDataColumn("特性等级", sort: 36_2, dataType: "string", description: "特性等级ID列表")]
    [SugarColumn(
        ColumnName = "F_APPEARANCE_FEATURE_LEVEL_IDS",
        ColumnDataType = "json",
        IsNullable = true
    )]
    public string AppearanceFeatureLevelIds { get; set; }

    /// <summary>
    /// 断头数(个).
    /// </summary>
    [IntermediateDataColumn("断头数", sort: 36, dataType: "int", description: "断头数")]
    [SugarColumn(ColumnName = "F_BREAK_COUNT", IsNullable = true)]
    public int? BreakCount { get; set; }

    /// <summary>
    /// 单卷重量(kg).
    /// </summary>
    [IntermediateDataColumn("单卷重量", sort: 36, dataType: "decimal", description: "单卷重量")]
    [SugarColumn(ColumnName = "F_SINGLE_COIL_WEIGHT", DecimalDigits = 6, IsNullable = true)]
    public decimal? SingleCoilWeight { get; set; }

    /// <summary>
    /// 中Si (左)，中Si含量左侧检测值.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_SI_LEFT", Length = 50, IsNullable = true)]
    public string MidSiLeft { get; set; }

    /// <summary>
    /// 中Si (右)，中Si含量右侧检测值.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_SI_RIGHT", Length = 50, IsNullable = true)]
    public string MidSiRight { get; set; }

    /// <summary>
    /// 中B (左)，中B含量左侧检测值.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_B_LEFT", Length = 50, IsNullable = true)]
    public string MidBLeft { get; set; }

    /// <summary>
    /// 中B (右)，中B含量右侧检测值.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_B_RIGHT", Length = 50, IsNullable = true)]
    public string MidBRight { get; set; }

    /// <summary>
    /// 左花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_L_PATTERN_W", DecimalDigits = 6, IsNullable = true)]
    public decimal? LeftPatternWidth { get; set; }

    /// <summary>
    /// 左花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_L_PATTERN_S", DecimalDigits = 6, IsNullable = true)]
    public decimal? LeftPatternSpacing { get; set; }

    /// <summary>
    /// 中花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_M_PATTERN_W", DecimalDigits = 6, IsNullable = true)]
    public decimal? MidPatternWidth { get; set; }

    /// <summary>
    /// 中花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_M_PATTERN_S", DecimalDigits = 6, IsNullable = true)]
    public decimal? MidPatternSpacing { get; set; }

    /// <summary>
    /// 右花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_R_PATTERN_W", DecimalDigits = 6, IsNullable = true)]
    public decimal? RightPatternWidth { get; set; }

    /// <summary>
    /// 右花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_R_PATTERN_S", DecimalDigits = 6, IsNullable = true)]
    public decimal? RightPatternSpacing { get; set; }

    /// <summary>
    /// 外观检验员编辑人ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDITOR_ID", Length = 50, IsNullable = true)]
    public string AppearEditorId { get; set; }

    /// <summary>
    /// 外观检验员编辑人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDITOR_NAME", Length = 50, IsNullable = true)]
    public string AppearEditorName { get; set; }

    /// <summary>
    /// 外观检验员编辑时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDIT_TIME", IsNullable = true)]
    public DateTime? AppearEditTime { get; set; }

    #endregion

    /// <summary>
    /// 平均厚度 (μm)：AVG(F_LAM_DIST_1..22) / LAYERS.
    /// </summary>
    [IntermediateDataColumn("平均厚度", sort: 37, dataType: "decimal", description: "平均厚度")]
    [SugarColumn(ColumnName = "F_AVG_THICKNESS", DecimalDigits = 6, IsNullable = true)]
    public decimal? AvgThickness { get; set; }

    /// <summary>
    /// 磁性能判定，根据 Ps 铁损值逻辑判断.
    /// </summary>
    [IntermediateDataColumn("磁性能判定", sort: 38, dataType: "string", description: "磁性能判定")]
    [SugarColumn(ColumnName = "F_MAGNETIC_RES", Length = 50, IsNullable = true)]
    public string MagneticResult { get; set; }

    /// <summary>
    /// 厚度判定，平均厚度达标判定.
    /// </summary>
    [IntermediateDataColumn("厚度判定", sort: 39, dataType: "string", description: "厚度判定")]
    [SugarColumn(ColumnName = "F_THICK_RES", Length = 50, IsNullable = true)]
    public string ThicknessResult { get; set; }

    /// <summary>
    /// 叠片系数判定，叠片系数达标判定.
    /// </summary>
    [IntermediateDataColumn(
        "叠片系数判定",
        sort: 40,
        dataType: "string",
        description: "叠片系数判定"
    )]
    [SugarColumn(ColumnName = "F_LAM_FACTOR_RES", Length = 50, IsNullable = true)]
    public string LaminationResult { get; set; }

    /// <summary>
    /// 四米带材重量.
    /// </summary>
    [IntermediateDataColumn(
        "四米带材重量",
        sort: 40,
        dataType: "decimal",
        description: "四米带材重量"
    )]
    [SugarColumn(ColumnName = "F_COIL_WEIGHT", DecimalDigits = 6, IsNullable = true)]
    public decimal? CoilWeight { get; set; }

    #region 叠片系数厚度分布（原始检测列）

    /// <summary>
    /// 检测数据列1-22（固定22列）.
    /// </summary>
    [IntermediateDataColumn(
        "检测数据列1",
        sort: 201,
        dataType: "decimal",
        description: "检测数据列1"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_1", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection1 { get; set; }

    [IntermediateDataColumn(
        "检测数据列2",
        sort: 202,
        dataType: "decimal",
        description: "检测数据列2"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_2", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection2 { get; set; }

    [IntermediateDataColumn(
        "检测数据列3",
        sort: 203,
        dataType: "decimal",
        description: "检测数据列3"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_3", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection3 { get; set; }

    [IntermediateDataColumn(
        "检测数据列4",
        sort: 204,
        dataType: "decimal",
        description: "检测数据列4"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_4", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection4 { get; set; }

    [IntermediateDataColumn(
        "检测数据列5",
        sort: 205,
        dataType: "decimal",
        description: "检测数据列5"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_5", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection5 { get; set; }

    [IntermediateDataColumn(
        "检测数据列6",
        sort: 206,
        dataType: "decimal",
        description: "检测数据列6"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_6", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection6 { get; set; }

    [IntermediateDataColumn(
        "检测数据列7",
        sort: 207,
        dataType: "decimal",
        description: "检测数据列7"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_7", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection7 { get; set; }

    [IntermediateDataColumn(
        "检测数据列8",
        sort: 208,
        dataType: "decimal",
        description: "检测数据列8"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_8", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection8 { get; set; }

    [IntermediateDataColumn(
        "检测数据列9",
        sort: 209,
        dataType: "decimal",
        description: "检测数据列9"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_9", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection9 { get; set; }

    [IntermediateDataColumn(
        "检测数据列10",
        sort: 210,
        dataType: "decimal",
        description: "检测数据列10"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_10", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection10 { get; set; }

    [IntermediateDataColumn(
        "检测数据列11",
        sort: 211,
        dataType: "decimal",
        description: "检测数据列11"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_11", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection11 { get; set; }

    [IntermediateDataColumn(
        "检测数据列12",
        sort: 212,
        dataType: "decimal",
        description: "检测数据列12"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_12", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection12 { get; set; }

    [IntermediateDataColumn(
        "检测数据列13",
        sort: 213,
        dataType: "decimal",
        description: "检测数据列13"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_13", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection13 { get; set; }

    [IntermediateDataColumn(
        "检测数据列14",
        sort: 214,
        dataType: "decimal",
        description: "检测数据列14"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_14", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection14 { get; set; }

    [IntermediateDataColumn(
        "检测数据列15",
        sort: 215,
        dataType: "decimal",
        description: "检测数据列15"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_15", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection15 { get; set; }

    [IntermediateDataColumn(
        "检测数据列16",
        sort: 216,
        dataType: "decimal",
        description: "检测数据列16"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_16", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection16 { get; set; }

    [IntermediateDataColumn(
        "检测数据列17",
        sort: 217,
        dataType: "decimal",
        description: "检测数据列17"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_17", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection17 { get; set; }

    [IntermediateDataColumn(
        "检测数据列18",
        sort: 218,
        dataType: "decimal",
        description: "检测数据列18"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_18", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection18 { get; set; }

    [IntermediateDataColumn(
        "检测数据列19",
        sort: 219,
        dataType: "decimal",
        description: "检测数据列19"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_19", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection19 { get; set; }

    [IntermediateDataColumn(
        "检测数据列20",
        sort: 220,
        dataType: "decimal",
        description: "检测数据列20"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_20", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection20 { get; set; }

    [IntermediateDataColumn(
        "检测数据列21",
        sort: 221,
        dataType: "decimal",
        description: "检测数据列21"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_21", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection21 { get; set; }

    [IntermediateDataColumn(
        "检测数据列22",
        sort: 222,
        dataType: "decimal",
        description: "检测数据列22"
    )]
    [SugarColumn(ColumnName = "F_DETECTION_22", DecimalDigits = 6, IsNullable = true)]
    public decimal? Detection22 { get; set; }

    #endregion

    /// <summary>
    /// 最大厚度（检测列最大值）.
    /// </summary>
    [IntermediateDataColumn("最大厚度", sort: 42, dataType: "decimal", description: "最大厚度")]
    [SugarColumn(ColumnName = "F_MAX_THICKNESS_RAW", DecimalDigits = 6, IsNullable = true)]
    public decimal? MaxThicknessRaw { get; set; }

    /// <summary>
    /// 最大平均厚度（最大值/层数）.
    /// </summary>
    [IntermediateDataColumn(
        "最大平均厚度",
        sort: 43,
        dataType: "decimal",
        description: "最大平均厚度"
    )]
    [SugarColumn(ColumnName = "F_MAX_AVG_THICKNESS", DecimalDigits = 6, IsNullable = true)]
    public decimal? MaxAvgThickness { get; set; }

    /// <summary>
    /// 带型，依据中段与两侧均值差计算.
    /// </summary>
    [IntermediateDataColumn("带型", sort: 44, dataType: "decimal", description: "带型")]
    [SugarColumn(ColumnName = "F_STRIP_TYPE", DecimalDigits = 6, IsNullable = true)]
    public decimal? StripType { get; set; }

    /// <summary>
    /// 一次交检.
    /// </summary>
    [IntermediateDataColumn("一次交检", sort: 45, dataType: "string", description: "一次交检")]
    [SugarColumn(ColumnName = "F_FIRST_INSPECTION", Length = 50, IsNullable = true)]
    public string FirstInspection { get; set; }

    /// <summary>
    /// 是否需要人工确认（特性匹配置信度 < 90% 时需要人工确认）.
    /// </summary>
    [SugarColumn(ColumnName = "F_REQUIRES_MANUAL_CONFIRM", IsNullable = true)]
    public bool? RequiresManualConfirm { get; set; }

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
    /// 检测列.
    /// </summary>
    [IntermediateDataColumn("检测列", sort: 41, dataType: "int", description: "检测列")]
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", IsNullable = true)]
    public int? DetectionColumns { get; set; }

    /// <summary>
    /// 特性匹配置信度（0-1）.
    /// </summary>
    [SugarColumn(ColumnName = "F_MATCH_CONFIDENCE", IsNullable = true)]
    public double? MatchConfidence { get; set; }

    #region 公式计算相关字段

    /// <summary>
    /// 批次ID（关联导入会话或导入日志）.
    /// </summary>
    [SugarColumn(ColumnName = "F_BATCH_ID", Length = 50, IsNullable = true)]
    public string BatchId { get; set; }

    /// <summary>
    /// 公式计算状态：PENDING-待计算，PROCESSING-计算中，SUCCESS-计算成功，FAILED-计算失败.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_CALC_STATUS",
        Length = 20,
        DefaultValue = "PENDING",
        IsNullable = true
    )]
    public IntermediateDataCalcStatus CalcStatus { get; set; } = IntermediateDataCalcStatus.PENDING;

    /// <summary>
    /// 计算状态更新时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CALC_STATUS_TIME", IsNullable = true)]
    public DateTime? CalcStatusTime { get; set; }

    /// <summary>
    /// 计算错误摘要（简要错误信息，用于前端展示）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CALC_ERROR_MESSAGE", Length = 500, IsNullable = true)]
    public string CalcErrorMessage { get; set; }

    /// <summary>
    /// 判定状态：0-未判定(PENDING), 1-判定中(PROCESSING), 2-成功(SUCCESS), 3-失败(FAILED).
    /// </summary>
    [SugarColumn(ColumnName = "F_JUDGE_STATUS")]
    public IntermediateDataCalcStatus JudgeStatus { get; set; } = IntermediateDataCalcStatus.PENDING;

    /// <summary>
    /// 判定状态更新时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_JUDGE_STATUS_TIME", IsNullable = true)]
    public DateTime? JudgeStatusTime { get; set; }

    /// <summary>
    /// 判定错误摘要（简要错误信息，用于前端展示）.
    /// </summary>
    [SugarColumn(ColumnName = "F_JUDGE_ERROR_MESSAGE", Length = 500, IsNullable = true)]
    public string JudgeErrorMessage { get; set; }

    #endregion

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

    /// <summary>
    /// 带厚(范围虚拟列).
    /// </summary>
    [IntermediateDataColumn(
        "带厚",
        sort: 201,
        IsRange = true,
        RangePrefix = "Thickness",
        RangeStart = 1,
        RangeEnd = "$DetectionColumns"
    )]
    [SugarColumn(IsIgnore = true)]
    public decimal Thickness { get; set; }

    /// <summary>
    /// 检测数据列(范围虚拟列).
    /// </summary>
    [IntermediateDataColumn(
        "检测数据列",
        sort: 202,
        IsRange = true,
        RangePrefix = "Detection",
        RangeStart = 1,
        RangeEnd = "$DetectionColumns"
    )]
    [SugarColumn(IsIgnore = true)]
    public decimal Detection { get; set; }

    #endregion
}
