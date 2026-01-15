using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.DataInterFace;

/// <summary>
/// 数据接口修改输入.
/// </summary>
[SuppressSniffer]
public class DataInterfaceUpInput : DataInterfaceCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}