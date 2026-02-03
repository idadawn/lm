using Poxiao.Lab.Entity.Enum;

namespace Poxiao.Lab.Entity.Dto.MonthlyQualityReport;

/// <summary>
/// 判定等级列定义（告诉前端有哪些动态列）
/// </summary>
public class JudgmentLevelColumnDto
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    
    /// <summary>
    /// 合格状态（Qualified=合格，Unqualified=不合格）
    /// </summary>
    public QualityStatusEnum QualityStatus { get; set; }
    
    public string Color { get; set; }
    public int? Priority { get; set; }
}
