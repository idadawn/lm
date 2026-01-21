using System.ComponentModel;
using Newtonsoft.Json;

namespace Poxiao.Lab.Entity.Enums;

/// <summary>
/// 中间数据公式类型.
/// 对应字段 F_FORMULA_TYPE，取值：
/// CALC-计算公式，JUDGE-判定公式，NO-只展示.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<IntermediateDataFormulaType>))]
public enum IntermediateDataFormulaType
{
    /// <summary>
    /// 计算公式.
    /// </summary>
    [Description("计算公式")]
    CALC = 1,

    /// <summary>
    /// 判定公式.
    /// </summary>
    [Description("判定公式")]
    JUDGE = 2,

    /// <summary>
    /// 只展示.
    /// </summary>
    [Description("只展示")]
    NO = 3,
}
