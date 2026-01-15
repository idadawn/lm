using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys
{
    /// <summary>
    /// 订单明细
    /// 版 本：V1.0.0
    /// 版 权：Poxiao
    /// 作 者：Poxiao
    /// 日 期：2018-07-23 .
    /// </summary>
    [SugarTable("WFORM_SALESORDERENTRY")]
    [Tenant(ClaimConst.TENANTID)]
    public class SalesOrderEntryEntity : OEntityBase<string>
    {
        /// <summary>
        /// 订单主键.
        /// </summary>
        [SugarColumn(ColumnName = "F_SALESORDERID")]
        public string SalesOrderId { get; set; }

        /// <summary>
        /// 商品名称.
        /// </summary>
        [SugarColumn(ColumnName = "F_GOODSNAME")]
        public string GoodsName { get; set; }

        /// <summary>
        /// 规格型号.
        /// </summary>
        [SugarColumn(ColumnName = "F_SPECIFICATIONS")]
        public string Specifications { get; set; }

        /// <summary>
        /// 单位.
        /// </summary>
        [SugarColumn(ColumnName = "F_UNIT")]
        public string Unit { get; set; }

        /// <summary>
        /// 数量.
        /// </summary>
        [SugarColumn(ColumnName = "F_QTY")]
        public string Qty { get; set; }

        /// <summary>
        /// 单价.
        /// </summary>
        [SugarColumn(ColumnName = "F_PRICE")]
        public decimal? Price { get; set; }

        /// <summary>
        /// 金额.
        /// </summary>
        [SugarColumn(ColumnName = "F_AMOUNT")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// 描述.
        /// </summary>
        [SugarColumn(ColumnName = "F_DESCRIPTION")]
        public string Description { get; set; }

        /// <summary>
        /// SortCode.
        /// </summary>
        [SugarColumn(ColumnName = "F_SORTCODE")]
        public long? SortCode { get; set; }
    }
}