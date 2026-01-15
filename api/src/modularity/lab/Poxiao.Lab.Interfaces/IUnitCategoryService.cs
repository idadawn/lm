using Poxiao.Lab.Entity.Dto.Unit;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 单位维度服务接口.
/// </summary>
public interface IUnitCategoryService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    Task<List<UnitCategoryDto>> GetList();

    /// <summary>
    /// 获取详情.
    /// </summary>
    Task<UnitCategoryDto> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    Task Create(UnitCategoryInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    Task Update(string id, UnitCategoryInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    Task Delete(string id);
}
