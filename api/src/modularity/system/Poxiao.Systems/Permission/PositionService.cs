using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Dto.Position;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 业务实现：岗位管理.
/// 版 本：V1.0.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021.06.07.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Position", Order = 162)]
[Route("api/Permission/[controller]")]
public class PositionService : IPositionService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PositionEntity> _repository;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="PositionService"/>类型的新实例.
    /// </summary>
    public PositionService(
        ISqlSugarRepository<PositionEntity> repository,
        IOrganizeService organizeService,
        ICacheManager cacheManager,
        IUserManager userManager)
    {
        _repository = repository;
        _organizeService = organizeService;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 获取列表 根据organizeId.
    /// </summary>
    /// <param name="organizeId">参数.</param>
    /// <returns></returns>
    [HttpGet("getList/{organizeId}")]
    public async Task<dynamic> GetListByOrganizeId(string organizeId)
    {
        List<string>? oid = new List<string>();
        if (!string.IsNullOrWhiteSpace(organizeId))
        {
            // 获取组织下的所有组织 id 集合
            List<OrganizeEntity>? oentity = await _repository.AsSugarClient().Queryable<OrganizeEntity>().ToChildListAsync(x => x.ParentId, organizeId);
            oid = oentity.Select(x => x.Id).ToList();
        }
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "PositionType" && x.DeleteMark == null);
        List<PositionListOutput>? data = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == dictionaryTypeEntity.Id))

            // 组织机构
            .WhereIF(!string.IsNullOrWhiteSpace(organizeId), a => oid.Contains(a.OrganizeId))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                Id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToListAsync();
        return data.OrderBy(x => x.sortCode).ToList();
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PositionListQuery input)
    {
        if (input.organizeId == "0") input.organizeId = _userManager.User.OrganizeId;

        // 获取分级管理组织
        var dataScope = _userManager.DataScope.Where(x => x.Select).Select(x => x.organizeId).Distinct().ToList();
        List<string>? childOrgIds = new List<string>();
        if (input.organizeId.IsNotEmptyOrNull())
        {
            childOrgIds.Add(input.organizeId);

            // 根据组织Id 获取所有子组织Id集合
            childOrgIds.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().ToChildList(x => x.ParentId, input.organizeId).Select(x => x.Id).ToList());
            childOrgIds = childOrgIds.Distinct().ToList();
        }
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "PositionType" && x.DeleteMark == null);
        var data = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == dictionaryTypeEntity.Id))

            // 组织机构
            .WhereIF(childOrgIds.Any(), a => childOrgIds.Contains(a.OrganizeId))
            .WhereIF(!_userManager.IsAdministrator, a => dataScope.Contains(a.OrganizeId))

            // 关键字（名称、编码）
            .WhereIF(!input.Keyword.IsNullOrEmpty(), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .Where(a => a.DeleteMark == null).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                Id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                organizeId = b.OrganizeIdTree,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);

        // 处理组织树 名称
        List<OrganizeEntity>? orgList = _organizeService.GetOrgListTreeName();

        #region 处理岗位所属组织树

        foreach (PositionListOutput? item in data.list)
        {
            // 获取用户组织集合
            item.department = orgList.Where(x => x.Id == item.organizeId.Split(",").LastOrDefault()).Select(x => x.Description).FirstOrDefault();
        }

        #endregion

        return PageResult<PositionListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<dynamic> GetList()
    {
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "PositionType" && x.DeleteMark == null);
        List<PositionListOutput>? data = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == dictionaryTypeEntity.Id))
            .Where(a => a.DeleteMark == null && a.EnabledMark == 1).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new PositionListOutput
            {
                Id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                type = c.FullName,
                department = b.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                description = a.Description,
                sortCode = a.SortCode
            }).ToListAsync();
        return new { list = data.OrderBy(x => x.sortCode).ToList() };
    }

    /// <summary>
    /// 获取下拉框（公司+部门+岗位）.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        var orgInfoList = _organizeService.GetOrgListTreeName();

        List<OrganizeEntity>? organizeList = await _organizeService.GetListAsync();
        List<PositionEntity>? positionList = await _repository.AsQueryable().Where(t => t.EnabledMark == 1 && t.DeleteMark == null)
            .OrderBy(o => o.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc).ToListAsync();
        List<PositionSelectorOutput>? treeList = new List<PositionSelectorOutput>();
        organizeList.ForEach(item =>
        {
            treeList.Add(
                new PositionSelectorOutput
                {
                    Id = item.Id,
                    ParentId = item.ParentId,
                    fullName = item.FullName,
                    enabledMark = item.EnabledMark,
                    icon = item.Category.Equals("department") ? "icon-ym icon-ym-tree-department1" : "icon-ym icon-ym-tree-organization3",
                    type = item.Category,
                    organize = orgInfoList.Find(x => x.Id.Equals(item.Id)).Description,
                    organizeIdTree = item.OrganizeIdTree,
                    Num = positionList.Count(x => x.OrganizeId.Equals(item.Id)),
                    sortCode = item.SortCode
                });
        });
        positionList.ForEach(item =>
        {
            treeList.Add(
                new PositionSelectorOutput
                {
                    Id = item.Id,
                    ParentId = item.OrganizeId,
                    fullName = item.FullName,
                    enabledMark = item.EnabledMark,
                    organize = orgInfoList.FirstOrDefault(x => x.Id.Equals(item.OrganizeId))?.Description,
                    icon = "icon-ym icon-ym-tree-position1",
                    type = "position",
                    Num = 1,
                    sortCode = -2
                });
        });

        treeList.Where(x => !x.type.Equals("position")).ToList().ForEach(item =>
        {
            if (treeList.Any(x => !x.type.Equals("position") && x.Num > 0 && x.organizeIdTree.Contains(item.organizeIdTree))) item.Num = 1;
            else item.Num = 0;
        });

        return new { list = treeList.Where(x => x.Num > 0).OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        PositionEntity? entity = await _repository.GetSingleAsync(p => p.Id == id);
        var res = entity.Adapt<PositionInfoOutput>();
        res.organizeIdTree = (await _organizeService.GetInfoById(entity.OrganizeId)).OrganizeIdTree.Split(",").ToList();
        return res;
    }

    #endregion

    #region POST

    /// <summary>
    /// 获取岗位列表 根据组织Id集合.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("getListByOrgIds")]
    public async Task<dynamic> GetListByOrgIds([FromBody] PositionListQuery input)
    {
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "PositionType" && x.DeleteMark == null);
        List<PositionListOutput>? data = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == dictionaryTypeEntity.Id))
            .Where(a => input.organizeIds.Contains(a.OrganizeId) && a.DeleteMark == null && a.EnabledMark == 1).OrderBy(a => a.SortCode)
            .Select((a, b, c) => new PositionListOutput
            {
                Id = a.Id,
                type = "position",
                ParentId = b.Id,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                sortCode = -2,
                IsLeaf = true
            }).ToListAsync();

        // 处理组织树 名称
        List<OrganizeEntity>? allOrgList = _organizeService.GetOrgListTreeName();
        List<PositionListOutput>? organizeList = allOrgList.Where(x => input.organizeIds.Contains(x.Id)).Select(x => new PositionListOutput()
        {
            Id = x.Id,
            type = x.Category,
            ParentId = "0",
            fullName = x.Description,
            Num = data.Count(x => x.ParentId == x.Id)
        }).ToList();

        return new { list = organizeList.Union(data).OrderBy(x => x.sortCode).ToList().ToTree("0") };
    }

    /// <summary>
    /// 通过部门、岗位id获取岗位列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("PositionCondition")]
    public async Task<dynamic> PositionCondition([FromBody] PositionConditionInput input)
    {
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "PositionType" && x.DeleteMark == null);
        List<PositionListOutput>? data = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity, DictionaryDataEntity>(
            (a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId, JoinType.Left, a.Type == c.EnCode && c.DictionaryTypeId == dictionaryTypeEntity.Id))
            .Where(a => a.DeleteMark == null)
            .Where(a => input.departIds.Contains(a.OrganizeId) || input.positionIds.Contains(a.Id))
            .WhereIF(input.Keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .Select((a, b, c) => new PositionListOutput
            {
                Id = a.Id,
                organizeId = a.OrganizeId,
                ParentId = b.Id,
                type = "position",
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                creatorTime = a.CreatorTime,
                icon = "icon-ym icon-ym-tree-position1",
                sortCode = -2,
                Num = 1,
                IsLeaf = true
            }).ToListAsync();

        // 获取所有组织
        List<OrganizeEntity>? allOrgList = _organizeService.GetOrgListTreeName();

        List<PositionListOutput>? organizeList = allOrgList.Where(x => data.Select(x => x.organizeId).Distinct().Contains(x.Id)).Select(x => new PositionListOutput()
        {
            Id = x.Id,
            type = x.Category,
            ParentId = x.ParentId.Equals("-1") ? "0" : x.ParentId,
            icon = x.Category.Equals("department") ? "icon-ym icon-ym-tree-department1" : "icon-ym icon-ym-tree-organization3",
            fullName = x.Description,
            organizeId = x.OrganizeIdTree,
            Num = data.Count(xx => xx.ParentId == x.Id),
            sortCode = 99
        }).ToList();

        organizeList.OrderByDescending(x => x.organizeId.Length).ToList().ForEach(item =>
        {
            if (!organizeList.Any(x => item.ParentId.Equals(x.Id))) item.ParentId = "0";
            var pOrgTree = organizeList.Where(x => x.organizeId != item.organizeId && item.organizeId.Contains(x.organizeId)).FirstOrDefault()?.fullName;
            if (organizeList.Any(x => item.ParentId.Equals(x.Id))) pOrgTree = organizeList.FirstOrDefault(x => item.ParentId.Equals(x.Id))?.fullName;

            if (pOrgTree.IsNotEmptyOrNull() && item.organizeId.IsNotEmptyOrNull()) item.fullName = item.fullName.Replace(pOrgTree + "/", string.Empty);
        });

        organizeList.Where(x => !x.type.Equals("position")).ToList().ForEach(item =>
        {
            if (organizeList.Any(x => !x.type.Equals("position") && x.Num > 0 && x.organizeId.Contains(item.organizeId))) item.Num = 1;
            else item.Num = 0;
            organizeList.Where(x => !x.type.Equals("position") && x.organizeId.Contains(item.organizeId) && x.organizeId != item.organizeId).ToList().ForEach(it =>
            {
                it.ParentId = item.Id;
            });
        });

        return new { list = organizeList.Where(x => x.Num > 0).ToList().Union(data).OrderBy(x => x.sortCode).ToList().ToTree("0") };
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] PositionCrInput input)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == input.organizeId && it.Add == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.IsAnyAsync(p => p.OrganizeId == input.organizeId && p.FullName == input.fullName && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6005);
        if (await _repository.IsAnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6000);
        PositionEntity? entity = input.Adapt<PositionEntity>();
        int isOk = await _repository.AsSugarClient().Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D6001);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        PositionEntity? entity = await _repository.GetSingleAsync(p => p.Id == id && p.DeleteMark == null);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Delete == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 岗位下有用户不能删
        if (await _repository.AsSugarClient().Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Position" && u.ObjectId == id))
            throw Oops.Oh(ErrorCode.D6007);

        int isOk = await _repository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6002);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] PositionUpInput input)
    {
        PositionEntity? oldEntity = await _repository.GetSingleAsync(it => it.Id == id);
        if (oldEntity.OrganizeId != input.organizeId && !_userManager.DataScope.Any(it => it.organizeId == oldEntity.OrganizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!_userManager.DataScope.Any(it => it.organizeId == input.organizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.IsAnyAsync(p => p.OrganizeId == input.organizeId && p.FullName == input.fullName && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D6005);
        if (await _repository.IsAnyAsync(p => p.EnCode == input.enCode && p.DeleteMark == null && p.Id != id))
            throw Oops.Oh(ErrorCode.D6000);

        // 如果变更组织，该岗位下已存在成员，则不允许修改
        if (input.organizeId != oldEntity.OrganizeId)
        {
            if (await _repository.AsSugarClient().Queryable<UserRelationEntity>().AnyAsync(u => u.ObjectType == "Position" && u.ObjectId == id))
                throw Oops.Oh(ErrorCode.D6008);
        }

        PositionEntity? entity = input.Adapt<PositionEntity>();
        int isOk = await _repository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6003);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        if (!_userManager.DataScope.Any(it => it.organizeId == id && it.Add == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!await _repository.IsAnyAsync(r => r.Id == id && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D6006);

        int isOk = await _repository.AsSugarClient().Updateable<PositionEntity>().UpdateColumns(it => new PositionEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.D6004);
        await DelPosition(string.Format("{0}_{1}", _userManager.TenantId, _userManager.UserId));
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<PositionEntity> GetInfoById(string id)
    {
        return await _repository.GetSingleAsync(p => p.Id == id);
    }

    /// <summary>
    /// 获取岗位列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<PositionEntity>> GetListAsync()
    {
        return await _repository.AsQueryable().Where(u => u.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 名称.
    /// </summary>
    /// <param name="ids">岗位ID组</param>
    /// <returns></returns>
    [NonAction]
    public string GetName(string ids)
    {
        if (ids.IsNullOrEmpty())
            return string.Empty;
        List<string>? idList = ids.Split(",").ToList();
        List<string>? nameList = new List<string>();
        List<PositionEntity>? roleList = _repository.AsQueryable().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToList();
        foreach (string item in idList)
        {
            var info = roleList.Find(x => x.Id == item);
            if (info != null && info.FullName.IsNotEmptyOrNull())
            {
                nameList.Add(info.FullName);
            }
        }

        return string.Join(",", nameList);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 删除岗位缓存.
    /// </summary>
    /// <param name="userId">适配多租户模式(userId:tenantId_userId).</param>
    /// <returns></returns>
    private async Task<bool> DelPosition(string userId)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYPOSITION, userId);
        await _cacheManager.DelAsync(cacheKey);
        return await Task.FromResult(true);
    }

    #endregion
}