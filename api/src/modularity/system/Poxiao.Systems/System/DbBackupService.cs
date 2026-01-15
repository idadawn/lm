using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.LinqBuilder;
using Poxiao.Systems.Entitys.Dto.DbBackup;
using Poxiao.Systems.Entitys.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Systems;

/// <summary>
/// 数据备份
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataBackup", Order = 207)]
[Route("api/system/[controller]")]
public class DbBackupService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DbBackupEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DbBackupService"/>类型的新实例.
    /// </summary>
    public DbBackupService(
        ISqlSugarRepository<DbBackupEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var queryWhere = LinqExpression.And<DbBackupEntity>();
        if (!string.IsNullOrEmpty(input.Keyword))
            queryWhere = queryWhere.And(m => m.FileName.Contains(input.Keyword) || m.FilePath.Contains(input.Keyword));
        var list = await _repository.AsQueryable().Where(queryWhere).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToPagedListAsync(input.CurrentPage, input.PageSize);
        var pageList = new SqlSugarPagedList<DbBackupListOutput>()
        {
            list = list.list.Adapt<List<DbBackupListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<DbBackupListOutput>.SqlSugarPageResult(pageList);
    }

    #endregion

    #region POST

    /// <summary>
    /// 创建备份(不支持跨库备份).
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create()
    {
        await DbBackup();
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (await _repository.IsAnyAsync(m => m.Id == id && m.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        await _repository.AsUpdateable().SetColumns(it => new DbBackupEntity()
        {
            DeleteTime = DateTime.Now,
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId
        }).Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 备份数据.
    /// </summary>
    private async Task DbBackup()
    {
        var fileName = SnowflakeIdHelper.NextId() + ".bak";
        var filePath = Path.Combine(FileVariable.DataBackupFilePath, fileName);

        // 备份数据
        var dataBase = App.Configuration["ConnectionStrings:DBName"];
        _repository.AsSugarClient().DbMaintenance.BackupDataBase(dataBase, filePath);

        // 备份记录
        DbBackupEntity entity = new DbBackupEntity();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.CreatorTime = DateTime.Now;
        entity.CreatorUserId = _userManager.UserId;
        entity.EnabledMark = 1;
        entity.FileName = fileName;
        entity.FilePath = "/api/Common/Download?encryption=" + _userManager.UserId + "|" + fileName + "|dataBackup";
        entity.FileSize = new FileInfo(filePath).Length.ToString();
        await _repository.AsInsertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 还原.
    /// </summary>
    /// <param name="disk">路径</param>
    private void DbRestore(string disk)
    {
        var dataBase = App.Configuration["ConnectionStrings:DBName"];
        _repository.AsSugarClient().DbMaintenance.CreateDatabase(dataBase, disk);
    }

    #endregion
}