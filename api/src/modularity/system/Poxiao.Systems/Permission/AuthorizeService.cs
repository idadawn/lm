using Aop.Api.Domain;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Dto.Authorize;
using Poxiao.Systems.Entitys.Model.Authorize;
using Poxiao.Systems.Entitys.Model.Menu;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.VisualDev.Entitys.Entity;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 业务实现：操作权限.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Authority", Order = 170)]
[Route("api/permission/[controller]")]
public class AuthorizeService : IAuthorizeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 权限操作表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<AuthorizeEntity> _authorizeRepository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// IM中心处理程序.
    /// </summary>
    private IMHandler _imHandler;

    /// <summary>
    /// 初始化一个<see cref="AuthorizeService"/>类型的新实例.
    /// </summary>
    public AuthorizeService(
        ISqlSugarRepository<AuthorizeEntity> authorizeRepository,
        ICacheManager cacheManager,
        IUserManager userManager,
        IMHandler imHandler)
    {
        _authorizeRepository = authorizeRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _imHandler = imHandler;
    }

    #region Get

    /// <summary>
    /// 获取功能权限数据.
    /// </summary>
    /// <param name="itemId">模块ID.</param>
    /// <param name="objectType">对象类型.</param>
    /// <returns></returns>
    [HttpGet("Model/{itemId}/{objectType}")]
    public async Task<dynamic> GetModelList(string itemId, string objectType)
    {
        IEnumerable<string> ids = await _authorizeRepository.AsQueryable().Where(a => a.ItemId == itemId && a.ObjectType == objectType).Select(s => s.ObjectId).ToListAsync();
        return new { ids };
    }

    /// <summary>
    /// 获取模块列表展示字段权限.
    /// </summary>
    /// <param name="moduleId">模块主键.</param>
    /// <returns></returns>
    [HttpGet("GetColumnsByModuleId/{moduleId}")]
    public async Task<dynamic> GetColumnsByModuleId(string moduleId)
    {
        string? data = await _authorizeRepository.AsSugarClient().Queryable<ColumnsPurviewEntity>().Where(x => x.ModuleId == moduleId).Select(x => x.FieldList).FirstAsync();
        if (!string.IsNullOrEmpty(data)) return data.ToObject<List<ListDisplayFieldOutput>>();
        else return new List<ListDisplayFieldOutput>();
    }

    #endregion

    #region Post

    /// <summary>
    /// 权限数据.
    /// </summary>
    /// <param name="objectId">对象主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Data/{objectId}/Values")]
    public async Task<dynamic> GetDataValues(string objectId, [FromBody] AuthorizeDataQuery input)
    {
        AuthorizeDataOutput? output = new AuthorizeDataOutput();
        AuthorizeModel? authorizeData = new AuthorizeModel();
        string? userId = _userManager.UserId;
        bool isAdmin = _userManager.IsAdministrator;

        List<ModuleEntity>? menuList = await GetCurrentUserModuleAuthorize(userId, isAdmin, _userManager.Roles.ToArray(), input.moduleIds.Split(",").ToArray());
        var systemIds = input.moduleIds.Split(",").ToList();
        var systemList = await _authorizeRepository.AsSugarClient().Queryable<SystemEntity>()
            .Where(x => x.DeleteMark == null && x.EnabledMark.Equals(1) && systemIds.Contains(x.Id))
            .Select(x => new ModuleEntity()
            {
                Id = x.Id,
                ParentId = "-1",
                FullName = x.FullName,
                Icon = x.Icon,
                SystemId = "-1",
                SortCode = x.SortCode,
            }).ToListAsync();
        systemList.ForEach(item =>
        {
            if (menuList.Any(it => it.Category.Equals("App") && it.SystemId.Equals(item.Id)))
            {
                var rId = Yitter.IdGenerator.YitIdHelper.NextId().ToString();
                menuList.Where(it => it.Category.Equals("App") && it.ParentId.Equals("-1") && it.SystemId.Equals(item.Id)).ToList().ForEach(it =>
                {
                    it.ParentId = rId;
                });
                menuList.Add(new ModuleEntity()
                {
                    Id = rId,
                    FullName = "app菜单",
                    Icon = "ym-custom ym-custom-cellphone",
                    ParentId = item.Id,
                    Category = "App",
                    EnCode = "app菜单",
                    Type = 1,
                    SystemId = item.Id,
                    SortCode = 99999
                });
            }

            menuList.Where(it => it.Category.Equals("Web") && it.SystemId.Equals(item.Id) && it.ParentId.Equals("-1")).ToList().ForEach(it =>
            {
                it.ParentId = item.Id;
            });
        });
        menuList.AddRange(systemList);

        List<ModuleButtonEntity>? moduleButtonList = await GetCurrentUserButtonAuthorize(userId, isAdmin, _userManager.Roles.ToArray());
        List<ModuleColumnEntity>? moduleColumnList = await GetCurrentUserColumnAuthorize(userId, isAdmin, _userManager.Roles.ToArray());
        List<ModuleFormEntity>? moduleFormList = await GetCurrentUserFormAuthorize(userId, isAdmin, _userManager.Roles.ToArray());
        List<ModuleDataAuthorizeSchemeEntity>? moduleDataSchemeList = await GetCurrentUserResourceAuthorize(userId, isAdmin, _userManager.Roles.ToArray());

        authorizeData.FunctionList = menuList.Adapt<List<FunctionalModel>>();
        authorizeData.ButtonList = moduleButtonList.Adapt<List<FunctionalButtonModel>>();
        authorizeData.ColumnList = moduleColumnList.Adapt<List<FunctionalViewModel>>();
        authorizeData.FormList = moduleFormList.Adapt<List<FunctionalFormModel>>();
        authorizeData.ResourceList = moduleDataSchemeList.Adapt<List<FunctionalResourceModel>>();

        #region 已勾选的权限id

        List<AuthorizeEntity>? authorizeList = await this.GetAuthorizeListByObjectId(objectId);
        List<string>? checkSystemList = authorizeList.Where(o => o.ItemType.Equals("system")).Select(m => m.ItemId).ToList();
        List<string>? checkModuleList = authorizeList.Where(o => o.ItemType.Equals("module")).Select(m => m.ItemId).ToList();
        List<string>? checkButtonList = authorizeList.Where(o => o.ItemType.Equals("button")).Select(m => m.ItemId).ToList();
        List<string>? checkColumnList = authorizeList.Where(o => o.ItemType.Equals("column")).Select(m => m.ItemId).ToList();
        List<string>? checkFormList = authorizeList.Where(o => o.ItemType.Equals("form")).Select(m => m.ItemId).ToList();
        List<string>? checkResourceList = authorizeList.Where(o => o.ItemType.Equals("resource")).Select(m => m.ItemId).ToList();

        #endregion

        List<ModuleEntity>? moduleList = new List<ModuleEntity>();
        List<string>? childNodesIds = new List<string>();
        switch (input.type)
        {
            case "system":
                systemIds = await _authorizeRepository.AsQueryable().Where(o => o.ItemType.Equals("system") && _userManager.Roles.Contains(o.ObjectId)).Select(o => o.ItemId).ToListAsync();
                output.list = await _authorizeRepository.AsSugarClient().Queryable<SystemEntity>()
                    .Where(x => x.DeleteMark == null && x.EnabledMark.Equals(1))
                    .WhereIF(!isAdmin, x => systemIds.Contains(x.Id))
                    .Select(x => new AuthorizeDataModelOutput()
                    {
                        Id = x.Id,
                        fullName = x.FullName,
                        icon = x.Icon,
                        sortCode = x.SortCode
                    }).ToListAsync();
                output.all = output.list.Select(x => x.Id).ToList();
                output.ids = await _authorizeRepository.AsSugarClient().Queryable<SystemEntity>()
                    .Where(x => x.DeleteMark == null && x.EnabledMark.Equals(1) && checkSystemList.Contains(x.Id)).Select(x => x.Id).ToListAsync();
                return output;
            case "module":
                List<AuthorizeDataModelOutput>? authorizeDataModuleList = authorizeData.FunctionList.Adapt<List<AuthorizeDataModelOutput>>();
                GetOutPutResult(ref output, authorizeDataModuleList, checkModuleList);
                return GetResult(output);
            case "button":
                if (string.IsNullOrEmpty(input.moduleIds))
                {
                    return output;
                }
                else
                {
                    List<string>? moduleIdList = new List<string>(input.moduleIds.Split(","));
                    moduleIdList.ForEach(ids =>
                    {
                        ModuleEntity? moduleEntity = menuList.Find(m => m.Id == ids);
                        if (moduleEntity != null) moduleList.Add(moduleEntity);
                    });

                    // 勾选的菜单末级节点菜单id集合
                    childNodesIds = GetChildNodesId(moduleList);
                }
                moduleList = await GetModuleAndSystemScheme(moduleList, menuList);
                output = GetButton(moduleList, moduleButtonList, childNodesIds, checkButtonList);
                return GetResult(output);
            case "column":
                if (string.IsNullOrEmpty(input.moduleIds))
                {
                    return output;
                }
                else
                {
                    List<string>? moduleIdList = new List<string>(input.moduleIds.Split(","));
                    moduleIdList.ForEach(ids =>
                    {
                        ModuleEntity? moduleEntity = menuList.Find(m => m.Id == ids);
                        if (moduleEntity != null) moduleList.Add(moduleEntity);
                    });

                    // 子节点菜单id集合
                    childNodesIds = GetChildNodesId(moduleList);
                }
                moduleList = await GetModuleAndSystemScheme(moduleList, menuList);
                output = GetColumn(moduleList, moduleColumnList, childNodesIds, checkColumnList);
                return GetResult(output);
            case "form":
                if (string.IsNullOrEmpty(input.moduleIds))
                {
                    return output;
                }
                else
                {
                    List<string>? moduleIdList = new List<string>(input.moduleIds.Split(","));
                    moduleIdList.ForEach(ids =>
                    {
                        ModuleEntity? moduleEntity = menuList.Find(m => m.Id == ids);
                        if (moduleEntity != null) moduleList.Add(moduleEntity);
                    });

                    // 子节点菜单id集合
                    childNodesIds = GetChildNodesId(moduleList);
                }

                moduleList = await GetModuleAndSystemScheme(moduleList, menuList);
                output = GetForm(moduleList, moduleFormList, childNodesIds, checkFormList);
                return GetResult(output);
            case "resource":
                if (string.IsNullOrEmpty(input.moduleIds))
                {
                    return output;
                }
                else
                {
                    List<string>? moduleIdList = new List<string>(input.moduleIds.Split(","));
                    moduleIdList.ForEach(ids =>
                    {
                        ModuleEntity? moduleEntity = menuList.Find(m => m.Id == ids);
                        if (moduleEntity != null) moduleList.Add(moduleEntity);
                    });

                    // 子节点菜单id集合
                    childNodesIds = GetChildNodesId(moduleList);
                }

                moduleList = await GetModuleAndSystemScheme(moduleList, menuList);
                output = GetResource(moduleList, moduleDataSchemeList, childNodesIds, checkResourceList);
                return GetResult(output);
            default:
                return output;
        }
    }

    /// <summary>
    /// 设置或更新岗位/角色/用户权限.
    /// </summary>
    /// <param name="objectId">参数.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Data/{objectId}")]
    public async Task UpdateData(string objectId, [FromBody] AuthorizeDataUpInput input)
    {
        #region 分级权限验证

        if (input.objectType.Equals("Role") && !_userManager.IsAdministrator)
        {
            RoleEntity? oldRole = await _authorizeRepository.AsSugarClient().Queryable<RoleEntity>().FirstAsync(x => x.Id.Equals(objectId));
            if (oldRole.GlobalMark == 1) throw Oops.Oh(ErrorCode.D1612); // 全局角色 只能超管才能变更
        }

        if (input.objectType.Equals("Position") || input.objectType.Equals("Role"))
        {
            var orgIds = new List<string>();
            if (input.objectType.Equals("Position")) orgIds = await _authorizeRepository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.Id.Equals(objectId)).Select(x => x.OrganizeId).ToListAsync();
            if (input.objectType.Equals("Role")) orgIds = await _authorizeRepository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId.Equals(objectId) && x.ObjectType == input.objectType).Select(x => x.OrganizeId).ToListAsync();

            if (!_userManager.DataScope.Any(it => orgIds.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
                throw Oops.Oh(ErrorCode.D1013); // 分级管控
        }

        #endregion

        input.button = input.button.Except(input.module).ToList();
        input.column = input.column.Except(input.module).ToList();
        input.form = input.form.Except(input.module).ToList();
        input.resource = input.resource.Except(input.module).ToList();
        List<AuthorizeEntity>? authorizeList = new List<AuthorizeEntity>();
        AddAuthorizeEntity(ref authorizeList, input.systemIds, objectId, input.objectType, "system");
        AddAuthorizeEntity(ref authorizeList, input.module, objectId, input.objectType, "module");
        AddAuthorizeEntity(ref authorizeList, input.button, objectId, input.objectType, "button");
        AddAuthorizeEntity(ref authorizeList, input.column, objectId, input.objectType, "column");
        AddAuthorizeEntity(ref authorizeList, input.form, objectId, input.objectType, "form");
        AddAuthorizeEntity(ref authorizeList, input.resource, objectId, input.objectType, "resource");

        // 删除除了门户外的相关权限
        await _authorizeRepository.DeleteAsync(a => a.ObjectId == objectId && !a.ItemType.Equals("portalManage"));

        if (authorizeList.Count > 0)
        {
            // 新增权限
            await _authorizeRepository.AsSugarClient().Insertable(authorizeList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        }

        // 编辑角色权限退出角色的登录用户
        if (input.objectType.Equals("Role")) await ForcedOffline(new List<string>() { objectId });
    }

    /// <summary>
    /// 批量设置权限.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Data/Batch")]
    public async Task BatchData([FromBody] AuthorizeDataBatchInput input)
    {
        #region 分级权限验证

        // 获取所有角色
        var allRole = await _authorizeRepository.AsSugarClient().Queryable<RoleEntity>().Where(x => input.roleIds.Contains(x.Id)).ToListAsync();
        if (allRole.Any(x => x.GlobalMark.Equals(1)) && !_userManager.IsAdministrator) throw Oops.Oh(ErrorCode.D1612); // 全局角色 只能超管才能变更

        // 获取组织角色 所属组织
        var orgIds = await _authorizeRepository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => allRole.Select(x => x.Id).Contains(x.ObjectId) && x.ObjectType.Equals("Role")).Select(x => x.OrganizeId).ToListAsync();

        if (!_userManager.DataScope.Any(it => orgIds.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013); // 分级管控

        #endregion

        // 计算按钮、列表、资源三个集合内不包含菜单ID的差
        input.button = input.button.Except(input.module).ToList();
        input.column = input.column.Except(input.module).ToList();
        input.form = input.form.Except(input.module).ToList();
        input.resource = input.resource.Except(input.module).ToList();

        // 拼装权限集合
        List<AuthorizeEntity>? authorizeItemList = new List<AuthorizeEntity>();
        List<AuthorizeEntity>? authorizeObejctList = new List<AuthorizeEntity>();
        BatchAddAuthorizeEntity(ref authorizeItemList, input.systemIds, "system", true);
        BatchAddAuthorizeEntity(ref authorizeItemList, input.module, "module", true);
        BatchAddAuthorizeEntity(ref authorizeItemList, input.button, "button", true);
        BatchAddAuthorizeEntity(ref authorizeItemList, input.column, "column", true);
        BatchAddAuthorizeEntity(ref authorizeItemList, input.form, "form", true);
        BatchAddAuthorizeEntity(ref authorizeItemList, input.resource, "resource", true);
        BatchAddAuthorizeEntity(ref authorizeObejctList, input.positionIds, "Position", false);
        BatchAddAuthorizeEntity(ref authorizeObejctList, input.roleIds, "Role", false);
        BatchAddAuthorizeEntity(ref authorizeObejctList, input.userIds, "User", false);
        List<AuthorizeEntity>? data = new List<AuthorizeEntity>();
        SeveBatch(ref data, authorizeObejctList, authorizeItemList);

        // 获取已有权限集合
        List<AuthorizeEntity>? existingRoleData = await _authorizeRepository.AsQueryable().Where(x => input.roleIds.Contains(x.ObjectId) && x.ObjectType.Equals("Role")).ToListAsync();

        // 计算新增菜单集合与已有权限集合差
        data = data.Except(existingRoleData).ToList();

        // 数据不为空添加
        if (data.Count > 0)
        {
            // 新增权限
            int num = await _authorizeRepository.AsSugarClient().Insertable(data).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        }

        // 编辑角色权限退出角色的登录用户
        await ForcedOffline(input.roleIds);
    }

    /// <summary>
    /// 设置/更新功能权限.
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Model/{itemId}")]
    public async Task UpdateModel(string itemId, [FromBody] AuthorizeModelInput input)
    {
        List<AuthorizeEntity>? authorizeList = new List<AuthorizeEntity>();
        List<PortalDataEntity>? portalList = new List<PortalDataEntity>();

        try
        {
            _authorizeRepository.AsSugarClient().Ado.BeginTran();

            // 角色ID不为空
            if (input.objectId.Count > 0)
            {
                input.objectId.ForEach(item =>
                {
                    AuthorizeEntity? entity = new AuthorizeEntity();
                    entity.Id = SnowflakeIdHelper.NextId();
                    entity.CreatorTime = DateTime.Now;
                    entity.CreatorUserId = _userManager.UserId;
                    entity.ItemId = itemId;
                    entity.ItemType = input.itemType;
                    entity.ObjectId = item;
                    entity.ObjectType = input.objectType;
                    entity.SortCode = input.objectId.IndexOf(item);
                    authorizeList.Add(entity);
                });

                // 删除除了门户外的相关权限
                await _authorizeRepository.DeleteAsync(a => a.ItemId == itemId);

                // 新增权限
                await _authorizeRepository.AsSugarClient().Insertable(authorizeList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            }
            else
            {
                // 删除除了门户外的相关权限
                await _authorizeRepository.DeleteAsync(a => a.ItemId == itemId);
            }

            _authorizeRepository.AsSugarClient().Ado.CommitTran();
        }
        catch
        {
            _authorizeRepository.AsSugarClient().Ado.RollbackTran();
        }

        if (input.objectId.Any() && !input.itemType.Equals("portalManage")) await ForcedOffline(input.objectId); // 编辑角色权限退出角色的登录用户
    }

    /// <summary>
    /// 设置模块列表展示字段权限.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("SetColumnsByModuleId")]
    public async Task SetColumnsByModuleId([FromBody] ColumnsPurviewDataUpInput input)
    {
        ColumnsPurviewEntity? entity = await _authorizeRepository.AsSugarClient().Queryable<ColumnsPurviewEntity>().Where(x => x.ModuleId == input.moduleId).FirstAsync();
        if (entity == null) entity = new ColumnsPurviewEntity();
        entity.FieldList = input.fieldList;
        entity.ModuleId = input.moduleId;

        if (entity.Id.IsNotEmptyOrNull())
        {
            // 更新
            int newEntity = await _authorizeRepository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        }
        else
        {
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreatorTime = DateTime.Now;
            entity.CreatorUserId = _userManager.UserId;
            await _authorizeRepository.AsSugarClient().Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        }
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 添加权限接口参数组装.
    /// </summary>
    /// <param name="list">返回参数.</param>
    /// <param name="itemIds">权限数据id.</param>
    /// <param name="objectId">对象ID.</param>
    /// <param name="objectType">分类.</param>
    /// <param name="itemType">权限分类.</param>
    private void AddAuthorizeEntity(ref List<AuthorizeEntity> list, List<string> itemIds, string objectId, string objectType, string itemType)
    {
        foreach (string? item in itemIds)
        {
            AuthorizeEntity? entity = new AuthorizeEntity();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreatorTime = DateTime.Now;
            entity.CreatorUserId = _userManager.UserId;
            entity.ItemId = item;
            entity.ObjectId = objectId;
            entity.ItemType = itemType;
            entity.ObjectType = objectType;
            entity.SortCode = itemIds.IndexOf(item);
            list.Add(entity);
        }
    }

    /// <summary>
    /// 批量添加权限接口参数组装.
    /// </summary>
    /// <param name="list">返回参数.</param>
    /// <param name="ids">来源数据.</param>
    /// <param name="type">来源类型.</param>
    /// <param name="isData">是否是权限数据.</param>
    private void BatchAddAuthorizeEntity(ref List<AuthorizeEntity> list, List<string> ids, string type, bool isData)
    {
        if (ids != null && ids.Count != 0)
        {
            if (isData)
            {
                foreach (string? item in ids)
                {
                    AuthorizeEntity? entity = new AuthorizeEntity();
                    entity.ItemId = item;
                    entity.ItemType = type;
                    list.Add(entity);
                }
            }
            else
            {
                foreach (string? item in ids)
                {
                    AuthorizeEntity? entity = new AuthorizeEntity();
                    entity.ObjectId = item;
                    entity.ObjectType = type;
                    list.Add(entity);
                }
            }
        }
    }

    /// <summary>
    /// 保存批量权限.
    /// </summary>
    /// <param name="list">返回list.</param>
    /// <param name="objectList">对象数据.</param>
    /// <param name="authorizeList">权限数据.</param>
    private void SeveBatch(ref List<AuthorizeEntity> list, List<AuthorizeEntity> objectList, List<AuthorizeEntity> authorizeList)
    {
        foreach (AuthorizeEntity? objectItem in objectList)
        {
            foreach (AuthorizeEntity entityItem in authorizeList)
            {
                AuthorizeEntity? entity = new AuthorizeEntity();
                entity.Id = SnowflakeIdHelper.NextId();
                entity.CreatorTime = DateTime.Now;
                entity.CreatorUserId = _userManager.UserId;
                entity.ItemId = entityItem.ItemId;
                entity.ItemType = entityItem.ItemType;
                entity.ObjectId = objectItem.ObjectId;
                entity.ObjectType = objectItem.ObjectType;
                entity.SortCode = authorizeList.IndexOf(entityItem);
                list.Add(entity);
            }
        }
    }

    /// <summary>
    /// 返回参数处理.
    /// </summary>
    /// <param name="output">返回参数.</param>
    /// <param name="list">返回参数数据.</param>
    /// <param name="checkList">已勾选的id.</param>
    /// <param name="parentId"></param>
    private void GetOutPutResult(ref AuthorizeDataOutput output, List<AuthorizeDataModelOutput> list, List<string> checkList, string parentId = "-1")
    {
        output.all = list.Select(l => l.Id).ToList();
        output.ids = checkList.Intersect(output.all).ToList();
        output.list = list.OrderBy(x => x.sortCode).ToList().ToTree(parentId);
    }

    /// <summary>
    /// 获取子节点菜单id.
    /// </summary>
    /// <param name="moduleEntitiesList"></param>
    /// <returns></returns>
    private List<string> GetChildNodesId(List<ModuleEntity> moduleEntitiesList)
    {
        List<string>? ids = moduleEntitiesList.Select(m => m.Id).ToList();
        List<string>? pids = moduleEntitiesList.Select(m => m.ParentId).ToList();
        List<string>? childNodesIds = ids.Where(x => !pids.Contains(x) && moduleEntitiesList.Find(m => m.Id == x).ParentId != "-1").ToList();
        return childNodesIds.Union(ids).ToList();
    }

    /// <summary>
    /// 过滤菜单权限数据.
    /// </summary>
    /// <param name="childNodesIds">其他权限数据菜单id集合.</param>
    /// <param name="moduleList">勾选菜单权限数据.</param>
    /// <param name="output">返回值.</param>
    private void GetParentsModuleList(List<string> childNodesIds, List<ModuleEntity> moduleList, ref List<AuthorizeDataModelOutput> output)
    {
        // 获取有其他权限的菜单末级节点id
        List<AuthorizeDataModelOutput>? authorizeModuleData = moduleList.Adapt<List<AuthorizeDataModelOutput>>();
        foreach (string? item in childNodesIds)
        {
            GteModuleListById(item, authorizeModuleData, output);
        }

        output = output.Distinct().ToList();
    }

    /// <summary>
    /// 根据菜单id递归获取authorizeDataOutputModel的父级菜单.
    /// </summary>
    /// <param name="id">菜单id.</param>
    /// <param name="authorizeDataOutputModel">选中菜单集合.</param>
    /// <param name="output">返回数据.</param>
    private void GteModuleListById(string id, List<AuthorizeDataModelOutput> authorizeDataOutputModel, List<AuthorizeDataModelOutput> output)
    {
        AuthorizeDataModelOutput? data = authorizeDataOutputModel.Find(l => l.Id == id);
        if (data != null)
        {
            if (data.ParentId != "-1")
            {
                if (!output.Contains(data)) output.Add(data);
                GteModuleListById(data.ParentId, authorizeDataOutputModel, output);
            }
            else
            {
                if (!output.Contains(data)) output.Add(data);
            }
        }
    }

    /// <summary>
    /// 按钮权限.
    /// </summary>
    /// <param name="moduleList">选中的菜单.</param>
    /// <param name="moduleButtonList">所有的按钮.</param>
    /// <param name="childNodesIds"></param>
    /// <param name="checkList"></param>
    /// <returns></returns>
    private AuthorizeDataOutput GetButton(List<ModuleEntity> moduleList, List<ModuleButtonEntity> moduleButtonList, List<string> childNodesIds, List<string> checkList)
    {
        AuthorizeDataOutput? output = new AuthorizeDataOutput();
        List<ModuleButtonEntity>? buttonList = new List<ModuleButtonEntity>();
        childNodesIds.ForEach(ids =>
        {
            List<ModuleButtonEntity>? buttonEntity = moduleButtonList.FindAll(m => m.ModuleId == ids);
            if (buttonEntity.Count != 0)
            {
                buttonEntity.ForEach(bt =>
                {
                    bt.Icon = string.Empty;
                    if (bt.ParentId.Equals("-1"))
                    {
                        bt.ParentId = ids;
                    }
                });
                buttonList = buttonList.Union(buttonEntity).ToList();
            }
        });
        List<AuthorizeDataModelOutput>? authorizeDataButtonList = buttonList.Adapt<List<AuthorizeDataModelOutput>>();
        List<AuthorizeDataModelOutput>? authorizeDataModuleList = new List<AuthorizeDataModelOutput>();

        // 末级菜单id集合
        List<string>? moduleIds = buttonList.Select(b => b.ModuleId).ToList().Distinct().ToList();
        GetParentsModuleList(moduleIds, moduleList, ref authorizeDataModuleList);
        List<AuthorizeDataModelOutput>? list = authorizeDataModuleList.Union(authorizeDataButtonList).ToList();
        GetOutPutResult(ref output, list, checkList);
        return output;
    }

    /// <summary>
    /// 列表权限.
    /// </summary>
    /// <param name="moduleList">选中的菜单.</param>
    /// <param name="moduleColumnEntity">所有的列表.</param>
    /// <param name="childNodesIds"></param>
    /// <param name="checkList"></param>
    /// <returns></returns>
    private AuthorizeDataOutput GetColumn(List<ModuleEntity> moduleList, List<ModuleColumnEntity> moduleColumnEntity, List<string> childNodesIds, List<string> checkList)
    {
        AuthorizeDataOutput? output = new AuthorizeDataOutput();
        List<ModuleColumnEntity>? columnList = new List<ModuleColumnEntity>();
        childNodesIds.ForEach(ids =>
        {
            List<ModuleColumnEntity>? columnEntity = moduleColumnEntity.FindAll(m => m.ModuleId == ids);
            if (columnEntity.Count != 0)
            {
                columnEntity.ForEach(bt =>
                {
                    bt.ParentId = ids;
                });
                columnList = columnList.Union(columnEntity).ToList();
            }
        });
        List<AuthorizeDataModelOutput>? authorizeDataColumnList = columnList.Adapt<List<AuthorizeDataModelOutput>>();
        List<AuthorizeDataModelOutput>? authorizeDataModuleList = new List<AuthorizeDataModelOutput>();
        List<string>? moduleIds = columnList.Select(b => b.ModuleId).ToList().Distinct().ToList();
        GetParentsModuleList(moduleIds, moduleList, ref authorizeDataModuleList);
        List<AuthorizeDataModelOutput>? list = authorizeDataModuleList.Union(authorizeDataColumnList).ToList();
        GetOutPutResult(ref output, list, checkList);
        return output;
    }

    /// <summary>
    /// 表单权限.
    /// </summary>
    /// <returns></returns>
    private AuthorizeDataOutput GetForm(List<ModuleEntity> moduleList, List<ModuleFormEntity> moduleFormEntity, List<string> childNodesIds, List<string> checkList)
    {
        AuthorizeDataOutput? output = new AuthorizeDataOutput();
        List<ModuleFormEntity>? formList = new List<ModuleFormEntity>();
        childNodesIds.ForEach(ids =>
        {
            List<ModuleFormEntity>? formEntity = moduleFormEntity.FindAll(m => m.ModuleId == ids);
            if (formEntity.Count != 0)
            {
                formEntity.ForEach(bt =>
                {
                    bt.ParentId = ids;
                });
                formList = formList.Union(formEntity).ToList();
            }
        });
        List<AuthorizeDataModelOutput>? authorizeDataFormList = formList.Adapt<List<AuthorizeDataModelOutput>>();
        List<AuthorizeDataModelOutput>? authorizeDataModuleList = new List<AuthorizeDataModelOutput>();
        List<string>? moduleIds = formList.Select(b => b.ModuleId).ToList().Distinct().ToList();
        GetParentsModuleList(moduleIds, moduleList, ref authorizeDataModuleList);
        List<AuthorizeDataModelOutput>? list = authorizeDataModuleList.Union(authorizeDataFormList).ToList();
        GetOutPutResult(ref output, list, checkList);
        return output;
    }

    /// <summary>
    /// 数据权限.
    /// </summary>
    /// <param name="moduleList"></param>
    /// <param name="moduleResourceEntity"></param>
    /// <param name="childNodesIds"></param>
    /// <param name="checkList"></param>
    /// <returns></returns>
    private AuthorizeDataOutput GetResource(List<ModuleEntity> moduleList, List<ModuleDataAuthorizeSchemeEntity> moduleResourceEntity, List<string> childNodesIds, List<string> checkList)
    {
        List<string>? moduleIds = new List<string>();
        AuthorizeDataOutput? output = new AuthorizeDataOutput();
        List<AuthorizeDataModelOutput>? authorizeDataResourceList = new List<AuthorizeDataModelOutput>();
        childNodesIds.ForEach(ids =>
        {
            List<ModuleDataAuthorizeSchemeEntity>? resourceEntity = moduleResourceEntity.FindAll(m => m.ModuleId == ids);
            if (resourceEntity.Count != 0)
            {
                moduleIds.Add(ids);
                List<AuthorizeDataModelOutput>? entity = resourceEntity.Adapt<List<AuthorizeDataModelOutput>>();

                entity.ForEach(e => e.ParentId = ids);
                authorizeDataResourceList = authorizeDataResourceList.Union(entity).ToList();
            }
        });
        List<AuthorizeDataModelOutput>? authorizeDataModuleList = new List<AuthorizeDataModelOutput>();
        GetParentsModuleList(moduleIds, moduleList, ref authorizeDataModuleList);
        List<AuthorizeDataModelOutput>? list = authorizeDataModuleList.Union(authorizeDataResourceList).ToList();
        GetOutPutResult(ref output, list, checkList);
        return output;
    }

    /// <summary>
    /// 强制角色下的所有用户下线.
    /// </summary>
    /// <param name="roleId">角色Id.</param>
    /// <returns></returns>
    private async Task ForcedOffline(List<string> roleId)
    {
        // 查找该角色下的所有成员id
        var roleUserIds = await _authorizeRepository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Role" && roleId.Contains(x.ObjectId)).Select(x => x.UserId).ToListAsync();
        roleUserIds.ForEach(async id =>
        {
            var tenantId = _userManager.TenantId;
            var list = await GetOnlineUserList(tenantId);
            var user = list?.Find(it => it.tenantId == tenantId && it.userId == id);
            if (user != null)
            {
                await _imHandler.SendMessageAsync(user.connectionId, new { method = "logout", msg = "权限已变更，请重新登录!" }.ToJsonString());
                await DelOnlineUser(tenantId, user.userId);
                await DelUserInfo(tenantId, user.userId);
            }
        });
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 当前用户模块权限.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="isAdmin">是否超管.</param>
    /// <param name="roleIds">用户角色Ids.</param>
    /// <param name="systemIds">当前系统Ids .</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleEntity>> GetCurrentUserModuleAuthorize(string userId, bool isAdmin, string[] roleIds, string[] systemIds)
    {
        List<ModuleEntity>? output = new List<ModuleEntity>();
        if (!isAdmin)
        {
            var items = await _authorizeRepository.AsQueryable().Where(a => a.ItemType == "module" && roleIds.Contains(a.ObjectId)).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return output;
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleEntity>()
                .Where(a => items.Select(it => it.ItemId).ToArray().Contains(a.Id) && a.EnabledMark == 1 && a.DeleteMark == null)
                .Where(a => systemIds.Contains(a.SystemId)).OrderBy(o => o.SortCode).ToListAsync();
        }
        else
        {
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleEntity>().Where(a => a.EnabledMark == 1 && a.DeleteMark == null)
                .Where(a => systemIds.Contains(a.SystemId)).OrderBy(o => o.SortCode).ToListAsync();
        }

        return output;
    }

    /// <summary>
    /// 当前用户模块按钮权限.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="isAdmin">是否超管.</param>
    /// <param name="roleIds">用户角色Ids.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleButtonEntity>> GetCurrentUserButtonAuthorize(string userId, bool isAdmin, string[] roleIds)
    {
        List<ModuleButtonEntity>? output = new List<ModuleButtonEntity>();
        if (!isAdmin)
        {
            var items = await _authorizeRepository.AsQueryable().Where(a => a.ItemType == "button" && roleIds.Contains(a.ObjectId)).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return output;
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleButtonEntity>().Where(a => items.Select(it => it.ItemId).ToArray().Contains(a.Id) && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ParentId.Equals("-1") ? a.ModuleId : a.ParentId;
                }).ToListAsync();
        }
        else
        {
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleButtonEntity>().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ParentId.Equals("-1") ? a.ModuleId : a.ParentId;
                }).ToListAsync();
        }

        return output;
    }

    /// <summary>
    /// 当前用户模块列权限.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="isAdmin">是否超管.</param>
    /// <param name="roleIds">用户角色Ids.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleColumnEntity>> GetCurrentUserColumnAuthorize(string userId, bool isAdmin, string[] roleIds)
    {
        List<ModuleColumnEntity>? output = new List<ModuleColumnEntity>();
        if (!isAdmin)
        {
            var items = await _authorizeRepository.AsQueryable().Where(a => a.ItemType == "column" && roleIds.Contains(a.ObjectId)).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return output;
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleColumnEntity>().Where(a => items.Select(it => it.ItemId).ToArray().Contains(a.Id) && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ModuleId;
                }).ToListAsync();
        }
        else
        {
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleColumnEntity>().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ModuleId;
                }).ToListAsync();
        }

        return output;
    }

    /// <summary>
    /// 当前用户模块表单权限.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="isAdmin">是否超管.</param>
    /// <param name="roleIds">用户角色Ids.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleFormEntity>> GetCurrentUserFormAuthorize(string userId, bool isAdmin, string[] roleIds)
    {
        List<ModuleFormEntity>? output = new List<ModuleFormEntity>();
        if (!isAdmin)
        {
            var items = await _authorizeRepository.AsQueryable().Where(a => a.ItemType == "form" && roleIds.Contains(a.ObjectId)).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return output;
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleFormEntity>().Where(a => items.Select(it => it.ItemId).ToArray().Contains(a.Id) && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ModuleId;
                }).ToListAsync();
        }
        else
        {
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleFormEntity>().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode)
                .Mapper(a =>
                {
                    a.ParentId = a.ModuleId;
                }).ToListAsync();
        }

        return output;
    }

    /// <summary>
    /// 当前用户模块权限资源.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="isAdmin">是否超管.</param>
    /// <param name="roleIds">用户角色Ids.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleDataAuthorizeSchemeEntity>> GetCurrentUserResourceAuthorize(string userId, bool isAdmin, string[] roleIds)
    {
        List<ModuleDataAuthorizeSchemeEntity>? output = new List<ModuleDataAuthorizeSchemeEntity>();
        if (!isAdmin)
        {
            var items = await _authorizeRepository.AsQueryable().Where(a => a.ItemType == "resource" && roleIds.Contains(a.ObjectId)).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
            if (items.Count == 0) return output;
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().Where(a => items.Select(it => it.ItemId).ToArray().Contains(a.Id) && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
        }
        else
        {
            output = await _authorizeRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
        }

        return output;
    }

    /// <summary>
    /// 获取权限项ids.
    /// </summary>
    /// <param name="roleId">角色id.</param>
    /// <param name="itemType">项类型.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<string>> GetAuthorizeItemIds(string roleId, string itemType)
    {
        var data = await _authorizeRepository.AsQueryable().Where(a => a.ObjectId == roleId && a.ItemType == itemType).GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
        return data.Select(it => it.ItemId).ToList();
    }

    /// <summary>
    /// 是否存在权限资源.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetIsExistModuleDataAuthorizeScheme(string[] ids)
    {
        return await _authorizeRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().AnyAsync(m => ids.Contains(m.Id) && m.DeleteMark == null);
    }

    /// <summary>
    /// 获取权限列表.
    /// </summary>
    /// <param name="objectId">对象主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<AuthorizeEntity>> GetAuthorizeListByObjectId(string objectId)
    {
        return await _authorizeRepository.AsQueryable().Where(a => a.ObjectId == objectId).ToListAsync();
    }

    /// <summary>
    /// 处理菜单+系统.
    /// </summary>
    /// <param name="moduleList"></param>
    /// <param name="systemList"></param>
    /// <returns></returns>
    public async Task<List<ModuleEntity>> GetModuleAndSystemScheme(List<ModuleEntity> moduleList, List<ModuleEntity> systemList)
    {
        var moduleSystemList = systemList.Where(x => x.SystemId != null && x.SystemId.Equals("-1")).ToList();

        moduleSystemList.ForEach(item =>
        {
            if (moduleList.Any(it => it.Category != null && it.Category.Equals("App") && it.SystemId.Equals(item.Id)))
            {
                var appMenu = systemList.FirstOrDefault(it => it.ParentId.Equals(item.Id) && it.Category.Equals("App"));
                moduleList.Where(it => it.Category != null && it.Category.Equals("App") && it.ParentId.Equals("-1") && it.SystemId.Equals(item.Id)).ToList().ForEach(it =>
                {
                    it.ParentId = appMenu.Id;
                });
                moduleList.Add(appMenu);
            }

            moduleList.Where(it => it.Category != null && it.Category.Equals("Web") && it.SystemId.Equals(item.Id) && it.ParentId.Equals("-1")).ToList().ForEach(it =>
            {
                it.ParentId = item.Id;
            });
        });

        moduleList.Where(x => x.ParentId.Equals("-1")).ToList().ForEach(item => item.ParentId = item.SystemId);
        moduleList.AddRange(moduleSystemList);

        return moduleList;
    }

    /// <summary>
    /// 处理app菜单 勾选问题，返回最终结果.
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public AuthorizeDataOutput GetResult(AuthorizeDataOutput output)
    {
        if (output.list.Any())
        {
            output.list.ForEach(item =>
            {
                var appItem = item.Children?.Adapt<List<AuthorizeDataModelOutput>>().FirstOrDefault(x => !output.ids.Contains(x.Id) && x.fullName.Equals("app菜单"));
                if (appItem != null)
                {
                    foreach (var it in output.ids)
                    {
                        if (appItem.ToJsonString().Contains(it))
                        {
                            output.ids.Add(appItem.Id);
                            break;
                        }
                    }
                }
            });
        }

        return output;
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <returns></returns>
    public async Task<List<UserOnlineModel>> GetOnlineUserList(string tenantId)
    {
        var cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 删除在线用户ID.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private async Task<bool> DelOnlineUser(string tenantId, string userId)
    {
        var cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        var list = await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
        var online = list.Find(it => it.userId == userId);
        list.RemoveAll((x) => x.connectionId == online.connectionId);
        return await _cacheManager.SetAsync(cacheKey, list);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private async Task<bool> DelUserInfo(string tenantId, string userId)
    {
        var cacheKey = string.Format("{0}:{1}:{2}", tenantId, CommonConst.CACHEKEYUSER, userId);
        return await _cacheManager.DelAsync(cacheKey);
    }

    #endregion
}