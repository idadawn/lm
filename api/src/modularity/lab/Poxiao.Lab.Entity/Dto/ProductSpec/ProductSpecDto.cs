using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.Lab.Entity.Dto.ProductSpec;

public class ProductSpecCrInput : ProductSpecEntity
{
    /// <summary>
    /// 扩展属性列表.
    /// </summary>
    public List<ProductSpecAttributeEntity> Attributes { get; set; } = new();
}

public class ProductSpecUpInput : ProductSpecEntity
{
    /// <summary>
    /// 扩展属性列表.
    /// </summary>
    public List<ProductSpecAttributeEntity> Attributes { get; set; } = new();

    /// <summary>
    /// 是否创建新版本（用户手动选择）.
    /// </summary>
    public bool? CreateNewVersion { get; set; }

    /// <summary>
    /// 版本说明（创建新版本时的说明）.
    /// </summary>
    public string VersionDescription { get; set; }
}

public class ProductSpecListQuery : PageInputBase
{
    public string Keyword { get; set; }
}

public class ProductSpecListOutput : ProductSpecEntity
{
    /// <summary>
    /// 扩展属性列表.
    /// </summary>
    public List<ProductSpecAttributeEntity> Attributes { get; set; } = new();
}

public class ProductSpecInfoOutput : ProductSpecEntity
{
    /// <summary>
    /// 扩展属性列表.
    /// </summary>
    public List<ProductSpecAttributeEntity> Attributes { get; set; } = new();
}
