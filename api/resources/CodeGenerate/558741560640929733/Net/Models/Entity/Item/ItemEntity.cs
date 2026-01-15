using Poxiao.Infrastructure.CodeGenUpload;
using SqlSugar;

namespace Poxiao.system.Entitys;

/// <summary>
/// 物料信息实体.
/// </summary>
[SugarTable("item")]
public class ItemEntity
{
    /// <summary>
    /// 物料编码.
    /// </summary>
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public string? Id { get; set; }

    /// <summary>
    /// 物料编码.
    /// </summary>
    [SugarColumn(ColumnName = "item_code")]
    public string? ItemCode { get; set; }

    /// <summary>
    /// 物料描述.
    /// </summary>
    [SugarColumn(ColumnName = "item_desc")]
    [CodeGenUpload("item_desc", "{\"tableName\":\"item\",\"regList\":[],\"poxiaoKey\":\"textarea\",\"rule\":null,\"dictionaryType\":null,\"required\":true,\"unique\":false,\"label\":\"物料描述\",\"dataType\":null,\"propsUrl\":null,\"children\":null,\"props\":null}")]
    public string? ItemDesc { get; set; }

    /// <summary>
    /// 物料类型.
    /// </summary>
    [SugarColumn(ColumnName = "item_category")]
    [CodeGenUpload("item_category", false, "{\"props\":null}", "[{\"fullName\":\"物料\",\"enCode\":\"material\",\"sortCode\":0,\"id\":\"558704434650021829\",\"parentId\":\"0\",\"hasChildren\":false,\"children\":[],\"num\":0,\"isLeaf\":false},{\"fullName\":\"产品\",\"enCode\":\"product\",\"sortCode\":0,\"id\":\"558704012132614085\",\"parentId\":\"0\",\"hasChildren\":false,\"children\":[],\"num\":0,\"isLeaf\":false}]", "{\"tableName\":\"item\",\"regList\":[],\"poxiaoKey\":\"select\",\"rule\":null,\"dictionaryType\":\"558703923028819909\",\"required\":true,\"unique\":false,\"label\":\"物料类型\",\"dataType\":\"dictionary\",\"propsUrl\":\"\",\"children\":null,\"props\":null}")]
    public string? ItemCategory { get; set; }

}