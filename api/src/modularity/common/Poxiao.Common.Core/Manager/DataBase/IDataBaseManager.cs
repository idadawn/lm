using Poxiao.Infrastructure.Dtos.DataBase;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Models.VisualDev;
using Poxiao.Systems.Entitys.Dto.Database;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Entitys.Dto.VisualDevModelData;
using SqlSugar;
using System.Data;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// 切换数据库抽象.
/// </summary>
public interface IDataBaseManager
{
    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="tenantName">租户数据库.</param>
    /// <returns>租户的DBLink实体对象.</returns>
    DbLinkEntity GetTenantDbLink(string tenantId, string tenantName);

    /// <summary>
    /// 获取租户SqlSugarClient.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    ISqlSugarClient GetTenantSqlSugarClient(string tenantId);

    /// <summary>
    /// 数据库切换.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns>切库后的SqlSugarClient.</returns>
    SqlSugarScope ChangeDataBase(DbLinkEntity link);

    /// <summary>
    /// 执行Sql(查询).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns></returns>
    Task<int> ExecuteSql(DbLinkEntity link, string strSql);

    /// <summary>
    /// 条件动态过滤.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns>条件是否成立.</returns>
    bool WhereDynamicFilter(DbLinkEntity link, string strSql);

    /// <summary>
    /// 执行Sql(新增、修改).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="dicList">数据.</param>
    /// <param name="primaryField">主键字段.</param>
    /// <returns></returns>
    Task<int> ExecuteSql(DbLinkEntity link, string table, List<Dictionary<string, object>> dicList, string primaryField = "");

    /// <summary>
    /// 执行Sql 新增 并返回自增长Id.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="dicList">数据.</param>
    /// <param name="primaryField">主键字段.</param>
    /// <returns></returns>
    Task<int> ExecuteReturnIdentityAsync(DbLinkEntity link, string table, List<Dictionary<string, object>> dicList, string primaryField = "");

    /// <summary>
    /// 创建表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    Task<bool> Create(DbLinkEntity link, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList);

    /// <summary>
    /// sqlsugar添加表字段.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="tableFieldList">表字段集合.</param>
    void AddTableColumn(DbLinkEntity link, string tableName, List<DbTableFieldModel> tableFieldList);

    /// <summary>
    /// 删除表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    bool Delete(DbLinkEntity link, string table);

    /// <summary>
    /// 修改表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="oldTable">旧表名称.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    Task<bool> Update(DbLinkEntity link, string oldTable, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList);

    /// <summary>
    /// 根据链接获取分页数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">Sql语句.</param>
    /// <param name="pageInput">页数.</param>
    /// <param name="columnDesign">列配置.</param>
    /// <param name="dataPermissions">数据权限.</param>
    /// <param name="outColumnName">输出列名称.</param>
    /// <returns></returns>
    PageResult<Dictionary<string, object>> GetInterFaceData(DbLinkEntity link, string strSql, VisualDevModelListQueryInput pageInput, MainBeltViceQueryModel columnDesign, List<IConditionalModel> dataPermissions, Dictionary<string, string> outColumnName = null);

    /// <summary>
    /// 表是否存在.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    bool IsAnyTable(DbLinkEntity link, string table);

    /// <summary>
    /// 表是否存在数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    bool IsAnyData(DbLinkEntity link, string table);

    /// <summary>
    /// 表字段是否存在.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="column">表字段名.</param>
    /// <returns></returns>
    bool IsAnyColumn(DbLinkEntity link, string table, string column);

    /// <summary>
    /// 获取表字段列表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    List<DbTableFieldModel> GetFieldList(DbLinkEntity? link, string? tableName);

    /// <summary>
    /// 获取表数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    DataTable GetData(DbLinkEntity link, string tableName);

    /// <summary>
    /// 根据链接获取数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">Sql语句.</param>
    /// <param name="parameters">参数.</param>
    /// <returns></returns>
    DataTable GetInterFaceData(DbLinkEntity link, string strSql, params SugarParameter[] parameters);

    /// <summary>
    /// 根据链接获取数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    /// <returns></returns>
    DataTable GetInterFaceDataNew(DbLinkEntity link, string strSql, params SugarParameter[] parameters);

    /// <summary>
    /// 执行统计sql.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    int GetCount(DbLinkEntity link, string strSql, params SugarParameter[] parameters);

    /// <summary>
    /// 获取表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    DatabaseTableInfoOutput GetDataBaseTableInfo(DbLinkEntity link, string tableName);

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    List<DatabaseTableListOutput> GetDBTableList(DbLinkEntity link);

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="isView">视图.</param>
    /// <returns></returns>
    List<DbTableInfo> GetTableInfos(DbLinkEntity link, bool isView = false);

    /// <summary>
    /// 获取数据表(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <returns></returns>
    Task<List<string>> GetListStringAsync(DbLinkEntity link, string dbSql);

    /// <summary>
    /// 获取单个值.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <returns></returns>
    Task<string> GetStringAsync(DbLinkEntity link, string dbSql);

    /// <summary>
    /// 获取数据表(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <returns></returns>
    Task<List<TEntity>> GetListAsync<TEntity>(DbLinkEntity link, string dbSql);

    /// <summary>
    /// 获取数据表分页(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    Task<dynamic> GetDataTablePage(DbLinkEntity link, string dbSql, int pageIndex, int pageSize);

    /// <summary>
    /// 获取数据表分页(实体).
    /// </summary>
    /// <typeparam name="TEntity">T.</typeparam>
    /// <param name="link">数据连接.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    Task<List<TEntity>> GetDataTablePage<TEntity>(DbLinkEntity link, int pageIndex, int pageSize);

    /// <summary>
    /// 使用存储过程.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="stored">存储过程名称.</param>
    /// <param name="parameters">参数.</param>
    void UseStoredProcedure(DbLinkEntity link, string stored, List<SugarParameter> parameters);

    /// <summary>
    /// 测试数据库连接.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    bool IsConnection(DbLinkEntity link);

    /// <summary>
    /// 同步数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dt">同步数据.</param>
    /// <param name="table">表.</param>
    /// <returns></returns>
    Task<bool> SyncData(DbLinkEntity link, DataTable dt, string table);

    /// <summary>
    /// 同步表操作.
    /// </summary>
    /// <param name="linkFrom">原数据库.</param>
    /// <param name="linkTo">目前数据库.</param>
    /// <param name="table">表名称.</param>
    /// <param name="type">操作类型.</param>
    /// <param name="fieldType">数据类型.</param>
    void SyncTable(DbLinkEntity linkFrom, DbLinkEntity linkTo, string table, int type, Dictionary<string, string> fieldType);

    /// <summary>
    /// 视图数据类型转换.
    /// </summary>
    /// <param name="fields">字段数据.</param>
    /// <param name="databaseType">数据库类型.</param>
    List<TableFieldOutput> ViewDataTypeConversion(List<TableFieldOutput> fields, SqlSugar.DbType databaseType);

    /// <summary>
    /// 转换数据库类型.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    SqlSugar.DbType ToDbType(string dbType);

    /// <summary>
    /// 转换连接字符串.
    /// </summary>
    /// <param name="dbLinkEntity">数据连接.</param>
    /// <returns></returns>
    string ToConnectionString(DbLinkEntity dbLinkEntity);

    /// <summary>
    /// 执行增删改sql.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    void ExecuteCommand(DbLinkEntity link, string strSql, params SugarParameter[] parameters);
}