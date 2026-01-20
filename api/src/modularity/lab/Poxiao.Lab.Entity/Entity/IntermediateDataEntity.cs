using Newtonsoft.Json;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Attributes;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

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
    public string DetectionDate { get; set; }

    /// <summary>
    /// 炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）.
    /// 从原始炉号解析后重新构建，不包含特性描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_FORMATTED", Length = 200, IsNullable = true)]
    public string FurnaceNoFormatted { get; set; }

    /// <summary>
    /// 原始炉号（去掉特性汉字后的炉号）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO", Length = 100, IsNullable = true)]
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 生产日期（原始数据日期）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE")]
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 日期月份（yyyy-MM格式，可手动修改）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATE_MONTH", Length = 10, IsNullable = true)]
    public string DateMonth { get; set; }

    /// <summary>
    /// 喷次（8位日期-炉号）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SPRAY_NO", Length = 50, IsNullable = true)]
    public string SprayNo { get; set; }

    /// <summary>
    /// 班次号（产线+班次+日期+炉号组合）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT_NO", Length = 100, IsNullable = true)]
    public string ShiftNo { get; set; }

    /// <summary>
    /// 批次号（产线数字+班次汉字+8位日期-炉号）.
    /// </summary>
    [SugarColumn(ColumnName = "F_BATCH_NO", Length = 100, IsNullable = true)]
    public string BatchNo { get; set; }

    /// <summary>
    /// 炉号（解析后的炉号数字部分）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_PARSED", Length = 50, IsNullable = true)]
    public string FurnaceNoParsed { get; set; }

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

    #region 性能数据（可编辑）

    /// <summary>
    /// 1.35T 50Hz Ss激磁功率 (VA/kg).
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_SS_POWER", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfSsPower { get; set; }

    /// <summary>
    /// 1.35T 50Hz Ps铁损 (W/kg).
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_PS_LOSS", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfPsLoss { get; set; }

    /// <summary>
    /// 1.35T 50Hz Hc (A/m).
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_HC", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfHc { get; set; }

    /// <summary>
    /// 刻痕后性能 Ss激磁功率 (VA/kg).
    /// </summary>
    [SugarColumn(ColumnName = "F_AFTER_SS_POWER", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfAfterSsPower { get; set; }

    /// <summary>
    /// 刻痕后性能 Ps铁损 (W/kg).
    /// </summary>
    [SugarColumn(ColumnName = "F_AFTER_PS_LOSS", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfAfterPsLoss { get; set; }

    /// <summary>
    /// 刻痕后性能 Hc (A/m).
    /// </summary>
    [SugarColumn(ColumnName = "F_AFTER_HC", DecimalDigits = 4, IsNullable = true)]
    public decimal? PerfAfterHc { get; set; }

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
    [SugarColumn(ColumnName = "F_ONE_METER_WT", DecimalDigits = 2, IsNullable = true)]
    public decimal? OneMeterWeight { get; set; }

    /// <summary>
    /// 四米带材重量（g），原始样段称重数据.
    /// </summary>
    [SugarColumn(ColumnName = "F_FOUR_METER_WT", DecimalDigits = 1, IsNullable = true)]
    public decimal? FourMeterWeight { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    [ExcelImportColumn("宽度", Sort = 3)]
    [SugarColumn(ColumnName = "F_WIDTH", IsNullable = true)]
    public decimal? Width { get; set; }

    /// <summary>
    /// 带宽 (mm).
    /// </summary>
    [SugarColumn(ColumnName = "F_STRIP_WIDTH", DecimalDigits = 2, IsNullable = true)]
    public decimal? StripWidth { get; set; }

    #region 带厚分布

    /// <summary>
    /// 带厚1 (μm)，前端动态计算列：F_LAM_DIST_i / LAYERS.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_1", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness1 { get; set; }

    /// <summary>
    /// 带厚2.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_2", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness2 { get; set; }

    /// <summary>
    /// 带厚3.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_3", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness3 { get; set; }

    /// <summary>
    /// 带厚4.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_4", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness4 { get; set; }

    /// <summary>
    /// 带厚5.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_5", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness5 { get; set; }

    /// <summary>
    /// 带厚6.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_6", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness6 { get; set; }

    /// <summary>
    /// 带厚7.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_7", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness7 { get; set; }

    /// <summary>
    /// 带厚8.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_8", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness8 { get; set; }

    /// <summary>
    /// 带厚9.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_9", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness9 { get; set; }

    /// <summary>
    /// 带厚10.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_10", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness10 { get; set; }

    /// <summary>
    /// 带厚11.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_11", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness11 { get; set; }

    /// <summary>
    /// 带厚12.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_12", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness12 { get; set; }

    /// <summary>
    /// 带厚13.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_13", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness13 { get; set; }

    /// <summary>
    /// 带厚14.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_14", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness14 { get; set; }

    /// <summary>
    /// 带厚15.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_15", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness15 { get; set; }

    /// <summary>
    /// 带厚16.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_16", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness16 { get; set; }

    /// <summary>
    /// 带厚17.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_17", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness17 { get; set; }

    /// <summary>
    /// 带厚18.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_18", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness18 { get; set; }

    /// <summary>
    /// 带厚19.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_19", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness19 { get; set; }

    /// <summary>
    /// 带厚20.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_20", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness20 { get; set; }

    /// <summary>
    /// 带厚21.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_21", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness21 { get; set; }

    /// <summary>
    /// 带厚22.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_22", DecimalDigits = 2, IsNullable = true)]
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
    [SugarColumn(ColumnName = "F_THICK_MIN", DecimalDigits = 2, IsNullable = true)]
    public decimal? ThicknessMin { get; set; }

    /// <summary>
    /// 带厚最大值.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_MAX", DecimalDigits = 2, IsNullable = true)]
    public decimal? ThicknessMax { get; set; }

    /// <summary>
    /// 带厚极差：最大值-最小值，一位小数.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_DIFF", DecimalDigits = 1, IsNullable = true)]
    public decimal? ThicknessDiff { get; set; }

    /// <summary>
    /// 平均厚度 (μm)：AVG(F_LAM_DIST_1..22) / LAYERS.
    /// </summary>
    [SugarColumn(ColumnName = "F_AVG_THICKNESS", DecimalDigits = 2, IsNullable = true)]
    public decimal? AvgThickness { get; set; }

    /// <summary>
    /// 密度 (g/cm³)：(F_ONE_M_WT * 1000) / (F_WIDTH * F_AVG_THICK).
    /// </summary>
    [SugarColumn(ColumnName = "F_DENSITY", DecimalDigits = 2, IsNullable = true)]
    public decimal? Density { get; set; }

    /// <summary>
    /// 叠片系数 (%)：F_FOUR_M_WT / (F_WIDTH * 400 * F_AVG_THICK * THEO_DENSITY * 10^-7).
    /// </summary>
    [SugarColumn(ColumnName = "F_LAM_FACTOR", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationFactor { get; set; }

    #region 外观特性（可编辑）
    /// <summary>
    /// 特性描述（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FEATURE_SUFFIX", Length = 50, IsNullable = true)]
    public string FeatureSuffix { get; set; }

    /// <summary>
    /// 韧性.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOUGHNESS", Length = 50, IsNullable = true)]
    public string Toughness { get; set; }

    /// <summary>
    /// 鱼鳞纹.
    /// </summary>
    [SugarColumn(ColumnName = "F_FISH_SCALE", Length = 50, IsNullable = true)]
    public string FishScale { get; set; }

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
    [SugarColumn(ColumnName = "F_SINGLE_COIL_WEIGHT", IsNullable = true)]
    public decimal? SingleCoilWeight { get; set; }

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
    /// 中Si（简化属性，用于编辑）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string MidSi { get; set; }

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
    /// 中B（简化属性，用于编辑）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string MidB { get; set; }

    /// <summary>
    /// 左花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_L_PATTERN_W", DecimalDigits = 2, IsNullable = true)]
    public decimal? LeftPatternWidth { get; set; }

    /// <summary>
    /// 左花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_L_PATTERN_S", DecimalDigits = 2, IsNullable = true)]
    public decimal? LeftPatternSpacing { get; set; }

    /// <summary>
    /// 左花纹（简化属性，用于编辑）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string LeftPattern { get; set; }

    /// <summary>
    /// 中花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_M_PATTERN_W", DecimalDigits = 2, IsNullable = true)]
    public decimal? MidPatternWidth { get; set; }

    /// <summary>
    /// 中花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_M_PATTERN_S", DecimalDigits = 2, IsNullable = true)]
    public decimal? MidPatternSpacing { get; set; }

    /// <summary>
    /// 中花纹（简化属性，用于编辑）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string MidPattern { get; set; }

    /// <summary>
    /// 右花纹纹宽，实测宽度.
    /// </summary>
    [SugarColumn(ColumnName = "F_R_PATTERN_W", DecimalDigits = 2, IsNullable = true)]
    public decimal? RightPatternWidth { get; set; }

    /// <summary>
    /// 右花纹纹间距，实测间距.
    /// </summary>
    [SugarColumn(ColumnName = "F_R_PATTERN_S", DecimalDigits = 2, IsNullable = true)]
    public decimal? RightPatternSpacing { get; set; }

    /// <summary>
    /// 右花纹（简化属性，用于编辑）.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string RightPattern { get; set; }

    /// <summary>
    /// 外观特性（原始特性汉字），炉号后缀解析（如"脆"）.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_FEATURE", Length = 200, IsNullable = true)]
    public string AppearanceFeature { get; set; }

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
    /// 外观检验员，自动获取当前用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_JUDGE", Length = 50, IsNullable = true)]
    public string AppearJudgeName { get; set; }

    /// <summary>
    /// 性能判定人.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_JUDGE_NAME", Length = 50, IsNullable = true)]
    public string PerfJudgeName { get; set; }

    #endregion

    /// <summary>
    /// 磁性能判定，根据 Ps 铁损值逻辑判断.
    /// </summary>
    [SugarColumn(ColumnName = "F_MAGNETIC_RES", Length = 50, IsNullable = true)]
    public string MagneticResult { get; set; }

    /// <summary>
    /// 厚度判定，平均厚度达标判定.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICK_RES", Length = 50, IsNullable = true)]
    public string ThicknessResult { get; set; }

    /// <summary>
    /// 叠片系数判定，叠片系数达标判定.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAM_FACTOR_RES", Length = 50, IsNullable = true)]
    public string LaminationResult { get; set; }

    /// <summary>
    /// 带材重量.
    /// </summary>
    [ExcelImportColumn("带材重量", Sort = 4)]
    [SugarColumn(ColumnName = "F_COIL_WEIGHT", IsNullable = true)]
    public decimal? CoilWeight { get; set; }

    /// <summary>
    /// 单卷重量（kg）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_WEIGHT_KG", DecimalDigits = 2, IsNullable = true)]
    public decimal? CoilWeightKg { get; set; }

    #region 叠片系数厚度分布（原始检测列）

    /// <summary>
    /// 检测数据列1-22（固定22列）.
    /// </summary>
    [ExcelImportColumn("检测1", Sort = 7)]
    [SugarColumn(ColumnName = "F_DETECTION_1", IsNullable = true)]
    public decimal? Detection1 { get; set; }

    [ExcelImportColumn("检测2", Sort = 8)]
    [SugarColumn(ColumnName = "F_DETECTION_2", IsNullable = true)]
    public decimal? Detection2 { get; set; }

    [ExcelImportColumn("检测3", Sort = 9)]
    [SugarColumn(ColumnName = "F_DETECTION_3", IsNullable = true)]
    public decimal? Detection3 { get; set; }

    [ExcelImportColumn("检测4", Sort = 10)]
    [SugarColumn(ColumnName = "F_DETECTION_4", IsNullable = true)]
    public decimal? Detection4 { get; set; }

    [ExcelImportColumn("检测5", Sort = 11)]
    [SugarColumn(ColumnName = "F_DETECTION_5", IsNullable = true)]
    public decimal? Detection5 { get; set; }

    [ExcelImportColumn("检测6", Sort = 12)]
    [SugarColumn(ColumnName = "F_DETECTION_6", IsNullable = true)]
    public decimal? Detection6 { get; set; }

    [ExcelImportColumn("检测7", Sort = 13)]
    [SugarColumn(ColumnName = "F_DETECTION_7", IsNullable = true)]
    public decimal? Detection7 { get; set; }

    [ExcelImportColumn("检测8", Sort = 14)]
    [SugarColumn(ColumnName = "F_DETECTION_8", IsNullable = true)]
    public decimal? Detection8 { get; set; }

    [ExcelImportColumn("检测9", Sort = 15)]
    [SugarColumn(ColumnName = "F_DETECTION_9", IsNullable = true)]
    public decimal? Detection9 { get; set; }

    [ExcelImportColumn("检测10", Sort = 16)]
    [SugarColumn(ColumnName = "F_DETECTION_10", IsNullable = true)]
    public decimal? Detection10 { get; set; }

    [ExcelImportColumn("检测11", Sort = 17)]
    [SugarColumn(ColumnName = "F_DETECTION_11", IsNullable = true)]
    public decimal? Detection11 { get; set; }

    [ExcelImportColumn("检测12", Sort = 18)]
    [SugarColumn(ColumnName = "F_DETECTION_12", IsNullable = true)]
    public decimal? Detection12 { get; set; }

    [ExcelImportColumn("检测13", Sort = 19)]
    [SugarColumn(ColumnName = "F_DETECTION_13", IsNullable = true)]
    public decimal? Detection13 { get; set; }

    [ExcelImportColumn("检测14", Sort = 20)]
    [SugarColumn(ColumnName = "F_DETECTION_14", IsNullable = true)]
    public decimal? Detection14 { get; set; }

    [ExcelImportColumn("检测15", Sort = 21)]
    [SugarColumn(ColumnName = "F_DETECTION_15", IsNullable = true)]
    public decimal? Detection15 { get; set; }

    [ExcelImportColumn("检测16", Sort = 22)]
    [SugarColumn(ColumnName = "F_DETECTION_16", IsNullable = true)]
    public decimal? Detection16 { get; set; }

    [ExcelImportColumn("检测17", Sort = 23)]
    [SugarColumn(ColumnName = "F_DETECTION_17", IsNullable = true)]
    public decimal? Detection17 { get; set; }

    [ExcelImportColumn("检测18", Sort = 24)]
    [SugarColumn(ColumnName = "F_DETECTION_18", IsNullable = true)]
    public decimal? Detection18 { get; set; }

    [ExcelImportColumn("检测19", Sort = 25)]
    [SugarColumn(ColumnName = "F_DETECTION_19", IsNullable = true)]
    public decimal? Detection19 { get; set; }

    [ExcelImportColumn("检测20", Sort = 26)]
    [SugarColumn(ColumnName = "F_DETECTION_20", IsNullable = true)]
    public decimal? Detection20 { get; set; }

    [ExcelImportColumn("检测21", Sort = 27)]
    [SugarColumn(ColumnName = "F_DETECTION_21", IsNullable = true)]
    public decimal? Detection21 { get; set; }

    [ExcelImportColumn("检测22", Sort = 28)]
    [SugarColumn(ColumnName = "F_DETECTION_22", IsNullable = true)]
    public decimal? Detection22 { get; set; }

    #endregion

    /// <summary>
    /// 最大厚度（检测列最大值）.
    /// </summary>
    [SugarColumn(ColumnName = "F_MAX_THICKNESS_RAW", DecimalDigits = 2, IsNullable = true)]
    public decimal? MaxThicknessRaw { get; set; }

    /// <summary>
    /// 最大平均厚度（最大值/层数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_MAX_AVG_THICKNESS", DecimalDigits = 2, IsNullable = true)]
    public decimal? MaxAvgThickness { get; set; }

    /// <summary>
    /// 带型，依据中段与两侧均值差计算.
    /// </summary>
    [SugarColumn(ColumnName = "F_STRIP_TYPE", DecimalDigits = 2, IsNullable = true)]
    public decimal? StripType { get; set; }

    /// <summary>
    /// 分段类型（前端/中端/后端）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SEGMENT_TYPE", Length = 20, IsNullable = true)]
    public string SegmentType { get; set; }

    /// <summary>
    /// 一次交检.
    /// </summary>
    [SugarColumn(ColumnName = "F_FIRST_INSPECTION", Length = 50, IsNullable = true)]
    public string FirstInspection { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_REMARK", Length = 500, IsNullable = true)]
    public string Remark { get; set; }

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
    /// 检测列（从产品规格中获取）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", Length = 100, IsNullable = true)]
    public string DetectionColumns { get; set; }

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

    #endregion
}
