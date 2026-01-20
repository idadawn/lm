using System.Text.Json.Serialization;

namespace Poxiao.Lab.Entity.Dto;

/// <summary>
/// Excel表头信息 DTO.
/// </summary>
public class ExcelHeaderDto
{
    /// <summary>
    /// Excel列名.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Excel列索引（A, B, C...）.
    /// </summary>
    public string Index { get; set; }
}
