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
    /// 模板编码（唯一）.
    /// </summary>
    public string TemplateCode { get; set; }

    /// <summary>
    /// 模板描述.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 模板类型：system系统模板/user个人模板.
    /// </summary>
    public string TemplateType { get; set; } = "system";

    /// <summary>
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 模板配置（JSON格式字符串）.
    /// </summary>
    public string ConfigJson { get; set; }

    /// <summary>
    /// 关联的产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 是否默认模板（0=否，1=是）.
    /// </summary>
    public int IsDefault { get; set; } = 0;

    /// <summary>
    /// 排序码.
    /// </summary>
    public long? SortCode { get; set; }
}