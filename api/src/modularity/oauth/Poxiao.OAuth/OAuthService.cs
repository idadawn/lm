using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Captcha.General;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos.OAuth;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using Poxiao.DataEncryption;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.EventBus;
using Poxiao.EventHandler;
using Poxiao.FriendlyException;
using Poxiao.Logging.Attributes;
using Poxiao.OAuth.Dto;
using Poxiao.OAuth.Model;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Enum;
using Poxiao.Systems.Entitys.Model.SysConfig;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Poxiao.Systems.Interfaces.System;
using Poxiao.UnifyResult;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SqlSugar;
using Poxiao.Systems.Entitys.Dto.Module;
using Poxiao.Systems.Entitys.Model.Permission.SocialsUser;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.Extras.CollectiveOAuth.Models;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Options;
using Microsoft.CodeAnalysis;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Message.Interfaces.Message;
using Poxiao.Extras.DatabaseAccessor.SqlSugar.Models;
using Poxiao.Message.Interfaces;
using Aop.Api.Domain;

namespace Poxiao.OAuth;

/// <summary>
/// 业务实现：身份认证模块 .
/// </summary>
[ApiDescriptionSettings(Tag = "OAuth", Name = "OAuth", Order = 160)]
[Route("api/[controller]")]
public class OAuthService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 配置文档.
    /// </summary>
    private readonly OauthOptions _oauthOptions = App.GetConfig<OauthOptions>("OAuth", true);

    /// <summary>
    /// 用户仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _userRepository;

    /// <summary>
    /// 功能模块.
    /// </summary>
    private readonly IModuleService _moduleService;

    /// <summary>
    /// 功能按钮.
    /// </summary>
    private readonly IModuleButtonService _moduleButtonService;

    /// <summary>
    /// 功能列.
    /// </summary>
    private readonly IModuleColumnService _columnService;

    /// <summary>
    /// 功能数据权限计划.
    /// </summary>
    private readonly IModuleDataAuthorizeSchemeService _moduleDataAuthorizeSchemeService;

    /// <summary>
    /// 功能表单.
    /// </summary>
    private readonly IModuleFormService _formService;

    /// <summary>
    /// 系统配置.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 验证码处理程序.
    /// </summary>
    private readonly IGeneralCaptcha _captchaHandler;

    /// <summary>
    /// 第三方登录.
    /// </summary>
    private readonly ISocialsUserService _socialsUserService;

    /// <summary>
    /// 数据库配置选项.
    /// </summary>
    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// Http上下文.
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 事件总线.
    /// </summary>
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// SqlSugarClient客户端.
    /// </summary>
    private SqlSugarScope _sqlSugarClient;

    private readonly IMessageManager _messageManager;

    /// <summary>
    /// 初始化一个<see cref="OAuthService"/>类型的新实例.
    /// </summary>
    public OAuthService(
        IGeneralCaptcha captchaHandler,
        ISqlSugarRepository<UserEntity> userRepository,
        IModuleService moduleService,
        IModuleButtonService moduleButtonService,
        IModuleColumnService columnService,
        IModuleDataAuthorizeSchemeService moduleDataAuthorizeSchemeService,
        IModuleFormService formService,
        ISysConfigService sysConfigService,
        ISocialsUserService socialsUserService,
        IMessageManager messageManager,
        IOptions<ConnectionStringsOptions> connectionOptions,
        IOptions<TenantOptions> tenantOptions,
        ISqlSugarClient sqlSugarClient,
        IHttpContextAccessor httpContextAccessor,
        ICacheManager cacheManager,
        IUserManager userManager,
        IEventPublisher eventPublisher)
    {
        _captchaHandler = captchaHandler;
        _userRepository = userRepository;
        _moduleService = moduleService;
        _moduleButtonService = moduleButtonService;
        _columnService = columnService;
        _moduleDataAuthorizeSchemeService = moduleDataAuthorizeSchemeService;
        _formService = formService;
        _sysConfigService = sysConfigService;
        _socialsUserService = socialsUserService;
        _messageManager = messageManager;
        _connectionStrings = connectionOptions.Value;
        _tenant = tenantOptions.Value;
        _sqlSugarClient = (SqlSugarScope)sqlSugarClient;
        _httpContextAccessor = httpContextAccessor;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 获取默认数据库连接配置
    /// </summary>
    /// <returns></returns>
    private DbConnectionConfig GetDefaultConnection()
    {
        var defaultConnection = _connectionStrings.ConnectionConfigs.Find(it => it.ConfigId?.ToString().Equals("default") == true);
        if (defaultConnection == null)
            throw Oops.Oh("Default database connection not found");
        return defaultConnection;
    }

    #region Get

    /// <summary>
    /// 获取图形验证码.
    /// </summary>
    /// <param name="codeLength">验证码长度.</param>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    [HttpGet("ImageCode/{codeLength}/{timestamp}")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<IActionResult> GetCode(int codeLength, string timestamp)
    {
        return new FileContentResult(await _captchaHandler.CreateCaptchaImage(timestamp, 120, 40, codeLength > 0 ? codeLength : 4), "image/jpeg");
    }

    /// <summary>
    /// 首次登录 根据账号获取数据库配置.
    /// </summary>
    /// <param name="account">账号.</param>
    [HttpGet("getConfig/{account}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> GetConfigCode(string account)
    {
        ConnectionConfigOptions options = new ConnectionConfigOptions();
        var defaultConnection = GetDefaultConnection();
        string tenantId = defaultConnection.ConfigId?.ToString() ?? string.Empty;
        if (_tenant.MultiTenancy)
        {
            string tenantAccout = string.Empty;

            // 分割账号
            var tenantAccount = account.Split('@');
            tenantId = tenantAccount.FirstOrDefault() ?? string.Empty;

            if (tenantAccount.Length == 1) account = "admin";
            else account = tenantAccount[1];

            tenantAccout = account;

            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
            {
                throw Oops.Oh(result.msg);
            }
            else if (result.data.dotnet == null && result.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (result.data.linkList == null || result.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(tenantId, result.data.dotnet);
                }
                else if (result.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(tenantId, result.data.linkList);
                }
            }
            if (!"default".Equals(tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(tenantId);
            }
        }

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection()) throw Oops.Oh(ErrorCode.D1032);

        // 读取配置文件
        var array = new Dictionary<string, object>();
        var sysConfigData = await _sqlSugarClient.Queryable<SysConfigEntity>()
            .Where(x => x.Category.Equals("SysConfig") && (SqlFunc.ToLower(x.Key).Equals("singlelogin") || SqlFunc.ToLower(x.Key).Equals("enableverificationcode") || SqlFunc.ToLower(x.Key).Equals("verificationcodenumber"))).ToListAsync();
        foreach (var item in sysConfigData)
        {
            if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        }

        var sysConfig = array.ToObject<SysConfigModel>();

        /*
        * 登录完成后添加全局租户缓存
        * 判断当前租户是否存在缓存
        * 不存在添加缓存
        * 存在更新缓存
        */
        if (!await IsAnyByTenantIdAsync(tenantId))
        {
            List<GlobalTenantCacheModel>? list = await GetGlobalTenantCache();
            list.Add(new GlobalTenantCacheModel
            {
                TenantId = tenantId,
                SingleLogin = (int)sysConfig.singleLogin,
                connectionConfig = options
            });
            await SetGlobalTenantCache(list);
        }
        else
        {
            List<GlobalTenantCacheModel>? list = await GetGlobalTenantCache();
            list.FindAll(it => it.TenantId.Equals(tenantId)).ForEach(item =>
            {
                item.TenantId = tenantId;
                item.SingleLogin = (int)sysConfig.singleLogin;
                item.connectionConfig = options;
            });
            await SetGlobalTenantCache(list);
        }

        // 返回给前端 是否开启验证码 和 验证码长度
        return new { enableVerificationCode = sysConfig.enableVerificationCode, verificationCodeNumber = sysConfig.verificationCodeNumber > 0 ? sysConfig.verificationCodeNumber : 4 };
    }

    /// <summary>
    /// 获取当前登录用户信息.
    /// </summary>
    /// <param name="type">Web和App</param>
    /// <returns></returns>
    [HttpGet("CurrentUser")]
    public async Task<dynamic> GetCurrentUser(string type)
    {
        if (type.IsNullOrEmpty()) type = "Web"; // 默认为Web端菜单目录

        var userId = _userManager.UserId;

        var loginOutput = new CurrentUserOutput();
        loginOutput.userInfo = await _userManager.GetUserInfo();

        // 菜单
        loginOutput.menuList = await _moduleService.GetUserTreeModuleList(type);

        // 子系统信息
        loginOutput.userInfo.systemIds = await _userRepository.AsSugarClient().Queryable<SystemEntity>().Where(x => x.DeleteMark == null && x.EnabledMark.Equals(1))
            .Select(x => new UserSystemModel()
            {
                id = x.Id,
                enCode = x.EnCode,
                name = x.FullName,
                icon = x.Icon,
                sortCode = x.SortCode,
                currentSystem = SqlFunc.Equals(loginOutput.userInfo.systemId, x.Id)
            }).OrderBy(x => x.sortCode).ToListAsync();

        if (!_userManager.IsAdministrator)
        {
            var sId = await _userRepository.AsSugarClient().Queryable<AuthorizeEntity>()
                .Where(x => x.ItemType.Equals("system") && loginOutput.userInfo.roleIds.Contains(x.ObjectId) && x.ObjectType.Equals("Role"))
                .Select(x => x.ItemId).ToListAsync();
            loginOutput.userInfo.systemIds = loginOutput.userInfo.systemIds.Where(x => sId.Contains(x.id)).ToList();

            var currSysId = _userManager.UserOrigin.Equals("pc") ? loginOutput.userInfo.systemId : loginOutput.userInfo.appSystemId;
            if ((loginOutput.userInfo.systemIds.Any() && !loginOutput.userInfo.systemIds.Any(x => x.id.Equals(currSysId))) || currSysId.IsNullOrEmpty() || !loginOutput.menuList.Any())
            {
                if (loginOutput.userInfo.systemIds.Any())
                {
                    var defaultItem = loginOutput.userInfo.systemIds.Find(x => x.enCode.Equals("mainSystem"));
                    if (defaultItem == null) defaultItem = loginOutput.userInfo.systemIds.FirstOrDefault();
                    if (_userManager.UserOrigin.Equals("pc"))
                    {
                        loginOutput.userInfo.systemId = defaultItem.id;
                        defaultItem.currentSystem = true;
                        await _userRepository.AsUpdateable().SetColumns(x => x.SystemId == loginOutput.userInfo.systemId).Where(x => x.Id.Equals(userId)).ExecuteCommandAsync();
                    }
                    else
                    {
                        loginOutput.userInfo.appSystemId = defaultItem.id;
                        defaultItem.currentSystem = true;
                        await _userRepository.AsUpdateable().SetColumns(x => x.AppSystemId == loginOutput.userInfo.appSystemId).Where(x => x.Id.Equals(userId)).ExecuteCommandAsync();
                    }
                    loginOutput.menuList = await _moduleService.GetUserTreeModuleList(type);
                    if (!loginOutput.menuList.Any())
                    {
                        for (var i = 1; i < loginOutput.userInfo.systemIds.Count; i++)
                        {
                            defaultItem = loginOutput.userInfo.systemIds[i];
                            if (_userManager.UserOrigin.Equals("pc"))
                            {
                                loginOutput.userInfo.systemId = defaultItem.id;
                                defaultItem.currentSystem = true;
                                await _userRepository.AsUpdateable().SetColumns(x => x.SystemId == loginOutput.userInfo.systemId).Where(x => x.Id.Equals(userId)).ExecuteCommandAsync();
                            }
                            else
                            {
                                loginOutput.userInfo.appSystemId = defaultItem.id;
                                defaultItem.currentSystem = true;
                                await _userRepository.AsUpdateable().SetColumns(x => x.AppSystemId == loginOutput.userInfo.appSystemId).Where(x => x.Id.Equals(userId)).ExecuteCommandAsync();
                            }

                            loginOutput.menuList = await _moduleService.GetUserTreeModuleList(type);
                            if (loginOutput.menuList.Any()) break;
                        }
                    }
                }
            }

            if (!loginOutput.userInfo.systemIds.Any()) loginOutput.menuList.Clear();
        }
        else
        {
            var currSysId = _userManager.UserOrigin != null && _userManager.UserOrigin.Equals("pc") ? loginOutput.userInfo.systemId : loginOutput.userInfo.appSystemId;

            if (currSysId.IsNullOrEmpty() || !loginOutput.menuList.Any())
            {
                var systemId = (await _userRepository.AsSugarClient().Queryable<SystemEntity>().Where(x => x.EnCode.Equals("mainSystem")).FirstAsync()).Id;
                switch (_userManager.UserOrigin)
                {
                    case "pc":
                        await _userRepository.AsSugarClient().Updateable<UserEntity>().SetColumns(it => new UserEntity() { SystemId = systemId }).Where(it => it.Id == _userManager.User.Id).ExecuteCommandAsync();
                        break;
                    case "app":
                        await _userRepository.AsSugarClient().Updateable<UserEntity>().SetColumns(it => new UserEntity() { AppSystemId = systemId }).Where(it => it.Id == _userManager.User.Id).ExecuteCommandAsync();
                        break;
                }

                loginOutput.menuList = await _moduleService.GetUserTreeModuleList(type);
            }
        }

        var currentUserModel = new CurrentUserModelOutput();
        currentUserModel.moduleList = await _moduleService.GetUserModueList(type);
        currentUserModel.buttonList = await _moduleButtonService.GetUserModuleButtonList();
        currentUserModel.columnList = await _columnService.GetUserModuleColumnList();
        currentUserModel.resourceList = await _moduleDataAuthorizeSchemeService.GetResourceList();
        currentUserModel.formList = await _formService.GetUserModuleFormList();

        // 权限信息
        var permissionList = new List<PermissionModel>();
        currentUserModel.moduleList.ForEach(menu =>
        {
            var permissionModel = new PermissionModel();
            permissionModel.modelId = menu.id;
            permissionModel.moduleName = menu.fullName;
            permissionModel.button = currentUserModel.buttonList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalButtonAuthorizeModel>>();
            permissionModel.column = currentUserModel.columnList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalColumnAuthorizeModel>>();
            permissionModel.form = currentUserModel.formList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalFormAuthorizeModel>>();
            permissionModel.resource = currentUserModel.resourceList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalResourceAuthorizeModel>>();
            permissionList.Add(permissionModel);
        });

        loginOutput.permissionList = permissionList;

        // 系统+菜单树
        loginOutput.routerList = new List<UserAllMenu>();
        foreach (var item in loginOutput.userInfo.systemIds)
        {
            var sysMenuList = GetUserAllMenu(await _moduleService.GetUserTreeModuleListBySystemId(type, item.id));
            sysMenuList.Where(x => x.systemId != null && x.systemId.Equals(item.id) && x.parentId.Equals("-1")).ToList().ForEach(it => it.parentId = item.id);
            var child = sysMenuList.Where(x => x.systemId != null && x.systemId.Equals(item.id)).ToList();
            loginOutput.routerList.Add(new UserAllMenu()
            {
                id = item.id,
                fullName = item.name,
                icon = item.icon,
                enCode = item.enCode,
                parentId = "-1",
                children = child,
                hasChildren = child.Any()
            });
        }

        loginOutput.routerList = loginOutput.routerList.Where(x => x.parentId.Equals("-1")).ToList();

        // 系统配置信息
        var sysInfo = await _sysConfigService.GetInfo();
        loginOutput.sysConfigInfo = sysInfo.Adapt<SysConfigInfo>();

        return loginOutput;
    }

    /// <summary>
    /// 退出.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Logout")]
    public async Task Logout([FromQuery] string ticket)
    {
        var tenantId = _userManager.TenantId ?? "default";
        var userId = _userManager.UserId ?? "admim";
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext.SignoutToSwagger();

        // 清除IM中的webSocket
        var list = await GetOnlineUserList(tenantId);
        if (list != null)
        {
            var onlineUser = list.Find(it => it.tenantId == tenantId && it.userId == userId);
            if (onlineUser != null)
            {
                list.RemoveAll((x) => x.connectionId == onlineUser.connectionId);
                await SetOnlineUserList(list, tenantId);
            }
        }

        await DelUserInfo(tenantId, userId);
    }

    #endregion

    #region POST

    /// <summary>
    /// 用户登录.
    /// </summary>
    /// <param name="input">登录输入参数.</param>
    /// <returns></returns>
    [HttpPost("Login")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> Login([FromForm] LoginInput input)
    {
        var defaultConnection = GetDefaultConnection();
        ConnectionConfigOptions options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
        UserAgent userAgent = new UserAgent(App.HttpContext);
        string tenantAccout = string.Empty;
        string tenantId = defaultConnection.ConfigId?.ToString() ?? string.Empty;
        if (_tenant.MultiTenancy)
        {
            // 分割账号
            var tenantAccount = input.account.Split('@');
            tenantId = tenantAccount.FirstOrDefault() ?? string.Empty;
            if (tenantAccount.Length == 1)
                input.account = "admin";
            else
                input.account = tenantAccount[1];
            tenantAccout = input.account;

            if (input.socialsOptions == null)
            {
                var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
                var response = await interFace.GetAsStringAsync();
                var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
                if (result.code != 200)
                {
                    throw Oops.Oh(result.msg);
                }
                else if (result.data.dotnet == null && result.data.linkList == null)
                {
                    throw Oops.Oh(ErrorCode.D1025);
                }
                else
                {
                    switch (_tenant.MultiTenancyType)
                    {
                        case "COLUMN":
                            if (result.data.linkList == null || result.data.linkList?.Count == 0)
                            {
                                options = PoxiaoTenantExtensions.GetLinkToOrdinary(tenantId, defaultConnection.DBName, result.data.dotnet);
                            }
                            else if (result.data.dotnet == null)
                            {
                                options = PoxiaoTenantExtensions.GetLinkToCustom(tenantId, result.data.linkList);
                            }
                            break;
                        default:
                            if (result.data.linkList == null || result.data.linkList?.Count == 0)
                            {
                                options = PoxiaoTenantExtensions.GetLinkToOrdinary(tenantId, result.data.dotnet);
                            }
                            else if (result.data.dotnet == null)
                            {
                                options = PoxiaoTenantExtensions.GetLinkToCustom(tenantId, result.data.linkList);
                            }
                            break;
                    }
                }
                if (!"default".Equals(tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
                {
                    _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
                }
                else
                {
                    _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                    _sqlSugarClient.ChangeDatabase(tenantId);
                }
            }
            else
            {
                options = input.socialsOptions;
            }
        }

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection()) throw Oops.Oh(ErrorCode.D1032);

        // 读取配置文件
        var array = new Dictionary<string, string>();
        var sysConfigData = await _sqlSugarClient.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig")).ToListAsync();
        foreach (var item in sysConfigData)
        {
            if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        }

        var sysConfig = array.ToObject<SysConfigByOAuthModel>();

        // 开启验证码验证
        if (sysConfig.enableVerificationCode)
        {
            if (string.IsNullOrWhiteSpace(input.timestamp) || string.IsNullOrWhiteSpace(input.code))
                throw Oops.Oh(ErrorCode.D1029);
            string imageCode = await GetCode(input.timestamp);
            if (imageCode.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.D1030);
            if (!input.code.ToLower().Equals(imageCode.ToLower()))
                throw Oops.Oh(ErrorCode.D1029);
        }

        // 根据用户账号获取用户秘钥
        var user = await _sqlSugarClient.Queryable<UserEntity>().FirstAsync(it => it.Account.Equals(input.account) && it.DeleteMark == null);
        _ = user ?? throw Oops.Oh(ErrorCode.D5002);

        // 验证账号是否未被激活
        if (user.EnabledMark == null) throw Oops.Oh(ErrorCode.D1018);

        // 验证账号是否被禁用
        if (user.EnabledMark == 0) throw Oops.Oh(ErrorCode.D1019);

        // 验证账号是否被删除
        if (user.DeleteMark == 1) throw Oops.Oh(ErrorCode.D1017);

        // 是否延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNullOrEmpty())
        {
            if (user.UnLockTime > DateTime.Now)
            {
                int unlockTime = ((user.UnLockTime - DateTime.Now)?.TotalMinutes).ParseToInt();
                if (unlockTime < 1) unlockTime = 1;
                throw Oops.Oh(ErrorCode.D1027, unlockTime);
            }
            else if (user.UnLockTime <= DateTime.Now)
            {
                user.EnabledMark = 1;
                user.LogErrorCount = 0;
                await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
            }
        }

        // 是否 延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime > DateTime.Now)
        {
            int? t3 = (user.UnLockTime - DateTime.Now)?.TotalMinutes.ParseToInt();
            if (t3 < 1) t3 = 1;
            throw Oops.Oh(ErrorCode.D1027, t3?.ToString() ?? "1");
        }

        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime <= DateTime.Now)
        {
            user.EnabledMark = 1;
            user.LogErrorCount = 0;
            await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
        }

        // 是否锁定
        if (user.EnabledMark == 2) throw Oops.Oh(ErrorCode.D1031);

        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(input.password + user.Secretkey);
        if (input.isSocialsLoginCallBack) encryptPasswod = input.password;

        // 账户密码是否匹配
        var userAnyPwd = await _sqlSugarClient.Queryable<UserEntity>().FirstAsync(u => u.Account == input.account && u.Password == encryptPasswod && u.DeleteMark == null);
        if (userAnyPwd.IsNullOrEmpty())
        {
            // 如果是密码错误 记录账号的密码错误次数
            await UpdateErrorLog(user, sysConfig);
        }
        else
        {
            // 清空记录错误次数
            userAnyPwd.LogErrorCount = 0;

            // 解除锁定
            userAnyPwd.EnabledMark = 1;
            await _sqlSugarClient.Updateable(userAnyPwd).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
        }

        _ = userAnyPwd ?? throw Oops.Oh(ErrorCode.D1000);

        // app权限验证
        if (userAgent.IsMobileDevice && user.IsAdministrator == 0 && !ExistRoleByApp(user.RoleId))
            throw Oops.Oh(ErrorCode.D1022);

        // 登录成功时 判断单点登录信息
        int whitelistSwitch = Convert.ToInt32(sysConfig.whitelistSwitch);
        string whiteListIp = sysConfig.whiteListIp;
        if (whitelistSwitch.Equals(1) && user.IsAdministrator.Equals(0))
        {
            if (!whiteListIp.Split(",").Contains(NetHelper.Ip))
                throw Oops.Oh(ErrorCode.D9002);
        }

        // token过期时间
        long tokenTimeout = sysConfig.tokenTimeout;

        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
                new Dictionary<string, object>
                {
                    { ClaimConst.CLAINMUSERID, userAnyPwd.Id },
                    { ClaimConst.CLAINMACCOUNT, userAnyPwd.Account },
                    { ClaimConst.CLAINMREALNAME, userAnyPwd.RealName },
                    { ClaimConst.CLAINMADMINISTRATOR, userAnyPwd.IsAdministrator },
                    { ClaimConst.TENANTID, tenantId},
                    { ClaimConst.OnlineTicket, input.online_ticket }
                }, tokenTimeout);

        // 单点登录标识缓存
        if (_oauthOptions.Enabled) _cacheManager.Set("OnlineTicket_" + input.online_ticket, options.ConfigId);

        // 设置Swagger自动登录
        _httpContextAccessor.HttpContext.SigninToSwagger(accessToken);

        // 设置刷新Token令牌
        _httpContextAccessor.HttpContext.Response.Headers["x-access-token"] = JWTEncryption.GenerateRefreshToken(accessToken, 30); // 生成刷新Token令牌

        var ip = NetHelper.Ip;

        /*
        * 登录完成后添加全局租户缓存
        * 判断当前租户是否存在缓存
        * 不存在添加缓存
        * 存在更新缓存
        */
        if (!await IsAnyByTenantIdAsync(tenantId))
        {
            List<GlobalTenantCacheModel>? list = await GetGlobalTenantCache();
            list.Add(new GlobalTenantCacheModel
            {
                TenantId = tenantId,
                SingleLogin = (int)sysConfig.singleLogin,
                connectionConfig = options
            });
            await SetGlobalTenantCache(list);
        }
        else
        {
            List<GlobalTenantCacheModel>? list = await GetGlobalTenantCache();
            list.FindAll(it => it.TenantId.Equals(tenantId)).ForEach(item =>
            {
                item.TenantId = tenantId;
                item.SingleLogin = (int)sysConfig.singleLogin;
                item.connectionConfig = options;
            });
            await SetGlobalTenantCache(list);
        }

        // 修改用户登录信息
        await _eventPublisher.PublishAsync(new UserEventSource("User:UpdateUserLogin", tenantId, new UserEntity
        {
            Id = user.Id,
            FirstLogIP = user.FirstLogIP ?? ip,
            FirstLogTime = user.FirstLogTime ?? DateTime.Now,
            PrevLogTime = user.LastLogTime,
            PrevLogIP = user.LastLogIP,
            LastLogTime = DateTime.Now,
            LastLogIP = ip,
            LogSuccessCount = user.LogSuccessCount + 1
        }));

        // 增加登录日志
        await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateVisLog", tenantId, new SysLogEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            UserId = user.Id,
            UserName = user.RealName,
            Category = 1,
            IPAddress = ip,
            Abstracts = "登录成功",
            PlatForm = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue),
            CreatorTime = DateTime.Now
        }));

        var ticket = await _cacheManager.GetAsync<SocialsLoginTicketModel>(input.poxiao_ticket);
        if (ticket.IsNotEmptyOrNull())
        {
            var socialsEntity = ticket.value.ToObject<SocialsUsersEntity>();
            var sInfo = await _userRepository.AsSugarClient().Queryable<SocialsUsersEntity>().Where(x => (x.SocialId.Equals(socialsEntity.SocialId) || x.UserId.Equals(user.Id)) && x.SocialType.Equals(socialsEntity.SocialType) && x.DeleteMark == null).FirstAsync();
            if (sInfo == null)
            {
                var socialsUserEntity = new SocialsUsersEntity();
                socialsUserEntity.UserId = user.Id;
                socialsUserEntity.SocialType = socialsEntity.SocialType.ToLower();
                socialsUserEntity.SocialName = socialsEntity.SocialName;
                socialsUserEntity.SocialId = socialsEntity.SocialId;
                await _userRepository.AsSugarClient().Insertable(socialsUserEntity).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                // 租户开启时-添加租户库绑定数据
                if (_tenant.MultiTenancy)
                {
                    var info = await _userRepository.AsSugarClient().Queryable<UserEntity>().FirstAsync(x => x.Id.Equals(user.Id));
                    var param = socialsUserEntity.ToObject<Dictionary<string, string>>();
                    if (param.ContainsKey("SocialType")) param["SocialType"] = param["SocialType"].ToLower();
                    param.Add("tenantId", tenantId);
                    param.Add("account", info.Account);
                    param.Add("accountName", info.RealName + "/" + info.Account);

                    var postUrl = _tenant.MultiTenancyDBInterFace + "socials";
                    var result = (await postUrl.SetBody(param).PostAsStringAsync()).ToObject<Dictionary<string, string>>();

                    if (result == null || "500".Equals(result["code"]) || "400".Equals(result["code"]))
                    {
                        return new { code = 201, message = "用户租户绑定错误!" }.ToJsonString();
                    }
                }
            }
        }

        return new {
            theme = user.Theme == null ? "classic" : user.Theme,
            token = string.Format("Bearer {0}", accessToken)
        };
    }

    /// <summary>
    /// 锁屏解锁登录.
    /// </summary>
    /// <param name="input">登录输入参数.</param>
    /// <returns></returns>
    [HttpPost("LockScreen")]
    public async Task LockScreen([FromBody] LockScreenInput input)
    {
        // 根据用户账号获取用户秘钥
        var secretkey = (await _userRepository.GetFirstAsync(u => u.Account == input.account && u.DeleteMark == null)).Secretkey;

        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(input.password + secretkey);

        var user = await _userRepository.GetFirstAsync(u => u.Account == input.account && u.Password == encryptPasswod && u.DeleteMark == null);
        _ = user ?? throw Oops.Oh(ErrorCode.D1000);
    }

    /// <summary>
    /// 注销用户.
    /// </summary>
    /// <returns></returns>
    [HttpPost("logoutCurrentUser")]
    [NonUnify]
    public async Task<dynamic> LogoutCurrentUser()
    {
        var userInfo = _userManager.User;
        if (userInfo.IsAdministrator.Equals(1)) throw Oops.Oh(ErrorCode.D1034);
        userInfo.DeleteMark = 1;
        userInfo.DeleteTime = DateTime.Now;
        userInfo.DeleteUserId = userInfo.Id;
        await _userRepository.AsUpdateable(userInfo).ExecuteCommandAsync();
        return new { code = 200, msg = "注销成功" };
    }

    /// <summary>
    /// 单点登录退出.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Logout/auth2")]
    [AllowAnonymous]
    public async Task OnlineLogout()
    {
        var ticket = _httpContextAccessor.HttpContext.Request.Form["ticket"];
        var tenantId = await _cacheManager.GetAsync("OnlineTicket_" + ticket);
        if (ticket.IsNotEmptyOrNull())
        {
            await _cacheManager.DelAsync("OnlineTicket_" + ticket);
            var userId = _userManager.GetAdminUserId();
            var userOnlineList = new List<UserOnlineModel>();
            userOnlineList = await GetOnlineUserList(tenantId);
            var userOnline = userOnlineList.Find(x => x.onlineTicket.Equals(ticket));
            if (userOnline != null)
            {
                userId = userOnline.userId;
                await _messageManager.ForcedOffline(userOnline.connectionId);
            }

            // 清除IM中的webSocket
            if (userOnlineList != null)
            {
                var onlineUser = userOnlineList.Find(it => it.tenantId == tenantId && it.userId == userId);
                if (onlineUser != null)
                {
                    userOnlineList.RemoveAll((x) => x.connectionId == onlineUser.connectionId);
                    await SetOnlineUserList(userOnlineList, tenantId);
                }
            }

            await DelUserInfo(tenantId, userId);
        }
    }

    /// <summary>
    /// 密码过期提醒.
    /// </summary>
    /// <returns></returns>
    [HttpPost("updatePasswordMessage")]
    public async Task PwdMessage()
    {
        var sysConfigInfo = await _sysConfigService.GetInfo();
        // 密码修改时间.
        var changePasswordDate = _userManager.User.ChangePasswordDate.IsNullOrEmpty() ? _userManager.User.CreatorTime : _userManager.User.ChangePasswordDate;
        // 提醒时间
        var remindDate = changePasswordDate.ParseToDateTime().AddDays(sysConfigInfo.updateCycle - sysConfigInfo.updateInAdvance);
        if (sysConfigInfo.passwordIsUpdatedRegularly == 1 && remindDate < DateTime.Now)
        {
            var paramsDic = new Dictionary<string, string>();
            paramsDic.Add("@Title", "");
            paramsDic.Add("@CreatorUserName", _userManager.GetUserName(_userManager.UserId));
            var msgEntity = _messageManager.GetMessageEntity("XTXXTX001", paramsDic, 3);
            var bodyDic = new Dictionary<string, object>();
            bodyDic.Add(_userManager.UserId, msgEntity.BodyText);
            var msgReceiveList = _messageManager.GetMessageReceiveList(new List<string> { _userManager.UserId }, msgEntity, bodyDic);
            await _messageManager.SendDefaultMsg(new List<string> { _userManager.UserId }, msgEntity, msgReceiveList);
        }
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private async Task<string> GetCode(string timestamp)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        return await _cacheManager.GetAsync<string>(cacheKey);
    }

    /// <summary>
    /// 判断app用户角色是否存在且有效.
    /// </summary>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    private bool ExistRoleByApp(string roleIds)
    {
        if (roleIds.IsEmpty())
            return false;
        var roleIdList1 = roleIds.Split(",").ToList();
        var roleIdList2 = _sqlSugarClient.Queryable<RoleEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).Select(x => x.Id).ToList();
        return roleIdList1.Intersect(roleIdList2).ToList().Count > 0;
    }

    /// <summary>
    /// 记录密码错误次数.
    /// </summary>
    /// <param name="entity">用户实体.</param>
    /// <param name="sysConfigOutput">系统配置输出.</param>
    /// <returns></returns>
    private async Task UpdateErrorLog(UserEntity entity, SysConfigByOAuthModel sysConfigOutput)
    {
        if (entity != null)
        {
            if (entity.EnabledMark.Equals(1) && !entity.Account.ToLower().Equals("admin") && sysConfigOutput.lockType > 0 && sysConfigOutput.passwordErrorsNumber > 2)
            {

                switch (sysConfigOutput.lockType)
                {
                    case ErrorStrategy.Lock:
                        entity.EnabledMark = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? 2 : 1;
                        break;
                    case ErrorStrategy.Delay:
                        entity.UnLockTime = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? DateTime.Now.AddMinutes(sysConfigOutput.lockTime) : null;
                        entity.EnabledMark = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? 2 : 1;
                        break;
                }

                entity.LogErrorCount++;

                await _sqlSugarClient.Updateable(entity).UpdateColumns(it => new { it.EnabledMark, it.UnLockTime, it.LogErrorCount }).ExecuteCommandAsync();
            }
        }
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <returns></returns>
    private async Task<List<UserOnlineModel>> GetOnlineUserList(string tenantId)
    {
        string cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 保存在线用户列表.
    /// </summary>
    /// <param name="onlineList">在线用户列表.</param>
    /// <returns></returns>
    private async Task<bool> SetOnlineUserList(List<UserOnlineModel> onlineList, string tenantId)
    {
        string cacheKey = string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        return await _cacheManager.SetAsync(cacheKey, onlineList);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    private async Task<bool> DelUserInfo(string tenantId, string userId)
    {
        string cacheKey = string.Format("{0}:{1}:{2}", tenantId, CommonConst.CACHEKEYUSER, userId);
        return await _cacheManager.DelAsync(cacheKey);
    }

    /// <summary>
    /// 是否存在租户缓存.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    private async Task<bool> IsAnyByTenantIdAsync(string tenantId)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        var list = await _cacheManager.GetAsync<List<GlobalTenantCacheModel>>(cacheKey);
        return list != null ? list.Any(it => it.TenantId.Equals(tenantId)) : false;
    }

    /// <summary>
    /// 获取全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private async Task<List<GlobalTenantCacheModel>> GetGlobalTenantCache()
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        var list = await _cacheManager.GetAsync<List<GlobalTenantCacheModel>>(cacheKey);
        return list != null ? list : new List<GlobalTenantCacheModel>();
    }

    /// <summary>
    /// 保存全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> SetGlobalTenantCache(List<GlobalTenantCacheModel> list)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        return await _cacheManager.SetAsync(cacheKey, list);
    }

    /// <summary>
    /// 组装用户所有菜单.
    /// </summary>
    /// <param name="menuList"></param>
    /// <returns></returns>
    private List<UserAllMenu> GetUserAllMenu(List<ModuleNodeOutput> menuList)
    {
        var result = new List<UserAllMenu>();
        menuList.ForEach(item =>
        {
            var menu = item.Adapt<UserAllMenu>();
            if (menu.children != null && menu.children.Any())
            {
                menu.hasChildren = true;
                menu.children = GetUserAllMenu(menu.children.Adapt<List<ModuleNodeOutput>>());
            }

            result.Add(menu);
        });

        return result;
    }
    #endregion

    #region 第三方登录回调

    /// <summary>
    /// 第三方登录回调.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Login/socials")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<dynamic> SocialsLoginCallBack([FromQuery] SocialsUserInputModel req)
    {
        ConnectionConfigOptions options = null;
        var defaultConnection = GetDefaultConnection();

        if (req.tenantLogin && req.tenantId.IsNotEmptyOrNull() && req.userId.IsNotEmptyOrNull())
        {
            options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, req.tenantId);
            var response = await interFace.GetAsStringAsync();
            var resultObj = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (resultObj.code != 200)
            {
                throw Oops.Oh(resultObj.msg);
            }
            else if (resultObj.data.dotnet == null && resultObj.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (resultObj.data.linkList == null || resultObj.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(req.tenantId, resultObj.data.dotnet);
                }
                else if (resultObj.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(req.tenantId, resultObj.data.linkList);
                }
            }

            if (!"default".Equals(req.tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == resultObj.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(req.tenantId);
            }
            var userEntity = await _userRepository.AsQueryable().FirstAsync(x => x.Id.Equals(req.userId));
            if (_tenant.MultiTenancy) userEntity.Account = req.tenantId + "@" + userEntity.Account;
            var result = await Login(new LoginInput() { account = userEntity.Account, password = userEntity.Password, isSocialsLoginCallBack = true, socialsOptions = options });
            return new { code = 200, data = result };
        }

        if (_tenant.MultiTenancy && req.tenantId.IsNullOrEmpty())
        {
            var result = new AuthResponse(5001, string.Empty);
            var resStr = string.Empty;

            // 微信小程序唤醒登录.
            if (req.uuid.IsNotEmptyOrNull())
            {
                AuthUser user = new AuthUser();
                user.uuid = req.uuid;
                user.source = req.source;
                user.username = req.socialName;
                result = new AuthResponse(2000, null, user);
            }
            else
            {
                if (req.code.IsNullOrWhiteSpace()) req.code = req.authCode != null ? req.authCode : req.auth_code;

                // 获取第三方请求
                AuthCallbackNew callback = _socialsUserService.SetAuthCallback(req.code, req.state);

                // 获取第三方请求
                var authRequest = _socialsUserService.GetAuthRequest(req.source, null, false, null, null);
                result = authRequest.login(callback);
            }

            if (result.ok())
            {
                var resData = result.data.ToObject<AuthUser>();
                var uuid = _socialsUserService.GetSocialUuid(result);

                options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
                var interFace = string.Format("{0}socials/list?socialsId={1}", _tenant.MultiTenancyDBInterFace, uuid);
                var response = await interFace.GetAsStringAsync();
                var resultObj = response.ToObject<Dictionary<string, object>>();
                if (resultObj["code"].ToString() != "200")
                {
                    throw Oops.Oh(resultObj["msg"].ToString());
                }

                var ticket = _cacheManager.Get<SocialsLoginTicketModel>(req.poxiao_ticket);
                if (ticket == null && req.code.IsNullOrWhiteSpace()) Oops.Oh(ErrorCode.D1035);
                if (ticket.IsNotEmptyOrNull())
                {
                    // 修改 缓存 状态
                    ticket.status = (int)SocialsLoginTicketStatus.Multitenancy;
                    if (resultObj["data"] != null && resultObj["data"].ToJsonString().Equals("[]"))
                    {
                        if (result.ok())
                        {
                            var socialsUserEntity = new SocialsUsersEntity();
                            socialsUserEntity.SocialType = resData.source;
                            socialsUserEntity.SocialName = resData.username;
                            socialsUserEntity.SocialId = uuid;
                            ticket.status = (int)SocialsLoginTicketStatus.UnBind;
                            ticket.value = socialsUserEntity.ToJsonString();
                            _cacheManager.Set(req.poxiao_ticket, ticket, TimeSpan.FromMinutes(5));
                            resStr = new { code = 400, msg = "等待登录自动绑定!", message = "等待登录自动绑定!" }.ToJsonString();
                        }
                        else
                        {
                            resStr = new { code = 400, msg = "第三方回调失败!", message = "第三方回调失败!" }.ToJsonString();
                        }
                    }
                    else
                    {
                        var tList = resultObj["data"].ToObject<List<Dictionary<string, object>>>();
                        if (tList.Count == 1)
                        {
                            var tInfo = tList.FirstOrDefault();
                            interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tInfo["tenantId"]);
                            response = await interFace.GetAsStringAsync();
                            var resObj = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
                            if (resObj.code != 200)
                            {
                                throw Oops.Oh(resObj.msg);
                            }
                            else if (resObj.data.dotnet == null && resObj.data.linkList == null)
                            {
                                throw Oops.Oh(ErrorCode.D1025);
                            }
                            else
                            {
                                if (resObj.data.linkList == null || resObj.data.linkList?.Count == 0)
                                {
                                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(tInfo["tenantId"].ToString(), resObj.data.dotnet);
                                }
                                else if (resObj.data.dotnet == null)
                                {
                                    options = PoxiaoTenantExtensions.GetLinkToCustom(tInfo["tenantId"].ToString(), resObj.data.linkList);
                                }
                            }

                            if (!"default".Equals(req.tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
                            {
                                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == resObj.data.dotnet);
                            }
                            else
                            {
                                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                                _sqlSugarClient.ChangeDatabase(tInfo["tenantId"]);
                            }
                            var userEntity = await _userRepository.AsQueryable().FirstAsync(x => x.Id.Equals(tInfo["userId"].ToString()));
                            if (_tenant.MultiTenancy) userEntity.Account = tInfo["tenantId"].ToString() + "@" + userEntity.Account;
                            var loginRes = await Login(new LoginInput() { account = userEntity.Account, password = userEntity.Password, isSocialsLoginCallBack = true, socialsOptions = options });

                            // 修改 缓存 状态
                            ticket.status = (int)SocialsLoginTicketStatus.Success;
                            ticket.value = loginRes.token;
                            _cacheManager.Set(req.poxiao_ticket, ticket.ToJsonString(), TimeSpan.FromMinutes(5));
                            return new { code = 200, data = ticket };
                        }
                        ticket.value = resultObj["data"].ToJsonString();
                        _cacheManager.Set(req.poxiao_ticket, ticket.ToJsonString(), TimeSpan.FromMinutes(5));
                        resStr = new { code = 200, data = ticket.value }.ToJsonString();
                    }
                }
            }
            else
            {
                resStr = new { code = 400, msg = "第三方回调失败!", message = "第三方回调失败!" }.ToJsonString();
            }

            if (req.poxiao_ticket.IsNullOrEmpty())
            {
                return new ContentResult()
                {
                    Content = string.Format("<script>window.opener.postMessage('{0}', '*');window.open('','_self','');window.close();</script>", resStr),
                    StatusCode = 200,
                    ContentType = "text/html;charset=utf-8"
                };
            }

            return resStr.ToObject<Dictionary<string, object>>();
        }
        if (_tenant.MultiTenancy && req.tenantId.IsNotEmptyOrNull())
        {
            options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, req.tenantId);
            var response = await interFace.GetAsStringAsync();
            var resultObj = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (resultObj.code != 200)
            {
                throw Oops.Oh(resultObj.msg);
            }
            else if (resultObj.data.dotnet == null && resultObj.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (resultObj.data.linkList == null || resultObj.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(req.tenantId, resultObj.data.dotnet);
                }
                else if (resultObj.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(req.tenantId, resultObj.data.linkList);
                }
            }

            if (!"default".Equals(req.tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == resultObj.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(req.tenantId);
            }
        }

        if (req.code.IsNullOrWhiteSpace()) req.code = req.authCode != null ? req.authCode : req.auth_code;

        var res = await _socialsUserService.Binding(req);

        if (req.poxiao_ticket.IsNotEmptyOrNull())
        {
            var ticket = _cacheManager.Get<SocialsLoginTicketModel>(req.poxiao_ticket);
            if (ticket == null && req.code.IsNullOrWhiteSpace()) Oops.Oh(ErrorCode.D1035);

            var data = res.ToObject<Dictionary<string, object>>();
            if (data.ContainsKey("data"))
            {
                var socialsEntity = data["data"].ToObject<SocialsUsersEntity>();

                // 接受CODE 进行登录
                var sEntity = await _userRepository.AsSugarClient().Queryable<SocialsUsersEntity>().FirstAsync(x => x.SocialType.Equals(socialsEntity.SocialType) && x.SocialId.Equals(socialsEntity.SocialId) && x.DeleteMark == null);
                if (sEntity != null)
                {
                    var userEntity = await _userRepository.AsQueryable().FirstAsync(x => x.Id.Equals(sEntity.UserId));
                    if (_tenant.MultiTenancy) userEntity.Account = req.tenantId + "@" + userEntity.Account;
                    var loginRes = await Login(new LoginInput() { account = userEntity.Account, password = userEntity.Password, isSocialsLoginCallBack = true, socialsOptions = options });

                    // 修改 缓存 状态
                    ticket.status = (int)SocialsLoginTicketStatus.Success;
                    ticket.value = loginRes.token;
                    _cacheManager.Set(req.poxiao_ticket, ticket.ToJsonString(), TimeSpan.FromMinutes(5));
                    return new { code = 200, data = ticket };
                }
                else
                {
                    var ticketValue = _cacheManager.Get(req.poxiao_ticket);
                    if (ticketValue.IsNotEmptyOrNull())
                    {
                        ticket.status = (int)SocialsLoginTicketStatus.UnBind;
                        ticket.value = socialsEntity.ToJsonString();
                        _cacheManager.Set(req.poxiao_ticket, ticket, TimeSpan.FromMinutes(5));
                        res = new { code = 400, msg = "等待登录自动绑定!", message = "等待登录自动绑定!" }.ToJsonString();
                    }
                    else
                    {
                        res = new { code = 400, msg = "第三方回调失败!", message = "第三方回调失败!" }.ToJsonString();
                    }
                }
            }
            else
            {
                res = new { code = 400, msg = "第三方回调失败!", message = "第三方回调失败!" }.ToJsonString();
            }
        }

        if (req.poxiao_ticket.IsNullOrEmpty())
        {
            var result = res.ToObject<Dictionary<string, object>>();
            if (result.ContainsKey("data")) result.Remove("data");
            return new ContentResult()
            {
                Content = string.Format("<script>window.opener.postMessage('{0}', '*');window.open('','_self','');window.close();</script>", result.ToJsonString()),
                StatusCode = 200,
                ContentType = "text/html;charset=utf-8"
            };
        }

        return res.ToObject<Dictionary<string, object>>();
    }

    /// <summary>
    /// 多租户第三方登录回调.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Login/socials")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> SocialsLogin([FromForm] SocialsUserCallBackModel req)
    {
        if (req.tenantLogin)
        {
            var defaultConnection = GetDefaultConnection();
            var options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, req.tenantId);
            var response = await interFace.GetAsStringAsync();
            var resultObj = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (resultObj.code != 200)
            {
                throw Oops.Oh(resultObj.msg);
            }
            else if (resultObj.data.dotnet == null && resultObj.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (resultObj.data.linkList == null || resultObj.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(req.tenantId, resultObj.data.dotnet);
                }
                else if (resultObj.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(req.tenantId, resultObj.data.linkList);
                }
            }

            if (!"default".Equals(req.tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == resultObj.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(req.tenantId);
            }
            var userEntity = await _userRepository.AsQueryable().FirstAsync(x => x.Id.Equals(req.userId));
            if (_tenant.MultiTenancy) userEntity.Account = req.tenantId + "@" + userEntity.Account;
            return await Login(new LoginInput() { account = userEntity.Account, password = userEntity.Password, isSocialsLoginCallBack = true, socialsOptions = options });
        }
        return null;
    }

    /// <summary>
    /// 获取登录配置, 是否需要跳转、第三方登录信息.
    /// </summary>
    [HttpGet("GetLoginConfig")]
    [AllowAnonymous]
    [IgnoreLog]
    public dynamic GetSocialsLoginConfig()
    {
        var loginConfigModel = new SocialsLoginConfigModel();

        if (_oauthOptions.Enabled)
        {
            var url = _oauthOptions.LoginPath + "/" + _oauthOptions.DefaultSSO;
            loginConfigModel.redirect = true;
            loginConfigModel.url = url;
            loginConfigModel.ticketParams = CommonConst.PARAMS_Poxiao_TICKET;
        }
        else
        {
            // 追加第三方登录配置
            var loginList = _socialsUserService.GetLoginList(CommonConst.PARAMS_Poxiao_TICKET.ToUpper());
            if (loginList == null) return loginConfigModel;
            if (loginList.Any())
            {
                loginConfigModel.socialsList = loginList.ToObject<List<object>>();
                loginConfigModel.redirect = false;
                loginConfigModel.ticketParams = CommonConst.PARAMS_Poxiao_TICKET;
            }
        }

        return loginConfigModel;
    }

    /// <summary>
    /// 获取登录票据.
    /// </summary>
    /// <returns>return {msg:有效期, data:票据}.</returns>
    [HttpGet("getTicket")]
    [AllowAnonymous]
    [IgnoreLog]
    public dynamic GetTicket()
    {
        SocialsLoginTicketModel ticketModel = new SocialsLoginTicketModel();
        var curDate = DateTime.Now.AddMinutes(_oauthOptions.TicketTimeout); // 默认过期5分钟.
        ticketModel.ticketTimeout = curDate.ParseToUnixTime();
        var key = "SocialsLogin_" + SnowflakeIdHelper.NextId();
        _cacheManager.Set(key, ticketModel.ToJsonString(), TimeSpan.FromMinutes(_oauthOptions.TicketTimeout));
        return key;
    }

    /// <summary>
    /// 检测票据登录状态.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getTicketStatus/{ticket}")]
    [AllowAnonymous]
    [IgnoreLog]
    public dynamic GetTicketStatus(string ticket)
    {
        var ticketModel = _cacheManager.Get<SocialsLoginTicketModel>(ticket);
        if (ticketModel == null)
        {
            ticketModel = new SocialsLoginTicketModel() { status = (int)SocialsLoginTicketStatus.Invalid };
        }

        return ticketModel;
    }

    #endregion

    #region 单点登录.

    /// <summary>
    /// 单点登录接口.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Login/{type}")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<dynamic> LoginByType(string type, [FromQuery] Dictionary<string, string> input)
    {
        #region Cas
        //if (type.ToLower().Equals("cas"))
        //{
        //    var ticket = input.ContainsKey(CommonConst.PARAMS_Poxiao_TICKET) ? input[CommonConst.PARAMS_Poxiao_TICKET].ToString() : string.Empty;
        //    var ticketModel = _cacheManager.Get<SocialsLoginTicketModel>(ticket);
        //    if (ticketModel == null) return "登录票据已失效";

        //    var casTicket = input.ContainsKey(CommonConst.CAS_Ticket) ? input[CommonConst.CAS_Ticket].ToString() : string.Empty;
        //    if (casTicket.IsNotEmptyOrNull())
        //    {

        //    }
        //    else
        //    {
        //        var loginUrl = _oauthOptions.SSO.Cas.ServerLoginUrl;
        //        //http://sso.maxkey.top:8527/sign/authz/cas/?service=http://sa-oauth-client.demo.maxkey.top:8002

        //        loginUrl = Extras.CollectiveOAuth.Utils.UrlBuilder.fromBaseUrl(loginUrl)
        //            .queryParam("service", _oauthOptions.LoginPath + "/cas")
        //            .queryParam(CommonConst.PARAMS_Poxiao_TICKET, ticket)
        //            .build();
        //        _httpContextAccessor.HttpContext.Response.Redirect(loginUrl);
        //    }
        //}
        #endregion

        if (type.ToLower().Equals("auth2"))
        {
            var ticket = string.Empty;
            if (input.ContainsKey(CommonConst.PARAMS_Poxiao_TICKET) && input[CommonConst.PARAMS_Poxiao_TICKET].IsNotEmptyOrNull())
            {
                ticket = input[CommonConst.PARAMS_Poxiao_TICKET];
                var ticketModel = _cacheManager.Get<SocialsLoginTicketModel>(ticket);
                if (ticketModel == null) return "登录票据已失效";
            }

            var code = input.ContainsKey(CommonConst.Code) ? input[CommonConst.Code] : string.Empty;

            // 接受CODE 进行登录
            if (code.IsNotEmptyOrNull())
            {
                try
                {
                    await loginByCode(code, ticket);
                }
                catch (Exception e)
                {
                    // 更新登录结果
                    return e.Message;
                }
            }
            else
            {
                redirectLogin(ticket);
            }
        }

        return null;
    }

    /// <summary>
    /// 跳转单点登录页面.
    /// </summary>
    protected void redirectLogin(string ticket)
    {
        var loginUrl = _oauthOptions.SSO.Auth2.AuthorizeUrl;
        var tmpAuthCallbackUrl = _oauthOptions.LoginPath + "/auth2";
        //http://sso.maxkey.top:8527/sign/authz/oauth/v20/authorize?response_type=code&client_id=745057899234983936&redirect_uri=http://sa-oauth-client.demo.maxkey.top:8002/&scope=all

        if (ticket.IsNotEmptyOrNull())
        {
            tmpAuthCallbackUrl = Extras.CollectiveOAuth.Utils.UrlBuilder.fromBaseUrl(tmpAuthCallbackUrl)
                    .queryParam(CommonConst.PARAMS_Poxiao_TICKET, ticket)
                    .build();
        }

        loginUrl = Extras.CollectiveOAuth.Utils.UrlBuilder.fromBaseUrl(loginUrl)
            .queryParam("response_type", CommonConst.Code)
            .queryParam("client_id", _oauthOptions.SSO.Auth2.ClientId)
            .queryParam("scope", "read")
            .queryParam("redirect_uri", tmpAuthCallbackUrl)
            .build();

        _httpContextAccessor.HttpContext.Response.Redirect(loginUrl);
    }

    /// <summary>
    /// Oauth2登录.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="ticket"></param>
    protected async Task loginByCode(string code, string ticket)
    {
        var token = await getAccessToken(code);
        var remoteUserInfo = await getRemoteInfo(token);
        //var userId = remoteUserInfo.getOrDefault("accounts.username", remoteUserInfo["username"]).ToString();
        var userId = remoteUserInfo.ContainsKey("accounts.username") ? remoteUserInfo["accounts.username"].ToString() : remoteUserInfo["username"].ToString();
        var userAccount = string.Empty;
        if (_tenant.MultiTenancy)
        {
            var instId = remoteUserInfo["institution"].ToString();
            userAccount = instId + "@" + userId;
        }
        else
        {
            userAccount = userId;
        }

        // 登录账号
        var loginInput = await GetUserInfoByUserAccount(userAccount);
        loginInput.online_ticket = remoteUserInfo["online_ticket"].ToString();
        var loginRes = await Login(loginInput);

        var poxiaoTicket = _cacheManager.Get<SocialsLoginTicketModel>(ticket);
        if (poxiaoTicket.IsNotEmptyOrNull())
        {
            // 修改 缓存 状态
            poxiaoTicket.status = (int)SocialsLoginTicketStatus.Success;
            poxiaoTicket.value = loginRes.token;
            _cacheManager.Set(ticket, poxiaoTicket.ToJsonString(), TimeSpan.FromMinutes(_oauthOptions.TicketTimeout));
        }
        else
        {
            var url = string.Format("{0}?token={1}&theme={2}", _oauthOptions.SucessFrontUrl, loginRes.token, loginRes.theme);
            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }
    }

    /// <summary>
    /// 获取OAUTH2 AccessToken.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private async Task<string> getAccessToken(string code)
    {
        var reqUrl = _oauthOptions.SSO.Auth2.AccessTokenUrl
                .AddUrlQuery(string.Format("grant_type={0}", "authorization_code"))
                .AddUrlQuery(string.Format("client_id={0}", _oauthOptions.SSO.Auth2.ClientId))
                .AddUrlQuery(string.Format("client_secret={0}", _oauthOptions.SSO.Auth2.ClientSecret))
                .AddUrlQuery(string.Format("redirect_uri={0}", _oauthOptions.LoginPath + "/auth2"))
                .AddUrlQuery(string.Format("code={0}", code));

        var response = await reqUrl.GetAsStringAsync();
        Dictionary<string, object> result = null;
        try
        {
            result = response.ToObject<Dictionary<string, object>>();
        }
        catch (Exception e)
        {
            // log.error("解析Auth2 access_token失败", e);
        }

        if (result == null || !result.ContainsKey("access_token"))
        {
            throw new Exception("Auth2: 获取access_token失败");
        }

        var access_token = result["access_token"].ToString();

        // log.debug("Auth2 Token: {}", access_token);
        return access_token;
    }

    /// <summary>
    /// 获取用户信息.
    /// </summary>
    /// <param name="access_token"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, object>> getRemoteInfo(string access_token)
    {
        var reqUrl = _oauthOptions.SSO.Auth2.UserInfoUrl
                .AddUrlQuery(string.Format("access_token={0}", access_token));
        var response = await reqUrl.GetAsStringAsync();

        Dictionary<string, object> result = null;
        try
        {
            // log.debug("Auth2 User: {}", response);
            result = response.ToObject<Dictionary<string, object>>();
        }
        catch (Exception e)
        {
            // log.error("解析Auth2 用户信息失败", e);
        }

        if (result == null || !result.ContainsKey("username"))
        {
            // log.error(response);
            throw new Exception("Auth2: 获取远程用户信息失败");
        }

        return result;
    }

    private async Task<LoginInput> GetUserInfoByUserAccount(string account)
    {
        var defaultConnection = GetDefaultConnection();
        ConnectionConfigOptions options = PoxiaoTenantExtensions.GetLinkToOrdinary(defaultConnection.ConfigId?.ToString() ?? string.Empty, defaultConnection.DBName);
        UserAgent userAgent = new UserAgent(App.HttpContext);
        if (_tenant.MultiTenancy)
        {
            // 分割账号
            var tenantAccount = account.Split('@');
            var tenantId = tenantAccount.FirstOrDefault();
            if (tenantAccount.Length == 1)
                account = "admin";
            else
                account = tenantAccount[1];

            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
            {
                throw Oops.Oh(result.msg);
            }
            else if (result.data.dotnet == null && result.data.linkList == null)
            {
                throw Oops.Oh(ErrorCode.D1025);
            }
            else
            {
                if (result.data.linkList == null || result.data.linkList?.Count == 0)
                {
                    options = PoxiaoTenantExtensions.GetLinkToOrdinary(tenantId, result.data.dotnet);
                }
                else if (result.data.dotnet == null)
                {
                    options = PoxiaoTenantExtensions.GetLinkToCustom(tenantId, result.data.linkList);
                }
            }

            if (!"default".Equals(tenantId) && _tenant.MultiTenancyType.Equals("COLUMN"))
            {
                _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == result.data.dotnet);
            }
            else
            {
                _sqlSugarClient.AddConnection(PoxiaoTenantExtensions.GetConfig(options));
                _sqlSugarClient.ChangeDatabase(tenantId);
            }

            var userEntity = _sqlSugarClient.Queryable<UserEntity>().Single(u => u.Account == account && u.DeleteMark == null);
            return new LoginInput()
            {
                account = string.Join("@", tenantAccount),
                password = userEntity.Password,
                isSocialsLoginCallBack = true
            };
        }
        else
        {
            var userEntity = _sqlSugarClient.Queryable<UserEntity>().Single(u => u.Account == account && u.DeleteMark == null);
            return new LoginInput()
            {
                account = userEntity.Account,
                password = userEntity.Password,
                isSocialsLoginCallBack = true
            };
        }
    }

    #endregion
}
