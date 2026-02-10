using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Poxiao.Lab.Entity.Dto.ReportConfig;

public class ReportConfigInputDto
{
    public string Id { get; set; }

    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; }

    public List<string> LevelNames { get; set; }

    public int SortOrder { get; set; }

    public string Description { get; set; }
    public bool IsHeader { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsShowInReport { get; set; }
    public bool IsShowRatio { get; set; }
    public string FormulaId { get; set; }
}
