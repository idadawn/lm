using Poxiao.DependencyInjection;
using Poxiao.Extend.Entitys.Dto.WorkLog;

namespace Poxiao.Extend.Entitys.Dto.WoekLog;

/// <summary>
///
/// </summary>
[SuppressSniffer]
public class WorkLogUpInput : WorkLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
