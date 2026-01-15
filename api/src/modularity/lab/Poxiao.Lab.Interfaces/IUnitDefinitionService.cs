using Poxiao.Lab.Entity.Dto.Unit;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 单位定义服务接口.
/// </summary>
public interface IUnitDefinitionService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    Task<List<UnitDefinitionDto>> GetList(string? categoryId = null);

    /// <summary>
    /// 获取详情.
    /// </summary>
    Task<UnitDefinitionDto> GetInfo(string id);

    /// <summary>
    /// 创建.
    /// </summary>
    Task Create(UnitDefinitionInput input);

    /// <summary>
    /// 更新.
    /// </summary>
    Task Update(string id, UnitDefinitionInput input);

    /// <summary>
    /// 删除.
    /// </summary>
    Task Delete(string id);
}
