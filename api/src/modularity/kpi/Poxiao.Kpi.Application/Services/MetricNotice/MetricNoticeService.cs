namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标通知服务.
/// </summary>
public class MetricNoticeService : IMetricNoticeService, ITransient
{
    /// <summary>
    /// 标签仓库.
    /// </summary>
    private readonly ISqlSugarRepository<MetricNoticeEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="MetricNoticeService"/>类型的新实例.
    /// </summary>
    public MetricNoticeService(ISqlSugarRepository<MetricNoticeEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取消息模板信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<MetricNoticeOutput> GetAsync(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var info = entity.Adapt<MetricNoticeOutput>();
        return info;
    }

    /// <inheritdoc />
    public async Task<List<MetricNoticeTemplateOutput>> GetTemplatesAsync()
    {
        var list = await _repository.AsSugarClient().Queryable<Message.Entitys.Entity.MessageTemplateEntity>()
            .Where(x => x.MessageSource != null && x.MessageSource.Equals(MessageConst.METRICNOTICETYPE))
            .Where(a => a.DeleteMark == null)
            .OrderBy(a => a.SortCode)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select(a => new MetricNoticeTemplateOutput
            {
                Id = a.Id,
                Name = a.FullName,
                EnCode = a.EnCode,
                TemplateType = a.TemplateType,
                MessageType = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "1" && u.EnCode == a.MessageType).Select(u => u.FullName),
                MessageSource = SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(u => u.Type == "4" && u.EnCode == a.MessageSource).Select(u => u.FullName)
            }).ToListAsync();
        return list;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<MetricNoticeOutput>> GetListAsync(MetricNoticeQryInput input)
    {
        var data = await _repository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.NodeId), it => it.NodeId.Equals(input.NodeId))
            .WhereIF(!string.IsNullOrEmpty(input.RuleId), it => it.RuleId != null && it.RuleId.Equals(input.RuleId))
            .Select(x => new MetricNoticeOutput
            {
                Id = x.Id,
                Type = x.Type,
                NodeId = x.NodeId,
                RuleId = x.RuleId,
                TemplateId = x.TemplateId,
                ScheduleId = x.ScheduleId,
                CreatedTime = x.CreatedTime,
                CreatedUserid = x.CreatedUserId,
                LastModifiedTime = x.LastModifiedTime,
                LastModifiedUserid = x.LastModifiedUserId,
                TenantId = x.TenantId,
            })
            .MergeTable()
            .OrderBy(it => it.NodeId)
            .OrderBy(it => it.RuleId)
            .OrderBy(it => it.CreatedTime)
            .ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PagedResultDto<MetricNoticeOutput>.SqlSugarPageResult(data);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(MetricNoticeCrInput input)
    {
        var entity = input.Adapt<MetricNoticeEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var count = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(x => x.Create()).ExecuteCommandAsync();
        return count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return 1;
        var count = await _repository.AsDeleteable().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        return count;
    }
}
