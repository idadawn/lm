using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.Department;

/// <summary>
/// 部门修改输入.
/// </summary>
[SuppressSniffer]
public class DepartmentUpInput : DepartmentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}