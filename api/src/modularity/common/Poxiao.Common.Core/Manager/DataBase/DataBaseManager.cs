using Mapster;
using Microsoft.Extensions.Options;
using Poxiao.DependencyInjection;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Dtos.DataBase;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Models.VisualDev;
using Poxiao.Infrastructure.Security;
using Poxiao.Logging;
using Poxiao.Systems.Entitys.Dto.Database;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Entitys.Dto.VisualDevModelData;
using SqlSugar;
using System.Data;
using System.Dynamic;
using System.Text;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// 实现切换数据库后操作.
/// </summary>
public class DataBaseManager : IDataBaseManager, ITransient
{
    /// <summary>
    /// 初始化客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 数据库配置选项.
    /// </summary>
    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public DataBaseManager(
        IOptions<ConnectionStringsOptions> connectionOptions,
        IUserManager userManager,
        IOptions<TenantOptions> tenantOptions,
        ISqlSugarClient context,
        ICacheManager cacheManager)
    {
        _sqlSugarClient = (SqlSugarScope)context;
        _userManager = userManager;
        _connectionStrings = connectionOptions.Value;
        _tenant = tenantOptions.Value;
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 数据库切换.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns>切库后的SqlSugarClient.</returns>
    public SqlSugarScope ChangeDataBase(DbLinkEntity link)
    {
        //if (!"default".Equals(link.Id) && _tenant.MultiTenancyType.Equals("COLUMN"))
        //{
        //    _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == link.ServiceName);
        //}
        if (_sqlSugarClient.AsTenant().IsAnyConnection(link.Id))
        {
            _sqlSugarClient.ChangeDatabase(link.Id);
        }
        else
        {
            _sqlSugarClient.AddConnection(new ConnectionConfig()
            {
                ConfigId = link.Id,
                DbType = ToDbType(link.DbType),
                ConnectionString = ToConnectionString(link),
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });

            _sqlSugarClient.Ado.CommandTimeOut = 30;

            var config = _sqlSugarClient.CurrentConnectionConfig;

            _sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
            {
                Log.Debug("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, sql, pars));
                App.PrintToMiniProfiler("SqlSugar", "Info", sql + "\r\n" + _sqlSugarClient.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };

            _sqlSugarClient.Aop.OnError = (ex) =>
            {
                if (ex.Parametres == null) return;
                var pars = _sqlSugarClient.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                var errorSql = UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres);
                Log.Error("【" + DateTime.Now + "——错误SQL】\r\n" + errorSql, ex);
                App.PrintToMiniProfiler("SqlSugar", "Error", $"{ex.Message}{Environment.NewLine}{ex.Sql}{pars}{Environment.NewLine}");
            };

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            {
                _sqlSugarClient.Aop.OnExecutingChangeSql = (sql, pars) =>
                {
                    if (pars != null)
                    {
                        foreach (var item in pars)
                        {
                            // 如果是DbTppe=string设置成OracleDbType.Nvarchar2
                            item.IsNvarchar2 = true;
                        }
                    }
                    return new KeyValuePair<string, SugarParameter[]>(sql, pars);
                };
            }
            _sqlSugarClient.ChangeDatabase(link.Id);
        }
        return _sqlSugarClient;
    }

    /// <summary>
    /// 获取租户SqlSugarClient客户端.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    public ISqlSugarClient GetTenantSqlSugarClient(string tenantId)
    {
        var tenant = GetGlobalTenantCache(tenantId);
        if (_sqlSugarClient.AsTenant().IsAnyConnection(tenantId))
        {
            _sqlSugarClient.ChangeDatabase(tenantId);
        }
        else
        {
            _sqlSugarClient.AddConnection(new ConnectionConfig()
            {
                ConfigId = tenant.TenantId,
                DbType = tenant.connectionConfig.ConfigList.FirstOrDefault().dbType,
                ConnectionString = tenant.connectionConfig.ConfigList.FirstOrDefault().connectionStr,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });

            _sqlSugarClient.Ado.CommandTimeOut = 30;

            var config = _sqlSugarClient.CurrentConnectionConfig;

            _sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
            {
                Log.Debug("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, sql, pars));
                App.PrintToMiniProfiler("SqlSugar", "Info", sql + "\r\n" + _sqlSugarClient.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };

            _sqlSugarClient.Aop.OnError = (ex) =>
            {
                if (ex.Parametres == null) return;
                var pars = _sqlSugarClient.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                var errorSql = UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres);
                Log.Error("【" + DateTime.Now + "——错误SQL】\r\n" + errorSql, ex);
                App.PrintToMiniProfiler("SqlSugar", "Error", $"{ex.Message}{Environment.NewLine}{ex.Sql}{pars}{Environment.NewLine}");
            };

            if (config.DbType == SqlSugar.DbType.Oracle)
            {
                _sqlSugarClient.Aop.OnExecutingChangeSql = (sql, pars) =>
                {
                    if (pars != null)
                    {
                        foreach (var item in pars)
                        {
                            // 如果是DbTppe=string设置成OracleDbType.Nvarchar2
                            item.IsNvarchar2 = true;
                        }
                    }
                    return new KeyValuePair<string, SugarParameter[]>(sql, pars);
                };
            }
            _sqlSugarClient.ChangeDatabase(tenantId);
        }
        return _sqlSugarClient;
    }

    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="tenantName">租户数据库.</param>
    /// <returns></returns>
    public DbLinkEntity GetTenantDbLink(string tenantId, string tenantName)
    {
        var defaultConnection = _connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        return new DbLinkEntity
        {
            Id = tenantId,
            ServiceName = tenantName,
            DbType = defaultConnection.DbType.ToString(),
            Host = defaultConnection.Host,
            Port = defaultConnection.Port,
            UserName = defaultConnection.UserName,
            Password = defaultConnection.Password
        };
    }

    /// <summary>
    /// 执行Sql(查询).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns></returns>
    public async Task<int> ExecuteSql(DbLinkEntity link, string strSql)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId?.ToString() != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        int flag = 0;
        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            flag = await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql.TrimEnd(';'));
        else
            flag = await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return flag;
    }

    /// <summary>
    /// 条件动态过滤.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns>条件是否成立.</returns>
    public bool WhereDynamicFilter(DbLinkEntity link, string strSql)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = _sqlSugarClient.Ado.SqlQuery<dynamic>(strSql).Count > 0;
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 执行Sql(新增、修改).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="dicList">数据.</param>
    /// <param name="primaryField">主键字段.</param>
    /// <returns></returns>
    public async Task<int> ExecuteSql(DbLinkEntity link, string table, List<Dictionary<string, object>> dicList, string primaryField = "")
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        int flag = 0;
        if (string.IsNullOrEmpty(primaryField))
        {
            foreach (var item in dicList) flag = await _sqlSugarClient.Insertable(item).AS(table).ExecuteCommandAsync();
        }
        else
        {
            foreach (var item in dicList) flag = await _sqlSugarClient.Updateable(item).AS(table).WhereColumns(primaryField).ExecuteCommandAsync();
        }
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return flag;
    }

