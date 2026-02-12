using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Poxiao.DataEncryption;
using Poxiao.Extras.WebSockets.Models;
using Poxiao.Infrastructure.Net;
using Poxiao.Infrastructure.Security;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Web;

namespace Poxiao.WebSockets;

/// <summary>
/// WebSocket 中间件.
/// </summary>
public class WebSocketMiddleware
{
    /// <summary>
    /// 请求委托.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// webSocket 处理程序.
    /// </summary>
    private WebSocketHandler _webSocketHandler { get; set; }

    /// <summary>
    /// 初始化一个<see cref="WebSocketMiddleware"/>类型的新实例.
    /// </summary>
    /// <param name="next"></param>
    /// <param name="webSocketHandler"></param>
    public WebSocketMiddleware(
        RequestDelegate next,
        WebSocketHandler webSocketHandler)
    {
        _next = next;
        _webSocketHandler = webSocketHandler;
    }

    /// <summary>
    /// 异步调用.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }

        WebSocket? socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

        // 支持 token 从查询字符串 (?token=xxx) 或路径 (/Bearer%20xxx) 获取，便于经反向代理时路径过长或截断
        var pathToken = context.Request.Path.ToString().TrimStart('/');
        var queryToken = context.Request.Query["token"].FirstOrDefault()
            ?? context.Request.Query["access_token"].FirstOrDefault();
        var rawToken = !string.IsNullOrWhiteSpace(queryToken)
            ? HttpUtility.UrlDecode(queryToken, Encoding.UTF8)
            : HttpUtility.UrlDecode(pathToken, Encoding.UTF8);
        if (!string.IsNullOrWhiteSpace(rawToken) && !rawToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            rawToken = "Bearer " + rawToken;

        var token = new JsonWebToken(rawToken?.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty);
        var httpContext = (DefaultHttpContext)context;
        httpContext.Request.Headers["Authorization"] = rawToken ?? string.Empty;
        UserAgent userAgent = new UserAgent(httpContext);
        if (!JWTEncryption.ValidateJwtBearerToken(httpContext, out token))
        {
            await _webSocketHandler.OnDisconnected(socket);
        }
        else
        {
            var connectionId = Guid.NewGuid().ToString("N");
            var wsClient = new WebSocketClient
            {
                ConnectionId = connectionId,
                WebSocket = socket,
                LoginIpAddress = NetHelper.Ip,
                LoginPlatForm = string.Format("{0}-{1}", userAgent.OS.ToString(), userAgent.RawValue)
            };

            await _webSocketHandler.OnConnected(connectionId, wsClient).ConfigureAwait(false);

            await Receive(wsClient, async (result, serializedMessage) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await _webSocketHandler.ReceiveAsync(wsClient, result, serializedMessage).ConfigureAwait(false);
                        break;
                    case WebSocketMessageType.Close:
                        await _webSocketHandler.OnDisconnected(socket);
                        break;
                    case WebSocketMessageType.Binary:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }
    }

    /// <summary>
    /// 接收数据.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="handleMessage"></param>
    /// <returns></returns>
    private async Task Receive(WebSocketClient client, Action<WebSocketReceiveResult, string> handleMessage)
    {
        while (client.WebSocket.State == WebSocketState.Open)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            string message = string.Empty;
            WebSocketReceiveResult result = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await client.WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        message = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }

                handleMessage(result, message);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    client.WebSocket.Abort();
                }
            }
        }

        await _webSocketHandler.OnDisconnected(client.WebSocket);
    }
}