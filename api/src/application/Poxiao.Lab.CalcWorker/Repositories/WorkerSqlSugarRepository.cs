using SqlSugar;

namespace Poxiao.Lab.CalcWorker.Repositories;

/// <summary>
/// Worker 专用轻量级 SqlSugar 仓储。
/// 不依赖 Poxiao 框架的 App.GetConfig / ICacheManager / 多租户机制，
/// 直接使用注入的 ISqlSugarClient 连接实例。
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class WorkerSqlSugarRepository<TEntity> : SimpleClient<TEntity>, ISqlSugarRepository<TEntity>
    where TEntity : class, new()
{
    public WorkerSqlSugarRepository(ISqlSugarClient context) : base(context)
    {
        base.Context = (SqlSugarScope)context;
    }
}