    /// <summary>
    /// 执行Sql 新增 并返回自增长Id.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="dicList">数据.</param>
    /// <param name="primaryField">主键字段.</param>
    /// <returns>id.</returns>
    public async Task<int> ExecuteReturnIdentityAsync(DbLinkEntity link, string table, List<Dictionary<string, object>> dicList, string primaryField = "")
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        int flag = 0;
        if (string.IsNullOrEmpty(primaryField))
            flag = await _sqlSugarClient.Insertable(dicList).AS(table).ExecuteReturnIdentityAsync();

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return flag;
    }

    /// <summary>
    /// 创建表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    public async Task<bool> Create(DbLinkEntity link, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);
        CreateTable(tableModel, tableFieldList);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return true;
    }

    /// <summary>
    /// sqlsugar添加表字段.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="tableFieldList">表字段集合.</param>
    public void AddTableColumn(DbLinkEntity link, string tableName, List<DbTableFieldModel> tableFieldList)
    {
        try
        {
            if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
                _sqlSugarClient = ChangeDataBase(link);
            var cloumnList = tableFieldList.Adapt<List<DbColumnInfo>>();
            DelDataLength(cloumnList);
            foreach (var item in cloumnList)
            {
                _sqlSugarClient.DbMaintenance.AddColumn(tableName, item);
                if (_sqlSugarClient.CurrentConnectionConfig.DbType != SqlSugar.DbType.MySql)
                    _sqlSugarClient.DbMaintenance.AddColumnRemark(item.DbColumnName, tableName, item.ColumnDescription);
            }
            //if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.MySql)
            //    AddColumnMySql(tableName, tableFieldList);
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 删除表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    public bool Delete(DbLinkEntity link, string table)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            _sqlSugarClient.DbMaintenance.DropTable(table);
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 修改表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="oldTable">原数据.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    public async Task<bool> Update(DbLinkEntity link, string oldTable, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);
        _sqlSugarClient.DbMaintenance.DropTable(oldTable);
        try
        {
            CreateTable(tableModel, tableFieldList);
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 根据链接获取分页数据.
    /// </summary>
    /// <returns></returns>
    public PageResult<Dictionary<string, object>> GetInterFaceData(DbLinkEntity link, string strSql, VisualDevModelListQueryInput pageInput, MainBeltViceQueryModel columnDesign, List<IConditionalModel> dataPermissions, Dictionary<string, string> outColumnName = null)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id) _sqlSugarClient = ChangeDataBase(link);

        try
        {
            int total = 0;

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle) strSql = strSql.Replace(";", string.Empty);

            var sidx = pageInput.Sidx.IsNotEmptyOrNull() && pageInput.Sort.IsNotEmptyOrNull(); // 按前端参数排序
            var defaultSidx = columnDesign.defaultSidx.IsNotEmptyOrNull() && columnDesign.sort.IsNotEmptyOrNull(); // 按模板默认排序

            var dataRuleJson = new List<IConditionalModel>();
            if (pageInput.dataRuleJson.IsNotEmptyOrNull()) dataRuleJson = _sqlSugarClient.Utilities.JsonToConditionalModels(pageInput.dataRuleJson);

            var querJson = new List<IConditionalModel>();
            if (pageInput.QueryJson.IsNotEmptyOrNull()) querJson = _sqlSugarClient.Utilities.JsonToConditionalModels(pageInput.QueryJson);

            var superQueryJson = new List<IConditionalModel>();
            if (pageInput.superQueryJson.IsNotEmptyOrNull()) superQueryJson = _sqlSugarClient.Utilities.JsonToConditionalModels(pageInput.superQueryJson);
            // var sql = _sqlSugarClient.SqlQueryable<object>(strSql)
            // .Where(dataRuleJson).Where(querJson).Where(superQueryJson).Where(dataPermissions).ToSqlString();
            DataTable dt = _sqlSugarClient.SqlQueryable<object>(strSql)
                .Where(dataRuleJson).Where(querJson).Where(superQueryJson).Where(dataPermissions)
                .OrderByIF(sidx, pageInput.Sidx + " " + pageInput.Sort).OrderByIF(!sidx && defaultSidx, columnDesign.defaultSidx + " " + columnDesign.sort)
                .ToDataTablePage(pageInput.CurrentPage, pageInput.PageSize, ref total);

            // 如果有字段别名 替换 ColumnName
            if (outColumnName != null && outColumnName.Count > 0)
            {
                var resultKey = string.Empty;
                for (var i = 0; i < dt.Columns.Count; i++)
                    dt.Columns[i].ColumnName = outColumnName.TryGetValue(dt.Columns[i].ColumnName.ToUpper(), out resultKey) == true ? outColumnName[dt.Columns[i].ColumnName.ToUpper()] : dt.Columns[i].ColumnName.ToUpper();
            }

            var data = new PageResult<Dictionary<string, object>>()
            {
                pagination = new PageInfo()
                {
                    currentPage = pageInput.CurrentPage,
                    pageSize = pageInput.PageSize,
                    total = total
                },
                list = dt.ToObject<List<Dictionary<string, string>>>().ToObject<List<Dictionary<string, object>>>()
            };

            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);

            return data;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 表是否存在.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    public bool IsAnyTable(DbLinkEntity link, string table)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = _sqlSugarClient.DbMaintenance.IsAnyTable(table, false);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);

        return data;
    }

    /// <summary>
    /// 表是否存在数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    public bool IsAnyData(DbLinkEntity link, string table)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = _sqlSugarClient.Queryable<dynamic>().AS(table).Any();
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 表字段是否存在.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="column">表字段名.</param>
    /// <returns></returns>
    public bool IsAnyColumn(DbLinkEntity link, string table, string column)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = _sqlSugarClient.DbMaintenance.IsAnyColumn(table, column, false);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取表字段列表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns>TableFieldListModel.</returns>
    public List<DbTableFieldModel> GetFieldList(DbLinkEntity? link, string? tableName)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id) _sqlSugarClient = ChangeDataBase(link);

        var list = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(tableName, false);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return list.Adapt<List<DbTableFieldModel>>();
    }

    /// <summary>
    /// 获取表数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    public DataTable GetData(DbLinkEntity link, string tableName)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);
        var data = _sqlSugarClient.Queryable<dynamic>().AS(tableName).ToDataTable();
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 根据链接获取数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    /// <returns></returns>
    public DataTable GetInterFaceData(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        try
        {
            if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
                _sqlSugarClient = ChangeDataBase(link);

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
                strSql = strSql.Replace(";", string.Empty);

            var data = _sqlSugarClient.Ado.GetDataTable(strSql, parameters);
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
            return data;
        }
        catch (Exception ex)
        {

            throw Oops.Oh(ErrorCode.D1511);
        }
    }

    /// <summary>
    /// 根据链接获取数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    /// <returns></returns>
    public DataTable GetInterFaceDataNew(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        try
        {
            if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
                _sqlSugarClient = ChangeDataBase(link);

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
                strSql = strSql.Replace(";", string.Empty);

            var data = _sqlSugarClient.CopyNew().Ado.GetDataTable(strSql, parameters);
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
            return data;
        }
        catch (Exception ex)
        {

            throw Oops.Oh(ErrorCode.D1511);
        }
    }

    /// <summary>
    /// 执行增删改sql.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    public void ExecuteCommand(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            strSql = strSql.Replace(";", string.Empty);

        _sqlSugarClient.CopyNew().Ado.ExecuteCommand(strSql, parameters);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
    }

    /// <summary>
    /// 执行统计sql.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    public int GetCount(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            strSql = strSql.Replace(";", string.Empty);

        var count = _sqlSugarClient.CopyNew().Ado.GetInt(strSql, parameters);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return count;
    }

    /// <summary>
    /// 获取表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    public DatabaseTableInfoOutput GetDataBaseTableInfo(DbLinkEntity link, string tableName)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = new DatabaseTableInfoOutput()
        {
            tableInfo = _sqlSugarClient.DbMaintenance.GetTableInfoList(false).Find(m => m.Name == tableName).Adapt<TableInfoOutput>(),
            tableFieldList = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(tableName, false).Adapt<List<TableFieldOutput>>()
        };

        data.tableFieldList = ViewDataTypeConversion(data.tableFieldList, _sqlSugarClient.CurrentConnectionConfig.DbType);

        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);

        return data;
    }

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public List<DatabaseTableListOutput> GetDBTableList(DbLinkEntity link)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var dbType = link.DbType;
        var sql = DBTableSql(dbType);
        var data = new List<DatabaseTableListOutput>();
        if ("postgresql".Equals(dbType.ToLower()))
        {
            data = _sqlSugarClient.Ado.SqlQuery<dynamic>(sql).Select(x => new DatabaseTableListOutput { table = x.f_table, tableName = x.f_tablename, sum = x.f_sum }).ToList();
        }
        else
        {
            var modelList = _sqlSugarClient.Ado.SqlQuery<DynamicDbTableModel>(sql).ToList();
            data = modelList.Select(x => new DatabaseTableListOutput { table = x.FTABLE, tableName = x.FTABLENAME, sum = x.FSUM.ParseToInt() }).ToList();
        }
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="isView">视图.</param>
    /// <returns></returns>
    public List<DbTableInfo> GetTableInfos(DbLinkEntity link, bool isView = false)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = isView ? _sqlSugarClient.DbMaintenance.GetViewInfoList(false) : _sqlSugarClient.DbMaintenance.GetTableInfoList(false);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取数据表分页(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetDataTablePage(DbLinkEntity link, string dbSql, int pageIndex, int pageSize)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        RefAsync<int> totalNumber = 0;
        var list = await _sqlSugarClient.SqlQueryable<object>(dbSql).ToDataTablePageAsync(pageIndex, pageSize, totalNumber);
        var data = PageResult<dynamic>.SqlSugarPageResult(new SqlSugarPagedList<dynamic>()
        {
            list = ToDynamicList(list),
            pagination = new Pagination()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Total = totalNumber
            }
        });
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取数据表分页(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <returns></returns>
    public async Task<List<string>> GetListStringAsync(DbLinkEntity link, string dbSql)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = await _sqlSugarClient.Ado.SqlQueryAsync<string>(dbSql);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取单个值.
    /// </summary>
    /// <param name="link"></param>
    /// <param name="dbSql"></param>
    /// <returns></returns>
    public async Task<string> GetStringAsync(DbLinkEntity link, string dbSql)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = await _sqlSugarClient.Ado.SqlQuerySingleAsync<string>(dbSql);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取数据表分页(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetListAsync<TEntity>(DbLinkEntity link, string dbSql)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);
        var data = await _sqlSugarClient.Ado.SqlQueryAsync<TEntity>(dbSql);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 获取数据表分页(实体).
    /// </summary>
    /// <typeparam name="TEntity">T.</typeparam>
    /// <param name="link">数据连接.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetDataTablePage<TEntity>(DbLinkEntity link, int pageIndex, int pageSize)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        var data = await _sqlSugarClient.Queryable<TEntity>().ToPageListAsync(pageIndex, pageSize);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return data;
    }

    /// <summary>
    /// 同步数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dt">同步数据.</param>
    /// <param name="table">表.</param>
    /// <returns></returns>
    public async Task<bool> SyncData(DbLinkEntity link, DataTable dt, string table)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        List<Dictionary<string, object>> dc = _sqlSugarClient.Utilities.DataTableToDictionaryList(dt); // 5.0.23版本支持
        var isOk = await _sqlSugarClient.Insertable(dc).AS(table).ExecuteCommandAsync();
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        return isOk > 0;
    }

    /// <summary>
    /// 同步表操作.
    /// </summary>
    /// <param name="linkFrom">原数据库.</param>
    /// <param name="linkTo">目前数据库.</param>
    /// <param name="table">表名称.</param>
    /// <param name="type">操作类型.</param>
    /// <param name="fieldType">数据类型.</param>
    public void SyncTable(DbLinkEntity linkFrom, DbLinkEntity linkTo, string table, int type, Dictionary<string, string> fieldType)
    {
        try
        {
            switch (type)
            {
                case 2:
                    {
                        if (linkFrom != null)
                            _sqlSugarClient = ChangeDataBase(linkFrom);
                        var columns = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(table, false);
                        if (linkTo != null)
                            _sqlSugarClient = ChangeDataBase(linkTo);
                        DelDataLength(columns, fieldType);
                        _sqlSugarClient.DbMaintenance.CreateTable(table, columns);
                    }
                    break;
                case 3:
                    {
                        if (linkTo != null)
                            _sqlSugarClient = ChangeDataBase(linkTo);
                        _sqlSugarClient.DbMaintenance.TruncateTable(table);
                    }
                    break;
            }
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 使用存储过程.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="stored">存储过程名称.</param>
    /// <param name="parameters">参数.</param>
    public void UseStoredProcedure(DbLinkEntity link, string stored, List<SugarParameter> parameters)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        _sqlSugarClient.Ado.UseStoredProcedure().GetDataTable(stored, parameters);
        _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
    }

    /// <summary>
    /// 测试数据库连接.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public bool IsConnection(DbLinkEntity link)
    {
        if (link != null && _sqlSugarClient.CurrentConnectionConfig.ConfigId != link.Id)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.Ado.IsValidConnection())
        {
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
            return true;
        }
        else
        {
            _sqlSugarClient.ChangeDatabase(_connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default").ConfigId);
            return false;
        }
    }

    /// <summary>
    /// 视图数据类型转换.
    /// </summary>
    /// <param name="fields">字段数据.</param>
    /// <param name="databaseType">数据库类型.</param>
    public List<TableFieldOutput> ViewDataTypeConversion(List<TableFieldOutput> fields, SqlSugar.DbType databaseType)
    {
        foreach (var item in fields)
        {
            item.dataType = item.dataType.ToLower();
            switch (item.dataType)
            {
                case "string":
                    {
                        item.dataType = "varchar";
                        if (item.dataLength.ParseToInt() > 2000)
                        {
                            item.dataType = "text";
                            item.dataLength = "50";
                        }
                    }

                    break;
                case "single":
                    item.dataType = "decimal";
                    break;
            }
        }

        return fields;
    }

    /// <summary>
    /// 转换数据库类型.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    public SqlSugar.DbType ToDbType(string dbType)
    {
        switch (dbType.ToLower())
        {
            case "sqlserver":
                return SqlSugar.DbType.SqlServer;
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
                throw Oops.Oh(ErrorCode.D1505);
        }
    }

    /// <summary>
    /// 转换连接字符串.
    /// </summary>
    /// <param name="dbLinkEntity">数据连接.</param>
    /// <returns></returns>
    public string ToConnectionString(DbLinkEntity dbLinkEntity)
    {
        switch (dbLinkEntity.DbType.ToLower())
        {
            case "sqlserver":
                return string.Format("Data Source={0},{4};Initial Catalog={1};User ID={2};Password={3};MultipleActiveResultSets=true", dbLinkEntity.Host, dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password, dbLinkEntity.Port);
            case "oracle":
                if (dbLinkEntity.OracleParam.IsNotEmptyOrNull())
                {
                    var oracleParam = dbLinkEntity.OracleParam.ToObject<OracleParamModel>();
                    return string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME={2})));User Id={3};Password={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), oracleParam.oracleService, dbLinkEntity.UserName, dbLinkEntity.Password);
                }
                else
                {
                    return string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME=ORCL)));User Id={2};Password={3}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.UserName, dbLinkEntity.Password);
                }

            case "mysql":
                return string.Format("server={0};port={1};database={2};user={3};password={4};AllowLoadLocalInfile=true", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "dm8":
            case "dm":
                return string.Format("server={0};port={1};database={2};User Id={3};PWD={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "kdbndp":
            case "kingbasees":
                return string.Format("server={0};port={1};database={2};UID={3};PWD={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "postgresql":
                return string.Format("server={0};port={1};Database={2};User Id={3};Password={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            default:
                throw Oops.Oh(ErrorCode.D1505);
        }
    }

    /// <summary>
    /// DataTable转DicList.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private List<Dictionary<string, object>> DataTableToDicList(DataTable dt)
    {
        return dt.AsEnumerable().Select(row => dt.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName.ToLower(), column => row[column])).ToList();
    }

    /// <summary>
    /// 将DataTable 转换成 List<dynamic>
    /// reverse 反转：控制返回结果中是只存在 FilterField 指定的字段,还是排除.
    /// [flase 返回FilterField 指定的字段]|[true 返回结果剔除 FilterField 指定的字段]
    /// FilterField  字段过滤，FilterField 为空 忽略 reverse 参数；返回DataTable中的全部数
    /// </summary>
    /// <param name="table">DataTable</param>
    /// <param name="reverse">
    /// 反转：控制返回结果中是只存在 FilterField 指定的字段,还是排除.
    /// [flase 返回FilterField 指定的字段]|[true 返回结果剔除 FilterField 指定的字段]
    /// </param>
    /// <param name="FilterField">字段过滤，FilterField 为空 忽略 reverse 参数；返回DataTable中的全部数据</param>
    /// <returns>List<dynamic></dynamic></returns>
    public static List<dynamic> ToDynamicList(DataTable table, bool reverse = true, params string[] FilterField)
    {
        var modelList = new List<dynamic>();
        foreach (DataRow row in table.Rows)
        {
            dynamic model = new ExpandoObject();
            var dict = (IDictionary<string, object>)model;
            foreach (DataColumn column in table.Columns)
            {
                if (FilterField.Length != 0)
                {
                    if (reverse)
                    {
                        if (!FilterField.Contains(column.ColumnName))
                        {
                            dict[column.ColumnName] = row[column];
                        }
                    }
                    else
                    {
                        if (FilterField.Contains(column.ColumnName))
                        {
                            dict[column.ColumnName] = row[column];
                        }
                    }
                }
                else
                {
                    dict[column.ColumnName.ToLower()] = row[column];
                }
            }

            modelList.Add(model);
        }

        return modelList;
    }

    /// <summary>
    /// 数据库表SQL.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    private string DBTableSql(string dbType)
    {
        StringBuilder sb = new StringBuilder();
        switch (dbType.ToLower())
        {
            case "sqlserver":
                //sb.Append(@"DECLARE @TABLEINFO TABLE ( NAME VARCHAR(50) , SUMROWS VARCHAR(11) , RESERVED VARCHAR(50) , DATA VARCHAR(50) , INDEX_SIZE VARCHAR(50) , UNUSED VARCHAR(50) , PK VARCHAR(50) ) DECLARE @TABLENAME TABLE ( NAME VARCHAR(50) ) DECLARE @NAME VARCHAR(50) DECLARE @PK VARCHAR(50) INSERT INTO @TABLENAME ( NAME ) SELECT O.NAME FROM SYSOBJECTS O , SYSINDEXES I WHERE O.ID = I.ID AND O.XTYPE = 'U' AND I.INDID < 2 ORDER BY I.ROWS DESC , O.NAME WHILE EXISTS ( SELECT 1 FROM @TABLENAME ) BEGIN SELECT TOP 1 @NAME = NAME FROM @TABLENAME DELETE @TABLENAME WHERE NAME = @NAME DECLARE @OBJECTID INT SET @OBJECTID = OBJECT_ID(@NAME) SELECT @PK = COL_NAME(@OBJECTID, COLID) FROM SYSOBJECTS AS O INNER JOIN SYSINDEXES AS I ON I.NAME = O.NAME INNER JOIN SYSINDEXKEYS AS K ON K.INDID = I.INDID WHERE O.XTYPE = 'PK' AND PARENT_OBJ = @OBJECTID AND K.ID = @OBJECTID INSERT INTO @TABLEINFO ( NAME , SUMROWS , RESERVED , DATA , INDEX_SIZE , UNUSED ) EXEC SYS.SP_SPACEUSED @NAME UPDATE @TABLEINFO SET PK = @PK WHERE NAME = @NAME END SELECT F.NAME AS F_TABLE,ISNULL(P.TDESCRIPTION,F.NAME) AS F_TABLENAME, F.RESERVED AS F_SIZE, RTRIM(F.SUMROWS) AS F_SUM, F.PK AS F_PRIMARYKEY FROM @TABLEINFO F LEFT JOIN ( SELECT NAME = CASE WHEN A.COLORDER = 1 THEN D.NAME ELSE '' END , TDESCRIPTION = CASE WHEN A.COLORDER = 1 THEN ISNULL(F.VALUE, '') ELSE '' END FROM SYSCOLUMNS A LEFT JOIN SYSTYPES B ON A.XUSERTYPE = B.XUSERTYPE INNER JOIN SYSOBJECTS D ON A.ID = D.ID AND D.XTYPE = 'U' AND D.NAME <> 'DTPROPERTIES' LEFT JOIN SYS.EXTENDED_PROPERTIES F ON D.ID = F.MAJOR_ID WHERE A.COLORDER = 1 AND F.MINOR_ID = 0 ) P ON F.NAME = P.NAME WHERE 1 = 1 ORDER BY F_TABLE");
                sb.Append(@"SELECT s.Name F_TABLE, Convert(nvarchar(max), tbp.value) as F_TABLENAME, b.ROWS F_SUM FROM sysobjects s LEFT JOIN sys.extended_properties as tbp ON s.id = tbp.major_id and tbp.minor_id = 0 AND ( tbp.Name = 'MS_Description' OR tbp.Name is null ) LEFT JOIN sysindexes AS b ON s.id = b.id WHERE s.xtype IN('U') AND (b.indid IN (0, 1))");
                break;
            case "oracle":
                //sb.Append(@"SELECT DISTINCT COL.TABLE_NAME AS F_TABLE,TAB.COMMENTS AS F_TABLENAME,0 AS F_SIZE,NVL(T.NUM_ROWS,0)AS F_SUM,COLUMN_NAME AS F_PRIMARYKEY FROM USER_CONS_COLUMNS COL INNER JOIN USER_CONSTRAINTS CON ON CON.CONSTRAINT_NAME=COL.CONSTRAINT_NAME INNER JOIN USER_TAB_COMMENTS TAB ON TAB.TABLE_NAME=COL.TABLE_NAME INNER JOIN USER_TABLES T ON T.TABLE_NAME=COL.TABLE_NAME WHERE CON.CONSTRAINT_TYPE NOT IN('C','R')ORDER BY COL.TABLE_NAME");
                sb.Append(@"SELECT table_name F_TABLE , (select COMMENTS from user_tab_comments where t.table_name=table_name ) as F_TABLENAME, T.NUM_ROWS F_SUM from user_tables t where table_name!='HELP' AND table_name NOT LIKE '%$%' AND table_name NOT LIKE 'LOGMNRC_%' AND table_name!='LOGMNRP_CTAS_PART_MAP' AND table_name!='LOGMNR_LOGMNR_BUILDLOG' AND table_name!='SQLPLUS_PRODUCT_PROFILE'");
                break;
            case "mysql":
                //sb.Append(@"SELECT T1.*,(SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.`COLUMNS`WHERE TABLE_SCHEMA=DATABASE()AND TABLE_NAME=T1.F_TABLE AND COLUMN_KEY='PRI')F_PRIMARYKEY FROM(SELECT TABLE_NAME F_TABLE,0 F_SIZE,TABLE_ROWS F_SUM,(SELECT IF(LENGTH(TRIM(TABLE_COMMENT))<1,TABLE_NAME,TABLE_COMMENT))F_TABLENAME FROM INFORMATION_SCHEMA.`TABLES`WHERE TABLE_SCHEMA=DATABASE())T1 ORDER BY T1.F_TABLE");
                sb.Append(@"select TABLE_NAME as F_TABLE,TABLE_ROWS as F_SUM ,TABLE_COMMENT as F_TABLENAME from information_schema.tables where TABLE_SCHEMA=(select database()) AND TABLE_TYPE='BASE TABLE'");
                break;
            case "dm8":
            case "dm":
                sb.Append(@"SELECT table_name F_TABLE , (select COMMENTS from user_tab_comments where t.table_name=table_name ) as F_TABLENAME, T.NUM_ROWS F_SUM from user_tables t where table_name!='HELP' AND table_name NOT LIKE '%$%' AND table_name NOT LIKE 'LOGMNRC_%' AND table_name!='LOGMNRP_CTAS_PART_MAP' AND table_name!='LOGMNR_LOGMNR_BUILDLOG' AND table_name!='SQLPLUS_PRODUCT_PROFILE'");
                break;
            case "kdbndp":
            case "kingbasees":
                sb.Append(@"select a.relname F_TABLE,a.n_live_tup F_SUM,b.description F_TABLENAME from sys_stat_user_tables a left outer join sys_description b on a.relid = b.objoid where a.schemaname='public' and b.objsubid='0'");
                break;
            case "postgresql":
                sb.Append(@"select cast(relname as varchar) as F_TABLE,cast(reltuples as int) as F_SUM, cast(obj_description(relfilenode,'pg_class') as varchar) as F_TABLENAME from pg_class c inner join pg_namespace n on n.oid = c.relnamespace and nspname='public' inner join pg_tables z on z.tablename=c.relname where relkind = 'r' and relname not like 'pg_%' and relname not like 'sql_%' and schemaname='public' order by relname");
                break;
            default:
                throw new Exception("不支持");
        }

        return sb.ToString();
    }

    /// <summary>
    /// MySql创建表单+注释.
    /// </summary>
    /// <param name="tableModel">表.</param>
    /// <param name="tableFieldList">字段.</param>
    private async Task CreateTableMySql(DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        StringBuilder strSql = new StringBuilder();
        strSql.Append("CREATE TABLE `" + tableModel.table + "` (\r\n");
        foreach (var item in tableFieldList)
        {
            if (item.primaryKey && item.allowNull == 1)
                throw Oops.Oh(ErrorCode.D1509);
            strSql.Append(" `" + item.field + "` " + item.dataType.ToUpper() + "");
            if (item.dataType == "varchar" || item.dataType == "nvarchar" || item.dataType == "decimal")
                strSql.Append(" (" + item.dataLength + ") ");
            if (item.primaryKey)
                strSql.Append(" primary key ");
            if (item.allowNull != 1)
                strSql.Append(" NOT NULL ");
            else
                strSql.Append(" NULL ");
            if (item.identity)
                strSql.Append(" AUTO_INCREMENT ");
            strSql.Append("COMMENT '" + item.fieldName + "'");
            strSql.Append(",");
        }

        strSql.Remove(strSql.Length - 1, 1);
        strSql.Append("\r\n");
        strSql.Append(") COMMENT = '" + tableModel.tableName + "';");
        await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql.ToString());
    }

    /// <summary>
    /// MySql创建表单+注释.
    /// </summary>
    /// <param name="table">表.</param>
    /// <param name="tableFieldList">字段.</param>
    private void AddColumnMySql(string table, List<DbTableFieldModel> tableFieldList)
    {
        StringBuilder strSql = new StringBuilder();
        foreach (var item in tableFieldList)
        {
            if (item.dataType == "varchar" || item.dataType == "nvarchar" || item.dataType == "decimal")
                strSql.AppendFormat("ALTER TABLE {0} MODIFY {1} {2}({3}) COMMENT '{4}';", table, item.field, item.dataType, item.dataLength, item.fieldName);
            else
                strSql.AppendFormat("ALTER TABLE {0} MODIFY {1} {2} COMMENT '{3}';", table, item.field, item.dataType, item.fieldName);
        }

        _sqlSugarClient.Ado.ExecuteCommand(strSql.ToString());
    }

    /// <summary>
    /// sqlsugar建表.
    /// </summary>
    /// <param name="tableModel">表.</param>
    /// <param name="tableFieldList">字段.</param>
    private void CreateTable(DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        var cloumnList = tableFieldList.Adapt<List<DbColumnInfo>>();
        DelDataLength(cloumnList);
        var isOk = _sqlSugarClient.DbMaintenance.CreateTable(tableModel.table, cloumnList);
        _sqlSugarClient.DbMaintenance.AddTableRemark(tableModel.table, tableModel.tableName);
        //mysql不需要单独添加字段注释
        if (_sqlSugarClient.CurrentConnectionConfig.DbType != SqlSugar.DbType.MySql)
        {
            foreach (var item in cloumnList)
            {
                _sqlSugarClient.DbMaintenance.AddColumnRemark(item.DbColumnName, tableModel.table, item.ColumnDescription);
            }
        }
    }

    /// <summary>
    /// 删除列长度（SqlSugar除了字符串其他不需要类型长度）.
    /// </summary>
    /// <param name="dbColumnInfos"></param>
    private void DelDataLength(List<DbColumnInfo> dbColumnInfos, Dictionary<string, string> dataTypeDic = null)
    {
        foreach (var item in dbColumnInfos)
        {
            if (item.DataType.ToLower() != "varchar" && item.DataType.ToLower() != "nvarchar" && item.DataType.ToLower() != "decimal")
                item.Length = 0;
            if (item.DataType.ToLower() != "varchar" && item.DataType.ToLower() != "nvarchar" && item.DataType.ToLower() != "decimal")
                item.DecimalDigits = 0;
            if (dataTypeDic == null)
            {
                item.DataType = DataTypeConversion(item.DataType.ToLower(), _sqlSugarClient.CurrentConnectionConfig.DbType);
            }
            else
            {
                if (dataTypeDic.ContainsKey(item.DataType.ToLower()))
                {
                    item.DataType = dataTypeDic[item.DataType.ToLower().Replace("(默认)", string.Empty)];
                }
            }
        }
    }

    /// <summary>
    /// 数据库数据类型转换.
    /// </summary>
    /// <param name="dataType">数据类型.</param>
    /// <param name="databaseType">数据库类型</param>
    /// <returns></returns>
    private string DataTypeConversion(string dataType, SqlSugar.DbType databaseType)
    {
        if (databaseType.Equals(SqlSugar.DbType.Oracle))
        {
            switch (dataType)
            {
                case "text":
                    return "CLOB";
                case "decimal":
                    return "DECIMAL(38,38)";
                case "datetime":
                    return "DATE";
                case "bigint":
                    return "NUMBER";
                default:
                    return dataType.ToUpper();
            }
        }
        else if (databaseType.Equals(SqlSugar.DbType.Dm))
        {
            return dataType.ToUpper();
        }
        else if (databaseType.Equals(SqlSugar.DbType.Kdbndp))
        {
            switch (dataType)
            {
                case "int":
                    return "NUMBER";
                case "datetime":
                    return "DATE";
                case "bigint":
                    return "INT8";
                default:
                    return dataType.ToUpper();
            }
        }
        else if (databaseType.Equals(SqlSugar.DbType.PostgreSQL))
        {
            switch (dataType)
            {
                case "varchar":
                    return "varchar";
                case "int":
                    return "INT4";
                case "datetime":
                    return "timestamp";
                case "decimal":
                    return "DECIMAL";
                case "bigint":
                    return "INT8";
                case "text":
                    return "TEXT";
                default:
                    return dataType;
            }
        }
        else
        {
            return dataType;
        }
    }

    /// <summary>
    /// 获取全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private GlobalTenantCacheModel GetGlobalTenantCache(string tenantId)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        return _cacheManager.Get<List<GlobalTenantCacheModel>>(cacheKey).Find(it => it.TenantId.Equals(tenantId));
    }
}