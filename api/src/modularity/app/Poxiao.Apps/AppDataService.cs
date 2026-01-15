using Poxiao.Apps.Entitys;
using Poxiao.Apps.Entitys.Dto;
using Poxiao.Apps.Interfaces;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.WorkFlow.Entitys.Dto.FlowEngine;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Interfaces.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Apps;

/// <summary>
/// App常用数据
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[ApiDescriptionSettings(Tag = "App", Name = "Data", Order = 800)]
[Route("api/App/[controller]")]
public class AppDataService : IAppDataService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<AppDataEntity> _repository; // App常用数据

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 流程管理.
    /// </summary>
    private readonly IFlowTemplateService _flowTemplateService;

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="userManager"></param>
    /// <param name="flowTemplateService"></param>
    public AppDataService(
        ISqlSugarRepository<AppDataEntity> repository,
        IUserManager userManager,
        IFlowTemplateService flowTemplateService)
    {
        _repository = repository;
        _userManager = userManager;
        _flowTemplateService = flowTemplateService;
    }

    #region Get

    /// <summary>
    /// 常用数据.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] string type)
    {
        List<AppDataEntity>? list = await GetListByType(type);
        List<AppDataListOutput>? output = list.Adapt<List<AppDataListOutput>>();
        if (type.Equals("1"))
        {
            foreach (var item in output)
            {
                var flowJson = _repository.AsSugarClient().Queryable<FlowTemplateJsonEntity>().First(x => x.TemplateId == item.objectId && x.EnabledMark == 1 && x.DeleteMark == null);
                if (flowJson != null)
                {
                    item.objectId = flowJson.Id;
                }
            }
        }
        return new { list = output };
    }

    /// <summary>
    /// 所有流程.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getFlowList")]
    public async Task<dynamic> GetFlowList([FromQuery] FlowEngineListInput input)
    {
        var list = await _repository.AsSugarClient().Queryable<FlowTemplateEntity>()
               .Where(a => a.DeleteMark == null && a.EnabledMark == 1 && a.Type == 0)
               .WhereIF(input.category.IsNotEmptyOrNull(), a => a.Category == input.category)
               .WhereIF(input.Keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
               .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
               .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
               .Select(a => new AppFlowListAllOutput
               {
                   Id = a.Id,
                   icon = a.Icon,
                   enCode = a.EnCode,
                   fullName = a.FullName,
                   iconBackground = a.IconBackground,
                   isData = SqlFunc.Subqueryable<AppDataEntity>().Where(x => x.ObjectType == "1" && x.CreatorUserId == _userManager.UserId && x.ObjectId == a.Id && x.DeleteMark == null).Any(),
               }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<AppFlowListAllOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 所有流程.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getDataList")]
    public async Task<dynamic> GetDataList(string keyword)
    {
        List<AppDataListAllOutput>? list = (await GetAppMenuList(keyword)).Adapt<List<AppDataListAllOutput>>();
        foreach (AppDataListAllOutput? item in list)
        {
            item.isData = _repository.IsAny(x => x.ObjectType == "2" && x.CreatorUserId == _userManager.UserId && x.ObjectId == item.Id && x.DeleteMark == null);
        }

        List<AppDataListAllOutput>? output = list.ToTree("-1");
        return new { list = output };
    }
    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] AppDataCrInput input)
    {
        AppDataEntity? entity = input.Adapt<AppDataEntity>();
        int isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    [HttpDelete("{objectId}")]
    public async Task Delete(string objectId)
    {
        AppDataEntity? entity = await _repository.GetSingleAsync(x => x.ObjectId == objectId && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null);
        var isOk = await _repository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private async Task<List<AppDataEntity>> GetListByType(string type)
    {
        return await _repository.AsQueryable().Where(x => x.ObjectType == type && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 菜单列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleEntity>> GetAppMenuList(string keyword)
    {
        List<ModuleEntity>? menuList = new List<ModuleEntity>();
        if (_userManager.IsAdministrator)
        {
            menuList = await _repository.AsSugarClient().Queryable<ModuleEntity>()
                .Where(x => x.EnabledMark == 1 && x.Category == "App" && x.DeleteMark == null && x.SystemId == _userManager.User.AppSystemId)
                .WhereIF(!string.IsNullOrEmpty(keyword), x => x.FullName.Contains(keyword) || x.ParentId == "-1")
                .OrderBy(o => o.SortCode)
                .ToListAsync();
        }
        else
        {
            string[]? objectIds = _userManager.Roles.ToArray();
            if (objectIds.Length == 0)
                return menuList;
            List<string>? ids = await _repository.AsSugarClient().Queryable<AuthorizeEntity>()
                .Where(x => objectIds.Contains(x.ObjectId) && x.ObjectType == "Role" && x.ItemType == "module").Select(x => x.ItemId).ToListAsync();
            if (ids.Count == 0)
                return menuList;
            menuList = await _repository.AsSugarClient().Queryable<ModuleEntity>()
                .Where(x => ids.Contains(x.Id) && x.EnabledMark == 1 && x.Category == "App" && x.DeleteMark == null && x.SystemId == _userManager.User.AppSystemId)
                .WhereIF(!string.IsNullOrEmpty(keyword), x => x.FullName.Contains(keyword) || x.ParentId == "-1")
                .OrderBy(o => o.SortCode)
                .ToListAsync();
        }

        return menuList;
    }

    #endregion
}