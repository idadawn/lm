using Poxiao.Lab.CollectorAgent.Options;

namespace Poxiao.Lab.CollectorAgent.Sources;

/// <summary>
/// 仿真数据源：按配置间隔生成叠片类仿真记录，用于联调全链路（无需真实设备数据库）。
/// 位点为自增序号的字符串表示，进程内单调递增，可长跑。
/// </summary>
public class MockDataSource : IDeviceDataSource
{
    private readonly SourceOptions _options;
    private readonly Random _random = new();
    private long _sequence;
    private DateTimeOffset _lastGeneratedAt = DateTimeOffset.MinValue;

    public MockDataSource(SourceOptions options)
    {
        _options = options;
        _sequence = 0;
    }

    public Task<List<CollectedRecord>> FetchNewAsync(string lastPosition, int batchSize, CancellationToken cancellationToken)
    {
        // 首次调用时，若 lastPosition 已有值（例如上次运行的状态文件被保留），从其之后继续。
        if (_sequence == 0 && long.TryParse(lastPosition, out var resume))
        {
            _sequence = resume;
        }

        var intervalSeconds = Math.Max(1, _options.Mock?.IntervalSeconds ?? 15);
        var now = DateTimeOffset.Now;
        if (_lastGeneratedAt != DateTimeOffset.MinValue && (now - _lastGeneratedAt).TotalSeconds < intervalSeconds)
        {
            return Task.FromResult(new List<CollectedRecord>());
        }

        var records = new List<CollectedRecord>();
        var count = Math.Min(batchSize, 1 + _random.Next(0, 3));
        for (var i = 0; i < count; i++)
        {
            _sequence++;
            _lastGeneratedAt = now;

            var furnaceNo = $"1甲{now:yyyyMMdd}-{1 + _random.Next(0, 6)}-{1 + _random.Next(0, 12)}-1";
            var width = Math.Round(1000 + (_random.NextDouble() * 200), 2);
            var coilWeight = Math.Round(3000 + (_random.NextDouble() * 1500), 2);

            var record = new CollectedRecord
            {
                SourceKey = _options.Name,
                CollectedAt = now,
                Position = _sequence.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["FurnaceNo"] = furnaceNo,
                    ["Width"] = width,
                    ["CoilWeight"] = coilWeight,
                    ["DeviceCode"] = _options.DeviceCode,
                    ["MeasuredAt"] = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["Sequence"] = _sequence,
                },
            };
            records.Add(record);
        }

        return Task.FromResult(records);
    }
}
