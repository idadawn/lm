using Newtonsoft.Json.Serialization;

namespace Poxiao.Infrastructure.Const;

/// <summary>
/// 公共常量.
/// </summary>
[SuppressSniffer]
public class CommonConst
{
    // 不带自定义转换器
    public static JsonSerializerSettings options => new JsonSerializerSettings
    {
        // 默认命名规则
        ContractResolver = new DefaultContractResolver(),

        // 设置时区为 UTC
        DateTimeZoneHandling = DateTimeZoneHandling.Local,

        // 格式化json输出的日期格式
        DateFormatString = "yyyy-MM-dd HH:mm:ss",
    };

    /// <summary>
    /// 全局租户缓存.
    /// </summary>
    public const string GLOBALTENANT = "poxiao:globaltenant";

    /// <summary>
    /// 默认密码.
    /// </summary>
    public const string DEFAULTPASSWORD = "123456";

    /// <summary>
    /// 用户缓存.
    /// </summary>
    public const string CACHEKEYUSER = "poxiao:permission:user";

    /// <summary>
    /// 菜单缓存.
    /// </summary>
    public const string CACHEKEYMENU = "menu_";

    /// <summary>
    /// 权限缓存.
    /// </summary>
    public const string CACHEKEYPERMISSION = "permission_";

    /// <summary>
    /// 数据范围缓存.
    /// </summary>
    public const string CACHEKEYDATASCOPE = "datascope_";

    /// <summary>
    /// 验证码缓存.
    /// </summary>
    public const string CACHEKEYCODE = "vercode_";

    /// <summary>
    /// 单据编码缓存.
    /// </summary>
    public const string CACHEKEYBILLRULE = "billrule_";

    /// <summary>
    /// 在线用户缓存.
    /// </summary>
    public const string CACHEKEYONLINEUSER = "poxiao:user:online";

    /// <summary>
    /// 岗位缓存.
    /// </summary>
    public const string CACHEKEYPOSITION = "position_";

    /// <summary>
    /// 角色缓存.
    /// </summary>
    public const string CACHEKEYROLE = "role_";

    /// <summary>
    /// 在线开发缓存.
    /// </summary>
    public const string VISUALDEV = "visualdev_";

    /// <summary>
    /// 代码生成远端数据缓存.
    /// </summary>
    public const string CodeGenDynamic = "codegendynamic_";

    /// <summary>
    /// 定时任务缓存.
    /// </summary>
    public const string CACHEKEYTIMERJOB = "timerjob_";

    /// <summary>
    /// 第三方登录 票据缓存key.
    /// </summary>
    public const string PARAMSPoxiaoTICKET = "poxiao_ticket";

    /// <summary>
    /// Cas Key.
    /// </summary>
    public const string CASTicket = "ticket";

    /// <summary>
    /// Code.
    /// </summary>
    public const string Code = "code";

    /// <summary>
    /// 外链密码开关(1：开 , 0：关).
    /// </summary>
    public const int OnlineDevDataStateEnable = 1;

    /// <summary>
    /// 门户日程缓存key.
    /// </summary>
    public const string CACHEKEYSCHEDULE = "poxiao:portal:schedule";
}