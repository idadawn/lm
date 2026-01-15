namespace Poxiao.Infrastructure.Models;

/// <summary>
/// 高级查询模型.
/// </summary>
public class SuperQueryModel
{
    /// <summary>
    /// 匹配逻辑.
    /// </summary>
    public string matchLogic { get; set; }

    /// <summary>
    /// 条件JSON列.
    /// </summary>
    public List<Conditionjson> conditionJson { get; set; }
}

/// <summary>
/// 条件JSON.
/// </summary>
public class Conditionjson
{
    /// <summary>
    /// 字段.
    /// </summary>
    public string field { get; set; }

    /// <summary>
    /// 字段值.
    /// </summary>
    public object fieldValue { get; set; }

    /// <summary>
    /// 象征.
    /// </summary>
    public string symbol { get; set; }

    /// <summary>
    /// poxiaoKey.
    /// </summary>
    public string poxiaoKey { get; set; }

    /// <summary>
    /// 多选.
    /// </summary>
    public bool multiple { get; set; }
}

/// <summary>
/// 转换高级查询.
/// </summary>
public class ConvertSuperQuery
{
    /// <summary>
    /// where类型.
    /// </summary>
    public WhereType whereType { get; set; }

    /// <summary>
    /// poxiaoKey.
    /// </summary>
    public string poxiaoKey { get; set; }

    /// <summary>
    /// 字段.
    /// </summary>
    public string field { get; set; }

    /// <summary>
    /// 字段值.
    /// </summary>
    public string fieldValue { get; set; }

    /// <summary>
    /// 条件类型.
    /// </summary>
    public ConditionalType conditionalType { get; set; }

    /// <summary>
    /// 是否主条件.
    /// </summary>
    public bool mainWhere { get; set; }

    /// <summary>
    /// 象征.
    /// </summary>
    public string symbol { get; set; }
}