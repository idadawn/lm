using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extend.Entitys;
using Poxiao.Extend.Entitys.Dto.Schedule;
using Poxiao.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Extend;

/// <summary>
/// 项目计划
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "Schedule", Order = 600)]
[Route("api/extend/[controller]")]
public class ScheduleService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ScheduleEntity> _repository;
    private readonly IUserManager _userManager;

    public ScheduleService(
        ISqlSugarRepository<ScheduleEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">参数</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ScheduleListQuery input)
    {
        var data = await _repository.AsQueryable()
            .Where(x => x.CreatorUserId == _userManager.UserId && x.StartTime >= input.startTime.ParseToDateTime() && x.EndTime <= input.endTime.ParseToDateTime() && x.DeleteMark == null).OrderBy(x => x.StartTime, OrderByType.Desc).ToListAsync();
        var output = data.Adapt<List<ScheduleListOutput>>();
        return new { list = output };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<ScheduleInfoOutput>();
    }

    /// <summary>
    /// app.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("AppList")]
    public async Task<dynamic> GetAppList([FromQuery] ScheduleListQuery input)
    {
        var days = new Dictionary<string, int>();
        var data = await _repository.AsQueryable()
            .Where(x => x.CreatorUserId == _userManager.UserId && x.StartTime >= input.startTime.ParseToDateTime() && x.EndTime <= input.endTime.ParseToDateTime() && x.DeleteMark == null)
            .OrderBy(x => x.StartTime, OrderByType.Desc).ToListAsync();
        var output = data.Adapt<List<ScheduleListOutput>>();
        foreach (var item in GetAllDays(input.startTime.ParseToDateTime(), input.endTime.ParseToDateTime()))
        {
            var _startTime = item.ToString("yyyy-MM-dd") + " 23:59";
            var _endTime = item.ToString("yyyy-MM-dd") + " 00:00";
            var count = output.FindAll(m => m.startTime <= _startTime.ParseToDateTime() && m.endTime >= _endTime.ParseToDateTime()).Count;
            days.Add(item.ToString("yyyyMMdd"), count);
        }
        var today_startTime = input.dateTime + " 23:59";
        var today_endTime = input.dateTime + " 00:00";
        return new
        {
            signList = days,
            todayList = output.FindAll(m => m.startTime <= today_startTime.ParseToDateTime() && m.endTime >= today_endTime.ParseToDateTime())
        };
    }

    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ScheduleCrInput input)
    {
        var entity = input.Adapt<ScheduleEntity>();
        var isOk = await _repository.AsSugarClient().Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ScheduleUpInput input)
    {
        var entity = input.Adapt<ScheduleEntity>();
        var isOk = await _repository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取固定日期范围内的所有日期，以数组形式返回.
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    private DateTime[] GetAllDays(DateTime startTime, DateTime endTime)
    {
        var listDay = new List<DateTime>();
        DateTime dtDay = new DateTime();
        //循环比较，取出日期；
        for (dtDay = startTime; dtDay.CompareTo(endTime) <= 0; dtDay = dtDay.AddDays(1))
        {
            listDay.Add(dtDay);
        }
        return listDay.ToArray();
    }

    #endregion
}