using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Dto.RawData;

/// <summary>
/// 原始数据导入输入.
/// </summary>
public class RawDataImportInput
{
    /// <summary>
    /// Excel文件流（Base64编码）.
    /// </summary>
    public string FileData { get; set; }

    /// <summary>
    /// 文件名.
    /// </summary>
    public string FileName { get; set; }
}

/// <summary>
/// 原始数据导入输出.
/// </summary>
public class RawDataImportOutput
{
    /// <summary>
    /// 成功数量.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量.
    /// </summary>
    public int FailCount { get; set; }

    /// <summary>
    /// 错误报告（Base64编码的Excel文件）.
    /// </summary>
    public string ErrorReport { get; set; }

    /// <summary>
    /// 错误报告文件名.
    /// </summary>
    public string ErrorReportFileName { get; set; }

    /// <summary>
    /// 错误详情列表.
    /// </summary>
    public List<RawDataImportErrorDetail> ErrorDetails { get; set; } = new();
}

/// <summary>
/// 原始数据导入错误详情.
/// </summary>
public class RawDataImportErrorDetail
{
    /// <summary>
    /// 行号.
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 原始数据.
    /// </summary>
    public Dictionary<string, object> RawData { get; set; } = new();
}

/// <summary>
/// 原始数据列表查询.
/// </summary>
public class RawDataListQuery : PageInputBase
{
    /// <summary>
    /// 关键词（炉号、产线等）.
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 开始日期.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 产品规格ID.
    /// </summary>
    public string ProductSpecId { get; set; }

    /// <summary>
    /// 产线.
    /// </summary>
    public string LineNo { get; set; }

    /// <summary>
    /// 排序规则（JSON格式，如：[{"field":"ProdDate","order":"asc"},{"field":"FurnaceNoParsed","order":"asc"}]）.
    /// </summary>
    public string SortRules { get; set; }

    /// <summary>
    /// 有效数据标识（1-有效数据，0-无效数据，null-全部）.
    /// </summary>
    public int? IsValidData { get; set; }

    /// <summary>
    /// 录入开始日期.
    /// </summary>
    public DateTime? CreatorTimeStart { get; set; }

    /// <summary>
    /// 录入结束日期.
    /// </summary>
    public DateTime? CreatorTimeEnd { get; set; }
}

/// <summary>
/// 排序规则.
/// </summary>
public class SortRule
{
    /// <summary>
    /// 字段名.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// 排序方式（asc/desc）.
    /// </summary>
    public string Order { get; set; }
}

/// <summary>
/// 原始数据列表输出.
/// </summary>
public class RawDataListOutput : RawDataEntity
{
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreatorUserName { get; set; }

    /// <summary>
    /// 检测日期字符串格式（用于前端展示，格式：2025/11/1）
    /// </summary>
    public string ProdDateStr { get; set; } = string.Empty;
}

/// <summary>
/// 原始数据详情输出.
/// </summary>
public class RawDataInfoOutput : RawDataEntity { }

/// <summary>
/// 原始数据预览输出.
/// </summary>
public class RawDataPreviewOutput
{
    /// <summary>
    /// 原始数据列表.
    /// </summary>
    public List<Dictionary<string, object>> OriginalData { get; set; } = new();

    /// <summary>
    /// 解析后的数据列表.
    /// </summary>
    public List<RawDataPreviewItem> ParsedData { get; set; } = new();

    /// <summary>
    /// Excel原始表头顺序.
    /// </summary>
    public List<string> HeaderOrder { get; set; } = new();

    /// <summary>
    /// 上次导入位置（跳过的行数）.
    /// </summary>
    public int SkippedRows { get; set; }
}

/// <summary>
/// 原始数据预览项.
/// </summary>
public class RawDataPreviewItem : RawDataEntity
{
    /// <summary>
    /// 状态 (success/failed).
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    public string ErrorMessage { get; set; }
}

/// <summary>
/// 导入日志列表输出.
/// </summary>
public class RawDataImportLogListOutput : RawDataImportLogEntity
{
    /// <summary>
    /// 操作人姓名
    /// </summary>
    public string OperatorName { get; set; }
}
