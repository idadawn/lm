using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 数据接口日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_DATAINTERFACELOG")]
public class DataInterfaceLogEntity : OEntityBase<string>
{
    /// <summary>
    /// 调用接口id.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKID")]
    public string InvokId { get; set; }

    /// <summary>
    /// 调用时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKTIME")]
    public DateTime? InvokTime { get; set; }

    /// <summary>
    /// 调用者.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string UserId { get; set; }

    /// <summary>
    /// 请求ip.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKIP")]
    public string InvokIp { get; set; }

    /// <summary>
    /// 请求设备.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKDEVICE")]
    public string InvokDevice { get; set; }

    /// <summary>
    /// 请求耗时.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKWASTETIME")]
    public int? InvokWasteTime { get; set; }

    /// <summary>
    /// 请求类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_INVOKTYPE")]
    public string InvokType { get; set; }

    /// <summary>
    /// 授权appid.
    /// </summary>
    [SugarColumn(ColumnName = "F_OAUTHAPPID")]
    public string OauthAppId { get; set; }
}