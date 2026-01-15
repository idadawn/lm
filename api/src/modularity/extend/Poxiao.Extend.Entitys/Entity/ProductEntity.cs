using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 销售订单.
/// </summary>
[SugarTable("ext_product")]
[Tenant(ClaimConst.TENANTID)]
public class ProductEntity : OEntityBase<string>
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_Code")]
    public string Code { get; set; }

    /// <summary>
    /// 客户类别.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_CustomerName")]
    public string CustomerName { get; set; }

    /// <summary>
    /// 制单人id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalesmanId")]
    public string SalesmanId { get; set; }

    /// <summary>
    /// 制单人名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalesmanName")]
    public string SalesmanName { get; set; }

    /// <summary>
    /// 制单日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalesmanDate")]
    public DateTime? SalesmanDate { get; set; }

    /// <summary>
    /// 审核人.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditName")]
    public string AuditName { get; set; }

    /// <summary>
    /// 审核日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditDate")]
    public DateTime? AuditDate { get; set; }

    /// <summary>
    /// 审核状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditState")]
    public int AuditState { get; set; }

    /// <summary>
    /// 发货仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_GoodsWarehouse")]
    public string GoodsWarehouse { get; set; }

    /// <summary>
    /// 发货通知时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_GoodsDate")]
    public DateTime? GoodsDate { get; set; }

    /// <summary>
    /// 发货通知人.
    /// </summary>
    [SugarColumn(ColumnName = "F_Consignor")]
    public string Consignor { get; set; }

    /// <summary>
    /// 发货状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_GoodsState")]
    public int GoodsState { get; set; }

    /// <summary>
    /// 关闭状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_CloseState")]
    public int CloseState { get; set; }

    /// <summary>
    /// 关闭日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CloseDate")]
    public DateTime? CloseDate { get; set; }

    /// <summary>
    /// 收款方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_GatheringType")]
    public string GatheringType { get; set; }

    /// <summary>
    /// 业务员.
    /// </summary>
    [SugarColumn(ColumnName = "F_Business")]
    public string Business { get; set; }

    /// <summary>
    /// 送货地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }

    /// <summary>
    /// 联系方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_ContactTel")]
    public string ContactTel { get; set; }

    /// <summary>
    /// 联系人.
    /// </summary>
    [SugarColumn(ColumnName = "F_ContactName")]
    public string ContactName { get; set; }

    /// <summary>
    /// 收货消息.
    /// </summary>
    [SugarColumn(ColumnName = "F_HarvestMsg")]
    public int HarvestMsg { get; set; }

    /// <summary>
    /// 收货仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_HarvestWarehouse")]
    public string HarvestWarehouse { get; set; }

    /// <summary>
    /// 代发客户.
    /// </summary>
    [SugarColumn(ColumnName = "F_IssuingName")]
    public string IssuingName { get; set; }

    /// <summary>
    /// 让利金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_PartPrice")]
    public decimal? PartPrice { get; set; }

    /// <summary>
    /// 优惠金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReducedPrice")]
    public decimal? ReducedPrice { get; set; }

    /// <summary>
    /// 折后金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_DiscountPrice")]
    public decimal? DiscountPrice { get; set; }

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
    public virtual DateTime? LastModifyTime { get; set; }

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

    /// <summary>
    /// 订单明细.
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ProductEntryEntity.ProductId), nameof(Id))]
    public List<ProductEntryEntity> productEntryList { get; set; }
}