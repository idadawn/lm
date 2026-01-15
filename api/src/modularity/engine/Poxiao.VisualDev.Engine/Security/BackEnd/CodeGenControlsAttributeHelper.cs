using Aop.Api.Domain;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Models.Authorize;
using Poxiao.Infrastructure.Security;
using Poxiao.VisualDev.Engine.Core;
using Poxiao.VisualDev.Entitys;
using Poxiao.VisualDev.Entitys.Enum;
using SqlSugar;

namespace Poxiao.VisualDev.Engine.Security;

/// <summary>
/// 代码生成控件属性帮助类.
/// </summary>
public class CodeGenControlsAttributeHelper
{
    /// <summary>
    /// 转换静态数据.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<StaticDataModel> ConversionStaticData(string data)
    {
        var list = new List<StaticDataModel>();
        if (!string.IsNullOrEmpty(data))
        {
            var conData = data.ToObject<List<StaticDataModel>>();
            if (conData != null)
            {
                foreach (var item in conData)
                {
                    list.Add(new StaticDataModel()
                    {
                        id = item.id,
                        fullName = item.fullName,
                    });
                    if (item.children != null)
                        list.AddRange(ConversionStaticData(item.children.ToJsonString()));
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 获取当前控件联动配置.
    /// </summary>
    /// <param name="linkageConfigs">联动配置.</param>
    /// <param name="tableRelationship">0-主表,1-副表,2-子表.</param>
    /// <param name="tableName">表名称.</param>
    /// <returns></returns>
    public static List<ControlLinkageParameterModel> ObtainTheCurrentControlLinkageConfiguration(List<LinkageConfig> linkageConfigs, int tableRelationship, string tableName = null)
    {
        var list = new List<ControlLinkageParameterModel>();
        if (linkageConfigs?.Count > 0)
        {
            linkageConfigs.ForEach(item =>
            {
                var formFieldValues = string.Empty;
                var primaryAndSecondaryField = false;
                switch (tableRelationship)
                {
                    case 2:
                        formFieldValues = item.relationField.Replace(string.Format("{0}-", tableName), string.Empty);
                        primaryAndSecondaryField = !item.relationField.Contains(tableName) ? true : false;
                        break;
                    default:
                        formFieldValues = item.relationField;
                        break;
                }

                list.Add(new ControlLinkageParameterModel
                {
                    ParameterName = item.field,
                    FormFieldValues = formFieldValues,
                    IsPrimaryAndSecondaryField = primaryAndSecondaryField,
                });
            });
        }
        return list;
    }

    /// <summary>
    /// 判断控件是否数据转换.
    /// </summary>
    /// <returns></returns>
    public static bool JudgeControlIsDataConversion(string poxiaoKey, string dataType, bool multiple)
    {
        bool tag = false;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.UPLOADFZ:
            case PoxiaoKeyConst.UPLOADIMG:
                tag = true;
                break;
            case PoxiaoKeyConst.SELECT:
            case PoxiaoKeyConst.TREESELECT:
                {
                    switch (dataType)
                    {
                        case "dictionary":
                            if (!multiple)
                            {
                                tag = false;
                            }
                            else
                            {
                                tag = true;
                            }

                            break;
                        default:
                            tag = true;
                            break;
                    }
                }

                break;
            case PoxiaoKeyConst.RADIO:
                {
                    switch (dataType)
                    {
                        case "dictionary":
                            tag = false;
                            break;
                        default:
                            tag = true;
                            break;
                    }
                }

                break;
            case PoxiaoKeyConst.DEPSELECT:
            case PoxiaoKeyConst.POSSELECT:
            case PoxiaoKeyConst.USERSELECT:
            case PoxiaoKeyConst.ROLESELECT:
            case PoxiaoKeyConst.GROUPSELECT:
                {
                    if (!multiple)
                        tag = false;
                    else
                        tag = true;
                }

                break;
            case PoxiaoKeyConst.CHECKBOX:
            case PoxiaoKeyConst.CASCADER:
            case PoxiaoKeyConst.COMSELECT:
            case PoxiaoKeyConst.POPUPTABLESELECT:
            case PoxiaoKeyConst.ADDRESS:
            case PoxiaoKeyConst.USERSSELECT:
                tag = true;
                break;
        }

        return tag;
    }

    /// <summary>
    /// 获取各模式控件是否列表转换.
    /// </summary>
    /// <param name="generatePattern">生成模式.</param>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="dataType">数据类型 dictionary-数据字段,dynamic-远端数据.</param>
    /// <param name="multiple">是否多选.</param>
    /// <param name="subTableListDisplay">子表列表显示.</param>
    /// <returns>true-ThenMapper转换,false-列表转换.</returns>
    public static bool GetWhetherToConvertAllModeControlsIntoLists(GeneratePatterns generatePattern, string poxiaoKey, string dataType, bool multiple, bool subTableListDisplay)
    {
        bool tag = false;

        /*
         * 因ORM原因 导航查询 一对多 列表查询
         * 不能使用ORM 自带函数 待作者开放.Select()
         * 导致一对多列表查询转换必须全使用子查询
         * 远端数据与静态数据无法列表转换所以全部ThenMapper内转换
         * 数据字典又分为两种值转换ID与EnCode
         */
        switch (subTableListDisplay)
        {
            case true:
                switch (generatePattern)
                {
                    case GeneratePatterns.MainBelt:
                    case GeneratePatterns.PrimarySecondary:
                        switch (poxiaoKey)
                        {
                            case PoxiaoKeyConst.CREATEUSER:
                            case PoxiaoKeyConst.MODIFYUSER:
                            case PoxiaoKeyConst.CURRORGANIZE:
                            case PoxiaoKeyConst.CURRPOSITION:
                            case PoxiaoKeyConst.DEPSELECT:
                            case PoxiaoKeyConst.POSSELECT:
                            case PoxiaoKeyConst.USERSELECT:
                            case PoxiaoKeyConst.POPUPTABLESELECT:
                            case PoxiaoKeyConst.ROLESELECT:
                            case PoxiaoKeyConst.GROUPSELECT:
                            case PoxiaoKeyConst.RADIO:
                            case PoxiaoKeyConst.SELECT:
                            case PoxiaoKeyConst.TREESELECT:
                            case PoxiaoKeyConst.CHECKBOX:
                            case PoxiaoKeyConst.CASCADER:
                            case PoxiaoKeyConst.COMSELECT:
                            case PoxiaoKeyConst.ADDRESS:
                            case PoxiaoKeyConst.SWITCH:
                                tag = true;
                                break;
                        }

                        break;
                }
                break;
            default:
                switch (poxiaoKey)
                {
                    case PoxiaoKeyConst.SELECT:
                    case PoxiaoKeyConst.TREESELECT:
                        {
                            switch (dataType)
                            {
                                case "dictionary":
                                    if (multiple)
                                        tag = true;
                                    else
                                        tag = false;
                                    break;
                                default:
                                    tag = true;
                                    break;
                            }
                        }

                        break;
                    case PoxiaoKeyConst.RADIO:
                        {
                            switch (dataType)
                            {
                                case "dictionary":
                                    tag = false;
                                    break;
                                default:
                                    tag = true;
                                    break;
                            }
                        }

                        break;
                    case PoxiaoKeyConst.DEPSELECT:
                    case PoxiaoKeyConst.POSSELECT:
                    case PoxiaoKeyConst.USERSELECT:
                    case PoxiaoKeyConst.ROLESELECT:
                    case PoxiaoKeyConst.GROUPSELECT:
                        {
                            if (multiple)
                                tag = true;
                            else
                                tag = false;
                        }

                        break;
                    case PoxiaoKeyConst.CHECKBOX:
                    case PoxiaoKeyConst.CASCADER:
                    case PoxiaoKeyConst.COMSELECT:
                    case PoxiaoKeyConst.ADDRESS:
                    case PoxiaoKeyConst.POPUPTABLESELECT:
                        tag = true;
                        break;
                }
                break;
        }
        return tag;
    }

    /// <summary>
    /// 判断含子表字段控件是否数据转换.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <returns></returns>
    public static bool JudgeContainsChildTableControlIsDataConversion(string poxiaoKey)
    {
        bool tag = false;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.UPLOADFZ:
            case PoxiaoKeyConst.UPLOADIMG:
            case PoxiaoKeyConst.CREATEUSER:
            case PoxiaoKeyConst.MODIFYUSER:
            case PoxiaoKeyConst.CURRORGANIZE:
            case PoxiaoKeyConst.CURRPOSITION:
            case PoxiaoKeyConst.DEPSELECT:
            case PoxiaoKeyConst.POSSELECT:
            case PoxiaoKeyConst.USERSELECT:
            case PoxiaoKeyConst.USERSSELECT:
            case PoxiaoKeyConst.POPUPTABLESELECT:
            case PoxiaoKeyConst.ROLESELECT:
            case PoxiaoKeyConst.GROUPSELECT:
            case PoxiaoKeyConst.RADIO:
            case PoxiaoKeyConst.SELECT:
            case PoxiaoKeyConst.TREESELECT:
            case PoxiaoKeyConst.CHECKBOX:
            case PoxiaoKeyConst.CASCADER:
            case PoxiaoKeyConst.COMSELECT:
            case PoxiaoKeyConst.ADDRESS:
            case PoxiaoKeyConst.SWITCH:
            case PoxiaoKeyConst.DATE:
                tag = true;
                break;
        }

        return tag;
    }

    /// <summary>
    /// 系统控件不更新.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <returns></returns>
    public static bool JudgeControlIsSystemControls(string poxiaoKey)
    {
        bool tag = true;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.CREATEUSER:
            case PoxiaoKeyConst.CREATETIME:
            case PoxiaoKeyConst.CURRPOSITION:
            case PoxiaoKeyConst.CURRORGANIZE:
            case PoxiaoKeyConst.BILLRULE:
                tag = false;
                break;
        }

        return tag;
    }

    /// <summary>
    /// 获取控件数据来源ID.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="dataType">数据类型.</param>
    /// <param name="control">控件全属性.</param>
    /// <returns></returns>
    public static string GetControlsPropsUrl(string poxiaoKey, string dataType, FieldsModel control)
    {
        string propsUrl = string.Empty;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.POPUPTABLESELECT:
                propsUrl = control.interfaceId;
                break;
            default:
                switch (dataType)
                {
                    case "dictionary":
                        propsUrl = control.__config__.dictionaryType;
                        break;
                    default:
                        propsUrl = control.__config__.propsUrl;
                        break;
                }

                break;
        }

        return propsUrl;
    }

