namespace Poxiao.Lab.Entity.Dto;

/// <summary>
/// Excel导入模板输入 DTO.
/// </summary>
public class ExcelImportTemplateInput
{
    /// <summary>
    /// 模板名称.
    /// </summary>
    public string TemplateName { get; set; }

    /// <summary>
    /// 模板描述.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 模板配置（JSON格式字符串）.
    /// </summary>
    public string ConfigJson { get; set; }
}
