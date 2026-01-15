using Aspose.Words.Fields;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.VisualDev.Engine.Model.CodeGen;
using Poxiao.VisualDev.Engine.Security;
using Poxiao.VisualDev.Entitys;

namespace Poxiao.VisualDev.Engine.CodeGen;

/// <summary>
/// 代码生成方式.
/// </summary>
public class CodeGenWay
{
    /// <summary>
    /// 副表表字段配置.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="tableNo">表序号.</param>
    /// <param name="modelType">0-主带副,1-主带子副.</param>
    /// <returns></returns>
    public static CodeGenConfigModel AuxiliaryTableBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity, int tableNo, int modelType)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsConversion = false,
                        IsSystemControl = false,
                        IsAuxiliary = true,
                        IsControlParsing = false,
                        IsUpdate = false,
                        TableNo = tableNo,
                        TableName = tableName,
                    });
                    break;
                default:
                    var childControl = string.Format("poxiao_{0}_poxiao_{1}", tableName, field);
                    switch (controls.Any(c => c.__vModel__.Equals(childControl)))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__.Equals(childControl));
                            var isImportField = templateEntity.WebType == 1 ? false : columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(childControl));
                            switch (control.__config__.poxiaoKey)
                            {
                                case PoxiaoKeyConst.MODIFYUSER:
                                case PoxiaoKeyConst.CREATEUSER:
                                case PoxiaoKeyConst.CURRORGANIZE:
                                case PoxiaoKeyConst.CURRPOSITION:
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, childControl),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, childControl),
                                        IsShow = control.isIndexShow,
                                        IsUnique = control.__config__.unique,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsSecondaryTableDateTime(control),
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, childControl)),
                                        IsConversion = modelType == 1 ? CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey) : CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, childControl)),
                                        IsSystemControl = true,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = true,
                                        TableNo = tableNo,
                                        TableName = tableName,
                                        FormatTableName = tableName.ParseToPascalCase(),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        IsControlParsing = false,
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = childControl.Equals(columnDesignModel.parentField),
                                    });
                                    break;
                                default:
                                    var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, childControl),
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, childControl),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, childControl),
                                        IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, childControl),
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, childControl),
                                        IsUnique = control.__config__.unique,
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsSecondaryTableDateTime(control),
                                        Format = control.format,
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        ControlsDataType = dataType,
                                        StaticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()),
                                        propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                        Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                        Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                        Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                        Separator = control.separator,
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, childControl)),
                                        IsConversion = modelType == 1 ? CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey) : CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, childControl)),
                                        IsSystemControl = false,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = true,
                                        TableNo = tableNo,
                                        TableName = tableName,
                                        FormatTableName = tableName.ParseToPascalCase(),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsControlParsing = CodeGenFieldJudgeHelper.IsControlParsing(control),
                                        ShowField = control.relational,
                                        IsTreeParentField = childControl.Equals(columnDesignModel.parentField),
                                        IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                        LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 1),
                                    });
                                    break;
                            }
                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                TableName = tableName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = false,
                                ForeignKeyField = true,
                                IsImportField = false,
                                IsSystemControl = false,
                                IsAuxiliary = true,
                                IsUpdate = false,
                                TableNo = tableNo,
                                IsControlParsing = false,
                            });
                            break;
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104);
        }

        // 是否存在上传控件.
        bool isUpload = tableColumnList.Any(it => it.PrimaryKey.Equals(false) && it.ForeignKeyField.Equals(false) && (it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADIMG) || it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = CodeGenFieldJudgeHelper.IsChildTableMapper(tableColumnList);

        // 是否查询条件多选
        bool isSearchMultiple = tableColumnList.Any(it => it.QueryMultiple);

        bool isLogicalDelete = formDataModel.logicalDelete;

        return new CodeGenConfigModel()
        {
            NameSpace = formDataModel.areasName,
            BusName = templateEntity.FullName,
            ClassName = formDataModel.className.FirstOrDefault(),
            PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
            OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
            MainTable = tableName.ParseToPascalCase(),
            OriginalMainTableName = tableName,
            TableField = tableColumnList,
            IsUpload = isUpload,
            IsMapper = isMapper,
            WebType = templateEntity.WebType,
            Type = templateEntity.Type,
            PrimaryKeyPolicy = formDataModel.primaryKeyPolicy,
            IsImportData = tableColumnList.Any(it => it.IsImportField.Equals(true)),
            EnableFlow = templateEntity.EnableFlow == 1 ? true : false,
            IsSearchMultiple = isSearchMultiple,
            IsLogicalDelete = isLogicalDelete,
        };
    }

    /// <summary>
    /// 子表表字段配置.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="className">功能名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="controlId">子表控件vModel.</param>
    /// <param name="tableField">关联字段.</param>
    /// <returns></returns>
    public static CodeGenConfigModel ChildTableBackEnd(string tableName, string className, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity, string controlId, string tableField)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsControlParsing = false,
                    });
                    break;
                default:
                    switch (controls.Any(c => c.__vModel__ == field))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__ == field);
                            var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                            var isImportField = templateEntity.WebType == 1 ? false : columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(string.Format("{0}-{1}", controlId, field)));
                            var staticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString());
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = column.primaryKey.ParseToBool(),
                                QueryWhether = control.isQueryField,
                                QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, string.Format("{0}-{1}", controlId, field)),
                                QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, string.Format("{0}-{1}", controlId, field)),
                                IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                IsUnique = control.__config__.unique,
                                poxiaoKey = control.__config__.poxiaoKey,
                                Rule = control.__config__.rule,
                                IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                ActiveTxt = control.activeTxt,
                                InactiveTxt = control.inactiveTxt,
                                IsShow = control.isIndexShow,
                                ControlsDataType = dataType,
                                StaticData = staticData,
                                Format = control.format,
                                propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                Separator = control.separator,
                                IsConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                IsDetailConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                ControlLabel = control.__config__.label,
                                ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                IsImportField = isImportField.ParseToBool(),
                                ChildControlKey = controlId,
                                ShowField = control.relational,
                                IsControlParsing = CodeGenFieldJudgeHelper.IsControlParsing(control),
                                IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 2, controlId),
                            });
                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                ForeignKeyField = tableField.Equals(field) ? true : false,
                                IsImportField = false,
                                IsControlParsing = false,
                            });
                            break;
                    }
                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104, tableName);
        }

        // 是否存在上传控件.
        bool isUpload = tableColumnList.Any(it => it.PrimaryKey.Equals(false) && it.ForeignKeyField.Equals(false) && it.poxiaoKey != null && (it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADIMG) || it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = CodeGenFieldJudgeHelper.IsChildTableMapper(tableColumnList);

        bool isShowSubTableField = tableColumnList.Any(it => it.IsShow.Equals(true));

        // 是否查询条件多选
        bool isSearchMultiple = tableColumnList.Any(it => it.QueryMultiple);

        bool isLogicalDelete = formDataModel.logicalDelete;

        return new CodeGenConfigModel()
        {
            NameSpace = formDataModel.areasName,
            BusName = templateEntity.FullName,
            ClassName = className,
            PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
            OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
            TableField = tableColumnList,
            IsUpload = isUpload,
            IsMapper = isMapper,
            WebType = templateEntity.WebType,
            Type = templateEntity.Type,
            PrimaryKeyPolicy = formDataModel.primaryKeyPolicy,
            IsImportData = tableColumnList.Any(it => it.IsImportField.Equals(true)),
            IsShowSubTableField = isShowSubTableField,
            IsSearchMultiple = isSearchMultiple,
            IsLogicalDelete = isLogicalDelete,
            TableType = columnDesignModel.type,
        };
    }

    /// <summary>
    /// 主表带子表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel MainBeltBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        var table = templateEntity.Tables.ToObject<List<DbTableRelationModel>>();

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsConversion = false,
                        IsSystemControl = false,
                        IsUpdate = false,
                    });
                    break;
                default:
                    switch (controls.Any(c => c.__vModel__ == field))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__ == field);
                            var childControl = string.Empty;
                            var isImportField = templateEntity.WebType == 1 ? false : columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field));

                            switch (control.__config__.poxiaoKey)
                            {
                                case PoxiaoKeyConst.MODIFYUSER:
                                case PoxiaoKeyConst.CREATEUSER:
                                case PoxiaoKeyConst.CURRORGANIZE:
                                case PoxiaoKeyConst.CURRPOSITION:
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsUnique = control.__config__.unique,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = true,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField == null ? false : (bool)isImportField,
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = childControl.Equals(columnDesignModel.parentField),
                                    });
                                    break;
                                default:
                                    var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        IsUnique = control.__config__.unique,
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        Format = control.format,
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        ControlsDataType = dataType,
                                        StaticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()),
                                        propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                        Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                        Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                        Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                        Separator = control.separator,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = false,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField == null ? false : (bool)isImportField,
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        ShowField = control.relational,
                                        IsTreeParentField = childControl.Equals(columnDesignModel.parentField),
                                        IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                        LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 0),
                                    });
                                    break;
                            }
                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = false,
                                IsConversion = false,
                                IsSystemControl = false,
                                ForeignKeyField = true,
                                IsUpdate = false,
                                IsControlParsing = false,
                            });
                            break;
                    }
                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey)) throw Oops.Oh(ErrorCode.D2104);

        return GetCodeGenConfigModel(formDataModel, columnDesignModel, tableColumnList, controls, tableName, templateEntity);
    }

    /// <summary>
    /// 主表带副表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="auxiliaryTableColumnList">副表字段配置.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel MainBeltViceBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<TableColumnConfigModel> auxiliaryTableColumnList, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsConversion = false,
                        IsSystemControl = false,
                        IsAuxiliary = false,
                        IsUpdate = false,
                    });
                    break;
                default:
                    switch (controls.Any(c => c.__vModel__ == field))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__ == field);
                            var isImportField = templateEntity.WebType == 1 ? false : columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field));
                            switch (control.__config__.poxiaoKey)
                            {
                                case PoxiaoKeyConst.MODIFYUSER:
                                case PoxiaoKeyConst.CREATEUSER:
                                case PoxiaoKeyConst.CURRORGANIZE:
                                case PoxiaoKeyConst.CURRPOSITION:
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsUnique = control.__config__.unique,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = true,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = false,
                                        TableName = tableName,
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                    });
                                    break;
                                default:
                                    var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        IsUnique = control.__config__.unique,
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        Format = control.format,
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        ControlsDataType = dataType,
                                        StaticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()),
                                        propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                        Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                        Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                        Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                        Separator = control.separator,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = false,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = false,
                                        TableName = tableName,
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                        IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                        LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 0),
                                    });
                                    break;
                            }
                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = false,
                                IsConversion = false,
                                IsSystemControl = false,
                                IsAuxiliary = false,
                                IsUpdate = false,
                                IsControlParsing = false,
                            });
                            break;
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104);
        }

        tableColumnList.AddRange(auxiliaryTableColumnList);

        return GetCodeGenConfigModel(formDataModel, columnDesignModel, tableColumnList, controls, tableName, templateEntity);
    }

    /// <summary>
    /// 主表带子副表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="auxiliaryTableColumnList">副表字段配置.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel PrimarySecondaryBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<TableColumnConfigModel> auxiliaryTableColumnList, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        var tableColumnList = new List<TableColumnConfigModel>();

        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsAuxiliary = false,
                        IsUpdate = false,
                    });
                    break;
                default:
                    switch (controls.Any(c => c.__vModel__.Equals(field)))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__ == field);
                            switch (control.__config__.poxiaoKey)
                            {
                                case PoxiaoKeyConst.MODIFYUSER:
                                case PoxiaoKeyConst.CREATEUSER:
                                case PoxiaoKeyConst.CURRORGANIZE:
                                case PoxiaoKeyConst.CURRPOSITION:
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsUnique = control.__config__.unique,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, "", CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = true,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = false,
                                        TableName = tableName,
                                        ControlLabel = control.__config__.label,
                                        IsImportField = columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field)) == null ? false : (bool)columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field)),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                    });
                                    break;
                                default:
                                    var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        IsUnique = control.__config__.unique,
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        Format = control.format,
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        ControlsDataType = dataType,
                                        StaticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()),
                                        propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                        Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                        Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                        Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                        Separator = control.separator,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeContainsChildTableControlIsDataConversion(control.__config__.poxiaoKey),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = false,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        IsAuxiliary = false,
                                        TableName = tableName,
                                        ControlLabel = control.__config__.label,
                                        IsImportField = columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field)) == null ? false : (bool)columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field)),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        ShowField = control.relational,
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                        IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                        LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 0),
                                    });
                                    break;
                            }
                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = false,
                                IsAuxiliary = false,
                                IsUpdate = false,
                            });
                            break;
                    }
                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey)) throw Oops.Oh(ErrorCode.D2104);

        tableColumnList.AddRange(auxiliaryTableColumnList);

        return GetCodeGenConfigModel(formDataModel, columnDesignModel, tableColumnList, controls, tableName, templateEntity);
    }

    /// <summary>
    /// 单表后端.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel SingleTableBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = GetMultiEndQueryMerging(templateEntity, controls);
        columnDesignModel.columnList = GetMultiTerminalListDisplayAndConsolidation(templateEntity);
        FormDataModel formDataModel = templateEntity.FormData.ToObjectOld<FormDataModel>();
        var tableColumnList = new List<TableColumnConfigModel>();

        // 移除乐观锁
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("version"));

        // 移除真实流程ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowtaskid"));

        // 移除流程引擎ID
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("flowid"));

        // 移除逻辑删除
        dbTableFields.RemoveAll(it => it.field.ReplaceRegex("^f_", string.Empty).ToLower().Equals("deletemark"));

        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsConversion = false,
                        IsSystemControl = false,
                        IsUpdate = false,
                    });
                    break;
                default:
                    // 存在表单内控件
                    switch (controls.Any(c => c.__vModel__ == field))
                    {
                        case true:
                            FieldsModel control = controls.Find(c => c.__vModel__ == field);
                            bool? isImportField = templateEntity.WebType == 1 ? false : columnDesignModel?.uploaderTemplateJson?.selectKey?.Any(it => it.Equals(field));
                            switch (control.__config__.poxiaoKey)
                            {
                                case PoxiaoKeyConst.MODIFYUSER:
                                case PoxiaoKeyConst.CREATEUSER:
                                case PoxiaoKeyConst.CURRORGANIZE:
                                case PoxiaoKeyConst.CURRPOSITION:
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsUnique = control.__config__.unique,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        IsConversion = false,
                                        IsDetailConversion = false,
                                        IsSystemControl = true,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                    });
                                    break;
                                default:
                                    var dataType = control.__config__.dataType != null ? control.__config__.dataType : null;
                                    tableColumnList.Add(new TableColumnConfigModel()
                                    {
                                        ColumnName = field.ToUpperCase(),
                                        OriginalColumnName = column.field,
                                        ColumnComment = column.fieldName,
                                        DataType = column.dataType,
                                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                        PrimaryKey = column.primaryKey.ParseToBool(),
                                        QueryWhether = control.isQueryField,
                                        QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                                        QueryMultiple = CodeGenFieldJudgeHelper.ColumnQueryMultiple(searchList: columnDesignModel.searchList, field),
                                        IsShow = control.isIndexShow,
                                        IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                                        IsUnique = control.__config__.unique,
                                        poxiaoKey = control.__config__.poxiaoKey,
                                        Rule = control.__config__.rule,
                                        IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                                        Format = control.format,
                                        ActiveTxt = control.activeTxt,
                                        InactiveTxt = control.inactiveTxt,
                                        ControlsDataType = dataType,
                                        StaticData = control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || control.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) ? CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()) : CodeGenControlsAttributeHelper.ConversionStaticData(control.options.ToJsonString()),
                                        propsUrl = CodeGenControlsAttributeHelper.GetControlsPropsUrl(control.__config__.poxiaoKey, dataType, control),
                                        Label = CodeGenControlsAttributeHelper.GetControlsLabel(control.__config__.poxiaoKey, dataType, control),
                                        Value = CodeGenControlsAttributeHelper.GetControlsValue(control.__config__.poxiaoKey, dataType, control),
                                        Children = CodeGenControlsAttributeHelper.GetControlsChildren(control.__config__.poxiaoKey, dataType, control),
                                        Separator = control.separator,
                                        IsConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsDetailConversion = CodeGenControlsAttributeHelper.JudgeControlIsDataConversion(control.__config__.poxiaoKey, dataType, CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field)),
                                        IsSystemControl = false,
                                        IsUpdate = CodeGenControlsAttributeHelper.JudgeControlIsSystemControls(control.__config__.poxiaoKey),
                                        ControlLabel = control.__config__.label,
                                        IsImportField = isImportField.ParseToBool(),
                                        ImportConfig = CodeGenControlsAttributeHelper.GetImportConfig(control, column.field, tableName),
                                        ShowField = control.relational,
                                        IsTreeParentField = field.Equals(columnDesignModel.parentField),
                                        IsLinkage = control.__config__.templateJson != null && control.__config__.templateJson.Count > 0 && control.__config__.templateJson.Any(it => !string.IsNullOrEmpty(it.relationField)) ? true : false,
                                        LinkageConfig = CodeGenControlsAttributeHelper.ObtainTheCurrentControlLinkageConfiguration(control.__config__.templateJson?.FindAll(it => !string.IsNullOrEmpty(it.relationField)), 0),
                                    });
                                    break;
                            }

                            break;
                        case false:
                            tableColumnList.Add(new TableColumnConfigModel()
                            {
                                ColumnName = field.ToUpperCase(),
                                OriginalColumnName = column.field,
                                ColumnComment = column.fieldName,
                                DataType = column.dataType,
                                NetType = CodeGenHelper.ConvertDataType(column.dataType),
                                PrimaryKey = false,
                                IsConversion = false,
                                IsSystemControl = false,
                                IsUpdate = false,
                            });
                            break;
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
            throw Oops.Oh(ErrorCode.D2104);

        return GetCodeGenConfigModel(formDataModel, columnDesignModel, tableColumnList, controls, tableName, templateEntity);
    }

    /// <summary>
    /// 前端.
    /// </summary>
    /// <param name="logic">生成逻辑;4-pc,5-app.</param>
    /// <param name="formDataModel">表单Json包.</param>
    /// <param name="controls">移除布局控件后的控件列表.</param>
    /// <param name="tableColumns">表字段.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static FrontEndGenConfigModel SingleTableFrontEnd(int logic, FormDataModel formDataModel, List<FieldsModel> controls, List<TableColumnConfigModel> tableColumns, VisualDevEntity templateEntity)
    {
        ColumnDesignModel columnDesignModel = new ColumnDesignModel();
        bool isInlineEditor = false;
        switch (logic)
        {
            case 4:
                columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();
                isInlineEditor = columnDesignModel.type == 4 ? true : false;
                break;
            case 5:
                ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                isInlineEditor = pcColumnDesignModel?.type == 4 ? true : false;
                columnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();

                // 移动端的分页遵循PC端
                columnDesignModel.hasPage = templateEntity.WebType == 1 ? false : pcColumnDesignModel.hasPage;
                break;
        }

        switch (templateEntity.Type)
        {
            case 3:
                break;
            default:
                if (templateEntity.WebType != 1)
                    controls = CodeGenUnifiedHandlerHelper.UnifiedHandlerFormDataModel(controls, columnDesignModel);
                break;
        }

        // 联动关系链判断
        controls = CodeGenUnifiedHandlerHelper.LinkageChainJudgment(controls, columnDesignModel);

        Dictionary<string, List<string>> listQueryControls = CodeGenQueryControlClassificationHelper.ListQueryControl(logic);

        /*
         *  PC 逻辑时： 行内编辑时 pc端需要循环子表日期控件
         *  APP 逻辑时：循环出除子表外全部开启千位符的数字输入控件字段
         */
        List<CodeGenSpecifyDateFormatSetModel> specifyDateFormatSet = new List<CodeGenSpecifyDateFormatSetModel>();
        var appThousandField = string.Empty;
        switch (logic)
        {
            case 4:
                switch (columnDesignModel.type)
                {
                    case 4:
                        foreach (var item in controls)
                        {
                            var config = item.__config__;
                            switch (config.poxiaoKey)
                            {
                                case PoxiaoKeyConst.TABLE:
                                    var model = CodeGenFormControlDesignHelper.CodeGenSpecifyDateFormatSetModel(item);
                                    if (model != null)
                                        specifyDateFormatSet.Add(model);
                                    break;
                            }
                        }
                        break;
                }
                break;
            case 5:
                appThousandField = controls.FindAll(it => it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.NUMINPUT) && it.thousands).Select(it => it.__vModel__).ToList().ToJsonString();
                appThousandField = appThousandField == "[]" ? null : appThousandField;
                break;
        }

        // 表单脚本设计
        List<FormScriptDesignModel>? formScriptDesign = CodeGenFormControlDesignHelper.FormScriptDesign("SingleTable", controls, tableColumns, columnDesignModel?.columnList);

        // 整个表单控件
        List<FormControlDesignModel>? formControlList = CodeGenFormControlDesignHelper.FormControlDesign(formDataModel.fields, controls, formDataModel.gutter, formDataModel.labelWidth, columnDesignModel?.columnList, columnDesignModel.type, logic, true);

        var formRealControl = CodeGenFormControlDesignHelper.FormRealControl(controls);

        // 列表控件Option
        var indnxControlOption = CodeGenFormControlDesignHelper.FormControlProps(formDataModel.fields, controls, columnDesignModel, logic, true);

        // 列表查询字段设计
        var indexSearchFieldDesign = new List<IndexSearchFieldDesignModel>();

        // 查询条件查询差异列表
        var queryCriteriaQueryVarianceList = new List<IndexSearchFieldModel>();

        // 列表顶部按钮
        var indexTopButton = new List<IndexButtonDesign>();

        // 列表行按钮
        var indexColumnButtonDesign = new List<IndexButtonDesign>();

        // 列表页列表
        var indexColumnDesign = new List<IndexColumnDesign>();

        switch (templateEntity.Type)
        {
            case 3:
                break;
            default:
                switch (templateEntity.WebType)
                {
                    case 2:
                        // 本身查询列表内带有控件全属性 单表不需要匹配表字段
                        foreach (var item in columnDesignModel?.searchList)
                        {
                            // 查询控件分类
                            var queryControls = listQueryControls.Where(q => q.Value.Contains(item.__config__.poxiaoKey)).FirstOrDefault();

                            var childTableLabel = string.Empty;
                            var childControl = item.__vModel__.Split('-');

                            // 是否子表查询
                            bool isChildQuery = false;

                            // 表单真实控件
                            FieldsModel? column = new FieldsModel();
                            if (item.__config__.relationTable != null && !item.__vModel__.IsMatch("_poxiao_"))
                            {
                                isChildQuery = true;
                                column = controls.Find(it => it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && it.__vModel__.Equals(childControl[0]) && it.__config__.children.Any(child => child.__vModel__.Equals(childControl[1])));
                                childTableLabel = column.__config__.label + "-";
                                column = column.__config__.children.Find(it => it.__vModel__ == childControl[1]);
                            }
                            else
                            {
                                column = controls.Find(c => c.__vModel__ == item.__vModel__);
                            }

                            indexSearchFieldDesign.Add(new IndexSearchFieldDesignModel()
                            {
                                OriginalName = string.IsNullOrEmpty(column.superiorVModel) ? column.__vModel__ : string.Format("{0}_{1}", column.superiorVModel, column.__vModel__),
                                Name = string.IsNullOrEmpty(column.superiorVModel) ? column.__vModel__ : string.Format("{0}_{1}", column.superiorVModel, column.__vModel__),
                                LowerName = column.__vModel__,
                                Tag = column.__config__.tag,
                                Clearable = item.clearable ? "clearable " : string.Empty,
                                Format = column.format,
                                ValueFormat = column.valueformat,
                                Label = childTableLabel + column.__config__.label,
                                IsChildQuery = isChildQuery,
                                QueryControlsKey = queryControls.Key != null ? queryControls.Key : null,
                                Props = column.props?.props,
                                Index = columnDesignModel.searchList.IndexOf(item),
                                Type = column.type,
                                ShowAllLevels = column.showalllevels ? "true" : "false",
                                Level = column.level,
                                IsMultiple = item.searchMultiple,
                                poxiaoKey = column.__config__.poxiaoKey,
                                SelectType = column.selectType != null ? column.selectType.Equals("custom") ? string.Format("selectType='{0}' ", column.selectType) : string.Format("selectType='all' ") : string.Empty,
                                AbleDepIds = column.selectType != null && column.selectType == "custom" ? string.Format(":ableDepIds='{0}_AbleDepIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                AblePosIds = column.selectType != null && column.selectType == "custom" && (column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POSSELECT)) ? string.Format(":ablePosIds='{0}_AblePosIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                AbleUserIds = column.selectType != null && column.selectType == "custom" && column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableUserIds='{0}_AbleUserIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                AbleRoleIds = column.selectType != null && column.selectType == "custom" && column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableRoleIds='{0}_AbleRoleIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                AbleGroupIds = column.selectType != null && column.selectType == "custom" && column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableGroupIds='{0}_AbleGroupIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                AbleIds = column.selectType != null && column.selectType == "custom" && column.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSSELECT) ? string.Format(":ableIds='{0}_AbleIds' ", childControl.Length >= 2 ? string.Format("{0}_{1}", childControl[0], column.__vModel__) : item.__vModel__) : string.Empty,
                                RelationField = column.relationField,
                                InterfaceId = column.interfaceId,
                                Total = column.total,
                            });
                        }

                        var multipleQueryFields = GetMultiEndQueryMerging(templateEntity);

                        // 控件查询多选数组
                        var controlQueryMultipleSelectionArray = new List<string>
                        {
                            PoxiaoKeyConst.SELECT,
                            PoxiaoKeyConst.DEPSELECT,
                            PoxiaoKeyConst.ROLESELECT,
                            PoxiaoKeyConst.USERSELECT,
                            PoxiaoKeyConst.USERSSELECT,
                            PoxiaoKeyConst.COMSELECT,
                            PoxiaoKeyConst.POSSELECT,
                            PoxiaoKeyConst.GROUPSELECT,
                        };

                        // 查询条件查询差异列表
                        queryCriteriaQueryVarianceList = columnDesignModel.searchList.FindAll(it => controlQueryMultipleSelectionArray.Contains(it.__config__.poxiaoKey)).ToList().FindAll(it => !it.searchMultiple.Equals(multipleQueryFields.Find(x => x.__config__.poxiaoKey.Equals(it.__config__.poxiaoKey) && x.prop.Equals(it.prop)).searchMultiple));

                        // 生成头部按钮信息
                        foreach (var item in columnDesignModel?.btnsList)
                        {
                            indexTopButton.Add(new IndexButtonDesign()
                            {
                                Type = columnDesignModel.btnsList.IndexOf(item) == 0 ? "primary" : "text",
                                Icon = item.icon,
                                Method = GetCodeGenIndexButtonHelper.IndexTopButton(item.value, templateEntity.EnableFlow == 1 ? true : false),
                                Value = item.value,
                                Label = item.label
                            });
                        }

                        // 生成行按钮信息
                        foreach (var item in columnDesignModel.columnBtnsList)
                        {
                            indexColumnButtonDesign.Add(new IndexButtonDesign()
                            {
                                Type = item.value == "remove" ? "class='Poxiao-table-delBtn' " : string.Empty,
                                Icon = item.icon,
                                Method = GetCodeGenIndexButtonHelper.IndexColumnButton(item.value, tableColumns.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName, formDataModel.primaryKeyPolicy, templateEntity.EnableFlow == 1 ? true : false, columnDesignModel?.type == 4 ? true : false),
                                Value = item.value,
                                Label = item.label,
                                Disabled = GetCodeGenIndexButtonHelper.WorkflowIndexColumnButton(item.value),
                                IsDetail = item.value == "detail" ? true : false
                            });
                        }

                        List<string> ChildControlField = new List<string>();

                        // 生成列信息
                        foreach (var item in columnDesignModel.columnList)
                        {
                            if (!ChildControlField.Any(it => it == item.__vModel__))
                            {
                                var relationTable = item?.__config__?.relationTable;
                                if (relationTable != null && !indexColumnDesign.Any(it => it.TableName == relationTable))
                                {
                                    var childTableAll = columnDesignModel.columnList.FindAll(it => it.__config__.relationTable == relationTable);
                                    var childTable = controls.Find(it => it.__config__.tableName == relationTable);
                                    if (childTable.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
                                    {
                                        var childTableColumnDesign = new List<IndexColumnDesign>();
                                        foreach (var child in childTableAll)
                                        {
                                            var columnControl = childTable.__config__.children.Find(it => it.__vModel__.Equals(child.__vModel__.Split('-')[1]));
                                            childTableColumnDesign.Add(new IndexColumnDesign()
                                            {
                                                TableName = child.__config__.tableName,
                                                Name = columnControl.__vModel__,
                                                OptionsName = columnControl.__vModel__,
                                                LowerName = columnControl.__vModel__,
                                                poxiaoKey = child.poxiaoKey,
                                                Label = columnControl.__config__.label,
                                                Width = child.width.ToString() == "0" ? "0" : string.Format("{0}", child.width),
                                                Align = child.align,
                                                IsSort = child.sortable ? string.Format("sortable='custom' ") : string.Empty,
                                                IsChildTable = true,
                                                Format = child.format?.ToLower().Replace(":mm", ":MM"),
                                                ModelId = child.modelId,
                                                Thousands = child.thousands,
                                                Precision = child.precision == null ? 0 : child.precision,
                                            });
                                            ChildControlField.Add(string.Format("{0}", child.__vModel__));
                                        }

                                        indexColumnDesign.Add(new IndexColumnDesign()
                                        {
                                            TableName = relationTable,
                                            Name = childTable.__vModel__,
                                            Label = childTable.__config__.label,
                                            poxiaoKey = PoxiaoKeyConst.TABLE,
                                            IsChildTable = true,
                                            ChildTableDesigns = childTableColumnDesign,
                                            Fixed = string.Empty,
                                        });
                                    }
                                }
                                else
                                {
                                    indexColumnDesign.Add(new IndexColumnDesign()
                                    {
                                        TableName = item?.__config__?.tableName,
                                        Name = item.prop,
                                        OptionsName = item.prop,
                                        LowerName = item.prop,
                                        poxiaoKey = item.poxiaoKey,
                                        Label = item.label,
                                        Width = item.width == null ? string.Empty : string.Format("width='{0}' ", item.width),
                                        Fixed = columnDesignModel.childTableStyle == 1 ? (item.@fixed == "none" || item.@fixed == null ? string.Empty : string.Format("fixed='{0}' ", item.@fixed)) : string.Empty,
                                        Align = item.align,
                                        IsSort = item.sortable ? string.Format("sortable='custom' ") : string.Empty,
                                        IsChildTable = false,
                                        ModelId = item.modelId,
                                        Thousands = item.thousands,
                                        Precision = item.precision == null ? 0 : item.precision,
                                    });
                                }

                            }
                        }

                        break;
                }

                break;
        }

        var propertyJson = CodeGenFormControlDesignHelper.GetPropertyJson(formScriptDesign);

        var printIds = columnDesignModel.printIds != null ? string.Join(",", columnDesignModel.printIds) : null;
        var isBatchRemoveDel = indexTopButton.Any(it => it.Value == "batchRemove");
        var isBatchPrint = indexTopButton.Any(it => it.Value == "batchPrint");
        var isUpload = indexTopButton.Any(it => it.Value == "upload");
        var isDownload = indexTopButton.Any(it => it.Value == "download");
        var isRemoveDel = indexColumnButtonDesign.Any(it => it.Value == "remove");
        var isEdit = indexColumnButtonDesign.Any(it => it.Value == "edit");
        var isDetail = indexColumnButtonDesign.Any(it => it.IsDetail.Equals(true));
        var isSort = columnDesignModel?.columnList?.Any(it => it.sortable) ?? false;
        var isSummary = formScriptDesign.Any(it => it.poxiaoKey.Equals("table") && it.ShowSummary.Equals(true));
        var isAdd = indexTopButton.Any(it => it.Value == "add");
        var isTreeRelation = !string.IsNullOrEmpty(columnDesignModel?.treeRelation);
        var isRelationForm = formControlList.Any(it => it.IsRelationForm);
        var isTreeRelationMultiple = indexSearchFieldDesign.Any(it => it.Name.Equals(columnDesignModel?.treeRelation?.Replace("-", "_")) && it.IsMultiple);
        var isFixed = columnDesignModel.childTableStyle == 1 ? indexColumnDesign.Any(it => it.Fixed.Equals("fixed='left' ") && !it.Name.Equals(columnDesignModel.groupField)) : false;
        var isChildrenRegular = formScriptDesign.Any(it => it.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && it.RegList != null && it.RegList.Count > 0);
        var treeRelationControlKey = indexSearchFieldDesign.Find(it => it.Name.Equals(columnDesignModel?.treeRelation?.Replace("-", "_")))?.poxiaoKey;

        string allThousandsField = columnDesignModel.summaryField?.Intersect(formScriptDesign.FindAll(it => it.Thousands && !it.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).Select(it => it.Name).ToList()).ToList().ToJsonString();
        bool isChildrenThousandsField = formScriptDesign.Any(it => it.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && it.Thousands);

        // 是否开启特殊属性
        var isDateSpecialAttribute = CodeGenFormControlDesignHelper.DetermineWhetherTheControlHasEnabledSpecialAttributes(controls, "date");
        var isTimeSpecialAttribute = CodeGenFormControlDesignHelper.DetermineWhetherTheControlHasEnabledSpecialAttributes(controls, "time");

        // 表单默认值控件列表
        var defaultFormControlList = new DefaultFormControlModel();
        switch (logic)
        {
            case 4:
                columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();
                defaultFormControlList = CodeGenFormControlDesignHelper.DefaultFormControlList(controls, columnDesignModel.searchList);
                break;
            case 5:
                ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();
                defaultFormControlList = CodeGenFormControlDesignHelper.DefaultFormControlList(controls, columnDesignModel.searchList);

                // 移动端的分页遵循PC端
                columnDesignModel.hasPage = templateEntity.WebType == 1 ? false : pcColumnDesignModel.hasPage;
                break;
        }

        var isDefaultFormControl = defaultFormControlList.IsExistTime || defaultFormControlList.IsExistDate || defaultFormControlList.IsExistDepSelect || defaultFormControlList.IsExistComSelect || defaultFormControlList.IsExistUserSelect || defaultFormControlList.IsExistSubTable ? true : false;

        switch (columnDesignModel.type)
        {
            case 3:
            case 5:
                columnDesignModel.hasPage = false;
                break;
        }

        switch (templateEntity.WebType)
        {
            case 1:
                return new FrontEndGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    ClassName = formDataModel.className.FirstOrDefault(),
                    FormRef = formDataModel.formRef,
                    FormModel = formDataModel.formModel,
                    Size = formDataModel.size,
                    LabelPosition = formDataModel.labelPosition,
                    LabelWidth = formDataModel.labelWidth,
                    FormRules = formDataModel.formRules,
                    GeneralWidth = formDataModel.generalWidth,
                    FullScreenWidth = formDataModel.fullScreenWidth,
                    DrawerWidth = formDataModel.drawerWidth,
                    FormStyle = formDataModel.formStyle,
                    Type = columnDesignModel.type,
                    PrimaryKey = tableColumns?.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName,
                    FormList = formScriptDesign,
                    PopupType = formDataModel.popupType,
                    OptionsList = indnxControlOption,
                    IsRemoveDel = isRemoveDel,
                    IsDetail = isDetail,
                    IsEdit = isEdit,
                    IsAdd = isAdd,
                    IsSort = isSort,
                    HasPage = columnDesignModel.hasPage,
                    FormAllContols = formControlList,
                    CancelButtonText = formDataModel.cancelButtonText,
                    ConfirmButtonText = formDataModel.confirmButtonText,
                    UseBtnPermission = columnDesignModel.useBtnPermission,
                    UseColumnPermission = columnDesignModel.useColumnPermission,
                    UseFormPermission = columnDesignModel.useFormPermission,
                    IsSummary = isSummary,
                    PageSize = columnDesignModel.pageSize,
                    Sort = columnDesignModel.sort,
                    HasPrintBtn = formDataModel.hasPrintBtn,
                    PrintButtonText = formDataModel.printButtonText,
                    PrintId = formDataModel.printId != null ? string.Join(",", formDataModel.printId) : null,
                    IsChildDataTransfer = formScriptDesign.Any(it => it.IsDataTransfer.Equals(true)),
                    IsChildTableQuery = indexSearchFieldDesign.Any(it => it.IsChildQuery.Equals(true)),
                    IsChildTableShow = indexColumnDesign.Any(it => it.IsChildTable.Equals(true)),
                    ColumnList = columnDesignModel.columnList.ToJsonString(),
                    IsInlineEditor = isInlineEditor,
                    GroupField = columnDesignModel.groupField,
                    GroupShowField = columnDesignModel?.columnList?.Where(x => x.__vModel__.ToLower() != columnDesignModel?.groupField?.ToLower()).FirstOrDefault()?.__vModel__,
                    PrimaryKeyPolicy = formDataModel.primaryKeyPolicy,
                    IsRelationForm = isRelationForm,
                    ChildTableStyle = columnDesignModel.childTableStyle,
                    IsChildrenRegular = isChildrenRegular,
                    DefaultFormControlList = defaultFormControlList,
                    IsDefaultFormControl = isDefaultFormControl,
                    PropertyJson = propertyJson,
                    FormRealControl = formRealControl,
                    IsDateSpecialAttribute = isDateSpecialAttribute,
                    IsTimeSpecialAttribute = isTimeSpecialAttribute,
                    IsChildrenThousandsField = isChildrenThousandsField,
                };
                break;
            default:
                var codeGenColumnData = new CodeGenColumnData
                {
                    treeInterfaceId = columnDesignModel.treeInterfaceId,
                    treeTemplateJson = columnDesignModel.treeTemplateJson
                };
                return new FrontEndGenConfigModel()
                {
                    PrintIds = printIds,
                    NameSpace = formDataModel.areasName,
                    ClassName = formDataModel.className.FirstOrDefault(),
                    FormRef = formDataModel.formRef,
                    FormModel = formDataModel.formModel,
                    Size = formDataModel.size,
                    LabelPosition = formDataModel.labelPosition,
                    LabelWidth = formDataModel.labelWidth,
                    FormRules = formDataModel.formRules,
                    GeneralWidth = formDataModel.generalWidth,
                    FullScreenWidth = formDataModel.fullScreenWidth,
                    DrawerWidth = formDataModel.drawerWidth,
                    FormStyle = formDataModel.formStyle,
                    Type = columnDesignModel.type,
                    TreeRelation = columnDesignModel?.treeRelation?.Replace("-", "_"),
                    TreeTitle = columnDesignModel?.treeTitle,
                    TreePropsValue = columnDesignModel?.treePropsValue,
                    TreeDataSource = columnDesignModel?.treeDataSource,
                    TreeDictionary = columnDesignModel?.treeDictionary,
                    TreePropsUrl = columnDesignModel?.treePropsUrl,
                    TreePropsChildren = columnDesignModel?.treePropsChildren,
                    TreePropsLabel = columnDesignModel?.treePropsLabel,
                    TreeRelationControlKey = treeRelationControlKey,
                    IsTreeRelationMultiple = isTreeRelationMultiple,
                    IsExistQuery = templateEntity.Type == 3 ? false : (bool)columnDesignModel?.searchList?.Any(it => it.prop.Equals(columnDesignModel?.treeRelation)),
                    PrimaryKey = tableColumns?.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName,
                    FormList = formScriptDesign,
                    PopupType = formDataModel.popupType,
                    SearchColumnDesign = indexSearchFieldDesign,
                    TopButtonDesign = indexTopButton,
                    ColumnButtonDesign = indexColumnButtonDesign,
                    ColumnDesign = indexColumnDesign,
                    OptionsList = indnxControlOption,
                    IsBatchRemoveDel = isBatchRemoveDel,
                    IsBatchPrint = isBatchPrint,
                    IsDownload = isDownload,
                    IsRemoveDel = isRemoveDel,
                    IsDetail = isDetail,
                    IsEdit = isEdit,
                    IsAdd = isAdd,
                    IsUpload = isUpload,
                    IsSort = isSort,
                    HasPage = columnDesignModel.hasPage,
                    FormAllContols = formControlList,
                    CancelButtonText = formDataModel.cancelButtonText,
                    ConfirmButtonText = formDataModel.confirmButtonText,
                    UseBtnPermission = columnDesignModel.useBtnPermission,
                    UseColumnPermission = columnDesignModel.useColumnPermission,
                    UseFormPermission = columnDesignModel.useFormPermission,
                    IsSummary = isSummary,
                    PageSize = columnDesignModel.pageSize,
                    Sort = columnDesignModel.sort,
                    HasPrintBtn = formDataModel.hasPrintBtn,
                    PrintButtonText = formDataModel.printButtonText,
                    PrintId = formDataModel.printId != null ? string.Join(",", formDataModel.printId) : null,
                    IsChildDataTransfer = formScriptDesign.Any(it => it.IsDataTransfer.Equals(true)),
                    IsChildTableQuery = indexSearchFieldDesign.Any(it => it.IsChildQuery.Equals(true)),
                    IsChildTableShow = indexColumnDesign.Any(it => it.IsChildTable.Equals(true)),
                    ColumnList = columnDesignModel.columnList.ToJsonString(),
                    HasSuperQuery = columnDesignModel.hasSuperQuery,
                    ColumnOptions = columnDesignModel.columnOptions.ToJsonString(),
                    IsInlineEditor = isInlineEditor,
                    GroupField = columnDesignModel.groupField,
                    GroupShowField = columnDesignModel?.columnList?.Where(x => x.__vModel__.ToLower() != columnDesignModel?.groupField?.ToLower()).FirstOrDefault()?.__vModel__,
                    PrimaryKeyPolicy = formDataModel.primaryKeyPolicy,
                    IsRelationForm = isRelationForm,
                    ChildTableStyle = columnDesignModel.childTableStyle,
                    IsFixed = isFixed,
                    IsChildrenRegular = isChildrenRegular,
                    TreeSynType = columnDesignModel.treeSynType,
                    HasTreeQuery = columnDesignModel.hasTreeQuery,
                    ColumnData = codeGenColumnData,
                    SummaryField = columnDesignModel.summaryField,
                    ShowSummary = columnDesignModel.showSummary,
                    DefaultFormControlList = defaultFormControlList,
                    IsDefaultFormControl = isDefaultFormControl,
                    PropertyJson = propertyJson,
                    FormRealControl = formRealControl,
                    QueryCriteriaQueryVarianceList = queryCriteriaQueryVarianceList,
                    IsDateSpecialAttribute = isDateSpecialAttribute,
                    IsTimeSpecialAttribute = isTimeSpecialAttribute,
                    AllThousandsField = allThousandsField,
                    IsChildrenThousandsField = isChildrenThousandsField,
                    SpecifyDateFormatSet = specifyDateFormatSet,
                    AppThousandField = appThousandField,
                };
                break;
        }
    }

    /// <summary>
    /// 多端查询合并.
    /// </summary>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="controls">移除布局演示后的表单全控件.</param>
    /// <returns></returns>
    public static List<IndexSearchFieldModel> GetMultiEndQueryMerging(VisualDevEntity templateEntity, List<FieldsModel> controls = null)
    {
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();

        if (templateEntity.Type != 3 && controls != null && pcColumnDesignModel?.type == 2 && (pcColumnDesignModel.searchList.Count == 0 || !pcColumnDesignModel.searchList.Any(it => it.prop.Equals(pcColumnDesignModel.treeRelation))))
        {
            var search = new FieldsModel();
            // 左侧树关联字段是否为子表字段
            switch (pcColumnDesignModel.treeRelation.StartsWith("tableField"))
            {
                case true:
                    foreach (var item in controls.FindAll(it => it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)))
                    {
                        search = item.__config__.children.Find(x => x.__vModel__.Equals(pcColumnDesignModel.treeRelation.Replace(string.Format("{0}-", item.__vModel__), "")));
                        if (search != null)
                        {
                            pcColumnDesignModel.searchList.Add(new IndexSearchFieldModel
                            {
                                label = search.__config__.label,
                                prop = string.Format("{0}-{1}", item.__vModel__, search.__vModel__),
                                poxiaoKey = search.__config__.poxiaoKey,
                                searchType = 1,
                                __vModel__ = string.Format("{0}-{1}", item.__vModel__, search.__vModel__),
                            });
                            continue;
                        }
                    }
                    break;
                default:
                    search = controls.Find(x => x.__vModel__.Equals(pcColumnDesignModel.treeRelation));
                    pcColumnDesignModel.searchList.Add(new IndexSearchFieldModel
                    {
                        label = search.__config__.label,
                        prop = search.__vModel__,
                        poxiaoKey = search.__config__.poxiaoKey,
                        searchType = 1,
                        __vModel__ = search.__vModel__
                    });
                    break;
            }
        }

        var newSearchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        newSearchList?.ForEach(item =>
        {
            var config = item.__config__;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.SELECT:
                case PoxiaoKeyConst.DEPSELECT:
                case PoxiaoKeyConst.ROLESELECT:
                case PoxiaoKeyConst.USERSELECT:
                case PoxiaoKeyConst.USERSSELECT:
                case PoxiaoKeyConst.COMSELECT:
                case PoxiaoKeyConst.POSSELECT:
                case PoxiaoKeyConst.GROUPSELECT:
                    var pc = (pcColumnDesignModel?.searchList.Find(it => it.prop.Equals(item.prop))?.searchMultiple).ParseToBool();
                    var app = (appColumnDesignModel?.searchList.Find(it => it.prop.Equals(item.prop))?.searchMultiple).ParseToBool();
                    if (pc && app)
                        item.searchMultiple = true;
                    else if (pc || !app)
                        item.searchMultiple = true;
                    else if (!pc || app)
                        item.searchMultiple = true;
                    else
                        item.searchMultiple = false;
                    break;
            }
        });
        return newSearchList;
    }

    /// <summary>
    /// 多端列表展示合并.
    /// </summary>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static List<IndexGridFieldModel> GetMultiTerminalListDisplayAndConsolidation(VisualDevEntity templateEntity)
    {
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        return pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
    }

    /// <summary>
    /// 代码生成配置模型.
    /// </summary>
    /// <param name="formDataModel">表单Json包.</param>
    /// <param name="columnDesignModel">列设计模型.</param>
    /// <param name="tableColumnList">数据库表列.</param>
    /// <param name="controls">表单控件列表.</param>
    /// <param name="tableName">表名称.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel GetCodeGenConfigModel(FormDataModel formDataModel, ColumnDesignModel columnDesignModel, List<TableColumnConfigModel> tableColumnList, List<FieldsModel> controls, string tableName, VisualDevEntity templateEntity)
    {
        // 默认排序 没设置以ID排序.
        var defaultSidx = string.Empty;

        // 是否导出
        bool isExport = false;

        // 是否批量删除
        bool isBatchRemove = false;

        // 是否查询条件多选
        bool isSearchMultiple = false;

        // 是否树形表格
        bool isTreeTable = false;

        // 树形表格-父级字段
        string parentField = string.Empty;

        // 树形表格-显示字段
        string treeShowField = string.Empty;

        switch (templateEntity.WebType)
        {
            case 2:
                // 默认排序 没设置以ID排序.
                defaultSidx = columnDesignModel.defaultSidx ?? tableColumnList.Find(t => t.PrimaryKey).ColumnName;
                isExport = columnDesignModel.btnsList.Any(it => it.value == "download");
                isBatchRemove = columnDesignModel.btnsList.Any(it => it.value == "batchRemove");
                isSearchMultiple = tableColumnList.Any(it => it.QueryMultiple && !it.IsAuxiliary);
                break;
        }

        switch (columnDesignModel.type)
        {
            case 5:
                isTreeTable = true;
                parentField = string.Format("{0}_pid", columnDesignModel.parentField);
                treeShowField = columnDesignModel.columnList.Find(it => it.__vModel__.ToLower() != columnDesignModel.parentField.ToLower()).__vModel__;
                break;
            default:
                break;
        }

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.poxiaoKey != null && (it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADIMG) || it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = tableColumnList.Any(it => it.poxiaoKey != null && (it.poxiaoKey.Equals(PoxiaoKeyConst.CHECKBOX) || it.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || it.poxiaoKey.Equals(PoxiaoKeyConst.ADDRESS) || it.poxiaoKey.Equals(PoxiaoKeyConst.COMSELECT) || it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADIMG) || it.poxiaoKey.Equals(PoxiaoKeyConst.UPLOADFZ) || (it.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && it.IsMultiple) || (it.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) && it.IsMultiple) || (it.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) && it.IsMultiple) || (it.poxiaoKey.Equals(PoxiaoKeyConst.DEPSELECT) && it.IsMultiple) || (it.poxiaoKey.Equals(PoxiaoKeyConst.POSSELECT) && it.IsMultiple)));

        // 是否存在单据规则控件
        bool isBillRule = controls.Any(it => it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.BILLRULE));

        bool isSystemControl = tableColumnList.Any(it => it.IsSystemControl);

        bool isUpdate = tableColumnList.Any(it => it.IsUpdate);

        bool isLogicalDelete = formDataModel.logicalDelete;

        List<CodeGenFunctionModel> function = new List<CodeGenFunctionModel>();

        switch (templateEntity.Type)
        {
            case 3:
                function = CodeGenFunctionHelper.GetPureFormWithProcessMethod();
                break;
            default:
                // 是否启用流程
                switch (templateEntity.EnableFlow)
                {
                    case 1:
                        switch (templateEntity.WebType)
                        {
                            case 1:
                                function = CodeGenFunctionHelper.GetPureFormWithProcessMethod();
                                break;
                            case 2:
                                columnDesignModel.btnsList.RemoveAll(it => it.value.Equals("add"));
                                columnDesignModel.btnsList.Add(new ButtonConfigModel()
                                {
                                    value = "save",
                                });
                                columnDesignModel.columnBtnsList.RemoveAll(it => it.value.Equals("edit"));
                                function = CodeGenFunctionHelper.GetGeneralListWithProcessMethod(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList);
                                break;
                        }

                        break;
                    default:
                        switch (templateEntity.WebType)
                        {
                            case 1:
                                function = CodeGenFunctionHelper.GetPureFormMethod();
                                break;
                            default:
                                function = CodeGenFunctionHelper.GetGeneralListMethod(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList);
                                break;
                        }
                        break;
                }
                break;
        }

        // 树形表格不管有没有导出 强行开双列表(分页与无分页接口)
        switch (columnDesignModel.type)
        {
            case 5:
                switch (function.Any(it => it.FullName.Equals("page") && it.FullName.Equals("noPage")))
                {
                    case false:
                        switch (function.Any(it => it.FullName.Equals("page")))
                        {
                            case true:
                                function.Add(new CodeGenFunctionModel()
                                {
                                    FullName = "noPage",
                                    IsInterface = true,
                                    orderBy = 3,
                                });
                                break;
                            default:
                                function.Add(new CodeGenFunctionModel()
                                {
                                    FullName = "page",
                                    IsInterface = true,
                                    orderBy = 3,
                                });
                                break;
                        }
                        break;
                    case true:
                        function.FindAll(it => it.FullName.Equals("page") || it.FullName.Equals("noPage")).ForEach(item =>
                        {
                            item.IsInterface = true;
                        });
                        break;
                }
                break;
        }

        return new CodeGenConfigModel()
        {
            NameSpace = formDataModel.areasName,
            BusName = templateEntity.FullName,
            ClassName = formDataModel.className.FirstOrDefault(),
            PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
            OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
            MainTable = tableName.ParseToPascalCase(),
            OriginalMainTableName = tableName,
            hasPage = columnDesignModel.hasPage,
            Function = function,
            TableField = tableColumnList,
            DefaultSidx = defaultSidx,
            IsExport = isExport,
            IsBatchRemove = isBatchRemove,
            IsUpload = isUpload,
            IsTableRelations = false,
            IsMapper = isMapper,
            IsBillRule = isBillRule,
            DbLinkId = templateEntity.DbLinkId,
            FormId = templateEntity.Id,
            WebType = templateEntity.WebType,
            Type = templateEntity.Type,
            EnableFlow = templateEntity.EnableFlow.ParseToBool(),
            IsMainTable = true,
            EnCode = templateEntity.EnCode,
            UseDataPermission = (bool)columnDesignModel?.useDataPermission,
            SearchControlNum = tableColumnList.FindAll(it => it.QueryType.Equals(1) || it.QueryType.Equals(2)).Count(),
            IsAuxiliaryTable = false,
            ExportField = templateEntity.Type == 3 || templateEntity.WebType == 1 ? null : CodeGenExportFieldHelper.ExportColumnField(columnDesignModel?.columnList),
            FullName = templateEntity.FullName,
            IsConversion = tableColumnList.Any(it => it.IsConversion.Equals(true)),
            PrimaryKeyPolicy = formDataModel.primaryKeyPolicy,
            ConcurrencyLock = formDataModel.concurrencyLock,
            HasSuperQuery = columnDesignModel.hasSuperQuery,
            IsUnique = tableColumnList.Any(it => it.IsUnique),
            GroupField = columnDesignModel?.groupField,
            GroupShowField = columnDesignModel?.columnList?.Where(x => x.__vModel__.ToLower() != columnDesignModel?.groupField?.ToLower()).FirstOrDefault()?.__vModel__,
            IsImportData = tableColumnList.Any(it => it.IsImportField.Equals(true)),
            ParsPoxiaoKeyConstList = CodeGenControlsAttributeHelper.GetParsPoxiaoKeyConstList(controls, (bool)columnDesignModel?.type.Equals(4)),
            ParsPoxiaoKeyConstListDetails = CodeGenControlsAttributeHelper.GetParsPoxiaoKeyConstListDetails(controls),
            ImportDataType = columnDesignModel?.uploaderTemplateJson?.dataType,
            IsSystemControl = isSystemControl,
            IsUpdate = isUpdate,
            IsSearchMultiple = isSearchMultiple,
            IsTreeTable = isTreeTable,
            ParentField = parentField,
            TreeShowField = treeShowField,
            IsLogicalDelete = isLogicalDelete,
            TableType = columnDesignModel.type,
        };
    }
}