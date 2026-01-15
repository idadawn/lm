using Poxiao.Xunit;
using System;
using Xunit;
using Xunit.Abstractions;

// 配置启动类类型
[assembly: TestFramework("Poxiao.UnitTests.Startup", "Poxiao.UnitTests")]

namespace Poxiao.UnitTests;

/// <summary>
/// 单元测试初始化类
/// </summary>
public sealed class Startup : TestStartup
{
    public Startup(IMessageSink messageSink) : base(messageSink)
    {
        Serve.Run(silence: true);
    }
}