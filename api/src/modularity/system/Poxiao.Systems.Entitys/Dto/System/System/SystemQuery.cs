using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.System.System;

public class SystemQuery : KeywordInput
{
    /// <summary>
    /// 开启 1 0 禁用.
    /// </summary>
    public string enableMark { get; set; }
}
