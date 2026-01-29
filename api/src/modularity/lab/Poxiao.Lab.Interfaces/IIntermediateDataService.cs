using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 中间数据服务接口.
/// </summary>
public interface IIntermediateDataService
{
    /// <summary>
    /// 获取中间数据列表.
    /// </summary>
    /// <param name="input">查询条件</param>
    /// <returns>分页数据</returns>
    Task<dynamic> GetList(IntermediateDataListQuery input);

    /// <summary>
    /// 获取中间数据详情.
    /// </summary>
    /// <param name="id">数据ID</param>
    /// <returns>详情数据</returns>
    Task<IntermediateDataInfoOutput> GetInfo(string id);

    /// <summary>
    /// 从原始数据生成中间数据.
    /// </summary>
    /// <param name="input">生成条件</param>
    /// <returns>生成结果</returns>
    Task<IntermediateDataGenerateOutput> Generate(IntermediateDataGenerateInput input);

    /// <summary>
    /// 更新性能数据.
    /// </summary>
    /// <param name="input">性能数据</param>
    /// <returns></returns>
    Task UpdatePerformance(IntermediateDataPerfUpdateInput input);

    /// <summary>
    /// 更新外观特性.
    /// </summary>
    /// <param name="input">外观特性数据</param>
    /// <returns></returns>
    Task UpdateAppearance(IntermediateDataAppearUpdateInput input);

    /// <summary>
    /// 更新基础信息.
    /// </summary>
    /// <param name="input">基础信息</param>
    /// <returns></returns>
    Task UpdateBaseInfo(IntermediateDataBaseUpdateInput input);

    /// <summary>
    /// 删除中间数据.
    /// </summary>
    /// <param name="id">数据ID</param>
    /// <returns></returns>
    Task Delete(string id);

    /// <summary>
    /// 批量删除中间数据.
    /// </summary>
    /// <param name="ids">数据ID列表</param>
    /// <returns></returns>
    Task BatchDelete(List<string> ids);

    /// <summary>
    /// 重新计算.
    /// </summary>
    /// <param name="ids">数据ID列表</param>
    /// <returns>计算结果</returns>
    Task<FormulaCalculationResult> Recalculate(List<string> ids);

    /// <summary>
    /// 获取产品规格列表（用于筛选）.
    /// </summary>
    /// <returns>产品规格列表</returns>
    Task<List<ProductSpecOption>> GetProductSpecOptions();

    /// <summary>
    /// 解析检测列配置 (生成 1 到 N 的列表).
    /// </summary>
    List<int> ParseDetectionColumns(int? detectionColumnsCount);

    /// <summary>
    /// 从原始数据生成中间数据.
    /// </summary>
    /// <param name="rawData">原始数据</param>
    /// <param name="productSpec">产品规格</param>
    /// <param name="detectionColumns">检测列</param>
    /// <param name="layers">层数</param>
    /// <param name="length">长度</param>
    /// <param name="density">密度</param>
    /// <param name="specVersion">规格版本</param>
    /// <param name="batchId">批次ID，用于后续异步公式计算</param>
    Task<IntermediateDataEntity> GenerateIntermediateDataAsync(
        RawDataEntity rawData,
        ProductSpecEntity productSpec,
        List<int> detectionColumns,
        int layers,
        decimal length,
        decimal density,
        int? specVersion,
        string batchId = null
    );

    /// <summary>
    /// 同步计算CALC公式（不持久化）.
    /// </summary>
    /// <param name="entities">中间数据实体列表</param>
    Task ApplyCalcFormulasForEntitiesAsync(List<IntermediateDataEntity> entities);

    /// <summary>
    /// 根据批次ID批量计算公式.
    /// </summary>
    /// <param name="batchId">批次ID</param>
    Task<FormulaCalculationResult> BatchCalculateFormulasByBatchIdAsync(string batchId);
}

/// <summary>
/// 产品规格选项.
/// </summary>
public class ProductSpecOption
{
    /// <summary>
    /// 规格ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 规格代码.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 规格名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 检测列.
    /// </summary>
    public int DetectionColumns { get; set; }

    /// <summary>
    /// 长度.
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// 层数.
    /// </summary>
    public int? Layers { get; set; }
}
