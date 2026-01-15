using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Extend.Entitys;
using Poxiao.Extend.Entitys.Dto.ProductGoods;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.Extend;

/// <summary>
/// 选择产品.
/// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "Goods", Order = 600)]
[Route("api/extend/saleOrder/[controller]")]
public class ProductGoodsService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ProductGoodsEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ProductGoodsService"/>类型的新实例.
    /// </summary>
    public ProductGoodsService(
        ISqlSugarRepository<ProductGoodsEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 产品列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ProductGoodsListQueryInput input)
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.classifyId), it => it.ClassifyId.Equals(input.classifyId))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.fullName), it => it.FullName.Contains(input.fullName))
            .Select(it => new ProductGoodsListOutput
            {
                id = it.Id,
                code = it.Code,
                fullName = it.FullName,
                qty = it.Qty,
            }).MergeTable().OrderByIF(string.IsNullOrEmpty(input.Sidx), it => it.id).OrderByIF(!string.IsNullOrEmpty(input.Sidx), input.Sidx + " " + input.Sort).ToPagedListAsync(input.CurrentPage, input.PageSize);
        return PageResult<ProductGoodsListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 商品编码.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelectorList([FromQuery] PageInputBase input)
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.DeleteMark == null)
            .WhereIF(input.Keyword.IsNotEmptyOrNull(), x => x.Code.Contains(input.Keyword))
            .Select(it => new ProductGoodsListOutput
            {
                id = it.Id,
                classifyId = it.ClassifyId,
                code = it.Code,
                fullName = it.FullName,
                qty = it.Qty,
                type = it.Type,
                amount = it.Amount,
                money = it.Money,
            }).ToListAsync();
        return new { list = data };
    }

    #endregion
}