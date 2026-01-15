using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.Authorize;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.Systems.Entitys.Entity.Permission;
using Poxiao.Systems.Entitys.Entity.System;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Entitys;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.Security.Claims;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// 当前登录用户.
/// </summary>
public class UserManager : IUserManager, IScoped
{
    /// <summary>
    /// 用户表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 当前Http请求.
    /// </summary>
    private readonly HttpContext _httpContext;

    /// <summary>
    /// 用户Claim主体.
    /// </summary>
    private readonly ClaimsPrincipal _user;

    /// <summary>
    /// 初始化一个<see cref="UserManager"/>类型的新实例.
    /// </summary>
    /// <param name="repository">用户仓储.</param>
    /// <param name="cacheManager">缓存管理.</param>
    public UserManager(
        ISqlSugarRepository<UserEntity> repository,
        ICacheManager cacheManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _httpContext = App.HttpContext;
        _user = _httpContext?.User;
    }

    /// <summary>
    /// 用户信息.
    /// </summary>
    public UserEntity User
    {
        get => _repository.GetSingle(u => u.Id == UserId);
    }

    /// <summary>
    /// 用户ID.
    /// </summary>
    public string UserId
    {
        get => _user.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
    }

    /// <summary>
    /// 获取用户角色.
    /// </summary>
    public List<string> Roles
    {
        get
        {
            var user = _repository.GetSingle(u => u.Id == UserId);
            return GetUserRoleIds(user.RoleId, user.OrganizeId);
        }
    }

    /// <summary>
    /// 用户账号.
    /// </summary>
    public string Account
    {
        get => _user.FindFirst(ClaimConst.CLAINMACCOUNT)?.Value;
    }

    /// <summary>
    /// 用户昵称.
    /// </summary>
    public string RealName
    {
        get => _user.FindFirst(ClaimConst.CLAINMREALNAME)?.Value;
    }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId
    {
        get => _user.FindFirst(ClaimConst.TENANTID)?.Value;
    }

    /// <summary>
    /// 租户数据库名称.
    /// </summary>
    public string TenantDbName
    {
        get
        {
            var tenant = GetGlobalTenantCache(TenantId);
            if (tenant == null) return null;
            else return tenant.connectionConfig.ConfigList.FirstOrDefault().ServiceName;
        }
    }

    /// <summary>
    /// 当前用户 token.
    /// </summary>
    public string ToKen
    {
        get => string.IsNullOrEmpty(App.HttpContext?.Request.Headers["Authorization"]) ? App.HttpContext?.Request.Query["token"] : App.HttpContext?.Request.Headers["Authorization"];
    }

    /// <summary>
    /// 是否是管理员.
    /// </summary>
    public bool IsAdministrator
    {
        get => _user.FindFirst(ClaimConst.CLAINMADMINISTRATOR)?.Value == ((int)AccountType.Administrator).ToString();
    }

    /// <summary>
    /// 当前租户配置.
    /// </summary>
    public GlobalTenantCacheModel CurrentTenantInformation
    {
        get => GetGlobalTenantCache(TenantId);
    }

    /// <summary>
    /// 当前用户下属.
    /// </summary>
    public List<string> Subordinates
    {
        get
        {
            return this.GetSubordinates(UserId).ToList();
        }
    }

    /// <summary>
    /// 当前用户及下属.
    /// </summary>
    public List<string> CurrentUserAndSubordinates
    {
        get
        {
            var array = new List<string> { UserId };
            array.AddRange(GetSubordinates(UserId).ToList());
            return array;
        }
    }

    /// <summary>
    /// 当前组织及子组织.
    /// </summary>
    public List<string> CurrentOrganizationAndSubOrganizations
    {
        get
        {
            List<string> list = new List<string> { User.OrganizeId };
            list.AddRange(GetSubsidiary(User.OrganizeId, IsAdministrator).ToObject<List<string>>());
            return list;
        }
    }

    /// <summary>
    /// 当前用户子组织.
    /// </summary>
    public List<string> CurrentUserSubOrganization
    {
        get
        {
            return GetSubsidiary(User.OrganizeId, IsAdministrator).ToObject<List<string>>();
        }
    }

    /// <summary>
    /// 获取用户的数据范围.
    /// </summary>
    public List<UserDataScopeModel> DataScope
    {
        get
        {
            return GetUserDataScope(UserId);
        }
    }

    /// <summary>
    /// 获取请求端类型 pc 、 app.
    /// </summary>
    public string UserOrigin
    {
        get => _httpContext?.Request.Headers["poxiao-origin"];
    }

