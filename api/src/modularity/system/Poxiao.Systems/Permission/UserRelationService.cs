using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Dto.User;
using Poxiao.Systems.Entitys.Dto.UserRelation;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 业务实现：用户关系.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "UserRelation", Order = 169)]
[Route("api/permission/[controller]")]
public class UserRelationService : IUserRelationService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserRelationEntity> _repository;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="UserRelationService"/>类型的新实例.
    /// </summary>
    public UserRelationService(
        ISqlSugarRepository<UserRelationEntity> userRelationRepository,
        IOrganizeService organizeService,
        IUserManager userManager)
    {
        _repository = userRelationRepository;
        _organizeService = organizeService;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 获取岗位/角色成员列表.
    /// </summary>
    /// <param name="objectId">岗位id或角色id.</param>
    /// <returns></returns>
    [HttpGet("{objectId}")]
    public async Task<dynamic> GetListByObjectId(string objectId)
    {
        return new { ids = await _repository.AsQueryable().Where(u => u.ObjectId == objectId).Select(s => s.UserId).ToListAsync() };
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建用户关系.
    /// </summary>
    /// <param name="objectId">功能主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{objectId}")]
    [UnitOfWork]
    public async Task Create(string objectId, [FromBody] UserRelationCrInput input)
    {
        #region 分级权限验证

        if (input.objectType.Equals("Role") && !_userManager.IsAdministrator)
        {
            RoleEntity? oldRole = await _repository.AsSugarClient().Queryable<RoleEntity>().FirstAsync(x => x.Id.Equals(objectId));
            if (oldRole.GlobalMark == 1) throw Oops.Oh(ErrorCode.D1612); // 全局角色 只能超管才能变更
        }

        if (input.objectType.Equals("Position") || input.objectType.Equals("Role"))
        {
            var orgIds = new List<string>();
            if (input.objectType.Equals("Position")) orgIds = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.Id.Equals(objectId)).Select(x => x.OrganizeId).ToListAsync();
            if (input.objectType.Equals("Role")) orgIds = await _repository.AsSugarClient().Queryable<OrganizeRelationEntity>().Where(x => x.ObjectId.Equals(objectId) && x.ObjectType == input.objectType).Select(x => x.OrganizeId).ToListAsync();

            if (!_userManager.DataScope.Any(it => orgIds.Contains(it.organizeId) && it.Edit) && !_userManager.IsAdministrator)
                throw Oops.Oh(ErrorCode.D1013); // 分级管控
        }

        #endregion

        List<string>? oldUserIds = await _repository.AsQueryable().Where(u => u.ObjectId.Equals(objectId) && u.ObjectType.Equals(input.objectType)).Select(s => s.UserId).ToListAsync();

        // 禁用用户不删除
        List<string>? disabledUserIds = await _repository.AsSugarClient().Queryable<UserEntity>().Where(u => u.EnabledMark == 0 && oldUserIds.Contains(u.Id)).Select(s => s.Id).ToListAsync();

        // 清空原有数据
        await _repository.DeleteAsync(u => u.ObjectId.Equals(objectId) && u.ObjectType.Equals(input.objectType) && !disabledUserIds.Contains(u.UserId));

        // 创建新数据
        List<UserRelationEntity>? dataList = new List<UserRelationEntity>();
        input.userIds.ForEach(item =>
        {
            dataList.Add(new UserRelationEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                CreatorTime = DateTime.Now,
                CreatorUserId = _userManager.UserId,
                UserId = item,
                ObjectType = input.objectType,
                ObjectId = objectId,
                SortCode = input.userIds.IndexOf(item)
            });
        });
        if (dataList.Count > 0)
            await _repository.AsInsertable(dataList).ExecuteCommandAsync();

        // 修改用户
        // 计算旧用户数组与新用户数组差
        List<string>? addList = input.userIds.Except(oldUserIds).ToList();
        List<string>? delList = oldUserIds.Except(input.userIds).ToList();
        delList = delList.Except(disabledUserIds).ToList();

        // 处理新增用户岗位
        if (addList.Count > 0)
        {
            List<UserEntity>? addUserList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(u => addList.Contains(u.Id)).ToListAsync();
            addUserList.ForEach(async item =>
            {
                if (input.objectType.Equals("Position"))
                {
                    List<string>? idList = string.IsNullOrEmpty(item.PositionId) ? new List<string>() : item.PositionId.Split(',').ToList();
                    idList.Add(objectId);

                    #region 获取默认组织下的岗位

                    if (item.PositionId.IsNullOrEmpty())
                    {
                        List<string>? pIdList = await _repository.AsSugarClient().Queryable<PositionEntity>()
                        .Where(x => x.OrganizeId == item.OrganizeId && idList.Contains(x.Id)).Select(x => x.Id).ToListAsync();
                        item.PositionId = pIdList.FirstOrDefault(); // 多岗位 默认取第一个
                        item.LastModifyTime = DateTime.Now;
                        item.LastModifyUserId = _userManager.UserId;
                    }
                    #endregion
                }
                else if (input.objectType.Equals("Role"))
                {
                    List<string>? idList = string.IsNullOrEmpty(item.RoleId) ? new List<string>() : item.RoleId.Split(',').ToList();
                    idList.Add(objectId);
                    item.RoleId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(',');
                    item.LastModifyTime = DateTime.Now;
                    item.LastModifyUserId = _userManager.UserId;
                }
                else if (input.objectType.Equals("Group"))
                {
                    List<string>? idList = string.IsNullOrEmpty(item.GroupId) ? new List<string>() : item.GroupId.Split(',').ToList();
                    idList.Add(objectId);
                    item.GroupId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(',');
                }
            });
            await _repository.AsSugarClient().Updateable(addUserList).UpdateColumns(it => new { it.RoleId, it.PositionId, it.GroupId }).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        }

        // 移除用户
        if (delList.Count > 0)
        {
            List<UserEntity>? delUserList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(u => delList.Contains(u.Id)).ToListAsync();
            foreach (UserEntity? item in delUserList)
            {
                if (input.objectType.Equals("Position"))
                {
                    if (item.PositionId.IsNotEmptyOrNull())
                    {
                        List<string>? idList = item.PositionId.Split(',').ToList();
                        idList.RemoveAll(x => x == objectId);
                    }

                    #region 获取默认组织下的岗位

                    List<string>? pList = _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.OrganizeId == item.OrganizeId).Select(x => x.Id).ToList();
                    List<string>? pIdList = _repository.AsQueryable().Where(x => x.UserId == item.Id && x.ObjectType == "Position" && pList.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();

                    item.PositionId = pIdList.FirstOrDefault(); // 多岗位 默认取第一个

                    #endregion

                    _repository.AsSugarClient().Updateable<UserEntity>().SetColumns(it => new UserEntity()
                    {
                        PositionId = item.PositionId,
                        LastModifyTime = SqlFunc.GetDate(),
                        LastModifyUserId = _userManager.UserId
                    }).Where(it => it.Id == item.Id).ExecuteCommand();
                }
                else if (input.objectType.Equals("Role"))
                {
                    if (item.RoleId.IsNotEmptyOrNull())
                    {
                        List<string>? idList = item.RoleId.Split(',').ToList();
                        idList.RemoveAll(x => x == objectId);

                        #region 多组织 优先选择有权限组织

                        string? defaultOrgId = item.OrganizeId;
                        item.OrganizeId = string.Empty;

                        List<string>? roleList = await _userManager.GetUserOrgRoleIds(string.Join(",", idList), item.OrganizeId);

                        // 获取有菜单的系统 默认系统
                        var sysList = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && roleList.Contains(x.ObjectId))
                            .Where(x => x.ItemType == "module" || x.ItemType == "system").ToListAsync();
                        var menuList = await _repository.AsSugarClient().Queryable<ModuleEntity>()
                            .Where(x => sysList.Where(xx => xx.ItemType.Equals("system")).Select(xx => xx.ItemId).Contains(x.SystemId)).ToListAsync();
                        if (!menuList.Any(x => x.SystemId.Equals(item.SystemId)))
                            item.SystemId = menuList.Where(x => sysList.Where(xx => xx.ItemType.Equals("module")).Select(xx => xx.ItemId).Contains(x.Id)).FirstOrDefault()?.SystemId;

                        // 如果该组织下有角色并且有角色权限 则为默认组织
                        if (roleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                            item.OrganizeId = defaultOrgId; // 多组织 默认

                        if (item.OrganizeId.IsNullOrEmpty())
                        {
                            List<string>? orgList = await _repository.AsQueryable().Where(x => x.ObjectType == "Organize" && x.UserId == item.Id).Select(x => x.ObjectId).ToListAsync(); // 多组织
                            foreach (string? it in orgList)
                            {
                                roleList = await _userManager.GetUserOrgRoleIds(string.Join(",", idList), it);

                                // 如果该组织下有角色并且有角色权限 则为默认组织
                                if (roleList.Any() && _repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                                {
                                    item.OrganizeId = it; // 多组织 默认
                                    break;
                                }
                            }
                        }

                        if (item.OrganizeId.IsNullOrEmpty()) item.OrganizeId = defaultOrgId; // 如果所选组织下都没有角色或者没有角色权限 多组织 默认

                        // 获取默认组织下的岗位
                        List<string>? pList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.OrganizeId == item.OrganizeId && x.DeleteMark == null).Select(x => x.Id).ToListAsync();
                        List<string>? pIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                            .Where(x => x.UserId == item.Id && pList.Contains(x.ObjectId) && x.ObjectType == "Position").Select(x => x.ObjectId).ToListAsync();

                        if (!pIdList.Contains(item.PositionId)) item.PositionId = pIdList.FirstOrDefault(); // 多 岗位 默认取第一个
                        if (item.PositionId.IsNullOrEmpty()) item.PositionId = pIdList.FirstOrDefault();
                        #endregion

                        _repository.AsSugarClient().Updateable<UserEntity>().SetColumns(it => new UserEntity()
                        {
                            RoleId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(','),
                            LastModifyTime = SqlFunc.GetDate(),
                            OrganizeId = item.OrganizeId,
                            PositionId = item.PositionId,
                            LastModifyUserId = _userManager.UserId
                        }).Where(it => it.Id == item.Id).ExecuteCommand();
                    }
                }
                else if (input.objectType.Equals("Group"))
                {
                    if (item.GroupId.IsNotEmptyOrNull())
                    {
                        List<string>? idList = item.GroupId.Split(',').ToList();
                        idList.RemoveAll(x => x == objectId);
                        _repository.AsSugarClient().Updateable<UserEntity>().SetColumns(it => new UserEntity()
                        {
                            GroupId = string.Join(",", idList.ToArray()).TrimStart(',').TrimEnd(','),
                            LastModifyTime = SqlFunc.GetDate(),
                            LastModifyUserId = _userManager.UserId
                        }).Where(it => it.Id == item.Id).ExecuteCommand();
                    }
                }
            }
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 创建用户关系.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <param name="ids">对象ID组.</param>
    /// <param name="relationType">关系类型(岗位-Position;角色-Role;组织-Organize;分组-Group;).</param>
    /// <returns></returns>
    [NonAction]
    public List<UserRelationEntity> CreateUserRelation(string userId, string ids, string relationType)
    {
        List<UserRelationEntity> userRelationList = new List<UserRelationEntity>();
        if (!ids.IsNullOrEmpty())
        {
            List<string>? position = new List<string>(ids.Split(','));
            position.ForEach(item =>
            {
                UserRelationEntity? entity = new UserRelationEntity();
                entity.Id = SnowflakeIdHelper.NextId();
                entity.ObjectType = relationType;
                entity.ObjectId = item;
                entity.SortCode = position.IndexOf(item);
                entity.UserId = userId;
                entity.CreatorTime = DateTime.Now;
                entity.CreatorUserId = _userManager.UserId;
                userRelationList.Add(entity);
            });
        }

        return userRelationList;
    }

    /// <summary>
    /// 创建用户关系.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task Create(List<UserRelationEntity> input)
    {
        await _repository.AsInsertable(input).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">用户ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task Delete(string id)
    {

        await _repository.AsDeleteable().Where(u => u.UserId == id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据用户主键获取列表.
    /// </summary>
    /// <param name="userId">用户主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<UserRelationEntity>> GetListByUserId(string userId)
    {
        return await _repository.AsQueryable().Where(m => m.UserId == userId).OrderBy(o => o.CreatorTime).ToListAsync();
    }

    /// <summary>
    /// 获取用户.
    /// </summary>
    /// <param name="type">关系类型.</param>
    /// <param name="objId">对象ID.</param>
    /// <returns></returns>
    [NonAction]
    public List<string> GetUserId(string type, string objId)
    {
        return _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
                .Where((a, b) => a.ObjectType == type && a.ObjectId == objId && b.DeleteMark == null && b.EnabledMark > 0).Select(a => a.UserId).Distinct().ToList();
    }

    /// <summary>
    /// 获取用户.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="objId"></param>
    /// <returns></returns>
    [NonAction]
    public List<string> GetUserId(List<string> objId, string type = null)
    {
        if (objId.Any())
        {
            if (type.IsNotEmptyOrNull())
            {
                return _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
                .Where((a, b) => a.ObjectType == type && b.DeleteMark == null && (objId.Contains(a.ObjectId) || objId.Contains(a.UserId)) && b.EnabledMark > 0).Select(a => a.UserId).Distinct().ToList();
            }
            else
            {
                return _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
               .Where((a, b) => b.DeleteMark == null && (objId.Contains(a.ObjectId)|| objId.Contains(a.UserId)) && b.EnabledMark > 0).Select(a => a.UserId).Distinct().ToList();
            }
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 获取用户(分页).
    /// </summary>
    /// <param name="userIds">用户ID组.</param>
    /// <param name="objIds">对象ID组.</param>
    /// <param name="pageInputBase">分页参数.</param>
    /// <returns></returns>
    [NonAction]
    public dynamic GetUserPage(UserConditionInput input, ref bool hasCandidates)
    {
        SqlSugarPagedList<UserListOutput>? data = new SqlSugarPagedList<UserListOutput>();

        if (input.departIds == null) input.departIds = new List<string>();
        if (input.positionIds != null) input.departIds.AddRange(input.positionIds);
        if (input.roleIds != null) input.departIds.AddRange(input.roleIds);
        if (input.groupIds != null) input.departIds.AddRange(input.groupIds);

        data = _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
            .Where((a, b) => b.DeleteMark == null && (input.departIds.Contains(a.ObjectId) || input.userIds.Contains(b.Id)))
            .WhereIF(input.pagination.Keyword.IsNotEmptyOrNull(), (a, b) => b.Account.Contains(input.pagination.Keyword) || b.RealName.Contains(input.pagination.Keyword))
            .GroupBy((a, b) => new { Id = b.Id, Account = b.Account, RealName = b.RealName, Gender = b.Gender, MobilePhone = b.MobilePhone })
            .Select((a, b) => new UserListOutput()
            {
                id = b.Id,
                organizeId = SqlFunc.Subqueryable<UserEntity>().Where(e => e.Id == b.Id).Select(u => u.OrganizeId),
                account = b.Account,
                fullName = SqlFunc.MergeString(b.RealName, "/", b.Account),
                headIcon = SqlFunc.Subqueryable<UserEntity>().Where(e => e.Id == b.Id).Select(u => SqlFunc.MergeString("/api/File/Image/userAvatar/", u.HeadIcon)),
                gender = b.Gender,
                mobilePhone = b.MobilePhone
            }).ToPagedList(input.pagination.CurrentPage, input.pagination.PageSize);

        if (data.list.Any())
        {
            hasCandidates = true;
            var orgList = _organizeService.GetOrgListTreeName();

            // 获取所属组织的所有成员
            List<UserRelationEntity>? userList = _repository.AsSugarClient().Queryable<UserRelationEntity>()
                .Where(x => x.ObjectType == "Organize" && data.list.Select(x => x.id).Contains(x.UserId)).ToList();

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
    /// 新用户组件获取人员.
    /// </summary>
    /// <param name="Ids"></param>
    /// <returns></returns>
    public async Task<List<string>> GetUserId(List<string> Ids)
    {
        var userIdList = new List<string>();
        var objIdList = new List<string>();
        foreach (var item in Ids)
        {
            var strList = item.Split("--").ToList();
            if (strList[1].Equals("user"))
            {
                userIdList.Add(strList[0]);
            }
            else if (strList[1].Equals("department") || strList[1].Equals("company"))
            {
                objIdList = objIdList.Union(await _organizeService.GetChildOrgId(strList[0])).ToList();
            }
            else
            {
                objIdList.Add(strList[0]);
            }
        }
        var otherUserIds = _repository.AsSugarClient().Queryable<UserRelationEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.UserId == b.Id))
                .Where((a, b) => b.DeleteMark == null && objIdList.Contains(a.ObjectId) && b.EnabledMark == 1).Select(a => a.UserId).Distinct().ToList();
        return otherUserIds.Union(userIdList).ToList();
    }

    /// <summary>
    /// 强制角色下的所有用户下线.
    /// </summary>
    /// <param name="roleId">角色Id.</param>
    /// <returns></returns>
    public async Task ForcedOffline(string roleId)
    {
        // 查找该角色下的所有成员id
        var roleUserIds = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.ObjectType == "Role" && x.ObjectId == roleId).Select(x => x.UserId).ToListAsync();
        Scoped.Create((_, scope) =>
        {
            roleUserIds.ForEach(id =>
            {
                var services = scope.ServiceProvider;
                var _onlineuser = App.GetService<OnlineUserService>(services);
                _onlineuser.ForcedOffline(id);
            });
        });
    }
    #endregion
}