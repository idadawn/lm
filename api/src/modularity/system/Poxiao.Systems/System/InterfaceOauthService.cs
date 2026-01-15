using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.LinqBuilder;
using Poxiao.Systems.Entitys.Dto.DataInterFace;
using Poxiao.Systems.Entitys.Dto.DataInterfaceLog;
using Poxiao.Systems.Entitys.Dto.System.DataInterfaceLog;
using Poxiao.Systems.Entitys.Dto.System.InterfaceOauth;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems.System;

/// <summary>
/// 接口认证
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "InterfaceOauth", Order = 202)]
[Route("api/system/[controller]")]
public class InterfaceOauthService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基本仓储.
    /// </summary>
    private readonly ISqlSugarRepository<InterfaceOauthEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DictionaryTypeService"/>类型的新实例.
    /// </summary>
    public InterfaceOauthService(
        ISqlSugarRepository<InterfaceOauthEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">请求参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        var output = info.Adapt<InterfaceOauthOutput>();
        if (info.IsNotEmptyOrNull() && info.DataInterfaceIds.IsNotEmptyOrNull())
        {
            var ids = info.DataInterfaceIds.Split(",");
            output.list = await _repository.AsSugarClient().Queryable<DataInterfaceEntity>()
                .Where(a => ids.Contains(a.Id))
                .Select(a => new DataInterfaceListOutput
                {
                    id = a.Id,
                    fullName = a.FullName,
                    enCode = a.EnCode,
                    path = a.Path,
                    requestParameters = a.RequestParameters,
                    dataType = a.DataType,
                    requestMethod = SqlFunc.IF(a.RequestMethod.Equals("1")).Return("新增").ElseIF(a.RequestMethod.Equals("2")).Return("修改")
            .ElseIF(a.RequestMethod.Equals("3")).Return("查询").ElseIF(a.RequestMethod.Equals("4")).Return("删除")
            .ElseIF(a.RequestMethod.Equals("5")).Return("存储过程").ElseIF(a.RequestMethod.Equals("6")).Return("Get")
            .End("Post")
                }).ToListAsync();
        }
        return output;
    }

    /// <summary>
    /// 列表.
    /// </summary>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var list = await _repository.AsSugarClient().Queryable<InterfaceOauthEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
        .Where(a => a.DeleteMark == null)
        .WhereIF(!string.IsNullOrEmpty(input.Keyword), a => a.AppId.Contains(input.Keyword) || a.AppName.Contains(input.Keyword))
        .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
        .Select((a, b) => new InterfaceOauthListOutput
        {
            id = a.Id,
            lastModifyTime = a.LastModifyTime,
            enabledMark = a.EnabledMark,
            creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
            appId = a.AppId,
            appName = a.AppName,
            usefulLife = a.UsefulLife,
            sortCode = a.SortCode,
            creatorTime = a.CreatorTime
        }).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<InterfaceOauthListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取秘钥.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAppSecret")]
    public async Task<dynamic> GetAppSecret()
    {
        return Guid.NewGuid().ToString().Replace("-", string.Empty);
    }

    /// <summary>
    /// 日志.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("dataInterfaceLog/{id}")]
    public async Task<dynamic> GetList(string id, [FromQuery] DataInterfaceLogListQuery input)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        var whereLambda = LinqExpression.And<DataInterfaceLogListOutput>();
        if (!input.startTime.IsNullOrEmpty() && !input.endTime.IsNullOrEmpty())
        {
            var startTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var endTime = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(a => SqlFunc.Between(a.invokTime, startTime, endTime));
        }
        var list = await _repository.AsSugarClient().Queryable<DataInterfaceLogEntity, UserEntity, DataInterfaceEntity>((a, b, c) =>
        new JoinQueryInfos(JoinType.Left, b.Id == a.UserId, JoinType.Left, a.InvokId == c.Id))
             .Where(a => a.OauthAppId == entity.AppId)
             .WhereIF(input.Keyword.IsNotEmptyOrNull(), a => a.UserId.Contains(input.Keyword) || a.InvokIp.Contains(input.Keyword))
            .Select((a, b, c) => new DataInterfaceLogListOutput
            {
                id = a.Id,
                fullName = c.FullName,
                enCode = c.EnCode,
                invokDevice = a.InvokDevice,
                invokIp = a.InvokIp,
                userId = SqlFunc.MergeString(b.RealName, "/", b.Account),
                invokTime = a.InvokTime,
                invokType = a.InvokType,
                invokWasteTime = a.InvokWasteTime
            }).MergeTable().Where(whereLambda).OrderBy(a => a.invokTime).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<DataInterfaceLogListOutput>.SqlSugarPageResult(list);
    }

    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create_Api([FromBody] InterfaceOauthInput input)
    {
        if (await _repository.IsAnyAsync(x => (x.AppId == input.appId || x.AppName == input.appName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3001);
        var entity = input.Adapt<InterfaceOauthEntity>();
        if (input.usefulLife.IsNullOrEmpty() || input.usefulLife == "0")
        {
            entity.UsefulLife = null;
        }
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id">id.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] InterfaceOauthInput input)
    {
        if (await _repository.IsAnyAsync(x => x.Id != id && (x.AppId == input.appId || x.AppName == input.appName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<InterfaceOauthEntity>();
        var isOk = await _repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (input.usefulLife.IsNullOrEmpty() || input.usefulLife == "0")
        {
            await _repository.AsUpdateable().SetColumns(it => new InterfaceOauthEntity()
            {
                UsefulLife = null
            }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        }
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 授权接口.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("saveInterfaceList")]
    public async Task SaveInterFaceList([FromBody] InterfaceOauthSaveInput input)
    {
        var isOk = await _repository.AsSugarClient().Updateable<InterfaceOauthEntity>()
            .SetColumns(it => it.DataInterfaceIds == input.dataInterfaceIds).Where(x => x.Id == input.interfaceIdentId).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion
}