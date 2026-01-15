using Microsoft.AspNetCore.Mvc;
using Poxiao.AI.Interfaces;
using Poxiao.DynamicApiController;

namespace Poxiao.AI.Web.Core;

/// <summary>
/// AI助手接口
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "AI" }, Tag = "ai", Name = "ai", Order = 200)]
[Route("api/ai")]
public class AiController : IDynamicApiController
{
    private readonly IAiService _aiService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns>AI回复</returns>
    [HttpPost("chat")]
    public async Task<ChatResponse> ChatAsync([FromBody] ChatRequest input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Message))
        {
            return new ChatResponse { Response = "请输入您的问题。" };
        }

        var response = await _aiService.ChatAsync(input.Message, input.SystemPrompt);
        return new ChatResponse { Response = response };
    }
}

/// <summary>
/// 聊天请求模型
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// 用户消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; set; }
}

/// <summary>
/// 聊天响应模型
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// AI回复
    /// </summary>
    public string Response { get; set; } = string.Empty;
}
