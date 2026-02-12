using Poxiao.Lab.Entity.Dto.IntermediateData;
using System.Collections.Concurrent;

namespace Poxiao.Lab.CalcWorker.Services;

/// <summary>
/// 批次进度追踪器 — 在 Worker 内存中追踪每个批次的并发计算进度。
/// 使用 ConcurrentDictionary + Interlocked 保证线程安全。
/// 当最后一条完成时，由 TryMarkAllCompleted 返回 true，触发公式计算。
/// </summary>
public sealed class BatchProgressTracker
{
    private readonly ConcurrentDictionary<string, BatchState> _batches = new();

    /// <summary>
    /// 获取或创建批次状态（首条消息到达时自动创建）。
    /// </summary>
    public BatchState GetOrCreate(CalcTaskMessage firstMessage)
    {
        return _batches.GetOrAdd(firstMessage.BatchId, _ => new BatchState
        {
            BatchId = firstMessage.BatchId,
            TenantId = firstMessage.TenantId,
            UserId = firstMessage.UserId,
            UnitPrecisionsJson = firstMessage.UnitPrecisionsJson,
            TaskType = firstMessage.TaskType,
            Total = firstMessage.TotalCount,
        });
    }

    /// <summary>
    /// 报告一条数据计算完成（成功或失败）。
    /// 返回当前已完成总数。
    /// </summary>
    public int ReportCompleted(string batchId, bool success)
    {
        if (!_batches.TryGetValue(batchId, out var state))
            return 0;

        if (success)
            Interlocked.Increment(ref state.Success);
        else
            Interlocked.Increment(ref state.Failed);

        return Interlocked.Increment(ref state.Completed);
    }

    /// <summary>
    /// 尝试标记批次所有基础计算已完成。
    /// 使用 CAS 保证只有一个线程返回 true（负责触发公式计算）。
    /// </summary>
    public bool TryMarkAllCompleted(string batchId)
    {
        if (!_batches.TryGetValue(batchId, out var state))
            return false;

        // 只有当 Completed == Total 时，才尝试标记
        if (Volatile.Read(ref state.Completed) < state.Total)
            return false;

        // CAS: 只有一个线程能从 0 → 1
        return Interlocked.CompareExchange(ref state.FormulaTriggered, 1, 0) == 0;
    }

    /// <summary>
    /// 获取批次当前快照（用于生成进度消息）。
    /// </summary>
    public BatchState? GetState(string batchId)
    {
        _batches.TryGetValue(batchId, out var state);
        return state;
    }

    /// <summary>
    /// 移除已完成的批次（释放内存）。
    /// </summary>
    public void Remove(string batchId)
    {
        _batches.TryRemove(batchId, out _);
    }
}

/// <summary>
/// 单个批次的进度状态（字段使用 volatile / Interlocked 操作）。
/// </summary>
public class BatchState
{
    public string BatchId = string.Empty;
    public string TenantId = string.Empty;
    public string UserId = string.Empty;
    public string UnitPrecisionsJson = string.Empty;
    public string TaskType = "CALC";
    public int Total;

    /// <summary>已完成计数（原子递增）</summary>
    public int Completed;

    /// <summary>成功计数（原子递增）</summary>
    public int Success;

    /// <summary>失败计数（原子递增）</summary>
    public int Failed;

    /// <summary>公式计算是否已触发（0=否, 1=是，CAS 保证唯一）</summary>
    public int FormulaTriggered;
}
