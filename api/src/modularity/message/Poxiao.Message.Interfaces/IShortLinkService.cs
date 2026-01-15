using Poxiao.Message.Entitys.Entity;

namespace Poxiao.Message.Interfaces.Message;

/// <summary>
/// 系统消息
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IShortLinkService
{
    Task<MessageShortLinkEntity> Create(string userId, string bodyText);

    string CreateToken(string userId);
}