using System.Data;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos;
using Poxiao.Infrastructure.Dtos.VisualDev;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Models.VisualDev;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.FriendlyException;
using Poxiao.JsonSerialization;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.UnifyResult;
using Poxiao.VisualDev.Engine;
using Poxiao.VisualDev.Engine.Core;
using Poxiao.VisualDev.Entitys;
using Poxiao.VisualDev.Entitys.Dto.VisualDevModelData;
using Poxiao.VisualDev.Interfaces;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Interfaces.Repository;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Poxiao.VisualDev;

/// <summary>
/// 在线开发运行服务 .
/// </summary>
public class RunService : IRunService, ITransient
{
    #region 构造

    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// 数据库配置选项.
    /// </summary>
    public ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualDevEntity> _visualDevRepository;  // 在线开发功能实体

    /// <summary>
    /// 表单数据解析.
    /// </summary>
    private readonly FormDataParsing _formDataParsing;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _databaseService;

    /// <summary>
    /// 单据.
    /// </summary>
    private readonly IBillRullService _billRuleService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 数据接口.
    /// </summary>
    private readonly IDataInterfaceService _dataInterfaceService;

    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 流程数据.
    /// </summary>
    private readonly IFlowTaskRepository _flowTaskRepository;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// 事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 构造.
    /// </summary>
    public RunService(
        IOptions<ConnectionStringsOptions> connectionOptions,
        IServiceScopeFactory serviceScopeFactory,
        ISqlSugarRepository<VisualDevEntity> visualDevRepository,
        FormDataParsing formDataParsing,
        IOptions<TenantOptions> tenantOptions,
        IUserManager userManager,
        IDbLinkService dbLinkService,
        IDataBaseManager databaseService,
        IFlowTaskRepository flowTaskRepository,
        IDataInterfaceService dataInterfaceService,
        ISqlSugarClient context,
        IBillRullService billRuleService)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _visualDevRepository = visualDevRepository;
        _dataInterfaceService = dataInterfaceService;
        _formDataParsing = formDataParsing;
        _userManager = userManager;
        _tenant = tenantOptions.Value;
        _databaseService = databaseService;
        _dbLinkService = dbLinkService;
        _billRuleService = billRuleService;
        _flowTaskRepository = flowTaskRepository;
        _connectionStrings = connectionOptions.Value;
        _db = context.AsTenant();
    }
    #endregion

    #region Get

    /// <summary>
    /// 列表数据处理.
    /// </summary>
    /// <param name="entity">功能实体.</param>
    /// <param name="input">查询参数.</param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public async Task<PageResult<Dictionary<string, object>>> GetListResult(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List")
    {
        PageResult<Dictionary<string, object>>? realList = new PageResult<Dictionary<string, object>>() { list = new List<Dictionary<string, object>>() }; // 返回结果集
        TemplateParsingBase templateInfo = new TemplateParsingBase(entity); // 解析模板控件
        if (entity.WebType.Equals(4)) return await GetDataViewResults(templateInfo, input); // 数据视图

        // 处理查询
        Dictionary<string, object> queryJson = string.IsNullOrEmpty(input.QueryJson) ? null : input.QueryJson.ToObject<Dictionary<string, object>>();
        if (queryJson != null)
        {
            foreach (KeyValuePair<string, object> item in queryJson)
            {
                if (!templateInfo.ColumnData.searchList.Any(it => it.__vModel__.Equals(item.Key)))
                {
                    var vmodel = templateInfo.AllFieldsModel.Find(it => it.__vModel__.Equals(item.Key));
                    templateInfo.ColumnData.searchList.Add(vmodel.ToObject<IndexSearchFieldModel>());
                }
                if (!templateInfo.AppColumnData.searchList.Any(it => it.__vModel__.Equals(item.Key)))
                {
                    var vmodel = templateInfo.AllFieldsModel.Find(it => it.__vModel__.Equals(item.Key));
                    templateInfo.AppColumnData.searchList.Add(vmodel.ToObject<IndexSearchFieldModel>());
                }
            }
        }

        input.superQueryJson = GetSuperQueryInput(input.superQueryJson);

        string? primaryKey = "F_Id"; // 列表主键

        // 获取请求端类型，并对应获取 数据权限
        DbLinkEntity link = await GetDbLink(entity.DbLinkId);
        templateInfo.DbLink = link;
        await SyncField(templateInfo); // 同步业务字段
        primaryKey = GetPrimary(link, templateInfo.MainTableName);
        bool udp = _userManager.UserOrigin == "pc" ? templateInfo.ColumnData.useDataPermission : templateInfo.AppColumnData.useDataPermission;
        templateInfo.ColumnData = _userManager.UserOrigin == "pc" ? templateInfo.ColumnData : templateInfo.AppColumnData;
        var pvalue = new List<IConditionalModel>();
        if (_userManager.User != null || _userManager.UserId.IsNotEmptyOrNull()) pvalue = await _userManager.GetCondition<Dictionary<string, object>>(primaryKey, input.MenuId, udp);
        var pvalueJson = pvalue.ToJsonString();
        foreach (var item in templateInfo.AllTableFields)
        {
            if (pvalueJson.Contains(string.Format("\"FieldName\":\"{0}\",", item.Key)))
                pvalueJson.Replace(string.Format("\"FieldName\":\"{0}\",", item.Value), string.Format("\"FieldName\":\"{0}\",", item.Key));
        }
        pvalue = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pvalueJson);
        if (templateInfo.ColumnData.type.Equals(5))
        {
            pvalue.Clear(); // 树形表格 去掉数据权限.
            input.PageSize = 999999;
        }

        // 所有查询条件
        input.dataRuleJson = _userManager.UserOrigin == "pc" ? templateInfo.DataRuleListJson.ToJsonString() : templateInfo.AppDataRuleListJson.ToJsonString(); // 数据过滤
        var dataRuleWhere = new List<IConditionalModel>();
        var queryWhere = new List<IConditionalModel>();
        var superQueryWhere = new List<IConditionalModel>();
        if (input.dataRuleJson.IsNotEmptyOrNull()) dataRuleWhere = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(input.dataRuleJson);
        if (input.QueryJson.IsNotEmptyOrNull()) queryWhere = GetQueryJson(input.QueryJson, _userManager.UserOrigin == "pc" ? templateInfo.ColumnData : templateInfo.AppColumnData);
        if (input.superQueryJson.IsNotEmptyOrNull()) superQueryWhere = GetSuperQueryJson(input.superQueryJson, templateInfo);

        if (templateInfo.ColumnData.type == 4) await OptimisticLocking(link, templateInfo); // 开启行编辑 处理 开启并发锁定
        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个)
        string? sql = GetListQuerySql(primaryKey, templateInfo, ref input, ref tableFieldKeyValue, pvalue); // 查询sql

        // 如果排序字段没有在显示列中，按默认排序
        if (input.Sidx.IsNotEmptyOrNull() && !templateInfo.ColumnData.columnList.Any(x => x.prop == input.Sidx)) input.Sidx = string.Empty;

        // 处理查询
        input.QueryJson = GetQueryJson(input.QueryJson, templateInfo.ColumnData).ToJsonString();
        input.superQueryJson = GetSuperQueryJson(input.superQueryJson, templateInfo).ToJsonString();

        realList = _databaseService.GetInterFaceData(link, sql, input, templateInfo.ColumnData.Adapt<MainBeltViceQueryModel>(), new List<IConditionalModel>(), tableFieldKeyValue);

        // 显示列有子表字段
        if ((templateInfo.ColumnData.type != 4 || entity.isShortLink) && templateInfo.ColumnData.columnList.Any(x => templateInfo.ChildTableFields.ContainsKey(x.__vModel__) || templateInfo.ChildTableFields.ContainsKey(x.prop)) && realList.list.Any())
            realList = await GetListChildTable(templateInfo, primaryKey, queryWhere, dataRuleWhere, superQueryWhere, realList, pvalue);

        // 处理 自增长ID 流程表单 自增长Id转成 流程Id
        if (entity.FlowId.IsNotEmptyOrNull() && entity.EnableFlow.Equals(1) && realList.list.Any())
        {
            var ids = realList.list.Select(x => x[primaryKey]).ToList();
            var newIds = GetPIdsByFlowIds(link, templateInfo, primaryKey, ids.ToObject<List<string>>(), true);
            realList.list.ForEach(item => item[primaryKey] = newIds.First(x => x.Value.Equals(item[primaryKey].ToString())).Key);
        }

        input.Sidx = string.IsNullOrEmpty(input.Sidx) ? (templateInfo.ColumnData.defaultSidx == string.Empty ? primaryKey : templateInfo.ColumnData.defaultSidx) : input.Sidx;

        // 增加前端回显字段 : key_name
        var roweditId = SnowflakeIdHelper.NextId();
        if (templateInfo.ColumnData.type.Equals(4) && _userManager.UserOrigin.Equals("pc"))
        {
            realList.list.ForEach(items =>
            {
                var addItem = new Dictionary<string, object>();
                foreach (var item in items) if (item.Key != "RowIndex") addItem.Add(item.Key + roweditId, item.Value);
                foreach (var item in addItem) items.Add(item.Key, item.Value);
            });
        }

        if (realList.list.Any())
        {
            // 树形表格
            if (templateInfo.ColumnData.type.Equals(5))
                realList.list.ForEach(item => item[templateInfo.ColumnData.parentField + "_pid"] = item[templateInfo.ColumnData.parentField]);

            // 数据解析
            if (templateInfo.SingleFormData.Any(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()))
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()).ToList(), realList.list, templateInfo.ColumnData, actionType, templateInfo.WebType, primaryKey, entity.isShortLink);
            realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => x.__config__.templateJson == null || !x.__config__.templateJson.Any()).ToList(), realList.list, templateInfo.ColumnData, actionType, templateInfo.WebType, primaryKey, entity.isShortLink);

            // 如果是无表数据并且排序字段不为空，再进行数据排序
            if (!templateInfo.IsHasTable && input.Sidx.IsNotEmptyOrNull())
            {
                if (input.Sort == "desc")
                {
                    realList.list = realList.list.OrderByDescending(x =>
                    {
                        var dic = x as IDictionary<string, object>;
                        dic.GetOrAdd(input.Sidx, () => null);
                        return dic[input.Sidx];
                    }).ToList();
                }
                else
                {
                    realList.list = realList.list.OrderBy(x =>
                    {
                        var dic = x as IDictionary<string, object>;
                        dic.GetOrAdd(input.Sidx, () => null);
                        return dic[input.Sidx];
                    }).ToList();
                }
            }
        }

        if (input.dataType == "0")
        {
            if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
            {
            }
            else
            {
                realList.pagination = new PageInfo();
                realList.pagination.total = realList.list.Count;
                realList.pagination.pageSize = input.PageSize;
                realList.pagination.currentPage = input.CurrentPage;
                realList.list = realList.list.Skip(input.PageSize * (input.CurrentPage - 1)).Take(input.PageSize).ToList();
            }

            // 分组表格
            if (templateInfo.ColumnData.type == 3 && _userManager.UserOrigin == "pc")
                realList.list = CodeGenHelper.GetGroupList(realList.list, templateInfo.ColumnData.groupField, templateInfo.ColumnData.columnList.Find(x => x.__vModel__.ToLower() != templateInfo.ColumnData.groupField.ToLower()).__vModel__);

            // 树形表格
            if (templateInfo.ColumnData.type.Equals(5))
                realList.list = CodeGenHelper.GetTreeList(realList.list, templateInfo.ColumnData.parentField + "_pid", templateInfo.ColumnData.columnList.Find(x => x.__vModel__.ToLower() != templateInfo.ColumnData.parentField.ToLower()).__vModel__);
        }
        else
        {
            if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
            {
            }
            else
            {
                realList.pagination = new PageInfo();
                realList.pagination.total = realList.list.Count;
                realList.pagination.pageSize = input.PageSize;
                realList.pagination.currentPage = input.CurrentPage;
                realList.list = realList.list.ToList();
            }
        }

        // 增加前端回显字段 : key_name
        if (!entity.isShortLink && templateInfo.ColumnData.type.Equals(4) && _userManager.UserOrigin.Equals("pc"))
        {
            var newList = new List<Dictionary<string, object>>();
            realList.list.ForEach(items =>
            {
                var newItem = new Dictionary<string, object>();
                foreach (var item in items)
                {
                    if (item.Key.Contains(roweditId))
                    {
                        if (item.Value.IsNotEmptyOrNull())
                        {
                            var obj = item.Value;
                            if (obj.ToString().Contains("[[")) obj = item.Value.ToString().ToObject<List<List<object>>>();
                            else if (obj.ToString().Contains("[")) obj = item.Value.ToString().ToObject<List<object>>();

                            var value = items.FirstOrDefault(x => x.Key == item.Key.Replace(roweditId, string.Empty)).Value;
                            if (value.IsNullOrEmpty()) obj = null;
                            if (!newItem.ContainsKey(item.Key.Replace(roweditId, string.Empty))) newItem.Add(item.Key.Replace(roweditId, string.Empty), obj);
                            if (!newItem.ContainsKey(item.Key.Replace(roweditId, string.Empty) + "_name")) newItem.Add(item.Key.Replace(roweditId, string.Empty) + "_name", value);
                        }
                        else
                        {
                            if (!newItem.ContainsKey(item.Key.Replace(roweditId, string.Empty))) newItem.Add(item.Key.Replace(roweditId, string.Empty), null);
                            if (!newItem.ContainsKey(item.Key.Replace(roweditId, string.Empty) + "_name")) newItem.Add(item.Key.Replace(roweditId, string.Empty) + "_name", null);
                        }
                    }
                    if (item.Key.Equals("flowState") || item.Key.Equals("flowState_name") || item.Key.Equals("flowId") || item.Key.Equals("flowId_name")) newItem.Add(item.Key, item.Value);
                    if (item.Key.Equals("id") && !newItem.ContainsKey(item.Key)) newItem.Add(item.Key, item.Value);
                    if (templateInfo.AllFieldsModel.Any(x => x.__vModel__.Equals(item.Key) && (x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TIME)
                    || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATETIME) || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATEUSER) || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYTIME)
                    || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYUSER) || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CURRDEPT) || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CURRORGANIZE)
                    || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CURRPOSITION))))
                        newItem[item.Key] = items[item.Key];
                }

                newList.Add(newItem);
            });
            realList.list = newList;
        }

        return realList;
    }

    /// <summary>
    /// 关联表单列表数据处理.
    /// </summary>
    /// <param name="entity">功能实体.</param>
    /// <param name="input">查询参数.</param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public async Task<PageResult<Dictionary<string, object>>> GetRelationFormList(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List")
    {
        PageResult<Dictionary<string, object>>? realList = new PageResult<Dictionary<string, object>>() { list = new List<Dictionary<string, object>>() }; // 返回结果集
        TemplateParsingBase? templateInfo = new TemplateParsingBase(entity); // 解析模板控件
        if (entity.WebType.Equals(4)) return await GetDataViewResults(templateInfo, input); // 数据视图
        string? primaryKey = "F_Id"; // 列表主键

        List<IConditionalModel>? pvalue = new List<IConditionalModel>(); // 关联表单调用 数据全部放开

        DbLinkEntity link = await GetDbLink(entity.DbLinkId);
        primaryKey = GetPrimary(link, templateInfo.MainTableName);
        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个)
        string? sql = GetListQuerySql(primaryKey, templateInfo, ref input, ref tableFieldKeyValue, pvalue, true); // 查询sql

        // 如果排序字段没有在显示列中，按默认排序
        if (input.Sidx.IsNotEmptyOrNull() && !templateInfo.ColumnData.columnList.Any(x => x.prop == input.Sidx)) input.Sidx = string.Empty;

        string? queryJson = input.QueryJson;
        input.QueryJson = string.Empty;

        realList = _databaseService.GetInterFaceData(link, sql, input, templateInfo.ColumnData.Adapt<MainBeltViceQueryModel>(), pvalue, tableFieldKeyValue);
        input.QueryJson = queryJson;

        // 处理 自增长ID 流程表单 自增长Id转成 流程Id
        if (entity.FlowId.IsNotEmptyOrNull() && entity.EnableFlow.Equals(1) && realList.list.Any())
        {
            var ids = realList.list.Select(x => x[primaryKey]).ToList();
            var newIds = GetPIdsByFlowIds(link, templateInfo, primaryKey, ids.ToObject<List<string>>(), true);
            realList.list.ForEach(item => item[primaryKey] = newIds.First(x => x.Value.Equals(item[primaryKey].ToString())).Key);
        }

        input.Sidx = string.IsNullOrEmpty(input.Sidx) ? (templateInfo.ColumnData.defaultSidx == string.Empty ? primaryKey : templateInfo.ColumnData.defaultSidx) : input.Sidx;

        if (realList.list.Any())
        {
            if (templateInfo.SingleFormData.Any(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()))
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()).ToList(), realList.list, templateInfo.ColumnData, actionType, templateInfo.WebType, primaryKey);
            realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => !x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM) && (x.__config__.templateJson == null || !x.__config__.templateJson.Any())).ToList(), realList.list, templateInfo.ColumnData, actionType, templateInfo.WebType.ParseToInt(), primaryKey);

            if (input.QueryJson.IsNotEmptyOrNull())
            {
                Dictionary<string, string>? search = input.QueryJson.ToObject<Dictionary<string, string>>();
                if (search.FirstOrDefault().Value.IsNotEmptyOrNull())
                {
                    string? keword = search.FirstOrDefault().Value;
                    List<Dictionary<string, object>>? newList = new List<Dictionary<string, object>>();
                    List<string>? columnName = templateInfo.ColumnData.columnList.Select(x => x.prop).ToList();
                    realList.list.ForEach(item =>
                    {
                        if (item.Where(x => x.Value != null && columnName.Contains(x.Key)).Where(x => x.Value.ToString().Contains(keword)).Any()) newList.Add(item);
                    });

                    realList.list = newList;
                }
            }

            if (input.Sort == "desc")
            {
                realList.list = realList.list.OrderByDescending(x =>
                {
                    IDictionary<string, object>? dic = x as IDictionary<string, object>;
                    dic.GetOrAdd(input.Sidx, () => null);
                    return dic[input.Sidx];
                }).ToList();
            }
            else
            {
                realList.list = realList.list.OrderBy(x =>
                {
                    IDictionary<string, object>? dic = x as IDictionary<string, object>;
                    dic.GetOrAdd(input.Sidx, () => null);
                    return dic[input.Sidx];
                }).ToList();
            }
        }

        if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
        {
        }
        else
        {
            realList.pagination = new PageInfo();
            realList.pagination.total = realList.list.Count;
            realList.pagination.pageSize = input.PageSize;
            realList.pagination.currentPage = input.CurrentPage;
            realList.list = realList.list.ToList();
        }

        return realList;
    }

    /// <summary>
    /// 获取有表详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetHaveTableInfo(string id, VisualDevEntity templateEntity)
    {
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
        string? mainPrimary = GetPrimary(link, templateInfo.MainTableName);
        await OptimisticLocking(link, templateInfo); // 处理 开启 并发锁定
        if (id.Equals("0") || id.IsNullOrWhiteSpace()) return new Dictionary<string, object>();
        id = GetPIdsByFlowIds(link, templateInfo, mainPrimary, new List<string>() { id }).First().Value;
        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段 别名
        tableFieldKeyValue[mainPrimary.ToUpper()] = mainPrimary;
        if (templateInfo.WebType.Equals(3)) tableFieldKeyValue["F_FlowId".ToUpper()] = "F_FlowId";
        var sql = GetInfoQuerySql(id, mainPrimary, templateInfo, ref tableFieldKeyValue); // 获取查询Sql
        Dictionary<string, object>? data = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
        if (data == null) return null;

        // 记录全部数据
        Dictionary<string, object> dataMap = new Dictionary<string, object>();

        // 查询别名转换
        if (templateInfo.AuxiliaryTableFieldsModelList.Any()) foreach (KeyValuePair<string, object> item in data) dataMap.Add(tableFieldKeyValue[item.Key.ToUpper()], item.Value);
        else dataMap = data;

        Dictionary<string, object> newDataMap = new Dictionary<string, object>();

        dataMap = _formDataParsing.GetTableDataInfo(new List<Dictionary<string, object>>() { dataMap }, templateInfo.FieldsModelList, "detail").FirstOrDefault();

        // 处理子表数据
        newDataMap = await GetChildTableData(templateInfo, link, dataMap, newDataMap, false);

        int dicCount = newDataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        newDataMap.Keys.CopyTo(strKey, 0);
        for (int i = 0; i < strKey.Length; i++)
        {
            FieldsModel? model = templateInfo.FieldsModelList.Where(m => m.__vModel__ == strKey[i]).FirstOrDefault();
            if (model != null)
            {
                List<Dictionary<string, object>> tables = newDataMap[strKey[i]].ToObject<List<Dictionary<string, object>>>();
                List<Dictionary<string, object>> newTables = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object>? item in tables)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> value in item)
                    {
                        FieldsModel? child = model.__config__.children.Find(c => c.__vModel__ == value.Key);
                        if (child != null || value.Key.Equals("id")) dic.Add(value.Key, value.Value);
                    }

                    newTables.Add(dic);
                }

                if (newTables.Count > 0) newDataMap[strKey[i]] = newTables;
            }
        }

        foreach (KeyValuePair<string, object> entryMap in dataMap)
        {
            FieldsModel? model = templateInfo.FieldsModelList.Where(m => m.__vModel__.ToLower() == entryMap.Key.ToLower()).FirstOrDefault();
            if (model != null && entryMap.Key.ToLower().Equals(model.__vModel__.ToLower())) newDataMap[entryMap.Key] = entryMap.Value;
        }

        if (!newDataMap.ContainsKey("id")) newDataMap.Add("id", data[mainPrimary]);
        _formDataParsing.GetBARAndQR(templateInfo.FieldsModelList, newDataMap, dataMap); // 处理 条形码 、 二维码 控件
        if (dataMap.ContainsKey("F_FlowId")) newDataMap["flowId"] = dataMap["F_FlowId"];
        return await _formDataParsing.GetSystemComponentsData(templateInfo.FieldsModelList, newDataMap.ToJsonString());
    }

    /// <summary>
    /// 获取有表详情转换.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="isFlowTask"></param>
    /// <returns></returns>
    public async Task<string> GetHaveTableInfoDetails(string id, VisualDevEntity templateEntity, bool isFlowTask = false)
    {
        TemplateParsingBase? templateInfo = new TemplateParsingBase(templateEntity, isFlowTask); // 解析模板控件
        DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
        string? mainPrimary = GetPrimary(link, templateInfo.MainTableName);
        id = GetPIdsByFlowIds(link, templateInfo, mainPrimary, new List<string>() { id }).First().Value;
        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段 别名
        tableFieldKeyValue[mainPrimary.ToUpper()] = mainPrimary;
        if (templateInfo.WebType.Equals(3)) tableFieldKeyValue["F_FlowId".ToUpper()] = "F_FlowId";
        var sql = GetInfoQuerySql(id, mainPrimary, templateInfo, ref tableFieldKeyValue); // 获取查询Sql

        Dictionary<string, object>? data = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, string>>>().ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
        if (data == null) return id;

        // 记录全部数据
        Dictionary<string, object> dataMap = new Dictionary<string, object>();

        // 查询别名转换
        if (templateInfo.AuxiliaryTableFieldsModelList.Any()) foreach (KeyValuePair<string, object> item in data) dataMap.Add(tableFieldKeyValue[item.Key.ToUpper()], item.Value);
        else dataMap = data;

        Dictionary<string, object> newDataMap = new Dictionary<string, object>();

        // 处理子表数据
        newDataMap = await GetChildTableData(templateInfo, link, dataMap, newDataMap, true);

        int dicCount = newDataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        newDataMap.Keys.CopyTo(strKey, 0);
        for (int i = 0; i < strKey.Length; i++)
        {
            FieldsModel? model = templateInfo.FieldsModelList.Find(m => m.__vModel__ == strKey[i]);
            if (model != null)
            {
                List<Dictionary<string, object>> childModelData = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object>? item in newDataMap[strKey[i]].ToObject<List<Dictionary<string, object>>>())
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> value in item)
                    {
                        FieldsModel? child = model.__config__.children.Find(c => c.__vModel__ == value.Key);
                        if (child != null && value.Value != null)
                        {
                            if (child.__config__.poxiaoKey.Equals(PoxiaoKeyConst.DATE))
                            {
                                var keyValue = value.Value.ToString();
                                DateTime dtDate;
                                if (DateTime.TryParse(keyValue, out dtDate)) dic.Add(value.Key, keyValue.ParseToDateTime().ParseToUnixTime());
                                else dic.Add(value.Key, value.Value.ToString().TimeStampToDateTime().ParseToUnixTime());
                            }
                            else dic.Add(value.Key, value.Value);
                        }
                        else dic.Add(value.Key, value.Value);
                    }

                    childModelData.Add(dic);
                }

                if (childModelData.Count > 0)
                {
                    // 将关键字查询传输的id转换成名称
                    if (model.__config__.children.Any(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()))
                        newDataMap[strKey[i]] = await _formDataParsing.GetKeyData(model.__config__.children.Where(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()).ToList(), childModelData, templateInfo.ColumnData, "List", templateInfo.WebType, mainPrimary, templateEntity.isShortLink);
                    newDataMap[strKey[i]] = await _formDataParsing.GetKeyData(model.__config__.children.Where(x => x.__config__.templateJson == null || !x.__config__.templateJson.Any()).ToList(), childModelData, templateInfo.ColumnData.ToObject<ColumnDesignModel>(), "List", templateInfo.WebType, mainPrimary, templateEntity.isShortLink);
                }
            }
        }

        List<Dictionary<string, object>> listEntity = new List<Dictionary<string, object>>() { dataMap };

        // 控件联动
        var tempDataMap = new Dictionary<string, object>();
        if (templateInfo.SingleFormData.Any(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()))
            tempDataMap = (await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()).ToList(), listEntity, templateInfo.ColumnData, "List", templateInfo.WebType, mainPrimary, templateEntity.isShortLink)).FirstOrDefault();
        tempDataMap = (await _formDataParsing.GetKeyData(templateInfo.SingleFormData.Where(x => x.__config__.templateJson == null || !x.__config__.templateJson.Any()).ToList(), listEntity, templateInfo.ColumnData, "List", templateInfo.WebType, mainPrimary, templateEntity.isShortLink)).FirstOrDefault();

        // 将关键字查询传输的id转换成名称
        foreach (var entryMap in tempDataMap)
        {
            if (entryMap.Value != null)
            {
                var model = templateInfo.FieldsModelList.Where(m => m.__vModel__.Contains(entryMap.Key)).FirstOrDefault();
                if (model != null && entryMap.Key.Equals(model.__vModel__)) newDataMap[entryMap.Key] = entryMap.Value;
                else if (templateInfo.FieldsModelList.Where(m => m.__vModel__ == entryMap.Key.Replace("_id", string.Empty)).Any()) newDataMap[entryMap.Key] = entryMap.Value;
            }
        }

        _formDataParsing.GetBARAndQR(templateInfo.FieldsModelList, newDataMap, dataMap); // 处理 条形码 、 二维码 控件

        if (!newDataMap.ContainsKey("id")) newDataMap.Add("id", id);
        return newDataMap.ToJsonString();
    }

    #endregion

    #region Post

    /// <summary>
    /// 创建在线开发功能.
    /// </summary>
    /// <param name="templateEntity">功能模板实体.</param>
    /// <param name="dataInput">数据输入.</param>
    /// <returns></returns>
    public async Task Create(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput)
    {
        string? mainId = SnowflakeIdHelper.NextId();
        DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
        var haveTableSql = await CreateHaveTableSql(templateEntity, dataInput, mainId);

        // 主表自增长Id.
        if (haveTableSql.ContainsKey("MainTableReturnIdentity")) haveTableSql.Remove("MainTableReturnIdentity");

        try
        {
            _db.BeginTran();
            foreach (var item in haveTableSql) await _databaseService.ExecuteSql(link, item.Key, item.Value); // 新增功能数据
            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 创建有表SQL.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="dataInput"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, List<Dictionary<string, object>>>> CreateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput, string mainId)
    {
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        templateInfo.DbLink = await GetDbLink(templateEntity.DbLinkId);
        return await GetCreateSqlByTemplate(templateInfo, dataInput, mainId);
    }
    public async Task<Dictionary<string, List<Dictionary<string, object>>>> GetCreateSqlByTemplate(TemplateParsingBase templateInfo, VisualDevModelDataCrInput dataInput, string mainId)
    {
        await SyncField(templateInfo); // 同步业务字段
        Dictionary<string, object>? allDataMap = dataInput.data.ToObject<Dictionary<string, object>>();
        if (!templateInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板

        // 处理系统控件(模板开启行编辑)
        if (templateInfo.ColumnData.type.Equals(4) && _userManager.UserOrigin.Equals("pc"))
        {
            templateInfo.GenerateFields.ForEach(item =>
            {
                if (!allDataMap.ContainsKey(item.__vModel__)) allDataMap.Add(item.__vModel__, string.Empty);
                if (item.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATETIME) && allDataMap.ContainsKey(item.__vModel__))
                {
                    var value = allDataMap[item.__vModel__].ToString();
                    allDataMap.Remove(item.__vModel__);
                    allDataMap.Add(item.__vModel__, DateTime.Now.ToString());
                }
            });
        }

        if (templateInfo.visualDevEntity != null && !templateInfo.visualDevEntity.isShortLink)
            allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, true); // 生成系统自动生成字段
        DbLinkEntity link = templateInfo.DbLink;

        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 表结构 信息
        DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
        string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.AsSugarClient().CurrentConnectionConfig.DbType.ToString();

        // 验证唯一值
        UniqueVerify(link, templateInfo, allDataMap, mainPrimary?.field, mainId, false);

        // 新增SQL
        Dictionary<string, List<Dictionary<string, object>>> dictionarySql = new Dictionary<string, List<Dictionary<string, object>>>();
        var tableField = new Dictionary<string, object>(); // 字段和值
        templateInfo?.MainTableFieldsModelList.ForEach(item =>
        {
            if (allDataMap.ContainsKey(item.__vModel__))
            {
                object? itemData = allDataMap[item.__vModel__];
                if (item.__vModel__.IsNotEmptyOrNull() && itemData != null && !string.IsNullOrEmpty(itemData.ToString()) && itemData.ToString() != "[]")
                {
                    var value = _formDataParsing.InsertValueHandle(dbType, tableList, item.__vModel__, itemData, templateInfo.MainTableFieldsModelList, "create", templateInfo.visualDevEntity != null ? templateInfo.visualDevEntity.isShortLink : false);
                    tableField.Add(item.__vModel__, value);
                }
            }
        });

        if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN"))
        {
            var isolationField = _userManager.CurrentTenantInformation.connectionConfig.IsolationField;
            tableField.Add("f_tenantid", isolationField); // 多租户
        }

        // 主键策略(雪花Id)
        if (templateInfo.FormModel.primaryKeyPolicy.Equals(1)) tableField.Add(mainPrimary?.field, mainId);

        // 前端空提交
        if (!tableField.Any()) tableField.Add(tableList.Where(x => !x.primaryKey).First().field, null);

        // 拼接主表 sql
        dictionarySql.Add(templateInfo.MainTableName, new List<Dictionary<string, object>>() { tableField });

        // 流程表单 需要增加字段 F_FlowTaskId
        if (templateInfo.visualDevEntity != null && templateInfo.visualDevEntity.EnableFlow.Equals(1))
        {
            dictionarySql[templateInfo.MainTableName].First().Add("F_FlowTaskId", mainId);
            dictionarySql[templateInfo.MainTableName].First().Add("F_FlowId", allDataMap["flowId"]);
        }

        // 自增长主键 需要返回的自增id
        if (templateInfo.FormModel.primaryKeyPolicy.Equals(2))
        {
            var mainSql = dictionarySql.First();
            mainId = (await _databaseService.ExecuteReturnIdentityAsync(link, mainSql.Key, mainSql.Value)).ToString();
            if (mainId.Equals("0")) throw Oops.Oh(ErrorCode.D1402);
            tableField.Clear();
            dictionarySql.Clear();
            tableField.Add("ReturnIdentity", mainId);
            dictionarySql.Add("MainTableReturnIdentity", new List<Dictionary<string, object>>() { tableField });
        }

        // 拼接副表 sql
        if (templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList().ForEach(tbname =>
            {
                tableField = new Dictionary<string, object>();

                // 主键策略(雪花Id)
                if (templateInfo.FormModel.primaryKeyPolicy.Equals(1))
                    tableField.Add(_databaseService.GetFieldList(link, tbname)?.Find(x => x.primaryKey).field, SnowflakeIdHelper.NextId());

                // 外键
                tableField.Add(templateInfo?.AllTable?.Find(t => t.table == tbname).tableField, mainId);

                // 字段
                templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__vModel__).Where(x => x.Contains("poxiao_" + tbname + "_poxiao_")).ToList().ForEach(item =>
                {
                    object? itemData = allDataMap.Where(x => x.Key == item).Count() > 0 ? allDataMap[item] : null;
                    if (item.IsNotEmptyOrNull() && itemData != null && !string.IsNullOrEmpty(itemData.ToString()) && itemData.ToString() != "[]")
                    {
                        var value = _formDataParsing.InsertValueHandle(dbType, tableList, item, allDataMap[item], templateInfo.FieldsModelList, "create", templateInfo.visualDevEntity != null ? templateInfo.visualDevEntity.isShortLink : false);
                        tableField.Add(item.ReplaceRegex(@"(\w+)_poxiao_", string.Empty), value);
                    }
                });

                dictionarySql.Add(tbname, new List<Dictionary<string, object>>() { tableField });
            });
        }

        // 拼接子表 sql
        foreach (string? item in allDataMap.Where(d => d.Key.ToLower().Contains("tablefield")).Select(d => d.Key).ToList())
        {
            if (!templateInfo.AllFieldsModel.Any(x => x.__vModel__.Equals(item)) || !templateInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item)).__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)) continue;

            // 查找到该控件数据
            object? objectData = allDataMap[item];
            List<Dictionary<string, object>>? model = objectData.ToObject<List<Dictionary<string, object>>>();
            if (model != null && model.Count > 0)
            {
                // 利用key去找模板
                FieldsModel? fieldsModel = templateInfo.FieldsModelList.Find(f => f.__vModel__ == item);
                Engine.Model.TableModel? childTable = templateInfo.AllTable.Find(t => t.table == fieldsModel.__config__.tableName);
                tableList = new List<DbTableFieldModel>();
                tableList = _databaseService.GetFieldList(link, childTable?.table);
                DbTableFieldModel? childPrimary = tableList.Find(t => t.primaryKey);
                foreach (Dictionary<string, object>? data in model)
                {
                    tableField = new Dictionary<string, object>();

                    // 主键策略(雪花Id)
                    if (templateInfo.FormModel.primaryKeyPolicy.Equals(1)) tableField.Add(childPrimary.field, SnowflakeIdHelper.NextId());

                    // 外键
                    tableField.Add(childTable.tableField, mainId);

                    // 字段
                    foreach (KeyValuePair<string, object> child in data)
                    {
                        if (child.Key.Equals("id") && child.Value.IsNotEmptyOrNull())
                        {
                            tableField[childPrimary.field] = child.Value;
                        }
                        else if (child.Key.IsNotEmptyOrNull() && child.Value.IsNotEmptyOrNull() && child.Value.ToString() != "[]")
                        {
                            var value = _formDataParsing.InsertValueHandle(dbType, tableList, child.Key, child.Value, fieldsModel?.__config__.children, "create", templateInfo.visualDevEntity != null ? templateInfo.visualDevEntity.isShortLink : false);
                            tableField.Add(child.Key, value);
                        }
                    }

                    if (dictionarySql.ContainsKey(fieldsModel.__config__.tableName))
                        dictionarySql[fieldsModel.__config__.tableName].Add(tableField);
                    else
                        dictionarySql.Add(fieldsModel.__config__.tableName, new List<Dictionary<string, object>>() { tableField });
                }
            }
        }

        // 处理 开启 并发锁定
        await OptimisticLocking(link, templateInfo);
        return dictionarySql;
    }

    /// <summary>
    /// 修改在线开发功能.
    /// </summary>
    /// <param name="id">修改ID.</param>
    /// <param name="templateEntity"></param>
    /// <param name="visualdevModelDataUpForm"></param>
    /// <returns></returns>
    public async Task Update(string id, VisualDevEntity templateEntity, VisualDevModelDataUpInput visualdevModelDataUpForm)
    {
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        if (templateInfo.ColumnData.type.Equals(4) && _userManager.UserOrigin.Equals("pc"))
        {
            // 剔除 [增加前端回显字段 : key_name]
            Dictionary<string, object> oldDataMap = visualdevModelDataUpForm.data.ToObject<Dictionary<string, object>>();
            Dictionary<string, object> newDataMap = new Dictionary<string, object>();
            foreach (var item in oldDataMap)
            {
                var key = item.Key.Substring(0, item.Key.LastIndexOf("_name") != -1 ? item.Key.LastIndexOf("_name") : item.Key.Length);
                if (!newDataMap.ContainsKey(key) && oldDataMap.ContainsKey(key)) newDataMap.Add(key, oldDataMap[key]);
            }

            if (newDataMap.Any()) visualdevModelDataUpForm.data = newDataMap.ToJsonString();
        }

        DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
        var haveTableSql = await UpdateHaveTableSql(templateEntity, visualdevModelDataUpForm, id);

        try
        {
            _db.BeginTran();
            foreach (var item in haveTableSql) await _databaseService.ExecuteSql(link, item); // 修改功能数据
            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 修改有表SQL.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="visualdevModelDataUpForm"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<string>> UpdateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataUpInput visualdevModelDataUpForm, string id)
    {
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        templateInfo.DbLink = await GetDbLink(templateEntity.DbLinkId);
        return await GetUpdateSqlByTemplate(templateInfo, visualdevModelDataUpForm, id);
    }
    public async Task<List<string>> GetUpdateSqlByTemplate(TemplateParsingBase templateInfo, VisualDevModelDataUpInput visualdevModelDataUpForm, string id)
    {
        await SyncField(templateInfo); // 同步业务字段
        Dictionary<string, object>? allDataMap = visualdevModelDataUpForm.data.ToObject<Dictionary<string, object>>();
        if (!templateInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板

        // 处理系统控件(模板开启行编辑)
        if (templateInfo.ColumnData.type.Equals(4) && _userManager.UserOrigin.Equals("pc"))
        {
            // 处理显示列和提交的表单数据匹配(行编辑空数据 前端会过滤该控件)
            templateInfo.ColumnData.columnList.Where(x => !allDataMap.ContainsKey(x.prop) && x.__config__.visibility.Equals("pc")).ToList()
                .ForEach(item => allDataMap.Add(item.prop, string.Empty));

            templateInfo.GenerateFields.ForEach(item =>
            {
                if (!allDataMap.ContainsKey(item.__vModel__)) allDataMap.Add(item.__vModel__, string.Empty);
                if (item.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATETIME) && allDataMap.ContainsKey(item.__vModel__))
                {
                    var value = allDataMap[item.__vModel__].ToString();
                    allDataMap.Remove(item.__vModel__);
                    DateTime dtDate;
                    if (DateTime.TryParse(value, out dtDate)) value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", value);
                    else value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", value.TimeStampToDateTime());
                    allDataMap.Add(item.__vModel__, value);
                }
            });
        }

        allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, false); // 生成系统自动生成字段
        DbLinkEntity link = templateInfo.DbLink;
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 表结构 信息
        DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
        string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.AsSugarClient().CurrentConnectionConfig.DbType.ToString();
        id = GetPIdsByFlowIds(link, templateInfo, mainPrimary.field, new List<string>() { id }).First().Value;

        // 验证唯一值
        UniqueVerify(link, templateInfo, allDataMap, mainPrimary?.field, id, true);

        // 主表查询语句
        List<string> mainSql = new List<string>();
        var fieldSql = new List<string>(); // key 字段名, value 修改值

        // 拼接主表 sql
        templateInfo?.MainTableFieldsModelList.ForEach(item =>
        {
            if (item.__vModel__.IsNotEmptyOrNull() && allDataMap.ContainsKey(item.__vModel__))
                fieldSql.Add(string.Format("{0}={1}", item.__vModel__, _formDataParsing.InsertValueHandle(dbType, tableList, item.__vModel__, allDataMap[item.__vModel__], templateInfo.MainTableFieldsModelList, "update")));
        });

        if (fieldSql.Any()) mainSql.Add(string.Format("update {0} set {1} where {2}='{3}';", templateInfo?.MainTableName, string.Join(",", fieldSql), mainPrimary?.field, id));

        // 拼接副表 sql
        if (templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList().ForEach(tbname =>
            {
                List<DbTableFieldModel>? tableAllField = _databaseService.GetFieldList(link, tbname); // 数据库里获取表的所有字段

                List<string>? tableFieldList = templateInfo.AuxiliaryTableFieldsModelList.Where(x => x.__config__.tableName.Equals(tbname)).Select(x => x.__vModel__).ToList();

                fieldSql.Clear(); // key 字段名, value 修改值
                templateInfo.AuxiliaryTableFieldsModelList.Where(x => x.__config__.tableName.Equals(tbname)).Select(x => x.__vModel__).ToList().ForEach(item =>
                {
                    // 前端未填写数据的字段，默认会找不到字段名，需要验证
                    object? itemData = allDataMap.Where(x => x.Key == item).Count() > 0 ? allDataMap[item] : null;
                    if (item.IsNotEmptyOrNull() && itemData != null)
                        fieldSql.Add(string.Format("{0}={1}", item.ReplaceRegex(@"(\w+)_poxiao_", string.Empty), _formDataParsing.InsertValueHandle(dbType, tableList, item, allDataMap[item], templateInfo.FieldsModelList, "update")));
                });

                var tableField = templateInfo.AllTable.Find(t => t.table.Equals(tbname)).tableField;
                if (fieldSql.Any()) mainSql.Add(string.Format("update {0} set {1} where {2}='{3}';", tbname, string.Join(",", fieldSql), tableField, id));
            });
        }

        // 非行编辑
        if (!templateInfo.ColumnData.type.Equals(4) || !_userManager.UserOrigin.Equals("pc"))
        {
            // 删除子表数据
            if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
            {
                // 拼接子表 sql
                foreach (string? item in allDataMap.Where(d => d.Key.ToLower().Contains("tablefield")).Select(d => d.Key).ToList())
                {
                    if (!templateInfo.AllFieldsModel.Any(x => x.__vModel__.Equals(item)) || !templateInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item)).__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)) continue;

                    // 查找到该控件数据
                    List<Dictionary<string, object>>? modelData = allDataMap[item].ToObject<List<Dictionary<string, object>>>();

                    // 利用key去找模板
                    FieldsModel? fieldsModel = templateInfo.FieldsModelList.Find(f => f.__vModel__ == item);
                    ConfigModel? fieldsConfig = fieldsModel?.__config__;
                    List<string>? childColumn = new List<string>();
                    List<object>? childValues = new List<object>();
                    List<string>? updateFieldSql = new List<string>();
                    Engine.Model.TableModel? childTable = templateInfo.AllTable.Find(t => t.table == fieldsModel.__config__.tableName && t.table != templateInfo.MainTableName);
                    if (childTable != null)
                    {
                        if (modelData != null && modelData.Count > 0)
                        {
                            if (!modelData.Any(x => x.ContainsKey("id")))
                            {
                                mainSql.Add(string.Format("delete from {0} where {1}='{2}';", childTable?.table, childTable.tableField, id));
                            }
                            else
                            {
                                var ctIdList = modelData.Where(x => x.ContainsKey("id")).Select(x => x["id"]).ToObject<List<string>>();
                                var querStr = string.Format("select {0} id from {1} where {0} in('{2}') ", childTable.fields.First(x => x.PrimaryKey.Equals(1)).Field, childTable?.table, string.Join("','", ctIdList));
                                var res = _databaseService.GetInterFaceData(link, querStr, null).ToObject<List<Dictionary<string, string>>>();
                                foreach (var it in modelData.Where(x => x.ContainsKey("id"))) if (!res.Any(x => x["id"].Equals(it["id"]))) it.Remove("id");
                                mainSql.Add(string.Format("delete from {0} where {1} not in ('{2}') and {3}='{4}';", childTable?.table, childTable.fields.First(x => x.PrimaryKey.Equals(1)).Field, string.Join("','", modelData.Where(x => x.ContainsKey("id")).Select(x => x["id"]).ToList()), childTable.tableField, id));
                            }
                            tableList = new List<DbTableFieldModel>();
                            tableList = _databaseService.GetFieldList(link, childTable?.table);
                            DbTableFieldModel? childPrimary = tableList.Find(t => t.primaryKey);
                            foreach (Dictionary<string, object>? data in modelData)
                            {
                                if (data.Count > 0)
                                {
                                    foreach (KeyValuePair<string, object> child in data)
                                    {
                                        if (child.Key.IsNotEmptyOrNull() && child.Key != "id")
                                        {
                                            childColumn.Add(child.Key); // Column部分
                                            var value = _formDataParsing.InsertValueHandle(dbType, tableList, child.Key, child.Value, fieldsConfig.children, "update");
                                            childValues.Add(value); // Values部分
                                            updateFieldSql.Add(string.Format("{0}={1}", child.Key, value));
                                        }
                                    }

                                    if (childColumn.Any())
                                    {
                                        if (data.ContainsKey("id"))
                                        {
                                            if (updateFieldSql.Any())
                                                mainSql.Add(string.Format("update {0} set {1} where {2}='{3}';", fieldsModel.__config__.tableName, string.Join(',', updateFieldSql), childPrimary.field, data["id"]));
                                        }
                                        else
                                        {
                                            // 主键策略(雪花Id)
                                            if (templateInfo.FormModel.primaryKeyPolicy.Equals(1))
                                            {
                                                mainSql.Add(string.Format(
                                                "insert into {0}({6},{4}{1}) values('{3}','{5}'{2});",
                                                fieldsModel.__config__.tableName,
                                                childColumn.Any() ? "," + string.Join(",", childColumn) : string.Empty,
                                                childColumn.Any() ? "," + string.Join(",", childValues) : string.Empty,
                                                SnowflakeIdHelper.NextId(),
                                                childTable.tableField,
                                                id,
                                                childPrimary.field));
                                            }
                                            else
                                            {
                                                mainSql.Add(string.Format(
                                                "insert into {0}({1}{2}) values('{3}'{4});",
                                                fieldsModel.__config__.tableName,
                                                childTable.tableField,
                                                childColumn.Any() ? "," + string.Join(",", childColumn) : string.Empty,
                                                id,
                                                childColumn.Any() ? "," + string.Join(",", childValues) : string.Empty));
                                            }
                                        }
                                    }

                                    childColumn.Clear();
                                    childValues.Clear();
                                    updateFieldSql.Clear();
                                }
                            }
                        }
                        else
                        {
                            mainSql.Add(string.Format("delete from {0} where {1}='{2}';", childTable?.table, childTable.tableField, id));
                        }
                    }
                }
            }
        }

        // 处理 开启 并发锁定
        await OptimisticLocking(link, templateInfo, mainSql, allDataMap);

        return mainSql;
    }

    #endregion

    #region 流程表单模块

    /// <summary>
    /// 添加、修改 流程表单数据.
    /// </summary>
    /// <param name="fEntity">表单模板.</param>
    /// <param name="formData">表单数据json.</param>
    /// <param name="dataId">主键Id.</param>
    /// <param name="flowId">flowId.</param>
    /// <param name="isUpdate">是否修改.</param>
    /// <returns></returns>
    public async Task SaveFlowFormData(FlowFormEntity fEntity, string formData, string dataId, string flowId, bool isUpdate = false)
    {
        if (fEntity != null)
        {
            // 自定义表单
            if (fEntity.FormType.Equals(2))
            {
                var vEntity = new VisualDevEntity() { FormData = fEntity.PropertyJson, Tables = fEntity.TableJson, WebType = 2, FullName = fEntity.FullName, FlowId = fEntity.FlowId, EnableFlow = 1 };
                var tInfo = new TemplateParsingBase(vEntity, true);
                tInfo.DbLink = await GetDbLink(fEntity.DbLinkId);
                var dic = formData.ToObject<Dictionary<string, object>>();
                dic["flowId"] = flowId;
                formData = dic.ToJsonString();
                if (isUpdate)
                {
                    var sqlList = await GetUpdateSqlByTemplate(tInfo, new VisualDevModelDataUpInput() { data = formData }, dataId);
                    foreach (var item in sqlList) await _databaseService.ExecuteSql(tInfo.DbLink, item); // 修改功能数据
                }
                else
                {
                    var sqlList = await GetCreateSqlByTemplate(tInfo, new VisualDevModelDataUpInput() { data = formData }, dataId);

                    // 主表自增长Id.
                    if (sqlList.ContainsKey("MainTableReturnIdentity")) sqlList.Remove("MainTableReturnIdentity");
                    foreach (var item in sqlList) await _databaseService.ExecuteSql(tInfo.DbLink, item.Key, item.Value); // 新增功能数据
                }
            }
            else if (fEntity.FormType.Equals(1))
            {
                // 新增,修改
                var dic = formData.ToObject<Dictionary<string, object>>();
                dic["flowId"] = flowId;
                var dicHerader = new Dictionary<string, object>();
                //dicHerader.Add("Poxiao_API", true);
                if (_userManager.ToKen != null && !_userManager.ToKen.Contains("::"))
                    dicHerader.Add("Authorization", _userManager.ToKen);

                // 本地url地址
                // var localAddress = App.Configuration["Kestrel:Endpoints:Http:Url"];
                var localAddress = GetLocalAddress();

                // 请求地址拼接
                if (fEntity.InterfaceUrl.First().Equals('/')) fEntity.InterfaceUrl = fEntity.InterfaceUrl.Substring(1, fEntity.InterfaceUrl.Length - 1);
                var path = string.Format("{0}/{1}/{2}", localAddress, fEntity.InterfaceUrl, dataId);

                var result = new RESTfulResult<object>();
                try
                {
                    result = (await path.SetJsonSerialization<NewtonsoftJsonSerializerProvider>().SetContentType("application/json").SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync()).ToObject<RESTfulResult<object>>();
                }
                catch (Exception ex)
                {
                    throw Oops.Oh(ErrorCode.IO0005);
                }

                if (!result.code.Equals(StatusCodes.Status200OK)) throw Oops.Oh(result.msg);
            }
        }
    }

    /// <summary>
    /// 获取流程表单数据解析详情.
    /// </summary>
    /// <param name="fId">表单模板id.</param>
    /// <param name="dataId">主键Id.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetFlowFormDataDetails(string fId, string dataId)
    {
        var fEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(x => x.Id.Equals(fId));
        if (fEntity == null) return new Dictionary<string, object>();
        var vEntity = new VisualDevEntity() { FormData = fEntity.PropertyJson, Tables = fEntity.TableJson, FlowId = fEntity.FlowId, EnableFlow = 1, WebType = 3, FullName = fEntity.FullName, DbLinkId = fEntity.DbLinkId };
        if (fEntity.FormType.Equals(1))
        {
            var res = new Dictionary<string, object>();
            // 获取详情
            var dicHerader = new Dictionary<string, object>();
            dicHerader.Add("Poxiao_API", true);
            if (_userManager.ToKen != null && !_userManager.ToKen.Contains("::"))
                dicHerader.Add("Authorization", _userManager.ToKen);

            // 本地url地址
            // var localAddress = App.Configuration["Kestrel:Endpoints:Http:Url"];
            var localAddress = GetLocalAddress();

            // 请求地址拼接
            if (fEntity.InterfaceUrl.First().Equals('/')) fEntity.InterfaceUrl = fEntity.InterfaceUrl.Substring(1, fEntity.InterfaceUrl.Length - 1);
            var path = string.Format("{0}/{1}/{2}", localAddress, fEntity.InterfaceUrl, dataId);
            try
            {
                var dataStr = await path.SetHeaders(dicHerader).GetAsStringAsync();
                return dataStr.ToObject<Dictionary<string, object>>();
            }
            catch (Exception)
            {
                throw Oops.Oh(ErrorCode.IO0005);
            }
            return res;
        }
        else
        {
            return (await GetHaveTableInfo(dataId, vEntity));
        }
    }

    /// <summary>
    /// 流程表单数据传递.
    /// </summary>
    /// <param name="oldFId">旧表单模板Id.</param>
    /// <param name="newFId">传递表单模板Id.</param>
    /// <param name="mapRule">映射规则字段 : Key 原字段, Value 映射字段.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="isSubFlow">是否子流程.</param>
    public async Task<Dictionary<string, object>> SaveDataToDataByFId(string oldFId, string newFId, List<Dictionary<string, string>> mapRule, Dictionary<string, object> formData, bool isSubFlow = false)
    {
        // if (oldFId.Equals(newFId)) return formData; // 新旧一致.
        var oldFEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(x => x.Id.Equals(oldFId));
        var newFEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(x => x.Id.Equals(newFId));
        if (oldFEntity == null || newFEntity == null) throw Oops.Oh(ErrorCode.WF0039); // 未找到流程表单模板
        var oldTInfo = new TemplateParsingBase(oldFEntity.PropertyJson, oldFEntity.TableJson, (int)oldFEntity.FormType); // 旧模板
        var newTInfo = new TemplateParsingBase(newFEntity.PropertyJson, newFEntity.TableJson, (int)newFEntity.FormType); // 新模板

        if (oldFEntity.FormType.Equals(1) || newFEntity.FormType.Equals(1))
        {
            oldTInfo.AllFieldsModel.ForEach(it =>
            {
                if (!it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYTIME) && !it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYUSER) && !it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
                    it.__config__.poxiaoKey = PoxiaoKeyConst.COMINPUT;
            });
            newTInfo.AllFieldsModel.ForEach(it =>
            {
                if (!it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYTIME) && !it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYUSER) && !it.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
                    it.__config__.poxiaoKey = PoxiaoKeyConst.COMINPUT;
            });
        }

        var newMapRule = new Dictionary<string, string>();
        oldTInfo.AllFieldsModel.ForEach(item =>
        {
            if (newTInfo.AllFieldsModel.Any(x => x.__vModel__.Equals(item.__vModel__) && x.multiple.Equals(item.multiple))
            && item.__vModel__.IsNotEmptyOrNull()
            && !newMapRule.ContainsKey(item.__vModel__))
                newMapRule.Add(item.__vModel__, item.__vModel__);
        });
        if (mapRule.IsNullOrEmpty()) mapRule = new List<Dictionary<string, string>>();
        foreach (var item in newMapRule)
        {
            if (!mapRule.Any(x => x.Any(xx => xx.Value.Equals(item.Value)) || x.Any(xx => xx.Key.Equals(item.Key)))) mapRule.Add(new Dictionary<string, string>() { { item.Key, item.Value } });
        }

        var childTableSplitKey = "tablefield";

        // 三个特殊的系统表单 (请假申请、销售订单、订单示例)
        if (oldFEntity.EnCode.Equals("leaveApply") || oldFEntity.EnCode.Equals("salesOrder") || oldFEntity.EnCode.Equals("crmOrder") ||
            newFEntity.EnCode.Equals("leaveApply") || newFEntity.EnCode.Equals("salesOrder") || newFEntity.EnCode.Equals("crmOrder"))
        {
            childTableSplitKey = "-";
        }

        foreach (var items in mapRule)
        {
            var item = items.First();
            var oldModel = oldTInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item.Key));
            var newModel = newTInfo.AllFieldsModel.Find(x => x.__vModel__.Equals(item.Value));
            if (oldModel == null || newModel == null || oldModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYTIME) || oldModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.MODIFYUSER))
            {
                formData[item.Key] = string.Empty; // 找不到 默认赋予 空字符串
                continue;
            }
            if (oldModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) || newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
                continue;

            // 子表字段 - 子表字段
            if ((oldModel.__vModel__.ToLower().Contains(childTableSplitKey) && newModel.__vModel__.ToLower().Contains(childTableSplitKey)))
            {
                if (DataTransferVerify(oldModel, newModel))
                {
                    var oldCTable = oldModel.__vModel__.Split("-").First();
                    var oldCField = oldModel.__vModel__.Split("-").Last();
                    var newCTable = newModel.__vModel__.Split("-").First();
                    var newCField = newModel.__vModel__.Split("-").Last();
                    if (oldCField.IsNullOrWhiteSpace() || newCField.IsNullOrWhiteSpace()) continue;

                    if (!formData.ContainsKey(newCTable)) formData.Add(newCTable, new List<Dictionary<string, object>>());
                    if (formData.ContainsKey(oldCTable) && formData[oldCTable] != null && formData[oldCTable].ToString() != "[]")
                    {
                        var oldCTData = formData[oldCTable].ToObject<List<Dictionary<string, object>>>();
                        var newCTData = formData.ContainsKey(newCTable) ? formData[newCTable].ToObject<List<Dictionary<string, object>>>() : new List<Dictionary<string, object>>();

                        for (var i = 0; i < oldCTData.Count; i++)
                        {
                            if (oldCTData[i].ContainsKey(oldCField))
                            {
                                if (newCTData.Count > i) newCTData[i][newCField] = oldCTData[i][oldCField];
                                else newCTData.Add(new Dictionary<string, object>() { { newCField, oldCTData[i][oldCField] } });
                            }
                        }
                        formData[newCTable] = newCTData;
                    }
                }
            }
            else if (oldModel.__vModel__.ToLower().Contains(childTableSplitKey) || newModel.__vModel__.ToLower().Contains(childTableSplitKey))
            {
                if (DataTransferVerify(oldModel, newModel))
                {
                    // 子表字段 - 非子表字段
                    // 传递规则：默认选用上节点的第一条子表数据赋值到下节点的非子表字段内
                    if (oldModel.__vModel__.ToLower().Contains(childTableSplitKey) && !newModel.__vModel__.ToLower().Contains(childTableSplitKey))
                    {
                        var childTable = oldModel.__vModel__.Split("-").First();
                        var childField = oldModel.__vModel__.Split("-").Last();
                        var childTableData = formData[childTable].ToObject<List<Dictionary<string, object>>>();
                        if (childTableData.Any() && childTableData.Any(x => x.ContainsKey(childField)))
                        {
                            if (formData.ContainsKey(oldModel.__vModel__)) formData[oldModel.__vModel__] = childTableData.First()[childField];
                            else formData.Add(oldModel.__vModel__, childTableData.First()[childField]);
                        }
                    }

                    // 非子表字段 - 子表字段
                    // 传递规则：下节点子表新增一行将上节点字段赋值进去
                    if (!oldModel.__vModel__.ToLower().Contains(childTableSplitKey) && newModel.__vModel__.ToLower().Contains(childTableSplitKey))
                    {
                        var childKey = newModel.__vModel__.Split("-");
                        var childTableKey = childKey.First();

                        var childFieldKey = childKey.Last();
                        if (formData.ContainsKey(oldModel.__vModel__))
                        {
                            var childFieldValue = formData[oldModel.__vModel__];

                            if (!formData.ContainsKey(childTableKey)) formData.Add(childTableKey, new List<Dictionary<string, object>>());

                            var childItems = formData[childTableKey].ToObject<List<Dictionary<string, object>>>();
                            if (!childItems.Any())
                            {
                                childItems.Add(new Dictionary<string, object>() { { childFieldKey, childFieldValue } });
                            }
                            else
                            {
                                if (childItems.Any(x => x.ContainsKey(childFieldKey))) childItems.First()[childFieldKey] = childFieldValue;
                                else childItems.First().Add(childFieldKey, childFieldValue);
                            }

                            formData[childTableKey] = childItems;
                        }
                    }
                }
            }
            else
            {
                // 三个特殊的系统表单，不做验证规则
                if (!childTableSplitKey.Equals("-") && !DataTransferVerify(oldModel, newModel)) formData[oldModel.__vModel__] = null;
            }
        }

        var res = new Dictionary<string, object>();
        foreach (var dicItems in mapRule)
        {
            var dicItem = dicItems.First();
            if (formData.ContainsKey(dicItem.Key) && dicItem.Value.IsNotEmptyOrNull())
            {
                var itemValue = formData.First(x => x.Key.Equals(dicItem.Key)).Value;
                if (!res.ContainsKey(dicItem.Value)) res.Add(dicItem.Value, itemValue);
            }
            if (dicItem.Value.ToLower().Contains(childTableSplitKey))
            {
                var cTableKey = dicItem.Value.Split("-").First();
                var itemValue = formData.First(x => x.Key.Equals(cTableKey));
                if (!res.ContainsKey(cTableKey)) res.Add(itemValue.Key, itemValue.Value);
            }
        }

        // 系统表单 直接请求接口.
        if (newFEntity.FormType.Equals(1))
        {
            if (!isSubFlow)
            {
                // 新增,修改
                var dic = formData.ToObject<Dictionary<string, object>>();
                var dicHerader = new Dictionary<string, object>();
                dicHerader.Add("Poxiao_API", true);
                if (_userManager.ToKen != null && !_userManager.ToKen.Contains("::"))
                    dicHerader.Add("Authorization", _userManager.ToKen);

                // 本地url地址
                // var localAddress = App.Configuration["Kestrel:Endpoints:Http:Url"];
                var localAddress = GetLocalAddress();

                // 请求地址拼接
                if (newFEntity.InterfaceUrl.First().Equals('/')) newFEntity.InterfaceUrl = newFEntity.InterfaceUrl.Substring(1, newFEntity.InterfaceUrl.Length - 1);
                var path = string.Format("{0}/{1}/{2}", localAddress, newFEntity.InterfaceUrl, formData["id"].ToString());
                try
                {
                    await path.SetJsonSerialization<NewtonsoftJsonSerializerProvider>().SetContentType("application/json").SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync();
                }
                catch (Exception ex)
                {
                }
            }
            res["id"] = formData["id"];
            return res;
        }

        // 获取请求端类型，并对应获取 数据权限
        DbLinkEntity link = await GetDbLink(newFEntity.DbLinkId);
        newTInfo.DbLink = link;
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, newTInfo.MainTableName); // 获取主表 表结构 信息
        newTInfo.MainPrimary = tableList.Find(t => t.primaryKey).field;
        if (!tableList.Any(x => SqlFunc.ToLower(x.field) == "f_flowtaskid"))
        {
            List<DbTableFieldModel>? pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "F_FlowTaskId", fieldName = "流程Id", dataType = "varchar", dataLength = "50", allowNull = 1 } };
            _databaseService.AddTableColumn(link, newTInfo.MainTableName, pFieldList);
        }

        var sqlFormat = "select {0},{1} from {2} where F_FlowTaskId='{3}';";
        if (newTInfo.FormModel.primaryKeyPolicy.Equals(2)) sqlFormat = "select {0},{1} from {2} where {1}={3};";
        var isUpdate = false;

        // 处理数据传递 乐观锁 场景.
        if (newTInfo.FormModel.concurrencyLock)
        {
            var sql = string.Format(sqlFormat, "F_Version", newTInfo.MainPrimary, newTInfo.MainTableName, formData["id"].ToString());
            var querData = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, string>>>();
            if (querData.Any() && querData.Any(x => x.ContainsKey("F_Version") || x.ContainsKey("F_VERSION")))
            {
                res.Add("f_version", querData.FirstOrDefault(x => x.Any(x => x.Key.Equals("F_Version") || x.Key.Equals("F_VERSION")))?.FirstOrDefault().Value);
                isUpdate = true; // 修改
            }
            else
            {
                isUpdate = false; // 新增
            }
        }
        else
        {
            var sql = string.Format(sqlFormat, newTInfo.MainPrimary, newTInfo.MainPrimary, newTInfo.MainTableName, formData["id"].ToString());
            var querData = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, string>>>();

            if (querData.Any() && querData.Any(x => x.ContainsKey(newTInfo.MainPrimary))) isUpdate = true; // 修改
            else isUpdate = false; // 新增
        }

        if (mapRule.Any(x => x.ContainsKey("@prevNodeFormId")))
        {
            var key = mapRule.Find(x => x.ContainsKey("@prevNodeFormId")).First().Value;
            if (key.ToLower().Contains("tablefield"))
            {
                var ctKey = key.Split('-');
                var ctValues = res[ctKey.FirstOrDefault()].ToObject<List<Dictionary<string, object>>>();
                ctValues.ForEach(item => item[ctKey.LastOrDefault()] = formData["id"]);
                res[ctKey.FirstOrDefault()] = ctValues;
            }
            res[mapRule.Find(x => x.ContainsKey("@prevNodeFormId")).First().Value] = formData["id"];
        }
		
        // 保存到数据库
        res["id"] = formData["id"];

        if (newTInfo.ChildTableFieldsModelList.Any())
        {
            var tInfoList = new List<string>();
            newTInfo.ChildTableFieldsModelList.ForEach(x =>
            {
                var newValueMapRule = mapRule.Select(xx => xx.FirstOrDefault().Value).ToList();
                if (!res.ContainsKey(x.__vModel__) && (!newValueMapRule.Contains(x.__vModel__))) tInfoList.Add(x.__vModel__);
            });
            if (tInfoList.Any())
            {
                var vEntity = new VisualDevEntity() { FormData = newFEntity.PropertyJson, Tables = newFEntity.TableJson, FlowId = newFEntity.FlowId, EnableFlow = 1, WebType = 3, FullName = newFEntity.FullName, DbLinkId = newFEntity.DbLinkId };
                var nDataInfo = await GetHaveTableInfo(res["id"].ToString(), vEntity);
                if (nDataInfo != null) tInfoList.ForEach(ctDataItem => { if (nDataInfo.ContainsKey(ctDataItem)) res[ctDataItem] = nDataInfo[ctDataItem]; });
            }
        }

        if (!isSubFlow)
        {
            await SaveFlowFormData(newFEntity, res.ToJsonString(), formData["id"].ToString(), formData["flowId"].ToString(), isUpdate);
        }

        return res;
    }

    private string GetLocalAddress()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var server = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var addressesFeature = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
        var addresses = addressesFeature?.Addresses;
        return addresses.FirstOrDefault().Replace("[::]", "localhost");
    }
    #endregion

    #region 公用方法

    /// <summary>
    /// 删除有表信息.
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="templateEntity">模板实体</param>
    /// <returns></returns>
    public async Task DelHaveTableInfo(string id, VisualDevEntity templateEntity)
    {
        if (templateEntity.EnableFlow == 1)
        {
            var flowTask = await _visualDevRepository.AsSugarClient().Queryable<FlowTaskEntity>().Where(f => f.Id.Equals(id) && f.Status != 4).FirstAsync();
            if (flowTask != null)
            {
                if (flowTask.ParentId != "0") throw Oops.Oh(ErrorCode.WF0003, flowTask.FullName);
                else throw Oops.Oh(ErrorCode.D1417);
            }
        }

        if (id.IsNotEmptyOrNull())
        {
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
            templateInfo.DbLink = link;
            string? mainPrimary = GetPrimary(link, templateInfo.MainTableName);

            // 树形表格 删除父节点时同时删除子节点数据
            if (templateInfo.ColumnData.type.Equals(5))
            {
                var delIdDic = new Dictionary<string, string>();
                var dataList = _databaseService.GetData(link, templateInfo.MainTableName).ToObject<List<Dictionary<string, string>>>();
                dataList.ForEach(item => delIdDic.Add(item[mainPrimary], item[templateInfo.ColumnData.parentField]));
                var delIds = new List<string>();
                CodeGenHelper.GetChildIdList(delIdDic, id, delIds);
                await BatchDelHaveTableData(delIds.Distinct().ToList(), templateEntity);
            }
            else
            {
                var resId = GetPIdsByFlowIds(link, templateInfo, mainPrimary, new List<string>() { id });
                id = resId.First().Value;
                if (templateInfo.FormModel.logicalDelete)
                {
                    await _databaseService.ExecuteSql(link, string.Format("update {0} set f_deletemark=1 where {1}='{2}'", templateInfo.MainTableName, mainPrimary, id)); // 删除标识
                    if (templateEntity.EnableFlow == 1)
                    {
                        FlowTaskEntity? entity = _flowTaskRepository.GetTaskFirstOrDefault(resId.First().Key);
                        if (entity != null)
                        {
                            if (!entity.ParentId.Equals("0")) throw Oops.Oh(ErrorCode.WF0003, entity.FullName);
                            await _flowTaskRepository.DeleteTask(entity);
                        }
                    }
                }
                else
                {
                    List<string>? allDelSql = new List<string>(); // 拼接语句
                    allDelSql.Add(string.Format("delete from {0} where {1} = '{2}';", templateInfo.MainTable.table, mainPrimary, id));
                    if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
                    {
                        templateInfo.AllTable.Where(x => x.typeId.Equals("0")).ToList()
                            .ForEach(item => allDelSql.Add(string.Format("delete from {0} where {1}='{2}';", item.table, item.tableField, id))); // 删除所有涉及表数据 sql
                    }

                    foreach (string? item in allDelSql) await _databaseService.ExecuteSql(link, item); // 删除有表数据

                    if (templateEntity.EnableFlow == 1)
                    {
                        FlowTaskEntity? entity = _flowTaskRepository.GetTaskFirstOrDefault(resId.First().Key);
                        if (entity != null)
                        {
                            if (!entity.ParentId.Equals("0")) throw Oops.Oh(ErrorCode.WF0003, entity.FullName);
                            await _flowTaskRepository.DeleteTask(entity);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 批量删除有表数据.
    /// </summary>
    /// <param name="ids">id数组</param>
    /// <param name="templateEntity">模板实体</param>
    /// <returns></returns>
    public async Task BatchDelHaveTableData(List<string> ids, VisualDevEntity templateEntity)
    {
        List<string>? idList = ids.Copy();
        if (templateEntity.EnableFlow == 1)
        {
            var fList = await _visualDevRepository.AsSugarClient().Queryable<FlowTaskEntity>().Where(f => ids.Contains(f.Id) && f.Status != 4).ToListAsync();
            if (fList.Any(x => x.ParentId != "0") && fList.Count(x => x.ParentId != "0").Equals(ids.Count)) throw Oops.Oh(ErrorCode.WF0003, fList.First(x => x.ParentId != "0").FullName);
            if (fList.Count.Equals(ids.Count)) throw Oops.Oh(ErrorCode.D1417);
            else ids = ids.Except(fList.Select(x => x.Id)).ToList();
        }

        if (ids.Count > 0)
        {
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            DbLinkEntity link = await GetDbLink(templateEntity.DbLinkId);
            string? mainPrimary = GetPrimary(link, templateInfo.MainTableName);
            var resIds = GetPIdsByFlowIds(link, templateInfo, mainPrimary, ids);
            ids = resIds.Select(x => x.Value).ToList();

            if (templateInfo.FormModel.logicalDelete)
            {
                await _databaseService.ExecuteSql(link, string.Format("update {0} set f_deletemark=1 where {1} in ('{2}')", templateInfo.MainTableName, mainPrimary, string.Join("','", ids))); // 删除标识
                if (templateEntity.EnableFlow == 1)
                {
                    ids = resIds.Select(x => x.Key).ToList();
                    foreach (var it in ids)
                    {
                        FlowTaskEntity? entity = _flowTaskRepository.GetTaskFirstOrDefault(it);
                        if (entity != null && entity.ParentId.Equals("0"))
                        {
                            if (!entity.ParentId.Equals("0")) throw Oops.Oh(ErrorCode.WF0003, entity.FullName);
                            _flowTaskRepository.DeleteTaskNoAwait(entity);
                        }
                    }
                }
            }
            else
            {
                List<string>? allDelSql = new List<string>(); // 拼接语句
                allDelSql.Add(string.Format("delete from {0} where {1} in ('{2}');", templateInfo.MainTable.table, mainPrimary, string.Join("','", ids))); // 主表数据
                if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
                {
                    templateInfo.AllTable.Where(x => x.typeId.Equals("0")).ToList()
                        .ForEach(item => allDelSql.Add(string.Format("delete from {0} where {1} in ('{2}');", item.table, item.tableField, string.Join("','", ids))));
                }

                try
                {
                    foreach (string? item in allDelSql) await _databaseService.ExecuteSql(link, item); // 删除有表数据

                    if (templateEntity.EnableFlow == 1)
                    {
                        ids = resIds.Select(x => x.Key).ToList();
                        foreach (var it in ids)
                        {
                            FlowTaskEntity? entity = _flowTaskRepository.GetTaskFirstOrDefault(it);
                            if (entity != null && entity.ParentId.Equals("0"))
                            {
                                if (!entity.ParentId.Equals("0")) throw Oops.Oh(ErrorCode.WF0003, entity.FullName);
                                _flowTaskRepository.DeleteTaskNoAwait(entity);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        if (templateEntity.EnableFlow == 1 && ids.Count < 1 && !idList.Count.Equals(ids.Count)) throw Oops.Oh(ErrorCode.D1417);
    }

    /// <summary>
    /// 生成系统自动生成字段.
    /// </summary>
    /// <param name="fieldsModelListJson">模板数据.</param>
    /// <param name="allDataMap">真实数据.</param>
    /// <param name="IsCreate">创建与修改标识 true创建 false 修改.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GenerateFeilds(string fieldsModelListJson, Dictionary<string, object> allDataMap, bool IsCreate)
    {
        List<FieldsModel> fieldsModelList = fieldsModelListJson.ToList<FieldsModel>();
        UserEntity? userInfo = _userManager.User;
        int dicCount = allDataMap.Keys.Count;
        string[] strKey = new string[dicCount];

        // 修改时 把 创建用户 和 创建时间 去掉.
        if (!IsCreate)
        {
            fieldsModelList.ForEach(item =>
            {
                switch (item.__config__.poxiaoKey)
                {
                    case PoxiaoKeyConst.CREATETIME:
                    case PoxiaoKeyConst.CREATEUSER:
                    case PoxiaoKeyConst.CURRPOSITION:
                    case PoxiaoKeyConst.CURRORGANIZE:
                        allDataMap.Remove(item.__vModel__);
                        break;
                    case PoxiaoKeyConst.TABLE:
                        var fList = item.__config__.children.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATETIME)
                        || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CREATEUSER)
                        || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CURRPOSITION)
                        || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CURRORGANIZE)).ToList();
                        fList.ForEach(child =>
                        {
                            if (allDataMap.ContainsKey(item.__vModel__))
                            {
                                var cDataMap = allDataMap[item.__vModel__].ToObject<List<Dictionary<string, object>>>();
                                cDataMap.ForEach(x => x.Remove(child.__vModel__));
                                allDataMap[item.__vModel__] = cDataMap;
                            }
                        });
                        break;
                }
            });
        }

        foreach (var model in fieldsModelList)
        {
            if (model != null && model.__vModel__.IsNotEmptyOrNull())
            {
                // 如果模板poxiaoKey为table为子表数据
                if (model.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && allDataMap.ContainsKey(model.__vModel__) && allDataMap[model.__vModel__] != null)
                {
                    List<FieldsModel> childFieldsModelList = model.__config__.children;
                    object? objectData = allDataMap[model.__vModel__];
                    List<Dictionary<string, object>> childAllDataMapList = objectData.ToJsonString().ToObject<List<Dictionary<string, object>>>();
                    if (childAllDataMapList != null && childAllDataMapList.Count > 0)
                    {
                        List<Dictionary<string, object>> newChildAllDataMapList = new List<Dictionary<string, object>>();
                        foreach (Dictionary<string, object>? childmap in childAllDataMapList)
                        {
                            Dictionary<string, object>? newChildData = new Dictionary<string, object>();
                            foreach (KeyValuePair<string, object> item in childmap)
                            {
                                if (item.Key.Equals("id")) newChildData[item.Key] = childmap[item.Key];
                                FieldsModel? childFieldsModel = childFieldsModelList.Where(c => c.__vModel__ == item.Key).FirstOrDefault();
                                if (childFieldsModel != null && childFieldsModel.__vModel__.Equals(item.Key))
                                {
                                    switch (childFieldsModel.__config__.poxiaoKey)
                                    {
                                        case PoxiaoKeyConst.BILLRULE:
                                            if (IsCreate || childmap[item.Key].IsNullOrEmpty())
                                            {
                                                string billNumber = await _billRuleService.GetBillNumber(childFieldsModel.__config__.rule);
                                                if (!"单据规则不存在".Equals(billNumber)) newChildData[item.Key] = billNumber;
                                                else newChildData[item.Key] = string.Empty;
                                            }
                                            else
                                            {
                                                newChildData[item.Key] = childmap[item.Key];
                                            }

                                            break;
                                        case PoxiaoKeyConst.CREATEUSER:
                                            if (IsCreate) newChildData[item.Key] = userInfo.Id;
                                            break;
                                        case PoxiaoKeyConst.MODIFYUSER:
                                            if (!IsCreate) newChildData[item.Key] = userInfo.Id;
                                            break;
                                        case PoxiaoKeyConst.CREATETIME:
                                            if (IsCreate) newChildData[item.Key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                            break;
                                        case PoxiaoKeyConst.MODIFYTIME:
                                            if (!IsCreate) newChildData[item.Key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                            break;
                                        case PoxiaoKeyConst.CURRPOSITION:
                                            if (IsCreate)
                                            {
                                                if (allDataMap.ContainsKey("Poxiao_FlowDelegate_CurrPosition")) // 流程委托 需要指定所属岗位
                                                {
                                                    allDataMap[model.__vModel__] = allDataMap["Poxiao_FlowDelegate_CurrPosition"];
                                                }
                                                else
                                                {
                                                    string? pid = await _visualDevRepository.AsSugarClient().Queryable<UserEntity, PositionEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.PositionId))
                                                        .Where((a, b) => a.Id == userInfo.Id && a.DeleteMark == null).Select((a, b) => a.PositionId).FirstAsync();
                                                    if (pid.IsNotEmptyOrNull()) newChildData[item.Key] = pid;
                                                    else newChildData[item.Key] = string.Empty;
                                                }
                                            }

                                            break;
                                        case PoxiaoKeyConst.CURRORGANIZE:
                                            if (IsCreate)
                                            {
                                                if (allDataMap.ContainsKey("Poxiao_FlowDelegate_CurrOrganize")) // 流程委托 需要指定所属组织
                                                {
                                                    allDataMap[model.__vModel__] = allDataMap["Poxiao_FlowDelegate_CurrOrganize"];
                                                }
                                                else
                                                {
                                                    if (userInfo.OrganizeId != null) newChildData[item.Key] = userInfo.OrganizeId;
                                                    else newChildData[item.Key] = string.Empty;
                                                }
                                            }

                                            break;
                                        case PoxiaoKeyConst.UPLOADFZ: // 文件上传
                                            if (!childmap.ContainsKey(item.Key) || childmap[item.Key].IsNullOrEmpty()) newChildData[item.Key] = new string[] { };
                                            else newChildData[item.Key] = childmap[item.Key];
                                            break;
                                        default:
                                            newChildData[item.Key] = childmap[item.Key];
                                            break;
                                    }
                                }
                            }

                            newChildAllDataMapList.Add(newChildData);
                            allDataMap[model.__vModel__] = newChildAllDataMapList;
                        }
                    }
                }
                else
                {
                    switch (model.__config__.poxiaoKey)
                    {
                        case PoxiaoKeyConst.BILLRULE:
                            if (IsCreate)
                            {
                                string billNumber = await _billRuleService.GetBillNumber(model.__config__.rule);
                                if (!"单据规则不存在".Equals(billNumber)) allDataMap[model.__vModel__] = billNumber;
                                else allDataMap[model.__vModel__] = string.Empty;
                            }
                            break;
                        case PoxiaoKeyConst.CREATEUSER:
                            if (IsCreate)
                            {
                                allDataMap[model.__vModel__] = userInfo.Id;
                            }
                            break;
                        case PoxiaoKeyConst.CREATETIME:
                            if (IsCreate) allDataMap[model.__vModel__] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                            break;
                        case PoxiaoKeyConst.MODIFYUSER:
                            if (!IsCreate) allDataMap[model.__vModel__] = userInfo.Id;
                            break;
                        case PoxiaoKeyConst.MODIFYTIME:
                            if (!IsCreate) allDataMap[model.__vModel__] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                            break;
                        case PoxiaoKeyConst.CURRPOSITION:
                            if (IsCreate)
                            {
                                if (allDataMap.ContainsKey("Poxiao_FlowDelegate_CurrPosition")) // 流程委托 需要指定所属岗位
                                {
                                    allDataMap[model.__vModel__] = allDataMap["Poxiao_FlowDelegate_CurrPosition"];
                                }
                                else
                                {
                                    string? pid = await _visualDevRepository.AsSugarClient().Queryable<UserEntity, PositionEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.PositionId))
                                        .Where((a, b) => a.Id == userInfo.Id && a.DeleteMark == null).Select((a, b) => a.PositionId).FirstAsync();
                                    if (pid.IsNotEmptyOrNull()) allDataMap[model.__vModel__] = pid;
                                    else allDataMap[model.__vModel__] = string.Empty;
                                }
                            }

                            break;
                        case PoxiaoKeyConst.CURRORGANIZE:
                            if (IsCreate)
                            {
                                if (allDataMap.ContainsKey("Poxiao_FlowDelegate_CurrOrganize")) // 流程委托 需要指定所属组织
                                {
                                    allDataMap[model.__vModel__] = allDataMap["Poxiao_FlowDelegate_CurrOrganize"];
                                }
                                else
                                {
                                    if (model.showLevel.Equals("last"))
                                    {
                                        if (userInfo.OrganizeId != null && await _visualDevRepository.AsSugarClient().Queryable<OrganizeEntity>().AnyAsync(x => x.Id.Equals(userInfo.OrganizeId) && x.Category.Equals("department")))
                                            allDataMap[model.__vModel__] = userInfo.OrganizeId;
                                        else allDataMap[model.__vModel__] = string.Empty;
                                    }
                                    else
                                    {
                                        if (userInfo.OrganizeId != null) allDataMap[model.__vModel__] = userInfo.OrganizeId;
                                        else allDataMap[model.__vModel__] = string.Empty;
                                    }
                                }
                            }

                            break;
                        case PoxiaoKeyConst.UPLOADFZ: // 文件上传
                            if (!allDataMap.ContainsKey(model.__vModel__) || allDataMap[model.__vModel__].IsNullOrEmpty()) allDataMap[model.__vModel__] = new string[] { };
                            break;
                    }
                }
            }
        }

        return allDataMap;
    }

    /// <summary>
    /// 获取数据连接, 根据连接Id.
    /// </summary>
    /// <param name="linkId"></param>
    /// <returns></returns>
    public async Task<DbLinkEntity> GetDbLink(string linkId)
    {
        DbLinkEntity link = await _dbLinkService.GetInfo(linkId);
        var defaultConnection = _connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString() == "default");
        if (link == null && _userManager.TenantDbName.IsNullOrWhiteSpace())
        {
            return new DbLinkEntity
            {
                Id = defaultConnection.ConfigId?.ToString() ?? string.Empty,
                ServiceName = defaultConnection.DBName,
                DbType = defaultConnection.DbType.ToString(),
                Host = defaultConnection.Host,
                Port = defaultConnection.Port,
                UserName = defaultConnection.UserName,
                Password = defaultConnection.Password
            };
        }

        if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
        return link;
    }

    /// <summary>
    /// 无限递归 给控件绑定默认值 (绕过 布局控件).
    /// </summary>
    public void FieldBindDefaultValue(ref List<Dictionary<string, object>> dicFieldsModelList, string defaultUserId, string defaultDepId, List<UserRelationEntity> userRelationList)
    {
        foreach (var item in dicFieldsModelList)
        {
            var obj = item["__config__"].ToObject<Dictionary<string, object>>();

            if (obj.ContainsKey("poxiaoKey") && (obj["poxiaoKey"].Equals(PoxiaoKeyConst.USERSELECT) || obj["poxiaoKey"].Equals(PoxiaoKeyConst.DEPSELECT)) && obj["defaultCurrent"].Equals(true))
            {
                switch (obj["poxiaoKey"])
                {
                    case PoxiaoKeyConst.USERSELECT:
                        if (item.ContainsKey("selectType") && item["selectType"].Equals("custom"))
                        {
                            var ableDepIds = item["ableDepIds"].ToObject<List<string>>();
                            if (ableDepIds == null) ableDepIds = new List<string>();
                            var ablePosIds = item["ablePosIds"].ToObject<List<string>>();
                            if (ablePosIds == null) ablePosIds = new List<string>();
                            var ableUserIds = item["ableUserIds"].ToObject<List<string>>();
                            if (ableUserIds == null) ableUserIds = new List<string>();
                            var ableRoleIds = item["ableRoleIds"].ToObject<List<string>>();
                            if (ableRoleIds == null) ableRoleIds = new List<string>();
                            var ableGroupIds = item["ableGroupIds"].ToObject<List<string>>();
                            if (ableGroupIds == null) ableGroupIds = new List<string>();
                            var userIdList = userRelationList.Where(x => ableUserIds.Contains(x.UserId) || ableDepIds.Contains(x.ObjectId)
                                || ablePosIds.Contains(x.ObjectId) || ableRoleIds.Contains(x.ObjectId) || ableGroupIds.Contains(x.ObjectId)).Select(x => x.UserId).ToList();
                            if (!userIdList.Contains(defaultUserId))
                            {
                                obj["defaultValue"] = null;
                                break;
                            }
                        }
                        if (item.ContainsKey("multiple") && item["multiple"].Equals(true)) obj["defaultValue"] = new List<string>() { defaultUserId };
                        else obj["defaultValue"] = defaultUserId;
                        break;
                    case PoxiaoKeyConst.DEPSELECT:
                        if (item.ContainsKey("selectType") && item["selectType"].Equals("custom"))
                        {
                            var defValue = item["ableDepIds"].ToObject<List<string>>();
                            if (!defValue.Contains(defaultDepId))
                            {
                                obj["defaultValue"] = null;
                                break;
                            }
                        }
                        if (item.ContainsKey("multiple") && item["multiple"].Equals(true)) obj["defaultValue"] = new List<string>() { defaultDepId };
                        else obj["defaultValue"] = defaultDepId;
                        break;
                }
            }

            // 子表控件
            if (obj.ContainsKey("poxiaoKey") && obj["poxiaoKey"].Equals(PoxiaoKeyConst.TABLE))
            {
                var cList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                foreach (var child in cList)
                {
                    var cObj = child["__config__"].ToObject<Dictionary<string, object>>();
                    if (cObj.ContainsKey("poxiaoKey") && (cObj["poxiaoKey"].Equals(PoxiaoKeyConst.USERSELECT) || cObj["poxiaoKey"].Equals(PoxiaoKeyConst.DEPSELECT)) && cObj["defaultCurrent"].Equals(true))
                    {
                        switch (cObj["poxiaoKey"])
                        {
                            case PoxiaoKeyConst.USERSELECT:
                                if (item.ContainsKey("multiple") && item["multiple"].Equals(true)) cObj["defaultValue"] = new List<string>() { defaultUserId };
                                else obj["defaultValue"] = defaultUserId;
                                break;
                            case PoxiaoKeyConst.DEPSELECT:
                                if (item.ContainsKey("multiple") && item["multiple"].Equals(true)) cObj["defaultValue"] = new List<string>() { defaultDepId };
                                else obj["defaultValue"] = defaultDepId;
                                break;
                        }
                    }

                    child["__config__"] = cObj;
                }

                obj["children"] = cList;
            }

            // 递归布局控件
            if (obj.ContainsKey("children"))
            {
                var fmList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                FieldBindDefaultValue(ref fmList, defaultUserId, defaultDepId, userRelationList);
                obj["children"] = fmList;
            }

            item["__config__"] = obj;
        }
    }

    /// <summary>
    /// 处理模板默认值 (针对流程表单).
    /// 用户选择 , 部门选择.
    /// </summary>
    /// <param name="propertyJson">表单json.</param>
    /// <param name="tableJson">关联表单.</param>
    /// <param name="formType">表单类型（1：系统表单 2：自定义表单）.</param>
    /// <returns></returns>
    public string GetVisualDevModelDataConfig(string propertyJson, string tableJson, int formType)
    {
        var tInfo = new TemplateParsingBase(propertyJson, tableJson, formType);
        if (tInfo.AllFieldsModel.Any(x => (x.__config__.defaultCurrent) && (x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.DEPSELECT))))
        {
            var userId = _userManager.UserId;
            var depId = _visualDevRepository.AsSugarClient().Queryable<UserEntity, OrganizeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.OrganizeId))
                .Where((a, b) => a.Id.Equals(_userManager.UserId) && b.Category.Equals("department")).Select((a, b) => a.OrganizeId).First();
            var allUserRelationList = _visualDevRepository.AsSugarClient().Queryable<UserRelationEntity>().Select(x => new UserRelationEntity() { UserId = x.UserId, ObjectId = x.ObjectId }).ToList();

            var configData = propertyJson.ToObject<Dictionary<string, object>>();
            var columnList = configData["fields"].ToObject<List<Dictionary<string, object>>>();
            FieldBindDefaultValue(ref columnList, userId, depId, allUserRelationList);
            configData["fields"] = columnList;
            propertyJson = configData.ToJsonString();
        }

        return propertyJson;
    }

    /// <summary>
    /// 同步业务需要的字段.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task SyncField(TemplateParsingBase tInfo)
    {
        if (tInfo.IsHasTable && !tInfo.visualDevEntity.WebType.Equals(4))
        {
            // 是否开启软删除配置 , 开启则增加 删除标识 字段.
            if (tInfo.FormModel.logicalDelete)
            {
                if (!_databaseService.IsAnyColumn(tInfo.DbLink, tInfo.MainTableName, "f_deletemark"))
                {
                    var pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "f_deletemark", fieldName = "删除标识", dataType = "int", dataLength = "50", allowNull = 1 } };
                    _databaseService.AddTableColumn(tInfo.DbLink, tInfo.MainTableName, pFieldList);
                }
            }

            // 是否开启多租户 字段隔离, 开启则增加 隔离 字段.
            if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                if (!_databaseService.IsAnyColumn(tInfo.DbLink, tInfo.MainTableName, "f_tenantid"))
                {
                    var pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "f_tenantid", fieldName = "租户Id", dataType = "varchar", dataLength = "50", allowNull = 1 } };
                    _databaseService.AddTableColumn(tInfo.DbLink, tInfo.MainTableName, pFieldList);
                }
            }

            if (tInfo.visualDevEntity.EnableFlow.Equals(1))
            {
                // 流程表单 需要增加字段 F_FlowTaskId
                List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(tInfo.DbLink, tInfo.MainTableName); // 获取主表 表结构 信息
                if (!tableList.Any(x => SqlFunc.ToLower(x.field) == "f_flowtaskid"))
                {
                    List<DbTableFieldModel>? pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "F_FlowTaskId", fieldName = "流程任务Id", dataType = "varchar", dataLength = "50", allowNull = 1 } };
                    _databaseService.AddTableColumn(tInfo.DbLink, tInfo.MainTableName, pFieldList);
                }
                if (!tableList.Any(x => SqlFunc.ToLower(x.field) == "f_flowid"))
                {
                    var pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "F_FlowId", fieldName = "流程引擎Id", dataType = "varchar", dataLength = "50", allowNull = 1 } };
                    _databaseService.AddTableColumn(tInfo.DbLink, tInfo.MainTableName, pFieldList);
                }

                var ffEntity = _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().First(x => x.Id.Equals(tInfo.visualDevEntity.Id));
                if (ffEntity != null)
                {
                    var flowId = ffEntity.FlowId;
                    var flowJsonId = (await _visualDevRepository.AsSugarClient().Queryable<FlowTemplateJsonEntity>().Where(x => x.TemplateId == flowId && x.EnabledMark == 1 && x.DeleteMark == null).Select(x => x.Id).FirstAsync());
                    var sql = string.Format("update {0} set f_flowtaskid={1},f_flowid='{2}' where f_flowid is null or f_flowid = '';", tInfo.MainTableName, tableList.First(x => x.primaryKey).field, flowJsonId);
                    await _databaseService.ExecuteSql(tInfo.DbLink, sql);
                }
            }
        }
    }
    #endregion

    #region 私有方法

    /// <summary>
    /// 获取数据表主键.
    /// </summary>
    /// <param name="link"></param>
    /// <param name="MainTableName"></param>
    /// <returns></returns>
    private string GetPrimary(DbLinkEntity link, string MainTableName)
    {
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, MainTableName); // 获取主表所有列
        DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
        if (mainPrimary == null || mainPrimary.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D1402); // 主表未设置主键
        return mainPrimary.field;
    }

    /// <summary>
    /// 根据流程Id 获取 主键 Id.
    /// </summary>
    /// <param name="link">数据库连接.</param>
    /// <param name="templateInfo">模板配置.</param>
    /// <param name="mainPrimary">主表主键名.</param>
    /// <param name="Ids">流程Ids.</param>
    /// <param name="isList">是否列表.</param>
    /// <param name="currIndex">.</param>
    /// <returns>F_FlowTaskId, mainPrimary.</returns>
    private Dictionary<string, string> GetPIdsByFlowIds(DbLinkEntity link, TemplateParsingBase templateInfo, string mainPrimary, List<string> Ids, bool isList = false, int currIndex = 0)
    {
        Dictionary<string, string> res = new Dictionary<string, string>();
        if (templateInfo.visualDevEntity != null && templateInfo.visualDevEntity.EnableFlow.Equals(1) && templateInfo.FormModel.primaryKeyPolicy.Equals(2) && currIndex < 3)
        {
            var sql = string.Format("select {0},F_FlowTaskId from {1} where F_FlowTaskId in ('{2}');", mainPrimary, templateInfo.MainTableName, string.Join("','", Ids));
            if (isList) sql = string.Format("select {0},F_FlowTaskId from {1} where {0} in ('{2}');", mainPrimary, templateInfo.MainTableName, string.Join("','", Ids));
            var data = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, string>>>();
            currIndex++;
            if (!data.Any()) return GetPIdsByFlowIds(link, templateInfo, mainPrimary, Ids, true, currIndex);
            data.ForEach(item => res.Add(item["F_FlowTaskId"], item[mainPrimary]));
        }
        else
        {
            Ids.ForEach(item => res.Add(item, item));
        }

        return res;
    }

    /// <summary>
    /// 获取允许删除任务列表.
    /// </summary>
    /// <param name="ids">id数组.</param>
    /// <returns></returns>
    private async Task<List<string>> GetAllowDeleteFlowTaskList(List<string> ids)
    {
        List<string>? idList = await _visualDevRepository.AsSugarClient().Queryable<FlowTaskEntity>().Where(f => ids.Contains(f.Id) && f.Status != 4).Select(f => f.Id).ToListAsync();

        return ids.Except(idList).ToList();
    }

    /// <summary>
    /// 组装高级查询信息.
    /// </summary>
    /// <param name="superQueryJson">查询条件json.</param>
    private string GetSuperQueryInput(string superQueryJson)
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        Dictionary<string, string>? dic = string.IsNullOrEmpty(superQueryJson) ? null : superQueryJson.ToObject<Dictionary<string, string>>();

        if (dic != null)
        {
            var matchLogic = dic.FirstOrDefault().Value;
            var whereType = matchLogic.Equals("AND") ? WhereType.And : WhereType.Or;
            var queryList = new List<Dictionary<string, object>>();

            foreach (var item in dic.LastOrDefault().Value.ToObject<List<Dictionary<string, object>>>())
            {
                var query = new Dictionary<string, object>();
                query.Add("whereType", whereType);
                query.Add("poxiaoKey", item["poxiaoKey"]);
                query.Add("field", item["field"].ToString());
                string itemValue = string.Empty;
                if (item.ContainsKey("fieldValue") && item["fieldValue"].IsNotEmptyOrNull())
                    itemValue = item["fieldValue"].ToString().Replace("\r\n", string.Empty).Replace(" ", string.Empty);

                switch (item["poxiaoKey"])
                {
                    case PoxiaoKeyConst.USERSSELECT:
                        {
                            if (item["fieldValue"] != null && (item["symbol"].Equals("like") || item["symbol"].Equals("notLike")))
                            {
                                var rIdList = _visualDevRepository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => x.UserId.Equals(item["fieldValue"].ToString().Replace("--user", string.Empty))).Select(x => new { x.ObjectId, x.ObjectType }).ToList();
                                var objIdList = new List<string>() { item["fieldValue"].ToString() };
                                rIdList.ForEach(x =>
                                {
                                    if (x.ObjectType.Equals("Organize"))
                                    {
                                        objIdList.Add(x.ObjectId + "--company");
                                        objIdList.Add(x.ObjectId + "--department");
                                    }
                                    else
                                    {
                                        objIdList.Add(x.ObjectId + "--" + x.ObjectType.ToLower());
                                    }
                                });

                                var whereList = new List<KeyValuePair<WhereType, ConditionalModel>>();
                                for (var i = 0; i < objIdList.Count(); i++)
                                {
                                    if (i == 0)
                                    {
                                        var queryOr = new Dictionary<string, object>();
                                        queryOr.Add("whereType", WhereType.And);
                                        queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                                        queryOr.Add("field", item["field"].ToString());
                                        queryOr.Add("fieldValue", objIdList[i]);
                                        queryOr.Add("ConditionalType", item["symbol"].Equals("like") ? ConditionalType.Like : ConditionalType.NoLike);
                                        queryList.Add(queryOr);
                                    }
                                    else
                                    {
                                        var queryOr = new Dictionary<string, object>();
                                        queryOr.Add("whereType", item["symbol"].Equals("like") ? WhereType.Or : WhereType.And);
                                        queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                                        queryOr.Add("field", item["field"].ToString());
                                        queryOr.Add("fieldValue", objIdList[i]);
                                        queryOr.Add("ConditionalType", item["symbol"].Equals("like") ? ConditionalType.Like : ConditionalType.NoLike);
                                        queryList.Add(queryOr);
                                    }
                                }

                                if (item["symbol"].Equals("notLike"))
                                {
                                    var queryOr = new Dictionary<string, object>();
                                    queryOr.Add("whereType", WhereType.Or);
                                    queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                                    queryOr.Add("field", item["field"].ToString());
                                    queryOr.Add("fieldValue", null);
                                    queryOr.Add("ConditionalType", ConditionalType.EqualNull);
                                    queryList.Add(queryOr);
                                }
                            }
                        }
                        break;
                    case PoxiaoKeyConst.DATE:
                    case PoxiaoKeyConst.CREATETIME:
                    case PoxiaoKeyConst.MODIFYTIME:
                        if (itemValue.IsNotEmptyOrNull()) itemValue = string.Format("{0:yyyy-MM-dd HH:mm:ss}", itemValue.TimeStampToDateTime());
                        item["symbol"] = item["symbol"].Equals("like") ? "in" : item["symbol"];
                        item["symbol"] = item["symbol"].Equals("notLike") ? "notIn" : item["symbol"];
                        break;
                    case PoxiaoKeyConst.CASCADER:
                    case PoxiaoKeyConst.ADDRESS:
                    case PoxiaoKeyConst.COMSELECT:
                        if ((item["symbol"].Equals("like") || item["symbol"].Equals("==")) && item["fieldValue"].IsNotEmptyOrNull())
                        {
                            var queryOr = new Dictionary<string, object>();
                            queryOr.Add("whereType", WhereType.Or);
                            queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                            queryOr.Add("field", item["field"].ToString());
                            queryOr.Add("fieldValue", item["fieldValue"].ToObject<List<string>>().ToJsonString().Replace("[", string.Empty));
                            queryOr.Add("ConditionalType", ConditionalType.Like);
                            queryList.Add(queryOr);
                            continue;
                        }
                        else if ((item["symbol"].Equals("notLike") || item["symbol"].Equals("<>")) && item["fieldValue"].IsNotEmptyOrNull())
                        {
                            var queryOr = new Dictionary<string, object>();
                            queryOr.Add("whereType", whereType);
                            queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                            queryOr.Add("field", item["field"].ToString());
                            queryOr.Add("fieldValue", item["fieldValue"].ToObject<List<string>>().ToJsonString().Replace("[", string.Empty));
                            queryOr.Add("ConditionalType", ConditionalType.NoLike);
                            queryList.Add(queryOr);
                            itemValue = itemValue.Replace("[", string.Empty);
                        }

                        break;
                    case PoxiaoKeyConst.CURRORGANIZE:
                        itemValue = itemValue.Contains("[") ? itemValue.ToObject<List<string>>().LastOrDefault() : itemValue;
                        break;
                    case PoxiaoKeyConst.NUMINPUT:
                    case PoxiaoKeyConst.CALCULATE:
                        query.Add("CSharpTypeName", "decimal");
                        break;
                }

                //if (itemValue.Contains("[")) itemValue = itemValue.Replace("[", string.Empty).Replace("]", string.Empty);
                query.Add("fieldValue", itemValue);
                if ((!item.ContainsKey("fieldValue") || item.ContainsKey("fieldValue").Equals("[]")) && item["symbol"].Equals("=="))
                {
                    if (item["poxiaoKey"].Equals(PoxiaoKeyConst.CALCULATE) || item["poxiaoKey"].Equals(PoxiaoKeyConst.NUMINPUT)) query.Add("ConditionalType", ConditionalType.EqualNull);
                    else query.Add("ConditionalType", ConditionalType.IsNullOrEmpty);
                    queryList.Add(query);
                    continue;
                }

                if ((!item.ContainsKey("fieldValue") || item.ContainsKey("fieldValue").Equals("[]") || itemValue.Equals(string.Empty)) && item["symbol"].Equals("<>"))
                {
                    query.Add("ConditionalType", ConditionalType.IsNot);
                    queryList.Add(query);
                    continue;
                }

                switch (item["symbol"])
                {
                    case ">=":
                        query.Add("ConditionalType", ConditionalType.GreaterThanOrEqual);
                        break;
                    case ">":
                        query.Add("ConditionalType", ConditionalType.GreaterThan);
                        break;
                    case "==":
                        query.Add("ConditionalType", ConditionalType.Equal);
                        break;
                    case "<=":
                        query.Add("ConditionalType", ConditionalType.LessThanOrEqual);
                        break;
                    case "<":
                        query.Add("ConditionalType", ConditionalType.LessThan);
                        break;
                    case "like":
                        query.Add("ConditionalType", itemValue.IsNotEmptyOrNull() ? ConditionalType.Like : ((item["poxiaoKey"].Equals(PoxiaoKeyConst.CALCULATE) || item["poxiaoKey"].Equals(PoxiaoKeyConst.NUMINPUT)) ? ConditionalType.EqualNull : ConditionalType.IsNullOrEmpty));
                        if (query["fieldValue"] != null && query["fieldValue"].ToString().Contains("["))
                            query["fieldValue"] = query["fieldValue"].ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                        break;
                    case "<>":
                        query.Add("ConditionalType", ConditionalType.NoEqual);
                        break;
                    case "notLike":
                        query.Add("ConditionalType", ConditionalType.NoLike);
                        if (query["fieldValue"] != null && query["fieldValue"].ToString().Contains("["))
                            query["fieldValue"] = query["fieldValue"].ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                        break;
                    case "in":
                        query.Add("ConditionalType", itemValue.IsNotEmptyOrNull() ? ConditionalType.In : ConditionalType.IsNullOrEmpty);
                        break;
                    case "notIn":
                        query.Add("ConditionalType", ConditionalType.NotIn);
                        break;
                }

                queryList.Add(query);

                if ((item["symbol"].Equals("<>") || item["symbol"].Equals("notLike")) && itemValue.IsNotEmptyOrNull() && (item["poxiaoKey"].Equals(PoxiaoKeyConst.NUMINPUT) || item["poxiaoKey"].Equals(PoxiaoKeyConst.CALCULATE)))
                {
                    var queryOr = new Dictionary<string, object>();
                    queryOr.Add("whereType", WhereType.Or);
                    queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                    queryOr.Add("field", item["field"].ToString());
                    queryOr.Add("fieldValue", null);
                    queryOr.Add("ConditionalType", ConditionalType.EqualNull);
                    queryList.Add(queryOr);
                }
                if ((item["symbol"].Equals("<>") || item["symbol"].Equals("notLike"))
                    && itemValue.IsNotEmptyOrNull()
                    && !item["poxiaoKey"].Equals(PoxiaoKeyConst.NUMINPUT)
                    && !item["poxiaoKey"].Equals(PoxiaoKeyConst.CALCULATE)
                    && !item["poxiaoKey"].Equals(PoxiaoKeyConst.SWITCH))
                {
                    var queryOr = new Dictionary<string, object>();
                    queryOr.Add("whereType", WhereType.Or);
                    queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                    queryOr.Add("field", item["field"].ToString());
                    queryOr.Add("fieldValue", null);
                    queryOr.Add("ConditionalType", ConditionalType.IsNullOrEmpty);
                    queryList.Add(queryOr);
                }

                if (item["symbol"].Equals("notLike") && itemValue.IsNullOrWhiteSpace())
                {
                    var queryOr = new Dictionary<string, object>();
                    queryOr.Add("whereType", WhereType.Or);
                    queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                    queryOr.Add("field", item["field"].ToString());
                    queryOr.Add("fieldValue", null);
                    queryOr.Add("ConditionalType", ConditionalType.IsNot);
                    queryList.Add(queryOr);
                }

                if (item["symbol"].Equals("notIn") && itemValue.IsNotEmptyOrNull())
                {
                    var queryOr = new Dictionary<string, object>();
                    queryOr.Add("whereType", WhereType.Or);
                    queryOr.Add("poxiaoKey", item["poxiaoKey"]);
                    queryOr.Add("field", item["field"].ToString());
                    queryOr.Add("fieldValue", null);
                    queryOr.Add("ConditionalType", ConditionalType.IsNullOrEmpty);
                    queryList.Add(queryOr);
                }
            }

            return queryList.ToJsonString();
        }
        else return string.Empty;
    }

    /// <summary>
    /// 数据唯一 验证.
    /// </summary>
    /// <param name="link">DbLinkEntity.</param>
    /// <param name="templateInfo">模板信息.</param>
    /// <param name="allDataMap">数据.</param>
    /// <param name="mainPrimary">主键名.</param>
    /// <param name="mainId">主键Id.</param>
    /// <param name="isUpdate">是否修改.</param>
    private void UniqueVerify(DbLinkEntity link, TemplateParsingBase templateInfo, Dictionary<string, object> allDataMap, string mainPrimary, string mainId, bool isUpdate = false)
    {
        // 单行输入 唯一验证
        if (templateInfo.AllFieldsModel.Any(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) && x.__config__.unique))
        {
            List<string>? relationKey = new List<string>();
            List<string>? auxiliaryFieldList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            auxiliaryFieldList.ForEach(tName =>
            {
                string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                relationKey.Add(templateInfo.MainTableName + "." + mainPrimary + "=" + tName + "." + tableField);
            });

            List<string>? fieldList = new List<string>();
            var whereList = new List<IConditionalModel>();

            templateInfo.SingleFormData.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) && x.__config__.unique).ToList().ForEach(item =>
            {
                if (allDataMap.ContainsKey(item.__vModel__) && allDataMap[item.__vModel__].IsNotEmptyOrNull())
                {
                    allDataMap[item.__vModel__] = allDataMap[item.__vModel__].ToString().Trim();
                    fieldList.Add(string.Format("{0}.{1}", item.__config__.tableName, item.__vModel__.Split("_poxiao_").Last()));
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

            var itemWhere = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>("@").Where(whereList).ToSqlString();
            if (!itemWhere.Equals("@"))
            {
                relationKey.Add(itemWhere.Split("WHERE").Last());
                var querStr = string.Format(
                    "select {0} from {1} where ({2}) ",
                    string.Join(",", fieldList),
                    auxiliaryFieldList.Any() ? templateInfo.MainTableName + "," + string.Join(",", auxiliaryFieldList) : templateInfo.MainTableName,
                    string.Join(" and ", relationKey)); // 多表， 联合查询
                if (isUpdate) querStr = string.Format("{0} and {1}<>'{2}'", querStr, templateInfo.MainTableName + "." + mainPrimary, mainId);
                if (templateInfo.FormModel.logicalDelete && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_deletemark")) querStr = string.Format(" {0} and {1} ", querStr, "f_deletemark is null");
                var res = _databaseService.GetInterFaceData(link, querStr, null).ToObject<List<Dictionary<string, string>>>();

                if (res.Any())
                {
                    var errorList = new List<string>();

                    res.ForEach(items =>
                    {
                        foreach (var item in items)
                            errorList.Add(templateInfo.SingleFormData.FirstOrDefault(x => x.__vModel__.Equals(item.Key) || x.__vModel__.Contains("_poxiao_" + item.Key))?.__config__.label);
                    });

                    throw Oops.Oh(ErrorCode.D1407, string.Join(",", errorList.Distinct()));
                }
            }

            foreach (var citem in templateInfo.ChildTableFieldsModelList)
            {
                if (allDataMap.ContainsKey(citem.__vModel__))
                {
                    var childrenValues = allDataMap[citem.__vModel__].ToObject<List<Dictionary<string, object>>>();
                    if (childrenValues.Any())
                    {
                        citem.__config__.children.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) && x.__config__.unique).ToList().ForEach(item =>
                        {
                            var vList = childrenValues.Where(xx => xx.ContainsKey(item.__vModel__)).ToList();
                            vList.ForEach(vitem =>
                            {
                                vitem[item.__vModel__] = vitem[item.__vModel__].ToString().Trim();
                                if (vitem[item.__vModel__] != null && childrenValues.Where(x => x.ContainsKey(item.__vModel__) && x.ContainsValue(vitem[item.__vModel__])).Count() > 1)
                                    throw Oops.Oh(ErrorCode.D1407, item.__config__.label);
                            });
                        });
                    }
                    allDataMap[citem.__vModel__] = childrenValues;
                }
            }
        }
    }

    /// <summary>
    /// 组装列表查询sql.
    /// </summary>
    /// <param name="primaryKey">主键.</param>
    /// <param name="templateInfo">模板.</param>
    /// <param name="input">查询输入.</param>
    /// <param name="tableFieldKeyValue">联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个).</param>
    /// <param name="dataPermissions">数据权限.</param>
    /// <param name="showColumnList">是否只查询显示列.</param>
    /// <returns></returns>
    private string GetListQuerySql(string primaryKey, TemplateParsingBase templateInfo, ref VisualDevModelListQueryInput input, ref Dictionary<string, string> tableFieldKeyValue, List<IConditionalModel> dataPermissions, bool showColumnList = false)
    {
        List<string> fields = new List<string>();

        string? sql = string.Empty; // 查询sql

        // 显示列和搜索列有子表字段
        if (templateInfo.ColumnData.columnList.Any(x => templateInfo.ChildTableFields.ContainsKey(x.__vModel__)) || templateInfo.ColumnData.searchList.Any(x => templateInfo.ChildTableFields.ContainsKey(x.__vModel__)))
        {
            var queryJson = input.QueryJson;
            var superQueryJson = input.superQueryJson;
            foreach (var item in templateInfo.AllTableFields)
            {
                if (input.dataRuleJson.IsNotEmptyOrNull() && input.dataRuleJson.Contains(string.Format("\"{0}\"", item.Key)))
                    input.dataRuleJson = input.dataRuleJson.Replace(string.Format("\"{0}\"", item.Key), string.Format("\"{0}\"", item.Value));

                if (queryJson.Contains(string.Format("\"{0}\"", item.Key)))
                {
                    queryJson = queryJson.Replace(string.Format("\"{0}\"", item.Key), string.Format("\"{0}\"", item.Value));
                    var vmodel = templateInfo.ColumnData.searchList.FirstOrDefault(x => x != null && x.__vModel__.Equals(item.Key));
                    if (vmodel != null)
                    {
                        vmodel.__vModel__ = item.Value;
                        fields.Add(item.Value);
                    }

                    var appVModel = templateInfo.AppColumnData.searchList.FirstOrDefault(x => x != null && x.__vModel__.Equals(item.Key));
                    if (appVModel != null)
                    {
                        appVModel.__vModel__ = item.Value;
                        fields.Add(item.Value);
                    }
                }

                if (superQueryJson.IsNotEmptyOrNull() && superQueryJson.Contains(string.Format("\"{0}\"", item.Key)))
                    superQueryJson = superQueryJson.Replace(string.Format("\"{0}\"", item.Key), string.Format("\"{0}\"", item.Value));
            }

            var dataRuleQuerDic = new List<IConditionalModel>();
            if (input.dataRuleJson.IsNotEmptyOrNull()) dataRuleQuerDic = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(input.dataRuleJson);
            var querDic = queryJson.IsNullOrEmpty() ? null : queryJson.ToObject<Dictionary<string, object>>();

            var superQuerDic = new List<ConditionalCollections>();
            var superCond = superQueryJson.IsNullOrEmpty() ? null : GetSuperQueryJson(superQueryJson, templateInfo);
            if (superCond != null) superQuerDic = superCond.ToObject<List<ConditionalCollections>>();
            var tableWhere = new Dictionary<string, List<IConditionalModel>>();
            var queryOrSqlList = new List<string>();
            templateInfo.AllTable.Select(x => x.table).Distinct().ToList().ForEach(tName =>
            {
                var query = new Dictionary<string, object>();
                if (querDic != null && querDic.Any())
                {
                    querDic.Where(x => x.Key.Contains(tName + ".")).ToList().ForEach(x => query.Add(x.Key, x.Value));
                    if (query.Any())
                    {
                        if (!tableWhere.ContainsKey(tName)) tableWhere.Add(tName, GetQueryJson(query.ToJsonString(), _userManager.UserOrigin == "pc" ? templateInfo.ColumnData : templateInfo.AppColumnData));
                        else tableWhere[tName].AddRange(GetQueryJson(query.ToJsonString(), _userManager.UserOrigin == "pc" ? templateInfo.ColumnData : templateInfo.AppColumnData));
                    }
                }

                if (superQuerDic != null && superQuerDic.Any())
                {
                    var conList = new ConditionalCollections() { ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>() };
                    var dicList = superQuerDic.Where(x => x.ConditionalList.Any(xx => xx.Value.FieldName.Contains(tName + "."))).ToList();
                    for (var i = 0; i < dicList.Count; i++)
                    {
                        var items = dicList[i];
                        var newItems = items.ToObject<ConditionalCollections>();
                        if (querDic != null && querDic.Any() && i == 0 && newItems.ConditionalList.Any())
                            newItems.ConditionalList[0] = newItems.ConditionalList.FirstOrDefault().ToJsonString().Replace("\"Key\":1,", "\"Key\":0,").ToObject<KeyValuePair<WhereType, ConditionalModel>>();
                        conList.ConditionalList.AddRange(newItems.ConditionalList);
                    }

                    // 子表字段为空 查询 处理.
                    if (templateInfo.ChildTableFields.Any(x => x.Value.Contains(tName + ".")) && (dicList.ToJsonString().Contains("\"ConditionalType\":11") || dicList.ToJsonString().Contains("\"ConditionalType\":14")))
                        queryOrSqlList.Add(string.Format(" OR ( {0} NOT IN ( SELECT {1} FROM {2} ) ) ", primaryKey, templateInfo.AllTable.Where(x => x.table.Equals(tName)).First().tableField, tName));

                    if (conList.ConditionalList.Any())
                    {
                        if (!tableWhere.ContainsKey(tName)) tableWhere.Add(tName, new List<IConditionalModel>() { conList });
                        else tableWhere[tName].AddRange(new List<IConditionalModel>() { conList });
                    }
                }

                if (dataRuleQuerDic != null && dataRuleQuerDic.Any() && dataRuleQuerDic.ToJsonString().Contains(tName))
                {
                    var dicList = GetIConditionalModelListByTableName(dataRuleQuerDic.Copy(), tName);

                    // 子表字段为空 查询 处理.
                    if (templateInfo.ChildTableFields.Any(x => x.Value.Contains(tName + ".")) && (dicList.ToJsonString().Contains("\"ConditionalType\":11") || dicList.ToJsonString().Contains("\"ConditionalType\":14")))
                        queryOrSqlList.Add(string.Format(" OR ( {0} NOT IN ( SELECT {1} FROM {2} ) ) ", primaryKey, templateInfo.AllTable.Where(x => x.table.Equals(tName)).First().tableField, tName));

                    if(dicList.Any())
                    {
                        if (!tableWhere.ContainsKey(tName)) tableWhere.Add(tName, dicList);
                        else tableWhere[tName].AddRange(dicList);
                    }
                }

                if (!tableWhere.ContainsKey(tName)) tableWhere.Add(tName, new List<IConditionalModel>());
            });

            var querySqlList = new List<string>();
            var sqlStr = "select {0} from {1} ";
            if (templateInfo.FormModel.logicalDelete && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_deletemark"))
                querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, string.Format(" select {0} from {1} where f_deletemark is null ", primaryKey, templateInfo.MainTableName))); // 处理软删除
            if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN") && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_tenantid"))
                querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, string.Format(" select {0} from {1} where f_tenantid='{2}'", primaryKey, templateInfo.MainTableName, _userManager.CurrentTenantInformation.connectionConfig.IsolationField))); // 多租户

            // 是否只展示流程数据
            //if (templateInfo.visualDevEntity.EnableFlow.Equals(1))
            //    querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, string.Format(" select {0} from {1} where f_flowid <> '' ", primaryKey, templateInfo.MainTableName)));
            //else
            //    querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, string.Format(" select {0} from {1} where f_flowid is null or f_flowid = '' ", primaryKey, templateInfo.MainTableName)));

            var childNotInQuery = new Dictionary<string, string>(); // 子表字段为空条件
            foreach (var item in tableWhere)
            {
                // 拼接数据权限
                var dataPermissionsList = new List<IConditionalModel>();
                if (dataPermissions != null && dataPermissions.Any())
                {
                    var pList = new List<object>();
                    var allPersissions = dataPermissions.ToObject<List<object>>();
                    allPersissions.ForEach(it =>
                    {
                        if (it.ToJsonString().Contains(item.Key)) pList.Add(it);
                        else if (item.Key.Equals(templateInfo.MainTableName) && it.ToJsonString().Contains(string.Format("\"FieldName\":\"{0}\"", primaryKey))) pList.Add(it);
                    });
                    if (pList.Any())
                        dataPermissionsList = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pList.ToJsonString());
                }

                var idField = templateInfo.AllTable.Where(x => x.table.Equals(item.Key)).First().tableField;
                var itemSql = string.Format(sqlStr, idField.IsNullOrEmpty() ? primaryKey : idField, item.Key);
                dataPermissionsList = GetIConditionalModelListByTableName(dataPermissionsList, item.Key); // 删除非当前表条件
                var itemWhere = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>("@").Where(item.Value).Where(dataPermissionsList).ToSqlString();
                if (itemWhere.Contains("WHERE"))
                {
                    itemSql = itemSql + " where " + itemWhere.Split("WHERE").Last();
                    querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, itemSql));
                }
            }
            if (!querySqlList.Any()) querySqlList.Add(string.Format(" {0} in ({1}) ", primaryKey, string.Format(sqlStr, primaryKey, templateInfo.MainTableName)));
            if (templateInfo.ChildTableFields.ContainsKey(input.Sidx)) input.Sidx = string.Empty; // 子表字段暂时无法排序
            if (templateInfo.ChildTableFields.ContainsKey(templateInfo.ColumnData.defaultSidx)) templateInfo.ColumnData.defaultSidx = string.Empty; // 子表字段暂时无法排序

            var ctFields = templateInfo.ChildTableFields;
            templateInfo.ChildTableFields = new Dictionary<string, string>();
            var strSql = GetListQuerySql(primaryKey, templateInfo, ref input, ref tableFieldKeyValue, new List<IConditionalModel>());
            input.dataRuleJson = string.Empty;
            input.QueryJson = string.Empty;
            input.superQueryJson = string.Empty;
            templateInfo.ChildTableFields = ctFields;

            sql = string.Format("select * from ({0}) mt where {1} {2}", strSql, string.Join(" and ", querySqlList), string.Join(string.Empty, queryOrSqlList));
        }
        else if (!templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            fields.Add(primaryKey); // 主键
            if (templateInfo.WebType.Equals(3)) fields.Add("F_FlowId");

            // 只查询 要显示的列
            if (showColumnList && (templateInfo.SingleFormData.Count > 0 || templateInfo.ColumnData.columnList.Count > 0))
                templateInfo.ColumnData.columnList.ForEach(item => { if (templateInfo.SingleFormData.Any(x => x.__vModel__.Equals(item.prop))) fields.Add(item.prop); });
            else
                templateInfo.MainTableFieldsModelList.Where(x => x.__vModel__.IsNotEmptyOrNull()).ToList().ForEach(item => fields.Add(item.__vModel__)); // 字段

            sql = string.Format("select {0} from {1}", string.Join(",", fields), templateInfo.MainTableName);
            if (templateInfo.FormModel.logicalDelete && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_deletemark"))
                sql += " where f_deletemark is null "; // 处理软删除
            if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN") && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_tenantid"))
                sql += string.Format(" {0} f_tenantid='{1}' ", sql.Contains("where") ? "and" : "where", _userManager.CurrentTenantInformation.connectionConfig.IsolationField); // 多租户

            // 是否只展示流程数据
            //if (templateInfo.visualDevEntity.EnableFlow.Equals(1)) sql += string.Format(" {0} f_flowid <> '' ", sql.Contains("where") ? "and" : "where");
            //else sql += string.Format(" {0} f_flowid is null or f_flowid = '' ", sql.Contains("where") ? "and" : "where");

            // 拼接数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                // 替换数据权限字段 别名
                var pvalue = dataPermissions.ToJsonString();
                foreach (var item in fields)
                {
                    if (pvalue.Contains(templateInfo.MainTableName + "." + item))
                        pvalue = pvalue.Replace(string.Format("\"FieldName\":\"{0}\",", templateInfo.MainTableName + "." + item), string.Format("\"FieldName\":\"{0}\",", item));
                }

                List<IConditionalModel>? newPvalue = new List<IConditionalModel>();
                if (pvalue.IsNotEmptyOrNull()) newPvalue = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pvalue);

                sql = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>(sql).Where(newPvalue).ToSqlString();
            }

        }
        else
        {
            #region 所有主、副表 字段名 和 处理查询、排序字段

            // 所有主、副表 字段名
            fields.Add(templateInfo.MainTableName + "." + primaryKey);
            if (templateInfo.WebType.Equals(3)) fields.Add(templateInfo.MainTableName + ".F_FlowId");
            tableFieldKeyValue.Add(primaryKey.ToUpper(), primaryKey);
            tableFieldKeyValue.Add("F_FlowId".ToUpper(), "F_FlowId");
            Dictionary<string, object>? inputJson = input.QueryJson?.ToObject<Dictionary<string, object>>();
            for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
            {
                string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_poxiao_", string.Empty); // Field

                // 只显示要显示的列
                if (showColumnList && !templateInfo.ColumnData.columnList.Any(x => x.prop == templateInfo.SingleFormData[i].__vModel__))
                    vmodel = string.Empty;

                if (vmodel.IsNotEmptyOrNull())
                {
                    fields.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD_" + i); // TableName.Field_0
                    tableFieldKeyValue.Add("FIELD_" + i, templateInfo.SingleFormData[i].__vModel__);

                    // 查询字段替换
                    if (inputJson != null && inputJson.Count > 0 && inputJson.ContainsKey(templateInfo.SingleFormData[i].__vModel__))
                        input.QueryJson = input.QueryJson.Replace("\"" + templateInfo.SingleFormData[i].__vModel__ + "\":", "\"FIELD_" + i + "\":");
                    if (input.superQueryJson.IsNotEmptyOrNull())
                        input.superQueryJson = input.superQueryJson.Replace(string.Format("\"field\":\"{0}\"", templateInfo.SingleFormData[i].__vModel__), string.Format("\"field\":\"{0}\"", "FIELD_" + i));
                    if (input.dataRuleJson.IsNotEmptyOrNull())
                        input.dataRuleJson = input.dataRuleJson.Replace(string.Format("\"FieldName\":\"{0}\"", templateInfo.SingleFormData[i].__vModel__), string.Format("\"FieldName\":\"{0}\"", "FIELD_" + i));

                    templateInfo.ColumnData.searchList.Where(x => x.__vModel__ == templateInfo.SingleFormData[i].__vModel__).ToList().ForEach(item =>
                    {
                        item.__vModel__ = item.__vModel__.Replace(templateInfo.SingleFormData[i].__vModel__, "FIELD_" + i);
                    });

                    // 排序字段替换
                    if (templateInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && templateInfo.ColumnData.defaultSidx == templateInfo.SingleFormData[i].__vModel__)
                        templateInfo.ColumnData.defaultSidx = "FIELD_" + i;

                    if (input.Sidx.IsNotEmptyOrNull() && input.Sidx == templateInfo.SingleFormData[i].__vModel__) input.Sidx = "FIELD_" + i;
                }

            }

            #endregion

            #region 关联字段

            List<string>? relationKey = new List<string>();
            List<string>? auxiliaryFieldList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            auxiliaryFieldList.ForEach(tName =>
            {
                var tableField = templateInfo.AllTable.Find(tf => tf.table == tName);
                relationKey.Add(templateInfo.MainTableName + "." + tableField.relationField + "=" + tName + "." + tableField.tableField);
            });
            if (templateInfo.FormModel.logicalDelete && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_deletemark"))
                relationKey.Add(templateInfo.MainTableName + ".f_deletemark is null "); // 处理软删除
            if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN") && _databaseService.IsAnyColumn(templateInfo.DbLink, templateInfo.MainTableName, "f_tenantid"))
                relationKey.Add(string.Format(" {0}.f_tenantid='{1}' ", templateInfo.MainTableName, _userManager.CurrentTenantInformation.connectionConfig.IsolationField)); // 多租户

            // 是否只展示流程数据
            //if (templateInfo.visualDevEntity.EnableFlow.Equals(1)) relationKey.Add(templateInfo.MainTableName + ".f_flowid <> '' ");
            //else relationKey.Add(templateInfo.MainTableName + ".f_flowid is null or f_flowid = '' ");

            string? whereStr = string.Join(" and ", relationKey);

            #endregion

            sql = string.Format("select {0} from {1} where {2}", string.Join(",", fields), templateInfo.MainTableName + "," + string.Join(",", auxiliaryFieldList), whereStr); // 多表， 联合查询

            // 拼接数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                // 替换数据权限字段 别名
                var pvalue = dataPermissions.ToJsonString();
                foreach (var item in tableFieldKeyValue)
                {
                    string? newValue = item.Value;
                    if (templateInfo.AllTableFields.ContainsKey(item.Value)) newValue = templateInfo.AllTableFields[item.Value];
                    if (pvalue.Contains(newValue))
                    {
                        pvalue = pvalue.Replace(string.Format("\"FieldName\":\"{0}\",", newValue), string.Format("\"FieldName\":\"{0}\",", item.Key));
                    }
                    else
                    {
                        if (newValue.Contains(templateInfo.MainTableName)) newValue = newValue.Replace(templateInfo.MainTableName + ".", string.Empty);
                        if (pvalue.Contains(newValue)) pvalue = pvalue.Replace(string.Format("\"FieldName\":\"{0}\",", newValue), string.Format("\"FieldName\":\"{0}\",", item.Key));
                    }
                }

                List<IConditionalModel>? newPvalue = new List<IConditionalModel>();
                if (pvalue.IsNotEmptyOrNull()) newPvalue = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pvalue);

                sql = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>(sql).Where(newPvalue).ToSqlString();
            }
        }

        return sql;
    }
    private List<IConditionalModel> GetIConditionalModelListByTableName(List<IConditionalModel> cList, string tableName)
    {
        for (int i = 0; i < cList.Count; i++)
        {
            if (cList[i] is ConditionalTree)
            {
                var newItem = (ConditionalTree)cList[i];
                for (int j = 0; j < newItem.ConditionalList.Count; j++)
                {
                    var value = GetIConditionalModelListByTableName(new List<IConditionalModel>() { newItem.ConditionalList[j].Value }, tableName);
                    if (value != null && value.Any())
                    {
                        newItem.ConditionalList[j] = new KeyValuePair<WhereType, IConditionalModel>(newItem.ConditionalList[j].Key, value.First());
                    }
                    else
                    {
                        newItem.ConditionalList.RemoveAt(j);
                        j--;
                    }
                }

                if (newItem.ConditionalList.Any())
                {
                    cList[i] = newItem;
                }
                else
                {
                    cList.RemoveAt(i);
                    i--;
                }
            }
            else if (cList[i] is ConditionalModel)
            {
                var newItem = (ConditionalModel)cList[i];
                if (!newItem.FieldName.Contains(tableName)) cList.RemoveAt(i);
            }
        }

        return cList;
    }

    /// <summary>
    /// 组装单条信息查询sql.
    /// </summary>
    /// <param name="id">id.</param>
    /// <param name="mainPrimary">主键.</param>
    /// <param name="templateInfo">模板.</param>
    /// <param name="tableFieldKeyValue">联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个).</param>
    /// <returns></returns>
    private string GetInfoQuerySql(string id, string mainPrimary, TemplateParsingBase templateInfo, ref Dictionary<string, string> tableFieldKeyValue)
    {
        List<string> fields = new List<string>();
        string? sql = string.Empty; // 查询sql

        // 没有副表,只查询主表
        if (!templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            fields.Add(mainPrimary); // 主表主键
            if (templateInfo.WebType.Equals(3)) fields.Add("F_FlowId");
            templateInfo.MainTableFieldsModelList.Where(x => x.__vModel__.IsNotEmptyOrNull()).ToList().ForEach(item => fields.Add(item.__vModel__)); // 主表列名
            sql = string.Format("select {0} from {1} where {2}='{3}'", string.Join(",", fields), templateInfo.MainTableName, mainPrimary, id);
        }
        else
        {
            #region 所有主表、副表 字段名
            fields.Add(templateInfo.MainTableName + "." + mainPrimary); // 主表主键
            if (templateInfo.WebType.Equals(3)) fields.Add(templateInfo.MainTableName + ".F_FlowId");
            for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
            {
                string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_poxiao_", ""); // Field
                if (vmodel.IsNotEmptyOrNull())
                {
                    fields.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD" + i); // TableName.Field_0
                    tableFieldKeyValue.Add("FIELD" + i, templateInfo.SingleFormData[i].__vModel__);
                }
            }
            #endregion

            #region 所有副表 关联字段
            List<string>? ctNameList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            List<string>? relationKey = new List<string>();
            relationKey.Add(string.Format(" {0}.{1}='{2}' ", templateInfo.MainTableName, mainPrimary, id)); // 主表ID
            ctNameList.ForEach(tName =>
            {
                string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                relationKey.Add(string.Format(" {0}.{1}={2}.{3} ", templateInfo.MainTableName, mainPrimary, tName, tableField));
            });
            string? whereStr = string.Join(" and ", relationKey);
            #endregion

            sql = string.Format("select {0} from {1} where {2}", string.Join(",", fields), templateInfo.MainTableName + "," + string.Join(",", ctNameList), whereStr); // 多表， 联合查询
        }

        return sql;
    }

    /// <summary>
    /// 组装 查询 json.
    /// </summary>
    /// <param name="queryJson"></param>
    /// <param name="columnDesign"></param>
    /// <returns></returns>
    private List<IConditionalModel> GetQueryJson(string queryJson, ColumnDesignModel columnDesign)
    {
        // 将查询的关键字json转成Dictionary
        Dictionary<string, object> keywordJsonDic = string.IsNullOrEmpty(queryJson) ? null : queryJson.ToObject<Dictionary<string, object>>();
        var conModels = new List<IConditionalModel>();
        if (keywordJsonDic != null)
        {
            foreach (KeyValuePair<string, object> item in keywordJsonDic)
            {
                var model = columnDesign.searchList.Find(it => it.__vModel__.Equals(item.Key));
                switch (model.poxiaoKey)
                {
                    case PoxiaoKeyConst.DATE:
                        {
                            var timeRange = item.Value.ToObject<List<string>>();
                            var startTime = timeRange.First().TimeStampToDateTime();
                            var endTime = timeRange.Last().TimeStampToDateTime();

                            if (model.format.Equals("yyyy"))
                            {
                                startTime = new DateTime(startTime.Year, 1, 1, 0, 0, 0, 0);
                                endTime = new DateTime(endTime.Year, 1, 1, 0, 0, 0, 0);
                            }

                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.GreaterThanOrEqual,
                                            FieldValue = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second, 0).ToString(),
                                            FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                        }),
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.LessThanOrEqual,
                                            FieldValue = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second, 999).ToString(),
                                            FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                        })
                                    }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.TIME:
                        {
                            var timeRange = item.Value.ToObject<List<string>>();
                            var startTime = string.Format("{0:" + model.format + "}", Convert.ToDateTime(timeRange.First()));
                            var endTime = string.Format("{0:" + model.format + "}", Convert.ToDateTime(timeRange.Last()));
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.GreaterThanOrEqual,
                                        FieldValue = startTime
                                    }),
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.LessThanOrEqual,
                                        FieldValue = endTime
                                    })
                                }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.CREATETIME:
                    case PoxiaoKeyConst.MODIFYTIME:
                        {
                            var timeRange = item.Value.ToObject<List<string>>();
                            var startTime = timeRange.First().TimeStampToDateTime();
                            var endTime = timeRange.Last().TimeStampToDateTime();
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.GreaterThanOrEqual,
                                        FieldValue = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0).ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    }),
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.LessThanOrEqual,
                                        FieldValue = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999).ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    })
                                }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.NUMINPUT:
                    case PoxiaoKeyConst.CALCULATE:
                        {
                            List<string> numArray = item.Value.ToObject<List<string>>();
                            var startNum = numArray.First().ParseToDecimal();
                            var endNum = numArray.Last() == null ? decimal.MaxValue : numArray.Last().ParseToDecimal();
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        CSharpTypeName="decimal",
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.GreaterThanOrEqual,
                                        FieldValue = startNum.ToString()
                                    }),
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        CSharpTypeName="decimal",
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.LessThanOrEqual,
                                        FieldValue = endNum.ToString()
                                    })
                                }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.CHECKBOX:
                        {
                            //if (model.searchType.Equals(1))
                            //    conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Equal, FieldValue = item.Value.ToString() });
                            //else
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.Like,
                                        FieldValue = item.Value.ToJsonString()
                                    })
                                    }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.ROLESELECT:
                    case PoxiaoKeyConst.GROUPSELECT:
                    case PoxiaoKeyConst.POSSELECT:
                    case PoxiaoKeyConst.USERSELECT:
                    case PoxiaoKeyConst.DEPSELECT:
                        {
                            // 多选时为模糊查询
                            if (model.multiple || model.searchMultiple)
                            {
                                var value = item.Value.ToString().Contains("[") ? item.Value.ToObject<List<object>>() : new List<object>() { item.Value.ToString() };
                                var addItems = new List<KeyValuePair<WhereType, ConditionalModel>>();
                                for (int i = 0; i < value.Count; i++)
                                {
                                    var add = new KeyValuePair<WhereType, ConditionalModel>(i == 0 ? WhereType.And : WhereType.Or, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = model.multiple ? ConditionalType.Like : ConditionalType.Equal,
                                        FieldValue = model.multiple ? value[i].ToJsonString() : value[i].ToString()
                                    });
                                    addItems.Add(add);
                                }

                                conModels.Add(new ConditionalCollections() { ConditionalList = addItems });
                            }
                            else
                            {
                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Equal,
                                            FieldValue = item.Value.ToString()
                                        })
                                    }
                                });
                            }
                        }

                        break;
                    case PoxiaoKeyConst.USERSSELECT:
                        {
                            if (item.Value != null)
                            {
                                if (model.multiple || model.searchMultiple)
                                {
                                    var objIdList = new List<string>();
                                    if (item.Value.ToString().Contains("[")) objIdList = item.Value.ToObject<List<string>>();
                                    else objIdList.Add(item.Value.ToString());
                                    var rIdList = _visualDevRepository.AsSugarClient().Queryable<UserRelationEntity>().Where(x => objIdList.Select(xx => xx.Replace("--user", string.Empty)).Contains(x.UserId)).Select(x => new { x.ObjectId, x.ObjectType }).ToList();
                                    rIdList.ForEach(x =>
                                    {
                                        if (x.ObjectType.Equals("Organize"))
                                        {
                                            objIdList.Add(x.ObjectId + "--company");
                                            objIdList.Add(x.ObjectId + "--department");
                                        }
                                        else
                                        {
                                            objIdList.Add(x.ObjectId + "--" + x.ObjectType.ToLower());
                                        }
                                    });

                                    var whereList = new List<KeyValuePair<WhereType, ConditionalModel>>();
                                    for (var i = 0; i < objIdList.Count(); i++)
                                    {
                                        if (i == 0)
                                        {
                                            whereList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Like,
                                                FieldValue = objIdList[i]
                                            }));
                                        }
                                        else
                                        {
                                            whereList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Like,
                                                FieldValue = objIdList[i]
                                            }));
                                        }
                                    }

                                    conModels.Add(new ConditionalCollections() { ConditionalList = whereList });
                                }
                                else
                                {
                                    conModels.Add(new ConditionalCollections()
                                    {
                                        ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                          {
                                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Equal,
                                                FieldValue = item.Value.ToString()
                                            })
                                          }
                                    });
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.TREESELECT:
                        {
                            if (item.Value.IsNotEmptyOrNull() && item.Value.ToString().Contains("["))
                            {
                                var value = item.Value.ToObject<List<string>>();

                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Like,
                                            FieldValue = value.LastOrDefault()
                                        })
                                    }
                                });
                            }
                            else
                            {
                                // 多选时为模糊查询
                                if (model.multiple)
                                {
                                    conModels.Add(new ConditionalCollections()
                                    {
                                        ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                          {
                                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Like,
                                                FieldValue = item.Value.ToString()
                                            })
                                          }
                                    });
                                }
                                else
                                {
                                    conModels.Add(new ConditionalCollections()
                                    {
                                        ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                          {
                                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Equal,
                                                FieldValue = item.Value.ToString()
                                            })
                                          }
                                    });
                                }
                            }
                        }

                        break;
                    case PoxiaoKeyConst.CURRORGANIZE:
                        {
                            var itemValue = item.Value.ToString().Contains("[") ? item.Value?.ToString().ToObject<List<string>>().LastOrDefault() : item.Value.ToString();
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Equal,
                                            FieldValue = itemValue
                                        })
                                    }
                            });
                        }

                        break;
                    case PoxiaoKeyConst.CASCADER:
                        {
                            var itemValue = item.Value.ToString().Contains("[") ? item.Value?.ToString().ToObject<List<string>>().ToJsonString() : item.Value.ToString();
                            conModels.Add(new ConditionalCollections()
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                {
                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.Like,
                                        FieldValue = itemValue.Replace("[", string.Empty).Replace("]", string.Empty)
                                    })
                                }
                            });
                        }
                        break;
                    case PoxiaoKeyConst.ADDRESS:
                    case PoxiaoKeyConst.COMSELECT:
                        {
                            // 多选时为模糊查询
                            if (model.multiple || model.searchMultiple)
                            {
                                var value = item.Value?.ToString().ToObject<List<object>>();
                                if (value.Any())
                                {
                                    var addItems = new List<KeyValuePair<WhereType, ConditionalModel>>();
                                    for (int i = 0; i < value.Count; i++)
                                    {
                                        var add = new KeyValuePair<WhereType, ConditionalModel>(i == 0 ? WhereType.And : WhereType.Or, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Like,
                                            FieldValue = value[i].ToJsonString().Contains("[") ? value[i].ToJsonString().Replace("[", string.Empty) : item.Value?.ToString().Replace("[", string.Empty).Replace("\r\n", string.Empty).Replace(" ", string.Empty),
                                        });
                                        addItems.Add(add);
                                    }
                                    conModels.Add(new ConditionalCollections() { ConditionalList = addItems });
                                }
                            }
                            else
                            {
                                var itemValue = item.Value.ToString().Contains("[") ? item.Value.ToJsonString() : item.Value.ToString();
                                if (itemValue.Contains("[[")) itemValue = itemValue.ToObject<List<List<object>>>().FirstOrDefault().ToJsonString();
                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Equal,
                                            FieldValue = itemValue
                                        })
                                    }
                                });
                            }
                        }

                        break;
                    case PoxiaoKeyConst.SELECT:
                        {
                            var itemValue = item.Value.ToString().Contains("[") ? item.Value.ToJsonString() : item.Value.ToString();

                            // 多选时为模糊查询
                            if (model.multiple || model.searchMultiple)
                            {
                                var value = item.Value.ToString().Contains("[") ? item.Value.ToObject<List<object>>() : new List<object>() { item.Value.ToString() };
                                var addItems = new List<KeyValuePair<WhereType, ConditionalModel>>();
                                for (int i = 0; i < value.Count; i++)
                                {
                                    var add = new KeyValuePair<WhereType, ConditionalModel>(i == 0 ? WhereType.And : WhereType.Or, new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = model.multiple ? ConditionalType.Like : ConditionalType.Equal,
                                        FieldValue = model.multiple ? value[i].ToJsonString() : value[i].ToString()
                                    });
                                    addItems.Add(add);
                                }

                                conModels.Add(new ConditionalCollections() { ConditionalList = addItems });
                            }
                            else
                            {
                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                    {
                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                        {
                                            FieldName = item.Key,
                                            ConditionalType = ConditionalType.Equal,
                                            FieldValue = itemValue
                                        })
                                    }
                                });
                            }
                        }

                        break;
                    default:
                        {
                            var itemValue = item.Value.ToString().Contains("[") ? item.Value.ToJsonString() : item.Value.ToString();

                            if (model.searchType == 1)
                            {
                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                          {
                                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Equal,
                                                FieldValue = itemValue
                                            })
                                          }
                                });
                            }
                            else
                            {
                                conModels.Add(new ConditionalCollections()
                                {
                                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                                          {
                                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                                            {
                                                FieldName = item.Key,
                                                ConditionalType = ConditionalType.Like,
                                                FieldValue = itemValue
                                            })
                                          }
                                });
                            }
                        }

                        break;
                }
            }
        }

        return conModels;
    }

    /// <summary>
    /// 组装高级查询条件.
    /// </summary>
    /// <param name="superQueryJson"></param>
    /// <returns></returns>
    private List<IConditionalModel> GetSuperQueryJson(string superQueryJson, TemplateParsingBase tInfo)
    {
        List<IConditionalModel> conModels = new List<IConditionalModel>();
        if (superQueryJson.IsNotEmptyOrNull())
        {
            var querList = superQueryJson.ToObject<List<Dictionary<string, object>>>();

            querList.DistinctBy(x => x["field"]).ToList().ForEach(item =>
              {
                  var items = querList.Where(x => x.ContainsValue(item["field"].ToString())).ToList();
                  string _CSharpTypeName = item.ContainsKey("CSharpTypeName") ? item["CSharpTypeName"].ToString() : null;
                  if (items.Count() > 1)
                  {
                      var whereTypeList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>();
                      items.ForEach(it =>
                      {
                          var whereType = it["whereType"].ToString().ToObject<WhereType>();
                          var conditionalType = it["ConditionalType"].ToString().ToObject<ConditionalType>();
                          whereTypeList.Add(new KeyValuePair<WhereType, ConditionalModel>(whereType, new ConditionalModel
                          {
                              //CSharpTypeName = _CSharpTypeName,
                              FieldName = it["field"].ToString(),
                              ConditionalType = conditionalType,
                              FieldValue = it["fieldValue"].IsNotEmptyOrNull() ? it["fieldValue"].ToString() : null
                          }));
                      });
                      conModels.Add(new ConditionalCollections() { ConditionalList = whereTypeList });
                  }
                  else
                  {
                      var whereType = item["whereType"].ToString().ToObject<WhereType>();
                      var conditionalType = item["ConditionalType"].ToString().ToObject<ConditionalType>();
                      conModels.Add(new ConditionalCollections()
                      {
                          ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                      {
                        new KeyValuePair<WhereType, ConditionalModel>(whereType, new ConditionalModel
                        {
                            CSharpTypeName = _CSharpTypeName,
                            FieldName = item["field"].ToString(),
                            ConditionalType = conditionalType,
                            FieldValue = item["fieldValue"].IsNotEmptyOrNull() ? item["fieldValue"].ToString() : null
                        })
                      }
                      });
                  }
              });

            for (int i = 0; i < conModels.Count; i++)
            {
                var newItem = (ConditionalCollections)conModels[i];
                if (newItem.ConditionalList.ToJsonString().Contains("\"ConditionalType\":11") || newItem.ConditionalList.ToJsonString().Contains("\"ConditionalType\":14"))
                {
                    var tName = string.Empty;
                    if (newItem.ConditionalList.First().Value.FieldName.Contains('.')) tName = newItem.ConditionalList.First().Value.FieldName.Split(".").First();
                    else tName = tInfo.AllTableFields[newItem.ConditionalList.First().Value.FieldName].Split(".").First();
                    var tCount = 0;
                    for (int j = 0; j < conModels.Count; j++)
                    {
                        var jName = string.Empty;
                        if (newItem.ConditionalList.First().Value.FieldName.Contains('.')) jName = ((ConditionalCollections)conModels[j]).ConditionalList.First().Value.FieldName.Split(".").First();
                        else jName = tInfo.AllTableFields[((ConditionalCollections)conModels[j]).ConditionalList.First().Value.FieldName].Split(".").First();
                        if (tName == jName) tCount++;
                    }

                    if (tCount > 1)
                    {
                        for (int k = 0; k < newItem.ConditionalList.Count; k++)
                            if (newItem.ConditionalList[k].Value.ConditionalType.Equals(ConditionalType.IsNullOrEmpty) || newItem.ConditionalList[k].Value.ConditionalType.Equals(ConditionalType.EqualNull)) newItem.ConditionalList.RemoveAt(k);
                    }

                    conModels[i] = newItem;
                }
            }
        }

        return conModels;
    }

    /// <summary>
    /// 显示列有子表字段,根据主键查询所有子表.
    /// </summary>
    /// <param name="templateInfo"></param>
    /// <param name="primaryKey"></param>
    /// <param name="querList"></param>
    /// <param name="dataRuleList"></param>
    /// <param name="superQuerList"></param>
    /// <param name="result"></param>
    /// <param name="dataPermissions"></param>
    /// <returns></returns>
    private async Task<PageResult<Dictionary<string, object>>> GetListChildTable(
        TemplateParsingBase templateInfo,
        string primaryKey,
        List<IConditionalModel> querList,
        List<IConditionalModel> dataRuleList,
        List<IConditionalModel> superQuerList,
        PageResult<Dictionary<string, object>> result,
        List<IConditionalModel> dataPermissions)
    {
        var ids = new List<object>();
        result.list.ForEach(item => ids.Add(item[primaryKey]));

        var childTableList = new Dictionary<string, List<string>>();

        templateInfo.AllFieldsModel.Where(x => x.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).ToList().ForEach(ctitem =>
        {
            templateInfo.AllFieldsModel.Where(x => x.__vModel__.Contains(ctitem.__vModel__ + "-")).ToList().ForEach(item =>
            {
                var value = item.__vModel__.Split("-").Last();
                if (value.IsNotEmptyOrNull())
                {
                    if (childTableList.ContainsKey(ctitem.__config__.tableName)) childTableList[ctitem.__config__.tableName].Add(value);
                    else childTableList.Add(ctitem.__config__.tableName, new List<string>() { value });
                }
            });
        });

        var relationField = new Dictionary<string, string>();
        templateInfo.ChildTableFieldsModelList.ForEach(item =>
        {
            var tableField = templateInfo.AllTable.Find(tf => tf.table == item.__config__.tableName)?.tableField;
            if (!relationField.ContainsKey(item.__config__.tableName)) relationField.Add(item.__config__.tableName, tableField.ToLower());
        });

        var dataRuleJson = dataRuleList.ToJsonString();
        foreach (var item in templateInfo.AllTableFields)
        {
            if (dataRuleJson.IsNotEmptyOrNull() && dataRuleJson.Contains(string.Format("\"{0}\"", item.Key)))
                dataRuleJson = dataRuleJson.Replace(string.Format("\"{0}\"", item.Key), string.Format("\"{0}\"", item.Value));
        }

        // 捞取 所有子表查询条件 <tableName , where>
        var childTableQuery = new Dictionary<string, List<IConditionalModel>>();
        var dataRule = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(dataRuleJson);
        var query = querList.ToObject<List<ConditionalCollections>>();
        var superQuery = superQuerList.ToObject<List<ConditionalCollections>>();
        foreach (var item in templateInfo.ChildTableFields)
        {
            var tableName = item.Value.Split(".").FirstOrDefault();
            var dataRuleConList = GetIConditionalModelListByTableName(dataRuleList, tableName);
            if (dataRuleConList.Any())
            {
                //foreach (var it in dataRuleConList) it.ConditionalList.ForEach(x => x.Value.FieldName = item.Value);
                if (!childTableQuery.ContainsKey(tableName)) childTableQuery.Add(tableName, new List<IConditionalModel>());
                childTableQuery[tableName].AddRange(dataRuleConList);
            }
            var conList = query.Where(x => x.ConditionalList.Any(xx => xx.Value.FieldName.Equals(item.Key))).ToList();
            if (conList.Any())
            {
                foreach (var it in conList) it.ConditionalList.ForEach(x => x.Value.FieldName = item.Value);
                if (!childTableQuery.ContainsKey(tableName)) childTableQuery.Add(tableName, new List<IConditionalModel>());
                childTableQuery[tableName].AddRange(conList);
            }
            conList = superQuery.Where(x => x.ConditionalList.Any(xx => xx.Value.FieldName.Equals(item.Key))).ToList();
            if (conList.Any())
            {
                foreach (var it in conList) it.ConditionalList.ForEach(x => x.Value.FieldName = item.Value);
                if (!childTableQuery.ContainsKey(tableName)) childTableQuery.Add(tableName, new List<IConditionalModel>());
                childTableQuery[tableName].AddRange(conList);
            }
        }

        foreach (var item in childTableList)
        {
            item.Value.Add(relationField[item.Key]);
            var sql = string.Format("select {0} from {1} where {2} in('{3}')", string.Join(",", item.Value), item.Key, relationField[item.Key], string.Join("','", ids));
            if (childTableQuery.ContainsKey(item.Key)) // 子表查询条件
            {
                var itemWhere = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>("@").Where(childTableQuery[item.Key]).ToSqlString();
                if (itemWhere.Contains("WHERE")) sql = string.Format(" {0} and {1} ", sql, itemWhere.Split("WHERE").Last());
            }

            // 拼接数据权限
            var dataPermissionsList = new List<IConditionalModel>();
            if (dataPermissions != null && dataPermissions.Any())
            {
                var pList = new List<object>();
                var allPersissions = dataPermissions.ToObject<List<object>>();
                allPersissions.ForEach(it =>
                {
                    if (it.ToJsonString().Contains(item.Key + ".")) pList.Add(it);
                });
                if (pList.Any())
                {
                    dataPermissionsList = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pList.ToJsonString());
                    dataPermissionsList = GetIConditionalModelListByTableName(dataPermissionsList, item.Key);
                    var json = dataPermissionsList.ToJsonString().Replace(item.Key + ".", string.Empty);
                    dataPermissionsList = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(json);
                }
            }

            // 数据过滤
            var dataRuleConditionalList = new List<IConditionalModel>();
            if (dataRule != null && dataRule.Any())
            {
                var pList = new List<object>();
                var allPersissions = dataRule.ToObject<List<object>>();
                allPersissions.ForEach(it =>
                {
                    if (it.ToJsonString().Contains(item.Key + ".")) pList.Add(it);
                });
                if (pList.Any())
                {
                    dataRuleConditionalList = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(pList.ToJsonString());
                    dataRuleConditionalList = GetIConditionalModelListByTableName(dataRuleConditionalList, item.Key);
                    var json = dataRuleConditionalList.ToJsonString().Replace(item.Key + ".", string.Empty);
                    dataRuleConditionalList = _visualDevRepository.AsSugarClient().Utilities.JsonToConditionalModels(json);
                }
            }

            sql = _visualDevRepository.AsSugarClient().SqlQueryable<dynamic>(sql).Where(dataPermissionsList).Where(dataRuleConditionalList).ToSqlString();

            var dt = _databaseService.GetInterFaceData(templateInfo.DbLink, sql).ToObject<List<Dictionary<string, object>>>();
            var vModel = templateInfo.AllFieldsModel.Find(x => x.__config__.tableName == item.Key)?.__vModel__;

            if (vModel.IsNotEmptyOrNull())
            {
                foreach (var it in result.list)
                {
                    var rows = dt.Where(x => x[relationField[item.Key]].ToString().Equals(it[primaryKey].ToString())).ToList();
                    var childTableModel = templateInfo.ChildTableFieldsModelList.First(x => x.__vModel__.Equals(vModel));

                    var datas = new List<Dictionary<string, object>>();
                    if (childTableModel.__config__.children.Any(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()))
                        datas = (await _formDataParsing.GetKeyData(childTableModel.__config__.children.Where(x => x.__config__.templateJson != null && x.__config__.templateJson.Any()).ToList(), rows, templateInfo.ColumnData, "List", templateInfo.WebType, primaryKey, templateInfo.visualDevEntity.isShortLink));
                    datas = await _formDataParsing.GetKeyData(childTableModel.__config__.children.Where(x => x.__config__.templateJson == null || !x.__config__.templateJson.Any()).ToList(), rows, templateInfo.ColumnData, "List", templateInfo.WebType, primaryKey, templateInfo.visualDevEntity.isShortLink);
                    var newDatas = datas.Copy();
                    newDatas.ForEach(x => x.Remove(relationField[item.Key]));
                    it.Add(vModel, newDatas);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取处理子表数据.
    /// </summary>
    /// <param name="templateInfo">模板信息.</param>
    /// <param name="link">数据库连接.</param>
    /// <param name="dataMap">全部数据.</param>
    /// <param name="newDataMap">新数据.</param>
    /// <param name="isDetail">是否详情转换.</param>
    /// <returns></returns>
    private async Task<Dictionary<string, object>> GetChildTableData(TemplateParsingBase templateInfo, DbLinkEntity? link, Dictionary<string, object> dataMap, Dictionary<string, object> newDataMap, bool isDetail = false)
    {
        foreach (var model in templateInfo.ChildTableFieldsModelList)
        {
            if (!string.IsNullOrEmpty(model.__vModel__))
            {
                if (model.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
                {
                    List<string> feilds = new List<string>();
                    var ctPrimaryKey = templateInfo.AllTable.Find(x => x.table.Equals(model.__config__.tableName)).fields.Find(x => x.PrimaryKey.Equals(1)).Field;
                    feilds.Add(ctPrimaryKey + " id "); // 子表主键
                    foreach (FieldsModel? childModel in model.__config__.children) if (!string.IsNullOrEmpty(childModel.__vModel__)) feilds.Add(childModel.__vModel__); // 拼接查询字段
                    string relationMainFeildValue = string.Empty;
                    string childSql = string.Format("select {0} from {1} where 1=1 ", string.Join(",", feilds), model.__config__.tableName); // 查询子表数据
                    foreach (Engine.Model.TableModel? tableMap in templateInfo.AllTable.Where(x => !x.table.Equals(templateInfo.MainTableName)).ToList())
                    {
                        if (tableMap.table.Equals(model.__config__.tableName))
                        {
                            if (dataMap.ContainsKey(tableMap.relationField)) childSql += string.Format(" And {0}='{1}'", tableMap.tableField, dataMap[tableMap.relationField]); // 外键
                            if (dataMap.ContainsKey(tableMap.relationField.ToUpper())) childSql += string.Format(" And {0}='{1}'", tableMap.tableField, dataMap[tableMap.relationField.ToUpper()]); // 外键
                            if (dataMap.ContainsKey(tableMap.relationField.ToLower())) childSql += string.Format(" And {0}='{1}'", tableMap.tableField, dataMap[tableMap.relationField.ToLower()]); // 外键
                            List<Dictionary<string, object>>? childTableData = _databaseService.GetInterFaceData(link, childSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();
                            if (!isDetail)
                            {
                                List<Dictionary<string, object>>? childData = _databaseService.GetInterFaceData(link, childSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();
                                childTableData = _formDataParsing.GetTableDataInfo(childData, model.__config__.children, "detail");
                            }

                            #region 获取关联表单属性 和 弹窗选择属性
                            foreach (var item in model.__config__.children.Where(x => x.__config__.poxiaoKey == PoxiaoKeyConst.RELATIONFORM).ToList())
                            {
                                foreach (var dataItem in childTableData)
                                {
                                    if (item.__vModel__.IsNotEmptyOrNull() && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                    {
                                        var relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id
                                        var relationInfo = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == item.modelId); // 获取 关联表单 转换后的数据
                                        var relationValueStr = string.Empty;
                                        relationValueStr = await GetHaveTableInfoDetails(relationValueId, relationInfo);

                                        if (!relationValueStr.IsNullOrEmpty() && !relationValueStr.Equals(relationValueId))
                                        {
                                            var relationValue = relationValueStr.ToObject<Dictionary<string, object>>();

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_poxiaoTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                if (relationValue.ContainsKey(citem.showField)) dataItem[item.__vModel__ + "_" + citem.showField] = relationValue[citem.showField];
                                                else dataItem[item.__vModel__ + "_" + citem.showField] = string.Empty;
                                            });
                                        }
                                    }
                                }
                            }

                            if (model.__config__.children.Where(x => x.__config__.poxiaoKey == PoxiaoKeyConst.POPUPATTR).Any())
                            {
                                foreach (var item in model.__config__.children.Where(x => x.__config__.poxiaoKey == PoxiaoKeyConst.POPUPSELECT).ToList())
                                {
                                    var pDataList = await _formDataParsing.GetPopupSelectDataList(item.interfaceId, item); // 获取接口数据列表
                                    foreach (var dataItem in childTableData)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.__vModel__) && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                        {
                                            var relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_poxiaoTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                var value = pDataList.Where(x => x.Values.Contains(dataItem[item.__vModel__].ToString())).FirstOrDefault();
                                                if (value != null && value.ContainsKey(citem.showField)) dataItem[item.__vModel__ + "_" + citem.showField] = value[citem.showField];
                                            });
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (childTableData.Count > 0) newDataMap[model.__vModel__] = childTableData;
                            else newDataMap[model.__vModel__] = new List<Dictionary<string, object>>();
                        }
                    }
                }
            }
        }

        return newDataMap;
    }

    /// <summary>
    /// 处理并发锁定(乐观锁).
    /// </summary>
    /// <param name="link">数据库连接.</param>
    /// <param name="templateInfo">模板信息.</param>
    /// <param name="updateSqlList">修改Sql集合(提交修改时接入).</param>
    /// <param name="allDataMap">前端提交的数据(提交修改时接入).</param>
    private async Task OptimisticLocking(DbLinkEntity? link, TemplateParsingBase templateInfo, List<string>? updateSqlList = null, Dictionary<string, object>? allDataMap = null)
    {
        if (templateInfo.FormModel.concurrencyLock)
        {
            try
            {
                // 主表修改语句, 如果有修改语句 获取执行结果.
                // 不是修改模式, 增加并发锁定字段 f_version.
                if (updateSqlList != null && updateSqlList.Any())
                {
                    var mainTableUpdateSql = updateSqlList.Find(x => x.Contains(templateInfo.MainTableName));
                    var versoin = (allDataMap.ContainsKey("f_version") && allDataMap["f_version"] != null) ? allDataMap["f_version"] : "-1";

                    // 并发乐观锁 字段 拼接条件
                    mainTableUpdateSql = string.Format("{0} and F_Version={1};", mainTableUpdateSql.Replace(";", string.Empty), versoin);
                    var res = await _databaseService.ExecuteSql(link, mainTableUpdateSql);
                    if (res.Equals(0) && !allDataMap.ContainsKey("poxiao_resurgence")) throw Oops.Oh(ErrorCode.D1408); // 该条数据已经被修改过

                    // f_version +1
                    string? sql = string.Format("update {0} set {1}={2};", templateInfo.MainTableName, "F_Version", versoin.ParseToInt() + 1);
                    await _databaseService.ExecuteSql(link, sql);
                }
                else
                {
                    List<DbTableFieldModel>? fieldList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表所有列

                    if (!fieldList.Any(x => SqlFunc.ToLower(x.field) == "f_version"))
                    {
                        List<DbTableFieldModel>? newField = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "F_Version", fieldName = "并发锁定字段", dataType = "int", dataLength = "50", allowNull = 1 } };
                        _databaseService.AddTableColumn(link, templateInfo.MainTableName, newField);
                    }

                    // f_version 赋予默认值 0
                    string? sql = string.Format("update {0} set {1}={2} where F_Version IS NULL ;", templateInfo.MainTableName, "F_Version", "0");
                    await _databaseService.ExecuteSql(link, sql);

                    var newVModel = new FieldsModel() { __vModel__ = "f_version", __config__ = new ConfigModel() { poxiaoKey = PoxiaoKeyConst.COMINPUT, relationTable = templateInfo.MainTableName, tableName = templateInfo.MainTableName } };
                    templateInfo.SingleFormData.Add(newVModel);
                    templateInfo.MainTableFieldsModelList.Add(newVModel);
                    templateInfo.FieldsModelList.Add(newVModel);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("[D1408]")) throw Oops.Oh(ErrorCode.D1408);
                else throw Oops.Oh(ErrorCode.COM1008);
            }
        }
    }

    /// <summary>
    /// 数据是否可以传递.
    /// </summary>
    /// <param name="oldModel">原控件模型.</param>
    /// <param name="newModel">新控件模型.</param>
    /// <returns>true 可以传递, false 不可以</returns>
    private bool DataTransferVerify(FieldsModel oldModel, FieldsModel newModel)
    {
        switch (oldModel.__config__.poxiaoKey)
        {
            case PoxiaoKeyConst.COMINPUT:
            case PoxiaoKeyConst.TEXTAREA:
            case PoxiaoKeyConst.RADIO:
            case PoxiaoKeyConst.EDITOR:
                if (!(newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) ||
                    newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TEXTAREA) ||
                    newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.RADIO) ||
                    (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && !newModel.multiple) ||
                    newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.EDITOR)))
                    return false;
                break;
            case PoxiaoKeyConst.CHECKBOX:
                if (!((newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPTABLESELECT) && newModel.multiple) ||
                    (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && newModel.multiple) ||
                    (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) && newModel.multiple) ||
                    newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CHECKBOX) ||
                    newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER)))
                    return false;
                break;
            case PoxiaoKeyConst.NUMINPUT:
            case PoxiaoKeyConst.DATE:
            case PoxiaoKeyConst.TIME:
            case PoxiaoKeyConst.UPLOADFZ:
            case PoxiaoKeyConst.UPLOADIMG:
            case PoxiaoKeyConst.COLORPICKER:
            case PoxiaoKeyConst.RATE:
            case PoxiaoKeyConst.SLIDER:
                if (!(oldModel.__config__.poxiaoKey.Equals(newModel.__config__.poxiaoKey)))
                    return false;
                break;
            case PoxiaoKeyConst.COMSELECT:
            case PoxiaoKeyConst.DEPSELECT:
            case PoxiaoKeyConst.POSSELECT:
            case PoxiaoKeyConst.USERSELECT:
            case PoxiaoKeyConst.ROLESELECT:
            case PoxiaoKeyConst.GROUPSELECT:
            case PoxiaoKeyConst.ADDRESS:
                if (!(oldModel.__config__.poxiaoKey.Equals(newModel.__config__.poxiaoKey) && oldModel.multiple.Equals(newModel.multiple)))
                    return false;
                break;
            case PoxiaoKeyConst.TREESELECT:
                if (oldModel.multiple)
                {
                    if (!((newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPTABLESELECT) && newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) && newModel.multiple) ||
                        newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER)))
                        return false;
                }
                else
                {
                    if (!(newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.COMINPUT) ||
                        newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TEXTAREA) ||
                        newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.RADIO) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && !newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) && !newModel.multiple) ||
                        newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.EDITOR)))
                        return false;
                }

                break;
            case PoxiaoKeyConst.POPUPTABLESELECT:
                if (oldModel.multiple)
                {
                    if (!((newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPTABLESELECT) && newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.SELECT) && newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.TREESELECT) && newModel.multiple) ||
                        newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER)))
                        return false;
                }
                else
                {
                    if (!((newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPTABLESELECT) && !newModel.multiple) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM)) ||
                        (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPSELECT))))
                        return false;
                }

                break;
            case PoxiaoKeyConst.POPUPSELECT:
            case PoxiaoKeyConst.RELATIONFORM:
                if (!((newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM)) ||
                    (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPSELECT)) ||
                    (newModel.__config__.poxiaoKey.Equals(PoxiaoKeyConst.POPUPTABLESELECT) && !newModel.multiple)))
                    return false;
                break;
        }

        return true;
    }

    /// <summary>
    /// 处理数据视图.
    /// </summary>
    /// <param name="templateInfo"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task<PageResult<Dictionary<string, object>>> GetDataViewResults(TemplateParsingBase templateInfo, VisualDevModelListQueryInput input)
    {
        var searchList = _userManager.UserOrigin.Equals("pc") ? templateInfo.ColumnData.searchList.Copy() : templateInfo.AppColumnData.searchList.Copy();
        PageResult<Dictionary<string, object>>? realList = new PageResult<Dictionary<string, object>>() { list = new List<Dictionary<string, object>>() }; // 返回结果集
        var par = input.Adapt<VisualDevDataFieldDataListInput>();
        par.paramList = templateInfo.visualDevEntity.InterfaceParam.ToObject<List<DataInterfaceReqParameterInfo>>();
        if (par.QueryJson.IsNotEmptyOrNull())
        {
            var querList = par.QueryJson.ToObject<Dictionary<string, string>>();
            var newList = new Dictionary<string, string>();
            foreach (var item in querList)
            {
                var key = item.Key;
                var vModel = searchList.Find(x => x.__vModel__.Equals(item.Key));
                if (vModel != null && vModel.searchType.Equals(1)) { key = "poxiao_searchType_equals_" + item.Key; }
                newList.Add(key, item.Value.Replace("'", "''"));
            }

            par.QueryJson = newList.ToJsonString();
        }

        var dataInterface = await _visualDevRepository.AsSugarClient().Queryable<DataInterfaceEntity>().FirstAsync(x => x.Id == templateInfo.visualDevEntity.InterfaceId && x.DeleteMark == null);
        if (templateInfo.ColumnData.hasPage && dataInterface.DataType.Equals(2)) par.PageSize = 999999;
        var res = await _dataInterfaceService.GetResponseByType(templateInfo.visualDevEntity.InterfaceId, 2, string.Empty, par);
        if (templateInfo.ColumnData.hasPage && !dataInterface.DataType.Equals(2))
        {
            if (!res.ToJsonString().Equals("[]") && res.ToJsonString() != string.Empty)
                realList = res.ToObject<PageResult<Dictionary<string, object>>>();
        }
        else
        {
            var resList = res.ToObject<List<Dictionary<string, object>>>();
            if (par.QueryJson.IsNotEmptyOrNull())
            {
                var querList = par.QueryJson.ToObject<Dictionary<string, string>>();
                foreach (var item in querList)
                {
                    Console.WriteLine(item.Value);
                    Console.WriteLine(item.Key.Replace("poxiao_searchType_equals_", ""));
                    if (item.Key.Contains("poxiao_searchType_equals_")) resList = resList.Where(x => x[item.Key.Replace("poxiao_searchType_equals_", "")].ToString().Equals(item.Value)).ToList();
                    else resList = resList.Where(x => x[item.Key].ToString().Contains(item.Value)).ToList();
                }
            }
            if (input.Sidx.IsNotEmptyOrNull())
            {
                if (input.Sort.Equals("desc")) resList = resList.OrderBy(x => x[input.Sidx]).ToList();
                else resList = resList.OrderByDescending(x => x[input.Sidx]).ToList();
            }
            var dt = GetPageToDataTable(resList, input.CurrentPage, input.PageSize);
            realList.pagination = new PageInfo()
            {
                currentPage = input.CurrentPage,
                pageSize = input.PageSize,
                total = resList.Count
            };
            realList.list = dt.ToObject<List<Dictionary<string, object>>>();
        }

        for (int i = 0; i < realList.list.Count; i++) if (!realList.list[i].ContainsKey("id")) realList.list[i].Add("id", i);

        if (input.QueryJson.IsNotEmptyOrNull())
        {
            var queryInfo = input.QueryJson.ToObject<Dictionary<string, string>>();
            foreach (var item in queryInfo)
            {
                var searchInfo = searchList.Find(x => x.__vModel__.Equals(item.Key));
                if (searchInfo == null || !searchInfo.searchType.Equals(1))
                    realList.list = realList.list.Where(x => x.Any(xx => xx.Key.Equals(item.Key) && xx.Value.IsNotEmptyOrNull() && xx.Value.ToString().Contains(item.Value))).ToList();
                else
                    realList.list = realList.list.Where(x => x.Any(xx => xx.Key.Equals(item.Key) && xx.Value.IsNotEmptyOrNull() && xx.Value.ToString().Equals(item.Value))).ToList();
            }
			
            if (dataInterface.CheckType.Equals(1))
                realList.pagination = new PageInfo() { currentPage = 1, pageSize = input.PageSize, total = realList.list.Count };
            else
                realList.pagination = new PageInfo() { currentPage = input.CurrentPage, pageSize = input.PageSize, total = realList.pagination.total != null ? realList.pagination.total : realList.list.Count };

        }
        if (input.Sidx.IsNotEmptyOrNull())
        {
            if (input.Sort.Equals("desc")) realList.list = realList.list.OrderByDescending(x => x[input.Sidx]).ToList();
            else realList.list = realList.list.OrderBy(x => x[input.Sidx]).ToList();
        }

        // 分组表格
        if (templateInfo.ColumnData.type == 3 && _userManager.UserOrigin == "pc")
            realList.list = CodeGenHelper.GetGroupList(realList.list, templateInfo.ColumnData.groupField, templateInfo.ColumnData.columnList.Find(x => x.__vModel__.ToLower() != templateInfo.ColumnData.groupField.ToLower()).__vModel__);

        return realList;
    }

    /// <summary>
    /// 静态数据分页.
    /// </summary>
    /// <param name="dt">数据源.</param>
    /// <param name="PageIndex">第几页.</param>
    /// <param name="PageSize">每页多少条.</param>
    /// <returns></returns>
    private List<Dictionary<string, object>> GetPageToDataTable(List<Dictionary<string, object>> dt, int PageIndex, int PageSize)
    {
        if (PageIndex == 0) return dt; // 0页代表每页数据，直接返回
        if (dt == null) return new List<Dictionary<string, object>>();
        var newdt = new List<Dictionary<string, object>>();
        int rowbegin = (PageIndex - 1) * PageSize;
        int rowend = PageIndex * PageSize; // 要展示的数据条数
        if (rowbegin >= dt.Count) return dt; // 源数据记录数小于等于要显示的记录，直接返回dt
        if (rowend > dt.Count) rowend = dt.Count;
        for (int i = rowbegin; i <= rowend - 1; i++) newdt.Add(dt[i]);
        return newdt;
    }

    #endregion
}