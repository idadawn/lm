using System.Collections.Generic;

namespace Poxiao.Lab.Entity.Dto.IntermediateData
{
    /// <summary>
    /// 中间数据颜色配置DTO
    /// </summary>
    public class IntermediateDataColorDto
    {
        /// <summary>
        /// 单元格颜色配置列表
        /// </summary>
        public List<CellColorInfo> Colors { get; set; }

        /// <summary>
        /// 产品规格ID
        /// </summary>
        public string ProductSpecId { get; set; }
    }

    /// <summary>
    /// 单元格颜色信息
    /// </summary>
    public class CellColorInfo
    {
        /// <summary>
        /// 中间数据ID
        /// </summary>
        public string IntermediateDataId { get; set; }

        /// <summary>
        /// 字段名（列名）
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 颜色值（HEX格式）
        /// </summary>
        public string ColorValue { get; set; }
    }

    /// <summary>
    /// 保存颜色配置请求
    /// </summary>
    public class SaveIntermediateDataColorInput
    {
        /// <summary>
        /// 要保存的颜色配置
        /// </summary>
        public List<CellColorInfo> Colors { get; set; }

        /// <summary>
        /// 产品规格ID
        /// </summary>
        public string ProductSpecId { get; set; }
    }

    /// <summary>
    /// 获取颜色配置请求
    /// </summary>
    public class GetIntermediateDataColorInput
    {
        /// <summary>
        /// 中间数据ID列表（可选，不填则获取所有）
        /// </summary>
        public List<string> IntermediateDataIds { get; set; }

        /// <summary>
        /// 产品规格ID
        /// </summary>
        public string ProductSpecId { get; set; }
    }

    /// <summary>
    /// 删除颜色配置请求
    /// </summary>
    public class DeleteIntermediateDataColorInput
    {
        /// <summary>
        /// 要删除的颜色配置ID列表
        /// </summary>
        public List<string> Ids { get; set; }

        /// <summary>
        /// 中间数据ID（可选，删除指定数据的所有颜色）
        /// </summary>
        public string IntermediateDataId { get; set; }

        /// <summary>
        /// 产品规格ID（可选，删除指定规格的所有颜色）
        /// </summary>
        public string ProductSpecId { get; set; }
    }

    /// <summary>
    /// 保存单个单元格颜色请求
    /// </summary>
    public class SaveCellColorInput
    {
        /// <summary>
        /// 中间数据ID
        /// </summary>
        public string IntermediateDataId { get; set; }

        /// <summary>
        /// 字段名（列名）
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 颜色值（HEX格式）
        /// </summary>
        public string ColorValue { get; set; }

        /// <summary>
        /// 产品规格ID
        /// </summary>
        public string ProductSpecId { get; set; }
    }
}
