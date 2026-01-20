namespace Poxiao.Lab.Entity.Dto.AppearanceFeature;

/// <summary>
/// 人工修正列表输出
/// </summary>
public class CorrectionListOutput : AppearanceFeatureCorrectionEntity
{
    /// <summary>
    /// 人工修正后的特征名称
    /// </summary>
    public string CorrectedFeatureName { get; set; }

    /// <summary>
    /// 自动匹配的特征名称
    /// </summary>
    public string AutoMatchedFeatureName { get; set; }

    /// <summary>
    /// 处理状态 (待处理/已确认) - 目前仅显示待处理
    /// </summary>
    public string Status { get; set; } = "待处理";

    /// <summary>
    /// 匹配模式显示文本
    /// </summary>
    public string MatchModeText { get; set; }
}
