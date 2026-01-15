namespace Poxiao.AI.Interfaces;

/// <summary>
/// AI服务接口
/// </summary>
public interface IAiService
{
    /// <summary>
    /// 发送聊天消息
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="systemPrompt">系统提示词</param>
    /// <returns>AI回复</returns>
    Task<string> ChatAsync(string message, string? systemPrompt = null);
}
