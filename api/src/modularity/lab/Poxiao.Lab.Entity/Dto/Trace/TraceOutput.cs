namespace Poxiao.Lab.Entity.Dto.Trace;

/// <summary>
/// 炉号追溯聚合查询输出（扫码枪场景：一次返回该炉号在四张业务表中的全链路数据）.
/// </summary>
public class TraceOutput
{
    /// <summary>
    /// 是否命中（原始数据/中间数据/磁性数据/单片数据任一命中即为true）.
    /// </summary>
    public bool Matched { get; set; }

    /// <summary>
    /// 扫码枪原始输入内容（扫码内容，可能带尾部K或特性汉字）.
    /// </summary>
    public string InputCode { get; set; }

    /// <summary>
    /// 归一化后的基准炉号（用于四表匹配，去除K/特性描述等噪声后的结果）.
    /// </summary>
    public string NormalizedFurnaceNo { get; set; }

    /// <summary>
    /// 批次级炉号（[产线][班次][日期]-[炉次]，不含卷号/分卷号；解析失败时为null）.
    /// 磁性/单片数据的炉号是批次级的，扫完整炉号时靠它回退匹配.
    /// </summary>
    public string NormalizedBatchNo { get; set; }

    /// <summary>
    /// 原始数据（LAB_RAW_DATA，按创建时间倒序取最新一条，未命中为null）.
    /// </summary>
    public RawDataEntity RawData { get; set; }

    /// <summary>
    /// 中间数据（LAB_INTERMEDIATE_DATA，按创建时间倒序取最新一条，未命中为null）.
    /// </summary>
    public IntermediateDataEntity Intermediate { get; set; }

    /// <summary>
    /// 磁性性能原始数据记录列表（LAB_MAGNETIC_RAW_DATA，未刻痕优先、按检测时间倒序）.
    /// </summary>
    public List<MagneticRawDataEntity> MagneticRecords { get; set; } = new();

    /// <summary>
    /// 单片性能原始数据记录列表（LAB_SINGLE_SHEET_RAW_DATA，未刻痕优先、按检测时间倒序）.
    /// </summary>
    public List<SingleSheetRawDataEntity> SingleSheetRecords { get; set; } = new();
}
