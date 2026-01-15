using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Poxiao.AI.Interfaces;
using Poxiao.DependencyInjection;

namespace Poxiao.AI.Service;

/// <summary>
/// AI服务实现
/// </summary>
public class AiService : IAiService, ITransient
{
    private readonly IChatClient _chatClient;
    private readonly string _modelId;

    public AiService(IConfiguration configuration)
    {
        var chatConfig = configuration.GetSection("AI:Chat");
        var endpoint = chatConfig["Endpoint"];
        _modelId = chatConfig["ModelId"] ?? "/data/qwen2.5-7b";
        var apiKey = chatConfig["Key"] ?? "dummy-key";

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("AI:Chat:Endpoint 配置不能为空");
        }

        // Initialize OpenAI Client
        var openAiClient = new OpenAI.OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpoint) }
        );

        // Use Microsoft.Extensions.AI abstraction
        _chatClient = openAiClient.AsChatClient(_modelId);
    }

    /// <inheritdoc />
    public async Task<string> ChatAsync(string message, string? systemPrompt = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "请输入您的问题。";
        }

        try
        {
            var messages = new List<ChatMessage>();

            // 添加系统提示词（如果提供）
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
            }

            // 添加用户消息
            messages.Add(new ChatMessage(ChatRole.User, message));

            var chatOptions = new ChatOptions { Temperature = 0.7f };

            var response = await _chatClient.CompleteAsync(messages, chatOptions);
            return response.Message.Text ?? "抱歉，我无法生成回复。";
        }
        catch (Exception ex)
        {
            // 记录错误并返回友好提示
            Console.WriteLine($"AI服务调用错误: {ex.Message}");
            return $"抱歉，处理您的请求时出现错误：{ex.Message}";
        }
    }
}
