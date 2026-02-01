using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.LinqBuilder;
using Poxiao.Logging.Attributes;
using Poxiao.Systems.Entitys.Dto.SysLog;
using Poxiao.Systems.Entitys.System;
using SqlSugar;
using System.Reflection;

namespace Poxiao.Systems;

/// <summary>
/// 系统日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Log", Order = 211)]
[Route("api/system/[controller]")]
public class SysLogService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SysLogEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="SysLogService"/>类型的新实例.
    /// </summary>
    public SysLogService(
        ISqlSugarRepository<SysLogEntity> repository)
    {
        _repository = repository;
    }

    #region GET

    /// <summary>
    /// 获取系统日志列表-登录日志（带分页）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="Type">分类.</param>
    /// <returns></returns>
    [HttpGet("{Type}")]
    public async Task<dynamic> GetList([FromQuery] LogListQuery input, int Type)
    {
        var whereLambda = LinqExpression.And<SysLogEntity>();
        whereLambda = whereLambda.And(x => x.Category == Type);
        var start = new DateTime();
        var end = new DateTime();
        if (input.endTime != null && input.startTime != null)
        {
            start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(x => SqlFunc.Between(x.CreatorTime, start, end));
        }

        // 关键字（用户、IP地址、功能名称）
        if (!string.IsNullOrEmpty(input.Keyword))
            whereLambda = whereLambda.And(m => m.UserName.Contains(input.Keyword) || m.IPAddress.Contains(input.Keyword) || m.ModuleName.Contains(input.Keyword));
        if (input.ipaddress.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.IPAddress.Contains(input.ipaddress));
        if (input.userName.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.UserName.Contains(input.userName));
        if (input.moduleName.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.ModuleName == input.moduleName);
        if (input.requestMethod.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.RequestMethod == input.requestMethod);
        var list = await _repository.AsQueryable().Where(whereLambda).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToPagedListAsync(input.CurrentPage, input.PageSize);
        object output = null;
        switch (Type)
        {
            case 1:
                {
                    var pageList = new SqlSugarPagedList<LogLoginOutput>()
                    {
                        list = list.list.Adapt<List<LogLoginOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogLoginOutput>.SqlSugarPageResult(pageList);
                }

            case 3:
                {
                    var pageList = new SqlSugarPagedList<LogOperationOutput>()
                    {
                        list = list.list.Adapt<List<LogOperationOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogOperationOutput>.SqlSugarPageResult(pageList);
                }

            case 4:
                {
                    var pageList = new SqlSugarPagedList<LogExceptionOutput>()
                    {
                        list = list.list.Adapt<List<LogExceptionOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogExceptionOutput>.SqlSugarPageResult(pageList);
                }

            case 5:
                {
                    var pageList = new SqlSugarPagedList<LogRequestOutput>()
                    {
                        list = list.list.Adapt<List<LogRequestOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogRequestOutput>.SqlSugarPageResult(pageList);
                }
        }
        return output;
    }

    /// <summary>
    /// 操作模块.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ModuleName")]
    public async Task<dynamic> ModuleNameSelector()
    {
        return App.EffectiveTypes
                .Where(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(IDynamicApiController).IsAssignableFrom(u))
                .SelectMany(u => u.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(x => x.IsDefined(typeof(OperateLogAttribute), false))
                .Select(x => new { moduleName = x.GetCustomAttribute<OperateLogAttribute>().ModuleName }).Distinct();
    }

    #endregion

    #region POST

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpDelete]
    [UnitOfWork]
    public async Task Delete([FromBody] LogDelInput input)
    {
        await _repository.AsDeleteable().In(it => it.Id, input.ids).ExecuteCommandAsync();
    }

    /// <summary>
    /// 一键删除.
    /// </summary>
    /// <param name="type">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("{type}")]
    [UnitOfWork]
    public async Task Delete(int type)
    {
        await _repository.DeleteAsync(x => x.Category == type);
    }

    #endregion
}