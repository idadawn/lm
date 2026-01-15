using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 订单收款
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[SugarTable("EXT_ORDERRECEIVABLE")]
public class OrderReceivableEntity : OEntityBase<string>
{
    /// <summary>
    /// 订单主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORDERID")]
    public string? OrderId { get; set; }

    /// <summary>
    /// 收款摘要.
    /// </summary>
    [SugarColumn(ColumnName = "F_ABSTRACT")]
    public string? Abstract { get; set; }

    /// <summary>
    /// 收款日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVABLEDATE")]
    public DateTime? ReceivableDate { get; set; }

    /// <summary>
    /// 收款比率.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVABLERATE")]
    public decimal? ReceivableRate { get; set; }

    /// <summary>
    /// 收款金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVABLEMONEY")]
    public decimal? ReceivableMoney { get; set; }

    /// <summary>
    /// 收款方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVABLEMODE")]
    public string? ReceivableMode { get; set; }

    /// <summary>
    /// 收款状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVABLESTATE")]
    public int? ReceivableState { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
