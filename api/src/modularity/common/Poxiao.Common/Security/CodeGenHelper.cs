using Poxiao.Infrastructure.Extension;

namespace Poxiao.Infrastructure.Security;

/// <summary>
/// 代码生成帮助类.
/// </summary>
[SuppressSniffer]
public static class CodeGenHelper
{
    public static string ConvertDataType(string dataType)
    {
        switch (dataType.ToLower())
        {
            case "text":
            case "longtext":
            case "varchar":
            case "char":
            case "nvarchar":
            case "nchar":
            case "timestamp":
            case "string":
                return "string?";

            case "int":
            case "smallint":
                return "int?";

            case "tinyint":
                return "byte?";

            case "bigint":
            // sqlite数据库
            case "integer":
                return "long";

            case "bit":
                return "bool";

            case "money":
            case "smallmoney":
            case "numeric":
            case "decimal":
                return "decimal?";

            case "real":
                return "Single?";

            case "datetime":
            case "datetime2":
            case "smalldatetime":
            case "date":
                return "DateTime?";

            case "float":
                return "double?";

            case "image":
            case "binary":
            case "varbinary":
                return "byte[]";

            case "uniqueidentifier":
                return "Guid";

            default:
                return "object";
        }
    }

    /// <summary>
    /// 数据类型转显示类型.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string DataTypeToEff(string dataType)
    {
        if (string.IsNullOrEmpty(dataType)) return string.Empty;
        return dataType switch
        {
            "string" => "input",
            "int" => "inputnumber",
            "long" => "input",
            "float" => "input",
            "double" => "input",
            "decimal" => "input",
            "bool" => "switch",
            "Guid" => "input",
            "DateTime" => "datepicker",
            _ => "input",
        };
    }

    /// <summary>
    /// 是否通用字段.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static bool IsCommonColumn(string columnName)
    {
        var columnList = new List<string>()
        {
            "CreatedTime", "UpdatedTime", "CreatedUserId", "CreatedUserName", "UpdatedUserId", "UpdatedUserName", "IsDeleted"
        };
        return columnList.Contains(columnName);
    }

