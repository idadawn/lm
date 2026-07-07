using Poxiao.Lab.Entity.Dto.SingleSheet;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 单片性能原始数据服务接口.
/// </summary>
public interface ISingleSheetRawDataService
{
    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<dynamic> GetList(SingleSheetRawDataListQuery input);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);
}
