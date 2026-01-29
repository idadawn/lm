using System.ComponentModel;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Lab.Entity.Attributes;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 磁性原始数据实体.
/// </summary>
[SugarTable("LAB_MAGNETIC_RAW_DATA")]
[Tenant(ClaimConst.TENANTID)]
public class MagneticRawDataEntity : CLDEntityBase
{
    /// <summary>
    /// 原始炉号（B列，包含K标识）.
    /// </summary>
    [ExcelImportColumn("炉号", Sort = 1)]
    [SugarColumn(ColumnName = "F_ORIGINAL_FURNACE_NO", Length = 100, IsNullable = true)]
    public string OriginalFurnaceNo { get; set; }

    /// <summary>
    /// 炉号（去掉K后的炉号）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO", Length = 100, IsNullable = true)]
    public string FurnaceNo { get; set; }

    /// <summary>
    /// 是否刻痕（0-否，1-是，是否带K）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_SCRATCHED", IsNullable = true)]
    public int IsScratched { get; set; }

    /// <summary>
    /// Ps铁损（H列）.
    /// </summary>
    [ExcelImportColumn("Ps铁损", Sort = 2)]
    [SugarColumn(ColumnName = "F_PS_LOSS", DecimalDigits = 6, IsNullable = true)]
    public decimal? PsLoss { get; set; }

    /// <summary>
    /// Ss激磁功率（I列）.
    /// </summary>
    [ExcelImportColumn("Ss激磁功率", Sort = 3)]
    [SugarColumn(ColumnName = "F_SS_POWER", DecimalDigits = 6, IsNullable = true)]
    public decimal? SsPower { get; set; }

    /// <summary>
    /// Hc（F列）.
    /// </summary>
    [ExcelImportColumn("Hc", Sort = 4)]
    [SugarColumn(ColumnName = "F_HC", DecimalDigits = 6, IsNullable = true)]
    public decimal? Hc { get; set; }

    /// <summary>
    /// 检测时间（P列）.
    /// </summary>
    [ExcelImportColumn("检测时间", Sort = 5)]
    [SugarColumn(ColumnName = "F_DETECTION_TIME", IsNullable = true)]
    public DateTime? DetectionTime { get; set; }

    /// <summary>
    /// 产线（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_LINE_NO", IsNullable = true)]
    public int? LineNo { get; set; }

    /// <summary>
    /// 班次（从炉号解析，存储原始汉字：甲、乙、丙）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT", Length = 10, IsNullable = true)]
    public string Shift { get; set; }

    /// <summary>
    /// 班次数字（用于排序：甲=1, 乙=2, 丙=3）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHIFT_NUMERIC", IsNullable = true)]
    public int? ShiftNumeric { get; set; }

    /// <summary>
    /// 炉次号（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_BATCH_NO", IsNullable = true)]
    public int? FurnaceBatchNo { get; set; }

    /// <summary>
    /// 卷号（从炉号解析，支持小数，磁性数据炉号格式不包含卷号，通常为NULL）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COIL_NO", IsNullable = true)]
    public decimal? CoilNo { get; set; }

    /// <summary>
    /// 分卷号（从炉号解析，支持小数，磁性数据炉号格式不包含分卷号，通常为NULL）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SUBCOIL_NO", IsNullable = true)]
    public decimal? SubcoilNo { get; set; }

    /// <summary>
    /// 生产日期（从炉号解析）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROD_DATE", IsNullable = true)]
    public DateTime? ProdDate { get; set; }

    /// <summary>
    /// 炉号数字部分（从炉号解析，与FurnaceBatchNo相同）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FURNACE_NO_PARSED", Length = 50, IsNullable = true)]
    public string FurnaceNoParsed { get; set; }

    /// <summary>
    /// 导入会话ID（关联导入会话）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_SESSION_ID", Length = 50, IsNullable = true)]
    public string ImportSessionId { get; set; }

    /// <summary>
    /// Excel行号（用于错误提示）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ROW_INDEX", IsNullable = true)]
    public int? RowIndex { get; set; }

    /// <summary>
    /// 是否有效数据（0-无效，1-有效）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_VALID", IsNullable = true)]
    public int IsValid { get; set; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    [SugarColumn(ColumnName = "F_ERROR_MESSAGE", Length = 500, IsNullable = true)]
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE", IsNullable = true)]
    public long? SortCode { get; set; }
}
