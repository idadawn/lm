namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 公式计算结果.
/// </summary>
public class FormulaCalculationResult
{
    /// <summary>
    /// 总数量.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功数量.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 错误列表.
    /// </summary>
    public List<FormulaCalculationError> Errors { get; set; } = new();

    /// <summary>
    /// 消息.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// 公式计算错误.
/// </summary>
public class FormulaCalculationError
{
    /// <summary>
    /// 中间数据ID.
    /// </summary>
    public string IntermediateDataId { get; set; }

    /// <summary>
    /// 炉号.
    /// </summary>
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 错误详情.
    /// </summary>
    public string ErrorDetail { get; set; }
}
