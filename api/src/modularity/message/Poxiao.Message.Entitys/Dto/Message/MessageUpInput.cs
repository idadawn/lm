using Poxiao.DependencyInjection;

namespace Poxiao.Message.Entitys.Dto.Message;

/// <summary>
/// 消息修改输入.
/// </summary>
[SuppressSniffer]
public class MessageUpInput : MessageCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}