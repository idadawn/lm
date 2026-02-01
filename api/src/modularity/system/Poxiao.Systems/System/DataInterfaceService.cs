using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Poxiao.ClayObject;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extras.DatabaseAccessor.SqlSugar.Models;
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
using Poxiao.SensitiveDetection;
using Poxiao.Systems.Entitys.Dto.DataInterFace;
using Poxiao.Systems.Entitys.Dto.System.DataInterFace;
using Poxiao.Systems.Entitys.Model.DataInterFace;
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
[ApiDescriptionSettings(Tag = "System", Name = "DataInterface11", Order = 204)]
[Route("api/system/[controller]")]
public class DataInterfaceService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DataInterfaceEntity> _repository;

    /// <summary>
    /// 数据字典服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 脱敏词汇提供器.
    /// </summary>
    private readonly ISensitiveDetectionProvider _sensitiveDetectionProvider;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化 SqlSugar 客户端.
    /// </summary>
    private readonly SqlSugarScope _sqlSugarClient;

    /// <summary>
    /// 数据库上下文ID.
    /// </summary>
    private string _configId = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default")?.ConfigId?.ToString() ?? "default";

    /// <summary>
    /// 数据库名称.
    /// </summary>
    private string _dbName = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default")?.DBName ?? "";

    /// <summary>
    /// 初始化一个<see cref="DataInterfaceService"/>类型的新实例.
    /// </summary>
    public DataInterfaceService(
        ISqlSugarRepository<DataInterfaceEntity> repository,
        IDictionaryDataService dictionaryDataService,
        IDataBaseManager dataBaseManager,
        IUserManager userManager,
        ICacheManager cacheManager,
        IFileManager fileManager,
        ISensitiveDetectionProvider sensitiveDetectionProvider,
        ISqlSugarClient context)
    {
        _sensitiveDetectionProvider = sensitiveDetectionProvider;
        _repository = repository;
        _dictionaryDataService = dictionaryDataService;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _sqlSugarClient = (SqlSugarScope)context;
    }

    #region Get

    /// <summary>
    /// 获取接口列表(分页).
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new DataInterfaceListOutput
            {
                id = a.Id,
                categoryId = a.CategoryId,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                dataType = a.DataType,
                dbLinkId = a.DBLinkId,
                description = a.Description,
                enCode = a.EnCode,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                path = a.Path,
                query = a.Query,
                requestMethod = SqlFunc.IF(a.RequestMethod.Equals("1")).Return("新增").ElseIF(a.RequestMethod.Equals("2")).Return("修改")
                .ElseIF(a.RequestMethod.Equals("3")).Return("查询").ElseIF(a.RequestMethod.Equals("4")).Return("删除")
                .ElseIF(a.RequestMethod.Equals("5")).Return("存储过程").ElseIF(a.RequestMethod.Equals("6")).Return("Get")
                .End("Post"),
                requestParameters = a.RequestParameters,
                responseType = a.ResponseType,
                sortCode = a.SortCode,
                checkType = a.CheckType,
                tenantId = _userManager.TenantId
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<DataInterfaceListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取接口列表(分页).
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("getList")]
    public async Task<dynamic> getList([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.dataType), a => a.DataType.ToString() == input.dataType)
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new DateInterfaceGetListOutput
            {
                id = a.Id,
                categoryId = a.CategoryId,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                DataType = a.DataType,
                dbLinkId = a.DBLinkId,
                description = a.Description,
                enCode = a.EnCode,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                path = a.Path,
                query = a.Query,
                requestMethod = SqlFunc.IF(a.RequestMethod.Equals("1")).Return("新增").ElseIF(a.RequestMethod.Equals("2")).Return("修改")
                .ElseIF(a.RequestMethod.Equals("3")).Return("查询").ElseIF(a.RequestMethod.Equals("4")).Return("删除")
                .ElseIF(a.RequestMethod.Equals("5")).Return("存储过程").ElseIF(a.RequestMethod.Equals("6")).Return("Get")
                .End("Post"),
                requestParameters = a.RequestParameters,
                responseType = a.ResponseType,
                sortCode = a.SortCode,
                checkType = a.CheckType,
                tenantId = _userManager.TenantId
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
        foreach (var entity in await _repository.AsQueryable().Where(x => x.DeleteMark == null && x.EnabledMark == 1).OrderBy(x => x.SortCode).ToListAsync())
        {
            var dictionaryDataEntity = await _dictionaryDataService.GetInfo(entity.CategoryId);
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

    /// <summary>
    /// 访问接口 选中 回写.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpGet("{id}/Action/Info")]
    public async Task<dynamic> ActionsResponseInfo(string id, [FromQuery] string tenantId, [FromQuery] VisualDevDataFieldDataListInput input)
    {
        return await GetResponseByType(id, 1, tenantId, input);
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

    #endregion

    #region Post

    /// <summary>
    /// 预览接口.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Preview")]
    [UnitOfWork]
    public async Task<object> Preview(string id, [FromBody] DataInterfacePreviewInput input)
    {
        _configId = _userManager.TenantId;
        _dbName = _userManager.TenantDbName;
        object output = null;
        var info = await GetInfo(id);
        var dicParameters = new Dictionary<string, string>();
        if (input.paramList.IsNotEmptyOrNull() && input.paramList.Count > 0)
        {
            dicParameters = input.paramList.ToDictionary(x => x.field, y => y.defaultValue);
        }
        VerifyRequired(info, dicParameters);
        ReplaceParameterValue(info, dicParameters);
        if (info.DataType == 1)
        {
            output = await GetData(info);
        }
        else if (info.DataType == 2)
        {
            output = info.Query.ToObject<object>();
        }
        else
        {
            output = await GetApiDataByTypePreview(info);
        }
        if (info.DataProcessing.IsNullOrEmpty())
        {
            return output;
        }
        else
        {
            string sheetData = Regex.Match(info.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
            var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
            return JsEngineUtil.CallFunction(scriptStr, output.ToJsonString(CommonConst.options)); //此处时间非时间戳
        }
    }

    /// <summary>
    /// 访问接口 选中 回写.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpPost("{id}/Action/InfoByIds")]
    public async Task<dynamic> ActionsResponseInfoNew(string id, [FromBody] VisualDevDataFieldDataListInput input)
    {
        return await GetResponseByType(id, 1, string.Empty, input);
    }

    /// <summary>
    /// 访问接口 分页.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpPost("{id}/Action/List")]
    public async Task<dynamic> ActionsResponseList(string id, [FromBody] VisualDevDataFieldDataListInput input)
    {
        return await GetResponseByType(id, 0, string.Empty, input);
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
    public async Task DeleteApi(string id)
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
            if (!"default".Equals(tenantId) && KeyVariable.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
            }
            else
            {
                _sqlSugarClient.ChangeDatabase(tenantId);
            }
        }
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
            return (await Preview(id, input)).ToObject<List<Dictionary<string, object>>>().FirstOrDefault().Keys.ToList();
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.IO0005);
        }
    }
    #endregion

    #region PublicMethod

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
    /// 查询.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<DataTable> GetData(DataInterfaceEntity entity)
    {
        return await connection(entity.DBLinkId, entity.Query, entity.RequestMethod);
    }

    /// <summary>
    /// 根据不同类型请求接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">0 ： 分页 1 ：详情 2：数据视图 ，其他 原始.</param>
    /// <param name="tenantId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null)
    {
        try
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

                if (!"default".Equals(tenantId) && KeyVariable.MultiTenancyType.Equals("COLUMN"))
                {
                    _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
                }
                else
                {
                    _sqlSugarClient.ChangeDatabase(tenantId);
                    _configId = tenantId;
                    _dbName = result.data.dotnet;
                }
            }

            var data = await _sqlSugarClient.CopyNew().Queryable<DataInterfaceEntity>().FirstAsync(x => x.Id == id && x.DeleteMark == null);
            if (data == null)
                throw Oops.Oh(ErrorCode.COM1005);

            // 远端数据（sql过滤）
            if (input.IsNotEmptyOrNull())
            {
                if (type == 2 && !2.Equals(data.DataType) && (input.QueryJson.IsNotEmptyOrNull() || input.Sidx.IsNotEmptyOrNull()))
                {
                    if (input.QueryJson.IsNotEmptyOrNull())
                    {
                        var sqlFields = input.QueryJson.ToObject<Dictionary<string, string>>();
                        var whereList = new List<string>();
                        foreach (var item in sqlFields)
                        {
                            if (item.Key.Contains("poxiao_searchType_equals_")) whereList.Add(string.Format("{0} = '{1}' ", item.Key.Replace("poxiao_searchType_equals_", string.Empty), item.Value));
                            else whereList.Add(string.Format("{0} like '%{1}%' ", item.Key, item.Value));
                        }
                        data.Query = string.Format("select * from ({0}) t where {1} ", data.Query.TrimEnd(';'), string.Join(" and ", whereList));
                    }
                    if (input.Sidx.IsNotEmptyOrNull()) data.Query = string.Format("{0} order by {1} {2}", data.Query.TrimEnd(';'), input.Sidx, input.Sort);
                }
                else
                {
                    var columnList = new List<string>();
                    if (input.Keyword.IsNotEmptyOrNull())
                        input.columnOptions.Split(",").ToList().ForEach(x => columnList.Add(string.Format("{0} like '%{1}%'", x, input.Keyword)));

                    if (columnList.Any() && !string.IsNullOrWhiteSpace(input.Keyword))
                        data.Query = string.Format("select * from ({0}) t where {1} ", data.Query.TrimEnd(';'), string.Join(" or ", columnList));
                    else if (!string.IsNullOrWhiteSpace(input.relationField) && !string.IsNullOrWhiteSpace(input.Keyword))
                        data.Query = string.Format("select * from ({0}) t where {1} like '%{2}%' ", data.Query.TrimEnd(';'), input.relationField, input.Keyword);

                    if (!string.IsNullOrWhiteSpace(input.propsValue) && !string.IsNullOrWhiteSpace(input.id))
                        data.Query = string.Format("select * from ({0}) t where {1} = '{2}' ", data.Query.TrimEnd(';'), input.propsValue, input.id);
                    if (!string.IsNullOrWhiteSpace(input.propsValue) && input.ids.Any())
                        data.Query = string.Format("select * from ({0}) t where {1} in ('{2}') ", data.Query.TrimEnd(';'), input.propsValue, string.Join("','", input.ids));
                    if (input.columnOptions.IsNotEmptyOrNull() && !string.IsNullOrWhiteSpace(input.Keyword))
                    {
                        var whereStr = new List<string>();
                        input.columnOptions.Split(",").ToList().ForEach(item => whereStr.Add(string.Format(" {0} like '%{1}%' ", item, input.Keyword)));
                        data.Query = string.Format("select * from ({0}) t where {1} ", data.Query.TrimEnd(';'), string.Join(" or ", whereStr));
                    }
                }
                if (input.paramList.IsNotEmptyOrNull() && input.paramList.Count > 0)
                {
                    dicParameters = input.paramList.ToDictionary(x => x.field, y => y.defaultValue);
                }
            }

            if (dicParameters.IsNullOrEmpty())
                dicParameters = new Dictionary<string, string>();
            ReplaceParameterValue(data, dicParameters);
            object output = null;
            #region 调用接口

            if (1.Equals(data.DataType))
            {
                var resTable = await GetData(data);
                if (type == 0 || type == 2)
                {
                    // 分页
                    var dt = GetPageToDataTable(resTable, input.CurrentPage, input.PageSize);
                    output = new
                    {
                        pagination = new PageInfo()
                        {
                            currentPage = input.CurrentPage,
                            pageSize = input.PageSize,
                            total = resTable.Rows.Count
                        },
                        list = dt.ToObject<List<Dictionary<string, object>>>(),
                    };
                }
                else if (type == 1)
                {
                    if (input.ids.Any())
                    {
                        output = resTable.ToObject<List<Dictionary<string, object>>>();
                    }
                    else
                    {
                        output = resTable.ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
                    }
                }
                else
                {
                    output = resTable;
                }

            }
            else if (2.Equals(data.DataType))
            {

                output = data.Query.ToObject<object>();
            }
            else
            {
                var result = await GetApiDataByTypePreview(data);
                var resList = result != null && result.ContainsKey("list") ? result["list"].ToObject<List<Dictionary<string, object>>>() : new List<Dictionary<string, object>>();
                if (data.DataProcessing.IsNotEmptyOrNull() && result.IsNotEmptyOrNull())
                {
                    string sheetData = Regex.Match(data.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
                    var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
                    var resJsonStr = JsEngineUtil.CallFunction(scriptStr, result.ToJsonString(CommonConst.options)).ToJsonString(); //此处时间非时间戳
                    resList = resJsonStr.ToObject<List<Dictionary<string, object>>>();
                }
                if (type == 0)
                {
                    //if (input.columnOptions.IsNotEmptyOrNull())
                    //{
                    //    var columList = input.columnOptions.Split(",").ToList();
                    //    resList.ForEach(item =>
                    //    {
                    //        item.Where(x => !columList.Contains(x.Key)).ToList().ForEach(it => item.Remove(it.Key));
                    //    });
                    //}
                    if (input.Keyword.IsNotEmptyOrNull())
                        resList = resList.FindAll(x => x.Where(xx => xx.Value != null && xx.Value.ToString().Contains(input.Keyword)).Any());
                    output = new
                    {
                        pagination = new PageInfo()
                        {
                            currentPage = input.CurrentPage,
                            pageSize = input.PageSize,
                            total = resList.Count
                        },
                        list = resList.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize).ToList(),
                    };
                }
                else if (type == 1)
                {
                    if (input.id != null)
                    {
                        return resList.Find(x => x.ContainsKey(input.propsValue) && x.ContainsValue(input.id));
                    }
                    else if (input.id == null && input.ids.Count > 0)
                    {
                        return resList.FindAll(x => x.ContainsKey(input.propsValue) && x.Any(it => input.ids.Contains(it.Value)));
                    }
                }
                else if (type == 2)
                {
                    if (input.QueryJson.IsNotEmptyOrNull() || input.Sidx.IsNotEmptyOrNull())
                    {
                        if (input.QueryJson.IsNotEmptyOrNull())
                        {
                            var querList = input.QueryJson.ToObject<Dictionary<string, string>>();
                            foreach (var item in querList)
                            {
                                if (item.Key.Contains("poxiao_searchType_equals_")) resList = resList.Where(x => x[item.Key.Replace("poxiao_searchType_equals_", "")].Equals(item.Value)).ToList();
                                else resList = resList.Where(x => x[item.Key].ToJsonString().Contains(item.Value)).ToList();
                            }
                        }
                        if (input.Sidx.IsNotEmptyOrNull())
                        {
                            if (input.Sort.Equals("desc")) resList = resList.OrderBy(x => x[input.Sidx]).ToList();
                            else resList = resList.OrderByDescending(x => x[input.Sidx]).ToList();
                        }
                        output = new
                        {
                            pagination = new PageInfo()
                            {
                                currentPage = input.CurrentPage,
                                pageSize = input.PageSize,
                                total = resList.Count
                            },
                            list = resList.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize).ToList(),
                        };
                    }
                    else
                    {
                        output = new
                        {
                            pagination = new PageInfo()
                            {
                                currentPage = input.CurrentPage,
                                pageSize = input.PageSize,
                                total = resList.Count
                            },
                            list = resList.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize).ToList(),
                        };
                    }
                }
                else
                {
                    output = result;
                }
            }
            #endregion

            return output;
        }
        catch (Exception e)
        {
            return new List<object>();
        }
    }

    /// <summary>
    /// 处理远端数据.
    /// </summary>
    /// <param name="propsUrl">远端数据ID.</param>
    /// <param name="value">指定选项标签为选项对象的某个属性值.</param>
    /// <param name="label">指定选项的值为选项对象的某个属性值.</param>
    /// <param name="children">指定选项的子选项为选项对象的某个属性值.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<StaticDataModel>> GetDynamicList(string propsUrl, string value, string label, string children)
    {
        List<StaticDataModel> list = new List<StaticDataModel>();

        // 获取远端数据
        DataInterfaceEntity? dynamic = await _repository.AsQueryable().Where(x => x.Id == propsUrl && x.DeleteMark == null).FirstAsync();
        if (dynamic == null) return list;

        list = await GetDynamicDataCache(dynamic.Id);

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

    #region PrivateMethod

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

    /// <summary>
    /// 通过连接执行sql.
    /// </summary>
    /// <returns></returns>
    private async Task<DataTable> connection(string dbLinkId, string sql, string reqMethod)
    {
        var link = new DbLinkEntity();
        if (!_sqlSugarClient.AsTenant().IsAnyConnection(_configId))
        {
            link = await _sqlSugarClient.CopyNew().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == dbLinkId && x.DeleteMark == null);
        }
        else
        {
            link = await _repository.AsSugarClient().CopyNew().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == dbLinkId && x.DeleteMark == null);
        }

        var tenantLink = link ?? await GetTenantDbLink();
        var parameter = new List<SugarParameter>();
        sql = await GetSqlParameter(sql, parameter);
        if (reqMethod.Equals("3"))
        {
            return _dataBaseManager.GetInterFaceDataNew(tenantLink, sql, parameter.ToArray());
        }
        else
        {
            _dataBaseManager.ExecuteCommand(tenantLink, sql, parameter.ToArray());
            return new DataTable();
        }
    }

    /// <summary>
    /// 根据不同规则请求接口(预览).
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
    /// DataTable 数据分页.
    /// </summary>
    /// <param name="dt">数据源.</param>
    /// <param name="PageIndex">第几页.</param>
    /// <param name="PageSize">每页多少条.</param>
    /// <returns></returns>
    public static DataTable GetPageToDataTable(DataTable dt, int PageIndex, int PageSize)
    {
        if (PageIndex == 0)
            return dt; // 0页代表每页数据，直接返回

        if (dt == null)
        {
            return new DataTable();
        }

        DataTable newdt = dt.Copy();
        newdt.Clear(); // copy dt的框架

        int rowbegin = (PageIndex - 1) * PageSize;
        int rowend = PageIndex * PageSize; // 要展示的数据条数

        if (rowbegin >= dt.Rows.Count)
            return dt; // 源数据记录数小于等于要显示的记录，直接返回dt

        if (rowend > dt.Rows.Count)
            rowend = dt.Rows.Count;
        for (int i = rowbegin; i <= rowend - 1; i++)
        {
            DataRow newdr = newdt.NewRow();
            DataRow dr = dt.Rows[i];
            foreach (DataColumn column in dt.Columns)
            {
                newdr[column.ColumnName] = dr[column.ColumnName];
            }

            newdt.Rows.Add(newdr);
        }

        return newdt;
    }

    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<DbLinkEntity> GetTenantDbLink()
    {
        var connectionConfigs = App.GetOptions<ConnectionStringsOptions>().ConnectionConfigs.FirstOrDefault(it => it.ConfigId?.ToString() == "default");
        if (connectionConfigs == null)
            throw Oops.Oh("Default database connection not found");
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
                if (dic.Keys.Contains(item.field))
                    item.defaultValue = HttpUtility.UrlDecode(dic[item.field], Encoding.UTF8); // 对参数解码
                if (entity.DataType == 1)
                {
                    // 将开头和结尾的空格去掉
                    item.defaultValue = item.defaultValue?.Trim();

                    // 将逗号替换成一个单引号、逗号、单引号
                    item.defaultValue = item.defaultValue?.Replace(",", "','");
                    entity.Query = entity.Query?.Replace("{" + item.field + "}", "'" + item.defaultValue + "'");
                }
                else
                {
                    entity.Query = entity.Query?.Replace("{" + item.field + "}", item.defaultValue);
                }
            }

            entity.RequestParameters = parameterList.ToJsonString();
        }
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
            // 当前用户及下属
            if (sql.Contains("@currentUsersAndSubordinates"))
            {
                var subordinates = _userManager.CurrentUserAndSubordinates;
                sugarParameters.Add(new SugarParameter("@currentUsersAndSubordinates", subordinates));
            }

            // 当前组织
            if (sql.Contains("@organization"))
            {
                sugarParameters.Add(new SugarParameter("@organization", _userManager?.User?.OrganizeId));
            }

            // 当前组织及子组织
            if (sql.Contains("@currentOrganizationAndSuborganization"))
            {
                var subsidiary = _userManager.CurrentOrganizationAndSubOrganizations;
                sugarParameters.Add(new SugarParameter("@currentOrganizationAndSuborganization", subsidiary));
            }

            // 当前分管组织
            if (sql.Contains("@chargeorganization"))
            {
                var chargeorganization = _userManager.DataScope.Where(x => x.organizeId == _userManager.User.OrganizeId && x.Select).ToList().FirstOrDefault();
                sugarParameters.Add(new SugarParameter("@chargeorganization", chargeorganization?.organizeId));
            }

            // 当前分管组织及子组织
            if (sql.Contains("@currentChargeorganizationAndSuborganization"))
            {
                var subsidiary = _userManager.DataScope.Select(x => x.organizeId).Intersect(_userManager.CurrentUserSubOrganization).ToList();
                sugarParameters.Add(new SugarParameter("@currentChargeorganizationAndSuborganization", subsidiary));
            }

            // 当前用户
            if (sql.Contains("@user"))
            {
                sql = sql.Replace("@user", "'" + _userManager.UserId + "'"); // orcale关键字处理
            }
        }
        return sql;
    }

    /// <summary>
    /// 验证必填参数.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dataInterfaceReqParameters"></param>
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
    /// 外部接口验证并请求.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tenantId"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    private async Task<dynamic> InterfaceVerify(string id, string tenantId, Dictionary<string, string> dic)
    {
        UserAgent userAgent = new UserAgent(App.HttpContext);
        var authorization = App.HttpContext.Request.Headers["Authorization"].ToString();
        if (authorization.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.IO0001);
        var ymDate = App.HttpContext.Request.Headers["YmDate"].ToString();
        if (ymDate.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.IO0002);
        var appId = authorization.Split("::")[0];
        var appSecret = authorization.Split("::")[1];
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

            if (!"default".Equals(tenantId) && KeyVariable.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
            }
            else
            {
                _sqlSugarClient.ChangeDatabase(tenantId);
            }
            _configId = tenantId;
            _dbName = result.data.dotnet;
        }
        var interfaceEntity = await _sqlSugarClient.Queryable<InterfaceOauthEntity>().FirstAsync(x => x.AppId == appId && x.DeleteMark == null && x.EnabledMark == 1);
        if (interfaceEntity.IsNullOrEmpty() || interfaceEntity.DataInterfaceIds.IsNullOrEmpty() || !interfaceEntity.DataInterfaceIds.Contains(id))
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
            var signature = GetVerifySignature(interfaceEntity, id, ymDate);
            if (authorization != signature)
                throw Oops.Oh(ErrorCode.IO0003);
        }
        else
        {
            if (interfaceEntity.AppSecret != appSecret)
                throw Oops.Oh(ErrorCode.IO0003);
        }

        var sw = new Stopwatch();
        sw.Start();
        object output = null;
        var info = await GetInfo(id);
        if (info != null && info.EnabledMark == 0)
            throw Oops.Oh(ErrorCode.IO0003);
        VerifyRequired(info, dic);
        ReplaceParameterValue(info, dic);
        if (info.DataType == 1)
        {
            var link = new DbLinkEntity();
            if (!_sqlSugarClient.AsTenant().IsAnyConnection(_configId))
            {
                link = await _sqlSugarClient.Queryable<DbLinkEntity>().FirstAsync(x => x.Id == info.DBLinkId && x.DeleteMark == null);
            }
            else
            {
                link = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(x => x.Id == info.DBLinkId && x.DeleteMark == null);
            }

            var tenantLink = link ?? await GetTenantDbLink();
            output = _dataBaseManager.GetInterFaceData(tenantLink, info.Query);
        }
        else if (info.DataType == 2)
        {
            output = info.Query.ToObject<object>();
        }
        else
        {
            output = await GetApiDataByTypePreview(info);
        }
        if (info.DataProcessing.IsNotEmptyOrNull())
        {
            string sheetData = Regex.Match(info.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
            var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
            output = JsEngineUtil.CallFunction(scriptStr, output.ToJsonString());
        }
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

    #endregion
}