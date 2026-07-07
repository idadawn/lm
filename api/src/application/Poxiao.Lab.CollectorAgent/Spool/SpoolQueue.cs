using System.Text.Json;
using Microsoft.Extensions.Options;
using Poxiao.Lab.CollectorAgent.Options;
using Poxiao.Lab.CollectorAgent.Sources;

namespace Poxiao.Lab.CollectorAgent.Spool;

/// <summary>
/// 断网/上报失败时的本地批次暂存队列：每个批次落盘为一个 JSON 文件，
/// 文件名形如 {yyyyMMddHHmmssfff}_{source}.json，按文件名（即时间）天然有序。
/// </summary>
public class SpoolQueue
{
    private readonly string _spoolDir;

    public SpoolQueue(IOptions<CollectorOptions> options)
    {
        var configuredDir = options.Value.SpoolDir;
        _spoolDir = Path.IsPathRooted(configuredDir)
            ? configuredDir
            : Path.Combine(AppContext.BaseDirectory, configuredDir);
        Directory.CreateDirectory(_spoolDir);
    }

    /// <summary>
    /// 将一个批次落盘。
    /// </summary>
    public async Task<string> EnqueueAsync(string sourceName, string deviceCode, List<CollectedRecord> records, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_spoolDir);

        var batchId = Guid.NewGuid().ToString("N");
        var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{SanitizeFileNamePart(sourceName)}.json";
        var filePath = Path.Combine(_spoolDir, fileName);

        var batch = new SpoolBatch
        {
            BatchId = batchId,
            SourceName = sourceName,
            DeviceCode = deviceCode,
            CreatedAt = DateTimeOffset.Now,
            Records = records,
        };

        var tempPath = filePath + ".tmp";
        var json = JsonSerializer.Serialize(batch, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(tempPath, json, cancellationToken);
        File.Move(tempPath, filePath, overwrite: true);

        return filePath;
    }

    /// <summary>
    /// 获取最旧的一个待上报批次文件路径（按文件名排序，即最先生成的），没有则返回 null。
    /// </summary>
    public string? PeekOldest()
    {
        if (!Directory.Exists(_spoolDir))
        {
            return null;
        }

        return Directory.GetFiles(_spoolDir, "*.json")
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    /// <summary>
    /// 读取批次文件内容。
    /// </summary>
    public async Task<SpoolBatch?> ReadAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<SpoolBatch>(json);
    }

    /// <summary>
    /// 移除已成功上报的批次文件。
    /// </summary>
    public void Remove(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }
}

/// <summary>
/// 落盘的一个上报批次。
/// </summary>
public class SpoolBatch
{
    public string BatchId { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string DeviceCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public List<CollectedRecord> Records { get; set; } = new();
}
