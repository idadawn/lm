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


/// <summary>
/// 中间数据计算/判定追踪输出.
/// </summary>
public class IntermediateDataExecutionTraceOutput
{
    public string IntermediateDataId { get; set; }

    public string FurnaceNo { get; set; }

    public int? CalcStatus { get; set; }

    public string CalcStatusText { get; set; }

    public string CalcErrorMessage { get; set; }

    public int? JudgeStatus { get; set; }

    public string JudgeStatusText { get; set; }

    public string JudgeErrorMessage { get; set; }

    public List<IntermediateDataCalcStepOutput> CalculationSteps { get; set; } = new();

    public List<IntermediateDataJudgeStepOutput> JudgmentSteps { get; set; } = new();
}

public class IntermediateDataCalcStepOutput
{
    public int Order { get; set; }

    public string FormulaName { get; set; }

    public string ColumnName { get; set; }

    public string DisplayName { get; set; }

    public string Formula { get; set; }

    public string UnitName { get; set; }

    public int? Precision { get; set; }

    public bool IsDefaultValue { get; set; }

    public bool Success { get; set; }

    public string RawResult { get; set; }

    public string ResultValue { get; set; }

    public string FailureReason { get; set; }

    public List<IntermediateDataTraceValueOutput> VariableValues { get; set; } = new();
}

public class IntermediateDataJudgeStepOutput
{
    public int Order { get; set; }

    public string FormulaName { get; set; }

    public string ColumnName { get; set; }

    public string DisplayName { get; set; }

    public string StepName { get; set; }

    public int? Priority { get; set; }

    public bool IsDefaultStep { get; set; }

    public bool Success { get; set; }

    public string ResultValue { get; set; }

    public string ConditionText { get; set; }

    public string FailureReason { get; set; }
}

public class IntermediateDataTraceValueOutput
{
    public string Name { get; set; }

    public string Value { get; set; }
}
