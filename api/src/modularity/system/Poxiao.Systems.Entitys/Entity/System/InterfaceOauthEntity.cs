using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 接口认证
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_INTERFACEOAUTH")]
public class InterfaceOauthEntity : CLDEntityBase
{
    /// <summary>
    /// 应用id.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPID")]
    public string AppId { get; set; }

    /// <summary>
    /// 应用名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPNAME")]
    public string AppName { get; set; }

    /// <summary>
    /// 应用秘钥.
    /// </summary>
    [SugarColumn(ColumnName = "F_APPSECRET")]
    public string AppSecret { get; set; }

    /// <summary>
    /// 验证签名.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERIFYSIGNATURE")]
    public int? VerifySignature { get; set; }

    /// <summary>
    /// 使用期限.
    /// </summary>
    [SugarColumn(ColumnName = "F_USEFULLIFE")]
    public DateTime? UsefulLife { get; set; }

    /// <summary>
    /// 白名单.
    /// </summary>
    [SugarColumn(ColumnName = "F_WHITELIST")]
    public string WhiteList { get; set; }

    /// <summary>
    /// 黑名单.
    /// </summary>
    [SugarColumn(ColumnName = "F_BLACKLIST")]
    public string BlackList { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 接口列表.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATAINTERFACEIDS")]
    public string DataInterfaceIds { get; set; }
}