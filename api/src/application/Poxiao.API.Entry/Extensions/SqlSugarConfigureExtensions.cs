using Mapster;
using Poxiao;
using Poxiao.DatabaseAccessor;
using Poxiao.Logging;
using SqlSugar;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// SqlSugar配置拓展.
/// </summary>
public static class SqlSugarConfigureExtensions
{
    public static void SqlSugarConfigure(this IServiceCollection services)
    {
        // 获取选项
        var dbOptions = App.GetOptions<ConnectionStringsOptions>();

        dbOptions.ConnectionConfigs.ForEach(SetDbConfig);

        List<ConnectionConfig> connectConfigList = new List<ConnectionConfig>();

        SqlSugarScope sqlSugar = new(dbOptions.ConnectionConfigs.Adapt<List<ConnectionConfig>>(), db =>
        {
            dbOptions.ConnectionConfigs.ForEach(config =>
            {
                var dbProvider = db.GetConnectionScope(config.ConfigId);
                SetDbAop(dbProvider);
            });
        });
        var jsonClient = new JsonClient();
        services.AddSingleton<IJsonClient>(jsonClient);
        services.AddSingleton<ISqlSugarClient>(sqlSugar); // 单例注册
        services.AddScoped(typeof(ISqlSugarRepository<>), typeof(SqlSugarRepository<>)); // 仓储注册
        services.AddUnitOfWork<SqlSugarUnitOfWork>(); // 事务与工作单元注册

    }

    /// <summary>
    /// 配置连接属性.
    /// </summary>
    /// <param name="config"></param>
    private static void SetDbConfig(DbConnectionConfig config)
    {
        config.ConnectionString = string.Format(config.DefaultConnection, config.DBName);
        config.ConfigureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 处理列
            {
                if (new NullabilityInfoContext().Create(type).WriteState is NullabilityState.Nullable)
                    column.IsNullable = true;

                if (config.DbType == SqlSugar.DbType.Oracle)
                {
                    if (type.PropertyType == typeof(long) || type.PropertyType == typeof(long?))
                        column.DataType = "number(18)";
                    if (type.PropertyType == typeof(bool) || type.PropertyType == typeof(bool?))
                        column.DataType = "number(1)";
                }
            },
        };
        config.IsAutoCloseConnection = true;
        config.MoreSettings = new ConnMoreSettings
        {
            IsAutoRemoveDataCache = true,
            SqlServerCodeFirstNvarchar = true // 采用Nvarchar
        };
    }

    /// <summary>
    /// 配置Aop.
    /// </summary>
    /// <param name="db"></param>
    public static void SetDbAop(SqlSugarScopeProvider db)
    {
        var config = db.CurrentConnectionConfig;

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;

        // 打印SQL语句
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Log.Debug("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, sql, pars));
            App.PrintToMiniProfiler("SqlSugar", "Info", sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
        };
        db.Aop.OnError = ex =>
        {
            if (ex.Parametres == null) return;
            var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
            var errorSql = UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres);
            Log.Error("【" + DateTime.Now + "——错误SQL】\r\n" + errorSql, ex);
            App.PrintToMiniProfiler("SqlSugar", "Error", $"{ex.Message}{Environment.NewLine}{ex.Sql}{pars}{Environment.NewLine}");
        };
    }
}