using Poxiao.Systems.Entitys.Dto.Organize;
using Poxiao.Systems.Entitys.Dto.SysConfig;
using Poxiao.Systems.Entitys.Permission;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 第三方同步
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface ISynThirdInfoService
{
    /// <summary>
    /// 组织同步.
    /// </summary>
    /// <param name="thirdType"></param>
    /// <param name="dataType"></param>
    /// <param name="sysConfig"></param>
    /// <param name="orgList"></param>
    /// <returns></returns>
    Task SynDep(int thirdType, int dataType, SysConfigOutput sysConfig, List<OrganizeListOutput> orgList);

    /// <summary>
    /// 用户同步.
    /// </summary>
    /// <param name="thirdType"></param>
    /// <param name="dataType"></param>
    /// <param name="sysConfig"></param>
    /// <param name="userList"></param>
    /// <returns></returns>
    Task SynUser(int thirdType, int dataType, SysConfigOutput sysConfig, List<UserEntity> userList);

    /// <summary>
    /// 删除同步数据.
    /// </summary>
    /// <param name="thirdType"></param>
    /// <param name="dataType"></param>
    /// <param name="sysConfig"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DelSynData(int thirdType, int dataType, SysConfigOutput sysConfig, string id);

    /// <summary>
    /// 根据系统主键获取第三方主键.
    /// </summary>
    /// <param name="ids">系统主键.</param>
    /// <param name="thirdType">第三方类型.</param>
    /// <param name="dataType">数据类型.</param>
    /// <returns></returns>
    Task<List<string>> GetThirdIdList(List<string> ids, int thirdType, int dataType);
}