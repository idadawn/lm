using Poxiao.Apps.Entitys;
using Poxiao.Apps.Entitys.Dto;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Dto.Module;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 菜单管理
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Menu", Order = 212)]
[Route("api/system/[controller]")]
public class ModuleService : IModuleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleEntity> _repository;

    /// <summary>
    /// 功能按钮服务.
    /// </summary>
    private readonly IModuleButtonService _moduleButtonService;

    /// <summary>
    /// 功能列表服务.
    /// </summary>
    private readonly IModuleColumnService _moduleColumnService;

    /// <summary>
    /// 功能数据资源服务.
    /// </summary>
    private readonly IModuleDataAuthorizeSchemeService _moduleDataAuthorizeSchemeService;

    /// <summary>
    /// 功能数据方案服务.
    /// </summary>
    private readonly IModuleDataAuthorizeService _moduleDataAuthorizeSerive;

    /// <summary>
    /// 功能表单服务.
    /// </summary>
    private readonly IModuleFormService _moduleFormSerive;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ModuleService"/>类型的新实例.
    /// </summary>
    public ModuleService(
        ISqlSugarRepository<ModuleEntity> repository,
        IFileManager fileManager,
        ModuleButtonService moduleButtonService,
        IModuleColumnService moduleColumnService,
        IModuleFormService moduleFormSerive,
        IModuleDataAuthorizeService moduleDataAuthorizeSerive,
        IModuleDataAuthorizeSchemeService moduleDataAuthorizeSchemeService,
        IUserManager userManager)
    {
        _repository = repository;
        _fileManager = fileManager;
        _moduleButtonService = moduleButtonService;
        _moduleColumnService = moduleColumnService;
        _moduleFormSerive = moduleFormSerive;
        _moduleDataAuthorizeSchemeService = moduleDataAuthorizeSchemeService;
        _moduleDataAuthorizeSerive = moduleDataAuthorizeSerive;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 获取菜单列表.
    /// </summary>
    /// <param name="systemId">模块ID.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("ModuleBySystem/{systemId}")]
    public async Task<dynamic> GetList(string systemId, [FromQuery] ModuleListQuery input)
    {
        var data = await GetList(systemId);
        if (!string.IsNullOrEmpty(input.category))
            data = data.FindAll(x => x.Category == input.category);
        if (!string.IsNullOrEmpty(input.Keyword))
            data = data.TreeWhere(t => t.FullName.Contains(input.Keyword) || t.EnCode.Contains(input.Keyword) || (t.UrlAddress.IsNotEmptyOrNull() && t.UrlAddress.Contains(input.Keyword)), t => t.Id, t => t.ParentId);
        var treeList = data.Adapt<List<ModuleListOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id, string category)
    {
        var data = await GetList(_userManager.User.SystemId);
        if (!string.IsNullOrEmpty(category))
            data = data.FindAll(x => x.Category == category && (x.Type == 1 || x.Type == 0));
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);
        var treeList = data.Where(x => x.EnabledMark.Equals(1)).ToList().Adapt<List<ModuleSelectorOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="systemId">模块ID.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("Selector/{id}/{systemId}")]
    public async Task<dynamic> GetSelector(string id, string systemId, string category)
    {
        var data = await GetList(systemId);
        if (!string.IsNullOrEmpty(category))
            data = data.FindAll(x => x.Category == category && (x.Type == 1 || x.Type == 0));
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);
        var treeList = data.Where(x => x.EnabledMark.Equals(1)).ToList().Adapt<List<ModuleSelectorOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单列表（下拉框）.
    /// </summary>
    /// <param name="category">菜单分类（参数有Web,App）.</param>
    /// <returns></returns>
    [HttpGet("Selector/All")]
    public async Task<dynamic> GetSelectorAll(string category)
    {
        var data = await _repository.AsQueryable().Where(x => x.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
        if (!string.IsNullOrEmpty(category))
            data = data.FindAll(x => x.Category == category);
        var systemList = await _repository.AsSugarClient().Queryable<SystemEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();
        var treeList = data.Adapt<List<ModuleSelectorAllOutput>>();
        foreach (var item in treeList)
        {
            if (item.type == "1")
            {
                item.hasModule = false;
            }

            if (item.ParentId == "-1")
            {
                item.ParentId = item.systemId;
            }
        }
        treeList = treeList.Union(systemList.Select(x => new ModuleSelectorAllOutput()
        {
            Id = x.Id,
            ParentId = "0",
            fullName = x.FullName,
            icon = x.Icon,
            type = "0",
            hasModule = false
        })).ToList();
        return new { list = treeList.ToTree() };
    }

    /// <summary>
    /// 获取菜单信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        var data = await GetInfo(id);
        return data.Adapt<ModuleInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var data = (await GetInfo(id)).Adapt<ModuleExportInput>();
        data.buttonEntityList = (await _moduleButtonService.GetList(id)).Adapt<List<ButtonEntityListItem>>();
        data.columnEntityList = (await _moduleColumnService.GetList(id)).Adapt<List<ColumnEntityListItem>>();
        data.authorizeEntityList = (await _moduleDataAuthorizeSerive.GetList(id)).Adapt<List<AuthorizeEntityListItem>>();
        data.schemeEntityList = (await _moduleDataAuthorizeSchemeService.GetList(id)).Adapt<List<SchemeEntityListItem>>();
        data.formEntityList = (await _moduleFormSerive.GetList(id)).Adapt<List<FromEntityListItem>>();
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.fullName, ExportFileType.bm);
    }
    #endregion

    #region Post

    /// <summary>
    /// 添加菜单.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [UnitOfWork]
    public async Task Creater([FromBody] ModuleCrInput input)
    {
        if (await _repository.IsAnyAsync(x => x.EnCode == input.enCode && x.DeleteMark == null && x.Category == input.category))
            throw Oops.Oh(ErrorCode.COM1025);
        if (await _repository.IsAnyAsync(x => x.FullName == input.fullName && x.DeleteMark == null && x.Category == input.category && input.parentId == x.ParentId && x.SystemId == input.systemId))
            throw Oops.Oh(ErrorCode.COM1024);
        var entity = input.Adapt<ModuleEntity>();

        // 添加字典菜单按钮
        if (entity.Type == 4)
        {
            foreach (var item in await _moduleButtonService.GetList())
            {
                if (item.ModuleId == "-1")
                {
                    item.ModuleId = entity.Id;
                    await _moduleButtonService.Create(item);
                }
            }
        }

        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ModuleUpInput input)
    {
        var info = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (await _repository.IsAnyAsync(x => x.Id != id && x.EnCode == input.enCode && x.DeleteMark == null && x.Category == input.category))
            throw Oops.Oh(ErrorCode.COM1004);
        if (await _repository.IsAnyAsync(x => x.Id != id && (x.FullName == input.fullName) && x.DeleteMark == null && x.Category == input.category && input.parentId == x.ParentId && x.SystemId == input.systemId))
            throw Oops.Oh(ErrorCode.COM1004);
        if (await _repository.IsAnyAsync(x => x.ParentId == id && x.DeleteMark == null && x.SystemId == input.systemId) && info.Type != input.type)
            throw Oops.Oh(ErrorCode.D4008);
        var entity = input.Adapt<ModuleEntity>();
        if (entity.Category.Equals("App"))
        {
            var appData = entity.Adapt<AppDataListAllOutput>();
            appData.isData = _repository.AsSugarClient().Queryable<AppDataEntity>().Any(x => x.ObjectType == "2" && x.CreatorUserId == _userManager.UserId && x.ObjectId == appData.Id && x.DeleteMark == null);
            var objData = appData.ToJsonString();
            await _repository.AsSugarClient().Updateable<AppDataEntity>().SetColumns(it => new AppDataEntity()
            {
                ObjectData = objData
            }).Where(it => it.ObjectId == id).ExecuteCommandAsync();
        }
        var isOk = await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [UnitOfWork]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null || await _repository.IsAnyAsync(x => x.ParentId == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1007);

        if (entity.Category.Equals("App"))
        {
            await _repository.AsSugarClient().Updateable<AppDataEntity>().SetColumns(it => new AppDataEntity()
            {
                DeleteMark = 1,
                DeleteUserId = _userManager.UserId,
                DeleteTime = SqlFunc.GetDate()
            }).Where(it => it.ObjectId == id && it.CreatorUserId == _userManager.UserId).ExecuteCommandAsync();
        }

        var isOk = await _repository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 更新菜单状态.
    /// </summary>
    /// <param name="id">菜单id.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.AsUpdateable().SetColumns(it => new ModuleEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{systemId}/Action/Import")]
    [UnitOfWork]
    public async Task ActionsImport(string systemId, IFormFile file, string parentId, string category)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.bm.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var moduleModel = josn.ToObject<ModuleExportInput>();
        moduleModel.moduleId = null;
        moduleModel.parentId = parentId;
        if (moduleModel == null)
            throw Oops.Oh(ErrorCode.D3006);
        var oldCode = moduleModel.enCode;
        moduleModel.enCode = string.Format("{0}_{1}", moduleModel.enCode, new Random().NextNumberString(6));
        if (moduleModel.type == 3)
        {
            moduleModel.urlAddress = moduleModel.urlAddress.Replace(oldCode, moduleModel.enCode);
        }
        if (moduleModel.parentId != "-1" && !await _repository.IsAnyAsync(x => x.Id == moduleModel.parentId && x.DeleteMark == null && x.SystemId == systemId))
            throw Oops.Oh(ErrorCode.D3007);
        if (category.Equals("Web") && !category.Equals(moduleModel.category))
            throw Oops.Oh(ErrorCode.D4013);
        if (category.Equals("App") && !category.Equals(moduleModel.category))
            throw Oops.Oh(ErrorCode.D4012);
        if (await _repository.IsAnyAsync(x => x.ParentId == moduleModel.parentId && x.FullName == moduleModel.fullName && x.DeleteMark == null && x.Category == moduleModel.category && x.SystemId == systemId))
            throw Oops.Oh(ErrorCode.D4000);
        moduleModel.systemId = systemId;
        await ImportData(moduleModel);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleEntity>> GetList(string systemId)
    {
        if (systemId.IsNullOrEmpty() || systemId.Equals("0"))
        {
            var systemList = await _repository.AsSugarClient().Queryable<SystemEntity>().Where(x => x.DeleteMark == null).
                Select(x => new ModuleEntity
                {
                    Id = x.Id,
                    ParentId = "-1",
                    EnabledMark = x.EnabledMark,
                    Type = 0,
                    Category = "Web",
                    FullName = x.FullName,
                    Icon = x.Icon,
                    EnCode = x.EnCode,
                    SystemId = x.Id
                }).ToListAsync();

            var moduleList = await _repository.AsQueryable().Where(x => x.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
            moduleList.Where(x => x.ParentId.Equals("-1")).ToList().ForEach(it => it.ParentId = it.SystemId);
            moduleList.AddRange(systemList);
            var appSystemList = systemList.Copy();
            appSystemList.ForEach(item => item.Category = "App");
            moduleList.AddRange(appSystemList);
            return moduleList;
        }

        return await _repository.AsQueryable().Where(x => x.DeleteMark == null && x.SystemId == systemId).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<ModuleEntity> GetInfo(string id)
    {
        return await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户树形模块功能列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    [NonAction]
    public async Task<List<ModuleNodeOutput>> GetUserTreeModuleList(string type)
    {
        var output = new List<ModuleNodeOutput>();
        var userSystemId = _userManager.UserOrigin != null && _userManager.UserOrigin.Equals("pc") ? _userManager.User.SystemId : _userManager.User.AppSystemId;
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.Roles;
            if (roles.Any())
            {
                var items = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "module").Select(a => a.ItemId).ToListAsync();
                if (userSystemId.IsNotEmptyOrNull())
                {
                    var menus = await _repository.AsQueryable().Where(a => a.SystemId.Equals(userSystemId) && items.Contains(a.Id))
                        .Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null)
                        .Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
                    output = menus.Adapt<List<ModuleNodeOutput>>();
                }
                else
                {
                    output = new List<ModuleNodeOutput>();
                }
            }
        }
        else
        {
            var menus = await _repository.AsQueryable().Where(a => a.SystemId.Equals(userSystemId) && a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null)
                .Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
            output = menus.Adapt<List<ModuleNodeOutput>>();
        }

        return output.ToTree("-1");
    }

    /// <summary>
    /// 获取用户树形模块功能列表根据 SystemId.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <param name="systemId">SystemId.</param>
    [NonAction]
    public async Task<List<ModuleNodeOutput>> GetUserTreeModuleListBySystemId(string type, string systemId)
    {
        var output = new List<ModuleNodeOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.Roles;
            if (roles.Any())
            {
                var items = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "module").Select(a => a.ItemId).ToListAsync();
                if (systemId.IsNotEmptyOrNull())
                {
                    var menus = await _repository.AsQueryable().Where(a => a.SystemId.Equals(systemId) && items.Contains(a.Id))
                        .Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
                    output = menus.Adapt<List<ModuleNodeOutput>>();
                }
                else
                {
                    output = new List<ModuleNodeOutput>();
                }
            }
        }
        else
        {
            var menus = await _repository.AsQueryable().Where(a => a.SystemId.Equals(systemId) && a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null)
                .Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
            output = menus.Adapt<List<ModuleNodeOutput>>();
        }

        return output.ToTree("-1");
    }

    /// <summary>
    /// 获取用户菜单模块功能列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<dynamic> GetUserModueList(string type)
    {
        var output = new List<ModuleOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.Roles;
            if (roles.Any())
            {
                var items = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "module").GroupBy(it => new { it.ItemId }).Select(it => it.ItemId).ToListAsync();
                output = await _repository.AsQueryable().Where(a => items.Contains(a.Id))
                    .Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select(a => new { Id = a.Id, FullName = a.FullName, SortCode = a.SortCode })
                    .MergeTable().OrderBy(o => o.SortCode).Select<ModuleOutput>().ToListAsync();
            }
        }
        else
        {
            output = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null)
                .Select(a => new { Id = a.Id, FullName = a.FullName, SortCode = a.SortCode }).MergeTable().OrderBy(o => o.SortCode).Select<ModuleOutput>().ToListAsync();
        }

        return output;
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task ImportData(ModuleExportInput data)
    {
        data.id = SnowflakeIdHelper.NextId();
        var idDic = new Dictionary<string, string>();
        if (data.buttonEntityList.Any())
        {
            foreach (var item in data.buttonEntityList)
            {
                item.id = SnowflakeIdHelper.NextId();
                item.moduleId = data.id;
            }
            var button = data.buttonEntityList.Adapt<List<ModuleButtonEntity>>();
            await _repository.AsSugarClient().Insertable(button).ExecuteCommandAsync();
        }
        if (data.columnEntityList.Any())
        {
            foreach (var item in data.columnEntityList)
            {
                item.id = SnowflakeIdHelper.NextId();
                item.moduleId = data.id;
            }
            var colum = data.columnEntityList.Adapt<List<ModuleColumnEntity>>();
            await _repository.AsSugarClient().Insertable(colum).ExecuteCommandAsync();
        }
        var dic = new Dictionary<string, string>();
        if (data.authorizeEntityList.Any())
        {
            foreach (var item in data.authorizeEntityList)
            {
                var id = SnowflakeIdHelper.NextId();
                dic[item.id] = id;
                item.id = id;
                item.moduleId = data.id;
            }
            var dataAuthorize = data.authorizeEntityList.Adapt<List<ModuleDataAuthorizeEntity>>();
            await _repository.AsSugarClient().Insertable(dataAuthorize).ExecuteCommandAsync();
        }
        if (data.schemeEntityList.Any())
        {
            foreach (var item in data.schemeEntityList)
            {
                item.id = SnowflakeIdHelper.NextId();
                item.moduleId = data.id;
                if (item.conditionJson.IsNotEmptyOrNull())
                {
                    foreach (var key in dic.Keys)
                    {
                        item.conditionJson = item.conditionJson.Replace(key, dic[key]);
                    }
                }
            }
            var dataAuthorizeScheme = data.schemeEntityList.Adapt<List<ModuleDataAuthorizeSchemeEntity>>();
            await _repository.AsSugarClient().Insertable(dataAuthorizeScheme).ExecuteCommandAsync();
        }
        if (data.formEntityList.Any())
        {
            foreach (var item in data.formEntityList)
            {
                item.id = SnowflakeIdHelper.NextId();
                item.moduleId = data.id;
            }
            var form = data.formEntityList.Adapt<List<ModuleFormEntity>>();
            await _repository.AsSugarClient().Insertable(form).ExecuteCommandAsync();
        }
        var module = data.Adapt<ModuleEntity>();
        var isOk = await _repository.AsSugarClient().Insertable(module).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.D3008);
    }

    #endregion
}