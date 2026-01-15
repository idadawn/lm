using Poxiao.Systems.Entitys.Dto.SysConfig;
using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 系统配置
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface ISysConfigService
{
    /// <summary>
    /// 系统配置信息.
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    Task<SysConfigEntity> GetInfo(string category, string key);

    /// <summary>
    /// 获取系统配置.
    /// </summary>
    /// <returns></returns>
    Task<SysConfigOutput> GetInfo();
}