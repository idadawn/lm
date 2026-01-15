using Poxiao.Systems.Entitys.Dto.DbLink;
using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 数据连接
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IDbLinkService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    Task<List<DbLinkListOutput>> GetList();

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    Task<DbLinkEntity> GetInfo(string id);
}