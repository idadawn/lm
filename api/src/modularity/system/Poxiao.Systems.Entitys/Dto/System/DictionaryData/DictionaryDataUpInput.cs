using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.DictionaryData;

/// <summary>
/// 数字字典修改输入.
/// </summary>
[SuppressSniffer]
public class DictionaryDataUpInput : DictionaryDataCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}
