using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Enum;
using System.ComponentModel.DataAnnotations;

namespace Poxiao.Lab.Entity.Dto.IntermediateDataJudgmentLevel;

/// <summary>
/// 中间数据判定等级输出 DTO.
/// </summary>
public class IntermediateDataJudgmentLevelDto
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 判定公式ID.
    /// </summary>
    public string FormulaId { get; set; }

    /// <summary>
    /// 判定公式名称.
    /// </summary>
    public string FormulaName { get; set; }

    /// <summary>
    /// 等级代码.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 等级名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 质量状态.
    /// </summary>
    public QualityStatusEnum QualityStatus { get; set; }

    /// <summary>
    /// 判定权重.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 展示颜色.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// 是否统计.
    /// </summary>
    public bool IsStatistic { get; set; }

    /// <summary>
    /// 是否默认.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 业务说明.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 判定条件 (JSON格式).
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产品规格名称.
    /// </summary>
    public string ProductSpecName { get; set; }
}

/// <summary>
/// 中间数据判定等级列表输入 DTO.
/// </summary>
public class IntermediateDataJudgmentLevelListInput
{
    /// <summary>
    /// 判定公式ID.
    /// </summary>
    [Required(ErrorMessage = "判定公式ID不能为空")]
    public string FormulaId { get; set; }

    /// <summary>
    /// 产品规格ID (可选筛选).
    /// </summary>
    public string ProductSpecId { get; set; }
}

/// <summary>
/// 中间数据判定等级添加输入 DTO.
/// </summary>
public class IntermediateDataJudgmentLevelAddInput
{
    /// <summary>
    /// 判定公式ID.
    /// </summary>
    [Required(ErrorMessage = "判定公式ID不能为空")]
    public string FormulaId { get; set; }

    /// <summary>
    /// 等级名称.
    /// </summary>
    [Required(ErrorMessage = "等级名称不能为空")]
    public string Name { get; set; }

    /// <summary>
    /// 质量状态.
    /// </summary>
    public QualityStatusEnum QualityStatus { get; set; }

    /// <summary>
    /// 判定权重.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 展示颜色.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// 是否统计.
    /// </summary>
    public bool IsStatistic { get; set; } = true;

    /// <summary>
    /// 是否默认.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 业务说明.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 判定条件 (JSON格式).
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }
}

/// <summary>
/// 中间数据判定等级修改输入 DTO.
/// </summary>
public class IntermediateDataJudgmentLevelUpdateInput : IntermediateDataJudgmentLevelAddInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public string Id { get; set; }
}

/// <summary>
/// 批量复制等级输入 DTO.
/// </summary>
public class BatchCopyLevelsInput
{
    /// <summary>
    /// 源判定项目ID.
    /// </summary>
    [Required(ErrorMessage = "源判定项目ID不能为空")]
    public string SourceFormulaId { get; set; }

    /// <summary>
    /// 目标判定项目ID列表.
    /// </summary>
    [Required(ErrorMessage = "目标判定项目不能为空")]
    public List<string> TargetFormulaIds { get; set; }

    /// <summary>
    /// 是否覆盖已存在的同名等级.
    /// </summary>
    public bool OverwriteExisting { get; set; }

    /// <summary>
    /// 源产品规格ID (可选).
    /// </summary>
    public string SourceProductSpecId { get; set; }

    /// <summary>
    /// 目标产品规格ID列表 (可选).
    /// </summary>
    public List<string> TargetProductSpecIds { get; set; }
}
