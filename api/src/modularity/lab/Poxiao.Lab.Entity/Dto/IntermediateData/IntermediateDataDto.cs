using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.Lab.Entity.Dto.IntermediateData;

/// <summary>
/// 中间数据列表查询.
/// </summary>
public class IntermediateDataListQuery : PageInputBase
{
    /// <summary>
    /// 关键词（炉号、产线等）.
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 特性后缀（用于筛选）.
    /// </summary>
    public string FeatureSuffix { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 日期月份（yyyy-MM）.
    /// </summary>
    public string DateMonth { get; set; }

    /// <summary>
    /// 开始日期（生产日期）.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期（生产日期）.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 检测开始日期.
    /// </summary>
    public DateTime? DetectionStartDate { get; set; }

    /// <summary>
    /// 检测结束日期.
    /// </summary>
    public DateTime? DetectionEndDate { get; set; }

    /// <summary>
    /// 产线.
    /// </summary>
    public string LineNo { get; set; }

    /// <summary>
    /// 排序规则（JSON格式）.
    /// </summary>
    public string SortRules { get; set; }

    /// <summary>
    /// 计算状态 (0=Pending, 1=Processing, 2=Success, 3=Failed).
    /// </summary>
    public int? CalcStatus { get; set; }

    /// <summary>
    /// 判定状态 (0=Pending, 1=Processing, 2=Success, 3=Failed).
    /// </summary>
    public int? JudgeStatus { get; set; }
}

/// <summary>
/// 中间数据列表输出.
/// </summary>
public class IntermediateDataListOutput : IntermediateDataEntity
{
    /// <summary>
    /// 创建者姓名.
    /// </summary>
    public string CreatorUserName { get; set; }

    /// <summary>
    /// 生产日期字符串格式（用于前端展示）.
    /// </summary>
    public string ProdDateStr { get; set; } = string.Empty;

    /// <summary>
    /// 检测日期字符串格式（用于前端展示）.
    /// </summary>
    public string DetectionDateStr { get; set; } = string.Empty;

    /// <summary>
    /// 带厚分布列表（用于子表展示）.
    /// </summary>
    public List<ThicknessDistItem> ThicknessDistList { get; set; } = new();

    /// <summary>
    /// 叠片系数分布列表（用于子表展示）.
    /// </summary>
    public List<LaminationDistItem> LaminationDistList { get; set; } = new();
}

/// <summary>
/// 带厚分布项.
/// </summary>
public class ThicknessDistItem
{
    /// <summary>
    /// 序号.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 带厚值.
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// 是否异常（需要红色标注）.
    /// </summary>
    public bool IsAbnormal { get; set; }
}

/// <summary>
/// 叠片系数分布项.
/// </summary>
public class LaminationDistItem
{
    /// <summary>
    /// 序号.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 叠片系数值.
    /// </summary>
    public decimal? Value { get; set; }
}

/// <summary>
/// 中间数据详情输出.
/// </summary>
public class IntermediateDataInfoOutput : IntermediateDataEntity
{
    /// <summary>
    /// 创建者姓名.
    /// </summary>
    public string CreatorUserName { get; set; }

    /// <summary>
    /// 带厚分布列表.
    /// </summary>
    public List<ThicknessDistItem> ThicknessDistList { get; set; } = new();

    /// <summary>
    /// 叠片系数分布列表.
    /// </summary>
    public List<LaminationDistItem> LaminationDistList { get; set; } = new();
}

/// <summary>
/// 性能数据更新输入.
/// </summary>
public class IntermediateDataPerfUpdateInput
{
    /// <summary>
    /// 数据ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 1.35T 50Hz Ss激磁功率.
    /// </summary>
    public decimal? PerfSsPower { get; set; }

    /// <summary>
    /// 1.35T 50Hz Ps铁损.
    /// </summary>
    public decimal? PerfPsLoss { get; set; }

    /// <summary>
    /// 1.35T 50Hz Hc.
    /// </summary>
    public decimal? PerfHc { get; set; }

    /// <summary>
    /// 刻痕后性能 Ss激磁功率.
    /// </summary>
    public decimal? PerfAfterSsPower { get; set; }

    /// <summary>
    /// 刻痕后性能 Ps铁损.
    /// </summary>
    public decimal? PerfAfterPsLoss { get; set; }

    /// <summary>
    /// 刻痕后性能 Hc.
    /// </summary>
    public decimal? PerfAfterHc { get; set; }

    /// <summary>
    /// 性能判定人.
    /// </summary>
    public string PerfJudgeName { get; set; }
}

/// <summary>
/// 外观特性更新输入.
/// </summary>
public class IntermediateDataAppearUpdateInput
{
    /// <summary>
    /// 数据ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 韧性.
    /// </summary>
    public string Toughness { get; set; }

    /// <summary>
    /// 鱼鳞纹.
    /// </summary>
    public string FishScale { get; set; }

    /// <summary>
    /// 中Si.
    /// </summary>
    public string MidSi { get; set; }

    /// <summary>
    /// 中B.
    /// </summary>
    public string MidB { get; set; }

    /// <summary>
    /// 左花纹.
    /// </summary>
    public string LeftPattern { get; set; }

    /// <summary>
    /// 中花纹.
    /// </summary>
    public string MidPattern { get; set; }

    /// <summary>
    /// 右花纹.
    /// </summary>
    public string RightPattern { get; set; }

    /// <summary>
    /// 断头数.
    /// </summary>
    public int? BreakCount { get; set; }

    /// <summary>
    /// 单卷重量.
    /// </summary>
    public decimal? CoilWeightKg { get; set; }

    /// <summary>
    /// 外观检验员.
    /// </summary>
    public string AppearJudgeName { get; set; }
}

/// <summary>
/// 基础信息更新输入.
/// </summary>
public class IntermediateDataBaseUpdateInput
{
    /// <summary>
    /// 数据ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 日期月份（yyyy-MM）.
    /// </summary>
    public string DateMonth { get; set; }

    /// <summary>
    /// 磁性能判定.
    /// </summary>
    public string MagneticResult { get; set; }

    /// <summary>
    /// 厚度判定.
    /// </summary>
    public string ThicknessResult { get; set; }

    /// <summary>
    /// 叠片系数判定.
    /// </summary>
    public string LaminationResult { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string Remark { get; set; }
}

/// <summary>
/// 批次计算状态.
/// </summary>
public class BatchCalculationStatus
{
    /// <summary>
    /// 批次ID.
    /// </summary>
    public string BatchId { get; set; }

    /// <summary>
    /// 总数量.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 待计算数量.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 计算中数量.
    /// </summary>
    public int ProcessingCount { get; set; }

    /// <summary>
    /// 成功数量.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 是否全部完成.
    /// </summary>
    public bool IsCompleted => PendingCount == 0 && ProcessingCount == 0;

    /// <summary>
    /// 完成进度百分比.
    /// </summary>
    public double ProgressPercent =>
        TotalCount == 0 ? 100 : (double)(SuccessCount + FailedCount) / TotalCount * 100;
}
