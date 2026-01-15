using Poxiao.Systems.Entitys.System;

namespace Poxiao.Apps.Interfaces;

/// <summary>
/// App常用数据
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
public interface IAppDataService
{
    /// <summary>
    /// 菜单列表.
    /// </summary>
    /// <returns></returns>
    Task<List<ModuleEntity>> GetAppMenuList(string keyword);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    Task Delete(string objectId);
}