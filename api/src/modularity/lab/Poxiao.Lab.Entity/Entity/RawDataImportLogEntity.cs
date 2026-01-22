using Poxiao.DatabaseAccessor;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;
using SugarColumn = SqlSugar.SugarColumn;
using SugarTable = SqlSugar.SugarTable;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 原始数据导入日志实体
/// </summary>
[SugarTable("lab_raw_data_import_log")]
[Tenant(ClaimConst.TENANTID)]
public class RawDataImportLogEntity : CLDEntityBase
{
    /// <summary>
    /// 文件名
    /// </summary>
    [SugarColumn(ColumnDescription = "文件名")]
    public string FileName { get; set; }

    /// <summary>
    /// 总行数（Excel行数）
    /// </summary>
    [SugarColumn(ColumnDescription = "总行数")]
    public int TotalRows { get; set; }

    /// <summary>
    /// 成功行数
    /// </summary>
    [SugarColumn(ColumnDescription = "成功行数")]
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败行数
    /// </summary>
    [SugarColumn(ColumnDescription = "失败行数")]
    public int FailCount { get; set; }

    /// <summary>
    /// 导入状态 (success/partial/failed)
    /// </summary>
    [SugarColumn(ColumnDescription = "导入状态")]
    public string Status { get; set; }

    /// <summary>
    /// 导入时间
    /// </summary>
    [SugarColumn(ColumnDescription = "导入时间")]
    public DateTime ImportTime { get; set; }

    /// <summary>
    /// Excel源文件ID（关联文件服务）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_FILE_ID", Length = 500, IsNullable = true, ColumnDescription = "Excel源文件ID")]
    public string SourceFileId { get; set; }

    /// <summary>
    /// Excel源文件哈希（用于重复上传识别）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_SOURCE_FILE_HASH",
        Length = 128,
        IsNullable = true,
        ColumnDescription = "Excel源文件哈希"
    )]
    public string SourceFileHash { get; set; }

    /// <summary>
    /// Excel源文件MD5（用于重复上传识别）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_SOURCE_FILE_MD5",
        Length = 32,
        IsNullable = true,
        ColumnDescription = "Excel源文件MD5"
    )]
    public string SourceFileMd5 { get; set; }

    /// <summary>
    /// 有效数据行数（符合炉号解析规则的有效数据行数）.
    /// </summary>
    [SugarColumn(ColumnName = "F_VALID_DATA_COUNT", IsNullable = true, ColumnDescription = "有效数据行数")]
    public int ValidDataCount { get; set; } = 0;

    /// <summary>
    /// 最后N行数据标识（哈希值，用于增量导入校验）.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAST_ROWS_HASH", Length = 200, IsNullable = true, ColumnDescription = "最后N行数据标识")]
    public string LastRowsHash { get; set; }

    /// <summary>
    /// 记录的最后行数（默认3行）.
    /// </summary>
    [SugarColumn(ColumnName = "F_LAST_ROWS_COUNT", IsNullable = true, ColumnDescription = "记录的最后行数")]
    public int LastRowsCount { get; set; } = 3;

    /// <summary>
    /// 导入会话ID（关联导入会话）.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_SESSION_ID", Length = 50, IsNullable = true, ColumnDescription = "导入会话ID")]
    public string ImportSessionId { get; set; }
}
