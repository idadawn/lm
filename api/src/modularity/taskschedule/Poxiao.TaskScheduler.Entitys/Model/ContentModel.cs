using Poxiao.DependencyInjection;
using Poxiao.JsonSerialization;
using Newtonsoft.Json;

namespace Poxiao.TaskScheduler.Entitys.Model;

/// <summary>
/// .
/// </summary>
[SuppressSniffer]
public class ContentModel
{
    /// <summary>
    /// 表达式.
    /// </summary>
    public string cron { get; set; }

    /// <summary>
    /// id.
    /// </summary>
    public string interfaceId { get; set; }

    /// <summary>
    /// 接口名.
    /// </summary>
    public string interfaceName { get; set; }

    /// <summary>
    /// 请求参数.
    /// </summary>
    public List<InterfaceParameter> parameter { get; set; }

    /// <summary>
    /// 本地任务id.
    /// </summary>
    public string localHostTaskId { get; set; }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [JsonConverter(typeof(NewtonsoftDateTimeJsonConverter))]
    public DateTime? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [JsonConverter(typeof(NewtonsoftDateTimeJsonConverter))]
    public DateTime? endTime { get; set; }
}

[SuppressSniffer]
public class InterfaceParameter
{
    /// <summary>
    /// 字段.
    /// </summary>
    public string field { get; set; }

    /// <summary>
    /// 默认值.
    /// </summary>
    public string defaultValue { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    public string value { get; set; }
}