using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Serilog 日志拓展
/// </summary>
public static class SerilogHostingExtensions
{
    /// <summary>
    /// 添加默认日志拓展（已废弃，请使用 IHostBuilder 版本）
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="configAction"></param>
    /// <returns>IWebHostBuilder</returns>
    [Obsolete("Prefer UseSerilog() on IHostBuilder")]
    public static IWebHostBuilder UseSerilogDefault(
        this IWebHostBuilder hostBuilder,
        Action<LoggerConfiguration> configAction = default
    )
    {
        // .NET 10 不再支持 IWebHostBuilder.UseSerilog
        // 返回原始 hostBuilder，让调用者使用 IHostBuilder 版本
        return hostBuilder;
    }

    /// <summary>
    /// 添加默认日志拓展
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    public static IHostBuilder UseSerilogDefault(
        this IHostBuilder builder,
        Action<LoggerConfiguration> configAction = default
    )
    {
        // 判断是否是单文件环境
        var isSingleFileEnvironment = string.IsNullOrWhiteSpace(
            Assembly.GetEntryAssembly().Location
        );

        builder.UseSerilog(
            (context, configuration) =>
            {
                // 加载配置文件
                var config = configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext();

                if (configAction != null)
                    configAction.Invoke(config);
                else
                {
                    // 判断是否有输出配置
                    var hasWriteTo = context.Configuration["Serilog:WriteTo:0:Name"];
                    if (hasWriteTo == null)
                    {
                        config
                            .WriteTo.Console(
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                            )
                            .WriteTo.File(
                                Path.Combine(
                                    !isSingleFileEnvironment
                                        ? AppDomain.CurrentDomain.BaseDirectory
                                        : AppContext.BaseDirectory,
                                    "logs",
                                    "application..log"
                                ),
                                LogEventLevel.Information,
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: null,
                                encoding: Encoding.UTF8
                            );
                    }
                }
            }
        );

        return builder;
    }

    /// <summary>
    /// 添加默认日志拓展
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    public static WebApplicationBuilder UseSerilogDefault(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration> configAction = default
    )
    {
        builder.Host.UseSerilogDefault(configAction);

        return builder;
    }
}
