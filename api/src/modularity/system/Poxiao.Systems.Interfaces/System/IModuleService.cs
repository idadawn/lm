using Poxiao.Systems.Entitys.Dto.Module;
using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 菜单管理
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IModuleService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    Task<List<ModuleEntity>> GetList(string systemId);

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    Task<ModuleEntity> GetInfo(string id);

    /// <summary>
    /// 获取用户菜单树.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <returns></returns>
    Task<List<ModuleNodeOutput>> GetUserTreeModuleList(string type);

    /// <summary>
    /// 获取用户树形模块功能列表根据 SystemId.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <param name="systemId">SystemId.</param>
    Task<List<ModuleNodeOutput>> GetUserTreeModuleListBySystemId(string type, string systemId);

    /// <summary>
    /// 获取用户菜单列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    Task<dynamic> GetUserModueList(string type);
}