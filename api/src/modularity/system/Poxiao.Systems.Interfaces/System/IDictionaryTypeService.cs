using Poxiao.Systems.Entitys.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 字典分类
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IDictionaryTypeService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    Task<List<DictionaryTypeEntity>> GetList();

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DictionaryTypeEntity> GetInfo(string id);

    /// <summary>
    /// 递归获取所有分类.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="typeList"></param>
    /// <returns></returns>
    Task GetListAllById(string id, List<DictionaryTypeEntity> typeList);

    /// <summary>
    /// 是否存在上级.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    bool IsExistParent(List<DictionaryTypeEntity> entities);
}