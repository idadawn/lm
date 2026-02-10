namespace Poxiao.Lab.Entity.Dto.Dashboard;

/// <summary>
/// 今日产量数据
/// </summary>
public class DailyProductionDto
{
    /// <summary>
    /// 今日产量(kg)
    /// </summary>
    public decimal TodayWeight { get; set; }

    /// <summary>
    /// 昨日产量(kg)
    /// </summary>
    public decimal YesterdayWeight { get; set; }

    /// <summary>
    /// 环比变化率(%)
    /// </summary>
    public decimal ChangeRate { get; set; }
}
