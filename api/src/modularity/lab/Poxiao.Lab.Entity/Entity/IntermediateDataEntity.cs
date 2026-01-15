using Newtonsoft.Json;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
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
    /// 日期（yyyy-MM格式，可手动修改）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATE_MONTH", Length = 10)]
    public string DateMonth { get; set; }

    /// <summary>
    /// 生产日期（原始数据日期）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE")]
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 喷次（产线+班次+日期+炉号组合）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SPRAY_NO", Length = 100, IsNullable = true)]
    public string SprayNo { get; set; }

    /// <summary>
    /// 产线.
    /// </summary>
    [SugarColumn(ColumnName = "F_LINE_NO", Length = 10, IsNullable = true)]
    public string LineNo { get; set; }

    /// <summary>
    /// 班次.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT", Length = 10, IsNullable = true)]
    public string Shift { get; set; }

    /// <summary>
    /// 炉号（解析后的炉号数字部分）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_PARSED", Length = 50, IsNullable = true)]
    public string FurnaceNoParsed { get; set; }

    /// <summary>
    /// 原始炉号.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO", Length = 100)]
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 卷号.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_NO", Length = 50, IsNullable = true)]
    public string CoilNo { get; set; }

    /// <summary>
    /// 分卷号.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBCOIL_NO", Length = 50, IsNullable = true)]
    public string SubcoilNo { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", Length = 50, IsNullable = true)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产品规格名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_NAME", Length = 100, IsNullable = true)]
    public string ProductSpecName { get; set; }

    /// <summary>
    /// 产品规格版本号（记录生成中间数据时使用的版本）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_VERSION", IsNullable = true)]
    public int? ProductSpecVersion { get; set; }

    /// <summary>
    /// 一米带材重量(g)：原始数据带材重量/产品规格长度.
    /// </summary>
    [SugarColumn(ColumnName = "F_ONE_METER_WEIGHT", DecimalDigits = 2, IsNullable = true)]
    public decimal? OneMeterWeight { get; set; }

    /// <summary>
    /// 带宽(mm).
    /// </summary>
    [SugarColumn(ColumnName = "F_STRIP_WIDTH", DecimalDigits = 2, IsNullable = true)]
    public decimal? StripWidth { get; set; }

    /// <summary>
    /// 带厚范围（最小值～最大值）.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_RANGE", Length = 50, IsNullable = true)]
    public string ThicknessRange { get; set; }

    /// <summary>
    /// 带厚最小值.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_MIN", DecimalDigits = 2, IsNullable = true)]
    public decimal? ThicknessMin { get; set; }

    /// <summary>
    /// 带厚最大值.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_MAX", DecimalDigits = 2, IsNullable = true)]
    public decimal? ThicknessMax { get; set; }

    /// <summary>
    /// 带厚极差：最大值-最小值，一位小数.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_DIFF", DecimalDigits = 1, IsNullable = true)]
    public decimal? ThicknessDiff { get; set; }

    /// <summary>
    /// 平均厚度.
    /// </summary>
    [SugarColumn(ColumnName = "F_AVG_THICKNESS", DecimalDigits = 2, IsNullable = true)]
    public decimal? AvgThickness { get; set; }

    /// <summary>
    /// 密度 (g/cm3).
    /// </summary>
    [SugarColumn(ColumnName = "F_DENSITY", DecimalDigits = 2, IsNullable = true)]
    public decimal? Density { get; set; }

    /// <summary>
    /// 叠片系数.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_FACTOR", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationFactor { get; set; }

    /// <summary>
    /// 外观特性（原始特性汉字）.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEARANCE_FEATURE", Length = 200, IsNullable = true)]
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
    /// 断头数（个）.
    /// </summary>
    [SugarColumn(ColumnName = "F_BREAK_COUNT", IsNullable = true)]
    public int? BreakCount { get; set; }

    /// <summary>
    /// 单卷重量（kg）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_WEIGHT_KG", DecimalDigits = 2, IsNullable = true)]
    public decimal? CoilWeightKg { get; set; }

    /// <summary>
    /// 外观检验员ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEARANCE_INSPECTOR_ID", Length = 50, IsNullable = true)]
    public string AppearanceInspectorId { get; set; }

    /// <summary>
    /// 外观检验员姓名.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEARANCE_INSPECTOR_NAME", Length = 50, IsNullable = true)]
    public string AppearanceInspectorName { get; set; }

    /// <summary>
    /// 磁性能判定.
    /// </summary>
    [SugarColumn(ColumnName = "F_MAGNETIC_RESULT", Length = 50, IsNullable = true)]
    public string MagneticResult { get; set; }

    /// <summary>
    /// 厚度判定.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_RESULT", Length = 50, IsNullable = true)]
    public string ThicknessResult { get; set; }

    /// <summary>
    /// 叠片系数判定.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_RESULT", Length = 50, IsNullable = true)]
    public string LaminationResult { get; set; }

    /// <summary>
    /// 四米带材重量（g）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FOUR_METER_WEIGHT", DecimalDigits = 2, IsNullable = true)]
    public decimal? FourMeterWeight { get; set; }

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
    /// 带型（中间段相对两侧段的位置判断）.
    /// </summary>
    [SugarColumn(ColumnName = "F_STRIP_TYPE", DecimalDigits = 2, IsNullable = true)]
    public decimal? StripType { get; set; }

    /// <summary>
    /// 分段类型（前端/中端/后端）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SEGMENT_TYPE", Length = 20, IsNullable = true)]
    public string SegmentType { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_REMARK", Length = 500, IsNullable = true)]
    public string Remark { get; set; }

    #region 带厚分布（1米带厚，检测列/层数）

    /// <summary>
    /// 带厚1.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_1", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness1 { get; set; }

    /// <summary>
    /// 带厚2.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_2", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness2 { get; set; }

    /// <summary>
    /// 带厚3.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_3", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness3 { get; set; }

    /// <summary>
    /// 带厚4.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_4", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness4 { get; set; }

    /// <summary>
    /// 带厚5.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_5", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness5 { get; set; }

    /// <summary>
    /// 带厚6.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_6", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness6 { get; set; }

    /// <summary>
    /// 带厚7.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_7", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness7 { get; set; }

    /// <summary>
    /// 带厚8.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_8", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness8 { get; set; }

    /// <summary>
    /// 带厚9.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_9", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness9 { get; set; }

    /// <summary>
    /// 带厚10.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_10", DecimalDigits = 2, IsNullable = true)]
    public decimal? Thickness10 { get; set; }

    /// <summary>
    /// 带厚异常标记（JSON格式，标记哪些带厚需要红色标注）.
    /// </summary>
    [SugarColumn(ColumnName = "F_THICKNESS_ABNORMAL", Length = 200, IsNullable = true)]
    public string ThicknessAbnormal { get; set; }

    #endregion

    #region 叠片系数厚度分布（原始检测列）

    /// <summary>
    /// 叠片系数分布1.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_1", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist1 { get; set; }

    /// <summary>
    /// 叠片系数分布2.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_2", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist2 { get; set; }

    /// <summary>
    /// 叠片系数分布3.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_3", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist3 { get; set; }

    /// <summary>
    /// 叠片系数分布4.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_4", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist4 { get; set; }

    /// <summary>
    /// 叠片系数分布5.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_5", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist5 { get; set; }

    /// <summary>
    /// 叠片系数分布6.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_6", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist6 { get; set; }

    /// <summary>
    /// 叠片系数分布7.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_7", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist7 { get; set; }

    /// <summary>
    /// 叠片系数分布8.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_8", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist8 { get; set; }

    /// <summary>
    /// 叠片系数分布9.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_9", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist9 { get; set; }

    /// <summary>
    /// 叠片系数分布10.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAMINATION_DIST_10", DecimalDigits = 2, IsNullable = true)]
    public decimal? LaminationDist10 { get; set; }

    #endregion

    #region 性能数据（可编辑）

    /// <summary>
    /// 1.35T 50Hz Ss激磁功率.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_SS_POWER", DecimalDigits = 2, IsNullable = true)]
    public decimal? PerfSsPower { get; set; }

    /// <summary>
    /// 1.35T 50Hz Ps铁损.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_PS_LOSS", DecimalDigits = 2, IsNullable = true)]
    public decimal? PerfPsLoss { get; set; }

    /// <summary>
    /// 1.35T 50Hz Hc.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_HC", DecimalDigits = 2, IsNullable = true)]
    public decimal? PerfHc { get; set; }

    /// <summary>
    /// 刻痕后性能 Ss激磁功率.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_AFTER_SS_POWER", DecimalDigits = 2, IsNullable = true)]
    public decimal? PerfAfterSsPower { get; set; }

    /// <summary>
    /// 刻痕后性能 Ps铁损.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_AFTER_PS_LOSS", DecimalDigits = 2, IsNullable = true)]
    public decimal? PerfAfterPsLoss { get; set; }

    /// <summary>
    /// 刻痕后性能 Hc.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_AFTER_HC", DecimalDigits = 2, IsNullable = true)]
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
    /// 性能判定人.
    /// </summary>
    [SugarColumn(ColumnName = "F_PERF_JUDGE_NAME", Length = 50, IsNullable = true)]
    public string PerfJudgeName { get; set; }

    #endregion

    #region 外观特性（可编辑）

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
    /// 中Si.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_SI", Length = 50, IsNullable = true)]
    public string MidSi { get; set; }

    /// <summary>
    /// 中B.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_B", Length = 50, IsNullable = true)]
    public string MidB { get; set; }

    /// <summary>
    /// 左花纹.
    /// </summary>
    [SugarColumn(ColumnName = "F_LEFT_PATTERN", Length = 50, IsNullable = true)]
    public string LeftPattern { get; set; }

    /// <summary>
    /// 中花纹.
    /// </summary>
    [SugarColumn(ColumnName = "F_MID_PATTERN", Length = 50, IsNullable = true)]
    public string MidPattern { get; set; }

    /// <summary>
    /// 右花纹.
    /// </summary>
    [SugarColumn(ColumnName = "F_RIGHT_PATTERN", Length = 50, IsNullable = true)]
    public string RightPattern { get; set; }

    /// <summary>
    /// 外观特性编辑人ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDITOR_ID", Length = 50, IsNullable = true)]
    public string AppearEditorId { get; set; }

    /// <summary>
    /// 外观特性编辑人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDITOR_NAME", Length = 50, IsNullable = true)]
    public string AppearEditorName { get; set; }

    /// <summary>
    /// 外观特性编辑时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_EDIT_TIME", IsNullable = true)]
    public DateTime? AppearEditTime { get; set; }

    /// <summary>
    /// 外观检验员.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPEAR_JUDGE_NAME", Length = 50, IsNullable = true)]
    public string AppearJudgeName { get; set; }

    #endregion

    /// <summary>
    /// 检测列配置（从产品规格中获取）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DETECTION_COLUMNS", Length = 100, IsNullable = true)]
    public string DetectionColumns { get; set; }

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

    #endregion
}
