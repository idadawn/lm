using System;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity.Entity
{
    /// <summary>
    /// 中间数据单元格颜色配置表
    /// </summary>
    [SugarTable("LAB_INTERMEDIATE_DATA_COLOR")]
    public class IntermediateDataColorEntity : CLDEntityBase
    {
        /// <summary>
        /// 中间数据ID
        /// </summary>
        [SugarColumn(ColumnName = "IntermediateDataId", Length = 50, IsNullable = false)]
        public string IntermediateDataId { get; set; }

        /// <summary>
        /// 字段名（列名）
        /// </summary>
        [SugarColumn(ColumnName = "FieldName", Length = 100, IsNullable = false)]
        public string FieldName { get; set; }

        /// <summary>
        /// 颜色值（HEX格式，如#FF0000）
        /// </summary>
        [SugarColumn(ColumnName = "ColorValue", Length = 7, IsNullable = false)]
        public string ColorValue { get; set; }

        /// <summary>
        /// 产品规格ID（用于按规格管理颜色配置）
        /// </summary>
        [SugarColumn(ColumnName = "ProductSpecId", Length = 50, IsNullable = false)]
        public string ProductSpecId { get; set; }

        /// <summary>
        /// 更新用户ID（业务字段，用于记录最后更新人）
        /// </summary>
        [SugarColumn(ColumnName = "UpdateUserId", Length = 50, IsNullable = true)]
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新时间（业务字段，用于记录最后更新时间）
        /// </summary>
        [SugarColumn(ColumnName = "UpdateTime", IsNullable = true)]
        public DateTime? UpdateTime { get; set; }
    }
}
