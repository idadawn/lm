namespace Poxiao.Lab.Entity.Dto.RawData;

/// <summary>
/// 导入会话创建输入
/// </summary>
public class RawDataImportSessionInput
{
    public string FileName { get; set; }

    /// <summary>
    /// 导入策略：已废弃，固定为"incremental"以保持向后兼容性
    /// </summary>
    [Obsolete("导入策略功能已移除，此属性仅用于向后兼容")]
    public string ImportStrategy { get; set; } = "incremental";

    /// <summary>
    /// 文件数据（Base64）- 可选，如果提供则在创建会话时保存文件
    /// </summary>
    public string FileData { get; set; }
}

/// <summary>
/// 第一步：上传并解析输入
/// </summary>
public class RawDataImportStep1Input : RawDataImportSessionInput
{
    /// <summary>
    /// 导入会话ID（必填，使用已存在的会话）
    /// </summary>
    public string ImportSessionId { get; set; }
}

/// <summary>
/// 第一步：上传并解析输出
/// </summary>
public class RawDataImportStep1Output
{
    public string ImportSessionId { get; set; }
    public RawDataPreviewOutput PreviewData { get; set; }
    public int TotalRows { get; set; }
    public int ValidDataRows { get; set; }
}

/// <summary>
/// 产品规格匹配结果输出
/// </summary>
public class RawDataProductSpecMatchOutput
{
    public string RawDataId { get; set; }
    public string FurnaceNo { get; set; }
    public DateTime? ProdDate { get; set; }
    public decimal? Width { get; set; }
    public decimal? CoilWeight { get; set; }
    public int? BreakCount { get; set; } // 断头数
    public decimal? SingleCoilWeight { get; set; } // 单卷重量
    public int? DetectionColumns { get; set; } // 数据的有效检测列
    public string ProductSpecId { get; set; }
    public string ProductSpecName { get; set; }
    public string ProductSpecCode { get; set; }
    public string MatchStatus { get; set; } // matched, unmatched, manual
    public Dictionary<int, decimal?> DetectionValues { get; set; } // 检测数据值
}

/// <summary>
/// 更新产品规格输入
/// </summary>
public class RawDataUpdateProductSpecsInput
{
    public string SessionId { get; set; }
    public List<RawDataUpdateProductSpecItem> Items { get; set; }
}

public class RawDataUpdateProductSpecItem
{
    public string RawDataId { get; set; }
    public string ProductSpecId { get; set; }
}

/// <summary>
/// 特性匹配结果输出
/// </summary>
public class RawDataFeatureMatchOutput
{
    public string RawDataId { get; set; }
    public string FurnaceNo { get; set; }
    public string FeatureSuffix { get; set; } // 原始特性汉字
    public List<string> AppearanceFeatureIds { get; set; } // 匹配的特性ID列表
    public double? MatchConfidence { get; set; } // 匹配置信度
    public List<FeatureMatchDetail> MatchDetails { get; set; } // 匹配详情
}

public class FeatureMatchDetail
{
    public string FeatureId { get; set; }
    public string FeatureName { get; set; }
    public string Confidence { get; set; }
}

/// <summary>
/// 更新特性匹配输入
/// </summary>
public class RawDataUpdateFeaturesInput
{
    public string SessionId { get; set; }
    public List<RawDataUpdateFeatureItem> Items { get; set; }
}

public class RawDataUpdateFeatureItem
{
    public string RawDataId { get; set; }
    public List<string> AppearanceFeatureIds { get; set; }
}

/// <summary>
/// 更新重复数据选择输入
/// </summary>
public class RawDataUpdateDuplicateSelectionsInput
{
    public string SessionId { get; set; }
    public List<RawDataUpdateDuplicateSelectionItem> Items { get; set; }
}

public class RawDataUpdateDuplicateSelectionItem
{
    public string RawDataId { get; set; }
    public bool IsValidData { get; set; }
}

/// <summary>
/// 导入核对输出
/// </summary>
public class RawDataReviewOutput
{
    public RawDataImportSessionEntity Session { get; set; }
    public int TotalRows { get; set; }
    public int ValidDataRows { get; set; }
    public int MatchedSpecRows { get; set; }
    public int MatchedFeatureRows { get; set; }
    public string MatchStatus { get; set; }
    public List<string> Errors { get; set; }
    public List<RawDataEntity> PreviewIntermediateData { get; set; } // 预览即将生成的中间数据（部分字段）
}
