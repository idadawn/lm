using System.Collections.Generic;

namespace Poxiao.Lab.Entity.Dto;

/// <summary>
/// 系统字段响应结果.
/// </summary>
public class SystemFieldResult
{
    /// <summary>
    /// 系统字段列表.
    /// </summary>
    public List<SystemFieldDto> Fields { get; set; } = new List<SystemFieldDto>();

    /// <summary>
    /// Excel表头信息（从已保存配置中恢复）.
    /// </summary>
    public List<ExcelHeaderDto> ExcelHeaders { get; set; } = new List<ExcelHeaderDto>();
}
