using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Models;

namespace Poxiao.Infrastructure.Security;

/// <summary>
/// 高级查询帮助类.
/// </summary>
public class SuperQueryHelper
{
    /// <summary>
    /// 组装高级查询信息.
    /// </summary>
    /// <param name="superQueryJson">查询条件json.</param>
    /// <param name="replaceContent">取代内容.</param>
    /// <param name="entityInfo">实体信息.</param>
    /// <param name="tableType">表类型 0-主表,1-子表,2-副表.</param>
    public static List<ConvertSuperQuery> GetSuperQueryInput(string superQueryJson, string replaceContent, EntityInfo entityInfo, int tableType)
    {
        SuperQueryModel result = new SuperQueryModel();
        SuperQueryModel? model = string.IsNullOrEmpty(superQueryJson) ? null : superQueryJson.ToObject<SuperQueryModel>();
        var queryList = new List<ConvertSuperQuery>();

        if (model != null)
        {
            var matchLogic = model.matchLogic;
            var whereType = matchLogic.Equals("AND") ? WhereType.And : WhereType.Or;
            foreach (var item in model.conditionJson)
            {
                var field = string.Empty;
                switch (tableType)
                {
                    case 1:
                        if (item.field.Contains(replaceContent))
                        {
                            field = entityInfo.Columns.Find(it => it.PropertyName.Equals(item.field.Replace(replaceContent, "").ToUpperCase()))?.DbColumnName;
                        }
                        break;
                    case 2:
                        if (item.field.Contains(replaceContent) && item.field.Contains("_poxiao_"))
                        {
                            var queryField = item.field.Replace("_poxiao_", "@").Split('@')[1];
                            field = entityInfo.Columns.Find(it => it.PropertyName.Equals(queryField.ToUpperCase()))?.DbColumnName;
                        }
                        break;
                    default:
                        field = entityInfo.Columns.Find(it => it.PropertyName.Equals(item.field.ToUpperCase()))?.DbColumnName;
                        break;
                }

                if (string.IsNullOrEmpty(field))
                {
                    continue;
                }

                var conditionalType = ConditionalType.IsNullOrEmpty;
                switch (item.poxiaoKey)
                {
                    case PoxiaoKeyConst.COMINPUT:
                    case PoxiaoKeyConst.TEXTAREA:
                    case PoxiaoKeyConst.CHECKBOX:
                    case PoxiaoKeyConst.SELECT:
                    case PoxiaoKeyConst.TIME:
                    case PoxiaoKeyConst.DEPSELECT:
                    case PoxiaoKeyConst.GROUPSELECT:
                    case PoxiaoKeyConst.POSSELECT:
                    case PoxiaoKeyConst.USERSELECT:
                    case PoxiaoKeyConst.ROLESELECT:
                    case PoxiaoKeyConst.TREESELECT:
                    case PoxiaoKeyConst.RELATIONFORM:
                    case PoxiaoKeyConst.RELATIONFORMATTR:
                    case PoxiaoKeyConst.POPUPSELECT:
                    case PoxiaoKeyConst.POPUPATTR:
                    case PoxiaoKeyConst.CALCULATE:
                    case PoxiaoKeyConst.CREATEUSER:
                    case PoxiaoKeyConst.MODIFYUSER:
                        item.fieldValue = item.fieldValue?.ToString().Replace("\r\n", string.Empty);
                        switch (item.symbol)
                        {
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Like;
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoLike;
                                break;
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, item.fieldValue.ParseToString(), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                    case PoxiaoKeyConst.DATE:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoEqual;
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, item.fieldValue.ToObject<DateTime>().ToString(), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                    case PoxiaoKeyConst.CREATETIME:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoEqual;
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, string.Format("{0:yyyy-MM-dd HH:mm:ss}", item.fieldValue.ToObject<DateTime>()), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                    case PoxiaoKeyConst.MODIFYTIME:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoEqual;
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, string.Format("{0:yyyy-MM-dd HH:mm:ss}", item.fieldValue.ToObject<DateTime>()), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                    case PoxiaoKeyConst.NUMINPUT:
                    case PoxiaoKeyConst.SWITCH:
                    case PoxiaoKeyConst.RADIO:
                    case PoxiaoKeyConst.POPUPTABLESELECT:
                    case PoxiaoKeyConst.COMSELECT:
                    case PoxiaoKeyConst.BILLRULE:
                    case PoxiaoKeyConst.CURRDEPT:
                    case PoxiaoKeyConst.CURRPOSITION:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Like;
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoLike;
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, item.fieldValue.ParseToString(), conditionalType, true));
                        break;
                    case PoxiaoKeyConst.USERSSELECT:
                        if (item.symbol.Equals("==")) item.symbol = "like";
                        if (item.symbol.Equals("<>")) item.symbol = "notLike";
                        if (item.fieldValue != null && (item.symbol.Equals("like") || item.symbol.Equals("notLike")))
                        {
                            var rIdList = GetUserRelationByUserId(item.fieldValue.ToString());
                            var objIdList = new List<string>() { item.fieldValue.ToString() };
                            rIdList.ForEach(x =>
                            {
                                if (x["OBJECTTYPE"].Equals("Organize"))
                                {
                                    objIdList.Add(x["OBJECTID"] + "--company");
                                    objIdList.Add(x["OBJECTID"] + "--department");
                                }
                                else
                                {
                                    objIdList.Add(x["OBJECTID"] + "--" + x["OBJECTTYPE"].ToLower());
                                }
                            });

                            var whereList = new List<KeyValuePair<WhereType, ConditionalModel>>();
                            for (var i = 0; i < objIdList.Count(); i++)
                            {
                                if (i == 0)
                                {
                                    var queryOr = new ConvertSuperQuery();
                                    queryOr.whereType = WhereType.And;
                                    queryOr.poxiaoKey = item.poxiaoKey;
                                    queryOr.field = field;
                                    queryOr.fieldValue = objIdList[i];
                                    queryOr.conditionalType = item.symbol.Equals("like") ? ConditionalType.Like : ConditionalType.NoLike;
                                    queryOr.mainWhere = true;
                                    queryList.Add(queryOr);
                                }
                                else
                                {
                                    var queryOr = new ConvertSuperQuery();
                                    queryOr.whereType = item.symbol.Equals("like") ? WhereType.Or : WhereType.And;
                                    queryOr.poxiaoKey = item.poxiaoKey;
                                    queryOr.field = field;
                                    queryOr.fieldValue = objIdList[i];
                                    queryOr.conditionalType = item.symbol.Equals("like") ? ConditionalType.Like : ConditionalType.NoLike;
                                    queryOr.mainWhere = true;
                                    queryList.Add(queryOr);
                                }
                            }

                            if (item.symbol.Equals("notLike"))
                            {
                                var queryOr = new ConvertSuperQuery();
                                queryOr.whereType = WhereType.Or;
                                queryOr.poxiaoKey = item.poxiaoKey;
                                queryOr.field = field;
                                queryOr.fieldValue = null;
                                queryOr.conditionalType = ConditionalType.EqualNull;
                                queryOr.mainWhere = true;
                                queryList.Add(queryOr);
                            }
                        }
                        continue;
                    case PoxiaoKeyConst.ADDRESS:
                    case PoxiaoKeyConst.CASCADER:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().ToJsonString();
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().ToJsonString();
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Like;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoLike;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, item.fieldValue.ParseToString(), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                    case PoxiaoKeyConst.CURRORGANIZE:
                        switch (item.symbol)
                        {
                            case ">=": // 大于等于
                                conditionalType = ConditionalType.GreaterThanOrEqual;
                                break;
                            case ">": // 大于
                                conditionalType = ConditionalType.GreaterThan;
                                break;
                            case "==": // 等于
                                conditionalType = ConditionalType.Equal;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                            case "<=": // 小于等于
                                conditionalType = ConditionalType.LessThanOrEqual;
                                break;
                            case "<": // 小于
                                conditionalType = ConditionalType.LessThan;
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.NoEqual;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                            case "like": // 包含
                                conditionalType = ConditionalType.Equal;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.NoEqual;
                                item.fieldValue = item.fieldValue.ToObject<List<string>>().Last();
                                break;
                        }
                        queryList.Add(ControlAdvancedQueryAssembly(whereType, item.poxiaoKey, field, item.fieldValue.ParseToString(), conditionalType, true));
                        switch (item.symbol)
                        {
                            case "notLike": // 不包含
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                            case "<>": // 不等于
                                conditionalType = ConditionalType.IsNullOrEmpty;
                                queryList.Add(ControlAdvancedQueryAssembly(WhereType.Or, item.poxiaoKey, field, null, conditionalType));
                                break;
                        }
                        break;
                }
            }
        }
        return queryList;
    }

    public static ConvertSuperQuery ControlAdvancedQueryAssembly(WhereType whereType, string poxiaoKey, string field, string fieldValue, ConditionalType conditionalType, bool mainWhere = false, string symbol = "")
    {
        return new ConvertSuperQuery
        {
            whereType = whereType,
            poxiaoKey = poxiaoKey,
            field = field,
            fieldValue = fieldValue,
            conditionalType = conditionalType,
            mainWhere = mainWhere,
            symbol = symbol
        };
    }

    /// <summary>
    /// 组装高级查询条件.
    /// </summary>
    /// <returns></returns>
    public static List<IConditionalModel> GetSuperQueryJson(List<ConvertSuperQuery> list)
    {
        List<IConditionalModel> conModels = new List<IConditionalModel>();

        list.FindAll(it => it.mainWhere).ForEach(item =>
        {
            ConditionalCollections conditional = new ConditionalCollections();
            conditional.ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>();
            var sameList = list.FindAll(it => it.field.Equals(item.field));
            sameList.ForEach(items =>
            {
                conditional.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(items.whereType, new ConditionalModel
                {
                    FieldName = items.field,
                    ConditionalType = items.conditionalType,
                    FieldValue = !string.IsNullOrEmpty(items.fieldValue) ? items.fieldValue : null
                }));
            });
            conModels.Add(conditional);
        });
        return conModels;
    }

    /// <summary>
    /// 根据用户Id,获取用户关系id集合.
    /// </summary>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    private static List<Dictionary<string, string>> GetUserRelationByUserId(string fieldValue)
    {
        // 获取数据库连接选项
        ConnectionStringsOptions connectionStrings = App.GetOptions<ConnectionStringsOptions>();
        var defaultConnection = connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        SqlSugarClient db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = string.Format(defaultConnection.DefaultConnection, defaultConnection.DBName),
            DbType = defaultConnection.DbType,
            IsAutoCloseConnection = true,
            ConfigId = defaultConnection.ConfigId,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings()
            {
                IsAutoRemoveDataCache = true // 自动清理缓存
            }
        });

        var sql = string.Format("SELECT F_OBJECTID OBJECTID,F_OBJECTTYPE OBJECTTYPE FROM BASE_USERRELATION WHERE F_USERID='{0}'", fieldValue.ToString().Replace("--user", string.Empty));
        var res = db.SqlQueryable<object>(sql).ToDataTable();
        return res.ToObject<List<Dictionary<string, string>>>();
    }
}