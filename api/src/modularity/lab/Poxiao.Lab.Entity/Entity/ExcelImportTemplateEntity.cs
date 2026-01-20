using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

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
    /// 所有者用户ID（个人模板时有效）.
    /// </summary>
    [SugarColumn(ColumnName = "F_OWNER_USER_ID", Length = 50, IsNullable = true)]
    public string OwnerUserId { get; set; }

    /// <summary>
    /// 模板配置（JSON格式）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_CONFIG_JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true
    )]
    public string ConfigJson { get; set; }
}
