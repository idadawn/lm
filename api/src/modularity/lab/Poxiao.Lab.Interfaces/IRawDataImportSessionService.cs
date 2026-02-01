using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.RawData;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 原始数据导入会话服务接口
/// </summary>
public interface IRawDataImportSessionService
{
    /// <summary>
    /// 创建导入会话
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> Create(RawDataImportSessionInput input);

    /// <summary>
    /// 获取会话信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<RawDataImportSessionEntity> Get(string id);

    /// <summary>
    /// 更新会话状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task UpdateStatus(string id, string status);

    /// <summary>
    /// 更新当前步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    Task UpdateStep(string id, int step);

    /// <summary>
    /// 上传并解析文件（第一步）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<RawDataImportStep1Output> UploadAndParse(RawDataImportStep1Input input);

    /// <summary>
    /// 获取产品规格匹配结果（第二步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<List<RawDataProductSpecMatchOutput>> GetProductSpecMatches(string sessionId);

    /// <summary>
    /// 更新产品规格（第二步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task UpdateProductSpecs(string sessionId, RawDataUpdateProductSpecsInput input);

    /// <summary>
    /// 获取特性匹配结果（第三步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<List<RawDataFeatureMatchOutput>> GetFeatureMatches(string sessionId);

    /// <summary>
    /// 更新特性匹配（第三步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task UpdateFeatures(string sessionId, RawDataUpdateFeaturesInput input);

    /// <summary>
    /// 获取核对结果（第四步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<RawDataReviewOutput> GetReviewData(string sessionId);

    /// <summary>
    /// 获取核对数据列表（分页）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<RawDataReviewDataOutput> GetReviewDataPage(string sessionId, int pageIndex = 1, int pageSize = 10);

    /// <summary>
    /// 完成导入（第四步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task CompleteImport(string sessionId);

    /// <summary>
    /// 取消导入（清理会话和临时文件）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task CancelImport(string sessionId);
}
