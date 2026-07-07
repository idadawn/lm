using Microsoft.Extensions.Hosting;
using Poxiao.Lab.CollectorAgent.Options;
using Poxiao.Lab.CollectorAgent.Spool;
using Poxiao.Lab.CollectorAgent.State;
using Poxiao.Lab.CollectorAgent.Upload;
using Poxiao.Lab.CollectorAgent.Workers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/collector-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("  采集网关服务启动中...");
    Log.Information("========================================");

    // 不使用 Poxiao 框架的 Inject()（会引入大量 Web 服务自动发现），仅使用 .NET 原生 Host + 手动注册所需服务。
    // 零 ProjectReference：不依赖 Poxiao.Lab，保持轻量、可独立发布到设备端电脑。
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureAppConfiguration((hostContext, config) =>
        {
            // 加载 Configurations 子目录下的所有配置文件
            var basePath = AppContext.BaseDirectory;
            var configDir = Path.Combine(basePath, "Configurations");
            if (Directory.Exists(configDir))
            {
                foreach (var jsonFile in Directory.GetFiles(configDir, "*.json"))
                {
                    config.AddJsonFile(jsonFile, optional: true, reloadOnChange: false);
                }
            }
        })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            // 绑定配置
            services.Configure<CollectorOptions>(configuration.GetSection("Collector"));

            // HTTP 客户端（ServerClient 内部设置 BaseAddress/Timeout）
            services.AddHttpClient<ServerClient>();

            // 状态 / 暂存 / 上报客户端
            services.AddSingleton<SourceStateStore>();
            services.AddSingleton<SpoolQueue>();

            // 核心 Worker：采集、上报、心跳
            services.AddHostedService<PollingWorker>();
            services.AddHostedService<UploadWorker>();
            services.AddHostedService<HeartbeatWorker>();
        });

    var host = builder.Build();

    Log.Information("采集网关服务已成功启动，等待数据源轮询...");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "采集网关服务启动失败！");
}
finally
{
    Log.Information("采集网关服务已停止");
    await Log.CloseAndFlushAsync();
}
