using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Helper;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.NPOI;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Options;
using Poxiao.Infrastructure.Security;
using Poxiao.DatabaseAccessor;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Dto.Organize;
using Poxiao.Systems.Entitys.Dto.Role;
using Poxiao.Systems.Entitys.Dto.SysConfig;
using Poxiao.Systems.Entitys.Dto.User;
using Poxiao.Systems.Entitys.Dto.UserRelation;
using Poxiao.Systems.Entitys.Enum;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Linq.Expressions;

namespace Poxiao.Systems;

/// <summary>
///  业务实现：用户信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Users", Order = 163)]
[Route("api/permission/[controller]")]
public class UsersService : IUsersService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 配置文档.
    /// </summary>
    private readonly OauthOptions _oauthOptions = App.GetConfig<OauthOptions>("OAuth", true);

    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;  // 用户表仓储

    /// <summary>
    /// 机构表服务.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 用户关系表服务.
    /// </summary>
    private readonly IUserRelationService _userRelationService;

    /// <summary>
    /// 系统配置服务.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 第三方同步服务.
    /// </summary>
    private readonly ISynThirdInfoService _synThirdInfoService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// IM中心处理程序.
    /// </summary>
    private IMHandler _imHandler;

    /// <summary>
    /// 初始化一个<see cref="UsersService"/>类型的新实例.
    /// </summary>
    public UsersService(
        ISqlSugarRepository<UserEntity> userRepository,
        IOrganizeService organizeService,
        IUserRelationService userRelationService,
        ISysConfigService sysConfigService,
        ISynThirdInfoService synThirdInfoService,
        ICacheManager cacheManager,
        IFileManager fileService,
        ISqlSugarClient sqlSugarClient,
        IOptions<TenantOptions> tenantOptions,
        IUserManager userManager,
        IMHandler imHandler)
    {
        _repository = userRepository;
        _organizeService = organizeService;
        _userRelationService = userRelationService;
        _sysConfigService = sysConfigService;
        _userManager = userManager;
        _cacheManager = cacheManager;
        _synThirdInfoService = synThirdInfoService;
        _fileManager = fileService;
        _imHandler = imHandler;
    }

    #region GET

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] UserListQuery input)
    {
        // 获取分级管理组织
        var dataScope = _userManager.DataScope.Where(x => x.Select).Select(x => x.organizeId).Distinct().ToList();

        PageInputBase? pageInput = input.Adapt<PageInputBase>();

        // 处理组织树 名称
        List<OrganizeEntity>? orgTreeNameList = _organizeService.GetOrgListTreeName();

        #region 获取组织层级

        List<string>? childOrgIds = new List<string>();
        if (input.organizeId.IsNotEmptyOrNull())
        {
            childOrgIds.Add(input.organizeId);

            // 根据组织Id 获取所有子组织Id集合
            childOrgIds.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().ToChildList(x => x.ParentId, input.organizeId).Select(x => x.Id).ToList());
            childOrgIds = childOrgIds.Distinct().ToList();
        }

        #endregion

        // 获取配置文件 账号锁定类型
        SysConfigEntity? config = await _repository.AsSugarClient().Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        ErrorStrategy configLockType = (ErrorStrategy)Enum.Parse(typeof(ErrorStrategy), config?.Value);

        SqlSugarPagedList<UserListOutput>? data = new SqlSugarPagedList<UserListOutput>();

        data = await _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
            .Where((a, b) => b.DeleteMark == null && !b.Account.Equals("admin"))
            .WhereIF(childOrgIds.Any(), (a, b) => childOrgIds.Contains(a.ObjectId))
            .WhereIF(!pageInput.Keyword.IsNullOrEmpty(), (a, b) => b.Account.Contains(pageInput.Keyword) || b.RealName.Contains(pageInput.Keyword))
            .WhereIF(!_userManager.IsAdministrator, (a, b) => dataScope.Contains(a.ObjectId))
            .OrderBy((a, b) => b.SortCode).OrderBy((a, b) => b.CreatorTime, OrderByType.Desc).OrderBy((a, b) => b.LastModifyTime, OrderByType.Desc)
            .GroupBy((a, b) => new { b.Id, b.Account, b.RealName, b.CreatorTime, b.LastModifyTime, b.Gender, b.MobilePhone, b.SortCode, b.EnabledMark, b.UnLockTime })
            .Select((a, b) => new UserListOutput
            {
                id = b.Id,
                account = b.Account,
                realName = b.RealName,
                headIcon = SqlFunc.Subqueryable<UserEntity>().Where(e => e.Id == b.Id).Select(u => SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon)),
                creatorTime = b.CreatorTime,
                gender = b.Gender,
                mobilePhone = b.MobilePhone,
                sortCode = b.SortCode,
                enabledMark = SqlFunc.IIF(configLockType == ErrorStrategy.Delay && b.EnabledMark == 2 && b.UnLockTime < DateTime.Now, 1, b.EnabledMark)
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);

        #region 处理 用户 多组织

        List<UserRelationEntity>? orgUserIdAll = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
            .Where(x => data.list.Select(u => u.id).Contains(x.UserId)).ToListAsync();
        foreach (UserListOutput? item in data.list)
        {
            // 获取用户组织集合
            item.organizeList = orgUserIdAll.Where(x => x.UserId == item.id).Select(x => x.ObjectId).ToList();
            item.organize = string.Join(" ; ", orgTreeNameList.Where(x => item.organizeList.Contains(x.Id)).Select(x => x.Description));
        }

        #endregion

        return PageResult<UserListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取全部用户.
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<dynamic> GetUserAllList()
    {
        return await _repository.AsSugarClient().Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
            .Select((a, b) => new UserListAllOutput
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                gender = a.Gender,
                department = b.FullName,
                sortCode = a.SortCode,
                quickQuery = a.QuickQuery,
            }).ToListAsync();
    }

    /// <summary>
    /// 获取用户数据分页 根据角色Id.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUsersByRoleId")]
    public async Task<dynamic> GetUsersByRoleId([FromQuery] RoleListInput input)
    {
        RoleEntity? roleInfo = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.Id == input.roleId).FirstAsync();

        // 查询全部用户 (全局角色)
        if (roleInfo.GlobalMark == 1)
        {
            SqlSugarPagedList<UserListAllOutput>? list = await _repository.AsQueryable()
                .WhereIF(!input.Keyword.IsNullOrEmpty(), a => a.Account.Contains(input.Keyword) || a.RealName.Contains(input.Keyword))
                .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
                .Select(a => new UserListAllOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    gender = a.Gender,
                    sortCode = a.SortCode,
                    quickQuery = a.QuickQuery
                }).ToPagedListAsync(input.CurrentPage, input.PageSize);

            return PageResult<UserListAllOutput>.SqlSugarPageResult(list);
        }

        // 查询角色 所属 所有组织 用户
        else
        {
            // 查询角色 所有所属组织
            List<string>? orgList = await _repository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == roleInfo.Id).Select(x => x.OrganizeId).ToListAsync();

            List<string>? userIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && orgList.Contains(x.ObjectId)).Select(x => x.UserId).Distinct().ToListAsync();

            SqlSugarPagedList<UserListAllOutput>? list = await _repository.AsQueryable()
                .Where(a => userIdList.Contains(a.Id))
                .Where(p => p.EnabledMark == 1 && p.DeleteMark == null).OrderBy(p => p.SortCode)
                .WhereIF(!input.Keyword.IsNullOrEmpty(), a => a.Account.Contains(input.Keyword) || a.RealName.Contains(input.Keyword))
                .Select(a => new UserListAllOutput
                {
                    id = a.Id,
                    account = a.Account,
                    realName = a.RealName,
                    gender = a.Gender,
                    sortCode = a.SortCode,
                    quickQuery = a.QuickQuery,
                }).ToPagedListAsync(input.CurrentPage, input.PageSize);

            return PageResult<UserListAllOutput>.SqlSugarPageResult(list);
        }
    }

    /// <summary>
    /// 获取用户数据 根据角色所属组织.
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUsersByRoleOrgId")]
    public async Task<dynamic> GetUsersByRoleOrgId([FromQuery] RoleListInput input)
    {
        RoleEntity? roleInfo = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.Id == input.roleId).FirstAsync();
        input.organizeId = input.organizeId == null ? "0" : input.organizeId;

        // 获取角色所属组织集合
        List<string>? orgList = await _repository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == roleInfo.Id).Select(x => x.OrganizeId).ToListAsync();

        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        if (input.organizeId.Equals("0"))
        {
            if (input.Keyword.IsNotEmptyOrNull())
            {
                // 获取角色所属组织 成员id
                var res = await _repository.AsSugarClient().Queryable<UserEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.UserId == a.Id))
                .Where((a, b) => b.ObjectType == "Organize" && orgList.Contains(b.ObjectId)).Where((a, b) => a.EnabledMark == 1 && a.DeleteMark == null)
                .Where((a, b) => a.RealName.Contains(input.Keyword) || a.Account.Contains(input.Keyword))
                .GroupBy((a, b) => new { a.Id, a.RealName, a.Account, a.EnabledMark })
                .Select((a, b) => new {
                    id = a.Id,
                    fullName = SqlFunc.MergeString(a.RealName, "/", a.Account),
                    enabledMark = a.EnabledMark,
                    type = "user",
                    icon = "icon-ym icon-ym-tree-user2",
                    hasChildren = false,
                    isLeaf = true
                }).ToListAsync();
                output.AddRange(res.Adapt<List<OrganizeMemberListOutput>>());
            }
            else
            {
                List<OrganizeEntity>? allOrg = _organizeService.GetOrgListTreeName();

                List<OrganizeEntity>? data = await _repository.AsSugarClient().Queryable<OrganizeEntity>()
                    .Where(o => orgList.Contains(o.Id) && o.DeleteMark == null && o.EnabledMark == 1)
                    .OrderBy(o => o.SortCode).ToListAsync();

                foreach (OrganizeEntity? o in data)
                {
                    if (o.OrganizeIdTree.IsNullOrEmpty()) o.OrganizeIdTree = o.Id;
                    if (!data.Where(x => x.Id != o.Id && o.OrganizeIdTree.Contains(x.OrganizeIdTree)).Any())
                    {
                        output.Add(new OrganizeMemberListOutput
                        {
                            Id = o.Id,
                            fullName = allOrg.FirstOrDefault(x => x.Id.Equals(o.Id))?.Description,
                            enabledMark = o.EnabledMark,
                            type = o.Category,
                            icon = "icon-ym icon-ym-tree-organization3",
                            HasChildren = true,
                            IsLeaf = false
                        });
                    }
                }
            }
        }
        else
        {
            List<OrganizeEntity>? allOrg = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1).OrderBy(o => o.ParentId).ToListAsync();

            var res = await _repository.AsSugarClient().Queryable<UserEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.UserId == a.Id))
                .Where((a, b) => b.ObjectType == "Organize" && b.ObjectId == input.organizeId).Where((a, b) => a.EnabledMark == 1 && a.DeleteMark == null)
                .GroupBy((a, b) => new { a.Id, a.RealName, a.Account, a.EnabledMark })
                .Select((a, b) => new {
                    id = a.Id,
                    fullName = SqlFunc.MergeString(a.RealName, "/", a.Account),
                    enabledMark = a.EnabledMark,
                    type = "user",
                    icon = "icon-ym icon-ym-tree-user2",
                    hasChildren = false,
                    isLeaf = true
                }).ToListAsync();
            output.AddRange(res.Adapt<List<OrganizeMemberListOutput>>());
            var departmentList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.OrganizeIdTree.Contains(input.organizeId) && orgList.Contains(o.Id)).ToListAsync();

            var orgTreeNameList = _organizeService.GetOrgListTreeName();

            departmentList.OrderBy(x => x.OrganizeIdTree.Length).ToList().ForEach(o =>
            {
                o.FullName = orgTreeNameList.FirstOrDefault(x => x.Id.Equals(o.Id)).Description;

                if (o.Id != input.organizeId && !output.Any(x => o.FullName.Contains(x.fullName)))
                {
                    var pName = string.Empty;
                    if (!departmentList.Any(x => x.Id == o.ParentId)) pName = orgTreeNameList.FirstOrDefault(x => x.Id.Equals(input.organizeId)).Description;
                    else pName = orgTreeNameList.FirstOrDefault(x => x.Id.Equals(o.ParentId)).Description;
                    output.Add(new OrganizeMemberListOutput()
                    {
                        Id = o.Id,
                        fullName = o.FullName.Replace(pName + "/", string.Empty),
                        enabledMark = o.EnabledMark,
                        type = o.Category,
                        icon = o.Category.Equals("company") ? "icon-ym icon-ym-tree-organization3" : "icon-ym icon-ym-tree-department1",
                        HasChildren = true,
                        IsLeaf = false
                    });
                }
            });
        }

        return output;
    }

    /// <summary>
    /// 获取IM用户列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImUser")]
    public async Task<dynamic> GetImUserList([FromQuery] PageInputBase input)
    {
        SqlSugarPagedList<IMUserListOutput>? list = await _repository.AsSugarClient().Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .WhereIF(!input.Keyword.IsNullOrEmpty(), a => a.Account.Contains(input.Keyword) || a.RealName.Contains(input.Keyword))
            .Where(a => a.Id != _userManager.UserId && a.EnabledMark == 1 && a.DeleteMark == null).OrderBy(a => a.SortCode)
            .Select((a, b) => new IMUserListOutput
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                department = b.FullName,
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<IMUserListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取下拉框（公司+部门+用户）.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<OrganizeEntity>? organizeList = await _organizeService.GetListAsync();
        List<UserEntity>? userList = await _repository.AsQueryable().Where(t => t.EnabledMark == 1 && t.DeleteMark == null).OrderBy(u => u.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        List<UserSelectorOutput>? organizeTreeList = organizeList.Adapt<List<UserSelectorOutput>>();
        List<UserSelectorOutput>? treeList = userList.Adapt<List<UserSelectorOutput>>();
        treeList = treeList.Concat(organizeTreeList).ToList();
        return new { list = treeList.OrderBy(x => x.sortCode).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        UserEntity? entity = await _repository.GetFirstAsync(u => u.Id == id);
        SysConfigEntity? config = await _repository.AsSugarClient().Queryable<SysConfigEntity>().Where(x => x.Key.Equals("lockType") && x.Category.Equals("SysConfig")).FirstAsync();
        string? configLockType = config?.Value;
        entity.EnabledMark = configLockType.IsNotEmptyOrNull() && configLockType == "2" && entity.EnabledMark == 2 && entity.UnLockTime < DateTime.Now ? 1 : entity.EnabledMark;
        UserInfoOutput? output = entity.Adapt<UserInfoOutput>();
        if (output.headIcon == "/api/File/Image/userAvatar/") output.headIcon = string.Empty;
        if (entity != null)
        {
            List<UserRelationEntity>? allRelationList = await _userRelationService.GetListByUserId(id);
            var relationIds = allRelationList.Where(x => x.ObjectType == "Organize" || x.ObjectType == "Position").Select(x => new { x.ObjectId, x.ObjectType }).ToList();
            List<OrganizeEntity>? oList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => relationIds.Where(x => x.ObjectType == "Organize").Select(x => x.ObjectId).Contains(x.Id)).ToListAsync();
            output.organizeIdTree = new List<List<string>>();
            oList.ForEach(item =>
            {
                if (item.OrganizeIdTree.IsNotEmptyOrNull()) output.organizeIdTree.Add(item.OrganizeIdTree.Split(",").ToList());
            });
            output.organizeId = string.Join(",", relationIds.Where(x => x.ObjectType == "Organize").Select(x => x.ObjectId));
            output.positionId = string.Join(",", relationIds.Where(x => x.ObjectType == "Position").Select(x => x.ObjectId));
        }

        return output;
    }

    /// <summary>
    /// 获取当前用户所属机构下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("getOrganization")]
    public async Task<dynamic> GetOrganizeMember([FromQuery] UserListQuery input)
    {
        if (input.organizeId.IsNotEmptyOrNull() && input.organizeId != "0") input.organizeId = input.organizeId.Split(",").LastOrDefault();
        else input.organizeId = _userManager.User.OrganizeId;

        // 获取所属组织的所有成员
        List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
            .Where(x => x.ObjectType == "Organize").ToListAsync();

        var res = await _repository.AsQueryable()
                .WhereIF(!input.Keyword.IsNullOrEmpty(), u => u.Account.Contains(input.Keyword) || u.RealName.Contains(input.Keyword))
                .Where(u => u.EnabledMark == 1 && u.DeleteMark == null && userList.Where(x => x.ObjectId == input.organizeId).Select(x => x.UserId).Contains(u.Id)).OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    Id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = u.EnabledMark,
                    icon = "icon-ym icon-ym-tree-user2",
                    headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon),
                    IsLeaf = true,
                    HasChildren = false,
                    type = "user",
                }).ToListAsync();

        if (res.Any())
        {
            var orgList = _organizeService.GetOrgListTreeName();

            // 处理组织树
            res.ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });
        }

        return res;
    }

    #endregion

    #region POST

    /// <summary>
    /// 根据用户Id List 获取当前用户Id.
    /// </summary>
    /// <returns></returns>
    [HttpPost("getDefaultCurrentValueUserId")]
    public async Task<dynamic> GetDefaultCurrentValueUserId([FromBody] GetDefaultCurrentValueInput input)
    {
        if ((input.UserIds == null || !input.UserIds.Any()) && (input.DepartIds == null || !input.DepartIds.Any()) && (input.PositionIds == null || !input.PositionIds.Any())
            && (input.RoleIds == null || !input.RoleIds.Any()) && (input.GroupIds == null || !input.GroupIds.Any())) return new { userId = _userManager.UserId };

        var userRelationList = _repository.AsSugarClient().Queryable<UserRelationEntity>().Select(x => new UserRelationEntity() { UserId = x.UserId, ObjectId = x.ObjectId }).ToList();
        var userIdList = userRelationList.Where(x => input.UserIds.Contains(x.UserId) || input.DepartIds.Contains(x.ObjectId)
            || input.PositionIds.Contains(x.ObjectId) || input.RoleIds.Contains(x.ObjectId) || input.GroupIds.Contains(x.ObjectId)).Select(x => x.UserId).ToList();

        if (userIdList.Contains(_userManager.UserId)) return new { userId = _userManager.UserId };
        else return new { userId = string.Empty };
    }

    /// <summary>
    /// 获取.
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetUserList")]
    public async Task<dynamic> GetUserList([FromBody] UserRelationInput input)
    {
        var data = await _repository.AsQueryable().Where(it => it.EnabledMark > 0 && it.DeleteMark == null)
            .Where(it => input.userId.Contains(it.Id))
            .Select(it => new OrganizeMemberListOutput()
            {
                Id = it.Id,
                fullName = SqlFunc.MergeString(it.RealName, "/", it.Account),
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", it.HeadIcon),
                enabledMark = it.EnabledMark,
            }).ToListAsync();

        data = data.OrderBy(x => input.userId.IndexOf(x.Id)).ToList();
        if (data.Any())
        {
            var orgList = _organizeService.GetOrgListTreeName();

            // 获取 所属组织的所有成员
            List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                .Where(x => x.ObjectType == "Organize" && data.Select(x => x.Id).Contains(x.UserId)).ToListAsync();

            // 处理组织树
            data.ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });
        }

        return new { list = data };
    }

    /// <summary>
    /// 获取机构成员列表.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("ImUser/Selector/{organizeId}")]
    public async Task<dynamic> GetOrganizeMemberList(string organizeId, [FromBody] PageInputBase input)
    {
        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        var orgList = _organizeService.GetOrgListTreeName();
        if (!input.Keyword.IsNullOrEmpty())
        {
            var outList = await _repository.AsQueryable()
                .WhereIF(!input.Keyword.IsNullOrEmpty(), u => u.Account.Contains(input.Keyword) || u.RealName.Contains(input.Keyword))
                .Where(u => u.EnabledMark > 0 && u.DeleteMark == null).OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    Id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = SqlFunc.IIF(u.EnabledMark == 2 && u.UnLockTime < DateTime.Now, 1, u.EnabledMark),
                    icon = "icon-ym icon-ym-tree-user2",
                    headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon),
                    IsLeaf = true,
                    HasChildren = false,
                    type = "user",
                }).ToPagedListAsync(input.CurrentPage, input.PageSize);

            if (outList.list.Any())
            {
                // 获取 所属组织的所有成员
                List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                    .Where(x => x.ObjectType == "Organize" && outList.list.Select(x => x.Id).Contains(x.UserId)).ToListAsync();

                // 处理组织树
                outList.list.Where(x => x.type.Equals("user")).ToList().ForEach(item =>
                {
                    var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                    var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                    item.organize = string.Join(",", oTree);
                });
            }

            return PageResult<OrganizeMemberListOutput>.SqlSugarPageResult(outList);
        }
        else
        {
            var pOrganize = orgList.FirstOrDefault(x => x.Id.Equals(organizeId));

            output = await _organizeService.GetOrganizeMemberList(organizeId);
            if (pOrganize != null) output.ForEach(item => item.fullName = item.fullName.Replace(pOrganize.FullName + "/", string.Empty));
        }

        if (output.Any())
        {
            // 获取 所属组织的所有成员
            List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                .Where(x => x.ObjectType == "Organize" && output.Select(x => x.Id).Contains(x.UserId)).ToListAsync();

            // 处理组织树
            output.Where(x => x.type.Equals("user")).ToList().ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });
        }

        return new { list = output };
    }

    /// <summary>
    /// 获取下拉框 根据权限.
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetListByAuthorize/{organizeId}")]
    public async Task<dynamic> GetListByAuthorize(string organizeId, [FromBody] KeywordInput input)
    {
        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();
        if (!input.Keyword.IsNullOrEmpty())
        {
            output = await _repository.AsQueryable()
                .WhereIF(!input.Keyword.IsNullOrEmpty(), u => u.Account.Contains(input.Keyword) || u.RealName.Contains(input.Keyword))
                .Where(u => u.EnabledMark > 0 && u.DeleteMark == null).OrderBy(o => o.SortCode)
                .Select(u => new OrganizeMemberListOutput
                {
                    Id = u.Id,
                    fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                    enabledMark = SqlFunc.IIF(u.EnabledMark == 2 && u.UnLockTime < DateTime.Now, 1, u.EnabledMark),
                    icon = "icon-ym icon-ym-tree-user2",
                    headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon),
                    IsLeaf = true,
                    HasChildren = false,
                    type = "user",
                }).Take(50).ToListAsync();

        }
        else
        {
            output = await GetOrganizeMemberList(organizeId);
        }

        if (output.Any())
        {
            var orgList = _organizeService.GetOrgListTreeName();
            // 获取所属组织的所有成员
            List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                .Where(x => x.ObjectType == "Organize" && output.Select(x => x.Id).Contains(x.UserId)).ToListAsync();

            // 处理组织树
            output.Where(x => x.type.Equals("user")).ToList().ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });
        }

        return new { list = output.DistinctBy(x => x.Id).ToList() };
    }

    /// <summary>
    /// 获取当前用户下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("getSubordinates")]
    public async Task<dynamic> GetSubordinate([FromBody] KeywordInput input)
    {
        var res = await _repository.AsQueryable()
                   .WhereIF(!input.Keyword.IsNullOrEmpty(), u => u.Account.Contains(input.Keyword) || u.RealName.Contains(input.Keyword))
                   .Where(u => u.EnabledMark == 1 && u.DeleteMark == null && u.ManagerId == _userManager.UserId).OrderBy(o => o.SortCode)
                   .Select(u => new OrganizeMemberListOutput
                   {
                       Id = u.Id,
                       fullName = SqlFunc.MergeString(u.RealName, "/", u.Account),
                       enabledMark = u.EnabledMark,
                       icon = "icon-ym icon-ym-tree-user2",
                       headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon),
                       IsLeaf = true,
                       HasChildren = false,
                       type = "user",
                   }).ToListAsync();

        // ��ȡ������֯�����г�Ա
        List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
            .Where(x => res.Select(xx => xx.Id).Contains(x.UserId)).ToListAsync();

        if (res.Any())
        {
            var orgList = _organizeService.GetOrgListTreeName();

            // ������֯��
            res.ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });
        }

        return res;
    }

    /// <summary>
    /// 获取当前用户所属机构下属成员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("GetUsersByPositionId")]
    public async Task<dynamic> GetUsersByPositionId([FromQuery] UserListQuery input)
    {
        List<OrganizeMemberListOutput>? outData = new List<OrganizeMemberListOutput>();
        UserEntity? user = _userManager.User;

        // 获取岗位所属组织信息
        OrganizeMemberListOutput? orgInfo = await _repository.AsSugarClient().Queryable<PositionEntity, OrganizeEntity>((a, b) =>
                new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId) && b.EnabledMark == 1 && b.DeleteMark == null))
            .Where((a, b) => a.Id == input.positionId).Select((a, b) => new OrganizeMemberListOutput
            {
                Id = b.Id,
                fullName = b.FullName,
                enabledMark = b.EnabledMark,
                type = b.Category,
                ParentId = "0",
                organize = b.Id,
                icon = b.Category.Equals("company") ? "icon-ym icon-ym-tree-organization3" : "icon-ym icon-ym-tree-department1",
                HasChildren = true,
                IsLeaf = false
            }).FirstAsync();

        // 处理组织树
        if (orgInfo.organize.IsNotEmptyOrNull())
        {
            var orgList = _organizeService.GetOrgListTreeName();
            orgInfo.fullName = orgList.FirstOrDefault(x => x.Id.Equals(orgInfo.organize))?.Description;
        }

        outData.Add(orgInfo);

        // 获取岗位所属组织的所有成员
        List<OrganizeMemberListOutput>? userData = await _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.UserId)))
            .Where((a, b) => a.ObjectType == "Organize" && a.ObjectId == orgInfo.Id && b.EnabledMark == 1 && b.DeleteMark == null)
            .WhereIF(!input.Keyword.IsNullOrEmpty(), (a, b) => b.Account.Contains(input.Keyword) || b.RealName.Contains(input.Keyword))
            .Select((a, b) => new OrganizeMemberListOutput
            {
                Id = b.Id,
                ParentId = orgInfo.Id,
                fullName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                enabledMark = b.EnabledMark,
                icon = "icon-ym icon-ym-tree-user2",
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", b.HeadIcon),
                IsLeaf = true,
                HasChildren = false,
                type = "user"
            }).ToListAsync();

        outData.AddRange(userData);

        return outData.ToTree("0");
    }

    /// <summary>
    /// 通过部门、岗位、用户、角色、分组id获取用户列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("UserCondition")]
    public async Task<dynamic> UserCondition([FromBody] UserConditionInput input)
    {
        SqlSugarPagedList<UserListOutput>? data = new SqlSugarPagedList<UserListOutput>();

        if (input.departIds == null) input.departIds = new List<string>();
        if (input.positionIds != null) input.departIds.AddRange(input.positionIds);
        if (input.roleIds != null) input.departIds.AddRange(input.roleIds);
        if (input.groupIds != null) input.departIds.AddRange(input.groupIds);
        if (!input.departIds.Any()) return PageResult<UserListOutput>.SqlSugarPageResult(data);
        var ids = await _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
            .Where((a, b) => b.DeleteMark == null)
            .WhereIF(input.departIds.Any() || input.userIds.Any(), (a, b) => input.departIds.Contains(a.ObjectId) || input.userIds.Contains(b.Id))
            .WhereIF(input.pagination.Keyword.IsNotEmptyOrNull(), (a, b) => b.Account.Contains(input.pagination.Keyword) || b.RealName.Contains(input.pagination.Keyword))
            .Select((a, b) => b.Id).Distinct().ToListAsync();
        data = await _repository.AsQueryable().Where(x => ids.Contains(x.Id)).Select(x => new UserListOutput()
        {
            id = x.Id,
            organizeId = x.OrganizeId,
            account = x.Account,
            fullName = SqlFunc.MergeString(x.RealName, "/", x.Account),
            headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", x.HeadIcon),
            gender = x.Gender,
            mobilePhone = x.MobilePhone
        }).ToPagedListAsync(input.pagination.CurrentPage, input.pagination.PageSize);
        if (data.list.Any())
        {
            var orgList = _organizeService.GetOrgListTreeName();

            // 获取所属组织的所有成员
            List<UserRelationEntity>? userList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                .Where(x => x.ObjectType == "Organize" && data.list.Select(x => x.id).Contains(x.UserId)).ToListAsync();

            // 处理组织树
            data.list.ToList().ForEach(item =>
            {
                var oids = userList.Where(x => x.UserId.Equals(item.id)).Select(x => x.ObjectId).ToList();
                var oTree = orgList.Where(x => oids.Contains(x.Id)).Select(x => x.Description).ToList();
                item.organize = string.Join(",", oTree);
            });

        }

        return PageResult<UserListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取选中组织、岗位、角色、分组、用户基本信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("GetSelectedList")]
    public async Task<dynamic> GetSelectedList([FromBody] UserSelectedInput input)
    {
        if (input.userId == null) return new { list = new List<UserSelectedOutput>() };

        var objIds = new List<string>();
        input.userId.Where(x => x.IsNotEmptyOrNull()).ToList().ForEach(item => objIds.Add(item.Split("--").First()));
        var orgInfoList = _organizeService.GetOrgListTreeName();

        var orgList = orgInfoList.Where(x => objIds.Contains(x.Id)).ToList();
        var posList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => objIds.Contains(x.Id)).Select(x => new { x.Id, x.OrganizeId, x.FullName }).ToListAsync();
        var roleList = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => objIds.Contains(x.Id)).Select(x => new { x.Id, x.FullName, x.GlobalMark }).ToListAsync();
        var groupList = await _repository.AsSugarClient().Queryable<GroupEntity>().Where(x => objIds.Contains(x.Id)).Select(x => new { x.Id, x.FullName }).ToListAsync();
        var userList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => objIds.Contains(x.Id)).Select(x => new { x.Id, x.RealName, x.Account, x.HeadIcon }).ToListAsync();

        var resList = new List<UserSelectedOutput>();

        orgList.ForEach(item =>
        {
            resList.Add(new UserSelectedOutput()
            {
                id = item.Id,
                fullName = item.FullName,
                type = item.Category,
                icon = item.Category.Equals("company") ? "icon-ym icon-ym-tree-organization3" : "icon-ym icon-ym-tree-department1",
                organize = item.Description,
                organizeIds = new List<string> { item.OrganizeIdTree },
            });
        });

        posList.ForEach(item =>
        {
            resList.Add(new UserSelectedOutput()
            {
                id = item.Id,
                fullName = item.FullName,
                type = "position",
                icon = "icon-ym icon-ym-tree-position1",
                organize = orgInfoList.Find(x => x.Id.Equals(item.OrganizeId)).Description,
                organizeIds = new List<string> { orgInfoList.Find(x => x.Id.Equals(item.OrganizeId)).OrganizeIdTree },
            });
        });

        var roleOrgList = await _repository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => roleList.Select(xx => xx.Id).Contains(x.ObjectId)).Select(x => new { x.ObjectId, x.OrganizeId }).ToListAsync();
        roleList.ForEach(item =>
        {
            resList.Add(new UserSelectedOutput()
            {
                id = item.Id,
                fullName = item.FullName,
                type = "role",
                organize = SqlFunc.IIF(item.GlobalMark == 1, "", string.Join(",", orgInfoList.Where(o => roleOrgList.Where(x => x.ObjectId.Equals(item.Id)).Select(x => x.OrganizeId).Contains(o.Id)).Select(x => x.Description))),
                icon = "icon-ym icon-ym-generator-role",
                organizeIds = orgInfoList.Where(o => roleOrgList.Where(x => x.ObjectId.Equals(item.Id)).Select(x => x.OrganizeId).Contains(o.Id)).Select(x => x.OrganizeIdTree).ToList(),
            });
        });

        groupList.ForEach(item =>
        {
            resList.Add(new UserSelectedOutput()
            {
                id = item.Id,
                fullName = item.FullName,
                type = "group",
                icon = "icon-ym icon-ym-generator-group1"
            });
        });

        var userOrgList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => userList.Select(xx => xx.Id).Contains(x.UserId) && x.ObjectType.Equals("Organize")).Select(x => new { x.ObjectId, x.UserId }).ToListAsync();
        userList.ForEach(item =>
        {
            resList.Add(new UserSelectedOutput()
            {
                id = item.Id,
                fullName = item.RealName + "/" + item.Account,
                type = "user",
                icon = "icon-ym icon-ym-tree-user2",
                headIcon = "/api/File/Image/userAvatar/" + item.HeadIcon,
                organize = string.Join(",", orgInfoList.Where(o => userOrgList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).Contains(o.Id)).Select(x => x.Description)),
                organizeIds = orgInfoList.Where(o => userOrgList.Where(x => x.UserId.Equals(item.Id)).Select(x => x.ObjectId).Contains(o.Id)).Select(x => x.OrganizeIdTree).ToList(),
            });
        });

        return new { list = resList.OrderBy(x => objIds.IndexOf(x.id)) };
    }

    /// <summary>
    /// 获取用户基本信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("GetSelectedUserList")]
    public async Task<dynamic> GetSelectedUserList([FromBody] UserSelectedInput input)
    {
        var userId = new List<string>();
        input.userId.ForEach(item => userId.Add(item.Split("--").First()));
        var orgInfoList = _organizeService.GetOrgListTreeName();
        var relIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => userId.Contains(x.ObjectId)).Select(x => new { x.UserId, x.ObjectId, x.ObjectType }).Distinct().ToListAsync();
        var userIdList = relIdList.Where(x => !x.ObjectType.Equals("Organize")).Select(x => x.UserId).ToList();

        // 子组织
        var childOrgIdList = new List<string>();
        relIdList.Where(x => x.ObjectType.Equals("Organize")).Select(x => x.ObjectId).ToList().ForEach(item => childOrgIdList.AddRange(orgInfoList.Where(x => x.OrganizeIdTree.Contains(item)).Select(x => x.Id)));
        userIdList.AddRange(await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => childOrgIdList.Contains(x.ObjectId) && x.ObjectType.Equals("Organize")).Select(x => x.UserId).ToListAsync());
        userIdList.AddRange(userId);
        var userOrgList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => userIdList.Contains(x.UserId) && x.ObjectType.Equals("Organize")).Select(x => new { x.ObjectId, x.UserId }).ToListAsync();
        var userInfoList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => userIdList.Contains(x.Id) && x.DeleteMark == null && x.EnabledMark > 0)
            .WhereIF(input.pagination.Keyword.IsNotEmptyOrNull(), x => x.RealName.Contains(input.pagination.Keyword) || x.Account.Contains(input.pagination.Keyword))
            .Select(x => new UserSelectedOutput
            {
                fullName = SqlFunc.MergeString(x.RealName, "/", x.Account),
                icon = "icon-ym icon-ym-tree-user2",
                headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", x.HeadIcon),
                id = x.Id,
                type = "user",
                gender = x.Gender,
                mobilePhone = x.MobilePhone
            }).ToPagedListAsync(input.pagination.CurrentPage, input.pagination.PageSize);

        userInfoList.list.ToList().ForEach(item =>
        {
            item.organize = string.Join(",", orgInfoList.Where(o => userOrgList.Where(x => x.UserId.Equals(item.id)).Select(x => x.ObjectId).Contains(o.Id)).Select(x => x.Description));
            item.organizeIds = orgInfoList.Where(o => userOrgList.Where(x => x.UserId.Equals(item.id)).Select(x => x.ObjectId).Contains(o.Id)).Select(x => x.OrganizeIdTree).ToList();
        });

        return PageResult<UserSelectedOutput>.SqlSugarPageResult(userInfoList);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] UserCrInput input)
    {
        var orgids = input.organizeId.Split(',');
        if (!_userManager.DataScope.Any(it => orgids.Contains(it.organizeId) && it.Add) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (await _repository.IsAnyAsync(u => u.Account == input.account && u.DeleteMark == null)) throw Oops.Oh(ErrorCode.D1003);
        UserEntity? entity = input.Adapt<UserEntity>();

        #region 用户表单

        entity.IsAdministrator = 0;
        entity.EntryDate = input.entryDate.IsNullOrEmpty() ? DateTime.Now : input.entryDate;
        entity.Birthday = input.birthday.IsNullOrEmpty() ? DateTime.Now : input.birthday;
        entity.QuickQuery = PinyinHelper.PinyinString(input.realName);
        entity.Secretkey = Guid.NewGuid().ToString();
        entity.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(CommonConst.DEFAULTPASSWORD) + entity.Secretkey);
        string? headIcon = input.headIcon.Split('/').ToList().Last();
        if (string.IsNullOrEmpty(headIcon))
            headIcon = "001.png";
        entity.HeadIcon = headIcon;

        #region 多组织 优先选择有权限组织

        // 多组织
        string[]? orgList = entity.OrganizeId.Split(",");
        entity.OrganizeId = string.Empty;

        foreach (string? item in orgList)
        {
            List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, item);

            // 如果该组织下有角色并且有角色权限 则为默认组织
            if (roleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
            {
                // 多 组织 默认
                entity.OrganizeId = item;
                break;
            }
        }

        // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
        if (entity.OrganizeId.IsNullOrEmpty()) entity.OrganizeId = input.organizeId.Split(",").FirstOrDefault();

        #endregion

        string[]? positionIds = entity.PositionId?.Split(",");
        List<string>? pIdList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.OrganizeId == entity.OrganizeId && positionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        entity.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取当前组织第一个

        // 获取有菜单的系统 默认系统
        var userRoleIds = input.roleId.Split(",");
        var sysList = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId))
            .Where(x => x.ItemType == "module" || x.ItemType == "system").ToListAsync();
        var menuList = await _repository.AsSugarClient().Queryable<ModuleEntity>()
            .Where(x => sysList.Where(xx => xx.ItemType.Equals("system")).Select(xx => xx.ItemId).Contains(x.SystemId)).ToListAsync();
        entity.SystemId = menuList.Where(x => sysList.Where(xx => xx.ItemType.Equals("module")).Select(xx => xx.ItemId).Contains(x.Id)).FirstOrDefault()?.SystemId;

        #endregion

        try
        {
            // 新增用户记录
            await _repository.AsInsertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

            // 将临时文件迁移至正式文件
            FileHelper.MoveFile(Path.Combine(FileVariable.TemporaryFilePath, headIcon), Path.Combine(FileVariable.UserAvatarFilePath, headIcon));

            List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.roleId, "Role"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.positionId, "Position"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.organizeId, "Organize"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(entity.Id, input.groupId, "Group"));

            if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系

            #region 第三方同步

            try
            {
                SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
                List<UserEntity>? userList = new List<UserEntity>();
                userList.Add(entity);
                if (sysConfig.dingSynIsSynUser)
                    await _synThirdInfoService.SynUser(2, 3, sysConfig, userList);
                if (sysConfig.qyhIsSynUser)
                    await _synThirdInfoService.SynUser(1, 3, sysConfig, userList);
            }
            catch (Exception)
            {
            }

            #endregion

            // 单点登录同步
            await syncUserInfo(entity, "create", _userManager.TenantId);
        }
        catch (Exception)
        {
            throw Oops.Bah(ErrorCode.D5001);
        }

    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        UserEntity? entity = await _repository.GetFirstAsync(u => u.Id == id && u.DeleteMark == null);

        // 所属组织 分级权限验证
        List<string>? orgIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Delete) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (await _organizeService.GetIsManagerByUserId(id))
            throw Oops.Oh(ErrorCode.D2003);
        _ = entity ?? throw Oops.Oh(ErrorCode.D5002);
        if (entity.IsAdministrator == (int)AccountType.Administrator)
            throw Oops.Oh(ErrorCode.D1014);
        if (entity.Id == _userManager.UserId)
            throw Oops.Oh(ErrorCode.D1001);
        entity.DeleteTime = DateTime.Now;
        entity.DeleteMark = 1;
        entity.DeleteUserId = _userManager.UserId;

        // 用户软删除
        await _repository.AsUpdateable(entity).UpdateColumns(it => new { it.DeleteTime, it.DeleteMark, it.DeleteUserId }).ExecuteCommandAsync();

        // 直接删除用户关系表相关相关数据
        await _userRelationService.Delete(id);

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.DelSynData(2, 3, sysConfig, id);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.DelSynData(1, 3, sysConfig, id);
        }
        catch (Exception)
        {
        }

        #endregion

        // 单点登录同步
        await syncUserInfo(entity, "delete", _userManager.TenantId);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] UserUpInput input)
    {
        UserEntity? oldUserEntity = await _repository.GetFirstAsync(it => it.Id == id);
        input.roleId = input.roleId == null ? string.Empty : input.roleId;

        // 超级管理员 只有 admin 账号才有变更权限
        if (_userManager.UserId != oldUserEntity.Id && oldUserEntity.IsAdministrator == 1 && _userManager.Account != "admin")
            throw Oops.Oh(ErrorCode.D1033);

        // 超级管理员不能禁用
        if (oldUserEntity.IsAdministrator.Equals(1) && input.enabledMark.Equals(0))
            throw Oops.Oh(ErrorCode.D1015);

        // 旧数据
        List<string>? orgIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 新数据
        var orgids = input.organizeId.Split(',');
        if (!_userManager.DataScope.Any(it => orgids.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        // 排除自己并且判断与其他是否相同
        if (await _repository.IsAnyAsync(u => u.Account == input.account && u.DeleteMark == null && u.Id != id)) throw Oops.Oh(ErrorCode.D1003);
        if (id == input.managerId) throw Oops.Oh(ErrorCode.D1021);

        // 直属主管的上级不能为自己的下属
        if (await GetIsMyStaff(id, input.managerId, 10)) throw Oops.Oh(ErrorCode.D1026);
        UserEntity? entity = input.Adapt<UserEntity>();
        entity.QuickQuery = PinyinHelper.PinyinString(input.realName);
        string? headIcon = input.headIcon.Split('/').ToList().Last();
        entity.HeadIcon = headIcon;
        entity.LastModifyTime = DateTime.Now;
        entity.LastModifyUserId = _userManager.UserId;
        if (entity.EnabledMark == 2) entity.UnLockTime = null;

        #region 多组织 优先选择有权限组织

        // 多 组织
        string[]? orgList = entity.OrganizeId.Split(",");
        entity.OrganizeId = string.Empty;

        if (orgList.Contains(oldUserEntity.OrganizeId))
        {
            List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, oldUserEntity.OrganizeId);

            // 如果该组织下有角色并且有角色权限 则为默认组织
            if (roleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                entity.OrganizeId = oldUserEntity.OrganizeId; // 多 组织 默认
        }

        if (entity.OrganizeId.IsNullOrEmpty())
        {
            foreach (string? item in orgList)
            {
                List<string>? roleList = await _userManager.GetUserOrgRoleIds(entity.RoleId, item);

                // 如果该组织下有角色并且有角色权限 则为默认组织
                if (roleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                {
                    // 多 组织 默认
                    entity.OrganizeId = item;
                    break;
                }
            }
        }

        // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
        if (entity.OrganizeId.IsNullOrEmpty()) entity.OrganizeId = input.organizeId.Split(",").FirstOrDefault();

        #endregion

        // 获取有菜单的系统 默认系统
        var userRoleIds = input.roleId.Split(",");
        var sysList = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId))
            .Where(x => x.ItemType == "module" || x.ItemType == "system").ToListAsync();
        var menuList = await _repository.AsSugarClient().Queryable<ModuleEntity>()
            .Where(x => sysList.Where(xx => xx.ItemType.Equals("system")).Select(xx => xx.ItemId).Contains(x.SystemId)).ToListAsync();
        if (menuList.Any() && !menuList.Any(x => x.SystemId.Equals(oldUserEntity.SystemId)))
            entity.SystemId = menuList.Where(x => sysList.Where(xx => xx.ItemType.Equals("module")).Select(xx => xx.ItemId).Contains(x.Id)).FirstOrDefault()?.SystemId;
        else
            entity.SystemId = oldUserEntity.SystemId;

        // 获取默认组织下的岗位
        string[]? positionIds = entity.PositionId?.Split(",");
        List<string>? pIdList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.OrganizeId == entity.OrganizeId && positionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        if (entity.PositionId.IsNotEmptyOrNull() && pIdList.Contains(oldUserEntity.PositionId))
            entity.PositionId = oldUserEntity.PositionId;
        else entity.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取第一个

        try
        {
            // 更新用户记录
            int newEntity = await _repository.AsUpdateable(entity).UpdateColumns(it => new {
                it.Account,
                it.RealName,
                it.QuickQuery,
                it.Gender,
                it.Email,
                it.OrganizeId,
                it.ManagerId,
                it.PositionId,
                it.RoleId,
                it.SortCode,
                it.EnabledMark,
                it.Description,
                it.HeadIcon,
                it.Nation,
                it.NativePlace,
                it.EntryDate,
                it.CertificatesType,
                it.CertificatesNumber,
                it.Education,
                it.UrgentContacts,
                it.UrgentTelePhone,
                it.PostalAddress,
                it.MobilePhone,
                it.Birthday,
                it.TelePhone,
                it.Landline,
                it.UnLockTime,
                it.GroupId,
                it.LastModifyTime,
                it.SystemId,
                it.LastModifyUserId
            }).ExecuteCommandAsync();

            // 将临时文件迁移至正式文件
            FileHelper.MoveFile(Path.Combine(FileVariable.TemporaryFilePath, headIcon), Path.Combine(FileVariable.UserAvatarFilePath, headIcon));

            // 直接删除用户关系表相关相关数据
            await _userRelationService.Delete(id);

            List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, entity.RoleId, "Role"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.positionId, "Position"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.organizeId, "Organize"));
            userRelationList.AddRange(_userRelationService.CreateUserRelation(id, input.groupId, "Group"));
            if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList); // 批量新增用户关系

            // 修改该用户信息，该用户会立即退出登录
            var onlineCacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId);
            var list = await _cacheManager.GetAsync<List<UserOnlineModel>>(onlineCacheKey);
            var user = list.Find(it => it.tenantId == _userManager.TenantId && it.userId == id);
            if (user != null)
            {
                await _imHandler.SendMessageAsync(user.connectionId, new { method = "logout", msg = "用户信息已变更，请重新登录！" }.ToJsonString());

                // 删除在线用户ID
                list.RemoveAll((x) => x.connectionId == user.connectionId);
                await _cacheManager.SetAsync(onlineCacheKey, list);

                // 删除用户登录信息缓存
                var cacheKey = string.Format("{0}:{1}:{2}", _userManager.TenantId, CommonConst.CACHEKEYUSER, user.userId);
                await _cacheManager.DelAsync(cacheKey);
            }
        }
        catch (Exception)
        {
            FileHelper.MoveFile(Path.Combine(FileVariable.UserAvatarFilePath, headIcon), Path.Combine(FileVariable.TemporaryFilePath, headIcon));
            throw Oops.Oh(ErrorCode.D5004);
        }

        #region 第三方同步

        try
        {
            SysConfigOutput? sysConfig = await _sysConfigService.GetInfo();
            List<UserEntity>? userList = new List<UserEntity>();
            userList.Add(entity);
            if (sysConfig.dingSynIsSynUser)
                await _synThirdInfoService.SynUser(2, 3, sysConfig, userList);
            if (sysConfig.qyhIsSynUser)
                await _synThirdInfoService.SynUser(1, 3, sysConfig, userList);
        }
        catch (Exception)
        {
        }

        #endregion

        // 单点登录同步
        await syncUserInfo(entity, "update", _userManager.TenantId);
    }

    /// <summary>
    /// 更新状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        UserEntity? entity = await _repository.GetFirstAsync(it => it.Id == id);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Edit == true) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        if (!await _repository.IsAnyAsync(u => u.Id == id && u.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1002);
        int isOk = await _repository.AsUpdateable().SetColumns(it => new UserEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);
    }

    /// <summary>
    /// 重置密码.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/ResetPassword")]
    public async Task ResetPassword(string id, [FromBody] UserResetPasswordInput input)
    {
        UserEntity? entity = await _repository.GetFirstAsync(u => u.Id == id && u.DeleteMark == null);

        // 所属组织 分级权限验证
        List<string>? orgIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.UserId == id && x.ObjectType == "Organize").Select(x => x.ObjectId).ToListAsync();
        if (!_userManager.DataScope.Any(it => orgIdList.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);

        if (!input.userPassword.Equals(input.validatePassword))
            throw Oops.Oh(ErrorCode.D5006);
        _ = entity ?? throw Oops.Oh(ErrorCode.D1002);

        string? password = MD5Encryption.Encrypt(input.userPassword + entity.Secretkey);

        int isOk = await _repository.AsUpdateable().SetColumns(it => new UserEntity()
        {
            Password = password,
            ChangePasswordDate = SqlFunc.GetDate(),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);

        // 重置该用户密码，该用户会立即退出登录
        var onlineCacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId);
        var list = await _cacheManager.GetAsync<List<UserOnlineModel>>(onlineCacheKey);
        var user = list?.Find(it => it.tenantId == _userManager.TenantId && it.userId == id);
        if (user != null)
        {
            await _imHandler.SendMessageAsync(user.connectionId, new { method = "logout", msg = "密码已变更，请重新登录！" }.ToJsonString());

            // 删除在线用户ID
            list.RemoveAll((x) => x.connectionId == user.connectionId);
            await _cacheManager.SetAsync(onlineCacheKey, list);

            // 删除用户登录信息缓存
            var cacheKey = string.Format("{0}:{1}:{2}", _userManager.TenantId, CommonConst.CACHEKEYUSER, user.userId);
            await _cacheManager.DelAsync(cacheKey);
        }

        // 单点登录同步
        entity.Password = input.userPassword;
        await syncUserInfo(entity, "modifyPassword", _userManager.TenantId);
    }

    /// <summary>
    /// 解除锁定.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Unlock")]
    public async Task Unlock(string id)
    {
        UserEntity? entity = await _repository.GetFirstAsync(u => u.Id == id && u.DeleteMark == null);
        if (!_userManager.DataScope.Any(it => it.organizeId == entity.OrganizeId && it.Edit) && !_userManager.IsAdministrator)
            throw Oops.Oh(ErrorCode.D1013);
        int isOk = await _repository.AsUpdateable().SetColumns(it => new UserEntity()
        {
            LockMark = 0, // 解锁
            LogErrorCount = 0, // 解锁
            EnabledMark = 1, // 解锁
            UnLockTime = DateTime.Now, // 取消解锁时间
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandAsync();

        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5005);
    }

    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] UserExportDataInput input)
    {
        // 用户信息列表
        List<UserListImportDataInput>? userList = new List<UserListImportDataInput>();
        var dictionaryTypeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "certificateType" && x.DeleteMark == null);
        var dictionaryTypeEntity1 = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "Education" && x.DeleteMark == null);
        var dictionaryTypeEntity2 = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "sex" && x.DeleteMark == null);
        var dictionaryTypeEntity3 = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "Nation" && x.DeleteMark == null);
        ISugarQueryable<UserListImportDataInput>? sqlQuery = _repository.AsSugarClient().Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId)))
            .WhereIF(!string.IsNullOrWhiteSpace(input.organizeId), a => a.OrganizeId == input.organizeId) // 组织机构
            .WhereIF(!input.Keyword.IsNullOrEmpty(), a => a.Account.Contains(input.Keyword) || a.RealName.Contains(input.Keyword))
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(a => new UserListImportDataInput()
            {
                id = a.Id,
                account = a.Account,
                realName = a.RealName,
                birthday = SqlFunc.ToString(a.Birthday),
                certificatesNumber = a.CertificatesNumber,
                managerId = SqlFunc.Subqueryable<UserEntity>().Where(e => e.Id == a.ManagerId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                organizeId = a.OrganizeId, // 组织结构
                positionId = a.PositionId, // 岗位
                roleId = a.RoleId, // 多角色
                certificatesType = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == dictionaryTypeEntity.Id && d.Id == a.CertificatesType).Select(b => b.FullName),
                education = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == dictionaryTypeEntity1.Id && d.Id == a.Education).Select(b => b.FullName),
                gender = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == dictionaryTypeEntity2.Id && d.EnCode == SqlFunc.ToString(a.Gender)).Select(b => b.FullName),
                nation = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == dictionaryTypeEntity3.Id && d.Id == a.Nation).Select(b => b.FullName),
                description = a.Description,
                entryDate = SqlFunc.ToString(a.EntryDate),
                email = a.Email,
                enabledMark = SqlFunc.IF(a.EnabledMark.Equals(0)).Return("禁用").ElseIF(a.EnabledMark.Equals(1)).Return("正常").End("锁定"),
                mobilePhone = a.MobilePhone,
                nativePlace = a.NativePlace,
                postalAddress = a.PostalAddress,
                telePhone = a.TelePhone,
                urgentContacts = a.UrgentContacts,
                urgentTelePhone = a.UrgentTelePhone,
                landline = a.Landline,
                sortCode = a.SortCode.ToString()
            });

        if (input.dataType == "0") userList = await sqlQuery.ToPageListAsync(input.CurrentPage, input.PageSize);
        else userList = await sqlQuery.ToListAsync();

        userList.ForEach(item =>
        {
            if (item.birthday.IsNotEmptyOrNull()) item.birthday = Convert.ToDateTime(item.birthday).ToString("yyyy-MM-dd HH:mm:ss");
            if (item.entryDate.IsNotEmptyOrNull()) item.entryDate = Convert.ToDateTime(item.entryDate).ToString("yyyy-MM-dd HH:mm:ss");
        });

        List<OrganizeEntity>? olist = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).OrderBy(a => a.CreatorTime, OrderByType.Asc).ToListAsync(); // 获取所有组织
        List<PositionEntity>? plist = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).ToListAsync(); // 获取所有岗位
        List<RoleEntity>? rlist = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(it => it.EnabledMark == 1 && it.DeleteMark == null).ToListAsync(); // 获取所有角色

        // 转换 组织结构 和 岗位(多岗位)
        foreach (UserListImportDataInput? item in userList)
        {
            // 获取用户组织关联数据
            List<string>? orgRelList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Organize" && x.UserId == item.id).Select(x => x.ObjectId).ToListAsync();

            if (orgRelList.Any())
            {
                List<OrganizeEntity>? oentityList = olist.Where(x => orgRelList.Contains(x.Id)).ToList();
                if (oentityList.Any())
                {
                    List<string>? userOrgList = new List<string>();
                    oentityList.ForEach(oentity =>
                    {
                        List<string>? oidList = oentity.OrganizeIdTree?.Split(',').ToList();
                        if (oidList != null)
                        {
                            List<string>? oNameList = olist.Where(x => oidList.Contains(x.Id)).Select(x => x.FullName).ToList();
                            userOrgList.Add(string.Join("/", oNameList));
                        }
                        else
                        {
                            List<string>? oNameList = new List<string>();
                            oNameList.Add(oentity.FullName);

                            // 递归获取上级组织
                            GetOrganizeParentName(olist, oentity.ParentId, oNameList);
                            userOrgList.Add(string.Join("/", oNameList));
                        }
                    });
                    item.organizeId = string.Join(";", userOrgList);
                }
            }
            else
            {
                item.organizeId = string.Empty;
            }

            // 获取用户岗位关联
            List<string>? posRelList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Position" && x.UserId == item.id).Select(x => x.ObjectId).ToListAsync();
            if (posRelList.Any())
                item.positionId = string.Join(";", plist.Where(x => posRelList.Contains(x.Id)).Select(x => x.FullName + "/" + x.EnCode).ToList());
            else
                item.positionId = string.Empty;

            // 角色
            if (item.roleId.IsNotEmptyOrNull())
            {
                List<string>? ridList = item.roleId.Split(',').ToList();
                item.roleId = string.Join(";", rlist.Where(x => ridList.Contains(x.Id)).Select(x => x.FullName).ToList());
            }
        }

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = string.Format("{0:yyyy-MM-dd}_用户信息.xls", DateTime.Now);
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle(input.selectKey.Split(',').ToList()))
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<UserListImportDataInput>.ExportMemoryStream(userList, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag)
        {
            fs.Flush();
            fs.Close();
        }

        _cacheManager.Set(excelconfig.FileName, string.Empty);
        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "Poxiao") };
    }

    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public async Task<dynamic> TemplateDownload()
    {
        // 初始化 一条空数据 
        List<UserListImportDataInput>? dataList = new List<UserListImportDataInput>() { new UserListImportDataInput() { } };

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "用户信息导入模板.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (!(await _fileManager.ExistsFile(addPath)))
        {
            var stream = ExcelExportHelper<UserListImportDataInput>.ToStream(dataList, excelconfig);
            await _fileManager.UploadFileByType(stream, FileVariable.TemporaryFilePath, excelconfig.FileName);
        }
        _cacheManager.Set(excelconfig.FileName, string.Empty);
        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "Poxiao") };
    }

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Uploader")]
    public async Task<dynamic> Uploader(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);
        return new { name = _fileName, url = string.Format("/api/File/Image/{0}/{1}", string.Empty, _fileName) };
    }

    /// <summary>
    /// 导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        try
        {
            Dictionary<string, string>? FileEncode = GetUserInfoFieldToTitle();

            string? filePath = FileVariable.TemporaryFilePath;
            string? savePath = Path.Combine(filePath, fileName);

            // 得到数据
            var sr = await _fileManager.GetFileStream(savePath);
            global::System.Data.DataTable? excelData = ExcelImportHelper.ToDataTable(savePath, sr);
            foreach (object? item in excelData.Columns)
            {
                excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
            }

            if (excelData.Rows.Count > 0) excelData.Rows.RemoveAt(0);

            // 返回结果
            return new { dataRow = excelData };
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.D1801);
        }
    }

    /// <summary>
    /// 导出错误报告.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ExportExceptionData")]
    [UnitOfWork]
    public async Task<dynamic> ExportExceptionData([FromBody] UserImportDataInput list)
    {
        object[]? res = await ImportUserData(list.list);

        // 错误数据
        List<UserListImportDataInput>? errorlist = res.Last() as List<UserListImportDataInput>;

        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "用户导入错误报告.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (KeyValuePair<string, string> item in GetUserInfoFieldToTitle())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        ExcelExportHelper<UserListImportDataInput>.Export(errorlist, excelconfig, addPath);

        _cacheManager.Set(excelconfig.FileName, string.Empty);
        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "Poxiao") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    [UnitOfWork]
    public async Task<dynamic> ImportData([FromBody] UserImportDataInput list)
    {
        object[]? res = await ImportUserData(list.list);
        List<UserEntity>? addlist = res.First() as List<UserEntity>;
        List<UserListImportDataInput>? errorlist = res.Last() as List<UserListImportDataInput>;
        return new UserImportResultOutput() { snum = addlist.Count, fnum = errorlist.Count, failResult = errorlist, resultType = errorlist.Count < 1 ? 0 : 1 };
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取用户信息 根据用户ID.
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    [NonAction]
    public UserEntity GetInfoByUserId(string userId)
    {
        return _repository.GetFirst(u => u.Id == userId && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户信息 根据用户ID.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByUserIdAsync(string userId)
    {
        return await _repository.GetFirstAsync(u => u.Id == userId && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetList()
    {
        return await _repository.AsQueryable().Where(u => u.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 获取用户信息 根据用户账户.
    /// </summary>
    /// <param name="account">用户账户.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByAccount(string account)
    {
        return await _repository.GetFirstAsync(u => u.Account == account && u.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户信息 根据登录信息.
    /// </summary>
    /// <param name="account">用户账户.</param>
    /// <param name="password">用户密码.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetInfoByLogin(string account, string password)
    {
        return await _repository.GetFirstAsync(u => u.Account == account && u.Password == password && u.DeleteMark == null);
    }

    /// <summary>
    /// 根据用户姓名获取用户ID.
    /// </summary>
    /// <param name="realName">用户姓名.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetUserIdByRealName(string realName)
    {
        return (await _repository.GetFirstAsync(u => u.RealName == realName && u.DeleteMark == null)).Id;
    }

    /// <summary>
    /// 获取用户名.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否显示账号.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetUserName(string userId, bool isAccount = true)
    {
        UserEntity? entity = await _repository.GetFirstAsync(x => x.Id == userId && x.DeleteMark == null);
        if (entity.IsNullOrEmpty()) return string.Empty;
        return isAccount ? entity.RealName + "/" + entity.Account : entity.RealName;
    }

    /// <summary>
    /// 获取当前用户岗位信息.
    /// </summary>
    /// <param name="PositionIds"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<PositionInfoModel>> GetPosition(string PositionIds)
    {
        string[]? ids = PositionIds.Split(",");
        return await _repository.AsSugarClient().Queryable<PositionEntity>().In(it => it.Id, ids).Select(it => new PositionInfoModel { id = it.Id, name = it.FullName }).ToListAsync();
    }

    /// <summary>
    /// 表达式获取用户.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<UserEntity> GetUserByExp(Expression<Func<UserEntity, bool>> expression)
    {
        return await _repository.GetFirstAsync(expression);
    }

    /// <summary>
    /// 表达式获取用户列表.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetUserListByExp(Expression<Func<UserEntity, bool>> expression)
    {
        return await _repository.AsQueryable().Where(expression).ToListAsync();
    }

    /// <summary>
    /// 表达式获取指定字段的用户列表.
    /// </summary>
    /// <param name="expression">where 条件表达式.</param>
    /// <param name="select">select 选择字段表达式.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserEntity>> GetUserListByExp(Expression<Func<UserEntity, bool>> expression, Expression<Func<UserEntity, UserEntity>> select)
    {
        return await _repository.AsQueryable().Where(expression).Select(select).ToListAsync();
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取集合中的组织 树,根据上级ID.
    /// </summary>
    /// <param name="list">组织 集合.</param>
    /// <param name="parentId">上级ID.</param>
    /// <param name="addList">返回.</param>
    /// <returns></returns>
    private List<string> GetOrganizeParentName(List<OrganizeEntity> list, string parentId, List<string> addList)
    {
        OrganizeEntity? entity = list.Find(x => x.Id == parentId);

        if (entity.ParentId != "-1") GetOrganizeParentName(list, entity.ParentId, addList);
        else addList.Add(entity.FullName);

        return addList;
    }

    /// <summary>
    /// 是否我的下属.
    /// </summary>
    /// <param name="userId">当前用户.</param>
    /// <param name="managerId">主管ID.</param>
    /// <param name="tier">层级.</param>
    /// <returns></returns>
    private async Task<bool> GetIsMyStaff(string userId, string managerId, int tier)
    {
        bool isMyStaff = false;
        if (tier <= 0) return true;
        string? superiorUserId = (await _repository.GetFirstAsync(it => it.Id.Equals(managerId) && it.DeleteMark == null))?.ManagerId;
        if (superiorUserId == null)
        {
            isMyStaff = false;
        }
        else if (userId == superiorUserId)
        {
            isMyStaff = true;
        }
        else
        {
            tier--;
            isMyStaff = await GetIsMyStaff(userId, superiorUserId, tier);
        }

        return isMyStaff;
    }

    /// <summary>
    /// 用户信息 字段对应 列名称.
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> GetUserInfoFieldToTitle(List<string> fields = null)
    {
        Dictionary<string, string>? res = new Dictionary<string, string>();
        res.Add("account", "账户");
        res.Add("realName", "姓名");
        res.Add("gender", "性别");
        res.Add("email", "电子邮箱");
        res.Add("organizeId", "所属组织");
        res.Add("managerId", "直属主管");
        res.Add("positionId", "岗位");
        res.Add("roleId", "角色");
        res.Add("sortCode", "排序");
        res.Add("enabledMark", "状态");
        res.Add("description", "说明");
        res.Add("nation", "民族");
        res.Add("nativePlace", "籍贯");
        res.Add("entryDate", "入职时间");
        res.Add("certificatesType", "证件类型");
        res.Add("certificatesNumber", "证件号码");
        res.Add("education", "文化程度");
        res.Add("birthday", "出生年月");
        res.Add("telePhone", "办公电话");
        res.Add("landline", "办公座机");
        res.Add("mobilePhone", "手机号码");
        res.Add("urgentContacts", "紧急联系");
        res.Add("urgentTelePhone", "紧急电话");
        res.Add("postalAddress", "通讯地址");

        if (fields == null || !fields.Any()) return res;

        Dictionary<string, string>? result = new Dictionary<string, string>();

        foreach (KeyValuePair<string, string> item in res)
        {
            if (fields.Contains(item.Key))
                result.Add(item.Key, item.Value);
        }

        return result;
    }

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportUserData(List<UserListImportDataInput> list)
    {
        List<UserListImportDataInput> userInputList = list;

        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        // 必填字段验证 (账号，姓名，性别，所属组织)
        List<UserListImportDataInput>? errorList = userInputList
            .Where(x => !x.account.IsNotEmptyOrNull() || !x.realName.IsNotEmptyOrNull() || !x.gender.IsNotEmptyOrNull() || !x.organizeId.IsNotEmptyOrNull()).ToList();

        // 上传重复的账号
        userInputList.ForEach(item =>
        {
            if (userInputList.Count(x => x.account == item.account) > 1)
            {
                var errorItems = userInputList.Where(x => x.account == item.account).ToList();
                errorItems.Remove(errorItems.First());
                errorList.AddRange(errorItems);
            }
        });

        errorList = errorList.Distinct().ToList();
        userInputList = userInputList.Except(errorList).ToList();

        // 用户账号 (匹配直属主管 和 验证重复账号)
        List<UserEntity>? _userRepositoryList = await _repository.AsQueryable().Where(it => it.DeleteMark == null).ToListAsync();

        // 已存在的账号
        List<UserEntity>? repeat = _userRepositoryList.Where(u => userInputList.Select(x => x.account).Contains(u.Account)).ToList();

        // 已存在的账号 列入 错误列表
        if (repeat.Any()) errorList.AddRange(userInputList.Where(u => repeat.Select(x => x.Account).Contains(u.account)));

        userInputList = userInputList.Except(errorList).ToList();

        #endregion

        List<UserEntity>? userList = new List<UserEntity>();

        #region 预处理关联表数据

        // 组织机构
        List<OrganizeEntity>? _organizeServiceList = await _organizeService.GetListAsync();
        Dictionary<string, string>? organizeDic = new Dictionary<string, string>();

        _organizeServiceList.ForEach(item =>
        {
            if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
            var orgNameList = new List<string>();
            item.OrganizeIdTree.Split(",").ToList().ForEach(it =>
            {
                var org = _organizeServiceList.Find(x => x.Id == it);
                if (org != null) orgNameList.Add(org.FullName);
            });
            organizeDic.Add(item.Id, string.Join("/", orgNameList));
        });

        List<PositionEntity>? _positionRepositoryList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null).ToListAsync(); // 岗位
        List<RoleEntity>? _roleRepositoryList = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.DeleteMark == null).ToListAsync(); // 角色

        DictionaryTypeEntity? typeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "963255a34ea64a2584c5d1ba269c1fe6" || x.EnCode == "sex") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _genderList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 性别

        typeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "b6cd65a763fa45eb9fe98e5057693e40" || x.EnCode == "Nation") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? _nationList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 民族

        typeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "7866376d5f694d4d851c7164bd00ebfc" || x.EnCode == "certificateType") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? certificateTypeList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 证件类型

        typeEntity = await _repository.AsSugarClient().Queryable<DictionaryTypeEntity>().Where(x => (x.Id == "6a6d6fb541b742fbae7e8888528baa16" || x.EnCode == "Education") && x.DeleteMark == null).FirstAsync();
        List<DictionaryDataEntity>? educationList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(d => d.DictionaryTypeId == typeEntity.Id && d.DeleteMark == null).ToListAsync(); // 文化程度

        #endregion

        // 用户关系数据
        List<UserRelationEntity>? userRelationList = new List<UserRelationEntity>();
        foreach (UserListImportDataInput? item in userInputList)
        {
            List<string>? orgIds = new List<string>(); // 多组织 , 号隔开
            List<string>? posIds = new List<string>(); // 多岗位 , 号隔开

            UserEntity? uentity = new UserEntity();
            uentity.Id = SnowflakeIdHelper.NextId();
            if (string.IsNullOrEmpty(uentity.HeadIcon)) uentity.HeadIcon = "001.png";
            uentity.Secretkey = Guid.NewGuid().ToString();
            uentity.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(CommonConst.DEFAULTPASSWORD) + uentity.Secretkey); // 初始化密码
            uentity.ManagerId = _userRepositoryList.Find(x => x.Account == item.managerId?.Split('/').LastOrDefault())?.Id; // 寻找主管

            // 寻找角色
            if (item.roleId.IsNotEmptyOrNull() && item.roleId.Split(";").Any())
                uentity.RoleId = string.Join(",", _roleRepositoryList.Where(r => item.roleId.Split(";").Contains(r.FullName)).Select(x => x.Id).ToList());

            // 寻找组织
            string[]? userOidList = item.organizeId.Split(";");
            if (userOidList.Any())
            {
                foreach (string? oinfo in userOidList)
                {
                    if (organizeDic.ContainsValue(oinfo)) orgIds.Add(organizeDic.Where(x => x.Value == oinfo).FirstOrDefault().Key);
                }
            }
            else
            {
                // 如果未找到组织，列入错误列表
                errorList.Add(item);
                continue;
            }

            // 如果未找到组织，列入错误列表
            if (!orgIds.Any())
            {
                errorList.Add(item);
                continue;
            }

            // 寻找岗位
            item.positionId?.Split(';').ToList().ForEach(it =>
            {
                string[]? pinfo = it.Split("/");
                string? pid = _positionRepositoryList.Find(x => x.FullName == pinfo.FirstOrDefault() && x.EnCode == pinfo.LastOrDefault())?.Id;
                if (pid.IsNotEmptyOrNull()) posIds.Add(pid); // 多岗位
            });

            // 性别
            if (_genderList.Find(x => x.FullName == item.gender) != null) uentity.Gender = _genderList.Find(x => x.FullName == item.gender).EnCode.ParseToInt();
            else uentity.Gender = _genderList.Find(x => x.FullName == "保密").EnCode.ParseToInt();

            uentity.Nation = _nationList.Find(x => x.FullName == item.nation)?.Id; // 民族
            uentity.Education = educationList.Find(x => x.FullName == item.education)?.Id; // 文化程度
            uentity.CertificatesType = certificateTypeList.Find(x => x.FullName == item.certificatesType)?.Id; // 证件类型
            uentity.Account = item.account;
            uentity.Birthday = item.birthday.IsNotEmptyOrNull() ? item.birthday.ParseToDateTime() : null;
            uentity.CertificatesNumber = item.certificatesNumber;
            uentity.CreatorUserId = _userManager.UserId;
            uentity.CreatorTime = DateTime.Now;
            uentity.Description = item.description;
            uentity.Email = item.email;
            switch (item.enabledMark)
            {
                case "禁用":
                    uentity.EnabledMark = 0;
                    break;
                case "正常":
                    uentity.EnabledMark = 1;
                    break;
                case "锁定":
                default:
                    uentity.EnabledMark = 2;
                    break;
            }

            uentity.EntryDate = item.entryDate.IsNotEmptyOrNull() ? item.entryDate.ParseToDateTime() : null;
            uentity.Landline = item.landline;
            uentity.MobilePhone = item.mobilePhone;
            uentity.NativePlace = item.nativePlace;
            uentity.PostalAddress = item.postalAddress;
            uentity.RealName = item.realName;
            uentity.SortCode = item.sortCode.ParseToInt();
            uentity.TelePhone = item.telePhone;
            uentity.UrgentContacts = item.urgentContacts;
            uentity.UrgentTelePhone = item.urgentTelePhone;

            #region 多组织 优先选择有权限组织

            uentity.OrganizeId = string.Empty;

            foreach (string? it in orgIds)
            {
                List<string>? UserRoleList = await _userManager.GetUserOrgRoleIds(uentity.RoleId, it);

                // 如果该组织下有角色并且有角色权限 则为默认组织
                if (UserRoleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && UserRoleList.Contains(x.ObjectId)).Any())
                {
                    uentity.OrganizeId = it; // 多 组织 默认
                    break;
                }
            }

            if (uentity.OrganizeId.IsNullOrEmpty()) // 如果所选组织下都没有角色或者没有角色权限 默认取第一个
                uentity.OrganizeId = orgIds.FirstOrDefault();

            #endregion

            // 岗位多组织 匹配
            var opIds = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null && orgIds.Contains(x.OrganizeId)).Select(x => x.Id).ToListAsync();
            posIds = opIds.Intersect(posIds).ToList();

            if (uentity.OrganizeId.IsNotEmptyOrNull())
            {
                List<UserRelationEntity>? roleList = _userRelationService.CreateUserRelation(uentity.Id, uentity.RoleId, "Role"); // 角色关系
                List<UserRelationEntity>? positionList = _userRelationService.CreateUserRelation(uentity.Id, string.Join(",", posIds), "Position"); // 岗位关系
                List<UserRelationEntity>? organizeList = _userRelationService.CreateUserRelation(uentity.Id, string.Join(",", orgIds), "Organize"); // 组织关系
                userRelationList.AddRange(positionList);
                userRelationList.AddRange(roleList);
                userRelationList.AddRange(organizeList);
            }

            userList.Add(uentity);
        }

        if (userList.Any())
        {
            try
            {
                // 新增用户记录
                UserEntity? newEntity = await _repository.AsInsertable(userList).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();

                // 批量新增用户关系
                if (userRelationList.Count > 0) await _userRelationService.Create(userRelationList);

            }
            catch (Exception)
            {
                errorList.AddRange(userInputList);
                userInputList = new List<UserListImportDataInput>();
            }
        }

        return new object[] { userList, errorList };
    }

    /// <summary>
    /// 获取机构成员列表.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<OrganizeMemberListOutput>> GetOrganizeMemberList(string organizeId)
    {
        // 获取分级管理组织
        var dataScope = _repository.AsSugarClient().Queryable<OrganizeAdministratorEntity>()
            .Where(it => SqlFunc.ToString(it.UserId) == _userManager.UserId && it.DeleteMark == null)
            .Where(it => it.ThisLayerSelect.Equals(1) || it.SubLayerSelect.Equals(1)
            || it.ThisLayerAdd.Equals(1) || it.SubLayerAdd.Equals(1)
            || it.ThisLayerDelete.Equals(1) || it.SubLayerDelete.Equals(1)
            || it.ThisLayerEdit.Equals(1) || it.SubLayerEdit.Equals(1)).ToList();

        var thisLayer = dataScope.Where(it => it.ThisLayerSelect.Equals(1) || it.ThisLayerAdd.Equals(1) || it.ThisLayerDelete.Equals(1) || it.ThisLayerEdit.Equals(1)).ToList();
        var subLayer = dataScope.Where(it => it.SubLayerSelect.Equals(1) || it.SubLayerAdd.Equals(1) || it.SubLayerDelete.Equals(1) || it.SubLayerEdit.Equals(1)).ToList();

        List<OrganizeMemberListOutput>? output = new List<OrganizeMemberListOutput>();

        if (organizeId.Equals("0"))
        {
            List<OrganizeEntity>? data = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1)
                .WhereIF(thisLayer.Any(), x => thisLayer.Select(x => x.OrganizeId).Contains(x.Id))
                .WhereIF(!thisLayer.Any(), x => thisLayer.Select(x => x.OrganizeId).Contains(x.Id)).OrderBy(o => o.SortCode).ToListAsync();

            if (subLayer.Any())
            {
                subLayer.ForEach(item =>
                {
                    var itemRes = _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1)
                   .Where(x => x.OrganizeIdTree.Contains(item.OrganizeId) && !x.Id.Equals(item.OrganizeId)).OrderBy(o => o.SortCode).ToList();
                    data.AddRange(itemRes);
                });
            }

            data.ForEach(o =>
            {
                output.Add(new OrganizeMemberListOutput
                {
                    Id = o.Id,
                    fullName = o.FullName,
                    enabledMark = o.EnabledMark,
                    type = o.Category,
                    icon = o.Category.Equals("company") ? "icon-ym icon-ym-tree-organization3" : "icon-ym icon-ym-tree-department1",
                    organizeIdTree = o.OrganizeIdTree,
                    HasChildren = true,
                    IsLeaf = false
                });
            });
        }
        else
        {
            var userRelationList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType.Equals("Organize") && x.ObjectId.Equals(organizeId)).Select(x => x.UserId).ToListAsync();
            List<UserEntity>? userList = await _repository.AsSugarClient().Queryable<UserEntity>()
                .Where(u => userRelationList.Contains(u.Id) && u.EnabledMark > 0 && u.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
            userList.ForEach(u =>
            {
                output.Add(new OrganizeMemberListOutput()
                {
                    Id = u.Id,
                    fullName = u.RealName + "/" + u.Account,
                    enabledMark = u.EnabledMark,
                    type = "user",
                    icon = "icon-ym icon-ym-tree-user2",
                    headIcon = "/api/File/Image/userAvatar/" + u.HeadIcon,
                    HasChildren = false,
                    IsLeaf = true
                });
            });

            List<OrganizeEntity>? departmentList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1)
                .Where(x => x.ParentId.Equals(organizeId))
                .WhereIF(thisLayer.Any(), x => thisLayer.Select(x => x.OrganizeId).Contains(x.Id))
                .WhereIF(!thisLayer.Any(), x => thisLayer.Select(x => x.OrganizeId).Contains(x.Id)).OrderBy(o => o.SortCode).ToListAsync();

            if (subLayer.Any())
            {
                subLayer.ForEach(item =>
                {
                    var itemRes = _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(o => o.DeleteMark == null && o.EnabledMark == 1)
                   .Where(x => x.OrganizeIdTree.Contains(item.OrganizeId) && !x.Id.Equals(item.OrganizeId) && x.OrganizeIdTree.Contains(organizeId)).OrderBy(o => o.SortCode).ToList();
                    departmentList.AddRange(itemRes);
                });
            }

            departmentList.ForEach(o =>
            {
                output.Add(new OrganizeMemberListOutput()
                {
                    Id = o.Id,
                    fullName = o.FullName,
                    enabledMark = o.EnabledMark,
                    type = o.Category,
                    icon = o.Category.Equals("company") ? "icon-ym icon-ym-tree-organization3" : "icon-ym icon-ym-tree-department1",
                    HasChildren = true,
                    organizeIdTree = o.OrganizeIdTree,
                    IsLeaf = false
                });
            });
        }

        if (!organizeId.Equals("0")) output.RemoveAll(x => x.Id.Equals(organizeId));

        // 获取组织树
        var orgTree = _organizeService.GetOrgListTreeName();

        // 组织断层处理
        output.Where(x => x.ParentId != "-1" && x.organizeIdTree.IsNotEmptyOrNull()).ToList().ForEach(item =>
        {
            item.fullName = orgTree.Find(x => x.Id.Equals(item.Id)).Description;
            if (!output.Any(x => x.Id.Equals(item.ParentId)))
            {
                var pItem = output.Where(x => x.organizeIdTree.IsNotEmptyOrNull() && x.Id != item.Id && item.organizeIdTree.Contains(x.organizeIdTree)).FirstOrDefault();
                if (pItem != null)
                {
                    item.ParentId = pItem.Id;
                    item.fullName = item.fullName.Replace(pItem.fullName + "/", string.Empty);
                }
                else
                {
                    item.ParentId = "-1";
                }
            }
            else
            {
                var pItem = output.Find(x => x.Id.Equals(item.ParentId));
                item.fullName = item.fullName.Replace(pItem.fullName + "/", string.Empty);
            }
        });

        output.RemoveAll(x => x.type != "user" && x.ParentId != "-1");

        if (!organizeId.Equals("0"))
        {
            var pOrgTreeName = orgTree.Find(x => x.Id.Equals(organizeId)).Description;
            output.ForEach(item => item.fullName = item.fullName.Replace(pOrgTreeName + "/", string.Empty));
        }

        return output;
    }
    #endregion

    #region 单点登录 数据同步

    /// <summary>
    /// 同步数据导maxkey.
    /// </summary>
    /// <param name="userEntity"></param>
    /// <param name="method"></param>
    /// <param name="tenantId"></param>
    public async Task syncUserInfo(UserEntity userEntity, string method, string tenantId)
    {
        try
        {
            if (_oauthOptions.Enabled)
            {
                var userName = string.Format("{0}:{1}", _oauthOptions.Pull.UserName, _oauthOptions.Pull.Password).ToBase64String();

                // http调用结果
                HttpResponse execute = null;
                var map = parse(userEntity);
                tenantId = tenantId != null && tenantId.Length > 0 ? tenantId : "1";
                if (tenantId.Equals("default")) tenantId = "1";
                map.Add("instId", tenantId);

                // 得到userId
                //            String username = userEntity.get("id") != null && userEntity.get("id").toString().length() > 0 ? userEntity.get("id").toString() : null;

                Dictionary<string, object> jsonObject = null;
                var resString = string.Empty;
                var headers = new Dictionary<string, object>();
                headers.Add("Authorization", _oauthOptions.Pull.CredentialType + " " + userName);

                if (method.Equals("create"))
                {
                    resString = await (_oauthOptions.Pull.CreateRestAddress + "?appId=" + _oauthOptions.Pull.UserName).SetHeaders(headers).SetBody(map).PostAsStringAsync();
                }
                else if (method.Equals("update"))
                {
                    resString = await (_oauthOptions.Pull.ReplaceRestAddress + "?appId=" + _oauthOptions.Pull.UserName).SetHeaders(headers).SetBody(map).PutAsStringAsync();
                }
                else if (method.Equals("delete"))
                {
                    resString = await (_oauthOptions.Pull.DeleteRestAddress + "?appId=" + _oauthOptions.Pull.UserName).SetHeaders(headers).SetBody(map).DeleteAsStringAsync();
                }
                else if (method.Equals("modifyPassword"))
                {
                    resString = await (_oauthOptions.Pull.ChangePasswordRestAddress + "?appId=" + _oauthOptions.Pull.UserName).SetHeaders(headers).SetBody(map).PostAsStringAsync();
                }

                //            else if (method.Equals("modifyPassword")) {
                //                jsonObject = HttpUtil.httpRequest(_oauthOptions.Pull.getGetRestAddress() + username
                //                        , "GET"
                //                        , null, _oauthOptions.Pull.getCredentialType() + " " + _oauthOptions.Pull.getUserName() + "Og==" + _oauthOptions.Pull.getPassword()
                //                        , null);
                //            }
            }
        }
        catch (Exception e)
        {

        }
    }

    private Dictionary<string, object> parse(UserEntity userEntity)
    {
        var map = new Dictionary<string, object>();

        // map.Add("id", userEntity.get("id"));
        map.Add("username", userEntity.Account);
        map.Add("password", userEntity.Password);
        map.Add("mobile", userEntity.MobilePhone);
        map.Add("email", userEntity.Email);
        map.Add("gender", (int)userEntity.Gender == 1 ? 2 : 1);
        map.Add("createdBy", userEntity.CreatorUserId);
        map.Add("createdDate", userEntity.CreatorTime);
        map.Add("modifiedBy", userEntity.LastModifyUserId);
        map.Add("modifiedDate", userEntity.LastModifyTime);
        map.Add("displayName", userEntity.RealName);

        // map.Add("managerId", userEntity.get("managerId"));
        // map.Add("departmentId", userEntity.get("organizeId"));
        map.Add("loginCount", userEntity.LogSuccessCount);
        map.Add("badPasswordCount", userEntity.LogErrorCount);
        map.Add("lastLoginIp", userEntity.LastLogIP);
        map.Add("lastLoginTime", userEntity.LastLogTime);
        map.Add("status", userEntity.EnabledMark != null ? (userEntity.EnabledMark == 1 ? 1 : 4) : 4);
        return map;
    }

    #endregion
}