    /// <summary>
    /// 数据列表生成分组表格.
    /// </summary>
    /// <param name="realList">数据列表.</param>
    /// <param name="groupField">分组字段名.</param>
    /// <param name="groupShowField">分组显示字段名.</param>
    /// <returns></returns>
    public static List<Dictionary<string, object>> GetGroupList(List<Dictionary<string, object>> realList, string groupField, string groupShowField)
    {
        if (realList.Any())
        {
            var fList = realList.FirstOrDefault().Select(x => x.Key).ToList();
            var prop = fList.Where(x => x.Equals(groupShowField)).FirstOrDefault();

            // 分组数据
            Dictionary<string, List<Dictionary<string, object>>> groupDic = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var item in realList)
            {
                if (item.ContainsKey(groupField))
                {
                    var groupDicKey = item[groupField] is null ? string.Empty : item[groupField].ToString();
                    if (!groupDic.ContainsKey(groupDicKey)) groupDic.Add(groupDicKey, new List<Dictionary<string, object>>()); // 初始化
                    item.Remove(groupField);
                    groupDic[groupDicKey].Add(item);
                }
                else
                {
                    var groupDicKey = "null";
                    if (!groupDic.ContainsKey(groupDicKey)) groupDic.Add(groupDicKey, new List<Dictionary<string, object>>()); // 初始化
                    groupDic[groupDicKey].Add(item);
                }
            }

            List<Dictionary<string, object>> realGroupDic = new List<Dictionary<string, object>>();
            foreach (var item in groupDic)
            {
                Dictionary<string, object> dataMap = new Dictionary<string, object>();
                dataMap.Add("top", true);
                dataMap.Add("id", SnowflakeIdHelper.NextId());
                dataMap.Add("children", item.Value);
                if (!string.IsNullOrWhiteSpace(prop)) dataMap[prop] = item.Key;
                else dataMap.Add(groupField, item.Key);
                realGroupDic.Add(dataMap);
            }

            return realGroupDic;
        }
        else
        {
            return new List<Dictionary<string, object>>();
        }
    }

    /// <summary>
    /// 数据列表生成树形表格.
    /// </summary>
    /// <param name="realList">数据列表.</param>
    /// <param name="parentField">树形父级字段.</param>
    /// <param name="treeShowField">树形显示字段.</param>
    /// <returns></returns>
    public static List<Dictionary<string, object>> GetTreeList(List<Dictionary<string, object>> realList, string parentField, string treeShowField)
    {
        var res = new List<Dictionary<string, object>>();
        if (realList.Any())
        {
            var parentFieldId = SnowflakeIdHelper.NextId();

            foreach (var item in realList)
            {
                if (realList.Any(x => x["id"].Equals(item[parentField]))) item[parentFieldId] = item[parentField];
                else item[parentFieldId] = null;
                item[parentField] = realList.Find(x => x["id"] == item["id"])[treeShowField];
            }
            var parentFieldRep = parentField.Substring(0, parentField.Length - 4);
            for (int i = 0; i < realList.Count; i++)
            {
                if (realList[i][parentFieldId].IsNullOrEmpty())
                {
                    if (realList[i][parentFieldRep] == null) realList[i][parentFieldRep] = realList[i][treeShowField];
                    var childList = realList.Where(x => x[parentFieldId] != null && x[parentFieldId].Equals(realList[i]["id"])).ToList();
                    if (childList.Any()) GetTreeList(realList, realList[i], parentFieldId);
                    res.Add(realList[i]);
                }
            }
        }
        return res;
    }

    private static void GetTreeList(List<Dictionary<string, object>> allList, Dictionary<string, object> currentItem, string pId)
    {
        var childList = allList.Where(x => x[pId] != null && x[pId].Equals(currentItem["id"])).ToList();
        if (childList.Any()) childList.ForEach(x => GetTreeList(allList, x, pId));
        if (childList.Any())
        {
            var item = allList.Find(x => x["id"].Equals(currentItem["id"]));
            item["children"] = childList;
        }
    }

    /// <summary>
    /// 根据集合捞取所有子集id.
    /// </summary>
    /// <param name="allList">key : 主键Id , value ： 父亲Id.</param>
    /// <param name="currentId">当前id.</param>
    /// <param name="resList">res.</param>
    public static List<string> GetChildIdList(Dictionary<string, string> allList, string currentId, List<string> resList)
    {
        if (resList == null) resList = new List<string>() { currentId };
        else resList.Add(currentId);
        if (allList.Any())
        {
            var cItemList = allList.Where(x => x.Value.IsNotEmptyOrNull() && x.Value.Equals(currentId)).ToList();
            if (cItemList.Any())
            {
                foreach (var item in cItemList)
                {
                    var cIdList = GetChildIdList(allList, item.Key, resList);
                    resList.Add(item.Key);
                    if (cIdList.Any()) resList.AddRange(cIdList);
                }
            }
        }

        return resList;
    }

    /// <summary>
    /// 获取排序真实字段.
    /// </summary>
    /// <param name="sort">排序字段.</param>
    /// <param name="replaceContent">取代内容.</param>
    /// <param name="entityInfo">实体信息.</param>
    /// <param name="tableType">表类型 0-主表,1-子表,2-副表.</param>
    /// <returns></returns>
    public static string GetSortRealField(string sort, string replaceContent, EntityInfo entityInfo, int tableType)
    {
        var field = string.Empty;
        switch (tableType)
        {
            case 1:
                if (sort.Contains(replaceContent))
                {
                    field = entityInfo.Columns.Find(it => it.PropertyName.Equals(sort.Replace(replaceContent, "").ToUpperCase()))?.DbColumnName;
                }
                break;
            case 2:
                if (sort.Contains("_poxiao_"))
                {
                    var queryField = sort.Replace("_poxiao_", "@").Split('@')[1];
                    field = entityInfo.Columns.Find(it => it.PropertyName.Equals(queryField.ToUpperCase()))?.DbColumnName;
                }
                break;
            default:
                field = entityInfo.Columns.Find(it => it.PropertyName.Equals(sort.ToUpperCase()))?.DbColumnName;
                break;
        }
        return string.IsNullOrEmpty(field) ? null : field;
    }

    /// <summary>
    /// 代码生成导出模板.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="multiple">是否多选.</param>
    /// <param name="label">标题.</param>
    /// <param name="format">时间格式化.</param>
    /// <param name="level">等级.</param>
    /// <returns></returns>
    public static Dictionary<string, string> CodeGenTemplate(string poxiaoKey, bool multiple, string label, string format, int level)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.CREATEUSER:
            case PoxiaoKeyConst.MODIFYUSER:
            case PoxiaoKeyConst.CREATETIME:
            case PoxiaoKeyConst.MODIFYTIME:
            case PoxiaoKeyConst.CURRORGANIZE:
            case PoxiaoKeyConst.CURRPOSITION:
            case PoxiaoKeyConst.CURRDEPT:
            case PoxiaoKeyConst.BILLRULE:
                result.Add(label, "系统自动生成");
                break;
            case PoxiaoKeyConst.COMSELECT:
                result.Add(label, multiple ? "例:XX集团/产品部,XX集团/技术部" : "例:XX集团/技术部");
                break;
            case PoxiaoKeyConst.DEPSELECT:
                result.Add(label, multiple ? "例:产品部/部门编码,技术部/部门编码" : "例:技术部/部门编码");
                break;
            case PoxiaoKeyConst.POSSELECT:
                result.Add(label, multiple ? "例:技术经理/岗位编码,技术员/岗位编码" : "例:技术员/岗位编码");
                break;
            case PoxiaoKeyConst.USERSELECT:
                result.Add(label, multiple ? "例:张三/账号,李四/账号" : "例:张三/账号");
                break;
            case PoxiaoKeyConst.USERSSELECT:
                result.Add(label, multiple ? "例:XX集团/产品部,产品部/部门编码,技术经理/岗位编码,研发人员/角色编码,A分组/分组编码,张三/账号" : "例:李四/账号");
                break;
            case PoxiaoKeyConst.ROLESELECT:
                result.Add(label, multiple ? "例:研发人员/角色编码,测试人员/角色编码" : "例:研发人员/角色编码");
                break;
            case PoxiaoKeyConst.GROUPSELECT:
                result.Add(label, multiple ? "例:A分组/分组编码,B分组/分组编码" : "例:A分组/分组编码");
                break;
            case PoxiaoKeyConst.DATE:
                result.Add(label, string.Format("例:{0}", format));
                break;
            case PoxiaoKeyConst.TIME:
                result.Add(label, string.Format("例:{0}", format));
                break;
            case PoxiaoKeyConst.ADDRESS:
                switch (level)
                {
                    case 0:
                        result.Add(label, multiple ? "例:福建省,广东省" : "例:福建省");
                        break;
                    case 1:
                        result.Add(label, multiple ? "例:福建省/莆田市,广东省/广州市" : "例:福建省/莆田市");
                        break;
                    case 2:
                        result.Add(label, multiple ? "例:福建省/莆田市/城厢区,广东省/广州市/荔湾区" : "例:福建省/莆田市/城厢区");
                        break;
                    case 3:
                        result.Add(label, multiple ? "例:福建省/莆田市/城厢区/霞林街道,广东省/广州市/荔湾区/沙面街道" : "例:福建省/莆田市/城厢区/霞林街道");
                        break;
                }
                break;
            default:
                result.Add(label, string.Empty);
                break;
        }

        return result;
    }
}