using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Security;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Dto.Permission.UsersCurrent;
using Poxiao.Systems.Entitys.Dto.UsersCurrent;
using Poxiao.Systems.Entitys.Entity.Permission;
using Poxiao.Systems.Entitys.Model.UsersCurrent;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 业务实现:个人资料.
/// </summary>
[ApiDescriptionSettings(Tag = "Permission", Name = "Current", Order = 168)]
[Route("api/permission/Users/[controller]")]
public class UsersCurrentService : IUsersCurrentService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;

    /// <summary>
    /// 操作权限服务.
    /// </summary>
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 组织管理.
    /// </summary>
    private readonly IOrganizeService _organizeService;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 系统配置.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 操作权限服务.
    /// </summary>
    private readonly OnlineUserService _onlineUserService;

    /// <summary>
    /// IM中心处理程序.
    /// </summary>
    private IMHandler _imHandler;

    /// <summary>
    /// 初始化一个<see cref="UsersCurrentService"/>类型的新实例.
    /// </summary>
    public UsersCurrentService(
        ISqlSugarRepository<UserEntity> userRepository,
        IAuthorizeService authorizeService,
        IOrganizeService organizeService,
        ICacheManager cacheManager,
        ISysConfigService sysConfigService,
        OnlineUserService onlineUserService,
        IUserManager userManager,
        IMHandler imHandler)
    {
        _repository = userRepository;
        _authorizeService = authorizeService;
        _organizeService = organizeService;
        _cacheManager = cacheManager;
        _sysConfigService = sysConfigService;
        _onlineUserService = onlineUserService;
        _userManager = userManager;
        _imHandler = imHandler;
    }

    #region GET

    /// <summary>
    /// 获取我的下属.
    /// </summary>
    /// <param name="id">用户Id.</param>
    /// <returns></returns>
    [HttpGet("Subordinate/{id}")]
    public async Task<dynamic> GetSubordinate(string id)
    {
        // 获取用户Id 下属 ,顶级节点为 自己
        List<string>? userIds = new List<string>();
        if (id == "0") userIds.Add(_userManager.UserId);
        else userIds = await _repository.AsQueryable().Where(m => m.ManagerId == id && m.DeleteMark == null).Select(m => m.Id).ToListAsync();

        if (userIds.Any())
        {
            return await _repository.AsSugarClient().Queryable<UserEntity, OrganizeEntity, PositionEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, b.Id == SqlFunc.ToString(a.OrganizeId), JoinType.Left, c.Id == SqlFunc.ToString(a.PositionId)))
                .WhereIF(userIds.Any(), a => userIds.Contains(a.Id))
                .Where(a => a.DeleteMark == null && a.EnabledMark == 1)
                .OrderBy(a => a.SortCode)
                .Select((a, b, c) => new UsersCurrentSubordinateOutput
                {
                    id = a.Id,
                    avatar = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
                    userName = SqlFunc.MergeString(a.RealName, "/", a.Account),
                    isLeaf = false,
                    department = b.FullName,
                    position = c.FullName
                })
                .ToListAsync();
        }
        else
        {
            return new List<UsersCurrentSubordinateOutput>();
        }
    }

    /// <summary>
    /// 获取个人资料.
    /// </summary>
    /// <returns></returns>
    [HttpGet("BaseInfo")]
    public async Task<dynamic> GetBaseInfo()
    {
        UsersCurrentInfoOutput? data = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => x.Id.Equals(_userManager.UserId))
            .Select(a => new UsersCurrentInfoOutput
            {
                id = a.Id,
                account = SqlFunc.IIF(KeyVariable.MultiTenancy == true, SqlFunc.MergeString(_userManager.TenantId, "@", a.Account), a.Account),
                realName = a.RealName,
                position = string.Empty,
                positionId = a.PositionId,
                organizeId = a.OrganizeId,
                manager = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id.Equals(a.ManagerId)).Select(x => SqlFunc.MergeString(x.RealName, "/", x.Account)),
                roleId = string.Empty,
                roleIds = a.RoleId,
                creatorTime = a.CreatorTime,
                prevLogTime = a.PrevLogTime,
                signature = a.Signature,
                gender = a.Gender.ToString(),
                nation = a.Nation,
                nativePlace = a.NativePlace,
                entryDate = a.EntryDate,
                certificatesType = a.CertificatesType,
                certificatesNumber = a.CertificatesNumber,
                education = a.Education,
                birthday = a.Birthday,
                telePhone = a.TelePhone,
                landline = a.Landline,
                mobilePhone = a.MobilePhone,
                email = a.Email,
                urgentContacts = a.UrgentContacts,
                urgentTelePhone = a.UrgentTelePhone,
                postalAddress = a.PostalAddress,
                theme = a.Theme,
                language = a.Language,
                avatar = SqlFunc.IIF(SqlFunc.IsNullOrEmpty(SqlFunc.ToString(a.HeadIcon)), string.Empty, SqlFunc.MergeString("/api/File/Image/userAvatar/", SqlFunc.ToString(a.HeadIcon)))
            }).FirstAsync();

        // 获取组织树
        var orgTree = _organizeService.GetOrgListTreeName();

        // 组织结构
        data.organize = orgTree.FirstOrDefault(x => x.Id.Equals(data.organizeId))?.Description;

        // 获取当前用户、当前组织下的所有岗位
        List<string>? pNameList = await _repository.AsSugarClient().Queryable<PositionEntity, UserRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.ObjectId))
            .Where((a, b) => b.ObjectType == "Position" && b.UserId == _userManager.UserId && a.OrganizeId == data.organizeId).Select(a => a.FullName).ToListAsync();
        data.position = string.Join(",", pNameList);

        // 获取当前用户、全局角色 和当前组织下的所有角色
        List<string>? roleList = await _userManager.GetUserOrgRoleIds(data.roleIds, data.organizeId);
        data.roleId = await _userManager.GetRoleNameByIds(string.Join(",", roleList));

        return data;
    }

    /// <summary>
    /// 获取系统权限 .
    /// </summary>
    /// <returns></returns>
    [HttpGet("Authorize")]
    public async Task<dynamic> GetAuthorize()
    {
        List<string>? roleIds = _userManager.Roles;
        string? userId = _userManager.UserId;
        bool isAdmin = _userManager.IsAdministrator;
        UsersCurrentAuthorizeOutput? output = new UsersCurrentAuthorizeOutput();
        List<ModuleEntity>? moduleList = await _authorizeService.GetCurrentUserModuleAuthorize(userId, isAdmin, roleIds.ToArray(), new string[] { _userManager.User.SystemId });

        if (moduleList.Any(it => it.Category.Equals("App")))
        {
            moduleList.Where(it => it.Category.Equals("App") && it.ParentId.Equals("-1")).ToList().ForEach(it =>
            {
                it.ParentId = "1";
            });
            moduleList.Add(new ModuleEntity()
            {
                Id = "1",
                FullName = "app菜单",
                Icon = "ym-custom ym-custom-cellphone",
                ParentId = "-1",
                Category = "App",
                Type = 1,
                SortCode = 99999
            });
        }
        List<ModuleButtonEntity>? buttonList = await _authorizeService.GetCurrentUserButtonAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleColumnEntity>? columnList = await _authorizeService.GetCurrentUserColumnAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleDataAuthorizeSchemeEntity>? resourceList = await _authorizeService.GetCurrentUserResourceAuthorize(userId, isAdmin, roleIds.ToArray());
        List<ModuleFormEntity>? formList = await _authorizeService.GetCurrentUserFormAuthorize(userId, isAdmin, roleIds.ToArray());
        if (moduleList.Count != 0)
            output.module = moduleList.Adapt<List<UsersCurrentAuthorizeMoldel>>().ToTree("-1");
        if (buttonList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = buttonList.Select(m => m.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.button = menuAuthorizeData.Union(buttonList.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (columnList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = columnList.Select(m => m.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.column = menuAuthorizeData.Union(columnList.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (resourceList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? resourceData = resourceList.Select(r => new UsersCurrentAuthorizeMoldel
            {
                Id = r.Id,
                ParentId = r.ModuleId,
                fullName = r.FullName,
                icon = "icon-ym icon-ym-extend"
            }).ToList();
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = resourceList.Select(bt => bt.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.resource = menuAuthorizeData.Union(resourceData.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        if (formList.Count != 0)
        {
            List<UsersCurrentAuthorizeMoldel>? formData = formList.Select(r => new UsersCurrentAuthorizeMoldel
            {
                Id = r.Id,
                ParentId = r.ModuleId,
                fullName = r.FullName,
                icon = "icon-ym icon-ym-extend"
            }).ToList();
            List<UsersCurrentAuthorizeMoldel>? menuAuthorizeData = new List<UsersCurrentAuthorizeMoldel>();
            List<string>? pids = formList.Select(bt => bt.ModuleId).ToList();
            GetParentsModuleList(pids, moduleList, ref menuAuthorizeData);
            output.form = menuAuthorizeData.Union(formData.Adapt<List<UsersCurrentAuthorizeMoldel>>()).ToList().ToTree("-1");
        }

        return output;
    }

    /// <summary>
    /// 获取系统日志.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("SystemLog")]
    public async Task<dynamic> GetSystemLog([FromQuery] UsersCurrentSystemLogQuery input)
    {
        string? userId = _userManager.UserId;
        PageInputBase? requestParam = input.Adapt<PageInputBase>();
        var startTime = input.startTime.TimeStampToDateTime();
        var endTime = input.endTime.TimeStampToDateTime();
        SqlSugarPagedList<UsersCurrentSystemLogOutput>? data = await _repository.AsSugarClient().Queryable<SysLogEntity>()
            .WhereIF(!input.startTime.IsNullOrEmpty(), s => s.CreatorTime >= new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0))
            .WhereIF(!input.endTime.IsNullOrEmpty(), s => s.CreatorTime <= new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999))
            .WhereIF(!input.Keyword.IsNullOrEmpty(), s => s.UserName.Contains(input.Keyword) || s.IPAddress.Contains(input.Keyword) || s.ModuleName.Contains(input.Keyword))
            .Where(s => s.Category == input.category && s.UserId == userId).OrderBy(o => o.CreatorTime, OrderByType.Desc)
            .Select(a => new UsersCurrentSystemLogOutput
            {
                creatorTime = a.CreatorTime,
                userName = a.UserName,
                ipaddress = a.IPAddress,
                moduleName = a.ModuleName,
                category = a.Category,
                userId = a.UserId,
                platForm = a.PlatForm,
                requestURL = a.RequestURL,
                requestMethod = a.RequestMethod,
                requestDuration = a.RequestDuration
            }).ToPagedListAsync(requestParam.CurrentPage, requestParam.PageSize);
        return PageResult<UsersCurrentSystemLogOutput>.SqlSugarPageResult(data);
    }

    #endregion

    #region Post

    /// <summary>
    /// 修改密码.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ModifyPassword")]
    public async Task ModifyPassword([FromBody] UsersCurrentActionsModifyPasswordInput input)
    {
        UserEntity? user = _userManager.User;
        //if(user.Id.ToLower().Equals("admin")) // admin账号不可修改密码
        //    throw Oops.Oh(ErrorCode.D5024);
        if (MD5Encryption.Encrypt(input.oldPassword + user.Secretkey) != user.Password.ToLower())
            throw Oops.Oh(ErrorCode.D5007);
        string? imageCode = await GetCode(input.timestamp);
        await PwdStrategy(input);
        if (!input.code.ToLower().Equals(imageCode.ToLower()))
        {
            throw Oops.Oh(ErrorCode.D5015);
        }
        else
        {
            await DelCode(input.timestamp);
            await DelUserInfo(_userManager.TenantId, user.Id);

            // 修改密码会立即退出登录
            var onlineCacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId);
            var list = await _cacheManager.GetAsync<List<UserOnlineModel>>(onlineCacheKey);

            var onlineUser = list.Find(it => it.tenantId == _userManager.TenantId && it.userId == _userManager.UserId);
            if (onlineUser != null)
            {
                await _imHandler.SendMessageAsync(onlineUser.connectionId, new { method = "logout", msg = "密码已变更，请重新登录！" }.ToJsonString());

                // 删除在线用户ID
                list.RemoveAll((x) => x.connectionId == onlineUser.connectionId);
                await _cacheManager.SetAsync(onlineCacheKey, list);

                // 删除用户登录信息缓存
                var cacheKey = string.Format("{0}:{1}:{2}", _userManager.TenantId, CommonConst.CACHEKEYUSER, onlineUser.userId);
                await _cacheManager.DelAsync(cacheKey);
            }
        }
        user.Password = MD5Encryption.Encrypt(input.password + user.Secretkey);
        user.ChangePasswordDate = DateTime.Now;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(user).UpdateColumns(it => new {
            it.Password,
            it.ChangePasswordDate,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5008);
    }

    /// <summary>
    /// 修改个人资料.
    /// </summary>
    /// <returns></returns>
    [HttpPut("BaseInfo")]
    public async Task UpdateBaseInfo([FromBody] UsersCurrentInfoUpInput input)
    {
        UserEntity? userInfo = input.Adapt<UserEntity>();
        userInfo.Id = _userManager.UserId;
        userInfo.IsAdministrator = Convert.ToInt32(_userManager.IsAdministrator);
        userInfo.LastModifyTime = DateTime.Now;
        userInfo.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(userInfo).UpdateColumns(it => new {
            it.RealName,
            it.Signature,
            it.Gender,
            it.Nation,
            it.NativePlace,
            it.CertificatesType,
            it.CertificatesNumber,
            it.Education,
            it.Birthday,
            it.TelePhone,
            it.Landline,
            it.MobilePhone,
            it.Email,
            it.UrgentContacts,
            it.UrgentTelePhone,
            it.PostalAddress,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5009);
    }

    /// <summary>
    /// 修改主题.
    /// </summary>
    /// <returns></returns>
    [HttpPut("SystemTheme")]
    public async Task UpdateBaseInfo([FromBody] UsersCurrentSysTheme input)
    {
        UserEntity? user = _userManager.User;
        user.Theme = input.theme;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(user).UpdateColumns(it => new {
            it.Theme,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5010);
    }

    /// <summary>
    /// 修改语言.
    /// </summary>
    /// <returns></returns>
    [HttpPut("SystemLanguage")]
    public async Task UpdateLanguage([FromBody] UsersCurrentSysLanguage input)
    {
        UserEntity? user = _userManager.User;
        user.Language = input.language;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(user).UpdateColumns(it => new {
            it.Language,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5011);
    }

    /// <summary>
    /// 修改头像.
    /// </summary>
    /// <returns></returns>
    [HttpPut("Avatar/{name}")]
    public async Task UpdateAvatar(string name)
    {
        UserEntity? user = _userManager.User;
        user.HeadIcon = name;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(user).UpdateColumns(it => new {
            it.HeadIcon,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5012);
    }

    /// <summary>
    /// 切换 默认 ： 组织、岗位、系统.
    /// </summary>
    /// <returns></returns>
    [HttpPut("major")]
    public async Task DefaultOrganize([FromBody] UsersCurrentDefaultOrganizeInput input)
    {
        UserEntity? userInfo = _userManager.User;

        switch (input.majorType)
        {
            case "Organize": // 组织
                {
                    userInfo.OrganizeId = input.majorId;

                    List<string>? roleList = await _userManager.GetUserOrgRoleIds(userInfo.RoleId, userInfo.OrganizeId);

                    // 如果该组织下没有角色 则 切换组织失败
                    if (!roleList.Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    // 该组织下没有任何权限 则 切换组织失败
                    if (!_repository.AsSugarClient().Queryable<AuthorizeEntity>().Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId)).Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    // 获取切换组织 Id 下的所有岗位
                    List<string>? pList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.OrganizeId == input.majorId).Select(x => x.Id).ToListAsync();

                    // 获取切换组织的 岗位，如果该组织没有岗位则为空
                    List<string>? idList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                        .Where(x => x.UserId == userInfo.Id && pList.Contains(x.ObjectId) && x.ObjectType == "Position").Select(x => x.ObjectId).ToListAsync();
                    userInfo.PositionId = idList.FirstOrDefault() == null ? string.Empty : idList.FirstOrDefault();
                }

                break;
            case "Position": // 岗位
                userInfo.PositionId = input.majorId;
                break;

            case "System": // 系统
                if (input.menuType.Equals(1))
                {
                    // 系统下没有菜单不允许切换.
                    var mList = await _repository.AsSugarClient().Queryable<ModuleEntity>().Where(x => x.SystemId.Equals(input.majorId) && x.DeleteMark == null && x.Category.Equals("App")).Select(x => x.Id).ToListAsync();
                    if (!mList.Any()) throw Oops.Oh(ErrorCode.D4009);

                    List<string>? roleList = await _userManager.GetUserOrgRoleIds(userInfo.RoleId, userInfo.OrganizeId);

                    // 非管理员 没有任何权限 切换失败
                    if (!_userManager.IsAdministrator && !_repository.AsSugarClient().Queryable<AuthorizeEntity>()
                        .Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId))
                        .Where(x => mList.Contains(x.ItemId)).Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    userInfo.AppSystemId = input.majorId;
                }
                else
                {
                    // 当前系统已被管理员禁用.
                    var switchSystem = await _repository.AsSugarClient().Queryable<SystemEntity>()
                        .Where(it => input.majorId.Equals(it.Id) && it.DeleteMark == null)
                        .FirstAsync();
                    if (switchSystem != null && !switchSystem.EnabledMark.Equals(1))
                        throw Oops.Oh(ErrorCode.D4014);

                    // 系统下没有菜单不允许切换.
                    var mList = await _repository.AsSugarClient().Queryable<ModuleEntity>().Where(x => x.SystemId.Equals(input.majorId) && x.DeleteMark == null && x.Category.Equals("Web")).Select(x => x.Id).ToListAsync();
                    if (!mList.Any()) throw Oops.Oh(ErrorCode.D4009);

                    List<string>? roleList = await _userManager.GetUserOrgRoleIds(userInfo.RoleId, userInfo.OrganizeId);

                    // 非管理员 没有任何权限 切换失败
                    if (!_userManager.IsAdministrator && !_repository.AsSugarClient().Queryable<AuthorizeEntity>()
                        .Where(x => x.ObjectType == "Role" && x.ItemType == "module" && roleList.Contains(x.ObjectId))
                        .Where(x => mList.Contains(x.ItemId)).Any())
                        throw Oops.Oh(ErrorCode.D5023);

                    userInfo.SystemId = input.majorId;
                }

                break;
        }

        userInfo.LastModifyTime = DateTime.Now;
        userInfo.LastModifyUserId = _userManager.UserId;
        int isOk = await _repository.AsUpdateable(userInfo).UpdateColumns(it => new {
            it.OrganizeId,
            it.PositionId,
            it.LastModifyUserId,
            it.LastModifyTime,
            it.SystemId,
            it.AppSystemId
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5020);
    }

    /// <summary>
    /// 获取当前用户所有组织.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUserOrganizes")]
    public async Task<dynamic> GetUserOrganizes()
    {
        UserEntity? userInfo = _userManager.User;

        // 获取当前用户所有关联 组织ID 集合
        List<string>? idList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
            .Where(x => x.UserId == userInfo.Id && x.ObjectType == "Organize")
            .Select(x => x.ObjectId).ToListAsync();

        // 获取组织树
        var orgTree = _organizeService.GetOrgListTreeName();

        // 根据关联组织ID 查询组织信息
        List<CurrentUserOrganizesOutput>? oList = orgTree.Where(x => idList.Contains(x.Id))
            .Select(x => new CurrentUserOrganizesOutput
            {
                id = x.Id,
                fullName = x.Description
            }).ToList();

        CurrentUserOrganizesOutput? def = oList.Where(x => x.id == userInfo.OrganizeId).FirstOrDefault();
        if (def != null) def.isDefault = true;

        return oList;
    }

    /// <summary>
    /// 获取当前用户所有岗位.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUserPositions")]
    public async Task<dynamic> GetUserPositions()
    {
        UserEntity? userInfo = _userManager.User;

        // 获取当前用户所有关联 岗位ID 集合
        List<string>? idList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
            .Where(x => x.UserId == userInfo.Id && x.ObjectType == "Position")
            .Select(x => x.ObjectId).ToListAsync();

        // 根据关联 岗位ID 查询岗位信息
        List<CurrentUserOrganizesOutput>? oList = await _repository.AsSugarClient().Queryable<PositionEntity>()
            .Where(x => x.OrganizeId == userInfo.OrganizeId).Where(x => idList.Contains(x.Id))
            .Select(x => new CurrentUserOrganizesOutput
            {
                id = x.Id,
                fullName = x.FullName
            }).ToListAsync();

        CurrentUserOrganizesOutput? def = oList.Where(x => x.id == userInfo.PositionId).FirstOrDefault();
        if (def != null) def.isDefault = true;

        return oList;
    }

    /// <summary>
    /// 获取当前用户所有签名.
    /// </summary>
    /// <returns></returns>
    [HttpGet("SignImg")]
    public async Task<dynamic> GetSignImg()
    {
        try
        {
            return (await _repository.AsSugarClient().Queryable<SignImgEntity>().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null).ToListAsync()).Adapt<List<UsersCurrentSignImgOutput>>();
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    /// <summary>
    /// 新增签名.
    /// </summary>
    /// <returns></returns>
    [HttpPost("SignImg")]
    public async Task CreateSignImg([FromBody] UsersCurrentSignImgOutput input)
    {
        if (!_repository.AsSugarClient().Queryable<SignImgEntity>().Any(x => x.CreatorUserId == _userManager.UserId))
        {
            input.isDefault = 1;
        }
        var signImgEntity = input.Adapt<SignImgEntity>();
        var entity = await _repository.AsSugarClient().Insertable(signImgEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync();
        if (entity.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.COM1000);
        if (input.isDefault == 1)
        {
            await _repository.AsSugarClient().Updateable<SignImgEntity>().SetColumns(x => x.IsDefault == 0).Where(x => x.Id != entity.Id && x.CreatorUserId == _userManager.UserId).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 设置默认签名.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}/SignImg")]
    public async Task UpdateSignImg(string id)
    {
        await _repository.AsSugarClient().Updateable<SignImgEntity>().SetColumns(x => x.IsDefault == 0).Where(x => x.Id != id && x.CreatorUserId == _userManager.UserId).ExecuteCommandAsync();
        await _repository.AsSugarClient().Updateable<SignImgEntity>().SetColumns(x => x.IsDefault == 1).Where(x => x.Id == id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除签名.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}/SignImg")]
    public async Task DeleteSignImg(string id)
    {
        var isOk = await _repository.AsSugarClient().Updateable<SignImgEntity>().SetColumns(it => new SignImgEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 过滤菜单权限数据.
    /// </summary>
    /// <param name="pids">其他权限数据.</param>
    /// <param name="moduleList">勾选菜单权限数据.</param>
    /// <param name="output">返回值.</param>
    private void GetParentsModuleList(List<string> pids, List<ModuleEntity> moduleList, ref List<UsersCurrentAuthorizeMoldel> output)
    {
        List<UsersCurrentAuthorizeMoldel>? authorizeModuleData = moduleList.Adapt<List<UsersCurrentAuthorizeMoldel>>();
        foreach (string? item in pids)
        {
            GteModuleListById(item, authorizeModuleData, output);
        }

        output = output.Distinct().ToList();
    }

    /// <summary>
    /// 根据菜单id递归获取authorizeDataOutputModel的父级菜单.
    /// </summary>
    /// <param name="id">菜单id.</param>
    /// <param name="authorizeModuleData">选中菜单集合.</param>
    /// <param name="output">返回数据.</param>
    private void GteModuleListById(string id, List<UsersCurrentAuthorizeMoldel> authorizeModuleData, List<UsersCurrentAuthorizeMoldel> output)
    {
        UsersCurrentAuthorizeMoldel? data = authorizeModuleData.Find(l => l.Id == id);
        if (data != null)
        {
            if (!data.ParentId.Equals("-1"))
            {
                if (!output.Contains(data)) output.Add(data);

                GteModuleListById(data.ParentId, authorizeModuleData, output);
            }
            else
            {
                if (!output.Contains(data)) output.Add(data);
            }
        }
    }

    /// <summary>
    /// 获取验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private async Task<string> GetCode(string timestamp)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        return await _cacheManager.GetAsync<string>(cacheKey);
    }

    /// <summary>
    /// 删除验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private Task<bool> DelCode(string timestamp)
    {
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        _cacheManager.DelAsync(cacheKey);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private Task<bool> DelUserInfo(string tenantId, string userId)
    {
        string? cacheKey = string.Format("{0}:{1}:{2}", tenantId, CommonConst.CACHEKEYUSER, userId);
        _cacheManager.DelAsync(cacheKey);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 密码策略验证.
    /// </summary>
    /// <returns></returns>
    private async Task PwdStrategy(UsersCurrentActionsModifyPasswordInput input)
    {
        // 系统配置信息
        var sysInfo = await _sysConfigService.GetInfo();
        // 禁用旧密码
        if (sysInfo.disableOldPassword == 1 && sysInfo.disableTheNumberOfOldPasswords > 0)
        {
            var oldPwdList = _repository.AsSugarClient().Queryable<UserOldPasswordEntity>().Where(x => x.UserId == _userManager.UserId).OrderByDescending(o => o.CreatorTime).Take(sysInfo.disableTheNumberOfOldPasswords).ToList();
            if (oldPwdList.Any())
            {
                foreach (var item in oldPwdList)
                {
                    if (MD5Encryption.Encrypt(input.password + item.Secretkey) == item.OldPassword.ToLower())
                        throw Oops.Oh(ErrorCode.D5026);
                }
            }
        }

        // 保存旧密码数据
        var oldPwdEntity = new UserOldPasswordEntity();
        oldPwdEntity.Id = SnowflakeIdHelper.NextId();
        oldPwdEntity.UserId = _userManager.UserId;
        oldPwdEntity.Account = _userManager.Account;
        oldPwdEntity.OldPassword = MD5Encryption.Encrypt(input.password + _userManager.User.Secretkey);
        oldPwdEntity.Secretkey = _userManager.User.Secretkey;
        oldPwdEntity.CreatorTime = DateTime.Now;
        oldPwdEntity.TenantId = _userManager.TenantId;
        _repository.AsSugarClient().Insertable(oldPwdEntity).ExecuteCommand();
    }
    #endregion
}