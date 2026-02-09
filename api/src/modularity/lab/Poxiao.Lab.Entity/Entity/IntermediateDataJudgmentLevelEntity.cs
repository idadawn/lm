using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Enum;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 中间数据判定等级实体.
/// </summary>
[SugarTable("LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL")]
[Tenant(ClaimConst.TENANTID)]
public class IntermediateDataJudgmentLevelEntity : CLDEntityBase
{
    /// <summary>
    /// 判定公式ID (关联 IntermediateDataFormulaEntity).
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_ID", Length = 50, IsNullable = false)]
    public string FormulaId { get; set; }

    /// <summary>
    /// 判定公式名称 (冗余字段).
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_NAME", Length = 100, IsNullable = true)]
    public string FormulaName { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", Length = 50, IsNullable = true)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产品规格名称 (冗余字段).
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_NAME", Length = 100, IsNullable = true)]
    public string ProductSpecName { get; set; }

    /// <summary>
    /// 等级代码 (自动生成唯一值).
    /// </summary>
    [SugarColumn(ColumnName = "F_CODE", Length = 50, IsNullable = false)]
    public string Code { get; set; }

    /// <summary>
    /// 等级名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 质量状态 (如：合格、不合格、其他).
    /// </summary>
    [SugarColumn(ColumnName = "F_QUALITY_STATUS")]
    public QualityStatusEnum QualityStatus { get; set; }

    /// <summary>
    /// 判定权重 (优先级).
    /// </summary>
    [SugarColumn(ColumnName = "F_PRIORITY")]
    public int Priority { get; set; }

    /// <summary>
    /// 展示颜色.
    /// </summary>
    [SugarColumn(ColumnName = "F_COLOR", Length = 20, IsNullable = true)]
    public string Color { get; set; }

    /// <summary>
    /// 是否统计.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_STATISTIC")]
    public bool IsStatistic { get; set; } = true;

    /// <summary>
    /// 是否默认 (兜底判定).
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_DEFAULT")]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 业务说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", Length = 500, IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 判定条件 (JSON格式，存储条件公式).
    /// </summary>
    [SugarColumn(ColumnName = "F_CONDITION", Length = 4000, IsNullable = true)]
    public string Condition { get; set; }
}
