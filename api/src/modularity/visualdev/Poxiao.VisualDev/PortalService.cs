using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Dto.System.Portal;
using Poxiao.Systems.Entitys.Entity.System;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Entitys;
using Poxiao.VisualDev.Entitys.Dto.Portal;
using Poxiao.VisualDev.Entitys.Entity;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.VisualDev;

/// <summary>
///  业务实现：门户设计.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Portal", Order = 173)]
[Route("api/visualdev/[controller]")]
public class PortalService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PortalEntity> _portalRepository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个<see cref="PortalService"/>类型的新实例.
    /// </summary>
    public PortalService(
        ISqlSugarRepository<PortalEntity> portalRepository,
        IUserManager userManager,
        IFileManager fileManager)
    {
        _portalRepository = portalRepository;
        _userManager = userManager;
        _fileManager = fileManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns>返回列表.</returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PortalListQueryInput input)
    {
        var portalList = await _portalRepository.AsQueryable()
           .WhereIF(input.Keyword.IsNotEmptyOrNull(), p => p.FullName.Contains(input.Keyword) || p.EnCode.Contains(input.Keyword))
           .WhereIF(input.category.IsNotEmptyOrNull(), p => p.Category == input.category)
           .WhereIF(input.type.IsNotEmptyOrNull(), p => p.Type == input.type)
           .WhereIF(input.enabledLock.IsNotEmptyOrNull(), p => p.EnabledLock == input.enabledLock)
           .Where(p => p.DeleteMark == null)
           .OrderBy(p => p.SortCode)
           .OrderBy(p => p.CreatorTime, OrderByType.Desc)
           .OrderBy(p => p.LastModifyTime, OrderByType.Desc)
           .Select(p => new PortalListOutput
           {
               Id = p.Id,
               fullName = p.FullName,
               enCode = p.EnCode,
               deleteMark = SqlFunc.ToString(p.DeleteMark),
               description = p.Description,
               category = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(it => it.Id.Equals(p.Category)).Select(it => it.FullName),
               creatorTime = p.CreatorTime,
               creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(it => it.Id.Equals(p.CreatorUserId)).Select(it => SqlFunc.MergeString(it.RealName, "/", it.Account)),
               ParentId = p.Category,
               //lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(it => it.Id.Equals(p.LastModifyUserId)).Select(it => SqlFunc.MergeString(it.RealName, SqlFunc.IIF(it.RealName == null, string.Empty, "/"), it.Account)),
               lastModifyTime = SqlFunc.ToDate(p.LastModifyTime),
               enabledMark = p.EnabledMark,
               enabledLock = p.EnabledLock,
               type = p.Type,
               sortCode = SqlFunc.ToString(p.SortCode),
               pcIsRelease = SqlFunc.Subqueryable<PortalManageEntity>().Where(it => it.DeleteMark == null && it.PortalId.Equals(p.Id) && it.Platform.Equals("Web")).Any() ? 1 : 0,
               appIsRelease = SqlFunc.Subqueryable<PortalManageEntity>().Where(it => it.DeleteMark == null && it.PortalId.Equals(p.Id) && it.Platform.Equals("App")).Any() ? 1 : 0
           })
           .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<PortalListOutput>.SqlSugarPageResult(portalList);
    }

    /// <summary>
    /// 获取门户侧边框列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] string platform)
    {
        List<PortalSelectOutput>? data = new List<PortalSelectOutput>();
        if (!_userManager.IsAdministrator)
        {
            List<string>? roleId = await _portalRepository.AsSugarClient().Queryable<RoleEntity>().In(r => r.Id, _userManager.Roles).Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(r => r.Id).ToListAsync();
            var items = await _portalRepository.AsSugarClient().Queryable<AuthorizeEntity>().Where(a => roleId.Contains(a.ObjectId) && a.ItemType == "portalManage").GroupBy(it => it.ItemId).Select(it => it.ItemId).ToListAsync();
            if (items.Any())
            {
                data = await _portalRepository.AsSugarClient().Queryable<PortalEntity, PortalManageEntity>((p, pm) => new JoinQueryInfos(JoinType.Left, p.Id == pm.PortalId))
                    .In((p, pm) => pm.Id, items.ToArray())
                    .Where((p, pm) => p.EnabledMark == 1 && p.DeleteMark == null && pm.EnabledMark == 1 && pm.DeleteMark == null)
                    .Where((p, pm) => pm.Platform.Equals(platform))
                    .WhereIF(platform.Equals("Web"), (p, pm) => pm.SystemId.Equals(_userManager.User.SystemId))
                    .WhereIF(platform.Equals("App"), (p, pm) => pm.SystemId.Equals(_userManager.User.AppSystemId))
                    .OrderBy((p, pm) => pm.SortCode)
                    .OrderBy((p, pm) => pm.CreatorTime, OrderByType.Desc)
                    .OrderBy((p, pm) => pm.LastModifyTime, OrderByType.Desc)
                    .Select(p => new PortalSelectOutput
                    {
                        Id = p.Id,
                        fullName = p.FullName,
                        ParentId = p.Category,
                    }).ToListAsync();
            }
        }
        else
        {
            data = await _portalRepository.AsSugarClient().Queryable<PortalEntity, PortalManageEntity>((p, pm) => new JoinQueryInfos(JoinType.Left, p.Id == pm.PortalId))
                .Where((p, pm) => p.EnabledMark == 1 && p.DeleteMark == null && pm.EnabledMark == 1 && pm.DeleteMark == null)
                .Where((p, pm) => pm.Platform.Equals(platform))
                .WhereIF(platform.Equals("Web"), (p, pm) => pm.SystemId.Equals(_userManager.User.SystemId))
                .WhereIF(platform.Equals("App"), (p, pm) => pm.SystemId.Equals(_userManager.User.AppSystemId))
                .OrderBy((p, pm) => pm.SortCode)
                .OrderBy((p, pm) => pm.CreatorTime, OrderByType.Desc)
                .OrderBy((p, pm) => pm.LastModifyTime, OrderByType.Desc)
                .Select(p => new PortalSelectOutput
                {
                    Id = p.Id,
                    fullName = p.FullName,
                    ParentId = p.Category,
                }).ToListAsync();
        }

        List<string>? parentIds = data.Select(it => it.ParentId).Distinct().ToList();
        List<PortalSelectOutput>? treeList = new List<PortalSelectOutput>();
        if (parentIds.Any())
        {
            treeList = await _portalRepository.AsSugarClient().Queryable<DictionaryDataEntity>().In(it => it.Id, parentIds.ToArray())
                .Where(d => d.DeleteMark == null && d.EnabledMark == 1)
                .OrderBy(o => o.SortCode)
                .Select(d => new PortalSelectOutput
                {
                    Id = d.Id,
                    ParentId = "0",
                    fullName = d.FullName
                }).ToListAsync();
        }

        return new { list = treeList.Union(data).ToList().ToTree("0") };
    }

    /// <summary>
    /// 获取门户信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var portalModData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.Type.Equals("model") && it.PortalId.Equals(id))
            .Select(it => it.FormData)
            .FirstAsync();

        var data = await _portalRepository.AsQueryable()
            .Where(it => it.DeleteMark == null && it.Id == id)
            .Select(it => new PortalInfoOutput()
            {
                id = it.Id,
                category = it.Category,
                customUrl = it.CustomUrl,
                //appCustomUrl = it.AppCustomUrl,
                description = it.Description,
                enCode = it.EnCode,
                enabledLock = it.EnabledLock,
                enabledMark = it.EnabledMark,
                fullName = it.FullName,
                linkType = it.LinkType,
                sortCode = it.SortCode,
                type = it.Type,
                formData = portalModData,
                pcIsRelease = SqlFunc.Subqueryable<PortalManageEntity>().Where(s => s.DeleteMark == null && s.PortalId.Equals(it.Id) && s.Platform.Equals("Web")).Any() ? 1 : 0,
                appIsRelease = SqlFunc.Subqueryable<PortalManageEntity>().Where(s => s.DeleteMark == null && s.PortalId.Equals(it.Id) && s.Platform.Equals("App")).Any() ? 1 : 0
            })
            .FirstAsync();

        return data;
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/auth")]
    public async Task<dynamic> GetInfoAuth(string id, [FromQuery] PortalAuthInput input)
    {
        var systemId = input.platform.Equals("App") ? _userManager.User.AppSystemId : _userManager.User.SystemId;
        if (_userManager.Roles != null && !_userManager.IsAdministrator)
        {
            List<string>? roleId = await _portalRepository.AsSugarClient().Queryable<RoleEntity>()
                .Where(r => _userManager.Roles.Contains(r.Id))
                .Where(r => r.EnabledMark == 1 && r.DeleteMark == null)
                .Select(r => r.Id)
                .ToListAsync();
            var items = await _portalRepository.AsSugarClient().Queryable<AuthorizeEntity>()
                .Where(a => roleId.Contains(a.ObjectId))
                .Where(a => a.ItemType == "portalManage")
                .GroupBy(it => it.ItemId)
                .Select(it => it.ItemId)
                .ToListAsync();
            if (items.Count == 0) return null;

            var portalIdList = await _portalRepository.AsSugarClient().Queryable<PortalManageEntity>()
                .Where(it => it.DeleteMark == null && it.EnabledMark == 1 && items.Contains(it.Id) && it.SystemId.Equals(systemId))
                .Select(it => it.PortalId)
                .ToListAsync();
            if (portalIdList.Contains(id))
            {
                // 判断子门户是否存在
                if (!await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>().AnyAsync(it => it.DeleteMark == null && it.Platform.Equals(input.platform) && it.Type.Equals("release") && it.SystemId.Equals(systemId) && it.PortalId.Equals(id) && it.CreatorUserId.Equals(_userManager.UserId)))
                {
                    var data = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                        .Where(it => it.DeleteMark == null && it.Type.Equals("release") && it.PortalId.Equals(id) && it.SystemId == null)
                        .FirstAsync();
                    if (data.IsNotEmptyOrNull())
                    {
                        data.Id = SnowflakeIdHelper.NextId();
                        data.Platform = input.platform;
                        data.SystemId = systemId;
                        data.CreatorTime = DateTime.Now;
                        data.CreatorUserId = _userManager.UserId;
                        await _portalRepository.AsSugarClient().Insertable(data).ExecuteCommandAsync();
                    }
                    else
                    {
                        return new PortalInfoAuthOutput();
                    }
                }

                var entity = await _portalRepository.AsQueryable()
                    .Where(it => it.EnabledMark == 1 && it.DeleteMark == null && it.Id.Equals(id))
                    .Select<PortalInfoAuthOutput>()
                    .FirstAsync();
                if (entity.IsNotEmptyOrNull())
                {
                    entity.formData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                        .Where(it => it.DeleteMark == null && it.CreatorUserId.Equals(_userManager.UserId) && it.Platform.Equals(input.platform) && it.Type.Equals("release") && it.PortalId.Equals(id) && it.SystemId.Equals(systemId))
                        .Select(it => it.FormData)
                        .FirstAsync();
                    return entity;
                }
                else
                {
                    return new PortalInfoAuthOutput();
                }
            }
        }
        else if (_userManager.IsAdministrator)
        {
            // 判断子门户是否存在
            if (!await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>().AnyAsync(it => it.DeleteMark == null && it.Platform.Equals(input.platform) && it.Type.Equals("release") && it.SystemId.Equals(systemId) && it.PortalId.Equals(id) && it.CreatorUserId.Equals(_userManager.UserId)))
            {
                var data = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                    .Where(it => it.DeleteMark == null && it.Type.Equals("release") && it.PortalId.Equals(id) && it.SystemId == null)
                    .FirstAsync();
                if (data.IsNotEmptyOrNull())
                {
                    data.Id = SnowflakeIdHelper.NextId();
                    data.Platform = input.platform;
                    data.SystemId = systemId;
                    data.CreatorTime = DateTime.Now;
                    data.CreatorUserId = _userManager.UserId;
                    await _portalRepository.AsSugarClient().Insertable(data).ExecuteCommandAsync();
                }
                else
                {
                    return new PortalInfoAuthOutput();
                }
            }

            var entity = await _portalRepository.AsQueryable()
                .Where(it => it.EnabledMark == 1 && it.DeleteMark == null && it.Id.Equals(id))
                .Select<PortalInfoAuthOutput>()
                .FirstAsync();
            if (entity.IsNotEmptyOrNull())
            {
                entity.formData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                    .Where(it => it.DeleteMark == null && it.CreatorUserId.Equals(_userManager.UserId) && it.Platform.Equals(input.platform) && it.Type.Equals("release") && it.PortalId.Equals(id) && it.SystemId.Equals(systemId))
                    .Select(it => it.FormData)
                    .FirstAsync();
                return entity;
            }
            else
            {
                return new PortalInfoAuthOutput();
            }
        }

        throw Oops.Oh(ErrorCode.D1900);
    }

    /// <summary>
    /// 门户选择.
    /// </summary>
    /// <param name="systemId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("manage/Selector/{systemId}")]
    public async Task<dynamic> GetManageSelector(string systemId, [FromQuery] PortalManageInput input)
    {
        // 系统已添加的门户
        var portalManageIdList = await _portalRepository.AsSugarClient().Queryable<PortalManageEntity>()
            .Where(it => it.DeleteMark == null && it.SystemId.Equals(systemId) && it.Platform.Equals(input.platform))
            .Select(it => it.PortalId)
            .ToListAsync();

        var portalList = await _portalRepository.AsSugarClient().Queryable<PortalEntity, DictionaryDataEntity>((p, d) => new JoinQueryInfos(JoinType.Left, p.Category == d.Id))
            .Where(p => p.EnabledMark == 1 && p.DeleteMark == null && !portalManageIdList.Contains(p.Id))
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), p => p.FullName.Contains(input.Keyword) || p.EnCode.Contains(input.Keyword))
            .OrderBy(p => p.SortCode)
            .OrderBy(p => p.CreatorTime, OrderByType.Desc)
            .OrderBy(p => p.LastModifyTime, OrderByType.Desc)
            .Select((p, d) => new PortalManageOutput
            {
                id = p.Id,
                fullName = p.FullName,
                enCode = p.EnCode,
                enabledMark = p.EnabledMark,
                type = p.Type,
                sortCode = p.SortCode,
                category = d.FullName,
                categoryId = p.Category,
                categoryName = d.FullName
            })
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<PortalManageOutput>.SqlSugarPageResult(portalList);
    }

    #endregion

    #region Post

    /// <summary>
    /// 门户导出.
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    [HttpPost("{modelId}/Actions/ExportData")]
    public async Task<dynamic> ActionsExportData(string modelId)
    {
        // 模板实体
        var templateEntity = await _portalRepository.AsQueryable()
            .Where(it => it.Id == modelId)
            .Select<PortalExportOutput>()
            .FirstAsync();
        templateEntity.formData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.PortalId.Equals(modelId) && it.Type.Equals("model"))
            .Select(it => it.FormData)
            .FirstAsync();

        string? jsonStr = templateEntity.ToJsonString();
        return await _fileManager.Export(jsonStr, templateEntity.fullName, ExportFileType.vp);
    }

    /// <summary>
    /// 门户导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Model/Actions/ImportData")]
    public async Task ActionsImportData(IFormFile file)
    {
        string? fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.vp.ToString())) throw Oops.Oh(ErrorCode.D3006);
        string? josn = _fileManager.Import(file);
        PortalEntity? templateEntity = null;
        PortalDataEntity dataEntity = new PortalDataEntity();
        try
        {
            var data = josn.ToObject<PortalExportOutput>();
            if (data.id.IsNullOrEmpty())
            {
                data.id = SnowflakeIdHelper.NextId();
            }

            dataEntity.Id = SnowflakeIdHelper.NextId();
            dataEntity.FormData = data.formData;
            dataEntity.PortalId = data.id;
            dataEntity.Type = "model";
            dataEntity.CreatorTime = DateTime.Now;
            dataEntity.CreatorUserId = _userManager.UserId;

            templateEntity = data.Adapt<PortalEntity>();
            templateEntity.SortCode = 0;
            templateEntity.CreatorTime = DateTime.Now;
            templateEntity.CreatorUserId = _userManager.UserId;
        }
        catch
        {
            throw Oops.Oh(ErrorCode.D3006);
        }

        if (templateEntity == null) throw Oops.Oh(ErrorCode.D3006);
        if (templateEntity != null && dataEntity.FormData.IsNotEmptyOrNull() && dataEntity.FormData.IndexOf("layoutId") <= 0)
            throw Oops.Oh(ErrorCode.D3006);
        if (templateEntity.Id.IsNotEmptyOrNull() && await _portalRepository.IsAnyAsync(it => it.Id == templateEntity.Id && it.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1400);
        if (await _portalRepository.IsAnyAsync(it => it.FullName == templateEntity.FullName && it.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1915);
        if (await _portalRepository.IsAnyAsync(it => it.EnCode == templateEntity.EnCode && it.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1916);

        StorageableResult<PortalEntity>? stor = _portalRepository.AsSugarClient().Storageable(templateEntity).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
        await stor.AsInsertable.ExecuteCommandAsync(); // 执行插入
        await stor.AsUpdateable.ExecuteCommandAsync(); // 执行更新

        StorageableResult<PortalDataEntity>? storData = _portalRepository.AsSugarClient().Storageable(dataEntity).WhereColumns(it => new { it.PortalId, it.Type }).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
        await storData.AsInsertable.ExecuteCommandAsync(); // 执行插入
        await storData.AsUpdateable.ExecuteCommandAsync(); // 执行更新
    }

    /// <summary>
    /// 新建门户信息.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<string> Create([FromBody] PortalCrInput input)
    {
        var entity = input.Adapt<PortalEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.CreatorTime = DateTime.Now;
        entity.CreatorUserId = _userManager.UserId;
        if (entity.Type.Equals(1))
        {
            entity.EnabledLock = null;
        }

        if (string.IsNullOrEmpty(entity.Category)) throw Oops.Oh(ErrorCode.D1901);
        else if (string.IsNullOrEmpty(entity.FullName)) throw Oops.Oh(ErrorCode.D1902);
        else if (string.IsNullOrEmpty(entity.EnCode)) throw Oops.Oh(ErrorCode.D1903);
        else if (await _portalRepository.AsQueryable().Where(it => it.DeleteMark == null && it.FullName.Equals(input.fullName)).AnyAsync())
            throw Oops.Oh(ErrorCode.D1915);
        else if (await _portalRepository.AsQueryable().Where(it => it.DeleteMark == null && it.EnCode.Equals(input.enCode)).AnyAsync())
            throw Oops.Oh(ErrorCode.D1916);
        else await _portalRepository.AsInsertable(entity).ExecuteCommandAsync();

        var dataEntity = new PortalDataEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            PortalId = entity.Id,
            FormData = input.formData,
            CreatorTime = DateTime.Now,
            CreatorUserId = _userManager.UserId,
            Type = "model"
        };
        var isOk = await _portalRepository.AsSugarClient().Insertable(dataEntity).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1000);

        // 确定并设计
        return entity.Id;
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] PortalUpInput input)
    {
        var entity = input.Adapt<PortalEntity>();
        if (await _portalRepository.AsQueryable().Where(it => it.DeleteMark == null && !it.Id.Equals(id) && it.FullName.Equals(input.fullName)).AnyAsync())
            throw Oops.Oh(ErrorCode.D1915);
        else if (await _portalRepository.AsQueryable().Where(it => it.DeleteMark == null && !it.Id.Equals(id) && it.EnCode.Equals(input.enCode)).AnyAsync())
            throw Oops.Oh(ErrorCode.D1916);
        if (entity.Type.Equals(1))
        {
            entity.EnabledLock = null;
        }

        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;

        if (input.fullName.IsNotEmptyOrNull())
        {
            await _portalRepository.AsSugarClient().Updateable(entity)
                .UpdateColumns(it => new {
                    it.CustomUrl,
                    //it.AppCustomUrl,
                    it.EnCode,
                    it.LinkType,
                    it.SortCode,
                    it.FullName,
                    it.Description,
                    it.Type,
                    it.Category,
                    it.EnabledMark,
                    it.EnabledLock,
                    it.LastModifyTime,
                    it.LastModifyUserId
                }).ExecuteCommandAsync();
        }
        else
        {
            await _portalRepository.AsSugarClient().Updateable(entity)
                .UpdateColumns(it => new {
                    it.LastModifyTime,
                    it.LastModifyUserId
                }).ExecuteCommandAsync();
        }

        int isOk = await _portalRepository.AsSugarClient().Updateable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.PortalId.Equals(id) && it.Type.Equals("model"))
            .SetColumns(it => new PortalDataEntity()
            {
                FormData = input.formData,
                LastModifyTime = SqlFunc.GetDate(),
                LastModifyUserId = _userManager.UserId
            })
            .ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _portalRepository.AsQueryable()
            .Where(it => it.Id == id && it.DeleteMark == null)
            .FirstAsync();
        _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);

        var entityLink = await _portalRepository.AsSugarClient().Queryable<PortalManageEntity>()
            .Where(it => it.DeleteMark == null && it.PortalId.Equals(id))
            .FirstAsync();
        if (entityLink.IsNotEmptyOrNull())
        {
            var systemName = await _portalRepository.AsSugarClient().Queryable<SystemEntity>()
                .Where(it => it.DeleteMark == null && it.Id.Equals(entityLink.SystemId))
                .Select(it => it.FullName)
                .FirstAsync();
            throw Oops.Oh(ErrorCode.D1917, systemName);
        }

        entity.DeleteMark = 1;
        entity.DeleteTime = DateTime.Now;
        entity.DeleteUserId = _userManager.UserId;
        await _portalRepository.AsSugarClient().Updateable(entity).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

        var isOk = await _portalRepository.AsSugarClient().Updateable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.PortalId.Equals(id))
            .SetColumns(it => new PortalDataEntity()
            {
                DeleteMark = 1,
                DeleteTime = SqlFunc.GetDate(),
                DeleteUserId = _userManager.UserId
            })
            .ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        string? random = new Random().NextLetterAndNumberString(5);
        var entity = await _portalRepository.AsQueryable()
            .Where(it => it.Id == id && it.DeleteMark == null)
            .FirstAsync();
        var newEntity = new PortalEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            CreatorTime = DateTime.Now,
            CreatorUserId = _userManager.UserId,
            FullName = entity.FullName + ".副本" + random,
            EnCode = entity.EnCode + random,
            Category = entity.Category,
            Description = entity.Description,
            EnabledMark = 0,
            SortCode = entity.SortCode,
            Type = entity.Type,
            LinkType = entity.LinkType,
            CustomUrl = entity.CustomUrl,
            //AppCustomUrl = entity.AppCustomUrl,
        };

        var dataEntity = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.PortalId.Equals(id) && it.Type.Equals("model"))
            .FirstAsync();
        var newDataEntity = new PortalDataEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            CreatorTime = DateTime.Now,
            CreatorUserId = _userManager.UserId,
            PortalId = newEntity.Id,
            FormData = dataEntity.FormData,
            Type = "model"
        };

        try
        {
            await _portalRepository.AsSugarClient().Insertable(newEntity).ExecuteCommandAsync();
            await _portalRepository.AsSugarClient().Insertable(newDataEntity).ExecuteCommandAsync();
        }
        catch
        {
            if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
            else throw;
        }
    }

    /// <summary>
    /// 设置默认门户.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="platform"></param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/SetDefault")]
    public async Task SetDefault(string id, string platform)
    {
        var portalDic = (await _portalRepository.AsSugarClient().Queryable<UserEntity>()
            .Where(it => it.Id.Equals(_userManager.UserId))
            .Select(it => it.PortalId)
            .FirstAsync())
            .ToObject<Dictionary<string, string>>();

        var key = string.Format("{0}:{1}", platform, _userManager.User.SystemId);
        if (portalDic.ContainsKey(key))
        {
            portalDic[key] = id;
        }
        else
        {
            portalDic.Add(key, id);
        }

        var portalId = portalDic.ToJsonString();
        var isOk = await _portalRepository.AsSugarClient().Updateable<UserEntity>()
            .Where(it => it.Id.Equals(_userManager.UserId))
            .SetColumns(it => new UserEntity()
            {
                PortalId = portalId,
                LastModifyTime = SqlFunc.GetDate(),
                LastModifyUserId = _userManager.UserId
            })
            .ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D5014);
    }

    /// <summary>
    /// 实时保存门户.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Custom/Save/{id}")]
    public async Task SavePortal(string id, [FromBody] PortalSaveInput input)
    {
        // 修改子门户的数据
        var isOk = await _portalRepository.AsSugarClient().Updateable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.Platform == "Web" && it.Type.Equals("release") && it.PortalId.Equals(id) && it.SystemId.Equals(_userManager.User.SystemId) && it.CreatorUserId.Equals(_userManager.UserId))
            .SetColumns(it => new PortalDataEntity()
            {
                FormData = input.formData,
                LastModifyTime = SqlFunc.GetDate(),
                LastModifyUserId = _userManager.UserId
            })
            .ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D1906);
    }

    /// <summary>
    /// 同步门户.
    /// </summary>
    /// <param name="portalId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Actions/release/{portalId}")]
    public async Task SyncPortal(string portalId, [FromBody] PortalSyncInput input)
    {
        List<PortalDataEntity> portalDataList = new List<PortalDataEntity>();
        List<PortalManageEntity> portalManageList = new List<PortalManageEntity>();

        // 主门户数据
        var modelData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.Type.Equals("model") && it.PortalId.Equals(portalId))
            .FirstAsync();

        // 发布门户数据
        var releaseData = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
            .Where(it => it.DeleteMark == null && it.Type.Equals("release") && it.PortalId.Equals(portalId))
            .FirstAsync();
        if (releaseData.IsNotEmptyOrNull())
        {
            releaseData.FormData = modelData.FormData;
            releaseData.LastModifyTime = DateTime.Now;
            releaseData.LastModifyUserId = _userManager.UserId;
            await _portalRepository.AsSugarClient().Updateable(releaseData)
                .UpdateColumns(it => new { it.FormData, it.LastModifyTime, it.LastModifyUserId })
                .ExecuteCommandAsync();
        }
        else
        {
            var newReleaseData = modelData.Adapt<PortalDataEntity>();
            newReleaseData.Id = SnowflakeIdHelper.NextId();
            newReleaseData.Type = "release";
            await _portalRepository.AsSugarClient().Insertable(newReleaseData).ExecuteCommandAsync();
        }

        if (input.pc == 1)
        {
            if (input.pcSystemId.IsNullOrEmpty())
            {

                var dataList = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                    .Where(it => it.DeleteMark == null && it.PortalId.Equals(portalId) && it.Platform.Equals("web") && it.Type.Equals("release"))
                    .ToListAsync();

                dataList.ForEach(item =>
                {
                    item.FormData = modelData.FormData;
                    item.LastModifyTime = DateTime.Now;
                    item.LastModifyUserId = _userManager.UserId;

                    portalDataList.Add(item);
                });
            }
            else
            {
                var pcSystemIdList = input.pcSystemId.Split(",").ToList();

                pcSystemIdList.ForEach(item =>
                {
                    portalManageList.Add(new PortalManageEntity()
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        PortalId = portalId,
                        SystemId = item,
                        EnabledMark = 1,
                        Platform = "Web",
                        CreatorTime = DateTime.Now,
                        CreatorUserId = _userManager.UserId,
                    });
                });
            }
        }

        if (input.app == 1)
        {
            if (input.appSystemId.IsNullOrEmpty())
            {
                var dataList = await _portalRepository.AsSugarClient().Queryable<PortalDataEntity>()
                    .Where(it => it.DeleteMark == null && it.PortalId.Equals(portalId) && it.Platform.Equals("app") && it.Type.Equals("release"))
                    .ToListAsync();

                dataList.ForEach(item =>
                {
                    item.FormData = modelData.FormData;
                    item.LastModifyTime = DateTime.Now;
                    item.LastModifyUserId = _userManager.UserId;

                    portalDataList.Add(item);
                });
            }
            else
            {
                var appSystemIdList = input.appSystemId.Split(",").ToList();

                appSystemIdList.ForEach(item =>
                {
                    portalManageList.Add(new PortalManageEntity()
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        PortalId = portalId,
                        SystemId = item,
                        EnabledMark = 1,
                        Platform = "App",
                        CreatorTime = DateTime.Now,
                        CreatorUserId = _userManager.UserId,
                    });
                });
            }
        }

        if (portalManageList.Count > 0)
        {
            var isOk = await _portalRepository.AsSugarClient().Insertable(portalManageList).ExecuteCommandAsync();
            if (!(isOk > 0))
                throw Oops.Oh(ErrorCode.D1907);
        }

        if (portalDataList.Count > 0)
        {
            var isOk = await _portalRepository.AsSugarClient().Updateable(portalDataList)
                .UpdateColumns(it => new {
                    it.FormData,
                    it.LastModifyTime,
                    it.LastModifyUserId
                }).ExecuteCommandAsync();
            if (!(isOk > 0))
                throw Oops.Oh(ErrorCode.D1907);
        }

    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取默认.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetDefault()
    {
        UserEntity? user = _userManager.User;
        if (!user.IsAdministrator.ParseToBool())
        {
            if (!string.IsNullOrEmpty(user.RoleId))
            {
                string[]? roleIds = user.RoleId.Split(',');
                List<string>? roleId = await _portalRepository.AsSugarClient().Queryable<RoleEntity>().Where(r => roleIds.Contains(r.Id)).Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(r => r.Id).ToListAsync();
                var items = await _portalRepository.AsSugarClient().Queryable<AuthorizeEntity>().Where(a => roleId.Contains(a.ObjectId)).Where(a => a.ItemType == "portal").GroupBy(it => new { it.ItemId }).Select(it => new { it.ItemId }).ToListAsync();
                if (items.Count == 0) return string.Empty;
                List<string>? portalList = await _portalRepository.AsQueryable().In(p => p.Id, items.Select(it => it.ItemId).ToArray()).Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(o => o.SortCode).Select(s => s.Id).ToListAsync();
                return portalList.FirstOrDefault();
            }

            return string.Empty;
        }
        else
        {
            List<string>? portalList = await _portalRepository.AsQueryable().Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(o => o.SortCode).Select(s => s.Id).ToListAsync();
            return portalList.FirstOrDefault();
        }
    }

    #endregion
}
