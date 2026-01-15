namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 控件联动参数模型.
/// </summary>
public class ControlLinkageParameterModel
{
    /// <summary>
    /// 参数名称.
    /// </summary>
    public string ParameterName { get; set; }

    /// <summary>
    /// 表单字段值.
    /// </summary>
    public string FormFieldValues { get; set; }

    /// <summary>
    /// 是否子副字段.
    /// </summary>
    public bool IsPrimaryAndSecondaryField { get; set; }
}