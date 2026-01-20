using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Dto.MagneticData;

/// <summary>
/// 磁性数据导入会话创建输入
/// </summary>
public class MagneticDataImportSessionInput
{
    public string FileName { get; set; }

    /// <summary>
    /// 文件数据（Base64）- 可选，如果提供则在创建会话时保存文件
    /// </summary>
    public string FileData { get; set; }
}

/// <summary>
/// 磁性数据导入项
/// </summary>
public class MagneticDataImportItem
{
    /// <summary>
    /// 原始炉号（B列，包含K标识）
    /// </summary>
    public string OriginalFurnaceNo { get; set; }

    /// <summary>
    /// 炉号（去掉K后的炉号）
    /// </summary>
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 是否刻痕（是否带K）
    /// </summary>
    public bool IsScratched { get; set; }

    /// <summary>
    /// Ps铁损（H列）
    /// </summary>
    public decimal? PsLoss { get; set; }

    /// <summary>
    /// Ss激磁功率（I列）
    /// </summary>
    public decimal? SsPower { get; set; }

    /// <summary>
    /// Hc（F列）
    /// </summary>
    public decimal? Hc { get; set; }

    /// <summary>
    /// 检测时间（P列）
    /// </summary>
    public DateTime? DetectionTime { get; set; }

    /// <summary>
    /// Excel行号（用于错误提示）
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// 是否有效数据
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
}

/// <summary>
/// 第一步：上传并解析输出
/// </summary>
public class MagneticDataImportStep1Output
{
    public string ImportSessionId { get; set; }
    public List<MagneticDataImportItem> ParsedData { get; set; }
    public int TotalRows { get; set; }
    public int ValidDataRows { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 导入核对输出
/// </summary>
public class MagneticDataReviewOutput
{
    public MagneticDataImportSessionEntity Session { get; set; }
    public int TotalRows { get; set; }
    public int ValidDataRows { get; set; }
    public int UpdatedRows { get; set; }
    public int SkippedRows { get; set; }
    public List<string> Errors { get; set; } = new();
}
