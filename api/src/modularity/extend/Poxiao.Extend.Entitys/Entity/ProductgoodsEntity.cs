using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 产品商品.
/// </summary>
[SugarTable("ext_productgoods")]
[Tenant(ClaimConst.TENANTID)]
public class ProductGoodsEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 分类主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_CLASSIFYID")]
    public string ClassifyId { get; set; }

    /// <summary>
    /// 产品编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_CODE")]
    public string Code { get; set; }

    /// <summary>
    /// 产品名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 订货类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public string Type { get; set; }

    /// <summary>
    /// 产品规格.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRODUCTSPECIFICATION")]
    public string ProductSpecification { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_MONEY")]
    public string Money { get; set; }

    /// <summary>
    /// 库存数.
    /// </summary>
    [SugarColumn(ColumnName = "F_QTY")]
    public int Qty { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_AMOUNT")]
    public string Amount { get; set; }

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