using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Dto.ProvinceAtlas;
using Poxiao.Systems.Entitys.Entity.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace Poxiao.Systems.System;

/// <summary>
/// 行政区划：地图.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Atlas", Order = 206)]
[Route("api/system/[controller]")]
public class ProvinceAtlasService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ProvinceAtlasEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ProvinceAtlasService"/>类型的新实例.
    /// </summary>
    public ProvinceAtlasService(
        ISqlSugarRepository<ProvinceAtlasEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    private static string atlasUrl = "https://geo.datav.aliyun.com/areas_v3/bound/geojson?code=";

    #region Get

    /// <summary>
    /// 获取所有列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList()
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.DeleteMark == null)
            .OrderBy(it => it.SortCode)
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .ToListAsync();
        var output = data.Adapt<List<ProvinceAtlasListOutput>>();
        return output.ToTree("-1");
    }

    /// <summary>
    /// 获取地图json.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("geojson")]
    public async Task<dynamic> Geojson([FromQuery] ProvinceAtlasGeojsonInput input)
    {
        string url = atlasUrl + input.code;

        var data = await _repository.AsQueryable()
            .Where(it => it.ParentId.Equals(input.code) && it.DeleteMark == null)
            .AnyAsync();
        if (data)
            url += "_full";

        try
        {
            var response = (await url.GetAsStringAsync()).ToObject<JObject>();
            if (response == null)
                throw Oops.Oh(ErrorCode.D1904);
            return response;
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.D1905);
        }
    }

    #endregion
}
