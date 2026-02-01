namespace Poxiao.Infrastructure.Contracts;

/// <summary>
/// 创更删实体基类.
/// </summary>
[SuppressSniffer]
public abstract class CDEntityBase : EntityBase<string>, ICreatorTime, IDeleteTime
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
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "delete_time", ColumnDescription = "删除时间")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    [SugarColumn(ColumnName = "delete_userid", ColumnDescription = "删除人")]
    public string? DeleteUserId { get; set; }

    /// <summary>
    /// 是否删除.
    /// </summary>
    [SugarColumn(ColumnName = "is_deleted", ColumnDescription = "是否删除")]
    public int IsDeleted { get; set; }

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

    /// <summary>
    /// 删除.
    /// </summary>
    public virtual void Delete()
    {
        var userId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        this.DeleteTime = DateTime.Now;
        this.IsDeleted = 1;
        if (!string.IsNullOrEmpty(userId))
        {
            this.DeleteUserId = userId;
        }
    }
}