using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.LinqBuilder;
using Poxiao.Systems.Entitys.Dto.System.PrintLog;
using Poxiao.Systems.Entitys.Entity.System;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

namespace Poxiao.Systems.System;

/// <summary>
/// 打印模板日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "PrintLog", Order = 200)]
[Route("api/system/[controller]")]
public class PrintLogService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PrintLogEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="PrintDevService"/>类型的新实例.
    /// </summary>
    public PrintLogService(
        ISqlSugarRepository<PrintLogEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 列表(分页).
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetList(string id, [FromQuery] PrintLogQuery input)
    {
        var whereLambda = LinqExpression.And<PrintLogEntity>();
        whereLambda = whereLambda.And(x => x.PrintId == id);
        var start = new DateTime();
        var end = new DateTime();
        if (input.endTime != null && input.startTime != null)
        {
            start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(x => SqlFunc.Between(x.PrintTime, start, end));
        }
        if (!string.IsNullOrEmpty(input.Keyword))
            whereLambda = whereLambda.And(m => m.PrintTitle.Contains(input.Keyword));
        var list = await _repository.AsQueryable().Where(whereLambda).OrderBy(x => x.PrintTime, OrderByType.Desc)
            .Select(a => new PrintLogOutuut
            {
                id = a.Id,
                printId = a.PrintId,
                printMan = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.PrintMan).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                printNum = a.PrintNum,
                printTime = a.PrintTime,
                printTitle = a.PrintTitle
            })
            .ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<PrintLogOutuut>.SqlSugarPageResult(list);
    }
    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("save")]
    public async Task Delete([FromBody] PrintLogOutuut input)
    {
        var entity = input.Adapt<PrintLogEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.PrintMan = _userManager.UserId;
        entity.PrintTime = DateTime.Now;
        var isOk = await _repository.AsInsertable(entity).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }
    #endregion
}
