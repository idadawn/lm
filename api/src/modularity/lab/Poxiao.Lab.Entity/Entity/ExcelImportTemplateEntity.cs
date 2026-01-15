using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity;

/// <summary>
/// Excel导入模板表.
/// </summary>
[SugarTable("LAB_EXCEL_IMPORT_TEMPLATE")]
[Tenant(ClaimConst.TENANTID)]
public class ExcelImportTemplateEntity : CLDEntityBase
{
    /// <summary>
    /// 模板名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATE_NAME", Length = 100)]
    public string TemplateName { get; set; }

    /// <summary>
    /// 模板编码（唯一）.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATE_CODE", Length = 50)]
    public string TemplateCode { get; set; }

    /// <summary>
    /// 模板描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", Length = 500, IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 模板类型：system系统模板/user个人模板.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATE_TYPE", Length = 20)]
    public string TemplateType { get; set; } = "system";

    /// <summary>
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    [SugarColumn(ColumnName = "F_OWNER_USER_ID", Length = 50, IsNullable = true)]
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 模板配置（JSON格式）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONFIG_JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string ConfigJson { get; set; }

    /// <summary>
    /// 关联的产品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCT_SPEC_ID", Length = 50, IsNullable = true)]
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 版本号.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// 是否默认模板（0=否，1=是）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_DEFAULT")]
    public int IsDefault { get; set; } = 0;

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", IsNullable = true)]
    public long? SortCode { get; set; }
}