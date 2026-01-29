using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 中间数据公式计算异常明细日志.
/// </summary>
[SugarTable("LAB_INTERMEDIATE_DATA_CALC_LOG")]
[Tenant(ClaimConst.TENANTID)]
public class IntermediateDataFormulaCalcLogEntity : CLDEntityBase
{
    /// <summary>
    /// 批次ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_BATCH_ID", Length = 50, IsNullable = true)]
    public string BatchId { get; set; }

    /// <summary>
    /// 中间数据ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_INTERMEDIATE_DATA_ID", Length = 50, IsNullable = true)]
    public string IntermediateDataId { get; set; }

    /// <summary>
    /// 列名.
    /// </summary>
    [SugarColumn(ColumnName = "F_COLUMN_NAME", Length = 100, IsNullable = true)]
    public string ColumnName { get; set; }

    /// <summary>
    /// 公式名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_NAME", Length = 100, IsNullable = true)]
    public string FormulaName { get; set; }

    /// <summary>
    /// 公式类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_TYPE", Length = 20, IsNullable = true)]
    public string FormulaType { get; set; }

    /// <summary>
    /// 错误类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_ERROR_TYPE", Length = 50, IsNullable = true)]
    public string ErrorType { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    [SugarColumn(ColumnName = "F_ERROR_MESSAGE", Length = 500, IsNullable = true)]
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 错误详情.
    /// </summary>
    [SugarColumn(ColumnName = "F_ERROR_DETAIL", ColumnDataType = "TEXT", IsNullable = true)]
    public string ErrorDetail { get; set; }
}
