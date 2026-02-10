using System.Collections.Generic;

namespace Poxiao.Lab.Entity.Dto.ReportConfig;

public class ReportConfigDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> LevelNames { get; set; }
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; }
    public bool IsHeader { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsShowInReport { get; set; }
    public bool IsShowRatio { get; set; }
    public string FormulaId { get; set; }
}
