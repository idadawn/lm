using Poxiao.Infrastructure.Security;
using System.Text.Json.Serialization;

namespace Poxiao.VisualData.Entitys.Dto.Screen;

/// <summary>
/// 大屏下拉框输出.
/// </summary>
public class ScreenSelectorOuput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 是否删除.
    /// </summary>
    [JsonIgnore]
    public int isDeleted { get; set; }
}
