using Poxiao.Infrastructure.Filter;

namespace Poxiao.Lab.Entity.Dto.MagneticData;

/// <summary>
/// 磁性原始数据列表查询参数.
/// </summary>
public class MagneticRawDataListQuery : PageInputBase
{
    /// <summary>
    /// 关键字（炉号）.
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 开始日期.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否有效数据（0-无效，1-有效）.
    /// </summary>
    public int? IsValidData { get; set; }
}
