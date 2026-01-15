using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys;

/// <summary>
/// 消息实例
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGE")]
public class MessageEntity : CLDEntityBase
{
    /// <summary>
    /// 类别：1-通知公告，2-系统消息、3-私信消息.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public int? Type { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 正文.
    /// </summary>
    [SugarColumn(ColumnName = "F_BODYTEXT")]
    public string BodyText { get; set; }

    /// <summary>
    /// 优先.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRIORITYLEVEL")]
    public int? PriorityLevel { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOUSERIDS")]
    public string ToUserIds { get; set; }

    /// <summary>
    /// 是否阅读.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISREAD")]
    public int? IsRead { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILES")]
    public string Files { get; set; }

    /// <summary>
    /// 流程跳转类型 1:审批 2:委托.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWTYPE")]
    public int? FlowType { get; set; }

    /// <summary>
    /// 封面图片.
    /// </summary>
    [SugarColumn(ColumnName = "F_COVERIMAGE")]
    public string CoverImage { get; set; }

    /// <summary>
    /// 过期时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXPIRATIONTIME")]
    public DateTime? ExpirationTime { get; set; }

    /// <summary>
    /// 分类 1-公告 2-通知.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY")]
    public string Category { get; set; }

    /// <summary>
    /// 提醒方式 1-站内信 2-自定义 3-不通知.
    /// </summary>
    [SugarColumn(ColumnName = "F_REMINDCATEGORY")]
    public int? RemindCategory { get; set; }

    /// <summary>
    /// 发送配置.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDCONFIGID")]
    public string SendConfigId { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXCERPT")]
    public string Excerpt { get; set; }

    /// <summary>
    /// 消息模板标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_DefaultTitle")]
    public string DefaultTitle { get; set; }
}