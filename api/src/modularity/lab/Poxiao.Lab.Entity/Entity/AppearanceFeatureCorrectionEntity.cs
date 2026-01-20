using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 外观特性人工修正记录.
/// </summary>
[SugarTable("LAB_APPEARANCE_FEATURE_CORRECTION")]
[Tenant(ClaimConst.TENANTID)]
public class AppearanceFeatureCorrectionEntity : CLDEntityBase
{
    /// <summary>
    /// 原始输入文本.
    /// </summary>
    [SugarColumn(ColumnName = "F_INPUT_TEXT", Length = 500, IsNullable = false)]
    public string InputText { get; set; }

    /// <summary>
    /// 自动匹配的特征ID（如果有）.
    /// </summary>
    [SugarColumn(ColumnName = "F_AUTO_MATCHED_FEATURE_ID", Length = 50, IsNullable = true)]
    public string AutoMatchedFeatureId { get; set; }

    /// <summary>
    /// 人工修正后的特征ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_CORRECTED_FEATURE_ID", Length = 50, IsNullable = true)]
    public string CorrectedFeatureId { get; set; }

    /// <summary>
    /// 匹配模式（auto/manual/create）.
    /// </summary>
    [SugarColumn(ColumnName = "F_MATCH_MODE", Length = 20, IsNullable = false)]
    public string MatchMode { get; set; }

    /// <summary>
    /// 使用场景（test/import）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SCENARIO", Length = 20, IsNullable = false)]
    public string Scenario { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_REMARK", Length = 500, IsNullable = true)]
    public string Remark { get; set; }

    /// <summary>
    /// 状态 (Pending/Confirmed/Ignored).
    /// </summary>
    [SugarColumn(ColumnName = "F_STATUS", Length = 20, IsNullable = true)]
    public string Status { get; set; }
}
