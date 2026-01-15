namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 创更删实体基类.
/// </summary>
[SuppressSniffer]
public abstract class CEntityBase : EntityBase<string>, ICreatorTime
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
    /// 创建.
    /// </summary>
    public virtual void Create()
    {
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.CreatedTime = DateTime.Now;
        this.Id = this.Id == null ? SnowflakeIdHelper.NextId() : this.Id;
        if (!string.IsNullOrEmpty(userId))
        {
            this.CreatedUserId = CreatedUserId == null ? userId : CreatedUserId;
        }
    }
}