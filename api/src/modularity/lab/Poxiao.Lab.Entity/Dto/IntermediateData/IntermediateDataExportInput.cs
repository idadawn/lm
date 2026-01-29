using Microsoft.AspNetCore.Mvc;

namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 中间数据导出请求参数.
/// </summary>
public class IntermediateDataExportInput
{
    /// <summary>
    /// 生产日期开始日期.
    /// </summary>
    [FromQuery(Name = "startDate")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 生产日期结束日期.
    /// </summary>
    [FromQuery(Name = "endDate")]
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 中间数据导出结果（按月+产品规格分组）.
/// </summary>
public class IntermediateDataExportResult
{
    /// <summary>
    /// 月份名称（如：11月）.
    /// </summary>
    public string MonthName { get; set; }

    /// <summary>
    /// 月份序号（1-12）.
    /// </summary>
    public int MonthOrder { get; set; }

    /// <summary>
    /// 产品规格代码.
    /// </summary>
    public string ProductSpecCode { get; set; }

    /// <summary>
    /// 产品规格名称.
    /// </summary>
    public string ProductSpecName { get; set; }

    /// <summary>
    /// Sheet名称（如：11月120）.
    /// </summary>
    public string SheetName => $"{MonthName}{ProductSpecCode}";

    /// <summary>
    /// 检测列数量.
    /// </summary>
    public int DetectionColumns { get; set; }

    /// <summary>
    /// 数据列表.
    /// </summary>
    public List<IntermediateDataExportItem> DataList { get; set; } = new();
}

/// <summary>
/// 中间数据导出项（单条记录）.
/// </summary>
public class IntermediateDataExportItem
{
    // 基础信息
    public string 检验日期 { get; set; }
    public string 喷次 { get; set; }
    public string 贴标 { get; set; }
    public string 炉号 { get; set; }

    // 性能数据
    public decimal? Ss激磁功率 { get; set; }
    public decimal? Ps铁损 { get; set; }
    public decimal? Hc { get; set; }
    public decimal? 刻痕后Ss激磁功率 { get; set; }
    public decimal? 刻痕后Ps铁损 { get; set; }
    public decimal? 刻痕后Hc { get; set; }
    public string 性能录入员 { get; set; }

    // 物理特性
    public decimal? 一米带重 { get; set; }
    public decimal? 带宽 { get; set; }

    // 带厚1-22（动态）
    public decimal? 带厚1 { get; set; }
    public decimal? 带厚2 { get; set; }
    public decimal? 带厚3 { get; set; }
    public decimal? 带厚4 { get; set; }
    public decimal? 带厚5 { get; set; }
    public decimal? 带厚6 { get; set; }
    public decimal? 带厚7 { get; set; }
    public decimal? 带厚8 { get; set; }
    public decimal? 带厚9 { get; set; }
    public decimal? 带厚10 { get; set; }
    public decimal? 带厚11 { get; set; }
    public decimal? 带厚12 { get; set; }
    public decimal? 带厚13 { get; set; }
    public decimal? 带厚14 { get; set; }
    public decimal? 带厚15 { get; set; }
    public decimal? 带厚16 { get; set; }
    public decimal? 带厚17 { get; set; }
    public decimal? 带厚18 { get; set; }
    public decimal? 带厚19 { get; set; }
    public decimal? 带厚20 { get; set; }
    public decimal? 带厚21 { get; set; }
    public decimal? 带厚22 { get; set; }

    // 带厚范围
    public decimal? 带厚最小值 { get; set; }
    public decimal? 带厚最大值 { get; set; }

    // 汇总特性
    public decimal? 带厚极差 { get; set; }
    public decimal? 密度 { get; set; }
    public decimal? 叠片系数 { get; set; }

    // 外观特性（动态特性大类）
    public Dictionary<string, object> 外观特性 { get; set; } = new();

    // 固定外观字段
    public int? 断头数 { get; set; }
    public decimal? 单卷重量 { get; set; }
    public string 外观检验员 { get; set; }

    // 判定结果
    public decimal? 平均厚度 { get; set; }
    public string 磁性能判定 { get; set; }
    public string 厚度判定 { get; set; }
    public string 叠片系数判定 { get; set; }

    // 花纹数据
    public string 中Si左 { get; set; }
    public string 中Si右 { get; set; }
    public string 中B左 { get; set; }
    public string 中B右 { get; set; }
    public decimal? 左花纹纹宽 { get; set; }
    public decimal? 左花纹纹距 { get; set; }
    public decimal? 中花纹纹宽 { get; set; }
    public decimal? 中花纹纹距 { get; set; }
    public decimal? 右花纹纹宽 { get; set; }
    public decimal? 右花纹纹距 { get; set; }

    // 其他
    public decimal? 四米带重 { get; set; }

    // 叠片系数厚度分布1-22（动态）
    public decimal? 叠片系数厚度分布1 { get; set; }
    public decimal? 叠片系数厚度分布2 { get; set; }
    public decimal? 叠片系数厚度分布3 { get; set; }
    public decimal? 叠片系数厚度分布4 { get; set; }
    public decimal? 叠片系数厚度分布5 { get; set; }
    public decimal? 叠片系数厚度分布6 { get; set; }
    public decimal? 叠片系数厚度分布7 { get; set; }
    public decimal? 叠片系数厚度分布8 { get; set; }
    public decimal? 叠片系数厚度分布9 { get; set; }
    public decimal? 叠片系数厚度分布10 { get; set; }
    public decimal? 叠片系数厚度分布11 { get; set; }
    public decimal? 叠片系数厚度分布12 { get; set; }
    public decimal? 叠片系数厚度分布13 { get; set; }
    public decimal? 叠片系数厚度分布14 { get; set; }
    public decimal? 叠片系数厚度分布15 { get; set; }
    public decimal? 叠片系数厚度分布16 { get; set; }
    public decimal? 叠片系数厚度分布17 { get; set; }
    public decimal? 叠片系数厚度分布18 { get; set; }
    public decimal? 叠片系数厚度分布19 { get; set; }
    public decimal? 叠片系数厚度分布20 { get; set; }
    public decimal? 叠片系数厚度分布21 { get; set; }
    public decimal? 叠片系数厚度分布22 { get; set; }

    // 扩展数据
    public decimal? 最大厚度 { get; set; }
    public decimal? 最大平均厚度 { get; set; }
    public string 录入人 { get; set; }
    public decimal? 带型 { get; set; }
    public string 一次交检 { get; set; }
    public string 班次 { get; set; }
    public string 计算状态 { get; set; }
}
