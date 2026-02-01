using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Models.Authorize;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.VisualDev.Engine;
using Poxiao.VisualDev.Engine.Core;
using Poxiao.VisualDev.Engine.Model;
using Poxiao.VisualDev.Entitys;
using Poxiao.VisualDev.Entitys.Dto.VisualDev;
using Poxiao.VisualDev.Entitys.Dto.VisualDevModelData;
using Poxiao.VisualDev.Interfaces;
using Poxiao.WorkFlow.Entitys.Entity;
using SqlSugar;

namespace Poxiao.VisualDev;

/// <summary>
/// 可视化开发基础 .
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Base", Order = 171)]
[Route("api/visualdev/[controller]")]
public class VisualDevService : IVisualDevService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualDevEntity> _visualDevRepository;

    /// <summary>
    /// 字典服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _changeDataBase;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 在线开发运行服务.
    /// </summary>
    private readonly RunService _runService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// 初始化一个<see cref="VisualDevService"/>类型的新实例.
    /// </summary>
    public VisualDevService(
        ISqlSugarRepository<VisualDevEntity> visualDevRepository,
        IDataBaseManager changeDataBase,
        IUserManager userManager,
        IOptions<TenantOptions> tenantOptions,
        RunService runService,
        IDictionaryDataService dictionaryDataService,
        ISqlSugarClient context)
    {
        _visualDevRepository = visualDevRepository;
        _dictionaryDataService = dictionaryDataService;
        _userManager = userManager;
        _runService = runService;
        _tenant = tenantOptions.Value;
        _changeDataBase = changeDataBase;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 获取功能列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] VisualDevListQueryInput input)
    {
        SqlSugarPagedList<VisualDevListOutput>? data = await _visualDevRepository.AsSugarClient().Queryable<VisualDevEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.FullName.Contains(input.Keyword) || a.EnCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrEmpty(input.category), a => a.Category == input.category)
            .Where(a => a.DeleteMark == null && a.Type == input.type)
            .OrderBy(a => a.SortCode, OrderByType.Asc)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select(a => new VisualDevListOutput
            {
                Id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                state = a.State,
                type = a.Type,
                webType = a.WebType,
                tables = a.Tables,
                description = a.Description,
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                deleteMark = a.DeleteMark,
                sortCode = a.SortCode,
                ParentId = a.Category,
                isRelease = a.State,
                enableFlow = a.EnableFlow,
                pcIsRelease = SqlFunc.Subqueryable<ModuleEntity>().Where(m => m.ModuleId == a.Id && m.Category == "Web" && m.DeleteMark == null && m.ModuleId != null).Count(),
                appIsRelease = SqlFunc.Subqueryable<ModuleEntity>().Where(m => m.ModuleId == a.Id && m.Category == "App" && m.DeleteMark == null && m.ModuleId != null).Count(),
                category = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.Id == a.Category).Select(d => d.FullName),
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account))
            }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        foreach (var item in data.list.Where(x => x.isRelease.IsNullOrEmpty())) if (item.pcIsRelease > 0 || item.appIsRelease > 0) item.isRelease = 1;
        return PageResult<VisualDevListOutput>.SqlSugarPageResult(data);

    }

    /// <summary>
    /// 获取功能列表下拉框.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] VisualDevSelectorInput input)
    {
        var webType = input.webType.IsNotEmptyOrNull() ? input.webType.Split(',').ToObject<List<int>>() : new List<int>();
        var data = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().Where(v => v.Type == input.type && v.DeleteMark == null).
            WhereIF(webType.Any(), v => webType.Contains(v.WebType)).
            OrderBy(a => a.Category).OrderBy(a => a.SortCode).ToListAsync();
        List<VisualDevSelectorOutput>? output = data.Adapt<List<VisualDevSelectorOutput>>();
        IEnumerable<string>? parentIds = output.Select(x => x.ParentId).ToList().Distinct();
        List<VisualDevSelectorOutput>? pList = new List<VisualDevSelectorOutput>();
        List<DictionaryDataEntity>? parentData = await _visualDevRepository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(d => parentIds.Contains(d.Id) && d.DeleteMark == null).OrderBy(x => x.SortCode).ToListAsync();
        foreach (DictionaryDataEntity? item in parentData)
        {
            VisualDevSelectorOutput? pData = item.Adapt<VisualDevSelectorOutput>();
            pData.ParentId = "-1";
            pList.Add(pData);
        }

        return new { list = output.Union(pList).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取功能信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        VisualDevEntity? data = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        return data.Adapt<VisualDevInfoOutput>();
    }

    /// <summary>
    /// 获取表单主表属性下拉框.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="filterType">1：过滤指定控件.</param>
    /// <returns></returns>
    [HttpGet("{id}/FormDataFields")]
    public async Task<dynamic> GetFormDataFields(string id, [FromQuery] int filterType)
    {
        var templateEntity = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        TemplateParsingBase? tInfo = new TemplateParsingBase(templateEntity.Adapt<VisualDevEntity>()); // 解析模板
        List<FieldsModel>? fieldsModels = tInfo.SingleFormData.FindAll(x => x.VModel.IsNotEmptyOrNull() && !PoxiaoKeyConst.RELATIONFORM.Equals(x.Config.poxiaoKey));
        if (filterType.Equals(1))
        {
            fieldsModels = fieldsModels.FindAll(x => !PoxiaoKeyConst.UPLOADIMG.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.UPLOADFZ.Equals(x.Config.poxiaoKey)
                        && !PoxiaoKeyConst.MODIFYUSER.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.MODIFYTIME.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.LINK.Equals(x.Config.poxiaoKey)
                        && !PoxiaoKeyConst.BUTTON.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.ALERT.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.PoxiaoTEXT.Equals(x.Config.poxiaoKey)
                        && !PoxiaoKeyConst.BARCODE.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.QRCODE.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.TABLE.Equals(x.Config.poxiaoKey)
                        && !PoxiaoKeyConst.CREATEUSER.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.CREATETIME.Equals(x.Config.poxiaoKey) && !PoxiaoKeyConst.BILLRULE.Equals(x.Config.poxiaoKey)
                        && !PoxiaoKeyConst.POPUPSELECT.Equals(x.Config.poxiaoKey));
        }

        List<VisualDevFormDataFieldsOutput>? output = fieldsModels.Select(x => new VisualDevFormDataFieldsOutput()
        {
            label = x.Config.label,
            vmodel = x.VModel
        }).ToList();
        return new { list = output };
    }

    /// <summary>
    /// 获取表单主表属性列表.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("{id}/FieldDataSelect")]
    public async Task<dynamic> GetFieldDataSelect(string id, [FromQuery] VisualDevDataFieldDataListInput input)
    {
        Dictionary<string, object> queryDic = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(input.relationField) && !string.IsNullOrWhiteSpace(input.Keyword)) queryDic.Add(input.relationField, input.Keyword);
        VisualDevEntity? templateEntity = await GetInfoById(id, true); // 取数据
        TemplateParsingBase? tInfo = new TemplateParsingBase(templateEntity); // 解析模板

        // 指定查询字段
        if (input.IsNotEmptyOrNull() && input.columnOptions.IsNotEmptyOrNull())
        {
            List<string>? showFieldList = input.columnOptions.Split(',').ToList(); // 显示的所有 字段
            List<FieldsModel>? flist = new List<FieldsModel>();
            List<IndexGridFieldModel>? clist = new List<IndexGridFieldModel>();

            // 获取 调用 该功能表单 的功能模板
            FieldsModel? smodel = tInfo.FieldsModelList.Where(x => x.VModel == input.relationField).First();
            smodel.searchType = 2;
            flist.Add(smodel); // 添加 关联查询字段
            if (tInfo.ColumnData == null)
            {
                tInfo.ColumnData = new ColumnDesignModel()
                {
                    columnList = new List<IndexGridFieldModel>() { new IndexGridFieldModel() { prop = input.relationField, label = input.relationField } },
                    searchList = new List<IndexSearchFieldModel>() { smodel.Adapt<IndexSearchFieldModel>() }
                };
            }

            if (!tInfo.ColumnData.columnList.Where(x => x.prop == input.relationField).Any())
                tInfo.ColumnData.columnList.Add(new IndexGridFieldModel() { prop = input.relationField, label = input.relationField });
            if (tInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && tInfo.FieldsModelList.Any(x => x.VModel == tInfo.ColumnData?.defaultSidx))
                flist.Add(tInfo.FieldsModelList.Where(x => x.VModel == tInfo.ColumnData?.defaultSidx).FirstOrDefault()); // 添加 关联排序字段

            tInfo.FieldsModelList.ForEach(item =>
            {
                if (showFieldList.Find(x => x == item.VModel) != null) flist.Add(item);
            });
            clist.Add(tInfo.ColumnData.columnList.Where(x => x.prop == input.relationField).FirstOrDefault()); // 添加 关联查询字段
            if (tInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && tInfo.FieldsModelList.Any(x => x.VModel == tInfo.ColumnData?.defaultSidx))
                clist.Add(tInfo.ColumnData.columnList.Where(x => x.prop == tInfo.ColumnData?.defaultSidx).FirstOrDefault()); // 添加 关联排序字段
            showFieldList.ForEach(item =>
            {
                if (!tInfo.ColumnData.columnList.Where(x => x.prop == item).Any())
                    clist.Add(new IndexGridFieldModel() { prop = item, label = item });
                else
                    clist.Add(tInfo.ColumnData.columnList.Find(x => x.prop == item));
            });

            if (flist.Count > 0)
            {
                tInfo.FormModel.fields = flist.Distinct().ToList();
                templateEntity.FormData = tInfo.FormModel.ToJsonString();
            }

            if (clist.Count > 0)
            {
                tInfo.ColumnData.columnList = clist.Distinct().ToList();
                templateEntity.ColumnData = tInfo.ColumnData.ToJsonString();
            }
        }

        // 获取值 无分页
        VisualDevModelListQueryInput listQueryInput = new VisualDevModelListQueryInput
        {
            QueryJson = queryDic.ToJsonString(),
            CurrentPage = input.CurrentPage > 0 ? input.CurrentPage : 1,
            PageSize = input.PageSize > 0 ? input.PageSize : 20,
            dataType = "1",
            Sidx = tInfo.ColumnData.defaultSidx,
            Sort = tInfo.ColumnData.sort
        };

        return await _runService.GetRelationFormList(templateEntity, listQueryInput, "List");
    }

    /// <summary>
    /// 回滚模板.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}/Actions/RollbackTemplate")]
    public async Task RollbackTemplate(string id)
    {
        var vREntity = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().FirstAsync(x => x.Id.Equals(id) && x.DeleteMark == null);
        if (vREntity == null) throw Oops.Oh(ErrorCode.D1415);
        VisualDevEntity? entity = vREntity.Adapt<VisualDevEntity>();
        entity.State = 1;
        await _visualDevRepository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建功能信息.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] VisualDevCrInput input)
    {
        VisualDevEntity? entity = input.Adapt<VisualDevEntity>();
        try
        {
            // 验证名称和编码是否重复
            if (await _visualDevRepository.IsAnyAsync(x => x.DeleteMark == null && x.Type == input.type && (x.FullName == input.fullName || x.EnCode == input.enCode))) throw Oops.Oh(ErrorCode.D1406);

            if (input.formData.IsNotEmptyOrNull())
            {
                TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
                if (!tInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板
                await VerifyPrimaryKeyPolicy(tInfo, entity.DbLinkId); // 验证雪花Id 和自增长Id 主键是否支持
            }
            _db.BeginTran(); // 开启事务
            entity.State = 0;

            // 添加功能
            entity = await _visualDevRepository.AsSugarClient().Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();

            // 同步流程相关
            if (entity.EnableFlow.Equals(1) && (!entity.Type.Equals(3) && !entity.Type.Equals(4)))
            {
                var fEntity = entity.Adapt<FlowFormEntity>();
                fEntity.PropertyJson = entity.FormData;
                fEntity.TableJson = entity.Tables;
                fEntity.DraftJson = fEntity.ToJsonString();
                fEntity.FlowType = 1;
                fEntity.FormType = 2;
                fEntity.FlowId = entity.Id;
                await _visualDevRepository.AsSugarClient().Insertable(fEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
                await SaveFlowTemplate(entity);
            }

            await SyncField(entity);
            _db.CommitTran(); // 提交事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id</param>
    /// <param name="input">参数</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] VisualDevUpInput input)
    {
        VisualDevEntity? entity = input.Adapt<VisualDevEntity>();
        try
        {
            if (!input.webType.Equals(4) && await _visualDevRepository.AsQueryable().AnyAsync(x => x.Id.Equals(id) && x.State.Equals(1)) && (entity.Tables.IsNullOrEmpty() || entity.Tables.Equals("[]")))
                throw Oops.Oh(ErrorCode.D1416); // 已发布的模板  表不能为空.

            // 验证名称和编码是否重复
            if (await _visualDevRepository.IsAnyAsync(x => x.DeleteMark == null && x.Id != entity.Id && x.Type == input.type && (x.FullName == input.fullName || x.EnCode == input.enCode))) throw Oops.Oh(ErrorCode.D1406);

            if (input.formData.IsNotEmptyOrNull())
            {
                TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
                if (!tInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板
                await VerifyPrimaryKeyPolicy(tInfo, entity.DbLinkId); // 验证雪花Id 和自增长Id 主键是否支持
            }
            _db.BeginTran(); // 开启事务

            // 修改功能
            await _visualDevRepository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            // 同步流程相关
            if (entity.EnableFlow.Equals(1) && (!entity.Type.Equals(3) && !entity.Type.Equals(4)))
            {
                var fEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(x => x.Id.Equals(id));
                if (fEntity != null)
                {
                    // EnabledMark=0 未发布，EnabledMark=1 已发布
                    if (fEntity.EnabledMark.Equals(0))
                    {
                        fEntity.FullName = entity.FullName;
                        fEntity.EnCode = entity.EnCode;
                        fEntity.TableJson = entity.Tables;
                        fEntity.PropertyJson = entity.FormData;
                        fEntity.DraftJson = fEntity.ToJsonString();
                    }
                    else
                    {
                        var dEntity = fEntity.Copy();
                        dEntity.TableJson = entity.Tables;
                        dEntity.PropertyJson = entity.FormData;
                        fEntity = new FlowFormEntity();
                        fEntity.DraftJson = dEntity.ToJsonString();
                        fEntity.Id = id;
                    }
                    fEntity.FlowType = 1;
                    fEntity.FormType = 2;
                    fEntity.FlowId = id;

                    await _visualDevRepository.AsSugarClient().Updateable(fEntity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    await SaveFlowTemplate(entity);
                }
                else
                {
                    fEntity = entity.Adapt<FlowFormEntity>();
                    fEntity.PropertyJson = entity.FormData;
                    fEntity.TableJson = entity.Tables;
                    fEntity.DraftJson = fEntity.ToJsonString();
                    fEntity.FlowType = 1;
                    fEntity.FormType = 2;
                    fEntity.FlowId = id;
                    await _visualDevRepository.AsSugarClient().Insertable(fEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
                    await SaveFlowTemplate(entity);
                }
            }

            await SyncField(entity);
            _db.CommitTran(); // 关闭事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        await _visualDevRepository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

        // 同步删除线上版本
        var rEntity = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        if (rEntity != null) await _visualDevRepository.AsSugarClient().Updateable(rEntity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

        // 同步删除流程表单
        var fEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        if (fEntity != null)
        {
            await _visualDevRepository.AsSugarClient().Updateable(fEntity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            await _visualDevRepository.AsSugarClient().Deleteable<FlowFormRelationEntity>().Where(x => x.FormId.Equals(fEntity.FlowId)).ExecuteCommandAsync();
        }

        // 同步删除流程引擎
        var tEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowTemplateEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        if (tEntity != null)
        {
            // 功能流程存在发起的流程，流程设计不应该删除.
            if (!await _visualDevRepository.AsSugarClient().Queryable<FlowTaskEntity>().AnyAsync(ft => ft.TemplateId.Equals(tEntity.Id) && ft.DeleteMark == null))
                await _visualDevRepository.AsSugarClient().Updateable(tEntity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        string? random = new Random().NextLetterAndNumberString(5);
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        if (entity.State.Equals(1) && (!entity.Type.Equals(3) && !entity.Type.Equals(4)))
        {
            var vREntity = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
            entity = vREntity.Adapt<VisualDevEntity>();
            entity.State = 0;
        }

        entity.FullName = entity.FullName + "副本" + random;
        entity.EnCode += random;
        entity.State = 0;
        entity.Id = null; // 复制的数据需要把Id清空，否则会主键冲突错误
        entity.LastModifyTime = null;

        if (entity.EnableFlow.Equals(1))
        {
            DictionaryDataEntity? categoryData = await _dictionaryDataService.GetInfo(entity.Category);
            FlowEngineEntity? flowEngine = new FlowEngineEntity();
            flowEngine.FlowTemplateJson = entity.FlowTemplateJson;
            flowEngine.EnCode = "#visualDev" + entity.EnCode;
            flowEngine.Type = 1;
            flowEngine.FormType = 2;
            flowEngine.FullName = entity.FullName;
            flowEngine.Category = categoryData.EnCode;
            flowEngine.VisibleType = 0;
            flowEngine.Icon = "icon-ym icon-ym-node";
            flowEngine.IconBackground = "#008cff";
            flowEngine.Tables = entity.Tables;
            flowEngine.DbLinkId = entity.DbLinkId;
            flowEngine.FormTemplateJson = entity.FormData;
            flowEngine.Version = "1";
            flowEngine.LastModifyTime = null;
            try
            {
                // 添加流程引擎
                FlowEngineEntity? engineEntity = await _visualDevRepository.AsSugarClient().Insertable(flowEngine).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync();
                entity.Id = engineEntity.Id;
                entity = await _visualDevRepository.AsSugarClient().Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
            }
            catch
            {
                if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
                else throw;
            }
        }
        else
        {
            try
            {
                await _visualDevRepository.AsSugarClient().Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            }
            catch
            {
                if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
                else throw;
            }
        }

        // 同步流程相关
        if (entity.EnableFlow.Equals(1) && (!entity.Type.Equals(3) && !entity.Type.Equals(4)))
        {
            var fEntity = entity.Adapt<FlowFormEntity>();
            fEntity.PropertyJson = entity.FormData;
            fEntity.TableJson = entity.Tables;
            fEntity.DraftJson = fEntity.ToJsonString();
            fEntity.FlowType = 1;
            fEntity.FormType = 2;
            fEntity.FlowId = entity.Id;
            await _visualDevRepository.AsSugarClient().Insertable(fEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
            await SaveFlowTemplate(entity);
        }
    }

    /// <summary>
    /// 功能同步菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Release")]
    public async Task FuncToMenu(string id, [FromBody] VisualDevToMenuInput input)
    {
        input.id = id;
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == input.id);
        input.pcSystemId = input.pcSystemId.IsNullOrEmpty() ? _userManager.User.SystemId : input.pcSystemId;
        input.appSystemId = input.appSystemId.IsNullOrEmpty() ? _userManager.User.AppSystemId : input.appSystemId;
        input.pcModuleParentId = input.pcModuleParentId.IsNullOrWhiteSpace() ? "-1" : input.pcModuleParentId;
        input.appModuleParentId = input.appModuleParentId.IsNullOrWhiteSpace() ? "-1" : input.appModuleParentId;

        //if (entity.State == 0) throw Oops.Oh(ErrorCode.D1405);
        if (entity.FormData.IsNullOrEmpty() && !entity.WebType.Equals(4)) throw Oops.Oh(ErrorCode.COM1013);
        if ((entity.WebType.Equals(2) || entity.WebType.Equals(4)) && entity.ColumnData.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.COM1014);

        #region 旧的菜单、权限数据
        var oldWebModule = await _visualDevRepository.AsSugarClient().Queryable<ModuleEntity>().FirstAsync(x => x.ModuleId == input.id && x.Category == "Web" && x.DeleteMark == null);
        var oldWebModuleButtonEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleButtonEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();
        var oldWebModuleColumnEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleColumnEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();
        var oldWebModuleFormEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleFormEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();

        var oldAppModule = await _visualDevRepository.AsSugarClient().Queryable<ModuleEntity>().FirstAsync(x => x.ModuleId == input.id && x.Category == "App" && x.DeleteMark == null);
        var oldAppModuleButtonEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleButtonEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
        var oldAppModuleColumnEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleColumnEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
        var oldAppModuleFormEntity = await _visualDevRepository.AsSugarClient().Queryable<ModuleFormEntity>().Where(x => x.DeleteMark == null)
            .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
        #endregion

        var oldWebId = oldWebModule != null ? oldWebModule.Id : "null";
        if (_visualDevRepository.AsSugarClient().Queryable<ModuleEntity>().Any(x => (x.EnCode == entity.EnCode || x.FullName == entity.FullName) && x.Id != oldWebId && x.SystemId == input.pcSystemId && x.Category == "Web" && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1015);
        var oldAppId = oldAppModule != null ? oldAppModule.Id : "null";
        if (_visualDevRepository.AsSugarClient().Queryable<ModuleEntity>().Any(x => (x.EnCode == entity.EnCode || x.FullName == entity.FullName) && x.Id != oldAppId && x.SystemId == input.appSystemId && x.Category == "App" && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1015);

        #region 数据视图
        if (entity.WebType.Equals(4))
        {
            #region 菜单组装
            var moduleModel = new ModuleEntity();
            moduleModel.Id = oldWebModule != null ? oldWebModule.Id : SnowflakeIdHelper.NextId();
            moduleModel.ModuleId = input.id;
            moduleModel.ParentId = oldWebModule != null ? oldWebModule.ParentId : (input.pcModuleParentId.Equals(input.pcSystemId) ? "-1" : input.pcModuleParentId); // 父级菜单节点
            moduleModel.Category = "Web";
            moduleModel.FullName = entity.FullName;
            moduleModel.EnCode = entity.EnCode;
            moduleModel.Icon = oldWebModule != null ? oldWebModule.Icon : "icon-ym icon-ym-webForm";
            moduleModel.UrlAddress = oldWebModule != null ? oldWebModule.UrlAddress : "model/" + entity.EnCode;
            moduleModel.Type = 3;
            moduleModel.EnabledMark = 1;
            moduleModel.IsColumnAuthorize = 1;
            moduleModel.IsButtonAuthorize = 1;
            moduleModel.IsFormAuthorize = 1;
            moduleModel.IsDataAuthorize = 1;
            moduleModel.SortCode = oldWebModule != null ? oldWebModule.SortCode : 999;
            moduleModel.CreatorTime = DateTime.Now;
            moduleModel.PropertyJson = new { moduleId = input.id, iconBackgroundColor = string.Empty, isTree = 0 }.ToJsonString();
            moduleModel.SystemId = oldWebModule != null ? oldWebModule.SystemId : input.pcSystemId;

            #endregion

            // 添加PC菜单
            if (input.pc == 1)
            {
                var storModuleModel = _visualDevRepository.AsSugarClient().Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新
            }

            // 添加App菜单
            if (input.app == 1)
            {
                #region App菜单
                moduleModel.Id = oldAppModule != null ? oldAppModule.Id : SnowflakeIdHelper.NextId();
                moduleModel.ModuleId = input.id;
                moduleModel.ParentId = oldAppModule != null ? oldAppModule.ParentId : (input.appModuleParentId.Equals(input.appSystemId) ? "-1" : input.appModuleParentId); // 父级菜单节点
                moduleModel.Category = "App";
                moduleModel.UrlAddress = oldAppModule != null ? oldAppModule.UrlAddress : "/pages/apply/dynamicModel/index?id=" + entity.EnCode;
                moduleModel.SystemId = oldAppModule != null ? oldAppModule.SystemId : input.appSystemId;
                moduleModel.SortCode = oldAppModule != null ? oldAppModule.SortCode : 999;
                moduleModel.Icon = oldAppModule != null ? oldAppModule.Icon : "icon-ym icon-ym-webForm";

                #endregion

                var storModuleModel = _visualDevRepository.AsSugarClient().Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新

            }

            // 修改功能发布状态
            await _visualDevRepository.AsUpdateable().SetColumns(it => it.State == 1).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();

            // 线上版本
            if (!await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().AnyAsync(x => x.Id.Equals(id)))
            {
                var vReleaseEntity = entity.Adapt<VisualDevReleaseEntity>();
                await _visualDevRepository.AsSugarClient().Insertable(vReleaseEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
            }
            else
            {
                var vReleaseEntity = entity.Adapt<VisualDevReleaseEntity>();
                await _visualDevRepository.AsSugarClient().Updateable(vReleaseEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            }

            return;
        }
        #endregion

        TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板

        // 无表转有表
        if (!tInfo.IsHasTable)
        {
            string? mTableName = "mt" + entity.Id; // 主表名称
            VisualDevEntity? res = await NoTblToTable(entity, mTableName);
            if (res != null)
                await _visualDevRepository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            else
                throw Oops.Oh(ErrorCode.D1414);
            tInfo = new TemplateParsingBase(res); // 解析模板
            entity = res;
        }

        ColumnDesignModel? columnData = new ColumnDesignModel();

        // 列配置模型
        if (!string.IsNullOrWhiteSpace(entity.ColumnData))
        {
            columnData = entity.ColumnData.ToObject<ColumnDesignModel>();
        }
        else
        {
            columnData = new ColumnDesignModel()
            {
                btnsList = new List<ButtonConfigModel>(),
                columnBtnsList = new List<ButtonConfigModel>(),
                customBtnsList = new List<ButtonConfigModel>(),
                columnList = new List<IndexGridFieldModel>(),
                defaultColumnList = new List<IndexGridFieldModel>()
            };
        }

        columnData.btnsList = columnData.btnsList.Union(columnData.columnBtnsList).ToList();
        if (columnData.customBtnsList != null && columnData.customBtnsList.Any()) columnData.btnsList = columnData.btnsList.Union(columnData.customBtnsList).ToList();

        ColumnDesignModel? appColumnData = new ColumnDesignModel();

        // App列配置模型
        if (!string.IsNullOrWhiteSpace(entity.AppColumnData))
        {
            appColumnData = tInfo.AppColumnData;
        }
        else
        {
            appColumnData = new ColumnDesignModel()
            {
                btnsList = new List<ButtonConfigModel>(),
                columnBtnsList = new List<ButtonConfigModel>(),
                customBtnsList = new List<ButtonConfigModel>(),
                columnList = new List<IndexGridFieldModel>(),
                defaultColumnList = new List<IndexGridFieldModel>()
            };
        }

        appColumnData.btnsList = appColumnData.btnsList.Union(appColumnData.columnBtnsList).ToList();
        if (appColumnData.customBtnsList != null && appColumnData.customBtnsList.Any()) appColumnData.btnsList = appColumnData.btnsList.Union(appColumnData.customBtnsList).ToList();

        try
        {
            _db.BeginTran();

            #region 菜单组装
            var moduleModel = new ModuleEntity();
            moduleModel.Id = oldWebModule != null ? oldWebModule.Id : SnowflakeIdHelper.NextId();
            moduleModel.ModuleId = input.id;
            moduleModel.ParentId = oldWebModule != null ? oldWebModule.ParentId : (input.pcModuleParentId.Equals(input.pcSystemId) ? "-1" : input.pcModuleParentId); // 父级菜单节点
            moduleModel.Category = "Web";
            moduleModel.FullName = entity.FullName;
            moduleModel.EnCode = entity.EnCode;
            moduleModel.Icon = oldWebModule != null ? oldWebModule.Icon : "icon-ym icon-ym-webForm";
            moduleModel.UrlAddress = oldWebModule != null ? oldWebModule.UrlAddress : "model/" + entity.EnCode;
            moduleModel.Type = 3;
            moduleModel.EnabledMark = 1;
            moduleModel.IsColumnAuthorize = 1;
            moduleModel.IsButtonAuthorize = 1;
            moduleModel.IsFormAuthorize = 1;
            moduleModel.IsDataAuthorize = 1;
            moduleModel.SortCode = oldWebModule != null ? oldWebModule.SortCode : 999;
            moduleModel.CreatorTime = DateTime.Now;
            moduleModel.PropertyJson = new { moduleId = input.id, iconBackgroundColor = string.Empty, isTree = 0 }.ToJsonString();
            moduleModel.SystemId = oldWebModule != null ? oldWebModule.SystemId : input.pcSystemId;

            #endregion
            #region 配置权限

            // 按钮权限
            var btnAuth = new List<ModuleButtonEntity>();
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_add", FullName = "新增", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_download", FullName = "导出", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_upload", FullName = "导入", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_batchRemove", FullName = "批量删除", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_edit", FullName = "编辑", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_remove", FullName = "删除", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_detail", FullName = "详情", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_batchPrint", FullName = "批量打印", ModuleId = moduleModel.Id });
            columnData.customBtnsList.ForEach(item =>
            {
                btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 1, SortCode = 0, ParentId = "-1", EnCode = item.value, FullName = item.label, ModuleId = moduleModel.Id });
            });

            columnData.btnsList.ForEach(item =>
            {
                var aut = btnAuth.Find(x => x.FullName == item.label);
                if (aut != null) aut.EnabledMark = 1;
            });

            // 表单权限
            var columnAuth = new List<ModuleColumnEntity>();
            var fieldList = tInfo.AllFieldsModel;
            var formAuth = new List<ModuleFormEntity>();

            var ctList = tInfo.AllFieldsModel.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE).ToList();
            var childTableIndex = new Dictionary<string, string>();
            for (var i = 0; i < ctList.Count; i++) childTableIndex.Add(ctList[i].VModel, ctList[i].Config.label + (i + 1));

            fieldList = fieldList.Where(x => x.Config.poxiaoKey != PoxiaoKeyConst.TABLE).ToList();
            fieldList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(item =>
            {
                var fRule = item.VModel.Contains("_poxiao_") ? 1 : 0;
                fRule = item.VModel.ToLower().Contains("tablefield") && item.VModel.Contains("-") ? 2 : fRule;
                var ctName = item.VModel.Split("-");
                formAuth.Add(new ModuleFormEntity()
                {
                    ParentId = "-1",
                    EnCode = item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ChildTableKey = fRule.Equals(2) ? ctName.FirstOrDefault() : string.Empty,
                    FieldRule = fRule,
                    ModuleId = moduleModel.Id,
                    FullName = fRule.Equals(2) ? childTableIndex[item.VModel.Split('-').First()] + "-" + item.Config.label : item.Config.label,
                    EnabledMark = 1,
                    SortCode = 0
                });
            });
            ctList.ForEach(item =>
            {

                formAuth.Add(new ModuleFormEntity()
                {
                    ParentId = "-1",
                    EnCode = item.VModel,
                    BindTable = tInfo.MainTableName,
                    ChildTableKey = item.VModel,
                    FieldRule = 0,
                    ModuleId = moduleModel.Id,
                    FullName = childTableIndex[item.VModel],
                    EnabledMark = 1,
                    SortCode = 0
                });
            });

            // 列表权限
            columnData.defaultColumnList.ForEach(item =>
            {
                var itemModel = fieldList.FirstOrDefault(x => x.Config.poxiaoKey == item.poxiaoKey && x.VModel == item.prop);
                if (itemModel != null)
                {
                    var fRule = itemModel.VModel.Contains("_poxiao_") ? 1 : 0;
                    fRule = itemModel.VModel.ToLower().Contains("tablefield") && itemModel.VModel.Contains("-") ? 2 : fRule;
                    var ctName = item.VModel.Split("-");
                    columnAuth.Add(new ModuleColumnEntity()
                    {
                        ParentId = "-1",
                        EnCode = itemModel.VModel,
                        BindTable = fRule.Equals(2) ? itemModel.Config.relationTable : itemModel.Config.tableName,
                        ChildTableKey = fRule.Equals(2) ? itemModel.VModel.Split("-").FirstOrDefault() : string.Empty,
                        FieldRule = fRule,
                        ModuleId = moduleModel.Id,
                        FullName = fRule.Equals(2) ? childTableIndex[item.VModel.Split('-').First()] + item.Config.label.Replace(item.Config.label.Split("-").First(), string.Empty) : item.Config.label,
                        EnabledMark = 0,
                        SortCode = 0
                    });
                }
            });

            columnData.columnList.ForEach(item =>
            {
                var aut = columnAuth.Find(x => x.EnCode == item.prop);
                if (aut != null) aut.EnabledMark = 1;
            });

            #endregion

            // 添加PC菜单和权限
            if (input.pc == 1)
            {
                var storModuleModel = _visualDevRepository.AsSugarClient().Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新

                #region 表单权限
                if (columnData.useFormPermission)
                {
                    if (!oldWebModuleFormEntity.Any())
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(formAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else
                    {
                        var formAuthAddList = new List<ModuleFormEntity>();
                        formAuth.ForEach(item =>
                        {
                            if (!oldWebModuleFormEntity.Any(x => x.EnCode == item.EnCode)) formAuthAddList.Add(item);
                        });
                        if (formAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(formAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldWebModuleFormEntity.ForEach(item =>
                        {
                            var it = formAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = 1; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldWebModuleFormEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 按钮权限
                if (columnData.useBtnPermission)
                {
                    if (!oldWebModuleButtonEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(btnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var btnAuthAddList = new List<ModuleButtonEntity>();
                        btnAuth.ForEach(item =>
                        {
                            if (!oldWebModuleButtonEntity.Any(x => x.EnCode == item.EnCode)) btnAuthAddList.Add(item);
                        });
                        if (btnAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(btnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldWebModuleButtonEntity.ForEach(item =>
                        {
                            var it = btnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldWebModuleButtonEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 列表权限
                if (columnData.useColumnPermission)
                {
                    if (!oldWebModuleColumnEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(columnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var columnAuthAddList = new List<ModuleColumnEntity>();
                        columnAuth.ForEach(item =>
                        {
                            if (!oldWebModuleColumnEntity.Any(x => x.EnCode == item.EnCode)) columnAuthAddList.Add(item);
                        });
                        if (columnAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(columnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldWebModuleColumnEntity.ForEach(item =>
                        {
                            var it = columnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldWebModuleColumnEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 数据权限
                if (columnData.useDataPermission)
                {
                    if (!_visualDevRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().Where(x => x.EnCode.Equals("poxiao_alldata") && x.ModuleId == moduleModel.Id && x.DeleteMark == null).Any())
                    {
                        // 全部数据权限方案
                        var allDataAuthScheme = new ModuleDataAuthorizeSchemeEntity()
                        {
                            FullName = "全部数据",
                            EnCode = "poxiao_alldata",
                            AllData = 1,
                            ConditionText = string.Empty,
                            ConditionJson = string.Empty,
                            ModuleId = moduleModel.Id
                        };
                        await _visualDevRepository.AsSugarClient().Insertable(allDataAuthScheme).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
                    }

                    // 创建用户和所属组织权限方案
                    // 只添加 主表控件的数据权限
                    var fList = fieldList.Where(x => !x.VModel.Contains("_poxiao_") && x.VModel.IsNotEmptyOrNull() && x.Config.visibility.Contains("pc"))
                        .Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.CREATEUSER || x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE).ToList();

                    var authList = await MenuMergeDataAuth(moduleModel.Id, fList);
                    await MenuMergeDataAuthScheme(moduleModel.Id, authList, fList);
                }
                #endregion
            }

            #region App菜单、 权限组装
            moduleModel.Id = oldAppModule != null ? oldAppModule.Id : SnowflakeIdHelper.NextId();
            moduleModel.ModuleId = input.id;
            moduleModel.ParentId = oldAppModule != null ? oldAppModule.ParentId : (input.appModuleParentId.Equals(input.appSystemId) ? "-1" : input.appModuleParentId); // 父级菜单节点
            moduleModel.Category = "App";
            moduleModel.UrlAddress = oldAppModule != null ? oldAppModule.UrlAddress : "/pages/apply/dynamicModel/index?id=" + entity.EnCode;
            moduleModel.SystemId = oldAppModule != null ? oldAppModule.SystemId : input.appSystemId;
            moduleModel.SortCode = oldAppModule != null ? oldAppModule.SortCode : 999;
            moduleModel.Icon = oldAppModule != null ? oldAppModule.Icon : "icon-ym icon-ym-webForm";

            btnAuth = new List<ModuleButtonEntity>();
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_add", FullName = "新增", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_edit", FullName = "编辑", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_remove", FullName = "删除", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, SortCode = 0, ParentId = "-1", EnCode = "btn_detail", FullName = "详情", ModuleId = moduleModel.Id });
            appColumnData.customBtnsList.ForEach(item =>
            {
                btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 1, SortCode = 0, ParentId = "-1", EnCode = item.value, FullName = item.label, ModuleId = moduleModel.Id });
            });
            appColumnData.btnsList.ForEach(item =>
            {
                var aut = btnAuth.Find(x => x.FullName == item.label);
                if (aut != null) aut.EnabledMark = 1;
            });

            formAuth.Clear();
            fieldList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(item =>
            {
                var fRule = item.VModel.Contains("_poxiao_") ? 1 : 0;
                fRule = item.VModel.ToLower().Contains("tablefield") && item.VModel.Contains("-") ? 2 : fRule;
                formAuth.Add(new ModuleFormEntity()
                {
                    ParentId = "-1",
                    EnCode = item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ChildTableKey = fRule.Equals(2) ? item.VModel.Split("-").FirstOrDefault() : string.Empty,
                    FieldRule = fRule,
                    ModuleId = moduleModel.Id,
                    FullName = fRule.Equals(2) ? childTableIndex[item.VModel.Split('-').First()] + "-" + item.Config.label : item.Config.label,
                    EnabledMark = 1,
                    SortCode = 0
                });
            });
            ctList.ForEach(item =>
            {

                formAuth.Add(new ModuleFormEntity()
                {
                    ParentId = "-1",
                    EnCode = item.VModel,
                    BindTable = tInfo.MainTableName,
                    ChildTableKey = item.VModel,
                    FieldRule = 0,
                    ModuleId = moduleModel.Id,
                    FullName = childTableIndex[item.VModel],
                    EnabledMark = 1,
                    SortCode = 0
                });
            });

            columnAuth.Clear();
            appColumnData.defaultColumnList.ForEach(item =>
            {
                var itemModel = fieldList.FirstOrDefault(x => x.Config.poxiaoKey == item.poxiaoKey && x.VModel == item.prop);
                if (itemModel != null)
                {
                    var fRule = itemModel.VModel.Contains("_poxiao_") ? 1 : 0;
                    fRule = itemModel.VModel.ToLower().Contains("tablefield") && itemModel.VModel.Contains("-") ? 2 : fRule;
                    columnAuth.Add(new ModuleColumnEntity()
                    {
                        ParentId = "-1",
                        EnCode = itemModel.VModel,
                        BindTable = fRule.Equals(2) ? itemModel.Config.relationTable : itemModel.Config.tableName,
                        ChildTableKey = fRule.Equals(2) ? itemModel.VModel.Split("-").FirstOrDefault() : string.Empty,
                        FieldRule = fRule,
                        ModuleId = moduleModel.Id,
                        FullName = fRule.Equals(2) ? childTableIndex[item.VModel.Split('-').First()] + item.Config.label.Replace(item.Config.label.Split("-").First(), string.Empty) : item.Config.label,
                        EnabledMark = 0,
                        SortCode = 0
                    });
                }
            });
            appColumnData.columnList.ForEach(item =>
            {
                var aut = columnAuth.Find(x => x.EnCode == item.prop);
                if (aut != null) aut.EnabledMark = 1;
            });

            columnAuth.ForEach(item => { item.ModuleId = moduleModel.Id; });
            #endregion

            // 添加App菜单和权限
            if (input.app == 1)
            {
                var storModuleModel = _visualDevRepository.AsSugarClient().Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新

                #region 表单权限
                if (appColumnData.useFormPermission)
                {
                    if (!oldAppModuleFormEntity.Any())
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(formAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else
                    {
                        var formAuthAddList = new List<ModuleFormEntity>();
                        formAuth.ForEach(item =>
                        {
                            if (!oldAppModuleFormEntity.Any(x => x.EnCode == item.EnCode)) formAuthAddList.Add(item);
                        });
                        if (formAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(formAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldAppModuleFormEntity.ForEach(item =>
                        {
                            var it = formAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = 1; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldAppModuleFormEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 按钮权限
                if (appColumnData.useBtnPermission)
                {
                    if (!oldAppModuleButtonEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(btnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var btnAuthAddList = new List<ModuleButtonEntity>();
                        btnAuth.ForEach(item =>
                        {
                            if (!oldAppModuleButtonEntity.Any(x => x.EnCode == item.EnCode)) btnAuthAddList.Add(item);
                        });
                        if (btnAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(btnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldAppModuleButtonEntity.ForEach(item =>
                        {
                            var it = btnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldAppModuleButtonEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 列表权限
                if (appColumnData.useColumnPermission)
                {
                    if (!oldAppModuleColumnEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.AsSugarClient().Insertable(columnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var columnAuthAddList = new List<ModuleColumnEntity>();
                        columnAuth.ForEach(item =>
                        {
                            if (!oldAppModuleColumnEntity.Any(x => x.EnCode == item.EnCode)) columnAuthAddList.Add(item);
                        });
                        if (columnAuthAddList.Any()) await _visualDevRepository.AsSugarClient().Insertable(columnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldAppModuleColumnEntity.ForEach(item =>
                        {
                            var it = columnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it != null) item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.AsSugarClient().Updateable(oldAppModuleColumnEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 数据权限
                if (appColumnData.useDataPermission)
                {
                    // 全部数据权限
                    if (!_visualDevRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>().Where(x => x.EnCode.Equals("poxiao_alldata") && x.ModuleId == moduleModel.Id && x.DeleteMark == null).Any())
                    {
                        // 全部数据权限方案
                        var allDataAuthScheme = new ModuleDataAuthorizeSchemeEntity()
                        {
                            FullName = "全部数据",
                            EnCode = "poxiao_alldata",
                            AllData = 1,
                            ConditionText = string.Empty,
                            ConditionJson = string.Empty,
                            ModuleId = moduleModel.Id
                        };
                        await _visualDevRepository.AsSugarClient().Insertable(allDataAuthScheme).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
                    }

                    // 创建用户和所属组织权限方案
                    // 只添加 主表控件的数据权限
                    var fList = fieldList.Where(x => !x.VModel.Contains("_poxiao_") && x.VModel.IsNotEmptyOrNull() && x.Config.visibility.Contains("app"))
                        .Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.CREATEUSER || x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE).ToList();

                    var authList = await MenuMergeDataAuth(moduleModel.Id, fList);
                    await MenuMergeDataAuthScheme(moduleModel.Id, authList, fList);
                }
                #endregion
            }

            // 修改功能发布状态
            await _visualDevRepository.AsUpdateable().SetColumns(it => it.State == 1).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();

            // 线上版本
            if (!await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().AnyAsync(x => x.Id.Equals(id)))
            {
                var vReleaseEntity = entity.Adapt<VisualDevReleaseEntity>();
                await _visualDevRepository.AsSugarClient().Insertable(vReleaseEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

                // 同步添加流程表单
                if (entity.EnableFlow.Equals(1))
                {
                    if (!_visualDevRepository.AsSugarClient().Queryable<FlowTemplateJsonEntity>().Any(x => x.TemplateId.Equals(entity.Id))) throw Oops.Oh(ErrorCode.D1421);
                    var fEntity = entity.Adapt<FlowFormEntity>();
                    fEntity.TableJson = entity.Tables;
                    fEntity.FlowType = 1;
                    fEntity.FormType = 2;
                    fEntity.EnabledMark = 1;
                    fEntity.PropertyJson = entity.FormData;
                    fEntity.DraftJson = fEntity.ToJsonString();
                    await _visualDevRepository.AsSugarClient().Updateable(fEntity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    await _visualDevRepository.AsSugarClient().Updateable<FlowTemplateEntity>().SetColumns(x => x.EnabledMark == 1).Where(it => it.Id == entity.Id).ExecuteCommandHasChangeAsync();
                }
            }
            else
            {
                var vReleaseEntity = entity.Adapt<VisualDevReleaseEntity>();
                await _visualDevRepository.AsSugarClient().Updateable(vReleaseEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

                if (entity.EnableFlow.Equals(1))
                {
                    if (!_visualDevRepository.AsSugarClient().Queryable<FlowTemplateJsonEntity>().Any(x => x.TemplateId.Equals(entity.Id))) throw Oops.Oh(ErrorCode.D1421);
                    var fEntity = entity.Adapt<FlowFormEntity>();
                    fEntity.TableJson = entity.Tables;
                    fEntity.FlowType = 1;
                    fEntity.FormType = 2;
                    fEntity.EnabledMark = 1;
                    fEntity.PropertyJson = entity.FormData;
                    fEntity.DraftJson = fEntity.ToJsonString();
                    if (!await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().AnyAsync(x => x.Id.Equals(id)))
                        await _visualDevRepository.AsSugarClient().Insertable(fEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
                    else
                        await _visualDevRepository.AsSugarClient().Updateable(fEntity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

                    await _visualDevRepository.AsSugarClient().Updateable<FlowTemplateEntity>().SetColumns(x => x.EnabledMark == 1).Where(it => it.Id == entity.Id).ExecuteCommandHasChangeAsync();
                }
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取功能信息.
    /// </summary>
    /// <param name="id">主键ID.</param>
    /// <param name="isGetRelease">是否获取发布版本.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<VisualDevEntity> GetInfoById(string id, bool isGetRelease = false)
    {
        if (isGetRelease)
        {
            var vREntity = await _visualDevRepository.AsSugarClient().Queryable<VisualDevReleaseEntity>().FirstAsync(x => x.Id == id && x.DeleteMark == null);
            if (vREntity != null && vREntity.EnableFlow == 1 && _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().Any(x => x.Id.Equals(id)))
            {
                vREntity.FlowId = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().Where(x => x.Id.Equals(id)).Select(x => x.FlowId).FirstAsync();
                if (vREntity.FlowId.IsNotEmptyOrNull())
                {
                    if (!_visualDevRepository.AsSugarClient().Queryable<FlowTemplateEntity>().Where(x => x.Id.Equals(vREntity.FlowId) && x.EnabledMark.Equals(1)).Any())
                        vREntity.EnableFlow = -1;
                }
            }
            return vREntity.Adapt<VisualDevEntity>();
        }
        else
        {
            var vEntity = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == id && x.DeleteMark == null);
            if (_visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().Any(x => x.Id.Equals(id)))
                vEntity.FlowId = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().Where(x => x.Id.Equals(id)).Select(x => x.FlowId).FirstAsync();
            return vEntity.Adapt<VisualDevEntity>();
        }
    }

    /// <summary>
    /// 判断功能ID是否存在.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetDataExists(string id)
    {
        return await _visualDevRepository.IsAnyAsync(it => it.Id == id && it.DeleteMark == null);
    }

    /// <summary>
    /// 判断是否存在编码、名称相同的数据.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetDataExists(string enCode, string fullName)
    {
        return await _visualDevRepository.IsAnyAsync(it => it.EnCode == enCode && it.FullName == fullName && it.DeleteMark == null);
    }

    /// <summary>
    /// 新增导入数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task CreateImportData(VisualDevEntity input)
    {
        try
        {
            _db.BeginTran(); // 开启事务
            input.State = 0;
            StorageableResult<VisualDevEntity>? stor = _visualDevRepository.AsSugarClient().Storageable(input).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
            await stor.AsInsertable.ExecuteCommandAsync(); // 执行插入
            await _visualDevRepository.AsSugarClient().Updateable(input).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            // 同步流程表单
            var fEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowFormEntity>().FirstAsync(v => v.Id == input.Id);
            if (fEntity != null)
            {
                fEntity.DeleteMark = null;
                fEntity.DeleteTime = null;
                fEntity.DeleteUserId = null;
                await _visualDevRepository.AsSugarClient().Updateable(fEntity).CallEntityMethod(m => m.LastModify()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId, it.LastModifyTime, it.LastModifyUserId }).ExecuteCommandAsync();
            }

            // 同步程引擎
            var tEntity = await _visualDevRepository.AsSugarClient().Queryable<FlowTemplateEntity>().FirstAsync(v => v.Id == input.Id);
            if (tEntity != null)
            {
                tEntity.DeleteMark = null;
                tEntity.DeleteTime = null;
                tEntity.DeleteUserId = null;
                await _visualDevRepository.AsSugarClient().Updateable(tEntity).CallEntityMethod(m => m.LastModify()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId, it.LastModifyTime, it.LastModifyUserId }).ExecuteCommandAsync();
            }

            _db.CommitTran(); // 关闭事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 功能模板 无表 转 有表.
    /// </summary>
    /// <param name="vEntity">功能实体.</param>
    /// <param name="mainTableName">主表名称.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<VisualDevEntity> NoTblToTable(VisualDevEntity vEntity, string mainTableName)
    {
        var dbtype = _visualDevRepository.AsSugarClient().CurrentConnectionConfig.DbType.ToString();
        var isUpper = false; // 是否大写
        if (dbtype.ToLower().Equals("oracle") || dbtype.ToLower().Equals("dm") || dbtype.ToLower().Equals("dm8")) isUpper = true;
        else isUpper = false;

        // Oracle和Dm数据库 表名全部大写, 其他全部小写
        mainTableName = isUpper ? mainTableName.ToUpper() : mainTableName.ToLower();

        FormDataModel formModel = vEntity.FormData.ToObjectOld<FormDataModel>();
        List<FieldsModel>? fieldsModelList = TemplateAnalysis.AnalysisTemplateData(formModel.fields);

        #region 创表信息组装

        List<DbTableAndFieldModel>? addTableList = new List<DbTableAndFieldModel>(); // 表集合

        // 主表信息
        DbTableAndFieldModel? mainInfo = new DbTableAndFieldModel();
        mainInfo.table = mainTableName;
        mainInfo.tableName = vEntity.FullName;
        mainInfo.FieldList = FieldsModelToTableFile(fieldsModelList, formModel.primaryKeyPolicy == 2);
        if (vEntity.EnableFlow.Equals(1)) mainInfo.FieldList.Add(new DbTableFieldModel() { field = "F_FlowTaskId", fieldName = "流程任务Id", dataType = "varchar", dataLength = "50", allowNull = 1 });
        if (vEntity.EnableFlow.Equals(1)) mainInfo.FieldList.Add(new DbTableFieldModel() { field = "F_FlowId", fieldName = "流程引擎Id", dataType = "varchar", dataLength = "50", allowNull = 1 });
        if (formModel.logicalDelete) mainInfo.FieldList.Add(new DbTableFieldModel() { field = "F_DeleteMark", fieldName = "删除标识", dataType = "int", dataLength = "50", allowNull = 1 });
        if (_tenant.MultiTenancy && _tenant.MultiTenancyType.Equals("COLUMN")) mainInfo.FieldList.Add(new DbTableFieldModel() { field = "f_tenantid", fieldName = "租户Id", dataType = "varchar", dataLength = "50", allowNull = 1 });

        // 子表信息
        Dictionary<string, string>? childTableDic = new Dictionary<string, string>();
        fieldsModelList.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE).ToList().ForEach(item =>
        {
            DbTableAndFieldModel? childTInfo = new DbTableAndFieldModel();
            childTInfo.table = "ct" + SnowflakeIdHelper.NextId();
            childTInfo.table = isUpper ? childTInfo.table.ToUpper() : childTInfo.table.ToLower();
            childTableDic.Add(item.VModel, childTInfo.table);
            childTInfo.tableName = vEntity.FullName + "_子表";
            childTInfo.FieldList = FieldsModelToTableFile(item.Config.children, formModel.primaryKeyPolicy == 2);
            childTInfo.FieldList.Add(new DbTableFieldModel() { dataLength = "50", allowNull = 1, dataType = "varchar", field = "F_Relation_Id", fieldName = vEntity.FullName + "_关联外键" });
            addTableList.Add(childTInfo);
        });

        #endregion

        #region 修改功能模板 有表改无表
        List<TableModel>? modelTableList = new List<TableModel>();

        // 处理主表
        TableModel? mainTable = new TableModel();
        mainTable.fields = new List<EntityFieldModel>();
        mainTable.table = mainInfo.table;
        mainTable.tableName = mainInfo.tableName;
        mainTable.typeId = "1";
        mainInfo.FieldList.ForEach(item => // 表字段
        {
            EntityFieldModel? etFieldModel = new EntityFieldModel();
            etFieldModel.DataLength = item.dataLength;
            etFieldModel.PrimaryKey = 1;
            etFieldModel.DataType = item.dataType;
            etFieldModel.Field = item.field;
            etFieldModel.FieldName = item.fieldName;
            mainTable.fields.Add(etFieldModel);
        });

        // 处理子表
        addTableList.ForEach(item =>
        {
            TableModel? childInfo = new TableModel();
            childInfo.fields = new List<EntityFieldModel>();
            childInfo.table = item.table;
            childInfo.tableName = item.tableName;
            childInfo.tableField = "F_Relation_Id"; // 关联外键
            childInfo.relationField = "F_Id"; // 关联主键
            childInfo.typeId = "0";
            item.FieldList.ForEach(it => // 子表字段
            {
                EntityFieldModel? etFieldModel = new EntityFieldModel();
                etFieldModel.DataLength = it.dataLength;
                etFieldModel.PrimaryKey = it.primaryKey.ParseToInt();
                etFieldModel.DataType = it.dataType;
                etFieldModel.Field = it.field;
                etFieldModel.FieldName = it.fieldName;
                childInfo.fields.Add(etFieldModel);
            });
            modelTableList.Add(childInfo);
        });
        modelTableList.Add(mainTable);

        #region 给控件绑定 tableName、relationTable 属性

        // 用字典反序列化， 避免多增加不必要的属性
        Dictionary<string, object>? dicFormModel = vEntity.FormData.ToObjectOld<Dictionary<string, object>>();
        List<Dictionary<string, object>>? dicFieldsModelList = dicFormModel.FirstOrDefault(x => x.Key == "fields").Value.ToJsonString().ToObjectOld<List<Dictionary<string, object>>>();

        // 给控件绑定 tableName
        FieldBindTable(dicFieldsModelList, childTableDic, mainTableName);

        #endregion

        dicFormModel["fields"] = dicFieldsModelList; // 修改表单控件
        vEntity.FormData = dicFormModel.ToJsonString(); // 修改模板
        vEntity.Tables = modelTableList.ToJsonString(); // 修改模板涉及表

        addTableList.Add(mainInfo);

        #endregion

        try
        {
            _db.BeginTran(); // 执行事务
            var link = await _runService.GetDbLink(vEntity.DbLinkId);
            foreach (DbTableAndFieldModel? item in addTableList)
            {
                bool res = await _changeDataBase.Create(link, item, item.FieldList);
                if (!res) throw null;
            }

            if (await _visualDevRepository.IsAnyAsync(x => x.Id.Equals(vEntity.Id)))
                await _visualDevRepository.AsUpdateable(vEntity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            else
                await _visualDevRepository.AsInsertable(vEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

            _db.CommitTran(); // 提交事务

            return vEntity;
        }
        catch (Exception e)
        {
            _db.RollbackTran(); // 回滚事务
            return null;
        }
    }

    #endregion

    #region Private

    /// <summary>
    /// 组件转换表字段.
    /// </summary>
    /// <param name="fmList">表单列表.</param>
    /// <param name="isIdentity">主键是否自增长.</param>
    /// <returns></returns>
    [NonAction]
    private List<DbTableFieldModel> FieldsModelToTableFile(List<FieldsModel> fmList, bool isIdentity)
    {
        List<DbTableFieldModel>? fieldList = new List<DbTableFieldModel>(); // 表字段
        List<FieldsModel>? mList = fmList.Where(x => x.Config.poxiaoKey.IsNotEmptyOrNull())
            .Where(x => x.Config.poxiaoKey != PoxiaoKeyConst.QRCODE && x.Config.poxiaoKey != PoxiaoKeyConst.BARCODE && x.Config.poxiaoKey != PoxiaoKeyConst.TABLE).ToList(); // 非存储字段
        fieldList.Add(new DbTableFieldModel()
        {
            primaryKey = true,
            dataType = isIdentity ? "int" : "varchar",
            dataLength = "50",
            identity = isIdentity,
            field = "F_Id",
            fieldName = "主键"
        });

        foreach (var item in mList)
        {
            // 不生成数据库字段(控件类型为：展示数据)，关联表单、弹窗选择、计算公式.
            if ((item.Config.poxiaoKey == PoxiaoKeyConst.RELATIONFORMATTR || item.Config.poxiaoKey == PoxiaoKeyConst.POPUPATTR || item.Config.poxiaoKey == PoxiaoKeyConst.CALCULATE) && item.Config.isStorage.Equals(1))
                continue;
            DbTableFieldModel? field = new DbTableFieldModel();
            field.field = item.VModel;
            field.fieldName = item.Config.label;
            switch (item.Config.poxiaoKey)
            {
                case PoxiaoKeyConst.NUMINPUT:
                    field.dataType = item.precision == 0 ? "int" : "decimal";
                    field.dataLength = "38";
                    field.decimalDigits = item.precision.IsNullOrEmpty() ? 15 : item.precision;
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.DATE:
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.TIME:
                    field.dataType = "varchar";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.CREATETIME:
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.MODIFYTIME:
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.EDITOR:
                    field.dataType = "text";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.CALCULATE:
                    field.dataType = "decimal";
                    field.dataLength = "38";
                    field.decimalDigits = item.precision.IsNullOrEmpty() ? 15 : item.precision;
                    field.allowNull = 1;
                    break;
                case PoxiaoKeyConst.UPLOADFZ:
                case PoxiaoKeyConst.UPLOADIMG:
                    field.dataType = "text";
                    field.dataLength = "500";
                    field.allowNull = 1;
                    break;
                default:
                    field.dataType = "varchar";
                    field.dataLength = "500";
                    field.allowNull = 1;
                    break;
            }

            if (field.field.IsNotEmptyOrNull()) fieldList.Add(field);
        }

        return fieldList;
    }

    /// <summary>
    /// 组装菜单 数据权限 字段管理数据.
    /// </summary>
    /// <param name="menuId">菜单ID.</param>
    /// <param name="fields">功能模板控件集合.</param>
    /// <returns></returns>
    private async Task<List<ModuleDataAuthorizeEntity>> MenuMergeDataAuth(string menuId, List<FieldsModel> fields)
    {
        // 旧的自动生成的 字段管理
        List<ModuleDataAuthorizeEntity>? oldDataAuth = await _visualDevRepository.AsSugarClient().Queryable<ModuleDataAuthorizeEntity>()
            .Where(x => x.ModuleId == menuId && x.DeleteMark == null && x.SortCode.Equals(-9527))
            .Where(x => x.ConditionText == "@organizationAndSuborganization" || x.ConditionText == "@organizeId"
            || x.ConditionText == "@userAraSubordinates" || x.ConditionText == "@userId"
            || x.ConditionText == "@branchManageOrganizeAndSub" || x.ConditionText == "@branchManageOrganize")
            .ToListAsync();

        List<ModuleDataAuthorizeEntity>? authList = new List<ModuleDataAuthorizeEntity>(); // 字段管理
        List<ModuleDataAuthorizeEntity>? noDelData = new List<ModuleDataAuthorizeEntity>(); // 记录未删除

        // 当前用户
        FieldsModel? item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CREATEUSER);
        if (item != null)
        {
            var fRule = item.VModel.Contains("_poxiao_") ? 1 : 0;
            fRule = item.VModel.ToLower().Contains("tablefield") && item.VModel.Contains("-") ? 2 : fRule;

            // 新增
            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@userId"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@userId", // 条件内容（当前用户）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@userAraSubordinates"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@userAraSubordinates", // 条件内容（当前用户及下属）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.EnCode != item.VModel && (x.ConditionText == "@userId" || x.ConditionText == "@userAraSubordinates")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();

            noDelData = oldDataAuth.Except(delData).ToList(); // 记录未删除
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.ConditionText == "@userId" || x.ConditionText == "@userAraSubordinates").ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        // 所属组织
        item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE);
        if (item != null)
        {
            var fRule = item.VModel.Contains("_poxiao_") ? 1 : 0;
            fRule = item.VModel.ToLower().Contains("tablefield") && item.VModel.Contains("-") ? 2 : fRule;

            // 新增
            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@organizeId"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@organizeId", // 条件内容（当前组织）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@organizationAndSuborganization"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@organizationAndSuborganization", // 条件内容（当前组织及组织）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.EnCode != item.VModel && (x.ConditionText == "@organizeId" || x.ConditionText == "@organizationAndSuborganization")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();

            noDelData = oldDataAuth.Except(delData).ToList(); // 记录未删除
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.ConditionText == "@organizeId" || x.ConditionText == "@organizationAndSuborganization").ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        // 当前分管组织
        item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE);
        if (item != null)
        {
            var fRule = item.VModel.Contains("_poxiao_") ? 1 : 0;
            fRule = item.VModel.ToLower().Contains("tablefield") && item.VModel.Contains("-") ? 2 : fRule;

            // 新增
            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@branchManageOrganize"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@branchManageOrganize", // 条件内容（当前分管组织）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            if (!oldDataAuth.Any(x => x.EnCode == item.VModel && x.ConditionText == "@branchManageOrganizeAndSub"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.Config.label, // 字段说明
                    ConditionText = "@branchManageOrganizeAndSub", // 条件内容（当前分管组织及子组织）
                    EnabledMark = 1,
                    SortCode = -9527,
                    FieldRule = fRule, // 主表/副表/子表
                    EnCode = fRule.Equals(1) ? item.VModel.Split("poxiao_").LastOrDefault() : item.VModel,
                    BindTable = fRule.Equals(2) ? item.Config.relationTable : item.Config.tableName,
                    ModuleId = menuId
                });
            }

            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.EnCode != item.VModel && (x.ConditionText == "@branchManageOrganize" || x.ConditionText == "@branchManageOrganizeAndSub")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();

            noDelData = oldDataAuth.Except(delData).ToList(); // 记录未删除
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.ConditionText == "@branchManageOrganize" || x.ConditionText == "@branchManageOrganizeAndSub").ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        if (authList.Any()) await _visualDevRepository.AsSugarClient().Insertable(authList).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        if (noDelData.Any()) authList.AddRange(noDelData);
        return authList.Any() ? authList : oldDataAuth;
    }

    /// <summary>
    /// 组装菜单 数据权限 方案管理数据.
    /// </summary>
    /// <param name="menuId">菜单ID.</param>
    /// <param name="authList">字段管理列表.</param>
    /// <param name="fields">功能模板控件集合.</param>
    /// <returns></returns>
    private async Task MenuMergeDataAuthScheme(string menuId, List<ModuleDataAuthorizeEntity> authList, List<FieldsModel> fields)
    {
        // 旧的自动生成的 方案管理
        List<ModuleDataAuthorizeSchemeEntity>? oldDataAuthScheme = await _visualDevRepository.AsSugarClient().Queryable<ModuleDataAuthorizeSchemeEntity>()
            .Where(x => x.ModuleId == menuId && x.DeleteMark == null)
            .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganize\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganizeAndSub\""))
            .ToListAsync();

        List<ModuleDataAuthorizeSchemeEntity>? authSchemeList = new List<ModuleDataAuthorizeSchemeEntity>(); // 方案管理

        // 当前用户
        FieldsModel? item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CREATEUSER);
        var condJson = new AuthorizeModuleResourceConditionModelInput()
        {
            logic = "and",
            groups = new List<AuthorizeModuleResourceConditionItemModelInput>() { new AuthorizeModuleResourceConditionItemModelInput() { id = "", bindTable = "", field = "", fieldRule = 0, value = "", type = "varchar", op = "Equal" } }
        };

        if (item != null)
        {
            ModuleDataAuthorizeEntity? model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@userId"));

            if (model != null)
            {
                condJson.groups.First().id = model.Id;
                condJson.groups.First().bindTable = model.BindTable;
                condJson.groups.First().field = item.VModel;
                condJson.groups.First().fieldRule = model.FieldRule.ParseToInt();
                condJson.groups.First().value = "@userId";

                // 新增
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@userId}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前用户",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@userId}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@userAraSubordinates"));
                condJson.groups.First().id = model.Id;
                condJson.groups.First().op = "Equal";
                condJson.groups.First().value = "@userAraSubordinates";
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@userAraSubordinates}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前用户及下属",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@userAraSubordinates}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                // 删除
                //List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme.Where(x => x.EnCode != item.__vModel__
                //&& (x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\""))).ToList();
                //await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
            else
            {
                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                    .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")).ToList();
                await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        // 当前组织
        item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE);
        if (item != null)
        {
            ModuleDataAuthorizeEntity? model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@organizeId"));

            if (model != null)
            {
                condJson.groups.First().id = model.Id;
                condJson.groups.First().bindTable = model.BindTable;
                condJson.groups.First().field = item.VModel;
                condJson.groups.First().fieldRule = model.FieldRule.ParseToInt();
                condJson.groups.First().op = "Equal";
                condJson.groups.First().value = "@organizeId";

                // 新增
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@organizeId}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前组织",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@organizeId}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@organizationAndSuborganization"));
                condJson.groups.First().id = model.Id;
                condJson.groups.First().op = "Equal";
                condJson.groups.First().value = "@organizationAndSuborganization";
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@organizationAndSuborganization}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前组织及子组织",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@organizationAndSuborganization}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                // 删除
                //List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme.Where(x => x.EnCode != item.__vModel__
                //&& (x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\""))).ToList();
                //await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
            else
            {
                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                    .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"")).ToList();
                await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        // 当前分管组织
        item = fields.FirstOrDefault(x => x.Config.poxiaoKey == PoxiaoKeyConst.CURRORGANIZE);
        if (item != null)
        {
            ModuleDataAuthorizeEntity? model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@branchManageOrganize"));

            if (model != null)
            {
                condJson.groups.First().id = model.Id;
                condJson.groups.First().bindTable = model.BindTable;
                condJson.groups.First().field = item.VModel;
                condJson.groups.First().fieldRule = model.FieldRule.ParseToInt();
                condJson.groups.First().op = "Equal";
                condJson.groups.First().value = "@branchManageOrganize";

                // 新增
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@branchManageOrganize}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前分管组织",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@branchManageOrganize}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                model = authList.FirstOrDefault(x => x.EnCode == item.VModel && x.ConditionText.Equals("@branchManageOrganizeAndSub"));
                condJson.groups.First().id = model.Id;
                condJson.groups.First().op = "Equal";
                condJson.groups.First().value = "@branchManageOrganizeAndSub";
                if (!oldDataAuthScheme.Any(x => x.ConditionText == "【{" + item.Config.label + "} {等于} {@branchManageOrganizeAndSub}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前分管组织及子组织",
                        EnCode = SnowflakeIdHelper.NextId(),
                        SortCode = -9527,
                        ConditionText = "【{" + item.Config.label + "} {等于} {@branchManageOrganizeAndSub}】",
                        ConditionJson = new List<AuthorizeModuleResourceConditionModelInput>() { condJson }.ToJsonString(),
                        ModuleId = menuId
                    });
                }

                // 删除
                //List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme.Where(x => x.EnCode != item.__vModel__
                //&& (x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganize\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganizeAndSub\""))).ToList();
                //await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
            else
            {
                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                    .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganize\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganizeAndSub\"")).ToList();
                await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
            }
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganize\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@branchManageOrganizeAndSub\"")).ToList();
            await _visualDevRepository.AsSugarClient().Deleteable(delData).ExecuteCommandAsync();
        }

        if (authSchemeList.Any()) await _visualDevRepository.AsSugarClient().Insertable(authSchemeList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无限递归 给控件绑定tableName (绕过 布局控件).
    /// </summary>
    private void FieldBindTable(List<Dictionary<string, object>> dicFieldsModelList, Dictionary<string, string> childTableDic, string tableName)
    {
        foreach (var item in dicFieldsModelList)
        {
            var obj = item["__config__"].ToObject<Dictionary<string, object>>();

            if (obj.ContainsKey("poxiaoKey") && obj["poxiaoKey"].Equals(PoxiaoKeyConst.TABLE)) obj["tableName"] = childTableDic[item["__vModel__"].ToString()];
            else if (obj.ContainsKey("tableName")) obj["tableName"] = tableName;

            // 关联表单属性和弹窗属性
            if (obj.ContainsKey("poxiaoKey") && (obj["poxiaoKey"].Equals(PoxiaoKeyConst.RELATIONFORMATTR) || obj["poxiaoKey"].Equals(PoxiaoKeyConst.POPUPATTR)))
            {
                string relationField = Convert.ToString(item["relationField"]);
                string? rField = relationField.ReplaceRegex(@"_poxiaoTable_(\w+)", string.Empty);
                item["relationField"] = string.Format("{0}{1}{2}{3}", rField, "_poxiaoTable_", tableName, "1");
            }

            // 子表控件
            if (obj.ContainsKey("poxiaoKey") && obj["poxiaoKey"].Equals(PoxiaoKeyConst.TABLE))
            {
                var cList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                foreach (var child in cList)
                {
                    var cObj = child["__config__"].ToObject<Dictionary<string, object>>();
                    if (cObj.ContainsKey("relationTable")) cObj["relationTable"] = childTableDic[item["__vModel__"].ToString()];
                    else cObj.Add("relationTable", childTableDic[item["__vModel__"].ToString()]);

                    if (cObj.ContainsKey("tableName")) cObj["tableName"] = tableName;

                    // 关联表单属性和弹窗属性
                    if (cObj.ContainsKey("poxiaoKey") && (cObj["poxiaoKey"].Equals(PoxiaoKeyConst.RELATIONFORMATTR) || cObj["poxiaoKey"].Equals(PoxiaoKeyConst.POPUPATTR)))
                    {
                        string relationField = Convert.ToString(child["relationField"]);
                        string? rField = relationField.ReplaceRegex(@"_poxiaoTable_(\w+)", string.Empty);
                        if (child.ContainsKey("relationField")) child["relationField"] = string.Format("{0}{1}{2}{3}", rField, "_poxiaoTable_", cObj["tableName"], "0");
                        else child.Add("relationField", string.Format("{0}{1}{2}{3}", rField, "_poxiaoTable_", cObj["tableName"], "0"));
                    }

                    child["__config__"] = cObj;
                }

                obj["children"] = cList;
            }

            // 递归
            if (obj.ContainsKey("children"))
            {
                var fmList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                FieldBindTable(fmList, childTableDic, tableName);
                obj["children"] = fmList;
            }

            item["__config__"] = obj;
        }
    }

    /// <summary>
    /// 验证主键策略 数据库表是否支持.
    /// </summary>
    /// <param name="tInfo">模板信息.</param>
    /// <param name="dbLinkId">数据库连接id.</param>
    private async Task VerifyPrimaryKeyPolicy(TemplateParsingBase tInfo, string dbLinkId)
    {
        if (tInfo.IsHasTable)
        {
            DbLinkEntity link = await _runService.GetDbLink(dbLinkId);
            tInfo.AllTable.ForEach(item =>
            {
                List<DbTableFieldModel>? tableList = _changeDataBase.GetFieldList(link, item.table); // 获取主表所有列
                var mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
                if (mainPrimary == null) throw Oops.Oh(ErrorCode.D1409, "主键为空", item.table);

                if (tInfo.FormModel.primaryKeyPolicy.Equals(2) && !mainPrimary.identity)
                {
                    throw Oops.Oh(ErrorCode.D1409, "自增长ID,没有自增标识", item.table);
                }
                if (tInfo.FormModel.primaryKeyPolicy.Equals(1) && !(mainPrimary.dataType.ToLower().Equals("string") || mainPrimary.dataType.ToLower().Equals("varchar") || mainPrimary.dataType.ToLower().Equals("nvarchar")))
                    throw Oops.Oh(ErrorCode.D1409, "雪花ID", item.table);
            });
        }
    }

    /// <summary>
    /// 同步到流程相关.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task SaveFlowTemplate(VisualDevEntity input)
    {
        if (!(await _visualDevRepository.AsSugarClient().Queryable<FlowTemplateEntity>().AnyAsync(x => x.Id.Equals(input.Id))))
        {
            if (await _visualDevRepository.AsSugarClient().Queryable<FlowTemplateEntity>().AnyAsync(x => (x.EnCode == input.EnCode || x.FullName == input.FullName) && x.DeleteMark == null))
                throw Oops.Oh(ErrorCode.COM1004);
            var dictionaryTypeEntity = await _visualDevRepository.AsSugarClient().Queryable<DictionaryTypeEntity>().FirstAsync(x => x.EnCode == "WorkFlowCategory" && x.DeleteMark == null);
            var dicType = await _visualDevRepository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(x => x.Id.Equals(input.Category)).FirstAsync();
            var flowType = await _visualDevRepository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(x => x.EnCode.Equals(dicType.EnCode) && x.DictionaryTypeId.Equals(dictionaryTypeEntity.Id)).FirstAsync();
            if (flowType == null) flowType = await _visualDevRepository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(x => x.DictionaryTypeId.Equals(dictionaryTypeEntity.Id)).FirstAsync();

            var flowTemplateEntity = input.Adapt<FlowTemplateEntity>();
            flowTemplateEntity.EnabledMark = 0;
            flowTemplateEntity.Type = 1;
            flowTemplateEntity.Category = flowType.Id;
            //flowTemplateEntity.IconBackground = "#008cff";.
            //flowTemplateEntity.Icon = "icon-ym icon-ym-node";

            var result = await _visualDevRepository.AsSugarClient().Insertable(flowTemplateEntity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
            if (result == null)
                throw Oops.Oh(ErrorCode.COM1005);
        }
    }

    /// <summary>
    /// 同步业务字段.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task SyncField(VisualDevEntity entity)
    {
        if (entity.Tables.IsNotEmptyOrNull() && !entity.Tables.Equals("[]") && entity.FormData.IsNotEmptyOrNull())
        {
            TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
            DbLinkEntity link = await _runService.GetDbLink(entity.DbLinkId);
            tInfo.DbLink = link;
            await _runService.SyncField(tInfo);
        }
    }
    #endregion
}
