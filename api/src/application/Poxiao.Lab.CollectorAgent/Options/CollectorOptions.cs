namespace Poxiao.Lab.CollectorAgent.Options;

/// <summary>
/// 采集网关根配置（对应 Configurations 下 JSON 的 "Collector" 节点）。
/// </summary>
public class CollectorOptions
{
    /// <summary>
    /// 中心服务器基础地址，例如 http://192.168.1.34。
    /// </summary>
    public string ServerBaseUrl { get; set; } = "http://192.168.1.34";

    /// <summary>
    /// 应用标识（对应服务端 BASE_INTERFACEOAUTH.F_APPID）。
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 应用密钥（对应服务端 BASE_INTERFACEOAUTH.F_APPSECRET）。
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>
    /// 采集网关标识（用于服务端区分设备/上报来源）。
    /// </summary>
    public string CollectorId { get; set; } = string.Empty;

    /// <summary>
    /// 断网暂存目录（相对程序目录，或绝对路径）。
    /// </summary>
    public string SpoolDir { get; set; } = "spool";

    /// <summary>
    /// 各数据源采集位点持久化文件路径（相对程序目录，或绝对路径）。
    /// </summary>
    public string StateFile { get; set; } = "state/source-state.json";

    /// <summary>
    /// 心跳上报间隔（秒）。
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// 数据源列表。
    /// </summary>
    public List<SourceOptions> Sources { get; set; } = new();
}

/// <summary>
/// 单个设备数据源配置。
/// </summary>
public class SourceOptions
{
    /// <summary>
    /// 数据源名称（用于日志/状态文件/暂存文件命名，需在 Sources 中唯一）。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备编码，取值约定：stacking（叠片） / ring-sample（环样） / single-sheet（单片）。
    /// </summary>
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用该数据源。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 数据源类型：mock（仿真数据，用于联调）/ access（本机 Access 数据库）。
    /// </summary>
    public string Type { get; set; } = "mock";

    /// <summary>
    /// 轮询间隔（秒）。
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// 单次采集最大批量条数。
    /// </summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// Access 数据源专属配置（Type = access 时使用）。
    /// </summary>
    public AccessSourceOptions? Access { get; set; }

    /// <summary>
    /// Mock 数据源专属配置（Type = mock 时使用）。
    /// </summary>
    public MockSourceOptions? Mock { get; set; }
}

/// <summary>
/// Access（.mdb/.accdb）数据源配置。
/// </summary>
public class AccessSourceOptions
{
    /// <summary>
    /// Access 数据库文件路径。
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 数据库密码（无密码留空）。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 目标表名。
    /// </summary>
    public string Table { get; set; } = string.Empty;

    /// <summary>
    /// 增量位点列（自增主键或时间戳列），用于 "KeyColumn > lastPosition" 增量查询。
    /// </summary>
    public string KeyColumn { get; set; } = string.Empty;

    /// <summary>
    /// 完成标志列（可选）——用于过滤尚未测量完成的记录。
    /// </summary>
    public string? CompleteFlagColumn { get; set; }

    /// <summary>
    /// 完成标志列的目标值（可选，与 CompleteFlagColumn 搭配使用）。
    /// </summary>
    public string? CompleteFlagValue { get; set; }

    /// <summary>
    /// 需要采集的列（为空表示全列 SELECT *）。
    /// </summary>
    public List<string>? Columns { get; set; }
}

/// <summary>
/// Mock 仿真数据源配置。
/// </summary>
public class MockSourceOptions
{
    /// <summary>
    /// 每隔多少秒生成一条仿真记录。
    /// </summary>
    public int IntervalSeconds { get; set; } = 15;
}
