using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 知识文档
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("EXT_DOCUMENT")]
public class DocumentEntity : CLDEntityBase
{
    /// <summary>
    /// 文档父级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 文档分类:【0-文件夹、1-文件】.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public int? Type { get; set; }

    /// <summary>
    /// 文件名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 文件路径.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILEPATH")]
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILESIZE")]
    public string? FileSize { get; set; }

    /// <summary>
    /// 文件后缀.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILEEXTENSION")]
    public string? FileExtension { get; set; }

    /// <summary>
    /// 阅读数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_READCCOUNT")]
    public int? ReadcCount { get; set; }

    /// <summary>
    /// 是否共享.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISSHARE")]
    public int? IsShare { get; set; }

    /// <summary>
    /// 共享时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHARETIME")]
    public DateTime? ShareTime { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 下载地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_UPLOADERURL")]
    public string? UploaderUrl { get; set; }
}
