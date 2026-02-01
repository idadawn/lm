using Poxiao.Infrastructure.Filter;

namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 中间数据计算日志查询.
/// </summary>
public class IntermediateDataCalcLogQuery : PageInputBase
{
    /// <summary>
    /// 批次ID.
    /// </summary>
    public string BatchId { get; set; }

    /// <summary>
    /// 中间数据ID.
    /// </summary>
    public string IntermediateDataId { get; set; }

    /// <summary>
    /// 列名.
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 公式类型.
    /// </summary>
    public string FormulaType { get; set; }

    /// <summary>
    /// 错误类型.
    /// </summary>
    public string ErrorType { get; set; }
}

/// <summary>
/// 中间数据计算日志输出.
/// </summary>
public class IntermediateDataCalcLogOutput
{
    public string Id { get; set; }

    public string BatchId { get; set; }

    public string IntermediateDataId { get; set; }

    public string ColumnName { get; set; }

    public string FormulaName { get; set; }

    public string FormulaType { get; set; }

    public string ErrorType { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorDetail { get; set; }

    public DateTime? CreatorTime { get; set; }
}
