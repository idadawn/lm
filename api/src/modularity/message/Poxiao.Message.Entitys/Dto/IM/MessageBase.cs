using System.Text.Json.Serialization;
using Poxiao.DependencyInjection;
using Poxiao.Message.Entitys.Enums;

namespace Poxiao.Message.Entitys.Dto.IM;

/// <summary>
/// 消息基类类.
/// </summary>
[SuppressSniffer]
public class MessageBase
{
    /// <summary>
    /// 方法.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageSendType method { get; set; }
}