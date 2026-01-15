using System.Reflection;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导入导出服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-01-05.
/// </summary>
public class MetricDataIEService : IMetricDataIEService, ITransient
{
    /// <summary>
    /// 仓库.
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 类型转换.
    /// </summary>
    private class TypesInfo
    {
        /// <summary>
        /// 给用户显示的类型名字.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 数据库字段类型.
        /// </summary>
        public string DbType { get; set; } = string.Empty;

        /// <summary>
        /// 可被type转换的类型.
        /// </summary>
        public string CodeType { get; set; } = string.Empty;

        /// <summary>
        /// 类型长度.
        /// </summary>
        public int? Len { get; set; }
    }

    /// <summary>
    /// 创建表时可接受的字段信息.
    /// </summary>
    private readonly List<TypesInfo> _fieldInfo = new() { new() { Name = "整数", DbType = "int", CodeType = "System.Int32" }, new() { Name = "长整数", DbType = "bigint", CodeType = "System.Int64" }, new() { Name = "浮点数", DbType = "double", CodeType = "System.Double" }, new() { Name = "字符串", DbType = "varchar", CodeType = "System.String", Len = 4000 }, new() { Name = "布尔", DbType = "tinyint", CodeType = "System.Boolean" }, new() { Name = "时间", DbType = "datetime", CodeType = "System.DateTime" } };

