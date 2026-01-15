using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Filter;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Poxiao.DynamicApiController;
using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Extension;
using Poxiao.Extend.Entitys.Entity;
using Poxiao.Extend.Entitys.Dto.Customer;

namespace Poxiao.Extend;

/// <summary>
/// 客户信息.
/// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "Customer", Order = 600)]
[Route("api/extend/saleOrder/[controller]")]
public class ProductCustomerService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ProductCustomerEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ProductCustomerService"/>类型的新实例.
    /// </summary>
    public ProductCustomerService(
        ISqlSugarRepository<ProductCustomerEntity> repository,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 客户列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var data = await _repository.AsQueryable()
            .Where(it => it.Deletemark == null)
            .WhereIF(input.Keyword.IsNotEmptyOrNull(), x => x.Name.Contains(input.Keyword))
            .Select(x => new ProductCustomerListOutput
            {
                id = x.Id,
                code = x.Code,
                name = x.Name,
                customerName = x.Customername,
                address = x.Address,
                contactTel = x.ContactTel
            })
            .ToListAsync();

        return new { list = data };
    }

    #endregion
}