namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 计算任务消息（API → Worker，通过 lab.calc.task 队列）。
/// </summary>
public class CalcTaskMessage
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
    /// 触发用户ID（用于完成后推送通知）
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 导入会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 数据总条数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 单位精度字典（JSON 序列化后传递）
    /// </summary>
    public string UnitPrecisionsJson { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型：CALC=计算, JUDGE=判定, MAGNETIC_JUDGE=磁性导入+判定
    /// </summary>
    public string TaskType { get; set; } = "CALC";

    /// <summary>
    /// 单条中间数据ID（per-item 模式：每条数据一条MQ消息）。
    /// 设置此字段时，Worker 以 per-item 并发模式处理。
    /// </summary>
    public string? IntermediateDataId { get; set; }

    /// <summary>
    /// 要重新计算的数据ID列表（手动重算/判定时使用，batch 模式）
    /// </summary>
    public List<string>? IntermediateDataIds { get; set; }

    /// <summary>
    /// 磁性数据载荷 JSON（MAGNETIC_JUDGE 任务使用）。
    /// 反序列化为 <see cref="MagneticDataPayload"/>。
    /// </summary>
    public string? MagneticDataJson { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 磁性数据载荷（随 MAGNETIC_JUDGE 消息传递，用于局部更新中间数据表磁性字段）。
/// </summary>
public class MagneticDataPayload
{
    /// <summary>
    /// 炉号（FurnaceNoFormatted），用于在 Worker 端查找对应的中间数据记录。
    /// </summary>
    public string FurnaceNo { get; set; } = string.Empty;

    /// <summary>
    /// 原始炉号（用于错误报告）
    /// </summary>
    public string OriginalFurnaceNo { get; set; } = string.Empty;

    /// <summary>
    /// 是否刻痕
    /// </summary>
    public bool IsScratched { get; set; }

    /// <summary>
    /// Ss激磁功率
    /// </summary>
    public decimal? SsPower { get; set; }

    /// <summary>
    /// Ps铁损
    /// </summary>
    public decimal? PsLoss { get; set; }

    /// <summary>
    /// Hc矫顽力
    /// </summary>
    public decimal? Hc { get; set; }

    /// <summary>
    /// 检测时间
    /// </summary>
    public DateTime? DetectionTime { get; set; }

    /// <summary>
    /// 编辑人ID
    /// </summary>
    public string? EditorId { get; set; }

    /// <summary>
    /// 编辑人姓名
    /// </summary>
    public string? EditorName { get; set; }
}
