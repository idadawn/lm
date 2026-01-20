using Poxiao.Lab.Entity.Dto.MagneticData;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 磁性原始数据服务接口.
/// </summary>
public interface IMagneticRawDataService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<List<MagneticRawDataListOutput>> GetList(MagneticRawDataListQuery input);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);
}
