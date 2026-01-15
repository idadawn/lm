namespace Poxiao.Infrastructure.Dtos;

/// <summary>
/// 数据导入输入.
/// </summary>
public class DataImportInput
{
    /// <summary>
    /// 数据集合.
    /// </summary>
    public List<Dictionary<string, object>> list { get; set; }
}