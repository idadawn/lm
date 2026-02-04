using Poxiao.Infrastructure.Core.Manager;
using Poxiao.ClayObject;
using Poxiao.Infrastructure.Models.NPOI;
using Poxiao.Infrastructure.CodeGen.ExportImport;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Dtos;
using Poxiao.Infrastructure.CodeGen.DataParsing;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.VisualDev.Engine;
using Poxiao.VisualDev.Engine.Core;
using Microsoft.AspNetCore.Http;
using Poxiao.Kpi.Entitys.Dto.Item;
using Poxiao.Kpi.Entitys;
using Poxiao.Kpi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Poxiao.Infrastructure.Models.Authorize;
using Poxiao.DatabaseAccessor;

namespace Poxiao.Kpi;

/// <summary>
/// 业务实现：物料信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Kpi", Name = "Item", Order = 200)]
[Route("api/Kpi/[controller]")]
public class ItemService : IItemService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ItemEntity> _repository;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 数据接口服务.
    /// </summary>
    private readonly IDataInterfaceService _dataInterfaceService;
    
    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 通用数据解析.
    /// </summary>
    private readonly ControlParsing _controlParsing;
    
    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 代码生成导出数据帮助类.
    /// </summary>
    private readonly ExportImportDataHelper _exportImportDataHelper;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 导出字段.
    /// </summary>
    private readonly List<ParamsModel> paramList = "[{\"value\":\"物料类型\",\"field\":\"itemCategory\"},{\"value\":\"物料编码\",\"field\":\"itemCode\"},{\"value\":\"物料描述\",\"field\":\"itemDesc\"},]".ToList<ParamsModel>();

    /// <summary>
    /// 导入字段.
    /// </summary>
    private readonly string[] uploaderKey = new string[] {"item_code", "item_desc", "item_category" };

    /// <summary>
    /// 初始化一个<see cref="ItemService"/>类型的新实例.
    /// </summary>
    public ItemService(
        ISqlSugarRepository<ItemEntity> repository,
        IDataInterfaceService dataInterfaceService,
        IDataBaseManager dataBaseManager,
        ISqlSugarClient context,
        ExportImportDataHelper exportImportDataHelper,
        IFileManager fileManager,
        ICacheManager cacheManager,
        ControlParsing controlParsing,
        IUserManager userManager)
    {
        _repository = repository;
        _dataBaseManager = dataBaseManager;
        _sqlSugarClient = (SqlSugarScope)context;
        _exportImportDataHelper = exportImportDataHelper;
        _fileManager = fileManager;
        _dataInterfaceService = dataInterfaceService;
        _cacheManager = cacheManager;
        _controlParsing = controlParsing;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取物料信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        return (await _sqlSugarClient.Queryable<ItemEntity>()
            .FirstAsync(it => it.Id.Equals(id))).Adapt<ItemInfoOutput>(); 
    }

    /// <summary>
    /// 获取物料信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("List")]
    public async Task<dynamic> GetList([FromBody] ItemListQueryInput input)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var entityInfo = _sqlSugarClient.EntityMaintenance.GetEntityInfo<ItemEntity>();
        var superQuery = SuperQueryHelper.GetSuperQueryInput(input.superQueryJson, string.Empty, entityInfo, 0);
        List<IConditionalModel> mainConditionalModel = SuperQueryHelper.GetSuperQueryJson(superQuery);
        var itemCategoryDbColumnName = entityInfo.Columns.Find(it => it.PropertyName == "ItemCategory").DbColumnName;
        var data = await _sqlSugarClient.Queryable<ItemEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.itemCode), it => it.ItemCode.Contains(input.itemCode))
            .WhereIF(!string.IsNullOrEmpty(input.itemDesc), it => it.ItemDesc.Contains(input.itemDesc))
            .Where(_controlParsing.GenerateMultipleSelectionCriteriaForQuerying(itemCategoryDbColumnName, input.itemCategory))
            .Where(mainConditionalModel)
            .Select(it => new ItemListOutput
            {
                id = it.Id,
                itemCode = it.ItemCode,
                itemDesc = it.ItemDesc,
                itemCategory = SqlFunc.Subqueryable<DictionaryDataEntity>().AS("db_default.kpi_sys.dbo.BASE_DICTIONARYDATA").Where(dic => dic.EnCode.Equals(it.ItemCategory) && dic.DictionaryTypeId.Equals("558703923028819909")).Select(dic => dic.FullName),
            }).MergeTable().OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.id).OrderByIF(!string.IsNullOrEmpty(input.sidx), input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ItemListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建物料信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ItemCrInput input)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var entity = input.Adapt<ItemEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        if(await _sqlSugarClient.Queryable<ItemEntity>().AnyAsync(it => it.ItemCode.Equals(input.itemCode)))
            throw Oops.Bah(ErrorCode.COM1023, "物料编码");
        var isOk = await _sqlSugarClient.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 获取物料信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList(ItemListQueryInput input)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var entityInfo = _sqlSugarClient.EntityMaintenance.GetEntityInfo<ItemEntity>();
        var superQuery = SuperQueryHelper.GetSuperQueryInput(input.superQueryJson, string.Empty, entityInfo, 0);
        List<IConditionalModel> mainConditionalModel = SuperQueryHelper.GetSuperQueryJson(superQuery);
        var itemCategoryDbColumnName = entityInfo.Columns.Find(it => it.PropertyName == "ItemCategory").DbColumnName;
        var list = await _sqlSugarClient.Queryable<ItemEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.itemCode), it => it.ItemCode.Contains(input.itemCode))
            .WhereIF(!string.IsNullOrEmpty(input.itemDesc), it => it.ItemDesc.Contains(input.itemDesc))
            .Where(_controlParsing.GenerateMultipleSelectionCriteriaForQuerying(itemCategoryDbColumnName, input.itemCategory))
            .Where(mainConditionalModel)
            .Select(it => new ItemListOutput
            {
                id = it.Id,
                itemCode = it.ItemCode,
                itemDesc = it.ItemDesc,
                itemCategory = SqlFunc.Subqueryable<DictionaryDataEntity>().AS("db_default.kpi_sys.dbo.BASE_DICTIONARYDATA").Where(dic => dic.EnCode.Equals(it.ItemCategory) && dic.DictionaryTypeId.Equals("558703923028819909")).Select(dic => dic.FullName),
            }).MergeTable().OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.id).OrderByIF(!string.IsNullOrEmpty(input.sidx), input.sidx + " " + input.sort).ToListAsync();

        return list;
    }

    /// <summary>
    /// 导出物料信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] ItemListQueryInput input)
    {
        var exportData = new List<ItemListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<ItemListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        var excelName="物料信息";
        _cacheManager.Set(excelName + ".xls", string.Empty);
        return CodeGenExportDataHelper.GetDataExport(excelName,input.selectKey, _userManager.UserId,exportData.ToJsonString().ToObjectOld<List<Dictionary<string, object>>>(), paramList, false);
    }

    /// <summary>
    /// 下载模板.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public async Task<dynamic> TemplateDownload()
    {
        List<Dictionary<string, object>>? dataList = new List<Dictionary<string, object>>();

        // 赋予默认值
        var dicItem = ExportImportDataHelper.GetTemplateHeader<ItemEntity>(new ItemEntity(), 1);

        dicItem.Add("id", "id");
        dataList.Add(dicItem);

        var excelName = string.Format("{0} 导入模板_{1}", "物料信息", SnowflakeIdHelper.NextId());
        _cacheManager.Set(excelName + ".xls", string.Empty);
        return CodeGenExportDataHelper.GetTemplateExport(excelName, string.Join(",", uploaderKey), _userManager.UserId, dataList, paramList);
    }

    /// <summary>
    /// Excel导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Uploader")]
    public async Task<dynamic> Uploader(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);
        return new { name = _fileName, url = string.Format("/api/File/Image/{0}/{1}", string.Empty, _fileName) };
    }

    /// <summary>
    /// 导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        fieldList.AddRange(ExportImportDataHelper.GetTemplateParsing<ItemEntity>(new ItemEntity()));
        var entityInfo = _repository.AsSugarClient().EntityMaintenance.GetEntityInfo<ItemEntity>();
        List<DbTableRelationModel> tables = new List<DbTableRelationModel>() { ExportImportDataHelper.GetTableRelation(entityInfo, "1") };
        DbLinkEntity link = _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
        var tInfo = new TemplateParsingBase(link, fieldList, tables, "id", 2, 1, uploaderKey.ToList(), "1");
        return await _exportImportDataHelper.GetImportPreviewData(tInfo, fileName);
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    [UnitOfWork]
    public async Task<dynamic> ImportData([FromBody] DataImportInput input)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        fieldList.AddRange(ExportImportDataHelper.GetTemplateParsing<ItemEntity>(new ItemEntity()));
        var entityInfo = _repository.AsSugarClient().EntityMaintenance.GetEntityInfo<ItemEntity>();
        List<DbTableRelationModel> tables = new List<DbTableRelationModel>() { ExportImportDataHelper.GetTableRelation(entityInfo, "1") };
        DbLinkEntity link = _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
        var tInfo = new TemplateParsingBase(link, fieldList, tables, "id", 2, 1, uploaderKey.ToList(), "1");

        object[]? res = await _exportImportDataHelper.ImportMenuData(tInfo, input.list);
        var addlist = res.First() as List<Dictionary<string, object>>;
        var errorlist = res.Last() as List<Dictionary<string, object>>;
        var result = new DataImportOutput()
        {
            snum = addlist.Count,
            fnum = errorlist.Count,
            failResult = errorlist,
            resultType = errorlist.Count < 1 ? 0 : 1
        };

        return result;
    }

    /// <summary>
    /// 导入数据的错误报告.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportExceptionData")]
    [UnitOfWork]
    public async Task<dynamic> ExportExceptionData([FromBody] DataImportInput list)
    {
        List<FieldsModel> fieldList = new List<FieldsModel>();
        fieldList.AddRange(ExportImportDataHelper.GetTemplateParsing<ItemEntity>(new ItemEntity()));
        var entityInfo = _repository.AsSugarClient().EntityMaintenance.GetEntityInfo<ItemEntity>();
        List<DbTableRelationModel> tables = new List<DbTableRelationModel>() { ExportImportDataHelper.GetTableRelation(entityInfo, "1") };
        DbLinkEntity link = _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
        var tInfo = new TemplateParsingBase(link, fieldList, tables, "id", 2, 1, uploaderKey.ToList(), "1");

        // 错误数据
        tInfo.selectKey.Add("errorsInfo");
        tInfo.AllFieldsModel.Add(new FieldsModel() { __vModel__ = "errorsInfo", __config__ = new ConfigModel() { label = "异常原因" } });
        for (var i = 0; i < list.list.Count(); i++) list.list[i].Add("id", i);

        var result = ExportImportDataHelper.GetCreateFirstColumnsHeader(tInfo.selectKey, list.list, paramList);
        var firstColumns = result.First().ToObject<Dictionary<string, int>>();
        var resultList = result.Last().ToObject<List<Dictionary<string, object>>>();
        _cacheManager.Set(string.Format("{0} 导入错误报告.xls", tInfo.FullName), string.Empty);
        return firstColumns.Any()
            ? await _exportImportDataHelper.ExcelCreateModel(tInfo, resultList, string.Format("{0} 导入错误报告", tInfo.FullName), firstColumns)
            : await _exportImportDataHelper.ExcelCreateModel(tInfo, resultList, string.Format("{0} 导入错误报告", tInfo.FullName));
    }

    /// <summary>
    /// 批量删除物料信息.
    /// </summary>
    /// <param name="ids">主键数组.</param>
    /// <returns></returns>
    [HttpPost("batchRemove")]
    [UnitOfWork]
    public async Task BatchRemove([FromBody] List<string> ids)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var entitys = await _sqlSugarClient.Queryable<ItemEntity>().In(it => it.Id, ids).ToListAsync();
        if (entitys.Count > 0)
        {
            // 批量删除物料信息
            await _sqlSugarClient.Deleteable<ItemEntity>().In(it => it.Id,ids).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新物料信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ItemUpInput input)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var entity = input.Adapt<ItemEntity>();
        if (await _sqlSugarClient.Queryable<ItemEntity>().AnyAsync(it => it.ItemCode.Equals(input.itemCode) && !it.Id.Equals(id)))
            throw Oops.Bah(ErrorCode.COM1023, "物料编码");
        var isOk = await _sqlSugarClient.Updateable(entity)
            .UpdateColumns(it => new
            {
                it.ItemCode,
                it.ItemDesc,
                it.ItemCategory,
            }).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除物料信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var dbLink = await _repository.AsSugarClient().Queryable<DbLinkEntity>().FirstAsync(it => it.Id.Equals("482106329557630917"));
        _sqlSugarClient = _dataBaseManager.ChangeDataBase(dbLink);

        var isOk = await _sqlSugarClient.Deleteable<ItemEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();   
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
