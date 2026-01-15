namespace Poxiao.Lab.Entity.Dto.Unit;

/// <summary>
/// 单位换算请求 DTO.
/// </summary>
public class UnitConversionRequestDto
{
    /// <summary>
    /// 原始数值.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 源单位 ID.
    /// </summary>
    public string FromUnitId { get; set; }

    /// <summary>
    /// 目标单位 ID.
    /// </summary>
    public string ToUnitId { get; set; }
}

/// <summary>
/// 单位换算响应 DTO.
/// </summary>
public class UnitConversionResponseDto
{
    /// <summary>
    /// 原始数值.
    /// </summary>
    public decimal OriginalValue { get; set; }

    /// <summary>
    /// 源单位 ID.
    /// </summary>
    public string FromUnitId { get; set; }

    /// <summary>
    /// 目标单位 ID.
    /// </summary>
    public string ToUnitId { get; set; }

    /// <summary>
    /// 换算后的数值.
    /// </summary>
    public decimal ConvertedValue { get; set; }
}
