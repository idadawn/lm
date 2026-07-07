using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poxiao.Lab.CollectorAgent.Options;

namespace Poxiao.Lab.CollectorAgent.State;

/// <summary>
/// 各数据源采集位点（lastPosition）的 JSON 文件持久化存储。
/// 写入采用"写临时文件 + File.Move 覆盖"的原子写方式，避免进程被中断时状态文件损坏。
/// </summary>
public class SourceStateStore
{
    private readonly string _stateFilePath;
    private readonly ILogger<SourceStateStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<string, string> _positions = new();
    private bool _loaded;

    public SourceStateStore(IOptions<CollectorOptions> options, ILogger<SourceStateStore> logger)
    {
        _logger = logger;
        var configuredPath = options.Value.StateFile;
        _stateFilePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath);
    }

    /// <summary>
    /// 获取指定数据源的上次采集位点，不存在时返回空字符串。
    /// </summary>
    public async Task<string> GetLastPositionAsync(string sourceName, CancellationToken cancellationToken)
    {
        await EnsureLoadedAsync(cancellationToken);
        return _positions.TryGetValue(sourceName, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// 更新指定数据源的采集位点并落盘。
    /// </summary>
    public async Task SetLastPositionAsync(string sourceName, string position, CancellationToken cancellationToken)
    {
        await EnsureLoadedAsync(cancellationToken);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _positions[sourceName] = position;
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_loaded)
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_loaded)
            {
                return;
            }

            if (File.Exists(_stateFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_stateFilePath, cancellationToken);
                    _positions = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "读取采集位点状态文件失败（{Path}），将视为空状态重新开始。", _stateFilePath);
                    _positions = new Dictionary<string, string>();
                }
            }

            _loaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = _stateFilePath + ".tmp";
        var json = JsonSerializer.Serialize(_positions, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(tempPath, json, cancellationToken);
        File.Move(tempPath, _stateFilePath, overwrite: true);
    }
}
