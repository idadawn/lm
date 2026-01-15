using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys.Entity;

/// <summary>
/// 客户信息.
/// </summary>
[SugarTable("ext_customer", TableDescription = "客户信息")]
public class ProductCustomerEntity : CLEntityBase
{
    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_Code")]
    public string Code { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_CustomerName")]
    public string Customername { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 联系方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_ContactTel")]
    public string ContactTel { get; set; }

    /// <summary>
    /// 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark")]
    public float Deletemark { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime Deletetime { get; set; }

    /// <summary>
    /// 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string Deleteuserid { get; set; }
}