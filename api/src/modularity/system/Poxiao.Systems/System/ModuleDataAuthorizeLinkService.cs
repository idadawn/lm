using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos.DataBase;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Dto.System.ModuleDataAuthorizeLink;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Engine.Core;
using Poxiao.VisualDev.Entitys;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 数据权限连接管理
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleDataAuthorizeLink", Order = 214)]
[Route("api/system/[controller]")]
public class ModuleDataAuthorizeLinkService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleDataAuthorizeLinkEntity> _repository;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ModuleDataAuthorizeSchemeService"/>类型的新实例.
    /// </summary>
    public ModuleDataAuthorizeLinkService(
        ISqlSugarRepository<ModuleDataAuthorizeLinkEntity> repository,
        IDataBaseManager dataBaseManager,
        IUserManager userManager)
    {
        _repository = repository;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 代码生成字段列表.
    /// </summary>
    /// <param name="linkId">连接id.</param>
    /// <param name="tableName">表名.</param>
    /// <param name="input">分页参数.</param>
    /// <returns></returns>
    [HttpGet("{linkId}/Tables/{tableName}/Fields/{menuType}/{dataType}")]
    public async Task<dynamic> GetList(string linkId, string tableName, string menuType, string dataType, [FromQuery] PageInputBase input)
    {
        var link = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == linkId && x.DeleteMark == null);
        if (string.IsNullOrEmpty(tableName)) return new PageInfo();
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var data = _dataBaseManager.GetFieldList(tenantLink, tableName).Adapt<List<TableFieldOutput>>();
        if (input.Keyword.IsNotEmptyOrNull())
            data = data.FindAll(a => a.field.ToLower().Contains(input.Keyword.ToLower()) || (a.fieldName.IsNotEmptyOrNull() && a.fieldName.ToLower().Contains(input.Keyword.ToLower()))).ToList();
        if (menuType == "2" && dataType != "3")
        {
            data.ForEach(item =>
            {
                item.field = item.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            });
        }
        var pageList = new SqlSugarPagedList<TableFieldOutput>()
        {
            list = data.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize).ToList(),
            pagination = new Pagination()
            {
                CurrentPage = input.CurrentPage,
                PageSize = input.PageSize,
                Total = data.Count
            }
        };
        return PageResult<TableFieldOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="menudId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("getInfo/{menudId}/{type}")]
    public async Task<dynamic> GetInfoApi(string menudId, string type)
    {
        var data = await _repository.GetFirstAsync(x => x.ModuleId == menudId && x.Type == type);
        return data.Adapt<ModuleDataAuthorizeLinkInfoOutput>();
    }

    /// <summary>
    /// 获取表名列表.
    /// </summary>
    /// <param name="menudId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("getVisualTables/{menuId}/{type}")]
    public async Task<dynamic> GetVisualTables(string menuId, string type)
    {
        var tableOutput = new ModuleDataAuthorizeLinkTableOutput();
        var moduleEntity = await _repository.AsSugarClient().Queryable<ModuleEntity>().FirstAsync(x => x.Id == menuId && x.DeleteMark == null);
        if (moduleEntity.Type == 3)
        {
            var visualDevId = moduleEntity.PropertyJson.ToObject<JObject>()["moduleId"].ToString();
            var visualDevEntity = await _repository.AsSugarClient().Queryable<VisualDevEntity>().FirstAsync(x => x.Id == visualDevId && x.DeleteMark == null);
            var tInfo = new TemplateParsingBase(visualDevEntity);
            tableOutput.linkId = visualDevEntity.DbLinkId;
            tableOutput.linkTables = tInfo.AllTable.Select(x => x.table).ToList();
        }

        if (moduleEntity.Type == 2)
        {
            var data = await _repository.GetFirstAsync(x => x.ModuleId == menuId && x.Type == type);
            if (data.IsNotEmptyOrNull())
            {
                tableOutput.linkId = data.LinkId;
                tableOutput.linkTables = data.LinkTables.Split(",").ToList();
            }
        }
        return tableOutput;
    }
    #endregion

    #region POST

    /// <summary>
    /// 保存数据.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("saveLinkData")]
    public async Task SaveLinkData([FromBody] ModuleDataAuthorizeLinkInfoOutput input)
    {
        await _repository.DeleteAsync(x => x.ModuleId == input.moduleId && x.Type == input.dataType);
        var entity = input.Adapt<ModuleDataAuthorizeLinkEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.Type = input.dataType;
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }
    #endregion
}