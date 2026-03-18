namespace Poxiao.Systems.Interfaces;

/// <summary>
/// 版本服务接口
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// 获取当前版本信息
    /// </summary>
    Task<VersionOutput> GetVersionAsync();

    /// <summary>
    /// 获取更新日志
    /// </summary>
    Task<List<ChangelogItem>> GetChangelogAsync();
}

/// <summary>
/// 版本信息输出
/// </summary>
public class VersionOutput
{
    /// <summary>
    /// API 版本
    /// </summary>
    public string ApiVersion { get; set; } = "";

    /// <summary>
    /// Web 版本
    /// </summary>
    public string WebVersion { get; set; } = "";

    /// <summary>
    /// 是否兼容
    /// </summary>
    public bool IsCompatible { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// 更新日志项
/// </summary>
public class ChangelogItem
{
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// 日期
    /// </summary>
    public string Date { get; set; } = "";

    /// <summary>
    /// 更新内容
    /// </summary>
    public List<string> Added { get; set; } = new();

    /// <summary>
    /// 修复内容
    /// </summary>
    public List<string> Fixed { get; set; } = new();

    /// <summary>
    /// 改进内容
    /// </summary>
    public List<string> Improved { get; set; } = new();
}
