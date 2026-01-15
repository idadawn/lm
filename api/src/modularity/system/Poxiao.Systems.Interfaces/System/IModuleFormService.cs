using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 表单权限
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IModuleFormService
{
    /// <summary>
    /// 表单权限列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleFormEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 获取用户功能表单.
    /// </summary>
    Task<dynamic> GetUserModuleFormList();
}