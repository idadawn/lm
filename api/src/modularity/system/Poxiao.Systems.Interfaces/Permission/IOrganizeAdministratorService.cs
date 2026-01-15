using Poxiao.Infrastructure.Models.User;

namespace Poxiao.Systems.Interfaces.Permission;

/// <summary>
/// 分级管理
/// 版 本：V1.0.0.5
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021.09.27.
/// </summary>
public interface IOrganizeAdministratorService
{
    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<UserDataScopeModel>> GetUserDataScopeModel(string userId);
}