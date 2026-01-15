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
    public string TemplateType { get; set; }

    /// <summary>
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 关联的产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产品规格名称（展示用）.
    /// </summary>
    public string ProductSpecName { get; set; }

    /// <summary>
    /// 版本号.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// 是否默认模板（0=否，1=是）.
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    public long? SortCode { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    public DateTime? LastModifyTime { get; set; }
}