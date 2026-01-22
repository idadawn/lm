using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 原始数据导入会话实体.
/// </summary>
[SugarTable("lab_raw_data_import_session")]
[Tenant(ClaimConst.TENANTID)]
public class RawDataImportSessionEntity : CLDEntityBase
{
    /// <summary>
    /// 文件名.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILE_NAME", Length = 200, IsNullable = false)]
    public string FileName { get; set; }

    /// <summary>
    /// Excel源文件ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_FILE_ID", Length = 500, IsNullable = true)]
    public string SourceFileId { get; set; }

    /// <summary>
    /// Excel源文件哈希（用于重复上传识别）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_FILE_HASH", Length = 128, IsNullable = true)]
    public string SourceFileHash { get; set; }

    /// <summary>
    /// Excel源文件MD5（用于重复上传识别）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_FILE_MD5", Length = 32, IsNullable = true)]
    public string SourceFileMd5 { get; set; }

    /// <summary>
    /// 解析后的数据JSON文件路径（临时存储，完成导入后才写入数据库）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARSED_DATA_FILE", Length = 500, IsNullable = true)]
    public string ParsedDataFile { get; set; }

    /// <summary>
    /// 导入策略：已废弃，固定为"incremental"以保持向后兼容性.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMPORT_STRATEGY", Length = 20, IsNullable = true)]
    [Obsolete("导入策略功能已移除，此字段仅用于向后兼容")]
    public string ImportStrategy { get; set; } = "incremental";

    /// <summary>
    /// 当前步骤（0-第一步，1-第二步，2-第三步，3-第四步）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CURRENT_STEP", IsNullable = true)]
    public int CurrentStep { get; set; } = 0;

    /// <summary>
    /// 总行数.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOTAL_ROWS", IsNullable = true)]
    public int? TotalRows { get; set; } = 0;

    /// <summary>
    /// 有效数据行数.
    /// </summary>
    [SugarColumn(ColumnName = "F_VALID_DATA_ROWS", IsNullable = true)]
    public int? ValidDataRows { get; set; } = 0;

    /// <summary>
    /// 状态：pending/in_progress/completed/failed/cancelled.
    /// </summary>
    [SugarColumn(ColumnName = "F_STATUS", Length = 20, IsNullable = true)]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// 重写创建时间字段映射，使用 F_CREATOR_TIME（带下划线）.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATOR_TIME", ColumnDescription = "创建时间", IsNullable = true)]
    public override DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 重写创建用户字段映射，使用 F_CREATOR_USER_ID（带下划线）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_CREATOR_USER_ID",
        ColumnDescription = "创建用户",
        IsNullable = true
    )]
    public override string CreatorUserId { get; set; }

    /// <summary>
    /// 重写修改时间字段映射，使用 F_LAST_MODIFY_TIME（带下划线）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_LAST_MODIFY_TIME",
        ColumnDescription = "修改时间",
        IsNullable = true
    )]
    public override DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 重写修改用户字段映射，使用 F_LAST_MODIFY_USER_ID（带下划线）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_LAST_MODIFY_USER_ID",
        ColumnDescription = "修改用户",
        IsNullable = true
    )]
    public override string LastModifyUserId { get; set; }
}
