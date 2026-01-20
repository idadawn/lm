using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity.Dto.RawData;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 原始数据服务接口.
/// </summary>
public interface IRawDataService
{
    /// <summary>
    /// 预览数据（用于数据核对）.
    /// </summary>
    /// <param name="input">导入输入（包含文件内容）.</param>
    /// <returns></returns>
    Task<RawDataPreviewOutput> Preview(RawDataImportInput input);

    /// <summary>
    /// 获取导入日志列表.
    /// </summary>
    /// <param name="input">分页查询.</param>
    /// <returns></returns>
    Task<List<RawDataImportLogListOutput>> GetImportLogList(PageInputBase input);

    /// <summary>
    /// 导入Excel数据.
    /// </summary>
    /// <param name="input">导入输入.</param>
    /// <returns></returns>
    Task<RawDataImportOutput> ImportExcel(RawDataImportInput input);

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<List<RawDataListOutput>> GetList(RawDataListQuery input);

    /// <summary>
    /// 获取详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task<RawDataInfoOutput> GetInfo(string id);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    Task Delete(string id);
}
