namespace Poxiao.Lab.Entity.Dto.MagneticData;

/// <summary>
/// 磁性原始数据列表输出.
/// </summary>
public class MagneticRawDataListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 原始炉号.
    /// </summary>
    public string OriginalFurnaceNo { get; set; }

    /// <summary>
    /// 炉号（解析后）.
    /// </summary>
    public string FurnaceNo { get; set; }

    /// <summary>
    /// Ps铁损.
    /// </summary>
    public decimal? PsLoss { get; set; }

    /// <summary>
    /// Ss激磁功率.
    /// </summary>
    public decimal? SsPower { get; set; }

    /// <summary>
    /// Hc.
    /// </summary>
    public decimal? Hc { get; set; }

    /// <summary>
    /// 检测时间.
    /// </summary>
    public DateTime? DetectionTime { get; set; }

    /// <summary>
    /// 是否刻痕（1-是，0-否）.
    /// </summary>
    public int IsScratched { get; set; }

    /// <summary>
    /// 是否有效数据.
    /// </summary>
    public int IsValid { get; set; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    public long? SortCode { get; set; }
}