    /// <summary>
    /// 获取控件指定选项的值.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="dataType">数据类型.</param>
    /// <param name="control">控件全属性.</param>
    /// <returns></returns>
    public static string GetControlsLabel(string poxiaoKey, string dataType, FieldsModel control)
    {
        string label = string.Empty;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.POPUPTABLESELECT:
                label = control.relationField;
                break;
            case PoxiaoKeyConst.CASCADER:
            case PoxiaoKeyConst.TREESELECT:
                label = control.props.props.label;
                break;
            default:
                label = control.props?.props?.label;
                break;
        }

        return label;
    }

    /// <summary>
    /// 获取控件指定选项标签.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="dataType">数据类型.</param>
    /// <param name="control">控件全属性.</param>
    /// <returns></returns>
    public static string GetControlsValue(string poxiaoKey, string dataType, FieldsModel control)
    {
        string value = string.Empty;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.POPUPTABLESELECT:
                value = control.propsValue;
                break;
            case PoxiaoKeyConst.CASCADER:
            case PoxiaoKeyConst.TREESELECT:
                value = control.props.props.value;
                break;
            default:
                value = control.props?.props?.value;
                break;
        }

        return value;
    }

    /// <summary>
    /// 获取控件指定选项的子选项.
    /// </summary>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <param name="dataType">数据类型.</param>
    /// <param name="control">控件全属性.</param>
    /// <returns></returns>
    public static string GetControlsChildren(string poxiaoKey, string dataType, FieldsModel control)
    {
        string children = string.Empty;
        switch (poxiaoKey)
        {
            case PoxiaoKeyConst.CASCADER:
            case PoxiaoKeyConst.TREESELECT:
                children = control.props.props.children;
                break;
            default:
                children = control.props?.props?.children;
                break;
        }

        return children;
    }

    /// <summary>
    /// 获取导出配置.
    /// </summary>
    /// <param name="control">控件全属性.</param>
    /// <param name="model">数据库真实字段.</param>
    /// <param name="tableName">表名称.</param>
    /// <returns></returns>
    public static CodeGenFieldsModel GetImportConfig(FieldsModel control, string model, string tableName)
    {
        var fieldModel = new CodeGenFieldsModel();
        var configModel = new CodeGenConfigModel();
        fieldModel.__vModel__ = model;
        fieldModel.level = control.level;
        fieldModel.min = control.min;
        fieldModel.max = control.max;
        fieldModel.activeTxt = control.activeTxt;
        fieldModel.inactiveTxt = control.inactiveTxt;
        fieldModel.format = control.format;
        fieldModel.multiple = CodeGenFieldJudgeHelper.IsMultipleColumn(control, model);
        fieldModel.separator = control.separator;
        fieldModel.props = control.props?.ToObject<CodeGenPropsModel>()?.ToJsonString().ToJsonString();
        fieldModel.options = control.options?.ToObject<List<object>>()?.ToJsonString().ToJsonString();
        fieldModel.propsValue = control.propsValue;
        fieldModel.relationField = control.relationField;
        fieldModel.modelId = control.modelId;
        fieldModel.interfaceId = control.interfaceId;
        fieldModel.selectType = control.selectType;
        fieldModel.ableDepIds = control.ableDepIds?.ToJsonString().ToJsonString();
        fieldModel.ablePosIds = control.ablePosIds?.ToJsonString().ToJsonString();
        fieldModel.ableUserIds = control.ableUserIds?.ToJsonString().ToJsonString();
        fieldModel.ableRoleIds = control.ableRoleIds?.ToJsonString().ToJsonString();
        fieldModel.ableGroupIds = control.ableGroupIds?.ToJsonString().ToJsonString();
        fieldModel.ableIds = control.ableIds?.ToJsonString().ToJsonString();
        configModel = control.__config__.ToObject<CodeGenConfigModel>();
        configModel.tableName = tableName;
        fieldModel.__config__ = configModel.ToJsonString().ToJsonString();
        return fieldModel;
    }

    /// <summary>
    /// 获取需解析的字段集合.
    /// </summary>
    /// <param name="control"></param>
    /// <param name="isInlineEditor"></param>
    /// <returns>poxiaoKey @@ vmodel集合以 , 号隔开.</returns>
    public static List<string[]> GetParsPoxiaoKeyConstList(List<FieldsModel> control, bool isInlineEditor)
    {
        var res = new Dictionary<string, List<string>>();

        control.ForEach(item =>
        {
            switch (item.__config__.poxiaoKey)
            {
                case PoxiaoKeyConst.USERSSELECT: // 用户选择组件(包含组织、角色、岗位、分组、用户 Id)
                    if (!res.ContainsKey(PoxiaoKeyConst.USERSSELECT)) res.Add(PoxiaoKeyConst.USERSSELECT, new List<string>());
                    res[PoxiaoKeyConst.USERSSELECT].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.POPUPSELECT: // 弹窗选择
                    if (!res.ContainsKey(PoxiaoKeyConst.POPUPSELECT)) res.Add(PoxiaoKeyConst.POPUPSELECT, new List<string>());
                    res[PoxiaoKeyConst.POPUPSELECT].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.RELATIONFORM: // 关联表单
                    if (!res.ContainsKey(PoxiaoKeyConst.RELATIONFORM)) res.Add(PoxiaoKeyConst.RELATIONFORM, new List<string>());
                    res[PoxiaoKeyConst.RELATIONFORM].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.TABLE: // 遍历 子表 控件
                    var ctRes = GetParsPoxiaoKeyConstList(item.__config__.children, false);
                    if (ctRes != null && ctRes.Any())
                    {
                        foreach (var ct in ctRes)
                        {
                            if (!res.ContainsKey(ct.FirstOrDefault())) res.Add(ct.FirstOrDefault(), new List<string>());
                            res[ct.FirstOrDefault()].Add(item.__vModel__ + "-" + ct.LastOrDefault());
                        }
                    }
                    break;
            }
        });

        var ret = new List<string[]>();
        foreach (var item in res)
        {
            // 如果是行内编辑
            if (isInlineEditor)
            {
                var newValue = new List<string>();
                foreach (var it in item.Value) newValue.Add(it + "_name");
                res[item.Key] = newValue;
            }
        }
        foreach (var item in res)
        {
            ret.Add(new string[] { item.Key, string.Join(",", item.Value) });
        }
        return ret;
    }

    /// <summary>
    /// 获取需解析的字段集合.
    /// </summary>
    /// <param name="control"></param>
    /// <param name="isInlineEditor"></param>
    /// <returns>poxiaoKey @@ vmodel集合以 , 号隔开.</returns>
    public static List<string[]> GetParsPoxiaoKeyConstListDetails(List<FieldsModel> control)
    {
        var res = new Dictionary<string, List<string>>();

        control.ForEach(item =>
        {
            switch (item.__config__.poxiaoKey)
            {
                case PoxiaoKeyConst.USERSSELECT: // 用户选择组件(包含组织、角色、岗位、分组、用户 Id)
                    if (!res.ContainsKey(PoxiaoKeyConst.USERSSELECT)) res.Add(PoxiaoKeyConst.USERSSELECT, new List<string>());
                    res[PoxiaoKeyConst.USERSSELECT].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.POPUPSELECT: // 弹窗选择.
                    if (!res.ContainsKey(PoxiaoKeyConst.POPUPSELECT)) res.Add(PoxiaoKeyConst.POPUPSELECT, new List<string>());
                    res[PoxiaoKeyConst.POPUPSELECT].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.RELATIONFORM: // 关联表单.
                    if (!res.ContainsKey(PoxiaoKeyConst.RELATIONFORM)) res.Add(PoxiaoKeyConst.RELATIONFORM, new List<string>());
                    res[PoxiaoKeyConst.RELATIONFORM].Add(item.__vModel__);
                    break;
                case PoxiaoKeyConst.TABLE: // 遍历 子表 控件
                    var ctRes = GetParsPoxiaoKeyConstListDetails(item.__config__.children);
                    if (ctRes != null && ctRes.Any())
                    {
                        foreach (var ct in ctRes)
                        {
                            if (!res.ContainsKey(ct.FirstOrDefault())) res.Add(ct.FirstOrDefault(), new List<string>());
                            res[ct.FirstOrDefault()].Add(item.__vModel__ + "-" + ct.LastOrDefault());
                        }
                    }

                    break;
            }
        });

        var ret = new List<string[]>();
        foreach (var item in res)
        {
            ret.Add(new string[] { item.Key, string.Join(",", item.Value) });
        }

        return ret;
    }

    /// <summary>
    /// 获取模板配置的数据过滤.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="codeGenConfigModel"></param>
    /// <returns></returns>
    public static List<CodeGenDataRuleModuleResourceModel> GetDataRuleList(VisualDevEntity templateEntity, Model.CodeGen.CodeGenConfigModel codeGenConfigModel)
    {
        var ruleList = new List<CodeGenDataRuleModuleResourceModel>();
        var tInfo = new TemplateParsingBase(templateEntity);

        // 数据过滤
        if (tInfo.ColumnData.ruleList != null && tInfo.ColumnData.ruleList.Any()) tInfo.ColumnData.ruleList.ForEach(item => ruleList.Add(GetItemRule(item, tInfo, "pc", codeGenConfigModel, ref ruleList)));
        if (tInfo.AppColumnData.ruleListApp != null && tInfo.AppColumnData.ruleListApp.Any()) tInfo.AppColumnData.ruleListApp.ForEach(item => ruleList.Add(GetItemRule(item, tInfo, "app", codeGenConfigModel, ref ruleList)));

        var res = new List<CodeGenDataRuleModuleResourceModel>();
        foreach (var userOriginItem in new List<string>() { "pc", "app" })
        {
            ruleList.Where(x => x.UserOrigin.Equals(userOriginItem)).Select(x => x.TableName).Distinct().ToList().ForEach(tName =>
            {
                var first = ruleList.FirstOrDefault(x => x.UserOrigin.Equals(userOriginItem) && x.TableName.Equals(tName));
                var condList = ruleList.Where(x => x.UserOrigin.Equals(userOriginItem) && x.TableName.Equals(tName)).Select(x => x.conditionalModel.First()).ToList();
                var dataRuleListJson = new List<IConditionalModel>();
                var condTree = new ConditionalTree() { ConditionalList = new List<KeyValuePair<WhereType, IConditionalModel>>() };
                condList.ForEach(cItem => condTree.ConditionalList.Add(new KeyValuePair<WhereType, IConditionalModel>(WhereType.And, cItem)));
                res.Add(new CodeGenDataRuleModuleResourceModel()
                {
                    FieldRule = first.TableName.Contains("@ChildFieldIsNull") ? -1 : first.FieldRule,
                    TableName = first.TableName.Replace("@ChildFieldIsNull", string.Empty),
                    UserOrigin = first.UserOrigin,
                    conditionalModelJson = new List<IConditionalModel>() { condTree }.ToJsonString()
                });
            });
        }

        return res;
    }

    private static CodeGenDataRuleModuleResourceModel GetItemRule(RuleFieldModel item, TemplateParsingBase tInfo, string userOrigin, Model.CodeGen.CodeGenConfigModel codeGenConfigModel, ref List<CodeGenDataRuleModuleResourceModel> ruleList)
    {
        var result = new CodeGenDataRuleModuleResourceModel() { FieldRule = 0, TableName = tInfo.MainTableName.ToLower(), UserOrigin = userOrigin, conditionalModel = new List<IConditionalModel>() };
        if (tInfo.AuxiliaryTableFields.ContainsKey(item.__vModel__))
        {
            var tf = tInfo.AuxiliaryTableFields[item.__vModel__].Split('.');
            result.FieldRule = 1;
            result.TableName = tf.FirstOrDefault().ToLower();
            item.field = tf.LastOrDefault();
        }
        else if (tInfo.ChildTableFields.ContainsKey(item.__vModel__))
        {
            var tf = tInfo.ChildTableFields[item.__vModel__].Split('.');
            result.FieldRule = 2;
            result.TableName = tf.FirstOrDefault().ToLower();
            item.field = tf.LastOrDefault();

            if (item.symbol.Equals("null"))
            {
                var mainTableRelationsQuery = result.Copy();
                mainTableRelationsQuery.TableName = mainTableRelationsQuery.TableName + "@ChildFieldIsNull";
                var ctPrimaryKey = codeGenConfigModel.TableRelations.Find(x => x.OriginalTableName.Equals(result.TableName)).ChilderColumnConfigList.Find(x => x.ColumnName.Equals(codeGenConfigModel.TableRelations.Find(x => x.OriginalTableName.Equals(result.TableName)).PrimaryKey)).OriginalColumnName;
                var condTree = new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, new ConditionalModel() { FieldName = ctPrimaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                };

                mainTableRelationsQuery.conditionalModel = new List<IConditionalModel>() { condTree };
                ruleList.Add(mainTableRelationsQuery);
            }
        }

        if (codeGenConfigModel.TableField.Any(x => x.LowerColumnName.Equals(item.field)))
        {
            var fieldList = codeGenConfigModel.TableField.Where(x => x.LowerColumnName.Equals(item.field)).ToList();
            if (fieldList.Any() && fieldList.Count.Equals(1)) item.field = fieldList.First().OriginalColumnName;
            else item.field = fieldList.Find(x => x.TableName != null && x.TableName.Equals(item.__config__.tableName)).OriginalColumnName;
        }
        else if (codeGenConfigModel.TableRelations.Any(x => x.OriginalTableName.Equals(result.TableName)))
        {
            var tableRelations = codeGenConfigModel.TableRelations.Find(x => x.OriginalTableName.Equals(result.TableName) && x.ChilderColumnConfigList.Any(xx => xx.LowerColumnName.Equals(item.field)));
            if (tableRelations != null) item.field = tableRelations.ChilderColumnConfigList.Find(x => x.LowerColumnName.Equals(item.field)).OriginalColumnName;
        }

        var conditionalType = ConditionalType.Equal;
        var between = new List<string>();
        if (item.fieldValue.IsNotEmptyOrNull())
        {
            if (item.symbol.Equals("between")) between = item.fieldValue.ToObject<List<string>>();
            switch (item.poxiaoKey)
            {
                case PoxiaoKeyConst.COMSELECT:
                    if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[[")) item.fieldValue = item.fieldValue.ToObject<List<List<string>>>().Select(x => x.Last() + "\"]").ToList();
                    else if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[")) item.fieldValue = item.fieldValue.ToObject<List<string>>().Select(x => x + "\"]").ToList();
                    break;
                case PoxiaoKeyConst.CREATETIME:
                case PoxiaoKeyConst.MODIFYTIME:
                case PoxiaoKeyConst.DATE:
                    {
                        if (item.symbol.Equals("between"))
                        {
                            var startTime = between.First().TimeStampToDateTime();
                            var endTime = between.Last().TimeStampToDateTime();
                            between[0] = startTime.ToString();
                            between[1] = endTime.ToString();
                            if (item.format == "yyyy-MM-dd")
                            {
                                between[0] = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0).ToString();
                                between[1] = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999).ToString();
                            }
                            else if (item.format == "yyyy")
                            {
                                between[0] = new DateTime(startTime.Year, 1, 1, 0, 0, 0, 0).ToString();
                                between[1] = new DateTime(endTime.Year, 1, 1, 0, 0, 0, 0).ToString();
                            }
                        }
                        else
                        {
                            if (item.format == "yyyy-MM-dd")
                            {
                                var value = item.fieldValue.ToString().TimeStampToDateTime();
                                item.fieldValue = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0).ToString();
                            }
                            else
                            {
                                item.fieldValue = item.fieldValue.IsNotEmptyOrNull() ? item.fieldValue.ToString().TimeStampToDateTime() : item.fieldValue;
                            }
                        }
                    }
                    break;
                case PoxiaoKeyConst.TIME:
                    {
                        if (!item.symbol.Equals("between"))
                        {
                            item.fieldValue = string.Format("{0:" + item.format + "}", Convert.ToDateTime(item.fieldValue));
                        }
                    }
                    break;
            }
        }
        switch (item.symbol)
        {
            case ">=":
                conditionalType = ConditionalType.GreaterThanOrEqual;
                break;
            case ">":
                conditionalType = ConditionalType.GreaterThan;
                break;
            case "==":
                conditionalType = ConditionalType.Equal;
                break;
            case "<=":
                conditionalType = ConditionalType.LessThanOrEqual;
                break;
            case "<":
                conditionalType = ConditionalType.LessThan;
                break;
            case "<>":
                conditionalType = ConditionalType.NoEqual;
                break;
            case "like":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("[")) item.fieldValue = item.fieldValue.ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                conditionalType = ConditionalType.Like;
                break;
            case "notLike":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("[")) item.fieldValue = item.fieldValue.ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                conditionalType = ConditionalType.NoLike;
                break;
            case "in":
            case "notIn":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("["))
                {
                    var isListValue = false;
                    var itemField = tInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item.__vModel__));
                    if (itemField.multiple || item.poxiaoKey.Equals(PoxiaoKeyConst.CHECKBOX) || item.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || item.poxiaoKey.Equals(PoxiaoKeyConst.ADDRESS))
                        isListValue = true;
                    if (item.poxiaoKey.Equals(PoxiaoKeyConst.COMSELECT)) isListValue = false;
                    var conditionalList = new ConditionalCollections() { ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>() };
                    var ids = new List<string>();
                    if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[[")) ids = item.fieldValue.ToObject<List<List<string>>>().Select(x => x.Last()).ToList();
                    else ids = item.fieldValue.ToObject<List<string>>();
                    for (var i = 0; i < ids.Count; i++)
                    {
                        var it = ids[i];
                        var whereType = WhereType.And;
                        if (item.symbol.Equals("in")) whereType = i.Equals(0) && item.logic.Equals("&&") ? WhereType.And : WhereType.Or;
                        else whereType = i.Equals(0) && item.logic.Equals("||") ? WhereType.Or : WhereType.And;
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(whereType, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = item.symbol.Equals("in") ? ConditionalType.Like : ConditionalType.NoLike,
                            FieldValue = isListValue ? it.ToJsonString() : it
                        }));
                    }

                    if (item.symbol.Equals("notIn"))
                    {
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.IsNot,
                            FieldValue = null
                        }));
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.IsNot,
                            FieldValue = string.Empty
                        }));
                    }

                    result.conditionalModel.Add(conditionalList);
                    return result;
                }
                conditionalType = item.symbol.Equals("in") ? ConditionalType.In : ConditionalType.NotIn;
                break;
            case "null":
                conditionalType = (item.poxiaoKey.Equals(PoxiaoKeyConst.CALCULATE) || item.poxiaoKey.Equals(PoxiaoKeyConst.NUMINPUT)) ? ConditionalType.EqualNull : ConditionalType.IsNullOrEmpty;
                break;
            case "notNull":
                conditionalType = ConditionalType.IsNot;
                break;
            case "between":
                var condItem = new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>((item.logic.Equals("&&") ? WhereType.And : WhereType.Or), new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.GreaterThanOrEqual,
                            FieldValue = between.First(),
                            FieldValueConvertFunc = it => Convert.ToDateTime(it)
                        }),
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.LessThanOrEqual,
                            FieldValue = between.Last(),
                            FieldValueConvertFunc = it => Convert.ToDateTime(it)
                        })
                    }
                };

                result.conditionalModel.Add(condItem);
                return result;
        }

        var resItem = new ConditionalCollections()
        {
            ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
            {
                new KeyValuePair<WhereType, ConditionalModel>((item.logic.Equals("&&") ? WhereType.And : WhereType.Or), new ConditionalModel
                {
                    FieldName = item.field,
                    ConditionalType = conditionalType,
                    FieldValue = item.fieldValue == null ? null : item.fieldValue.ToString()
                })
            }
        };
        result.conditionalModel.Add(resItem);
        return result;
    }
}