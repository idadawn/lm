using Poxiao.Lab.Entity.Dto.Trace;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 炉号追溯聚合查询服务接口.
/// </summary>
public interface ITraceService
{
    /// <summary>
    /// 根据扫码内容追溯炉号全链路数据（原始数据/中间数据/磁性数据/单片数据）.
    /// </summary>
    /// <param name="code">扫码枪扫描内容（炉号纯文本，可能带尾部K或特性汉字）.</param>
    /// <returns>炉号追溯聚合结果.</returns>
    Task<TraceOutput> Trace(string code);
}
