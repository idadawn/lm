using Poxiao;
using Poxiao.Extras.DatabaseAccessor.SqlSugar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace SqlSugar;

/// <summary>
/// SqlSugar 仓储实现类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public partial class SqlSugarRepository<TEntity> : SimpleClient<TEntity>, ISqlSugarRepository<TEntity>
where TEntity : class, new()
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SqlSugarRepository(IServiceProvider serviceProvider, ISqlSugarClient context = null) : base(context)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var _cacheManager = serviceScope.ServiceProvider.GetService<ICacheManager>();

        // 获取数据库连接选项
        ConnectionStringsOptions connectionStrings = App.GetConfig<ConnectionStringsOptions>("ConnectionStrings", true);

        // 获取多租户选项
        TenantOptions tenant = App.GetConfig<TenantOptions>("Tenant", true);
        var httpContext = App.HttpContext;

        base.Context = (SqlSugarScope)context;

        string tenantId = connectionStrings.ConnectionConfigs.FirstOrDefault()?.ConfigId?.ToString() ?? "default";
        string tenantDbName = string.Empty;
        if (httpContext?.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() == null)
        {
            if (tenant.MultiTenancy && httpContext != null)
            {
                tenantId = httpContext?.User.FindFirst("TenantId")?.Value;
                var tenantCache = _cacheManager.Get<List<GlobalTenantCacheModel>>("poxiao:globaltenant").Find(it => it.TenantId.Equals(tenantId));
                if (tenant.MultiTenancyType.Equals("SCHEMA"))
                {
                    if (!base.Context.AsTenant().IsAnyConnection(tenantCache.connectionConfig.ConfigId))
                    {
                        base.Context.AsTenant().AddConnection(PoxiaoTenantExtensions.GetConfig(tenantCache.connectionConfig));
                    }
                    base.Context = base.Context.AsTenant().GetConnectionScope(tenantCache.connectionConfig.ConfigId);

                    if (!base.Context.Ado.IsValidConnection())
                    {
                        throw Oops.Oh("数据库连接错误");
                    }
                }
                tenantDbName = tenantCache.connectionConfig.IsolationField;
            }
            else
            {
                base.Context = base.Context.AsTenant().GetConnectionScope(tenantId);
            }
        }
        // 字段数据隔离
        if (!"default".Equals(tenantId) && tenant.MultiTenancyType.Equals("COLUMN"))
        {
            base.Context.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == tenantDbName);
            base.Context.Aop.DataExecuting = (oldValue, entityInfo) =>
            {
                if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.InsertByObject)
                {
                    entityInfo.SetValue(tenantDbName);
                }
                if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.UpdateByObject)
                {
                    entityInfo.SetValue(tenantDbName);
                }
                if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.DeleteByObject)
                {
                    entityInfo.SetValue(tenantDbName);
                }
            };
        }

        // 设置超时时间
        base.Context.Ado.CommandTimeOut = 30;

        base.Context.Aop.OnLogExecuting = (sql, pars) =>
        {
            if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Green;
            if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.White;
            if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Blue;
            // 在控制台输出sql语句
            Console.WriteLine("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(base.Context.CurrentConnectionConfig.DbType, sql, pars) + "\r\n");
            //App.PrintToMiniProfiler("SqlSugar", "Info", sql + "\r\n" + base.Context.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
        };

        base.Context.Aop.OnError = (ex) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            var pars = base.Context.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
            Console.WriteLine("【" + DateTime.Now + "——错误SQL】\r\n" + UtilMethods.GetSqlString(base.Context.CurrentConnectionConfig.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n");
            //App.PrintToMiniProfiler("SqlSugar", "Error", $"{ex.Message}{Environment.NewLine}{ex.Sql}{pars}{Environment.NewLine}");
        };

        if (base.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
        {
            base.Context.Aop.OnExecutingChangeSql = (sql, pars) =>
            {
                if (pars != null)
                {
                    foreach (var item in pars)
                    {
                        //如果是DbTppe=string设置成OracleDbType.Nvarchar2 
                        item.IsNvarchar2 = true;
                    }
                };
                return new KeyValuePair<string, SugarParameter[]>(sql, pars);
            };
        }
    }
}