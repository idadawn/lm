using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Security;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extend.Entitys;
using Poxiao.Extend.Entitys.Dto.BigData;
using Poxiao.FriendlyException;
using Poxiao.LinqBuilder;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Extend;

/// <summary>
/// 大数据测试
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "BigData", Order = 600)]
[Route("api/extend/[controller]")]
public class BigDataService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<BigDataEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="BigDataService"/>类型的新实例.
    /// </summary>
    public BigDataService(ISqlSugarRepository<BigDataEntity> repository)
    {
        _repository = repository;
    }

    #region GET

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var queryWhere = LinqExpression.And<BigDataEntity>();
        if (!string.IsNullOrEmpty(input.Keyword))
            queryWhere = queryWhere.And(m => m.FullName.Contains(input.Keyword) || m.EnCode.Contains(input.Keyword));
        var list = await _repository.AsQueryable().Where(queryWhere).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToPagedListAsync(input.CurrentPage, input.PageSize);
        var pageList = new SqlSugarPagedList<BigDataListOutput>()
        {
            list = list.list.Adapt<List<BigDataListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<BigDataListOutput>.SqlSugarPageResult(pageList);
    }
    #endregion

    #region POST

    /// <summary>
    /// 新建
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create()
    {
        var list = await _repository.GetListAsync();
        var code = 0;
        if (list.Count > 0)
        {
            code = list.Select(x => x.EnCode).ToList().Max().ParseToInt();
        }
        var index = code == 0 ? 10000001 : code;
        if (index > 11500001)
            throw Oops.Oh(ErrorCode.Ex0001);
        List<BigDataEntity> entityList = new List<BigDataEntity>();
        for (int i = 0; i < 10000; i++)
        {
            entityList.Add(new BigDataEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                EnCode = index.ToString(),
                FullName = "测试大数据" + index,
                CreatorTime = DateTime.Now,
            });
            index++;
        }
        Blukcopy(entityList);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 大数据批量插入.
    /// </summary>
    /// <param name="entityList"></param>
    private void Blukcopy(List<BigDataEntity> entityList)
    {
        try
        {
            var storageable = _repository.AsSugarClient().Storageable(entityList).SplitInsert(x => true).ToStorage();
            switch (_repository.AsSugarClient().CurrentConnectionConfig.DbType)
            {
                case DbType.Dm:
                case DbType.Kdbndp:
                    storageable.AsInsertable.ExecuteCommand();
                    break;
                case DbType.Oracle:
                    _repository.AsSugarClient().Storageable(entityList).ToStorage().BulkCopy();
                    break;
                default:
                    _repository.AsSugarClient().Fastest<BigDataEntity>().BulkCopy(entityList);
                    break;
            }
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    #endregion
}