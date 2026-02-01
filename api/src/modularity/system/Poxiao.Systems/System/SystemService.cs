using Aop.Api.Domain;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Security;
using Poxiao.LinqBuilder;
using Poxiao.Systems.Entitys.Dto.System.System;
using Poxiao.Systems.Entitys.Entity.System;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using SqlSugar;

namespace Poxiao.Systems.System;

/// <summary>
/// 系统功能.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "System", Order = 200)]
[Route("api/system/[controller]")]
public class SystemService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SystemEntity> _repository;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// IM中心处理程序.
    /// </summary>
    private IMHandler _imHandler;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="SystemService"/>类型的新实例.
    /// </summary>
    public SystemService(
        ISqlSugarRepository<SystemEntity> repository,
        ICacheManager cacheManager,
        IMHandler imHandler,
        IUserManager userManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _imHandler = imHandler;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] SystemQuery input)
    {
        var authorIds = await _repository.AsSugarClient().Queryable<AuthorizeEntity>()
            .Where(x => x.ItemType.Equals("system") && x.ObjectType.Equals("Role") && _userManager.Roles.Contains(x.ObjectId)).Select(x => x.ItemId).ToListAsync();

        var whereLambda = LinqExpression.And<SystemEntity>();
        whereLambda = whereLambda.And(x => x.DeleteMark == null);
        if (!_userManager.IsAdministrator)
            whereLambda = whereLambda.And(x => authorIds.Contains(x.Id));
        if (input.Keyword.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.FullName.Contains(input.Keyword) || x.EnCode.Contains(input.Keyword));
        if (input.enableMark.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.EnabledMark == SqlFunc.ToInt32(input.enableMark));
        var output = (await _repository.AsQueryable().Where(whereLambda).OrderBy(a => a.SortCode).OrderByDescending(a => a.CreatorTime).ToListAsync()).Adapt<List<SystemListOutput>>();
        return new { list = output };
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        return data.Adapt<SystemCrInput>();
    }
    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] SystemCrInput input)
    {
        if (await _repository.IsAnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<SystemEntity>();
        var isOk = await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] SystemCrInput input)
    {
        if (await _repository.IsAnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);

        var mainSystem = await _repository.GetFirstAsync(it => it.IsMain.Equals(1) && it.EnabledMark.Equals(1) && it.DeleteMark == null);

        // 判断主系统是否被禁用.
        if (input.id.Equals(mainSystem.Id) && input.enabledMark.Equals(0))
            throw Oops.Oh(ErrorCode.D1036);

        // 判断主系统是否有修改系统编码.
        if (input.id.Equals(mainSystem.Id) && !input.enCode.Equals(mainSystem.EnCode))
            throw Oops.Oh(ErrorCode.D1037);

        var isOk = await _repository.AsUpdateable()
            .SetColumns(it => new SystemEntity()
            {
                FullName = input.fullName,
                EnCode = input.enCode,
                Icon = input.icon,
                PropertyJson = input.propertyJson,
                SortCode = input.sortCode,
                Description = input.description,
                EnabledMark = input.enabledMark,
                LastModifyUserId = _userManager.UserId,
                LastModifyTime = SqlFunc.GetDate(),
            })
            .Where(it => it.Id.Equals(id))
            .ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1001);

        // 当用户的子系统被禁用时，提醒在线用户.
        if (!input.id.Equals(mainSystem.Id) && input.enabledMark.Equals(0))
        {
            var systemUser = await _repository.AsSugarClient().Queryable<UserEntity>()
                .Where(it => it.DeleteMark == null && (input.id.Equals(it.SystemId) || input.id.Equals(it.AppSystemId)))
                .ToListAsync();

            var upUserList = new List<UserEntity>();
            systemUser.ForEach(item =>
            {
                item.SystemId = mainSystem.Id;
                item.AppSystemId = mainSystem.Id;
                item.LastModifyTime = DateTime.Now;
                item.LastModifyUserId = _userManager.UserId;
                upUserList.Add(item);
            });

            // 更新用户的系统id.
            var res = await _repository.AsSugarClient().Updateable(upUserList)
                .UpdateColumns(it => new
                {
                    it.SystemId,
                    it.AppSystemId,
                    it.LastModifyTime,
                    it.LastModifyUserId
                }).ExecuteCommandAsync();

            var tenantId = _userManager.TenantId;
            var cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
            var allUserOnlineList = await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);

            var userOnlineList = allUserOnlineList.FindAll(it => systemUser.Select(s => s.Id).ToList().Contains(it.userId));
            userOnlineList.ForEach(async item =>
            {
                await _imHandler.SendMessageAsync(item.connectionId, new { method = "refresh" }.ToJsonString());
            });
        }
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (await _repository.AsSugarClient().Queryable<CommonWordsEntity>().AnyAsync(x => x.SystemIds.Contains(id) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1021);
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion
}
