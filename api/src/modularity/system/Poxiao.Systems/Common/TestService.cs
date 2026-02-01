using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extras.Thirdparty.JSEngine;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.TaskScheduler;
using SqlSugar;
using System.Data;

namespace Poxiao.Systems.Common;

/// <summary>
/// 测试接口.
/// </summary>
[ApiDescriptionSettings(Name = "Test", Order = 306)]
[Route("api")]
public class TestService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<UserEntity> _sqlSugarRepository;
    private readonly IDataBaseManager _databaseService;
    private readonly ITenant _db;

    public TestService(ISqlSugarRepository<UserEntity> sqlSugarRepository, ISqlSugarClient context, IDataBaseManager databaseService)
    {
        _sqlSugarRepository = sqlSugarRepository;
        _databaseService = databaseService;
        _db = context.AsTenant();
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public async Task<dynamic> test()
    {
        try
        {
            PutObjectArgs a = new PutObjectArgs().WithObjectSize(0);
            //var aaaaa= JsEngineUtil.AggreFunction("COUNT('1','1','1')").ToString();
            //var xx = App.HttpContext.Request.Host.ToString();
            //var sql = "SELECT  TOP 1 [F_PARENTID],[F_PROCESSID],[F_ENCODE],[F_FULLNAME],[F_FLOWURGENT],[F_FLOWID],[F_FLOWCODE],[F_FLOWNAME],[F_FLOWTYPE],[F_FLOWCATEGORY],[F_FLOWFORM],[F_FLOWFORMCONTENTJSON],[F_FLOWTEMPLATEJSON],[F_FLOWVERSION],[F_STARTTIME],[F_ENDTIME],[F_THISSTEP],[F_THISSTEPID],[F_GRADE],[F_STATUS],[F_COMPLETION],[F_DESCRIPTION],[F_SORTCODE],[F_ISASYNC],[F_ISBATCH],[F_TASKNODEID],[F_TEMPLATEID],[F_REJECTDATAID],[F_DELEGATEUSER],[F_CREATORTIME],[F_CREATORUSERID],[F_ENABLEDMARK],[F_LastModifyTime],[F_LastModifyUserId],[F_DeleteMark],[F_DeleteTime],[F_DeleteUserId],[F_Id] FROM [FLOW_TASK]  WHERE (( [F_DeleteMark] IS NULL ) AND ( [F_Id] = N'367536153122855173' ))";
            //var darta = _sqlSugarRepository.AsSugarClient().Ado.SqlQuery<dynamic>(sql);

            return "";
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void xx(UserEntity user)
    {
        user.Account = "2312321";

    }

    public void xx1(UserEntity user)
    {
        user.Account = "2312321";

    }

}
