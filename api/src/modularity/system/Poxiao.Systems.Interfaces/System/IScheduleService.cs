using Poxiao.Systems.Entitys.Entity.System;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 业务契约：门户日程
/// 版 本：V3.4.6
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-04-24.
/// </summary>
public interface IScheduleService
{
    /// <summary>
    /// 日程当天推送列表.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    Task<List<ScheduleEntity>> GetCalendarDayPushList(string tenantId);

    /// <summary>
    /// 添加推送任务队列.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="enCode"></param>
    /// <param name="type"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    Task AddPushTaskQueue(ScheduleEntity entity, string enCode, string type, string tenantId);

    /// <summary>
    /// 发送日程消息.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="userList"></param>
    /// <param name="type"></param>
    /// <param name="enCode"></param>
    /// <returns></returns>
    Task SendScheduleMsg(ScheduleEntity entity, List<string> userList, string type, string enCode);
}