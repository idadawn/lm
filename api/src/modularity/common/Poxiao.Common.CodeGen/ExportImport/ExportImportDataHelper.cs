using System.Reflection;
using System.Text.RegularExpressions;
using Poxiao.Infrastructure.CodeGenUpload;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Dtos.VisualDev;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Helper;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Models.NPOI;
using Poxiao.Infrastructure.Security;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.FriendlyException;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Model.DataInterFace;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.VisualDev;
using Poxiao.VisualDev.Engine;
using Poxiao.VisualDev.Engine.Core;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Interfaces.Service;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace Poxiao.Infrastructure.CodeGen.ExportImport;

/// <summary>
/// 代码生成导出数据帮助类.
/// </summary>
public class ExportImportDataHelper : ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<OrganizeEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 在线开发运行服务.
    /// </summary>
    private readonly RunService _runService;

    /// <summary>
    /// 模板表单列表数据解析.
    /// </summary>
    private readonly FormDataParsing _formDataParsing;

    /// <summary>
    /// 单据.
    /// </summary>
    private readonly IBillRullService _billRuleService;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _databaseService;

    /// <summary>
    /// 数据接口.
    /// </summary>
    private readonly IDataInterfaceService _dataInterfaceService;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 工作流.
    /// </summary>
    private readonly IFlowTaskService _flowTaskService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public ExportImportDataHelper(
        ISqlSugarRepository<OrganizeEntity> repositoryRepository,
        IUserManager userManager,
        RunService runService,
        FormDataParsing formDataParsing,
        IBillRullService billRuleService,
        IDataInterfaceService dataInterfaceService,
        IDataBaseManager databaseService,
        ICacheManager cacheManager,
        IFlowTaskService flowTaskService,
        IFileManager fileManager,
        ISqlSugarClient context)
    {
        _repository = repositoryRepository;
        _sqlSugarClient = (SqlSugarScope)context;
        _billRuleService = billRuleService;
        _databaseService = databaseService;
        _dataInterfaceService = dataInterfaceService;
        _runService = runService;
        _formDataParsing = formDataParsing;
        _cacheManager = cacheManager;
        _flowTaskService = flowTaskService;
        _fileManager = fileManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 组装导出带子表的数据,返回 第一个合并行标头,第二个导出数据.
    /// </summary>
    /// <param name="selectKey">导出选择列.</param>
    /// <param name="realList">原数据集合.</param>
    /// <param name="paramsModels">模板信息.</param>
    /// <returns>第一行标头 , 导出数据.</returns>
    public static object[] GetCreateFirstColumnsHeader(List<string> selectKey, List<Dictionary<string, object>> realList, List<ParamsModel> paramsModels)
    {
        selectKey.ForEach(item =>
        {
            realList.ForEach(it =>
            {
                if (!it.ContainsKey(item)) it.Add(item, string.Empty);
            });
        });

        var newRealList = realList.Copy();

        realList.ForEach(items =>
        {
            var rowChildDatas = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var item in items)
            {
                if (item.Value != null && item.Key.ToLower().Contains("tablefield") && (item.Value is List<Dictionary<string, object>> || item.Value.GetType().Name.Equals("JArray")))
                {
                    var ctList = item.Value.ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
                    rowChildDatas.Add(item.Key, ctList);
                }
            }

            var len = rowChildDatas.Select(x => x.Value.Count()).OrderByDescending(x => x).FirstOrDefault();

            if (len != null && len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    if (i == 0)
                    {
                        var newRealItem = newRealList.Find(x => x["id"].Equals(items["id"]));
                        foreach (var cData in rowChildDatas)
                        {
                            var itemData = cData.Value.FirstOrDefault();
                            if (itemData != null)
                            {
                                foreach (var key in itemData)
                                    if (newRealItem.ContainsKey(cData.Key + "-" + key.Key)) newRealItem[cData.Key + "-" + key.Key] = key.Value;
                            }
                        }
                    }
                    else
                    {
                        var newRealItem = new Dictionary<string, object>();
                        foreach (var it in items)
                        {
                            if (it.Key.Equals("id")) newRealItem.Add(it.Key, it.Value);
                            else newRealItem.Add(it.Key, string.Empty);
                        }
                        foreach (var cData in rowChildDatas)
                        {
                            if (cData.Value.Count > i)
                            {
                                foreach (var it in cData.Value[i])
                                    if (newRealItem.ContainsKey(cData.Key + "-" + it.Key)) newRealItem[cData.Key + "-" + it.Key] = it.Value;
                            }
                        }
                        newRealList.Add(newRealItem);
                    }
                }
            }
        });

        var resultList = new List<Dictionary<string, object>>();

        newRealList.ForEach(newRealItem =>
        {
            if (!resultList.Any(x => x["id"].Equals(newRealItem["id"]))) resultList.AddRange(newRealList.Where(x => x["id"].Equals(newRealItem["id"])).ToList());
        });

        var firstColumns = new Dictionary<string, int>();

        if (selectKey.Any(x => x.Contains("-") && x.ToLower().Contains("tablefield")))
        {
            var empty = string.Empty;
            var keyList = selectKey.Select(x => x.Split("-").First()).Distinct().ToList();
            var mainFieldIndex = 1;
            keyList.ForEach(item =>
            {
                if (item.ToLower().Contains("tablefield"))
                {
                    var title = paramsModels.FirstOrDefault(x => x.field.Contains(item))?.value.Split("-")[0];
                    firstColumns.Add(title + empty, selectKey.Count(x => x.Contains(item)));
                    empty += " ";
                    mainFieldIndex = 1;
                }
                else
                {
                    if (mainFieldIndex == 1) empty += " ";
                    if (!firstColumns.ContainsKey(empty)) firstColumns.Add(empty, mainFieldIndex);
                    else firstColumns[empty] = mainFieldIndex;
                    mainFieldIndex++;
                }
            });
        }

        return new object[] { firstColumns, resultList };
    }

    /// <summary>
    /// 数据导出通用.
    /// </summary>
    /// <param name="fileName">导出文件名.</param>
    /// <param name="selectKey">selectKey.</param>
    /// <param name="userId">用户ID.</param>
    /// <param name="realList">数据集合.</param>
    /// <param name="paramList">参数.</param>
    /// <param name="isGroupTable">是否分组表格.</param>
    /// <param name="isInlineEditor">是否行内编辑.</param>
    /// <returns></returns>
    public static dynamic GetDataExport(string fileName, string selectKey, string userId, List<Dictionary<string, object>> realList, List<ParamsModel> paramList, bool isGroupTable = false, bool isInlineEditor = false)
    {
        switch (isInlineEditor)
        {
            case true:
                paramList.ForEach(item =>
                {
                    item.field = string.Format("{0}_name", item.field);
                });
                break;
        }

        var res = GetCreateFirstColumnsHeader(selectKey.Split(',').ToList(), realList, paramList);
        var firstColumns = res.First().ToJsonString().ToObjectOld<Dictionary<string, int>>();
        var resultList = res.Last().ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
        List<string> newSelectKey = selectKey.Split(',').ToList();

        // 如果是 分组表格 类型
        if (isGroupTable)
        {
            List<Dictionary<string, object>>? newValueList = new List<Dictionary<string, object>>();
            realList.ForEach(item =>
            {
                List<Dictionary<string, object>>? tt = item["children"].ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
                newValueList.AddRange(tt);
            });
            realList = newValueList;
        }

        List<ParamsModel> newParamList = new List<ParamsModel>();

        // 全部参数顺序
        foreach (var item in firstColumns)
        {
            Regex re = new Regex(@"[\u4e00-\u9fa5]+");
            switch (re.IsMatch(item.Key))
            {
                case false:
                    {
                        var param = newSelectKey.GetRange(0, item.Value);
                        newParamList.AddRange(paramList.FindAll(it => param.Contains(it.field)));
                        newSelectKey.RemoveAll(it => newParamList.Select(it => it.field).ToList().Contains(it));
                    }
                    break;
                default:
                    var childTable = paramList.FindAll(it => it.value.Contains(item.Key.TrimEnd(' ')));
                    childTable = childTable.FindAll(it => selectKey.Split(',').ToList().Contains(it.field));
                    newParamList.AddRange(childTable);
                    newSelectKey.RemoveAll(it => newParamList.Select(it => it.field).ToList().Contains(it));
                    break;
            }
        }

        if (newParamList.Count > 0) newSelectKey = newParamList.Select(it => it.field).ToList();

        try
        {
            List<string> columnList = new List<string>();
            ExcelConfig excelconfig = new ExcelConfig();
            excelconfig.FileName = string.Format("{0}.xls", fileName);
            excelconfig.HeadFont = "微软雅黑";
            excelconfig.HeadPoint = 10;
            excelconfig.IsAllSizeColumn = true;
            excelconfig.ColumnModel = new List<ExcelColumnModel>();
            foreach (var item in newSelectKey)
            {
                ParamsModel isExist = new ParamsModel();
                switch (isInlineEditor)
                {
                    case true:
                        isExist = paramList.Find(p => p.field.Equals(string.Format("{0}_name", item)));
                        break;
                    default:
                        isExist = paramList.Find(p => p.field == item);
                        break;
                }
                if (isExist != null)
                {
                    excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
                    columnList.Add(isExist.value);
                }
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = (firstColumns == null || firstColumns.Count() < 1) ? ExcelExportHelper<Dictionary<string, object>>.ExportMemoryStream(realList, excelconfig, columnList) : ExcelExportHelper<Dictionary<string, object>>.ExportMemoryStream(realList, excelconfig, columnList, firstColumns);
            ExcelExportHelper<Dictionary<string, object>>.Export(fs, addPath);
            var fName = userId + "|" + addPath + "|xls";
            return new {
                name = excelconfig.FileName,
                url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fName, "Poxiao")
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 获取模板解析.
    /// </summary>
    public static List<FieldsModel> GetTemplateParsing<T>(T entity)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        foreach (PropertyInfo prop in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            FieldsModel field = new FieldsModel();
            foreach (var att in prop.GetCustomAttributes(false))
            {
                if (att is CodeGenUploadAttribute)
                {
                    var configModel = new ConfigModel();
                    field.__vModel__ = (att as CodeGenUploadAttribute).__Model__;
                    field.level = (att as CodeGenUploadAttribute).level;
                    field.min = (att as CodeGenUploadAttribute).min == 0 ? null : (att as CodeGenUploadAttribute).min;
                    field.max = (att as CodeGenUploadAttribute).max == 0 ? null : (att as CodeGenUploadAttribute).max;
                    field.activeTxt = (att as CodeGenUploadAttribute).activeTxt;
                    field.inactiveTxt = (att as CodeGenUploadAttribute).inactiveTxt;
                    field.format = (att as CodeGenUploadAttribute).format;
                    field.multiple = (att as CodeGenUploadAttribute).multiple;
                    field.separator = (att as CodeGenUploadAttribute).separator;
                    field.props = (att as CodeGenUploadAttribute).props.Adapt<PropsModel>();
                    field.options = (att as CodeGenUploadAttribute).options;
                    field.propsValue = (att as CodeGenUploadAttribute).propsValue;
                    field.relationField = (att as CodeGenUploadAttribute).relationField;
                    field.modelId = (att as CodeGenUploadAttribute).modelId;
                    field.interfaceId = (att as CodeGenUploadAttribute).interfaceId;
                    field.selectType = (att as CodeGenUploadAttribute).selectType;
                    field.ableDepIds = (att as CodeGenUploadAttribute).ableDepIds;
                    field.ablePosIds = (att as CodeGenUploadAttribute).ablePosIds;
                    field.ableUserIds = (att as CodeGenUploadAttribute).ableUserIds;
                    field.ableRoleIds = (att as CodeGenUploadAttribute).ableRoleIds;
                    field.ableGroupIds = (att as CodeGenUploadAttribute).ableGroupIds;
                    field.ableIds = (att as CodeGenUploadAttribute).ableIds;
                    field.relational = (att as CodeGenUploadAttribute).showField;
                    configModel = (att as CodeGenUploadAttribute).__config__.Adapt<ConfigModel>();
                    configModel.label = string.Format("{0}({1})", configModel.label, field.__vModel__);
                    field.__config__ = configModel;
                    fieldList.Add(field);
                }
            }
        }
        return fieldList;
    }

    /// <summary>
    /// 获取数据转换模板解析.
    /// </summary>
    public static List<FieldsModel> GetDataConversionTemplateParsing<T>(T entity)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        foreach (PropertyInfo prop in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            FieldsModel field = new FieldsModel();
            foreach (var att in prop.GetCustomAttributes(false))
            {
                if (att is CodeGenUploadAttribute)
                {
                    var configModel = new ConfigModel();
                    field.__vModel__ = (att as CodeGenUploadAttribute).__vModel__ ?? (att as CodeGenUploadAttribute).__Model__;
                    field.level = (att as CodeGenUploadAttribute).level;
                    field.min = (att as CodeGenUploadAttribute).min == 0 ? null : (att as CodeGenUploadAttribute).min;
                    field.max = (att as CodeGenUploadAttribute).max == 0 ? null : (att as CodeGenUploadAttribute).max;
                    field.activeTxt = (att as CodeGenUploadAttribute).activeTxt;
                    field.inactiveTxt = (att as CodeGenUploadAttribute).inactiveTxt;
                    field.format = (att as CodeGenUploadAttribute).format;
                    field.multiple = (att as CodeGenUploadAttribute).multiple;
                    field.separator = (att as CodeGenUploadAttribute).separator;
                    field.props = (att as CodeGenUploadAttribute).props.Adapt<PropsModel>();
                    field.options = (att as CodeGenUploadAttribute).options;
                    field.propsValue = (att as CodeGenUploadAttribute).propsValue;
                    field.relationField = (att as CodeGenUploadAttribute).relationField;
                    field.modelId = (att as CodeGenUploadAttribute).modelId;
                    field.interfaceId = (att as CodeGenUploadAttribute).interfaceId;
                    field.selectType = (att as CodeGenUploadAttribute).selectType;
                    field.ableDepIds = (att as CodeGenUploadAttribute).ableDepIds;
                    field.ablePosIds = (att as CodeGenUploadAttribute).ablePosIds;
                    field.ableUserIds = (att as CodeGenUploadAttribute).ableUserIds;
                    field.ableRoleIds = (att as CodeGenUploadAttribute).ableRoleIds;
                    field.ableGroupIds = (att as CodeGenUploadAttribute).ableGroupIds;
                    field.ableIds = (att as CodeGenUploadAttribute).ableIds;
                    field.relational = (att as CodeGenUploadAttribute).showField;
                    configModel = (att as CodeGenUploadAttribute).__config__.Adapt<ConfigModel>();
                    configModel.label = string.Format("{0}({1})", configModel.label, field.__vModel__);
                    field.__config__ = configModel;
                    fieldList.Add(field);
                }
            }
        }
        return fieldList;
    }

    /// <summary>
    /// 获取模板解析.
    /// </summary>
    public static List<FieldsModel> GetTemplateParsing<T>(T entity, string tableName)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        foreach (PropertyInfo prop in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            FieldsModel field = new FieldsModel();
            foreach (var att in prop.GetCustomAttributes(false))
            {
                if (att is CodeGenUploadAttribute)
                {
                    var configModel = new ConfigModel();
                    field.__vModel__ = string.Format("poxiao_{0}_poxiao_{1}", tableName, (att as CodeGenUploadAttribute).__Model__);
                    field.level = (att as CodeGenUploadAttribute).level;
                    field.min = (att as CodeGenUploadAttribute).min == 0 ? null : (att as CodeGenUploadAttribute).min;
                    field.max = (att as CodeGenUploadAttribute).max == 0 ? null : (att as CodeGenUploadAttribute).max;
                    field.activeTxt = (att as CodeGenUploadAttribute).activeTxt;
                    field.inactiveTxt = (att as CodeGenUploadAttribute).inactiveTxt;
                    field.format = (att as CodeGenUploadAttribute).format;
                    field.multiple = (att as CodeGenUploadAttribute).multiple;
                    field.separator = (att as CodeGenUploadAttribute).separator;
                    field.props = (att as CodeGenUploadAttribute).props.Adapt<PropsModel>();
                    field.options = (att as CodeGenUploadAttribute).options;
                    field.propsValue = (att as CodeGenUploadAttribute).propsValue;
                    field.relationField = (att as CodeGenUploadAttribute).relationField;
                    field.modelId = (att as CodeGenUploadAttribute).modelId;
                    field.interfaceId = (att as CodeGenUploadAttribute).interfaceId;
                    field.selectType = (att as CodeGenUploadAttribute).selectType;
                    field.ableDepIds = (att as CodeGenUploadAttribute).ableDepIds;
                    field.ablePosIds = (att as CodeGenUploadAttribute).ablePosIds;
                    field.ableUserIds = (att as CodeGenUploadAttribute).ableUserIds;
                    field.ableRoleIds = (att as CodeGenUploadAttribute).ableRoleIds;
                    field.ableGroupIds = (att as CodeGenUploadAttribute).ableGroupIds;
                    field.ableIds = (att as CodeGenUploadAttribute).ableIds;
                    field.relational = (att as CodeGenUploadAttribute).showField;
                    configModel = (att as CodeGenUploadAttribute).__config__.Adapt<ConfigModel>();
                    configModel.label = string.Format("{0}({1})", configModel.label, field.__vModel__);
                    field.__config__ = configModel;
                    fieldList.Add(field);
                }
            }
        }

        return fieldList;
    }

    /// <summary>
    /// 获取数据转换模板解析.
    /// </summary>
    public static List<FieldsModel> GetDataConversionTemplateParsing<T>(T entity, string tableName)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        foreach (PropertyInfo prop in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            FieldsModel field = new FieldsModel();
            foreach (var att in prop.GetCustomAttributes(false))
            {
                if (att is CodeGenUploadAttribute)
                {
                    var configModel = new ConfigModel();
                    field.__vModel__ = string.Format("poxiao_{0}_poxiao_{1}", tableName, (att as CodeGenUploadAttribute).__vModel__ ?? (att as CodeGenUploadAttribute).__Model__);
                    field.level = (att as CodeGenUploadAttribute).level;
                    field.min = (att as CodeGenUploadAttribute).min == 0 ? null : (att as CodeGenUploadAttribute).min;
                    field.max = (att as CodeGenUploadAttribute).max == 0 ? null : (att as CodeGenUploadAttribute).max;
                    field.activeTxt = (att as CodeGenUploadAttribute).activeTxt;
                    field.inactiveTxt = (att as CodeGenUploadAttribute).inactiveTxt;
                    field.format = (att as CodeGenUploadAttribute).format;
                    field.multiple = (att as CodeGenUploadAttribute).multiple;
                    field.separator = (att as CodeGenUploadAttribute).separator;
                    field.props = (att as CodeGenUploadAttribute).props.Adapt<PropsModel>();
                    field.options = (att as CodeGenUploadAttribute).options;
                    field.propsValue = (att as CodeGenUploadAttribute).propsValue;
                    field.relationField = (att as CodeGenUploadAttribute).relationField;
                    field.modelId = (att as CodeGenUploadAttribute).modelId;
                    field.interfaceId = (att as CodeGenUploadAttribute).interfaceId;
                    field.selectType = (att as CodeGenUploadAttribute).selectType;
                    field.ableDepIds = (att as CodeGenUploadAttribute).ableDepIds;
                    field.ablePosIds = (att as CodeGenUploadAttribute).ablePosIds;
                    field.ableUserIds = (att as CodeGenUploadAttribute).ableUserIds;
                    field.ableRoleIds = (att as CodeGenUploadAttribute).ableRoleIds;
                    field.ableGroupIds = (att as CodeGenUploadAttribute).ableGroupIds;
                    field.ableIds = (att as CodeGenUploadAttribute).ableIds;
                    field.relational = (att as CodeGenUploadAttribute).showField;
                    configModel = (att as CodeGenUploadAttribute).__config__.Adapt<ConfigModel>();
                    configModel.label = string.Format("{0}({1})", configModel.label, field.__vModel__);
                    field.__config__ = configModel;
                    fieldList.Add(field);
                }
            }
        }

        return fieldList;
    }

    /// <summary>
    /// 获取模板Excel头部.
    /// </summary>
    /// <typeparam name="T">对象.</typeparam>
    /// <param name="entity">实体.</param>
    /// <param name="type">1-主表,2-副表,3-子表.</param>
    /// <param name="replaceContent">替换内容.</param>
    /// <returns></returns>
    public static Dictionary<string, object> GetTemplateHeader<T>(T entity, int type, string replaceContent = default)
    {
        var dicItem = new Dictionary<string, object>();
        foreach (PropertyInfo prop in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttributes(typeof(CodeGenUploadAttribute), false).Length > 0))
        {
            foreach (var att in prop.GetCustomAttributes(typeof(CodeGenUploadAttribute), false))
            {
                string vModel = (att as CodeGenUploadAttribute).__Model__;
                CodeGenConfigModel congig = (att as CodeGenUploadAttribute).__config__.ToObject<CodeGenConfigModel>();
                bool multiple = (bool)(att as CodeGenUploadAttribute)?.multiple;
                string format = (att as CodeGenUploadAttribute)?.format;
                int level = (int)(att as CodeGenUploadAttribute)?.level;
                var dic = CodeGenHelper.CodeGenTemplate(congig.poxiaoKey, multiple, congig?.label, format, level);
                var title = string.Empty;
                switch (type)
                {
                    case 2:
                        title = string.Format("{0}(poxiao_{1}_poxiao_{2})", dic.Keys.FirstOrDefault(), replaceContent, vModel);
                        break;
                    case 3:
                        title = string.Format("{0}({1}-{2})", dic.Keys.FirstOrDefault(), replaceContent, vModel);
                        break;
                    default:
                        title = string.Format("{0}({1})", dic.Keys.FirstOrDefault(), vModel);
                        break;
                }
                dicItem[title] = dic.Values.FirstOrDefault();
            }
        }
        return dicItem;
    }

    /// <summary>
    /// 获取表数据关联.
    /// </summary>
    /// <param name="entityInfo">实体信息.</param>
    /// <param name="type">表类型.</param>
    /// <param name="tableField">外键字段.</param>
    /// <param name="relationTable">关联主表.</param>
    /// <param name="relationField">关联主键.</param>
    /// <returns></returns>
    public static DbTableRelationModel GetTableRelation(EntityInfo entityInfo, string type, string tableField = default, string relationTable = default, string relationField = default)
    {
        DbTableRelationModel model = new DbTableRelationModel();
        model.typeId = type;
        model.table = entityInfo.DbTableName;
        model.tableName = entityInfo.TableDescription;
        model.tableKey = entityInfo.Columns.Find(it => it.IsPrimarykey).DbColumnName;
        model.relationTable = relationTable;
        model.tableField = tableField;
        model.relationField = relationField;
        return model;
    }

    /// <summary>
    /// 获取导入预览返回值.
    /// </summary>
    /// <param name="tInfo"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<dynamic> GetImportPreviewData(TemplateParsingBase tInfo, string fileName)
    {
        var resData = new List<Dictionary<string, object>>();
        var headerRow = new List<dynamic>();

        var isChildTable = tInfo.selectKey.Any(x => tInfo.ChildTableFields.ContainsKey(x));
        try
        {
            var FileEncode = tInfo.AllFieldsModel.Where(x => tInfo.selectKey.Contains(x.__vModel__)).ToList();

            string? savePath = Path.Combine(FileVariable.TemporaryFilePath, fileName);

            // 得到数据
            var sr = await _fileManager.GetFileStream(savePath);
            var excelData = new System.Data.DataTable();
            if (isChildTable) excelData = ExcelImportHelper.ToDataTable(savePath, sr, 0, 1);
            else excelData = ExcelImportHelper.ToDataTable(savePath, sr);
            if (excelData.Columns.Count > tInfo.selectKey.Count) excelData.Columns.RemoveAt(tInfo.selectKey.Count);
            foreach (object? item in excelData.Columns)
            {
                excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.__config__.label == item.ToString()).FirstOrDefault()?.__vModel__;
            }

            resData = excelData.ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
            if (resData.Any())
            {
                if (isChildTable)
                {
                    var hRow = resData[1].Copy();
                    foreach (var item in hRow)
                    {
                        if (item.Key.Contains("tableField") && item.Key.Contains("-"))
                        {
                            var childVModel = item.Key.Split("-").First();
                            if (!headerRow.Any(x => x.id.Equals(childVModel)))
                            {
                                var child = new List<dynamic>();
                                hRow.Where(x => x.Key.Contains(childVModel)).ToList().ForEach(x =>
                                {
                                    child.Add(new { id = x.Key.Replace(childVModel + "-", string.Empty), fullName = x.Value.ToString().Replace(string.Format("({0})", x.Key), string.Empty) });
                                });
                                headerRow.Add(new { id = childVModel, fullName = tInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(childVModel)).__config__.label.Replace(string.Format("({0})", childVModel), string.Empty), children = child });
                            }
                        }
                        else
                        {
                            headerRow.Add(new { id = item.Key, fullName = item.Value.ToString().Replace(string.Format("({0})", item.Key), string.Empty) });
                        }
                    }
                    resData.Remove(resData.First());
                    resData.Remove(resData.First());
                }
                else
                {
                    foreach (var item in resData.First().Copy()) headerRow.Add(new { id = item.Key, fullName = item.Value.ToString().Replace(string.Format("({0})", item.Key), string.Empty) });
                    resData.Remove(resData.First());
                }
            }
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.D1410);
        }

        try
        {
            // 带子表字段数据导入
            if (isChildTable)
            {
                var newData = new List<Dictionary<string, object>>();
                var singleForm = tInfo.selectKey.Where(x => !x.Contains("tableField")).ToList();
                var childTableVModel = tInfo.AllFieldsModel.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).Select(x => x.__vModel__).ToList();
                resData.ForEach(dataItem =>
                {
                    var addItem = new Dictionary<string, object>();
                    var isNextRow = false;
                    singleForm.ForEach(item =>
                    {
                        if (dataItem[item].IsNotEmptyOrNull()) isNextRow = true;
                    });

                    // 单条数据 (多行子表数据合并)
                    if (isNextRow)
                    {
                        singleForm.ForEach(item => addItem.Add(item, dataItem[item]));

                        // 子表数据
                        childTableVModel.ForEach(item =>
                        {
                            var childAddItem = new Dictionary<string, object>();
                            tInfo.selectKey.Where(x => x.Contains(item) && x != item).ToList().ForEach(it =>
                            {
                                childAddItem.Add(it.Replace(item + "-", string.Empty), dataItem[it]);
                            });

                            addItem.Add(item, new List<Dictionary<string, object>> { childAddItem });
                        });

                        newData.Add(addItem);
                    }
                    else
                    {
                        var item = newData.LastOrDefault();
                        if (item != null)
                        {
                            // 子表数据
                            childTableVModel.ForEach(citem =>
                            {
                                var childAddItem = new Dictionary<string, object>();
                                tInfo.selectKey.Where(x => x.Contains(citem)).ToList().ForEach(it =>
                                {
                                    if (dataItem.ContainsKey(it)) childAddItem.Add(it.Replace(citem + "-", string.Empty), dataItem[it]);
                                });

                                if (!item.ContainsKey(citem))
                                {
                                    item.Add(citem, new List<Dictionary<string, object>> { childAddItem });
                                }
                                else
                                {
                                    var childList = item[citem].ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
                                    childList.Add(childAddItem);
                                    item[citem] = childList;
                                }
                            });
                        }
                        else
                        {
                            singleForm.ForEach(item => addItem.Add(item, dataItem[item]));

                            // 子表数据
                            childTableVModel.ForEach(item =>
                            {
                                var childAddItem = new Dictionary<string, object>();
                                tInfo.selectKey.Where(x => x.Contains(item)).ToList().ForEach(it =>
                                {
                                    if (dataItem.ContainsKey(it)) childAddItem.Add(it.Replace(item + "-", string.Empty), dataItem[it]);
                                });

                                addItem.Add(item, new List<Dictionary<string, object>> { childAddItem });
                            });

                            newData.Add(addItem);
                        }
                    }
                });
                resData = newData;
            }
        }
        catch
        {
            throw Oops.Oh(ErrorCode.D1412);
        }

        resData.ForEach(items =>
        {
            foreach (var item in items)
            {
                var vmodel = tInfo.AllFieldsModel.FirstOrDefault(x => x.__vModel__.Equals(item.Key));
                if (vmodel != null && vmodel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.DATE) && item.Value.IsNotEmptyOrNull())
                    items[item.Key] = string.Format("{0:" + vmodel.format + "} ", item.Value);
                else if (vmodel != null && vmodel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && item.Value.IsNotEmptyOrNull())
                {
                    var ctList = item.Value.ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();
                    ctList.ForEach(ctItems =>
                    {
                        foreach (var ctItem in ctItems)
                        {
                            var ctVmodel = tInfo.AllFieldsModel.FirstOrDefault(x => x.__vModel__.Equals(vmodel.__vModel__ + "-" + ctItem.Key));
                            if (ctVmodel != null && ctVmodel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.DATE) && ctItem.Value.IsNotEmptyOrNull())
                                ctItems[ctItem.Key] = string.Format("{0:" + vmodel.format + "} ", ctItem.Value);
                        }
                    });
                    items[item.Key] = ctList;
                }
            }
        });

        // 返回结果
        return new { dataRow = resData, headerRow = headerRow };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="tInfo">模板信息.</param>
    /// <param name="list">数据集合.</param>
    /// <param name="vId">模板Id.</param>
    /// <returns>[成功列表,失败列表].</returns>
    public async Task<object[]> ImportMenuData(TemplateParsingBase tInfo, List<Dictionary<string, object>> list, string vId = "")
    {
        List<Dictionary<string, object>> userInputList = ImportFirstVerify(tInfo, list);
        List<FieldsModel> fieldsModelList = tInfo.AllFieldsModel.Where(x => tInfo.selectKey.Contains(x.__vModel__)).ToList();

        var successList = new List<Dictionary<string, object>>();
        var errorsList = new List<Dictionary<string, object>>();

        // 捞取控件解析数据
        var cData = await GetCDataList(tInfo.AllFieldsModel, new Dictionary<string, List<Dictionary<string, string>>>());
        var res = await ImportDataAssemble(fieldsModelList, userInputList, cData);
        res.Where(x => x.ContainsKey("errorsInfo")).ToList().ForEach(item => errorsList.Add(item));
        res.Where(x => !x.ContainsKey("errorsInfo")).ToList().ForEach(item => successList.Add(item));

        try
        {
            // 唯一验证已处理，入库前去掉.
            tInfo.AllFieldsModel.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) && x.__config__.unique).ToList().ForEach(item => item.__config__.unique = false);
            _sqlSugarClient.BeginTran();
            foreach (var item in successList)
            {
                if (item.ContainsKey("Update_MainTablePrimary_Id"))
                {
                    string? mainId = item["Update_MainTablePrimary_Id"].ToString();
                    var haveTableSql = await _runService.GetUpdateSqlByTemplate(tInfo, new VisualDevModelDataUpInput() { data = item.ToJsonString() }, mainId);
                    foreach (var it in haveTableSql) await _databaseService.ExecuteSql(tInfo.DbLink, it); // 修改功能数据
                }
                else
                {
                    if ((tInfo.visualDevEntity?.EnableFlow.Equals(1)).ParseToBool())
                    {
                        var flowId = _repository.AsSugarClient().Queryable<WorkFlow.Entitys.Entity.FlowFormEntity>().First(x => x.Id.Equals(vId)).FlowId;
                        var id = (await _repository.AsSugarClient().Queryable<FlowTemplateJsonEntity>().Where(x => x.TemplateId == flowId && x.EnabledMark == 1 && x.DeleteMark == null).Select(x => x.Id).FirstAsync());
                        await _flowTaskService.Create(new Infrastructure.Models.WorkFlow.FlowTaskSubmitModel() { formData = item, flowId = id, flowUrgent = 1, status = 1 });
                    }
                    else
                    {
                        string? mainId = SnowflakeIdHelper.NextId().ToString();
                        var haveTableSql = await _runService.GetCreateSqlByTemplate(tInfo, new VisualDevModelDataCrInput() { data = item.ToJsonString() }, mainId);

                        // 主表自增长Id.
                        if (haveTableSql.ContainsKey("MainTableReturnIdentity"))
                        {
                            mainId = haveTableSql["MainTableReturnIdentity"].First().First().Value.ToString();
                            haveTableSql.Remove("MainTableReturnIdentity");
                        }
                        foreach (var it in haveTableSql)
                            await _databaseService.ExecuteSql(tInfo.DbLink, it.Key, it.Value); // 新增功能数据
                    }
                }
            }

            _sqlSugarClient.CommitTran();
        }
        catch (Exception e)
        {
            _sqlSugarClient.RollbackTran();
            throw;
        }

        errorsList.ForEach(item =>
        {
            if (item.ContainsKey("errorsInfo") && item["errorsInfo"].IsNotEmptyOrNull()) item["errorsInfo"] = item["errorsInfo"].ToString().TrimStart(',').TrimEnd(',');
        });

        return new object[] { successList, errorsList };
    }

    /// <summary>
    /// Excel 转输出 Model.
    /// </summary>
    /// <param name="tInfo">模板信息.</param>
    /// <param name="realList">数据列表.</param>
    /// <param name="excelName">导出文件名称.</param>
    /// <param name="firstColumns">手动输入第一行（合并主表列和各个子表列）.</param>
    /// <returns>dynamic.</returns>
    public async Task<dynamic> ExcelCreateModel(TemplateParsingBase tInfo, List<Dictionary<string, object>> realList, string excelName = null, Dictionary<string, int> firstColumns = null)
    {
        List<ExcelTemplateModel> templateList = new List<ExcelTemplateModel>();
        List<string> columnList = new List<string>();
        try
        {
            ExcelConfig excelconfig = new ExcelConfig();
            excelconfig.FileName = (excelName.IsNullOrEmpty() ? SnowflakeIdHelper.NextId().ToString() : excelName) + ".xls";
            excelconfig.HeadFont = "微软雅黑";
            excelconfig.HeadPoint = 10;
            excelconfig.IsAllSizeColumn = true;
            excelconfig.ColumnModel = new List<ExcelColumnModel>();
            foreach (string? item in tInfo.selectKey)
            {
                var excelColumn = tInfo.AllFieldsModel.Find(t => t.__vModel__ == item);
                if (excelColumn != null)
                {
                    excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = item, ExcelColumn = excelColumn.__config__.label });
                    columnList.Add(excelColumn.__config__.label);
                }
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = firstColumns == null ? ExcelExportHelper<Dictionary<string, object>>.ExportMemoryStream(realList, excelconfig, columnList) : ExcelExportHelper<Dictionary<string, object>>.ExportMemoryStream(realList, excelconfig, columnList, firstColumns);
            var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
            if (flag)
            {
                fs.Flush();
                fs.Close();
            }
            var name = excelconfig.FileName;
            var url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "Poxiao");
            return new { name = name, url = url };
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 导入功能数据初步验证.
    /// </summary>
    private List<Dictionary<string, object>> ImportFirstVerify(TemplateParsingBase tInfo, List<Dictionary<string, object>> list)
    {
        var errorKey = "errorsInfo";
        var resList = new List<Dictionary<string, object>>();
        list.ForEach(item =>
        {
            var addItem = item.Copy();
            addItem.Add(errorKey, string.Empty);
            resList.Add(addItem);
        });

        #region 验证必填控件
        var childTableList = tInfo.AllFieldsModel.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).Select(x => x.__vModel__).ToList();
        var requiredList = tInfo.AllFieldsModel.Where(x => x.__config__.required).ToList();
        var VModelList = requiredList.Select(x => x.__vModel__).ToList();

        if (VModelList.Any())
        {
            var newResList = new List<Dictionary<string, object>>();
            resList.ForEach(items =>
            {
                var newItems = items.Copy();
                foreach (var item in items)
                {
                    if (item.Value.IsNullOrEmpty() && VModelList.Contains(item.Key))
                    {
                        var errorInfo = requiredList.Find(x => x.__vModel__.Equals(item.Key)).__config__.label + ": 值不能为空";
                        if (newItems.ContainsKey(errorKey)) newItems[errorKey] = newItems[errorKey] + "," + errorInfo;
                        else newItems.Add(errorKey, errorInfo);
                    }

                    // 子表
                    if (childTableList.Contains(item.Key))
                    {
                        item.Value.ToObject<List<Dictionary<string, object>>>().ForEach(childItems =>
                        {
                            foreach (var childItem in childItems)
                            {
                                if (childItem.Value.IsNullOrEmpty() && VModelList.Contains(item.Key + "-" + childItem.Key))
                                {
                                    var errorInfo = tInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item.Key)).__config__.children.Find(x => x.__vModel__.Equals(item.Key + "-" + childItem.Key)).__config__.label + ": 值不能为空";
                                    if (newItems.ContainsKey(errorKey)) newItems[errorKey] = newItems[errorKey] + "," + errorInfo;
                                    else newItems.Add(errorKey, errorInfo);
                                }
                            }
                        });
                    }
                }
                newResList.Add(newItems);
            });
            resList = newResList;
        }
        #endregion

        #region 验证唯一
        var uniqueList = tInfo.AllFieldsModel.Where(x => x.__config__.unique).ToList();
        VModelList = uniqueList.Select(x => x.__vModel__).ToList();

        if (uniqueList.Any())
        {
            resList.ForEach(items =>
            {
                foreach (var item in items)
                {
                    if (VModelList.Contains(item.Key))
                    {
                        var vlist = new List<Dictionary<string, object>>();
                        resList.Where(x => x.ContainsKey(item.Key) && x.ContainsValue(item.Value)).ToList().ForEach(it =>
                        {
                            foreach (var dic in it)
                            {
                                if (dic.Value != null && item.Value != null && dic.Key.Equals(item.Key) && dic.Value.Equals(item.Value))
                                {
                                    vlist.Add(it);
                                    break;
                                }
                            }
                        });
                        if (vlist.Count > 1)
                        {
                            for (var i = 1; i < vlist.Count; i++)
                            {
                                var errorInfo = tInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item.Key)).__config__.label + ": 值不能重复";
                                items[errorKey] = items[errorKey] + "," + errorInfo;
                            }
                        }
                    }

                    // 子表
                    var updateItemCList = new List<Dictionary<string, object>>();
                    var ctItemErrors = new List<string>();
                    if (childTableList.Contains(item.Key))
                    {
                        var itemCList = item.Value.ToObject<List<Dictionary<string, object>>>();
                        itemCList.ForEach(childItems =>
                        {
                            if (tInfo.dataType.Equals("2"))
                            {
                                foreach (var childItem in childItems)
                                {
                                    var uniqueKey = item.Key + "-" + childItem.Key;
                                    if (VModelList.Contains(uniqueKey))
                                    {
                                        var vlist = itemCList.Where(x => x.ContainsKey(childItem.Key) && x.ContainsValue(childItem.Value)).ToList();
                                        if (!updateItemCList.Any(x => x.ContainsKey(childItem.Key) && x.ContainsValue(childItem.Value)))
                                            updateItemCList.Add(vlist.Last());
                                    }
                                }
                            }
                            else
                            {
                                foreach (var childItem in childItems)
                                {
                                    var uniqueKey = item.Key + "-" + childItem.Key;
                                    if (VModelList.Contains(uniqueKey) && childItem.Value != null)
                                    {
                                        var vlist = itemCList.Where(x => x.ContainsKey(childItem.Key) && x.ContainsValue(childItem.Value)).ToList();
                                        if (vlist.Count > 1)
                                        {
                                            for (var i = 1; i < vlist.Count; i++)
                                            {
                                                var errorTxt = tInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(uniqueKey)).__config__.label + ": 值不能重复";
                                                if (!ctItemErrors.Any(x => x.Equals(errorTxt))) ctItemErrors.Add(errorTxt);
                                            }
                                        }
                                    }
                                }
                            }
                        });
                    }

                    if (tInfo.dataType.Equals("2") && updateItemCList.Any()) items[item.Key] = updateItemCList;
                    if (ctItemErrors.Any())
                    {
                        items[errorKey] = items[errorKey].IsNullOrEmpty() ? string.Join(",", ctItemErrors) : items[errorKey] + "," + string.Join(",", ctItemErrors);
                    }
                }
            });

            // 表里的数据验证唯一
            List<string>? relationKey = new List<string>();
            List<string>? auxiliaryFieldList = tInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            auxiliaryFieldList.ForEach(tName =>
            {
                string? tableField = tInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                relationKey.Add(tInfo.MainTableName + "." + tInfo.MainPrimary + "=" + tName + "." + tableField);
            });

            resList.ForEach(allDataMap =>
            {
                List<string>? fieldList = new List<string>();
                var whereList = new List<IConditionalModel>();
                fieldList.Add(string.Format("{0}.{1}", tInfo.MainTableName, tInfo.MainPrimary));
                tInfo.SingleFormData.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) && x.__config__.unique).ToList().ForEach(item =>
                {
                    fieldList.Add(string.Format("{0}.{1} {2}", item.__config__.tableName, item.__vModel__.Split("_poxiao_").Last(), item.__vModel__));

                    if (allDataMap.ContainsKey(item.__vModel__) && allDataMap[item.__vModel__] != null)
                    {
                        whereList.Add(new ConditionalCollections()
                        {
                            ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, new ConditionalModel
                                    {
                                        FieldName = string.Format("{0}.{1}", item.__config__.tableName, item.__vModel__.Split("_poxiao_").Last()),
                                        ConditionalType =allDataMap.ContainsKey(item.__vModel__) ? ConditionalType.Equal: ConditionalType.IsNullOrEmpty,
                                        FieldValue = allDataMap.ContainsKey(item.__vModel__) ? allDataMap[item.__vModel__].ToString() : string.Empty,
                                    })
                                }
                        });
                    }
                });

                var itemWhere = _repository.AsSugarClient().SqlQueryable<dynamic>("@").Where(whereList).ToSqlString();
                if (!itemWhere.Equals("@"))
                {
                    var whereStrList = new List<string>();
                    whereStrList.AddRange(relationKey);
                    whereStrList.Add(itemWhere.Split("WHERE").Last());
                    var querStr = string.Format(
                        "select {0} from {1} where {2}",
                        string.Join(",", fieldList),
                        auxiliaryFieldList.Any() ? tInfo.MainTableName + "," + string.Join(",", auxiliaryFieldList) : tInfo.MainTableName,
                        string.Join(" and ", whereStrList)); // 多表， 联合查询

                    var res = _databaseService.GetInterFaceData(tInfo.DbLink, querStr, null).ToObject<List<Dictionary<string, string>>>();

                    if (res.Any())
                    {
                        var errorList = new List<string>();

                        res.ForEach(items =>
                        {
                            if (tInfo.dataType.Equals("2"))
                            {
                                if (items.Last().Value == null || items.Last().Value.Equals(allDataMap[items.Last().Key].ToString()))
                                {
                                    if (!allDataMap.ContainsKey("Update_MainTablePrimary_Id")) allDataMap.Add("Update_MainTablePrimary_Id", items[tInfo.MainPrimary]);
                                }
                            }
                            else
                            {
                                if (items.Last().Value == null || items.Last().Value.Equals(allDataMap[items.Last().Key].ToString()))
                                {
                                    var errorInfo = tInfo.SingleFormData.First(x => x.__vModel__.Equals(items.Last().Key))?.__config__.label + ": 值不能重复";
                                    if (allDataMap.ContainsKey(errorKey))
                                    {
                                        if (!allDataMap[errorKey].ToString().Contains(errorInfo)) allDataMap[errorKey] = allDataMap[errorKey] + "," + errorInfo;
                                    }
                                    else
                                    {
                                        allDataMap.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        });
                    }
                }
            });
        }

        #endregion

        resList.ForEach(item =>
        {
            if (item[errorKey].IsNullOrEmpty()) item.Remove(errorKey);
        });
        return resList;
    }

    /// <summary>
    /// 获取模板控件解析数据.
    /// </summary>
    /// <param name="tInfo"></param>
    /// <param name="resData"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, List<Dictionary<string, string>>>> GetCDataList(List<FieldsModel> listFieldsModel, Dictionary<string, List<Dictionary<string, string>>> resData)
    {
        foreach (var item in listFieldsModel.Where(x => !x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).ToList())
        {
            var addItem = new List<Dictionary<string, string>>();
            switch (item.__config__.poxiaoKey)
            {
                case PoxiaoKeyConst.COMSELECT:
                    {
                        if (!resData.ContainsKey(PoxiaoKeyConst.COMSELECT))
                        {
                            var dataList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1)
                                .Select(x => new OrganizeEntity { Id = x.Id, OrganizeIdTree = x.OrganizeIdTree, FullName = x.FullName }).ToListAsync();
                            dataList.ForEach(item =>
                            {
                                if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
                                var orgNameList = new List<string>();
                                item.OrganizeIdTree.Split(",").ToList().ForEach(it =>
                                {
                                    var org = dataList.Find(x => x.Id == it);
                                    if (org != null) orgNameList.Add(org.FullName);
                                });
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary.Add(item.OrganizeIdTree, string.Join("/", orgNameList));
                                addItem.Add(dictionary);
                            });

                            resData.Add(PoxiaoKeyConst.COMSELECT, addItem);
                        }
                    }

                    break;
                case PoxiaoKeyConst.ADDRESS:
                    {
                        string? addCacheKey = "Import_Address";

                        if (!resData.ContainsKey(PoxiaoKeyConst.ADDRESS))
                        {
                            if (_cacheManager.Exists(addCacheKey))
                            {
                                addItem = _cacheManager.Get(addCacheKey).ToObject<List<Dictionary<string, string>>>();
                                resData.Add(PoxiaoKeyConst.ADDRESS, addItem);
                            }
                            else
                            {
                                var dataList = await _repository.AsSugarClient().Queryable<ProvinceEntity>().Select(x => new ProvinceEntity { Id = x.Id, ParentId = x.ParentId, Type = x.Type, FullName = x.FullName }).ToListAsync();

                                // 处理省市区树
                                dataList.Where(x => x.Type == "1").ToList().ForEach(item =>
                                {
                                    item.QuickQuery = item.FullName;
                                    item.Description = item.Id;
                                    Dictionary<string, string> address = new Dictionary<string, string>();
                                    address.Add(item.Description, item.QuickQuery);
                                    addItem.Add(address);
                                });
                                dataList.Where(x => x.Type == "2").ToList().ForEach(item =>
                                {
                                    item.QuickQuery = dataList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName;
                                    item.Description = dataList.Find(x => x.Id == item.ParentId).Description + "," + item.Id;
                                    Dictionary<string, string> address = new Dictionary<string, string>();
                                    address.Add(item.Description, item.QuickQuery);
                                    addItem.Add(address);
                                });
                                dataList.Where(x => x.Type == "3").ToList().ForEach(item =>
                                {
                                    item.QuickQuery = dataList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName;
                                    item.Description = dataList.Find(x => x.Id == item.ParentId).Description + "," + item.Id;
                                    Dictionary<string, string> address = new Dictionary<string, string>();
                                    address.Add(item.Description, item.QuickQuery);
                                    addItem.Add(address);
                                });
                                dataList.Where(x => x.Type == "4").ToList().ForEach(item =>
                                {
                                    ProvinceEntity? it = dataList.Find(x => x.Id == item.ParentId);
                                    if (it != null)
                                    {
                                        item.QuickQuery = it.QuickQuery + "/" + item.FullName;
                                        item.Description = it.Description + "," + item.Id;
                                        Dictionary<string, string> address = new Dictionary<string, string>();
                                        address.Add(item.Description, item.QuickQuery);
                                        addItem.Add(address);
                                    }
                                });
                                dataList.ForEach(it =>
                                {
                                    if (it.Description.IsNotEmptyOrNull())
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(it.Description, it.QuickQuery);
                                        addItem.Add(dictionary);
                                    }
                                });

                                var noTypeList = dataList.Where(x => x.Type.IsNullOrWhiteSpace()).ToList();
                                foreach (var it in noTypeList)
                                {
                                    it.QuickQuery = GetAddressByPList(noTypeList, it);
                                    it.Description = GetAddressIdByPList(noTypeList, it);
                                }
                                foreach (var it in noTypeList)
                                {
                                    Dictionary<string, string> address = new Dictionary<string, string>();
                                    address.Add(it.Description, it.QuickQuery);
                                    addItem.Add(address);
                                }

                                _cacheManager.Set(addCacheKey, addItem, TimeSpan.FromDays(7)); // 缓存七天
                                resData.Add(PoxiaoKeyConst.ADDRESS, addItem);
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.GROUPSELECT:
                    {
                        if (!resData.ContainsKey(PoxiaoKeyConst.GROUPSELECT))
                        {
                            var dataList = await _repository.AsSugarClient().Queryable<GroupEntity>().Where(x => x.DeleteMark == null).Select(x => new GroupEntity() { Id = x.Id, EnCode = x.EnCode }).ToListAsync();
                            dataList.ForEach(item =>
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary.Add(item.Id, item.EnCode);
                                addItem.Add(dictionary);
                            });
                            resData.Add(PoxiaoKeyConst.GROUPSELECT, addItem);
                        }
                    }

                    break;
                case PoxiaoKeyConst.ROLESELECT:
                    {
                        if (!resData.ContainsKey(PoxiaoKeyConst.ROLESELECT))
                        {
                            var dataList = await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.DeleteMark == null).Select(x => new RoleEntity() { Id = x.Id, EnCode = x.EnCode }).ToListAsync();
                            dataList.ForEach(item =>
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary.Add(item.Id, item.EnCode);
                                addItem.Add(dictionary);
                            });
                            resData.Add(PoxiaoKeyConst.ROLESELECT, addItem);
                        }
                    }

                    break;
                case PoxiaoKeyConst.SWITCH:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary.Add("1", item.activeTxt);
                            addItem.Add(dictionary);
                            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                            dictionary2.Add("0", item.inactiveTxt);
                            addItem.Add(dictionary2);
                            resData.Add(item.__vModel__, addItem);
                        }
                    }

                    break;
                case PoxiaoKeyConst.CHECKBOX:
                case PoxiaoKeyConst.SELECT:
                case PoxiaoKeyConst.RADIO:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            var propsValue = string.Empty;
                            var propsLabel = string.Empty;
                            var children = string.Empty;
                            if (item.props != null && item.props.props != null)
                            {
                                propsValue = item.props.props.value;
                                propsLabel = item.props.props.label;
                                children = item.props.props.children;
                            }

                            if (item.__config__.dataType.Equals("static"))
                            {
                                if (item != null && item.options != null)
                                {
                                    item.options.ForEach(option =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(option[propsValue].ToString(), option[propsLabel].ToString());
                                        addItem.Add(dictionary);
                                    });
                                    resData.Add(item.__vModel__, addItem);
                                }
                            }
                            else if (item.__config__.dataType.Equals("dictionary"))
                            {
                                var dictionaryDataList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity, DictionaryTypeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.DictionaryTypeId))
                                    .WhereIF(item.__config__.dictionaryType.IsNotEmptyOrNull(), (a, b) => b.Id == item.__config__.dictionaryType || b.EnCode == item.__config__.dictionaryType)
                                    .Where(a => a.DeleteMark == null).Select(a => new { a.Id, a.EnCode, a.FullName }).ToListAsync();

                                foreach (var it in dictionaryDataList)
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    if (propsValue.Equals("id")) dictionary.Add(it.Id, it.FullName);
                                    if (propsValue.Equals("enCode")) dictionary.Add(it.EnCode, it.FullName);
                                    addItem.Add(dictionary);
                                }

                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.__config__.dataType.Equals("dynamic"))
                            {
                                var popDataList = await _formDataParsing.GetDynamicList(item);
                                resData.Add(item.__vModel__, popDataList);
                            }
                        }
                    }
                    break;
                case PoxiaoKeyConst.TREESELECT:
                case PoxiaoKeyConst.CASCADER:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            if (item.__config__.dataType.Equals("static"))
                            {
                                if (item.options != null)
                                    resData.Add(item.__vModel__, GetStaticList(item));
                            }
                            else if (item.__config__.dataType.Equals("dictionary"))
                            {
                                var dictionaryDataList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity, DictionaryTypeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.DictionaryTypeId))
                                    .WhereIF(item.__config__.dictionaryType.IsNotEmptyOrNull(), (a, b) => b.Id == item.__config__.dictionaryType || b.EnCode == item.__config__.dictionaryType)
                                    .Where(a => a.DeleteMark == null).Select(a => new { a.Id, a.EnCode, a.FullName }).ToListAsync();

                                foreach (var it in dictionaryDataList)
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary.Add(it.Id, it.FullName);
                                    dictionary.Add(it.EnCode, it.FullName);
                                    addItem.Add(dictionary);
                                }

                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.__config__.dataType.Equals("dynamic"))
                            {
                                var popDataList = await _formDataParsing.GetDynamicList(item);
                                resData.Add(item.__vModel__, popDataList);
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.POPUPTABLESELECT:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            var popDataList = await _formDataParsing.GetDynamicList(item);
                            resData.Add(item.__vModel__, popDataList);
                        }
                    }
                    break;

                case PoxiaoKeyConst.USERSELECT:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            if (item.selectType.Equals("all"))
                            {
                                var dataList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => x.DeleteMark == null).Select(x => new UserEntity() { Id = x.Id, Account = x.Account }).ToListAsync();
                                dataList.ForEach(item =>
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary.Add(item.Id, item.Account);
                                    addItem.Add(dictionary);
                                });
                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.selectType.Equals("custom"))
                            {
                                var userIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>()
                                    .WhereIF(item.ableUserIds.Any(), x => item.ableUserIds.Contains(x.UserId) || item.ableDepIds.Contains(x.ObjectId)
                                    || item.ablePosIds.Contains(x.ObjectId) || item.ableRoleIds.Contains(x.ObjectId) || item.ableGroupIds.Contains(x.ObjectId)).Select(x => x.UserId).ToListAsync();
                                var dataList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => x.DeleteMark == null && userIdList.Contains(x.Id))
                                        .Select(x => new UserEntity() { Id = x.Id, Account = x.Account }).ToListAsync();
                                dataList.ForEach(item =>
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary.Add(item.Id, item.Account);
                                    if (!addItem.Any(x => x.ContainsKey(item.Id))) addItem.Add(dictionary);
                                });
                                resData.Add(item.__vModel__, addItem);
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.USERSSELECT:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            if (item.selectType.Equals("all"))
                            {
                                if (item.multiple)
                                {
                                    (await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => x.DeleteMark == null).Select(x => new { x.Id, x.RealName, x.Account }).ToListAsync()).ForEach(item =>
                                    {
                                        Dictionary<string, string> user = new Dictionary<string, string>();
                                        user.Add(item.Id + "--user", item.RealName + "/" + item.Account);
                                        addItem.Add(user);
                                    });
                                    var dataList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null)
                                        .Select(x => new OrganizeEntity { Id = x.Id, OrganizeIdTree = x.OrganizeIdTree, FullName = x.FullName, EnCode = x.EnCode }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> user = new Dictionary<string, string>();
                                        user.Add(item.Id + "--department", item.FullName + "/" + item.EnCode);
                                        addItem.Add(user);

                                        if (item.OrganizeIdTree.IsNullOrEmpty()) item.OrganizeIdTree = item.Id;
                                        var orgNameList = new List<string>();
                                        item.OrganizeIdTree.Split(",").ToList().ForEach(it =>
                                        {
                                            var org = dataList.Find(x => x.Id == it);
                                            if (org != null) orgNameList.Add(org.FullName);
                                        });
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id + "--company", string.Join("/", orgNameList));
                                        addItem.Add(dictionary);
                                    });
                                    (await _repository.AsSugarClient().Queryable<RoleEntity>().Where(x => x.DeleteMark == null).Select(x => new { x.Id, x.FullName, x.EnCode }).ToListAsync()).ForEach(item =>
                                    {
                                        Dictionary<string, string> user = new Dictionary<string, string>();
                                        user.Add(item.Id + "--role", item.FullName + "/" + item.EnCode);
                                        addItem.Add(user);
                                    });
                                    (await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null).Select(x => new { x.Id, x.FullName, x.EnCode }).ToListAsync()).ForEach(item =>
                                    {
                                        Dictionary<string, string> user = new Dictionary<string, string>();
                                        user.Add(item.Id + "--position", item.FullName + "/" + item.EnCode);
                                        addItem.Add(user);
                                    });
                                    (await _repository.AsSugarClient().Queryable<GroupEntity>().Where(x => x.DeleteMark == null).Select(x => new { x.Id, x.FullName, x.EnCode }).ToListAsync()).ForEach(item =>
                                    {
                                        Dictionary<string, string> user = new Dictionary<string, string>();
                                        user.Add(item.Id + "--group", item.FullName + "/" + item.EnCode);
                                        addItem.Add(user);
                                    });
                                }
                                else
                                {
                                    var dataList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => x.DeleteMark == null).Select(x => new UserEntity() { Id = x.Id, Account = x.Account }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id + "--user", item.Account);
                                        if (!addItem.Any(x => x.ContainsKey(item.Id))) addItem.Add(dictionary);
                                    });
                                }
                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.selectType.Equals("custom"))
                            {
                                if (item.ableIds.Any())
                                {
                                    var newAbleIds = new List<string>();
                                    item.ableIds.ForEach(x => newAbleIds.Add(x.Split("--").FirstOrDefault()));
                                    var userIdList = await _repository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => newAbleIds.Contains(x.UserId) || newAbleIds.Contains(x.ObjectId)).Select(x => x.UserId).ToListAsync();
                                    var dataList = await _repository.AsSugarClient().Queryable<UserEntity>().Where(x => userIdList.Contains(x.Id)).Select(x => new UserEntity() { Id = x.Id, Account = x.Account }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id + "--user", item.Account);
                                        if (!addItem.Any(x => x.ContainsKey(item.Id))) addItem.Add(dictionary);
                                    });
                                    resData.Add(item.__vModel__, addItem);
                                }
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.DEPSELECT:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            if (item.selectType.Equals("all"))
                            {
                                var dataList = await _repository.AsSugarClient().Queryable<OrganizeEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).Select(x => new { x.Id, x.EnCode }).ToListAsync();
                                dataList.ForEach(item =>
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary.Add(item.Id, item.EnCode);
                                    addItem.Add(dictionary);
                                });
                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.selectType.Equals("custom"))
                            {
                                if (item.ableDepIds.Any())
                                {
                                    var listQuery = new List<ISugarQueryable<OrganizeEntity>>();
                                    item.ableDepIds.ForEach(x => listQuery.Add(_repository.AsSugarClient().Queryable<OrganizeEntity>().Where(xx => xx.OrganizeIdTree.Contains(x))));
                                    var dataList = await _repository.AsSugarClient().UnionAll(listQuery).Where(x => x.DeleteMark == null).Select(x => new { x.Id, x.EnCode }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id, item.EnCode);
                                        if (!addItem.Any(x => x.ContainsKey(item.Id))) addItem.Add(dictionary);
                                    });
                                    resData.Add(item.__vModel__, addItem);
                                }
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.POSSELECT:
                    {
                        if (!resData.ContainsKey(item.__vModel__))
                        {
                            if (item.selectType.Equals("all"))
                            {
                                var dataList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null).Select(x => new PositionEntity() { Id = x.Id, EnCode = x.EnCode }).ToListAsync();
                                dataList.ForEach(item =>
                                {
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary.Add(item.Id, item.EnCode);
                                    addItem.Add(dictionary);
                                });
                                resData.Add(item.__vModel__, addItem);
                            }
                            else if (item.selectType.Equals("custom"))
                            {
                                if (item.ableDepIds.Any())
                                {
                                    var dataList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null && item.ableDepIds.Contains(x.OrganizeId))
                                        .Select(x => new PositionEntity() { Id = x.Id, EnCode = x.EnCode }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id, item.EnCode);
                                        addItem.Add(dictionary);
                                    });
                                    resData.Add(item.__vModel__, addItem);
                                }
                                if (item.ablePosIds.Any())
                                {
                                    var dataList = await _repository.AsSugarClient().Queryable<PositionEntity>().Where(x => x.DeleteMark == null && item.ablePosIds.Contains(x.Id))
                                        .Select(x => new PositionEntity() { Id = x.Id, EnCode = x.EnCode }).ToListAsync();
                                    dataList.ForEach(item =>
                                    {
                                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                        dictionary.Add(item.Id, item.EnCode);
                                        addItem.Add(dictionary);
                                    });

                                    if (resData.ContainsKey(item.__vModel__))
                                    {
                                        var newAddItem = new List<Dictionary<string, string>>();
                                        foreach (var it in addItem)
                                        {
                                            var tempIt = it.FirstOrDefault().Value;
                                            if (tempIt.IsNotEmptyOrNull() && !resData[item.__vModel__].Any(x => x.ContainsValue(tempIt))) newAddItem.Add(it);
                                        }
                                        resData[item.__vModel__].AddRange(newAddItem);
                                    }
                                    else resData.Add(item.__vModel__, addItem);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        listFieldsModel.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).ToList().ForEach(async item =>
        {
            var res = await GetCDataList(item.__config__.children, resData);
            if (res.Any()) foreach (var it in res) if (!resData.ContainsKey(it.Key)) resData.Add(it.Key, it.Value);
        });

        return resData;
    }

    /// <summary>
    /// 导入数据组装.
    /// </summary>
    /// <param name="fieldsModelList">控件列表.</param>
    /// <param name="dataList">导入数据列表.</param>
    /// <param name="cDataList">控件解析缓存数据.</param>
    /// <returns></returns>
    private async Task<List<Dictionary<string, object>>> ImportDataAssemble(List<FieldsModel> fieldsModelList, List<Dictionary<string, object>> dataList, Dictionary<string, List<Dictionary<string, string>>> cDataList)
    {
        var errorKey = "errorsInfo";
        UserEntity? userInfo = _userManager.User;

        var resList = new List<Dictionary<string, object>>();
        foreach (var dataItems in dataList)
        {
            var newDataItems = dataItems.Copy();
            foreach (var item in dataItems)
            {
                var vModel = fieldsModelList.Find(x => x.__vModel__.Equals(item.Key));
                if (vModel == null) continue;
                var dicList = new List<Dictionary<string, string>>();
                if (cDataList.ContainsKey(vModel.__config__.poxiaoKey)) dicList = cDataList[vModel.__config__.poxiaoKey];
                if ((dicList == null || !dicList.Any()) && cDataList.ContainsKey(vModel.__vModel__)) dicList = cDataList[vModel.__vModel__];

                switch (vModel.__config__.poxiaoKey)
                {
                    case PoxiaoKeyConst.DATE:
                        try
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                // 判断格式是否正确
                                var value = DateTime.ParseExact(item.Value.ToString().TrimEnd(), vModel.format, System.Globalization.CultureInfo.CurrentCulture);
                                var date = value.ParseToUnixTime();

                                if (vModel.__config__.startTimeRule && vModel.__config__.startTimeValue.IsNotEmptyOrNull())
                                {
                                    var minDate = DateTime.Now.ParseToUnixTime();
                                    switch (vModel.__config__.startTimeType)
                                    {
                                        case 1:
                                            {
                                                if (vModel.__config__.startTimeValue.IsNotEmptyOrNull())
                                                    minDate = vModel.__config__.startTimeValue.ParseToLong();
                                            }

                                            break;
                                        case 2:
                                            {
                                                if (vModel.__config__.startRelationField.IsNotEmptyOrNull() && dataItems.ContainsKey(vModel.__config__.startRelationField))
                                                {
                                                    var data = dataItems[vModel.__config__.startRelationField].ToString();
                                                    minDate = data.Substring(0, data.Length - 1).ParseToDateTime().ParseToUnixTime();
                                                }
                                            }

                                            break;
                                        case 3:
                                            break;
                                        case 4:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        minDate = minDate.TimeStampToDateTime().AddYears(-vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 2:
                                                        minDate = minDate.TimeStampToDateTime().AddMonths(-vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 3:
                                                        minDate = minDate.TimeStampToDateTime().AddDays(-vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                }
                                            }

                                            break;
                                        case 5:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        minDate = minDate.TimeStampToDateTime().AddYears(vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 2:
                                                        minDate = minDate.TimeStampToDateTime().AddMonths(vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 3:
                                                        minDate = minDate.TimeStampToDateTime().AddDays(vModel.__config__.startTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                }
                                            }

                                            break;
                                    }

                                    if (minDate > date)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 日期选择值不在范围内";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }

                                if (vModel.__config__.endTimeRule && vModel.__config__.endTimeValue.IsNotEmptyOrNull())
                                {
                                    var maxDate = DateTime.Now.ParseToUnixTime();
                                    switch (vModel.__config__.endTimeType)
                                    {
                                        case 1:
                                            {
                                                if (vModel.__config__.endTimeValue.IsNotEmptyOrNull())
                                                    maxDate = vModel.__config__.endTimeValue.ParseToLong();
                                            }

                                            break;
                                        case 2:
                                            {
                                                if (vModel.__config__.endRelationField.IsNotEmptyOrNull() && dataItems.ContainsKey(vModel.__config__.endRelationField))
                                                {
                                                    var data = dataItems[vModel.__config__.endRelationField].ToString();
                                                    maxDate = data.Substring(0, data.Length - 1).ParseToDateTime().ParseToUnixTime();
                                                }
                                            }

                                            break;
                                        case 3:
                                            break;
                                        case 4:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        maxDate = maxDate.TimeStampToDateTime().AddYears(-vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 2:
                                                        maxDate = maxDate.TimeStampToDateTime().AddMonths(-vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 3:
                                                        maxDate = maxDate.TimeStampToDateTime().AddDays(-vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                }
                                            }

                                            break;
                                        case 5:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        maxDate = maxDate.TimeStampToDateTime().AddYears(vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 2:
                                                        maxDate = maxDate.TimeStampToDateTime().AddMonths(vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                    case 3:
                                                        maxDate = maxDate.TimeStampToDateTime().AddDays(vModel.__config__.endTimeValue.ParseToInt()).ParseToUnixTime();
                                                        break;
                                                }
                                            }

                                            break;
                                    }

                                    if (maxDate < date)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 日期选择值不在范围内";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }

                                newDataItems[item.Key] = date.ToString();
                            }
                        }
                        catch
                        {
                            var errorInfo = vModel.__config__.label + ": 值不正确";
                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                            else newDataItems.Add(errorKey, errorInfo);
                        }

                        break;

                    case PoxiaoKeyConst.TIME: // 时间选择
                        try
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                var value = DateTime.ParseExact(item.Value.ToString().TrimEnd(), vModel.format, System.Globalization.CultureInfo.CurrentCulture);
                                if (vModel.__config__.startTimeRule && vModel.__config__.startTimeValue.IsNotEmptyOrNull())
                                {
                                    var minTime = value;
                                    switch (vModel.__config__.startTimeType)
                                    {
                                        case 1:
                                            {
                                                if (vModel.__config__.startTimeValue.IsNotEmptyOrNull())
                                                    minTime = DateTime.Parse(vModel.__config__.startTimeValue);
                                            }

                                            break;
                                        case 2:
                                            {
                                                if (vModel.__config__.startRelationField.IsNotEmptyOrNull() && dataItems.ContainsKey(vModel.__config__.startRelationField))
                                                    minTime = DateTime.Parse(dataItems[vModel.__config__.startRelationField].ToString());
                                            }

                                            break;
                                        case 3:
                                            break;
                                        case 4:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        minTime = minTime.AddHours(-vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                    case 2:
                                                        minTime = minTime.AddMinutes(-vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                    case 3:
                                                        minTime = minTime.AddSeconds(-vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                }
                                            }

                                            break;
                                        case 5:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        minTime = minTime.AddHours(vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                    case 2:
                                                        minTime = minTime.AddMinutes(vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                    case 3:
                                                        minTime = minTime.AddSeconds(vModel.__config__.startTimeValue.ParseToInt());
                                                        break;
                                                }
                                            }

                                            break;
                                    }

                                    if (minTime > value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 时间选择值不在范围内";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }

                                if (vModel.__config__.endTimeRule && vModel.__config__.endTimeValue.IsNotEmptyOrNull())
                                {
                                    var maxTime = value;
                                    switch (vModel.__config__.endTimeType)
                                    {
                                        case 1:
                                            {
                                                if (vModel.__config__.endTimeValue.IsNotEmptyOrNull())
                                                    maxTime = DateTime.Parse(vModel.__config__.endTimeValue);
                                            }

                                            break;
                                        case 2:
                                            {
                                                if (vModel.__config__.endRelationField.IsNotEmptyOrNull() && dataItems.ContainsKey(vModel.__config__.endRelationField))
                                                    maxTime = DateTime.Parse(dataItems[vModel.__config__.endRelationField].ToString());
                                            }

                                            break;
                                        case 3:
                                            break;
                                        case 4:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        maxTime = maxTime.AddHours(-vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                    case 2:
                                                        maxTime = maxTime.AddMinutes(-vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                    case 3:
                                                        maxTime = maxTime.AddSeconds(-vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                }
                                            }

                                            break;
                                        case 5:
                                            {
                                                switch (vModel.__config__.startTimeTarget)
                                                {
                                                    case 1:
                                                        maxTime = maxTime.AddHours(vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                    case 2:
                                                        maxTime = maxTime.AddMinutes(vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                    case 3:
                                                        maxTime = maxTime.AddSeconds(vModel.__config__.endTimeValue.ParseToInt());
                                                        break;
                                                }
                                            }

                                            break;
                                    }

                                    if (maxTime < value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 时间选择值不在范围内";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            var errorInfo = vModel.__config__.label + ": 值不正确";
                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                            else newDataItems.Add(errorKey, errorInfo);
                        }

                        break;
                    case PoxiaoKeyConst.COMSELECT:
                    case PoxiaoKeyConst.ADDRESS:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.multiple)
                                {
                                    var addList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(it =>
                                    {
                                        if (vModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMSELECT) || (it.Count(x => x == '/') == vModel.level || (vModel.level == 3 && it.Count(x => x == '/') == 2 && it.Contains("市辖区"))))
                                        {
                                            if (dicList.Where(x => x.ContainsValue(it)).Any())
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key.Split(",").ToList());
                                            }
                                            else
                                            {
                                                var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                                else
                                {
                                    if (vModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMSELECT) || (item.Value?.ToString().Count(x => x == '/') == vModel.level || (vModel.level == 3 && item.Value.ToString().Count(x => x == '/') == 2 && item.Value.ToString().Contains("市辖区"))))
                                    {
                                        if (dicList.Where(x => x.ContainsValue(item.Value?.ToString())).Any())
                                        {
                                            var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString())).FirstOrDefault().FirstOrDefault();
                                            newDataItems[item.Key] = value.Key.Split(",").ToList();
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    }
                                    else
                                    {
                                        var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.CHECKBOX:
                    case PoxiaoKeyConst.SWITCH:
                    case PoxiaoKeyConst.SELECT:
                    case PoxiaoKeyConst.RADIO:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.multiple || vModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CHECKBOX))
                                {
                                    var addList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(it =>
                                    {
                                        if (dicList.Where(x => x.ContainsValue(it)).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(it)) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key);
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                                else
                                {
                                    if (dicList.Where(x => x.ContainsValue(item.Value.ToString())).Any())
                                    {
                                        if (dicList.Count(x => x.ContainsValue(item.Value.ToString())) > 1)
                                        {
                                            var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                        else
                                        {
                                            var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString())).FirstOrDefault().FirstOrDefault();
                                            newDataItems[item.Key] = value.Key;
                                        }
                                    }
                                    else
                                    {
                                        var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.DEPSELECT:
                    case PoxiaoKeyConst.POSSELECT:
                    case PoxiaoKeyConst.GROUPSELECT:
                    case PoxiaoKeyConst.ROLESELECT:
                    case PoxiaoKeyConst.USERSELECT:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.multiple)
                                {
                                    var addList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(it =>
                                    {
                                        if (dicList.Where(x => x.ContainsValue(it.Split("/").Last())).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(it.Split("/").Last())) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it.Split("/").Last())).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key);
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                                else
                                {
                                    if (dicList.Where(x => x.ContainsValue(item.Value.ToString().Split("/").Last())).Any())
                                    {
                                        if (dicList.Count(x => x.ContainsValue(item.Value.ToString().Split("/").Last())) > 1)
                                        {
                                            var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                        else
                                        {
                                            var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString().Split("/").Last())).FirstOrDefault().FirstOrDefault();
                                            newDataItems[item.Key] = value.Key;
                                        }
                                    }
                                    else
                                    {
                                        var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.USERSSELECT:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.multiple)
                                {
                                    var addList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(it =>
                                    {
                                        if (dicList.Where(x => x.ContainsValue(it)).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(it)) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key);
                                            }
                                        }
                                        else
                                        {
                                            if (dicList.Where(x => x.ContainsValue(it.Split("/").Last())).Any())
                                            {
                                                if (dicList.Count(x => x.ContainsValue(it.Split("/").Last())) > 1)
                                                {
                                                    var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                    if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                    else newDataItems.Add(errorKey, errorInfo);
                                                }
                                                else
                                                {
                                                    var value = dicList.Where(x => x.ContainsValue(it.Split("/").Last())).FirstOrDefault().FirstOrDefault();
                                                    addList.Add(value.Key);
                                                }
                                            }
                                            else
                                            {
                                                var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                                else
                                {
                                    if (dicList.Where(x => x.ContainsValue(item.Value.ToString())).Any())
                                    {
                                        if (dicList.Count(x => x.ContainsValue(item.Value.ToString())) > 1)
                                        {
                                            var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                        else
                                        {
                                            var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString())).FirstOrDefault().FirstOrDefault();
                                            newDataItems[item.Key] = value.Key;
                                        }
                                    }
                                    else
                                    {
                                        if (dicList.Where(x => x.ContainsValue(item.Value.ToString().Split("/").Last())).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(item.Value.ToString().Split("/").Last())) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString().Split("/").Last())).FirstOrDefault().FirstOrDefault();
                                                newDataItems[item.Key] = value.Key;
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.TREESELECT:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.multiple)
                                {
                                    var addList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(it =>
                                    {
                                        if (dicList.Where(x => x.ContainsValue(it)).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(it)) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key);
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                                else
                                {
                                    if (dicList.Where(x => x.ContainsValue(item.Value.ToString())).Any())
                                    {
                                        if (dicList.Count(x => x.ContainsValue(item.Value.ToString())) > 1)
                                        {
                                            var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                        else
                                        {
                                            var value = dicList.Where(x => x.ContainsValue(item.Value?.ToString())).FirstOrDefault().FirstOrDefault();
                                            newDataItems[item.Key] = value.Key;
                                        }
                                    }
                                    else
                                    {
                                        var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.CASCADER:
                        {
                            if (item.Value.IsNotEmptyOrNull())
                            {
                                if (vModel.props.props.multiple)
                                {
                                    var addsList = new List<object>();
                                    item.Value.ToString().Split(",").ToList().ForEach(its =>
                                    {
                                        var txtList = its.Split(vModel.separator).ToList();

                                        var add = new List<object>();
                                        txtList.ForEach(it =>
                                        {
                                            if (dicList.Where(x => x.ContainsValue(it)).Any())
                                            {
                                                if (dicList.Count(x => x.ContainsValue(it)) > 1)
                                                {
                                                    var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                    if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                    else newDataItems.Add(errorKey, errorInfo);
                                                }
                                                else
                                                {
                                                    var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                    add.Add(value.Key);
                                                }
                                            }
                                            else
                                            {
                                                var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                        });
                                        addsList.Add(add);
                                    });
                                    newDataItems[item.Key] = addsList;
                                }
                                else
                                {
                                    var txtList = item.Value.ToString().Split(vModel.separator).ToList();

                                    var addList = new List<object>();
                                    txtList.ForEach(it =>
                                    {
                                        if (dicList.Where(x => x.ContainsValue(it)).Any())
                                        {
                                            if (dicList.Count(x => x.ContainsValue(it)) > 1)
                                            {
                                                var errorInfo = vModel.__config__.label + ": 存在多条值-无法匹配";
                                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                                else newDataItems.Add(errorKey, errorInfo);
                                            }
                                            else
                                            {
                                                var value = dicList.Where(x => x.ContainsValue(it)).FirstOrDefault().FirstOrDefault();
                                                addList.Add(value.Key);
                                            }
                                        }
                                        else
                                        {
                                            var errorInfo = vModel.__config__.label + ": 值无法匹配";
                                            if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                            else newDataItems.Add(errorKey, errorInfo);
                                        }
                                    });
                                    newDataItems[item.Key] = addList;
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.TABLE:
                        {
                            if (item.Value != null)
                            {
                                var valueList = item.Value.ToObject<List<Dictionary<string, object>>>();
                                var newValueList = new List<Dictionary<string, object>>();
                                valueList.ForEach(it =>
                                {
                                    var addValue = new Dictionary<string, object>();
                                    foreach (var value in it) addValue.Add(vModel.__vModel__ + "-" + value.Key, value.Value);
                                    newValueList.Add(addValue);
                                });

                                var res = await ImportDataAssemble(vModel.__config__.children, newValueList, cDataList);
                                if (res.Any(x => x.ContainsKey(errorKey)))
                                {
                                    if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + res.FirstOrDefault(x => x.ContainsKey(errorKey))[errorKey].ToString();
                                    else newDataItems.Add(errorKey, res.FirstOrDefault(x => x.ContainsKey(errorKey))[errorKey].ToString());
                                    res.Remove(res.FirstOrDefault(x => x.ContainsKey(errorKey)));
                                }

                                var result = new List<Dictionary<string, object>>();
                                res.ForEach(it =>
                                {
                                    var addValue = new Dictionary<string, object>();
                                    foreach (var value in it) addValue.Add(value.Key.Replace(vModel.__vModel__ + "-", string.Empty), value.Value);
                                    result.Add(addValue);
                                });
                                newDataItems[item.Key] = result;
                            }
                        }
                        break;
                    case PoxiaoKeyConst.RATE:
                        if (item.Value.IsNotEmptyOrNull())
                        {
                            try
                            {
                                var value = int.Parse(item.Value.ToString());
                                if (vModel.max != null)
                                {
                                    if (vModel.max < value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 评分超过设置的最大值";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                            catch
                            {
                                var errorInfo = vModel.__config__.label + ": 评分格式错误";
                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                else newDataItems.Add(errorKey, errorInfo);
                            }
                        }
                        break;
                    case PoxiaoKeyConst.SLIDER:
                        if (item.Value.IsNotEmptyOrNull())
                        {
                            try
                            {
                                var value = int.Parse(item.Value.ToString());
                                if (vModel.max != null)
                                {
                                    if (vModel.max < value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 滑块超过设置的最大值";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                                if (vModel.min != null)
                                {
                                    if (vModel.min > value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 滑块超过设置的最小值";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                            catch
                            {
                                var errorInfo = vModel.__config__.label + ": 滑块格式错误";
                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                else newDataItems.Add(errorKey, errorInfo);
                            }
                        }
                        break;
                    case PoxiaoKeyConst.NUMINPUT:
                        if (item.Value.IsNotEmptyOrNull())
                        {
                            try
                            {
                                var value = decimal.Parse(item.Value.ToString());
                                if (vModel.max != null)
                                {
                                    if (vModel.max < value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 数字输入超过设置的最大值";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                                if (vModel.min != null)
                                {
                                    if (vModel.min > value)
                                    {
                                        var errorInfo = vModel.__config__.label + ": 数字输入超过设置的最小值";
                                        if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                        else newDataItems.Add(errorKey, errorInfo);
                                    }
                                }
                            }
                            catch
                            {
                                var errorInfo = vModel.__config__.label + ": 数字输入格式错误";
                                if (newDataItems.ContainsKey(errorKey)) newDataItems[errorKey] = newDataItems[errorKey] + "," + errorInfo;
                                else newDataItems.Add(errorKey, errorInfo);
                            }
                        }
                        break;
                }
            }

            // 系统自动生成控件
            foreach (var item in dataItems)
            {
                if (newDataItems.ContainsKey(errorKey)) continue; // 如果存在错误信息 则 不生成
                var vModel = fieldsModelList.Find(x => x.__vModel__.Equals(item.Key));
                if (vModel == null) continue;

                switch (vModel.__config__.poxiaoKey)
                {
                    case PoxiaoKeyConst.BILLRULE:
                        string billNumber = await _billRuleService.GetBillNumber(vModel.__config__.rule);
                        if (!"单据规则不存在".Equals(billNumber)) newDataItems[item.Key] = billNumber;
                        else newDataItems[item.Key] = string.Empty;

                        break;
                    case PoxiaoKeyConst.MODIFYUSER:
                        newDataItems[item.Key] = string.Empty;
                        break;
                    case PoxiaoKeyConst.CREATEUSER:
                        newDataItems[item.Key] = userInfo.Id;
                        break;
                    case PoxiaoKeyConst.MODIFYTIME:
                        newDataItems[item.Key] = string.Empty;
                        break;
                    case PoxiaoKeyConst.CREATETIME:
                        newDataItems[item.Key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                        break;
                    case PoxiaoKeyConst.CURRPOSITION:
                        string? pid = await _repository.AsSugarClient().Queryable<UserEntity, PositionEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.PositionId))
                            .Where((a, b) => a.Id == userInfo.Id && a.DeleteMark == null).Select((a, b) => a.PositionId).FirstAsync();
                        if (pid.IsNotEmptyOrNull()) newDataItems[item.Key] = pid;
                        else newDataItems[item.Key] = string.Empty;

                        break;
                    case PoxiaoKeyConst.CURRORGANIZE:
                        if (userInfo.OrganizeId != null) newDataItems[item.Key] = userInfo.OrganizeId;
                        else newDataItems[item.Key] = string.Empty;
                        break;
                }
            }

            if (newDataItems.ContainsKey(errorKey))
            {
                if (dataItems.ContainsKey(errorKey)) dataItems[errorKey] = newDataItems[errorKey].ToString();
                else dataItems.Add(errorKey, newDataItems[errorKey]);
                resList.Add(dataItems);
            }
            else
            {
                resList.Add(newDataItems);
            }
        }

        return resList;
    }

    /// <summary>
    /// 处理静态数据.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private List<Dictionary<string, string>> GetStaticList(FieldsModel model)
    {
        PropsBeanModel? props = model.props.props;
        List<OptionsModel>? optionList = GetTreeOptions(model.options, props);
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        foreach (OptionsModel? item in optionList)
        {
            Dictionary<string, string> option = new Dictionary<string, string>();
            option.Add(item.value, item.label);
            list.Add(option);
        }

        return list;
    }

    /// <summary>
    /// options无限级.
    /// </summary>
    /// <returns></returns>
    private List<OptionsModel> GetTreeOptions(List<Dictionary<string, object>> model, PropsBeanModel props)
    {
        List<OptionsModel> options = new List<OptionsModel>();
        foreach (object? item in model)
        {
            OptionsModel option = new OptionsModel();
            Dictionary<string, object>? dicObject = item.ToJsonString().ToObject<Dictionary<string, object>>();
            option.label = dicObject[props.label].ToString();
            option.value = dicObject[props.value].ToString();
            if (dicObject.ContainsKey(props.children))
            {
                List<Dictionary<string, object>>? children = dicObject[props.children].ToJsonString().ToObject<List<Dictionary<string, object>>>();
                options.AddRange(GetTreeOptions(children, props));
            }

            options.Add(option);
        }

        return options;
    }

    /// <summary>
    /// 获取动态无限级数据.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="props"></param>
    /// <returns></returns>
    private List<Dictionary<string, string>> GetDynamicInfiniteData(string data, PropsBeanModel props)
    {
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        string? value = props.value;
        string? label = props.label;
        string? children = props.children;
        foreach (JToken? info in JToken.Parse(data))
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic[info.Value<string>(value)] = info.Value<string>(label);
            list.Add(dic);
            if (info.Value<object>(children) != null && info.Value<object>(children).ToString() != string.Empty)
                list.AddRange(GetDynamicInfiniteData(info.Value<object>(children).ToString(), props));
        }

        return list;
    }

    /// <summary>
    /// 递归获取手动添加的省市区,名称处理成树形结构.
    /// </summary>
    /// <param name="addressEntityList"></param>
    private string GetAddressByPList(List<ProvinceEntity> addressEntityList, ProvinceEntity pEntity)
    {
        if (pEntity.ParentId == null || pEntity.ParentId.Equals("-1"))
        {
            return pEntity.FullName;
        }
        else
        {
            var pItem = addressEntityList.Find(x => x.Id == pEntity.ParentId);
            if (pItem != null) pEntity.QuickQuery = GetAddressByPList(addressEntityList, pItem) + "/" + pEntity.FullName;
            else pEntity.QuickQuery = pEntity.FullName;
            return pEntity.QuickQuery;
        }
    }

    /// <summary>
    /// 递归获取手动添加的省市区,Id处理成树形结构.
    /// </summary>
    /// <param name="addressEntityList"></param>
    private string GetAddressIdByPList(List<ProvinceEntity> addressEntityList, ProvinceEntity pEntity)
    {
        if (pEntity.ParentId == null || pEntity.ParentId.Equals("-1"))
        {
            return pEntity.Id;
        }
        else
        {
            var pItem = addressEntityList.Find(x => x.Id == pEntity.ParentId);
            if (pItem != null) pEntity.Id = GetAddressIdByPList(addressEntityList, pItem) + "," + pEntity.Id;
            else pEntity.Id = pEntity.Id;
            return pEntity.Id;
        }
    }
}