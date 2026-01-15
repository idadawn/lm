using Poxiao.Infrastructure.Dtos.Message;
using Poxiao.Message.Entitys;

namespace Poxiao.Message.Interfaces;

/// <summary>
/// 消息中心处理接口类.
/// </summary>
public interface IMessageManager
{
    Task SendDefaultMsg(List<string> toUserIds, MessageEntity messageEntity, List<MessageReceiveEntity> receiveEntityList);

    Task<string> SendDefinedMsg(MessageSendModel messageSendModel, Dictionary<string, object> bodyDic);

    MessageEntity GetMessageEntity(string enCode, Dictionary<string, string> paramDic, int type, int flowType = 1);

    List<MessageReceiveEntity> GetMessageReceiveList(List<string> toUserIds, MessageEntity messageEntity, Dictionary<string, object> bodyDic = null);

    Task<List<MessageSendModel>> GetMessageSendModels(string sendConfigId);

    Task ForcedOffline(string connectionId);
}
