using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Poxiao.EventBus;
using Poxiao.Lab.EventBus;

namespace Poxiao.UnitTests.Lab;

public class NlqAgentSyncSubscriberTests
{
    private static HttpClient CreateMockClient(HttpMessageHandler handler, string name = "nlq-agent")
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(name)).Returns(() => new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:18100"),
        });
        return factoryMock.Object.CreateClient(name);
    }

    private static IHttpClientFactory CreateMockFactory(HttpMessageHandler handler, string name = "nlq-agent")
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(name)).Returns(() => new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:18100"),
        });
        return factoryMock.Object;
    }

    [Fact]
    public async Task HandleRuleChanged_PostsToSyncRulesWithExpectedPayload()
    {
        string? capturedPath = null;
        string? capturedBody = null;

        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            capturedPath = req.RequestUri?.PathAndQuery;
            capturedBody = req.Content != null
                ? req.Content.ReadAsStringAsync(ct).Result
                : null;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var factory = CreateMockFactory(handler);
        var subscriber = new NlqAgentSyncSubscriber(factory);

        var source = new RuleChangedEventSource("rule-123", RuleChangeKind.Updated, new { name = "test" });
        var context = CreateContext(source);

        await subscriber.HandleRuleChanged(context);

        Assert.Equal("/api/v1/sync/rules", capturedPath);
        Assert.NotNull(capturedBody);
        var doc = JsonDocument.Parse(capturedBody!);
        Assert.Equal("rule-123", doc.RootElement.GetProperty("rule_id").GetString());
        Assert.Equal("updated", doc.RootElement.GetProperty("change_kind").GetString());
        Assert.True(doc.RootElement.TryGetProperty("rule", out var ruleProp));
    }

    [Fact]
    public async Task HandleRuleChanged_Deleted_NullsRuleInPayload()
    {
        string? capturedBody = null;

        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            capturedBody = req.Content != null
                ? req.Content.ReadAsStringAsync(ct).Result
                : null;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var factory = CreateMockFactory(handler);
        var subscriber = new NlqAgentSyncSubscriber(factory);

        var source = new RuleChangedEventSource("rule-456", RuleChangeKind.Deleted, null);
        var context = CreateContext(source);

        await subscriber.HandleRuleChanged(context);

        Assert.NotNull(capturedBody);
        var doc = JsonDocument.Parse(capturedBody!);
        Assert.Equal("rule-456", doc.RootElement.GetProperty("rule_id").GetString());
        Assert.Equal("deleted", doc.RootElement.GetProperty("change_kind").GetString());
        Assert.True(doc.RootElement.GetProperty("rule").ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task HandleSpecChanged_PostsToSyncSpecsWithExpectedPayload()
    {
        string? capturedPath = null;
        string? capturedBody = null;

        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            capturedPath = req.RequestUri?.PathAndQuery;
            capturedBody = req.Content != null
                ? req.Content.ReadAsStringAsync(ct).Result
                : null;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var factory = CreateMockFactory(handler);
        var subscriber = new NlqAgentSyncSubscriber(factory);

        var source = new SpecChangedEventSource("spec-789", SpecChangeKind.Created, new { code = "S1" });
        var context = CreateContext(source);

        await subscriber.HandleSpecChanged(context);

        Assert.Equal("/api/v1/sync/specs", capturedPath);
        Assert.NotNull(capturedBody);
        var doc = JsonDocument.Parse(capturedBody!);
        Assert.Equal("spec-789", doc.RootElement.GetProperty("spec_id").GetString());
        Assert.Equal("created", doc.RootElement.GetProperty("change_kind").GetString());
    }

    [Fact]
    public async Task HandleRuleChanged_HttpError_DoesNotThrow()
    {
        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var factory = CreateMockFactory(handler);
        var subscriber = new NlqAgentSyncSubscriber(factory);

        var source = new RuleChangedEventSource("rule-000", RuleChangeKind.Updated, new { });
        var context = CreateContext(source);

        var ex = await Record.ExceptionAsync(() => subscriber.HandleRuleChanged(context));
        Assert.Null(ex);
    }

    private static EventHandlerExecutingContext CreateContext(IEventSource source)
    {
        // EventHandlerExecutingContext 的构造函数是 internal 的，无法直接 new。
        // 这里通过反射构造一个用于测试。
        var ctor = typeof(EventHandlerExecutingContext).GetConstructors(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        ).First();
        var properties = new Dictionary<object, object>();
        var attribute = new EventSubscribeAttribute(source.EventId);
        return (EventHandlerExecutingContext)ctor.Invoke(new object?[]
        {
            source,
            properties,
            null,
            attribute,
        });
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _sendAsync;

        public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_sendAsync(request, cancellationToken));
        }
    }
}
