using Poxiao.Lab.Entity.Config;

namespace Poxiao.Lab.Entity.Dto;

/// <summary>
/// Excel导入模板 DTO.
/// </summary>
public class ExcelImportTemplateDto
{
    /// <summary>
    /// 主键 ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 模板名称.
    /// </summary>
    public string TemplateName { get; set; }

    /// <summary>
    /// 模板编码.
    /// </summary>
    public string TemplateCode { get; set; }

    /// <summary>
    /// 模板描述.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 单位ID.
    /// </summary>
    public string UnitId { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    public DateTime? LastModifyTime { get; set; }
}

/// <summary>
/// Excel表头解析输入参数.
/// </summary>
public class ExcelParseHeadersInput
{
    /// <summary>
    /// 文件名.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件数据(Base64).
    /// </summary>
    public string FileData { get; set; }
}

/// <summary>
/// Excel模板验证输入参数.
/// </summary>
public class ExcelTemplateValidationInput
{
    /// <summary>
    /// 模板编码.
    /// </summary>
    public string TemplateCode { get; set; }

    /// <summary>
    /// 文件名.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件数据(Base64).
    /// </summary>
    public string FileData { get; set; }
}

/// <summary>
/// Excel模板验证结果.
/// </summary>
public class ExcelTemplateValidationResult
{
    /// <summary>
    /// 验证是否通过.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 验证错误信息列表.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Excel表头信息.
    /// </summary>
    public List<ExcelHeaderDto> ExcelHeaders { get; set; } = new List<ExcelHeaderDto>();

    /// <summary>
    /// 模板配置信息.
    /// </summary>
    public ExcelTemplateConfig TemplateConfig { get; set; }
}
