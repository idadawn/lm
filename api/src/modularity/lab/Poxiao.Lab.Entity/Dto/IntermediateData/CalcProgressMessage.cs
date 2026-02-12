namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 计算进度消息（Worker → API，通过 lab.calc.progress 队列）。
/// </summary>
public class CalcProgressMessage
{
    /// <summary>
    /// 批次ID
    /// </summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// 触发用户ID（用于 WebSocket 推送）
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型：CALC=计算, JUDGE=判定
    /// </summary>
    public string TaskType { get; set; } = "CALC";

    /// <summary>
    /// 数据总条数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 已完成条数
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// 成功条数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败条数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 状态：PROCESSING / COMPLETED / FAILED
    /// </summary>
    public string Status { get; set; } = "PROCESSING";

    /// <summary>
    /// 进度消息描述
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
