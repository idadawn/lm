using Poxiao.DynamicApiController;

namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 驾驶舱查询参数
/// </summary>
public class DashboardQueryDto
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 班次筛选
    /// </summary>
    public string? Shift { get; set; }
}
