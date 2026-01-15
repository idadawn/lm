using Poxiao.DependencyInjection;
using Poxiao.Systems.Entitys.Model.DataInterFace;

namespace Poxiao.Systems.Entitys.Dto.System.DataInterFace;

/// <summary>
/// 数据接口预览输入.
/// </summary>
[SuppressSniffer]
public class DataInterfacePreviewInput
{
    /// <summary>
    /// 租户id.
    /// </summary>
    public string? tenantId { get; set; }

    /// <summary>
    /// 预览参数.
    /// </summary>
    public List<DataInterfaceReqParameter>? paramList { get; set; } = new List<DataInterfaceReqParameter>();
}
