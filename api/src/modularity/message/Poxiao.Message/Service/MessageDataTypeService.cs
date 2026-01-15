using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Message.Entitys.Dto.MessageAccount;
using Poxiao.Message.Entitys.Entity;
using Poxiao.Systems.Entitys.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Message.Service;

/// <summary>
/// 消息账号
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "MessageDataType", Order = 240)]
[Route("api/message/[controller]")]
public class MessageDataTypeService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageDataTypeEntity> _repository;

    public MessageDataTypeService(ISqlSugarRepository<MessageDataTypeEntity> repository)
    {
        _repository = repository;
    }

    #region Get

    /// <summary>
    /// 下拉列表.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("getTypeList/{type}")]
    public async Task<dynamic> GetTypeList(string type)
    {
        return (await _repository.GetListAsync(x => x.Type == type && x.DeleteMark == null)).OrderBy(x => x.EnCode).Select(x => new { id = x.Id, fullName = x.FullName, enCode = x.EnCode }).ToList();
    }
    #endregion
}
