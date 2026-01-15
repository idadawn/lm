using Poxiao;

namespace SqlSugar;

/// <summary>
/// Poxiao多租户拓展.
/// </summary>
public class PoxiaoTenantExtensions
{
    /// <summary>
    /// 获取普通链接.
    /// </summary>
    /// <param name="configId">配置ID.</param>
    /// <param name="tableName">数据库名称.</param>
    /// <param name="isolationField">隔离字段.</param>
    /// <returns></returns>
    public static ConnectionConfigOptions GetLinkToOrdinary(string configId, string tableName, string isolationField = null)
    {
        var dbOptions = App.GetOptions<ConnectionStringsOptions>();
        var defaultConnection = dbOptions.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        List<DBConnectionConfig> configList = new List<DBConnectionConfig>();
        configList.Add(new DBConnectionConfig()
        {
            IsMaster = true,
            ServiceName = tableName,
            dbType = defaultConnection.DbType,
            connectionStr = string.Format(defaultConnection.DefaultConnection, tableName)
        });
        return new ConnectionConfigOptions()
        {
            ConfigId = configId,
            IsCustom = false,
            IsolationField = isolationField,
            IsMasterSlaveSeparation = false,
            ConfigList = configList
        };
    }

    /// <summary>
    /// 获取自定义链接
    /// </summary>
    /// <param name="configId">配置ID.</param>
    /// <param name="tenantLinkModels">数据库连接列表.</param>
    /// <returns></returns>
    public static ConnectionConfigOptions GetLinkToCustom(string configId, List<TenantLinkModel> tenantLinkModels)
    {
        List<DBConnectionConfig> configList = new List<DBConnectionConfig>();
        foreach (var item in tenantLinkModels)
        {
            if (item.configType == 0)
            {
                if (!string.IsNullOrEmpty(item.connectionStr))
                {
                    configList.Add(new DBConnectionConfig()
                    {
                        IsMaster = true,
                        dbType = ToDbType(item.dbType),
                        ServiceName = item.serviceName,
                        connectionStr = item.connectionStr,
                    });
                }
                else
                {
                    configList.Add(new DBConnectionConfig()
                    {
                        IsMaster = true,
                        dbType = ToDbType(item.dbType),
                        connectionStr = ToConnectionString(ToDbType(item.dbType), item.host, Convert.ToInt32(item.port), item.serviceName, item.userName, item.password, item.dbSchema)
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.connectionStr))
                {
                    configList.Add(new DBConnectionConfig()
                    {
                        IsMaster = false,
                        dbType = ToDbType(item.dbType),
                        ServiceName = item.serviceName,
                        connectionStr = item.connectionStr,
                    });
                }
                else
                {
                    configList.Add(new DBConnectionConfig()
                    {
                        IsMaster = false,
                        dbType = ToDbType(item.dbType),
                        connectionStr = ToConnectionString(ToDbType(item.dbType), item.host, Convert.ToInt32(item.port), item.serviceName, item.userName, item.password, item.dbSchema)
                    });
                }
            }

        }
        return new ConnectionConfigOptions()
        {
            ConfigId = configId,
            IsCustom = true,
            IsMasterSlaveSeparation = tenantLinkModels.Any(it => it.configType.Equals(1)),
            ConfigList = configList
        };
    }

    /// <summary>
    /// 获取配置.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ConnectionConfig GetConfig(ConnectionConfigOptions options)
    {
        if (!options.IsCustom)
        {
            DBConnectionConfig config = options.ConfigList.FirstOrDefault();
            return new ConnectionConfig()
            {
                DbType = config.dbType,
                ConfigId = options.ConfigId,
                IsAutoCloseConnection = true,
                ConnectionString = config.connectionStr
            };
        }
        else
        {
            var slaveConnection = new List<SlaveConnectionConfig>();
            foreach (var item in options.ConfigList.FindAll(it => it.IsMaster.Equals(false)))
            {
                slaveConnection.Add(new SlaveConnectionConfig()
                {
                    HitRate = 10,
                    ConnectionString = item.connectionStr
                });
            }
            return new ConnectionConfig()
            {
                DbType = options.ConfigList.Find(it => it.IsMaster.Equals(true)).dbType,
                ConfigId = options.ConfigId,
                IsAutoCloseConnection = true,
                ConnectionString = options.ConfigList.Find(it => it.IsMaster.Equals(true)).connectionStr,
                SlaveConnectionConfigs = slaveConnection
            };
        }
    }

    /// <summary>
    /// 转换数据库类型.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    public static DbType ToDbType(string dbType)
    {
        switch (dbType.ToLower())
        {
            case "mysql":
                return SqlSugar.DbType.MySql;
            case "oracle":
                return SqlSugar.DbType.Oracle;
            case "dm8":
            case "dm":
                return SqlSugar.DbType.Dm;
            case "kdbndp":
            case "kingbasees":
                return SqlSugar.DbType.Kdbndp;
            case "postgresql":
                return SqlSugar.DbType.PostgreSQL;
            default:
                return SqlSugar.DbType.SqlServer;
        }
    }

    /// <summary>
    /// 转换连接字符串.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <param name="host">主机地址.</param>
    /// <param name="port">端口.</param>
    /// <param name="tableName">数据库名.</param>
    /// <param name="userName">用户名.</param>
    /// <param name="password">密码.</param>
    /// <param name="dbSchema">模式.</param>
    /// <returns></returns>
    public static string ToConnectionString(DbType dbType, string host, int port, string tableName, string userName, string password, string dbSchema)
    {
        switch (dbType)
        {
            case DbType.SqlServer:
                return string.Format("Data Source={0},{4};Initial Catalog={1};User ID={2};Password={3};Connection Timeout=5;MultipleActiveResultSets=true", host, tableName, userName, password, port);
            case DbType.Oracle:
                return string.Format("Data Source={0}:{1}/{2};User ID={3};Password={4};", host, port, dbSchema, userName, password);
            case DbType.MySql:
                return string.Format("server={0};port={1};database={2};user={3};password={4};AllowLoadLocalInfile=true", host, port.ToString(), tableName, userName, password);
            case DbType.Dm:
                return string.Format("server={0};port={1};database={2};User Id={3};PWD={4}", host, port.ToString(), tableName, userName, password);
            case DbType.Kdbndp:
                return string.Format("server={0};port={1};database={2};UID={3};PWD={4}", host, port.ToString(), tableName, userName, password);
            case DbType.PostgreSQL:
                return string.Format("server={0};port={1};Database={2};User Id={3};Password={4}", host, port, tableName, userName, password);
            default:
                return string.Format("Data Source={0},{4};Initial Catalog={1};User ID={2};Password={3};Connection Timeout=5;MultipleActiveResultSets=true", host, tableName, userName, password, port);
        }
    }
}