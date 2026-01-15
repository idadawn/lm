using Poxiao.Systems.Entitys.Dto.ModuleColumn;
using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 功能列表
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IModuleColumnService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleColumnEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 获取用户功能列表.
    /// </summary>
    Task<dynamic> GetUserModuleColumnList();
}