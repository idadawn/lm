using Poxiao.DependencyInjection;

namespace Poxiao.Extend.Entitys.Dto.ProductClassify;

/// <summary>
/// 产品分类.
/// </summary>
[SuppressSniffer]
public class ProductClassifyUpInput : ProductClassifyCrInput
{
    public string id { get; set; }
}