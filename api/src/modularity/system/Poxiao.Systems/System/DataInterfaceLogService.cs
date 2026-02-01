using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Systems.Entitys.Dto.DataInterfaceLog;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 数据接口日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataInterfaceLog", Order = 204)]
[Route("api/system/[controller]")]
public class DataInterfaceLogService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DataInterfaceLogEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="DataInterfaceLogService"/>类型的新实例.
    /// </summary>
    public DataInterfaceLogService(
        ISqlSugarRepository<DataInterfaceLogEntity> repository)
    {
        _repository = repository;
    }

    #region Get

    [HttpGet("{id}")]
    public async Task<dynamic> GetList(string id, [FromQuery] PageInputBase input)
    {
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceLogEntity, UserEntity>((a, b) =>
        new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
             .Where(a => a.InvokId == id)
             .WhereIF(input.Keyword.IsNotEmptyOrNull(), a => a.UserId.Contains(input.Keyword) || a.InvokIp.Contains(input.Keyword)).OrderBy(a => a.InvokTime)
            .Select((a, b) => new DataInterfaceLogListOutput
            {
                id = a.Id,
                invokDevice = a.InvokDevice,
                invokIp = a.InvokIp,
                userId = SqlFunc.MergeString(b.RealName, "/", b.Account),
                invokTime = a.InvokTime,
                invokType = a.InvokType,
                invokWasteTime = a.InvokWasteTime
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<DataInterfaceLogListOutput>.SqlSugarPageResult(list);
    }

    #endregion
}