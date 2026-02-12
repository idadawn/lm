using Poxiao.Lab.CalcWorker.Repositories;
using SqlSugar;
using System.Reflection;

namespace Poxiao.Lab.CalcWorker.Extensions;

/// <summary>
/// Worker 专用 SqlSugar 配置扩展。
/// 使用 WorkerSqlSugarRepository 替代框架的 SqlSugarRepository，
/// 避免依赖 App.GetConfig / ICacheManager / 多租户等框架机制。
/// </summary>
public static class SqlSugarExtensions
{
    /// <summary>
    /// 为 Worker 注册 SqlSugar 数据库连接。
    /// </summary>
    public static IServiceCollection AddSqlSugarForWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("未配置数据库连接字符串 ConnectionStrings:Default");

        var sqlSugar = new SqlSugarScope(new ConnectionConfig
        {
            ConfigId = "default",
            ConnectionString = connectionString,
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            MoreSettings = new ConnMoreSettings
            {
                IsAutoRemoveDataCache = true,
                SqlServerCodeFirstNvarchar = true,
            },
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (type, column) =>
                {
                    if (new NullabilityInfoContext().Create(type).WriteState is NullabilityState.Nullable)
                    {
                        column.IsNullable = true;
                    }
                },
            },
        },
        db =>
        {
            var provider = db.GetConnectionScope("default");
            provider.Ado.CommandTimeOut = 30;

            // 开发阶段输出 SQL 日志
            provider.Aop.OnLogExecuting = (sql, pars) =>
            {
                // 可选：仅在 Debug 模式下输出
                // Console.WriteLine($"[SQL] {sql}");
            };
        });

        services.AddSingleton<ISqlSugarClient>(sqlSugar);

        // 使用 Worker 专用轻量仓储，而非框架的 SqlSugarRepository（后者依赖 App.GetConfig + 多租户）
        services.AddScoped(typeof(ISqlSugarRepository<>), typeof(WorkerSqlSugarRepository<>));

        return services;
    }
}