    public MetricDataIEService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<(bool IsOK, string Msg)> CreateDBTable(MetricDataIECreateTableInput input)
    {
        // 查看数据库是否存在指定的表名
        if (await _db.Queryable<MetricDataIETableCollectionEntity>().FirstAsync(it => it.TableName == input.TableName) is not null) return (false, $"'{input.TableName}'表已存在");

        (bool isOk, string msg) = FieldRule(input.FieldInfos);
        if (!isOk) return (false, msg);

        DynamicProperyBuilder typeBilder;
        typeBilder = _db.DynamicBuilder().CreateClass(input.TableName, new SugarTable());

        // 实体类添加字段信息
        foreach (var field in input.FieldInfos)
        {
            var sugarColumn = new SugarColumn();

            TypesInfo? fieldInfo = _fieldInfo.FirstOrDefault(it => it.Name == field?.Type);

            if (fieldInfo?.Len is not null) sugarColumn.Length = field.Len ?? 0;
            sugarColumn.IsNullable = field.IsNull;
            sugarColumn.IsIdentity = field.IsIncrement;
            sugarColumn.IsPrimaryKey = field.IsPrimaryKey;
            if (field.IsPrimaryKey && field.IsIncrement) typeBilder.CreateProperty(field.FieldName, Type.GetType("System.Int64"), sugarColumn);
            else if (field.IsPrimaryKey && !field.IsIncrement) typeBilder.CreateProperty(field.FieldName, Type.GetType("System.String"), sugarColumn);
            else typeBilder.CreateProperty(field.FieldName, Type.GetType(fieldInfo.CodeType), sugarColumn);
        }
        typeBilder.WithCache();
        Type table = typeBilder.BuilderType();
        try
        {
            _db.CodeFirst.InitTables(table); // 创建表
        }
        catch (Exception e)
        {

            throw;
        }


        var insertObj = new MetricDataIETableCollectionEntity
        {
            TableName = input.TableName
        };
        await _db.Insertable(insertObj).CallEntityMethod(c => c.Create()).ExecuteCommandAsync(); // 添加创建表的记录

        if (input.Data is not null)
        {
            return await InsertData(input.TableName, input.Data);
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// 查看每个字段是否符合规则.
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    private (bool IsOK, string Msg) FieldRule(List<FieldInfo> fields)
    {
        foreach (FieldInfo item in fields)
        {
            if (item.FieldName.IsNullOrEmpty()) return (false, "字段名不能为空");
            if (item.Type.IsNullOrEmpty() && !item.IsPrimaryKey && !item.IsIncrement) return (false, $"'{item.FieldName}'字段类型不能为空");
            var fieldInfo = _fieldInfo.Find(it => it.Name == item.Type);
            if (fieldInfo is null && !item.IsPrimaryKey && !item.IsIncrement) return (false, $"'{item.FieldName}'字段找不到对应的类型");
            if (fieldInfo?.Len is not null)
            {
                if (item.Len is null) return (false, $"'{item.FieldName}'字段类型长度不能为空");
                if (item.Len > fieldInfo.Len) return (false, $"'{item.FieldName}'字段类型长度不能大于最大值{fieldInfo.Len}");
                if (item.Len < 1) return (false, $"'{item.FieldName}'字段类型长度不能小于1");
            }
        }
        if (fields.FirstOrDefault(it => it.IsPrimaryKey) is null) return (false, "需要指定主键");
        return (true, string.Empty);
    }

    public MetricDataIECreateTemplateOutput GetCreateTemplate()
    {
        var resule = new MetricDataIECreateTemplateOutput();
        foreach (var item in _fieldInfo)
        {
            resule.Fields.Add(new FieldInfoBase() { Type = item.Name, Len = item.Len });
        }

        return resule;
    }

    public async Task<(MetricDataIEInsertTemplateOutput Output, bool IsOK, string Msg)> GetInsertTemplate(string tableName)
    {
        // 查看数据库是否存在指定的表名
        if (await _db.Queryable<MetricDataIETableCollectionEntity>().FirstAsync(it => it.TableName == tableName) is null) return (new(), false, $"'{tableName}'表不存在");

        var output = new MetricDataIEInsertTemplateOutput();
        output.TableName = tableName;
        output.Fields = new();

        // 获取表中字段信息
        var fieldInfoList = _db.DbMaintenance.GetColumnInfosByTableName(tableName);
        foreach (var item in fieldInfoList)
        {

            var field = new FieldInfo();
            field.IsNull = item.IsNullable;
            field.FieldName = item.DbColumnName;
            field.IsPrimaryKey = item.IsPrimarykey;
            field.IsIncrement = item.IsIdentity;
            if (item.Length != 0) field.Len = item.Length;
            var t = _fieldInfo.Find(it => it.DbType == item.DataType);
            if (t is not null)
            {
                field.Type = t.Name;
            }

            output.Fields.Add(field);
        }

        return (output, true, string.Empty);
    }

    public async Task<(bool IsOK, string Msg)> InsertDBTable(MetricDataIEInsertTableInput input)
    {
        if (input.Data.Count < 1) return (false, "没有可添加的数据");

        // 查看数据库是否存在指定的表名
        if (await _db.Queryable<MetricDataIETableCollectionEntity>().FirstAsync(it => it.TableName == input.TableName) is null) return (false, $"'{input.TableName}'表不存在");
        return await InsertData(input.TableName, input.Data);
    }

    /// <summary>
    /// 向数据表中添加数据.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="data">数据.</param>
    /// <returns></returns>
    private async Task<(bool IsOK, string Msg)> InsertData(string tableName, List<Dictionary<string, object>> data)
    {
        // 获取表中字段信息
        var fieldInfoList = _db.DbMaintenance.GetColumnInfosByTableName(tableName);

        DynamicProperyBuilder typeBilder;
        typeBilder = _db.DynamicBuilder().CreateClass(tableName, new SugarTable());

        // 实体类添加字段信息
        foreach (var item in fieldInfoList)
        {

            var typeInfo = _fieldInfo.FirstOrDefault(it => it.DbType == item.DataType); // 在_fieldInfo中是否保存了数据库字段类型
            var sc = new SugarColumn();
            sc.IsNullable = item.IsNullable;
            sc.IsIdentity = item.IsIdentity;
            sc.IsPrimaryKey = item.IsPrimarykey;
            if (typeInfo is null)
            {
                typeBilder.CreateProperty(item.DbColumnName, typeof(object), sc);
            }
            else
            {
                if (typeInfo.Len is not null) sc.Length = item.Length;
                Type t = Type.GetType(typeInfo.CodeType)!;
                if (sc.IsNullable && t.IsValueType)
                {
                    typeBilder.CreateProperty(item.DbColumnName, typeof(Nullable<>).MakeGenericType(t), sc); // 可空的值类型
                }
                else
                {
                    typeBilder.CreateProperty(item.DbColumnName, t, sc);
                }
            }
        }

        typeBilder.WithCache();
        var table = typeBilder.BuilderType();
        var list = new List<object>();
        for (var i = 0; i < data.Count; i++) // 将字典数据转换填充,因db.DynamicBuilder().CreateObjectByType()不能追踪报错位置，故重新实现了该方法
        {
            var obj = Activator.CreateInstance(table)!;

            foreach (KeyValuePair<string, object> item in data[i])
            {
                PropertyInfo? property = table.GetProperty(item.Key);
                if (property != null)
                {
                    try
                    {
                        property.SetValue(obj, UtilMethods.ChangeType2(item.Value, property.PropertyType));
                    }
                    catch (Exception)
                    {
                        var dbFields = fieldInfoList.FirstOrDefault(i => i.DbColumnName == item.Key);
                        var typeInfo = _fieldInfo.FirstOrDefault(it => it.DbType == dbFields!.DataType);
                        return (false, $"插入数据失败,第{i + 1}行数据的'{item.Key}'字段数据类型有误，应为'{typeInfo!.Name}'类型");
                    }
                }
            }

            // 主键设置雪花id
            var primary = fieldInfoList.FirstOrDefault(it => it.IsPrimarykey && !it.IsIdentity);
            if (primary is not null)
            {
                var pro = table.GetProperty(primary.DbColumnName);
                if(pro?.GetValue(obj) is null) pro?.SetValue(obj, SnowflakeIdHelper.NextId());
            }

            list.Add(obj);
        }

        try
        {
            await _db.InsertableByObject(list).ExecuteCommandAsync(); // 插入数据
        }
        catch (Exception e)
        {
            return (false, $"插入数据失败,{e.Message}");
        }

        return (true, string.Empty);
    }

    public async Task<(MetricDataIEExportOutput Output, bool IsOK, string Msg)> ExportData(MetricDataIEExportInput input)
    {
        // 查看数据库是否存在指定的表名
        if (await _db.Queryable<MetricDataIETableCollectionEntity>().FirstAsync(it => it.TableName == input.TableName) is null) return (new(), false, $"'{input.TableName}'表不存在");

        var len = 0;
        var res = _db.Queryable<object>().AS(input.TableName).ToPageList(input.CurrentPage, input.PageSize, ref len);
        var result = new MetricDataIEExportOutput();
        double page = (double)len / (double)input.PageSize;
        result.TotalCount = (int)Math.Ceiling(page);
        foreach (IDictionary<string, object> item in res)
        {
            result.Data.Add(item.ToDictionary(k => k.Key, y =>
            {
                DateTime dateTime;
                if (y.Value is not null && DateTime.TryParse(y.Value.ToString(), out dateTime))
                {
                    return dateTime.ToString();
                }

                return y.Value;
            }));
        }

        return (result, true, string.Empty);
    }
}