    /// <summary>
    /// 获取用户登录信息.
    /// </summary>
    /// <returns></returns>
    public async Task<UserInfoModel> GetUserInfo()
    {
        UserAgent userAgent = new UserAgent(_httpContext);
        var data = new UserInfoModel();
        var userCache = string.Format("{0}:{1}:{2}", TenantId, CommonConst.CACHEKEYUSER, UserId);
        var userDataScope = await GetUserDataScopeAsync(UserId);

        var ipAddress = NetHelper.Ip;
        var ipAddressName = await NetHelper.GetLocation(ipAddress);
        var sysConfigInfo = await _repository.AsSugarClient().Queryable<SysConfigEntity>().FirstAsync(s => s.Category.Equals("SysConfig") && s.Key.ToLower().Equals("tokentimeout"));
        data = await _repository.AsQueryable().Where(it => it.Id == UserId)
           .Select(a => new UserInfoModel
           {
               userId = a.Id,
               headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
               userAccount = a.Account,
               userName = a.RealName,
               gender = a.Gender,
               organizeId = a.OrganizeId,
               departmentId = a.OrganizeId,
               departmentName = SqlFunc.Subqueryable<OrganizeEntity>().Where(o => o.Id == SqlFunc.ToString(a.OrganizeId) && o.Category.Equals("department")).Select(o => o.FullName),
               organizeName = SqlFunc.Subqueryable<OrganizeEntity>().Where(o => o.Id == SqlFunc.ToString(a.OrganizeId)).Select(o => o.OrganizeIdTree),
               managerId = a.ManagerId,
               isAdministrator = SqlFunc.IIF(a.IsAdministrator == 1, true, false),
               portalId = a.PortalId,
               positionId = a.PositionId,
               roleId = a.RoleId,
               prevLoginTime = a.PrevLogTime,
               prevLoginIPAddress = a.PrevLogIP,
               landline = a.Landline,
               telePhone = a.TelePhone,
               manager = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.ManagerId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
               mobilePhone = a.MobilePhone,
               email = a.Email,
               birthday = a.Birthday,
               systemId = a.SystemId,
               appSystemId = a.AppSystemId,
               signImg = SqlFunc.Subqueryable<SignImgEntity>().Where(a => a.CreatorUserId == UserId && a.IsDefault == 1).Select(a => a.SignImg),
               changePasswordDate = a.ChangePasswordDate,
               loginTime = DateTime.Now,
           }).FirstAsync();

        if (data != null && data.organizeName.IsNotEmptyOrNull())
        {
            var orgIdTree = data?.organizeName?.Split(',');
            data.organizeIdList = orgIdTree.ToList();
            var organizeName = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => orgIdTree.Contains(x.Id)).OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime).Select(x => x.FullName).ToListAsync();
            data.organizeName = string.Join("/", organizeName);
        }
        else
        {
            data.organizeName = data.departmentName;
        }
        data.prevLogin = (await _repository.AsSugarClient().Queryable<SysConfigEntity>().FirstAsync(x => x.Category.Equals("SysConfig") && x.Key.ToLower().Equals("lastlogintimeswitch"))).Value.ParseToInt();
        data.loginIPAddress = ipAddress;
        data.loginIPAddressName = ipAddressName;
        data.prevLoginIPAddressName = await NetHelper.GetLocation(data.prevLoginIPAddress);
        data.loginPlatForm = userAgent.RawValue;
        data.subsidiary = await GetSubsidiaryAsync(data.organizeId, data.isAdministrator);
        data.subordinates = await this.GetSubordinatesAsync(UserId);
        data.positionIds = data.positionId == null ? null : await GetPosition(data.positionId);
        data.positionName = data.positionIds == null ? null : string.Join(",", data.positionIds.Select(it => it.name));
        var roleList = await GetUserOrgRoleIds(data.roleId, data.organizeId);
        data.roleName = await GetRoleNameByIds(string.Join(",", roleList));
        data.roleIds = roleList.ToArray();

        // 门户
        var portalManageList = new List<PortalManageEntity>();
        if (!data.isAdministrator && data.roleIds.Any())
        {
            // 授权的所有门户管理
            var portalManageIds = await _repository.AsSugarClient().Queryable<AuthorizeEntity>().In(a => a.ObjectId, data.roleIds).Where(a => a.ItemType == "portalManage").GroupBy(it => new { it.ItemId }).Select(it => it.ItemId).ToListAsync();

            // 授权的当前系统的所有门户管理
            portalManageList = await _repository.AsSugarClient().Queryable<PortalManageEntity, PortalEntity>((pm, p) => new JoinQueryInfos(JoinType.Left, pm.PortalId == p.Id))
                .Where((pm, p) => pm.EnabledMark == 1 && pm.DeleteMark == null && p.EnabledMark == 1 && p.DeleteMark == null)
                .Where(pm => portalManageIds.Contains(pm.Id))
                .Select<PortalManageEntity>()
                .ToListAsync();
        }
        else if (data.isAdministrator)
        {
            // 当前系统的所有门户管理
            portalManageList = await _repository.AsSugarClient().Queryable<PortalManageEntity, PortalEntity>((pm, p) => new JoinQueryInfos(JoinType.Left, pm.PortalId == p.Id))
                .Where((pm, p) => pm.EnabledMark == 1 && pm.DeleteMark == null && p.EnabledMark == 1 && p.DeleteMark == null)
                .Select<PortalManageEntity>()
                .ToListAsync();
        }
        var webPortalManageList = portalManageList.Where(it => it.SystemId.Equals(data.systemId)).ToList();
        var appPortalManageList = portalManageList.Where(it => it.SystemId.Equals(data.appSystemId)).ToList();
        var portalId = data.portalId;
        data.portalId = await GetPortalId(webPortalManageList, portalId, data.systemId, data.userId, "Web");
        data.appPortalId = await GetPortalId(appPortalManageList, portalId, data.appSystemId, data.userId, "App");

        data.overdueTime = TimeSpan.FromMinutes(sysConfigInfo.Value.ParseToDouble());
        data.dataScope = userDataScope;
        data.tenantId = TenantId;

        // 根据系统配置过期时间自动过期
        await SetUserInfo(userCache, data, TimeSpan.FromMinutes(sysConfigInfo.Value.ParseToDouble()));

        return data;
    }

    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private async Task<List<UserDataScopeModel>> GetUserDataScopeAsync(string userId)
    {
        List<UserDataScopeModel> data = new List<UserDataScopeModel>();
        List<UserDataScopeModel> subData = new List<UserDataScopeModel>();
        List<UserDataScopeModel> inteList = new List<UserDataScopeModel>();
        var list = await _repository.AsSugarClient().Queryable<OrganizeAdministratorEntity>().Where(it => SqlFunc.ToString(it.UserId) == userId && it.DeleteMark == null).ToListAsync();

        // 填充数据
        foreach (var item in list)
        {
            if (item.SubLayerAdd.ParseToBool() || item.SubLayerEdit.ParseToBool() || item.SubLayerDelete.ParseToBool())
            {
                var subsidiary = (await GetSubsidiaryAsync(item.OrganizeId, false)).ToList();
                subsidiary.Remove(item.OrganizeId);
                subsidiary.ToList().ForEach(it =>
                {
                    subData.Add(new UserDataScopeModel()
                    {
                        organizeId = it,
                        Add = item.SubLayerAdd.ParseToBool(),
                        Edit = item.SubLayerEdit.ParseToBool(),
                        Delete = item.SubLayerDelete.ParseToBool()
                    });
                });
            }

            if (item.ThisLayerAdd.ParseToBool() || item.ThisLayerEdit.ParseToBool() || item.ThisLayerDelete.ParseToBool())
            {
                data.Add(new UserDataScopeModel()
                {
                    organizeId = item.OrganizeId,
                    Add = item.ThisLayerAdd.ParseToBool(),
                    Edit = item.ThisLayerEdit.ParseToBool(),
                    Delete = item.ThisLayerDelete.ParseToBool()
                });
            }
        }

        /* 比较数据
        所有分级数据权限以本级权限为主 子级为辅
        将本级数据与子级数据对比 对比出子级数据内组织ID存在本级数据的组织ID*/
        var intersection = data.Select(it => it.organizeId).Intersect(subData.Select(it => it.organizeId)).ToList();
        intersection.ForEach(it =>
        {
            var parent = data.Find(item => item.organizeId == it);
            var child = subData.Find(item => item.organizeId == it);
            var add = false;
            var edit = false;
            var delete = false;
            if (parent.Add || child.Add)
                add = true;
            if (parent.Edit || child.Edit)
                edit = true;
            if (parent.Delete || child.Delete)
                delete = true;
            inteList.Add(new UserDataScopeModel()
            {
                organizeId = it,
                Add = add,
                Edit = edit,
                Delete = delete
            });
            data.Remove(parent);
            subData.Remove(child);
        });
        return data.Union(subData).Union(inteList).ToList();
    }

    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    private List<UserDataScopeModel> GetUserDataScope(string userId)
    {
        List<UserDataScopeModel> data = new List<UserDataScopeModel>();
        List<UserDataScopeModel> subData = new List<UserDataScopeModel>();
        List<UserDataScopeModel> inteList = new List<UserDataScopeModel>();

        // 填充数据
        foreach (var item in _repository.AsSugarClient().Queryable<OrganizeAdministratorEntity>()
            .Where(it => SqlFunc.ToString(it.UserId) == userId && it.DeleteMark == null).ToList())
        {
            if (item.SubLayerSelect.ParseToBool() || item.SubLayerAdd.ParseToBool() || item.SubLayerEdit.ParseToBool() || item.SubLayerDelete.ParseToBool())
            {
                var subsidiary = GetSubsidiary(item.OrganizeId, false).ToList();
                subsidiary.Remove(item.OrganizeId);
                subsidiary.ToList().ForEach(it =>
                {
                    subData.Add(new UserDataScopeModel()
                    {
                        organizeId = it,
                        Add = item.SubLayerAdd.ParseToBool(),
                        Edit = item.SubLayerEdit.ParseToBool(),
                        Delete = item.SubLayerDelete.ParseToBool(),
                        Select = item.SubLayerSelect.ParseToBool()
                    });
                });
            }

            if (item.ThisLayerSelect.ParseToBool() || item.ThisLayerAdd.ParseToBool() || item.ThisLayerEdit.ParseToBool() || item.ThisLayerDelete.ParseToBool())
            {
                data.Add(new UserDataScopeModel()
                {
                    organizeId = item.OrganizeId,
                    Add = item.ThisLayerAdd.ParseToBool(),
                    Edit = item.ThisLayerEdit.ParseToBool(),
                    Delete = item.ThisLayerDelete.ParseToBool(),
                    Select = item.ThisLayerSelect.ParseToBool()
                });
            }
        }

        /* 比较数据
        所有分级数据权限以本级权限为主 子级为辅
        将本级数据与子级数据对比 对比出子级数据内组织ID存在本级数据的组织ID*/
        var intersection = data.Select(it => it.organizeId).Intersect(subData.Select(it => it.organizeId)).ToList();
        intersection.ForEach(it =>
        {
            var parent = data.Find(item => item.organizeId == it);
            var child = subData.Find(item => item.organizeId == it);
            var add = false;
            var edit = false;
            var delete = false;
            var select = false;
            if (parent.Add || child.Add) add = true;
            if (parent.Edit || child.Edit) edit = true;
            if (parent.Delete || child.Delete) delete = true;
            if (parent.Select || child.Select) select = true;
            inteList.Add(new UserDataScopeModel()
            {
                organizeId = it,
                Add = add,
                Edit = edit,
                Delete = delete,
                Select = select
            });
            data.Remove(parent);
            subData.Remove(child);
        });
        return data.Union(subData).Union(inteList).ToList();
    }

    /// <summary>
    /// 获取数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <param name="tableNumber">联表编号.</param>
    /// <returns></returns>
    public async Task<List<IConditionalModel>> GetConditionAsync<T>(string moduleId, string primaryKey = "F_Id", bool isDataPermissions = true, string tableNumber = "")
        where T : new()
    {
        var conModels = new List<IConditionalModel>();
        if (IsAdministrator) return conModels;

        var roleAuthorizeList = _repository.AsSugarClient().Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, Roles).Where(a => a.ItemType == "resource").Select(a => new { a.ItemId, a.ObjectId }).ToList();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }
        else if (roleAuthorizeList.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }

        var resourceList = _repository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, roleAuthorizeList).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        if (resourceList.Any(x => x.AllData == 1 || "poxiao_alldata".Equals(x.EnCode)))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                        }
            });
        }
        else
        {
            var allList = new List<object>(); // 构造任何层级的条件
            var resultList = new List<object>();
            foreach (var roleId in Roles)
            {
                var isCurrentRole = true;
                var roleList = new List<object>();
                foreach (var item in resourceList.Where(x => roleAuthorizeList.Where(xx => xx.ObjectId.Equals(roleId)).Select(x => x.ItemId).Contains(x.Id)).ToList())
                {
                    var groupsList = new List<object>();
                    foreach (var conditionItem in item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>())
                    {
                        var conditionalList = new List<object>();
                        foreach (var fieldItem in conditionItem.Groups)
                        {
                            var itemField = string.Format("{0}{1}", tableNumber, fieldItem.Field);
                            var itemValue = fieldItem.Value;
                            var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);

                            var cmodel = GetConditionalModel(itemMethod, itemField, User.OrganizeId);
                            if (itemMethod.Equals(QueryType.Equal)) cmodel.ConditionalType = ConditionalType.Like;
                            if (itemMethod.Equals(QueryType.NotEqual)) cmodel.ConditionalType = ConditionalType.NoLike;
                            switch (itemValue)
                            {
                                case "@userId": // 当前用户
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                            case "or":
                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                        }
                                    }

                                    break;
                                case "@userAraSubordinates": // 当前用户集下属
                                    {
                                        var ids = new List<string>() { UserId };
                                        ids.AddRange(Subordinates);
                                        for (int i = 0; i < ids.Count; i++)
                                        {
                                            if (i == 0)
                                            {
                                                switch (conditionItem.Logic)
                                                {
                                                    case "and":
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                    case "or":
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                else
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                            }
                                        }
                                    }

                                    break;
                                case "@organizeId": // 当前组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                            }
                                        }
                                    }

                                    break;
                                case "@organizationAndSuborganization": // 当前组织及子组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            var ids = CurrentOrganizationAndSubOrganizations;
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                    }

                                    break;

                                case "@branchManageOrganize": // 当前分管组织
                                    {
                                        var ids = DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList();
                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                case "@branchManageOrganizeAndSub": // 当前分管组织及子组织
                                    {
                                        var ids = new List<string>();
                                        DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList()
                                            .ForEach(item => ids.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.OrganizeIdTree.Contains(item)).Select(x => x.Id).ToList()));

                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                default:
                                    {
                                        if (!string.IsNullOrEmpty(itemValue))
                                        {
                                            var defCmodel = GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type);
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.In)) defCmodel.ConditionalType = ConditionalType.Like;
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.NotIn)) defCmodel.ConditionalType = ConditionalType.NoLike;
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                            }
                                        }

                                    }

                                    break;
                            }
                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = string.Empty, ConditionalType = ConditionalType.IsNullOrEmpty } });
                        }

                        if (conditionalList.Any())
                        {
                            var firstItem = conditionalList.First().ToObject<dynamic>();
                            firstItem.Key = 0;
                            conditionalList[0] = firstItem;
                            groupsList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = conditionalList } });
                        }
                    }

                    if (groupsList.Any()) roleList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = groupsList } });
                    isCurrentRole = false;
                }

                if (roleList.Any()) allList.Add(new { Key = (int)WhereType.Or, Value = new { ConditionalList = roleList } });
            }

            if (allList.Any()) resultList.Add(new { ConditionalList = allList });

            if (resultList.Any()) conModels.AddRange(_repository.AsSugarClient().Utilities.JsonToConditionalModels(resultList.ToJsonString()));
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = string.Format("{0}{1}", tableNumber, primaryKey), ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 获取数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    public async Task<List<IConditionalModel>> GetDataConditionAsync<T>(string moduleId, string primaryKey, bool isDataPermissions = true)
        where T : new()
    {
        var conModels = new List<IConditionalModel>();
        if (IsAdministrator) return conModels;

        var roleAuthorizeList = _repository.AsSugarClient().Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, Roles).Where(a => a.ItemType == "resource").Select(a => new { a.ItemId, a.ObjectId }).ToList();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }
        else if (roleAuthorizeList.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
            return conModels;
        }

        var resourceList = _repository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, roleAuthorizeList).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        if (resourceList.Any(x => x.AllData == 1 || x.EnCode.Equals("poxiao_alldata")))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                        }
            });
        }
        else
        {
            var allList = new List<object>(); // 构造任何层级的条件
            var resultList = new List<object>();
            foreach (var roleId in Roles)
            {
                var isCurrentRole = true;
                var roleList = new List<object>();
                foreach (var item in resourceList)
                {
                    var groupsList = new List<object>();
                    foreach (var conditionItem in item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>())
                    {
                        var conditionalList = new List<object>();
                        foreach (var fieldItem in conditionItem.Groups)
                        {
                            var itemField = string.IsNullOrEmpty(fieldItem.BindTable) ? fieldItem.Field : string.Format("{0}.{1}", fieldItem.BindTable, fieldItem.Field);
                            var itemValue = fieldItem.Value;
                            var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);

                            var cmodel = GetConditionalModel(itemMethod, itemField, User.OrganizeId);
                            if (itemMethod.Equals(QueryType.Equal)) cmodel.ConditionalType = ConditionalType.Like;
                            if (itemMethod.Equals(QueryType.NotEqual)) cmodel.ConditionalType = ConditionalType.NoLike;
                            switch (itemValue)
                            {
                                case "@userId": // 当前用户
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                            case "or":
                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                        }
                                    }

                                    break;
                                case "@userAraSubordinates": // 当前用户集下属
                                    {
                                        var ids = new List<string>() { UserId };
                                        ids.AddRange(Subordinates);
                                        for (int i = 0; i < ids.Count; i++)
                                        {
                                            if (i == 0)
                                            {
                                                switch (conditionItem.Logic)
                                                {
                                                    case "and":
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                    case "or":
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                else
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                            }
                                        }
                                    }

                                    break;
                                case "@organizeId": // 当前组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                            }
                                        }
                                    }

                                    break;
                                case "@organizationAndSuborganization": // 当前组织及子组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            var ids = CurrentOrganizationAndSubOrganizations;
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                    }

                                    break;

                                case "@branchManageOrganize": // 当前分管组织
                                    {
                                        var ids = DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList();
                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                case "@branchManageOrganizeAndSub": // 当前分管组织及子组织
                                    {
                                        var ids = new List<string>();
                                        DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList()
                                            .ForEach(item => ids.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.OrganizeIdTree.Contains(item)).Select(x => x.Id).ToList()));

                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                default:
                                    {
                                        if (!string.IsNullOrEmpty(itemValue))
                                        {
                                            var defCmodel = GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type);
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.In)) defCmodel.ConditionalType = ConditionalType.Like;
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.NotIn)) defCmodel.ConditionalType = ConditionalType.NoLike;
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                            }
                                        }

                                    }

                                    break;
                            }
                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = string.Empty, ConditionalType = ConditionalType.IsNullOrEmpty } });
                        }

                        if (conditionalList.Any())
                        {
                            var firstItem = conditionalList.First().ToObject<dynamic>();
                            firstItem.Key = 0;
                            conditionalList[0] = firstItem;
                            groupsList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = conditionalList } });
                        }
                    }

                    if (groupsList.Any()) roleList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = groupsList } });
                    isCurrentRole = false;
                }

                if (roleList.Any()) allList.Add(new { Key = (int)WhereType.Or, Value = new { ConditionalList = roleList } });
            }

            if (allList.Any()) resultList.Add(new { ConditionalList = allList });

            if (resultList.Any()) conModels.AddRange(_repository.AsSugarClient().Utilities.JsonToConditionalModels(resultList.ToJsonString()));
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 获取代码生成数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    public async Task<List<CodeGenAuthorizeModuleResourceModel>> GetCodeGenAuthorizeModuleResource<T>(string moduleId, string primaryKey, bool isDataPermissions = true)
        where T : new()
    {
        var codeGenConditional = new List<CodeGenAuthorizeModuleResourceModel>()
        {
            new CodeGenAuthorizeModuleResourceModel
            {
                FieldRule = 0,
                conditionalModel = new List<IConditionalModel>()
            }
        };
        if (IsAdministrator) return codeGenConditional; // 管理员全部放开
        var items = await _repository.AsSugarClient().Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                  .In((a, b) => b.Id, Roles)
                  .Where(a => a.ItemType == "resource")
                  .GroupBy(a => new { a.ItemId }).Select(a => a.ItemId).ToListAsync();

        switch (isDataPermissions)
        {
            case true:
                // 开启权限 但是没有权限资源.
                switch (items.Count)
                {
                    case 0:
                        codeGenConditional.Find(it => it.FieldRule.Equals(0)).conditionalModel.Add(new ConditionalCollections()
                        {
                            ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                            {
                                new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                            }
                        });
                        break;
                }

                break;
            default:
                // 未开启数据权限
                codeGenConditional.Find(it => it.FieldRule.Equals(0)).conditionalModel.Add(new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                });
                break;
        }

        var resourceList = await _repository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, items).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToListAsync();

        // 权限资源是否为全部数据.
        switch (resourceList?.Any(x => x.AllData == 1 || x.EnCode.Equals("poxiao_alldata")))
        {
            case true:
                codeGenConditional.Find(it => it.FieldRule.Equals(0)).conditionalModel.Add(new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                });
                break;
            case false:
                switch (resourceList.Count)
                {
                    case 0:
                        codeGenConditional.Find(it => it.FieldRule.Equals(0)).conditionalModel.Add(new ConditionalCollections()
                        {
                            ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                            {
                                new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                            }
                        });
                        break;
                    default:
                        codeGenConditional = new List<CodeGenAuthorizeModuleResourceModel>();

                        var allList = new List<object>(); // 构造任何层级的条件
                        var resultList = new List<object>();
                        var codeGenConditionalObject = new List<CodeGenAuthorizeModuleResource>();
                        foreach (var item in resourceList)
                        {
                            var groupsList = new List<object>();
                            var fieldRule = 0;
                            var tableName = string.Empty;
                            foreach (var conditionItem in item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>())
                            {
                                var conditionalList = new List<object>();
                                foreach (var fieldItem in conditionItem.Groups)
                                {
                                    fieldRule = fieldItem.FieldRule;
                                    tableName = string.IsNullOrEmpty(fieldItem.BindTable) ? fieldItem.Field.Split('.').First() : fieldItem.BindTable;
                                    if (!codeGenConditionalObject.Any(it => it.FieldRule == fieldRule && it.TableName == tableName))
                                    {
                                        codeGenConditionalObject.Add(new CodeGenAuthorizeModuleResource()
                                        {
                                            FieldRule = fieldRule,
                                            TableName = tableName,
                                            conditionalModel = new List<object>()
                                        });
                                    }

                                    var itemField = fieldRule == 0 ? fieldItem.Field : (string.IsNullOrEmpty(fieldItem.BindTable) ? fieldItem.Field.Split('.').Last() : fieldItem.Field);
                                    var itemValue = fieldItem.Value;
                                    var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);

                                    var cmodel = GetConditionalModel(itemMethod, itemField, User.OrganizeId);
                                    if (itemMethod.Equals(QueryType.Equal)) cmodel.ConditionalType = ConditionalType.Like;
                                    if (itemMethod.Equals(QueryType.NotEqual)) cmodel.ConditionalType = ConditionalType.NoLike;
                                    switch (itemValue)
                                    {
                                        case "@userId": // 当前用户
                                            {
                                                switch (conditionItem.Logic)
                                                {
                                                    case "and":
                                                        conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                    case "or":
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                }
                                            }

                                            break;
                                        case "@userAraSubordinates": // 当前用户集下属
                                            {
                                                var ids = new List<string>() { UserId };
                                                ids.AddRange(Subordinates);
                                                for (int i = 0; i < ids.Count; i++)
                                                {
                                                    if (i == 0)
                                                    {
                                                        switch (conditionItem.Logic)
                                                        {
                                                            case "and":
                                                                conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                break;
                                                            case "or":
                                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                            conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                        else
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    }
                                                }
                                            }

                                            break;
                                        case "@organizeId": // 当前组织
                                            {
                                                var organizeId = User.OrganizeId;
                                                if (!string.IsNullOrEmpty(organizeId))
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = organizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = organizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                            break;
                                                    }
                                                }

                                            }

                                            break;
                                        case "@organizationAndSuborganization": // 当前组织及子组织
                                            {
                                                if (!string.IsNullOrEmpty(User.OrganizeId))
                                                {
                                                    var ids = CurrentOrganizationAndSubOrganizations;
                                                    for (int i = 0; i < ids.Count; i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            switch (conditionItem.Logic)
                                                            {
                                                                case "and":
                                                                    conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                                case "or":
                                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                                conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                            else
                                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case "@branchManageOrganize": // 当前分管组织
                                            {
                                                var ids = DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList();
                                                if (ids != null)
                                                {
                                                    for (int i = 0; i < ids.Count; i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            switch (conditionItem.Logic)
                                                            {
                                                                case "and":
                                                                    conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                                case "or":
                                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                                conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                            else
                                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                                }
                                            }

                                            break;

                                        case "@branchManageOrganizeAndSub": // 当前分管组织及子组织
                                            {
                                                var ids = new List<string>();
                                                DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList()
                                                    .ForEach(item => ids.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.OrganizeIdTree.Contains(item)).Select(x => x.Id).ToList()));

                                                if (ids.Any())
                                                {
                                                    for (int i = 0; i < ids.Count; i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            switch (conditionItem.Logic)
                                                            {
                                                                case "and":
                                                                    conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                                case "or":
                                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                                conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                            else
                                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                                }
                                            }

                                            break;

                                        default:
                                            {
                                                if (!string.IsNullOrEmpty(itemValue))
                                                {
                                                    var defCmodel = GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type);
                                                    if (defCmodel.ConditionalType.Equals(ConditionalType.In)) defCmodel.ConditionalType = ConditionalType.Like;
                                                    if (defCmodel.ConditionalType.Equals(ConditionalType.NotIn)) defCmodel.ConditionalType = ConditionalType.NoLike;
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                            break;
                                                    }
                                                }
                                            }

                                            break;
                                    }
                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = string.Empty, ConditionalType = ConditionalType.IsNullOrEmpty } });
                                    codeGenConditionalObject.Find(it => it.FieldRule == fieldRule && it.TableName.Equals(tableName)).conditionalModel.AddRange(conditionalList);
                                }

                                if (codeGenConditionalObject.Any())
                                {
                                    var firstItem = codeGenConditionalObject.Find(it => it.FieldRule.Equals(fieldRule) && it.TableName.Equals(tableName)).conditionalModel.First().ToObject<dynamic>();
                                    firstItem.Key = 0;
                                    conditionalList[0] = firstItem;
                                    groupsList.Add(new { Key = (int)WhereType.And, Value = new { ConditionalList = codeGenConditionalObject.Find(it => it.FieldRule == fieldRule && it.TableName.Equals(tableName)).conditionalModel } });
                                    codeGenConditionalObject.Find(it => it.FieldRule.Equals(fieldRule) && it.TableName.Equals(tableName)).conditionalModel = groupsList;
                                    groupsList = new List<object>();
                                }
                            }

                            if (codeGenConditionalObject.Any())
                            {
                                allList.Add(new { Key = (int)WhereType.And, Value = new { ConditionalList = codeGenConditionalObject.Find(it => it.FieldRule.Equals(fieldRule) && it.TableName.Equals(tableName)).conditionalModel } });
                                codeGenConditionalObject.Find(it => it.FieldRule.Equals(fieldRule) && it.TableName.Equals(tableName)).conditionalModel = allList;
                                allList = new List<object>();
                            }
                        }

                        if (codeGenConditionalObject.Any())
                        {
                            foreach (var conditional in codeGenConditionalObject)
                            {
                                resultList.Add(new { ConditionalList = conditional.conditionalModel });
                                conditional.conditionalModel = resultList;
                                resultList = new List<object>();
                            }
                        }

                        if (codeGenConditionalObject.Any())
                        {
                            foreach (var conditional in codeGenConditionalObject)
                            {
                                if (!codeGenConditional.Any(it => it.FieldRule == conditional.FieldRule && it.TableName.Equals(conditional.TableName)))
                                {
                                    codeGenConditional.Add(new CodeGenAuthorizeModuleResourceModel
                                    {
                                        FieldRule = conditional.FieldRule,
                                        TableName = conditional.TableName,
                                        conditionalModel = new List<IConditionalModel>()
                                    });
                                }
                                codeGenConditional.Find(it => it.FieldRule.Equals(conditional.FieldRule) && it.TableName.Equals(conditional.TableName)).conditionalModel = _repository.AsSugarClient().Utilities.JsonToConditionalModels(conditional.conditionalModel.ToJsonString());
                            }
                        }
                        break;
                }
                break;
        }

        return codeGenConditional.FindAll(it => it.conditionalModel.Count > 0);
    }

    /// <summary>
    /// 获取数据条件(在线开发专用) .
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    public async Task<List<IConditionalModel>> GetCondition<T>(string primaryKey, string moduleId, bool isDataPermissions = true)
        where T : new()
    {
        var conModels = new List<IConditionalModel>();
        if (IsAdministrator) return conModels; // 管理员全部放开

        var roleAuthorizeList = _repository.AsSugarClient().Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                   .In((a, b) => b.Id, Roles).Where(a => a.ItemType == "resource").Select(a => new { a.ItemId, a.ObjectId }).ToList();

        if (!isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                {
                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                }
            });
            return conModels;
        }
        else if (roleAuthorizeList.Count == 0 && isDataPermissions)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                {
                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                }
            });
            return conModels;
        }

        var resourceList = _repository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().In(it => it.Id, roleAuthorizeList.Select(x => x.ItemId).ToList()).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

        if (resourceList.Any(x => x.AllData == 1 || x.EnCode.Equals("poxiao_alldata")))
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                }
            });
        }
        else
        {
            var allList = new List<object>(); // 构造任何层级的条件
            var resultList = new List<object>();
            foreach (var roleId in Roles)
            {
                var isCurrentRole = true;
                var roleList = new List<object>();
                foreach (var item in resourceList.Where(x => roleAuthorizeList.Where(xx => xx.ObjectId.Equals(roleId)).Select(x => x.ItemId).Contains(x.Id)).ToList())
                {
                    var groupsList = new List<object>();
                    foreach (var conditionItem in item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>())
                    {
                        var conditionalList = new List<object>();
                        foreach (var fieldItem in conditionItem.Groups)
                        {
                            var itemField = fieldItem.BindTable.IsNullOrWhiteSpace() ? fieldItem.Field : string.Format("{0}.{1}", fieldItem.BindTable, fieldItem.Field);
                            var itemValue = fieldItem.Value;
                            var itemMethod = (QueryType)System.Enum.Parse(typeof(QueryType), fieldItem.Op);

                            var cmodel = GetConditionalModel(itemMethod, itemField, User.OrganizeId);
                            if (itemMethod.Equals(QueryType.Equal)) cmodel.ConditionalType = ConditionalType.Like;
                            if (itemMethod.Equals(QueryType.NotEqual)) cmodel.ConditionalType = ConditionalType.NoLike;
                            switch (itemValue)
                            {
                                case "@userId": // 当前用户
                                    {
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                            case "or":
                                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = UserId, ConditionalType = (int)cmodel.ConditionalType } });

                                                break;
                                        }
                                    }

                                    break;
                                case "@userAraSubordinates": // 当前用户集下属
                                    {
                                        var ids = new List<string>() { UserId };
                                        ids.AddRange(Subordinates);
                                        for (int i = 0; i < ids.Count; i++)
                                        {
                                            if (i == 0)
                                            {
                                                switch (conditionItem.Logic)
                                                {
                                                    case "and":
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                    case "or":
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                else
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                            }
                                        }
                                    }

                                    break;
                                case "@organizeId": // 当前组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = User.OrganizeId, ConditionalType = (int)cmodel.ConditionalType } });
                                                    break;
                                            }
                                        }
                                    }

                                    break;
                                case "@organizationAndSuborganization": // 当前组织及子组织
                                    {
                                        if (!string.IsNullOrEmpty(User.OrganizeId))
                                        {
                                            var ids = CurrentOrganizationAndSubOrganizations;
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                    }

                                    break;

                                case "@branchManageOrganize": // 当前分管组织
                                    {
                                        var ids = DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList();
                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                case "@branchManageOrganizeAndSub": // 当前分管组织及子组织
                                    {
                                        var ids = new List<string>();
                                        DataScope.Where(x => x.Select).Select(x => x.organizeId).ToList()
                                            .ForEach(item => ids.AddRange(_repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.OrganizeIdTree.Contains(item)).Select(x => x.Id).ToList()));

                                        if (ids.Any())
                                        {
                                            for (int i = 0; i < ids.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    switch (conditionItem.Logic)
                                                    {
                                                        case "and":
                                                            conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                        case "or":
                                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                                        conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                    else
                                                        conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = ids[i], ConditionalType = (int)cmodel.ConditionalType } });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = "poxiao", ConditionalType = (int)ConditionalType.Equal } });
                                        }
                                    }

                                    break;

                                default:
                                    {
                                        if (!string.IsNullOrEmpty(itemValue))
                                        {
                                            var defCmodel = GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type);
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.In)) defCmodel.ConditionalType = ConditionalType.Like;
                                            if (defCmodel.ConditionalType.Equals(ConditionalType.NotIn)) defCmodel.ConditionalType = ConditionalType.NoLike;
                                            switch (conditionItem.Logic)
                                            {
                                                case "and":
                                                    conditionalList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                                case "or":
                                                    conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = itemValue, ConditionalType = (int)defCmodel.ConditionalType } });
                                                    break;
                                            }
                                        }

                                    }

                                    break;
                            }
                            if (itemMethod.Equals(QueryType.NotEqual) || itemMethod.Equals(QueryType.NotIncluded))
                                conditionalList.Add(new { Key = (int)WhereType.Or, Value = new { FieldName = itemField, FieldValue = string.Empty, ConditionalType = ConditionalType.IsNullOrEmpty } });
                        }

                        if (conditionalList.Any())
                        {
                            var firstItem = conditionalList.First().ToObject<dynamic>();
                            firstItem.Key = 0;
                            conditionalList[0] = firstItem;
                            groupsList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = conditionalList } });
                        }
                    }

                    if (groupsList.Any()) roleList.Add(new { Key = isCurrentRole ? (int)WhereType.Or : (int)WhereType.And, Value = new { ConditionalList = groupsList } });
                    isCurrentRole = false;
                }

                if (roleList.Any()) allList.Add(new { Key = (int)WhereType.Or, Value = new { ConditionalList = roleList } });
            }

            if (allList.Any()) resultList.Add(new { ConditionalList = allList });

            if (resultList.Any()) conModels.AddRange(_repository.AsSugarClient().Utilities.JsonToConditionalModels(resultList.ToJsonString()));
        }

        if (resourceList.Count == 0)
        {
            conModels.Add(new ConditionalCollections()
            {
                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
            });
        }

        return conModels;
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="isAdmin">是否管理员.</param>
    /// <returns></returns>
    private async Task<string[]> GetSubsidiaryAsync(string organizeId, bool isAdmin)
    {
        var data = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null && it.EnabledMark.Equals(1)).ToListAsync();
        if (!isAdmin)
            data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);

        return data.Select(m => m.Id).ToArray();
    }

    /// <summary>
    /// 下属机构.
    /// </summary>
    /// <param name="organizeId">机构ID.</param>
    /// <param name="isAdmin">是否管理员.</param>
    /// <returns></returns>
    private string[] GetSubsidiary(string organizeId, bool isAdmin)
    {
        var data = _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(it => it.DeleteMark == null && it.EnabledMark.Equals(1)).ToList();
        if (!isAdmin)
            data = data.TreeChildNode(organizeId, t => t.Id, t => t.ParentId);

        return data.Select(m => m.Id).ToArray();
    }

    /// <summary>
    /// 获取下属.
    /// </summary>
    /// <param name="managerId">主管Id.</param>
    /// <returns></returns>
    private async Task<string[]> GetSubordinatesAsync(string managerId)
    {
        List<string> data = new List<string>();
        var userIds = await _repository.AsQueryable().Where(m => m.ManagerId == managerId && m.DeleteMark == null).Select(m => m.Id).ToListAsync();
        data.AddRange(userIds);

        // 关闭无限级我的下属
        // data.AddRange(await GetInfiniteSubordinats(userIds.ToArray()));
        return data.ToArray();
    }

    /// <summary>
    /// 获取下属.
    /// </summary>
    /// <param name="managerId">主管Id.</param>
    /// <returns></returns>
    private string[] GetSubordinates(string managerId)
    {
        List<string> data = new List<string>();
        var userIds = _repository.AsQueryable().Where(m => m.ManagerId == managerId && m.DeleteMark == null).Select(m => m.Id).ToList();
        data.AddRange(userIds);

        // 关闭无限级我的下属
        // data.AddRange(await GetInfiniteSubordinats(userIds.ToArray()));
        return data.ToArray();
    }

    /// <summary>
    /// 获取下属无限极.
    /// </summary>
    /// <param name="parentIds"></param>
    /// <returns></returns>
    private async Task<List<string>> GetInfiniteSubordinats(string[] parentIds)
    {
        List<string> data = new List<string>();
        if (parentIds.ToList().Count > 0)
        {
            var userIds = await _repository.AsQueryable().In(it => it.ManagerId, parentIds).Where(it => it.DeleteMark == null).OrderBy(it => it.SortCode).Select(it => it.Id).ToListAsync();
            data.AddRange(userIds);
            data.AddRange(await GetInfiniteSubordinats(userIds.ToArray()));
        }

        return data;
    }

    /// <summary>
    /// 获取当前用户岗位信息.
    /// </summary>
    /// <param name="PositionIds"></param>
    /// <returns></returns>
    private async Task<List<PositionInfoModel>> GetPosition(string PositionIds)
    {
        return await _repository.AsSugarClient().Queryable<PositionEntity>().In(it => it.Id, PositionIds.Split(",")).Select(it => new PositionInfoModel { id = it.Id, name = it.FullName }).ToListAsync();
    }

    /// <summary>
    /// 获取条件模型.
    /// </summary>
    /// <returns></returns>
    private ConditionalModel GetConditionalModel(QueryType expressType, string fieldName, string fieldValue, string dataType = "string")
    {
        switch (expressType)
        {
            // 模糊
            case QueryType.Contains:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Like, FieldValue = fieldValue };

            // 等于
            case QueryType.Equal:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue };
                }

            // 不等于
            case QueryType.NotEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue };
                }

            // 小于
            case QueryType.LessThan:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue };
                }

            // 小于等于
            case QueryType.LessThanOrEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue };
                }

            // 大于
            case QueryType.GreaterThan:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue };
                }

            // 大于等于
            case QueryType.GreaterThanOrEqual:
                switch (dataType)
                {
                    case "Double":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                    case "Int32":
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                    default:
                        return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue };
                }

            // 包含
            case QueryType.In:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.In, FieldValue = fieldValue };
            case QueryType.Included:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Like, FieldValue = fieldValue };
            // 不包含
            case QueryType.NotIn:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.In, FieldValue = fieldValue };
            case QueryType.NotIncluded:
                return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoLike, FieldValue = fieldValue };
        }

        return new ConditionalModel();
    }

    /// <summary>
    /// 获取角色名称 根据 角色Ids.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<string> GetRoleNameByIds(string ids)
    {
        if (ids.IsNullOrEmpty())
            return string.Empty;

        var idList = ids.Split(",").ToList();
        var nameList = new List<string>();
        var roleList = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();
        foreach (var item in idList)
        {
            var info = roleList.Find(x => x.Id == item);
            if (info != null && info.FullName.IsNotEmptyOrNull())
            {
                nameList.Add(info.FullName);
            }
        }

        return string.Join(",", nameList);
    }

    /// <summary>
    /// 根据角色Ids和组织Id 获取组织下的角色以及全局角色.
    /// </summary>
    /// <param name="roleIds">角色Id集合.</param>
    /// <param name="organizeId">组织Id.</param>
    /// <returns></returns>
    public async Task<List<string>> GetUserOrgRoleIds(string roleIds, string organizeId)
    {
        if (roleIds.IsNotEmptyOrNull())
        {
            var userRoleIds = roleIds.Split(",");

            // 当前组织下的角色Id 集合
            var roleList = await _repository.AsSugarClient().Queryable<OrganizeRelationEntity>()
                .Where(x => x.OrganizeId == organizeId && x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToListAsync();

            // 全局角色Id 集合
            var gRoleList = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => userRoleIds.Contains(x.Id) && x.GlobalMark == 1)
                .Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(x => x.Id).ToListAsync();

            roleList.AddRange(gRoleList); // 组织角色 + 全局角色

            return roleList;
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 根据角色Ids和组织Id 获取组织下的角色以及全局角色.
    /// </summary>
    /// <param name="roleIds">角色Id集合.</param>
    /// <param name="organizeId">组织Id.</param>
    /// <returns></returns>
    public List<string> GetUserRoleIds(string roleIds, string organizeId)
    {
        if (roleIds.IsNotEmptyOrNull())
        {
            var userRoleIds = roleIds.Split(",");

            // 当前组织下的角色Id 集合
            var roleList = _repository.AsSugarClient().Queryable<OrganizeRelationEntity>()
                .Where(x => x.OrganizeId == organizeId && x.ObjectType == "Role" && userRoleIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();

            // 全局角色Id 集合
            var gRoleList = _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => userRoleIds.Contains(x.Id) && x.GlobalMark == 1)
                .Where(r => r.EnabledMark == 1 && r.DeleteMark == null).Select(x => x.Id).ToList();

            roleList.AddRange(gRoleList); // 组织角色 + 全局角色

            return roleList;
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 会否存在用户缓存.
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    private async Task<bool> ExistsUserInfo(string cacheKey)
    {
        return await _cacheManager.ExistsAsync(cacheKey);
    }

    /// <summary>
    /// 保存用户登录信息.
    /// </summary>
    /// <param name="cacheKey">key.</param>
    /// <param name="userInfo">用户信息.</param>
    /// <param name="timeSpan">过期时间.</param>
    /// <returns></returns>
    private async Task<bool> SetUserInfo(string cacheKey, UserInfoModel userInfo, TimeSpan timeSpan)
    {
        return await _cacheManager.SetAsync(cacheKey, userInfo, timeSpan);
    }

    /// <summary>
    /// 获取全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private GlobalTenantCacheModel GetGlobalTenantCache(string tenantId)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        return _cacheManager.Get<List<GlobalTenantCacheModel>>(cacheKey).Find(it => it.TenantId.Equals(tenantId));
    }

    /// <summary>
    /// 获取用户登录信息.
    /// </summary>
    /// <param name="cacheKey">key.</param>
    /// <returns></returns>
    private async Task<UserInfoModel> GetUserInfo(string cacheKey)
    {
        return (await _cacheManager.GetAsync(cacheKey)).Adapt<UserInfoModel>();
    }

    /// <summary>
    /// 获取用户名称.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否带账号.</param>
    /// <returns></returns>
    public string GetUserName(string userId, bool isAccount = true)
    {
        UserEntity? entity = _repository.GetFirst(x => x.Id == userId && x.DeleteMark == null);
        if (entity.IsNullOrEmpty()) return string.Empty;
        return isAccount ? entity.RealName + "/" + entity.Account : entity.RealName;
    }

    /// <summary>
    /// 获取用户名称.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否带账号.</param>
    /// <returns></returns>
    public async Task<string> GetUserNameAsync(string userId, bool isAccount = true)
    {
        UserEntity? entity = await _repository.GetFirstAsync(x => x.Id == userId && x.DeleteMark == null);
        if (entity.IsNullOrEmpty()) return string.Empty;
        return isAccount ? entity.RealName + "/" + entity.Account : entity.RealName;
    }

    /// <summary>
    /// 获取管理员用户id.
    /// </summary>
    public string GetAdminUserId()
    {
        var user = _repository.AsSugarClient().Queryable<UserEntity>().First(x => x.Account == "admin" && x.DeleteMark == null);
        if (user.IsNotEmptyOrNull()) return user.Id;
        return string.Empty;
    }

    /// <summary>
    /// 获取门户id.
    /// </summary>
    /// <param name="entityList">当前系统的所有门户管理.</param>
    /// <param name="portalId">用户表门户ID.</param>
    /// <param name="systemId">当前系统ID.</param>
    /// <param name="userId">当前用户ID.</param>
    /// <param name="type">门户类型（web、app）.</param>
    /// <returns></returns>
    private async Task<string> GetPortalId(List<PortalManageEntity> entityList, string portalId, string systemId, string userId, string type)
    {
        var portalDic = new Dictionary<string, string>();
        if (portalId.IsNotEmptyOrNull())
        {
            // 目前系统的门户
            if (portalId.Contains("{"))
            {
                portalDic = portalId.ToObject<Dictionary<string, string>>();
            }
        }

        var portalIds = entityList
            .Where(it => it.Platform != null && it.Platform.Equals(type))
            .Select(it => it.PortalId)
            .ToList();
        if (portalIds.Count > 0)
        {
            // 侧边栏第一个门户
            var firstPortalId = await _repository.AsSugarClient().Queryable<PortalEntity, DictionaryDataEntity>((p, d) => new JoinQueryInfos(JoinType.Left, p.Category == d.Id))
                .Where(p => portalIds.Contains(p.Id))
                .OrderBy((p, d) => d.SortCode)
                .OrderBy(p => p.SortCode)
                .OrderBy(p => p.CreatorTime, OrderByType.Desc)
                .OrderBy(p => p.LastModifyTime, OrderByType.Desc)
                .Select(p => p.Id)
                .FirstAsync();

            var key = string.Format("{0}:{1}", type, systemId);
            if (portalDic.ContainsKey(key))
            {
                var portalData = portalDic[key];
                if (portalData.IsNotEmptyOrNull() && portalIds.Contains(portalData))
                {
                    return portalData;
                }
                else
                {
                    portalDic[key] = firstPortalId;

                    var portal = portalDic.ToJsonString();
                    await _repository.AsSugarClient().Updateable<UserEntity>()
                        .Where(it => it.Id.Equals(userId))
                        .SetColumns(it => new UserEntity()
                        {
                            PortalId = portal,
                            LastModifyTime = SqlFunc.GetDate(),
                            LastModifyUserId = userId
                        })
                        .ExecuteCommandAsync();

                    return firstPortalId;
                }
            }
            else
            {
                portalDic.Add(key, firstPortalId);

                var portal = portalDic.ToJsonString();
                await _repository.AsSugarClient().Updateable<UserEntity>()
                    .Where(it => it.Id.Equals(userId))
                    .SetColumns(it => new UserEntity()
                    {
                        PortalId = portal,
                        LastModifyTime = SqlFunc.GetDate(),
                        LastModifyUserId = userId
                    })
                    .ExecuteCommandAsync();

                return firstPortalId;
            }
        }
        else
        {
            return string.Empty;
        }
    }
}