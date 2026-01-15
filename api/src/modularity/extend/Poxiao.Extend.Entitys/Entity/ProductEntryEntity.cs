using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 产品明细.
/// </summary>
[SugarTable("ext_productentry")]
[Tenant(ClaimConst.TENANTID)]
public class ProductEntryEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 订单主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_ProductId")]
    public string ProductId { get; set; }

    /// <summary>
    /// 产品编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_ProductCode")]
    public string ProductCode { get; set; }

    /// <summary>
    /// 产品名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_ProductName")]
    public string ProductName { get; set; }

    /// <summary>
    /// 产品规格.
    /// </summary>
    [SugarColumn(ColumnName = "F_ProductSpecification")]
    public string ProductSpecification { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Qty")]
    public int Qty { get; set; }

    /// <summary>
    /// 控制方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_CommandType")]
    public string CommandType { get; set; }

    /// <summary>
    /// 订货类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Money")]
    public decimal Money { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_Util")]
    public string Util { get; set; }

    /// <summary>
    /// 折后单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 活动.
    /// </summary>
    [SugarColumn(ColumnName = "F_Activity")]
    public string Activity { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME", ColumnDescription = "创建时间")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 获取或设置 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORUSERID", ColumnDescription = "创建用户")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 获取或设置 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 获取或设置 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改用户")]
    public string LastModifyUserId { get; set; }

    /// <summary>
    /// 获取或设置 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标志")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 获取或设置 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 获取或设置 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除用户")]
    public string DeleteUserId { get; set; }
}