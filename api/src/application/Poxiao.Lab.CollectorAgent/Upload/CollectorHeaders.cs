namespace Poxiao.Lab.CollectorAgent.Upload;

/// <summary>
/// 与中心服务器约定的鉴权/接口常量。
/// 头名以服务端实现为准，当前先按约定值集中定义，便于后续对齐修改。
/// </summary>
public static class CollectorHeaders
{
    /// <summary>
    /// 应用标识请求头。
    /// </summary>
    public const string AppIdHeader = "X-Collector-AppId";

    /// <summary>
    /// 应用密钥请求头。
    /// </summary>
    public const string AppSecretHeader = "X-Collector-Secret";

    /// <summary>
    /// 批量数据上报接口相对路径。
    /// </summary>
    public const string BatchUploadPath = "/api/lab/device-data/batch";

    /// <summary>
    /// 心跳上报接口相对路径。
    /// </summary>
    public const string HeartbeatPath = "/api/lab/device-data/heartbeat";
}
