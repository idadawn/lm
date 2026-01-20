using Poxiao.Lab.Entity.Dto.MagneticData;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 磁性数据导入会话服务接口
/// </summary>
public interface IMagneticDataImportSessionService
{
    /// <summary>
    /// 创建导入会话
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> Create(MagneticDataImportSessionInput input);

    /// <summary>
    /// 获取会话信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<MagneticDataImportSessionEntity> Get(string id);

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
    /// <param name="sessionId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MagneticDataImportStep1Output> UploadAndParse(string sessionId, MagneticDataImportSessionInput input);

    /// <summary>
    /// 获取核对结果（第二步）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<MagneticDataReviewOutput> GetReviewData(string sessionId);

    /// <summary>
    /// 完成导入（第二步）
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
