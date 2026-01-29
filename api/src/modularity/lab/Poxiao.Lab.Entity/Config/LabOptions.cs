namespace Poxiao.Lab.Entity.Config;

/// <summary>
/// 实验室模块配置选项
/// </summary>
public class LabOptions
{
    /// <summary>
    /// 配置节点名称
    /// </summary>
    public const string SectionName = "Lab";

    /// <summary>
    /// 公式计算配置
    /// </summary>
    public FormulaOptions Formula { get; set; } = new();
}

/// <summary>
/// 公式计算配置选项
/// </summary>
public class FormulaOptions
{
    /// <summary>
    /// 是否启用计算精度调整（在原始精度基础上+1位，最多6位）
    /// 默认为 false，即按照数据库精度保存，不额外增加精度
    /// </summary>
    public bool EnablePrecisionAdjustment { get; set; } = false;

    /// <summary>
    /// 默认计算精度（小数位数）
    /// 优先级最低，仅在以下情况都找不到精度时使用：
    /// 1. 公式本身的 Precision 属性
    /// 2. unitPrecisions 字典中的精度信息
    /// 3. 单位定义的 Precision 属性
    /// 设置为 0 或 null 表示不限制精度（保持原始计算精度）
    /// </summary>
    public int? DefaultPrecision { get; set; } = 6;

    /// <summary>
    /// 最大计算精度（小数位数）
    /// </summary>
    public int MaxPrecision { get; set; } = 6;
}
