namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 等级统计（合格等级需要 重量+占比）
/// </summary>
public class LevelStatDto
{
    /// <summary>
    /// 重量（kg）
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// 占比（%）
    /// </summary>
    public decimal Rate { get; set; }
}
