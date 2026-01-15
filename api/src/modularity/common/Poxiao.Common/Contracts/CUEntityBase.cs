namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 创建修改实体基类.
/// </summary>
[SuppressSniffer]
public class CUEntityBase : EntityBase<string>, ICreatorTime
{
    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "created_time", ColumnDescription = "创建时间")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 创建人.
    /// </summary>
    [SugarColumn(ColumnName = "created_userid", ColumnDescription = "创建人")]
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "last_modified_time", ColumnDescription = "最后修改时间")]
    public DateTime? LastModifiedTime { get; set; }

    /// <summary>
    /// 最后修改人.
    /// </summary>
    [SugarColumn(ColumnName = "last_modified_userid", ColumnDescription = "最后修改人")]
    public string? LastModifiedUserId { get; set; }

    /// <summary>
    /// 创建.
    /// </summary>
    public virtual void Create()
    {
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.CreatedTime = DateTime.Now;
        this.Id = this.Id ?? SnowflakeIdHelper.NextId();
        if (!string.IsNullOrEmpty(userId))
        {
            this.CreatedUserId = CreatedUserId ?? userId;
        }
    }

    /// <summary>
    /// 修改.
    /// </summary>
    public virtual void Update()
    {
        var userId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.LastModifiedTime = DateTime.Now;
        if (string.IsNullOrEmpty(userId)) return;
        this.LastModifiedUserId = userId;
    }
}

