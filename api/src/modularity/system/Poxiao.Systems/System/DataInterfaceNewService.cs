using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extras.Thirdparty.JSEngine;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Dtos.OAuth;
using Poxiao.Infrastructure.Dtos.VisualDev;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using Poxiao.LinqBuilder;
using Poxiao.Logging.Attributes;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Dto.DataInterFace;
using Poxiao.Systems.Entitys.Dto.System.DataInterFace;
using Poxiao.Systems.Entitys.Model.DataInterFace;
using Poxiao.Systems.Entitys.Model.System.DataInterFace;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.UnifyResult;
using SqlSugar;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Poxiao.Systems;

/// <summary>
/// 数据接口
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataInterface", Order = 204)]
[Route("api/system/[controller]")]
public class DataInterfaceNewService : IDataInterfaceService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DataInterfaceEntity> _repository;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化 SqlSugar 客户端.
    /// </summary>
    private readonly SqlSugarScope _sqlSugarClient;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 数据库上下文ID.
    /// </summary>
    private string _configId = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default")?.ConfigId?.ToString() ?? "default";

    /// <summary>
    /// 数据库名称.
    /// </summary>
    private string _dbName = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default")?.DBName ?? "";

    private readonly DataInterfaceService _dataInterfaceService;

    private int _currentPage = 1;
    private int _pageSize = 20;
    private string _keyword = string.Empty;
    private string _showKey = string.Empty;
    private string _showValue = string.Empty;

    /// <summary>
    /// 初始化一个<see cref="DataInterfaceService"/>类型的新实例.
    /// </summary>
    public DataInterfaceNewService(
        ISqlSugarRepository<DataInterfaceEntity> repository,
        IDataBaseManager dataBaseManager,
        IUserManager userManager,
        ICacheManager cacheManager,
        IFileManager fileManager,
        DataInterfaceService dataInterfaceService,
        ISqlSugarClient context)
    {
        _repository = repository;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _cacheManager = cacheManager;
        _dataInterfaceService = dataInterfaceService;
        _sqlSugarClient = (SqlSugarScope)context;
    }

    #region Get
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceEntity>()
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(a => new DataInterfaceListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                dataType = a.DataType,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                sortCode = a.SortCode,
                enabledMark = a.EnabledMark,
                tenantId = _userManager.TenantId
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<DataInterfaceListOutput>.SqlSugarPageResult(list);
    }

    [HttpGet("getList")]
    public async Task<dynamic> GetListByDataType([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceEntity>()
            .Where(a => a.DeleteMark == null && a.EnabledMark == 1)
            .WhereIF(input.hasPage == 0, a => a.CheckType == input.hasPage)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.dataType), a => a.DataType.ToString() == input.dataType)
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(a => new DateInterfaceGetListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                DataType = a.DataType,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                sortCode = a.SortCode,
                enabledMark = a.EnabledMark,
                tenantId = _userManager.TenantId,
                requestParameters = a.RequestParameters,
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<DateInterfaceGetListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取接口列表下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<DataInterfaceSelectorOutput> tree = new List<DataInterfaceSelectorOutput>();
        foreach (var entity in await _repository.AsQueryable().Where(x => x.DeleteMark == null && x.EnabledMark == 1 && x.CheckType == 0).OrderBy(x => x.SortCode).ToListAsync())
        {
            var dictionaryDataEntity = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().FirstAsync(x => x.Id == entity.CategoryId && x.DeleteMark == null);
            if (dictionaryDataEntity != null && tree.Where(t => t.Id == entity.CategoryId).Count() == 0)
            {
                DataInterfaceSelectorOutput firstModel = dictionaryDataEntity.Adapt<DataInterfaceSelectorOutput>();
                firstModel.categoryId = "0";
                DataInterfaceSelectorOutput treeModel = entity.Adapt<DataInterfaceSelectorOutput>();
                treeModel.categoryId = "1";
                treeModel.ParentId = dictionaryDataEntity.Id;
                firstModel.Children.Add(treeModel);
                tree.Add(firstModel);
            }
            else
            {
                DataInterfaceSelectorOutput treeModel = entity.Adapt<DataInterfaceSelectorOutput>();
                treeModel.categoryId = "1";
                treeModel.ParentId = entity.CategoryId;
                var parent = tree.Where(t => t.Id == entity.CategoryId).FirstOrDefault();
                if (parent != null)
                {
                    parent.Children.Add(treeModel);
                }
            }
        }

        return tree.OrderBy(x => x.sortCode).ToList();
    }

    /// <summary>
    /// 获取接口数据.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfoApi(string id)
    {
        return (await GetInfo(id)).Adapt<DataInterfaceInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var data = await GetInfo(id);
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.FullName, ExportFileType.bd);
    }

    /// <summary>
    /// 获取预览参数.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("GetParam/{id}")]
    [UnitOfWork]
    public async Task<dynamic> GetParam(string id)
    {
        var info = await GetInfo(id);
        if (info.IsNotEmptyOrNull() && info.RequestParameters.IsNotEmptyOrNull())
        {
            return info.RequestParameters.ToList<DataInterfaceReqParameter>();
        }
        else
        {
            return new List<DataInterfaceReqParameter>();
        }
    }
    #endregion

    #region Post

    /// <summary>
    /// 预览接口.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Preview")]
    [UnitOfWork]
    public async Task<dynamic> Preview(string id, [FromBody] DataInterfacePreviewInput input)
    {
        _configId = _userManager.TenantId;
        _dbName = _userManager.TenantDbName;
        var dicParameters = input.paramList != null && input.paramList.Any() ? input.paramList.ToDictionary(x => x.field, y => y.defaultValue) : new Dictionary<string, string>();
        return await GetDataInterfaceData(id, dicParameters);
    }

    /// <summary>
    /// 访问接口 选中 回写.
    /// </summary>
    /// <returns></returns>
    [IgnoreLog]
    [HttpPost("{id}/Action/InfoByIds")]
    public async Task<dynamic> ActionsResponseInfo(string id, [FromBody] VisualDevDataFieldDataListInput input)
    {
        _configId = _userManager.TenantId;
        _dbName = _userManager.TenantDbName;
        if (await _repository.IsAnyAsync(x => x.Id == id && x.CheckType == 0))
        {
            return await _dataInterfaceService.ActionsResponseInfoNew(id, input);
        }
        else
        {
            var dicParameters = input.paramList != null && input.paramList.Any() ? input.paramList.ToDictionary(x => x.field, y => y.defaultValue) : new Dictionary<string, string>();
            var output = new List<object>();
            _keyword = input.Keyword;
            _showKey = input.propsValue;
            dicParameters.Add("@showKey", input.propsValue);
            foreach (var item in input.ids)
            {
                _showValue = item;
                dicParameters["@showValue"] = item;
                var data = await GetDataInterfaceData(id, dicParameters, true);
                output.Add(data);
            }
            return output;
        }
    }

    /// <summary>
    /// 访问接口 分页.
    /// </summary>
    /// <returns></returns>
    [IgnoreLog]
    [HttpPost("{id}/Action/List")]
    public async Task<dynamic> ActionsResponseList(string id, [FromBody] VisualDevDataFieldDataListInput input)
    {
        _configId = _userManager.TenantId;
        _dbName = _userManager.TenantDbName;
        if (await _repository.IsAnyAsync(x => x.Id == id && x.CheckType == 0))
        {
            return await _dataInterfaceService.ActionsResponseList(id, input);
        }
        else
        {
            _currentPage = input.CurrentPage;
            _pageSize = input.PageSize;
            _keyword = input.Keyword;
            var dicParameters = input.paramList != null && input.paramList.Any() ? input.paramList.ToDictionary(x => x.field, y => y.defaultValue) : new Dictionary<string, string>();
            return await GetDataInterfaceData(id, dicParameters);
        }
    }

    /// <summary>
    /// 外部访问接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tenantId">有值则为地址请求，没有则是内部请求.</param>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpPost("{id}/Actions/Response")]
    [UnitOfWork]
    public async Task<dynamic> ActionsResponse(string id, [FromQuery] string tenantId, [FromBody] Dictionary<string, string> dic)
    {
        return await InterfaceVerify(id, tenantId, dic);
    }

    /// <summary>
    /// 添加接口.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] DataInterfaceCrInput input)
    {
        if (await _repository.IsAnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<DataInterfaceEntity>();
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] DataInterfaceUpInput input)
    {
        if (await _repository.IsAnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<DataInterfaceEntity>();
        var isOk = await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.AsUpdateable().SetColumns(it => new DataInterfaceEntity()
        {
            DeleteMark = 1,
            DeleteTime = DateTime.Now,
            DeleteUserId = _userManager.UserId
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 更新接口状态.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        var isOk = await _repository.AsUpdateable().SetColumns(it => new DataInterfaceEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyTime = DateTime.Now,
            LastModifyUserId = _userManager.UserId
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Action/Import")]
    public async Task ActionsImport(IFormFile file)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.bd.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var data = josn.ToObject<DataInterfaceEntity>();
        if (data == null)
            throw Oops.Oh(ErrorCode.D3006);
        var isOk = await _repository.AsSugarClient().Storageable(data).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.D3008);
    }

    /// <summary>
    /// 外部接口授权码.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="intefaceId"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpPost("Actions/GetAuth")]
    public async Task<dynamic> GetAuthorization([FromQuery] string appId, [FromQuery] string tenantId, [FromQuery] string intefaceId, [FromBody] Dictionary<string, string> dic)
    {
        await ChangeTenantDB(tenantId);
        var interfaceOauthEntity = await _sqlSugarClient.Queryable<InterfaceOauthEntity>().FirstAsync(x => x.AppId == appId && x.DeleteMark == null && x.EnabledMark == 1);
        if (interfaceOauthEntity == null) return null;
        var ymDate = DateTime.Now.ParseToUnixTime().ToString();
        var authorization = GetVerifySignature(interfaceOauthEntity, intefaceId, ymDate);
        return new
        {
            YmDate = ymDate,
            Authorization = authorization,
        };
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var random = RandomExtensions.NextLetterAndNumberString(new Random(), 5).ToLower();
        entity.FullName = string.Format("{0}.副本{1}", entity.FullName, random);
        entity.EnCode = string.Format("{0}{1}", entity.EnCode, random);
        if (entity.FullName.Length >= 50 || entity.EnCode.Length >= 50)
            throw Oops.Oh(ErrorCode.COM1009);
        entity.EnabledMark = 0;
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 预览接口字段.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/GetFields")]
    [UnitOfWork]
    public async Task<dynamic> GetFields(string id, [FromBody] DataInterfacePreviewInput input)
    {
        try
        {
            _configId = _userManager.TenantId;
            _dbName = _userManager.TenantDbName;
            var dicParameters = input.paramList != null && input.paramList.Any() ? input.paramList.ToDictionary(x => x.field, y => y.defaultValue) : new Dictionary<string, string>();
            var data = await GetDataInterfaceData(id, dicParameters);
            if (await _repository.IsAnyAsync(x => x.Id == id && x.CheckType == 0))
            {
                return data.ToObject<List<Dictionary<string, object>>>().FirstOrDefault().Keys.ToList();
            }
            else
            {
                var result = data.ToObject<Dictionary<string, object>>();
                if (result.ContainsKey("list"))
                {
                    return result["list"].ToObject<List<Dictionary<string, object>>>().FirstOrDefault().Keys.ToList();
                }
                else
                {
                    throw Oops.Oh(ErrorCode.IO0005);
                }
            }
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.IO0005);
        }
    }
    #endregion

    #region Public

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<DataInterfaceEntity> GetInfo(string id)
    {
        return await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 获取数据接口数据.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dicParameters"></param>
    /// <param name="isEcho"></param>
    /// <returns></returns>
    public async Task<object> GetDataInterfaceData(string id, Dictionary<string, string> dicParameters, bool isEcho = false)
    {
        object output = null;
        var info = await GetInfo(id);
        VerifyRequired(info, dicParameters);
        ReplaceParameterValue(info, dicParameters);
        if (info.DataType == 1)
        {
            output = await GetSqlData(info, isEcho);
        }
        else if (info.DataType == 2)
        {
            output = info.Query.ToObject<object>();
        }
        else
        {
            output = await GetApiData(info, isEcho);
        }

        if (info.DataProcessing.IsNotEmptyOrNull() && output.IsNotEmptyOrNull())
        {
            string sheetData = Regex.Match(info.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
            var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
            output = JsEngineUtil.CallFunction(scriptStr, output.ToJsonString(CommonConst.options)); //此处时间非时间戳
        }
        return output;
    }

    /// <summary>
    /// 执行sql.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isEcho"></param>
    /// <returns></returns>
    public async Task<object> GetSqlData(DataInterfaceEntity entity, bool isEcho = false)
    {
        var link = new DbLinkEntity();
        if (!_sqlSugarClient.AsTenant().IsAnyConnection(_configId))
        {
            link = await _sqlSugarClient.CopyNew().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == entity.DBLinkId && x.DeleteMark == null);
        }
        else
        {
            link = await _repository.AsSugarClient().CopyNew().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == entity.DBLinkId && x.DeleteMark == null);
        }
        var tenantLink = link ?? GetTenantDbLink();
        var dt = new DataTable();
        var parameter = new List<SugarParameter>();

        // 是否回显
        if (isEcho)
        {
            var propJson = entity.PropertyJson.ToObject<DataInterfaceProperJson>();
            parameter = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>().Select(x => new SugarParameter(string.Format("{0}", x.field), x.defaultValue)).ToList();
            propJson.echoSql = await GetSqlParameter(propJson.echoSql, parameter);
            if (entity.RequestMethod.Equals("3"))
            {
                dt = _dataBaseManager.GetInterFaceDataNew(tenantLink, propJson.echoSql, parameter.ToArray());
            }
            else
            {
                _dataBaseManager.ExecuteCommand(tenantLink, propJson.echoSql, parameter.ToArray());
            }
            return dt.ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
        }
        else
        {
            parameter = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>().Select(x => new SugarParameter("@" + x.field, x.defaultValue)).ToList();
            entity.Query = await GetSqlParameter(entity.Query, parameter);
            if (entity.RequestMethod.Equals("3"))
            {
                dt = _dataBaseManager.GetInterFaceDataNew(tenantLink, entity.Query, parameter.ToArray());
            }
            else
            {
                _dataBaseManager.ExecuteCommand(tenantLink, entity.Query, parameter.ToArray());
            }
            if (entity.CheckType == 1)
            {
                var propJson = entity.PropertyJson.ToObject<DataInterfaceProperJson>();
                propJson.countSql = await GetSqlParameter(propJson.countSql, parameter);
                var count = _dataBaseManager.GetCount(tenantLink, propJson.countSql, parameter.ToArray());
                return new { list = dt, pagination = new PageInfo() { currentPage = _currentPage, pageSize = _pageSize, total = count } };
            }
            else
            {
                return dt;
            }
        }
    }

    /// <summary>
    /// 根据不同规则请求接口(预览).
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task<JObject> GetApiData(DataInterfaceEntity entity, bool isEcho = false)
    {
        try
        {
            var result = new JObject();
            var heraderParameters = entity.RequestHeaders.ToObject<List<DataInterfaceReqParameter>>(); // 头部参数.
            var reqParameters = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>(); // 请求参数.
            var heraderDic = new Dictionary<string, object>();
            heraderDic.Add("Poxiao_API", true);
            if (_userManager.ToKen != null && !_userManager.ToKen.Contains("::"))
                heraderDic.Add("Authorization", _userManager.ToKen);
            var reqDic = new Dictionary<string, object>();
            var path = entity.Path;
            var requestMethod = entity.RequestMethod;
            if (entity.CheckType == 1)
            {
                var propJson = entity.PropertyJson.ToObject<DataInterfaceProperJson>();
                if (isEcho)
                {
                    heraderParameters = propJson.echoReqHeaders.ToObject<List<DataInterfaceReqParameter>>();
                    reqParameters = propJson.echoReqParameters.ToObject<List<DataInterfaceReqParameter>>();
                    path = propJson.echoPath.Replace("{showValue}", _showValue);
                    requestMethod = propJson.echoReqMethod;
                }
            }
            foreach (var key in reqParameters)
            {
                reqDic.Add(key.field, key.defaultValue);
            }

            foreach (var key in heraderParameters)
            {
                heraderDic[key.field] = key.defaultValue;
            }

            switch (requestMethod)
            {
                case "6":
                    result = (await path.SetHeaders(heraderDic).SetQueries(reqDic).GetAsStringAsync()).ToObject<JObject>();
                    break;
                case "7":
                    result = (await path.SetHeaders(heraderDic).SetBody(reqDic).PostAsStringAsync()).ToObject<JObject>();
                    break;
            }
            result = result.ContainsKey("data") ? result["data"].ToObject<JObject>() : result;
            if (isEcho && result is JArray)
            {
                return JArray.Parse(result.ToJsonString()).FirstOrDefault().ToObject<JObject>();
            }
            return result;
        }
        catch (Exception)
        {
            throw Oops.Oh(ErrorCode.IO0005);
        }
    }

    public async Task<DataTable> GetData(DataInterfaceEntity entity)
    {
        if (entity.CheckType == 0)
        {
            return await _dataInterfaceService.GetData(entity);
        }
        else
        {
            return (await GetSqlData(entity)).ToObject<DataTable>();
        }

    }

    public async Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null)
    {
        if (await _repository.IsAnyAsync(x => x.Id == id && x.CheckType == 0))
        {
            return await _dataInterfaceService.GetResponseByType(id, type, tenantId, input, dicParameters);
        }
        else
        {
            if (tenantId.IsNotEmptyOrNull() && tenantId == "defualt")
            {
                ChangeTenantDB(tenantId);
            }
            if (input.IsNotEmptyOrNull())
            {
                _currentPage = input.CurrentPage;
                _pageSize = input.PageSize;
                _keyword = input.Keyword.IsNotEmptyOrNull() ? input.Keyword : string.Empty;
                dicParameters = input.paramList != null && input.paramList.Any() ? input.paramList.ToDictionary(x => x.field, y => y.defaultValue) : new Dictionary<string, string>();
            }
            return await GetDataInterfaceData(id, dicParameters);
        }
    }

    /// <summary>
    /// 处理远端数据.
    /// </summary>
    /// <param name="propsUrl">远端数据ID.</param>
    /// <param name="value">指定选项标签为选项对象的某个属性值.</param>
    /// <param name="label">指定选项的值为选项对象的某个属性值.</param>
    /// <param name="children">指定选项的子选项为选项对象的某个属性值.</param>
    /// <param name="linkageParameters">联动参数.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<StaticDataModel>> GetDynamicList(string propsUrl, string value, string label, string children, List<ControlLinkageParameterModel> linkageParameters = null)
    {
        List<StaticDataModel> list = new List<StaticDataModel>();

        // 获取远端数据
        DataInterfaceEntity? dynamic = await _repository.AsQueryable().Where(x => x.Id == propsUrl && x.DeleteMark == null).FirstAsync();
        if (dynamic == null) return list;

        // 控件联动 不能缓存
        if (linkageParameters == null)
        {
            list = await GetDynamicDataCache(dynamic.Id);
        }

        if (list == null || list.Count == 0)
        {
            list = new List<StaticDataModel>();
            // 远端数据 配置参数
            List<SugarParameter>? parameter = new List<SugarParameter>();

            // 未数据处理
            var resList = string.Empty;

            // 数据处理结果
            var dataProcessingResults = string.Empty;
            // 获取数据
            switch (dynamic.DataType)
            {
                // SQL数据
                case 1:
                    {

                        DbLinkEntity? linkEntity = await _repository.AsSugarClient().Queryable<DbLinkEntity>().Where(m => m.Id == dynamic.DBLinkId && m.DeleteMark == null).FirstAsync();
                        if (linkEntity == null) linkEntity = _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                        ReplaceParameterValue(dynamic, new Dictionary<string, string>());
                        if (linkageParameters?.Count > 0)
                        {
                            linkageParameters.ForEach(item =>
                            {
                                parameter.Add(new SugarParameter(string.Format("@{0}", item.ParameterName), item.FormFieldValues));
                            });
                        }
                        var sql = await GetSqlParameter(dynamic.Query, parameter);
                        DataTable? dt = _dataBaseManager.GetInterFaceData(linkEntity, sql, parameter.ToArray());
                        resList = dt.ToJsonString();
                    }

                    break;

                // 静态数据
                case 2:
                    {
                        resList = JValue.Parse(dynamic.Query).ToJsonString();
                    }

                    break;

                // Api数据
                case 3:
                    {
                        var result = await GetApiDataByTypePreview(dynamic);
                        resList = result.ToJsonString();
                    }

                    break;
            }

            // 处理数据
            switch (dynamic.DataType)
            {
                // SQL数据
                case 1:
                // Api数据
                case 3:
                    if (!dynamic.DataProcessing.IsNullOrEmpty())
                    {
                        string sheetData = Regex.Match(dynamic.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
                        var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
                        try
                        {
                            dataProcessingResults = JsEngineUtil.CallFunction(scriptStr, resList).ToJsonString();
                        }
                        catch (Exception)
                        {
                            dataProcessingResults = string.Empty;
                        }
                    }
                    else
                    {
                        dataProcessingResults = resList;
                    }
                    break;
                // 静态数据
                case 2:
                    dataProcessingResults = resList;
                    break;
            }
            if (!dataProcessingResults.IsNullOrEmpty())
            {
                foreach (JToken? item in JToken.Parse(dataProcessingResults))
                {
                    StaticDataModel dynamicDic = new StaticDataModel()
                    {
                        id = item.Value<string>(value),
                        fullName = item.Value<string>(label)
                    };
                    list.Add(dynamicDic);

                    // 为避免子级有数据.
                    if (item.Value<object>(children) != null && item.Value<object>(children).ToString().IsNotEmptyOrNull())
                        list.AddRange(GetDynamicInfiniteData(item.Value<object>(children).ToString(), value, label, children));
                }
                await SetDynamicDataCache(dynamic.Id, list);
            }
        }
        return list;
    }

    #endregion

    #region Private

    /// <summary>
    /// 验证必填参数.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dicParams"></param>
    private void VerifyRequired(DataInterfaceEntity entity, Dictionary<string, string> dicParams)
    {
        try
        {
            if (entity.IsNotEmptyOrNull() && entity.RequestParameters.IsNotEmptyOrNull() && !entity.RequestParameters.Equals("[]"))
            {
                var reqParams = entity.RequestParameters.ToList<DataInterfaceReqParameter>();
                if (reqParams.Count > 0)
                {
                    // 必填参数
                    var requiredParams = reqParams.Where(x => x.required == "1").ToList();
                    if (requiredParams.Any() && (dicParams.IsNullOrEmpty() || dicParams.Keys.Count == 0))
                        throw Oops.Oh(ErrorCode.xg1003);
                    foreach (var item in requiredParams)
                    {
                        if (dicParams.ContainsKey(item.field))
                        {
                            switch (item.dataType)
                            {
                                case "varchar":
                                    if (dicParams[item.field].IsNullOrEmpty())
                                    {
                                        throw Oops.Oh(item.field + "不能为空");
                                    }
                                    break;
                                case "int":
                                    dicParams[item.field].ParseToInt();
                                    break;
                                case "datetime":
                                    dicParams[item.field].ParseToDateTime();
                                    break;
                                case "decimal":
                                    dicParams[item.field].ParseToDecimal();
                                    break;
                            }
                        }
                        else
                        {
                            throw Oops.Oh(item.field + "不能为空");
                        }
                    }
                }
            }
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ErrorCode.xg1003);
        }
    }

    /// <summary>
    /// 替换参数默认值.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dic"></param>
    [NonAction]
    public void ReplaceParameterValue(DataInterfaceEntity entity, Dictionary<string, string> dic)
    {
        if (dic.IsNotEmptyOrNull() && entity.IsNotEmptyOrNull() && entity.RequestParameters.IsNotEmptyOrNull())
        {
            var parameterList = entity.RequestParameters.ToList<DataInterfaceReqParameter>();
            foreach (var item in parameterList)
            {
                entity.Query = entity.Query?.Replace("{" + item.field + "}", "@" + item.field);
                if (entity.DataType == 1)
                {
                    entity.PropertyJson = entity.PropertyJson?.Replace("{" + item.field + "}", "@" + item.field);
                }
                if (dic.Keys.Contains(item.field))
                    item.defaultValue = HttpUtility.UrlDecode(dic[item.field], Encoding.UTF8); // 对参数解码
            }
            if (entity.CheckType == 1 && entity.DataType == 3)
            {
                var propertyJson = entity.PropertyJson.ToObject<DataInterfaceProperJson>();
                foreach (var item in propertyJson.pageParameters)
                {
                    if (item.fieldName.Equals("currentPage"))
                    {
                        parameterList.Add(new DataInterfaceReqParameter { field = item.field, defaultValue = _currentPage.ToString() });
                    }
                    if (item.fieldName.Equals("pageSize"))
                    {
                        parameterList.Add(new DataInterfaceReqParameter { field = item.field, defaultValue = _pageSize.ToString() });
                    }
                    if (item.fieldName.Equals("keyword"))
                    {
                        parameterList.Add(new DataInterfaceReqParameter { field = item.field, defaultValue = _keyword });
                    }
                }
                foreach (var item in propertyJson.echoReqParameters)
                {
                    if (dic.Keys.Contains(item.field))
                        item.defaultValue = HttpUtility.UrlDecode(dic[item.field], Encoding.UTF8); // 对参数解码
                }
                propertyJson.echoReqParameters.Add(new DataInterfaceReqParameter { field = propertyJson.echoParameters.FirstOrDefault(x => x.fieldName == "showKey").field, defaultValue = propertyJson.echoParameters.FirstOrDefault(x => x.fieldName == "showValue").field });
                entity.PropertyJson = propertyJson.ToJsonString();
            }
            entity.RequestParameters = parameterList.ToJsonString();
        }
    }

    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public DbLinkEntity GetTenantDbLink()
    {
        var connectionConfigs = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default");
        return new DbLinkEntity
        {
            Id = _configId,
            ServiceName = _dbName,
            DbType = connectionConfigs.DbType.ToString(),
            Host = connectionConfigs.Host,
            Port = connectionConfigs.Port,
            UserName = connectionConfigs.UserName,
            Password = connectionConfigs.Password
        };
    }

    /// <summary>
    /// 获取sql系统变量参数.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="sugarParameters"></param>
    private async Task<string> GetSqlParameter(string sql, List<SugarParameter> sugarParameters)
    {
        if (_userManager.ToKen != null)
        {
            if (sql.Contains("@currentUsersAndSubordinates") && !sugarParameters.Any(x => x.ParameterName == "@currentUsersAndSubordinates"))
            {
                var subordinates = _userManager.CurrentUserAndSubordinates;
                sugarParameters.Add(new SugarParameter("@currentUsersAndSubordinates", subordinates));
            }
            if (sql.Contains("@organization") && !sugarParameters.Any(x => x.ParameterName == "@organization"))
            {
                sugarParameters.Add(new SugarParameter("@organization", _userManager?.User?.OrganizeId));
            }
            if (sql.Contains("@currentOrganizationAndSuborganization") && !sugarParameters.Any(x => x.ParameterName == "@currentOrganizationAndSuborganization"))
            {
                var subsidiary = _userManager.CurrentOrganizationAndSubOrganizations;
                sugarParameters.Add(new SugarParameter("@currentOrganizationAndSuborganization", subsidiary));
            }
            if (sql.Contains("@chargeorganization") && !sugarParameters.Any(x => x.ParameterName == "@chargeorganization"))
            {
                var chargeorganization = _userManager.DataScope.Where(x => x.organizeId == _userManager?.User?.OrganizeId && x.Select).ToList().FirstOrDefault();
                sugarParameters.Add(new SugarParameter("@chargeorganization", chargeorganization?.organizeId));
            }
            if (sql.Contains("@currentChargeorganizationAndSuborganization") && !sugarParameters.Any(x => x.ParameterName == "@currentChargeorganizationAndSuborganization"))
            {
                var subsidiary = _userManager.DataScope.Select(x => x.organizeId).Intersect(_userManager.CurrentUserSubOrganization).ToList();
                sugarParameters.Add(new SugarParameter("@currentChargeorganizationAndSuborganization", subsidiary));
            }
            if (sql.Contains("@offsetSize") && !sugarParameters.Any(x => x.ParameterName == "@offsetSize"))
            {
                sugarParameters.Add(new SugarParameter("@offsetSize", (_currentPage - 1) * _pageSize));
            }
            if (sql.Contains("@pageSize") && !sugarParameters.Any(x => x.ParameterName == "@pageSize"))
            {
                sugarParameters.Add(new SugarParameter("@pageSize", _pageSize));
            }
            if (sql.Contains("@showValue") && !sugarParameters.Any(x => x.ParameterName == "@showValue"))
            {
                sugarParameters.Add(new SugarParameter("@showValue", _showValue));
            }
            if (sql.Contains("@keyword") && !sugarParameters.Any(x => x.ParameterName == "@keyword"))
            {
                sugarParameters.Add(new SugarParameter("@keyword", _keyword));
            }
            if (sql.Contains("@showKey"))
            {
                sql = sql.Replace("@showKey", _showKey);
            }
            if (sql.Contains("@user"))
            {
                sql = sql.Replace("@user", "'" + _userManager.UserId + "'"); // orcale关键字处理
            }
        }
        return sql;
    }

    /// <summary>
    /// 外部接口验证并请求.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tenantId"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    private async Task<dynamic> InterfaceVerify(string id, string tenantId, Dictionary<string, string> dic)
    {
        UserAgent userAgent = new UserAgent(App.HttpContext);
        await ChangeTenantDB(tenantId);
        var appId = await VerifyInterfaceOauth(id);

        var sw = new Stopwatch();
        sw.Start();
        object output = await GetDataInterfaceData(id, dic);
        sw.Stop();

        #region 插入日志

        if (App.HttpContext.IsNotEmptyOrNull())
        {
            var httpContext = App.HttpContext;
            var headers = httpContext.Request.Headers;
            var log = new DataInterfaceLogEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                OauthAppId = appId,
                InvokId = id,
                InvokTime = DateTime.Now,
                InvokIp = httpContext.GetLocalIpAddressToIPv4(),
                InvokDevice = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue),
                InvokWasteTime = (int)sw.ElapsedMilliseconds,
                InvokType = httpContext.Request.Method
            };
            await _sqlSugarClient.Insertable(log).ExecuteCommandAsync();
        }
        #endregion

        return output;
    }

    /// <summary>
    /// HMACSHA256加密.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="interfaceId"></param>
    /// <param name="ymDate"></param>
    /// <returns></returns>
    private string GetVerifySignature(InterfaceOauthEntity entity, string interfaceId, string ymDate)
    {
        string secret = entity.AppSecret;
        string method = "POST";
        string urlPath = string.Format("/dev/api/system/DataInterface/{0}/Actions/Response", interfaceId);
        string YmDate = ymDate;
        string host = App.HttpContext.Request.Host.ToString();
        string source = new StringBuilder().Append(method).Append('\n').Append(urlPath).Append('\n')
                .Append(YmDate).Append('\n').Append(host).ToString();
        using (var hmac = new HMACSHA256(secret.ToBase64String().ToBytes()))
        {
            byte[] hashmessage = hmac.ComputeHash(source.ToBytes(Encoding.UTF8));
            var signature = hashmessage.ToHexString();
            return entity.AppId + "::" + signature;
        }
    }

    /// <summary>
    /// 切换租户数据库.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    private async Task ChangeTenantDB(string tenantId)
    {
        if (KeyVariable.MultiTenancy)
        {
            tenantId = tenantId.IsNullOrEmpty() ? _userManager.TenantId : tenantId;
            var interFace = App.Configuration["Tenant:MultiTenancyDBInterFace"] + tenantId;
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
                throw Oops.Oh(result.msg);
            else if (result.data.dotnet == null)
                throw Oops.Oh(ErrorCode.D1025);
            if (!_sqlSugarClient.IsAnyConnection(tenantId))
            {
                _sqlSugarClient.AddConnection(new ConnectionConfig()
                {
                    DbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), App.Configuration["ConnectionStrings:DBType"]),
                    ConfigId = tenantId, // 设置库的唯一标识
                    IsAutoCloseConnection = true,
                    ConnectionString = string.Format($"{App.Configuration["ConnectionStrings:DefaultConnection"]}", result.data.dotnet)
                });
            }

            _sqlSugarClient.ChangeDatabase(tenantId);
            _configId = tenantId;
            _dbName = result.data.dotnet;
        }
    }

    /// <summary>
    /// 外部接口验证.
    /// </summary>
    /// <param name="interfaceId"></param>
    /// <returns></returns>
    private async Task<string> VerifyInterfaceOauth(string interfaceId)
    {
        var authorization = App.HttpContext.Request.Headers["Authorization"].ToString();
        if (authorization.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.IO0001);
        var ymDate = App.HttpContext.Request.Headers["YmDate"].ToString();
        if (ymDate.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.IO0002);
        var appId = authorization.Split("::")[0];
        var appSecret = authorization.Split("::")[1];
        var interfaceEntity = await _sqlSugarClient.Queryable<InterfaceOauthEntity>().FirstAsync(x => x.AppId == appId && x.DeleteMark == null && x.EnabledMark == 1);
        if (interfaceEntity.IsNullOrEmpty() || interfaceEntity.DataInterfaceIds.IsNullOrEmpty() || !interfaceEntity.DataInterfaceIds.Contains(interfaceId))
            throw Oops.Oh(ErrorCode.IO0003);
        if (interfaceEntity.WhiteList.IsNotEmptyOrNull())
        {
            var ipList = interfaceEntity.WhiteList.Split(",").ToList();
            if (!ipList.Contains(App.HttpContext.GetLocalIpAddressToIPv4()))
                throw Oops.Oh(ErrorCode.D9002);
        }
        if (interfaceEntity.UsefulLife.IsNotEmptyOrNull() && interfaceEntity.UsefulLife < DateTime.Now)
            throw Oops.Oh(ErrorCode.IO0004);
        if (interfaceEntity.VerifySignature == 1)
        {
            if (DateTime.Now > ymDate.TimeStampToDateTime().AddMinutes(1))
                throw Oops.Oh(ErrorCode.IO0004);
            var signature = GetVerifySignature(interfaceEntity, interfaceId, ymDate);
            if (authorization != signature)
                throw Oops.Oh(ErrorCode.IO0003);
        }
        else
        {
            if (interfaceEntity.AppSecret != appSecret)
                throw Oops.Oh(ErrorCode.IO0003);
        }
        return appId;
    }

    /// <summary>
    /// 获取代码生成远端数据缓存.
    /// </summary>
    /// <param name="dynamicId">远端数据ID.</param>
    /// <returns></returns>
    private async Task<List<StaticDataModel>> GetDynamicDataCache(string dynamicId)
    {
        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CodeGenDynamic, _userManager.TenantId, dynamicId);
        return await _cacheManager.GetAsync<List<StaticDataModel>>(cacheKey);
    }

    /// <summary>
    /// 保存代码生成远端数据缓存.
    /// </summary>
    /// <param name="dynamicId">远端数据ID.</param>
    /// <param name="list">在线用户列表.</param>
    /// <returns></returns>
    private async Task<bool> SetDynamicDataCache(string dynamicId, List<StaticDataModel> list)
    {
        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CodeGenDynamic, _userManager.TenantId, dynamicId);
        return await _cacheManager.SetAsync(cacheKey, list, TimeSpan.FromMinutes(3));
    }

    /// <summary>
    /// 根据不同规则请求接口(代码生成专用).
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task<JObject> GetApiDataByTypePreview(DataInterfaceEntity entity)
    {
        var result = new JObject();
        var parameters = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>();
        var parametersHerader = entity.RequestHeaders.ToObject<List<DataInterfaceReqParameter>>();
        var dic = new Dictionary<string, object>();
        var dicHerader = new Dictionary<string, object>();
        dicHerader.Add("Poxiao_API", true);
        if (_userManager.ToKen != null && !_userManager.ToKen.Contains("::"))
            dicHerader.Add("Authorization", _userManager.ToKen);
        foreach (var key in parameters)
        {
            dic.Add(key.field, key.defaultValue);
        }

        foreach (var key in parametersHerader)
        {
            dicHerader[key.field] = key.defaultValue;
        }

        try
        {
            switch (entity.RequestMethod)
            {
                case "6":
                    result = (await entity.Path.SetHeaders(dicHerader).SetQueries(dic).GetAsStringAsync()).ToObject<JObject>();
                    break;
                case "7":
                    result = (await entity.Path.SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync()).ToObject<JObject>();
                    break;
            }
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1018);
        }
        return result.ContainsKey("data") && result["data"].IsNotEmptyOrNull() ? result["data"].ToObject<JObject>() : result;
    }

    /// <summary>
    /// 获取动态无限级数据.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value">指定选项标签为选项对象的某个属性值.</param>
    /// <param name="label">指定选项的值为选项对象的某个属性值.</param>
    /// <param name="children">指定选项的子选项为选项对象的某个属性值.</param>
    /// <returns></returns>
    private List<StaticDataModel> GetDynamicInfiniteData(string data, string value, string label, string children)
    {
        List<StaticDataModel> list = new List<StaticDataModel>();
        foreach (JToken? info in JToken.Parse(data))
        {
            StaticDataModel dic = new StaticDataModel()
            {
                id = info.Value<string>(value),
                fullName = info.Value<string>(label)
            };
            list.Add(dic);
            if (info.Value<object>(children) != null && info.Value<object>(children).ToString() != string.Empty)
                list.AddRange(GetDynamicInfiniteData(info.Value<object>(children).ToString(), value, label, children));
        }

        return list;
    }

    #endregion
}