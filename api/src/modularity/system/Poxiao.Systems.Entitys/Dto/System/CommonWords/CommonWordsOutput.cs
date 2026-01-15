namespace Poxiao.Systems.Entitys.Dto.System.CommonWords;

public class CommonWordsOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 应用id.
    /// </summary>
    public string systemId { get; set; }

    /// <summary>
    /// 应用名称.
    /// </summary>
    public string systemNames { get; set; }

    /// <summary>
    /// 常用语.
    /// </summary>
    public string commonWordsText { get; set; }

    /// <summary>
    /// 常用语类型(0:系统,1:个人).
    /// </summary>
    public int commonWordsType { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long sortCode { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    public int? enabledMark { get; set; }
}
