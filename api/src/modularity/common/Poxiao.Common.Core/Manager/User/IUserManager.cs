using Poxiao.Infrastructure.Models.Authorize;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Systems.Entitys.Permission;
using SqlSugar;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// 用户管理抽象.
/// </summary>
public interface IUserManager
{
    /// <summary>
    /// 用户编号.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// 用户角色.
    /// </summary>
    List<string> Roles { get; }

    /// <summary>
    /// 租户ID.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// 租户数据库名称.
    /// </summary>
    string TenantDbName { get; }

    /// <summary>
    /// 用户账号.
    /// </summary>
    string Account { get; }

    /// <summary>
    /// 用户昵称.
    /// </summary>
    string RealName { get; }

    /// <summary>
    /// 当前用户 ToKen.
    /// </summary>
    string ToKen { get; }

    /// <summary>
    /// 是否管理员.
    /// </summary>
    bool IsAdministrator { get; }

    /// <summary>
    /// 当前用户下属.
    /// </summary>
    List<string> Subordinates { get; }

    /// <summary>
    /// 当前用户及下属.
    /// </summary>
    List<string> CurrentUserAndSubordinates { get; }

    /// <summary>
    /// 用户当前组织及子组织.
    /// </summary>
    List<string> CurrentOrganizationAndSubOrganizations { get; }

    /// <summary>
    /// 当前用户子组织.
    /// </summary>
    List<string> CurrentUserSubOrganization { get; }

    /// <summary>
    /// 获取用户的数据范围.
    /// </summary>
    List<UserDataScopeModel> DataScope { get; }

    /// <summary>
    /// 获取请求端类型 pc 、 app.
    /// </summary>
    string UserOrigin { get; }

    /// <summary>
    /// 用户信息.
    /// </summary>
    UserEntity User { get; }

    GlobalTenantCacheModel CurrentTenantInformation { get; }

    /// <summary>
    /// 获取用户登录信息.
    /// </summary>
    /// <returns></returns>
    Task<UserInfoModel> GetUserInfo();

    /// <summary>
    /// 获取数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <param name="tableNumber">联表编号.</param>
    /// <returns></returns>
    Task<List<IConditionalModel>> GetConditionAsync<T>(string moduleId, string primaryKey = "F_Id", bool isDataPermissions = true, string tableNumber = "")
        where T : new();

    /// <summary>
    /// 获取数据条件(带副表) .
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    Task<List<IConditionalModel>> GetDataConditionAsync<T>(string moduleId, string primaryKey, bool isDataPermissions = true)
        where T : new();

    /// <summary>
    /// 获取数据条件(在线开发专用).
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    Task<List<IConditionalModel>> GetCondition<T>(string primaryKey, string moduleId, bool isDataPermissions = true)
        where T : new();

    /// <summary>
    /// 获取代码生成数据条件.
    /// </summary>
    /// <typeparam name="T">实体.</typeparam>
    /// <param name="moduleId">模块ID.</param>
    /// <param name="primaryKey">表主键.</param>
    /// <param name="isDataPermissions">是否开启数据权限.</param>
    /// <returns></returns>
    Task<List<CodeGenAuthorizeModuleResourceModel>> GetCodeGenAuthorizeModuleResource<T>(string moduleId, string primaryKey, bool isDataPermissions = true)
        where T : new();

    /// <summary>
    /// 获取角色名称 根据 角色Ids.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<string> GetRoleNameByIds(string ids);

    /// <summary>
    /// 根据角色Ids和组织Id 获取组织下的角色以及全局角色.
    /// </summary>
    /// <param name="roleIds">角色Id集合.</param>
    /// <param name="organizeId">组织Id.</param>
    /// <returns></returns>
    Task<List<string>> GetUserOrgRoleIds(string roleIds, string organizeId);

    /// <summary>
    ///  获取用户名.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否带账号.</param>
    /// <returns></returns>
    string GetUserName(string userId, bool isAccount = true);

    /// <summary>
    ///  获取用户名.
    /// </summary>
    /// <param name="userId">用户id.</param>
    /// <param name="isAccount">是否带账号.</param>
    /// <returns></returns>
    Task<string> GetUserNameAsync(string userId, bool isAccount = true);

    /// <summary>
    /// 获取管理员用户id.
    /// </summary>
    string GetAdminUserId();
}