using Mapster;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Message.Entitys.Dto.IM;
using Poxiao.Message.Entitys.Dto.MessageAccount;
using Poxiao.Message.Entitys.Model.IM;

namespace Poxiao.Message.Entitys.Mapper;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<IMContentEntity, IMUnreadNumModel>()
            .Map(dest => dest.unreadNum, src => src.State);
        config.ForType<UserOnlineModel, OnlineUserListOutput>()
          .Map(dest => dest.userId, src => src.userId)
          .Map(dest => dest.userAccount, src => src.account)
          .Map(dest => dest.userName, src => src.userName)
          .Map(dest => dest.loginTime, src => src.lastTime)
          .Map(dest => dest.loginIPAddress, src => src.lastLoginIp)
          .Map(dest => dest.loginPlatForm, src => src.lastLoginPlatForm);
    }
}