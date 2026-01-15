using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 数据权限
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IModuleDataAuthorizeService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleDataAuthorizeEntity>> GetList(string? moduleId = default);
}